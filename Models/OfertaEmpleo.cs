using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class OfertaEmpleo
    {
        [Key]
        public int id_ofertaempleo { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public int id_empresa { get; set; }
        public string titulo { get; set; }
        public string descripcion { get; set; }
        public int vacante { get; set; }
        public float salario { get; set; }
        public string horario { get; set; }
        public string duracion_contrato { get; set; }
        public DateTime fecha_publicacion { get; set; }
        public bool? estado { get; set; }

        //
        [NotMapped]
        public string PaisNombre { get; set; }
        [NotMapped]
        public string ProvinciaNombre { get; set; }
        [NotMapped]
        public string EmpresaNombre { get; set; }
    }

    
}
