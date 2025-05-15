using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Certificacion
    {
        [Key]
        public int id_certificacion { get; set; }
        public string nombre { get; set; }
    }
}
