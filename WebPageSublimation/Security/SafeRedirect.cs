namespace WebPageSublimation.Security;

public static class SafeRedirect
{
    public static string Resolve(string? returnUrl, string fallback)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return fallback;
        }

        var path = returnUrl.Trim();
        if (path[0] != '/' ||
            (path.Length > 1 && (path[1] == '/' || path[1] == '\\')) ||
            path.Any(char.IsControl) ||
            !Uri.TryCreate(path, UriKind.Relative, out _))
        {
            return fallback;
        }

        return path;
    }
}
