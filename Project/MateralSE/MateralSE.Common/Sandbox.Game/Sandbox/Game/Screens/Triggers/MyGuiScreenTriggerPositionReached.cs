namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;

    public class MyGuiScreenTriggerPositionReached : MyGuiScreenTriggerPosition
    {
        public MyGuiScreenTriggerPositionReached(MyTrigger trg) : base(trg)
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionPositionReached, captionTextColor, captionOffset, 0.8f);
            base.m_xCoord.Text = ((MyTriggerPositionReached) trg).TargetPos.X.ToString();
            base.m_yCoord.Text = ((MyTriggerPositionReached) trg).TargetPos.Y.ToString();
            base.m_zCoord.Text = ((MyTriggerPositionReached) trg).TargetPos.Z.ToString();
            base.m_radius.Text = ((MyTriggerPositionReached) trg).Radius.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerPositionReached";

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            double? nullable = base.StrToDouble(base.m_radius.Text);
            if (nullable != null)
            {
                ((MyTriggerPositionReached) base.m_trigger).Radius = nullable.Value;
            }
            if (base.m_coordsChanged)
            {
                ((MyTriggerPositionReached) base.m_trigger).TargetPos = base.m_coords;
            }
            base.OnOkButtonClick(sender);
        }
    }
}

