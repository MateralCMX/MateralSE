namespace Sandbox.Game.Screens.Triggers
{
    using Sandbox.Game.Localization;
    using Sandbox.Game.World.Triggers;
    using Sandbox.Graphics.GUI;
    using System;

    public class MyGuiScreenTriggerPositionLeft : MyGuiScreenTriggerPosition
    {
        public MyGuiScreenTriggerPositionLeft(MyTrigger trg) : base(trg)
        {
            Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MySpaceTexts.GuiTriggerCaptionPositionLeft, captionTextColor, captionOffset, 0.8f);
            base.m_xCoord.Text = ((MyTriggerPositionLeft) trg).TargetPos.X.ToString();
            base.m_yCoord.Text = ((MyTriggerPositionLeft) trg).TargetPos.Y.ToString();
            base.m_zCoord.Text = ((MyTriggerPositionLeft) trg).TargetPos.Z.ToString();
            base.m_radius.Text = ((MyTriggerPositionLeft) trg).Radius.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenTriggerPositionLeft";

        protected override void OnOkButtonClick(MyGuiControlButton sender)
        {
            double? nullable = base.StrToDouble(base.m_radius.Text);
            if (nullable != null)
            {
                ((MyTriggerPositionLeft) base.m_trigger).Radius = nullable.Value;
            }
            if (base.m_coordsChanged)
            {
                ((MyTriggerPositionLeft) base.m_trigger).TargetPos = base.m_coords;
            }
            base.OnOkButtonClick(sender);
        }
    }
}

