using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Idioma
    {
        [Key]
        public int id_idioma { get; set; }
        public string? nombre { get; set; }
    }
}
