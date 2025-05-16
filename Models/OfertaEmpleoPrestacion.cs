using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class OfertaEmpleoPrestacion
    {
        [Key]
        public int id_ofertaempleopresentacion { get; set; }
        public int id_ofertaempleo { get; set; }
        public int id_prestacionley { get; set; }
        public string? descripcion { get; set; }
    }
}
