using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Direccion_Postulante
    {
        [Key]
        public int id_direccion_postulante { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public string? direccion { get; set; }
    }
}
