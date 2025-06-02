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

        // Reporte 5 - Suscripciones a categorías (versión corregida)
        public IActionResult ReporteSuscripcionesCategorias()
        {
            // Obtener datos necesarios
            var suscripciones = _context.SuscripcionCategoria.ToList();
            var categorias = _context.CategoriaProfesional.ToList();
            var postulantes = _context.Usuario.Count(u => u.tipo_usuario == 'P');

            // Agrupar suscripciones por categoría principal
            var reporteData = categorias.Select(c => new
            {
                CategoriaId = c.id_categoriaprofesional,
                NombreCategoria = c.nombre,
                CantidadSuscriptores = suscripciones.Count(sc =>
                    sc.id_categoriaprofesional == c.id_categoriaprofesional &&
                    sc.estado == true) // Solo suscripciones activas
            })
            .OrderByDescending(x => x.CantidadSuscriptores)
            .ToList();

            ViewBag.TituloReporte = "Suscripciones a Categorías Profesionales";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalSuscripciones = suscripciones.Count(sc => sc.estado == true);
            ViewBag.TotalPostulantes = postulantes;

            return View(reporteData);
        }

        // Reporte 6 - Empresas registradas
        public IActionResult ReporteEmpresasRegistradas()
        {
            // Obtener datos necesarios sin usar relaciones de navegación
            var empresas = _context.Empresa.ToList();
            var usuarios = _context.Usuario.ToList();
            var contactos = _context.Contacto.ToList();

            // Combinar los datos
            var reporteData = empresas.Select(e => new
            {
                EmpresaId = e.id_empresa,
                NombreEmpresa = e.nombre,
                Email = usuarios.FirstOrDefault(u => u.id_usuario == e.id_usuario)?.email ?? "No disponible",
                Telefono = contactos.FirstOrDefault(c => c.id_usuario == e.id_usuario)?.telefono ?? "No disponible",
                Direccion = e.direccion,
                Sector = e.sector_empresa
            })
            .OrderBy(e => e.NombreEmpresa)
            .ToList();

            ViewBag.TituloReporte = "Empresas Registradas en el Sistema";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalEmpresas = empresas.Count;

            return View(reporteData);
        }

        // Reporte 7 - Candidatos por formación académica
        // Reporte 7 - Candidatos por formación académica (sin relaciones)
        public IActionResult ReporteCandidatosPorFormacion()
        {
            // Obtener todas las tablas necesarias
            var titulos = _context.Titulo.ToList();
            var especialidades = _context.Especialidad.ToList();
            var formaciones = _context.FormacionAcademica.ToList();
            var curriculums = _context.Curriculum.ToList();
            var postulantes = _context.Postulante.ToList();

            // Crear el reporte combinando los datos
            var reporteData = titulos.Select(t => new
            {
                TituloId = t.id_titulo,
                NombreTitulo = t.nombre,
                TipoTitulo = t.tipo,
                Especialidad = especialidades.FirstOrDefault(e => e.id_especialidad == t.id_especialidad)?.nombre ?? "Sin especificar",
                CantidadPostulantes = formaciones.Count(f =>
                    f.id_titulo == t.id_titulo &&
                    curriculums.Any(c => c.id_curriculum == f.id_curriculum) &&
                    postulantes.Any(p => p.id_postulante == curriculums.First(c => c.id_curriculum == f.id_curriculum).id_postulante))
            })
            .OrderByDescending(x => x.CantidadPostulantes)
            .ToList();

            ViewBag.TituloReporte = "Candidatos por Formación Académica";
            ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewBag.TotalPostulantes = postulantes.Count;

            return View(reporteData);
        }

        // Reporte 8 - Postulantes por formación académica (estilo consistente)
        public IActionResult ReportePostulantesPorFormacion()
        {
            try
            {
                // 1. Obtener datos básicos
                var postulantes = _context.Postulante.ToList();
                var usuarios = _context.Usuario.ToList();
                var contactos = _context.Contacto.ToList();

                // 2. Obtener datos de formación académica
                var formacionData = (
                    from p in postulantes
                    join c in _context.Curriculum on p.id_postulante equals c.id_postulante
                    join fa in _context.FormacionAcademica on c.id_curriculum equals fa.id_curriculum
                    join t in _context.Titulo on fa.id_titulo equals t.id_titulo
                    join e in _context.Especialidad on t.id_especialidad equals e.id_especialidad
                    join i in _context.Institucion on fa.id_institucion equals i.id_institucion
                    select new
                    {
                        p.id_postulante,
                        p.nombre,
                        p.apellido,
                        p.id_usuario,
                        fa.id_titulo,
                        Titulo = t.nombre, // Cambiado de TituloNombre a Titulo
                        TipoTitulo = t.tipo,
                        Especialidad = e.nombre,
                        Institucion = i.nombre
                    }).ToList();

                // 3. Combinar datos para el reporte
                var reporteData = postulantes.Select(p => new
                {
                    PostulanteId = p.id_postulante,
                    NombreCompleto = $"{p.nombre} {p.apellido}",
                    Email = usuarios.FirstOrDefault(u => u.id_usuario == p.id_usuario)?.email ?? "No disponible",
                    Telefono = contactos.FirstOrDefault(c => c.id_usuario == p.id_usuario)?.telefono ?? "No disponible",
                    Formaciones = formacionData
                        .Where(f => f.id_postulante == p.id_postulante)
                        .Select(f => new
                        {
                            Titulo = f.Titulo, // Cambiado para coincidir con la vista
                            f.TipoTitulo,
                            f.Especialidad,
                            f.Institucion
                        })
                        .Distinct()
                        .ToList()
                })
                .OrderBy(p => p.NombreCompleto)
                .ToList();

                // 4. Preparar estadísticas para ViewBag (ajustado para coincidir con la vista)
                var tiposTitulo = reporteData
                    .SelectMany(p => p.Formaciones)
                    .GroupBy(f => f.TipoTitulo)
                    .Select(g => new
                    {
                        Tipo = g.Key,
                        Cantidad = g.Count() // Cambiado para contar ocurrencias como hace la vista
                    })
                    .OrderByDescending(t => t.Cantidad)
                    .ToList();

                ViewBag.TituloReporte = "Postulantes según Formación Académica";
                ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                ViewBag.TotalPostulantes = postulantes.Count;
                ViewBag.PostulantesConFormacion = reporteData.Count(p => p.Formaciones.Count > 0);
                ViewBag.TiposTitulo = tiposTitulo;

                return View(reporteData);
            }
            catch (Exception ex)
            {
                // Log del error (puedes implementar un sistema de logging aquí)
                Console.WriteLine($"Error en ReportePostulantesPorFormacion: {ex.Message}");

                // Retornar vista con datos vacíos para evitar interrumpir la aplicación
                ViewBag.TituloReporte = "Postulantes según Formación Académica";
                ViewBag.FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                ViewBag.TotalPostulantes = 0;
                ViewBag.PostulantesConFormacion = 0;
                ViewBag.TiposTitulo = new List<dynamic>();

                return View(new List<dynamic>());
            }
        }
    }
}
