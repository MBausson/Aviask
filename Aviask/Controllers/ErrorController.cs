using Microsoft.AspNetCore.Mvc;

namespace Aviask.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("Error404");
            }

            return RedirectToRoute("/");
        }
    }
}
