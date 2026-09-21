using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Features.Catalogo;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Pedidos;
using WebPageSublimation.Features.Promotores;
using WebPageSublimation.Features.Proformas;

namespace WebPageSublimation.Data;

/// <summary>
/// Carga una demostración repetible en una base nueva. Solo se ejecuta cuando
/// DemoData:Enabled está activado y nunca copia datos personales o credenciales.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly Guid ProformaDemoId = Guid.Parse("a1e98f11-8c0d-4e5d-b6aa-0f9c25e98c01");
    private static readonly Guid PedidoEntregadoDemoId = Guid.Parse("b2e98f11-8c0d-4e5d-b6aa-0f9c25e98c02");
    private static readonly Guid PedidoPendienteDemoId = Guid.Parse("c3e98f11-8c0d-4e5d-b6aa-0f9c25e98c03");
    private static readonly Guid PedidoPendienteKey = Guid.Parse("d4e98f11-8c0d-4e5d-b6aa-0f9c25e98c04");
    private const string NotaProforma = "Datos de demostración cargados automáticamente.";
    private const string NotaPedido = "Pedido de demostración cargado automáticamente.";

    public static async Task SeedAsync(
        AppDbContext db,
        Promotor promotor,
        Cliente cliente,
        string contentRoot,
        CancellationToken cancellationToken = default)
    {
        await CatalogoAssetImporter.ImportAsync(db, contentRoot, cancellationToken);

        var products = await db.Productos.AsNoTracking()
            .Where(product => product.Nombre == "Gorra personalizada" ||
                              product.Nombre == "Jarra personalizada" ||
                              product.Nombre == "Botella personalizada")
            .ToDictionaryAsync(product => product.Nombre, cancellationToken);

        if (products.Count != 3)
            throw new InvalidOperationException("No se pudo preparar el catálogo de demostración.");

        var gorra = products["Gorra personalizada"];
        var jarra = products["Jarra personalizada"];
        var botella = products["Botella personalizada"];

        if (!await db.Proformas.AnyAsync(proforma => proforma.Id == ProformaDemoId, cancellationToken))
        {
            var proforma = new Proforma
            {
                Id = ProformaDemoId,
                FechaUtc = DateTime.UtcNow.AddDays(-4),
                ClienteId = cliente.Id,
                ClienteNombre = cliente.Nombre,
                PromotorId = promotor.Id,
                PromotorNombre = promotor.Nombre,
                Observaciones = NotaProforma
            };

            proforma.Detalles.Add(CrearLineaProforma(gorra, 24m));
            proforma.Detalles.Add(CrearLineaProforma(jarra, 12m));
            proforma.Total = proforma.Detalles.Sum(linea => linea.Subtotal);
            db.Proformas.Add(proforma);
        }

        if (!await db.Pedidos.AnyAsync(pedido => pedido.Id == PedidoEntregadoDemoId, cancellationToken))
        {
            var pedido = new Pedido
            {
                Id = PedidoEntregadoDemoId,
                FechaUtc = DateTime.UtcNow.AddDays(-2),
                FechaRequerida = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
                Estado = EstadoPedido.Entregado,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.Nombre,
                PromotorId = promotor.Id,
                PromotorNombre = promotor.Nombre,
                ProformaId = ProformaDemoId,
                Observaciones = NotaPedido
            };

            pedido.Detalles.Add(CrearLineaPedido(gorra, 24m));
            pedido.Detalles.Add(CrearLineaPedido(jarra, 12m));
            pedido.Total = pedido.Detalles.Sum(linea => linea.Subtotal);
            db.Pedidos.Add(pedido);
        }

        if (!await db.Pedidos.AnyAsync(pedido => pedido.Id == PedidoPendienteDemoId, cancellationToken))
        {
            var pedido = new Pedido
            {
                Id = PedidoPendienteDemoId,
                ClaveIdempotencia = PedidoPendienteKey,
                FechaUtc = DateTime.UtcNow.AddDays(-1),
                FechaRequerida = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                Estado = EstadoPedido.EnProduccion,
                ClienteId = cliente.Id,
                ClienteNombre = cliente.Nombre,
                PromotorId = promotor.Id,
                PromotorNombre = promotor.Nombre,
                Observaciones = NotaPedido
            };

            pedido.Detalles.Add(CrearLineaPedido(botella, 18m));
            pedido.Total = pedido.Detalles.Sum(linea => linea.Subtotal);
            db.Pedidos.Add(pedido);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static DetalleProforma CrearLineaProforma(Producto producto, decimal cantidad)
    {
        var subtotal = cantidad * producto.PrecioComercial;
        return new DetalleProforma
        {
            ProductoId = producto.Id,
            ProductoNombre = producto.Nombre,
            Cantidad = cantidad,
            PrecioUnitario = producto.PrecioComercial,
            Subtotal = subtotal
        };
    }

    private static DetallePedido CrearLineaPedido(Producto producto, decimal cantidad)
    {
        var subtotal = cantidad * producto.PrecioComercial;
        return new DetallePedido
        {
            ProductoId = producto.Id,
            ProductoNombre = producto.Nombre,
            Cantidad = cantidad,
            PrecioUnitario = producto.PrecioComercial,
            Subtotal = subtotal
        };
    }
}
