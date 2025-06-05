using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models.Viewmodels
{
    public class EditarOfertaViewModel
    {
        public int id_ofertaempleo { get; set; }

        [Required]
        public int? id_pais { get; set; }

        [Required]
        public int? id_provincia { get; set; }

        [Required]
        public string titulo { get; set; }

        [Required]
        public string descripcion { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int vacante { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal salario { get; set; }

        public string horario { get; set; }

        [Required]
        public string duracion_contrato { get; set; }

        [Required]
        public bool? estado { get; set; }

        public string[] HorarioArray { get; set; } = new string[28];
        public SelectList Paises { get; set; }
        public SelectList Provincias { get; set; }
        public List<SelectListItem> OpcionesEstado { get; set; }
        public char tipo_usuario { get; set; }

        public string PaisNombre { get; set; }
        public string ProvinciaNombre { get; set; }
        public string EmpresaNombre { get; set; }

    }
}
