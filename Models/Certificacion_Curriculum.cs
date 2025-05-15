using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Certificacion_Curriculum
    {
        [Key]
        public int id_certificacion_curriculum { get; set; }
        public int id_curriculum { get; set; }
        public int id_institucion { get; set; }
        public int id_certificacion { get; set; }
        public DateTime fecha { get; set; }
    }
}
