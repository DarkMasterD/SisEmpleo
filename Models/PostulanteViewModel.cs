namespace SisEmpleo.Models
{
    public class PostulanteViewModel
    {

        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string TipoUsuario { get; set; }

        public List<string> Idiomas { get; set; }
        public List<string> Habilidades { get; set; }

        public List<FormacionAcademicaViewModel> FormacionAcademica { get; set; }
        public List<ExperienciaViewModel> Experiencias { get; set; }
        public List<CertificacionViewModel> Certificaciones { get; set; }
    }
}
