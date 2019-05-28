namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRageMath;
    using VRageRender;

    public class MyStatControlImage : MyStatControlBase
    {
        public MyStatControlImage(MyStatControls parent) : base(parent)
        {
        }

        public override void Draw(float transitionAlpha)
        {
            Vector4 colorMask = base.ColorMask;
            bool flag = false;
            base.BlinkBehavior.UpdateBlink();
            if (base.BlinkBehavior.Blink)
            {
                float single1 = MathHelper.Min(transitionAlpha, base.BlinkBehavior.CurrentBlinkAlpha);
                transitionAlpha = single1;
                if (base.BlinkBehavior.ColorMask != null)
                {
                    base.ColorMask = base.BlinkBehavior.ColorMask.Value;
                    flag = true;
                }
            }
            RectangleF destination = new RectangleF(base.Position, base.Size);
            Rectangle? sourceRectangle = null;
            Vector2 zero = Vector2.Zero;
            MyRenderProxy.DrawSprite(this.Texture.Path, ref destination, false, ref sourceRectangle, MyGuiControlBase.ApplyColorMaskModifiers(base.ColorMask, true, transitionAlpha), 0f, Vector2.UnitX, ref zero, SpriteEffects.None, 0f, true, null);
            if (flag)
            {
                base.ColorMask = colorMask;
            }
        }

        public MyObjectBuilder_GuiTexture Texture { get; set; }
    }
}

