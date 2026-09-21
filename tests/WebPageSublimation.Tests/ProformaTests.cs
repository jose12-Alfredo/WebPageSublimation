using WebPageSublimation.Features.Proformas;
using Xunit;

namespace WebPageSublimation.Tests;

public class ProformaTests
{
    [Fact]
    public void Subtotal_conserva_cantidad_y_precio_decimal()
        => Assert.Equal(31.875m, ProformaCalculos.CalcularSubtotal(1.25m, 25.50m));

    [Fact]
    public void Total_suma_los_subtotales_congelados()
        => Assert.Equal(131.875m, ProformaCalculos.CalcularTotal([31.875m, 100m]));

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Cantidad_no_positiva_es_rechazada(string value)
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProformaCalculos.CalcularSubtotal(decimal.Parse(value), 10m));

    [Fact]
    public void Precio_negativo_es_rechazado()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProformaCalculos.CalcularSubtotal(1m, -0.01m));
}
