using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using SisEmpleo.Services;
using System.Diagnostics;

namespace SisEmpleo.Controllers
{
    [Autenticacion]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmpleoContext _empleoContext;

        public HomeController(ILogger<HomeController> logger, EmpleoContext context)
        {
            _logger = logger;
            _empleoContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Notificaciones()
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));
            var ofertas = (from sc in _empleoContext.SuscripcionCategoria
                           join oc in _empleoContext.OfertaCategoria
                           on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                           join o in _empleoContext.OfertaEmpleo
                           on oc.id_ofertaempleo equals o.id_ofertaempleo
                           where o.estado == 'A' && o.fecha_publicacion >= DateTime.Now.AddDays(-7)
                           && sc.id_usuario == id_usuario
                           select new
                           {
                               IdCateProf = sc.id_categoriaprofesional,
                               IdSubCateProf = sc.id_subcategoriaprofesional,
                               Titulo = o.titulo,
                               Fecha = o.fecha_publicacion,
                               Id = o.id_ofertaempleo
                           }).GroupBy(x => new { x.IdCateProf, x.IdSubCateProf, x.Titulo, x.Fecha, x.Id })
               .Select(g => g.Key)
               .ToList();

            var categoriasubs = (from sc in _empleoContext.SuscripcionCategoria
                                 join c in _empleoContext.CategoriaProfesional
                                 on sc.id_categoriaprofesional equals c.id_categoriaprofesional
                                 where sc.id_usuario == id_usuario
                                 select new
                                 {
                                     Id = sc.id_categoriaprofesional,
                                     Categoria = c.nombre
                                 }).Distinct().ToList();

            ViewBag.Categorias = new SelectList(categoriasubs, "Id", "Categoria");
            ViewBag.Ofertas = ofertas;
            return View();
        }

        [HttpGet]
        public IActionResult NotificacionesFiltro(int id_categoria, int id_subcategoria)
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));
            var ofertas = (from sc in _empleoContext.SuscripcionCategoria
                           join oc in _empleoContext.OfertaCategoria
                           on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                           join o in _empleoContext.OfertaEmpleo
                           on oc.id_ofertaempleo equals o.id_ofertaempleo
                           where o.estado == 'A' && o.fecha_publicacion >= DateTime.Now.AddDays(-7)
                           && sc.id_usuario == id_usuario && sc.id_categoriaprofesional == id_categoria
                           && sc.id_subcategoriaprofesional == id_subcategoria
                           select new
                           {
                               IdCateProf = sc.id_categoriaprofesional,
                               IdSubCateProf = sc.id_subcategoriaprofesional,
                               Titulo = o.titulo,
                               Fecha = o.fecha_publicacion,
                               Id = o.id_ofertaempleo
                           }).GroupBy(x => new { x.IdCateProf, x.IdSubCateProf, x.Titulo, x.Fecha, x.Id })
               .Select(g => g.Key)
               .ToList();

            ViewBag.Ofertas = ofertas;
            return PartialView("~/Views/Home/_NotificacionParcial.cshtml");
        }

        [HttpGet]
        public JsonResult ObtenerSubCategoriaSubs(int id_categoria)
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));
            var subcategoriasubs = (from sc in _empleoContext.SuscripcionCategoria
                                    join scp in _empleoContext.SubcategoriaProfesional
                                    on sc.id_subcategoriaprofesional equals scp.id_subcategoriaprofesional
                                    where sc.id_usuario == id_usuario &&
                                    scp.id_categoriaprofesional == id_categoria
                                    select new
                                    {
                                        Id = scp.id_subcategoriaprofesional,
                                        SubCategoria = scp.nombre
                                    }).Distinct().ToList();

            return Json(subcategoriasubs);
        }

        [HttpPost]
        public JsonResult CantNotificaciones()
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));
            var cantOfertas = (from sc in _empleoContext.SuscripcionCategoria
                               join oc in _empleoContext.OfertaCategoria
                               on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                               join o in _empleoContext.OfertaEmpleo
                               on oc.id_ofertaempleo equals o.id_ofertaempleo
                               where o.estado == 'A' && o.fecha_publicacion >= DateTime.Now.AddDays(-7)
                               && sc.id_usuario == id_usuario
                               select new
                               {
                                   IdCateProf = sc.id_categoriaprofesional,
                                   IdSubCateProf = sc.id_subcategoriaprofesional,
                                   Titulo = o.titulo,
                                   Fecha = o.fecha_publicacion,
                                   Id = o.id_ofertaempleo
                               }).GroupBy(x => new { x.IdCateProf, x.IdSubCateProf, x.Titulo, x.Fecha, x.Id })
               .Select(g => g.Key)
               .Count();

            return Json(new { cantidad = cantOfertas });
        }
    }
}
