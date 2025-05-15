using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Idioma_Curriculum
    {
        [Key]
        public int id_idioma_curriculum { get; set; }
        public int id_curriculum { get; set; }
        public int id_institucion { get; set; }
        public int id_idioma { get; set; }
        public DateTime fecha { get; set; }
    }
}
