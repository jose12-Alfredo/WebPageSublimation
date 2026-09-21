namespace WebPageSublimation.Features.Auth;

/// <summary>
/// Expone las cuentas de promotor y administrador configuradas para una demostración.
/// La visualización exige habilitar DemoUsers:Enabled y DemoAccess:ShowCredentials.
/// </summary>
public sealed record DemoLoginCredentials(
    string PromoterEmail,
    string PromoterPassword,
    string? AdministratorEmail,
    string? AdministratorPassword)
{
    public bool HasAdministrator =>
        !string.IsNullOrWhiteSpace(AdministratorEmail) && !string.IsNullOrWhiteSpace(AdministratorPassword);

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
