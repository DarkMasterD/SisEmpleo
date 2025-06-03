using System;
using System.Collections.Generic; // Para ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Postulante
    {
        [Key]
        public int id_postulante { get; set; }
        public int id_usuario { get; set; } // FK a Usuario
        public string? nombre { get; set; }
        public string? apellido { get; set; }
        public int id_pais { get; set; } // FK a Pais
        public int id_provincia { get; set; } // FK a Provincia
        public string? direccion { get; set; }
        public int id_idioma { get; set; } // FK a Idioma (idioma principal)
        public DateTime fecha_nacimiento { get; set; }

        // Propiedades de Navegación
        [ForeignKey("id_usuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("id_pais")]
        public virtual Pais Pais { get; set; }

        [ForeignKey("id_provincia")]
        public virtual Provincia Provincia { get; set; }

        [ForeignKey("id_idioma")]
        public virtual Idioma IdiomaPrincipal { get; set; }

        public virtual Curriculum Curriculum { get; set; }

        public virtual ICollection<Institucion> Instituciones { get; set; } = new List<Institucion>();
    }
}