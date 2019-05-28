namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Library;
    using VRage.Utils;

    public class MyGuiControlCountdownWheel : MyGuiControlRotatingWheel
    {
        private StringBuilder m_sb;
        private int m_time;
        private int m_shownTime;

        public MyGuiControlCountdownWheel(Vector2? position = new Vector2?(), string texture = @"Textures\GUI\screens\screen_loading_wheel.dds", Vector2? textureResolution = new Vector2?(), int seconds = 10, float radiansPerSecond = 6.283185f, float scale = 0.36f) : base(position, nullable2, scale, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, str, true, false, textureResolution, radiansPerSecond)
        {
            string str = texture;
            this.m_sb = new StringBuilder();
            this.m_sb.Append(seconds);
            this.m_time = (MyEnvironment.TickCount + (seconds * 0x3e8)) + 0x3e7;
            this.m_shownTime = seconds;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            Color? colorMask = null;
            MyGuiManager.DrawString("White", this.m_sb, base.GetPositionAbsoluteCenter(), 1f, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, float.PositiveInfinity);
        }

        public override void Update()
        {
            base.Update();
            int num = (this.m_time - MyEnvironment.TickCount) / 0x3e8;
            if (num < 0)
            {
                num = 0;
            }
            if (num != this.m_shownTime)
            {
                this.m_shownTime = num;
                this.m_sb.Clear();
                if (this.m_shownTime > 0)
                {
                    this.m_sb.Append(this.m_shownTime);
                }
            }
        }
    }
}

