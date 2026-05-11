using System;
using System.IO;
using InventarioSimple.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioSimple
{
    // ==============================
    // ENTIDADES INTERNAS (solo estas)
    // ==============================

    public class Usuario
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Administrador";
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class Producto
    {
        public int Id { get; set; }
        public string CodigoInterno { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int StockActual { get; set; } = 0;
        public bool Activo { get; set; } = true;

        public int? UbicacionId { get; set; }
        public Ubicacion? Ubicacion { get; set; }
    }

    public class Movimiento
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Tipo { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public string? Observacion { get; set; }

        public Producto? Producto { get; set; }
    }

    // ==============================
    // DB CONTEXT
    // ==============================

    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
        public DbSet<Movimiento> Movimientos => Set<Movimiento>();
        public DbSet<Bien> Bienes => Set<Bien>();

        // ✅ Movimientos de bienes
        public DbSet<MovimientoBien> MovimientosBienes => Set<MovimientoBien>();

        public AppDbContext()
        {
            // ✅ Sin migraciones (tu decisión)
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
         
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "inventario_simple.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Índices únicos existentes (no tocar)
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.CodigoInterno)
                .IsUnique();

            // ✅ Relaciones para MovimientoBien (para que NO se bugueen los combos/joins)
            modelBuilder.Entity<MovimientoBien>()
                .HasOne(m => m.Bien)
                .WithMany()
                .HasForeignKey(m => m.BienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoBien>()
                .HasOne(m => m.UbicacionOrigen)
                .WithMany()
                .HasForeignKey(m => m.UbicacionOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoBien>()
                .HasOne(m => m.UbicacionDestino)
                .WithMany()
                .HasForeignKey(m => m.UbicacionDestinoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
