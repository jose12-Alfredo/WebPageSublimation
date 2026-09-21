using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Pedidos;

namespace WebPageSublimation.Features.Comisiones;

public sealed class ComisionService(IDbContextFactory<AppDbContext> dbFactory)
{
    public static readonly Expression<Func<Pedido, bool>> PedidoElegible =
        pedido => pedido.ProformaId != null && pedido.Estado == EstadoPedido.Entregado;

    public async Task<ResultadoComision> CalcularAsync(
        DateOnly? desde,
        DateOnly? hasta,
        Guid? promotorId,
        string? porcentajeIngresado,
        CancellationToken cancellationToken = default)
    {
        if (desde is not null && hasta is not null && desde > hasta)
            return ResultadoComision.Error("La fecha inicial no puede ser posterior a la fecha final.");

        decimal? porcentaje = null;
        if (!string.IsNullOrWhiteSpace(porcentajeIngresado))
        {
            if (!TryParsePorcentaje(porcentajeIngresado, out var parsed) || parsed <= 0 || parsed > 100)
                return ResultadoComision.Error("El porcentaje debe ser mayor que 0 y no puede superar 100.");
            porcentaje = parsed;
        }

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var pedidos = db.Pedidos.AsNoTracking().Where(PedidoElegible);

        if (desde is not null)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            pedidos = pedidos.Where(x => x.FechaUtc >= inicio);
        }

        if (hasta is not null)
        {
            var finExclusivo = hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            pedidos = pedidos.Where(x => x.FechaUtc < finExclusivo);
        }

        if (promotorId is not null)
            pedidos = pedidos.Where(x => x.PromotorId == promotorId);

        var ventas = await pedidos
            .GroupBy(x => new { x.PromotorId, x.PromotorNombre })
            .Select(group => new
            {
                group.Key.PromotorId,
                group.Key.PromotorNombre,
                Pedidos = group.Count(),
                Importe = group.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.Importe)
            .ToListAsync(cancellationToken);

        var filas = ventas.Select(x => new ComisionPromotor(
            x.PromotorId,
            x.PromotorNombre,
            x.Pedidos,
            x.Importe,
            porcentaje is null ? null : CalcularImporte(x.Importe, porcentaje.Value)))
            .ToList();

        return ResultadoComision.Ok(
            porcentaje,
            filas.Sum(x => x.Pedidos),
            filas.Sum(x => x.ImporteVentas),
            porcentaje is null ? null : filas.Sum(x => x.ComisionCalculada ?? 0),
            filas);
    }

    public static decimal CalcularImporte(decimal importeVentas, decimal porcentaje) =>
        decimal.Round(importeVentas * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);

    public static bool TryParsePorcentaje(string value, out decimal porcentaje)
    {
        var normalized = value.Trim();
        var lastComma = normalized.LastIndexOf(',');
        var lastDot = normalized.LastIndexOf('.');

        if (lastComma >= 0 && lastDot >= 0)
        {
            var decimalSeparator = lastComma > lastDot ? ',' : '.';
            var thousandsSeparator = decimalSeparator == ',' ? "." : ",";
            normalized = normalized.Replace(thousandsSeparator, string.Empty)
                .Replace(decimalSeparator, '.');
        }
        else
        {
            normalized = normalized.Replace(',', '.');
        }

        return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out porcentaje) && decimal.Round(porcentaje, 4) == porcentaje;
    }
}

public sealed record ComisionPromotor(
    Guid PromotorId,
    string PromotorNombre,
    int Pedidos,
    decimal ImporteVentas,
    decimal? ComisionCalculada);

public sealed record ResultadoComision(
    bool Success,
    string? Message,
    decimal? Porcentaje,
    int Pedidos,
    decimal ImporteVentas,
    decimal? ComisionTotal,
    IReadOnlyList<ComisionPromotor> PorPromotor)
{
    public static ResultadoComision Ok(decimal? porcentaje, int pedidos, decimal importe,
        decimal? comision, IReadOnlyList<ComisionPromotor> filas) =>
        new(true, null, porcentaje, pedidos, importe, comision, filas);

    public static ResultadoComision Error(string message) =>
        new(false, message, null, 0, 0, null, []);
}
