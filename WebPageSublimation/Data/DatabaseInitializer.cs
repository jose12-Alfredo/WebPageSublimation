using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Security;
using WebPageSublimation.Features.Promotores;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Auth;

namespace WebPageSublimation.Data;

public static class DatabaseInitializer
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");

        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in Roles.Todos)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            EnsureSucceeded(result, $"crear el rol {roleName}");
        }

        var email = app.Configuration["InitialAdmin:Email"]?.Trim().ToLowerInvariant();
        var password = app.Configuration["InitialAdmin:Password"];

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "No se creó el administrador inicial porque InitialAdmin:Email o " +
                "InitialAdmin:Password no están configurados.");
        }
        else
        {
            var administrator = await userManager.FindByEmailAsync(email);
            if (administrator is null)
            {
                administrator = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(administrator, password);
                EnsureSucceeded(createResult, "crear el administrador inicial");
            }
            else if (!string.Equals(administrator.Email, email, StringComparison.Ordinal) ||
                     !string.Equals(administrator.UserName, email, StringComparison.Ordinal))
            {
                administrator.Email = email;
                administrator.UserName = email;
                EnsureSucceeded(await userManager.UpdateAsync(administrator),
                    "normalizar el correo del administrador inicial");
            }

            if (!await userManager.IsInRoleAsync(administrator, Roles.Administrador))
            {
                var roleResult = await userManager.AddToRoleAsync(administrator, Roles.Administrador);
                EnsureSucceeded(roleResult, "asignar el rol Administrador");
            }
        }

        var demoDataEnabled = app.Configuration.GetValue<bool>("DemoData:Enabled");

        await EnsureDemoUserAsync(
            userManager,
            DemoUserDefaults.AdministratorEmail,
            DemoUserDefaults.AdministratorPassword,
            Roles.Administrador);

        var promoterEmail = DemoUserDefaults.PromoterEmail;
        var previousPromoterEmail = app.Configuration["DemoUsers:PreviousPromoterEmail"]?
            .Trim().ToLowerInvariant();
        var promoterPassword = DemoUserDefaults.PromoterPassword;
        var promoterName = DemoUserDefaults.PromoterName;

        var promoterUser = await userManager.FindByEmailAsync(promoterEmail);
        if (promoterUser is null && !string.IsNullOrWhiteSpace(previousPromoterEmail))
            promoterUser = await userManager.FindByEmailAsync(previousPromoterEmail);
        if (promoterUser is null)
        {
            promoterUser = new ApplicationUser
            {
                UserName = promoterEmail,
                Email = promoterEmail,
                EmailConfirmed = true,
                IsActive = true
            };
            EnsureSucceeded(await userManager.CreateAsync(promoterUser, promoterPassword),
                "crear el usuario promotor demo");
        }
        else if (!string.Equals(promoterUser.Email, promoterEmail, StringComparison.Ordinal) ||
                 !string.Equals(promoterUser.UserName, promoterEmail, StringComparison.Ordinal))
        {
            promoterUser.Email = promoterEmail;
            promoterUser.UserName = promoterEmail;
            EnsureSucceeded(await userManager.UpdateAsync(promoterUser),
                "normalizar el correo del promotor demo");
        }

        if (!await userManager.IsInRoleAsync(promoterUser, Roles.Promotor))
            EnsureSucceeded(await userManager.AddToRoleAsync(promoterUser, Roles.Promotor),
                "asignar el rol Promotor al usuario demo");

        if (!await db.Promotores.AnyAsync(x => x.UserId == promoterUser.Id))
        {
            db.Promotores.Add(new Promotor
            {
                UserId = promoterUser.Id,
                Nombre = promoterName
            });
            await db.SaveChangesAsync();
        }

        var clientEmail = DemoUserDefaults.ClientEmail;
        var clientName = DemoUserDefaults.ClientName;

        var promoterProfile = await db.Promotores.SingleAsync(x => x.UserId == promoterUser.Id);
        var client = await db.Clientes.SingleOrDefaultAsync(x => x.Email == clientEmail);
        if (client is null)
        {
            client = new Cliente { Nombre = clientName, Email = clientEmail };
            db.Clientes.Add(client);
            await db.SaveChangesAsync();
        }

        if (!await db.ClientesPromotores.AnyAsync(x =>
                x.ClienteId == client.Id && x.PromotorId == promoterProfile.Id))
        {
            db.ClientesPromotores.Add(new ClientePromotor
            {
                ClienteId = client.Id,
                PromotorId = promoterProfile.Id
            });
            await db.SaveChangesAsync();
        }

        if (demoDataEnabled)
        {
            await DemoDataSeeder.SeedAsync(
                db,
                promoterProfile,
                client,
                app.Environment.ContentRootPath);
            logger.LogInformation("Se cargaron los datos de demostración.");
        }
    }

    private static async Task<ApplicationUser> EnsureDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                IsActive = true
            };
            EnsureSucceeded(await userManager.CreateAsync(user, password), $"crear el usuario demo {role}");
        }

        if (!await userManager.IsInRoleAsync(user, role))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), $"asignar el rol {role} al usuario demo");

        return user;
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"No fue posible {operation}: {errors}");
    }
}
