using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using System;
using System.Linq;

namespace SisEmpleo.Controllers
{
    public class RegistroUserPostulanteDTO
    {
        public string email { get; set; }
        public string contrasenia { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public string direccion { get; set; }
        public int id_idioma { get; set; }
        public DateTime fecha_nacimiento { get; set; }
        public List<SelectListItem> Paises { get; set; } = new();
        public List<SelectListItem> Provincias { get; set; } = new();
        public List<SelectListItem> Idiomas { get; set; } = new();
    }

    public class RegistroUserEmpresaDTO
    {
        public string email { get; set; }
        public string contrasenia { get; set; }
        public string nombre { get; set; }
        public string direccion { get; set; }
        public string descripcion_empresa { get; set; }
        public string sector_empresa { get; set; }
    }

    public class LoginController : Controller
    {
        private readonly EmpleoContext _EmpleoContext;

        public LoginController(EmpleoContext empleoContext)
        {
            _EmpleoContext = empleoContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string contrasenia)
        {
            var usuario = (from u in _EmpleoContext.Usuario
                           where u.email == email && u.contrasenia == contrasenia
                           select u).FirstOrDefault();

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("id_usuario", usuario.id_usuario);
                HttpContext.Session.SetString("tipo_usuario", usuario.tipo_usuario.ToString());

                if (usuario.tipo_usuario == 'P')
                {
                    var postulante = (from p in _EmpleoContext.Postulante
                                      where p.id_usuario == usuario.id_usuario
                                      select p).FirstOrDefault();
                    if (postulante != null)
                    {
                        HttpContext.Session.SetInt32("id_postulante", postulante.id_postulante);
                    }
                }
                else if (usuario.tipo_usuario == 'E')
                {
                    var empresa = (from e in _EmpleoContext.Empresa
                                   where e.id_usuario == usuario.id_usuario
                                   select e).FirstOrDefault();
                    if (empresa != null)
                    {
                        HttpContext.Session.SetInt32("id_empresa", empresa.id_empresa);
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = "Usuario o contraseña incorrecta";
            return View();
        }

        [HttpGet]
        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult RegistrarsePostulante()
        {
            var model = new RegistroUserPostulanteDTO
            {
                Paises = _EmpleoContext.Pais.Select(p => new SelectListItem
                {
                    Value = p.id_pais.ToString(),
                    Text = p.nombre
                }).ToList(),

                Provincias = _EmpleoContext.Provincia.Select(p => new SelectListItem
                {
                    Value = p.id_provincia.ToString(),
                    Text = p.nombre
                }).ToList(),

                Idiomas = _EmpleoContext.Idioma.Select(i => new SelectListItem
                {
                    Value = i.id_idioma.ToString(),
                    Text = i.nombre
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult RegistrarsePostulante([FromForm] RegistroUserPostulanteDTO datos)
        {
            if (!ModelState.IsValid)
            {
                // Recargar listas para volver a mostrar la vista con errores
                datos.Paises = _EmpleoContext.Pais.Select(p => new SelectListItem
                {
                    Value = p.id_pais.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.Provincias = _EmpleoContext.Provincia.Select(p => new SelectListItem
                {
                    Value = p.id_provincia.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.Idiomas = _EmpleoContext.Idioma.Select(i => new SelectListItem
                {
                    Value = i.id_idioma.ToString(),
                    Text = i.nombre
                }).ToList();

                return View(datos);
            }

            try
            {
                Usuario user = new Usuario
                {
                    email = datos.email,
                    contrasenia = datos.contrasenia, // Recuerda hashear la contraseña en producción
                    tipo_usuario = 'P',
                    nombre_usuario = datos.nombre,
                    fecha_creacion = DateTime.UtcNow,
                    last_login = DateTime.UtcNow,
                    estado = 'A'
                };

                _EmpleoContext.Usuario.Add(user);
                _EmpleoContext.SaveChanges();

                Postulante postulante = new Postulante
                {
                    id_usuario = user.id_usuario,
                    nombre = datos.nombre,
                    apellido = datos.apellido,
                    id_pais = datos.id_pais,
                    id_provincia = datos.id_provincia,
                    direccion = datos.direccion,
                    id_idioma = datos.id_idioma,
                    fecha_nacimiento = datos.fecha_nacimiento
                };

                _EmpleoContext.Postulante.Add(postulante);
                _EmpleoContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                datos.Paises = _EmpleoContext.Pais.Select(p => new SelectListItem
                {
                    Value = p.id_pais.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.Provincias = _EmpleoContext.Provincia.Select(p => new SelectListItem
                {
                    Value = p.id_provincia.ToString(),
                    Text = p.nombre
                }).ToList();

                datos.Idiomas = _EmpleoContext.Idioma.Select(i => new SelectListItem
                {
                    Value = i.id_idioma.ToString(),
                    Text = i.nombre
                }).ToList();

                ModelState.AddModelError("", "Error al registrar: " + ex.Message);
                return View(datos);
            }
        }

        [HttpGet]
        public IActionResult RegistrarseEmpresa()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarseEmpresa([FromForm] RegistroUserEmpresaDTO datos)
        {
            try
            {
                Usuario user = new Usuario
                {
                    email = datos.email,
                    contrasenia = datos.contrasenia, // Ideal: Hashear la contraseña antes de guardar
                    tipo_usuario = 'E',
                    nombre_usuario = datos.nombre,
                    fecha_creacion = DateTime.UtcNow,
                    last_login = DateTime.UtcNow,
                    estado = 'A'
                };

                _EmpleoContext.Usuario.Add(user);
                _EmpleoContext.SaveChanges();

                Empresa empresa = new Empresa
                {
                    id_usuario = user.id_usuario,
                    nombre = datos.nombre,
                    direccion = datos.direccion,
                    descripcion_empresa = datos.descripcion_empresa,
                    sector_empresa = datos.sector_empresa
                };

                _EmpleoContext.Empresa.Add(empresa);
                _EmpleoContext.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}