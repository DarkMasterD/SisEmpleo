using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class PrestacionLey
    {
        [Key]
        public int id_prestacionley { get; set; }
        public string? nombre { get; set; }
    }
}
