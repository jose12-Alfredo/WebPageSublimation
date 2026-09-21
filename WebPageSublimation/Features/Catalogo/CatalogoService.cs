using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;

namespace WebPageSublimation.Features.Catalogo;

public sealed class CatalogoService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<IReadOnlyList<CategoriaResumen>> ListarCategoriasAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Categorias.AsNoTracking().OrderBy(x => x.Nombre)
            .Select(x => new CategoriaResumen(x.Id, x.Nombre, x.IsActive)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductoResumen>> ListarProductosAsync(string? search, bool soloActivos,
        Guid? categoriaId = null, bool soloPreciosConfirmados = false, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var query = db.Productos.AsNoTracking().Include(x => x.Categoria).Include(x => x.Variantes)
            .Include(x => x.Imagenes).AsQueryable();
        if (soloActivos) query = query.Where(x => x.IsActive && x.Categoria!.IsActive);
        if (soloPreciosConfirmados) query = query.Where(x => !x.PrecioEsReferencial);
        if (categoriaId is not null) query = query.Where(x => x.CategoriaId == categoriaId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Nombre, $"%{term}%") || (x.Descripcion != null && EF.Functions.ILike(x.Descripcion, $"%{term}%")));
        }
        return await query.OrderBy(x => x.Nombre).Select(x => new ProductoResumen(
            x.Id, x.Nombre, x.Descripcion, x.CategoriaId, x.Categoria!.Nombre, x.PrecioComercial, x.PrecioEsReferencial,
            x.IsActive, x.Imagenes.OrderByDescending(i => i.EsPrincipal).ThenBy(i => i.Orden)
                .Select(i => (Guid?)i.Id).FirstOrDefault(),
            x.Variantes.Where(v => !soloActivos || v.IsActive)
                .OrderBy(v => v.Nombre).Select(v => new VarianteResumen(v.Id, v.Nombre, v.IsActive)).ToList(),
            x.Imagenes.OrderByDescending(i => i.EsPrincipal).ThenBy(i => i.Orden)
                .Select(i => new ImagenResumen(i.Id, i.EsPrincipal, i.Orden)).ToList()))
            .ToListAsync(ct);
    }

    public async Task<ResultadoCatalogo> CrearCategoriaAsync(string? nombre, CancellationToken ct = default)
    {
        var clean = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(clean)) return ResultadoCatalogo.Error("Indica el nombre de la categoría.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (await db.Categorias.AnyAsync(x => x.Nombre == clean, ct)) return ResultadoCatalogo.Error("Esa categoría ya existe.");
        db.Categorias.Add(new Categoria { Nombre = clean });
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("La categoría fue creada.");
    }

    public async Task<ResultadoCatalogo> ActualizarCategoriaAsync(Guid id, string? nombre, CancellationToken ct = default)
    {
        var clean = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(clean) || clean.Length > 120) return ResultadoCatalogo.Error("Indica un nombre de categoría válido.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var category = await db.Categorias.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (category is null) return ResultadoCatalogo.Error("La categoría no existe.");
        if (await db.Categorias.AnyAsync(x => x.Id != id && x.Nombre == clean, ct)) return ResultadoCatalogo.Error("Esa categoría ya existe.");
        category.Nombre = clean;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("La categoría fue actualizada.");
    }

    public async Task<ResultadoCatalogo> CambiarEstadoCategoriaAsync(Guid id, bool active, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var category = await db.Categorias.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (category is null) return ResultadoCatalogo.Error("La categoría no existe.");
        category.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok(active ? "La categoría fue activada." : "La categoría y sus productos dejaron de estar disponibles para nuevas operaciones.");
    }

    public async Task<ResultadoCatalogo> CrearProductoAsync(string? nombre, string? descripcion, string? categoriaId,
        string? precio, bool precioReferencial, CancellationToken ct = default)
    {
        if (!Guid.TryParse(categoriaId, out var category) || !TryParsePrecio(precio, out var value) || value < 0)
            return ResultadoCatalogo.Error("Selecciona una categoría y un precio comercial válido.");
        var clean = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(clean)) return ResultadoCatalogo.Error("Indica el nombre del producto.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.Categorias.AnyAsync(x => x.Id == category && x.IsActive, ct)) return ResultadoCatalogo.Error("La categoría seleccionada no está disponible.");
        db.Productos.Add(new Producto { Nombre = clean, Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(), CategoriaId = category, PrecioComercial = value, PrecioEsReferencial = precioReferencial });
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("El producto fue creado.");
    }

    public async Task<ResultadoCatalogo> ActualizarProductoAsync(Guid id, string? nombre, string? descripcion,
        string? categoriaId, string? precio, bool precioReferencial, CancellationToken ct = default)
    {
        if (!Guid.TryParse(categoriaId, out var category) || !TryParsePrecio(precio, out var value) || value < 0)
            return ResultadoCatalogo.Error("Selecciona una categoría y un precio comercial válido.");
        var clean = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(clean) || clean.Length > 180 || descripcion?.Trim().Length > 1000)
            return ResultadoCatalogo.Error("Revisa el nombre y la descripción del producto.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var product = await db.Productos.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return ResultadoCatalogo.Error("El producto no está disponible.");
        if (!await db.Categorias.AnyAsync(x => x.Id == category && x.IsActive, ct))
            return ResultadoCatalogo.Error("La categoría seleccionada no está disponible.");
        product.Nombre = clean;
        product.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        product.CategoriaId = category;
        product.PrecioComercial = value;
        product.PrecioEsReferencial = precioReferencial;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("El producto fue actualizado.");
    }

    public async Task<ResultadoCatalogo> CambiarEstadoProductoAsync(Guid id, bool active, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var product = await db.Productos.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return ResultadoCatalogo.Error("El producto no está disponible.");
        if (active && product.PrecioComercial <= 0)
            return ResultadoCatalogo.Error("Define un precio comercial mayor a cero antes de publicar el producto.");
        product.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok(active ? "El producto fue activado." : "El producto fue desactivado; su historial se conserva.");
    }

    public async Task<ResultadoCatalogo> CrearVarianteAsync(Guid productId, string? nombre, CancellationToken ct = default)
    {
        var clean = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(clean) || clean.Length > 160)
            return ResultadoCatalogo.Error("Indica una variante válida.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.Productos.AnyAsync(x => x.Id == productId, ct))
            return ResultadoCatalogo.Error("El producto no está disponible.");
        if (await db.VariantesProducto.AnyAsync(x => x.ProductoId == productId && x.Nombre == clean, ct))
            return ResultadoCatalogo.Error("Esa variante ya existe para el producto.");
        db.VariantesProducto.Add(new VarianteProducto { ProductoId = productId, Nombre = clean });
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("La variante fue agregada.");
    }

    public async Task<ResultadoCatalogo> CambiarEstadoVarianteAsync(Guid productId, Guid variantId, bool active, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var variant = await db.VariantesProducto.SingleOrDefaultAsync(x => x.Id == variantId && x.ProductoId == productId, ct);
        if (variant is null) return ResultadoCatalogo.Error("La variante no está disponible.");
        variant.IsActive = active;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok(active ? "La variante fue activada." : "La variante fue desactivada.");
    }

    public async Task<ResultadoCatalogo> GuardarImagenesAsync(Guid productId, IReadOnlyList<IFormFile>? images,
        CancellationToken ct = default)
    {
        if (images is null || images.Count == 0) return ResultadoCatalogo.Error("Selecciona al menos una imagen.");
        if (images.Count > 8) return ResultadoCatalogo.Error("Puedes subir hasta 8 imágenes por vez.");
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (!await db.Productos.AnyAsync(x => x.Id == productId, ct)) return ResultadoCatalogo.Error("El producto no está disponible.");
        var currentCount = await db.ImagenesProducto.CountAsync(x => x.ProductoId == productId, ct);
        if (currentCount + images.Count > 8) return ResultadoCatalogo.Error("Cada producto puede tener hasta 8 imágenes.");
        var prepared = new List<(byte[] Data, string ContentType)>();
        foreach (var image in images)
        {
            if (image.Length == 0 || image.Length > 5 * 1024 * 1024)
                return ResultadoCatalogo.Error("Cada imagen debe pesar entre 1 byte y 5 MB.");
            await using var memory = new MemoryStream();
            await image.CopyToAsync(memory, ct);
            var bytes = memory.ToArray();
            var detectedType = DetectImageContentType(bytes);
            if (detectedType is null)
                return ResultadoCatalogo.Error($"El archivo {image.FileName} debe ser una imagen JPG, PNG o WebP válida.");
            prepared.Add((bytes, detectedType));
        }
        var nextOrder = await db.ImagenesProducto.Where(x => x.ProductoId == productId)
            .Select(x => (int?)x.Orden).MaxAsync(ct) ?? -1;
        var hasPrimary = await db.ImagenesProducto.AnyAsync(x => x.ProductoId == productId && x.EsPrincipal, ct);
        foreach (var item in prepared)
        {
            nextOrder++;
            db.ImagenesProducto.Add(new ImagenProducto { ProductoId = productId, ContentType = item.ContentType,
                Data = item.Data, Orden = nextOrder, EsPrincipal = !hasPrimary });
            hasPrimary = true;
        }
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok(prepared.Count == 1 ? "La imagen fue agregada." : $"Se agregaron {prepared.Count} imágenes.");
    }

    public async Task<ResultadoCatalogo> EstablecerImagenPrincipalAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var images = await db.ImagenesProducto.Where(x => x.ProductoId == productId).ToListAsync(ct);
        if (images.All(x => x.Id != imageId)) return ResultadoCatalogo.Error("La imagen no pertenece al producto.");
        foreach (var image in images) image.EsPrincipal = image.Id == imageId;
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("La imagen principal fue actualizada.");
    }

    public async Task<ResultadoCatalogo> EliminarImagenAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var image = await db.ImagenesProducto.SingleOrDefaultAsync(x => x.Id == imageId && x.ProductoId == productId, ct);
        if (image is null) return ResultadoCatalogo.Error("La imagen no está disponible.");
        var wasPrimary = image.EsPrincipal;
        db.ImagenesProducto.Remove(image);
        if (wasPrimary)
        {
            var replacement = await db.ImagenesProducto.Where(x => x.ProductoId == productId && x.Id != imageId)
                .OrderBy(x => x.Orden).FirstOrDefaultAsync(ct);
            if (replacement is not null) replacement.EsPrincipal = true;
        }
        await db.SaveChangesAsync(ct);
        return ResultadoCatalogo.Ok("La imagen fue eliminada.");
    }

    public async Task<ImagenProductoContenido?> ObtenerImagenAsync(Guid imageId, bool allowInactive, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.ImagenesProducto.AsNoTracking()
            .Where(x => x.Id == imageId && (allowInactive || x.Producto!.IsActive))
            .Select(x => new ImagenProductoContenido(x.Data, x.ContentType)).SingleOrDefaultAsync(ct);
    }

    public static bool TryParsePrecio(string? value, out decimal price)
    {
        price = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var normalized = value.Trim().Replace(" ", string.Empty);
        var comma = normalized.LastIndexOf(',');
        var dot = normalized.LastIndexOf('.');
        if (comma >= 0 && dot >= 0)
        {
            normalized = comma > dot
                ? normalized.Replace(".", string.Empty).Replace(',', '.')
                : normalized.Replace(",", string.Empty);
        }
        else if (comma >= 0)
        {
            normalized = normalized.Replace(',', '.');
        }

        return decimal.TryParse(normalized,
            System.Globalization.NumberStyles.AllowDecimalPoint |
            System.Globalization.NumberStyles.AllowLeadingSign,
            System.Globalization.CultureInfo.InvariantCulture,
            out price);
    }

    public static string? DetectImageContentType(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF) return "image/jpeg";
        if (data.Length >= 8 && data[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return "image/png";
        if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data.Slice(8, 4).SequenceEqual("WEBP"u8)) return "image/webp";
        return null;
    }
}

public sealed record CategoriaResumen(Guid Id, string Nombre, bool IsActive);
public sealed record ProductoResumen(Guid Id, string Nombre, string? Descripcion, Guid CategoriaId, string Categoria, decimal PrecioComercial, bool PrecioEsReferencial, bool IsActive, Guid? ImagenId, IReadOnlyList<VarianteResumen> Variantes, IReadOnlyList<ImagenResumen> Imagenes);
public sealed record VarianteResumen(Guid Id, string Nombre, bool IsActive);
public sealed record ImagenResumen(Guid Id, bool EsPrincipal, int Orden);
public sealed record ImagenProductoContenido(byte[] Data, string ContentType);
public sealed record ResultadoCatalogo(bool Success, string Message) { public static ResultadoCatalogo Ok(string m) => new(true,m); public static ResultadoCatalogo Error(string m) => new(false,m); }
