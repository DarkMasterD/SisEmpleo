using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class OfertaEmpleoPrestacion
    {
        [Key]
        [Column("id_ofertaempleoprestacion")] // Especifica el nombre real de la columna
        public int IdOfertaEmpleoPrestacion { get; set; }

        [ForeignKey("OfertaEmpleo")]
        public int id_ofertaempleo { get; set; }

        [ForeignKey("PrestacionLey")]
        public int id_prestacionley { get; set; }

        public string? descripcion { get; set; }

        // Propiedades de navegación
        public virtual OfertaEmpleo? OfertaEmpleo { get; set; }
        public virtual PrestacionLey? PrestacionLey { get; set; }
    }
}
