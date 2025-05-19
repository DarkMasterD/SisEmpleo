using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class SubcategoriaProfesional
    {
        [Key]
        public int id_subcategoriaprofesional {  get; set; }
        public int id_categoriaprofesional { get; set; }
        public string? nombre { get; set; }
        public int id_empresa { get; set; }
    }
}
