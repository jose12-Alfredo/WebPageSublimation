using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Auth;
using Xunit;

namespace WebPageSublimation.Tests;

public class DemoUserConfigurationTests
{
    [Fact]
    public void La_politica_normal_permanece_fuerte_sin_demo_habilitada()
    {
        using var provider = CrearProveedor(new Dictionary<string, string?>
        {
            ["DemoUsers:AllowSimplePassword"] = "true"
        });
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value.Password;
        Assert.Equal(10, options.RequiredLength);
        Assert.True(options.RequireLowercase);
        Assert.True(options.RequireUppercase);
        Assert.True(options.RequireNonAlphanumeric);
    }

    [Fact]
    public void La_excepcion_simple_requiere_las_dos_banderas_demo()
    {
        using var provider = CrearProveedor(new Dictionary<string, string?>
        {
            ["DemoUsers:Enabled"] = "true",
            ["DemoUsers:AllowSimplePassword"] = "true"
        });
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value.Password;
        Assert.Equal(8, options.RequiredLength);
        Assert.True(options.RequireDigit);
        Assert.False(options.RequireLowercase);
        Assert.False(options.RequireUppercase);
        Assert.False(options.RequireNonAlphanumeric);
    }

    [Fact]
    public void Las_credenciales_visibles_exigen_habilitacion_explicita()
    {
        var hidden = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DemoUsers:Enabled"] = "true"
        }).Build();
        Assert.Null(DemoLoginCredentials.From(hidden));

        var visible = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DemoAccess:ShowCredentials"] = "true"
        }).Build();
        Assert.NotNull(DemoLoginCredentials.From(visible));
    }

    [Fact]
    public void El_login_muestra_las_cuentas_sembradas_y_nunca_el_administrador_inicial()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DemoAccess:ShowCredentials"] = "true",
            ["InitialAdmin:Email"] = "admin.real@simons.test",
            ["InitialAdmin:Password"] = "Secreta!Real2026"
        }).Build();

        var credentials = DemoLoginCredentials.From(configuration);

        Assert.NotNull(credentials);
        Assert.Equal(DemoUserDefaults.AdministratorEmail, credentials.AdministratorEmail);
        Assert.Equal(DemoUserDefaults.AdministratorPassword, credentials.AdministratorPassword);
        Assert.Equal(DemoUserDefaults.PromoterEmail, credentials.PromoterEmail);
        Assert.Equal(DemoUserDefaults.PromoterPassword, credentials.PromoterPassword);
    }

    private static ServiceProvider CrearProveedor(Dictionary<string, string?> values)
    {
        values["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused";
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPersistence(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        return services.BuildServiceProvider();
    }
}
