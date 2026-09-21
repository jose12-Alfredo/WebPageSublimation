using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebPageSublimation.Data;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Auth;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var account = endpoints.MapGroup("/cuenta");

        account.MapPost("/ingresar", SignInAsync)
            .AllowAnonymous()
            .RequireRateLimiting("login");

        account.MapPost("/salir", SignOutAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<RedirectHttpResult> SignInAsync(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        [FromForm] string? email,
        [FromForm] string? password,
        [FromForm] string? returnUrl)
    {
        var safeReturnUrl = SafeRedirect.Resolve(returnUrl, "/");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return LoginError("Completa el correo y la contraseña.", safeReturnUrl);
        }

        var normalizedEmail = email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || !user.IsActive)
        {
            return LoginError("El correo o la contraseña no son correctos.", safeReturnUrl);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            var message = result.IsLockedOut
                ? "La cuenta está temporalmente bloqueada por varios intentos fallidos."
                : "El correo o la contraseña no son correctos.";
            return LoginError(message, safeReturnUrl);
        }

        if (safeReturnUrl != "/")
        {
            return TypedResults.LocalRedirect(safeReturnUrl);
        }

        var destination = await userManager.IsInRoleAsync(user, Roles.Administrador)
            ? "/admin"
            : "/promotor";

        return TypedResults.LocalRedirect(destination);
    }

    private static async Task<IResult> SignOutAsync(
        HttpContext context,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery,
        [FromForm] string? returnUrl)
    {
        if (!await antiforgery.IsRequestValidAsync(context))
        {
            return Results.BadRequest("La solicitud no contiene un token de seguridad válido.");
        }

        await signInManager.SignOutAsync();
        return Results.LocalRedirect(SafeRedirect.Resolve(returnUrl, "/cuenta/ingresar"));
    }

    private static RedirectHttpResult LoginError(string message, string returnUrl)
    {
        var destination = $"/cuenta/ingresar?error={Uri.EscapeDataString(message)}";
        if (returnUrl != "/")
        {
            destination += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        return TypedResults.LocalRedirect(destination);
    }
}
