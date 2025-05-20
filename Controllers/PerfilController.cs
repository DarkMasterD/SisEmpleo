    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using SisEmpleo.Models;
    using SisEmpleo.Services;

    namespace SisEmpleo.Controllers
    {
    
        public class PerfilController : Controller
        {
            private readonly EmpleoContext _context;

            public PerfilController(EmpleoContext context)
            {
                _context = context;
            }

            public IActionResult Index()
            {
                // Obtener el id del usuario de la sesión
                int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

                if (idUsuario == null)
                {
                    // Usuario no logueado, redirigir a login o mostrar error
                    return RedirectToAction("Login", "Cuenta");
                }

                var postulante = (from u in _context.Usuario
                                  join p in _context.Postulante on u.id_usuario equals p.id_usuario
                                  join c in _context.Contacto on u.id_usuario equals c.id_usuario
                                  join pa in _context.Pais on p.id_pais equals pa.id_pais
                                  join pr in _context.Provincia on p.id_provincia equals pr.id_provincia
                                  where u.id_usuario == idUsuario
                                  select new PostulanteViewModel
                                  {
                                      Nombre = p.nombre,
                                      Apellidos = p.apellido,
                                      Email = u.email,
                                      Telefono = c.telefono,
                                      Fecha_Nacimiento = p.fecha_nacimiento,
                                      Pais = pa.nombre,
                                      Provincia = pr.nombre,
                                      TipoUsuario = u.tipo_usuario.ToString(),
                                      Idiomas = (from i in _context.Idioma
                                                 where i.id_idioma == p.id_idioma
                                                 select i.nombre).ToList(),

                                      Habilidades = (from cur in _context.Curriculum
                                                     join hc in _context.Habilidad_Curriculum on cur.id_curriculum equals hc.id_curriculum
                                                     join h in _context.Habilidad on hc.id_habilidad equals h.id_habilidad
                                                     where cur.id_postulante == p.id_postulante
                                                     select h.nombre).ToList(),

                                      FormacionAcademica = (from fa in _context.FormacionAcademica
                                                            join cur in _context.Curriculum on fa.id_curriculum equals cur.id_curriculum
                                                            join ins in _context.Institucion on fa.id_institucion equals ins.id_institucion
                                                            join tit in _context.Titulo on fa.id_titulo equals tit.id_titulo
                                                            join esp in _context.Especialidad on tit.id_especialidad equals esp.id_especialidad
                                                            where cur.id_postulante == p.id_postulante
                                                            select new FormacionAcademicaViewModel
                                                            {
                                                                Titulo = tit.nombre,
                                                                Especialidad = esp.nombre,
                                                                Institucion = ins.nombre
                                                            }).ToList(),

                                      Experiencias = (from ex in _context.ExperienciaProfesional
                                                      join cur in _context.Curriculum on ex.id_curriculum equals cur.id_curriculum
                                                      join p in _context.Puesto on ex.id_puesto equals p.id_puesto
                                                      join te in _context.TrabajoEmpresa on ex.id_trabajoempresa equals te.id_trabajoempresa
                                                      where cur.id_postulante == p.id_postulante
                                                      select new ExperienciaViewModel
                                                      {
                                                          Puesto = p.nombre,
                                                          Empresa = te.nombre,
                                                          FechaInicio = ex.fecha_inicio,
                                                          FechaFin = ex.fecha_fin
                                                      }).ToList(),

                                      Certificaciones = (from ce in _context.Certificacion
                                                         join cc in _context.Certificacion_Curriculum on ce.id_certificacion equals cc.id_certificacion
                                                         join c in _context.Curriculum on cc.id_curriculum equals c.id_curriculum
                                                         where c.id_postulante == p.id_postulante
                                                         select new CertificacionViewModel
                                                         {
                                                             Nombre = ce.nombre,
                                                             FechaObtencion = cc.fecha
                                                         }).ToList()
                                  }).FirstOrDefault();

                return View(postulante);
            }



        }

    }
