using Microsoft.AspNetCore.Mvc;
using SisEmpleo.Models;
using SisEmpleo.Services;

namespace SisEmpleo.Controllers
{
    [AutenticacionPostulante]
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
            // Verificar si el usuario está autenticado como postulante
            if (HttpContext.Session.GetString("tipo_usuario") != "P")
            {
                return RedirectToAction("Index", "Home");
            }

            // Obtener las categorías a las que está suscrito el postulante
            int idUsuario = HttpContext.Session.GetInt32("id_usuario") ?? 0;
            var categoriasSuscritas = (from sc in _EmpleoContext.SuscripcionCategoria
                                       where sc.id_usuario == idUsuario
                                       select sc.id_categoriaprofesional).ToList();

            // Obtener las ofertas que coincidan con las categorías suscritas
            var ofertas = (from o in _EmpleoContext.OfertaEmpleo
                           join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais
                           join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia
                           join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa
                           join oc in _EmpleoContext.OfertaCategoria on o.id_ofertaempleo equals oc.id_ofertaempleo
                           where categoriasSuscritas.Contains(oc.id_categoriaprofesional) && o.estado == 'A' // Solo ofertas activas
                           orderby o.fecha_publicacion descending
                           select new
                           {
                               Id = o.id_ofertaempleo,
                               Titulo = o.titulo,
                               Vacantes = o.vacantes,
                               Salario = o.salario,
                               Duracion_Contrato = o.duracion_contrato,
                               Fecha_Publicacion = o.fecha_publicacion,
                               Nombre_Empresa = e.nombre,
                               Ubi_Pais = p.nombre,
                               Ubi_Pro = pro.nombre
                           }).ToList();

            ViewBag.ofertas = ofertas; // Ajustamos a minúsculas para coincidir con la vista
            return View("Listar");
        }

        private List<Dictionary<string, string>> SepararHorario(string horario)
        {
            List<Dictionary<string, string>> SepHorario = new List<Dictionary<string, string>>();
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
                {
                    HorarioCon1 = "Sin Horario";
                }
                if (Horario2Inicio == "00:00" && Horario2Fin == "00:00")
                {
                    HorarioCon2 = "Sin Horario";
                }

                SepHorario.Add(new Dictionary<string, string>
                {
                    { "Dia", Dias[i] },
                    { "Horario1", $"{HorarioCon1}" },
                    { "Horario2", $"{HorarioCon2}" }
                });
            }

            return SepHorario;
        }

        [HttpGet]
        public IActionResult VerOferta(int id_ofertaempleo)
        {
            var oferta = (from o in _EmpleoContext.OfertaEmpleo
                          join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais
                          join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia
                          join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa
                          where o.id_ofertaempleo == id_ofertaempleo
                          select new
                          {
                              Id = o.id_ofertaempleo,
                              Titulo = o.titulo,
                              Vacantes = o.vacantes,
                              Salario = o.salario,
                              Duracion_Contrato = o.duracion_contrato,
                              Fecha_Publicacion = o.fecha_publicacion,
                              Nombre_Empresa = e.nombre,
                              Ubi_Pais = p.nombre,
                              Ubi_Pro = pro.nombre,
                              Horario = o.horario
                          }).FirstOrDefault();

            if (oferta == null)
            {
                return NotFound();
            }

            ViewBag.Horario = SepararHorario(oferta.Horario);
            ViewBag.Oferta = oferta;
            return View();
        }

        [HttpPost]
        public IActionResult PostularseOferta(int id_ofertaempleo)
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));

            OfertaCandidatos candidato = new OfertaCandidatos
            {
                id_usuario = id_usuario,
                id_ofertaempleo = id_ofertaempleo
            };

            _EmpleoContext.OfertaCandidatos.Add(candidato);
            _EmpleoContext.SaveChanges();

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult VerOfertaPostulado()
        {
            int id_usuario = Convert.ToInt32(HttpContext.Session.GetInt32("id_usuario"));
            var oferta = (from oc in _EmpleoContext.OfertaCandidatos
                          join o in _EmpleoContext.OfertaEmpleo on oc.id_ofertaempleo equals o.id_ofertaempleo
                          join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais
                          join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia
                          join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa
                          where oc.id_usuario == id_usuario
                          select new
                          {
                              Id = o.id_ofertaempleo,
                              Titulo = o.titulo,
                              Vacantes = o.vacantes,
                              Salario = o.salario,
                              Duracion_Contrato = o.duracion_contrato,
                              Fecha_Publicacion = o.fecha_publicacion,
                              Nombre_Empresa = e.nombre,
                              Ubi_Pais = p.nombre,
                              Ubi_Pro = pro.nombre,
                              Horario = o.horario
                          }).ToList();

            ViewBag.ofertas = oferta;
            return View();
        }
    }
}