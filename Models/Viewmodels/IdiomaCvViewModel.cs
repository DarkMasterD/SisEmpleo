namespace SisEmpleo.Models.Viewmodels
{
    public class IdiomaCvViewModel
    {
        public int IdiomaId { get; set; }
        public int InstitucionId { get; set; } // El usuario selecciona de MisInstituciones
        public DateTime FechaObtencion { get; set; }
       
    }
}
