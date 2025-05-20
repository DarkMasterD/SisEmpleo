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
        private readonly EmpleoContext _context;

        public OfertaEmpleoEmpresaController(EmpleoContext context)
        {
            _context = context;
        }

        // GET: Mostrar formulario para crear nueva oferta
        [HttpGet]
        public IActionResult NuevaOferta()
        {
            // Populate ViewBag.Pais with SelectList
            ViewBag.Pais = _context.Pais.Select(p => new SelectListItem
            {
                Value = p.id_pais.ToString(),
                Text = p.nombre
            }).ToList();

            // Populate ViewBag.Habilidades
            ViewBag.Habilidades = _context.Habilidad.ToList();

            // Get id_empresa and EmpresaNombre from session
            var idEmpresa = HttpContext.Session.GetInt32("id_empresa");
            if (idEmpresa == null)
            {
                ModelState.AddModelError("", "No se ha identificado la empresa.");
                return View(new OfertaEmpleo());
            }

            var empresa = _context.Empresa.FirstOrDefault(e => e.id_empresa == idEmpresa.Value);
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
                        ViewBag.Pais = _context.Pais.Select(p => new SelectListItem
                        {
                            Value = p.id_pais.ToString(),
                            Text = p.nombre
                        }).ToList();
                        ViewBag.Habilidades = _context.Habilidad.ToList();
                        return View(oferta);
                    }
                    oferta.id_empresa = empresaId.Value; // Updated to use 'empresaId'

                    // Save oferta
                    _context.OfertaEmpleo.Add(oferta);
                    _context.SaveChanges();

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
                            _context.RequisitoOferta.Add(requisito);
                        }
                        _context.SaveChanges();
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar la oferta: " + ex.Message);
                }
            }

            // Repopulate ViewBag on validation failure
            ViewBag.Pais = _context.Pais.Select(p => new SelectListItem
            {
                Value = p.id_pais.ToString(),
                Text = p.nombre
            }).ToList();
            ViewBag.Habilidades = _context.Habilidad.ToList();
            var empresaIdForViewBag = HttpContext.Session.GetInt32("id_empresa"); 
            var empresa = empresaIdForViewBag != null ? _context.Empresa.FirstOrDefault(e => e.id_empresa == empresaIdForViewBag.Value) : null;
            ViewBag.id_empresa = empresa?.id_empresa;
            ViewBag.EmpresaNombre = empresa?.nombre;

            return View(oferta);
        }

        [HttpGet]
        public JsonResult ObtenerProvincia(int id_pais)
        {
            var provincias = _context.Provincia
                .Where(p => p.id_pais == id_pais)
                .Select(p => new { p.id_provincia, p.nombre })
                .ToList();
            return Json(provincias);
        }
    }
}