using System;

namespace InventarioSimple.Services
{
    public static class ConsoleHelpers
    {
        public static string ReadNonEmpty(string label)
        {
            while (true)
            {
                Console.Write($"{label}: ");
                var text = Console.ReadLine()?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(text)) return text;
                Console.WriteLine("⚠️  Este campo es obligatorio.");
            }
        }

        public static string ReadPassword(string label)
        {
            Console.Write($"{label}: ");
            var pass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass[..^1];
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    pass += keyInfo.KeyChar;
                    Console.Write("*");
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            return pass;
        }

        public static int ReadInt(string label, int? defaultValue = null)
        {
            while (true)
            {
                Console.Write(defaultValue is null ? $"{label}: " : $"{label} ({defaultValue}): ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue) return defaultValue.Value;
                if (int.TryParse(input, out var value)) return value;
                Console.WriteLine("⚠️  Ingresa un número entero válido.");
            }
        }

        public static decimal ReadDecimal(string label, decimal? defaultValue = null)
        {
            while (true)
            {
                Console.Write(defaultValue is null ? $"{label}: " : $"{label} ({defaultValue}): ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue) return defaultValue.Value;
                if (decimal.TryParse(input, out var value)) return value;
                Console.WriteLine("⚠️  Ingresa un número decimal válido.");
            }
        }

        public static void Pause()
        {
            Console.WriteLine();
            Console.Write("Presiona ENTER para continuar...");
            Console.ReadLine();
        }
    }
}
