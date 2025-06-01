// Archivo: Models/Institucion.cs
using System.Collections.Generic; // Para ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Institucion
    {
        [Key]
        public int id_institucion { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public int id_postulante { get; set; }
        public string? nombre { get; set; }
        public string? direccion { get; set; }

        [ForeignKey("id_pais")]
        public virtual Pais Pais { get; set; }

        [ForeignKey("id_provincia")]
        public virtual Provincia Provincia { get; set; }

        [ForeignKey("id_postulante")]
        public virtual Postulante Postulante { get; set; }

        // Colecciones para relaciones muchos a muchos o uno a muchos inversas
        public virtual ICollection<Idioma_Curriculum> IdiomaCurriculums { get; set; } = new List<Idioma_Curriculum>();
        public virtual ICollection<FormacionAcademica> FormacionesAcademicas { get; set; } = new List<FormacionAcademica>();
        public virtual ICollection<Certificacion_Curriculum> CertificacionCurriculums { get; set; } = new List<Certificacion_Curriculum>();
    }
}