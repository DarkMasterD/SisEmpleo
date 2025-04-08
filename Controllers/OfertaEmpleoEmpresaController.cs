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
            oferta.estado = 'A';
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
                               Vacantes = o.vacantes,
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
            ofertaAct.vacantes = ofertaMod.vacantes;
            ofertaAct.salario = ofertaMod.salario;
            ofertaAct.horario = ofertaMod.horario;
            ofertaAct.duracion_contrato = ofertaMod.duracion_contrato;
            ofertaAct.estado = ofertaMod.estado;

            _EmpleoContext.Entry(ofertaAct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _EmpleoContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}
