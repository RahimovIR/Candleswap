using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebSocket.Uniswap.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : Controller
    {
        [HttpGet("/socket")]
        public async Task SendMessage([FromQuery] string message)
        {

        }
    }
}
