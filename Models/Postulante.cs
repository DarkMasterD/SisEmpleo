using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Postulante
    {
        [Key]
        public int id_postulante { get; set; }
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string direccion { get; set; }
        public DateTime fecha_nacimiento {  get; set; }

    }
}
