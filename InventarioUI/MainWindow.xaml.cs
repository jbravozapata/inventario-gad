using InventarioUI.Views;
using System.Windows;
using InventarioUI.Utils;
using Microsoft.Win32;


namespace InventarioUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (SessionManager.UsuarioActual != null)
            {
                txtUsuarioActivo.Text = SessionManager.UsuarioActual.NombreCompleto;
                txtRolUsuario.Text = SessionManager.UsuarioActual.Rol;
            }

            AplicarPermisos();


            MainContent.Content = new DashboardView();
        }

        private void BtnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardView();
        }

       

        private void BtnBienes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new BienesView();
        }

        private void BtnUbicaciones_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UbicacionesView();
        }

       

        // ✅ NUEVO
        private void BtnMovimientosBienes_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new MovimientosBienesView();
        }

        private void BtnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new UsuariosView();
        }

        private void AplicarPermisos()
        {
            var rol = SessionManager.UsuarioActual?.Rol;

            if (rol == "Administrador")
                return;

            if (rol == "Operador")
            {
                btnUsuarios.Visibility = Visibility.Collapsed;
            }

            if (rol == "Consulta")
            {
                btnUsuarios.Visibility = Visibility.Collapsed;
                btnMovimientosBienes.Visibility = Visibility.Collapsed;
            }
        }   

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void BtnExportarBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Archivo ZIP (*.zip)|*.zip",
                FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmm}.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                BackupService.ExportarBackup(dialog.FileName);
            }
        }

        private void BtnImportarBackup_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivo ZIP (*.zip)|*.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                var confirm = MessageBox.Show(
                    "Esto reemplazará todos los datos actuales.\n¿Deseas continuar?",
                    "Confirmar restauración",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirm == MessageBoxResult.Yes)
                {
                    BackupService.ImportarBackup(dialog.FileName);
                }
            }
        }
    }
}
