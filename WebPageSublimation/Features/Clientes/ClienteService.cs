using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;

namespace WebPageSublimation.Features.Clientes;

public sealed class ClienteService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<IReadOnlyList<ClienteResumen>> ListarParaAdministradorAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Clientes.AsNoTracking()
            .OrderBy(cliente => cliente.Nombre)
            .Select(cliente => new ClienteResumen(
                cliente.Id, cliente.Nombre, cliente.Nit, cliente.Telefono, cliente.WhatsApp,
                cliente.Email, cliente.Direccion, cliente.PersonaContacto, cliente.Notas,
                cliente.IsActive,
                cliente.Promotores.OrderBy(item => item.Promotor!.Nombre)
                    .Select(item => new PromotorAsignado(item.PromotorId, item.Promotor!.Nombre))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClienteResumen>> ListarParaPromotorAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.Clientes.AsNoTracking()
            .Where(cliente => cliente.IsActive && cliente.Promotores.Any(item =>
                item.Promotor!.UserId == userId && item.Promotor.IsActive))
            .OrderBy(cliente => cliente.Nombre)
            .Select(cliente => new ClienteResumen(
                cliente.Id, cliente.Nombre, cliente.Nit, cliente.Telefono, cliente.WhatsApp,
                cliente.Email, cliente.Direccion, cliente.PersonaContacto, cliente.Notas,
                cliente.IsActive, new List<PromotorAsignado>()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResultadoCliente> CrearAsync(
        string? nombre, string? nit, string? telefono, string? whatsapp, string? email,
        string? direccion, string? personaContacto, string? notas,
        CancellationToken cancellationToken = default)
    {
        var cleanName = Limpiar(nombre);
        if (string.IsNullOrWhiteSpace(cleanName))
            return ResultadoCliente.Error("Escribe el nombre o la razon social del cliente.");
        if (cleanName.Length > 180)
            return ResultadoCliente.Error("El nombre puede tener hasta 180 caracteres.");

        var cleanEmail = Limpiar(email);
        if (cleanEmail is not null && !System.Net.Mail.MailAddress.TryCreate(cleanEmail, out _))
            return ResultadoCliente.Error("El correo del cliente no tiene un formato valido.");

        var cliente = new Cliente
        {
            Nombre = cleanName,
            Nit = Limpiar(nit),
            Telefono = Limpiar(telefono),
            WhatsApp = Limpiar(whatsapp),
            Email = cleanEmail,
            Direccion = Limpiar(direccion),
            PersonaContacto = Limpiar(personaContacto),
            Notas = Limpiar(notas)
        };

        if (!LongitudesValidas(cliente))
            return ResultadoCliente.Error("Uno de los datos supera la longitud permitida.");

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoCliente.Ok("El cliente fue registrado.");
    }

    public async Task<ResultadoCliente> AsignarAsync(
        Guid clienteId, Guid promotorId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var clienteActivo = await db.Clientes.AnyAsync(x => x.Id == clienteId && x.IsActive, cancellationToken);
        var promotorActivo = await db.Promotores.AnyAsync(x => x.Id == promotorId && x.IsActive, cancellationToken);
        if (!clienteActivo || !promotorActivo)
            return ResultadoCliente.Error("Selecciona un cliente y un promotor activos.");

        if (await db.ClientesPromotores.AnyAsync(x => x.ClienteId == clienteId && x.PromotorId == promotorId, cancellationToken))
            return ResultadoCliente.Ok("El promotor ya tiene acceso a este cliente.");

        db.ClientesPromotores.Add(new ClientePromotor { ClienteId = clienteId, PromotorId = promotorId });
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoCliente.Ok("El cliente fue asignado al promotor.");
    }

    public async Task<ResultadoCliente> DesasignarAsync(
        Guid clienteId, Guid promotorId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var acceso = await db.ClientesPromotores.FindAsync([clienteId, promotorId], cancellationToken);
        if (acceso is null)
            return ResultadoCliente.Ok("La asignacion ya no estaba activa.");

        db.ClientesPromotores.Remove(acceso);
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoCliente.Ok("El acceso del promotor fue retirado.");
    }

    public async Task<ResultadoCliente> DesactivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var cliente = await db.Clientes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (cliente is null)
            return ResultadoCliente.Error("El cliente ya no esta disponible.");
        if (!cliente.IsActive)
            return ResultadoCliente.Ok("El cliente ya estaba desactivado.");

        cliente.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return ResultadoCliente.Ok("El cliente fue desactivado y su historial se conserva.");
    }

    private static string? Limpiar(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool LongitudesValidas(Cliente cliente) =>
        (cliente.Nit?.Length ?? 0) <= 40 && (cliente.Telefono?.Length ?? 0) <= 40 &&
        (cliente.WhatsApp?.Length ?? 0) <= 40 && (cliente.Email?.Length ?? 0) <= 254 &&
        (cliente.Direccion?.Length ?? 0) <= 300 && (cliente.PersonaContacto?.Length ?? 0) <= 160 &&
        (cliente.Notas?.Length ?? 0) <= 1500;
}

public sealed record PromotorAsignado(Guid Id, string Nombre);

public sealed record ClienteResumen(
    Guid Id, string Nombre, string? Nit, string? Telefono, string? WhatsApp,
    string? Email, string? Direccion, string? PersonaContacto, string? Notas,
    bool IsActive, IReadOnlyList<PromotorAsignado> Promotores);

public sealed record ResultadoCliente(bool Success, string Message)
{
    public static ResultadoCliente Ok(string message) => new(true, message);
    public static ResultadoCliente Error(string message) => new(false, message);
}
