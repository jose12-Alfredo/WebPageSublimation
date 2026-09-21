using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Pedidos;

namespace WebPageSublimation.Features.Dashboard;

public sealed class DashboardService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<DashboardResumen> ObtenerAsync(ClaimsPrincipal principal,bool admin,DateOnly? desde,DateOnly? hasta,Guid? clienteId,Guid? promotorId,CancellationToken ct=default)
    {
        await using var db=await factory.CreateDbContextAsync(ct);var orders=db.Pedidos.AsNoTracking().AsQueryable();var quotes=db.Proformas.AsNoTracking().AsQueryable();
        if(!admin){var uid=principal.FindFirstValue(ClaimTypes.NameIdentifier);orders=orders.Where(x=>x.Promotor!.UserId==uid);quotes=quotes.Where(x=>x.Promotor!.UserId==uid);}
        if(desde is not null){var d=desde.Value.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc);orders=orders.Where(x=>x.FechaUtc>=d);quotes=quotes.Where(x=>x.FechaUtc>=d);}if(hasta is not null){var h=hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc);orders=orders.Where(x=>x.FechaUtc<h);quotes=quotes.Where(x=>x.FechaUtc<h);}if(clienteId is not null){orders=orders.Where(x=>x.ClienteId==clienteId);quotes=quotes.Where(x=>x.ClienteId==clienteId);}if(promotorId is not null){orders=orders.Where(x=>x.PromotorId==promotorId);quotes=quotes.Where(x=>x.PromotorId==promotorId);}
        var totals=await orders.GroupBy(_=>1).Select(group=>new
        {
            Total=group.Where(x=>x.Estado==EstadoPedido.Entregado).Sum(x=>(decimal?)x.Total)??0,
            Delivered=group.Count(x=>x.Estado==EstadoPedido.Entregado),
            Pending=group.Count(x=>x.Estado!=EstadoPedido.Entregado&&x.Estado!=EstadoPedido.Cancelado),
            Orders=group.Count()
        }).SingleOrDefaultAsync(ct);
        var sales=orders.Where(x=>x.Estado==EstadoPedido.Entregado);
        var groupedSales=await sales
            .GroupBy(x=>new{x.PromotorId,x.PromotorNombre})
            .Select(g=>new{g.Key.PromotorId,g.Key.PromotorNombre,Cantidad=g.Count(),Total=g.Sum(x=>x.Total)})
            .OrderByDescending(x=>x.Total)
            .ToListAsync(ct);
        var byPromoter=groupedSales
            .Select(x=>new VentaPromotor(x.PromotorId,x.PromotorNombre,x.Cantidad,x.Total))
            .ToList();
        var recent=await quotes.OrderByDescending(x=>x.FechaUtc).Take(8).Select(x=>new ProformaReciente(x.Id,x.Numero,x.FechaUtc,x.ClienteNombre,x.PromotorNombre,x.Total)).ToListAsync(ct);
        return new DashboardResumen(totals?.Total??0,totals?.Delivered??0,totals?.Pending??0,totals?.Orders??0,byPromoter,recent);
    }
}
public sealed record DashboardResumen(decimal TotalVentas,int VentasEntregadas,int PedidosPendientes,int PedidosPeriodo,IReadOnlyList<VentaPromotor> VentasPorPromotor,IReadOnlyList<ProformaReciente> ProformasRecientes);
public sealed record VentaPromotor(Guid PromotorId,string PromotorNombre,int Cantidad,decimal Total);
public sealed record ProformaReciente(Guid Id,long Numero,DateTime FechaUtc,string ClienteNombre,string PromotorNombre,decimal Total);
