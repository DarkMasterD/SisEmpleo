using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using SisEmpleo.Models.Graficos;

namespace SisEmpleo.Controllers
{
    public class GraficosController : Controller
    {
        private readonly EmpleoContext _context;
        private readonly ILogger<GraficosController> _logger; 
        public GraficosController(EmpleoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Grap 1
        public async Task<IActionResult> VacantesPorEmpresa()
        {
            try
            {
                // Consulta para obtener el total de vacantes por empresa
                var datos = await (from e in _context.Empresa
                                   join oe in _context.OfertaEmpleo on e.id_empresa equals oe.id_empresa into ofertas
                                   from oferta in ofertas.DefaultIfEmpty()
                                   group oferta by e.nombre into g
                                   select new VacantesPorEmpresaViewModel
                                   {
                                       Empresa = g.Key,
                                       TotalVacantes = g.Sum(x => x != null ? x.vacante : 0),
                                       TipoUsuario = "E"
                                   }).ToListAsync();

                // Depuración
                System.Diagnostics.Debug.WriteLine($"Datos obtenidos (Vacantes por Empresa): {Newtonsoft.Json.JsonConvert.SerializeObject(datos)}");

                if (!datos.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No se encontraron datos para el gráfico de vacantes por empresa.");
                }

                return View("~/Views/Gráficos/VacantesPorEmpresa.cshtml", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Grap 2 - Número de Inscripciones por Oferta
        public async Task<IActionResult> InscripcionesPorOferta()
        {
            try
            {
                var datos = await (from oferta in _context.OfertaEmpleo
                                   join inscripcion in _context.OfertaCandidatos
                                   on oferta.id_ofertaempleo equals inscripcion.id_ofertaempleo into inscripciones
                                   select new InscripcionesPorOfertaViewModel
                                   {
                                       Oferta = oferta.titulo,
                                       TotalInscripciones = inscripciones.Count()
                                   }).ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Datos obtenidos (Inscripciones por Oferta): {Newtonsoft.Json.JsonConvert.SerializeObject(datos)}");

                if (!datos.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No se encontraron inscripciones para las ofertas.");
                }

                return View("~/Views/Gráficos/InscripcionesPorOferta.cshtml", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Grap 3
        public async Task<IActionResult> DistribucionCandidatosPorFormacionAcademica()
        {
            try
            {
                // Realizamos la consulta para obtener la distribución de candidatos por formación académica
                var datos = await (from fa in _context.FormacionAcademica
                                   join t in _context.Titulo on fa.id_titulo equals t.id_titulo
                                   group fa by t.tipo into g
                                   select new
                                   {
                                       NivelFormacion = g.Key,
                                       TotalCandidatos = g.Count()
                                   }).ToListAsync();

                // Log de los datos obtenidos
                System.Diagnostics.Debug.WriteLine($"Datos obtenidos (Distribución de Candidatos por Formación Académica): {Newtonsoft.Json.JsonConvert.SerializeObject(datos)}");

                // Retornamos los datos a la vista
                return View("~/Views/Gráficos/DistribucionCandidatosPorFormacionAcademica.cshtml", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Grap 4
        public async Task<IActionResult> PorcentajeCandidatosPorCategoria()
        {
            try
            {
                // Consulta en una línea para obtener los datos
                var datos = await _context.CategoriaProfesional
                    .GroupJoin(_context.SuscripcionCategoria, cp => cp.id_categoriaprofesional, sc => sc.id_categoriaprofesional, (cp, subs) => new SuscripcionCategoriaViewModel
                    {
                        Categoria = cp.nombre,
                        CantidadCandidatos = subs.Count(),
                        Porcentaje = _context.SuscripcionCategoria.Count() > 0 ? (subs.Count() * 100.0 / _context.SuscripcionCategoria.Count()) : 0
                    }).ToListAsync();

                // Depuración
                System.Diagnostics.Debug.WriteLine($"Datos obtenidos: {Newtonsoft.Json.JsonConvert.SerializeObject(datos)}");

                return View("~/Views/Gráficos/PorcentajeCandidatosPorCategoria.cshtml", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Metodo para devolver la vista, ya que utilizan js en la vista para obtener los datos
        public IActionResult Grafico5()
        {
            return View("~/Views/Gráficos/Grafico5.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> GetSalarioVacantesData()
        {
            try
            {
                if (!_context.Database.CanConnect())
                {
                    return Json(new { error = "No se pudo conectar a la base de datos" });
                }

                var query =
                    from o in _context.OfertaEmpleo
                    join e in _context.Empresa on o.id_empresa equals e.id_empresa
                    where o.salario > 0 && o.vacante > 0
                    select new
                    {
                        Salario = o.salario,
                        Vacantes = o.vacante,
                        Titulo = o.titulo,
                        Empresa = e.nombre
                    };

                var datos = await query.ToListAsync();

                if (!datos.Any())
                {
                    return Json(new { warning = "Consulta exitosa pero no hay registros que cumplan los criterios" });
                }

                return Json(datos);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "Error en el servidor",
                    details = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }



        //Grap 6
        public async Task<IActionResult> OfertasPorEmpresa()
        {
            try
            {

                var datos = await (from e in _context.Empresa
                                   join oe in _context.OfertaEmpleo on e.id_empresa equals oe.id_empresa into ofertas
                                   from oferta in ofertas.DefaultIfEmpty()
                                   group oferta by e.nombre into g
                                   select new OfertasPorEmpresaViewModel
                                   {
                                       Empresa = g.Key,
                                       TotalOfertas = g.Count(x => x != null)
                                   }).ToListAsync();


                System.Diagnostics.Debug.WriteLine($"Datos obtenidos (Ofertas por Empresa): {Newtonsoft.Json.JsonConvert.SerializeObject(datos)}");

                if (!datos.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No se encontraron datos para el gráfico de ofertas por empresa.");
                }

                return View("~/Views/Gráficos/OfertasPorEmpresa.cshtml", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        //Grap 7
        [HttpGet]
        public async Task<IActionResult> OfertasPorRangoFechas(DateTime fechaInicio, DateTime fechaFin, bool json = false)
        {
            try
            {
                // Validaciones (se mantienen igual)
                if (fechaInicio > fechaFin)
                {
                    return json
                        ? BadRequest(new { error = "La fecha de inicio no puede ser mayor a la fecha final" })
                        : BadRequest("La fecha de inicio no puede ser mayor a la fecha final");
                }

                if ((fechaFin - fechaInicio).TotalDays > 365)
                {
                    return json
                        ? BadRequest(new { error = "El rango de fechas no puede ser mayor a 1 año" })
                        : BadRequest("El rango de fechas no puede ser mayor a 1 año");
                }

                var datos = await _context.OfertaEmpleo
                    .Where(o => o.fecha_publicacion >= fechaInicio && o.fecha_publicacion <= fechaFin)
                    .GroupBy(o => o.fecha_publicacion.Date)
                    .Select(g => new
                    {
                        Fecha = g.Key,
                        TotalOfertas = g.Count()
                    })
                    .OrderBy(g => g.Fecha)
                    .ToListAsync();

                // Respuesta según el formato solicitado
                if (json)
                {
                    return Json(new
                    {
                        fechas = datos.Select(d => d.Fecha.ToString("yyyy-MM-dd")),
                        cantidades = datos.Select(d => d.TotalOfertas),
                        totalOfertas = datos.Sum(d => d.TotalOfertas),
                        promedioPorDia = datos.Any() ? datos.Average(d => d.TotalOfertas) : 0,
                        diaMasOfertas = datos.Any() ? datos.OrderByDescending(d => d.TotalOfertas).First().Fecha.ToString("dd/MM/yyyy") : "N/A"
                    });
                }
                else
                {
                    // Para la vista HTML, pasamos datos dinámicos
                    var viewModel = datos.Select(d => new
                    {
                        Fecha = d.Fecha,
                        TotalOfertas = d.TotalOfertas
                    }).ToList<dynamic>();

                    return View("~/Views/Gráficos/OfertasPorRangoFechas.cshtml", viewModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en OfertasPorRangoFechas");
                return json
                    ? StatusCode(500, new { error = "Error interno del servidor" })
                    : StatusCode(500, "Error interno del servidor");
            }
        }

        //Devuelve la vista del gráfico 8, que utiliza JS para obtener los datos
        public IActionResult Grafico8()
        {
            return View("~/Views/Gráficos/Grafico8.cshtml");
        }

        //Metodo para obtener los datos para el gráfico 8
        [HttpGet]
        public async Task<IActionResult> GetOfertasPorPaisData()
        {
            try
            {
                // 1. Verificar conexión
                if (!_context.Database.CanConnect())
                {
                    return Json(new { error = "No se pudo conectar a la base de datos" });
                }

                // 2. Verificar que haya ofertas
                var totalOfertas = await _context.OfertaEmpleo.CountAsync();
                if (totalOfertas == 0)
                {
                    return Json(new { warning = "No hay ofertas de empleo registradas" });
                }

                // 3. Consulta tipo Qlik: JOIN manual entre OfertaEmpleo y Pais
                var datos = await (
                    from o in _context.OfertaEmpleo
                    join p in _context.Pais on o.id_pais equals p.id_pais
                    group o by p.nombre into g
                    select new
                    {
                        pais = g.Key,
                        cantidad = g.Count(),
                        porcentaje = (g.Count() * 100.0) / totalOfertas
                    }
                )
                .OrderByDescending(x => x.cantidad)
                .ToListAsync();

                return Json(datos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completo: {ex}");
                return Json(new
                {
                    error = "Error en el servidor",
                    details = ex.Message
                });
            }
        }
    }
}
