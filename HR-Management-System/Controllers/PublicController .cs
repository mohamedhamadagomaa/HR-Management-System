using Microsoft.AspNetCore.Mvc;

namespace LeavePayrollSystem.Web.Controllers
{
    public class PublicController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}