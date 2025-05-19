using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Puesto
    {
        [Key]
        public int id_puesto { get; set; }
        public string? nombre { get; set; }
        public int id_postulante { get; set; }
    }
}
