using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SisEmpleo.Models;
using System.Drawing;

namespace SisEmpleo.Controllers
{
    public class ReportesController : Controller
    {
        private readonly EmpleoContext _context;
        public ReportesController(EmpleoContext context)
        {
            _context = context;
        }

        //Reporte 1
        public IActionResult ReporteCandidatosPorOferta()
        {
            // Consulta directa sin usar relaciones de navegación
            var ofertas = _context.OfertaEmpleo.ToList();
            var empresas = _context.Empresa.ToList();
            var candidatos = _context.OfertaCandidatos.ToList();

            var reporteData = ofertas.Select(o => new
            {
                OfertaId = o.id_ofertaempleo,
                TituloOferta = o.titulo,
                Empresa = empresas.FirstOrDefault(e => e.id_empresa == o.id_empresa)?.nombre ?? "Desconocida",
                FechaPublicacion = o.fecha_publicacion,
                Vacantes = o.vacante,
                CandidatosInscritos = candidatos.Count(oc => oc.id_ofertaempleo == o.id_ofertaempleo),
                PorcentajeOcupacion = (o.vacante > 0) ?
                Math.Round((decimal)candidatos.Count(oc => oc.id_ofertaempleo == o.id_ofertaempleo) / o.vacante * 100, 2) : 0
            })
            .OrderByDescending(x => x.CandidatosInscritos)
            .ToList();

            ViewBag.TituloReporte = "Reporte de Candidatos Inscritos por Oferta";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            return View(reporteData);
        }

        //Reporte 2
        public IActionResult ReporteOfertasPorCategoria()
        {
            // Obtener solo las ofertas activas
            var ofertasActivas = _context.OfertaEmpleo
                .Where(o => o.estado == true)
                .Select(o => o.id_ofertaempleo)
                .ToList();

            // Obtener las categorías profesionales
            var categorias = _context.CategoriaProfesional.ToList();

            // Obtener la tabla intermedia que relaciona ofertas con categorías
            var ofertaCategorias = _context.OfertaCategoria.ToList();

            // Agrupar ofertas activas por categoría
            var reporteData = categorias.Select(c => new
            {
                CategoriaId = c.id_categoriaprofesional,
                NombreCategoria = c.nombre,
                CantidadOfertas = ofertaCategorias.Count(oc =>
                    oc.id_categoriaprofesional == c.id_categoriaprofesional &&
                    ofertasActivas.Contains(oc.id_ofertaempleo))
            })
            .OrderByDescending(x => x.CantidadOfertas)
            .ToList();

            ViewBag.TituloReporte = "Reporte de Ofertas Activas por Categoría";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalOfertasActivas = ofertasActivas.Count;

            return View(reporteData);
        }

        // Reporte 3 - Versión corregida
        public IActionResult ReporteEmpresasTopOfertas()
        {
            // Obtener datos necesarios
            var ofertas = _context.OfertaEmpleo.ToList();
            var empresas = _context.Empresa.ToList();
            var totalEmpresas = empresas.Count; // Calculamos aquí el total

            // Agrupar ofertas por empresa
            var reporteData = empresas.Select(e => new
            {
                EmpresaId = e.id_empresa,
                NombreEmpresa = e.nombre,
                CantidadOfertas = ofertas.Count(o => o.id_empresa == e.id_empresa)
            })
            .OrderByDescending(x => x.CantidadOfertas)
            .Take(20)
            .ToList();

            ViewBag.TituloReporte = "Empresas con Mayor Cantidad de Ofertas Publicadas";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalOfertas = ofertas.Count;
            ViewBag.TotalEmpresas = totalEmpresas; // Pasamos el total a la vista

            return View(reporteData);
        }

        //Reporte 4 - Requisitos más frecuentes en ofertas de empleo
        public IActionResult ReporteRequisitosFrecuentes()
        {
            var requisitosFrecuentes = (from ro in _context.RequisitoOferta
                                        join o in _context.OfertaEmpleo on ro.id_ofertaempleo equals o.id_ofertaempleo
                                        join h in _context.Habilidad on ro.id_habilidad equals h.id_habilidad
                                        where o.estado == true
                                        group h by h.nombre into g
                                        select new
                                        {
                                            Requisito = g.Key,
                                            CantidadOfertas = g.Count()
                                        })
                                        .OrderByDescending(x => x.CantidadOfertas)
                                        .ToList();

            int totalOfertasActivas = _context.OfertaEmpleo.Count(o => o.estado == true);

            ViewBag.TituloReporte = "Requisitos Más Frecuentes en Ofertas de Empleo";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalOfertasActivas = totalOfertasActivas;

            return View(requisitosFrecuentes);
        }
    }
}
