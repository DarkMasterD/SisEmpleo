using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class FormacionAcademica
    {
        [Key]
        public int id_formacionacademica { get; set; }
        public int id_curriculum { get; set; }
        public int id_institucion { get; set; }
        public int id_titulo { get; set; }



        public virtual Especialidad Especialidad { get; set; }
        public virtual Titulo Titulo { get; set; }
        public virtual Institucion Institucion { get; set; }
    }
}
