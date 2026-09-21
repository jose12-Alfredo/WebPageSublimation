using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Pedidos;
using Xunit;

namespace WebPageSublimation.Tests;

public sealed class PedidoModelTests
{
    [Fact]
    public void ProformaId_TieneIndiceUnico_ParaEvitarDobleConversion()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model;Username=model;Password=model")
            .UseSnakeCaseNamingConvention().Options;
        using var db = new AppDbContext(options);
        var entity = db.Model.FindEntityType(typeof(Pedido));
        var index = entity!.GetIndexes().Single(x => x.Properties.Single().Name == nameof(Pedido.ProformaId));
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void ClaveIdempotencia_TieneIndiceUnico_ParaEvitarDobleEnvioDirecto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model;Username=model;Password=model")
            .UseSnakeCaseNamingConvention().Options;
        using var db = new AppDbContext(options);
        var entity = db.Model.FindEntityType(typeof(Pedido));
        var index = entity!.GetIndexes().Single(x => x.Properties.Single().Name == nameof(Pedido.ClaveIdempotencia));
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Estados_IncluyenElFlujoConfirmado()
    {
        var states = Enum.GetValues<EstadoPedido>();
        Assert.Contains(EstadoPedido.Recibido, states);
        Assert.Contains(EstadoPedido.EnProduccion, states);
        Assert.Contains(EstadoPedido.Terminado, states);
        Assert.Contains(EstadoPedido.Entregado, states);
    }
}
