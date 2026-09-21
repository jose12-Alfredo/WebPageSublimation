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
        if (!configuration.GetValue<bool>("DemoUsers:Enabled") ||
            !configuration.GetValue<bool>("DemoAccess:ShowCredentials"))
        {
            return null;
        }

        var promoterEmail = configuration["DemoUsers:PromoterEmail"]?.Trim();
        var promoterPassword = configuration["DemoUsers:PromoterPassword"];
        if (string.IsNullOrWhiteSpace(promoterEmail) || string.IsNullOrWhiteSpace(promoterPassword))
        {
            return null;
        }

        return new DemoLoginCredentials(
            promoterEmail,
            promoterPassword,
            configuration["InitialAdmin:Email"]?.Trim(),
            configuration["InitialAdmin:Password"]);
    }
}
