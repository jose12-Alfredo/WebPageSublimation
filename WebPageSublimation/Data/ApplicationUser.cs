using Microsoft.AspNetCore.Identity;

namespace WebPageSublimation.Data;

/// <summary>
/// Usuario autenticado del sistema. Las entidades comerciales referenciarán su identificador
/// sin depender directamente de ASP.NET Core Identity.
/// </summary>
public sealed class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
