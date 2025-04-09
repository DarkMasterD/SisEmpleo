using Microsoft.AspNetCore.Mvc;
using SisEmpleo.Models;
using SisEmpleo.Services;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

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

            var model = new HomeViewModel
            {
                IdUsuario = idUsuario.Value,
                TipoUsuario = tipoUsuario
            };

            if (tipoUsuario == "E")
            {
                var empresa = (from e in _EmpleoContext.Empresa
                               where e.id_usuario == idUsuario
                               select e.nombre).FirstOrDefault();
                model.Nombre = empresa ?? "Empresa";

                int idEmpresa = HttpContext.Session.GetInt32("id_empresa") ?? 0;

                model.Ofertas = (from oe in _EmpleoContext.OfertaEmpleo
                                 join p in _EmpleoContext.Pais on oe.id_pais equals p.id_pais
                                 join pr in _EmpleoContext.Provincia on oe.id_provincia equals pr.id_provincia
                                 where oe.id_empresa == idEmpresa
                                 orderby oe.fecha_publicacion descending
                                 select new OfertaEmpleoViewModel
                                 {
                                     IdOfertaEmpleo = oe.id_ofertaempleo,
                                     Titulo = oe.titulo,
                                     Descripcion = oe.descripcion,
                                     Pais = p.nombre,
                                     Provincia = pr.nombre,
                                     Vacantes = oe.vacantes
                                 }).Take(3).ToList();
            }
            else if (tipoUsuario == "P")
            {
                var postulante = (from p in _EmpleoContext.Postulante
                                  where p.id_usuario == idUsuario
                                  select p.nombre).FirstOrDefault();
                model.Nombre = postulante ?? "Postulante";


                var categoriasSuscritas = (from sc in _EmpleoContext.SuscripcionCategoria
                                           where sc.id_usuario == idUsuario
                                           select sc.id_categoriaprofesional).ToList();

                model.Ofertas = (from oe in _EmpleoContext.OfertaEmpleo
                                 join oc in _EmpleoContext.OfertaCategoria on oe.id_ofertaempleo equals oc.id_ofertaempleo
                                 join p in _EmpleoContext.Pais on oe.id_pais equals p.id_pais
                                 join pr in _EmpleoContext.Provincia on oe.id_provincia equals pr.id_provincia
                                 join e in _EmpleoContext.Empresa on oe.id_empresa equals e.id_empresa
                                 where categoriasSuscritas.Contains(oc.id_categoriaprofesional)
                                 orderby oe.fecha_publicacion descending
                                 select new OfertaEmpleoViewModel
                                 {
                                     IdOfertaEmpleo = oe.id_ofertaempleo,
                                     Titulo = oe.titulo,
                                     Descripcion = oe.descripcion,
                                     Pais = p.nombre,
                                     Provincia = pr.nombre,
                                     Empresa = e.nombre,
                                     Vacantes = oe.vacantes
                                 }).Take(3).ToList();
            }

            model.Categorias = (from cp in _EmpleoContext.CategoriaProfesional
                                select new CategoriaViewModel
                                {
                                    IdCategoriaProfesional = cp.id_categoriaprofesional,
                                    Nombre = cp.nombre
                                }).ToList();

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

    public class HomeViewModel
    {
        public int IdUsuario { get; set; }
        public string TipoUsuario { get; set; }
        public string Nombre { get; set; }
        public List<CategoriaViewModel> Categorias { get; set; }
        public List<OfertaEmpleoViewModel> Ofertas { get; set; }
    }

    public class CategoriaViewModel
    {
        public int IdCategoriaProfesional { get; set; }
        public string Nombre { get; set; }
    }

    public class OfertaEmpleoViewModel
    {
        public int IdOfertaEmpleo { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string Empresa { get; set; }
        public int Vacantes { get; set; }
    }
}