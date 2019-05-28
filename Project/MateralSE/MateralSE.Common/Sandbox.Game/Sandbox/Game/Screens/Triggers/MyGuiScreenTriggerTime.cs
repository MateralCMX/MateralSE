namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Utils;
    using VRageMath;

    public abstract class MyGuiScreenTriggerTime : MyGuiScreenTrigger
    {
        private MyGuiControlLabel m_labelTime;
        protected MyGuiControlTextbox m_textboxTime;
        private const float WINSIZEX = 0.4f;
        private const float WINSIZEY = 0.37f;
        private const float spacingH = 0.01f;

        public MyGuiScreenTriggerTime(MyTrigger trg, MyStringId labelText) : base(trg, new Vector2(0.5f, 0.37f))
        {
            float x = base.m_textboxMessage.Position.X - (base.m_textboxMessage.Size.X / 2f);
            float y = -0.185f + MyGuiScreenTrigger.MIDDLE_PART_ORIGIN.Y;
            Vector4? colorMask = null;
            this.m_labelTime = new MyGuiControlLabel(new Vector2(x, y), new Vector2(0.013f, 0.035f), MyTexts.Get(labelText).ToString(), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            x += this.m_labelTime.Size.X + 0.01f;
            MyGuiControlTextbox textbox1 = new MyGuiControlTextbox();
            textbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            textbox1.Position = new Vector2(x, y);
            textbox1.Size = new Vector2(0.05f, 0.035f);
            textbox1.Type = MyGuiControlTextboxType.DigitsOnly;
            textbox1.Name = "time";
            this.m_textboxTime = textbox1;
            this.m_textboxTime.TextChanged += new Action<MyGuiControlTextbox>(this.OnTimeChanged);
            this.Controls.Add(this.m_labelTime);
            this.Controls.Add(this.m_textboxTime);
        }

        public virtual bool IsValid(int time) => 
            true;

        public void OnTimeChanged(MyGuiControlTextbox sender)
        {
            int? nullable = base.StrToInt(sender.Text);
            if ((nullable == null) || !this.IsValid(nullable.Value))
            {
                sender.ColorMask = Color.Red.ToVector4();
                base.m_okButton.Enabled = false;
            }
            else
            {
                sender.ColorMask = Vector4.One;
                base.m_okButton.Enabled = true;
            }
        }
    }
}

