using InventarioSimple;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventarioUI.Views
{
    public partial class UsuarioEditorWindow : Window
    {

        private Usuario usuarioActual;
        private bool modoEdicion = false;

        public UsuarioEditorWindow()
        {
            InitializeComponent();
            cmbRol.SelectedIndex = 0;
        }
        public UsuarioEditorWindow(Usuario usuario)
        {
            InitializeComponent();

            usuarioActual = usuario;
            modoEdicion = true;

            txtNombre.Text = usuario.NombreCompleto;
            txtUsername.Text = usuario.Username;
            chkActivo.IsChecked = usuario.Activo;

            txtPassword.Visibility = Visibility.Collapsed;

            cmbRol.SelectedIndex = usuario.Rol switch
            {
                "Administrador" => 0,
                "Operador" => 1,
                _ => 2
            };
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombre.Text.Trim();
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password.Trim();
            var rol = ((ComboBoxItem)cmbRol.SelectedItem).Content.ToString();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Completa todos los campos.");
                return;
            }

            using var db = new AppDbContext();

            if (!modoEdicion)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Debes ingresar una contraseña.");
                    return;
                }

                if (db.Usuarios.Any(u => u.Username == username))
                {
                    MessageBox.Show("Ese usuario ya existe.");
                    return;
                }

                db.Usuarios.Add(new Usuario
                {
                    NombreCompleto = nombre,
                    Username = username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Rol = rol,
                    Activo = chkActivo.IsChecked == true
                });
            }
            else
            {
                var user = db.Usuarios.First(u => u.Id == usuarioActual.Id);

                user.NombreCompleto = nombre;
                user.Username = username;
                user.Rol = rol;
                user.Activo = chkActivo.IsChecked == true;
            }

            db.SaveChanges();

            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}