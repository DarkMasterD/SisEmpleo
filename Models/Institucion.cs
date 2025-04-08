using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Institucion
    {
        [Key]
        public int id_institucion { get; set; }
        public string nombre { get; set; }
        public string descripicion { get; set; }
    }
}
