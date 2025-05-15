using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class TrabajoEmpresa
    {
        [Key]
        public int id_trabajoempresa { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public string? nombre { get; set; }
    }
}
