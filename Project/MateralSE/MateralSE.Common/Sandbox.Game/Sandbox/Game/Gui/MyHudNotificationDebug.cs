namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    public class MyHudNotificationDebug : MyHudNotificationBase
    {
        private string m_originalText;

        public MyHudNotificationDebug(string text, int disapearTimeMs = 0x9c4, string font = "White", MyGuiDrawAlignEnum textAlign = 4, int priority = 0, MyNotificationLevel level = 3) : base(disapearTimeMs, font, textAlign, priority, level)
        {
            this.m_originalText = text;
        }

        protected override string GetOriginalText() => 
            this.m_originalText;
    }
}

