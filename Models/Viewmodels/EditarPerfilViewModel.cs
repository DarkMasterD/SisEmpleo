// Archivo: SisEmpleo/Models/Viewmodels/EditarPerfilViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SisEmpleo.Models.Viewmodels
{
    // --- ViewModels Auxiliares ---

    public class HabilidadCvViewModel
    {
        public int IdHabilidadCurriculum { get; set; }
        public int IdHabilidad { get; set; }
        public string NombreHabilidad { get; set; }
    }

    public class IdiomaCvDisplayViewModel
    {
        public int IdIdiomaCurriculum { get; set; }
        public string NombreIdioma { get; set; }
        public string NombreInstitucion { get; set; }
        public string FechaObtencionFormateada { get; set; }
    }

    public class IdiomaCvInputViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un idioma.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un idioma válido.")]
        public int IdiomaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una institución.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una institución válida.")]
        public int InstitucionId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaObtencion { get; set; } = DateTime.Today;
    }

    public class FormacionAcademicaDisplayViewModel
    {
        public int IdFormacionAcademica { get; set; }
        public string NombreTitulo { get; set; }
        public string TipoTitulo { get; set; }
        public string NombreInstitucion { get; set; }
        public string NombreEspecialidad { get; set; }
        // Podrías añadir fechas de inicio/fin si las tienes en FormacionAcademica
    }

    public class FormacionAcademicaInputViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un título.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un título válido.")]
        [Display(Name = "Título Obtenido")]
        public int TituloId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una institución educativa.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una institución válida.")]
        [Display(Name = "Institución Educativa")]
        public int InstitucionId { get; set; }
    }

    public class TituloInputViewModel
    {
        [Required(ErrorMessage = "El nombre del título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre del título no puede exceder los 200 caracteres.")] // Ajusta según tu BD (VARCHAR(MAX) es grande)
        [Display(Name = "Nombre del Título*")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        [Display(Name = "Descripción (Opcional)")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el tipo de título.")]
        [Display(Name = "Tipo de Título*")]
        public string Tipo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una especialidad.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una especialidad válida.")]
        [Display(Name = "Especialidad del Título*")]
        public int EspecialidadId { get; set; }
    }

    // --- ViewModel Principal para Editar Perfil ---
    public class EditarPerfilViewModel
    {
        // Información Personal y de Contacto
        [Display(Name = "Nombre(s)*")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200)]
        public string Nombre { get; set; }

        [Display(Name = "Apellido(s)*")]
        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(200)]
        public string Apellidos { get; set; }

        public string Email { get; set; } // Readonly

        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
        [StringLength(18)]
        public string? Telefono { get; set; }

        [Display(Name = "Email de Contacto Alternativo")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [StringLength(200)]
        public string? EmailContacto { get; set; }

        [Display(Name = "Fecha de Nacimiento*")]
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? Fecha_Nacimiento { get; set; }

        [ValidateNever]
        public string TipoUsuario { get; set; }

        // Ubicación
        [Display(Name = "País de Residencia*")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un país.")]
        public int PaisId { get; set; }

        [Display(Name = "Provincia de Residencia*")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una provincia.")]
        public int ProvinciaId { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> Paises { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();

        // Idioma Principal
        [Display(Name = "Idioma Principal*")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar su idioma principal.")]
        public int PrimaryIdiomaId { get; set; }

        [ValidateNever]
        public List<SelectListItem> IdiomasPrincipalesDisponibles { get; set; } = new List<SelectListItem>();

        // Para la gestión de Habilidades
        [ValidateNever]
        public IEnumerable<SelectListItem> HabilidadesDisponiblesParaAnadir { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public List<HabilidadCvViewModel> HabilidadesActuales { get; set; } = new List<HabilidadCvViewModel>();
        [Display(Name = "Habilidad a añadir")] // Para el dropdown de añadir habilidad
        public int IdHabilidadSeleccionadaParaAnadir { get; set; }

        // Para la gestión de "Otros Idiomas del CV"
        [ValidateNever]
        public List<IdiomaCvDisplayViewModel> OtrosIdiomasActuales { get; set; } = new List<IdiomaCvDisplayViewModel>();
        public IdiomaCvInputViewModel NuevoOtroIdioma { get; set; } = new IdiomaCvInputViewModel();
        [ValidateNever]
        public IEnumerable<SelectListItem> TodosLosIdiomasDisponibles { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public IEnumerable<SelectListItem> InstitucionesRegistradasPorUsuario { get; set; } = new List<SelectListItem>();

        // Para la gestión de Formación Académica
        [ValidateNever]
        public List<FormacionAcademicaDisplayViewModel> FormacionesAcademicasActuales { get; set; } = new List<FormacionAcademicaDisplayViewModel>();
        public FormacionAcademicaInputViewModel NuevaFormacionAcademica { get; set; } = new FormacionAcademicaInputViewModel();
        [ValidateNever]
        public IEnumerable<SelectListItem> TitulosRegistradosPorUsuario { get; set; } = new List<SelectListItem>();
        // La lista InstitucionesRegistradasPorUsuario se reutiliza para formación.

        // Para el formulario de "Añadir Nuevo Título"
        public TituloInputViewModel NuevoTitulo { get; set; } = new TituloInputViewModel();
        [ValidateNever]
        public IEnumerable<SelectListItem> EspecialidadesDisponibles { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public IEnumerable<SelectListItem> TiposDeTituloDisponibles { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Seleccione tipo..." },
            new SelectListItem { Value = "Licenciatura", Text = "Licenciatura" },
            new SelectListItem { Value = "Maestría", Text = "Maestría" },
            new SelectListItem { Value = "Doctorado", Text = "Doctorado" },
            new SelectListItem { Value = "Formación Académica", Text = "Formación Académica (Curso, Diplomado, etc.)" }
        };
    }
}