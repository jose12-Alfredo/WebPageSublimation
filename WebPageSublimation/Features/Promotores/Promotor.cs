using WebPageSublimation.Data;

namespace WebPageSublimation.Features.Promotores;

public sealed class Promotor
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string UserId { get; set; }

    public required string Nombre { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
}
