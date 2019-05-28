namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlOnOffSwitch))]
    public class MyGuiControlOnOffSwitch : MyGuiControlBase
    {
        private MyGuiControlCheckbox m_onButton;
        private MyGuiControlLabel m_onLabel;
        private MyGuiControlCheckbox m_offButton;
        private MyGuiControlLabel m_offLabel;
        private bool m_value;
        [CompilerGenerated]
        private Action<MyGuiControlOnOffSwitch> ValueChanged;

        public event Action<MyGuiControlOnOffSwitch> ValueChanged
        {
            [CompilerGenerated] add
            {
                Action<MyGuiControlOnOffSwitch> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<MyGuiControlOnOffSwitch> a = valueChanged;
                    Action<MyGuiControlOnOffSwitch> action3 = (Action<MyGuiControlOnOffSwitch>) Delegate.Combine(a, value);
                    valueChanged = Interlocked.CompareExchange<Action<MyGuiControlOnOffSwitch>>(ref this.ValueChanged, action3, a);
                    if (ReferenceEquals(valueChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyGuiControlOnOffSwitch> valueChanged = this.ValueChanged;
                while (true)
                {
                    Action<MyGuiControlOnOffSwitch> source = valueChanged;
                    Action<MyGuiControlOnOffSwitch> action3 = (Action<MyGuiControlOnOffSwitch>) Delegate.Remove(source, value);
                    valueChanged = Interlocked.CompareExchange<Action<MyGuiControlOnOffSwitch>>(ref this.ValueChanged, action3, source);
                    if (ReferenceEquals(valueChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiControlOnOffSwitch(bool initialValue = false, string onText = null, string offText = null) : base(position, position, color, null, null, true, true, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            Vector2? position = null;
            position = null;
            Vector4? color = null;
            position = null;
            color = null;
            this.m_onButton = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.SwitchOnOffLeft, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
            position = null;
            color = null;
            this.m_offButton = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.SwitchOnOffRight, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            position = null;
            color = null;
            this.m_onLabel = new MyGuiControlLabel(new Vector2(this.m_onButton.Size.X * -0.5f, 0f), position, onText ?? MyTexts.GetString(MySpaceTexts.SwitchText_On), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            color = null;
            this.m_offLabel = new MyGuiControlLabel(new Vector2(this.m_onButton.Size.X * 0.5f, 0f), position, offText ?? MyTexts.GetString(MySpaceTexts.SwitchText_Off), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            base.Size = new Vector2(this.m_onButton.Size.X + this.m_offButton.Size.X, Math.Max(this.m_onButton.Size.Y, this.m_offButton.Size.Y));
            base.Elements.Add(this.m_onButton);
            base.Elements.Add(this.m_offButton);
            base.Elements.Add(this.m_onLabel);
            base.Elements.Add(this.m_offLabel);
            this.m_value = initialValue;
            this.UpdateButtonState();
        }

        public override MyGuiControlBase HandleInput()
        {
            MyGuiControlBase base2 = base.HandleInput();
            if (base2 == null)
            {
                bool flag = MyInput.Static.IsNewLeftMouseReleased() || MyControllerHelper.IsControl(MyControllerHelper.CX_GUI, MyControlsGUI.ACCEPT, MyControlStateType.NEW_RELEASED, false);
                if (((base.Enabled && base.IsMouseOver) & flag) || (base.HasFocus && MyInput.Static.IsNewKeyPressed(MyKeys.Enter)))
                {
                    this.Value = !this.Value;
                    base2 = this;
                    MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                }
            }
            return base2;
        }

        public override void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            base.Size = new Vector2(this.m_onButton.Size.X + this.m_offButton.Size.X, Math.Max(this.m_onButton.Size.Y, this.m_offButton.Size.Y));
            this.UpdateButtonState();
        }

        protected override void OnVisibleChanged()
        {
            if (this.m_onButton != null)
            {
                this.m_onButton.Visible = base.Visible;
            }
            if (this.m_offButton != null)
            {
                this.m_offButton.Visible = base.Visible;
            }
            base.OnVisibleChanged();
        }

        private void UpdateButtonState()
        {
            this.m_onButton.IsChecked = this.Value;
            this.m_offButton.IsChecked = !this.Value;
            this.m_onLabel.Font = this.Value ? "White" : "Blue";
            this.m_offLabel.Font = this.Value ? "Blue" : "White";
        }

        public bool Value
        {
            get => 
                this.m_value;
            set
            {
                if (this.m_value != value)
                {
                    this.m_value = value;
                    this.UpdateButtonState();
                    if (this.ValueChanged != null)
                    {
                        this.ValueChanged(this);
                    }
                }
            }
        }
    }
}

