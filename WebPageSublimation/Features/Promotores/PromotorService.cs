using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Promotores;

/// <summary>
/// Administra el perfil comercial y la cuenta individual de los promotores.
/// Toda cuenta creada aquí recibe exclusivamente el rol Promotor.
/// </summary>
public sealed class PromotorService(
    IDbContextFactory<AppDbContext> dbFactory,
    UserManager<ApplicationUser> userManager,
    ActiveUserSessionCache activeUserSessions)
{
    public async Task<IReadOnlyList<PromotorResumen>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Promotores.AsNoTracking()
            .OrderBy(promotor => promotor.Nombre)
            .Select(promotor => new PromotorResumen(
                promotor.Id,
                promotor.Nombre,
                promotor.User!.Email ?? string.Empty,
                promotor.IsActive,
                promotor.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoPromotor> CrearAsync(
        string? nombre,
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        var cleanName = nombre?.Trim();
        var cleanEmail = email?.Trim();
        if (string.IsNullOrWhiteSpace(cleanName) || string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(password))
        {
            return ResultadoPromotor.Error("Completa nombre, correo y contraseña.");
        }

        if (cleanName.Length > 160)
        {
            return ResultadoPromotor.Error("El nombre puede tener hasta 160 caracteres.");
        }

        if (await userManager.FindByEmailAsync(cleanEmail) is not null)
        {
            return ResultadoPromotor.Error("Ya existe una cuenta con ese correo.");
        }

        var user = new ApplicationUser
        {
            UserName = cleanEmail,
            Email = cleanEmail,
            EmailConfirmed = true,
            IsActive = true
        };

        var createUser = await userManager.CreateAsync(user, password);
        if (!createUser.Succeeded)
        {
            return ResultadoPromotor.Error(string.Join(" ", createUser.Errors.Select(error => error.Description)));
        }

        var addRole = await userManager.AddToRoleAsync(user, Roles.Promotor);
        if (!addRole.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return ResultadoPromotor.Error("No fue posible asignar el rol de promotor.");
        }

        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            db.Promotores.Add(new Promotor
            {
                UserId = user.Id,
                Nombre = cleanName
            });
            await db.SaveChangesAsync(cancellationToken);
            return ResultadoPromotor.Ok("La cuenta del promotor fue creada.");
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<ResultadoPromotor> DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var promotor = await db.Promotores.Include(item => item.User)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (promotor is null)
        {
            return ResultadoPromotor.Error("El promotor ya no está disponible.");
        }

        if (!promotor.IsActive)
        {
            return ResultadoPromotor.Ok("El promotor ya estaba desactivado.");
        }

        // RN-AUTH-005: conservar historial y bloquear futuras operaciones.
        promotor.IsActive = false;
        if (promotor.User is not null)
        {
            promotor.User.IsActive = false;
        }
        await db.SaveChangesAsync(cancellationToken);

        if (promotor.User is not null)
        {
            await userManager.UpdateSecurityStampAsync(promotor.User);
            activeUserSessions.Invalidate(promotor.User.Id);
        }

        return ResultadoPromotor.Ok("El promotor fue desactivado. Su historial se conserva.");
    }
}

public sealed record PromotorResumen(Guid Id, string Nombre, string Email, bool IsActive, DateTime CreatedAtUtc);

public sealed record ResultadoPromotor(bool Success, string Message)
{
    public static ResultadoPromotor Ok(string message) => new(true, message);
    public static ResultadoPromotor Error(string message) => new(false, message);
}
