namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Utils;

    public class MyHudMissingComponentNotification : MyHudNotificationBase
    {
        private MyStringId m_originalText;

        public MyHudMissingComponentNotification(MyStringId text, int disapearTimeMs = 0x9c4, string font = "White", MyGuiDrawAlignEnum textAlign = 4, int priority = 0, MyNotificationLevel level = 0) : base(disapearTimeMs, font, textAlign, priority, level)
        {
            this.m_originalText = text;
        }

        public override void BeforeAdd()
        {
            MyHud.BlockInfo.MissingComponentIndex = 0;
        }

        public override void BeforeRemove()
        {
            MyHud.BlockInfo.MissingComponentIndex = -1;
        }

        protected override string GetOriginalText() => 
            MyTexts.GetString(this.m_originalText);

        public void SetBlockDefinition(MyCubeBlockDefinition definition)
        {
            object[] arguments = new object[] { definition.Components[0].Definition.DisplayNameText.ToString(), definition.DisplayNameText.ToString() };
            base.SetTextFormatArguments(arguments);
        }
    }
}

