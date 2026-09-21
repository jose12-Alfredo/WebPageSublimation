using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Proformas;

public sealed class ProformaService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<ResultadoProforma> CrearAsync(
        ClaimsPrincipal principal,
        string? clienteId,
        IReadOnlyList<string> productoIds,
        IReadOnlyList<string> cantidades,
        IReadOnlyList<string> varianteIds,
        string? observaciones,
        CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return ResultadoProforma.Error("La sesión no es válida.");
        if (!Guid.TryParse(clienteId, out var clienteGuid))
            return ResultadoProforma.Error("Selecciona un cliente.");
        if (productoIds.Count == 0 || productoIds.Count != cantidades.Count)
            return ResultadoProforma.Error("Agrega al menos un producto con su cantidad.");

        var lineas = new List<LineaSolicitada>(productoIds.Count);
        for (var i = 0; i < productoIds.Count; i++)
        {
            if (!Guid.TryParse(productoIds[i], out var productoId) ||
                !TryParseCantidad(cantidades[i], out var cantidad) ||
                cantidad <= 0 || cantidad > 999_999_999_999_999.999m ||
                decimal.Round(cantidad, 3) != cantidad)
                return ResultadoProforma.Error("Cada producto debe tener una cantidad válida de hasta tres decimales.");
            Guid? varianteId = null;
            if (i < varianteIds.Count && !string.IsNullOrWhiteSpace(varianteIds[i]))
            {
                if (!Guid.TryParse(varianteIds[i], out var parsedVariant))
                    return ResultadoProforma.Error("La variante seleccionada no es válida.");
                varianteId = parsedVariant;
            }
            lineas.Add(new LineaSolicitada(productoId, varianteId, cantidad));
        }

        var notas = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        if (notas?.Length > 1500)
            return ResultadoProforma.Error("Las observaciones pueden tener hasta 1500 caracteres.");

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var promotor = await db.Promotores
            .SingleOrDefaultAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
        if (promotor is null)
            return ResultadoProforma.Error("Tu perfil de promotor no está activo.");

        var cliente = await db.Clientes
            .Where(x => x.Id == clienteGuid && x.IsActive &&
                x.Promotores.Any(acceso => acceso.PromotorId == promotor.Id))
            .SingleOrDefaultAsync(cancellationToken);
        if (cliente is null)
            return ResultadoProforma.Error("El cliente no está disponible para tu cuenta.");

        var productIds = lineas.Select(x => x.ProductoId).Distinct().ToArray();
        var productos = await db.Productos.AsNoTracking()
            .Where(x => productIds.Contains(x.Id) && x.IsActive && x.Categoria!.IsActive && !x.PrecioEsReferencial)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (productos.Count != productIds.Length)
            return ResultadoProforma.Error("Uno de los productos ya no está disponible.");

        var variantIds = lineas.Where(x => x.VarianteId is not null).Select(x => x.VarianteId!.Value).Distinct().ToArray();
        var variantes = await db.VariantesProducto.AsNoTracking()
            .Where(x => variantIds.Contains(x.Id) && x.IsActive)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (variantes.Count != variantIds.Length || lineas.Any(x => x.VarianteId is not null &&
            variantes[x.VarianteId.Value].ProductoId != x.ProductoId))
            return ResultadoProforma.Error("Una variante no pertenece al producto seleccionado o ya no está activa.");

        var proforma = new Proforma
        {
            ClienteId = cliente.Id,
            ClienteNombre = cliente.Nombre,
            PromotorId = promotor.Id,
            PromotorNombre = promotor.Nombre,
            Observaciones = notas
        };

        try
        {
            foreach (var linea in lineas)
            {
                var producto = productos[linea.ProductoId];
                var subtotal = ProformaCalculos.CalcularSubtotal(linea.Cantidad, producto.PrecioComercial);
                if (subtotal > 9_999_999_999_999.99999m)
                    return ResultadoProforma.Error("Un subtotal supera el límite permitido.");
                proforma.Detalles.Add(new DetalleProforma
                {
                    ProductoId = producto.Id,
                    ProductoNombre = producto.Nombre,
                    VarianteNombre = linea.VarianteId is null ? null : variantes[linea.VarianteId.Value].Nombre,
                    Cantidad = linea.Cantidad,
                    PrecioUnitario = producto.PrecioComercial,
                    Subtotal = subtotal
                });
            }
            proforma.Total = ProformaCalculos.CalcularTotal(proforma.Detalles.Select(x => x.Subtotal));
            if (proforma.Total > 9_999_999_999_999.99999m)
                return ResultadoProforma.Error("El total de la proforma supera el límite permitido.");
        }
        catch (OverflowException)
        {
            return ResultadoProforma.Error("El total de la proforma supera el límite permitido.");
        }

        db.Proformas.Add(proforma);
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoProforma.Ok("La proforma fue creada.", proforma.Id);
    }

    public async Task<IReadOnlyList<ProformaResumen>> ListarParaPromotorAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return [];
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await ProyectarResumen(db.Proformas.AsNoTracking()
            .Where(x => x.Promotor!.UserId == userId)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProformaResumen>> ListarParaAdministradorAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await ProyectarResumen(db.Proformas.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public async Task<ProformaDetalle?> ObtenerAsync(
        ClaimsPrincipal principal, Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.Proformas.AsNoTracking().Where(x => x.Id == id);
        if (!principal.IsInRole(Roles.Administrador))
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return null;
            query = query.Where(x => x.Promotor!.UserId == userId);
        }

        return await query.Select(x => new ProformaDetalle(
            x.Id, x.Numero, x.FechaUtc, x.ClienteNombre, x.PromotorNombre, x.Observaciones,
            x.Total, x.Detalles.OrderBy(d => d.Id).Select(d => new LineaProforma(
                d.ProductoNombre, d.VarianteNombre, d.Cantidad, d.PrecioUnitario, d.Subtotal)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ProformaResumen> ProyectarResumen(IQueryable<Proforma> query) =>
        query.OrderByDescending(x => x.Numero).Select(x =>
            new ProformaResumen(x.Id, x.Numero, x.FechaUtc, x.ClienteNombre, x.PromotorNombre, x.Total));

    private static bool TryParseCantidad(string value, out decimal cantidad)
    {
        const NumberStyles style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        return decimal.TryParse(value, style, CultureInfo.InvariantCulture, out cantidad) ||
               decimal.TryParse(value, style, CultureInfo.GetCultureInfo("es-BO"), out cantidad);
    }

    private sealed record LineaSolicitada(Guid ProductoId, Guid? VarianteId, decimal Cantidad);
}

public sealed record ProformaResumen(Guid Id, long Numero, DateTime FechaUtc, string ClienteNombre, string PromotorNombre, decimal Total);
public sealed record LineaProforma(string ProductoNombre, string? VarianteNombre, decimal Cantidad, decimal PrecioUnitario, decimal Subtotal);
public sealed record ProformaDetalle(Guid Id, long Numero, DateTime FechaUtc, string ClienteNombre, string PromotorNombre, string? Observaciones, decimal Total, IReadOnlyList<LineaProforma> Lineas);

public sealed record ResultadoProforma(bool Success, string Message, Guid? ProformaId = null)
{
    public static ResultadoProforma Ok(string message, Guid id) => new(true, message, id);
    public static ResultadoProforma Error(string message) => new(false, message);
}
