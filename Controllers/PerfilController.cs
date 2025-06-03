using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using SisEmpleo.Models.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SisEmpleo.Controllers
{
    public class PerfilController : Controller
    {
        private readonly EmpleoContext _context;

        public PerfilController(EmpleoContext context)
        {
            _context = context;
        }
        private async Task CargarListasParaEditarViewModel(EditarPerfilViewModel model, int idUsuario, int? idPaisActual = null, int? idPostulante = null)
        {
            model.Paises = await _context.Pais
                .OrderBy(p => p.nombre)
                .Select(p => new SelectListItem { Value = p.id_pais.ToString(), Text = p.nombre })
                .ToListAsync();

            if (idPaisActual.HasValue && idPaisActual.Value > 0)
            {
                model.Provincias = await _context.Provincia
                    .Where(pr => pr.id_pais == idPaisActual.Value)
                    .OrderBy(pr => pr.nombre)
                    .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                    .ToListAsync();
            }
            else
            {
                model.Provincias = new List<SelectListItem>();
            }

            var todosLosIdiomas = await _context.Idioma
                .OrderBy(i => i.nombre)
                .Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre })
                .ToListAsync();
            model.IdiomasPrincipalesDisponibles = todosLosIdiomas;
            model.TodosLosIdiomasDisponibles = todosLosIdiomas;

            var idsHabilidadesActuales = model.HabilidadesActuales?.Select(h => h.IdHabilidad).ToList() ?? new List<int>();
            model.HabilidadesDisponiblesParaAnadir = await _context.Habilidad
                .Where(h => !idsHabilidadesActuales.Contains(h.id_habilidad))
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.id_habilidad.ToString(), Text = h.nombre })
                .ToListAsync();

            var institucionesDelUsuario = new List<SelectListItem>();
            var titulosDelUsuario = new List<SelectListItem>();

            if (idPostulante.HasValue && idPostulante.Value > 0)
            {
                // Load institutions created by this postulante, EXCLUDING "Autodidacta"
                institucionesDelUsuario = await _context.Institucion
                    .Where(i => i.id_postulante == idPostulante.Value && i.nombre != "Autodidacta")
                    .OrderBy(i => i.nombre)
                    .Select(i => new SelectListItem
                    {
                        Value = i.id_institucion.ToString(),
                        Text = i.nombre
                    })
                    .ToListAsync();

                // Load titles created by this postulante
                model.TitulosRegistradosPorUsuario = await _context.Titulo
                    .Where(t => t.id_postulante == idPostulante.Value)
                    .OrderBy(t => t.nombre)
                    .Select(t => new SelectListItem
                    {
                        Value = t.id_titulo.ToString(),
                        Text = $"{t.nombre} ({t.tipo})"
                    })
                    .ToListAsync();
            }

            var institucionAutodidactaGlobal = await _context.Institucion
        .FirstOrDefaultAsync(i => i.nombre == "Autodidacta" && i.id_postulante == null);

            if (institucionAutodidactaGlobal != null)
            {
                if (!institucionesDelUsuario.Any(sli => sli.Value == institucionAutodidactaGlobal.id_institucion.ToString()))
                {
                    institucionesDelUsuario.Add(new SelectListItem
                    {
                        Value = institucionAutodidactaGlobal.id_institucion.ToString(),
                        Text = institucionAutodidactaGlobal.nombre
                    });
                }
            }

            model.InstitucionesRegistradasPorUsuario = institucionesDelUsuario.OrderBy(i => i.Text).ToList();

            model.EspecialidadesDisponibles = await _context.Especialidad
                .OrderBy(e => e.nombre)
                .Select(e => new SelectListItem
                {
                    Value = e.id_especialidad.ToString(),
                    Text = e.nombre
                })
                .ToListAsync();

        }
        public async Task<IActionResult> Index()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");
            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante
                .Include(p => p.Pais).Include(p => p.Provincia)
                .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            if (postulante == null)
            {
                TempData["InfoMessage"] = "Aún no has completado tu perfil.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var contacto = await _context.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
            var curriculum = await _context.Curriculum.AsNoTracking()
                .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Idioma)
                .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Institucion)
                .Include(c => c.HabilidadCurriculums).ThenInclude(hc => hc.Habilidad)
                .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Titulo).ThenInclude(t => t.Especialidad)
                .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Institucion)
                .FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);

            var model = new PostulanteViewModel
            {
                Nombre = postulante.nombre,
                Apellidos = postulante.apellido,
                Email = usuario.email,
                Telefono = contacto?.telefono,
                Fecha_Nacimiento = postulante.fecha_nacimiento,
                Pais = postulante.Pais?.nombre,
                Provincia = postulante.Provincia?.nombre,
                TipoUsuario = usuario.tipo_usuario.ToString(),
                Idiomas = curriculum?.IdiomaCurriculums?
                            .OrderBy(ic => ic.Institucion?.nombre == "Autodidacta" ? 0 : 1)
                            .ThenBy(ic => ic.Idioma?.nombre)
                            .Select(ic => ic.Idioma?.nombre).Where(n => n != null).Distinct().ToList() ?? new List<string>(),
                Habilidades = curriculum?.HabilidadCurriculums?
                                .OrderBy(hc => hc.Habilidad?.nombre)
                                .Select(hc => hc.Habilidad?.nombre).Where(n => n != null).Distinct().ToList() ?? new List<string>(),
                FormacionesAcademicas = curriculum?.FormacionesAcademicas?
                                        .Select(fa => new FormacionAcademicaViewModel
                                        {
                                            NombreTitulo = fa.Titulo?.nombre,
                                            TipoTitulo = fa.Titulo?.tipo,
                                            NombreInstitucion = fa.Institucion?.nombre,
                                            NombreEspecialidad = fa.Titulo?.Especialidad?.nombre
                                        }).OrderBy(fa => fa.NombreTitulo).ToList() ?? new List<FormacionAcademicaViewModel>()
            };
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            var postulante = await _context.Postulante.AsNoTracking()
                                    .Include(p => p.Curriculum)
                                    .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            EditarPerfilViewModel model;
            int? idPostulanteActual = null;

            if (postulante == null)
            {
                model = new EditarPerfilViewModel { Email = usuario.email, TipoUsuario = usuario.tipo_usuario.ToString() };
                TempData["InfoMessage"] = "Completa tu perfil para continuar.";
            }
            else
            {
                idPostulanteActual = postulante.id_postulante;
                var contacto = await _context.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);

                model = new EditarPerfilViewModel
                {
                    Nombre = postulante.nombre,
                    Apellidos = postulante.apellido,
                    Email = usuario.email,
                    PrimaryIdiomaId = postulante.id_idioma,
                    Telefono = contacto?.telefono,
                    EmailContacto = contacto?.email,
                    Fecha_Nacimiento = postulante.fecha_nacimiento,
                    PaisId = postulante.id_pais,
                    ProvinciaId = postulante.id_provincia,
                    TipoUsuario = usuario.tipo_usuario.ToString()
                };

                if (postulante.Curriculum != null)
                {
                    var curriculumDb = await _context.Curriculum.AsNoTracking()
                        .Include(c => c.HabilidadCurriculums).ThenInclude(hc => hc.Habilidad)
                        .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Idioma)
                        .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Institucion)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Titulo).ThenInclude(t => t.Especialidad)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Institucion)
                        .FirstOrDefaultAsync(c => c.id_curriculum == postulante.Curriculum.id_curriculum);

                    if (curriculumDb != null)
                    {
                        model.HabilidadesActuales = await _context.Habilidad_Curriculum
                        .Where(hc => hc.id_curriculum == postulante.Curriculum.id_curriculum)
                        .Include(hc => hc.Habilidad)
                        .Select(hc => new HabilidadCvViewModel
                        {
                            IdHabilidadCurriculum = hc.id_habilidad_curriculum,
                            IdHabilidad = hc.id_habilidad,
                            NombreHabilidad = hc.Habilidad.nombre
                        }).OrderBy(h => h.NombreHabilidad).ToListAsync();

                        model.OtrosIdiomasActuales = await _context.Idioma_Curriculum
                            .Where(ic => ic.id_curriculum == postulante.Curriculum.id_curriculum &&
                                         ic.Institucion != null)
                            .Include(ic => ic.Idioma)
                            .Include(ic => ic.Institucion)
                            .Select(ic => new IdiomaCvDisplayViewModel
                            {
                                IdIdiomaCurriculum = ic.id_idioma_curriculum,
                                NombreIdioma = ic.Idioma.nombre,
                                NombreInstitucion = ic.Institucion.nombre,
                                FechaObtencionFormateada = ic.fecha.ToString("dd/MM/yyyy")
                            }).OrderBy(i => i.NombreIdioma).ToListAsync();

                        model.FormacionesAcademicasActuales = curriculumDb.FormacionesAcademicas
                            .Select(fa => new FormacionAcademicaDisplayViewModel
                            {
                                IdFormacionAcademica = fa.id_formacionacademica,
                                NombreTitulo = fa.Titulo.nombre,
                                TipoTitulo = fa.Titulo.tipo,
                                NombreInstitucion = fa.Institucion.nombre,
                                NombreEspecialidad = fa.Titulo.Especialidad?.nombre ?? "N/A"
                            }).OrderBy(f => f.NombreTitulo).ToList();
                    }
                }
            }

            await CargarListasParaEditarViewModel(model, idUsuario.Value, model.PaisId, idPostulanteActual);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuarioParaValidacion = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.id_usuario == idUsuario.Value);
            if (usuarioParaValidacion == null) return Unauthorized("Usuario no válido.");

            // --- Limpiar errores de ModelState para sub-formularios que no se envían con este POST principal ---
            ModelState.Remove("NuevoOtroIdioma.IdiomaId");
            ModelState.Remove("NuevoOtroIdioma.InstitucionId");
            ModelState.Remove("NuevoOtroIdioma.FechaObtencion");
            ModelState.Remove("NuevaFormacionAcademica.TituloId");
            ModelState.Remove("NuevaFormacionAcademica.InstitucionId");
            ModelState.Remove("NuevoTitulo.Nombre");
            ModelState.Remove("NuevoTitulo.Tipo");
            ModelState.Remove("NuevoTitulo.EspecialidadId");
            ModelState.Remove("IdHabilidadSeleccionadaParaAnadir");
            if (!model.Fecha_Nacimiento.HasValue)
            {
                ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "La fecha de nacimiento es obligatoria.");
            }
            else
            {
                if (model.Fecha_Nacimiento.Value > DateTime.Now)
                    ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "La fecha de nacimiento no puede ser futura.");
                if (model.Fecha_Nacimiento.Value > DateTime.Now.AddYears(-18))
                    ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "Debes tener al menos 18 años.");
            }

            var postulanteExistenteParaValidar = await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (model.PrimaryIdiomaId == 0 && postulanteExistenteParaValidar != null && postulanteExistenteParaValidar.id_idioma != 0)
            {
                model.PrimaryIdiomaId = postulanteExistenteParaValidar.id_idioma;
                if (ModelState.ContainsKey(nameof(model.PrimaryIdiomaId)))
                {
                    ModelState[nameof(model.PrimaryIdiomaId)].Errors.Clear(); // Limpiar el error si [Range] ya lo marcó
                }
            }
            // --- Fin Validaciones Manuales ---

            if (!ModelState.IsValid)
            {
                string errorsText = "Por favor corrija los errores del formulario principal: ";
                foreach (var ms in ModelState.Where(ms => ms.Value.Errors.Any()))
                {
                    errorsText += $"\n- Campo '{ms.Key}': {string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage))}";
                    System.Diagnostics.Debug.WriteLine($"Error ModelState en POST EditarPerfil (Principal): {ms.Key} - {string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                TempData["ErrorMessage"] = errorsText;

                // Repoblar todas las listas necesarias, incluyendo las que se cargan en el GET para Habilidades, OtrosIdiomas, Formacion
                if (postulanteExistenteParaValidar != null && postulanteExistenteParaValidar.Curriculum != null)
                {
                    var curriculumDb = await _context.Curriculum.AsNoTracking()
                        .Include(c => c.HabilidadCurriculums).ThenInclude(hc => hc.Habilidad)
                        .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Idioma)
                        .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Institucion)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Titulo).ThenInclude(t => t.Especialidad)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Institucion)
                        .FirstOrDefaultAsync(c => c.id_curriculum == postulanteExistenteParaValidar.Curriculum.id_curriculum);

                    if (curriculumDb != null)
                    {
                        model.HabilidadesActuales = curriculumDb.HabilidadCurriculums
                            .Select(hc => new HabilidadCvViewModel { IdHabilidadCurriculum = hc.id_habilidad_curriculum, IdHabilidad = hc.id_habilidad, NombreHabilidad = hc.Habilidad.nombre })
                            .OrderBy(h => h.NombreHabilidad).ToList();
                        model.OtrosIdiomasActuales = curriculumDb.IdiomaCurriculums
                            .Where(ic => ic.Institucion?.nombre != "Autodidacta")
                            .Select(ic => new IdiomaCvDisplayViewModel { IdIdiomaCurriculum = ic.id_idioma_curriculum, NombreIdioma = ic.Idioma.nombre, NombreInstitucion = ic.Institucion.nombre, FechaObtencionFormateada = ic.fecha.ToString("dd/MM/yyyy") })
                            .OrderBy(i => i.NombreIdioma).ToList();
                        model.FormacionesAcademicasActuales = curriculumDb.FormacionesAcademicas
                            .Select(fa => new FormacionAcademicaDisplayViewModel { IdFormacionAcademica = fa.id_formacionacademica, NombreTitulo = fa.Titulo.nombre, TipoTitulo = fa.Titulo.tipo, NombreInstitucion = fa.Institucion.nombre, NombreEspecialidad = fa.Titulo.Especialidad?.nombre ?? "N/A" })
                            .OrderBy(f => f.NombreTitulo).ToList();
                    }
                }
                // Asegurarse de que las listas no sean null si no se poblaron arriba
                if (model.HabilidadesActuales == null) model.HabilidadesActuales = new List<HabilidadCvViewModel>();
                if (model.OtrosIdiomasActuales == null) model.OtrosIdiomasActuales = new List<IdiomaCvDisplayViewModel>();
                if (model.FormacionesAcademicasActuales == null) model.FormacionesAcademicasActuales = new List<FormacionAcademicaDisplayViewModel>();

                await CargarListasParaEditarViewModel(model, idUsuario.Value, model.PaisId, postulanteExistenteParaValidar?.id_postulante);
                return View(model);
            }

            var postulante = await _context.Postulante
                                 .Include(p => p.Curriculum) // Para acceder y modificar p.Curriculum
                                    .ThenInclude(c => c.IdiomaCurriculums) // Para modificar Idioma_Curriculum
                                        .ThenInclude(ic => ic.Institucion) // Para acceder a Institucion.nombre
                                 .Include(p => p.Instituciones) // Para la colección de instituciones del postulante
                                 .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            if (postulante == null)
            {
                postulante = new Postulante { id_usuario = idUsuario.Value };
                _context.Postulante.Add(postulante);
            }

            // 1. Actualizar datos del Postulante
            postulante.nombre = model.Nombre;
            postulante.apellido = model.Apellidos;
            postulante.id_pais = model.PaisId;
            postulante.id_provincia = model.ProvinciaId;
            postulante.fecha_nacimiento = model.Fecha_Nacimiento.Value; // Ya validado que tiene valor
            postulante.id_idioma = model.PrimaryIdiomaId;

            // 2. Actualizar/Crear Contacto
            var contacto = await _context.Contacto.FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
            if (contacto == null)
            {
                if (!string.IsNullOrWhiteSpace(model.Telefono) || !string.IsNullOrWhiteSpace(model.EmailContacto))
                {
                    contacto = new Contacto { id_usuario = idUsuario.Value };
                    _context.Contacto.Add(contacto);
                }
            }
            if (contacto != null)
            {
                contacto.telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono;
                contacto.email = string.IsNullOrWhiteSpace(model.EmailContacto) ? null : model.EmailContacto;
            }

            // 3. Manejar Curriculum
            var curriculum = postulante.Curriculum;
            if (curriculum == null)
            {
                curriculum = new Curriculum { fecha = DateTime.UtcNow };
                postulante.Curriculum = curriculum; // EF Core lo asociará al guardar Postulante
            }
            else
            {
                curriculum.fecha = DateTime.UtcNow;
            }

            // --- 4.A. Manejo del Idioma Principal con Institución "Autodidacta" GLOBAL ---
            string nombreInstitucionAutodidacta = "Autodidacta";
            Institucion institucionGlobalAutodidacta = null;

            if (postulante.id_idioma != 0) // Si se seleccionó un idioma principal
            {
                institucionGlobalAutodidacta = await _context.Institucion
                    .FirstOrDefaultAsync(i => i.nombre == nombreInstitucionAutodidacta && i.id_postulante == null);
                if (institucionGlobalAutodidacta == null) // Si no existe, la creamos
                {
                    institucionGlobalAutodidacta = new Institucion
                    {
                        nombre = nombreInstitucionAutodidacta,
                        id_pais = postulante.id_pais,
                        id_provincia = postulante.id_provincia,
                        id_postulante = null // Es global
                    };
                    _context.Institucion.Add(institucionGlobalAutodidacta);
                }
                if (curriculum.IdiomaCurriculums == null) curriculum.IdiomaCurriculums = new List<Idioma_Curriculum>();
                var oldPrimaryEntries = curriculum.IdiomaCurriculums
                    .Where(ic => (ic.Institucion?.nombre == nombreInstitucionAutodidacta && ic.Institucion?.id_postulante == null && ic.id_idioma != postulante.id_idioma) || // Era "Autodidacta" pero cambió el idioma
                                 (ic.id_idioma == postulante.id_idioma && (ic.Institucion?.nombre != nombreInstitucionAutodidacta || ic.Institucion?.id_postulante != null)) // Mismo idioma pero estaba con otra institución
                    ).ToList();
                _context.Idioma_Curriculum.RemoveRange(oldPrimaryEntries);
                bool currentPrimaryInCvExists = false;
                if (institucionGlobalAutodidacta.id_institucion > 0) // Si la institución Autodidacta ya existía (tiene ID)
                {
                    currentPrimaryInCvExists = curriculum.IdiomaCurriculums
                        .Any(ic => ic.id_institucion == institucionGlobalAutodidacta.id_institucion &&
                                   ic.id_idioma == postulante.id_idioma);
                }

                if (!currentPrimaryInCvExists)
                {
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        Curriculum = curriculum,
                        Institucion = institucionGlobalAutodidacta, // Enlazar la entidad global (nueva o existente)
                        id_idioma = postulante.id_idioma,
                        fecha = DateTime.UtcNow
                    });
                }
            }
            else // No se seleccionó idioma principal (o es 0), eliminar la entrada de la institución global "Autodidacta"
            {
                if (curriculum.IdiomaCurriculums != null)
                {
                    var entriesToRemove = curriculum.IdiomaCurriculums
                        .Where(ic => ic.Institucion?.nombre == nombreInstitucionAutodidacta && ic.Institucion?.id_postulante == null)
                        .ToList();
                    _context.Idioma_Curriculum.RemoveRange(entriesToRemove);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cambios principales del perfil actualizados con éxito.";
                return RedirectToAction(nameof(EditarPerfil));
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DbUpdateException en EditarPerfil Principal: {ex.ToString()}");
                if (ex.InnerException != null) { System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.ToString()}"); }
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Detalles: " + (ex.InnerException?.Message ?? ex.Message));
                TempData["ErrorMessage"] = "Error al guardar el perfil: " + (ex.InnerException?.Message ?? ex.Message);

                var postulanteIdOnErrorRetry = (await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value))?.id_postulante;
                if (postulanteIdOnErrorRetry.HasValue && postulanteIdOnErrorRetry.Value > 0)
                {
                    var curriculumParaError = await _context.Curriculum.AsNoTracking()
                        .Include(c => c.HabilidadCurriculums).ThenInclude(hc => hc.Habilidad)
                         .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Idioma)
                        .Include(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Institucion)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Titulo).ThenInclude(t => t.Especialidad)
                        .Include(c => c.FormacionesAcademicas).ThenInclude(fa => fa.Institucion)
                        .FirstOrDefaultAsync(c => c.id_postulante == postulanteIdOnErrorRetry.Value);
                    if (curriculumParaError != null)
                    {
                        model.HabilidadesActuales = curriculumParaError.HabilidadCurriculums.Select(hc => new HabilidadCvViewModel { /*...*/ }).ToList();
                        model.OtrosIdiomasActuales = curriculumParaError.IdiomaCurriculums.Where(ic => ic.Institucion?.nombre != "Autodidacta").Select(ic => new IdiomaCvDisplayViewModel { /*...*/ }).ToList();
                        model.FormacionesAcademicasActuales = curriculumParaError.FormacionesAcademicas.Select(fa => new FormacionAcademicaDisplayViewModel { /*...*/ }).ToList();
                    }
                }
                if (model.HabilidadesActuales == null) model.HabilidadesActuales = new List<HabilidadCvViewModel>();
                if (model.OtrosIdiomasActuales == null) model.OtrosIdiomasActuales = new List<IdiomaCvDisplayViewModel>();
                if (model.FormacionesAcademicasActuales == null) model.FormacionesAcademicasActuales = new List<FormacionAcademicaDisplayViewModel>();

                await CargarListasParaEditarViewModel(model, idUsuario.Value, model.PaisId, postulanteIdOnErrorRetry);
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirHabilidadCv(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return Json(new { success = false, message = "Sesión expirada." }); // Mejor para AJAX

            if (model.IdHabilidadSeleccionadaParaAnadir <= 0)
            {
                TempData["ErrorMessageHabilidad"] = "Por favor, seleccione una habilidad válida.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var postulante = await _context.Postulante.Include(p => p.Curriculum)
                .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null)
            {
                TempData["ErrorMessageHabilidad"] = "Postulante no encontrado.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var curriculum = postulante.Curriculum;
            if (curriculum == null)
            {
                curriculum = new Curriculum { fecha = DateTime.UtcNow };
                postulante.Curriculum = curriculum;
            }

            bool yaExiste = await _context.Habilidad_Curriculum
                .AnyAsync(hc => hc.id_curriculum == curriculum.id_curriculum && hc.id_habilidad == model.IdHabilidadSeleccionadaParaAnadir);

            if (yaExiste)
            {
                TempData["InfoMessageHabilidad"] = "Esta habilidad ya está en tu perfil.";
            }
            else
            {
                var habilidadDb = await _context.Habilidad.FindAsync(model.IdHabilidadSeleccionadaParaAnadir);
                if (habilidadDb != null)
                {
                    _context.Habilidad_Curriculum.Add(new Habilidad_Curriculum
                    {
                        Curriculum = curriculum,
                        id_habilidad = model.IdHabilidadSeleccionadaParaAnadir,
                        fecha = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessageHabilidad"] = $"Habilidad '{habilidadDb.nombre}' añadida.";
                }
                else
                {
                    TempData["ErrorMessageHabilidad"] = "Habilidad no encontrada.";
                }
            }
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarHabilidadCv(int idHabilidadCurriculum)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return Json(new { success = false, message = "Sesión expirada." });

            var habilidadCvAEliminar = await _context.Habilidad_Curriculum
                .Include(hc => hc.Curriculum).ThenInclude(c => c.Postulante)
                .Include(hc => hc.Habilidad)
                .FirstOrDefaultAsync(hc => hc.id_habilidad_curriculum == idHabilidadCurriculum &&
                                            hc.Curriculum.Postulante.id_usuario == idUsuario.Value);

            if (habilidadCvAEliminar != null)
            {
                _context.Habilidad_Curriculum.Remove(habilidadCvAEliminar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessageHabilidad"] = $"Habilidad '{habilidadCvAEliminar.Habilidad?.nombre}' eliminada.";
            }
            else
            {
                TempData["ErrorMessageHabilidad"] = "No se pudo eliminar la habilidad.";
            }
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirTitulo(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) { TempData["ErrorMessageFormacion"] = "Sesión expirada."; return RedirectToAction(nameof(EditarPerfil)); }

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null) { TempData["ErrorMessageFormacion"] = "Debe guardar su perfil básico primero."; return RedirectToAction(nameof(EditarPerfil)); }

            var nuevoTituloInput = model.NuevoTitulo;
            if (string.IsNullOrWhiteSpace(nuevoTituloInput.Nombre) || string.IsNullOrWhiteSpace(nuevoTituloInput.Tipo) || nuevoTituloInput.EspecialidadId <= 0)
            {
                TempData["ErrorMessageFormacion"] = "Para añadir un título, el nombre, tipo y especialidad son requeridos.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            bool tituloExiste = await _context.Titulo.AnyAsync(t => t.id_postulante == postulante.id_postulante &&
                               t.nombre.ToLower() == nuevoTituloInput.Nombre.ToLower() &&
                               t.tipo == nuevoTituloInput.Tipo && t.id_especialidad == nuevoTituloInput.EspecialidadId);
            if (tituloExiste)
            {
                TempData["InfoMessageFormacion"] = "Este título ya lo ha registrado previamente.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            Titulo tituloEntidad = new Titulo
            {
                nombre = nuevoTituloInput.Nombre,
                descripcion = nuevoTituloInput.Descripcion,
                tipo = nuevoTituloInput.Tipo,
                id_especialidad = nuevoTituloInput.EspecialidadId,
                id_postulante = postulante.id_postulante
            };
            _context.Titulo.Add(tituloEntidad);
            await _context.SaveChangesAsync();
            TempData["SuccessMessageFormacion"] = $"Título '{tituloEntidad.nombre}' registrado. Ahora puede asignarlo a su formación.";
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirOtroIdiomaCv(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null)
            {
                TempData["ErrorMessageOtroIdioma"] = "Sesión expirada o inválida.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var inputIdioma = model.NuevoOtroIdioma;

            if (inputIdioma.IdiomaId <= 0 || inputIdioma.InstitucionId <= 0 || inputIdioma.FechaObtencion == default(DateTime) || inputIdioma.FechaObtencion > DateTime.Now)
            {
                if (inputIdioma.IdiomaId <= 0) TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "Debe seleccionar un idioma. ";
                if (inputIdioma.InstitucionId <= 0) TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "Debe seleccionar una institución. ";
                if (inputIdioma.FechaObtencion == default(DateTime) || inputIdioma.FechaObtencion > DateTime.Now) TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "La fecha de obtención no es válida. ";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var postulante = await _context.Postulante.Include(p => p.Curriculum)
                                    .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null)
            {
                TempData["ErrorMessageOtroIdioma"] = "Postulante no encontrado.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var curriculum = postulante.Curriculum;
            if (curriculum == null)
            {
                curriculum = new Curriculum { fecha = DateTime.UtcNow };
                postulante.Curriculum = curriculum;
            }

            var institucionSeleccionada = await _context.Institucion
                                              .AsNoTracking() // No necesitamos rastrearla para esta validación
                                              .FirstOrDefaultAsync(i => i.id_institucion == inputIdioma.InstitucionId);

            bool institucionValidaParaEsteUso = false;
            if (institucionSeleccionada != null)
            {
                if (institucionSeleccionada.nombre == "Autodidacta" && institucionSeleccionada.id_postulante == null)
                {
                    institucionValidaParaEsteUso = true;
                }
                else if (institucionSeleccionada.id_postulante == postulante.id_postulante && institucionSeleccionada.nombre != "Autodidacta")
                {
                    institucionValidaParaEsteUso = true;
                }
            }

            if (!institucionValidaParaEsteUso)
            {
                TempData["ErrorMessageOtroIdioma"] = "La institución seleccionada no es válida. Puede seleccionar 'Autodidacta' o una de sus instituciones personales (que no se llame 'Autodidacta').";
                return RedirectToAction(nameof(EditarPerfil));
            }

            bool yaExiste = await _context.Idioma_Curriculum
                .AnyAsync(ic => ic.id_curriculum == curriculum.id_curriculum &&
                               ic.id_idioma == inputIdioma.IdiomaId &&
                               ic.id_institucion == inputIdioma.InstitucionId); // Usar el ID de la institución seleccionada

            if (yaExiste)
            {
                TempData["InfoMessageOtroIdioma"] = "Este idioma con esta institución ya existe en tu CV.";
            }
            else
            {
                var idiomaDb = await _context.Idioma.FindAsync(inputIdioma.IdiomaId);
                // No necesitamos re-buscar 'institucionSeleccionada' si ya la tenemos y es válida

                if (idiomaDb != null && institucionSeleccionada != null)
                {
                    // Validar que la institucionSeleccionada tenga un ID válido (no sea una entidad nueva con ID=0 si no la guardaste antes)
                    if (institucionSeleccionada.id_institucion <= 0)
                    {
                        TempData["ErrorMessageOtroIdioma"] = "La institución seleccionada no tiene un ID válido. Intente de nuevo.";
                        return RedirectToAction(nameof(EditarPerfil));
                    }


                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        Curriculum = curriculum,          // Puedes enlazar la entidad Curriculum
                        id_idioma = inputIdioma.IdiomaId, // Asigna la FK de Idioma

                        // Para Institucion, asigna SOLO la FK si ya existe en la BD y no la estás modificando
                        id_institucion = institucionSeleccionada.id_institucion, // <--- USA ESTO
                                                                                 // Institucion = institucionSeleccionada, // Evita esto si solo quieres crear la relación

                        fecha = inputIdioma.FechaObtencion
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessageOtroIdioma"] = $"Idioma '{idiomaDb.nombre}' en '{institucionSeleccionada.nombre}' añadido al CV.";
                }
            }
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirFormacionAcademica(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) { TempData["ErrorMessageFormacion"] = "Sesión expirada."; return RedirectToAction(nameof(EditarPerfil)); }

            var formacionInput = model.NuevaFormacionAcademica;
            if (formacionInput.TituloId <= 0 || formacionInput.InstitucionId <= 0)
            {
                TempData["ErrorMessageFormacion"] = "Debe seleccionar un título y una institución. Si no hay opciones disponibles, cree un título o institución primero.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var postulante = await _context.Postulante.Include(p => p.Curriculum)
                .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null) { TempData["ErrorMessageFormacion"] = "Postulante no encontrado."; return RedirectToAction(nameof(EditarPerfil)); }

            var curriculum = postulante.Curriculum;
            if (curriculum == null) { curriculum = new Curriculum { fecha = DateTime.UtcNow }; postulante.Curriculum = curriculum; }
            bool institucionValida = await _context.Institucion.AnyAsync(i =>
                i.id_institucion == formacionInput.InstitucionId &&
                (i.id_postulante == postulante.id_postulante || (i.nombre == "Autodidacta" && i.id_postulante == null)));
            bool tituloValido = await _context.Titulo.AnyAsync(t => t.id_titulo == formacionInput.TituloId && t.id_postulante == postulante.id_postulante);

            if (!institucionValida || !tituloValido)
            {
                TempData["ErrorMessageFormacion"] = "El título o la institución seleccionada no son válidos o no le pertenecen. Asegúrese de seleccionar una institución válida.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            bool yaExiste = await _context.FormacionAcademica.AnyAsync(fa => fa.id_curriculum == curriculum.id_curriculum &&
                                   fa.id_titulo == formacionInput.TituloId && fa.id_institucion == formacionInput.InstitucionId);
            if (yaExiste)
            {
                TempData["InfoMessageFormacion"] = "Esta formación académica ya existe en tu CV.";
            }
            else
            {
                _context.FormacionAcademica.Add(new FormacionAcademica
                {
                    Curriculum = curriculum,
                    id_titulo = formacionInput.TituloId,
                    id_institucion = formacionInput.InstitucionId
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessageFormacion"] = "Formación académica añadida al CV.";
            }
            return RedirectToAction(nameof(EditarPerfil));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarOtroIdiomaCv(int idIdiomaCurriculum)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var idiomaCvAEliminar = await _context.Idioma_Curriculum
                .Include(ic => ic.Curriculum).ThenInclude(c => c.Postulante)
                .Include(ic => ic.Idioma)
                .Include(ic => ic.Institucion) 
                .FirstOrDefaultAsync(ic => ic.id_idioma_curriculum == idIdiomaCurriculum &&
                                            ic.Curriculum.Postulante.id_usuario == idUsuario.Value &&
                                            ic.Institucion.nombre != "Autodidacta");

            if (idiomaCvAEliminar != null)
            {
                _context.Idioma_Curriculum.Remove(idiomaCvAEliminar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessageOtroIdioma"] = $"Idioma '{idiomaCvAEliminar.Idioma?.nombre}' eliminado del CV.";
            }
            else
            {
                TempData["ErrorMessageOtroIdioma"] = "No se pudo eliminar el idioma del CV.";
            }
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFormacionAcademica(int idFormacionAcademica)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) { TempData["ErrorMessageFormacion"] = "Sesión expirada."; return RedirectToAction(nameof(EditarPerfil)); }

            var formacionAEliminar = await _context.FormacionAcademica
                .Include(fa => fa.Curriculum).ThenInclude(c => c.Postulante)
                .Include(fa => fa.Titulo).Include(fa => fa.Institucion) // Para el mensaje
                .FirstOrDefaultAsync(fa => fa.id_formacionacademica == idFormacionAcademica &&
                                            fa.Curriculum.Postulante.id_usuario == idUsuario.Value);
            if (formacionAEliminar != null)
            {
                _context.FormacionAcademica.Remove(formacionAEliminar);
                await _context.SaveChangesAsync();
                TempData["SuccessMessageFormacion"] = $"Formación '{formacionAEliminar.Titulo?.nombre}' en '{formacionAEliminar.Institucion?.nombre}' eliminada.";
            }
            else
            {
                TempData["ErrorMessageFormacion"] = "No se pudo eliminar la formación académica.";
            }
            return RedirectToAction(nameof(EditarPerfil));
        }


        [HttpGet]
        public async Task<JsonResult> GetProvinciasPorPais(int id_pais)
        {
            if (id_pais <= 0) return Json(new List<object>());
            var provincias = await _context.Provincia
                .Where(p => p.id_pais == id_pais).OrderBy(p => p.nombre)
                .Select(p => new { id_provincia = p.id_provincia, nombre = p.nombre }).ToListAsync();
            return Json(provincias);
        }

        public async Task<IActionResult> IndexEmpresa()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null || usuario.tipo_usuario != 'E') return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.id_usuario == idUsuario.Value);

            if (empresa == null)
            {
                TempData["InfoMessage"] = "Aún no has completado el perfil de tu empresa.";
                return RedirectToAction(nameof(EditarEmpresa));
            }

            var model = new EmpresaViewModel
            {
                Nombre = empresa.nombre,
                Email = usuario.email,
                Direccion = empresa.direccion,
                Descripcion = empresa.descripcion_empresa,
                Sector = empresa.sector_empresa,
                TipoUsuario = usuario.tipo_usuario.ToString()
            };
            return View("IndexEmpresa", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditarEmpresa()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null || usuario.tipo_usuario != 'E') return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresa.AsNoTracking().FirstOrDefaultAsync(e => e.id_usuario == idUsuario.Value);

            EmpresaViewModel model;
            if (empresa == null)
            {
                model = new EmpresaViewModel { Email = usuario.email, TipoUsuario = usuario.tipo_usuario.ToString() };
                TempData["InfoMessage"] = "Completa el perfil de tu empresa.";
            }
            else
            {
                model = new EmpresaViewModel
                {
                    Nombre = empresa.nombre,
                    Email = usuario.email,
                    Direccion = empresa.direccion,
                    Descripcion = empresa.descripcion_empresa,
                    Sector = empresa.sector_empresa,
                    TipoUsuario = usuario.tipo_usuario.ToString()
                };
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpresa(EmpresaViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuarioParaValidacion = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.id_usuario == idUsuario.Value);
            if (usuarioParaValidacion == null || usuarioParaValidacion.tipo_usuario != 'E')
            {
                TempData["ErrorMessage"] = "Acceso no autorizado."; // Mensaje más claro
                return RedirectToAction("Index", "Home"); // O a una página de error
            }

            if (!ModelState.IsValid)
            {
                string errorsText = "Por favor corrija los errores del formulario (Empresa): ";
                foreach (var ms in ModelState.Where(ms => ms.Value.Errors.Any()))
                {
                    errorsText += $"\n- Campo '{ms.Key}': ";
                    foreach (var error in ms.Value.Errors)
                    {
                        errorsText += error.ErrorMessage + " ";
                        System.Diagnostics.Debug.WriteLine($"Error ModelState (Empresa) en '{ms.Key}': {error.ErrorMessage}");
                    }
                }
                TempData["ErrorMessage"] = errorsText;

                model.Email = usuarioParaValidacion.email;
                model.TipoUsuario = usuarioParaValidacion.tipo_usuario.ToString();
                return View(model); // Retorna a la vista EditarEmpresa.cshtml
            }

            var empresa = await _context.Empresa.FirstOrDefaultAsync(e => e.id_usuario == idUsuario.Value);
            bool isNewEmpresa = false;
            if (empresa == null)
            {
                empresa = new Empresa { id_usuario = idUsuario.Value };
                _context.Empresa.Add(empresa);
                isNewEmpresa = true;
                System.Diagnostics.Debug.WriteLine($"Creando nueva entidad Empresa para id_usuario: {idUsuario.Value}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Actualizando entidad Empresa existente (ID DB: {empresa.id_empresa}) para id_usuario: {idUsuario.Value}");
            }
            bool hasChanges = false;

            if (empresa.nombre != model.Nombre)
            {
                empresa.nombre = model.Nombre;
                hasChanges = true;
            }
            if (empresa.direccion != model.Direccion)
            {
                empresa.direccion = model.Direccion; // Permitir que sea null si el ViewModel lo envía así y la BD lo permite
                hasChanges = true;
            }
            if (empresa.sector_empresa != model.Sector)
            { // ViewModel usa "Sector", Entidad usa "sector_empresa"
                empresa.sector_empresa = model.Sector;
                hasChanges = true;
            }
            if (empresa.descripcion_empresa != model.Descripcion)
            { // ViewModel usa "Descripcion", Entidad usa "descripcion_empresa"
                empresa.descripcion_empresa = model.Descripcion;
                hasChanges = true;
            }

            if (!isNewEmpresa && !hasChanges)
            {
                TempData["InfoMessage"] = "No se detectaron cambios para guardar en el perfil de la empresa.";
                return RedirectToAction("IndexEmpresa");
            }

            try
            {
                int cambios = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"SaveChangesAsync() para Empresa ejecutado. Cambios guardados en DB: {cambios}");

                TempData["SuccessMessage"] = "Perfil de empresa actualizado con éxito.";
                return RedirectToAction("IndexEmpresa");
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DbUpdateException al guardar Empresa: {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception (Guardando Empresa): {ex.InnerException.ToString()}");
                }
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Detalles: " + (ex.InnerException?.Message ?? ex.Message));
                TempData["ErrorMessage"] = "Error al guardar el perfil de empresa: " + (ex.InnerException?.Message ?? ex.Message);
                model.Email = usuarioParaValidacion.email;
                model.TipoUsuario = usuarioParaValidacion.tipo_usuario.ToString();
                return View(model);
            }
        }

    }
}