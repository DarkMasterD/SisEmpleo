using Microsoft.AspNetCore.Mvc;
using SisEmpleo.Models;

namespace SisEmpleo.Controllers
{
    public class RegistroUserPostulanteDTO
    {
        public string email { get; set; }
        public string contrasenia { get; set; }
        public string nombre_usuario { get; set; }

        public string apellido { get; set; }
        public string direccion { get; set; }
         public int id_pais { get; set; }
        public int id_provincia { get; set; }
        public int id_idioma { get; set; }
        public DateTime fecha_nacimiento { get; set; }


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
        [HttpGet]
        public ActionResult RecuperarContra()
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
                // Actualización del last_login (único cambio agregado)
                usuario.last_login = DateTime.Now;
                _EmpleoContext.SaveChanges();

                HttpContext.Session.SetInt32("id_usuario", usuario.id_usuario);
                HttpContext.Session.SetString("tipo_usuario", usuario.tipo_usuario.ToString());

                if (usuario.tipo_usuario.ToString() == "P")
                {
                    var postulante = (from u in _EmpleoContext.Usuario
                                      join p in _EmpleoContext.Postulante
                                      on u.id_usuario equals p.id_usuario
                                      where u.id_usuario == usuario.id_usuario
                                      select new
                                      {
                                          p.id_postulante
                                      }).FirstOrDefault();
                    if (postulante != null)
                    {
                        HttpContext.Session.SetInt32("id_postulante", postulante.id_postulante);
                    }
                }
                else if (usuario.tipo_usuario.ToString() == "E")
                {
                    var empresa = (from u in _EmpleoContext.Usuario
                                   join e in _EmpleoContext.Empresa
                                   on u.id_usuario equals e.id_usuario
                                   where u.id_usuario == usuario.id_usuario
                                   select new
                                   {
                                       e.id_empresa
                                   }).FirstOrDefault();
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
            ViewBag.Paises = _EmpleoContext.Pais.ToList();
            ViewBag.Provincias = _EmpleoContext.Provincia.ToList();
            ViewBag.Idiomas = _EmpleoContext.Idioma.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarsePostulante(RegistroUserPostulanteDTO datos)
        {
            try
            {
                Usuario user = new Usuario
                {
                    nombre_usuario = datos.nombre_usuario,
                    email = datos.email,
                    contrasenia = datos.contrasenia,
                    tipo_usuario = 'P',
                    fecha_creacion = DateTime.Now,
                    last_login = DateTime.Now,
                    estado = 'A'
                };

                _EmpleoContext.Usuario.Add(user);
                _EmpleoContext.SaveChanges();

                Postulante postulante = new Postulante
                {
                    id_usuario = user.id_usuario,
                    nombre = datos.nombre_usuario,
                    apellido = datos.apellido,
                    direccion = datos.direccion,
                    fecha_nacimiento = datos.fecha_nacimiento,
                    id_pais = datos.id_pais,
                    id_provincia = datos.id_provincia,
                    id_idioma = datos.id_idioma
                };

                _EmpleoContext.Postulante.Add(postulante);
                _EmpleoContext.SaveChanges();

                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Paises = _EmpleoContext.Pais.ToList();
                ViewBag.Provincias = _EmpleoContext.Provincia.ToList();
                ViewBag.Idiomas = _EmpleoContext.Idioma.ToList();
                return View();
            }
        }

        [HttpGet]
        public JsonResult ObtenerProvinciasPorPais(int id_pais)
        {
            var provincias = _EmpleoContext.Provincia
                .Where(p => p.id_pais == id_pais)
                .Select(p => new { p.id_provincia, p.nombre })
                .ToList();

            return Json(provincias);
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
                Usuario user = new Usuario();
                user.email = datos.email;
                user.contrasenia = datos.contrasenia;
                user.tipo_usuario = 'E';
                _EmpleoContext.Usuario.Add(user);
                _EmpleoContext.SaveChanges();

                Empresa empresa = new Empresa();
                empresa.id_usuario = user.id_usuario;
                empresa.nombre = datos.nombre;
                empresa.direccion = datos.direccion;
                empresa.descripcion_empresa = datos.descripcion_empresa;
                empresa.sector_empresa = datos.sector_empresa;
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
