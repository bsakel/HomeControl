namespace HomeControl.Common.Hardware.Interfaces
{
    public interface IRelay
    {
        string GetName();
        int GetPinNumber();

        bool Query();
        void On();
        void Off();
        void Toggle();
    }
}
