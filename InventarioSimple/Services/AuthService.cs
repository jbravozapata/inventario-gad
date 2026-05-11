using System;
using System.Linq;
using BCrypt.Net;

namespace InventarioSimple.Services
{
    public class AuthService
    {
        public Usuario? Login()
        {
            // 3 intentos
            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine("\n=== Iniciar sesión ===");
                var username = ConsoleHelpers.ReadNonEmpty("Usuario");
                var password = ConsoleHelpers.ReadPassword("Contraseña");

                using var db = new AppDbContext();
                var user = db.Usuarios.FirstOrDefault(u => u.Username == username && u.Activo);

                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    Console.WriteLine($"✅ Bienvenido, {user.NombreCompleto} ({user.Rol}).");
                    return user;
                }

                Console.WriteLine("❌ Credenciales inválidas o usuario inactivo.");
                if (i < 3) Console.WriteLine($"Intentos restantes: {3 - i}");
            }

            Console.WriteLine("⛔ Se superó el número de intentos.");
            return null;
        }
    }
}
