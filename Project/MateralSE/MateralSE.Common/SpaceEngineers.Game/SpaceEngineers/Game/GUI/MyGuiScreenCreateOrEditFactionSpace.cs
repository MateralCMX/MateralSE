namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenCreateOrEditFactionSpace : MyGuiScreenCreateOrEditFaction
    {
        public MyGuiScreenCreateOrEditFactionSpace()
        {
        }

        public MyGuiScreenCreateOrEditFactionSpace(ref IMyFaction editData) : base(ref editData)
        {
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenCreateOrEditFactionSpace";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MySpaceTexts.TerminalTab_Factions_EditFaction, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.78f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.78f, 0f, captionTextColor);
            this.Controls.Add(list2);
            float x = -0.19f;
            float y = -0.153f;
            float num3 = 0.045f;
            Vector2 vector = new Vector2(0.29f, 0.052f);
            Vector2? size = new Vector2?(vector);
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(x, y + num3), size, MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_CreateFactionTag), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            size = new Vector2?(vector);
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(x, y + (2f * num3)), size, MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_CreateFactionName), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            size = new Vector2?(vector);
            captionTextColor = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2(x, y + (3f * num3)), size, MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_CreateFactionDescription), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            size = new Vector2?(vector);
            captionTextColor = null;
            MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2(x, y + (4f * num3)), size, MyTexts.GetString(MySpaceTexts.TerminalTab_Factions_CreateFactionPrivateInfo), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.Controls.Add(label);
            this.Controls.Add(label2);
            this.Controls.Add(label3);
            this.Controls.Add(label4);
            x += 0.268f;
            y += 0.055f;
            Vector2 vector2 = new Vector2(0.23f, 0.1f);
            captionTextColor = null;
            this.m_shortcut = new MyGuiControlTextbox(new Vector2(x, y), (base.m_editFaction != null) ? base.m_editFaction.Tag : "", 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            captionTextColor = null;
            this.m_name = new MyGuiControlTextbox(new Vector2(x, y + num3), (base.m_editFaction != null) ? MyStatControlText.SubstituteTexts(base.m_editFaction.Name, null) : "", 0x40, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            captionTextColor = null;
            this.m_desc = new MyGuiControlTextbox(new Vector2(x, y + (2f * num3)), (base.m_editFaction != null) ? base.m_editFaction.Description : "", 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            captionTextColor = null;
            this.m_privInfo = new MyGuiControlTextbox(new Vector2(x, y + (3f * num3)), (base.m_editFaction != null) ? base.m_editFaction.PrivateInfo : "", 0x200, captionTextColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            base.m_shortcut.Size = vector2;
            base.m_name.Size = vector2;
            base.m_desc.Size = vector2;
            base.m_privInfo.Size = vector2;
            base.m_shortcut.SetToolTip(MySpaceTexts.TerminalTab_Factions_CreateFactionTagToolTip);
            base.m_privInfo.SetToolTip(MySpaceTexts.TerminalTab_Factions_CreateFactionPrivateInfoToolTip);
            base.m_name.SetToolTip(MyCommonTexts.MessageBoxErrorFactionsNameTooShort);
            base.m_desc.SetToolTip(MySpaceTexts.TerminalTab_Factions_CreateFactionPublicInfoToolTip);
            this.Controls.Add(base.m_shortcut);
            this.Controls.Add(base.m_name);
            this.Controls.Add(base.m_desc);
            this.Controls.Add(base.m_privInfo);
            y -= 0.003f;
            Vector2 vector3 = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.041f);
            Vector2 vector4 = new Vector2(0.018f, 0f);
            size = new Vector2?(vector);
            captionTextColor = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.Ok);
            int? buttonIndex = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2?(vector3 - vector4), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Ok), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, buttonIndex, false));
            size = new Vector2?(vector);
            captionTextColor = null;
            text = MyTexts.Get(MyCommonTexts.Cancel);
            buttonIndex = null;
            this.Controls.Add(new MyGuiControlButton(new Vector2?(vector3 + vector4), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, buttonIndex, false));
        }
    }
}

