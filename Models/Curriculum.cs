using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Curriculum
    {
        [Key]
        public int id_curriculum { get; set; }
        public int id_postulante { get; set; }
        public string habilidades { get; set; }
        public string idiomas { get; set; }
        public string certificaciones { get; set; }
    }
}
