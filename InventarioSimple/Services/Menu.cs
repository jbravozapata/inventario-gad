using System;

namespace InventarioSimple.Services
{
    public class Menu
    {
        private readonly Usuario _user;

        public Menu(Usuario user)
        {
            _user = user;
        }

        public void Mostrar()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== Sistema de Inventario GAD — {_user.NombreCompleto} ({_user.Rol}) ===");
                Console.WriteLine("1) Categorías");
                Console.WriteLine("2) Productos");
                Console.WriteLine("3) Movimientos");
                Console.WriteLine("4) Reportes");

                Console.WriteLine("0) Salir");
                Console.Write("Selecciona una opción: ");
                var op = Console.ReadLine();

                switch (op)
                {
                    case "4":
                        new ReporteService().Menu();
                        break;


                    case "3":
                        new MovimientoService().Menu();
                        break;
                    
                    
                    case "2":
                        new ProductoService().Menu();
                        break;

                    
                    case "1":
                        new CategoriaService().Menu();
                        break;

                    case "0":
                        Console.WriteLine("👋 ¡Hasta pronto!");
                        return;

                    default:
                        Console.WriteLine("⚠️  Opción inválida.");
                        ConsoleHelpers.Pause();
                        break;
                }
            }
        }
    }
}
