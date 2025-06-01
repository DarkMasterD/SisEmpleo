// Archivo: Models/Provincia.cs
using System.Collections.Generic; // Para ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Provincia
    {
        [Key]
        public int id_provincia { get; set; }
        public int id_pais { get; set; }
        public string? nombre { get; set; }

        [ForeignKey("id_pais")]
        public virtual Pais Pais { get; set; }

        public virtual ICollection<Postulante> Postulantes { get; set; } = new List<Postulante>();
        public virtual ICollection<Institucion> Instituciones { get; set; } = new List<Institucion>();
    }
}