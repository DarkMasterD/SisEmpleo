using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class OfertaCategoria
    {
        [Key]
        public int id_ofertacategoria { get; set; }
        public int id_ofertaempleo { get; set; }
        public int id_categoriaprofesional { get; set; }
        public int id_subcategoriaprofesional { get; set; }
    }
}
