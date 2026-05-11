using System.Linq;
using BCrypt.Net;
using InventarioSimple;

namespace InventarioUI.Utils
{
    public static class DbInitializer
    {
        public static void Inicializar()
        {
            using var db = new AppDbContext();

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
            }
        }
    }
}