using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InventarioSimple.Services
{
    public class CategoriaService
    {
        public void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Gestión de Categorías ===");
                Console.WriteLine("1) Listar");
                Console.WriteLine("2) Crear");
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
            var items = db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToList();
            Console.WriteLine("\nID\tNombre\t\tDescripción");
            Console.WriteLine("-------------------------------------------");
            foreach (var c in items)
            {
                Console.WriteLine($"{c.Id}\t{c.Nombre}\t\t{c.Descripcion}");
            }
            ConsoleHelpers.Pause();
        }

        private void Crear()
        {
            using var db = new AppDbContext();

            var nombre = ConsoleHelpers.ReadNonEmpty("Nombre");
            var desc = "";
            Console.Write("Descripción (opcional): ");
            desc = Console.ReadLine()?.Trim() ?? "";

            var existe = db.Categorias.Any(c => c.Nombre.ToLower() == nombre.ToLower());
            if (existe)
            {
                Console.WriteLine("⚠️  Ya existe una categoría con ese nombre.");
                ConsoleHelpers.Pause();
                return;
            }

            db.Categorias.Add(new Categoria { Nombre = nombre, Descripcion = string.IsNullOrWhiteSpace(desc) ? null : desc });
            db.SaveChanges();
            Console.WriteLine("✅ Categoría creada.");
            ConsoleHelpers.Pause();
        }

        private void Editar()
        {
            using var db = new AppDbContext();
            var id = ConsoleHelpers.ReadInt("ID de la categoría a editar");
            var cat = db.Categorias.FirstOrDefault(c => c.Id == id);
            if (cat is null)
            {
                Console.WriteLine("⚠️  No se encontró la categoría.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.WriteLine($"\nEditando: {cat.Nombre} (ID {cat.Id})");
            Console.Write($"Nuevo nombre ({cat.Nombre}): ");
            var nuevoNombre = Console.ReadLine();
            Console.Write($"Nueva descripción ({cat.Descripcion}): ");
            var nuevaDesc = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(nuevoNombre))
            {
                var existe = db.Categorias.Any(c => c.Id != id && c.Nombre.ToLower() == nuevoNombre.ToLower());
                if (existe)
                {
                    Console.WriteLine("⚠️  Ya existe otra categoría con ese nombre.");
                    ConsoleHelpers.Pause();
                    return;
                }
                cat.Nombre = nuevoNombre.Trim();
            }

            cat.Descripcion = string.IsNullOrWhiteSpace(nuevaDesc) ? cat.Descripcion : nuevaDesc.Trim();
            db.SaveChanges();
            Console.WriteLine("✅ Cambios guardados.");
            ConsoleHelpers.Pause();
        }

        private void Eliminar()
        {
            using var db = new AppDbContext();
            var id = ConsoleHelpers.ReadInt("ID de la categoría a eliminar");
            var cat = db.Categorias.FirstOrDefault(c => c.Id == id);
            if (cat is null)
            {
                Console.WriteLine("⚠️  No se encontró la categoría.");
                ConsoleHelpers.Pause();
                return;
            }

            Console.Write($"¿Confirmas eliminar '{cat.Nombre}'? (s/N): ");
            var conf = (Console.ReadLine() ?? "").Trim().ToLower();
            if (conf == "s" || conf == "si" || conf == "sí")
            {
                db.Categorias.Remove(cat);
                db.SaveChanges();
                Console.WriteLine("✅ Categoría eliminada.");
            }
            else
            {
                Console.WriteLine("Acción cancelada.");
            }
            ConsoleHelpers.Pause();
        }
    }
}
