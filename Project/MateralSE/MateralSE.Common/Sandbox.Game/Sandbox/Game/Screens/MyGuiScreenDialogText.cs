namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Graphics.GUI;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyGuiScreenDialogText : MyGuiScreenBase
    {
        private MyGuiControlLabel m_captionLabel;
        private MyGuiControlTextbox m_valueTextbox;
        private MyGuiControlButton m_confirmButton;
        private MyGuiControlButton m_cancelButton;
        private MyStringId m_caption;
        private readonly string m_value;
        [CompilerGenerated]
        private Action<string> OnConfirmed;

        public event Action<string> OnConfirmed
        {
            [CompilerGenerated] add
            {
                Action<string> onConfirmed = this.OnConfirmed;
                while (true)
                {
                    Action<string> a = onConfirmed;
                    Action<string> action3 = (Action<string>) Delegate.Combine(a, value);
                    onConfirmed = Interlocked.CompareExchange<Action<string>>(ref this.OnConfirmed, action3, a);
                    if (ReferenceEquals(onConfirmed, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<string> onConfirmed = this.OnConfirmed;
                while (true)
                {
                    Action<string> source = onConfirmed;
                    Action<string> action3 = (Action<string>) Delegate.Remove(source, value);
                    onConfirmed = Interlocked.CompareExchange<Action<string>>(ref this.OnConfirmed, action3, source);
                    if (ReferenceEquals(onConfirmed, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenDialogText(string initialValue = null, MyStringId? caption = new MyStringId?()) : base(nullable, nullable2, nullable, false, null, 0f, 0f)
        {
            Vector2? nullable = null;
            nullable = null;
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
            this.m_value = initialValue ?? string.Empty;
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = true;
            MyStringId? nullable3 = caption;
            this.m_caption = (nullable3 != null) ? nullable3.GetValueOrDefault() : MyCommonTexts.DialogAmount_SetValueCaption;
            this.RecreateControls(true);
        }

        private void cancelButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        private void confirmButton_OnButtonClick(MyGuiControlButton sender)
        {
            if (this.OnConfirmed != null)
            {
                this.OnConfirmed(this.m_valueTextbox.Text);
            }
            this.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDialogText";

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            base.HandleUnhandledInput(receivedFocusInThisUpdate);
            if (MyInput.Static.IsNewKeyPressed(MyKeys.Enter))
            {
                this.confirmButton_OnButtonClick(this.m_confirmButton);
            }
        }

        public override void RecreateControls(bool contructor)
        {
            MyObjectBuilder_GuiScreen screen;
            base.RecreateControls(contructor);
            string str = MakeScreenFilepath("DialogText");
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_GuiScreen>(Path.Combine(MyFileSystem.ContentPath, str), out screen);
            base.Init(screen);
            this.m_valueTextbox = (MyGuiControlTextbox) this.Controls.GetControlByName("ValueTextbox");
            this.m_confirmButton = (MyGuiControlButton) this.Controls.GetControlByName("ConfirmButton");
            this.m_cancelButton = (MyGuiControlButton) this.Controls.GetControlByName("CancelButton");
            this.m_captionLabel = (MyGuiControlLabel) this.Controls.GetControlByName("CaptionLabel");
            this.m_captionLabel.Text = null;
            this.m_captionLabel.TextEnum = this.m_caption;
            this.m_confirmButton.ButtonClicked += new Action<MyGuiControlButton>(this.confirmButton_OnButtonClick);
            this.m_cancelButton.ButtonClicked += new Action<MyGuiControlButton>(this.cancelButton_OnButtonClick);
            this.m_valueTextbox.Text = this.m_value;
        }
    }
}

