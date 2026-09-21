using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Comisiones;
using WebPageSublimation.Features.Pedidos;
using Xunit;

namespace WebPageSublimation.Tests;

public sealed class ComisionTests
{
    [Theory]
    [InlineData("10", 10)]
    [InlineData("7,5", 7.5)]
    [InlineData("7.25", 7.25)]
    [InlineData("12,3456", 12.3456)]
    public void PorcentajeAceptaFormatosComerciales(string input, decimal expected)
    {
        Assert.True(ComisionService.TryParsePorcentaje(input, out var actual));
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("7,12345")]
    [InlineData("abc")]
    [InlineData("")]
    public void PorcentajeRechazaValoresInvalidos(string input)
    {
        Assert.False(ComisionService.TryParsePorcentaje(input, out _));
    }

    [Fact]
    public void CalculaYRedondeaLaComisionMonetaria()
    {
        Assert.Equal(12.35m, ComisionService.CalcularImporte(123.45m, 10m));
    }

    [Theory]
    [InlineData(EstadoPedido.Entregado, true, true)]
    [InlineData(EstadoPedido.Entregado, false, false)]
    [InlineData(EstadoPedido.Terminado, true, false)]
    [InlineData(EstadoPedido.Cancelado, true, false)]
    public void SoloPedidosEntregadosOriginadosEnProformaSonElegibles(
        EstadoPedido estado, bool tieneProforma, bool esperado)
    {
        var pedido = new Pedido
        {
            ClienteNombre = "Cliente",
            PromotorNombre = "Promotor",
            Estado = estado,
            ProformaId = tieneProforma ? Guid.NewGuid() : null
        };

        Assert.Equal(esperado, ComisionService.PedidoElegible.Compile()(pedido));
    }

    [Fact]
    public async Task RechazaPorcentajeFueraDeRangoAntesDeConsultarBase()
    {
        var service = new ComisionService(new ThrowingFactory());
        var result = await service.CalcularAsync(null, null, null, "101");

        Assert.False(result.Success);
        Assert.Contains("100", result.Message);
    }

    [Fact]
    public async Task RechazaPeriodoInvertidoAntesDeConsultarBase()
    {
        var service = new ComisionService(new ThrowingFactory());
        var result = await service.CalcularAsync(
            new DateOnly(2026, 9, 20), new DateOnly(2026, 9, 1), null, "10");

        Assert.False(result.Success);
        Assert.Contains("fecha", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ThrowingFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => throw new InvalidOperationException("No debe consultar la base.");
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No debe consultar la base.");
    }
}
