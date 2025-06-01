// Archivo: Models/Pais.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SisEmpleo.Models
{
    public class Pais
    {
        [Key]
        public int id_pais { get; set; }
        public string nombre { get; set; }

        public virtual ICollection<Provincia> Provincias { get; set; } = new List<Provincia>();
        public virtual ICollection<Postulante> Postulantes { get; set; } = new List<Postulante>();
        public virtual ICollection<Institucion> Instituciones { get; set; } = new List<Institucion>();
    }
}