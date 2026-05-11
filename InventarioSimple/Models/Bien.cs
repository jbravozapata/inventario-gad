using System;

namespace InventarioSimple.Models
{
    public class Bien
    {
        public int Id { get; set; }

        // Campos que vienen del Excel
        public string CodigoContable { get; set; }     // Ej. 141.01.01.07
        public int Cantidad { get; set; } = 1;

        public string Descripcion { get; set; }        // Nombre del bien
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Serie { get; set; }
        public string Clasificacion { get; set; } = "";

        public decimal Precio { get; set; }            // Precio del bien
        public DateTime? FechaCompra { get; set; }     // Puede estar vacía
        public string VidaUtil { get; set; }           // Texto: ej. "5 años"

        public string EstadoBien { get; set; }         // B, R o M
        public string Observacion { get; set; }

        public decimal DepreciacionAcumulada { get; set; }

        public decimal ValorEnLibros { get; set; }

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // NUEVO: Ubicación del bien
        public int? UbicacionId { get; set; }
        public Ubicacion? Ubicacion { get; set; }
    }
}
