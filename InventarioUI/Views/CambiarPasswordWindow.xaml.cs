using InventarioSimple;
using System.Linq;
using System.Windows;

namespace InventarioUI.Views
{
    public partial class CambiarPasswordWindow : Window
    {
        private Usuario usuario;

        public CambiarPasswordWindow(Usuario user)
        {
            InitializeComponent();
            usuario = user;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var password = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(password))
                return;

            using var db = new AppDbContext();

            var user = db.Usuarios.First(u => u.Id == usuario.Id);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            db.SaveChanges();

            DialogResult = true;
            Close();
        }
    }
}