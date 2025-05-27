    using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using SisEmpleo.Models;
    using SisEmpleo.Models.Viewmodels;
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

            [HttpGet]
            public IActionResult EditarPerfil()
            {
                int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

                if (idUsuario == null)
                {
                    return RedirectToAction("Login", "Cuenta");
                }

                var datos = (from u in _context.Usuario
                             join p in _context.Postulante on u.id_usuario equals p.id_usuario
                             join c in _context.Contacto on u.id_usuario equals c.id_usuario
                             where u.id_usuario == idUsuario
                             select new EditarPerfilViewModel
                             {
                                 Nombre = p.nombre,
                                 Apellidos = p.apellido,
                                 Email = u.email,
                                 Telefono = c.telefono,
                                 Fecha_Nacimiento = p.fecha_nacimiento,
                                 TipoUsuario = u.tipo_usuario.ToString()
                             }).FirstOrDefault();

                if (datos == null)
                {
                    datos = new EditarPerfilViewModel();
                }

                // Inicializar listas referenciales (aunque sea vacías si no hay datos en BD)
                datos.Paises = _context.Pais.Select(p => new SelectListItem
                {
                    Value = p.id_pais.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.Provincias = _context.Provincia.Select(p => new SelectListItem
                {
                    Value = p.id_provincia.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.IdiomasDisponibles = _context.Idioma.Select(i => new SelectListItem
                {
                    Value = i.id_idioma.ToString(),
                    Text = i.nombre
                }).ToList();

                datos.HabilidadesDisponibles = _context.Habilidad.ToList();

                // Inicializar listas seleccionadas y objetos auxiliares si son null
                datos.IdiomaIds = new List<int>();
                datos.HabilidadIds = new List<int>();
                datos.NuevaFormacion = new FormacionAcademica();
                datos.NuevaExperiencia = new ExperienciaViewModel(); // O ExperienciaInputModel
                datos.NuevaCertificacion = new CertificacionViewModel(); // O CertificacionInputModel
                datos.Experiencias = (from exp in _context.ExperienciaProfesional
                                      join c in _context.Curriculum on exp.id_curriculum equals c.id_curriculum
                                      join p in _context.Postulante on c.id_postulante equals p.id_postulante
                                      join te in _context.TrabajoEmpresa on exp.id_trabajoempresa equals te.id_trabajoempresa
                                      join pu in _context.Puesto on exp.id_puesto equals pu.id_puesto
                                      where p.id_usuario == idUsuario
                                      select new ExperienciaViewModel
                                      {
                                          Empresa = te.nombre,
                                          Puesto = pu.nombre,
                                          FechaInicio = exp.fecha_inicio,
                                          FechaFin = exp.fecha_fin
                                      }).ToList();

                datos.Certificaciones = (from cc in _context.Certificacion_Curriculum
                                         join c in _context.Curriculum on cc.id_curriculum equals c.id_curriculum
                                         join p in _context.Postulante on c.id_postulante equals p.id_postulante
                                         join ce in _context.Certificacion on cc.id_certificacion equals ce.id_certificacion
                                         where p.id_usuario == idUsuario
                                         select new CertificacionViewModel
                                         {
                                             Id = ce.id_certificacion,
                                             Nombre = ce.nombre,
                                         }).ToList();

                datos.ListaFormacion = (from fa in _context.FormacionAcademica
                                        join c in _context.Curriculum on fa.id_curriculum equals c.id_curriculum
                                        join p in _context.Postulante on c.id_postulante equals p.id_postulante
                                        join ins in _context.Institucion on fa.id_institucion equals ins.id_institucion
                                        join t in _context.Titulo on fa.id_titulo equals t.id_titulo
                                        join esp in _context.Especialidad on t.id_especialidad equals esp.id_especialidad
                                        where p.id_usuario == idUsuario
                                        select new FormacionAcademicaViewModel
                                        {
                                            Titulo = t.nombre,
                                            Especialidad = esp.nombre,
                                            Institucion = ins.nombre
                                        }).ToList();

                return View(datos);
            }

            [HttpPost]
            public IActionResult GuardarCambios(EditarPerfilViewModel model, string command)
            {
                int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

                if (idUsuario == null)
                {
                    return RedirectToAction("Login", "Cuenta");
                }

                try
                {
                    // Obtener el postulante y usuario actual
                    var postulante = _context.Postulante.FirstOrDefault(p => p.id_usuario == idUsuario);
                    var usuario = _context.Usuario.FirstOrDefault(u => u.id_usuario == idUsuario);
                    var contacto = _context.Contacto.FirstOrDefault(c => c.id_usuario == idUsuario);
                    var curriculum = _context.Curriculum.FirstOrDefault(c => c.id_postulante == postulante.id_postulante);

                    if (postulante == null || usuario == null || contacto == null || curriculum == null)
                    {
                        TempData["ErrorMessage"] = "No se encontró el perfil del usuario.";
                        return RedirectToAction("EditarPerfil");
                    }

                    // Actualizar información básica
                    postulante.nombre = model.Nombre;
                    postulante.apellido = model.Apellidos;
                    postulante.fecha_nacimiento = model.Fecha_Nacimiento.Value;
                    postulante.id_pais = model.PaisId;
                    postulante.id_provincia = model.ProvinciaId;

                    usuario.email = model.Email;
                    contacto.telefono = model.Telefono;

                    // Manejar habilidades
                    if (model.HabilidadIds != null && model.HabilidadIds.Any())
                    {
                        // Eliminar habilidades existentes
                        var habilidadesActuales = _context.Habilidad_Curriculum
                            .Where(hc => hc.id_curriculum == curriculum.id_curriculum)
                            .ToList();

                        _context.Habilidad_Curriculum.RemoveRange(habilidadesActuales);

                        // Agregar nuevas habilidades
                        foreach (var habilidadId in model.HabilidadIds)
                        {
                            _context.Habilidad_Curriculum.Add(new Habilidad_Curriculum
                            {
                                id_curriculum = curriculum.id_curriculum,
                                id_habilidad = habilidadId
                            });
                        }
                    }

                    // Manejar experiencias profesionales
                    if (model.Experiencias != null && model.Experiencias.Any())
                    {
                        // Eliminar experiencias existentes
                        var experienciasActuales = _context.ExperienciaProfesional
                            .Where(e => e.id_curriculum == curriculum.id_curriculum)
                            .ToList();

                        _context.ExperienciaProfesional.RemoveRange(experienciasActuales);

                        // Agregar nuevas experiencias
                        foreach (var experiencia in model.Experiencias)
                        {
                            // Buscar o crear empresa
                            var empresa = _context.TrabajoEmpresa.FirstOrDefault(e => e.nombre == experiencia.Empresa);
                            if (empresa == null)
                            {
                                empresa = new TrabajoEmpresa { nombre = experiencia.Empresa };
                                _context.TrabajoEmpresa.Add(empresa);
                                _context.SaveChanges(); // Guardar para obtener el ID
                            }

                            // Buscar o crear puesto
                            var puesto = _context.Puesto.FirstOrDefault(p => p.nombre == experiencia.Puesto);
                            if (puesto == null)
                            {
                                puesto = new Puesto { nombre = experiencia.Puesto };
                                _context.Puesto.Add(puesto);
                                _context.SaveChanges(); // Guardar para obtener el ID
                            }

                            _context.ExperienciaProfesional.Add(new ExperienciaProfesional
                            {
                                id_curriculum = curriculum.id_curriculum,
                                id_trabajoempresa = empresa.id_trabajoempresa,
                                id_puesto = puesto.id_puesto,
                                fecha_inicio = experiencia.FechaInicio,
                                fecha_fin = experiencia.FechaFin
                            });
                        }
                    }

                    // Manejar formación académica
                    if (model.ListaFormacion != null && model.ListaFormacion.Any())
                    {
                        // Eliminar formación existente
                        var formacionActual = _context.FormacionAcademica
                            .Where(f => f.id_curriculum == curriculum.id_curriculum)
                            .ToList();

                        _context.FormacionAcademica.RemoveRange(formacionActual);

                        // Agregar nueva formación
                        foreach (var formacion in model.ListaFormacion)
                        {
                            // Buscar o crear institución
                            var institucion = _context.Institucion.FirstOrDefault(i => i.nombre == formacion.Institucion);
                            if (institucion == null)
                            {
                                institucion = new Institucion { nombre = formacion.Institucion };
                                _context.Institucion.Add(institucion);
                                _context.SaveChanges();
                            }

                            // Buscar o crear especialidad
                            var especialidad = _context.Especialidad.FirstOrDefault(e => e.nombre == formacion.Especialidad);
                            if (especialidad == null)
                            {
                                especialidad = new Especialidad { nombre = formacion.Especialidad };
                                _context.Especialidad.Add(especialidad);
                                _context.SaveChanges();
                            }

                            // Buscar o crear título
                            var titulo = _context.Titulo.FirstOrDefault(t => t.nombre == formacion.Titulo && t.id_especialidad == especialidad.id_especialidad);
                            if (titulo == null)
                            {
                                titulo = new Titulo
                                {
                                    nombre = formacion.Titulo,
                                    id_especialidad = especialidad.id_especialidad
                                };
                                _context.Titulo.Add(titulo);
                                _context.SaveChanges();
                            }

                            _context.FormacionAcademica.Add(new FormacionAcademica
                            {
                                id_curriculum = curriculum.id_curriculum,
                                id_institucion = institucion.id_institucion,
                                id_titulo = titulo.id_titulo
                            });
                        }
                    }

                    // Manejar certificaciones
                    if (model.Certificaciones != null && model.Certificaciones.Any())
                    {
                        // Eliminar certificaciones existentes
                        var certificacionesActuales = _context.Certificacion_Curriculum
                            .Where(cc => cc.id_curriculum == curriculum.id_curriculum)
                            .ToList();

                        _context.Certificacion_Curriculum.RemoveRange(certificacionesActuales);

                        // Agregar nuevas certificaciones
                        foreach (var cert in model.Certificaciones)
                        {
                            // Buscar o crear certificación
                            var certificacion = _context.Certificacion.FirstOrDefault(c => c.nombre == cert.Nombre);
                            if (certificacion == null)
                            {
                                certificacion = new Certificacion { nombre = cert.Nombre };
                                _context.Certificacion.Add(certificacion);
                                _context.SaveChanges();
                            }

                            _context.Certificacion_Curriculum.Add(new Certificacion_Curriculum
                            {
                                id_curriculum = curriculum.id_curriculum,
                                id_certificacion = certificacion.id_certificacion,
                                fecha = DateTime.Now // O usar fecha específica si está en el modelo
                            });
                        }
                    }

                    // Guardar todos los cambios
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Perfil actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ocurrió un error al actualizar el perfil: " + ex.Message;
                    return RedirectToAction("EditarPerfil");
                }
            }




        }

}
