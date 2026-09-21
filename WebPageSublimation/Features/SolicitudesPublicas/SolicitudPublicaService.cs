using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;

namespace WebPageSublimation.Features.SolicitudesPublicas;

public sealed class SolicitudPublicaService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<ResultadoSolicitud> CrearAsync(string? nombre, string? empresa, string? telefono,
        string? email, string? detalle, CancellationToken cancellationToken = default)
    {
        var cleanName = Limpiar(nombre);
        var cleanPhone = Limpiar(telefono);
        var cleanEmail = Limpiar(email)?.ToLowerInvariant();
        var cleanDetail = Limpiar(detalle);
        if (string.IsNullOrWhiteSpace(cleanName) || string.IsNullOrWhiteSpace(cleanDetail))
            return ResultadoSolicitud.Error("Completa tu nombre y cuéntanos qué necesitas.");
        if (string.IsNullOrWhiteSpace(cleanPhone) && string.IsNullOrWhiteSpace(cleanEmail))
            return ResultadoSolicitud.Error("Deja un teléfono o correo para poder responderte.");
        if (cleanEmail is not null && !System.Net.Mail.MailAddress.TryCreate(cleanEmail, out _))
            return ResultadoSolicitud.Error("El correo no tiene un formato válido.");
        if (cleanName.Length > 160 || Limpiar(empresa)?.Length > 180 || cleanPhone?.Length > 40 ||
            cleanEmail?.Length > 254 || cleanDetail.Length > 2000)
            return ResultadoSolicitud.Error("Uno de los datos supera la longitud permitida.");

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        db.SolicitudesPublicas.Add(new SolicitudPublica
        {
            Nombre = cleanName,
            Empresa = Limpiar(empresa),
            Telefono = cleanPhone,
            Email = cleanEmail,
            Detalle = cleanDetail
        });
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoSolicitud.Ok("Recibimos tu solicitud. Simons podrá contactarte con los datos que dejaste.");
    }

    public async Task<IReadOnlyList<SolicitudPublicaResumen>> ListarAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.SolicitudesPublicas.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new SolicitudPublicaResumen(x.Id, x.Nombre, x.Empresa, x.Telefono, x.Email, x.Detalle, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private static string? Limpiar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed record SolicitudPublicaResumen(Guid Id, string Nombre, string? Empresa, string? Telefono, string? Email, string Detalle, DateTime CreatedAtUtc);
public sealed record ResultadoSolicitud(bool Success, string Message)
{
    public static ResultadoSolicitud Ok(string message) => new(true, message);
    public static ResultadoSolicitud Error(string message) => new(false, message);
}
