using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Shop()
        {
            return View();
        }
    }
}
