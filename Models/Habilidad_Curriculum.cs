using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Habilidad_Curriculum
    {
        [Key]
        public int id_habilidad_curriculum { get; set; }

        public int id_curriculum { get; set; }
        public int id_habilidad { get; set; }
        public DateTime fecha { get; set; }
        [ForeignKey("id_curriculum")]
        public virtual Curriculum Curriculum { get; set; }
        [ForeignKey("id_habilidad")]
        public virtual Habilidad Habilidad { get; set; }
    }
}
