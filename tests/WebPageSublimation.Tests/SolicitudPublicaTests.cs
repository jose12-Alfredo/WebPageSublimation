using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.SolicitudesPublicas;
using Xunit;

namespace WebPageSublimation.Tests;

public class SolicitudPublicaTests
{
    private readonly SolicitudPublicaService _service = new(new ThrowingFactory());

    [Fact]
    public async Task Requiere_nombre_y_detalle()
    {
        var result = await _service.CrearAsync(null, null, "70000000", null, null);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Requiere_al_menos_un_medio_de_contacto()
    {
        var result = await _service.CrearAsync("Cliente", null, null, null, "Necesito una cotización");
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Rechaza_un_correo_invalido_antes_de_ir_a_la_base()
    {
        var result = await _service.CrearAsync("Cliente", null, null, "correo-invalido", "Detalle");
        Assert.False(result.Success);
    }

    private sealed class ThrowingFactory : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => throw new InvalidOperationException();
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("La validación debía terminar antes de consultar la base.");
    }
}
