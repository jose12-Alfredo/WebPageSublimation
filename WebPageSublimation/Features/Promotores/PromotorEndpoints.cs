using Microsoft.AspNetCore.Mvc;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Promotores;

public static class PromotorEndpoints
{
    public static IEndpointRouteBuilder MapPromotorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/admin/promotores")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

        group.MapPost("/crear", CrearAsync);
        group.MapPost("/{id:guid}/desactivar", DesactivarAsync);
        return endpoints;
    }

    private static async Task<IResult> CrearAsync(
        PromotorService service,
        [FromForm] string? nombre,
        [FromForm] string? email,
        [FromForm] string? password,
        CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(nombre, email, password, cancellationToken);
        return Results.LocalRedirect($"/admin/promotores?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
    }

    private static async Task<IResult> DesactivarAsync(
        PromotorService service,
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await service.DesactivarAsync(id, cancellationToken);
        return Results.LocalRedirect($"/admin/promotores?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
    }
}
