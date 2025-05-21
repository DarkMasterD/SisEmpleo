using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class OfertaCandidatos
    {
        [Key]
        public int id_ofertacandidatos {  get; set; }
        public int id_ofertaempleo { get; set; }
        public int id_usuario { get; set; }
        public string estado { get; set; }
        public bool? visto { get; set; }
    }
}
