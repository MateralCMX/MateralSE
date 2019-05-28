namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Globalization;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenTrigger : MyGuiScreenBase
    {
        private MyGuiControlLabel m_textboxName;
        protected MyGuiControlTextbox m_textboxMessage;
        private MyGuiControlLabel m_wwwLabel;
        protected MyGuiControlTextbox m_wwwTextbox;
        private MyGuiControlLabel m_nextMisLabel;
        protected MyGuiControlTextbox m_nextMisTextbox;
        protected MyGuiControlButton m_okButton;
        protected MyGuiControlButton m_cancelButton;
        protected MyTrigger m_trigger;
        protected const float VERTICAL_OFFSET = 0.005f;
        protected static readonly Vector2 RESERVED_SIZE = new Vector2(0f, 0.196f);
        protected static readonly Vector2 MIDDLE_PART_ORIGIN = ((-RESERVED_SIZE / 2f) + new Vector2(0f, 0.17f));

        public unsafe MyGuiScreenTrigger(MyTrigger trg, Vector2 size) : base(nullable, new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(size + RESERVED_SIZE), false, null, 0f, 0f)
        {
            Vector2? nullable = null;
            size += RESERVED_SIZE;
            Vector2 vector = new Vector2 {
                Y = (-size.Y / 2f) + 0.1f
            };
            nullable = null;
            VRageMath.Vector4? colorMask = null;
            this.m_textboxName = new MyGuiControlLabel(new Vector2?(vector), nullable, MyTexts.Get(MySpaceTexts.GuiTriggerMessage).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            float* singlePtr1 = (float*) ref vector.Y;
            singlePtr1[0] += this.m_textboxName.Size.Y + 0.005f;
            this.m_trigger = trg;
            colorMask = null;
            this.m_textboxMessage = new MyGuiControlTextbox(new Vector2?(vector), trg.Message, 0x55, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_textboxName.Position -= new Vector2(this.m_textboxMessage.Size.X / 2f, 0f);
            this.Controls.Add(this.m_textboxName);
            this.Controls.Add(this.m_textboxMessage);
            nullable = base.Size;
            vector.Y = (nullable.Value.Y * 0.5f) - 0.3f;
            nullable = null;
            colorMask = null;
            this.m_wwwLabel = new MyGuiControlLabel(new Vector2?(vector), nullable, string.Format(MyTexts.GetString(MySpaceTexts.GuiTriggerWwwLink), MySession.Platform), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            float* singlePtr2 = (float*) ref vector.Y;
            singlePtr2[0] += this.m_wwwLabel.Size.Y + 0.005f;
            colorMask = null;
            this.m_wwwTextbox = new MyGuiControlTextbox(new Vector2?(vector), trg.WwwLink, 300, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            float* singlePtr3 = (float*) ref vector.Y;
            singlePtr3[0] += this.m_wwwTextbox.Size.Y + 0.005f;
            this.m_wwwLabel.Position -= new Vector2(this.m_wwwTextbox.Size.X / 2f, 0f);
            this.m_wwwTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.OnWwwTextChanged);
            this.Controls.Add(this.m_wwwLabel);
            this.Controls.Add(this.m_wwwTextbox);
            nullable = null;
            colorMask = null;
            this.m_nextMisLabel = new MyGuiControlLabel(new Vector2?(vector), nullable, MyTexts.Get(MySpaceTexts.GuiTriggerNextMission).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            float* singlePtr4 = (float*) ref vector.Y;
            singlePtr4[0] += this.m_wwwLabel.Size.Y + 0.005f;
            colorMask = null;
            this.m_nextMisTextbox = new MyGuiControlTextbox(new Vector2?(vector), this.m_trigger.NextMission, 300, colorMask, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            float* singlePtr5 = (float*) ref vector.Y;
            singlePtr5[0] += this.m_wwwTextbox.Size.Y + 0.005f;
            this.m_nextMisLabel.Position -= new Vector2(this.m_nextMisTextbox.Size.X / 2f, 0f);
            this.m_nextMisTextbox.SetToolTip(string.Format(MyTexts.GetString(MySpaceTexts.GuiTriggerNextMissionTooltip), MySession.Platform));
            this.Controls.Add(this.m_nextMisLabel);
            this.Controls.Add(this.m_nextMisTextbox);
            nullable = base.Size;
            Vector2 vector2 = new Vector2(0f, (nullable.Value.Y * 0.5f) - 0.05f);
            Vector2 vector3 = new Vector2(0.01f, 0f);
            nullable = null;
            nullable = null;
            colorMask = null;
            int? buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(nullable, MyGuiControlButtonStyleEnum.Default, nullable, colorMask, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_okButton.Position = vector2 - vector3;
            nullable = null;
            nullable = null;
            colorMask = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(nullable, MyGuiControlButtonStyleEnum.Default, nullable, colorMask, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_cancelButton.Position = vector2 + vector3;
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            this.OnWwwTextChanged(this.m_wwwTextbox);
        }

        public override bool CloseScreen()
        {
            this.m_wwwTextbox.TextChanged -= new Action<MyGuiControlTextbox>(this.OnWwwTextChanged);
            return base.CloseScreen();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTrigger";

        private void OnCancelButtonClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        protected virtual void OnOkButtonClick(MyGuiControlButton sender)
        {
            this.m_trigger.Message = this.m_textboxMessage.Text;
            this.m_trigger.WwwLink = this.m_wwwTextbox.Text;
            this.m_trigger.NextMission = this.m_nextMisTextbox.Text;
            this.CloseScreen();
        }

        private void OnWwwTextChanged(MyGuiControlTextbox source)
        {
            if ((source.Text.Length == 0) || MyGuiSandbox.IsUrlWhitelisted(source.Text))
            {
                source.ColorMask = VRageMath.Vector4.One;
                source.SetToolTip((MyToolTips) null);
                this.m_okButton.Enabled = true;
            }
            else
            {
                MyStringId text = !MySession.Platform.Equals("Steam") ? MySpaceTexts.WwwLinkNotAllowed : MySpaceTexts.WwwLinkNotAllowed_Steam;
                this.m_wwwTextbox.SetToolTip(text);
                source.ColorMask = Color.Red.ToVector4();
                this.m_okButton.Enabled = false;
            }
        }

        protected double? StrToDouble(string str)
        {
            double num;
            try
            {
                num = double.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
            return new double?(num);
        }

        protected int? StrToInt(string str)
        {
            int num;
            try
            {
                num = int.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return null;
            }
            return new int?(num);
        }
    }
}

