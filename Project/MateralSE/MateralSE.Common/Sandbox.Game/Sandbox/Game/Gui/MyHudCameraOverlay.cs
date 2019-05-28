namespace Sandbox.Game.GUI
{
    using Sandbox.Graphics;
    using System;
    using VRage.Game.Components;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 400)]
    internal class MyHudCameraOverlay : MySessionComponentBase
    {
        private static string m_textureName;
        private static bool m_enabled;

        public override void Draw()
        {
            base.Draw();
            if (Enabled)
            {
                DrawFullScreenSprite();
            }
        }

        private static void DrawFullScreenSprite()
        {
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            RectangleF destination = new RectangleF((float) fullscreenRectangle.X, (float) fullscreenRectangle.Y, (float) fullscreenRectangle.Width, (float) fullscreenRectangle.Height);
            Rectangle? sourceRectangle = null;
            Vector2 zero = Vector2.Zero;
            MyRenderProxy.DrawSprite(m_textureName, ref destination, false, ref sourceRectangle, Color.White, 0f, Vector2.UnitX, ref zero, SpriteEffects.None, 0f, true, null);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Enabled = false;
        }

        public static string TextureName
        {
            get => 
                m_textureName;
            set => 
                (m_textureName = value);
        }

        public static bool Enabled
        {
            get => 
                m_enabled;
            set
            {
                if (m_enabled != value)
                {
                    m_enabled = value;
                }
            }
        }
    }
}

