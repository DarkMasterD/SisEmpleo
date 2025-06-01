using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models.Viewmodels
{
    public class InstitucionViewModel
    {
        public int IdInstitucion { get; set; }

        [Required(ErrorMessage = "El nombre de la institución es obligatorio.")]
        [StringLength(120)]
        public string Nombre { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un país.")]
        public int IdPais { get; set; }
        public string NombrePais { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una provincia.")]
        public int IdProvincia { get; set; }
        public string NombreProvincia { get; set; } 

        [StringLength(200)]
        public string Direccion { get; set; }

        // Para los dropdowns en el formulario de añadir nueva institución
        public IEnumerable<SelectListItem> PaisesList { get; set; }
        public IEnumerable<SelectListItem> ProvinciasList { get; set; }
    }
}