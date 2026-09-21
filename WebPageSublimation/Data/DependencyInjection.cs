using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Features.Promotores;
using WebPageSublimation.Features.Catalogo;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Proformas;
using WebPageSublimation.Features.SolicitudesPublicas;
using WebPageSublimation.Features.Pedidos;
using WebPageSublimation.Features.Dashboard;
using WebPageSublimation.Features.Comisiones;
using WebPageSublimation.Security;

namespace WebPageSublimation.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se configuró ConnectionStrings:DefaultConnection. " +
                "Use User Secrets en desarrollo o una variable de entorno en producción.");
        }

        void Configure(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
            options.UseSnakeCaseNamingConvention();
        }

        services.AddDbContext<AppDbContext>(Configure);
        services.AddDbContextFactory<AppDbContext>(Configure, ServiceLifetime.Scoped);

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                var allowSimpleDemoPassword = configuration.GetValue<bool>("DemoUsers:Enabled") &&
                    configuration.GetValue<bool>("DemoUsers:AllowSimplePassword");
                options.Password.RequiredLength = allowSimpleDemoPassword ? 8 : 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = !allowSimpleDemoPassword;
                options.Password.RequireUppercase = !allowSimpleDemoPassword;
                options.Password.RequireNonAlphanumeric = !allowSimpleDemoPassword;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "Simons.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = configuration.GetValue<bool>("Deployment:SecureCookies")
                ? CookieSecurePolicy.Always
                : CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.LoginPath = "/cuenta/ingresar";
            options.AccessDeniedPath = "/cuenta/acceso-denegado";
            var validateIdentity = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                await validateIdentity(context);
                if (context.Principal?.Identity?.IsAuthenticated != true)
                    return;

                // RN-AUTH-005: la desactivación también corta el acceso con cookies existentes.
                var manager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();
                var sessionCache = context.HttpContext.RequestServices
                    .GetRequiredService<ActiveUserSessionCache>();
                var userId = manager.GetUserId(context.Principal);
                var isActive = !string.IsNullOrWhiteSpace(userId) &&
                    await sessionCache.IsActiveAsync(userId,
                        async () => (await manager.GetUserAsync(context.Principal))?.IsActive == true);
                if (!isActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                }
            };
            // Mantener OnValidatePrincipal de Identity: reemplazar Events elimina
            // la comprobación del sello de seguridad de las sesiones existentes.
            options.Events.OnRedirectToLogin = context => Redirigir(context, "/cuenta/ingresar");
            options.Events.OnRedirectToAccessDenied = context => Redirigir(context, "/cuenta/acceso-denegado");
        });

        services.AddAuthorization();
        services.AddMemoryCache();
        services.AddSingleton<ActiveUserSessionCache>();
        var keyPath = configuration["Deployment:DataProtectionKeysPath"];
        if (!string.IsNullOrWhiteSpace(keyPath))
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyPath)).SetApplicationName("SimonsPublicidad");
        services.AddScoped<PromotorService>();
        services.AddScoped<CatalogoService>();
        services.AddScoped<ClienteService>();
        services.AddScoped<ProformaService>();
        services.AddScoped<SolicitudPublicaService>();
        services.AddScoped<PedidoService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<ComisionService>();

        return services;
    }

    private static Task Redirigir(RedirectContext<CookieAuthenticationOptions> context, string ruta)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = context.HttpContext.User.Identity?.IsAuthenticated == true
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        var destino = $"{ruta}?returnUrl={Uri.EscapeDataString(context.Request.PathBase + context.Request.Path + context.Request.QueryString)}";
        context.Response.Redirect(destino);
        return Task.CompletedTask;
    }
}
