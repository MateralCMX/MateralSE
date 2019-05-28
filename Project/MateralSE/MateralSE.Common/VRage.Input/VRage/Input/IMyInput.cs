namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public interface IMyInput
    {
        event Action<bool> JoystickConnected;

        void AddDefaultControl(MyStringId stringId, MyControl control);
        void ClearBlacklist();
        int DeltaMouseScrollWheelValue();
        void EnableInput(bool enable);
        List<string> EnumerateJoystickNames();
        void GetActualJoystickState(StringBuilder text);
        MyControl GetControl(MyKeys key);
        MyControl GetControl(MyMouseButtonsEnum button);
        MyControl GetGameControl(MyStringId controlEnum);
        float GetGameControlAnalogState(MyStringId controlEnum);
        DictionaryValuesReader<MyStringId, MyControl> GetGameControlsList();
        float GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis);
        float GetJoystickDeadzone();
        float GetJoystickExponent();
        float GetJoystickSensitivity();
        string GetKeyName(MyKeys key);
        void GetListOfPressedKeys(List<MyKeys> keys);
        void GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result);
        Vector2 GetMouseAreaSize();
        Vector2 GetMousePosition();
        float GetMouseSensitivity();
        int GetMouseX();
        int GetMouseXForGamePlay();
        float GetMouseXForGamePlayF();
        bool GetMouseXInversion();
        int GetMouseY();
        int GetMouseYForGamePlay();
        float GetMouseYForGamePlayF();
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
        bool IsControlBlocked(MyStringId controlEnum);
        bool IsEnabled();
        bool IsGameControlPressed(MyStringId controlEnum);
        bool IsGameControlReleased(MyStringId controlEnum);
        bool IsGamepadKeyLeftPressed();
        bool IsGamepadKeyRightPressed();
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
        bool IsLeftCtrlKeyPressed();
        bool IsLeftMousePressed();
        bool IsLeftMouseReleased();
        bool IsMiddleMousePressed();
        bool IsMouseButtonValid(MyMouseButtonsEnum button);
        bool IsMousePressed(MyMouseButtonsEnum button);
        bool IsMouseReleased(MyMouseButtonsEnum button);
        bool IsNewButtonPressed(MySharedButtonsEnum button);
        bool IsNewButtonReleased(MySharedButtonsEnum button);
        bool IsNewGameControlJoystickOnlyPressed(MyStringId controlId);
        bool IsNewGameControlPressed(MyStringId controlEnum);
        bool IsNewGameControlReleased(MyStringId controlEnum);
        bool IsNewGamepadKeyDownPressed();
        bool IsNewGamepadKeyUpPressed();
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
        bool IsRightCtrlKeyPressed();
        bool IsRightMousePressed();
        bool IsSecondaryButtonPressed();
        bool IsSecondaryButtonReleased();
        bool IsXButton1MousePressed();
        bool IsXButton2MousePressed();
        void LoadContent(IntPtr windowHandle);
        void LoadData(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons);
        int MouseScrollWheelValue();
        int PreviousMouseScrollWheelValue();
        void RevertChanges();
        void RevertToDefaultControls();
        void SaveControls(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons);
        void SetControlBlock(MyStringId controlEnum, bool block = false);
        void SetJoystickDeadzone(float newDeadzone);
        void SetJoystickExponent(float newExponent);
        void SetJoystickSensitivity(float newSensitivity);
        void SetMousePosition(int x, int y);
        void SetMouseSensitivity(float sensitivity);
        void SetMouseXInversion(bool inverted);
        void SetMouseYInversion(bool inverted);
        void TakeSnapshot();
        void UnloadData();
        bool Update(bool gameFocused);
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

        string JoystickInstanceName { get; set; }

        IntPtr WindowHandle { get; }

        ListReader<char> TextInput { get; }

        bool JoystickAsMouse { get; set; }

        bool IsJoystickLastUsed { get; }

        bool ENABLE_DEVELOPER_KEYS { get; }
    }
}

