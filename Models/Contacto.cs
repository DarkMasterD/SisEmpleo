using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Contacto
    {
        [Key]
        public int id_contacto { get; set; }

        public int id_usuario { get; set; }

        [StringLength(18)]
        public string? telefono { get; set; } 
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "El formato del email de contacto no es válido.")]
        public string? email { get; set; }

        [ForeignKey("id_usuario")]
        public virtual Usuario Usuario { get; set; }
    }
}