using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class CategoriaProfesional
    {
        [Key]
        public int id_categoriaprofesional {  get; set; }
        public string nombre { get; set; }
    }
}
