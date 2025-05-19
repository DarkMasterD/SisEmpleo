using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Habilidad
    {
        [Key]
        public int id_habilidad { get; set; }
        public string nombre { get; set; }
        public int id_usuario { get; set; }
    }
}
