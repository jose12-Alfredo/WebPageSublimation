using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Pedidos;
using Xunit;

namespace WebPageSublimation.Tests;

public sealed class PedidoValidationTests
{
    private static ClaimsPrincipal Principal() => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, "user-test")], "test"));

    [Fact]
    public async Task RechazaCantidadFueraDePrecisionAntesDeConsultarBase()
    {
        var service = new PedidoService(new ThrowingFactory());
        var result = await service.CrearDirectoAsync(Principal(), Guid.NewGuid().ToString(),
            [Guid.NewGuid().ToString()], ["1000000000000000"], [""], [""], null, null,
            Guid.NewGuid().ToString());
        Assert.False(result.Success);
        Assert.Contains("cantidades", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RechazaClaveIdempotenciaManipuladaAntesDeConsultarBase()
    {
        var service = new PedidoService(new ThrowingFactory());
        var result = await service.CrearDirectoAsync(Principal(), Guid.NewGuid().ToString(),
            [Guid.NewGuid().ToString()], ["1"], [""], [""], null, null, "invalida");
        Assert.False(result.Success);
        Assert.Contains("solicitud", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ThrowingFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => throw new InvalidOperationException("No debe consultar la base.");
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("No debe consultar la base.");
    }
}
