using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using HomeControl.Common.Messaging.Messages;
using HomeControl.Common.Hardware.Interfaces;

namespace HomeControl.Common.Hardware
{
    public class RelayBoard : IRelayBoard
    {
        private readonly IMediator _mediator;
        private readonly IList<IRelay> _relayList;

        public RelayBoard(IMediator mediator, IList<IRelay> relayList)
        {
            _mediator = mediator;
            _relayList = relayList;
        }

        public bool Query(int pinNumber)
        {
            var relay = _relayList.SingleOrDefault(r => r.GetPinNumber() == pinNumber);
            if (relay != null)
            {
                return relay.Query();
            }

            throw new ArgumentOutOfRangeException(nameof(pinNumber));
        }

        public void On(int pinNumber, bool reportUpdate = true)
        {
            var relay = _relayList.SingleOrDefault(r => r.GetPinNumber() == pinNumber);
            if (relay != null)
            {
                relay.On();

                if (reportUpdate)
                {
                    ReportUpdate(pinNumber);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }
        }
        
        public void Off(int pinNumber, bool reportUpdate = true)
        {
            var relay = _relayList.SingleOrDefault(r => r.GetPinNumber() == pinNumber);
            if (relay != null)
            {
                relay.Off();

                if (reportUpdate)
                {
                    ReportUpdate(pinNumber);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }
        }

        public void Toggle(int pinNumber, bool reportUpdate = true)
        {
            var relay = _relayList.SingleOrDefault(r => r.GetPinNumber() == pinNumber);
            if (relay != null)
            {
                relay.Toggle();

                if (reportUpdate)
                {
                    ReportUpdate(pinNumber);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }
        }

        public void AllOn() => _relayList.ToList().ForEach(relay => On(relay.GetPinNumber()));
        public void AllOff() => _relayList.ToList().ForEach(relay => Off(relay.GetPinNumber()));

        private void ReportUpdate(int pinNumber) => Task.Run(async () => await _mediator.Publish(new PinStateChanged(pinNumber, Query(pinNumber))));
    }
}
