using System;
using System.Linq;
using BCrypt.Net;
using InventarioSimple.Services;

namespace InventarioSimple
{
    internal class Program
    {
        static void Main()
        {
            // Crear DB y seed si hace falta
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                if (!db.Usuarios.Any())
                {
                    db.Usuarios.Add(new Usuario
                    {
                        NombreCompleto = "Administrador del Sistema",
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Rol = "Administrador",
                        Activo = true
                    });
                    db.SaveChanges();
                    Console.WriteLine("✅ Base creada y usuario admin generado.");
                }
            }

            // Login
            var auth = new AuthService();
            var user = auth.Login();
            if (user is null) return;

            // Menú principal
            var menu = new Menu(user);
            menu.Mostrar();
        }
    }
}
