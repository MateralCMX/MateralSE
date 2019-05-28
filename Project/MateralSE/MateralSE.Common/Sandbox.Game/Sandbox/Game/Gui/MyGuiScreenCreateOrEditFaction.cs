namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game.ModAPI;

    public class MyGuiScreenCreateOrEditFaction : MyGuiScreenBase
    {
        protected MyGuiControlTextbox m_shortcut;
        protected MyGuiControlTextbox m_name;
        protected MyGuiControlTextbox m_desc;
        protected MyGuiControlTextbox m_privInfo;
        protected IMyFaction m_editFaction;

        public MyGuiScreenCreateOrEditFaction() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.4169847f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = false;
        }

        public MyGuiScreenCreateOrEditFaction(ref IMyFaction editData) : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.4971429f, 0.4169847f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.CanHideOthers = false;
            base.EnabledBackgroundFade = false;
            this.m_editFaction = editData;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenCreateOrEditFaction";

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
        }

        public void Init(ref IMyFaction editData)
        {
            this.m_editFaction = editData;
            this.RecreateControls(true);
        }

        protected void OnCancelClick(MyGuiControlButton sender)
        {
            this.CloseScreenNow();
        }

        protected void OnOkClick(MyGuiControlButton sender)
        {
            this.m_shortcut.Text = this.m_shortcut.Text.Replace(" ", string.Empty);
            this.m_name.Text = this.m_name.Text.Trim();
            if (this.m_shortcut.Text.Length != 3)
            {
                this.ShowErrorBox(MyTexts.Get(MyCommonTexts.MessageBoxErrorFactionsTag));
            }
            else if (MySession.Static.Factions.FactionTagExists(this.m_shortcut.Text, this.m_editFaction))
            {
                this.ShowErrorBox(MyTexts.Get(MyCommonTexts.MessageBoxErrorFactionsTagAlreadyExists));
            }
            else if (this.m_name.Text.Length < 4)
            {
                this.ShowErrorBox(MyTexts.Get(MyCommonTexts.MessageBoxErrorFactionsNameTooShort));
            }
            else if (MySession.Static.Factions.FactionNameExists(this.m_name.Text, this.m_editFaction))
            {
                this.ShowErrorBox(MyTexts.Get(MyCommonTexts.MessageBoxErrorFactionsNameAlreadyExists));
            }
            else if (this.m_editFaction != null)
            {
                MySession.Static.Factions.EditFaction(this.m_editFaction.FactionId, this.m_shortcut.Text, this.m_name.Text, this.m_desc.Text, this.m_privInfo.Text);
                this.CloseScreenNow();
            }
            else
            {
                MySession.Static.Factions.CreateFaction(MySession.Static.LocalPlayerId, this.m_shortcut.Text, this.m_name.Text, this.m_desc.Text, this.m_privInfo.Text);
                this.CloseScreenNow();
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
        }

        protected void ShowErrorBox(StringBuilder text)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, text, messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size);
            screen.SkipTransition = true;
            screen.CloseBeforeCallback = true;
            screen.CanHideOthers = false;
            MyGuiSandbox.AddScreen(screen);
        }
    }
}

