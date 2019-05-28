namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;

    internal class MyGuiScreenTriggerTimeLimit : MyGuiScreenTriggerTime
    {
        public MyGuiScreenTriggerTimeLimit(MyTrigger trg) : base(trg, MySpaceTexts.GuiTriggerTimeLimit)
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionTimeLimit, captionTextColor, captionOffset, 0.8f);
            base.m_textboxTime.Text = ((MyTriggerTimeLimit) trg).LimitInMinutes.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerTimeLimit";

        public override bool IsValid(int time) => 
            (time > 0);

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            int? nullable = base.StrToInt(base.m_textboxTime.Text);
            if (nullable != null)
            {
                ((MyTriggerTimeLimit) base.m_trigger).LimitInMinutes = nullable.Value;
            }
            base.OnOkButtonClick(sender);
        }
    }
}

