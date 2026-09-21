using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebPageSublimation.Features.Promotores;
using WebPageSublimation.Features.Catalogo;
using WebPageSublimation.Features.Clientes;
using WebPageSublimation.Features.Proformas;
using WebPageSublimation.Features.SolicitudesPublicas;
using WebPageSublimation.Features.Pedidos;

namespace WebPageSublimation.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Promotor> Promotores => Set<Promotor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<VarianteProducto> VariantesProducto => Set<VarianteProducto>();
    public DbSet<ImagenProducto> ImagenesProducto => Set<ImagenProducto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClientePromotor> ClientesPromotores => Set<ClientePromotor>();
    public DbSet<Proforma> Proformas => Set<Proforma>();
    public DbSet<DetalleProforma> DetallesProforma => Set<DetalleProforma>();
    public DbSet<SolicitudPublica> SolicitudesPublicas => Set<SolicitudPublica>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);

            entity.Property(user => user.CreatedAtUtc)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");

        builder.Entity<Promotor>(entity =>
        {
            entity.ToTable("promotores");
            entity.HasKey(promotor => promotor.Id);
            entity.Property(promotor => promotor.Nombre).HasMaxLength(160);
            entity.Property(promotor => promotor.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(promotor => promotor.UserId).IsUnique();
            entity.HasOne(promotor => promotor.User)
                .WithOne()
                .HasForeignKey<Promotor>(promotor => promotor.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Categoria>(entity => { entity.ToTable("categorias"); entity.Property(x => x.Nombre).HasMaxLength(120); entity.HasIndex(x => x.Nombre).IsUnique(); });
        builder.Entity<Producto>(entity => { entity.ToTable("productos"); entity.Property(x => x.Nombre).HasMaxLength(180); entity.Property(x => x.Descripcion).HasMaxLength(1000); entity.Property(x => x.PrecioComercial).HasPrecision(18, 2); entity.Property(x => x.PrecioEsReferencial).HasDefaultValue(false); entity.HasOne(x => x.Categoria).WithMany(x => x.Productos).HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict); });
        builder.Entity<VarianteProducto>(entity => { entity.ToTable("variantes_producto"); entity.Property(x => x.Nombre).HasMaxLength(160); entity.HasOne(x => x.Producto).WithMany(x => x.Variantes).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict); entity.HasIndex(x => new { x.ProductoId, x.Nombre }).IsUnique(); });
        builder.Entity<ImagenProducto>(entity =>
        {
            entity.ToTable("imagenes_producto");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContentType).HasMaxLength(80);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => new { x.ProductoId, x.Orden }).IsUnique();
            entity.HasIndex(x => new { x.ProductoId, x.EsPrincipal });
            entity.HasOne(x => x.Producto).WithMany(x => x.Imagenes).HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(180);
            entity.Property(x => x.Nit).HasMaxLength(40);
            entity.Property(x => x.Telefono).HasMaxLength(40);
            entity.Property(x => x.WhatsApp).HasMaxLength(40);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Direccion).HasMaxLength(300);
            entity.Property(x => x.PersonaContacto).HasMaxLength(160);
            entity.Property(x => x.Notas).HasMaxLength(1500);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.Nombre);
        });

        builder.Entity<ClientePromotor>(entity =>
        {
            entity.ToTable("clientes_promotores");
            entity.HasKey(x => new { x.ClienteId, x.PromotorId });
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(x => x.Cliente).WithMany(x => x.Promotores)
                .HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Promotor).WithMany()
                .HasForeignKey(x => x.PromotorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PromotorId);
        });

        builder.Entity<Proforma>(entity =>
        {
            entity.ToTable("proformas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Numero).UseIdentityByDefaultColumn();
            entity.HasIndex(x => x.Numero).IsUnique();
            entity.Property(x => x.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.ClienteNombre).HasMaxLength(180);
            entity.Property(x => x.PromotorNombre).HasMaxLength(160);
            entity.Property(x => x.Observaciones).HasMaxLength(1500);
            entity.Property(x => x.Total).HasPrecision(18, 5);
            entity.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Promotor).WithMany().HasForeignKey(x => x.PromotorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ClienteId);
            entity.HasIndex(x => x.PromotorId);
        });

        builder.Entity<DetalleProforma>(entity =>
        {
            entity.ToTable("detalles_proforma");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductoNombre).HasMaxLength(180);
            entity.Property(x => x.VarianteNombre).HasMaxLength(160);
            entity.Property(x => x.Cantidad).HasPrecision(18, 3);
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 5);
            entity.HasOne(x => x.Proforma).WithMany(x => x.Detalles).HasForeignKey(x => x.ProformaId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ProformaId);
            entity.HasIndex(x => x.ProductoId);
        });

        builder.Entity<SolicitudPublica>(entity =>
        {
            entity.ToTable("solicitudes_publicas");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(160);
            entity.Property(x => x.Empresa).HasMaxLength(180);
            entity.Property(x => x.Telefono).HasMaxLength(40);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Detalle).HasMaxLength(2000);
            entity.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        builder.Entity<Pedido>(entity =>
        {
            entity.ToTable("pedidos"); entity.HasKey(x=>x.Id); entity.Property(x=>x.Numero).UseIdentityByDefaultColumn(); entity.HasIndex(x=>x.Numero).IsUnique();
            entity.Property(x=>x.FechaUtc).HasDefaultValueSql("CURRENT_TIMESTAMP"); entity.Property(x=>x.Estado).HasConversion<string>().HasMaxLength(40);
            entity.Property(x=>x.ClienteNombre).HasMaxLength(180); entity.Property(x=>x.PromotorNombre).HasMaxLength(160); entity.Property(x=>x.Observaciones).HasMaxLength(1500); entity.Property(x=>x.Total).HasPrecision(18,5);
            entity.HasOne(x=>x.Cliente).WithMany().HasForeignKey(x=>x.ClienteId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(x=>x.Promotor).WithMany().HasForeignKey(x=>x.PromotorId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x=>x.Proforma).WithMany().HasForeignKey(x=>x.ProformaId).OnDelete(DeleteBehavior.Restrict); entity.HasIndex(x=>x.ProformaId).IsUnique(); entity.HasIndex(x=>x.ClienteId); entity.HasIndex(x=>x.PromotorId); entity.HasIndex(x=>x.FechaUtc);
            entity.HasIndex(x=>x.ClaveIdempotencia).IsUnique();
        });
        builder.Entity<DetallePedido>(entity =>
        {
            entity.ToTable("detalles_pedido"); entity.HasKey(x=>x.Id); entity.Property(x=>x.ProductoNombre).HasMaxLength(180); entity.Property(x=>x.VarianteNombre).HasMaxLength(160); entity.Property(x=>x.Especificacion).HasMaxLength(500); entity.Property(x=>x.Cantidad).HasPrecision(18,3); entity.Property(x=>x.PrecioUnitario).HasPrecision(18,2); entity.Property(x=>x.Subtotal).HasPrecision(18,5);
            entity.HasOne(x=>x.Pedido).WithMany(x=>x.Detalles).HasForeignKey(x=>x.PedidoId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(x=>x.Producto).WithMany().HasForeignKey(x=>x.ProductoId).OnDelete(DeleteBehavior.Restrict); entity.HasIndex(x=>x.PedidoId); entity.HasIndex(x=>x.ProductoId);
        });
    }
}
