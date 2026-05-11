using System.IO;
using System.Text.RegularExpressions;

namespace InventarioUI.Utils
{
    public static class StoragePaths
    {
        // Carpeta base del sistema (en Documentos del usuario)
        // Queda algo como: C:\Users\Alex\Documents\InventarioGAD_Caracol
        public static string BaseFolder
        {
            get
            {
                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                Directory.CreateDirectory(basePath);
                return basePath;
            }
        }

        // Carpeta donde van todas las fotos de bienes
        public static string BienesFotosFolder
        {
            get
            {
                var p = Path.Combine(BaseFolder, "Bienes", "Fotos");
                Directory.CreateDirectory(p);
                return p;
            }
        }

        // Devuelve la carpeta de un bien, y la crea si no existe
        // Ejemplo: ...\Bienes\Fotos\0001_PC_ESCRITORIO_01-03-001
        public static string GetFolderForBien(int bienId, string? descripcion, string? codigoContable)
        {
            var safeDesc = ToSafeFolderName(descripcion ?? "SIN_DESCRIPCION");
            var safeCod = ToSafeFolderName(codigoContable ?? "SIN_CODIGO");

            var folderName = $"{bienId:D4}_{safeDesc}_{safeCod}";
            var fullPath = Path.Combine(BienesFotosFolder, folderName);

            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        // Limpia texto para nombre de carpeta
        private static string ToSafeFolderName(string input)
        {
            input = input.Trim();

            // Reemplazar caracteres no válidos por guion bajo
            var invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var pattern = $"[{Regex.Escape(invalid)}]";
            input = Regex.Replace(input, pattern, "_");

            // Compactar espacios
            input = Regex.Replace(input, @"\s+", "_");

            // Limitar longitud para evitar rutas gigantes
            if (input.Length > 40) input = input.Substring(0, 40);

            return input.ToUpperInvariant();
        }
    }
}
