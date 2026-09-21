using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;

namespace WebPageSublimation.Features.Catalogo;

public static class CatalogoAssetImporter
{
    private sealed record Draft(string Category, string Name, string Description, decimal ReferencePrice,
        string[] RelativeImages);

    private static readonly Draft[] Drafts =
    [
        new("Gorras", "Gorra personalizada",
            "Gorra con frente personalizable para diseños, nombres o identidad visual. El precio final depende de cantidad y personalización.", 45m,
            ["gorras/330508f5-8679-491f-a962-ffd7e172f39f.jpg"]),
        new("Tazas y jarras", "Jarra personalizada",
            "Jarra personalizada para regalos, promociones o celebraciones. El precio final depende de cantidad, material y diseño.", 55m,
            ["tazas/2e46c656-4fbc-4737-92e8-460197f8b4a9.jpg", "tazas/c73eb08b-4de4-46d0-a9bf-aa4efb8496ae.jpg"]),
        new("Botellas y toma todos", "Botella personalizada",
            "Botella con personalización gráfica. El precio final depende de cantidad, capacidad, modelo y diseño.", 65m,
            ["toma-todos/2be1d99a-fc68-4890-800c-90c77175156b.jpg", "toma-todos/94280d1d-f0d5-456d-95e7-447b814d2c4f.jpg"]),
        new("Vasos térmicos", "Vaso térmico personalizado",
            "Vaso térmico personalizable de acero inoxidable y 450 ml según la pieza entregada. Consulta condiciones por cantidad.", 90m,
            ["toma-todos/aeb0cf61-3102-4813-97d3-d23f2b37a105.jpg"])
    ];

    public static async Task<ImportResult> ImportAsync(AppDbContext db, string contentRoot,
        CancellationToken cancellationToken = default)
    {
        var assetRoot = Path.Combine(contentRoot, "Assets", "CatalogoPendiente");
        if (!Directory.Exists(assetRoot))
            throw new DirectoryNotFoundException($"No existe el directorio de catálogo pendiente: {assetRoot}");

        var productsCreated = 0;
        var imagesCreated = 0;
        foreach (var draft in Drafts)
        {
            var category = await db.Categorias.SingleOrDefaultAsync(x => x.Nombre == draft.Category, cancellationToken);
            if (category is null)
            {
                category = new Categoria { Nombre = draft.Category };
                db.Categorias.Add(category);
                await db.SaveChangesAsync(cancellationToken);
            }

            var product = await db.Productos.Include(x => x.Imagenes)
                .SingleOrDefaultAsync(x => x.Nombre == draft.Name, cancellationToken);
            if (product is null)
            {
                product = new Producto
                {
                    Nombre = draft.Name,
                    Descripcion = draft.Description,
                    CategoriaId = category.Id,
                    PrecioComercial = draft.ReferencePrice,
                    PrecioEsReferencial = true,
                    IsActive = true
                };
                db.Productos.Add(product);
                await db.SaveChangesAsync(cancellationToken);
                productsCreated++;
            }
            else
            {
                product.CategoriaId = category.Id;
                product.Descripcion = draft.Description;
                product.PrecioComercial = draft.ReferencePrice;
                product.PrecioEsReferencial = true;
                product.IsActive = true;
                await db.SaveChangesAsync(cancellationToken);
            }

            var nextOrder = product.Imagenes.Count == 0 ? 0 : product.Imagenes.Max(x => x.Orden) + 1;
            var knownImages = product.Imagenes.ToList();
            foreach (var relativeImage in draft.RelativeImages)
            {
                var path = Path.GetFullPath(Path.Combine(assetRoot, relativeImage.Replace('/', Path.DirectorySeparatorChar)));
                if (!path.StartsWith(Path.GetFullPath(assetRoot) + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Una imagen quedó fuera del directorio permitido.");
                var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
                if (CatalogoService.DetectImageContentType(bytes) != "image/jpeg")
                    throw new InvalidDataException($"La imagen no es un JPEG válido: {relativeImage}");
                if (knownImages.Any(x => x.Data.AsSpan().SequenceEqual(bytes))) continue;
                var image = new ImagenProducto
                {
                    ProductoId = product.Id,
                    ContentType = "image/jpeg",
                    Data = bytes,
                    Orden = nextOrder++,
                    EsPrincipal = knownImages.Count == 0
                };
                db.ImagenesProducto.Add(image);
                knownImages.Add(image);
                imagesCreated++;
            }
            await db.SaveChangesAsync(cancellationToken);
        }
        return new ImportResult(productsCreated, imagesCreated, Drafts.Length);
    }
}

public sealed record ImportResult(int ProductsCreated, int ImagesCreated, int DraftsProcessed);
