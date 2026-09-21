using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Catalogo;

public static class CatalogoEndpoints
{
    public static IEndpointRouteBuilder MapCatalogoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("/admin/catalogo").RequireAuthorization(x => x.RequireRole(Roles.Administrador));
        admin.MapPost("/categorias", async (CatalogoService service, [FromForm] string? nombre, CancellationToken ct) => Redirect(await service.CrearCategoriaAsync(nombre, ct)));
        admin.MapPost("/categorias/{id:guid}/editar", async (Guid id, CatalogoService service, [FromForm] string? nombre, CancellationToken ct) => Redirect(await service.ActualizarCategoriaAsync(id, nombre, ct)));
        admin.MapPost("/categorias/{id:guid}/estado", async (Guid id, CatalogoService service, [FromForm] bool activo, CancellationToken ct) => Redirect(await service.CambiarEstadoCategoriaAsync(id, activo, ct)));
        admin.MapPost("/productos", async (CatalogoService service, [FromForm] string? nombre, [FromForm] string? descripcion, [FromForm] string? categoriaId, [FromForm] string? precio, [FromForm] bool? precioReferencial, CancellationToken ct) => Redirect(await service.CrearProductoAsync(nombre, descripcion, categoriaId, precio, precioReferencial ?? false, ct)));
        admin.MapPost("/productos/{id:guid}/editar", async (Guid id, CatalogoService service, [FromForm] string? nombre, [FromForm] string? descripcion, [FromForm] string? categoriaId, [FromForm] string? precio, [FromForm] bool? precioReferencial, CancellationToken ct) => Redirect(await service.ActualizarProductoAsync(id, nombre, descripcion, categoriaId, precio, precioReferencial ?? false, ct)));
        admin.MapPost("/productos/{id:guid}/estado", async (Guid id, CatalogoService service, [FromForm] bool activo, CancellationToken ct) => Redirect(await service.CambiarEstadoProductoAsync(id, activo, ct)));
        admin.MapPost("/productos/{id:guid}/variantes", async (Guid id, CatalogoService service, [FromForm] string? nombre, CancellationToken ct) => Redirect(await service.CrearVarianteAsync(id, nombre, ct)));
        admin.MapPost("/productos/{productId:guid}/variantes/{variantId:guid}/estado", async (Guid productId, Guid variantId, CatalogoService service, [FromForm] bool activo, CancellationToken ct) => Redirect(await service.CambiarEstadoVarianteAsync(productId, variantId, activo, ct)));
        admin.MapPost("/productos/{id:guid}/imagenes", async (Guid id, CatalogoService service, [FromForm] List<IFormFile>? imagenes, CancellationToken ct) => Redirect(await service.GuardarImagenesAsync(id, imagenes?.Select(AsImagenCarga).ToList(), ct)));
        admin.MapPost("/productos/{productId:guid}/imagenes/{imageId:guid}/principal", async (Guid productId, Guid imageId, CatalogoService service, CancellationToken ct) => Redirect(await service.EstablecerImagenPrincipalAsync(productId, imageId, ct)));
        admin.MapPost("/productos/{productId:guid}/imagenes/{imageId:guid}/eliminar", async (Guid productId, Guid imageId, CatalogoService service, CancellationToken ct) => Redirect(await service.EliminarImagenAsync(productId, imageId, ct)));

        endpoints.MapGet("/media/productos/{imageId:guid}", async (Guid imageId, ClaimsPrincipal user, CatalogoService service, CancellationToken ct) =>
        {
            var image = await service.ObtenerImagenAsync(imageId, user.IsInRole(Roles.Administrador), ct);
            return image is null ? Results.NotFound() : Results.File(image.Data, image.ContentType, enableRangeProcessing: true);
        });
        return endpoints;
    }
    private static ImagenCarga AsImagenCarga(IFormFile file) => new(file.FileName, file.Length,
        _ => Task.FromResult<Stream>(file.OpenReadStream()));
    private static IResult Redirect(ResultadoCatalogo result) => Results.LocalRedirect($"/admin/catalogo?mensaje={Uri.EscapeDataString(result.Message)}&tipo={(result.Success ? "ok" : "error")}");
}
