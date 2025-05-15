using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Habilidad_Curriculum
    {
        [Key]
        public int id_habilidad_curriculum { get; set; }

        public int id_curriculum { get; set; }
        public int id_habilidad { get; set; }
        public DateTime fecha { get; set; }

        public virtual Curriculum Curriculum { get; set; }
        public virtual Habilidad Habilidad { get; set; }
    }
}
