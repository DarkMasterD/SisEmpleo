using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using SisEmpleo.Services;

namespace SisEmpleo.Controllers
{
    [AutenticacionEmpresa]
    public class OfertaEmpleoEmpresaController : Controller
    {
        private readonly EmpleoContext _EmpleoContext;

        public OfertaEmpleoEmpresaController(EmpleoContext context)
        {
            _EmpleoContext = context;
        }

        [HttpGet]
        public IActionResult NuevaOferta()
        {
            List<Pais> paises = (from p in _EmpleoContext.Pais
                                 select p).ToList();

            ViewBag.Pais = new SelectList(paises, "id_pais", "nombre");

            return View();
        }

        [HttpGet]
        public JsonResult ObtenerProvincia(int id_pais)
        {
            List<Provincia> provincias = (from p in _EmpleoContext.Provincia
                                          where p.id_pais == id_pais
                                          select p).ToList();
            return Json(provincias);
        }

        [HttpPost]
        public IActionResult NuevaOferta([FromForm] OfertaEmpleo oferta)
        {
            oferta.id_empresa = Convert.ToInt32(HttpContext.Session.GetInt32("id_empresa"));
            oferta.estado.Equals(true);
            oferta.fecha_publicacion = DateTime.Now;

            _EmpleoContext.OfertaEmpleo.Add(oferta);
            _EmpleoContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ListarOfertas()
        {
            int id_empresa = Convert.ToInt32(HttpContext.Session.GetInt32("id_empresa"));
            var ofertas = (from o in _EmpleoContext.OfertaEmpleo
                           join p in _EmpleoContext.Pais
                           on o.id_pais equals p.id_pais
                           join pro in _EmpleoContext.Provincia
                           on o.id_provincia equals pro.id_provincia
                           join e in _EmpleoContext.Empresa
                           on o.id_empresa equals e.id_empresa
                           where o.id_empresa == id_empresa
                           select new
                           {
                               Id = o.id_ofertaempleo,
                               Titulo = o.titulo,
                               Vacantes = o.vacante,
                               Salario = o.salario,
                               Duracion_Contrato = o.duracion_contrato,
                               Fecha_Publicacion = o.fecha_publicacion,
                               Ubi_Pais = p.nombre,
                               Ubi_Pro = pro.nombre
                           }).ToList();
            ViewData["listOfertas"] = ofertas;
            return View();
        }

        [HttpGet]
        public IActionResult EditarOferta(int id_ofertaempleo)
        {
            List<Pais> paises = (from p in _EmpleoContext.Pais
                                 select p).ToList();

            OfertaEmpleo? oferta = (from o in _EmpleoContext.OfertaEmpleo
                                    where o.id_ofertaempleo == id_ofertaempleo
                                    select o).FirstOrDefault();

            string[] lista = oferta.horario.Split(";");

            var opcEstado = new List<SelectListItem>
            {
                new SelectListItem{ Text = "Abierto", Value = "A" },
                new SelectListItem{ Text = "Cerrado", Value = "C" }
            };

            ViewBag.OpcEstado = opcEstado;
            ViewBag.Oferta = oferta;
            ViewBag.Horario = lista;
            ViewBag.Pais = new SelectList(paises, "id_pais", "nombre");
            return View();
        }

        [HttpPost]
        public IActionResult EditarOferta([FromForm] OfertaEmpleo ofertaMod)
        {
            OfertaEmpleo? ofertaAct = (from o in _EmpleoContext.OfertaEmpleo
                                       where o.id_ofertaempleo == ofertaMod.id_ofertaempleo
                                       select o).FirstOrDefault();

            ofertaAct.id_pais = ofertaMod.id_pais;
            ofertaAct.id_provincia = ofertaMod.id_provincia;
            ofertaAct.titulo = ofertaMod.titulo;
            ofertaAct.descripcion = ofertaMod.descripcion;
            ofertaAct.vacante = ofertaMod.vacante;
            ofertaAct.salario = ofertaMod.salario;
            ofertaAct.horario = ofertaMod.horario;
            ofertaAct.duracion_contrato = ofertaMod.duracion_contrato;
            ofertaAct.estado = ofertaMod.estado;

            _EmpleoContext.Entry(ofertaAct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _EmpleoContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult VerPostulantesEmpresa(int id_ofertaempleo, string nivelExperiencia = null)
        {
            try
            {
                // Consulta principal para obtener postulantes
                var query = (from oc in _EmpleoContext.OfertaCandidatos
                             join u in _EmpleoContext.Usuario on oc.id_usuario equals u.id_usuario
                             join p in _EmpleoContext.Postulante on u.id_usuario equals p.id_usuario
                             join c in _EmpleoContext.Contacto on u.id_usuario equals c.id_usuario
                             join cur in _EmpleoContext.Curriculum on p.id_postulante equals cur.id_postulante
                             join exp in _EmpleoContext.ExperienciaProfesional on cur.id_curriculum equals exp.id_curriculum into experiencias
                             from exp in experiencias.DefaultIfEmpty()
                             where oc.id_ofertaempleo == id_ofertaempleo
                             select new
                             {
                                 IdPostulante = p.id_postulante,
                                 Nombre = p.nombre,
                                 Email = c.email,
                                 Telefono = c.telefono,
                                 IdUsuario = u.id_usuario,
                                 FechaInicioExperiencia = exp != null ? exp.fecha_inicio : (DateTime?)null,
                                 FechaFinExperiencia = exp != null ? exp.fecha_fin : (DateTime?)null
                             }).AsEnumerable();

                // Procesamiento de postulantes y cálculo de experiencia
                var postulantes = query
                    .GroupBy(p => new { p.IdPostulante, p.Nombre, p.Email, p.Telefono, p.IdUsuario })
                    .Select(g =>
                    {
                        var totalAnios = g.Sum(p => CalcularAniosExperiencia(p.FechaInicioExperiencia, p.FechaFinExperiencia));
                        return new
                        {
                            g.Key.IdPostulante,
                            g.Key.Nombre,
                            g.Key.Email,
                            g.Key.Telefono,
                            g.Key.IdUsuario,
                            Experiencia = ClasificarExperiencia(totalAnios)
                        };
                    })
                    .ToList();

                // Filtrado por nivel de experiencia si se especificó
                if (!string.IsNullOrEmpty(nivelExperiencia))
                {
                    postulantes = postulantes.Where(p => p.Experiencia == nivelExperiencia).ToList();
                }

                // SOLUCIÓN DEFINITIVA PARA OBTENER EL TÍTULO
                string ofertaTitulo = _EmpleoContext.OfertaEmpleo
                    .Where(o => o.id_ofertaempleo == id_ofertaempleo)
                    .Select(o => o.titulo)
                    .FirstOrDefault() ?? "Sin título";

                // Configuración de ViewBag y ViewData
                ViewBag.OfertaTitulo = ofertaTitulo;
                ViewBag.IdOferta = id_ofertaempleo;
                ViewBag.NivelesExperiencia = new List<string> { "Sin experiencia", "1-3 años", "3-5 años", "Más de 5 años" };
                ViewBag.NivelSeleccionado = nivelExperiencia;
                ViewData["Postulantes"] = postulantes;

                // Obtener los requisitos asociados a la oferta
                var requisitos = (from ro in _EmpleoContext.RequisitoOferta
                                  join r in _EmpleoContext.Requisito on ro.id_requisito equals r.id_requisito
                                  where ro.id_ofertaempleo == id_ofertaempleo
                                  select new
                                  {
                                      Id = r.id_requisito,
                                      Descripcion = r.descripcion
                                  }).ToList();

                // Pasarlo a la vista
                ViewBag.Requisitos = requisitos;

                return View();
            }
            catch (Exception ex)
            {

                return View();
            }
        }


        // Método para calcular años de experiencia entre dos fechas
        private double CalcularAniosExperiencia(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue) return 0;
            DateTime fin = fechaFin ?? DateTime.Now;
            return (fin - fechaInicio.Value).TotalDays / 365.25;
        }

        // Método para clasificar el nivel de experiencia
        private string ClasificarExperiencia(double anios)
        {
            if (anios < 1)
                return "Sin experiencia";
            else if (anios >= 1 && anios <= 3)
                return "1-3 años";
            else if (anios > 3 && anios <= 5)
                return "3-5 años";
            else
                return "Más de 5 años";
        }



    }
}