namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenDebugInput : MyGuiScreenDebugBase
    {
        private static StringBuilder m_debugText = new StringBuilder(0x3e8);

        public MyGuiScreenDebugInput() : base(new Vector2(0.5f, 0.5f), new Vector2?(vector), nullable, true)
        {
            Vector2 vector = new Vector2();
            base.m_isTopMostScreen = true;
            base.m_drawEvenWithoutFocus = true;
            base.CanHaveFocus = false;
        }

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            this.SetTexts();
            Vector2 screenLeftTopPosition = this.GetScreenLeftTopPosition();
            MyGuiManager.DrawString("White", m_debugText, screenLeftTopPosition, MyGuiConstants.DEBUG_STATISTICS_TEXT_SCALE, new Color?(Color.Yellow), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, float.PositiveInfinity);
            return true;
        }

        public override string GetFriendlyName() => 
            "DebugInputScreen";

        public Vector2 GetScreenLeftTopPosition()
        {
            MyGuiManager.GetSafeFullscreenRectangle();
            return MyGuiManager.GetNormalizedCoordinateFromScreenCoordinate_FULLSCREEN(new Vector2(25f * MyGuiManager.GetSafeScreenScale(), 25f * MyGuiManager.GetSafeScreenScale()));
        }

        public void SetTexts()
        {
            m_debugText.Clear();
            MyInput.Static.GetActualJoystickState(m_debugText);
        }
    }
}

