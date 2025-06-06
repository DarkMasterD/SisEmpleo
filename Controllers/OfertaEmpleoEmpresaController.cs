using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using SisEmpleo.Models.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PaisModel = SisEmpleo.Models.Pais;


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
            // Obtener id_empresa de la sesión
            var idEmpresa = HttpContext.Session.GetInt32("id_empresa");
            if (idEmpresa == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Obtener datos de la empresa
            var empresa = _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == idEmpresa.Value);
            if (empresa == null)
            {
                return NotFound();
            }

            // Cargar datos para los dropdowns
            ViewBag.Pais = _EmpleoContext.Pais
                .Select(p => new SelectListItem
                {
                    Value = p.id_pais.ToString(),
                    Text = p.nombre
                }).ToList();

            // Cargar categorías profesionales de la empresa
            ViewBag.Categorias = _EmpleoContext.CategoriaProfesional
                .ToList();

            // Cargar subcategorías profesionales de la empresa
            ViewBag.Subcategorias = _EmpleoContext.SubcategoriaProfesional
                .Where(s => s.id_empresa == idEmpresa.Value)
                .ToList();

            // Cargar prestaciones de ley de la empresa
            ViewBag.Prestaciones = _EmpleoContext.PrestacionLey
                .ToList();

            // Cargar habilidades
            ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();

            // Pasar datos de la empresa a la vista
            ViewBag.id_empresa = empresa.id_empresa;
            ViewBag.EmpresaNombre = empresa.nombre;

            return View(new OfertaEmpleo());
        }

        //ORIGINAL

        //// GET: Mostrar formulario para crear nueva oferta
        //[HttpGet]
        //public IActionResult NuevaOferta()
        //{
        //    // Populate ViewBag.Pais with SelectList
        //    ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
        //    {
        //        Value = p.id_pais.ToString(),
        //        Text = p.nombre
        //    }).ToList();

        //    // Populate ViewBag.Habilidades
        //    ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();

        //    // Get id_empresa and EmpresaNombre from session
        //    var idEmpresa = HttpContext.Session.GetInt32("id_empresa");
        //    if (idEmpresa == null)
        //    {
        //        ModelState.AddModelError("", "No se ha identificado la empresa.");
        //        return View(new OfertaEmpleo());
        //    }

        //    var empresa = _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == idEmpresa.Value);
        //    if (empresa == null)
        //    {
        //        ModelState.AddModelError("", "Empresa no encontrada.");
        //        return View(new OfertaEmpleo());
        //    }

        //    ViewBag.id_empresa = empresa.id_empresa;
        //    ViewBag.EmpresaNombre = empresa.nombre;

        //    return View(new OfertaEmpleo());
        //}


        //ORIGINAL

        //// Fix for CS0136: Renamed the inner variable 'idEmpresa' to 'empresaId' to avoid conflict with the outer variable.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult NuevaOferta(OfertaEmpleo oferta, List<int> HabilidadesSeleccionadas)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            // Set server-side fields
        //            oferta.fecha_publicacion = DateTime.Now;
        //            oferta.estado = true;

        //            // Assign id_empresa from session
        //            var empresaId = HttpContext.Session.GetInt32("id_empresa"); // Renamed variable
        //            if (empresaId == null)
        //            {
        //                ModelState.AddModelError("", "No se ha identificado la empresa.");
        //                // Repopulate ViewBag for error view
        //                ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
        //                {
        //                    Value = p.id_pais.ToString(),
        //                    Text = p.nombre
        //                }).ToList();
        //                ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();
        //                return View(oferta);
        //            }
        //            oferta.id_empresa = empresaId.Value; // Updated to use 'empresaId'

        //            // Save oferta
        //            _EmpleoContext.OfertaEmpleo.Add(oferta);
        //            _EmpleoContext.SaveChanges();

        //            // Save selected habilidades to RequisitoOferta
        //            if (HabilidadesSeleccionadas != null && HabilidadesSeleccionadas.Any())
        //            {
        //                foreach (var idHabilidad in HabilidadesSeleccionadas)
        //                {
        //                    var requisito = new RequisitoOferta
        //                    {
        //                        id_ofertaempleo = oferta.id_ofertaempleo,
        //                        id_habilidad = idHabilidad
        //                    };
        //                    _EmpleoContext.RequisitoOferta.Add(requisito);
        //                }
        //                _EmpleoContext.SaveChanges();
        //            }

        //            return RedirectToAction("Index", "Home");
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", "Error al guardar la oferta: " + ex.Message);
        //        }
        //    }

        //    // Repopulate ViewBag on validation failure
        //    ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
        //    {
        //        Value = p.id_pais.ToString(),
        //        Text = p.nombre
        //    }).ToList();
        //    ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();
        //    var empresaIdForViewBag = HttpContext.Session.GetInt32("id_empresa");
        //    var empresa = empresaIdForViewBag != null ? _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == empresaIdForViewBag.Value) : null;
        //    ViewBag.id_empresa = empresa?.id_empresa;
        //    ViewBag.EmpresaNombre = empresa?.nombre;

        //    return View(oferta);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NuevaOferta(OfertaEmpleo oferta, List<int> HabilidadesSeleccionadas, List<int> PrestacionesSeleccionadas, int id_categoriaprofesional)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set server-side fields
                    oferta.fecha_publicacion = DateTime.Now;
                    oferta.estado = true;

                    // Assign id_empresa from session
                    var empresaId = HttpContext.Session.GetInt32("id_empresa");
                    if (empresaId == null)
                    {
                        ModelState.AddModelError("", "No se ha identificado la empresa.");
                        return View(RepoblarViewBags(oferta));
                    }
                    oferta.id_empresa = empresaId.Value;

                    // Save oferta
                    _EmpleoContext.OfertaEmpleo.Add(oferta);
                    _EmpleoContext.SaveChanges();

                    // Save selected habilidades to RequisitoOferta
                    if (HabilidadesSeleccionadas != null && HabilidadesSeleccionadas.Any())
                    {
                        foreach (var idHabilidad in HabilidadesSeleccionadas)
                        {
                            _EmpleoContext.RequisitoOferta.Add(new RequisitoOferta
                            {
                                id_ofertaempleo = oferta.id_ofertaempleo,
                                id_habilidad = idHabilidad
                            });
                        }
                    }

                    if (PrestacionesSeleccionadas != null && PrestacionesSeleccionadas.Any())
                    {
                        // Obtener todas las prestaciones de una vez para mejor performance
                        var prestaciones = _EmpleoContext.PrestacionLey
                            .Where(p => PrestacionesSeleccionadas.Contains(p.id_prestacionley))
                            .ToList();

                        foreach (var prestacion in prestaciones)
                        {
                            _EmpleoContext.OfertaEmpleoPrestacion.Add(new OfertaEmpleoPrestacion
                            {
                                id_ofertaempleo = oferta.id_ofertaempleo,
                                id_prestacionley = prestacion.id_prestacionley,
                                descripcion = prestacion.nombre // Usamos el nombre de la prestación como descripción
                            });
                        }
                    }

                    // Guardar la categoría después de tener el id_ofertaempleo
                    if (id_categoriaprofesional > 0)
                    {
                        // Necesitas también el id_subcategoriaprofesional
                        // Asumo que puedes obtenerlo de alguna forma, aquí un ejemplo:
                        var primeraSubcategoria = _EmpleoContext.SubcategoriaProfesional
                            .FirstOrDefault(s => s.id_categoriaprofesional == id_categoriaprofesional);

                        if (primeraSubcategoria != null)
                        {
                            _EmpleoContext.OfertaCategoria.Add(new OfertaCategoria
                            {
                                id_ofertaempleo = oferta.id_ofertaempleo,
                                id_categoriaprofesional = id_categoriaprofesional,
                                id_subcategoriaprofesional = primeraSubcategoria.id_subcategoriaprofesional
                            });
                        }
                    }

                    // Guardar todos los cambios juntos
                    _EmpleoContext.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // Obtener el mensaje de error interno
                    string errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " | Inner Exception: " + ex.InnerException.Message;

                        // Si es un error de SQL, podemos obtener aún más detalles
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessage += " | " + ex.InnerException.InnerException.Message;
                        }
                    }

                    ModelState.AddModelError("", "Error al guardar la oferta: " + errorMessage);
                    return View(RepoblarViewBags(oferta));
                }
            }

            return View(RepoblarViewBags(oferta));
        }

        // Método auxiliar para repoblar ViewBags
        private OfertaEmpleo RepoblarViewBags(OfertaEmpleo oferta)
        {
            ViewBag.Pais = _EmpleoContext.Pais.Select(p => new SelectListItem
            {
                Value = p.id_pais.ToString(),
                Text = p.nombre
            }).ToList();

            ViewBag.Habilidades = _EmpleoContext.Habilidad.ToList();
            ViewBag.Prestaciones = _EmpleoContext.PrestacionLey.ToList(); // Añadir esto

            ViewBag.Categorias = _EmpleoContext.CategoriaProfesional.ToList(); // También faltaba esto

            var empresaId = HttpContext.Session.GetInt32("id_empresa");
            var empresa = empresaId != null ? _EmpleoContext.Empresa.FirstOrDefault(e => e.id_empresa == empresaId.Value) : null;
            ViewBag.id_empresa = empresa?.id_empresa;
            ViewBag.EmpresaNombre = empresa?.nombre;

            return oferta;
        }

        [HttpGet]
        public JsonResult ObtenerProvincia(int id_pais)
        {
            var provincias = _EmpleoContext.Provincia
                .Where(p => p.id_pais == id_pais)
                .Select(p => new { p.id_provincia, p.nombre })
                .ToList();
            return Json(provincias, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // Mantener el casing original
                WriteIndented = true
            });
        }

        [HttpGet]
        public IActionResult GetProvinciasByPais(int idPais)
        {
            var provincias = _EmpleoContext.Provincia
                .Where(p => p.id_pais == idPais)
                .Select(p => new { p.id_provincia, p.nombre })
                .ToList();

            return Json(provincias);
        }

        [HttpGet]
        public IActionResult ListarOfertas()
        {
            int id_empresa = Convert.ToInt32(HttpContext.Session.GetInt32("id_empresa"));

            // Obtener todas las ofertas de la empresa con sus categorías
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
                               Ubi_Pro = pro.nombre,
                               CategoriaId = (from oc in _EmpleoContext.OfertaCategoria
                                              where oc.id_ofertaempleo == o.id_ofertaempleo
                                              select oc.id_categoriaprofesional).FirstOrDefault()
                           }).ToList();

            // Obtener todas las categorías profesionales disponibles
            var categorias = _EmpleoContext.CategoriaProfesional.ToList();

            // Preparar las categorías de la empresa con conteo de ofertas
            var categoriasEmpresa = categorias.Select(c => new
            {
                Id = c.id_categoriaprofesional,
                Nombre = c.nombre,
                CantidadOfertas = ofertas.Count(o => o.CategoriaId == c.id_categoriaprofesional)
            }).Where(c => c.CantidadOfertas > 0).ToList();

            ViewData["listOfertas"] = ofertas;
            ViewData["categoriasEmpresa"] = categoriasEmpresa;

            return View();
        }



        [HttpGet]
        public async Task<IActionResult> EditarOferta(int id_ofertaempleo)
        {
            var oferta = await _EmpleoContext.OfertaEmpleo
                .FirstOrDefaultAsync(o => o.id_ofertaempleo == id_ofertaempleo);

            if (oferta == null) return NotFound();

            // Procesar horario
            string[] horarioArray = new string[28];
            if (!string.IsNullOrEmpty(oferta.horario))
            {
                var temp = oferta.horario.Split(';');
                Array.Copy(temp, horarioArray, Math.Min(temp.Length, 28));
            }

            // Opciones para el estado
            var opcionesEstado = new List<SelectListItem>
    {
        new SelectListItem { Text = "Activo", Value = "1", Selected = oferta.estado == true },
        new SelectListItem { Text = "Inactivo", Value = "0", Selected = oferta.estado == false }
    };

            // Cargar datos asíncronos
            var paises = await _EmpleoContext.Pais.ToListAsync();
            var provincias = await _EmpleoContext.Provincia
                .Where(p => p.id_pais == oferta.id_pais)
                .ToListAsync();

            // Cargar categorías, prestaciones y habilidades
            var idEmpresa = HttpContext.Session.GetInt32("id_empresa");
            var empresa = idEmpresa != null ? await _EmpleoContext.Empresa.FirstOrDefaultAsync(e => e.id_empresa == idEmpresa.Value) : null;

            ViewBag.Pais = new SelectList(paises, "id_pais", "nombre", oferta.id_pais);
            ViewBag.Provincia = new SelectList(provincias, "id_provincia", "nombre", oferta.id_provincia);
            ViewBag.Horario = horarioArray;
            ViewBag.Oferta = oferta;
            ViewBag.OpcEstado = opcionesEstado;
            ViewBag.TipoUsuario = "E";
            ViewBag.id_empresa = empresa?.id_empresa;
            ViewBag.EmpresaNombre = empresa?.nombre;

            // Cargar categorías profesionales
            ViewBag.Categorias = await _EmpleoContext.CategoriaProfesional.ToListAsync();

            // Cargar prestaciones de ley
            ViewBag.Prestaciones = await _EmpleoContext.PrestacionLey.ToListAsync();

            // Cargar habilidades
            ViewBag.Habilidades = await _EmpleoContext.Habilidad.ToListAsync();

            // Cargar habilidades seleccionadas
            var habilidadesSeleccionadas = await _EmpleoContext.RequisitoOferta
                .Where(ro => ro.id_ofertaempleo == id_ofertaempleo)
                .Select(ro => ro.id_habilidad)
                .ToListAsync();
            ViewBag.HabilidadesSeleccionadas = habilidadesSeleccionadas;

            // Cargar prestaciones seleccionadas
            var prestacionesSeleccionadas = await _EmpleoContext.OfertaEmpleoPrestacion
                .Where(op => op.id_ofertaempleo == id_ofertaempleo)
                .Select(op => op.id_prestacionley)
                .ToListAsync();
            ViewBag.PrestacionesSeleccionadas = prestacionesSeleccionadas;

            // Cargar categoría seleccionada
            var categoriaSeleccionada = await _EmpleoContext.OfertaCategoria
                .Where(oc => oc.id_ofertaempleo == id_ofertaempleo)
                .Select(oc => oc.id_categoriaprofesional)
                .FirstOrDefaultAsync();
            ViewBag.CategoriaSeleccionada = categoriaSeleccionada;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarOferta(OfertaEmpleo ofertaMod, List<int> HabilidadesSeleccionadas, List<int> PrestacionesSeleccionadas, int id_categoriaprofesional)
        {
            ModelState.Remove("estado");
            ModelState.Remove("PaisNombre");
            ModelState.Remove("EmpresaNombre");
            ModelState.Remove("ProvinciaNombre");

            ofertaMod.estado = Request.Form["estado"] == "1";

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Modelo inválido. Errores:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                RecargarViewBags(ofertaMod);
                return View(ofertaMod);
            }

            try
            {
                var ofertaActual = await _EmpleoContext.OfertaEmpleo
                    .FirstOrDefaultAsync(o => o.id_ofertaempleo == ofertaMod.id_ofertaempleo);

                if (ofertaActual == null)
                {
                    Console.WriteLine("Oferta no encontrada");
                    return NotFound();
                }

                // Actualizar propiedades
                ofertaActual.id_pais = ofertaMod.id_pais;
                ofertaActual.id_provincia = ofertaMod.id_provincia;
                ofertaActual.titulo = ofertaMod.titulo;
                ofertaActual.descripcion = ofertaMod.descripcion;
                ofertaActual.vacante = ofertaMod.vacante;
                ofertaActual.salario = ofertaMod.salario;
                ofertaActual.horario = ofertaMod.horario;
                ofertaActual.duracion_contrato = ofertaMod.duracion_contrato;
                ofertaActual.estado = ofertaMod.estado;

                // Actualizar habilidades
                var habilidadesActuales = await _EmpleoContext.RequisitoOferta
                    .Where(ro => ro.id_ofertaempleo == ofertaMod.id_ofertaempleo)
                    .ToListAsync();
                _EmpleoContext.RequisitoOferta.RemoveRange(habilidadesActuales);

                if (HabilidadesSeleccionadas != null && HabilidadesSeleccionadas.Any())
                {
                    foreach (var idHabilidad in HabilidadesSeleccionadas)
                    {
                        _EmpleoContext.RequisitoOferta.Add(new RequisitoOferta
                        {
                            id_ofertaempleo = ofertaMod.id_ofertaempleo,
                            id_habilidad = idHabilidad
                        });
                    }
                }

                // Actualizar prestaciones
                var prestacionesActuales = await _EmpleoContext.OfertaEmpleoPrestacion
                    .Where(op => op.id_ofertaempleo == ofertaMod.id_ofertaempleo)
                    .ToListAsync();
                _EmpleoContext.OfertaEmpleoPrestacion.RemoveRange(prestacionesActuales);

                if (PrestacionesSeleccionadas != null && PrestacionesSeleccionadas.Any())
                {
                    var prestaciones = await _EmpleoContext.PrestacionLey
                        .Where(p => PrestacionesSeleccionadas.Contains(p.id_prestacionley))
                        .ToListAsync();

                    foreach (var prestacion in prestaciones)
                    {
                        _EmpleoContext.OfertaEmpleoPrestacion.Add(new OfertaEmpleoPrestacion
                        {
                            id_ofertaempleo = ofertaMod.id_ofertaempleo,
                            id_prestacionley = prestacion.id_prestacionley,
                            descripcion = prestacion.nombre
                        });
                    }
                }

                // Actualizar categoría
                var categoriaActual = await _EmpleoContext.OfertaCategoria
                    .FirstOrDefaultAsync(oc => oc.id_ofertaempleo == ofertaMod.id_ofertaempleo);
                if (categoriaActual != null)
                {
                    _EmpleoContext.OfertaCategoria.Remove(categoriaActual);
                }

                if (id_categoriaprofesional > 0)
                {
                    var primeraSubcategoria = await _EmpleoContext.SubcategoriaProfesional
                        .FirstOrDefaultAsync(s => s.id_categoriaprofesional == id_categoriaprofesional);

                    if (primeraSubcategoria != null)
                    {
                        _EmpleoContext.OfertaCategoria.Add(new OfertaCategoria
                        {
                            id_ofertaempleo = ofertaMod.id_ofertaempleo,
                            id_categoriaprofesional = id_categoriaprofesional,
                            id_subcategoriaprofesional = primeraSubcategoria.id_subcategoriaprofesional
                        });
                    }
                }

                // Guardar cambios
                _EmpleoContext.Update(ofertaActual);
                await _EmpleoContext.SaveChangesAsync();

                Console.WriteLine("Oferta actualizada exitosamente");
                return RedirectToAction("ListarOfertas", "OfertaEmpleoEmpresa");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}\n{ex.InnerException?.Message}");
                ModelState.AddModelError("", $"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
                RecargarViewBags(ofertaMod);
                return View(ofertaMod);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                RecargarViewBags(ofertaMod);
                return View(ofertaMod);
            }
        }

        private void RecargarViewBags(OfertaEmpleo ofertaMod)
        {
            // 1. Lista de países
            ViewBag.Pais = new SelectList(
                _EmpleoContext.Pais.ToList(),
                "id_pais",
                "nombre",
                ofertaMod.id_pais
            );

            // 2. Lista de provincias según país seleccionado
            ViewBag.Provincia = new SelectList(
                _EmpleoContext.Provincia.Where(p => p.id_pais == ofertaMod.id_pais).ToList(),
                "id_provincia",
                "nombre",
                ofertaMod.id_provincia
            );

            // 3. Opciones de estado
            ViewBag.OpcEstado = new List<SelectListItem>
    {
        new SelectListItem { Text = "Activo", Value = "1", Selected = ofertaMod.estado == true },
        new SelectListItem { Text = "Inactivo", Value = "0", Selected = ofertaMod.estado == false }
    };

            // 4. Procesar horario para la vista
            var horarioArray = ofertaMod.horario?.Split(';') ?? Array.Empty<string>();
            var horarioCompleto = new string[28];
            Array.Copy(horarioArray, horarioCompleto, Math.Min(horarioArray.Length, 28));
            ViewBag.Horario = horarioCompleto;
        }







        [HttpGet]
        public IActionResult VerPostulantesEmpresa(int id_ofertaempleo)
        {
            try
            {
                // 1. Obtener información de la oferta (código existente)
                var oferta = (from o in _EmpleoContext.OfertaEmpleo
                              join p in _EmpleoContext.Set<SisEmpleo.Models.Pais>() on o.id_pais equals p.id_pais
                              join prov in _EmpleoContext.Provincia on o.id_provincia equals prov.id_provincia
                              join e in _EmpleoContext.Empresa on o.id_empresa equals e.id_empresa
                              where o.id_ofertaempleo == id_ofertaempleo
                              select new
                              {
                                  Oferta = o,
                                  PaisNombre = p.nombre,
                                  ProvinciaNombre = prov.nombre,
                                  EmpresaNombre = e.nombre
                              }).FirstOrDefault();

                if (oferta == null)
                {
                    ViewBag.Error = "La oferta de empleo no fue encontrada";
                    return View();
                }

                // Asignar nombres a propiedades NotMapped
                oferta.Oferta.PaisNombre = oferta.PaisNombre;
                oferta.Oferta.ProvinciaNombre = oferta.ProvinciaNombre;
                oferta.Oferta.EmpresaNombre = oferta.EmpresaNombre;

                ViewBag.OfertaTitulo = oferta.Oferta.titulo;
                ViewBag.OfertaDetalle = oferta.Oferta;

                // 2. Obtener postulantes para esta oferta
                var postulantes = (from oc in _EmpleoContext.OfertaCandidatos
                                   join u in _EmpleoContext.Usuario on oc.id_usuario equals u.id_usuario
                                   join p in _EmpleoContext.Postulante on u.id_usuario equals p.id_usuario
                                   join c in _EmpleoContext.Contacto on u.id_usuario equals c.id_usuario
                                   where oc.id_ofertaempleo == id_ofertaempleo
                                   select new PostulanteViewModel
                                   {
                                       IdPostulante = p.id_postulante,
                                       IdUsuario = u.id_usuario,
                                       Nombre = $"{p.nombre} {p.apellido}",
                                       Email = c.email,
                                       Telefono = c.telefono
                                   }).ToList();

                // 3. Cargar habilidades, responsabilidades y experiencia para cada postulante
                var todasHabilidades = new List<string>();
                var todasResponsabilidades = new List<string>();

                foreach (var postulante in postulantes)
                {
                    // Habilidades
                    postulante.Habilidades = _EmpleoContext.Curriculum
                        .Where(c => c.id_postulante == postulante.IdPostulante)
                        .Join(_EmpleoContext.Habilidad_Curriculum, c => c.id_curriculum, hc => hc.id_curriculum, (c, hc) => hc)
                        .Join(_EmpleoContext.Habilidad, hc => hc.id_habilidad, h => h.id_habilidad, (hc, h) => h.nombre)
                        .Distinct()
                        .ToList();

                    todasHabilidades.AddRange(postulante.Habilidades);

                    // Responsabilidades
                    postulante.Responsabilidades = _EmpleoContext.Curriculum
                        .Where(c => c.id_postulante == postulante.IdPostulante)
                        .Join(_EmpleoContext.ExperienciaProfesional, c => c.id_curriculum, ep => ep.id_curriculum, (c, ep) => ep.responsabilidad)
                        .Where(r => !string.IsNullOrEmpty(r))
                        .Distinct()
                        .ToList();

                    todasResponsabilidades.AddRange(postulante.Responsabilidades);

                    // Experiencia (calculamos los años totales pero no lo mostramos en la vista)
                    var curriculum = _EmpleoContext.Curriculum.FirstOrDefault(c => c.id_postulante == postulante.IdPostulante);
                    if (curriculum != null)
                    {
                        var experiencias = _EmpleoContext.ExperienciaProfesional
                            .Where(e => e.id_curriculum == curriculum.id_curriculum)
                            .ToList();

                        double totalDias = experiencias
                            .Where(e => e.fecha_fin.HasValue && e.fecha_inicio.HasValue)
                            .Sum(e => (e.fecha_fin.Value - e.fecha_inicio.Value).TotalDays);

                        postulante.AniosExperiencia = totalDias / 365; // Guardamos los años como decimal para filtrar
                    }
                }

                // 4. Preparar datos para los combo boxes
                ViewBag.Habilidades = new SelectList(todasHabilidades.Distinct().OrderBy(h => h));
                ViewBag.Responsabilidades = new SelectList(todasResponsabilidades.Distinct().OrderBy(r => r));
                ViewBag.ExperienciaFiltro = new SelectList(new List<SelectListItem>
                {
                    new SelectListItem { Value = "0-1", Text = "Mínimo 1 año" },
                    new SelectListItem { Value = "1-5", Text = "1 a 5 años" },
                    new SelectListItem { Value = "5-100", Text = "Más de 5 años" }
                }, "Value", "Text");

                ViewData["Postulantes"] = postulantes;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los postulantes";
                ViewBag.DebugInfo = ex.Message;

                // Inicializa vacíos para evitar NullReferenceException en la vista
                ViewBag.Habilidades = new SelectList(Enumerable.Empty<string>());
                ViewBag.Responsabilidades = new SelectList(Enumerable.Empty<string>());
                ViewBag.ExperienciaFiltro = new SelectList(Enumerable.Empty<string>());
                ViewData["Postulantes"] = new List<PostulanteViewModel>();
                return View();
            }
        }






    }


}