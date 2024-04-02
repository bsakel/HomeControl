//using HomeControl.Actors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HomeControl.TelegramUpdater.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramWebhookController : ControllerBase
    {
        private readonly ILogger<TelegramWebhookController> _logger;
        //private readonly IActorBridge _bridge;

        public TelegramWebhookController(ILogger<TelegramWebhookController> logger)
        {
            _logger = logger;
        }


        //public TelegramWebhookController(ILogger<TelegramWebhookController> logger, IActorBridge bridge)
        //{
        //    _logger = logger;
        //    _bridge = bridge;
        //}

        //[HttpPost]
        //public void Post()
        //{
        //    _bridge.Tell(Request.Body.ToString());
        //}


        //[HttpGet]
        //public RedirectResult Get()
        //{
        //    _bridge.Tell("hello!");

        //    return Redirect("http://localhost:4040/inspect/http");
        //}
    }
}
