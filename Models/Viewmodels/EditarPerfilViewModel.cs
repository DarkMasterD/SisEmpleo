using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Necesario para [ValidateNever]
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models.Viewmodels
{
    public class EditarPerfilViewModel
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
        public string Nombre { get; set; }

        [Display(Name = "Apellidos")]
        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(200, ErrorMessage = "Los apellidos no pueden exceder los 200 caracteres.")]
        public string Apellidos { get; set; }

        public string Email { get; set; } // Readonly en el formulario

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(18, ErrorMessage = "El teléfono no puede exceder los 18 caracteres.")]
        public string Telefono { get; set; } // Opcional, sin [Required]

        [Display(Name = "Email de Contacto Alternativo")]
        [EmailAddress(ErrorMessage = "El formato del email de contacto no es válido.")]
        [StringLength(200)]
        public string EmailContacto { get; set; } // Opcional, sin [Required]

        public bool UsarTelefonoCuenta { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")] // Si tu BD es NOT NULL
        [DataType(DataType.Date)]
        public DateTime? Fecha_Nacimiento { get; set; } // Nullable para el picker, pero [Required] para la validación

        [ValidateNever] // No validar esta propiedad en el POST de EditarPerfil
        public string TipoUsuario { get; set; }

        [Display(Name = "País de Residencia")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un país de residencia.")]
        public int PaisId { get; set; }

        [Display(Name = "Provincia de Residencia")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una provincia de residencia.")]
        public int ProvinciaId { get; set; }

        // --- Listas para poblar Dropdowns (NO deben tener [Required]) ---
        public IEnumerable<SelectListItem> Paises { get; set; }
        public IEnumerable<SelectListItem> Provincias { get; set; }
        public IEnumerable<SelectListItem> IdiomasDisponibles { get; set; }
        public List<SisEmpleo.Models.Habilidad> HabilidadesDisponibles { get; set; }

        // --- IDs seleccionados de los multi-select (se manejarán o rediseñarán después) ---
        public List<int> IdiomaIds { get; set; }
        public List<int> HabilidadIds { get; set; }

        // --- Propiedades para "Añadir Nuevo X" - Con [ValidateNever] ---
        [ValidateNever]
        public FormacionAcademica NuevaFormacion { get; set; } // Asumiendo que FormacionAcademica es tu modelo EF

        [ValidateNever]
        public ExperienciaViewModel NuevaExperiencia { get; set; }

        [ValidateNever]
        public InstitucionViewModel NuevaInstitucion { get; set; }

        [ValidateNever]
        public CertificacionViewModel NuevaCertificacion { get; set; }

        // --- Listas para mostrar elementos existentes ---
        public List<FormacionAcademicaViewModel> ListaFormacion { get; set; } = new();
        public List<ExperienciaViewModel> Experiencias { get; set; } = new();
        public List<CertificacionViewModel> Certificaciones { get; set; } = new();
        public List<InstitucionViewModel> MisInstituciones { get; set; } = new();
    }
}