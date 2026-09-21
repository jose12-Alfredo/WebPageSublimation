using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebPageSublimation.Data;
using WebPageSublimation.Security;
using Xunit;

namespace WebPageSublimation.Tests;

public class SecurityTests
{
    [Fact]
    public async Task La_validacion_de_usuario_activo_se_reutiliza_hasta_invalidarla()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<ActiveUserSessionCache>();
        await using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ActiveUserSessionCache>();
        var databaseReads = 0;

        async Task<bool> LoadActive()
        {
            databaseReads++;
            await Task.CompletedTask;
            return true;
        }

        Assert.True(await cache.IsActiveAsync("usuario-1", LoadActive));
        Assert.True(await cache.IsActiveAsync("usuario-1", LoadActive));
        Assert.Equal(1, databaseReads);

        cache.Invalidate("usuario-1");
        Assert.True(await cache.IsActiveAsync("usuario-1", LoadActive));
        Assert.Equal(2, databaseReads);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("https://externo.example")]
    [InlineData("//externo.example")]
    [InlineData("/\\externo.example")]
    [InlineData("/admin\r\nLocation: https://externo.example")]
    [InlineData("admin")]
    public void Un_destino_no_local_vuelve_a_la_ruta_segura(string? destination)
        => Assert.Equal("/", SafeRedirect.Resolve(destination, "/"));

    [Theory]
    [InlineData("/admin")]
    [InlineData("/promotor?pagina=2")]
    [InlineData("/cuenta/ingresar")]
    public void Una_ruta_local_se_conserva(string destination)
        => Assert.Equal(destination, SafeRedirect.Resolve(destination, "/"));

    [Fact]
    public async Task Personalizar_redirecciones_conserva_la_validacion_de_sesion_de_Identity()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var validator = new RejectingStampValidator();
        services.AddPersistence(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused"
            }).Build());
        services.AddScoped<ISecurityStampValidator>(_ => validator);
        await using var provider = services.BuildServiceProvider();
        var cookie = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        var context = new CookieValidatePrincipalContext(
            new DefaultHttpContext { RequestServices = provider },
            new AuthenticationScheme(IdentityConstants.ApplicationScheme, null, typeof(CookieAuthenticationHandler)),
            cookie,
            new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "test")], "test")),
                IdentityConstants.ApplicationScheme));
        await cookie.Events.OnValidatePrincipal(context);
        Assert.True(validator.Called);
        Assert.Null(context.Principal);
    }

    private sealed class RejectingStampValidator : ISecurityStampValidator
    {
        public bool Called { get; private set; }
        public Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            Called = true;
            context.RejectPrincipal();
            return Task.CompletedTask;
        }
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(null, true)]
    public async Task Una_cookie_solo_conserva_acceso_si_el_usuario_sigue_activo(bool? active, bool rejected)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPersistence(new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused"
            }).Build());
        services.AddScoped<ISecurityStampValidator, AcceptingStampValidator>();
        services.AddScoped<UserManager<ApplicationUser>>(sp => new TestUserManager(
            sp.GetRequiredService<IUserStore<ApplicationUser>>(),
            active.HasValue ? new ApplicationUser { IsActive = active.Value } : null));
        var auth = new RecordingAuthentication();
        services.AddSingleton<IAuthenticationService>(auth);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var cookie = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
        var context = new CookieValidatePrincipalContext(
            new DefaultHttpContext { RequestServices = scope.ServiceProvider },
            new AuthenticationScheme(IdentityConstants.ApplicationScheme, null, typeof(CookieAuthenticationHandler)),
            cookie,
            new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "test")], "test")), IdentityConstants.ApplicationScheme));
        await cookie.Events.OnValidatePrincipal(context);
        Assert.Equal(rejected, context.Principal is null);
        Assert.Equal(rejected, auth.SignedOut);
    }

    private sealed class AcceptingStampValidator : ISecurityStampValidator
    {
        public Task ValidateAsync(CookieValidatePrincipalContext context) => Task.CompletedTask;
    }

    private sealed class TestUserManager(IUserStore<ApplicationUser> store, ApplicationUser? user)
        : UserManager<ApplicationUser>(store, Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(), [], [], new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), null!,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<UserManager<ApplicationUser>>.Instance)
    {
        public override Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal) => Task.FromResult(user);
    }

    private sealed class RecordingAuthentication : IAuthenticationService
    {
        public bool SignedOut { get; private set; }
        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignedOut = true;
            return Task.CompletedTask;
        }
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => throw new NotSupportedException();
        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();
        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => throw new NotSupportedException();
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => throw new NotSupportedException();
    }
}
