using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Contacto
    {
        [Key]
        public int id_contacto { get; set; }
        public int id_usuario { get; set; }
        public string telefono { get; set; }
        public string email { get; set; }
    }
}
