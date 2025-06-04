using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class ExperienciaProfesional
    {
        



        [Key]
        public int id_experiencia_profesional { get; set; }

        [ForeignKey("Curriculum")]
        public int id_curriculum { get; set; }

        [ForeignKey("TrabajoEmpresa")]
        public int id_trabajoempresa { get; set; }

        [ForeignKey("Puesto")]
        public int id_puesto { get; set; }

        public string? responsabilidad { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }

        // Propiedades de navegación
        public virtual TrabajoEmpresa TrabajoEmpresa { get; set; }
        public virtual Puesto Puesto { get; set; }
        public virtual Curriculum Curriculum { get; set; }
    }
}
