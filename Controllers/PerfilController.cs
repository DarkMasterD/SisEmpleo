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

            model.IdiomasPrincipalesDisponibles = await _context.Idioma
                .OrderBy(i => i.nombre)
                .Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre })
                .ToListAsync();

            model.TodosLosIdiomasDisponibles = model.IdiomasPrincipalesDisponibles;

            var idsHabilidadesActuales = model.HabilidadesActuales?.Select(h => h.IdHabilidad).ToList() ?? new List<int>();
            model.HabilidadesDisponiblesParaAnadir = await _context.Habilidad
                .Where(h => !idsHabilidadesActuales.Contains(h.id_habilidad))
                .OrderBy(h => h.nombre)
                .Select(h => new SelectListItem { Value = h.id_habilidad.ToString(), Text = h.nombre })
                .ToListAsync();

            // --- INICIO: LÓGICA PARA CARGAR INSTITUCIONES DEL USUARIO ---
            if (idPostulante.HasValue && idPostulante.Value > 0)
            {
                model.InstitucionesRegistradasPorUsuario = await _context.Institucion
                    .OrderBy(i => i.nombre)
                    .Select(i => new SelectListItem
                    {
                        Value = i.id_institucion.ToString(),
                        Text = i.nombre
                    })
                    .ToListAsync();
            }
            // --- FIN: LÓGICA PARA CARGAR INSTITUCIONES DEL USUARIO ---
        }


        public async Task<IActionResult> Index()
        {
            // ... (Tu código actual para Index, que ya funciona, se mantiene)
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
                            .OrderBy(ic => ic.Institucion?.nombre == "Idioma Principal (Auto-declarado)" ? 0 : 1)
                            .ThenBy(ic => ic.Idioma?.nombre)
                            .Select(ic => ic.Idioma?.nombre).Where(n => n != null).Distinct().ToList() ?? new List<string>(),
                Habilidades = curriculum?.HabilidadCurriculums?
                                .OrderBy(hc => hc.Habilidad?.nombre)
                                .Select(hc => hc.Habilidad?.nombre).Where(n => n != null).Distinct().ToList() ?? new List<string>()
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
                                    .Include(p => p.Curriculum) // Necesario para obtener id_curriculum
                                    .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            EditarPerfilViewModel model;
            int? idPostulanteActual = null; // Necesario para CargarListasParaEditarViewModel

            if (postulante == null)
            {
                model = new EditarPerfilViewModel { Email = usuario.email, TipoUsuario = usuario.tipo_usuario.ToString() };
                TempData["InfoMessage"] = "Completa tu perfil para continuar.";
                // idPostulanteActual sigue siendo null, CargarListasParaEditarViewModel manejará esto
            }
            else
            {
                idPostulanteActual = postulante.id_postulante; // Obtener el id del postulante
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

                // Cargar Habilidades e Idiomas Actuales para mostrar en las listas
                if (postulante.Curriculum != null)
                {
                    // Cargar HabilidadesActuales
                    model.HabilidadesActuales = await _context.Habilidad_Curriculum
                        .Where(hc => hc.id_curriculum == postulante.Curriculum.id_curriculum)
                        .Include(hc => hc.Habilidad)
                        .Select(hc => new HabilidadCvViewModel
                        {
                            IdHabilidadCurriculum = hc.id_habilidad_curriculum,
                            IdHabilidad = hc.id_habilidad,
                            NombreHabilidad = hc.Habilidad.nombre
                        }).OrderBy(h => h.NombreHabilidad).ToListAsync();

                    // Cargar OtrosIdiomasActuales
                    model.OtrosIdiomasActuales = await _context.Idioma_Curriculum
                        .Where(ic => ic.id_curriculum == postulante.Curriculum.id_curriculum &&
                                     ic.Institucion != null &&
                                     ic.Institucion.nombre != "Idioma Principal (Auto-declarado)")
                        .Include(ic => ic.Idioma)
                        .Include(ic => ic.Institucion)
                        .Select(ic => new IdiomaCvDisplayViewModel
                        {
                            IdIdiomaCurriculum = ic.id_idioma_curriculum,
                            NombreIdioma = ic.Idioma.nombre,
                            NombreInstitucion = ic.Institucion.nombre,
                            FechaObtencionFormateada = ic.fecha.ToString("dd/MM/yyyy")
                        }).OrderBy(i => i.NombreIdioma).ToListAsync();
                }
            }

            // Llamar al método auxiliar para poblar todas las listas necesarias para los dropdowns/selects
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

            // --- INICIO: Limpiar errores de ModelState para NuevoOtroIdioma ---
            // Estos campos solo se validan cuando se envía el formulario AnadirOtroIdiomaCv
            ModelState.Remove("NuevoOtroIdioma.IdiomaId");
            ModelState.Remove("NuevoOtroIdioma.InstitucionId");
            ModelState.Remove("NuevoOtroIdioma.FechaObtencion");
            // Opcionalmente, para estar más seguro, puedes intentar remover el nodo raíz del submodelo
            // aunque a veces con solo remover los campos individuales es suficiente.
            // ModelState.Remove("NuevoOtroIdioma"); 
            // --- FIN: Limpiar errores ---

            // Validaciones manuales si es necesario para el formulario principal
            if (!model.Fecha_Nacimiento.HasValue)
            { // Por ejemplo, si [Required] no es suficiente por alguna razón.
                ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "La fecha de nacimiento es obligatoria.");
            }
            else // Solo validar edad y fecha futura si se proveyó una fecha
            {
                if (model.Fecha_Nacimiento.Value > DateTime.Now)
                    ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "La fecha de nacimiento no puede ser futura.");
                if (model.Fecha_Nacimiento.Value > DateTime.Now.AddYears(-18))
                    ModelState.AddModelError(nameof(model.Fecha_Nacimiento), "Debes tener al menos 18 años.");
            }
            // Puedes añadir más validaciones manuales aquí si es necesario

            if (!ModelState.IsValid)
            {
                string errorsText = "Por favor corrija los errores del formulario: ";
                foreach (var ms in ModelState.Where(ms => ms.Value.Errors.Any()))
                {
                    errorsText += $"\n- Campo '{ms.Key}': {string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage))}";
                    System.Diagnostics.Debug.WriteLine($"Error ModelState en POST EditarPerfil (Principal): {ms.Key} - {string.Join(", ", ms.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                TempData["ErrorMessage"] = errorsText;

                var postulanteExistente = await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
                await CargarListasParaEditarViewModel(model, idUsuario.Value, model.PaisId, postulanteExistente?.id_postulante);
                return View(model);
            }

            // ... (resto de tu lógica de guardado para Postulante, Contacto, Curriculum, Idioma Principal) ...
            // ¡Importante! Esta acción EditarPerfil YA NO DEBE MODIFICAR Habilidad_Curriculum
            // ni las entradas de Idioma_Curriculum para "Otros Idiomas". Esas se manejan
            // por sus propias acciones POST (AnadirHabilidadCv, EliminarHabilidadCv, AnadirOtroIdiomaCv, EliminarOtroIdiomaCv).

            var postulante = await _context.Postulante
                                 .Include(p => p.Curriculum).ThenInclude(c => c.IdiomaCurriculums).ThenInclude(ic => ic.Institucion)
                                 .Include(p => p.Instituciones)
                                 .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            if (postulante == null)
            {
                postulante = new Postulante { id_usuario = idUsuario.Value };
                _context.Postulante.Add(postulante);
            }

            postulante.nombre = model.Nombre;
            postulante.apellido = model.Apellidos;
            postulante.id_pais = model.PaisId;
            postulante.id_provincia = model.ProvinciaId;
            if (model.Fecha_Nacimiento.HasValue) postulante.fecha_nacimiento = model.Fecha_Nacimiento.Value;
            postulante.id_idioma = model.PrimaryIdiomaId;

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

            var curriculum = postulante.Curriculum;
            if (curriculum == null)
            {
                curriculum = new Curriculum { fecha = DateTime.UtcNow };
                postulante.Curriculum = curriculum;
            }
            else
            {
                curriculum.fecha = DateTime.UtcNow;
            }

            // Manejo del Idioma Principal en Idioma_Curriculum (como antes)
            string defaultInstitutionName = "Idioma Principal (Auto-declarado)";
            if (postulante.id_idioma != 0)
            {
                var defaultInstitution = postulante.Instituciones?.FirstOrDefault(i => i.nombre == defaultInstitutionName);
                if (defaultInstitution == null)
                {
                    defaultInstitution = new Institucion { id_pais = postulante.id_pais, id_provincia = postulante.id_provincia, nombre = defaultInstitutionName };
                    if (postulante.Instituciones == null) postulante.Instituciones = new List<Institucion>();
                    postulante.Instituciones.Add(defaultInstitution);
                }
                if (curriculum.IdiomaCurriculums == null) curriculum.IdiomaCurriculums = new List<Idioma_Curriculum>();
                var oldEntries = curriculum.IdiomaCurriculums.Where(ic => ic.Institucion?.nombre == defaultInstitutionName && ic.id_idioma != postulante.id_idioma).ToList();
                _context.Idioma_Curriculum.RemoveRange(oldEntries);
                if (!curriculum.IdiomaCurriculums.Any(ic => ic.Institucion?.nombre == defaultInstitutionName && ic.id_idioma == postulante.id_idioma))
                {
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum { Curriculum = curriculum, Institucion = defaultInstitution, id_idioma = postulante.id_idioma, fecha = DateTime.UtcNow });
                }
            }
            else
            {
                if (curriculum.IdiomaCurriculums != null)
                {
                    var entriesToRemove = curriculum.IdiomaCurriculums.Where(ic => ic.Institucion?.nombre == defaultInstitutionName).ToList();
                    _context.Idioma_Curriculum.RemoveRange(entriesToRemove);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cambios principales del perfil actualizados con éxito.";
                // Redirige a la misma página para que el usuario vea los cambios y pueda seguir editando habilidades/idiomas
                return RedirectToAction(nameof(EditarPerfil));
            }
            catch (DbUpdateException ex)
            {
                System.Diagnostics.Debug.WriteLine($"DbUpdateException en EditarPerfil Principal: {ex.ToString()}");
                if (ex.InnerException != null) { System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.ToString()}"); }
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Detalles: " + (ex.InnerException?.Message ?? ex.Message));
                TempData["ErrorMessage"] = "Error al guardar el perfil: " + (ex.InnerException?.Message ?? ex.Message);
                await CargarListasParaEditarViewModel(model, idUsuario.Value, model.PaisId, postulante?.id_postulante);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirHabilidadCv(EditarPerfilViewModel model) // Recibe el ViewModel completo para el binding de IdHabilidadSeleccionadaParaAnadir
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

        // --- ACCIONES PARA "OTROS IDIOMAS" ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnadirOtroIdiomaCv(EditarPerfilViewModel model) // El model binding intentará poblar model.NuevoOtroIdioma
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null)
            {
                // Considera retornar un error JSON si es una llamada AJAX, o redirigir
                TempData["ErrorMessageOtroIdioma"] = "Sesión expirada o inválida.";
                return RedirectToAction(nameof(EditarPerfil)); // O a Login
            }

            var inputIdioma = model.NuevoOtroIdioma; // Acceder al sub-modelo que contiene los datos del formulario

            // Validar manualmente los datos de inputIdioma
            // porque `ModelState.IsValid` en este punto validaría TODO el `EditarPerfilViewModel`
            bool esValidoInputIdioma = true;
            if (inputIdioma.IdiomaId <= 0)
            {
                TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "Debe seleccionar un idioma. ";
                esValidoInputIdioma = false;
            }
            if (inputIdioma.InstitucionId <= 0)
            {
                TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "Debe seleccionar una institución. ";
                esValidoInputIdioma = false;
            }
            if (inputIdioma.FechaObtencion == default(DateTime) || inputIdioma.FechaObtencion > DateTime.Now)
            { // Fecha no puede ser default o futura
                TempData["ErrorMessageOtroIdioma"] = (TempData["ErrorMessageOtroIdioma"]?.ToString() ?? "") + "La fecha de obtención no es válida. ";
                esValidoInputIdioma = false;
            }

            if (!esValidoInputIdioma)
            {
                return RedirectToAction(nameof(EditarPerfil)); // Redirige de vuelta para mostrar el TempData
            }

            var postulante = await _context.Postulante
                .Include(p => p.Curriculum) // Asegurar que Curriculum se carga
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
                // Si el currículum es nuevo, necesitará ser guardado para tener un ID
                // antes de añadir Idioma_Curriculum, o EF Core lo manejará si SaveChanges es al final.
            }

            // Validar que la institución seleccionada pertenezca al postulante y no sea la por defecto
            bool institucionValida = await _context.Institucion
                .AnyAsync(i => i.id_institucion == inputIdioma.InstitucionId &&
                               i.id_postulante == postulante.id_postulante &&
                               i.nombre != "Idioma Principal (Auto-declarado)");
            if (!institucionValida)
            {
                TempData["ErrorMessageOtroIdioma"] = "La institución seleccionada no es válida o no le pertenece.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            bool yaExiste = await _context.Idioma_Curriculum
                .AnyAsync(ic => ic.id_curriculum == curriculum.id_curriculum &&
                               ic.id_idioma == inputIdioma.IdiomaId &&
                               ic.id_institucion == inputIdioma.InstitucionId);
            if (yaExiste)
            {
                TempData["InfoMessageOtroIdioma"] = "Este idioma con esta institución ya existe en tu CV.";
            }
            else
            {
                var idiomaDb = await _context.Idioma.FindAsync(inputIdioma.IdiomaId);
                var institucionDb = await _context.Institucion.FindAsync(inputIdioma.InstitucionId); // Ya validamos que pertenece al postulante

                if (idiomaDb != null && institucionDb != null)
                {
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        Curriculum = curriculum,
                        // id_idioma = inputIdioma.IdiomaId, // O enlazar entidad Idioma
                        // id_institucion = inputIdioma.InstitucionId, // O enlazar entidad Institucion
                        Idioma = idiomaDb,
                        Institucion = institucionDb,
                        fecha = inputIdioma.FechaObtencion
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Idioma '{idiomaDb.nombre}' añadido al CV.";
                }
                else
                {
                    TempData["ErrorMessageOtroIdioma"] = "El idioma o la institución seleccionados no son válidos.";
                }
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
                .Include(ic => ic.Institucion) // Importante para la validación
                .FirstOrDefaultAsync(ic => ic.id_idioma_curriculum == idIdiomaCurriculum &&
                                            ic.Curriculum.Postulante.id_usuario == idUsuario.Value &&
                                            ic.Institucion.nombre != "Idioma Principal (Auto-declarado)");

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
            if (usuarioParaValidacion == null || usuarioParaValidacion.tipo_usuario != 'E') return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Email = usuarioParaValidacion.email;
                model.TipoUsuario = usuarioParaValidacion.tipo_usuario.ToString();
                return View(model);
            }

            var empresa = await _context.Empresa.FirstOrDefaultAsync(e => e.id_usuario == idUsuario.Value);
            if (empresa == null)
            {
                empresa = new Empresa { id_usuario = idUsuario.Value };
                _context.Empresa.Add(empresa);
            }

            empresa.nombre = model.Nombre;
            empresa.direccion = model.Direccion;
            empresa.sector_empresa = model.Sector;
            empresa.descripcion_empresa = model.Descripcion;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil de empresa actualizado con éxito.";
                return RedirectToAction("IndexEmpresa");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Detalles: " + (ex.InnerException?.Message ?? ex.Message));
                TempData["ErrorMessage"] = "Error al guardar el perfil de empresa: " + (ex.InnerException?.Message ?? ex.Message);
                model.Email = usuarioParaValidacion.email;
                model.TipoUsuario = usuarioParaValidacion.tipo_usuario.ToString();
                return View(model);
            }
        }
    }
}