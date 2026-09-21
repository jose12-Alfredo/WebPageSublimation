namespace WebPageSublimation.Features.Proformas;

public static class ProformaCalculos
{
    public static decimal CalcularSubtotal(decimal cantidad, decimal precioUnitario)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        if (precioUnitario < 0) throw new ArgumentOutOfRangeException(nameof(precioUnitario));
        return checked(cantidad * precioUnitario);
    }

    public static decimal CalcularTotal(IEnumerable<decimal> subtotales)
    {
        decimal total = 0;
        foreach (var subtotal in subtotales)
        {
            if (subtotal < 0) throw new ArgumentOutOfRangeException(nameof(subtotales));
            total = checked(total + subtotal);
        }
        return total;
    }
}
