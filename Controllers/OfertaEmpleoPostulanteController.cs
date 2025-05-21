using Microsoft.AspNetCore.Mvc;
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
                                  Salario = o.salario, // double
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

                var id_usuario = HttpContext.Session.GetInt32("id_usuario");
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
        public IActionResult PostularseOferta(int id_ofertaempleo)
        {
            if (HttpContext.Session.GetString("tipo_usuario") != "P")
            {
                return Json(new { success = false, message = "Debes iniciar sesión como postulante." });
            }

            int id_usuario = HttpContext.Session.GetInt32("id_usuario") ?? 0;
            int id_postulante = HttpContext.Session.GetInt32("id_postulante") ?? 0;

            try
            {
                if (_EmpleoContext.OfertaCandidatos.Any(oc => oc.id_ofertaempleo == id_ofertaempleo && oc.id_usuario == id_usuario))
                {
                    return Json(new { success = false, message = "Ya te has postulado a esta oferta." });
                }

                bool hasContact = _EmpleoContext.Contacto.Any(c => c.id_usuario == id_usuario);
                bool hasCurriculum = _EmpleoContext.Curriculum.Any(c => c.id_postulante == id_postulante);
                bool hasFormacion = _EmpleoContext.FormacionAcademica.Any(f => _EmpleoContext.Curriculum.Any(c => c.id_postulante == id_postulante && c.id_curriculum == f.id_curriculum));

                if (!hasContact || !hasCurriculum || !hasFormacion)
                {
                    return Json(new { success = false, message = "Debes completar tu perfil (información de contacto, currículum y formación académica) antes de postularte." });
                }

                var requiredSkills = _EmpleoContext.RequisitoOferta
                    .Where(r => r.id_ofertaempleo == id_ofertaempleo)
                    .Select(r => r.id_habilidad)
                    .ToList();

                var candidateSkills = (from hc in _EmpleoContext.Habilidad_Curriculum
                                       join c in _EmpleoContext.Curriculum on hc.id_curriculum equals c.id_curriculum
                                       where c.id_postulante == id_postulante
                                       select hc.id_habilidad).ToList();

                bool meetsRequirements = requiredSkills.All(rs => candidateSkills.Contains(rs));

                if (!meetsRequirements)
                {
                    return Json(new { success = false, message = "No cumples con los requisitos de la oferta." });
                }

                var candidato = new OfertaCandidatos
                {
                    id_usuario = id_usuario,
                    id_ofertaempleo = id_ofertaempleo,
                    estado = "En Espera",
                    visto = false
                };

                _EmpleoContext.OfertaCandidatos.Add(candidato);
                _EmpleoContext.SaveChanges();

                return Json(new { success = true, message = "Postulación registrada con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al procesar la postulación: " + ex.Message });
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