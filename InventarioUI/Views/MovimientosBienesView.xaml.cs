using System.Linq;
using System.Windows.Controls;
using InventarioSimple;
using Microsoft.EntityFrameworkCore;

namespace InventarioUI.Views
{
    public partial class MovimientosBienesView : UserControl
    {
        public MovimientosBienesView()
        {
            InitializeComponent();
            CargarMovimientos();
        }

        private void CargarMovimientos()
        {
            using var db = new AppDbContext();

            var movimientos = db.MovimientosBienes
                .Include(m => m.Bien)
                .Include(m => m.UbicacionOrigen)
                .Include(m => m.UbicacionDestino)
                .OrderByDescending(m => m.Fecha)
                .ToList();

            dgMovimientosBienes.ItemsSource = movimientos;
        }
    }
}
