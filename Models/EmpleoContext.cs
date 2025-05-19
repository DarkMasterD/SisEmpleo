using Microsoft.EntityFrameworkCore;

namespace SisEmpleo.Models
{
    public class EmpleoContext : DbContext
    {
        public EmpleoContext(DbContextOptions<EmpleoContext> options) : base(options)
        {
        }

        public DbSet<CategoriaProfesional> CategoriaProfesional { get; set; }
        public DbSet<Contacto> Contacto { get; set; }
        public DbSet<Habilidad> Habilidad { get; set; }
        public DbSet<Habilidad_Curriculum> Habilidad_Curriculum { get; set; } 
        public DbSet<Curriculum> Curriculum { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Especialidad> Especialidad { get; set; }
        public DbSet<ExperienciaProfesional> ExperienciaProfesional { get; set; }
        public DbSet<FormacionAcademica> FormacionAcademica { get; set; }
        public DbSet<Institucion> Institucion { get; set; }
        public DbSet<Idioma> Idioma { get; set; } 
        public DbSet<Idioma_Curriculum> Idioma_Curriculum { get; set; } 
        public DbSet<OfertaCandidatos> OfertaCandidatos { get; set; }
        public DbSet<OfertaCategoria> OfertaCategoria { get; set; }
        public DbSet<OfertaEmpleo> OfertaEmpleo { get; set; }
        public DbSet<Pais> Pais { get; set; }
        public DbSet<Postulante> Postulante { get; set; }
        public DbSet<Provincia> Provincia { get; set; }
        public DbSet<RequisitoOferta> RequisitoOferta { get; set; }
        public DbSet<SubcategoriaProfesional> SubcategoriaProfesional { get; set; }
        public DbSet<SuscripcionCategoria> SuscripcionCategoria { get; set; }
        public DbSet<Titulo> Titulo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Certificacion> Certificacion { get; set; }
        public DbSet<TrabajoEmpresa> TrabajoEmpresa { get; set; } 
        public DbSet<PrestacionLey> PrestacionLey { get; set; } 
        public DbSet<OfertaEmpleoPrestacion> OfertaEmpleoPrestacion { get; set; } 
        public DbSet<Certificacion_Curriculum> Certificacion_Curriculum { get; set; } 
    }
}
