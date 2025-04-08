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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Obtener el id_usuario desde la sesión
            int? idUsuario = HttpContext.Session.GetInt32("id_usuario");

            // Verificar si el idUsuario es null
            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Account"); // Redirige al login si no hay sesión
            }

            // Consulta para obtener el perfil y los datos relacionados
            var perfil = (from p in _context.Postulante
                          where p.id_postulante == id && p.id_usuario == idUsuario
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
                return NotFound(); // Si no se encuentra el perfil, muestra una página de error 404
            }

            // Crear el modelo con los datos cargados
            var model = new PostulanteViewModel
            {
                Postulante = perfil.Postulante,
                Usuario = perfil.Usuario,
                Contacto = perfil.Contacto,
                Curriculum = perfil.Curriculum,
                Formaciones = perfil.Formaciones,
                Experiencias = perfil.Experiencias
            };

            // Retornar la vista con el modelo cargado
            return View(model);
        }

    }
}
