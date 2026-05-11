using System.Windows;
using InventarioSimple;
using InventarioSimple.Services;
using InventarioUI.Utils;
using System.Threading.Tasks;

namespace InventarioUI
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DbInitializer.Inicializar();

            this.KeyDown += LoginWindow_KeyDown;
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            lblMensaje.Text = "";

            var username = txtUsuario.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblMensaje.Text = "Por favor ingresa tus credenciales.";
                return;
            }

            // 🔹 ACTIVAR LOADER
            btnLogin.IsEnabled = false;
            txtBtnLogin.Visibility = Visibility.Collapsed;
            txtLoading.Visibility = Visibility.Visible;

            await Task.Delay(800);

            using var db = new AppDbContext();

            var user = db.Usuarios
                .FirstOrDefault(u => u.Username == username && u.Activo);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                SessionManager.UsuarioActual = user;

                var fade = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(250)
                };

                fade.Completed += (s, a) =>
                {
                    var main = new MainWindow();
                    Application.Current.MainWindow = main;
                    main.Show();
                    this.Close();
                };

                this.BeginAnimation(Window.OpacityProperty, fade);
            }
            else
            {
                lblMensaje.Text = "Usuario o contraseña incorrectos.";

                btnLogin.IsEnabled = true;
                txtBtnLogin.Visibility = Visibility.Visible;
                txtLoading.Visibility = Visibility.Collapsed;
            }
        }
        private void LoginWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnLogin_Click(null, null);
            }
        }
    }
}
