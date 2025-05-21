namespace SisEmpleo.Models
{
    public class PostulanteViewModel
    {
        // Datos básicos del postulante
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }

        // Info de contacto
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Email { get; set; }

        // Localización
        public string? Pais { get; set; }
        public string? Provincia { get; set; }

        // Idiomas
        public List<string> Idiomas { get; set; } = new();

        // Habilidades
        public List<string> Habilidades { get; set; } = new();

        // Formación académica
        public List<FormacionViewModel> Formaciones { get; set; } = new();

        // Certificaciones
        public List<CertificacionViewModel> Certificaciones { get; set; } = new();

        // Experiencia profesional
        public List<ExperienciaViewModel> Experiencias { get; set; } = new();
        public Usuario Usuario { get; set; }
        public Postulante Postulante { get; set; }
    }
}
