namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenTriggerLives : MyGuiScreenTrigger
    {
        private MyGuiControlLabel m_labelLives;
        protected MyGuiControlTextbox m_lives;
        private const float WINSIZEX = 0.4f;
        private const float WINSIZEY = 0.37f;
        private const float spacingH = 0.01f;

        public MyGuiScreenTriggerLives(MyTrigger trg) : base(trg, new Vector2(0.5f, 0.37f))
        {
            float x = base.m_textboxMessage.Position.X - (base.m_textboxMessage.Size.X / 2f);
            float y = -0.185f + MyGuiScreenTrigger.MIDDLE_PART_ORIGIN.Y;
            Vector4? colorMask = null;
            this.m_labelLives = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.01f, 0.035f), MyTexts.Get(MySpaceTexts.GuiTriggersLives).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelLives.Size.X + 0.01f;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox1.Position = new Vector2(x, y);
            textbox1.Size = new Vector2(0.11f - this.m_labelLives.Size.X, 0.035f);
            textbox1.Type = MyGuiControlTextboxType.DigitsOnly;
            textbox1.Name = "lives";
            this.m_lives = textbox1;
            this.m_lives.TextChanged += new Action<MyGuiControlTextbox>(this.OnLivesChanged);
            colorMask = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionLives, colorMask, captionOffset, 0.8f);
            this.Controls.Add(this.m_labelLives);
            this.Controls.Add(this.m_lives);
            this.m_lives.Text = ((MyTriggerLives) trg).LivesLeft.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerLives";

        public void OnLivesChanged(MyGuiControlTextbox sender)
        {
            int? nullable = base.StrToInt(sender.Text);
            if (nullable != null)
            {
                int? nullable2 = nullable;
                int num = 0;
                if ((nullable2.GetValueOrDefault() > num) & (nullable2 != null))
                {
                    sender.ColorMask = Vector4.One;
                    base.m_okButton.Enabled = true;
                    return;
                }
            }
            sender.ColorMask = Color.Red.ToVector4();
            base.m_okButton.Enabled = false;
        }

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            int? nullable = base.StrToInt(this.m_lives.Text);
            if (nullable != null)
            {
                ((MyTriggerLives) base.m_trigger).LivesLeft = nullable.Value;
            }
            base.OnOkButtonClick(sender);
        }
    }
}

