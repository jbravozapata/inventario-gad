using System;

namespace InventarioSimple.Models
{
    public class MovimientoBien
    {
       

        public int Id { get; set; }

        public int BienId { get; set; }
        public Bien Bien { get; set; } = null!;

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Tipo { get; set; } = string.Empty;
        // INGRESO | TRASLADO | BAJA | CAMBIO_ESTADO

        public int? UbicacionOrigenId { get; set; }
        public Ubicacion? UbicacionOrigen { get; set; }

        public int? UbicacionDestinoId { get; set; }
        public Ubicacion? UbicacionDestino { get; set; }

        public string? EstadoAnterior { get; set; }
        public string? EstadoNuevo { get; set; }

        public string? Observacion { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }

}
