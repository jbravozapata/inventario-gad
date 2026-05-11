using InventarioSimple.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using InventarioUI.Utils;

namespace InventarioUI.Views
{
    public partial class BienDetalleWindow : Window
    {
        private string rutaFotoActual;
        private Bien bienActual;
        public BienDetalleWindow(Bien bien)
        {
            InitializeComponent();
            bienActual = bien;

            txtCodigo.Text = bien.CodigoContable;
            txtDescripcion.Text = bien.Descripcion;
            txtMarca.Text = bien.Marca;
            txtModelo.Text = bien.Modelo;
            txtSerie.Text = bien.Serie;
            txtPrecio.Text = bien.Precio.ToString("C");
            txtValorTotal.Text = (bien.Precio * bien.Cantidad).ToString("C");
            txtEstado.Text = bien.EstadoBien;
            txtClasificacion.Text = bien.Clasificacion;
            txtObservacion.Text = bien.Observacion;

            CargarFoto(bien);
        }

        private void CargarFoto(Bien bien)
        {
            var carpeta = StoragePaths.GetFolderForBien(
                bien.Id,
                bien.Descripcion,
                bien.CodigoContable
            );

            if (!Directory.Exists(carpeta))
                return;

            var foto = Directory.GetFiles(carpeta)
                .FirstOrDefault(f =>
                    f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

            if (foto == null)
                return;

            rutaFotoActual = foto;

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(foto, UriKind.Absolute);
            bitmap.EndInit();

            imgBien.Source = bitmap;
        }

        private void ImgBien_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(rutaFotoActual))
                return;

            var visor = new BienFotoViewerWindow(rutaFotoActual)
            {
                Owner = this
            };

            visor.ShowDialog();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF (*.pdf)|*.pdf",
                    FileName = $"Ficha_{bienActual.CodigoContable}.pdf"
                };

                if (dialog.ShowDialog() != true)
                    return;

                // 🔥 ABRIR VENTANA PERSONALIZADA
                var responsablesWindow = new ResponsablesWindow
                {
                    Owner = this
                };

                if (responsablesWindow.ShowDialog() != true)
                    return;

                // 🔥 OBTENER DATOS
                string entrega = responsablesWindow.NombreEntrega;
                string recibe = responsablesWindow.NombreRecibe;

                // 🔥 GENERAR PDF
                InventarioUI.Reports.BienFichaPDF.Generar(
                    bienActual,
                    rutaFotoActual,
                    dialog.FileName,
                    entrega,
                    recibe
                );

                MessageBox.Show(
                    "La ficha del bien se generó correctamente.",
                    "PDF generado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al generar el PDF:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}