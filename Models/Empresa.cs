using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Empresa
    {
        [Key]
        public int id_empresa { get; set; }
        public int id_usuario { get; set; }
        public string nombre { get; set; }
        public string direccion {  get; set; }
        public string descripcion_empresa { get; set; }
        public string sector_empresa { get; set; }
    }
}
