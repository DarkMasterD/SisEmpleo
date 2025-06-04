// En Models/Viewmodels/EmpresaViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models.Viewmodels
{
    public class EmpresaViewModel
    {
        [Required(ErrorMessage = "El nombre de la empresa es obligatorio.")]
        [StringLength(200)]
        public string? Nombre { get; set; }

        public string? Email { get; set; }

        [Display(Name = "Dirección")]
        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(300)]
        public string? Direccion { get; set; }

        [Display(Name = "Descripción de la Empresa")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300)]
        public string? Descripcion { get; set; }

        [Display(Name = "Sector de Trabajo")]
        [Required(ErrorMessage = "El sector es obligatorio.")]
        [StringLength(20)]
        public string? Sector { get; set; } 

        public string? TipoUsuario { get; set; }
    }
}