using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Proformas;

public static class ProformaEndpoints
{
    public static IEndpointRouteBuilder MapProformaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/proformas/crear", CrearAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Promotor));
        return endpoints;
    }

    private static async Task<IResult> CrearAsync(
        ProformaService service,
        ClaimsPrincipal principal,
        HttpContext httpContext,
        IAntiforgery antiforgery,
        CancellationToken cancellationToken)
    {
        if (!await antiforgery.IsRequestValidAsync(httpContext))
            return Results.BadRequest("La solicitud no superó la validación de seguridad.");

        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var result = await service.CrearAsync(
            principal,
            form["clienteId"].FirstOrDefault(),
            form["productoId"].Select(value => value ?? string.Empty).ToArray(),
            form["cantidad"].Select(value => value ?? string.Empty).ToArray(),
            form["varianteId"].Select(value => value ?? string.Empty).ToArray(),
            form["observaciones"].FirstOrDefault(),
            cancellationToken);
        var route = result.Success && result.ProformaId is not null
            ? $"/proformas/{result.ProformaId}"
            : "/proformas/nueva";
        return Results.LocalRedirect($"{route}?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
    }
}
