using Microsoft.AspNetCore.Mvc;

namespace SocketServer.Controllers
{
    public class ErrorController : Controller
    {        
        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
