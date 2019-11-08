using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using HomeControl.Common.Hardware.Interfaces;

namespace HomeControl.RelayRpi.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private readonly IRelayBoard _relayBoard;
        private readonly IList<IRelay> _relayList;

        public StatusController(IRelayBoard relayBoard, IList<IRelay> relayList)
        {
            _relayBoard = relayBoard;
            _relayList = relayList;
        }

        [HttpGet]
        public IActionResult Get() 
        {
            var response = new Dictionary<string, string>();
            foreach (var relay in _relayList)
            {
                var relayPinNumber = relay.GetPinNumber();
                response.Add(relay.GetName(), _relayBoard.Query(relayPinNumber).ToString());
            }

            return Ok(response);
        }

        [HttpGet("{pinNumber}")]
        public IActionResult Get(int pinNumber) => Ok(_relayBoard.Query(pinNumber));

        //TODO: Add Set
    }
}
