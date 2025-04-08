using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class RequisitoOferta
    {
        [Key]
        public int id_requisitooferta { get; set; }
        public int id_ofertaempleo { get; set; }
        public int id_requisito {  get; set; }
    }
}
