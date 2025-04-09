namespace SisEmpleo.Models
{
    public class Dashboard
    {
        public int IdUsuario { get; set; }
        public string TipoUsuario { get; set; }
        public string Nombre { get; set; }
        public List<CategoriaProfesional> Categorias { get; set; }
        public List<OfertaEmpleo> Ofertas { get; set; }
    }
}
