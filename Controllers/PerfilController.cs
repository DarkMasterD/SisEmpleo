    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using SisEmpleo.Models;
    using SisEmpleo.Services;

    namespace SisEmpleo.Controllers
    {
    
        public class PerfilController : Controller
        {
            private readonly EmpleoContext _context;

            public PerfilController(EmpleoContext context)
            {
                _context = context;
            }

            public IActionResult Index()
            {
                // Obtener el id del usuario de la sesión
                int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

                if (idUsuario == null)
                {
                    // Usuario no logueado, redirigir a login o mostrar error
                    return RedirectToAction("Login", "Cuenta");
                }

                var usuario =(from u in _context.Usuario
                              join p in _context.Postulante 
                              on u.id_usuario equals p.id_usuario
                              join c in _context.Contacto
                              on u.id_usuario equals c.id_usuario
                              where u.id_usuario == idUsuario
                              select new { 
                                Nombre = p.nombre,
                                Apellido = p.apellido,
                                Correo = u.email,
                                Telefono = c.telefono,

                              })

                var idiomas = (from p in _context.Postulante
                               join i in _context.Idioma
                               on p.id_idioma equals i.id_idioma
                               where p.id_usuario == idUsuario
                               select i.nombre).ToList();

                ViewBag.Idioma = idiomas ?? new List<string>();

                // Retornar la vista con el modelo
                return View();
            }



        }

    }
