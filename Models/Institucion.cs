// Archivo: Models/Institucion.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [ForeignKey]

namespace SisEmpleo.Models
{
    public class Institucion
    {
        [Key]
        public int id_institucion { get; set; }

        // --- Propiedades de Clave Foránea ---
        // Estas son las columnas que existen en tu base de datos
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public int id_postulante { get; set; }

        // --- Otras Propiedades ---
        public string? nombre { get; set; }
        public string? direccion { get; set; }


        [ForeignKey("id_pais")] // Apunta a la propiedad 'id_pais' de esta misma clase
        public virtual Pais Pais { get; set; }

        [ForeignKey("id_provincia")] // Apunta a la propiedad 'id_provincia'
        public virtual Provincia Provincia { get; set; }

        [ForeignKey("id_postulante")] // Apunta a la propiedad 'id_postulante'
        public virtual Postulante Postulante { get; set; }
    }
}