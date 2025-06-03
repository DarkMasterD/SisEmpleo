using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class FormacionAcademica
    {
        [Key]
        public int id_formacionacademica { get; set; }

        public int id_curriculum { get; set; } // Propiedad FK
        [ForeignKey("id_curriculum")]
        public virtual Curriculum Curriculum { get; set; }

        public int id_institucion { get; set; } // Propiedad FK
        [ForeignKey("id_institucion")]
        public virtual Institucion Institucion { get; set; }

        public int id_titulo { get; set; } // Propiedad FK
        [ForeignKey("id_titulo")]
        public virtual Titulo Titulo { get; set; }
    }
}