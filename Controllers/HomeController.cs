using Microsoft.AspNetCore.Mvc;
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
    }
}