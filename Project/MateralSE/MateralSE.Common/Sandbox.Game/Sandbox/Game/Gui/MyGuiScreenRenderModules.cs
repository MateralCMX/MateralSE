namespace Sandbox.Game.Gui
{
    using System;
    using VRageMath;

    internal class MyGuiScreenRenderModules : MyGuiScreenDebugBase
    {
        public MyGuiScreenRenderModules() : base(nullable, false)
        {
            base.m_closeOnEsc = true;
            base.m_drawEvenWithoutFocus = true;
            base.m_isTopMostScreen = false;
            base.CanHaveFocus = false;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenRenderModules";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2? captionOffset = null;
            base.AddCaption("Render modules", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_scale = 0.7f;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }
    }
}

