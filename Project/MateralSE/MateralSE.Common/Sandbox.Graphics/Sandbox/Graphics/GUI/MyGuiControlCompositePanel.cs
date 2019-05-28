namespace Sandbox.Graphics.GUI
{
    using System;
    using VRage.Game;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlCompositePanel))]
    public class MyGuiControlCompositePanel : MyGuiControlPanel
    {
        private float m_innerHeight;

        public MyGuiControlCompositePanel()
        {
            base.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_NEUTRAL;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.BackgroundTexture.Draw(base.GetPositionAbsoluteTopLeft(), base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, backgroundTransitionAlpha), 1f);
            base.DrawBorder(transitionAlpha);
        }

        public override MyObjectBuilder_GuiControlBase GetObjectBuilder()
        {
            MyObjectBuilder_GuiControlCompositePanel objectBuilder = (MyObjectBuilder_GuiControlCompositePanel) base.GetObjectBuilder();
            objectBuilder.InnerHeight = this.InnerHeight;
            return objectBuilder;
        }

        public override void Init(MyObjectBuilder_GuiControlBase objectBuilder)
        {
            base.Init(objectBuilder);
            MyObjectBuilder_GuiControlCompositePanel panel = (MyObjectBuilder_GuiControlCompositePanel) objectBuilder;
            this.InnerHeight = panel.InnerHeight;
        }

        private void RefreshInternals()
        {
            base.MinSize = base.BackgroundTexture.MinSizeGui;
            base.MaxSize = base.BackgroundTexture.MaxSizeGui;
            base.Size = new Vector2(base.Size.X, base.MinSize.Y + this.InnerHeight);
        }

        public float InnerHeight
        {
            get => 
                this.m_innerHeight;
            set
            {
                this.m_innerHeight = value;
                this.RefreshInternals();
            }
        }
    }
}

