using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Promotores;
using Xunit;

namespace WebPageSublimation.Tests;

public class DemoDataSeederTests
{
    [Fact]
    public async Task Una_base_nueva_recibe_catalogo_imagenes_y_operaciones_sin_duplicarlas()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"demo-{Guid.NewGuid()}")
            .Options;
        await using var db = new AppDbContext(options);
        var promoter = new Promotor { UserId = "promotor-demo", Nombre = "Promotor de demostración" };
        var customer = new Cliente { Nombre = "Cliente de demostración", Email = "cliente.demo@simons.test" };
        db.AddRange(promoter, customer);
        await db.SaveChangesAsync();

        var contentRoot = EncontrarRaizAplicacion();
        await DemoDataSeeder.SeedAsync(db, promoter, customer, contentRoot);
        await DemoDataSeeder.SeedAsync(db, promoter, customer, contentRoot);

        Assert.Equal(4, await db.Categorias.CountAsync());
        Assert.Equal(4, await db.Productos.CountAsync());
        Assert.Equal(6, await db.ImagenesProducto.CountAsync());
        Assert.Equal(1, await db.Proformas.CountAsync());
        Assert.Equal(2, await db.Pedidos.CountAsync());
        Assert.All(await db.ImagenesProducto.ToListAsync(), image => Assert.NotEmpty(image.Data));
    }

    private static string EncontrarRaizAplicacion()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "WebPageSublimation", "Assets", "CatalogoPendiente");
            if (Directory.Exists(candidate)) return Path.Combine(directory.FullName, "WebPageSublimation");
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("No se encontró la carpeta de recursos del catálogo.");
    }
}
