using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
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

    [Fact]
    public async Task Guarda_un_Jpeg_aunque_el_navegador_no_reporte_su_tipo_correctamente()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"catalogo-{Guid.NewGuid()}").Options;
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await using (var database = new AppDbContext(options))
        {
            database.Categorias.Add(new Categoria { Id = categoryId, Nombre = "Ropa" });
            database.Productos.Add(new Producto { Id = productId, CategoriaId = categoryId, Nombre = "Polera", PrecioComercial = 50 });
            await database.SaveChangesAsync();
        }

        var factory = new TestDbContextFactory(options);
        var service = new CatalogoService(factory);
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
        await using var stream = new MemoryStream(jpeg);
        IFormFile image = new FormFile(stream, 0, jpeg.Length, "imagenes", "polera.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };

        var result = await service.GuardarImagenesAsync(productId, [image]);

        Assert.True(result.Success);
        await using var verification = new AppDbContext(options);
        var saved = await verification.ImagenesProducto.SingleAsync();
        Assert.Equal("image/jpeg", saved.ContentType);
        Assert.Equal(jpeg, saved.Data);
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
