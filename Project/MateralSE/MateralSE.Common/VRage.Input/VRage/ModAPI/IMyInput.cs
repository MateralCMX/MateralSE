namespace VRage.ModAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public interface IMyInput
    {
        event Action<bool> JoystickConnected;

        int DeltaMouseScrollWheelValue();
        List<string> EnumerateJoystickNames();
        IMyControl GetControl(MyKeys key);
        IMyControl GetControl(MyMouseButtonsEnum button);
        IMyControl GetGameControl(MyStringId controlEnum);
        float GetGameControlAnalogState(MyStringId controlEnum);
        float GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis);
        string GetKeyName(MyKeys key);
        void GetListOfPressedKeys(List<MyKeys> keys);
        void GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result);
        Vector2 GetMouseAreaSize();
        Vector2 GetMousePosition();
        float GetMouseSensitivity();
        int GetMouseX();
        int GetMouseXForGamePlay();
        bool GetMouseXInversion();
        int GetMouseY();
        int GetMouseYForGamePlay();
        bool GetMouseYInversion();
        string GetName(MyJoystickAxesEnum joystickAxis);
        string GetName(MyJoystickButtonsEnum joystickButton);
        string GetName(MyMouseButtonsEnum mouseButton);
        void GetPressedKeys(List<MyKeys> keys);
        string GetUnassignedName();
        bool IsAnyAltKeyPressed();
        bool IsAnyCtrlKeyPressed();
        bool IsAnyKeyPress();
        bool IsAnyMouseOrJoystickPressed();
        bool IsAnyMousePressed();
        bool IsAnyNewMouseOrJoystickPressed();
        bool IsAnyNewMousePressed();
        bool IsAnyShiftKeyPressed();
        bool IsButtonPressed(MySharedButtonsEnum button);
        bool IsButtonReleased(MySharedButtonsEnum button);
        bool IsGameControlPressed(MyStringId controlEnum);
        bool IsGameControlReleased(MyStringId controlEnum);
        bool IsJoystickAxisNewPressed(MyJoystickAxesEnum axis);
        bool IsJoystickAxisPressed(MyJoystickAxesEnum axis);
        bool IsJoystickAxisValid(MyJoystickAxesEnum axis);
        bool IsJoystickButtonNewPressed(MyJoystickButtonsEnum button);
        bool IsJoystickButtonPressed(MyJoystickButtonsEnum button);
        bool IsJoystickButtonValid(MyJoystickButtonsEnum button);
        bool IsJoystickConnected();
        bool IsKeyDigit(MyKeys key);
        bool IsKeyPress(MyKeys key);
        bool IsKeyValid(MyKeys key);
        bool IsLeftMousePressed();
        bool IsLeftMouseReleased();
        bool IsMiddleMousePressed();
        bool IsMouseButtonValid(MyMouseButtonsEnum button);
        bool IsMousePressed(MyMouseButtonsEnum button);
        bool IsMouseReleased(MyMouseButtonsEnum button);
        bool IsNewButtonPressed(MySharedButtonsEnum button);
        bool IsNewButtonReleased(MySharedButtonsEnum button);
        bool IsNewGameControlPressed(MyStringId controlEnum);
        bool IsNewGameControlReleased(MyStringId controlEnum);
        bool IsNewJoystickAxisReleased(MyJoystickAxesEnum axis);
        bool IsNewJoystickButtonReleased(MyJoystickButtonsEnum button);
        bool IsNewKeyPressed(MyKeys key);
        bool IsNewKeyReleased(MyKeys key);
        bool IsNewLeftMousePressed();
        bool IsNewLeftMouseReleased();
        bool IsNewMiddleMousePressed();
        bool IsNewMiddleMouseReleased();
        bool IsNewMousePressed(MyMouseButtonsEnum button);
        bool IsNewPrimaryButtonPressed();
        bool IsNewPrimaryButtonReleased();
        bool IsNewRightMousePressed();
        bool IsNewRightMouseReleased();
        bool IsNewSecondaryButtonPressed();
        bool IsNewSecondaryButtonReleased();
        bool IsNewXButton1MousePressed();
        bool IsNewXButton1MouseReleased();
        bool IsNewXButton2MousePressed();
        bool IsNewXButton2MouseReleased();
        bool IsPrimaryButtonPressed();
        bool IsPrimaryButtonReleased();
        bool IsRightMousePressed();
        bool IsSecondaryButtonPressed();
        bool IsSecondaryButtonReleased();
        bool IsXButton1MousePressed();
        bool IsXButton2MousePressed();
        int MouseScrollWheelValue();
        int PreviousMouseScrollWheelValue();
        bool WasKeyPress(MyKeys key);
        bool WasMiddleMousePressed();
        bool WasMiddleMouseReleased();
        bool WasRightMousePressed();
        bool WasRightMouseReleased();
        bool WasXButton1MousePressed();
        bool WasXButton1MouseReleased();
        bool WasXButton2MousePressed();
        bool WasXButton2MouseReleased();

        bool IsCapsLock { get; }

        string JoystickInstanceName { get; }

        ListReader<char> TextInput { get; }

        bool JoystickAsMouse { get; }

        bool IsJoystickLastUsed { get; }
    }
}

