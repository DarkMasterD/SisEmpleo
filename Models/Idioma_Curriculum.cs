using SisEmpleo.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Idioma_Curriculum
{
    [Key]
    public int id_idioma_curriculum { get; set; }
    public int id_curriculum { get; set; }
    public int id_institucion { get; set; }
    public int id_idioma { get; set; }
    public DateTime fecha { get; set; }

    [ForeignKey("id_curriculum")]
    public virtual Curriculum Curriculum { get; set; }

    [ForeignKey("id_institucion")] // Añade esta propiedad de navegación
    public virtual Institucion Institucion { get; set; }

    [ForeignKey("id_idioma")]
    public virtual Idioma Idioma { get; set; }
}
