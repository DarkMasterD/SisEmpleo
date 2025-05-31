// Models/Viewmodels/InstitucionViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SisEmpleo.Models.Viewmodels
{
    public class InstitucionViewModel
    {
        public int IdInstitucion { get; set; } // Para editar/eliminar, si lo implementas

        [Required(ErrorMessage = "El nombre de la institución es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede exceder los 120 caracteres.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un país.")]
        [Display(Name = "País")]
        public int IdPais { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una provincia.")]
        [Display(Name = "Provincia")]
        public int IdProvincia { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
        [Display(Name = "Dirección (Opcional)")]
        public string Direccion { get; set; }

        // Para llenar los dropdowns en el formulario
        public List<SelectListItem> PaisesList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProvinciasList { get; set; } = new List<SelectListItem>();
    }
}