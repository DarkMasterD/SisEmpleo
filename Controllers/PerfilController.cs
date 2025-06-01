using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models; // Asegúrate que este namespace es correcto para EmpleoContext y tus modelos
using SisEmpleo.Models.Viewmodels;
// using SisEmpleo.Services; // Descomenta si usas [AutenticacionPostulante], etc.
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

        // Método auxiliar para poblar dropdowns y listas del ViewModel
        private async Task PopulateViewModelForEdit(EditarPerfilViewModel model, int idUsuario, int? idPostulanteActual = null)
        {
            model.Paises = await _context.Pais
                .OrderBy(p => p.nombre)
                .Select(p => new SelectListItem { Value = p.id_pais.ToString(), Text = p.nombre })
                .ToListAsync();

            if (model.PaisId > 0)
            {
                model.Provincias = await _context.Provincia
                    .Where(pr => pr.id_pais == model.PaisId)
                    .OrderBy(pr => pr.nombre)
                    .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                    .ToListAsync();
            }
            else
            {
                model.Provincias = new List<SelectListItem>();
            }

            // Para el idioma principal del Postulante
            model.IdiomasPrincipalesDisponibles = await _context.Idioma
                .OrderBy(i => i.nombre)
                .Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre })
                .ToListAsync();

            // Para el multiselect de idiomas del CV (si se va a implementar de forma más robusta)
            // Por ahora, EditarPerfilViewModel tiene IdiomasDisponibles como IEnumerable<SelectListItem>
            // y también IdiomaIds como List<int>. Asegúrate de que esto se alinee con tu formulario.
            model.IdiomasDisponibles = model.IdiomasPrincipalesDisponibles; // Puedes usar la misma lista o una filtrada

            // HabilidadesDisponibles debe ser List<SisEmpleo.Models.Habilidad> según tu ViewModel
            model.HabilidadesDisponibles = await _context.Habilidad
                                            .OrderBy(h => h.nombre)
                                            .Select(h => new Habilidad { id_habilidad = h.id_habilidad, nombre = h.nombre, id_usuario = h.id_usuario })
                                            .ToListAsync(); // No es necesario el Select si ya es del tipo correcto y no se modifica

            int? postulanteIdParaInstituciones = idPostulanteActual;
            if (postulanteIdParaInstituciones == null || postulanteIdParaInstituciones == 0)
            { // Corrección: chequear también si es 0
                var tempPostulante = await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario);
                if (tempPostulante != null) postulanteIdParaInstituciones = tempPostulante.id_postulante;
            }

            if (postulanteIdParaInstituciones != null && postulanteIdParaInstituciones > 0)
            {
                model.MisInstituciones = await _context.Institucion
                    .Where(i => i.id_postulante == postulanteIdParaInstituciones)
                    .Include(i => i.Pais)
                    .Include(i => i.Provincia)
                    .OrderBy(i => i.nombre)
                    .Select(i => new InstitucionViewModel
                    {
                        IdInstitucion = i.id_institucion,
                        Nombre = i.nombre,
                        IdPais = i.id_pais,
                        NombrePais = i.Pais != null ? i.Pais.nombre : "N/A",
                        IdProvincia = i.id_provincia,
                        NombreProvincia = i.Provincia != null ? i.Provincia.nombre : "N/A",
                        Direccion = i.direccion
                    })
                    .ToListAsync();
            }
            else
            {
                model.MisInstituciones = new List<InstitucionViewModel>();
            }

            if (model.NuevaInstitucion == null) model.NuevaInstitucion = new InstitucionViewModel();
            model.NuevaInstitucion.PaisesList = model.Paises.ToList();
            if (model.NuevaInstitucion.IdPais > 0)
            {
                model.NuevaInstitucion.ProvinciasList = await _context.Provincia
                    .Where(pr => pr.id_pais == model.NuevaInstitucion.IdPais)
                    .OrderBy(pr => pr.nombre)
                    .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                    .ToListAsync();
            }
            else
            {
                model.NuevaInstitucion.ProvinciasList = new List<SelectListItem>();
            }
        }

        // Muestra el perfil del postulante
        public async Task<IActionResult> Index()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante
                .Include(p => p.Pais)
                .Include(p => p.Provincia)
                .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            if (postulante == null)
            {
                TempData["InfoMessage"] = "Aún no has completado tu perfil. Por favor, completa tus datos.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            var contacto = await _context.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
            var curriculum = await _context.Curriculum.AsNoTracking()
                                .FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);

            List<string> idiomas = new List<string>();
            List<string> habilidades = new List<string>();

            if (curriculum != null)
            {
                idiomas = await _context.Idioma_Curriculum
                    .Where(ic => ic.id_curriculum == curriculum.id_curriculum)
                    .Include(ic => ic.Idioma)
                    .Include(ic => ic.Institucion)
                    .OrderBy(ic => ic.Institucion != null && ic.Institucion.nombre == "Idioma Principal (Auto-declarado)" ? 0 : 1)
                    .ThenBy(ic => ic.Idioma.nombre)
                    .Select(ic => ic.Idioma.nombre)
                    .Distinct()
                    .ToListAsync();

                habilidades = await _context.Habilidad_Curriculum
                    .Where(hc => hc.id_curriculum == curriculum.id_curriculum)
                    .Include(hc => hc.Habilidad)
                    .OrderBy(hc => hc.Habilidad.nombre)
                    .Select(hc => hc.Habilidad.nombre)
                    .Distinct()
                    .ToListAsync();
            }

            var model = new PostulanteViewModel
            {
                Nombre = postulante.nombre ?? "",
                Apellidos = postulante.apellido ?? "",
                Email = usuario.email ?? "",
                Telefono = contacto?.telefono ?? "",
                Fecha_Nacimiento = postulante.fecha_nacimiento,
                Pais = postulante.Pais?.nombre ?? "N/A",
                Provincia = postulante.Provincia?.nombre ?? "N/A",
                TipoUsuario = usuario.tipo_usuario.ToString(),
                Idiomas = idiomas,
                Habilidades = habilidades
                // Inicializa otras listas si tu PostulanteViewModel las tiene (FormacionAcademica, etc.)
            };
            return View(model);
        }

        // GET: Muestra el formulario para editar el perfil
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FindAsync(idUsuario.Value);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            var postulante = await _context.Postulante
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            EditarPerfilViewModel model;
            int? idPostulanteActual = null;

            if (postulante == null) // Si es la primera vez, o no tiene perfil de postulante
            {
                model = new EditarPerfilViewModel
                {
                    Email = usuario.email,
                    TipoUsuario = usuario.tipo_usuario.ToString(),
                    HabilidadIds = new List<int>(),
                    IdiomaIds = new List<int>() // Para los otros idiomas del CV
                };
                TempData["InfoMessage"] = "Completa tu perfil para continuar.";
            }
            else // El postulante ya existe
            {
                idPostulanteActual = postulante.id_postulante;
                var contacto = await _context.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
                var curriculum = await _context.Curriculum.AsNoTracking()
                    .Include(c => c.IdiomaCurriculums) // Incluir para cargar IdiomaIds
                        .ThenInclude(ic => ic.Institucion) // Necesario para el filtro de idioma principal
                    .Include(c => c.HabilidadCurriculums) // Incluir para cargar HabilidadIds
                    .FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);

                model = new EditarPerfilViewModel
                {
                    Nombre = postulante.nombre,
                    Apellidos = postulante.apellido,
                    Email = usuario.email,
                    PrimaryIdiomaId = postulante.id_idioma, // Cargar idioma principal
                    Telefono = contacto?.telefono,
                    EmailContacto = contacto?.email,
                    Fecha_Nacimiento = postulante.fecha_nacimiento,
                    PaisId = postulante.id_pais,
                    ProvinciaId = postulante.id_provincia,
                    TipoUsuario = usuario.tipo_usuario.ToString(),
                    HabilidadIds = curriculum?.HabilidadCurriculums?.Select(hc => hc.id_habilidad).ToList() ?? new List<int>(),
                    IdiomaIds = curriculum?.IdiomaCurriculums?
                                    .Where(ic => !(ic.Institucion?.nombre == "Idioma Principal (Auto-declarado)" && ic.Institucion?.id_postulante == postulante.id_postulante))
                                    .Select(ic => ic.id_idioma).ToList() ?? new List<int>()
                };
            }

            await PopulateViewModelForEdit(model, idUsuario.Value, idPostulanteActual);
            return View(model);
        }

        // POST: Guarda los cambios del perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var usuarioParaValidacion = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.id_usuario == idUsuario.Value);
            if (usuarioParaValidacion == null) return Unauthorized("Usuario no válido.");


            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores del formulario.";
                var postulanteIdOnError = (await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value))?.id_postulante;
                await PopulateViewModelForEdit(model, idUsuario.Value, postulanteIdOnError);
                return View(model);
            }

            var postulante = await _context.Postulante
                                 .Include(p => p.Curriculum) // Para acceder a p.Curriculum
                                 .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            bool isNewPostulante = false;
            if (postulante == null)
            {
                postulante = new Postulante { id_usuario = idUsuario.Value };
                _context.Postulante.Add(postulante);
                isNewPostulante = true;
            }

            // 1. Actualizar datos del Postulante
            postulante.nombre = model.Nombre;
            postulante.apellido = model.Apellidos;
            postulante.id_pais = model.PaisId;
            postulante.id_provincia = model.ProvinciaId;
            postulante.fecha_nacimiento = model.Fecha_Nacimiento.Value;
            postulante.id_idioma = model.PrimaryIdiomaId; // Guardar el ID del idioma principal

            // 2. Actualizar Contacto
            var contacto = await _context.Contacto.FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
            if (contacto == null)
            {
                if (!string.IsNullOrWhiteSpace(model.Telefono) || !string.IsNullOrWhiteSpace(model.EmailContacto))
                {
                    contacto = new Contacto { id_usuario = idUsuario.Value }; // No Postulante = postulante, es por id_usuario
                    _context.Contacto.Add(contacto);
                }
            }
            if (contacto != null)
            { // Asignar incluso si es nuevo
                contacto.telefono = model.Telefono;
                contacto.email = model.EmailContacto;
            }

            // 3. Manejar Curriculum
            var curriculum = postulante.Curriculum;
            if (curriculum == null && !isNewPostulante)
            {
                curriculum = await _context.Curriculum.FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);
            }
            if (curriculum == null)
            {
                curriculum = new Curriculum { /* id_postulante se establecerá por EF Core al asignar Postulante = postulante */ fecha = DateTime.UtcNow };
                postulante.Curriculum = curriculum; // Enlazar curriculum al postulante si es nuevo
                // _context.Curriculum.Add(curriculum); // No es necesario si se enlaza y postulante es nuevo o se actualiza
            }
            else
            {
                curriculum.fecha = DateTime.UtcNow;
            }

            // --- 4.A. Manejar Idioma Principal en Idioma_Curriculum ---
            string defaultInstitutionName = "Idioma Principal (Auto-declarado)";
            Institucion defaultInstitutionForPrimary = null;

            if (postulante.id_idioma != 0)
            { // Si se seleccionó un idioma principal
                // Siempre buscar o crear la institución por defecto asociada al postulante
                defaultInstitutionForPrimary = await _context.Institucion
                    .FirstOrDefaultAsync(i => i.id_postulante == postulante.id_postulante && i.nombre == defaultInstitutionName);

                if (defaultInstitutionForPrimary == null)
                {
                    defaultInstitutionForPrimary = new Institucion
                    {
                        id_pais = postulante.id_pais,
                        id_provincia = postulante.id_provincia,
                        nombre = defaultInstitutionName,
                        // id_postulante se establecerá por EF Core al asignar Postulante = postulante
                    };
                    // Enlazar la nueva institución al postulante
                    if (postulante.Instituciones == null) postulante.Instituciones = new List<Institucion>();
                    postulante.Instituciones.Add(defaultInstitutionForPrimary);
                    // _context.Institucion.Add(defaultInstitutionForPrimary); // No es necesario si se añade a la colección del postulante
                }

                // Remover entradas anteriores de idioma principal para este CV y esta institución por defecto, si el idioma cambió
                // Es importante que 'curriculum' y 'defaultInstitutionForPrimary' ya tengan sus IDs si son existentes,
                // o que EF Core pueda resolverlo si son nuevos y se usan propiedades de navegación.
                var oldPrimaryCvEntries = await _context.Idioma_Curriculum
                    .Where(ic => ic.Curriculum.id_postulante == postulante.id_postulante &&
                                 ic.Institucion.nombre == defaultInstitutionName &&
                                 ic.Institucion.id_postulante == postulante.id_postulante &&
                                 ic.id_idioma != postulante.id_idioma)
                    .ToListAsync();
                if (oldPrimaryCvEntries.Any()) _context.Idioma_Curriculum.RemoveRange(oldPrimaryCvEntries);

                // Asegurar que la entrada actual del idioma principal exista
                bool currentPrimaryInCvExists = await _context.Idioma_Curriculum
                    .AnyAsync(ic => ic.Curriculum.id_postulante == postulante.id_postulante &&
                                   ic.Institucion.nombre == defaultInstitutionName &&
                                   ic.Institucion.id_postulante == postulante.id_postulante &&
                                   ic.id_idioma == postulante.id_idioma);

                if (!currentPrimaryInCvExists)
                {
                    // Si defaultInstitutionForPrimary es nuevo, EF Core lo insertará y usará su ID.
                    // Si curriculum es nuevo, EF Core lo insertará y usará su ID.
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        Curriculum = curriculum,
                        Institucion = defaultInstitutionForPrimary,
                        id_idioma = postulante.id_idioma,
                        fecha = DateTime.UtcNow
                    });
                }
            }
            else
            { // No se seleccionó idioma principal (o es 0), eliminar la entrada de la institución por defecto
                var existingDefaultEntries = await _context.Idioma_Curriculum
                    .Where(ic => ic.Curriculum.id_postulante == postulante.id_postulante &&
                                 ic.Institucion.nombre == defaultInstitutionName &&
                                 ic.Institucion.id_postulante == postulante.id_postulante)
                    .ToListAsync();
                if (existingDefaultEntries.Any()) _context.Idioma_Curriculum.RemoveRange(existingDefaultEntries);
            }

            // --- 4.B. Manejar Habilidades en Habilidad_Curriculum ---
            // Primero, obtener las habilidades actuales del currículum para eliminarlas
            var habilidadesEnCV = await _context.Habilidad_Curriculum
                .Where(hc => hc.Curriculum.id_postulante == postulante.id_postulante).ToListAsync();
            _context.Habilidad_Curriculum.RemoveRange(habilidadesEnCV);

            if (model.HabilidadIds != null)
            { // Añadir las nuevas habilidades seleccionadas
                foreach (var habilidadId in model.HabilidadIds)
                {
                    _context.Habilidad_Curriculum.Add(new Habilidad_Curriculum
                    {
                        Curriculum = curriculum,
                        id_habilidad = habilidadId,
                        fecha = DateTime.UtcNow
                    });
                }
            }

            // --- 4.C. Manejar OTROS Idiomas del CV (model.IdiomaIds) ---
            // Esta sección necesita un rediseño completo en el ViewModel y Formulario.
            // Deberías tener una lista de objetos en tu ViewModel que contenga { IdiomaId, InstitucionId, Fecha }
            // y un mecanismo en el formulario para gestionar estas entradas (añadir/eliminar).
            // La lógica actual de `model.IdiomaIds` (List<int>) es insuficiente.

            // Comento la eliminación masiva que podría borrar la entrada del idioma principal gestionada arriba.
            /*
            var otrosIdiomasNoPrincipales = await _context.Idioma_Curriculum
                .Where(ic => ic.Curriculum.id_postulante == postulante.id_postulante &&
                             !(ic.Institucion.nombre == defaultInstitutionName && ic.Institucion.id_postulante == postulante.id_postulante))
                .ToListAsync();
            _context.Idioma_Curriculum.RemoveRange(otrosIdiomasNoPrincipales);

            if (model.IdiomaIds != null) {
                foreach (var otroIdiomaId in model.IdiomaIds) {
                    // Aquí necesitarías el id_institucion y fecha para este 'otroIdiomaId'
                    // que debería venir del modelo de alguna forma estructurada.
                    // Ejemplo conceptual:
                    // var infoOtroIdioma = model.OtrosIdiomasInfo.FirstOrDefault(oii => oii.IdiomaId == otroIdiomaId);
                    // if(infoOtroIdioma != null) {
                    //    _context.Idioma_Curriculum.Add(new Idioma_Curriculum {
                    //        Curriculum = curriculum,
                    //        id_idioma = otroIdiomaId,
                    //        id_institucion = infoOtroIdioma.InstitucionId,
                    //        fecha = infoOtroIdioma.FechaObtencion
                    //    });
                    // }
                }
            }
            */

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log detallado del error para depuración
                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex.ToString()}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.ToString()}");
                }
                ModelState.AddModelError("", "No se pudieron guardar los cambios. Detalles: " + (ex.InnerException?.Message ?? ex.Message));
                TempData["ErrorMessage"] = "Error al guardar el perfil: " + (ex.InnerException?.Message ?? ex.Message);

                var postulanteIdOnErrorRetry = (await _context.Postulante.AsNoTracking().FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value))?.id_postulante;
                await PopulateViewModelForEdit(model, idUsuario.Value, postulanteIdOnErrorRetry);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetProvinciasPorPais(int id_pais)
        {
            if (id_pais <= 0) return Json(new List<object>());
            var provincias = await _context.Provincia
                .Where(p => p.id_pais == id_pais)
                .OrderBy(p => p.nombre)
                .Select(p => new { id_provincia = p.id_provincia, nombre = p.nombre })
                .ToListAsync();
            return Json(provincias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstitucionPostulante(EditarPerfilViewModel modelFromMainForm)
        {
            var institucionModel = modelFromMainForm.NuevaInstitucion;
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null)
            {
                TempData["ErrorMessage"] = "Debes guardar tu perfil básico antes de añadir instituciones.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            if (institucionModel != null &&
                !string.IsNullOrEmpty(institucionModel.Nombre) &&
                institucionModel.IdPais > 0 &&
                institucionModel.IdProvincia > 0)
            {
                var nuevaInstitucion = new Institucion
                {
                    nombre = institucionModel.Nombre,
                    id_pais = institucionModel.IdPais,
                    id_provincia = institucionModel.IdProvincia,
                    direccion = institucionModel.Direccion,
                    // id_postulante se establecerá por EF Core al enlazar la entidad Postulante
                    Postulante = postulante // Enlazar la entidad Postulante
                };
                _context.Institucion.Add(nuevaInstitucion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Institución agregada con éxito.";
            }
            else
            {
                var errors = new List<string>();
                if (string.IsNullOrEmpty(institucionModel?.Nombre)) errors.Add("Nombre es requerido.");
                if (institucionModel?.IdPais <= 0) errors.Add("País es requerido.");
                if (institucionModel?.IdProvincia <= 0) errors.Add("Provincia es requerida.");
                TempData["ErrorMessage"] = "Error al agregar la institución. " + string.Join(" ", errors);
            }
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstitucionPostulante(int idInstitucion)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null) return NotFound("Postulante no encontrado."); // O redirigir a EditarPerfil

            var institucion = await _context.Institucion
                .FirstOrDefaultAsync(i => i.id_institucion == idInstitucion && i.id_postulante == postulante.id_postulante);

            if (institucion == null)
            {
                TempData["ErrorMessage"] = "Institución no encontrada o no tiene permiso para eliminarla.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            bool isUsedInIdioma = await _context.Idioma_Curriculum.AnyAsync(ic => ic.id_institucion == idInstitucion);
            bool isUsedInFormacion = await _context.FormacionAcademica.AnyAsync(fa => fa.id_institucion == idInstitucion);
            bool isUsedInCertificacion = await _context.Certificacion_Curriculum.AnyAsync(cc => cc.id_institucion == idInstitucion);

            if (isUsedInIdioma || isUsedInFormacion || isUsedInCertificacion)
            {
                TempData["ErrorMessage"] = "No se puede eliminar la institución porque está siendo utilizada en su currículum.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            _context.Institucion.Remove(institucion);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Institución eliminada con éxito.";
            return RedirectToAction(nameof(EditarPerfil));
        }

        // --- Métodos para Empresa ---
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