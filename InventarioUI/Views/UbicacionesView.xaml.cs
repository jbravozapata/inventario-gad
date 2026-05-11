using InventarioSimple;
using InventarioSimple.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InventarioUI.Models;

namespace InventarioUI.Views
{
    public partial class UbicacionesView : UserControl
    {
        public UbicacionesView()
        {
            InitializeComponent();
            CargarUbicaciones();
        }

        private void CargarUbicaciones()
        {
            using var db = new AppDbContext();

            var data = db.Ubicaciones
                .OrderBy(u => u.Nombre)
                .Select(u => new UbicacionGrid
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    TotalBienes = db.Bienes.Count(b => b.UbicacionId == u.Id)
                })
                .ToList();

            dgUbicaciones.ItemsSource = data;
        }

        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var form = new UbicacionForm();
            if (form.ShowDialog() == true)
                CargarUbicaciones();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgUbicaciones.SelectedItem is not UbicacionGrid row)
                return;

            using var db = new AppDbContext();

            var ub = db.Ubicaciones.Find(row.Id);

            if (ub == null)
                return;

            var form = new UbicacionForm(ub);
            if (form.ShowDialog() == true)
                CargarUbicaciones();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgUbicaciones.SelectedItem is not UbicacionGrid row)
            {
                MessageBox.Show("Selecciona una ubicación.");
                return;
            }

            using var db = new AppDbContext();

            var ub = db.Ubicaciones.Find(row.Id);

            if (ub == null)
                return;

            bool tieneBienes = db.Bienes.Any(b => b.UbicacionId == ub.Id);

            if (tieneBienes)
            {
                MessageBox.Show(
                    "No se puede eliminar esta ubicación porque tiene bienes asociados.",
                    "Operación no permitida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (MessageBox.Show(
                "¿Seguro que deseas eliminar esta ubicación?",
                "Confirmación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            db.Ubicaciones.Remove(ub);
            db.SaveChanges();

            CargarUbicaciones();
        }

        private void DgUbicaciones_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgUbicaciones.SelectedItem is not UbicacionGrid row)
                return;

            using var db = new AppDbContext();

            var ub = db.Ubicaciones.Find(row.Id);

            if (ub == null)
                return;

            var form = new UbicacionForm(ub);
            if (form.ShowDialog() == true)
                CargarUbicaciones();
        }
    }
}
