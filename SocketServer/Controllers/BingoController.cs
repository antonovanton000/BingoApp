using Microsoft.AspNetCore.Mvc;

namespace SocketServer.Controllers
{
    public class BingoController : Controller
    {
        public IActionResult Spectate(string id)
        {
            ViewBag.RoomId = id;
            return View();
        }
    }
}
