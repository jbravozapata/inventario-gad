using System.Linq;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using InventarioSimple;

namespace InventarioUI.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            using var db = new AppDbContext();

            // Total bienes
            txtTotalBienes.Text = db.Bienes.Count().ToString();

            // Valor patrimonial
            var totalValor = db.Bienes
                .Sum(b => (decimal?)(b.ValorEnLibros * b.Cantidad)) ?? 0;

            txtValorTotal.Text = $"${totalValor:N2}";

            // Depreciación acumulada
            var depreciacionTotal = db.Bienes
                .Sum(b => (decimal?)(b.DepreciacionAcumulada * b.Cantidad)) ?? 0;

            txtDepreciacion.Text = $"${depreciacionTotal:N2}";

            // Estados
            var buenos = db.Bienes.Count(b => b.EstadoBien == "B");
            var regulares = db.Bienes.Count(b => b.EstadoBien == "R");
            var malos = db.Bienes.Count(b => b.EstadoBien == "M");

            txtEstadoBueno.Text = $"🟢 Buenos: {buenos}";
            txtEstadoRegular.Text = $"🟡 Regulares: {regulares}";
            txtEstadoMalo.Text = $"🔴 Malos: {malos}";

            txtBienesMalos.Text = malos.ToString();

            // Últimos movimientos
            lvMovimientos.ItemsSource = db.MovimientosBienes
    .Include(m => m.Bien)
    .OrderByDescending(m => m.Fecha)
    .Take(10)
    .ToList();

            txtAdmin.Text = db.Bienes
    .Count(b => b.Clasificacion == "Control Administrativo")
    .ToString();

            txtPPE.Text = db.Bienes
                .Count(b => b.Clasificacion == "Propiedad, Planta y Equipo")
                .ToString();
        }

        private void LvMovimientos_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            lvMovimientos.SelectedItem = null;
        }
    }
}