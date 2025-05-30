using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models.Viewmodels
{
    public class EditarPerfilViewModel
    {
        // Datos personales
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
        public string TipoUsuario { get; set; }

        public int PaisId { get; set; }
        public int ProvinciaId { get; set; }

        public IEnumerable<SelectListItem> Paises { get; set; }
        public IEnumerable<SelectListItem> Provincias { get; set; }

        // Idiomas
        public List<int> IdiomaIds { get; set; }
        public IEnumerable<SelectListItem> IdiomasDisponibles { get; set; }
        public List<string> ListaIdiomas { get; set; } = new List<string>();


        // Habilidades
        public List<int> HabilidadIds { get; set; }
        public List<SisEmpleo.Models.Habilidad> HabilidadesDisponibles { get; set; }

        // Formación, Experiencia y Certificaciones (nuevos ingresos)
        public FormacionAcademica NuevaFormacion { get; set; }
        public List<FormacionAcademicaViewModel> ListaFormacion { get; set; } = new();
        public ExperienciaViewModel NuevaExperiencia { get; set; }
        public List<ExperienciaViewModel> Experiencias { get; set; } = new();
        public CertificacionViewModel NuevaCertificacion { get; set; }
        public List<CertificacionViewModel> Certificaciones { get; set; } = new();

    }

    // Clases auxiliares
    public class FormacionInputModel
    {
        public string Especialidad { get; set; }
        public string Titulo { get; set; }
        public string Institucion { get; set; }
    }

    public class ExperienciaInputModel
    {
        public string Puesto { get; set; }
        public string Empresa { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class CertificacionInputModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime? FechaObtencion { get; set; }
    }

    // Entidades referenciales
    public class Pais
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class Provincia
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int PaisId { get; set; }
    }

    public class Idioma
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class Habilidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
