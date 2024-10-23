using Microsoft.AspNetCore.Mvc;

namespace IdentityDemo.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


    }
}
