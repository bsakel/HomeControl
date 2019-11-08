using MediatR;

namespace HomeControl.Common.Messaging.Messages
{
    public class PinStateChanged : INotification 
    {
        public int PinNumber { get; }
        public bool NewState { get; }

        public PinStateChanged(int pinNumber, bool newState)
        {
            PinNumber = pinNumber;
            NewState = newState;
        }
    }
}
