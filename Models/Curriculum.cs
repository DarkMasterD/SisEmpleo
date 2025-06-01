// Archivo: Models/Curriculum.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SisEmpleo.Models
{
    public class Curriculum
    {
        [Key]
        public int id_curriculum { get; set; }
        public int id_postulante { get; set; } // FK a Postulante
        public DateTime fecha { get; set; }

        [ForeignKey("id_postulante")]
        public virtual Postulante Postulante { get; set; }

        public virtual ICollection<Idioma_Curriculum> IdiomaCurriculums { get; set; } = new List<Idioma_Curriculum>();
        public virtual ICollection<Habilidad_Curriculum> HabilidadCurriculums { get; set; } = new List<Habilidad_Curriculum>();
        public virtual ICollection<FormacionAcademica> FormacionesAcademicas { get; set; } = new List<FormacionAcademica>();
        public virtual ICollection<ExperienciaProfesional> ExperienciasProfesionales { get; set; } = new List<ExperienciaProfesional>();
        public virtual ICollection<Certificacion_Curriculum> CertificacionCurriculums { get; set; } = new List<Certificacion_Curriculum>();
    }
}