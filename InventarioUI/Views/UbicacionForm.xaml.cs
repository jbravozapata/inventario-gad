using System.Windows;
using InventarioSimple;
using InventarioSimple.Models;

namespace InventarioUI.Views
{

    public partial class UbicacionForm : Window
    {


        private readonly Ubicacion? _ubicacionEditando;

        // NUEVA UBICACIÓN
        public UbicacionForm()
        {
            InitializeComponent();
            txtNombre.Focus();
        }

        // EDITAR UBICACIÓN EXISTENTE
        public UbicacionForm(Ubicacion ubicacion) : this()
        {
            _ubicacionEditando = ubicacion;
            Title = "Editar ubicación";
            txtNombre.Text = ubicacion.Nombre;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = txtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre de la ubicación es obligatorio.",
                                "Validación",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            using var db = new AppDbContext();

            if (_ubicacionEditando == null)
            {
                // Crear nueva
                var nueva = new Ubicacion
                {
                    Nombre = nombre
                };

                db.Ubicaciones.Add(nueva);
            }
            else
            {
                // Editar existente (recargar desde DB por seguridad)
                var ub = db.Ubicaciones.Find(_ubicacionEditando.Id);
                if (ub == null)
                {
                    MessageBox.Show("No se encontró la ubicación en base de datos.",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    return;
                }

                ub.Nombre = nombre;
            }

            db.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


    }
}
