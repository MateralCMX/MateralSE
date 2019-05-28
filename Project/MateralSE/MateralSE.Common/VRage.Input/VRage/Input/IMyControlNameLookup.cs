namespace VRage.Input
{
    using System;

    public interface IMyControlNameLookup
    {
        string GetKeyName(MyKeys key);
        string GetName(MyJoystickAxesEnum joystickAxis);
        string GetName(MyJoystickButtonsEnum joystickButton);
        string GetName(MyMouseButtonsEnum button);

        string UnassignedText { get; }
    }
}

