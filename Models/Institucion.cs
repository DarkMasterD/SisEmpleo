using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Institucion
    {
        [Key]
        public int id_institucion { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public string nombre { get; set; }
        public string descripcion { get; set; } //Pelie tanto por encontrar esto porque se llamaba descripicion
    }
}
