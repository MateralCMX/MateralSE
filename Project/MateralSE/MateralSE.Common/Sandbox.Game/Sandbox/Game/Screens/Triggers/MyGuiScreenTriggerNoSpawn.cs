namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;

    internal class MyGuiScreenTriggerNoSpawn : MyGuiScreenTriggerTime
    {
        public MyGuiScreenTriggerNoSpawn(MyTrigger trg) : base(trg, MySpaceTexts.GuiTriggerNoSpawnTimeLimit)
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionNoSpawn, captionTextColor, captionOffset, 0.8f);
            base.m_textboxTime.Text = ((MyTriggerNoSpawn) trg).LimitInSeconds.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerNoSpawn";

        public override bool IsValid(int time) => 
            (time >= 15);

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            int? nullable = base.StrToInt(base.m_textboxTime.Text);
            if (nullable != null)
            {
                ((MyTriggerNoSpawn) base.m_trigger).LimitInSeconds = nullable.Value;
            }
            base.OnOkButtonClick(sender);
        }
    }
}

