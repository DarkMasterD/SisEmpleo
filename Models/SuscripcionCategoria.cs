using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class SuscripcionCategoria
    {
        [Key]
        public int id_suscripcioncategoria {  get; set; }
        public int id_usuario { get; set; }
        public int id_categoriaprofesional { get; set; }
        public int id_subcategoriaprofesional { get; set; }
        public bool estado { get; set; }
    }
}
