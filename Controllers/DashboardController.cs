using Microsoft.AspNetCore.Mvc;

namespace SisEmpleo.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Dashboard.cshtml");
        }
    }
}
