using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SisEmpleo.Models;
using System.Net.Mail;
using System.Net;
using System;
using System.Linq;


namespace SisEmpleo.Controllers
{
    public class RegistroUserPostulanteDTO
    {
        public string email { get; set; }
        public string contrasenia { get; set; }
        public string nombre_usuario { get; set; } 
        public string nombre { get; set; }
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
        private const string PasswordResetSessionKey = "PasswordResetInfo";

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
        public IActionResult Login(string credencial, string contrasenia)
        {
            // Buscar usuario por email O nombre de usuario
            var usuario = (from u in _EmpleoContext.Usuario
                           where (u.email == credencial || u.nombre_usuario == credencial)
                           && u.contrasenia == contrasenia
                           select u).FirstOrDefault();

            if (usuario != null)
            {
                // Actualización del last_login
                usuario.last_login = DateTime.Now;
                _EmpleoContext.SaveChanges();

                HttpContext.Session.SetInt32("id_usuario", usuario.id_usuario);
                HttpContext.Session.SetString("tipo_usuario", usuario.tipo_usuario.ToString());

                if (usuario.tipo_usuario == 'P')
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
                else if (usuario.tipo_usuario == 'E')
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

            ViewData["ErrorMessage"] = "Credenciales incorrectas. Intente nuevamente.";
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
                // 1. Validación de edad (mínimo 18 años)
                var edadMinima = DateTime.Now.AddYears(-18);
                if (datos.fecha_nacimiento > edadMinima)
                {
                    ModelState.AddModelError("fecha_nacimiento", "Debes tener al menos 18 años para registrarte.");
                }

                // 2. Validación de fecha no futura
                if (datos.fecha_nacimiento > DateTime.Now)
                {
                    ModelState.AddModelError("fecha_nacimiento", "La fecha de nacimiento no puede ser futura.");
                }

                // 3. Validación de correo único
                var correoExistente = _EmpleoContext.Usuario.Any(u => u.email == datos.email);
                if (correoExistente)
                {
                    ModelState.AddModelError("email", "Este correo electrónico ya está registrado.");
                }

                // 4. Validación de contraseña
                var password = datos.contrasenia;
                if (string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("contrasenia", "La contraseña es obligatoria.");
                }
                else
                {
                    if (password.Length < 8)
                    {
                        ModelState.AddModelError("contrasenia", "La contraseña debe tener al menos 8 caracteres.");
                    }
                    if (!password.Any(char.IsDigit))
                    {
                        ModelState.AddModelError("contrasenia", "La contraseña debe contener al menos un número.");
                    }
                    if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                    {
                        ModelState.AddModelError("contrasenia", "La contraseña debe contener al menos un carácter especial.");
                    }
                }

                // Si hay errores, recargar vista con datos
                if (!ModelState.IsValid)
                {
                    ViewBag.Paises = _EmpleoContext.Pais.ToList();
                    ViewBag.Provincias = _EmpleoContext.Provincia.ToList();
                    ViewBag.Idiomas = _EmpleoContext.Idioma.ToList();
                    return View(datos);
                }
                // Generar un nombre de usuario único si es necesario
                var nombreUsuarioUnico = datos.nombre_usuario;
                if (_EmpleoContext.Usuario.Any(u => u.nombre_usuario == nombreUsuarioUnico))
                {
                    // Agregar un sufijo numérico si ya existe
                    int suffix = 1;
                    while (_EmpleoContext.Usuario.Any(u => u.nombre_usuario == $"{nombreUsuarioUnico}{suffix}"))
                    {
                        suffix++;
                    }
                    nombreUsuarioUnico = $"{nombreUsuarioUnico}{suffix}";
                }

                Usuario user = new Usuario
                {

                    nombre_usuario = nombreUsuarioUnico, 

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

                    nombre = datos.nombre, 
                    
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
        [HttpPost]
        public IActionResult EnviarCodigoRecuperacion(string email)
        {
            // Verificar si el email existe en la base de datos
            var usuario = _EmpleoContext.Usuario.FirstOrDefault(u => u.email == email);
            if (usuario == null)
            {
                TempData["ErrorMessage"] = "El correo electrónico no está registrado.";
                return RedirectToAction("RecuperarContra");
            }

            // Generar código de verificación
            var codigo = new Random().Next(100000, 999999).ToString();

            // Guardar en sesión (email + código + fecha de generación)
            var resetInfo = new PasswordResetInfo
            {
                Email = email,
                Codigo = codigo,
                FechaGeneracion = DateTime.Now
            };
            HttpContext.Session.SetString(PasswordResetSessionKey,
                                        System.Text.Json.JsonSerializer.Serialize(resetInfo));

            // Enviar correo electrónico
            EnviarCorreoRecuperacion(email, codigo);

            return RedirectToAction("VerificarCodigo");
        }

        [HttpGet]
        public IActionResult VerificarCodigo()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerificarCodigo(string codigo)
        {
            var resetInfoJson = HttpContext.Session.GetString(PasswordResetSessionKey);
            if (string.IsNullOrEmpty(resetInfoJson))
            {
                TempData["ErrorMessage"] = "La sesión ha expirado o no se ha iniciado el proceso de recuperación.";
                return RedirectToAction("RecuperarContra");
            }

            var resetInfo = System.Text.Json.JsonSerializer.Deserialize<PasswordResetInfo>(resetInfoJson);

            // Verificar si el código ha expirado (15 minutos)
            if (DateTime.Now > resetInfo.FechaGeneracion.AddMinutes(15))
            {
                TempData["ErrorMessage"] = "El código ha expirado. Por favor, solicita uno nuevo.";
                return RedirectToAction("RecuperarContra");
            }

            // Verificar el código
            if (codigo != resetInfo.Codigo)
            {
                TempData["ErrorMessage"] = "El código de verificación es incorrecto.";
                return View();
            }

            // Código correcto, redirigir a cambiar contraseña
            return RedirectToAction("CambiarContrasenia");
        }

        [HttpGet]
        public IActionResult CambiarContrasenia()
        {
            var resetInfoJson = HttpContext.Session.GetString(PasswordResetSessionKey);
            if (string.IsNullOrEmpty(resetInfoJson))
            {
                TempData["ErrorMessage"] = "La sesión ha expirado. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("RecuperarContra");
            }

            return View();
        }

        [HttpPost]
        public IActionResult CambiarContrasenia(string nuevaContrasenia, string confirmarContrasenia)
        {
            // 1. Validar que las contraseñas coincidan
            if (nuevaContrasenia != confirmarContrasenia)
            {
                TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
                return View();
            }

            // 2. Validar requisitos de contraseña segura
            if (string.IsNullOrWhiteSpace(nuevaContrasenia))
            {
                TempData["ErrorMessage"] = "La contraseña es obligatoria.";
                return View();
            }
            else
            {
                if (nuevaContrasenia.Length < 8)
                {
                    TempData["ErrorMessage"] = "La contraseña debe tener al menos 8 caracteres.";
                    return View();
                }
                if (!nuevaContrasenia.Any(char.IsDigit))
                {
                    TempData["ErrorMessage"] = "La contraseña debe contener al menos un número.";
                    return View();
                }
                if (!nuevaContrasenia.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    TempData["ErrorMessage"] = "La contraseña debe contener al menos un carácter especial.";
                    return View();
                }
            }

            // 3. Validar sesión de recuperación
            var resetInfoJson = HttpContext.Session.GetString(PasswordResetSessionKey);
            if (string.IsNullOrEmpty(resetInfoJson))
            {
                TempData["ErrorMessage"] = "La sesión ha expirado. Por favor, inicia el proceso nuevamente.";
                return RedirectToAction("RecuperarContra");
            }

            var resetInfo = System.Text.Json.JsonSerializer.Deserialize<PasswordResetInfo>(resetInfoJson);

            // 4. Actualizar contraseña en la base de datos
            var usuario = _EmpleoContext.Usuario.FirstOrDefault(u => u.email == resetInfo.Email);
            if (usuario != null)
            {
                usuario.contrasenia = nuevaContrasenia;
                _EmpleoContext.SaveChanges();
            }

            // 5. Limpiar la sesión
            HttpContext.Session.Remove(PasswordResetSessionKey);

          
            return RedirectToAction("Login");
        }

        private void EnviarCorreoRecuperacion(string emailDestinatario, string codigo)
        {
            // 1. Configuración del remitente (cambia estos datos)
            var fromAddress = new MailAddress("paolavidalmovil@gmail.com", "Workeen");
            const string fromPassword = "sgva otis afua myii"; // Contraseña de aplicación de Gmail

            // 2. Configuración del mensaje
            string subject = "Código de recuperación - Workeen";
            string body = $@"
                <h2>Recuperación de contraseña</h2>
                <p>Tu código de verificación es: <strong>{codigo}</strong></p>
                <p>Este código expirará en 15 minutos.</p>
                <p>Si no solicitaste este cambio, por favor ignora este mensaje.</p>
            ";

            // 3. Configuración del servidor SMTP (Gmail)
            using var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 10000 // 10 segundos de timeout
            };

            // 4. Construcción del mensaje
            using var message = new MailMessage(fromAddress, new MailAddress(emailDestinatario))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Permite formato HTML en el cuerpo
            };

            // 5. Envío seguro con manejo de errores
            try
            {
                smtp.Send(message);
            }
            catch (SmtpException ex)
            {
                // Loggear el error (recomendado usar ILogger en producción)
                Console.WriteLine($"Error al enviar correo: {ex.StatusCode} - {ex.Message}");
                throw; // Relanza la excepción para manejarla en el llamador
            }
        }

        private class PasswordResetInfo
        {
            public string Email { get; set; }
            public string Codigo { get; set; }
            public DateTime FechaGeneracion { get; set; }
        }
    }

}
