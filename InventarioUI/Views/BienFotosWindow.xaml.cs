using InventarioSimple.Models;
using InventarioUI.Utils;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace InventarioUI.Views
{
    public partial class BienFotosWindow : Window
    {
        private readonly Bien _bien;
        private readonly string _carpetaBien;

        public BienFotosWindow(Bien bien)
        {
            InitializeComponent();

            _bien = bien;
            _carpetaBien = StoragePaths.GetFolderForBien(
                bien.Id,
                bien.Descripcion,
                bien.CodigoContable
            );

            txtTitulo.Text = "Fotos del bien";
            txtSubtitulo.Text = $"{bien.CodigoContable} - {bien.Descripcion}";

            CargarFotos();
        }

        private void CargarFotos()
        {
            if (!Directory.Exists(_carpetaBien))
                return;

            var imagenes = Directory.GetFiles(_carpetaBien)
                .Where(f =>
    f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase) ||
    f.EndsWith(".jpeg", System.StringComparison.OrdinalIgnoreCase) ||
    f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
)
                .Select(f =>
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.UriSource = new System.Uri(f);
                    img.EndInit();
                    return img;
                })
                .ToList();

            itemsFotos.ItemsSource = imagenes;
        }

        private void BtnAgregarFoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = true
            };

            if (dlg.ShowDialog() != true)
                return;

            foreach (var archivo in dlg.FileNames)
            {
                var info = new FileInfo(archivo);

                // Validar tamaño máximo (5MB)
                if (info.Length > 10 * 1024 * 1024)
                {
                    MessageBox.Show(
                        $"La imagen {info.Name} supera el tamaño máximo de 5MB.",
                        "Imagen demasiado grande",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    continue;
                }

                var extension = info.Extension.ToLower();

                var nombreDestino = Path.Combine(
                    _carpetaBien,
                    $"foto_{System.Guid.NewGuid()}{extension}"
                );

                File.Copy(archivo, nombreDestino);
            }

            CargarFotos();
        }

        private void ImgFoto_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not Image img)
                return;

            if (img.Source is not BitmapImage bitmap)
                return;

            var visor = new BienFotoViewerWindow(bitmap.UriSource.LocalPath)
            {
                Owner = this
            };

            visor.ShowDialog();
        }


        private void BtnEliminarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            if (btn.Tag is not BitmapImage img)
                return;

            var ruta = img.UriSource.LocalPath;

            if (MessageBox.Show("¿Deseas eliminar esta foto?",
                                "Confirmación",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            if (File.Exists(ruta))
                File.Delete(ruta);

            CargarFotos();
        }


        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
