namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenAdvancedScenarioSettings : MyGuiScreenBase
    {
        private MyGuiScreenMissionTriggers m_parent;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlCheckbox m_canJoinRunning;
        [CompilerGenerated]
        private Action OnOkButtonClicked;

        public event Action OnOkButtonClicked
        {
            [CompilerGenerated] add
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action a = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, a);
                    if (ReferenceEquals(onOkButtonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action source = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, source);
                    if (ReferenceEquals(onOkButtonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenAdvancedScenarioSettings(MyGuiScreenMissionTriggers parent) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.9f, 0.9f), false, null, 0f, 0f)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedScenarioSettings.ctor START");
            this.m_parent = parent;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedScenarioSettings.ctor END");
        }

        public void BuildControls()
        {
            Vector2? position = null;
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlScrollablePanel control = new MyGuiControlScrollablePanel(new MyGuiControlParent(position, new Vector2(base.Size.Value.X - 0.05f, base.Size.Value.Y - 0.1f), backgroundColor, null)) {
                ScrollbarVEnabled = true
            };
            position = base.Size;
            control.Size = new Vector2(position.Value.X - 0.05f, 0.8f);
            this.Controls.Add(control);
            Vector2 vector = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 vector2 = (base.m_size.Value / 2f) - new Vector2(0.23f, 0.03f);
            backgroundColor = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(new Vector2?(vector2 - new Vector2(0.01f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            backgroundColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(new Vector2?(vector2 + new Vector2(0.01f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector), backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.CancelButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            float num = 0.055f;
            MyGuiControlLabel label = this.MakeLabel(MySpaceTexts.ScenarioSettings_CanJoinRunning);
            position = null;
            backgroundColor = null;
            this.m_canJoinRunning = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_canJoinRunning.Position = new Vector2((-control.Size.X / 2f) + num, (-control.Size.Y / 2f) + num);
            label.Position = new Vector2(this.m_canJoinRunning.Position.X + num, this.m_canJoinRunning.Position.Y);
            this.m_canJoinRunning.IsChecked = MySession.Static.Settings.CanJoinRunning;
            MyGuiControlParent parent1 = new MyGuiControlParent(position, new Vector2(base.Size.Value.X - 0.05f, base.Size.Value.Y - 0.1f), backgroundColor, null);
            parent1.Controls.Add(this.m_canJoinRunning);
            parent1.Controls.Add(label);
            base.CloseButtonEnabled = true;
        }

        private void CancelButtonClicked(object sender)
        {
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenAdvancedScenarioSettings";

        public void GetSettings(MyObjectBuilder_SessionSettings settings)
        {
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void OkButtonClicked(object sender)
        {
            MySession.Static.Settings.CanJoinRunning = this.m_canJoinRunning.IsChecked;
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
        }

        public void SetSettings(MyObjectBuilder_SessionSettings settings)
        {
        }
    }
}

