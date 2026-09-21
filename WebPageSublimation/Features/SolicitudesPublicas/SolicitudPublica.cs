namespace WebPageSublimation.Features.SolicitudesPublicas;

public sealed class SolicitudPublica
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Empresa { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public required string Detalle { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
