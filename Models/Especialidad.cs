using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Especialidad
    {
        [Key]
        public int id_especialidad {  get; set; }
        public string nombre { get; set; }
    }
}
