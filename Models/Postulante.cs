using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Postulante
    {
        [Key]
        public int id_postulante { get; set; }
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string direccion {  get; set; }

        public string NivelExperiencia { get; set; }
    }
}
