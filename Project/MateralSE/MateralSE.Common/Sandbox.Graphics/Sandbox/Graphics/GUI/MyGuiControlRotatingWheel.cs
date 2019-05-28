namespace Sandbox.Graphics.GUI
{
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Library;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlRotatingWheel : MyGuiControlBase
    {
        private float m_rotationSpeed;
        private float m_rotatingAngle;
        private float m_wheelScale;
        private string m_texture;
        private Vector2 m_textureResolution;
        public bool MultipleSpinningWheels;
        public bool ManualRotationUpdate;

        public MyGuiControlRotatingWheel(Vector2? position = new Vector2?(), Vector4? colorMask = new Vector4?(), float scale = 0.36f, MyGuiDrawAlignEnum align = 4, string texture = @"Textures\GUI\screens\screen_loading_wheel.dds", bool manualRotationUpdate = true, bool multipleSpinningWheels = true, Vector2? textureResolution = new Vector2?(), float radiansPerSecond = 1.5f) : base(position, nullable, colorMask, null, null, false, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, align)
        {
            this.UpdateRotation();
            this.m_wheelScale = scale;
            this.m_texture = texture;
            this.m_textureResolution = (textureResolution != null) ? textureResolution.Value : new Vector2(256f, 256f);
            this.MultipleSpinningWheels = multipleSpinningWheels;
            this.ManualRotationUpdate = manualRotationUpdate;
            this.m_rotationSpeed = radiansPerSecond;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
            Vector2 positionAbsolute = base.GetPositionAbsolute();
            Color color = new Color((Vector4) (transitionAlpha * new Color(0, 0, 0, 80).ToVector4()));
            this.DrawWheel(positionAbsolute + MyGuiConstants.SHADOW_OFFSET, this.m_wheelScale, color, this.m_rotatingAngle, this.m_rotationSpeed);
            Color color2 = ApplyColorMaskModifiers(base.ColorMask, base.Enabled, transitionAlpha);
            this.DrawWheel(positionAbsolute, this.m_wheelScale, color2, this.m_rotatingAngle, this.m_rotationSpeed);
            if (this.MultipleSpinningWheels)
            {
                this.DrawWheel(positionAbsolute, 0.6f * this.m_wheelScale, color2, -this.m_rotatingAngle * 1.1f, -this.m_rotationSpeed);
                this.DrawWheel(positionAbsolute, 0.36f * this.m_wheelScale, color2, this.m_rotatingAngle * 1.2f, this.m_rotationSpeed);
            }
        }

        private void DrawWheel(Vector2 position, float scale, Color color, float rotationAngle, float rotationSpeed)
        {
            Vector2? nullable;
            if (this.ManualRotationUpdate)
            {
                nullable = null;
                MyGuiManager.DrawSpriteBatch(this.m_texture, position, scale, color, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, rotationAngle, nullable, true);
            }
            else
            {
                nullable = null;
                MyGuiManager.DrawSpriteBatchRotate(this.m_texture, position, scale, color, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, rotationAngle, nullable, rotationSpeed, true);
            }
        }

        public override void Update()
        {
            if (this.ManualRotationUpdate && base.Visible)
            {
                this.UpdateRotation();
            }
            base.Update();
        }

        private void UpdateRotation()
        {
            this.m_rotatingAngle = (((float) MyEnvironment.TickCount) / 1000f) * this.m_rotationSpeed;
        }
    }
}

