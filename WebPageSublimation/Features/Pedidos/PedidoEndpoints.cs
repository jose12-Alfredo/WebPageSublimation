using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Pedidos;

public static class PedidoEndpoints
{
    public static IEndpointRouteBuilder MapPedidoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/pedidos/crear", CrearAsync).RequireAuthorization(p=>p.RequireRole(Roles.Promotor));
        endpoints.MapPost("/proformas/{id:guid}/convertir", ConvertirAsync).RequireAuthorization(p=>p.RequireRole(Roles.Promotor));
        endpoints.MapPost("/admin/pedidos/{id:guid}/estado", async(Guid id,PedidoService service,[FromForm]string? estado,CancellationToken ct)=>Redirect(await service.CambiarEstadoAsync(id,estado,ct))).RequireAuthorization(p=>p.RequireRole(Roles.Administrador));
        return endpoints;
    }
    private static async Task<IResult> CrearAsync(PedidoService service,ClaimsPrincipal principal,HttpContext context,IAntiforgery antiforgery,CancellationToken ct){if(!await antiforgery.IsRequestValidAsync(context))return Results.BadRequest("La solicitud no superó la validación de seguridad.");var f=await context.Request.ReadFormAsync(ct);var result=await service.CrearDirectoAsync(principal,f["clienteId"].FirstOrDefault(),f["productoId"].Select(x=>x??"").ToArray(),f["cantidad"].Select(x=>x??"").ToArray(),f["varianteId"].Select(x=>x??"").ToArray(),f["especificacion"].Select(x=>x??"").ToArray(),f["fechaRequerida"].FirstOrDefault(),f["observaciones"].FirstOrDefault(),f["claveIdempotencia"].FirstOrDefault(),ct);return Redirect(result,result.Success?$"/pedidos/{result.PedidoId}":"/pedidos/nuevo");}
    private static async Task<IResult> ConvertirAsync(Guid id,PedidoService service,ClaimsPrincipal principal,[FromForm]string? fechaRequerida,CancellationToken ct){var result=await service.ConvertirAsync(principal,id,fechaRequerida,ct);return Redirect(result,result.Success?$"/pedidos/{result.PedidoId}":$"/proformas/{id}");}
    private static IResult Redirect(ResultadoPedido result,string? route=null)=>Results.LocalRedirect($"{route??"/admin/pedidos"}?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success?"ok":"error")}");
}
