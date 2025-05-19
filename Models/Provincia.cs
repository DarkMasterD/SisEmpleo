using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Provincia
    {
        [Key]
        public int id_provincia {  get; set; }
        public int id_pais { get; set; }
        public string? nombre { get; set; }
    }
}
