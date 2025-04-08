using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class ExperienciaProfesional
    {
        [Key]
        public int id_experienciaprofesional { get; set; }
        public int id_curriculum { get; set; }
        public string puesto { get; set; }
        public string descripciones_responsabilidades { get; set; }
        public string empresa { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin {  get; set; }
    }
}
