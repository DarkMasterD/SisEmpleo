using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SisEmpleo.Models;
using SisEmpleo.Services;

namespace SisEmpleo.Controllers
{
    [AutenticacionPostulante]
    public class PerfilController : Controller
    {
        private readonly EmpleoContext _context;

        public PerfilController(EmpleoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Obtener el id_usuario desde la sesión
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

            // Verificar si el idUsuario es null
            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account"); // Redirige al login si no hay sesión
            }

            // Realizar la consulta para obtener el perfil del postulante
            var perfil = (from p in _context.Postulante
                          where p.id_usuario == idUsuario
                          join u in _context.Usuario on p.id_usuario equals u.id_usuario
                          join c in _context.Contacto on u.id_usuario equals c.id_usuario into contactos
                          from contacto in contactos.DefaultIfEmpty()
                          join cv in _context.Curriculum on p.id_postulante equals cv.id_postulante into curricula
                          from curriculum in curricula.DefaultIfEmpty()
                          select new PostulanteViewModel
                          {
                              Postulante = p,
                              Usuario = u,
                              Contacto = contacto,
                              Curriculum = curriculum,
                              Formaciones = curriculum != null ?
                                  (from fa in _context.FormacionAcademica
                                   where fa.id_curriculum == curriculum.id_curriculum
                                   join especialidad in _context.Especialidad on fa.id_especialidad equals especialidad.id_especialidad
                                   join titulo in _context.Titulo on fa.id_titulo equals titulo.id_titulo
                                   join institucion in _context.Institucion on fa.id_institucion equals institucion.id_institucion
                                   select new FormacionAcademica
                                   {
                                       id_formacionacademica = fa.id_formacionacademica,
                                       id_curriculum = fa.id_curriculum,
                                       id_especialidad = fa.id_especialidad,
                                       id_titulo = fa.id_titulo,
                                       id_institucion = fa.id_institucion,
                                       fecha = fa.fecha,
                                       Especialidad = especialidad,
                                       Titulo = titulo,
                                       Institucion = institucion
                                   }).ToList() :
                                  new List<FormacionAcademica>(),
                              Experiencias = curriculum != null ?
                                  (from ep in _context.ExperienciaProfesional
                                   where ep.id_curriculum == curriculum.id_curriculum
                                   select ep).ToList() :
                                  new List<ExperienciaProfesional>()
                          }).FirstOrDefault();


            // Verificar si no se encontró el perfil
            if (perfil == null)
            {
                return NotFound();  // Si no se encuentra el perfil, muestra una página de error 404
            }

            // Retornar la vista con el perfil cargado
            return View("/Views/Perfil/Perfil.cshtml",perfil);  // Se pasa el modelo directamente a la vista
        }

        public IActionResult EditarPerfil()
        {
            // Obtener el id_usuario desde la sesión
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

            // Verificar si el idUsuario es null
            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account"); // Redirige al login si no hay sesión
            }

            // Obtener el perfil y devolver la vista de edición
            var perfil = (from p in _context.Postulante
                          where p.id_usuario == idUsuario
                          join u in _context.Usuario on p.id_usuario equals u.id_usuario
                          join c in _context.Contacto on u.id_usuario equals c.id_usuario into contactos
                          from contacto in contactos.DefaultIfEmpty()
                          join cv in _context.Curriculum on p.id_postulante equals cv.id_postulante into curricula
                          from curriculum in curricula.DefaultIfEmpty()
                          select new PostulanteViewModel
                          {
                              Postulante = p,
                              Usuario = u,
                              Contacto = contacto,
                              Curriculum = curriculum,
                              Formaciones = curriculum != null ?
                                  (from fa in _context.FormacionAcademica
                                   where fa.id_curriculum == curriculum.id_curriculum
                                   join especialidad in _context.Especialidad on fa.id_especialidad equals especialidad.id_especialidad
                                   join titulo in _context.Titulo on fa.id_titulo equals titulo.id_titulo
                                   join institucion in _context.Institucion on fa.id_institucion equals institucion.id_institucion
                                   select new FormacionAcademica
                                   {
                                       id_formacionacademica = fa.id_formacionacademica,
                                       id_curriculum = fa.id_curriculum,
                                       id_especialidad = fa.id_especialidad,
                                       id_titulo = fa.id_titulo,
                                       id_institucion = fa.id_institucion,
                                       fecha = fa.fecha,
                                       Especialidad = especialidad,
                                       Titulo = titulo,
                                       Institucion = institucion
                                   }).ToList() :
                                  new List<FormacionAcademica>(),
                              Experiencias = curriculum != null ?
                                  (from ep in _context.ExperienciaProfesional
                                   where ep.id_curriculum == curriculum.id_curriculum
                                   select ep).ToList() :
                                  new List<ExperienciaProfesional>()
                          }).FirstOrDefault();

            // Verificar si no se encontró el perfil
            if (perfil == null)
            {
                return NotFound();  // Si no se encuentra el perfil, muestra una página de error 404
            }

            return View("/Views/Perfil/EditarPerfil.cshtml", perfil);  // Mostrar la vista de edición
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarPerfil(PostulanteViewModel model)
        {
            
            

                // Obtener el id_usuario desde la sesión
                int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

                // Verificar si el idUsuario es null
                if (idUsuario == null)
                {
                    return RedirectToAction("Login", "Account"); // Redirige al login si no hay sesión
                }

                var postulante = (from p in _context.Postulante
                                  where p.id_usuario == idUsuario
                                  select p).FirstOrDefault();


                // Actualizar los datos del postulante
                postulante.nombre = model.Postulante.nombre;
                postulante.direccion = model.Postulante.direccion;

                // Buscar y actualizar los datos de contacto
                var contacto = _context.Contacto
                    .FirstOrDefault(c => c.id_usuario == idUsuario);

                if (contacto != null)
                {
                    contacto.email = model.Contacto?.email;
                    contacto.telefono = model.Contacto?.telefono;
                }

                var curriculum = (from c in _context.Curriculum
                                  where c.id_postulante == postulante.id_postulante
                                  select c).FirstOrDefault();


                if (curriculum != null)
                {
                    curriculum.habilidades = model.Curriculum?.habilidades;
                    curriculum.idiomas = model.Curriculum?.idiomas;
                    curriculum.certificaciones = model.Curriculum?.certificaciones;
                }

                // Guardar los cambios en la base de datos
                _context.SaveChanges();

            // Redirigir a la vista de perfil actualizado
            return RedirectToAction("Index");
            //return View(model); // Si el modelo no es válido, mostrar de nuevo el formulario con errores
        }


    }

}
