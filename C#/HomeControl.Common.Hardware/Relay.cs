using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;
using HomeControl.Common.Hardware.Interfaces;

namespace HomeControl.Common.Hardware
{
    public class Relay : IRelay
    {
        private readonly GpioPin _gpioPin;
        private readonly int _pinNumber;

        public Relay(int pinNumber)
        {
            _pinNumber = pinNumber;
            _gpioPin = Pi.Gpio[pinNumber];

            _gpioPin.PinMode = GpioPinDriveMode.Output;
            _gpioPin.Write(GpioPinValue.Low);
        }

        public string GetName() => _pinNumber.ToString();
        public int GetPinNumber() => _pinNumber;

        public static IRelay Initialize(int pinNumber) => new Relay(pinNumber);

        public bool Query() => _gpioPin.Read();
        public void On() => _gpioPin.Write(GpioPinValue.High);
        public void Off() => _gpioPin.Write(GpioPinValue.Low);
        public void Toggle()
        {
            if (Query())
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
