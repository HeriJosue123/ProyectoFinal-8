using Microsoft.AspNetCore.Mvc;

namespace ContactManagerWeb.Controllers
{
    public class ContactosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
