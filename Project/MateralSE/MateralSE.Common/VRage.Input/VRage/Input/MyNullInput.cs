namespace VRage.Input
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.ModAPI;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;

    public class MyNullInput : VRage.ModAPI.IMyInput, VRage.Input.IMyInput
    {
        private MyControl m_nullControl;
        private List<char> m_listChars;
        private List<string> m_listStrings;

        event Action<bool> VRage.Input.IMyInput.JoystickConnected
        {
            add
            {
            }
            remove
            {
            }
        }

        event Action<bool> VRage.ModAPI.IMyInput.JoystickConnected
        {
            add
            {
                this.JoystickConnected += value;
            }
            remove
            {
                this.JoystickConnected -= value;
            }
        }

        public MyNullInput()
        {
            MyStringId controlId = new MyStringId();
            controlId = new MyStringId();
            MyMouseButtonsEnum? defaultControlMouse = null;
            MyKeys? defaultControlKey = null;
            MyStringId? helpText = null;
            defaultControlKey = null;
            helpText = null;
            this.m_nullControl = new MyControl(controlId, controlId, MyGuiControlTypeEnum.General, defaultControlMouse, defaultControlKey, helpText, defaultControlKey, helpText);
            this.m_listChars = new List<char>();
            this.m_listStrings = new List<string>();
        }

        public void AddDefaultControl(MyStringId stringId, MyControl control)
        {
        }

        public void ClearBlacklist()
        {
        }

        public void EnableInput(bool enable)
        {
        }

        public bool IsControlBlocked(MyStringId controlEnum) => 
            false;

        public bool IsEnabled() => 
            false;

        public string ReplaceControlsInText(string text)
        {
            throw new NotImplementedException();
        }

        public void SetControlBlock(MyStringId controlEnum, bool block = false)
        {
        }

        int VRage.Input.IMyInput.DeltaMouseScrollWheelValue() => 
            0;

        List<string> VRage.Input.IMyInput.EnumerateJoystickNames() => 
            this.m_listStrings;

        void VRage.Input.IMyInput.GetActualJoystickState(StringBuilder text)
        {
        }

        MyControl VRage.Input.IMyInput.GetControl(MyKeys key) => 
            null;

        MyControl VRage.Input.IMyInput.GetControl(MyMouseButtonsEnum button) => 
            null;

        MyControl VRage.Input.IMyInput.GetGameControl(MyStringId controlEnum) => 
            this.m_nullControl;

        float VRage.Input.IMyInput.GetGameControlAnalogState(MyStringId controlEnum) => 
            0f;

        DictionaryValuesReader<MyStringId, MyControl> VRage.Input.IMyInput.GetGameControlsList() => 
            0;

        float VRage.Input.IMyInput.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => 
            0f;

        float VRage.Input.IMyInput.GetJoystickDeadzone() => 
            0f;

        float VRage.Input.IMyInput.GetJoystickExponent() => 
            0f;

        float VRage.Input.IMyInput.GetJoystickSensitivity() => 
            0f;

        string VRage.Input.IMyInput.GetKeyName(MyKeys key) => 
            "";

        void VRage.Input.IMyInput.GetListOfPressedKeys(List<MyKeys> keys)
        {
        }

        void VRage.Input.IMyInput.GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result)
        {
        }

        Vector2 VRage.Input.IMyInput.GetMouseAreaSize() => 
            Vector2.Zero;

        Vector2 VRage.Input.IMyInput.GetMousePosition() => 
            Vector2.Zero;

        float VRage.Input.IMyInput.GetMouseSensitivity() => 
            0f;

        int VRage.Input.IMyInput.GetMouseX() => 
            0;

        int VRage.Input.IMyInput.GetMouseXForGamePlay() => 
            0;

        float VRage.Input.IMyInput.GetMouseXForGamePlayF() => 
            0f;

        bool VRage.Input.IMyInput.GetMouseXInversion() => 
            false;

        int VRage.Input.IMyInput.GetMouseY() => 
            0;

        int VRage.Input.IMyInput.GetMouseYForGamePlay() => 
            0;

        float VRage.Input.IMyInput.GetMouseYForGamePlayF() => 
            0f;

        bool VRage.Input.IMyInput.GetMouseYInversion() => 
            false;

        string VRage.Input.IMyInput.GetName(MyJoystickAxesEnum joystickAxis) => 
            "";

        string VRage.Input.IMyInput.GetName(MyJoystickButtonsEnum joystickButton) => 
            "";

        string VRage.Input.IMyInput.GetName(MyMouseButtonsEnum mouseButton) => 
            "";

        void VRage.Input.IMyInput.GetPressedKeys(List<MyKeys> keys)
        {
        }

        string VRage.Input.IMyInput.GetUnassignedName() => 
            "";

        bool VRage.Input.IMyInput.IsAnyAltKeyPressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyCtrlKeyPressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyKeyPress() => 
            false;

        bool VRage.Input.IMyInput.IsAnyMouseOrJoystickPressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyNewMouseOrJoystickPressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyNewMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsAnyShiftKeyPressed() => 
            false;

        bool VRage.Input.IMyInput.IsButtonPressed(MySharedButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsButtonReleased(MySharedButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsGameControlPressed(MyStringId controlEnum) => 
            false;

        bool VRage.Input.IMyInput.IsGameControlReleased(MyStringId controlEnum) => 
            false;

        bool VRage.Input.IMyInput.IsGamepadKeyLeftPressed() => 
            false;

        bool VRage.Input.IMyInput.IsGamepadKeyRightPressed() => 
            false;

        bool VRage.Input.IMyInput.IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickAxisPressed(MyJoystickAxesEnum axis) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickAxisValid(MyJoystickAxesEnum axis) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickButtonPressed(MyJoystickButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickButtonValid(MyJoystickButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsJoystickConnected() => 
            false;

        bool VRage.Input.IMyInput.IsKeyDigit(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.IsKeyPress(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.IsKeyValid(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.IsLeftCtrlKeyPressed() => 
            false;

        bool VRage.Input.IMyInput.IsLeftMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsLeftMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsMiddleMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsMouseButtonValid(MyMouseButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsMousePressed(MyMouseButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsMouseReleased(MyMouseButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsNewButtonPressed(MySharedButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsNewButtonReleased(MySharedButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsNewGameControlJoystickOnlyPressed(MyStringId controlId) => 
            false;

        bool VRage.Input.IMyInput.IsNewGameControlPressed(MyStringId controlEnum) => 
            false;

        bool VRage.Input.IMyInput.IsNewGameControlReleased(MyStringId controlEnum) => 
            false;

        bool VRage.Input.IMyInput.IsNewGamepadKeyDownPressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewGamepadKeyUpPressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => 
            false;

        bool VRage.Input.IMyInput.IsNewJoystickButtonReleased(MyJoystickButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsNewKeyPressed(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.IsNewKeyReleased(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.IsNewLeftMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewLeftMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewMiddleMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewMiddleMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewMousePressed(MyMouseButtonsEnum button) => 
            false;

        bool VRage.Input.IMyInput.IsNewPrimaryButtonPressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewPrimaryButtonReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewRightMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewRightMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewSecondaryButtonPressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewSecondaryButtonReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewXButton1MousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewXButton1MouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsNewXButton2MousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsNewXButton2MouseReleased() => 
            false;

        bool VRage.Input.IMyInput.IsPrimaryButtonPressed() => 
            false;

        bool VRage.Input.IMyInput.IsPrimaryButtonReleased() => 
            false;

        bool VRage.Input.IMyInput.IsRightCtrlKeyPressed() => 
            false;

        bool VRage.Input.IMyInput.IsRightMousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsSecondaryButtonPressed() => 
            false;

        bool VRage.Input.IMyInput.IsSecondaryButtonReleased() => 
            false;

        bool VRage.Input.IMyInput.IsXButton1MousePressed() => 
            false;

        bool VRage.Input.IMyInput.IsXButton2MousePressed() => 
            false;

        void VRage.Input.IMyInput.LoadContent(IntPtr windowHandle)
        {
        }

        void VRage.Input.IMyInput.LoadData(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons)
        {
        }

        int VRage.Input.IMyInput.MouseScrollWheelValue() => 
            0;

        int VRage.Input.IMyInput.PreviousMouseScrollWheelValue() => 
            0;

        void VRage.Input.IMyInput.RevertChanges()
        {
        }

        void VRage.Input.IMyInput.RevertToDefaultControls()
        {
        }

        void VRage.Input.IMyInput.SaveControls(SerializableDictionary<string, object> controlsGeneral, SerializableDictionary<string, object> controlsButtons)
        {
        }

        void VRage.Input.IMyInput.SetJoystickDeadzone(float newDeadzone)
        {
        }

        void VRage.Input.IMyInput.SetJoystickExponent(float newExponent)
        {
        }

        void VRage.Input.IMyInput.SetJoystickSensitivity(float newSensitivity)
        {
        }

        void VRage.Input.IMyInput.SetMousePosition(int x, int y)
        {
        }

        void VRage.Input.IMyInput.SetMouseSensitivity(float sensitivity)
        {
        }

        void VRage.Input.IMyInput.SetMouseXInversion(bool inverted)
        {
        }

        void VRage.Input.IMyInput.SetMouseYInversion(bool inverted)
        {
        }

        void VRage.Input.IMyInput.TakeSnapshot()
        {
        }

        void VRage.Input.IMyInput.UnloadData()
        {
        }

        bool VRage.Input.IMyInput.Update(bool gameFocused) => 
            false;

        bool VRage.Input.IMyInput.WasKeyPress(MyKeys key) => 
            false;

        bool VRage.Input.IMyInput.WasMiddleMousePressed() => 
            false;

        bool VRage.Input.IMyInput.WasMiddleMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.WasRightMousePressed() => 
            false;

        bool VRage.Input.IMyInput.WasRightMouseReleased() => 
            false;

        bool VRage.Input.IMyInput.WasXButton1MousePressed() => 
            false;

        bool VRage.Input.IMyInput.WasXButton1MouseReleased() => 
            false;

        bool VRage.Input.IMyInput.WasXButton2MousePressed() => 
            false;

        bool VRage.Input.IMyInput.WasXButton2MouseReleased() => 
            false;

        int VRage.ModAPI.IMyInput.DeltaMouseScrollWheelValue() => 
            ((VRage.Input.IMyInput) this).DeltaMouseScrollWheelValue();

        List<string> VRage.ModAPI.IMyInput.EnumerateJoystickNames() => 
            ((VRage.Input.IMyInput) this).EnumerateJoystickNames();

        IMyControl VRage.ModAPI.IMyInput.GetControl(MyKeys key) => 
            ((VRage.Input.IMyInput) this).GetControl(key);

        IMyControl VRage.ModAPI.IMyInput.GetControl(MyMouseButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).GetControl(button);

        IMyControl VRage.ModAPI.IMyInput.GetGameControl(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).GetGameControl(controlEnum);

        float VRage.ModAPI.IMyInput.GetGameControlAnalogState(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).GetGameControlAnalogState(controlEnum);

        float VRage.ModAPI.IMyInput.GetJoystickAxisStateForGameplay(MyJoystickAxesEnum axis) => 
            ((VRage.Input.IMyInput) this).GetJoystickAxisStateForGameplay(axis);

        string VRage.ModAPI.IMyInput.GetKeyName(MyKeys key) => 
            ((VRage.Input.IMyInput) this).GetKeyName(key);

        void VRage.ModAPI.IMyInput.GetListOfPressedKeys(List<MyKeys> keys)
        {
            ((VRage.Input.IMyInput) this).GetListOfPressedKeys(keys);
        }

        void VRage.ModAPI.IMyInput.GetListOfPressedMouseButtons(List<MyMouseButtonsEnum> result)
        {
            ((VRage.Input.IMyInput) this).GetListOfPressedMouseButtons(result);
        }

        Vector2 VRage.ModAPI.IMyInput.GetMouseAreaSize() => 
            ((VRage.Input.IMyInput) this).GetMouseAreaSize();

        Vector2 VRage.ModAPI.IMyInput.GetMousePosition() => 
            ((VRage.Input.IMyInput) this).GetMousePosition();

        float VRage.ModAPI.IMyInput.GetMouseSensitivity() => 
            ((VRage.Input.IMyInput) this).GetMouseSensitivity();

        int VRage.ModAPI.IMyInput.GetMouseX() => 
            ((VRage.Input.IMyInput) this).GetMouseX();

        int VRage.ModAPI.IMyInput.GetMouseXForGamePlay() => 
            ((VRage.Input.IMyInput) this).GetMouseXForGamePlay();

        bool VRage.ModAPI.IMyInput.GetMouseXInversion() => 
            ((VRage.Input.IMyInput) this).GetMouseXInversion();

        int VRage.ModAPI.IMyInput.GetMouseY() => 
            ((VRage.Input.IMyInput) this).GetMouseY();

        int VRage.ModAPI.IMyInput.GetMouseYForGamePlay() => 
            ((VRage.Input.IMyInput) this).GetMouseYForGamePlay();

        bool VRage.ModAPI.IMyInput.GetMouseYInversion() => 
            ((VRage.Input.IMyInput) this).GetMouseYInversion();

        string VRage.ModAPI.IMyInput.GetName(MyJoystickAxesEnum joystickAxis) => 
            ((VRage.Input.IMyInput) this).GetName(joystickAxis);

        string VRage.ModAPI.IMyInput.GetName(MyJoystickButtonsEnum joystickButton) => 
            ((VRage.Input.IMyInput) this).GetName(joystickButton);

        string VRage.ModAPI.IMyInput.GetName(MyMouseButtonsEnum mouseButton) => 
            ((VRage.Input.IMyInput) this).GetName(mouseButton);

        void VRage.ModAPI.IMyInput.GetPressedKeys(List<MyKeys> keys)
        {
            ((VRage.Input.IMyInput) this).GetPressedKeys(keys);
        }

        string VRage.ModAPI.IMyInput.GetUnassignedName() => 
            ((VRage.Input.IMyInput) this).GetUnassignedName();

        bool VRage.ModAPI.IMyInput.IsAnyAltKeyPressed() => 
            ((VRage.Input.IMyInput) this).IsAnyAltKeyPressed();

        bool VRage.ModAPI.IMyInput.IsAnyCtrlKeyPressed() => 
            ((VRage.Input.IMyInput) this).IsAnyCtrlKeyPressed();

        bool VRage.ModAPI.IMyInput.IsAnyKeyPress() => 
            ((VRage.Input.IMyInput) this).IsAnyKeyPress();

        bool VRage.ModAPI.IMyInput.IsAnyMouseOrJoystickPressed() => 
            ((VRage.Input.IMyInput) this).IsAnyMouseOrJoystickPressed();

        bool VRage.ModAPI.IMyInput.IsAnyMousePressed() => 
            ((VRage.Input.IMyInput) this).IsAnyMousePressed();

        bool VRage.ModAPI.IMyInput.IsAnyNewMouseOrJoystickPressed() => 
            ((VRage.Input.IMyInput) this).IsAnyNewMouseOrJoystickPressed();

        bool VRage.ModAPI.IMyInput.IsAnyNewMousePressed() => 
            ((VRage.Input.IMyInput) this).IsAnyNewMousePressed();

        bool VRage.ModAPI.IMyInput.IsAnyShiftKeyPressed() => 
            ((VRage.Input.IMyInput) this).IsAnyShiftKeyPressed();

        bool VRage.ModAPI.IMyInput.IsButtonPressed(MySharedButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsButtonReleased(MySharedButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsGameControlPressed(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).IsGameControlPressed(controlEnum);

        bool VRage.ModAPI.IMyInput.IsGameControlReleased(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).IsGameControlReleased(controlEnum);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisNewPressed(MyJoystickAxesEnum axis) => 
            ((VRage.Input.IMyInput) this).IsJoystickAxisNewPressed(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisPressed(MyJoystickAxesEnum axis) => 
            ((VRage.Input.IMyInput) this).IsJoystickAxisPressed(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickAxisValid(MyJoystickAxesEnum axis) => 
            ((VRage.Input.IMyInput) this).IsJoystickAxisValid(axis);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonNewPressed(MyJoystickButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsJoystickButtonNewPressed(button);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonPressed(MyJoystickButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsJoystickButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsJoystickButtonValid(MyJoystickButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsJoystickButtonValid(button);

        bool VRage.ModAPI.IMyInput.IsJoystickConnected() => 
            ((VRage.Input.IMyInput) this).IsJoystickConnected();

        bool VRage.ModAPI.IMyInput.IsKeyDigit(MyKeys key) => 
            ((VRage.Input.IMyInput) this).IsKeyDigit(key);

        bool VRage.ModAPI.IMyInput.IsKeyPress(MyKeys key) => 
            ((VRage.Input.IMyInput) this).IsKeyPress(key);

        bool VRage.ModAPI.IMyInput.IsKeyValid(MyKeys key) => 
            ((VRage.Input.IMyInput) this).IsKeyValid(key);

        bool VRage.ModAPI.IMyInput.IsLeftMousePressed() => 
            ((VRage.Input.IMyInput) this).IsLeftMousePressed();

        bool VRage.ModAPI.IMyInput.IsLeftMouseReleased() => 
            ((VRage.Input.IMyInput) this).IsLeftMouseReleased();

        bool VRage.ModAPI.IMyInput.IsMiddleMousePressed() => 
            ((VRage.Input.IMyInput) this).IsMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.IsMouseButtonValid(MyMouseButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsMouseButtonValid(button);

        bool VRage.ModAPI.IMyInput.IsMousePressed(MyMouseButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsMousePressed(button);

        bool VRage.ModAPI.IMyInput.IsMouseReleased(MyMouseButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsMouseReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewButtonPressed(MySharedButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsNewButtonPressed(button);

        bool VRage.ModAPI.IMyInput.IsNewButtonReleased(MySharedButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsNewButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewGameControlPressed(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).IsNewGameControlPressed(controlEnum);

        bool VRage.ModAPI.IMyInput.IsNewGameControlReleased(MyStringId controlEnum) => 
            ((VRage.Input.IMyInput) this).IsNewGameControlReleased(controlEnum);

        bool VRage.ModAPI.IMyInput.IsNewJoystickAxisReleased(MyJoystickAxesEnum axis) => 
            ((VRage.Input.IMyInput) this).IsNewJoystickAxisReleased(axis);

        bool VRage.ModAPI.IMyInput.IsNewJoystickButtonReleased(MyJoystickButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsNewJoystickButtonReleased(button);

        bool VRage.ModAPI.IMyInput.IsNewKeyPressed(MyKeys key) => 
            ((VRage.Input.IMyInput) this).IsNewKeyPressed(key);

        bool VRage.ModAPI.IMyInput.IsNewKeyReleased(MyKeys key) => 
            ((VRage.Input.IMyInput) this).IsNewKeyReleased(key);

        bool VRage.ModAPI.IMyInput.IsNewLeftMousePressed() => 
            ((VRage.Input.IMyInput) this).IsNewLeftMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewLeftMouseReleased() => 
            ((VRage.Input.IMyInput) this).IsNewLeftMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewMiddleMousePressed() => 
            ((VRage.Input.IMyInput) this).IsNewMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewMiddleMouseReleased() => 
            ((VRage.Input.IMyInput) this).IsNewMiddleMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewMousePressed(MyMouseButtonsEnum button) => 
            ((VRage.Input.IMyInput) this).IsNewMousePressed(button);

        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonPressed() => 
            ((VRage.Input.IMyInput) this).IsNewPrimaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsNewPrimaryButtonReleased() => 
            ((VRage.Input.IMyInput) this).IsNewPrimaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsNewRightMousePressed() => 
            ((VRage.Input.IMyInput) this).IsNewRightMousePressed();

        bool VRage.ModAPI.IMyInput.IsNewRightMouseReleased() => 
            ((VRage.Input.IMyInput) this).IsNewRightMouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonPressed() => 
            ((VRage.Input.IMyInput) this).IsNewSecondaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsNewSecondaryButtonReleased() => 
            ((VRage.Input.IMyInput) this).IsNewSecondaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsNewXButton1MousePressed() => 
            ((VRage.Input.IMyInput) this).IsNewXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.IsNewXButton1MouseReleased() => 
            ((VRage.Input.IMyInput) this).IsNewXButton1MouseReleased();

        bool VRage.ModAPI.IMyInput.IsNewXButton2MousePressed() => 
            ((VRage.Input.IMyInput) this).IsNewXButton2MousePressed();

        bool VRage.ModAPI.IMyInput.IsNewXButton2MouseReleased() => 
            ((VRage.Input.IMyInput) this).IsNewXButton2MouseReleased();

        bool VRage.ModAPI.IMyInput.IsPrimaryButtonPressed() => 
            ((VRage.Input.IMyInput) this).IsPrimaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsPrimaryButtonReleased() => 
            ((VRage.Input.IMyInput) this).IsPrimaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsRightMousePressed() => 
            ((VRage.Input.IMyInput) this).IsRightMousePressed();

        bool VRage.ModAPI.IMyInput.IsSecondaryButtonPressed() => 
            ((VRage.Input.IMyInput) this).IsSecondaryButtonPressed();

        bool VRage.ModAPI.IMyInput.IsSecondaryButtonReleased() => 
            ((VRage.Input.IMyInput) this).IsSecondaryButtonReleased();

        bool VRage.ModAPI.IMyInput.IsXButton1MousePressed() => 
            ((VRage.Input.IMyInput) this).IsXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.IsXButton2MousePressed() => 
            ((VRage.Input.IMyInput) this).IsXButton2MousePressed();

        int VRage.ModAPI.IMyInput.MouseScrollWheelValue() => 
            ((VRage.Input.IMyInput) this).MouseScrollWheelValue();

        int VRage.ModAPI.IMyInput.PreviousMouseScrollWheelValue() => 
            ((VRage.Input.IMyInput) this).PreviousMouseScrollWheelValue();

        bool VRage.ModAPI.IMyInput.WasKeyPress(MyKeys key) => 
            ((VRage.Input.IMyInput) this).WasKeyPress(key);

        bool VRage.ModAPI.IMyInput.WasMiddleMousePressed() => 
            ((VRage.Input.IMyInput) this).WasMiddleMousePressed();

        bool VRage.ModAPI.IMyInput.WasMiddleMouseReleased() => 
            ((VRage.Input.IMyInput) this).WasMiddleMouseReleased();

        bool VRage.ModAPI.IMyInput.WasRightMousePressed() => 
            ((VRage.Input.IMyInput) this).WasRightMousePressed();

        bool VRage.ModAPI.IMyInput.WasRightMouseReleased() => 
            ((VRage.Input.IMyInput) this).WasRightMouseReleased();

        bool VRage.ModAPI.IMyInput.WasXButton1MousePressed() => 
            ((VRage.Input.IMyInput) this).WasXButton1MousePressed();

        bool VRage.ModAPI.IMyInput.WasXButton1MouseReleased() => 
            ((VRage.Input.IMyInput) this).WasXButton1MouseReleased();

        bool VRage.ModAPI.IMyInput.WasXButton2MousePressed() => 
            ((VRage.Input.IMyInput) this).WasXButton2MousePressed();

        bool VRage.ModAPI.IMyInput.WasXButton2MouseReleased() => 
            ((VRage.Input.IMyInput) this).WasXButton2MouseReleased();

        bool VRage.ModAPI.IMyInput.IsCapsLock =>
            ((VRage.Input.IMyInput) this).IsCapsLock;

        string VRage.ModAPI.IMyInput.JoystickInstanceName =>
            ((VRage.Input.IMyInput) this).JoystickInstanceName;

        ListReader<char> VRage.ModAPI.IMyInput.TextInput =>
            ((VRage.Input.IMyInput) this).TextInput;

        bool VRage.ModAPI.IMyInput.JoystickAsMouse =>
            ((VRage.Input.IMyInput) this).JoystickAsMouse;

        bool VRage.ModAPI.IMyInput.IsJoystickLastUsed =>
            ((VRage.Input.IMyInput) this).IsJoystickLastUsed;

        bool VRage.Input.IMyInput.IsCapsLock =>
            false;

        string VRage.Input.IMyInput.JoystickInstanceName
        {
            get => 
                "";
            set
            {
            }
        }

        IntPtr VRage.Input.IMyInput.WindowHandle =>
            IntPtr.Zero;

        ListReader<char> VRage.Input.IMyInput.TextInput =>
            this.m_listChars;

        bool VRage.Input.IMyInput.JoystickAsMouse
        {
            get => 
                false;
            set
            {
            }
        }

        bool VRage.Input.IMyInput.IsJoystickLastUsed =>
            false;

        bool VRage.Input.IMyInput.ENABLE_DEVELOPER_KEYS =>
            false;
    }
}

