namespace HomeControl.Common.Hardware.Interfaces
{
    public interface IRelayBoard
    {
        bool Query(int pinNumber);
        void On(int pinNumber, bool reportUpdate = true);
        void Off(int pinNumber, bool reportUpdate = true);
        void Toggle(int pinNumber, bool reportUpdate = true);

        void AllOn();
        void AllOff();
    }
}
