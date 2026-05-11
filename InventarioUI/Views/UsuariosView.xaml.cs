using InventarioSimple;
using System.Linq;
using System.Windows.Controls;
using InventarioUI.Views;

namespace InventarioUI.Views
{
    public partial class UsuariosView : UserControl
    {
        public UsuariosView()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            using var db = new AppDbContext();

            tablaUsuarios.ItemsSource = db.Usuarios
                .OrderBy(u => u.NombreCompleto)
                .ToList();
        }

        private void BtnNuevo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var win = new UsuarioEditorWindow();

            if (win.ShowDialog() == true)
            {
                CargarUsuarios();
            }
        }

        private void BtnEditar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (tablaUsuarios.SelectedItem is not Usuario usuario)
                return;

            var win = new UsuarioEditorWindow(usuario);

            if (win.ShowDialog() == true)
            {
                CargarUsuarios();
            }
        }

        private void BtnPassword_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (tablaUsuarios.SelectedItem is not Usuario usuario)
                return;

            var win = new CambiarPasswordWindow(usuario);

            win.ShowDialog();
        }

        private void BtnEstado_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (tablaUsuarios.SelectedItem is not Usuario usuario)
                return;

            using var db = new AppDbContext();

            var user = db.Usuarios.First(u => u.Id == usuario.Id);

            user.Activo = !user.Activo;

            db.SaveChanges();

            CargarUsuarios();
        }
    }
}