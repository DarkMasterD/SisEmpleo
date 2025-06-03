using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using System.Net;
using System.Net.Mail;

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
        [ValidateAntiForgeryToken] // Siempre es bueno tener esto
        public async Task<IActionResult> RegistrarsePostulante(RegistroUserPostulanteDTO datos) // Cambiado a async Task
        {
            // --- INICIO: VALIDACIONES ---
            // (Tus validaciones existentes para edad, fecha, correo único, nombre de usuario único, contraseña)
            // Ejemplo de validación de correo y nombre de usuario (ya las tienes, solo asegúrate que usen await):
            if (!string.IsNullOrWhiteSpace(datos.email) && await _EmpleoContext.Usuario.AnyAsync(u => u.email == datos.email))
            {
                ModelState.AddModelError("email", "Este correo electrónico ya está registrado.");
            }
            if (!string.IsNullOrWhiteSpace(datos.nombre_usuario) && await _EmpleoContext.Usuario.AnyAsync(u => u.nombre_usuario == datos.nombre_usuario))
            {
                ModelState.AddModelError("nombre_usuario", "Este nombre de usuario ya está en uso.");
            }
            // ... (el resto de tus validaciones)

            if (!ModelState.IsValid)
            {
                // Es mejor usar SelectList para los ViewBags para que puedas preseleccionar valores si es necesario
                ViewBag.Paises = new SelectList(await _EmpleoContext.Pais.OrderBy(p => p.nombre).ToListAsync(), "id_pais", "nombre", datos.id_pais);
                if (datos.id_pais > 0)
                {
                    ViewBag.Provincias = new SelectList(await _EmpleoContext.Provincia.Where(p => p.id_pais == datos.id_pais).OrderBy(p => p.nombre).ToListAsync(), "id_provincia", "nombre", datos.id_provincia);
                }
                else
                {
                    ViewBag.Provincias = new List<SelectListItem>();
                }
                ViewBag.Idiomas = new SelectList(await _EmpleoContext.Idioma.OrderBy(i => i.nombre).ToListAsync(), "id_idioma", "nombre", datos.id_idioma);
                return View("RegistrarsePostulante", datos); // Asegúrate que la vista se llame así
            }
            // --- FIN: VALIDACIONES ---

            try
            {
                Usuario user = new Usuario
                {
                    nombre_usuario = datos.nombre_usuario,
                    email = datos.email,
                    contrasenia =datos.contrasenia,
                    tipo_usuario = 'P',
                    fecha_creacion = DateTime.UtcNow,
                    last_login = DateTime.UtcNow,
                    estado = 'A'
                };
                // No añadir al contexto aún si usas transacción abajo

                Postulante postulante = new Postulante
                {
                    // id_usuario se asignará por EF Core si enlazas la entidad User
                    // o si guardas User primero y luego usas user.id_usuario
                    Usuario = user, // Enlazar entidad
                    nombre = datos.nombre, // O datos.nombre_usuario, según tu lógica
                    apellido = datos.apellido,
                    direccion = datos.direccion,
                    fecha_nacimiento = datos.fecha_nacimiento,
                    id_pais = datos.id_pais,
                    id_provincia = datos.id_provincia,
                    id_idioma = datos.id_idioma
                };
                // No añadir al contexto aún

                Curriculum curriculum = new Curriculum
                {
                    // id_postulante se asignará por EF Core
                    Postulante = postulante, // Enlazar entidad
                    fecha = DateTime.UtcNow
                };
                // No añadir al contexto aún

                // Utilizar una transacción para asegurar la atomicidad de todas las operaciones
                using (var transaction = await _EmpleoContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _EmpleoContext.Usuario.Add(user);
                        await _EmpleoContext.SaveChangesAsync(); // Guardar Usuario para obtener su ID

                        // Postulante ya está enlazado a user, EF Core manejará la FK
                        _EmpleoContext.Postulante.Add(postulante);
                        await _EmpleoContext.SaveChangesAsync(); // Guardar Postulante para obtener su ID

                        // Curriculum ya está enlazado a postulante
                        _EmpleoContext.Curriculum.Add(curriculum);
                        await _EmpleoContext.SaveChangesAsync(); // Guardar Curriculum para obtener su ID

                        // Lógica para Institución "Autodidacta" e Idioma_Curriculum
                        if (postulante.id_idioma != 0)
                        {
                            string nombreInstitucionAutodidacta = "Autodidacta";

                            // Buscar la institución "Autodidacta" global (donde id_postulante es NULL)
                            Institucion institucionGlobalAutodidacta = await _EmpleoContext.Institucion
                                .FirstOrDefaultAsync(i => i.nombre == nombreInstitucionAutodidacta && i.id_postulante == null);

                            if (institucionGlobalAutodidacta == null) // Si no existe la global, crearla
                            {
                                institucionGlobalAutodidacta = new Institucion
                                {
                                    nombre = nombreInstitucionAutodidacta,
                                    // Debes proveer id_pais e id_provincia ya que son NOT NULL en tu tabla Institucion.
                                    // Usa los del postulante actual o IDs de un país/provincia "genéricos" o "No Aplica".
                                    id_pais = postulante.id_pais,
                                    id_provincia = postulante.id_provincia,
                                    id_postulante = null // MUY IMPORTANTE: id_postulante es NULL para la global
                                };
                                _EmpleoContext.Institucion.Add(institucionGlobalAutodidacta);
                                await _EmpleoContext.SaveChangesAsync(); // Guardar la nueva institución global para obtener su ID
                            }

                            // Añadir la entrada a Idioma_Curriculum usando la institucionAutodidacta (global)
                            // Verificar si ya existe esta combinación exacta para este currículum (poco probable en registro)
                            bool idiomaCvYaExiste = await _EmpleoContext.Idioma_Curriculum
                                .AnyAsync(ic => ic.id_curriculum == curriculum.id_curriculum &&
                                               ic.id_idioma == postulante.id_idioma &&
                                               ic.id_institucion == institucionGlobalAutodidacta.id_institucion);

                            if (!idiomaCvYaExiste)
                            {
                                Idioma_Curriculum nuevaEntradaIdiomaCv = new Idioma_Curriculum
                                {
                                    // id_curriculum y id_institucion se tomarán de las entidades enlazadas
                                    Curriculum = curriculum,
                                    Institucion = institucionGlobalAutodidacta,
                                    id_idioma = postulante.id_idioma,
                                    fecha = DateTime.UtcNow
                                };
                                _EmpleoContext.Idioma_Curriculum.Add(nuevaEntradaIdiomaCv);
                                await _EmpleoContext.SaveChangesAsync(); // Guardar la entrada de Idioma_Curriculum
                            }
                        }

                        await transaction.CommitAsync(); // Confirmar todos los cambios si todo fue bien
                        TempData["SuccessMessage"] = "¡Registro exitoso! Ahora puede iniciar sesión.";
                        return RedirectToAction("Login", "Login");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Revertir cambios en caso de error
                        System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en RegistrarsePostulante (transacción): {ex.ToString()}");
                        ModelState.AddModelError("", "Ocurrió un error inesperado durante el registro.");
                        // Repoblar ViewBags
                        ViewBag.Paises = new SelectList(await _EmpleoContext.Pais.ToListAsync(), "id_pais", "nombre", datos.id_pais);
                        // ... (repoblar otros ViewBags)
                        return View("RegistrarsePostulante", datos);
                    }
                }
            }
            catch (Exception ex) // Captura para errores antes de iniciar la transacción (ej. validación)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCIÓN en RegistrarsePostulante (fuera de transacción): {ex.ToString()}");
                ModelState.AddModelError("", "Ocurrió un error inesperado durante el registro.");
                ViewBag.Paises = new SelectList(await _EmpleoContext.Pais.ToListAsync(), "id_pais", "nombre", datos.id_pais);
                // ... (repoblar otros ViewBags)
                return View("RegistrarsePostulante", datos);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarseEmpresa([FromForm] RegistroUserEmpresaDTO datos)
        {
            if (!string.IsNullOrWhiteSpace(datos.email) &&
                await _EmpleoContext.Usuario.AnyAsync(u => u.email == datos.email))
            {
                ModelState.AddModelError("email", "Este correo electrónico ya está registrado.");
            }

            // El nombre de la empresa se usará como nombre_usuario. Validar su unicidad en la tabla Usuario.
            if (!string.IsNullOrWhiteSpace(datos.nombre) &&
                await _EmpleoContext.Usuario.AnyAsync(u => u.nombre_usuario == datos.nombre))
            {
                ModelState.AddModelError("nombre", "Este nombre de empresa ya se encuentra en uso como identificador de usuario. Por favor, elija un nombre ligeramente diferente.");
            }

            // Validar que el sector sea uno de los permitidos si tienes un CHECK constraint.
            var sectoresPermitidos = new List<string> { "Primario", "Secundario", "Terciario", "Cuaternario", "Quinario" };
            if (!string.IsNullOrWhiteSpace(datos.sector_empresa) && !sectoresPermitidos.Contains(datos.sector_empresa, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("sector_empresa", "El sector de trabajo no es válido. Sectores permitidos: " + string.Join(", ", sectoresPermitidos) + ".");
            }


            if (!ModelState.IsValid)
            {
                // La vista RegistrarseEmpresa.cshtml debe usar @model RegistroUserEmpresaDTO
                // y tener los asp-validation-for para mostrar estos errores.
                return View("RegistrarseEmpresa", datos);
            }

            try
            {
                Usuario user = new Usuario
                {
                    nombre_usuario = datos.nombre, // <--- ASIGNACIÓN QUE CREO QUE NO ESTABA
                    email = datos.email,
                    contrasenia = datos.contrasenia,
                    tipo_usuario = 'E',       
                    fecha_creacion = DateTime.UtcNow, 
                    last_login = DateTime.UtcNow,
                    estado = 'A'                    
                };

                _EmpleoContext.Usuario.Add(user);
                await _EmpleoContext.SaveChangesAsync();

                Empresa empresa = new Empresa
                {
                    id_usuario = user.id_usuario,
                    nombre = datos.nombre, 
                    direccion = datos.direccion,
                    descripcion_empresa = datos.descripcion_empresa,
                    sector_empresa = datos.sector_empresa
                };
                _EmpleoContext.Empresa.Add(empresa);
                await _EmpleoContext.SaveChangesAsync();

                TempData["SuccessMessage"] = "¡Empresa registrada exitosamente! Ahora puede iniciar sesión.";
                return RedirectToAction("Login", "Login");
            }
            catch (DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"DbUpdateException en RegistrarseEmpresa: {dbEx.ToString()}");
                if (dbEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception (RegistrarseEmpresa): {dbEx.InnerException.ToString()}");
                }
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos. Por favor, verifique la información e intente de nuevo.");
                return View("RegistrarseEmpresa", datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción general en RegistrarseEmpresa: {ex.ToString()}");
                ModelState.AddModelError("", "Ocurrió un error inesperado. Por favor, intente de nuevo más tarde.");
                return View("RegistrarseEmpresa", datos);
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
