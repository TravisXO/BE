using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Cart()
        {
            return View();
        }
    }
}
