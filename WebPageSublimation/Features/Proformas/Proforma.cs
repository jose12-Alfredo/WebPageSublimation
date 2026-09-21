using WebPageSublimation.Features.Catalogo;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Promotores;

namespace WebPageSublimation.Features.Proformas;

public sealed class Proforma
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long Numero { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public required string ClienteNombre { get; set; }
    public Guid PromotorId { get; set; }
    public Promotor? Promotor { get; set; }
    public required string PromotorNombre { get; set; }
    public string? Observaciones { get; set; }
    public decimal Total { get; set; }
    public ICollection<DetalleProforma> Detalles { get; set; } = [];
}

public sealed class DetalleProforma
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProformaId { get; set; }
    public Proforma? Proforma { get; set; }
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public required string ProductoNombre { get; set; }
    public string? VarianteNombre { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
