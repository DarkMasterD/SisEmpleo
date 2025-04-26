using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using SisEmpleo.Services;
using System.Diagnostics;
using System.Linq;

namespace SisEmpleo.Controllers
{
    [Autenticacion]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmpleoContext _EmpleoContext;

        public HomeController(ILogger<HomeController> logger, EmpleoContext empleoContext)
        {
            _logger = logger;
            _EmpleoContext = empleoContext;
        }

        public IActionResult Index()
        {
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");
            string tipoUsuario = HttpContext.Session.GetString("tipo_usuario");

            if (idUsuario == null || string.IsNullOrEmpty(tipoUsuario))
            {
                return RedirectToAction("Login", "Login");
            }

            var model = new Dashboard
            {
                IdUsuario = idUsuario.Value,
                TipoUsuario = tipoUsuario,
                Nombre = string.Empty,
                Categorias = _EmpleoContext.CategoriaProfesional.ToList(),
                Ofertas = new List<OfertaEmpleo>()
            };

            Console.WriteLine($"Home - TipoUsuario: {tipoUsuario}, IdUsuario: {idUsuario}");

            if (tipoUsuario == "E")
            {
                var empresa = _EmpleoContext.Empresa
                    .Where(e => e.id_usuario == idUsuario)
                    .Select(e => e.nombre)
                    .FirstOrDefault();
                model.Nombre = empresa ?? "Empresa";

                int idEmpresa = HttpContext.Session.GetInt32("id_empresa") ?? 0;

                model.Ofertas = (from oe in _EmpleoContext.OfertaEmpleo
                                 join p in _EmpleoContext.Pais on oe.id_pais equals p.id_pais
                                 join pro in _EmpleoContext.Provincia on oe.id_provincia equals pro.id_provincia
                                 where oe.id_empresa == idEmpresa && oe.estado == 'A'
                                 orderby oe.fecha_publicacion descending
                                 select new OfertaEmpleo
                                 {
                                     id_ofertaempleo = oe.id_ofertaempleo,
                                     id_pais = oe.id_pais,
                                     id_provincia = oe.id_provincia,
                                     id_empresa = oe.id_empresa,
                                     titulo = oe.titulo,
                                     vacantes = oe.vacantes,
                                     PaisNombre = p.nombre,
                                     ProvinciaNombre = pro.nombre
                                 }).Take(3).ToList();

                Console.WriteLine($"Ofertas Count (Empresa): {model.Ofertas.Count}");
            }
            else if (tipoUsuario == "P")
            {
                var postulante = _EmpleoContext.Postulante
                    .Where(p => p.id_usuario == idUsuario)
                    .Select(p => p.nombre)
                    .FirstOrDefault();
                model.Nombre = postulante ?? "Postulante";

                var categoriasSuscritas = _EmpleoContext.SuscripcionCategoria
                    .Where(sc => sc.id_usuario == idUsuario)
                    .Select(sc => sc.id_categoriaprofesional)
                    .ToList();
                Console.WriteLine($"Categorias Suscritas: {string.Join(", ", categoriasSuscritas)}");

                model.Ofertas = (from oe in _EmpleoContext.OfertaEmpleo
                                 join oc in _EmpleoContext.OfertaCategoria on oe.id_ofertaempleo equals oc.id_ofertaempleo
                                 join p in _EmpleoContext.Pais on oe.id_pais equals p.id_pais
                                 join pro in _EmpleoContext.Provincia on oe.id_provincia equals pro.id_provincia
                                 join e in _EmpleoContext.Empresa on oe.id_empresa equals e.id_empresa
                                 where categoriasSuscritas.Contains(oc.id_categoriaprofesional) && oe.estado == 'A'
                                 orderby oe.fecha_publicacion descending
                                 select new OfertaEmpleo
                                 {
                                     id_ofertaempleo = oe.id_ofertaempleo,
                                     id_pais = oe.id_pais,
                                     id_provincia = oe.id_provincia,
                                     id_empresa = oe.id_empresa,
                                     titulo = oe.titulo,
                                     vacantes = oe.vacantes,
                                     PaisNombre = p.nombre,
                                     ProvinciaNombre = pro.nombre,
                                     EmpresaNombre = e.nombre
                                 }).Take(3).ToList();

                Console.WriteLine($"Ofertas Count (Postulante): {model.Ofertas.Count}");
            }

            return View(model);
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
            var ofertas = (from sc in _EmpleoContext.SuscripcionCategoria
                           join oc in _EmpleoContext.OfertaCategoria
                           on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                           join o in _EmpleoContext.OfertaEmpleo
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

            var categoriasubs = (from sc in _EmpleoContext.SuscripcionCategoria
                                 join c in _EmpleoContext.CategoriaProfesional
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
            var ofertas = (from sc in _EmpleoContext.SuscripcionCategoria
                           join oc in _EmpleoContext.OfertaCategoria
                           on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                           join o in _EmpleoContext.OfertaEmpleo
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
            var subcategoriasubs = (from sc in _EmpleoContext.SuscripcionCategoria
                                    join scp in _EmpleoContext.SubcategoriaProfesional
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
            var cantOfertas = (from sc in _EmpleoContext.SuscripcionCategoria
                               join oc in _EmpleoContext.OfertaCategoria
                               on new { sc.id_categoriaprofesional, sc.id_subcategoriaprofesional } equals new { oc.id_categoriaprofesional, oc.id_subcategoriaprofesional }
                               join o in _EmpleoContext.OfertaEmpleo
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