namespace WebPageSublimation.Features.Auth;

/// <summary>
/// Expone las cuentas configuradas para una demostración pública.
/// La visualización exige que el acceso demo y la publicación de credenciales estén habilitados.
/// </summary>
public sealed record DemoLoginCredentials(
    string PromoterEmail,
    string PromoterPassword,
    string? AdministratorEmail,
    string? AdministratorPassword)
{
    public static DemoLoginCredentials? From(IConfiguration configuration)
    {
        return new DemoLoginCredentials(
            DemoUserDefaults.PromoterEmail,
            DemoUserDefaults.PromoterPassword,
            DemoUserDefaults.AdministratorEmail,
            DemoUserDefaults.AdministratorPassword);
    }
}
