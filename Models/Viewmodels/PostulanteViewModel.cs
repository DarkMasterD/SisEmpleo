namespace SisEmpleo.Models.Viewmodels
{
    public class PostulanteViewModel
    {
        public int IdPostulante { get; set; }
        public int IdUsuario { get; set; }

        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Experiencia { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string TipoUsuario { get; set; }

        public List<string> Idiomas { get; set; }
        public List<string> Habilidades { get; set; }

        public List<FormacionAcademicaViewModel> FormacionAcademica { get; set; }
        public List<ExperienciaViewModel> Experiencias { get; set; }
        public List<CertificacionViewModel> Certificaciones { get; set; }

        public double AniosExperiencia { get; set; } // Nueva propiedad para filtrar
       



        public List<string> Responsabilidades { get; set; } = new List<string>();

        [Newtonsoft.Json.JsonIgnore] // Para no serializar en otros casos
        public Dictionary<string, double> ExperienciaPorResponsabilidad { get; set; }




    }

    public class ResponsabilidadViewModel
    {
        public string Nombre { get; set; }
        public double DuracionTotalAnios { get; set; }
    }
}
