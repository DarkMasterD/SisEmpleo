using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Usuario
    {
        [Key]
        public int id_usuario {  get; set; }
        public string email { get; set; }
        public string contrasenia { get; set; }
        public char tipo_usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime last_login { get; set; }
    }
}
