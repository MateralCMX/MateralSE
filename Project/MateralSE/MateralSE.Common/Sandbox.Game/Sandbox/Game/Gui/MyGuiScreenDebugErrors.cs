namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq;
    using VRage;
    using VRage.Game;
    using VRageMath;

    internal class MyGuiScreenDebugErrors : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugErrors() : base(new Vector2(0.5f, 0.5f), nullable, nullable2, true)
        {
            base.EnabledBackgroundFade = true;
            base.m_backgroundTexture = null;
            Rectangle safeFullscreenRectangle = MyGuiManager.GetSafeFullscreenRectangle();
            float num = ((float) safeFullscreenRectangle.Width) / ((float) safeFullscreenRectangle.Height);
            base.Size = new Vector2((num * 3f) / 4f, 1f);
            base.CanHideOthers = true;
            base.m_isTopScreen = true;
            base.m_canShareInput = false;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugErrors";

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector2? nullable3;
            Vector2? nullable1;
            Vector2? nullable4;
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenDebugOfficial_ErrorLogCaption, captionTextColor, new Vector2(0f, MyGuiConstants.SCREEN_CAPTION_DELTA_Y * -0.5f), 0.8f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += MyGuiConstants.SCREEN_CAPTION_DELTA_Y;
            Vector2? size = base.Size;
            Vector2 vector = new Vector2(0f, MyGuiConstants.SCREEN_CAPTION_DELTA_Y);
            if (size != null)
            {
                nullable1 = new Vector2?(size.GetValueOrDefault() - vector);
            }
            else
            {
                nullable3 = null;
                nullable1 = nullable3;
            }
            size = base.Size;
            float num = -0.5f;
            if (size != null)
            {
                nullable4 = new Vector2?(size.GetValueOrDefault() * num);
            }
            else
            {
                nullable3 = null;
                nullable4 = nullable3;
            }
            MyGuiControlMultilineText text = this.AddMultilineText(nullable1, nullable4, 0.7f, false);
            if (MyDefinitionErrors.GetErrors().Count<MyDefinitionErrors.Error>() == 0)
            {
                text.AppendText(MyTexts.Get(MyCommonTexts.ScreenDebugOfficial_NoErrorText));
            }
            foreach (MyDefinitionErrors.Error error in MyDefinitionErrors.GetErrors())
            {
                text.AppendText(error.ToString(), text.Font, text.TextScaleWithLanguage, error.GetSeverityColor().ToVector4());
                text.AppendLine();
                text.AppendLine();
            }
        }
    }
}

