using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace InventarioUI.Utils
{
    public static class BackupService
    {
        // 📁 Ruta donde está la BD (junto al .exe)
        private static string BasePath => StoragePaths.BaseFolder;
        private static string DbPath =>
    Path.Combine(BasePath, "inventario_simple.db");

        // 📁 Ruta REAL de archivos (la que ya usas en StoragePaths)
        private static string DataPath =>
    Path.Combine(BasePath, "Bienes"); // o como tengas tus fotos

        // =========================
        // EXPORTAR BACKUP
        // =========================
        public static void ExportarBackup(string destinoZip)
        {
            try
            {
                var tempFolder = Path.Combine(Path.GetTempPath(), "InventarioBackup");

                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);

                Directory.CreateDirectory(tempFolder);

                // 🔹 Copiar BD
                if (File.Exists(DbPath))
                {
                    File.Copy(DbPath, Path.Combine(tempFolder, "inventario_simple.db"), true);
                }
                else
                {
                    MessageBox.Show("No se encontró la base de datos.", "Error");
                    return;
                }

                // 🔹 Copiar archivos (fotos)
                var dataDestino = Path.Combine(tempFolder, "data");

                if (Directory.Exists(DataPath))
                {
                    CopyDirectory(DataPath, dataDestino);
                }

                // 🔹 Crear ZIP
                if (File.Exists(destinoZip))
                    File.Delete(destinoZip);

                ZipFile.CreateFromDirectory(tempFolder, destinoZip);

                Directory.Delete(tempFolder, true);

                MessageBox.Show("Backup generado correctamente.", "Éxito");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar backup:\n{ex.Message}");
            }
        }

        // =========================
        // IMPORTAR BACKUP
        // =========================
        public static void ImportarBackup(string zipPath)
        {
            try
            {
                var tempFolder = Path.Combine(Path.GetTempPath(), "InventarioRestore");

                if (Directory.Exists(tempFolder))
                    Directory.Delete(tempFolder, true);

                ZipFile.ExtractToDirectory(zipPath, tempFolder);

                // 🔹 Validar BD
                var dbBackup = Path.Combine(tempFolder, "inventario_simple.db");

                if (!File.Exists(dbBackup))
                {
                    MessageBox.Show("El backup no contiene la base de datos.", "Error");
                    return;
                }

                // 🔹 Reemplazar BD
                File.Copy(dbBackup, DbPath, true);

                // 🔹 Restaurar archivos
                var dataBackup = Path.Combine(tempFolder, "data");

                if (Directory.Exists(dataBackup))
                {
                    if (Directory.Exists(DataPath))
                        Directory.Delete(DataPath, true);

                    CopyDirectory(dataBackup, DataPath);
                }

                Directory.Delete(tempFolder, true);

                MessageBox.Show("Backup restaurado correctamente.\nReinicia el sistema.", "Éxito");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar backup:\n{ex.Message}");
            }
        }

        // =========================
        // UTILIDAD
        // =========================
        private static void CopyDirectory(string source, string destination)
        {
            Directory.CreateDirectory(destination);

            foreach (var file in Directory.GetFiles(source))
            {
                var dest = Path.Combine(destination, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (var dir in Directory.GetDirectories(source))
            {
                var dest = Path.Combine(destination, Path.GetFileName(dir));
                CopyDirectory(dir, dest);
            }
        }
    }
}