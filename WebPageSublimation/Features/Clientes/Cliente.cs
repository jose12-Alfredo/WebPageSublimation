using WebPageSublimation.Features.Promotores;

namespace WebPageSublimation.Features.Clientes;

public sealed class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Nit { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? PersonaContacto { get; set; }
    public string? Notas { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ClientePromotor> Promotores { get; set; } = [];
}

/// <summary>
/// Acceso explicito a la cartera. Permite cero, uno o varios promotores por cliente
/// mientras Simons define su politica comercial definitiva.
/// </summary>
public sealed class ClientePromotor
{
    public Guid ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid PromotorId { get; set; }
    public Promotor? Promotor { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
