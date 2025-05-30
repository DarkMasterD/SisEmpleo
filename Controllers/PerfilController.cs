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
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            if (usuario == null)
            {
                return RedirectToAction("Login", "Account");
            }

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

            var idiomas = await _context.Idioma_Curriculum
                .Where(ic => ic.id_curriculum == postulante.id_postulante)
                .Join(_context.Idioma,
                      ic => ic.id_idioma,
                      i => i.id_idioma,
                      (ic, i) => i.nombre)
                .ToListAsync();

            var habilidades = await _context.Habilidad_Curriculum
                .Where(hc => hc.id_curriculum == postulante.id_postulante)
                .Join(_context.Habilidad,
                      hc => hc.id_habilidad,
                      h => h.id_habilidad,
                      (hc, h) => h.nombre)
                .ToListAsync();

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

                Idiomas = idiomas ?? new List<string>(),
                Habilidades = habilidades ?? new List<string>(),

                FormacionAcademica = new List<FormacionAcademicaViewModel>(),
                Experiencias = new List<ExperienciaViewModel>(),
                Certificaciones = new List<CertificacionViewModel>()
            };

            return View(model);
        }

        public async Task<IActionResult> EditarPerfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario);
            if (postulante == null) return NotFound();

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.id_usuario == idUsuario);

            // Cargar listas para selects
            var paises = await _context.Pais.Select(p => new SelectListItem { Value = p.id_pais.ToString(), Text = p.nombre }).ToListAsync();
            var provincias = await _context.Provincia.Where(pr => pr.id_provincia == postulante.id_pais)
                .Select(pr => new SelectListItem { Value = pr.id_pais.ToString(), Text = pr.nombre }).ToListAsync();

            var idiomasDisponibles = await _context.Idioma.Select(i => new SelectListItem { Value = i.id_idioma.ToString(), Text = i.nombre }).ToListAsync();

            var habilidadesDisponibles = await _context.Habilidad.ToListAsync();

            // Obtener ids seleccionados
            var idiomaIds = await _context.Idioma_Curriculum
                .Where(ic => ic.id_curriculum == postulante.id_postulante)
                .Select(ic => ic.id_idioma)
                .ToListAsync();

            var habilidadIds = await _context.Habilidad_Curriculum
                .Where(hc => hc.id_curriculum == postulante.id_postulante)
                .Select(hc => hc.id_habilidad)
                .ToListAsync();

            var model = new EditarPerfilViewModel
            {
                Nombre = postulante.nombre,
                Apellidos = postulante.apellido,
                Email = usuario.email,
                Telefono = "", // Carga desde tabla Contacto si tienes
                Fecha_Nacimiento = postulante.fecha_nacimiento,
                TipoUsuario = usuario.tipo_usuario.ToString(),

                PaisId = postulante.id_pais,
                ProvinciaId = postulante.id_provincia,
                Paises = paises,
                Provincias = provincias,

                IdiomaIds = idiomaIds,
                IdiomasDisponibles = idiomasDisponibles,

                HabilidadIds = habilidadIds,
                HabilidadesDisponibles = habilidadesDisponibles,

                // Inicializa listas vacías o carga datos para formación, experiencia y certificaciones si quieres
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            

            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            if (idUsuario == null) return RedirectToAction("Login", "Account");

            var postulante = await _context.Postulante.FirstOrDefaultAsync(p => p.id_usuario == idUsuario);
            if (postulante == null) return NotFound();

            postulante.nombre = model.Nombre;
            postulante.apellido = model.Apellidos;
            postulante.id_pais = model.PaisId;
            postulante.id_provincia = model.ProvinciaId;
            postulante.fecha_nacimiento = model.Fecha_Nacimiento ?? postulante.fecha_nacimiento;

            _context.Postulante.Update(postulante);

            // Actualizar idiomas
            var idiomasExistentes = _context.Idioma_Curriculum.Where(ic => ic.id_curriculum == postulante.id_postulante);
            _context.Idioma_Curriculum.RemoveRange(idiomasExistentes);

            if (model.IdiomaIds != null)
            {
                foreach (var idiomaId in model.IdiomaIds)
                {
                    _context.Idioma_Curriculum.Add(new Idioma_Curriculum
                    {
                        id_curriculum = postulante.id_postulante,
                        id_idioma = idiomaId,
                        fecha = DateTime.Now
                    });
                }
            }

            // Actualizar habilidades
            var habilidadesExistentes = _context.Habilidad_Curriculum.Where(hc => hc.id_curriculum == postulante.id_postulante);
            _context.Habilidad_Curriculum.RemoveRange(habilidadesExistentes);

            if (model.HabilidadIds != null)
            {
                foreach (var habilidadId in model.HabilidadIds)
                {
                    _context.Habilidad_Curriculum.Add(new Habilidad_Curriculum
                    {
                        id_curriculum = postulante.id_postulante,
                        id_habilidad = habilidadId,
                        fecha = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }

}
