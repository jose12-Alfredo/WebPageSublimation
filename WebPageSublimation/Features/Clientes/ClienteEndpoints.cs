using Microsoft.AspNetCore.Mvc;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Clientes;

public static class ClienteEndpoints
{
    public static IEndpointRouteBuilder MapClienteEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/admin/clientes")
            .RequireAuthorization(policy => policy.RequireRole(Roles.Administrador));

        group.MapPost("/crear", CrearAsync);
        group.MapPost("/{id:guid}/desactivar", DesactivarAsync);
        group.MapPost("/{clienteId:guid}/asignar", AsignarAsync);
        group.MapPost("/{clienteId:guid}/promotores/{promotorId:guid}/desasignar", DesasignarAsync);
        return endpoints;
    }

    private static async Task<IResult> CrearAsync(
        ClienteService service,
        [FromForm] string? nombre, [FromForm] string? nit, [FromForm] string? telefono,
        [FromForm] string? whatsapp, [FromForm] string? email, [FromForm] string? direccion,
        [FromForm] string? personaContacto, [FromForm] string? notas,
        CancellationToken cancellationToken)
    {
        var result = await service.CrearAsync(nombre, nit, telefono, whatsapp, email, direccion, personaContacto, notas, cancellationToken);
        return Volver(result);
    }

    private static async Task<IResult> DesactivarAsync(ClienteService service, Guid id, CancellationToken cancellationToken) =>
        Volver(await service.DesactivarAsync(id, cancellationToken));

    private static async Task<IResult> AsignarAsync(ClienteService service, Guid clienteId, [FromForm] Guid promotorId, CancellationToken cancellationToken) =>
        Volver(await service.AsignarAsync(clienteId, promotorId, cancellationToken));

    private static async Task<IResult> DesasignarAsync(ClienteService service, Guid clienteId, Guid promotorId, CancellationToken cancellationToken) =>
        Volver(await service.DesasignarAsync(clienteId, promotorId, cancellationToken));

    private static IResult Volver(ResultadoCliente result) => Results.LocalRedirect(
        $"/admin/clientes?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
}
