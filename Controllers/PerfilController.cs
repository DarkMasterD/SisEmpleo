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

        public async Task<IActionResult> Index()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante
                .FirstOrDefaultAsync(p => p.id_usuario == idUsuario);

            if (postulante == null)
            {
                ViewData["Error"] = "No se encontró información del perfil.";
                return View(null);
            }

            var pais = await _context.Pais
                .FirstOrDefaultAsync(pa => pa.id_pais == postulante.id_pais);

            var provincia = await _context.Provincia
                .FirstOrDefaultAsync(pr => pr.id_provincia == postulante.id_provincia);

            var curriculum = await _context.Curriculum
                .FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);

            List<string> idiomas = new List<string>();
            List<string> habilidades = new List<string>();

            if (curriculum != null)
            {
                idiomas = await _context.Idioma_Curriculum
                    .Where(ic => ic.id_curriculum == curriculum.id_curriculum)
                    .Join(_context.Idioma,
                          ic => ic.id_idioma,
                          i => i.id_idioma,
                          (ic, i) => i.nombre)
                    .ToListAsync();

                habilidades = await _context.Habilidad_Curriculum
                    .Where(hc => hc.id_curriculum == curriculum.id_curriculum)
                    .Join(_context.Habilidad,
                          hc => hc.id_habilidad,
                          h => h.id_habilidad,
                          (hc, h) => h.nombre)
                    .ToListAsync();
            }

            var model = new PostulanteViewModel
            {
                Nombre = postulante.nombre ?? "",
                Apellidos = postulante.apellido ?? "",
                Email = usuario.email ?? "",
                Telefono = "",
                Fecha_Nacimiento = postulante.fecha_nacimiento,
                Pais = pais?.nombre ?? "",
                Provincia = provincia?.nombre ?? "",
                TipoUsuario = usuario.tipo_usuario.ToString(),

                Idiomas = idiomas,
                Habilidades = habilidades,

                FormacionAcademica = new List<FormacionAcademicaViewModel>(),
                Experiencias = new List<ExperienciaViewModel>(),
                Certificaciones = new List<CertificacionViewModel>()
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            var query = from p in _context.Postulante
                        join u in _context.Usuario on p.id_usuario equals u.id_usuario
                        where p.id_usuario == idUsuario
                        select new { Postulante = p, Usuario = u };

            var result = await query.FirstOrDefaultAsync();
            if (result == null)
                return NotFound();

            var contacto = await _context.Contacto.FirstOrDefaultAsync(c => c.id_usuario == idUsuario);
            var curriculum = await _context.Curriculum
                .FirstOrDefaultAsync(c => c.id_postulante == result.Postulante.id_postulante);

            var paises = await _context.Pais
                .Select(p => new SelectListItem { Value = p.id_pais.ToString(), Text = p.nombre })
                .ToListAsync();

            var provincias = new List<SelectListItem>();
            if (result.Postulante.id_pais != 0)
            {
                provincias = await _context.Provincia
                    .Where(pr => pr.id_pais == result.Postulante.id_pais)
                    .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                    .ToListAsync();
            }

            var idiomasDisponibles = await _context.Idioma
                .Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre })
                .ToListAsync();

            var habilidadesDisponibles = await _context.Habilidad.ToListAsync(); 

            var idiomaIds = new List<int>();
            var habilidadIds = new List<int>();
            if (curriculum != null)
            {
                idiomaIds = await _context.Idioma_Curriculum
                    .Where(ic => ic.id_curriculum == curriculum.id_curriculum)
                    .Select(ic => ic.id_idioma)
                    .ToListAsync();

                habilidadIds = await _context.Habilidad_Curriculum
                    .Where(hc => hc.id_curriculum == curriculum.id_curriculum)
                    .Select(hc => hc.id_habilidad)
                    .ToListAsync();
            }

            var model = new EditarPerfilViewModel
            {
                Nombre = result.Postulante.nombre,
                Apellidos = result.Postulante.apellido,
                Email = result.Usuario.email,
                Telefono = contacto?.telefono ?? "",
                UsarTelefonoCuenta = string.IsNullOrEmpty(contacto?.telefono),
                Fecha_Nacimiento = result.Postulante.fecha_nacimiento,
                PaisId = result.Postulante.id_pais,
                ProvinciaId = result.Postulante.id_provincia,
                Paises = paises,
                Provincias = provincias,
                IdiomasDisponibles = idiomasDisponibles,
                HabilidadesDisponibles = habilidadesDisponibles.Select(h => new SisEmpleo.Models.Habilidad { id_habilidad = h.id_habilidad, nombre = h.nombre }).ToList(), // Asegúrate que el tipo coincida con la propiedad del VM
                IdiomaIds = idiomaIds,
                HabilidadIds = habilidadIds,
                MisInstituciones = await _context.Institucion 
                    .Where(i => i.id_postulante == result.Postulante.id_postulante)
                    .Select(i => new InstitucionViewModel 
                    {
                        IdInstitucion = i.id_institucion,
                        Nombre = i.nombre,
                        IdPais = i.id_pais,
                       
                        IdProvincia = i.id_provincia,
                        Direccion = i.direccion
                    }).ToListAsync(),

                NuevaInstitucion = new InstitucionViewModel
                {
                    PaisesList = paises,
                    ProvinciasList = new List<SelectListItem>()
                }
            };

            foreach (var instViewModel in model.MisInstituciones)
            {
                var paisDeInst = await _context.Pais.FirstOrDefaultAsync(p => p.id_pais == instViewModel.IdPais);

                var provDeInst = await _context.Provincia.FirstOrDefaultAsync(p => p.id_provincia == instViewModel.IdProvincia);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<JsonResult> GetProvinciasPorPais(int id_pais)
        {
            if (id_pais <= 0)
            {
                return Json(new List<object>());
            }

            var provincias = await _context.Provincia
                .Where(p => p.id_pais == id_pais)
                .Select(p => new { id_provincia = p.id_provincia, nombre = p.nombre })
                .ToListAsync();

            return Json(provincias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");
            var postulante = await _context.Postulante
                                        .FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);

            if (postulante == null) return NotFound("Perfil de postulante no encontrado.");
            async Task PopulateDropdownsForViewModel(EditarPerfilViewModel vm)
            {
                vm.Paises = await _context.Pais
                    .Select(p => new SelectListItem { Value = p.id_pais.ToString(), Text = p.nombre })
                    .ToListAsync();
                if (vm.PaisId > 0)
                {
                    vm.Provincias = await _context.Provincia
                        .Where(pr => pr.id_pais == vm.PaisId)
                        .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                        .ToListAsync();
                }
                else
                {
                    vm.Provincias = new List<SelectListItem>();
                }
                vm.IdiomasDisponibles = await _context.Idioma
                    .Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre })
                    .ToListAsync();
                vm.HabilidadesDisponibles = (await _context.Habilidad.ToListAsync())
                                              .Select(h => new SisEmpleo.Models.Habilidad { id_habilidad = h.id_habilidad, nombre = h.nombre }).ToList(); // Asegúrate que el tipo coincida

                if (vm.NuevaInstitucion == null) vm.NuevaInstitucion = new InstitucionViewModel();
                vm.NuevaInstitucion.PaisesList = vm.Paises.ToList();
                if (vm.NuevaInstitucion.IdPais > 0)
                {
                    vm.NuevaInstitucion.ProvinciasList = await _context.Provincia
                       .Where(pr => pr.id_pais == vm.NuevaInstitucion.IdPais)
                       .Select(pr => new SelectListItem { Value = pr.id_provincia.ToString(), Text = pr.nombre })
                       .ToListAsync();
                }
                else
                {
                    vm.NuevaInstitucion.ProvinciasList = new List<SelectListItem>();
                }
                vm.MisInstituciones = await _context.Institucion
                   .Where(i => i.id_postulante == postulante.id_postulante)
                   .Select(i => new InstitucionViewModel {IdInstitucion = i.id_institucion, Nombre = i.nombre, IdPais = i.id_pais, IdProvincia = i.id_provincia, Direccion = i.direccion })
                   .ToListAsync();

            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor corrija los errores del formulario.";
                await PopulateDropdownsForViewModel(model);
                return View(model);
            }
            postulante.nombre = model.Nombre;
            postulante.apellido = model.Apellidos;
            postulante.id_pais = model.PaisId;
            postulante.id_provincia = model.ProvinciaId;
            postulante.fecha_nacimiento = model.Fecha_Nacimiento ?? default(DateTime);
            var contacto = await _context.Contacto.FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
            if (contacto == null && !string.IsNullOrEmpty(model.Telefono))
            {
                contacto = new Contacto { id_usuario = idUsuario.Value, telefono = model.Telefono };
                _context.Contacto.Add(contacto);
            }
            else if (contacto != null)
            {
                contacto.telefono = model.Telefono;
            }
            var curriculum = await _context.Curriculum.FirstOrDefaultAsync(c => c.id_postulante == postulante.id_postulante);
            if (curriculum == null)
            {
                curriculum = new Curriculum { id_postulante = postulante.id_postulante, fecha = DateTime.UtcNow };
                _context.Curriculum.Add(curriculum);
            }

            var idiomasEnCV = _context.Idioma_Curriculum.Where(ic => ic.id_curriculum == curriculum.id_curriculum);
            _context.Idioma_Curriculum.RemoveRange(idiomasEnCV);
            var habilidadesEnCV = _context.Habilidad_Curriculum.Where(hc => hc.id_curriculum == curriculum.id_curriculum);
            _context.Habilidad_Curriculum.RemoveRange(habilidadesEnCV);
            if (model.IdiomaIds != null)
            {
                foreach (var idiomaId in model.IdiomaIds)
                {
                    // AHORA MISMO ESTO DARÁ ERROR EN LA BD porque id_institucion es NOT NULL
                    // Esta lógica debe cambiar.
                    // Por ahora, lo comento para evitar el error y que el resto funcione.
                    /*
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        id_curriculum = curriculum.id_curriculum,
                        id_idioma = idiomaId,
                        // id_institucion = ??? , // ¡NECESITO ESTO DE UN FORMULARIO DIFERENTE!
                        // fecha = ???,          // ¡NECESITO ESTO!
                    });
                    */
                }
            }

            if (model.HabilidadIds != null)
            {
                foreach (var habilidadId in model.HabilidadIds)
                {
                    _context.Habilidad_Curriculum.Add(new Habilidad_Curriculum
                    {
                        id_curriculum = curriculum.id_curriculum,
                        id_habilidad = habilidadId,
                        fecha = DateTime.UtcNow
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Perfil actualizado con éxito.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "No se pudieron guardar los cambios. " + ex.InnerException?.Message);
                TempData["ErrorMessage"] = "Error al guardar el perfil: " + ex.InnerException?.Message;
                await PopulateDropdownsForViewModel(model);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInstitucionPostulante(EditarPerfilViewModel modelFromMainForm) 
        {

            var institucionModel = modelFromMainForm.NuevaInstitucion;

            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario.Value);
            if (postulante == null) return NotFound("Postulante no encontrado.");
            if (institucionModel != null && !string.IsNullOrEmpty(institucionModel.Nombre) && institucionModel.IdPais > 0 && institucionModel.IdProvincia > 0)
            {
                var nuevaInstitucion = new Institucion
                {
                    nombre = institucionModel.Nombre,
                    id_pais = institucionModel.IdPais,
                    id_provincia = institucionModel.IdProvincia,
                    direccion = institucionModel.Direccion,
                    id_postulante = postulante.id_postulante
                };
                _context.Institucion.Add(nuevaInstitucion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Institución agregada con éxito.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al agregar la institución. Verifique los campos requeridos (Nombre, País, Provincia).";
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
            if (postulante == null) return NotFound("Postulante no encontrado.");

            var institucion = await _context.Institucion
                .FirstOrDefaultAsync(i => i.id_institucion == idInstitucion && i.id_postulante == postulante.id_postulante);

            if (institucion == null)
            {
                TempData["ErrorMessage"] = "Institución no encontrada o no tiene permiso para eliminarla.";
                return RedirectToAction(nameof(EditarPerfil));
            }

            // Verificar si la institución está siendo usada antes de eliminar
            bool isUsed = await _context.Idioma_Curriculum.AnyAsync(ic => ic.id_institucion == idInstitucion);
            
            if (isUsed /* || isUsedInFormacion || isUsedInCertificacion */)
            {
                TempData["ErrorMessage"] = "No se puede eliminar la institución porque está siendo utilizada en su currículum (idiomas, formación, etc.).";
                return RedirectToAction(nameof(EditarPerfil));
            }

            _context.Institucion.Remove(institucion);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Institución eliminada con éxito.";
            return RedirectToAction(nameof(EditarPerfil));
        }


        public async Task<IActionResult> IndexEmpresa()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            if (usuario == null || usuario.tipo_usuario != 'E')
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.id_usuario == idUsuario);

            if (empresa == null)
            {
                ViewData["Error"] = "No se encontró información del perfil de la empresa.";
                return View(null);
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

            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.id_usuario == idUsuario);
            if (usuario == null || usuario.tipo_usuario != 'E')
                return RedirectToAction("Login", "Account");

            var empresa = await _context.Empresa.FirstOrDefaultAsync(e => e.id_usuario == idUsuario);
            if (empresa == null)
            {
                ViewData["Error"] = "No se encontró información del perfil de la empresa.";
                return View(new EmpresaViewModel());
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

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEmpresa(EmpresaViewModel model)
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var empresa = await _context.Empresa.FirstOrDefaultAsync(e => e.id_usuario == idUsuario);
            if (empresa == null)
            {
                ModelState.AddModelError("", "No se encontró la empresa para actualizar.");
                return View(model);
            }

            empresa.nombre = model.Nombre;
            empresa.direccion = model.Direccion;
            empresa.sector_empresa = model.Sector;
            empresa.descripcion_empresa = model.Descripcion;

            await _context.SaveChangesAsync();

            return RedirectToAction("IndexEmpresa");
        }



    }

}
