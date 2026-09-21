using Microsoft.AspNetCore.Mvc;

namespace WebPageSublimation.Features.SolicitudesPublicas;

public static class SolicitudPublicaEndpoints
{
    public static IEndpointRouteBuilder MapSolicitudPublicaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/solicitar-cotizacion", async (SolicitudPublicaService service,
                [FromForm] string? nombre, [FromForm] string? empresa, [FromForm] string? telefono,
                [FromForm] string? email, [FromForm] string? detalle, CancellationToken ct) =>
            {
                var result = await service.CrearAsync(nombre, empresa, telefono, email, detalle, ct);
                return Results.LocalRedirect($"/solicitar-cotizacion?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
            })
            .AllowAnonymous()
            .RequireRateLimiting("public-form");
        return endpoints;
    }
}
