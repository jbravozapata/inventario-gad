using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace InventarioUI.Views
{
    public partial class BienFotoViewerWindow : Window
    {
        public BienFotoViewerWindow(string rutaImagen)
        {
            InitializeComponent();

            this.KeyDown += BienFotoViewerWindow_KeyDown;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(rutaImagen);
            bitmap.EndInit();

            imgFoto.Source = bitmap;
        }

        private void BienFotoViewerWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ImgFoto_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? 0.1 : -0.1;

            imgScale.ScaleX += zoom;
            imgScale.ScaleY += zoom;

            if (imgScale.ScaleX < 0.1)
            {
                imgScale.ScaleX = 0.1;
                imgScale.ScaleY = 0.1;
            }
        }
    }
}