using HomeControl.Common.Hardware.Interfaces;

namespace HomeControl.Common.Hardware
{
    public class FakeRelay : IRelay
    {
        private readonly int _pinNumber;
        private bool _state; 

        public FakeRelay(int pinNumber)
        {
            _pinNumber = pinNumber;
            _state = false;
        }

        public string GetName() => _pinNumber.ToString();
        public int GetPinNumber() => _pinNumber;

        public static IRelay Initialize(int pinNumber) => new FakeRelay(pinNumber);

        public bool Query() => _state;
        public void On() => _state = true;
        public void Off() => _state = false;
        public void Toggle()
        {
            if(Query())
            {
                Off();
            }
            else
            {
                On();
            }
        }
    }
}
