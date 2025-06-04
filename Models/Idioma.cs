// Archivo: Models/Idioma.cs
using System.Collections.Generic; // Para ICollection
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Idioma
    {
        [Key]
        public int id_idioma { get; set; }
        public string? nombre { get; set; }

        // Para los postulantes que tienen este idioma como principal
        public virtual ICollection<Postulante> PostulantesConEsteIdiomaPrincipal { get; set; } = new List<Postulante>();

        // Para las entradas en Idioma_Curriculum
        public virtual ICollection<Idioma_Curriculum> IdiomaEnCurriculums { get; set; } = new List<Idioma_Curriculum>();
    }
}