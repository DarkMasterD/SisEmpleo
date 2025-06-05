using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SisEmpleo.Controllers
{
    public class OfertaEmpleoPostulanteController : Controller
    {
        private readonly EmpleoContext _EmpleoContext;

        public OfertaEmpleoPostulanteController(EmpleoContext context)
        {
            _EmpleoContext = context;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var tipoUsuario = HttpContext.Session.GetString("tipo_usuario");
            if (tipoUsuario != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var totalOffers = _EmpleoContext.OfertaEmpleo.Count();
                var activeOffers = _EmpleoContext.OfertaEmpleo.Count(o => o.estado == true);

                var ofertas = (from o in _EmpleoContext.OfertaEmpleo
                               join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais into paisJoin
                               from p in paisJoin.DefaultIfEmpty()
                               join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia into provJoin
                               from pro in provJoin.DefaultIfEmpty()
                               join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa into empJoin
                               from e in empJoin.DefaultIfEmpty()
                               where o.estado == true
                               orderby o.fecha_publicacion descending
                               select new
                               {
                                   Id = o.id_ofertaempleo,
                                   Titulo = o.titulo,
                                   Vacantes = o.vacante,
                                   Salario = o.salario, // double
                                   Duracion_Contrato = o.duracion_contrato,
                                   Fecha_Publicacion = o.fecha_publicacion,
                                   Nombre_Empresa = e != null ? e.nombre : "Desconocida",
                                   Ubi_Pais = p != null ? p.nombre : "Desconocido",
                                   Ubi_Pro = pro != null ? pro.nombre : "Desconocido"
                               }).ToList();

                ViewBag.ofertas = ofertas;
                ViewBag.TotalOffers = totalOffers;
                ViewBag.ActiveOffers = activeOffers;
                ViewBag.TipoUsuario = tipoUsuario;

                return View("Listar");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las ofertas: " + ex.Message;
                ViewBag.ofertas = new List<object>();
                ViewBag.TotalOffers = 0;
                ViewBag.ActiveOffers = 0;
                ViewBag.TipoUsuario = tipoUsuario;
                return View("Listar");
            }
        }

        private List<Dictionary<string, string>> SepararHorario(string horario)
        {
            List<Dictionary<string, string>> SepHorario = new();
            string[] Horas = horario.Split(";");
            string[] Dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };

            for (int i = 0; i < Dias.Length; i++)
            {
                int k = i * 4;
                string Horario1Inicio = Horas[k];
                string Horario1Fin = Horas[k + 1];
                string Horario2Inicio = Horas[k + 2];
                string Horario2Fin = Horas[k + 3];

                string HorarioCon1 = Horario1Inicio + " - " + Horario1Fin;
                string HorarioCon2 = Horario2Inicio + " - " + Horario2Fin;

                if (Horario1Inicio == "00:00" && Horario1Fin == "00:00")
                    HorarioCon1 = "Sin Horario";
                if (Horario2Inicio == "00:00" && Horario2Fin == "00:00")
                    HorarioCon2 = "Sin Horario";

                SepHorario.Add(new Dictionary<string, string>
                {
                    { "Dia", Dias[i] },
                    { "Horario1", HorarioCon1 },
                    { "Horario2", HorarioCon2 }
                });
            }

            return SepHorario;
        }

        [HttpGet]
        public IActionResult VerOferta(int id_ofertaempleo)
        {
            if (HttpContext.Session.GetString("tipo_usuario") != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var oferta = (from o in _EmpleoContext.OfertaEmpleo
                              join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais into paisJoin
                              from p in paisJoin.DefaultIfEmpty()
                              join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia into provJoin
                              from pro in provJoin.DefaultIfEmpty()
                              join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa into empJoin
                              from e in empJoin.DefaultIfEmpty()
                              where o.id_ofertaempleo == id_ofertaempleo && o.estado == true
                              select new
                              {
                                  Id = o.id_ofertaempleo,
                                  Titulo = o.titulo,
                                  Descripcion = o.descripcion,
                                  Vacantes = o.vacante,
                                  Salario = o.salario,
                                  Duracion_Contrato = o.duracion_contrato,
                                  Fecha_Publicacion = o.fecha_publicacion,
                                  Nombre_Empresa = e != null ? e.nombre : "Desconocida",
                                  Ubi_Pais = p != null ? p.nombre : "Desconocido",
                                  Ubi_Pro = pro != null ? pro.nombre : "Desconocido",
                                  Horario = o.horario
                              }).FirstOrDefault();

                if (oferta == null)
                {
                    return NotFound();
                }

                var requisitos = (from r in _EmpleoContext.RequisitoOferta
                                  join h in _EmpleoContext.Habilidad on r.id_habilidad equals h.id_habilidad
                                  where r.id_ofertaempleo == id_ofertaempleo
                                  select h.nombre).ToList();

                ViewBag.Oferta = oferta;
                ViewBag.Horario = SepararHorario(oferta.Horario);
                ViewBag.Requisitos = requisitos;

                var id_usuario = HttpContext.Session.GetInt32("idUsuario");
                var hasApplied = _EmpleoContext.OfertaCandidatos
                    .Any(oc => oc.id_ofertaempleo == id_ofertaempleo && oc.id_usuario == id_usuario);
                ViewBag.HasApplied = hasApplied;

                return View();
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostularseOferta(int id_ofertaempleo)
        {
            var tipoUsuario = HttpContext.Session.GetString("tipo_usuario");
            if (tipoUsuario != "P")
            {
                return Json(new { success = false, message = "Acceso denegado. Debes iniciar sesión como postulante.", redirectTo = Url.Action("Login", "Account") });
            }

            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            int? idPostulante = HttpContext.Session.GetInt32("id_postulante");

            if (idUsuario == null || idUsuario.Value == 0 || idPostulante == null || idPostulante.Value == 0) // Más robusto
            {
                return Json(new { success = false, message = "Sesión inválida o información de postulante faltante.", redirectTo = Url.Action("Login", "Account") });
            }

            System.Diagnostics.Debug.WriteLine($"Iniciando PostularseOferta para id_ofertaempleo: {id_ofertaempleo}, id_usuario: {idUsuario.Value}, id_postulante: {idPostulante.Value}");

            try
            {
                bool yaPostulado = await _EmpleoContext.OfertaCandidatos
                    .AnyAsync(oc => oc.id_ofertaempleo == id_ofertaempleo && oc.id_usuario == idUsuario.Value);
                if (yaPostulado)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Ya postulado.");
                    return Json(new { success = false, message = "Ya te has postulado a esta oferta." });
                }

                // Verificar perfil completo
                var contactoPostulante = await _EmpleoContext.Contacto.AsNoTracking().FirstOrDefaultAsync(c => c.id_usuario == idUsuario.Value);
                if (contactoPostulante == null || string.IsNullOrWhiteSpace(contactoPostulante.telefono))
                {
                    System.Diagnostics.Debug.WriteLine("Error: Falta información de contacto (teléfono).");
                    return Json(new { success = false, message = "Completa tu información de contacto (teléfono) en tu perfil antes de postularte.", redirectTo = Url.Action("EditarPerfil", "Perfil") });
                }

                var curriculumPostulante = await _EmpleoContext.Curriculum.AsNoTracking()
                                .Include(c => c.FormacionesAcademicas)
                                .Include(c => c.HabilidadCurriculums)
                                .FirstOrDefaultAsync(c => c.id_postulante == idPostulante.Value);

                if (curriculumPostulante == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error: Curriculum no encontrado.");
                    return Json(new { success = false, message = "Completa tu currículum en tu perfil antes de postularte.", redirectTo = Url.Action("EditarPerfil", "Perfil") });
                }
                if (curriculumPostulante.FormacionesAcademicas == null || !curriculumPostulante.FormacionesAcademicas.Any())
                {
                    System.Diagnostics.Debug.WriteLine("Error: Falta formación académica.");
                    return Json(new { success = false, message = "Añade al menos una formación académica a tu currículum antes de postularte.", redirectTo = Url.Action("EditarPerfil", "Perfil") });
                }

                // Verificar requisitos de habilidades
                var habilidadesRequeridasIds = await _EmpleoContext.RequisitoOferta
                    .Where(r => r.id_ofertaempleo == id_ofertaempleo)
                    .Select(r => r.id_habilidad)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Habilidades requeridas para oferta {id_ofertaempleo}: {string.Join(", ", habilidadesRequeridasIds)}");

                if (habilidadesRequeridasIds.Any())
                {
                    // Obtener IDs de habilidades del candidato desde Habilidad_Curriculum
                    // Asegúrate que curriculumPostulante.HabilidadCurriculums no sea null
                    var habilidadesCandidatoIds = curriculumPostulante.HabilidadCurriculums?
                                                    .Select(hc => hc.id_habilidad).ToList() ?? new List<int>();

                    System.Diagnostics.Debug.WriteLine($"Habilidades del candidato {idPostulante.Value}: {string.Join(", ", habilidadesCandidatoIds)}");

                    bool cumpleHabilidades = habilidadesRequeridasIds.All(reqId => habilidadesCandidatoIds.Contains(reqId));
                    if (!cumpleHabilidades)
                    {
                        var nombresHabilidadesFaltantes = await _EmpleoContext.Habilidad
                            .Where(h => habilidadesRequeridasIds.Except(habilidadesCandidatoIds).Contains(h.id_habilidad))
                            .Select(h => h.nombre).ToListAsync();

                        string mensajeErrorHabilidades = "No cumples con todas las habilidades requeridas para esta oferta. ";
                        if (nombresHabilidadesFaltantes.Any())
                        {
                            mensajeErrorHabilidades += $"Te faltan: {string.Join(", ", nombresHabilidadesFaltantes)}. ";
                        }
                        mensajeErrorHabilidades += "Por favor, actualiza tus habilidades en tu perfil.";
                        System.Diagnostics.Debug.WriteLine($"Error: No cumple habilidades. Faltantes: {string.Join(", ", nombresHabilidadesFaltantes)}");
                        return Json(new { success = false, message = mensajeErrorHabilidades, redirectTo = Url.Action("EditarPerfil", "Perfil") });
                    }
                }

                var candidato = new OfertaCandidatos
                {
                    id_usuario = idUsuario.Value,
                    id_ofertaempleo = id_ofertaempleo,
                    estado = "En Espera",
                    visto = false
                };

                _EmpleoContext.OfertaCandidatos.Add(candidato);
                await _EmpleoContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Postulación registrada con éxito.");
                return Json(new { success = true, message = "¡Postulación exitosa! Tu aplicación ha sido enviada." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en PostularseOferta: {ex.ToString()}");
                // En producción, loguear ex.ToString() a un sistema de logging.
                return Json(new { success = false, message = "Ocurrió un error inesperado al procesar tu postulación. Por favor, inténtalo de nuevo más tarde." });
            }
        }


        [HttpGet]
        public IActionResult VerOfertaPostulado()
        {
            if (HttpContext.Session.GetString("tipo_usuario") != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                int id_usuario = HttpContext.Session.GetInt32("id_usuario") ?? 0;
                var ofertas = (from oc in _EmpleoContext.OfertaCandidatos
                               join o in _EmpleoContext.OfertaEmpleo on oc.id_ofertaempleo equals o.id_ofertaempleo
                               join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais into paisJoin
                               from p in paisJoin.DefaultIfEmpty()
                               join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia into provJoin
                               from pro in provJoin.DefaultIfEmpty()
                               join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa into empJoin
                               from e in empJoin.DefaultIfEmpty()
                               where oc.id_usuario == id_usuario
                               select new
                               {
                                   Id = o.id_ofertaempleo,
                                   Titulo = o.titulo,
                                   Vacantes = o.vacante,
                                   Salario = o.salario, // double
                                   Duracion_Contrato = o.duracion_contrato,
                                   Fecha_Publicacion = o.fecha_publicacion,
                                   Nombre_Empresa = e != null ? e.nombre : "Desconocida",
                                   Ubi_Pais = p != null ? p.nombre : "Desconocido",
                                   Ubi_Pro = pro != null ? pro.nombre : "Desconocido",
                                   Horario = o.horario,
                                   Estado = oc.estado
                               }).ToList();

                ViewBag.ofertas = ofertas;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las ofertas postuladas: " + ex.Message;
                ViewBag.ofertas = new List<object>();
                return View();
            }
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (HttpContext.Session.GetString("tipo_usuario") != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                int idUsuario = HttpContext.Session.GetInt32("id_usuario") ?? 0;
                var categoriasSuscritas = _EmpleoContext.SuscripcionCategoria
                    .Where(sc => sc.id_usuario == idUsuario)
                    .Select(sc => sc.id_categoriaprofesional)
                    .ToList();

                var ofertas = (from o in _EmpleoContext.OfertaEmpleo
                               join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais into paisJoin
                               from p in paisJoin.DefaultIfEmpty()
                               join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia into provJoin
                               from pro in provJoin.DefaultIfEmpty()
                               join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa into empJoin
                               from e in empJoin.DefaultIfEmpty()
                               join oc in _EmpleoContext.OfertaCategoria on o.id_ofertaempleo equals oc.id_ofertaempleo
                               where categoriasSuscritas.Contains(oc.id_categoriaprofesional) && o.estado == true
                               && (string.IsNullOrEmpty(query) || o.titulo.Contains(query) || (e != null && e.nombre.Contains(query)) || (p != null && p.nombre.Contains(query)) || (pro != null && pro.nombre.Contains(query)))
                               orderby o.fecha_publicacion descending
                               select new
                               {
                                   Id = o.id_ofertaempleo,
                                   Titulo = o.titulo,
                                   Vacantes = o.vacante,
                                   Salario = o.salario, // double
                                   Duracion_Contrato = o.duracion_contrato,
                                   Fecha_Publicacion = o.fecha_publicacion,
                                   Nombre_Empresa = e != null ? e.nombre : "Desconocida",
                                   Ubi_Pais = p != null ? p.nombre : "Desconocido",
                                   Ubi_Pro = pro != null ? pro.nombre : "Desconocido"
                               }).ToList();

                ViewBag.ofertas = ofertas;
                ViewBag.SearchQuery = query;
                return View("Listar");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al buscar las ofertas: " + ex.Message;
                ViewBag.ofertas = new List<object>();
                ViewBag.SearchQuery = query;
                return View("Listar");
            }
        }
    }
}