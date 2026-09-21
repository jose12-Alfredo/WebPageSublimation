namespace WebPageSublimation.Security;

public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Promotor = "Promotor";

    public static readonly string[] Todos = [Administrador, Promotor];
}
