namespace WebPageSublimation.Features.Auth;

/// <summary>
/// Expone en el login las cuentas demo que siembra <c>DatabaseInitializer</c> (<see cref="DemoUserDefaults"/>).
/// Nunca muestra la cuenta de InitialAdmin. La visualización exige DemoAccess:ShowCredentials.
/// </summary>
public sealed record DemoLoginCredentials(
    string PromoterEmail,
    string PromoterPassword,
    string AdministratorEmail,
    string AdministratorPassword)
{
    public static DemoLoginCredentials? From(IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("DemoAccess:ShowCredentials"))
        {
            return null;
        }

        return new DemoLoginCredentials(
            DemoUserDefaults.PromoterEmail,
            DemoUserDefaults.PromoterPassword,
            DemoUserDefaults.AdministratorEmail,
            DemoUserDefaults.AdministratorPassword);
    }
}
