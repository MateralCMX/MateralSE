namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using System;

    public class MyGuiScreenTriggerSomeoneWon : MyGuiScreenTrigger
    {
        public MyGuiScreenTriggerSomeoneWon(MyTrigger trg) : base(trg, new Vector2(0.5f, 0.3f))
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionSomeoneWon, captionTextColor, captionOffset, 0.8f);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerSomeoneWon";
    }
}

