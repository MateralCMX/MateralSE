namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using System;

    public class MyGuiScreenTriggerAllOthersLost : MyGuiScreenTrigger
    {
        public MyGuiScreenTriggerAllOthersLost(MyTrigger trg) : base(trg, new Vector2(0.5f, 0.3f))
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionAllOthersLost, captionTextColor, captionOffset, 0.8f);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerAllOthersLost";
    }
}

