using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InventarioSimple.Services
{
    public class MovimientoService
    {
        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Movimientos de Inventario ===");
                Console.WriteLine("1) Registrar Entrada");
                Console.WriteLine("2) Registrar Salida");
                Console.WriteLine("3) Ver historial");
                Console.WriteLine("0) Volver");
                Console.Write("Selecciona una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "1": Registrar("Entrada"); break;
                    case "2": Registrar("Salida"); break;
                    case "3": Historial(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("⚠️  Opción inválida.");
                        ConsoleHelpers.Pause();
                        break;
                }
            }
        }

        private void Registrar(string tipo)
        {
            using var db = new AppDbContext();
            var productos = db.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .AsNoTracking()
                .ToList();

            if (!productos.Any())
            {
                Console.WriteLine("⚠️  No hay productos activos registrados.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine("\nProductos disponibles:");
            foreach (var p in productos)
                Console.WriteLine($"{p.Id}) {p.Nombre} (Stock: {p.StockActual})");

            var id = ConsoleHelpers.ReadInt($"ID del producto para registrar {tipo.ToLower()}");
            var producto = db.Productos.FirstOrDefault(p => p.Id == id);

            if (producto == null)
            {
                Console.WriteLine("⚠️  Producto no encontrado.");
                ConsoleHelpers.Pause();
                return;
            }

            var cantidad = ConsoleHelpers.ReadInt($"Cantidad a registrar como {tipo.ToLower()}");

            if (tipo == "Salida" && cantidad > producto.StockActual)
            {
                Console.WriteLine("⚠️  No hay suficiente stock disponible.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.Write("Observación (opcional): ");
            var obs = Console.ReadLine()?.Trim() ?? "";

            // Actualizar stock
            if (tipo == "Entrada")
                producto.StockActual += cantidad;
            else
                producto.StockActual -= cantidad;

            // Registrar movimiento
            var mov = new Movimiento
            {
                Tipo = tipo,
                ProductoId = producto.Id,
                Cantidad = cantidad,
                Observacion = string.IsNullOrWhiteSpace(obs) ? null : obs,
                Fecha = DateTime.Now
            };

            db.Movimientos.Add(mov);
            db.SaveChanges();

            Console.WriteLine($"✅ {tipo} registrada correctamente. Stock actual: {producto.StockActual}");
            ConsoleHelpers.Pause();
        }

        private void Historial()
        {
            using var db = new AppDbContext();
            var movimientos = db.Movimientos
                .Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .Take(20)
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
