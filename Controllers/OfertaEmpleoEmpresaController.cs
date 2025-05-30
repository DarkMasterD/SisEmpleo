using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SisEmpleo.Controllers
{
    public class OfertaEmpleoEmpresaController : Controller
    {
        private readonly EmpleoContext _EmpleoContext;

        public OfertaEmpleoEmpresaController(EmpleoContext context)
        {
            _EmpleoContext = context;
        }

        // GET: Mostrar formulario para crear nueva oferta
        [HttpGet]
        public IActionResult NuevaOferta()
        {
            // Populate ViewBag.Pais with SelectList
            ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
            {
                Value = p.id_pais.ToString(),
                Text = p.nombre
            }).ToList();

            // Populate ViewBag.Habilidades
            ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();

            // Get id_empresa and EmpresaNombre from session
            var idEmpresa = HttpContext.Session.GetInt32("id_empresa");
            if (idEmpresa == null)
            {
                ModelState.AddModelError("", "No se ha identificado la empresa.");
                return View(new OfertaEmpleo());
            }

            var empresa = _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == idEmpresa.Value);
            if (empresa == null)
            {
                ModelState.AddModelError("", "Empresa no encontrada.");
                return View(new OfertaEmpleo());
            }

            ViewBag.id_empresa = empresa.id_empresa;
            ViewBag.EmpresaNombre = empresa.nombre;

            return View(new OfertaEmpleo());
        }

        // Fix for CS0136: Renamed the inner variable 'idEmpresa' to 'empresaId' to avoid conflict with the outer variable.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevaOferta(OfertaEmpleo oferta, List<int> HabilidadesSeleccionadas)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set server-side fields
                    oferta.fecha_publicacion = DateTime.Now;
                    oferta.estado = true;

                    // Assign id_empresa from session
                    var empresaId = HttpContext.Session.GetInt32("id_empresa"); // Renamed variable
                    if (empresaId == null)
                    {
                        ModelState.AddModelError("", "No se ha identificado la empresa.");
                        // Repopulate ViewBag for error view
                        ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
                        {
                            Value = p.id_pais.ToString(),
                            Text = p.nombre
                        }).ToList();
                        ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();
                        return View(oferta);
                    }
                    oferta.id_empresa = empresaId.Value; // Updated to use 'empresaId'

                    // Save oferta
                    _EmpleoContext.OfertaEmpleo.Add(oferta);
                    _EmpleoContext.SaveChanges();

                    // Save selected habilidades to RequisitoOferta
                    if (HabilidadesSeleccionadas != null && HabilidadesSeleccionadas.Any())
                    {
                        foreach (var idHabilidad in HabilidadesSeleccionadas)
                        {
                            var requisito = new RequisitoOferta
                            {
                                id_ofertaempleo = oferta.id_ofertaempleo,
                                id_habilidad = idHabilidad
                            };
                            _EmpleoContext.RequisitoOferta.Add(requisito);
                        }
                        _EmpleoContext.SaveChanges();
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar la oferta: " + ex.Message);
                }
            }

            // Repopulate ViewBag on validation failure
            ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
            {
                Value = p.id_pais.ToString(),
                Text = p.nombre
            }).ToList();
            ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();
            var empresaIdForViewBag = HttpContext.Session.GetInt32("id_empresa"); 
            var empresa = empresaIdForViewBag != null ? _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == empresaIdForViewBag.Value) : null;
            ViewBag.id_empresa = empresa?.id_empresa;
            ViewBag.EmpresaNombre = empresa?.nombre;

            return View(oferta);
        }

        [HttpGet]
        public JsonResult ObtenerProvincia(int id_pais)
        {
            var provincias = _EmpleoContext.Provincia
                .Where(p => p.id_pais == id_pais)
                .Select(p => new { p.id_provincia, p.nombre })
                .ToList();
            return Json(provincias);
        }

        [HttpGet]
        public IActionResult ListarOfertas()
        {
            int id_empresa = Convert.ToInt32(HttpContext.Session.GetInt32("id_empresa"));

            var ofertas = (from o in _EmpleoContext.OfertaEmpleo
                           join p in _EmpleoContext.Pais on o.id_pais equals p.id_pais
                           join pro in _EmpleoContext.Provincia on o.id_provincia equals pro.id_provincia
                           join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa
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
        new SelectListItem { Text = "Abierto", Value = "A" },
        new SelectListItem { Text = "Cerrado", Value = "C" }
    };

            ViewBag.OpcEstado = opcEstado;
            ViewBag.Oferta = oferta;
            ViewBag.Horario = lista;
            ViewBag.Pais = new SelectList(paises, "id_pais", "nombre");

            return View();
        }
        // este no funciona problema: SqlException: Cannot insert the value NULL into column
        // 'estado', table 'Empleo1.dbo.OfertaEmpleo'; column does not allow nulls. UPDATE fails.
        [HttpPost]
        public IActionResult EditarOferta([FromForm] OfertaEmpleo ofertaMod)
        {
            OfertaEmpleo? ofertaAct = _EmpleoContext.OfertaEmpleo
                .FirstOrDefault(o => o.id_ofertaempleo == ofertaMod.id_ofertaempleo);

            if (ofertaAct == null)
                return NotFound();

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


    }
}