using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventarioUI.Models
{
    public class UbicacionGrid
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int TotalBienes { get; set; }
    }
}
