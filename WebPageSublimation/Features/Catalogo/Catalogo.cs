namespace WebPageSublimation.Features.Catalogo;

public sealed class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Producto> Productos { get; set; } = [];
}

public sealed class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public Guid CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public decimal PrecioComercial { get; set; }
    public bool PrecioEsReferencial { get; set; }
    public bool IsActive { get; set; } = true;
    public List<VarianteProducto> Variantes { get; set; } = [];
    public List<ImagenProducto> Imagenes { get; set; } = [];
}

public sealed class VarianteProducto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public required string Nombre { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ImagenProducto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Producto? Producto { get; set; }
    public required string ContentType { get; set; }
    public required byte[] Data { get; set; }
    public bool EsPrincipal { get; set; }
    public int Orden { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
