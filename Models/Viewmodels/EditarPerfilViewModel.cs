    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

    namespace SisEmpleo.Models.Viewmodels
    {
        // ViewModel auxiliar para mostrar habilidades actuales con opción de eliminar
        public class HabilidadCvViewModel
        {
            public int IdHabilidadCurriculum { get; set; } // PK de Habilidad_Curriculum
            public int IdHabilidad { get; set; }
            public string NombreHabilidad { get; set; }
        }

        // ViewModel auxiliar para mostrar "Otros Idiomas" actuales con opción de eliminar
        public class IdiomaCvDisplayViewModel
        {
            public int IdIdiomaCurriculum { get; set; } // PK de Idioma_Curriculum
            public string NombreIdioma { get; set; }
            public string NombreInstitucion { get; set; }
            public string FechaObtencionFormateada { get; set; }
        }

        // ViewModel para el input de "Añadir Otro Idioma al CV"
        public class IdiomaCvInputViewModel
        {
            [Required(ErrorMessage = "Debe seleccionar un idioma.")]
            [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un idioma.")]
            public int IdiomaId { get; set; }

            [Required(ErrorMessage = "Debe seleccionar una institución.")]
            [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una institución donde aprendió/certificó el idioma.")]
            public int InstitucionId { get; set; }

            [Required(ErrorMessage = "La fecha de obtención/certificación es obligatoria.")]
            [DataType(DataType.Date)]
            public DateTime FechaObtencion { get; set; } = DateTime.Today;
        }

        // --- ViewModel Principal para Editar Perfil ---
        public class EditarPerfilViewModel
        {
            // --- Información Personal y de Contacto ---
            [Display(Name = "Nombre")]
            [Required(ErrorMessage = "El nombre es obligatorio.")]
            [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
            public string Nombre { get; set; }

            [Display(Name = "Apellidos")]
            [Required(ErrorMessage = "Los apellidos son obligatorios.")]
            [StringLength(200, ErrorMessage = "Los apellidos no pueden exceder los 200 caracteres.")]
            public string Apellidos { get; set; }

            public string Email { get; set; }

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")] // Se aplicará si se ingresa un valor
        [StringLength(18, ErrorMessage = "El teléfono no puede exceder los 18 caracteres.")]
        public string? Telefono { get; set; }

        [Display(Name = "Email de Contacto Alternativo")]
        [EmailAddress(ErrorMessage = "El formato del email de contacto no es válido.")] // Se aplicará si se ingresa un valor
        [StringLength(200)]
        public string? EmailContacto { get; set; } 

        [Display(Name = "Fecha de Nacimiento")]
            [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
            [DataType(DataType.Date)]
            public DateTime? Fecha_Nacimiento { get; set; }

            [ValidateNever]
            public string TipoUsuario { get; set; } // Para mostrar, no para editar aquí

            // --- Ubicación ---
            [Display(Name = "País de Residencia")]
            [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un país de residencia.")]
            public int PaisId { get; set; }

            [Display(Name = "Provincia de Residencia")]
            [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una provincia de residencia.")]
            public int ProvinciaId { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> Paises { get; set; } = new List<SelectListItem>();
            [ValidateNever]
            public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();

            // --- Idioma Principal ---
            [Display(Name = "Idioma Principal")]
            [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar su idioma principal.")]
            public int PrimaryIdiomaId { get; set; }

            [ValidateNever]
            public List<SelectListItem> IdiomasPrincipalesDisponibles { get; set; } = new List<SelectListItem>();

            // --- Para la gestión de Habilidades ---
            [ValidateNever] 
            public IEnumerable<SelectListItem> HabilidadesDisponiblesParaAnadir { get; set; } = new List<SelectListItem>();

            [ValidateNever]
            public List<HabilidadCvViewModel> HabilidadesActuales { get; set; } = new List<HabilidadCvViewModel>();

            // Para el binding del dropdown al añadir una nueva habilidad desde su propio formulario
            public int IdHabilidadSeleccionadaParaAnadir { get; set; }

            // --- Para la gestión de "Otros Idiomas del CV" ---
            [ValidateNever]
            public List<IdiomaCvDisplayViewModel> OtrosIdiomasActuales { get; set; } = new List<IdiomaCvDisplayViewModel>();

            // Se usa como modelo para el sub-formulario de añadir nuevo idioma al CV
            // Los atributos de validación están en la clase IdiomaCvInputViewModel
            public IdiomaCvInputViewModel NuevoOtroIdioma { get; set; } = new IdiomaCvInputViewModel();

            [ValidateNever] // Para el dropdown de idiomas al añadir un "Otro Idioma"
            public IEnumerable<SelectListItem> TodosLosIdiomasDisponibles { get; set; } = new List<SelectListItem>();

            [ValidateNever] // Para el dropdown de instituciones (las del usuario) al añadir un "Otro Idioma"
            public IEnumerable<SelectListItem> InstitucionesRegistradasPorUsuario { get; set; } = new List<SelectListItem>();
        }
    }