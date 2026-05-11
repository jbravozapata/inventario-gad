using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InventarioSimple.Services
{
    public class ProductoService
    {
        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Gestión de Bienes e Insumos ===");
                Console.WriteLine("1) Listar");
                Console.WriteLine("2) Registrar nuevo");
                Console.WriteLine("3) Editar");
                Console.WriteLine("4) Eliminar");
                Console.WriteLine("0) Volver");
                Console.Write("Selecciona una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "1": Listar(); break;
                    case "2": Crear(); break;
                    case "3": Editar(); break;
                    case "4": Eliminar(); break;
                    case "0": return;
                    default:
                        Console.WriteLine("⚠️  Opción inválida.");
                        ConsoleHelpers.Pause();
                        break;
                }
            }
        }

        private void Listar()
        {
            using var db = new AppDbContext();
            var productos = db.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToList();

            Console.WriteLine("\nID\tCódigo\t\tNombre\t\tCategoría\tCantidad\tEstado");
            Console.WriteLine("----------------------------------------------------------------");

            foreach (var p in productos)
            {
                var categoria = db.Categorias.FirstOrDefault(c => c.Id == p.CategoriaId);
                var estado = p.Activo ? "Activo" : "Inactivo";
                Console.WriteLine($"{p.Id}\t{p.CodigoInterno}\t{p.Nombre}\t{categoria?.Nombre}\t{p.StockActual}\t{estado}");
            }

            ConsoleHelpers.Pause();
        }

        private void Crear()
        {
            using var db = new AppDbContext();

            Console.WriteLine("\n=== Registrar nuevo bien o insumo ===");
            var codigo = ConsoleHelpers.ReadNonEmpty("Código interno");

            if (db.Productos.Any(p => p.CodigoInterno.ToLower() == codigo.ToLower()))
            {
                Console.WriteLine("⚠️  Ya existe un producto con ese código.");
                ConsoleHelpers.Pause();
                return;
            }

            var nombre = ConsoleHelpers.ReadNonEmpty("Nombre del bien o insumo");
            Console.Write("Descripción (opcional): ");
            var desc = Console.ReadLine()?.Trim() ?? "";
            var stock = ConsoleHelpers.ReadInt("Cantidad actual");

            var categorias = db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToList();
            if (!categorias.Any())
            {
                Console.WriteLine("⚠️  Debes crear al menos una categoría antes de registrar productos.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine("\nCategorías disponibles:");
            foreach (var c in categorias)
                Console.WriteLine($"{c.Id}) {c.Nombre}");

            var catId = ConsoleHelpers.ReadInt("ID de la categoría");
            if (!db.Categorias.Any(c => c.Id == catId))
            {
                Console.WriteLine("⚠️  Categoría no válida.");
                ConsoleHelpers.Pause();
                return;
            }

            db.Productos.Add(new Producto
            {
                CodigoInterno = codigo,
                Nombre = nombre,
                Descripcion = string.IsNullOrWhiteSpace(desc) ? null : desc,
                StockActual = stock,
                CategoriaId = catId,
                Activo = true
            });

            db.SaveChanges();
            Console.WriteLine("✅ Bien registrado correctamente.");
            ConsoleHelpers.Pause();
        }

        private void Editar()
        {
            using var db = new AppDbContext();
            var id = ConsoleHelpers.ReadInt("ID del bien a editar");
            var p = db.Productos.FirstOrDefault(p => p.Id == id);
            if (p == null)
            {
                Console.WriteLine("⚠️  Bien no encontrado.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine($"\nEditando: {p.Nombre} (ID {p.Id})");
            Console.Write($"Nuevo nombre ({p.Nombre}): ");
            var nuevoNombre = Console.ReadLine();
            Console.Write($"Nueva descripción ({p.Descripcion}): ");
            var nuevaDesc = Console.ReadLine();
            var nuevoStock = ConsoleHelpers.ReadInt("Nueva cantidad actual", p.StockActual);

            Console.Write($"¿Deseas cambiar el estado actual ({(p.Activo ? "Activo" : "Inactivo")})? (s/N): ");
            var cambiar = (Console.ReadLine() ?? "").Trim().ToLower();
            if (cambiar == "s" || cambiar == "si" || cambiar == "sí")
                p.Activo = !p.Activo;

            if (!string.IsNullOrWhiteSpace(nuevoNombre)) p.Nombre = nuevoNombre.Trim();
            if (!string.IsNullOrWhiteSpace(nuevaDesc)) p.Descripcion = nuevaDesc.Trim();
            p.StockActual = nuevoStock;

            db.SaveChanges();
            Console.WriteLine("✅ Cambios guardados.");
            ConsoleHelpers.Pause();
        }

        private void Eliminar()
        {
            using var db = new AppDbContext();
            var id = ConsoleHelpers.ReadInt("ID del bien a eliminar");
            var p = db.Productos.FirstOrDefault(p => p.Id == id);
            if (p == null)
            {
                Console.WriteLine("⚠️  Bien no encontrado.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.Write($"¿Confirmas eliminar '{p.Nombre}'? (s/N): ");
            var conf = (Console.ReadLine() ?? "").Trim().ToLower();
            if (conf == "s" || conf == "si" || conf == "sí")
            {
                db.Productos.Remove(p);
                db.SaveChanges();
                Console.WriteLine("✅ Bien eliminado.");
            }
            else
            {
                Console.WriteLine("Acción cancelada.");
            }
            ConsoleHelpers.Pause();
        }
    }
}
