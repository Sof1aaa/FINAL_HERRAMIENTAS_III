using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HERRAMIENTAS.Controllers
{
    [Authorize] // 🔥 protege todo el controller
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Dashboard
        }
    }
}