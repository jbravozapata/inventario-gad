using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InventarioSimple.Services
{
    public class ReporteService
    {
        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Reportes del Inventario ===");
                Console.WriteLine("1) Listado general de productos");
                Console.WriteLine("2) Productos con bajo stock o sin existencias");
                Console.WriteLine("3) Resumen de movimientos recientes");
                Console.WriteLine("0) Volver");
                Console.Write("Selecciona una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "1": ReporteGeneral(); break;
                    case "2": ReporteStockBajo(); break;
                    case "3": ReporteMovimientos(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("⚠️  Opción inválida.");
                        ConsoleHelpers.Pause();
                        break;
                }
            }
        }

        private void ReporteGeneral()
        {
            using var db = new AppDbContext();
            var productos = db.Productos
                .Include(p => p.CategoriaId)
                .OrderBy(p => p.Nombre)
                .ToList();

            Console.WriteLine("\nID\tCódigo\t\tNombre\t\tCategoría\tStock\tEstado");
            Console.WriteLine("----------------------------------------------------------------");

            foreach (var p in productos)
            {
                var categoria = db.Categorias.FirstOrDefault(c => c.Id == p.CategoriaId);
                var estado = p.Activo ? "Activo" : "Inactivo";
                Console.WriteLine($"{p.Id}\t{p.CodigoInterno}\t{p.Nombre}\t{categoria?.Nombre}\t{p.StockActual}\t{estado}");
            }

            ConsoleHelpers.Pause();
        }

        private void ReporteStockBajo()
        {
            using var db = new AppDbContext();
            var productos = db.Productos
                .Where(p => p.StockActual <= 0 || p.StockActual < 5)
                .OrderBy(p => p.StockActual)
                .ToList();

            if (!productos.Any())
            {
                Console.WriteLine("\n✅ Todos los productos tienen stock suficiente.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine("\n=== Productos con bajo stock o sin existencias ===");
            Console.WriteLine("Código\t\tNombre\t\tStock\tCategoría");
            Console.WriteLine("---------------------------------------------------");

            foreach (var p in productos)
            {
                var categoria = db.Categorias.FirstOrDefault(c => c.Id == p.CategoriaId);
                Console.WriteLine($"{p.CodigoInterno}\t{p.Nombre}\t{p.StockActual}\t{categoria?.Nombre}");
            }

            ConsoleHelpers.Pause();
        }

        private void ReporteMovimientos()
        {
            using var db = new AppDbContext();
            var movimientos = db.Movimientos
                .Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .Take(15)
                .ToList();

            if (!movimientos.Any())
            {
                Console.WriteLine("\n⚠️  No hay movimientos registrados.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine("\nFecha\t\tTipo\tProducto\tCantidad\tObservación");
            Console.WriteLine("-----------------------------------------------------------------");

            foreach (var m in movimientos)
            {
                Console.WriteLine($"{m.Fecha:g}\t{m.Tipo}\t{m.Producto?.Nombre}\t{m.Cantidad}\t{m.Observacion}");
            }

            ConsoleHelpers.Pause();
        }
    }
}
