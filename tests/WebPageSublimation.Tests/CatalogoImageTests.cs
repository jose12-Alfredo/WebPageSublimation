using WebPageSublimation.Features.Catalogo;
using Xunit;

namespace WebPageSublimation.Tests;

public sealed class CatalogoImageTests
{
    [Theory]
    [InlineData("65.00", 65)]
    [InlineData("65,00", 65)]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    public void Interpreta_precios_sin_multiplicar_los_decimales(string input, decimal expected)
    {
        Assert.True(CatalogoService.TryParsePrecio(input, out var actual));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DetectaFirmasPermitidas()
    {
        Assert.Equal("image/jpeg", CatalogoService.DetectImageContentType([0xFF, 0xD8, 0xFF, 0x00]));
        Assert.Equal("image/png", CatalogoService.DetectImageContentType([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
        Assert.Equal("image/webp", CatalogoService.DetectImageContentType("RIFF0000WEBP"u8));
    }

    [Fact]
    public void RechazaContenidoDisfrazadoDeImagen()
    {
        Assert.Null(CatalogoService.DetectImageContentType("<script>alert(1)</script>"u8));
        Assert.Null(CatalogoService.DetectImageContentType([]));
    }
}
