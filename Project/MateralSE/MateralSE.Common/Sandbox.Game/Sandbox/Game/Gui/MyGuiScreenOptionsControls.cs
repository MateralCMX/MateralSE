namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenOptionsControls : MyGuiScreenBase
    {
        private MyGuiControlTypeEnum m_currentControlType;
        private MyGuiControlCombobox m_controlTypeList;
        private Dictionary<MyGuiControlTypeEnum, List<MyGuiControlBase>> m_allControls;
        private List<MyGuiControlButton> m_key1Buttons;
        private List<MyGuiControlButton> m_key2Buttons;
        private List<MyGuiControlButton> m_mouseButtons;
        private List<MyGuiControlButton> m_joystickButtons;
        private List<MyGuiControlButton> m_joystickAxes;
        private MyGuiControlCheckbox m_invertMouseXCheckbox;
        private MyGuiControlCheckbox m_invertMouseYCheckbox;
        private MyGuiControlSlider m_mouseSensitivitySlider;
        private MyGuiControlSlider m_joystickSensitivitySlider;
        private MyGuiControlSlider m_joystickDeadzoneSlider;
        private MyGuiControlSlider m_joystickExponentSlider;
        private MyGuiControlCombobox m_joystickCombobox;
        private Vector2 m_controlsOriginLeft;
        private Vector2 m_controlsOriginRight;
        private MyGuiControlElementGroup m_elementGroup;

        public MyGuiScreenOptionsControls() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.9465649f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_allControls = new Dictionary<MyGuiControlTypeEnum, List<MyGuiControlBase>>();
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void ActivateControls(MyGuiControlTypeEnum type)
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.m_allControls[type].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = true;
                }
            }
        }

        private void AddControls()
        {
            this.m_key1Buttons = new List<MyGuiControlButton>();
            this.m_key2Buttons = new List<MyGuiControlButton>();
            this.m_mouseButtons = new List<MyGuiControlButton>();
            if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
            {
                this.m_joystickButtons = new List<MyGuiControlButton>();
                this.m_joystickAxes = new List<MyGuiControlButton>();
            }
            this.AddControlsByType(MyGuiControlTypeEnum.General);
            this.AddControlsByType(MyGuiControlTypeEnum.Navigation);
            this.AddControlsByType(MyGuiControlTypeEnum.Systems1);
            this.AddControlsByType(MyGuiControlTypeEnum.Systems2);
            this.AddControlsByType(MyGuiControlTypeEnum.Systems3);
            this.AddControlsByType(MyGuiControlTypeEnum.ToolsOrWeapons);
            this.AddControlsByType(MyGuiControlTypeEnum.Spectator);
            foreach (KeyValuePair<MyGuiControlTypeEnum, List<MyGuiControlBase>> pair in this.m_allControls)
            {
                foreach (MyGuiControlBase base2 in pair.Value)
                {
                    this.Controls.Add(base2);
                }
                this.DeactivateControls(pair.Key);
            }
            if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
            {
                this.RefreshJoystickControlEnabling();
            }
        }

        private unsafe void AddControlsByType(MyGuiControlTypeEnum type)
        {
            if (type == MyGuiControlTypeEnum.General)
            {
                this.AddGeneralControls();
            }
            else
            {
                MyGuiControlButton.StyleDefinition visualStyle = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.ControlSetting);
                Vector2 controlsOriginRight = this.m_controlsOriginRight;
                float* singlePtr1 = (float*) ref controlsOriginRight.X;
                singlePtr1[0] -= 0.02f;
                float* singlePtr2 = (float*) ref controlsOriginRight.Y;
                singlePtr2[0] -= 0.01f;
                this.m_allControls[type] = new List<MyGuiControlBase>();
                float num = 2f;
                float num2 = 0.85f;
                DictionaryValuesReader<MyStringId, MyControl> gameControlsList = MyInput.Static.GetGameControlsList();
                MyGuiControlLabel item = this.MakeLabel(MyCommonTexts.ScreenOptionsControls_Keyboard1, Vector2.Zero);
                MyGuiControlLabel label2 = this.MakeLabel(MyCommonTexts.ScreenOptionsControls_Keyboard2, Vector2.Zero);
                MyGuiControlLabel label3 = this.MakeLabel(MyCommonTexts.ScreenOptionsControls_Mouse, Vector2.Zero);
                if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
                {
                    this.MakeLabel(MyCommonTexts.ScreenOptionsControls_Gamepad, Vector2.Zero);
                }
                if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
                {
                    this.MakeLabel(MyCommonTexts.ScreenOptionsControls_AnalogAxes, Vector2.Zero);
                }
                float num3 = 1.1f * Math.Max(Math.Max(item.Size.X, label2.Size.X), Math.Max(label3.Size.X, visualStyle.SizeOverride.Value.X));
                Vector2 position = ((Vector2) ((num - 1f) * MyGuiConstants.CONTROLS_DELTA)) + controlsOriginRight;
                float* singlePtr3 = (float*) ref position.X;
                singlePtr3[0] += (num3 * 0.5f) - 0.265f;
                float* singlePtr4 = (float*) ref position.Y;
                singlePtr4[0] -= 0.015f;
                item.Position = position;
                float* singlePtr5 = (float*) ref position.X;
                singlePtr5[0] += num3;
                label2.Position = position;
                float* singlePtr6 = (float*) ref position.X;
                singlePtr6[0] += num3;
                label3.Position = position;
                this.m_allControls[type].Add(item);
                this.m_allControls[type].Add(label2);
                this.m_allControls[type].Add(label3);
                bool flag1 = MyFakes.ENABLE_JOYSTICK_SETTINGS;
                foreach (MyControl control in gameControlsList)
                {
                    if (control.GetControlTypeEnum() == type)
                    {
                        Vector2? size = null;
                        VRageMath.Vector4? colorMask = null;
                        this.m_allControls[type].Add(new MyGuiControlLabel(new Vector2?((this.m_controlsOriginLeft + (num * MyGuiConstants.CONTROLS_DELTA)) - new Vector2(0f, 0.03f)), size, MyTexts.GetString(control.GetControlName()), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER));
                        position = (controlsOriginRight + (num * MyGuiConstants.CONTROLS_DELTA)) - new Vector2(0.265f, 0.015f);
                        float* singlePtr7 = (float*) ref position.X;
                        singlePtr7[0] += num3 * 0.5f;
                        MyGuiControlButton button = this.MakeControlButton(control, position, MyGuiInputDeviceEnum.Keyboard);
                        button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_ClickToEdit));
                        this.m_allControls[type].Add(button);
                        this.m_key1Buttons.Add(button);
                        float* singlePtr8 = (float*) ref position.X;
                        singlePtr8[0] += num3;
                        MyGuiControlButton button2 = this.MakeControlButton(control, position, MyGuiInputDeviceEnum.KeyboardSecond);
                        button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_ClickToEdit));
                        this.m_allControls[type].Add(button2);
                        this.m_key2Buttons.Add(button2);
                        float* singlePtr9 = (float*) ref position.X;
                        singlePtr9[0] += num3;
                        MyGuiControlButton button3 = this.MakeControlButton(control, position, MyGuiInputDeviceEnum.Mouse);
                        button3.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_ClickToEdit));
                        this.m_allControls[type].Add(button3);
                        this.m_mouseButtons.Add(button3);
                        float* singlePtr10 = (float*) ref position.X;
                        singlePtr10[0] += num3;
                        bool flag2 = MyFakes.ENABLE_JOYSTICK_SETTINGS;
                        num += num2;
                    }
                }
            }
        }

        private unsafe void AddGeneralControls()
        {
            float* singlePtr1 = (float*) ref this.m_controlsOriginRight.Y;
            singlePtr1[0] -= 0.025f;
            float* singlePtr2 = (float*) ref this.m_controlsOriginLeft.Y;
            singlePtr2[0] -= 0.025f;
            this.m_allControls[MyGuiControlTypeEnum.General] = new List<MyGuiControlBase>();
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(1.7f, MyCommonTexts.MouseSensitivity));
            VRageMath.Vector4? color = null;
            this.m_mouseSensitivitySlider = new MyGuiControlSlider(new Vector2?((this.m_controlsOriginRight + (1.7f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f)), 0.1f, 3f, 0.29f, new float?(MyInput.Static.GetMouseSensitivity()), color, null, 1, 0.8f, 0f, "White", MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_MouseSensitivity), MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, false, false);
            this.m_mouseSensitivitySlider.Size = new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
            this.m_mouseSensitivitySlider.Value = MyInput.Static.GetMouseSensitivity();
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_mouseSensitivitySlider);
            if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
            {
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(2.63f, MyCommonTexts.JoystickSensitivity));
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(3.56f, MyCommonTexts.JoystickExponent));
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(4.49f, MyCommonTexts.JoystickDeadzone));
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(5.72f, MyCommonTexts.Joystick));
                string toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_JoystickSensitivity);
                color = null;
                this.m_joystickSensitivitySlider = new MyGuiControlSlider(new Vector2?((this.m_controlsOriginRight + (2.63f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2((455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) / 2f, 0f)), 0.1f, 6f, 0.29f, new float?(MyInput.Static.GetJoystickSensitivity()), color, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                this.m_joystickSensitivitySlider.Size = new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
                this.m_joystickSensitivitySlider.Value = MyInput.Static.GetJoystickSensitivity();
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_joystickSensitivitySlider);
                toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_JoystickGradualPrecision);
                color = null;
                this.m_joystickExponentSlider = new MyGuiControlSlider(new Vector2?((this.m_controlsOriginRight + (3.56f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2((455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) / 2f, 0f)), 1f, 8f, 0.29f, new float?(MyInput.Static.GetJoystickExponent()), color, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                this.m_joystickExponentSlider.Size = new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
                this.m_joystickExponentSlider.Value = MyInput.Static.GetJoystickExponent();
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_joystickExponentSlider);
                toolTip = MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_JoystickDeadzoneWidth);
                color = null;
                this.m_joystickDeadzoneSlider = new MyGuiControlSlider(new Vector2?((this.m_controlsOriginRight + (4.49f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2((455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) / 2f, 0f)), 0f, 0.5f, 0.29f, new float?(MyInput.Static.GetJoystickDeadzone()), color, null, 1, 0.8f, 0f, "White", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
                this.m_joystickDeadzoneSlider.Size = new Vector2(455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
                this.m_joystickDeadzoneSlider.Value = MyInput.Static.GetJoystickDeadzone();
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_joystickDeadzoneSlider);
                Vector2? size = null;
                color = null;
                size = null;
                size = null;
                color = null;
                this.m_joystickCombobox = new MyGuiControlCombobox(new Vector2?((this.m_controlsOriginRight + (5.72f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2((455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X) / 2f, 0f)), size, color, size, 10, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, color);
                this.m_joystickCombobox.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_JoystickOrGamepad));
                this.m_joystickCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnSelectJoystick);
                this.AddJoysticksToComboBox();
                this.m_joystickCombobox.Size = new Vector2(452f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f);
                this.m_joystickCombobox.Enabled = !MyFakes.ENFORCE_CONTROLLER || !MyInput.Static.IsJoystickConnected();
                this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_joystickCombobox);
            }
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(6.85f, MyCommonTexts.InvertMouseX));
            color = null;
            bool mouseXInversion = MyInput.Static.GetMouseXInversion();
            this.m_invertMouseXCheckbox = new MyGuiControlCheckbox(new Vector2?((this.m_controlsOriginRight + (6.85f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2(456.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f)), color, MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_InvertMouseX), mouseXInversion, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_invertMouseXCheckbox);
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.MakeLabel(7.7f, MyCommonTexts.InvertMouseY));
            color = null;
            mouseXInversion = MyInput.Static.GetMouseYInversion();
            this.m_invertMouseYCheckbox = new MyGuiControlCheckbox(new Vector2?((this.m_controlsOriginRight + (7.7f * MyGuiConstants.CONTROLS_DELTA)) - new Vector2(456.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 0f)), color, MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_InvertMouseY), mouseXInversion, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_allControls[MyGuiControlTypeEnum.General].Add(this.m_invertMouseYCheckbox);
            float* singlePtr3 = (float*) ref this.m_controlsOriginRight.Y;
            singlePtr3[0] += 0.025f;
            float* singlePtr4 = (float*) ref this.m_controlsOriginLeft.Y;
            singlePtr4[0] += 0.025f;
        }

        private void AddJoysticksToComboBox()
        {
            int index = 0;
            bool flag = false;
            index++;
            int? sortOrder = null;
            this.m_joystickCombobox.AddItem((long) index, MyTexts.Get(MyCommonTexts.Disabled), sortOrder, null);
            foreach (string str in MyInput.Static.EnumerateJoystickNames())
            {
                sortOrder = null;
                this.m_joystickCombobox.AddItem((long) index, new StringBuilder(str), sortOrder, null);
                if (MyInput.Static.JoystickInstanceName == str)
                {
                    flag = true;
                    this.m_joystickCombobox.SelectItemByIndex(index);
                }
                index++;
            }
            if (!flag)
            {
                this.m_joystickCombobox.SelectItemByIndex(0);
            }
        }

        protected override void Canceling()
        {
            MyInput.Static.RevertChanges();
            base.Canceling();
        }

        private void CloseScreenAndSave()
        {
            MyInput.Static.SetMouseXInversion(this.m_invertMouseXCheckbox.IsChecked);
            MyInput.Static.SetMouseYInversion(this.m_invertMouseYCheckbox.IsChecked);
            MyInput.Static.SetMouseSensitivity(this.m_mouseSensitivitySlider.Value);
            if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
            {
                MyInput.Static.JoystickInstanceName = (this.m_joystickCombobox.GetSelectedIndex() == 0) ? null : this.m_joystickCombobox.GetSelectedValue().ToString();
                MyInput.Static.SetJoystickSensitivity(this.m_joystickSensitivitySlider.Value);
                MyInput.Static.SetJoystickExponent(this.m_joystickExponentSlider.Value);
                MyInput.Static.SetJoystickDeadzone(this.m_joystickDeadzoneSlider.Value);
            }
            MyInput.Static.SaveControls(MySandboxGame.Config.ControlsGeneral, MySandboxGame.Config.ControlsButtons);
            MySandboxGame.Config.Save();
            MyScreenManager.RecreateControls();
            this.CloseScreen();
        }

        private void DeactivateControls(MyGuiControlTypeEnum type)
        {
            using (List<MyGuiControlBase>.Enumerator enumerator = this.m_allControls[type].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Visible = false;
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptionsControls";

        private void m_elementGroup_HighlightChanged(MyGuiControlElementGroup obj)
        {
            foreach (MyGuiControlBase base2 in this.m_elementGroup)
            {
                if (base2.HasFocus && !ReferenceEquals(obj.SelectedElement, base2))
                {
                    base.FocusedControl = obj.SelectedElement;
                    break;
                }
            }
        }

        private MyGuiControlButton MakeControlButton(MyControl control, Vector2 position, MyGuiInputDeviceEnum device)
        {
            StringBuilder output = null;
            control.AppendBoundButtonNames(ref output, device, null);
            MyControl.AppendUnknownTextIfNeeded(ref output, MyTexts.GetString(MyCommonTexts.UnknownControl_None));
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.ControlSetting, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, output, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnControlClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.UserData = new ControlButtonData(control, device);
            return button1;
        }

        private MyGuiControlLabel MakeLabel(float deltaMultip, MyStringId textEnum)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(this.m_controlsOriginLeft + (deltaMultip * MyGuiConstants.CONTROLS_DELTA)), size, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum, Vector2 position)
        {
            Vector2? size = null;
            return new MyGuiControlLabel(new Vector2?(position), size, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP);
        }

        private void OnCancelClick(MyGuiControlButton sender)
        {
            MyInput.Static.RevertChanges();
            this.CloseScreen();
        }

        private void OnControlClick(MyGuiControlButton button)
        {
            ControlButtonData userData = (ControlButtonData) button.UserData;
            MyStringId assignControlKeyboard = MyCommonTexts.AssignControlKeyboard;
            if (userData.Device == MyGuiInputDeviceEnum.Mouse)
            {
                assignControlKeyboard = MyCommonTexts.AssignControlMouse;
            }
            MyGuiControlAssignKeyMessageBox screen = new MyGuiControlAssignKeyMessageBox(userData.Device, userData.Control, assignControlKeyboard);
            screen.Closed += s => this.RefreshButtonTexts();
            MyGuiSandbox.AddScreen(screen);
        }

        private void OnOkClick(MyGuiControlButton sender)
        {
            this.CloseScreenAndSave();
        }

        private void OnResetDefaultsClick(MyGuiControlButton sender)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionResetControlsToDefault);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextResetControlsToDefault), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum res) {
                if (res == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MyInput.Static.RevertToDefaultControls();
                    this.DeactivateControls(this.m_currentControlType);
                    this.AddControls();
                    this.ActivateControls(this.m_currentControlType);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void OnSelectJoystick()
        {
            MyInput.Static.JoystickInstanceName = (this.m_joystickCombobox.GetSelectedIndex() == 0) ? null : this.m_joystickCombobox.GetSelectedValue().ToString();
            this.RefreshJoystickControlEnabling();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.m_elementGroup = new MyGuiControlElementGroup();
            this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionControls, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyInput.Static.TakeSnapshot();
            Vector2 local1 = base.m_size.Value * new Vector2(0f, -0.5f);
            Vector2 local2 = base.m_size.Value * -0.5f;
            this.m_controlsOriginLeft = (((base.m_size.Value / 2f) - (new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE)) * new Vector2(-1f, -1f)) + new Vector2(0f, MyGuiConstants.SCREEN_CAPTION_DELTA_Y);
            this.m_controlsOriginRight = (((base.m_size.Value / 2f) - (new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE)) * new Vector2(1f, -1f)) + new Vector2(0f, MyGuiConstants.SCREEN_CAPTION_DELTA_Y);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.144f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(list2);
            MyGuiControlSeparatorList list3 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list3.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(list3);
            Vector2 vector = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2 vector2 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            float num = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float x = 25f;
            float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y;
            Vector2 vector1 = new Vector2(0f, 0.045f);
            Vector2 local3 = (((base.m_size.Value / 2f) - vector) * new Vector2(-1f, -1f)) + new Vector2(0f, y);
            Vector2 vector3 = (((base.m_size.Value / 2f) - vector) * new Vector2(1f, -1f)) + new Vector2(0f, y);
            Vector2 vector4 = ((base.m_size.Value / 2f) - vector2) * new Vector2(0f, 1f);
            Vector2 vector5 = new Vector2(vector3.X - (num + 0.0015f), vector3.Y);
            Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
            Vector2 local4 = base.m_size.Value * new Vector2(0f, 0.5f);
            Vector2? position = null;
            position = null;
            captionTextColor = null;
            int? buttonIndex = null;
            MyGuiControlButton button = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            button.Position = vector4 + (new Vector2(-x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            position = null;
            position = null;
            captionTextColor = null;
            buttonIndex = null;
            MyGuiControlButton button2 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            button2.Position = vector4 + (new Vector2(x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            position = null;
            captionTextColor = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.Revert);
            buttonIndex = null;
            MyGuiControlButton button3 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ComboBoxButton, new Vector2?(MyGuiConstants.MESSAGE_BOX_BUTTON_SIZE_SMALL), captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_Defaults), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnResetDefaultsClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            button3.Position = new Vector2(0f, 0f) - new Vector2((0f - ((base.m_size.Value.X * 0.832f) / 2f)) + (button3.Size.X / 2f), (base.m_size.Value.Y / 2f) - 0.113f);
            button3.TextScale = 0.7f;
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            this.Controls.Add(button2);
            this.m_elementGroup.Add(button2);
            this.Controls.Add(button3);
            this.m_elementGroup.Add(button3);
            this.m_currentControlType = MyGuiControlTypeEnum.General;
            position = null;
            captionTextColor = null;
            position = null;
            position = null;
            captionTextColor = null;
            this.m_controlTypeList = new MyGuiControlCombobox(new Vector2?(new Vector2((0f - (button3.Size.X / 2f)) - 0.009f, 0f) - new Vector2(0f, (base.m_size.Value.Y / 2f) - 0.11f)), position, captionTextColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
            this.m_controlTypeList.Size = new Vector2(base.m_size.Value.X * 0.595f, 1f);
            this.m_controlTypeList.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsControls_Category));
            buttonIndex = null;
            MyStringId? toolTip = null;
            this.m_controlTypeList.AddItem(0L, MyCommonTexts.ControlTypeGeneral, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(1L, MyCommonTexts.ControlTypeNavigation, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(5L, MyCommonTexts.ControlTypeSystems1, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(6L, MyCommonTexts.ControlTypeSystems2, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(7L, MyCommonTexts.ControlTypeSystems3, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(3L, MyCommonTexts.ControlTypeToolsOrWeapons, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_controlTypeList.AddItem(8L, MyCommonTexts.ControlTypeView, buttonIndex, toolTip);
            this.m_controlTypeList.SelectItemByKey((long) this.m_currentControlType, true);
            this.Controls.Add(this.m_controlTypeList);
            this.AddControls();
            this.ActivateControls(this.m_currentControlType);
            base.FocusedControl = button;
            base.CloseButtonEnabled = true;
        }

        private void RefreshButtonTexts()
        {
            this.RefreshButtonTexts(this.m_key1Buttons);
            this.RefreshButtonTexts(this.m_key2Buttons);
            this.RefreshButtonTexts(this.m_mouseButtons);
            if (MyFakes.ENABLE_JOYSTICK_SETTINGS)
            {
                this.RefreshButtonTexts(this.m_joystickButtons);
                this.RefreshButtonTexts(this.m_joystickAxes);
            }
        }

        private void RefreshButtonTexts(List<MyGuiControlButton> buttons)
        {
            StringBuilder output = null;
            foreach (MyGuiControlButton local1 in buttons)
            {
                ControlButtonData userData = (ControlButtonData) local1.UserData;
                userData.Control.AppendBoundButtonNames(ref output, userData.Device, null);
                MyControl.AppendUnknownTextIfNeeded(ref output, MyTexts.GetString(MyCommonTexts.UnknownControl_None));
                local1.Text = output.ToString();
                output.Clear();
            }
        }

        private void RefreshJoystickControlEnabling()
        {
            List<MyGuiControlButton>.Enumerator enumerator;
            bool flag = this.m_joystickCombobox.GetSelectedIndex() != 0;
            using (enumerator = this.m_joystickButtons.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = flag;
                }
            }
            using (enumerator = this.m_joystickAxes.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = flag;
                }
            }
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_controlTypeList.GetSelectedKey() != ((long) this.m_currentControlType))
            {
                this.DeactivateControls(this.m_currentControlType);
                this.m_currentControlType = (MyGuiControlTypeEnum) ((byte) this.m_controlTypeList.GetSelectedKey());
                this.ActivateControls(this.m_currentControlType);
            }
            return base.Update(hasFocus);
        }

        private class ControlButtonData
        {
            public readonly MyControl Control;
            public readonly MyGuiInputDeviceEnum Device;

            public ControlButtonData(MyControl control, MyGuiInputDeviceEnum device)
            {
                this.Control = control;
                this.Device = device;
            }
        }

        private class MyGuiControlAssignKeyMessageBox : MyGuiScreenMessageBox
        {
            private MyControl m_controlBeingSet;
            private MyGuiInputDeviceEnum m_deviceType;
            private List<MyKeys> m_newPressedKeys;
            private List<MyMouseButtonsEnum> m_newPressedMouseButtons;
            private List<MyJoystickButtonsEnum> m_newPressedJoystickButtons;
            private List<MyJoystickAxesEnum> m_newPressedJoystickAxes;
            private List<MyKeys> m_oldPressedKeys;
            private List<MyMouseButtonsEnum> m_oldPressedMouseButtons;
            private List<MyJoystickButtonsEnum> m_oldPressedJoystickButtons;
            private List<MyJoystickAxesEnum> m_oldPressedJoystickAxes;

            public MyGuiControlAssignKeyMessageBox(MyGuiInputDeviceEnum deviceType, MyControl controlBeingSet, MyStringId messageText) : base(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.NONE, MyTexts.Get(messageText), MyTexts.Get(MyCommonTexts.SelectControl), id, id, id, id, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable, 0f, 0f)
            {
                this.m_newPressedKeys = new List<MyKeys>();
                this.m_newPressedMouseButtons = new List<MyMouseButtonsEnum>();
                this.m_newPressedJoystickButtons = new List<MyJoystickButtonsEnum>();
                this.m_newPressedJoystickAxes = new List<MyJoystickAxesEnum>();
                this.m_oldPressedKeys = new List<MyKeys>();
                this.m_oldPressedMouseButtons = new List<MyMouseButtonsEnum>();
                this.m_oldPressedJoystickButtons = new List<MyJoystickButtonsEnum>();
                this.m_oldPressedJoystickAxes = new List<MyJoystickAxesEnum>();
                MyStringId id = new MyStringId();
                id = new MyStringId();
                id = new MyStringId();
                id = new MyStringId();
                base.DrawMouseCursor = false;
                base.m_isTopMostScreen = false;
                this.m_controlBeingSet = controlBeingSet;
                this.m_deviceType = deviceType;
                MyInput.Static.GetListOfPressedKeys(this.m_oldPressedKeys);
                MyInput.Static.GetListOfPressedMouseButtons(this.m_oldPressedMouseButtons);
                base.m_closeOnEsc = false;
                base.CanBeHidden = true;
            }

            public override bool CloseScreen()
            {
                base.DrawMouseCursor = true;
                return base.CloseScreen();
            }

            public override void HandleInput(bool receivedFocusInThisUpdate)
            {
                base.HandleInput(receivedFocusInThisUpdate);
                if (MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
                {
                    this.Canceling();
                }
                if ((base.State != MyGuiScreenState.CLOSING) && (base.State != MyGuiScreenState.HIDING))
                {
                    switch (this.m_deviceType)
                    {
                        case MyGuiInputDeviceEnum.Keyboard:
                        case MyGuiInputDeviceEnum.KeyboardSecond:
                            this.HandleKey();
                            return;

                        case MyGuiInputDeviceEnum.Mouse:
                            this.HandleMouseButton();
                            break;

                        case (MyGuiInputDeviceEnum.Keyboard | MyGuiInputDeviceEnum.Mouse):
                        case ((MyGuiInputDeviceEnum) 4):
                            break;

                        default:
                            return;
                    }
                }
            }

            private void HandleKey()
            {
                this.ReadPressedKeys();
                using (List<MyKeys>.Enumerator enumerator = this.m_newPressedKeys.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyKeys key;
                        if (!this.m_oldPressedKeys.Contains(key))
                        {
                            if (!MyInput.Static.IsKeyValid(key))
                            {
                                this.ShowControlIsNotValidMessageBox();
                            }
                            else
                            {
                                MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                                MyControl ctrl = MyInput.Static.GetControl(key);
                                if (ctrl == null)
                                {
                                    this.m_controlBeingSet.SetControl(this.m_deviceType, key);
                                    this.CloseScreen();
                                }
                                else if (ctrl.Equals(this.m_controlBeingSet))
                                {
                                    this.OverwriteAssignment(ctrl, key);
                                    this.CloseScreen();
                                }
                                else
                                {
                                    StringBuilder output = null;
                                    MyControl.AppendName(ref output, key);
                                    this.ShowControlIsAlreadyAssigned(ctrl, output, () => this.OverwriteAssignment(ctrl, key));
                                }
                            }
                            break;
                        }
                    }
                }
                this.m_oldPressedKeys.Clear();
                MyUtils.Swap<List<MyKeys>>(ref this.m_oldPressedKeys, ref this.m_newPressedKeys);
            }

            private void HandleMouseButton()
            {
                MyInput.Static.GetListOfPressedMouseButtons(this.m_newPressedMouseButtons);
                using (List<MyMouseButtonsEnum>.Enumerator enumerator = this.m_newPressedMouseButtons.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyMouseButtonsEnum button;
                        if (!this.m_oldPressedMouseButtons.Contains(button))
                        {
                            MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
                            if (!MyInput.Static.IsMouseButtonValid(button))
                            {
                                this.ShowControlIsNotValidMessageBox();
                            }
                            else
                            {
                                MyControl ctrl = MyInput.Static.GetControl(button);
                                if (ctrl == null)
                                {
                                    this.m_controlBeingSet.SetControl(button);
                                    this.CloseScreen();
                                }
                                else if (ctrl.Equals(this.m_controlBeingSet))
                                {
                                    this.OverwriteAssignment(ctrl, button);
                                    this.CloseScreen();
                                }
                                else
                                {
                                    StringBuilder output = null;
                                    MyControl.AppendName(ref output, button);
                                    this.ShowControlIsAlreadyAssigned(ctrl, output, () => this.OverwriteAssignment(ctrl, button));
                                }
                            }
                            break;
                        }
                    }
                }
                this.m_oldPressedMouseButtons.Clear();
                MyUtils.Swap<List<MyMouseButtonsEnum>>(ref this.m_oldPressedMouseButtons, ref this.m_newPressedMouseButtons);
            }

            private MyGuiScreenMessageBox MakeControlIsAlreadyAssignedDialog(MyControl controlAlreadySet, StringBuilder controlButtonName)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                return MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.ControlAlreadyAssigned), controlButtonName, MyTexts.Get(controlAlreadySet.GetControlName()))), MyTexts.Get(MyCommonTexts.CanNotAssignControl), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            }

            private void OverwriteAssignment(MyControl controlAlreadySet, MyKeys key)
            {
                if (controlAlreadySet.GetKeyboardControl() == key)
                {
                    controlAlreadySet.SetControl(MyGuiInputDeviceEnum.Keyboard, MyKeys.None);
                }
                else
                {
                    controlAlreadySet.SetControl(MyGuiInputDeviceEnum.KeyboardSecond, MyKeys.None);
                }
                this.m_controlBeingSet.SetControl(this.m_deviceType, key);
            }

            private void OverwriteAssignment(MyControl controlAlreadySet, MyMouseButtonsEnum button)
            {
                controlAlreadySet.SetControl(MyMouseButtonsEnum.None);
                this.m_controlBeingSet.SetControl(button);
            }

            private void ReadPressedKeys()
            {
                MyInput.Static.GetListOfPressedKeys(this.m_newPressedKeys);
                this.m_newPressedKeys.Remove(MyKeys.Control);
                this.m_newPressedKeys.Remove(MyKeys.Shift);
                this.m_newPressedKeys.Remove(MyKeys.Alt);
                if (this.m_newPressedKeys.Contains(MyKeys.LeftControl) && this.m_newPressedKeys.Contains(MyKeys.RightAlt))
                {
                    this.m_newPressedKeys.Remove(MyKeys.LeftControl);
                }
            }

            private void ShowControlIsAlreadyAssigned(MyControl controlAlreadySet, StringBuilder controlButtonName, Action overwriteAssignmentCallback)
            {
                MyGuiScreenMessageBox screen = this.MakeControlIsAlreadyAssignedDialog(controlAlreadySet, controlButtonName);
                screen.ResultCallback = delegate (MyGuiScreenMessageBox.ResultEnum r) {
                    if (r == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        overwriteAssignmentCallback();
                        this.CloseScreen();
                    }
                    else
                    {
                        MyInput.Static.GetListOfPressedKeys(this.m_oldPressedKeys);
                        MyInput.Static.GetListOfPressedMouseButtons(this.m_oldPressedMouseButtons);
                    }
                };
                MyGuiSandbox.AddScreen(screen);
            }

            private void ShowControlIsNotValidMessageBox()
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.ControlIsNotValid), MyTexts.Get(MyCommonTexts.CanNotAssignControl), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }
    }
}

