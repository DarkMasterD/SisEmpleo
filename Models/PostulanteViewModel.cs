namespace SisEmpleo.Models
{
    public class PostulanteViewModel
    {
        public Postulante Postulante { get; set; }
        public Usuario Usuario { get; set; }
        public Contacto Contacto { get; set; }
        public Curriculum Curriculum { get; set; }
        public List<FormacionAcademica> Formaciones { get; set; }
        public List<ExperienciaProfesional> Experiencias { get; set; }
    }
}
