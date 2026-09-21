using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Data;
using WebPageSublimation.Features.Proformas;
using WebPageSublimation.Security;

namespace WebPageSublimation.Features.Pedidos;

public sealed class PedidoService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<ResultadoPedido> CrearDirectoAsync(ClaimsPrincipal principal, string? clienteId, IReadOnlyList<string> productoIds, IReadOnlyList<string> cantidades, IReadOnlyList<string> varianteIds, IReadOnlyList<string> especificaciones, string? fechaRequerida, string? observaciones, string? claveIdempotencia, CancellationToken ct=default)
    {
        var userId=principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrWhiteSpace(userId)||!Guid.TryParse(clienteId,out var clienteGuid)) return ResultadoPedido.Error("Selecciona un cliente válido.");
        if(productoIds.Count==0||productoIds.Count!=cantidades.Count) return ResultadoPedido.Error("Agrega al menos un producto con su cantidad.");
        if(!Guid.TryParse(claveIdempotencia,out var idempotencyKey)) return ResultadoPedido.Error("La solicitud del pedido no es válida. Recarga el formulario.");
        if(!TryDate(fechaRequerida,out var required)) return ResultadoPedido.Error("La fecha requerida no es válida.");
        var notes=Clean(observaciones,1500); if(observaciones?.Trim().Length>1500) return ResultadoPedido.Error("Las observaciones pueden tener hasta 1500 caracteres.");
        var lines=new List<(Guid ProductId,Guid? VariantId,decimal Quantity,string? Specification)>();
        for(var i=0;i<productoIds.Count;i++){
            if(!Guid.TryParse(productoIds[i],out var productId)||!TryDecimal(cantidades[i],out var quantity)||quantity<=0||quantity>999_999_999_999_999.999m||decimal.Round(quantity,3)!=quantity) return ResultadoPedido.Error("Revisa las cantidades del pedido.");
            Guid? variantId=null;if(i<varianteIds.Count&&!string.IsNullOrWhiteSpace(varianteIds[i])){if(!Guid.TryParse(varianteIds[i],out var parsedVariant))return ResultadoPedido.Error("La variante seleccionada no es válida.");variantId=parsedVariant;}
            var specification=i<especificaciones.Count?Clean(especificaciones[i],500):null;
            if(i<especificaciones.Count&&especificaciones[i].Trim().Length>500) return ResultadoPedido.Error("Cada especificación puede tener hasta 500 caracteres.");
            lines.Add((productId,variantId,quantity,specification));
        }
        await using var db=await dbFactory.CreateDbContextAsync(ct);
        var promoter=await db.Promotores.SingleOrDefaultAsync(x=>x.UserId==userId&&x.IsActive,ct);
        if(promoter is null) return ResultadoPedido.Error("Tu perfil de promotor no está activo.");
        var existing=await db.Pedidos.AsNoTracking().Where(x=>x.ClaveIdempotencia==idempotencyKey&&x.PromotorId==promoter.Id).Select(x=>x.Id).SingleOrDefaultAsync(ct);if(existing!=Guid.Empty)return ResultadoPedido.Ok("El pedido ya había sido registrado.",existing);
        var client=await db.Clientes.SingleOrDefaultAsync(x=>x.Id==clienteGuid&&x.IsActive&&x.Promotores.Any(a=>a.PromotorId==promoter.Id),ct);
        if(client is null) return ResultadoPedido.Error("El cliente no está disponible para tu cuenta.");
        var ids=lines.Select(x=>x.ProductId).Distinct().ToArray();
        var products=await db.Productos.AsNoTracking().Where(x=>ids.Contains(x.Id)&&x.IsActive&&x.Categoria!.IsActive&&!x.PrecioEsReferencial).ToDictionaryAsync(x=>x.Id,ct);
        if(products.Count!=ids.Length) return ResultadoPedido.Error("Uno de los productos ya no está disponible.");
        var requestedVariants=lines.Where(x=>x.VariantId is not null).Select(x=>x.VariantId!.Value).Distinct().ToArray();var variants=await db.VariantesProducto.AsNoTracking().Where(x=>requestedVariants.Contains(x.Id)&&x.IsActive).ToDictionaryAsync(x=>x.Id,ct);if(variants.Count!=requestedVariants.Length||lines.Any(x=>x.VariantId is not null&&variants[x.VariantId.Value].ProductoId!=x.ProductId))return ResultadoPedido.Error("Una variante no pertenece al producto seleccionado o ya no está activa.");
        var order=new Pedido{ClienteId=client.Id,ClienteNombre=client.Nombre,PromotorId=promoter.Id,PromotorNombre=promoter.Nombre,FechaRequerida=required,Observaciones=notes,ClaveIdempotencia=idempotencyKey};
        try{foreach(var line in lines){var product=products[line.ProductId];var subtotal=ProformaCalculos.CalcularSubtotal(line.Quantity,product.PrecioComercial);if(subtotal>9_999_999_999_999.99999m)return ResultadoPedido.Error("Un subtotal supera el límite permitido.");order.Detalles.Add(new DetallePedido{ProductoId=product.Id,ProductoNombre=product.Nombre,VarianteNombre=line.VariantId is null?null:variants[line.VariantId.Value].Nombre,Especificacion=line.Specification,Cantidad=line.Quantity,PrecioUnitario=product.PrecioComercial,Subtotal=subtotal});}order.Total=ProformaCalculos.CalcularTotal(order.Detalles.Select(x=>x.Subtotal));if(order.Total>9_999_999_999_999.99999m)return ResultadoPedido.Error("El total del pedido supera el límite permitido.");}catch(OverflowException){return ResultadoPedido.Error("El total del pedido supera el límite permitido.");}
        db.Pedidos.Add(order);try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){existing=await db.Pedidos.AsNoTracking().Where(x=>x.ClaveIdempotencia==idempotencyKey&&x.PromotorId==promoter.Id).Select(x=>x.Id).SingleOrDefaultAsync(ct);if(existing!=Guid.Empty)return ResultadoPedido.Ok("El pedido ya había sido registrado.",existing);throw;}return ResultadoPedido.Ok("El pedido fue creado.",order.Id);
    }

    public async Task<ResultadoPedido> ConvertirAsync(ClaimsPrincipal principal,Guid proformaId,string? fechaRequerida,CancellationToken ct=default)
    {
        if(!TryDate(fechaRequerida,out var required)) return ResultadoPedido.Error("La fecha requerida no es válida.");
        var userId=principal.FindFirstValue(ClaimTypes.NameIdentifier);if(string.IsNullOrWhiteSpace(userId)) return ResultadoPedido.Error("La sesión no es válida.");
        await using var db=await dbFactory.CreateDbContextAsync(ct);
        var existing=await db.Pedidos.AsNoTracking().Where(x=>x.ProformaId==proformaId).Select(x=>x.Id).SingleOrDefaultAsync(ct);if(existing!=Guid.Empty)return ResultadoPedido.Ok("La proforma ya estaba convertida en pedido.",existing);
        var quote=await db.Proformas.AsNoTracking().Include(x=>x.Detalles).SingleOrDefaultAsync(x=>x.Id==proformaId&&x.Promotor!.UserId==userId,ct);if(quote is null)return ResultadoPedido.Error("La proforma no está disponible para tu cuenta.");
        var order=new Pedido{ClienteId=quote.ClienteId,ClienteNombre=quote.ClienteNombre,PromotorId=quote.PromotorId,PromotorNombre=quote.PromotorNombre,ProformaId=quote.Id,FechaRequerida=required,Observaciones=quote.Observaciones,Total=quote.Total};
        foreach(var line in quote.Detalles)order.Detalles.Add(new DetallePedido{ProductoId=line.ProductoId,ProductoNombre=line.ProductoNombre,VarianteNombre=line.VarianteNombre,Cantidad=line.Cantidad,PrecioUnitario=line.PrecioUnitario,Subtotal=line.Subtotal});
        db.Pedidos.Add(order);try{await db.SaveChangesAsync(ct);}catch(DbUpdateException){existing=await db.Pedidos.AsNoTracking().Where(x=>x.ProformaId==proformaId).Select(x=>x.Id).SingleOrDefaultAsync(ct);if(existing!=Guid.Empty)return ResultadoPedido.Ok("La proforma ya estaba convertida en pedido.",existing);throw;}return ResultadoPedido.Ok("La proforma fue convertida en pedido.",order.Id);
    }

    public async Task<ResultadoPedido> CambiarEstadoAsync(Guid id,string? estado,CancellationToken ct=default){if(!Enum.TryParse<EstadoPedido>(estado,true,out var parsed)||!Enum.IsDefined(parsed))return ResultadoPedido.Error("Selecciona un estado válido.");await using var db=await dbFactory.CreateDbContextAsync(ct);var order=await db.Pedidos.SingleOrDefaultAsync(x=>x.Id==id,ct);if(order is null)return ResultadoPedido.Error("El pedido no existe.");order.Estado=parsed;await db.SaveChangesAsync(ct);return ResultadoPedido.Ok("El estado fue actualizado.",id);}
    public async Task<IReadOnlyList<PedidoResumen>> ListarAsync(ClaimsPrincipal principal,bool admin,DateOnly? desde=null,DateOnly? hasta=null,Guid? clienteId=null,Guid? promotorId=null,CancellationToken ct=default){await using var db=await dbFactory.CreateDbContextAsync(ct);var q=db.Pedidos.AsNoTracking().AsQueryable();if(!admin){var uid=principal.FindFirstValue(ClaimTypes.NameIdentifier);q=q.Where(x=>x.Promotor!.UserId==uid);}if(desde is not null){var d=desde.Value.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc);q=q.Where(x=>x.FechaUtc>=d);}if(hasta is not null){var h=hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc);q=q.Where(x=>x.FechaUtc<h);}if(clienteId is not null)q=q.Where(x=>x.ClienteId==clienteId);if(promotorId is not null)q=q.Where(x=>x.PromotorId==promotorId);return await q.OrderByDescending(x=>x.Numero).Select(x=>new PedidoResumen(x.Id,x.Numero,x.FechaUtc,x.FechaRequerida,x.ClienteNombre,x.PromotorNombre,x.Estado,x.Total,x.ProformaId)).ToListAsync(ct);}
    public async Task<PedidoDetalle?> ObtenerAsync(ClaimsPrincipal principal,Guid id,CancellationToken ct=default){await using var db=await dbFactory.CreateDbContextAsync(ct);var q=db.Pedidos.AsNoTracking().Where(x=>x.Id==id);if(!principal.IsInRole(Roles.Administrador)){var uid=principal.FindFirstValue(ClaimTypes.NameIdentifier);q=q.Where(x=>x.Promotor!.UserId==uid);}return await q.Select(x=>new PedidoDetalle(x.Id,x.Numero,x.FechaUtc,x.FechaRequerida,x.ClienteNombre,x.PromotorNombre,x.Estado,x.Observaciones,x.Total,x.ProformaId,x.Detalles.OrderBy(d=>d.Id).Select(d=>new LineaPedido(d.ProductoNombre,d.VarianteNombre,d.Especificacion,d.Cantidad,d.PrecioUnitario,d.Subtotal)).ToList())).SingleOrDefaultAsync(ct);}
    private static string? Clean(string? value,int max)=>string.IsNullOrWhiteSpace(value)?null:value.Trim()[..Math.Min(value.Trim().Length,max)];
    private static bool TryDecimal(string value,out decimal result)=>decimal.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture,out result)||decimal.TryParse(value,NumberStyles.Number,CultureInfo.GetCultureInfo("es-BO"),out result);
    private static bool TryDate(string? value,out DateOnly? result){result=null;if(string.IsNullOrWhiteSpace(value))return true;if(DateOnly.TryParseExact(value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)){result=date;return true;}return false;}
}
public sealed record PedidoResumen(Guid Id,long Numero,DateTime FechaUtc,DateOnly? FechaRequerida,string ClienteNombre,string PromotorNombre,EstadoPedido Estado,decimal Total,Guid? ProformaId);
public sealed record LineaPedido(string ProductoNombre,string? VarianteNombre,string? Especificacion,decimal Cantidad,decimal PrecioUnitario,decimal Subtotal);
public sealed record PedidoDetalle(Guid Id,long Numero,DateTime FechaUtc,DateOnly? FechaRequerida,string ClienteNombre,string PromotorNombre,EstadoPedido Estado,string? Observaciones,decimal Total,Guid? ProformaId,IReadOnlyList<LineaPedido> Lineas);
public sealed record ResultadoPedido(bool Success,string Message,Guid? PedidoId=null){public static ResultadoPedido Ok(string m,Guid id)=>new(true,m,id);public static ResultadoPedido Error(string m)=>new(false,m);}
