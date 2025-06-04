using Microsoft.AspNetCore.Mvc;

namespace SisEmpleo.Controllers
{
    public class AnalisisEmpresarialController : Controller
    {
        public IActionResult AnalisisEmpresarial()
        {
            return View("~/Views/AnalisisEmpresarial.cshtml");
        }
    }
}
