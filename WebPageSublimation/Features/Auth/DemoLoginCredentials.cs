namespace WebPageSublimation.Features.Auth;

/// <summary>
/// Expone únicamente la cuenta de promotor configurada para una demostración pública.
/// La visualización exige una confirmación explícita y nunca muestra la contraseña administrativa.
/// </summary>
public sealed record DemoLoginCredentials(
    string PromoterEmail,
    string PromoterPassword,
    string? AdministratorEmail)
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
            configuration["InitialAdmin:Email"]?.Trim());
    }
}
