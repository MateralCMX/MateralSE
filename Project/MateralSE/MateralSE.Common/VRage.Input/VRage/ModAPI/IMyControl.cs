namespace VRage.ModAPI
{
    using System;
    using VRage.Input;
    using VRage.Utils;

    public interface IMyControl
    {
        float GetAnalogState();
        string GetControlButtonName(MyGuiInputDeviceEnum deviceType);
        MyStringId? GetControlDescription();
        MyStringId GetControlName();
        MyGuiControlTypeEnum GetControlTypeEnum();
        MyStringId GetGameControlEnum();
        MyKeys GetKeyboardControl();
        MyMouseButtonsEnum GetMouseControl();
        MyKeys GetSecondKeyboardControl();
        bool IsControlAssigned();
        bool IsJoystickPressed();
        bool IsNewJoystickPressed();
        bool IsNewJoystickReleased();
        bool IsNewPressed();
        bool IsNewReleased();
        bool IsPressed();
    }
}

