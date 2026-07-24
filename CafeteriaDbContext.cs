using Microsoft.EntityFrameworkCore;

public class CafeteriaDbContext : DbContext
{
    public CafeteriaDbContext(DbContextOptions<CafeteriaDbContext> options) : base(options) { }

    public DbSet<ProductoEntity> Productos { get; set; }
    public DbSet<TransaccionEntity> Transacciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        modelBuilder.Entity<TransaccionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PrecioAnterior).HasColumnType("decimal(10,2)");
            entity.Property(e => e.PrecioNuevo).HasColumnType("decimal(10,2)");
        });
    }
}

public class ProductoEntity
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int Existencia { get; set; }
}

public class TransaccionEntity
{
    public int Id { get; set; }
    public string Tipo { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public int Cantidad { get; set; }
    public decimal? PrecioAnterior { get; set; }
    public decimal? PrecioNuevo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}
