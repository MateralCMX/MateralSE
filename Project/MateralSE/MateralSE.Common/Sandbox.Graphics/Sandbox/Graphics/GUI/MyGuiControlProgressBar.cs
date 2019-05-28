namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlProgressBar))]
    public class MyGuiControlProgressBar : MyGuiControlBase
    {
        public Color ProgressColor;
        private float m_value;
        public bool IsHorizontal;
        public bool EnableBorderAutohide;
        public float BorderAutohideThreshold;
        private MyGuiControlPanel m_potentialBar;
        private MyGuiControlPanel m_progressForeground;
        private MyGuiControlPanel m_progressBarLine;
        private static readonly Color DEFAULT_PROGRESS_COLOR = Color.White;

        public MyGuiControlProgressBar(Vector2? position = new Vector2?(), Vector2? size = new Vector2?(), Color? progressBarColor = new Color?(), MyGuiDrawAlignEnum originAlign = 4, MyGuiCompositeTexture backgroundTexture = null, bool isHorizontal = true, bool potentialBarEnabled = true, bool enableBorderAutohide = false, float borderAutohideThreshold = 0.01f) : base(position, size, backgroundColor, null, texture, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, originAlign)
        {
            this.m_value = 1f;
            this.IsHorizontal = true;
            this.BorderAutohideThreshold = 0.01f;
            MyGuiCompositeTexture texture = backgroundTexture;
            Vector4? backgroundColor = null;
            this.ProgressColor = (progressBarColor != null) ? progressBarColor.Value : DEFAULT_PROGRESS_COLOR;
            this.IsHorizontal = isHorizontal;
            this.EnableBorderAutohide = enableBorderAutohide;
            this.BorderAutohideThreshold = borderAutohideThreshold;
            Vector2? nullable2 = null;
            this.m_progressForeground = new MyGuiControlPanel(new Vector2(-base.Size.X / 2f, 0f), nullable2, new Vector4?((Vector4) this.ProgressColor), null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_progressForeground.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
            nullable2 = null;
            backgroundColor = null;
            this.m_potentialBar = new MyGuiControlPanel(nullable2, new Vector2(0f, base.Size.Y), backgroundColor, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_potentialBar.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
            this.m_potentialBar.ColorMask = new Vector4((Vector3) this.ProgressColor, 0.7f);
            this.m_potentialBar.Visible = false;
            this.m_potentialBar.Enabled = potentialBarEnabled;
            base.Elements.Add(this.m_potentialBar);
            base.Elements.Add(this.m_progressForeground);
        }

        public Vector2 CalculatePotentialBarPosition() => 
            new Vector2(this.m_progressForeground.Position.X + this.m_progressForeground.Size.X, 0f);

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            Vector2 vector = base.Size * new Vector2(this.IsHorizontal ? this.Value : 1f, this.IsHorizontal ? 1f : this.Value);
            this.m_progressForeground.Size = vector;
            if (!this.EnableBorderAutohide || (this.Value > this.BorderAutohideThreshold))
            {
                this.m_progressForeground.BorderEnabled = true;
            }
            else
            {
                this.m_progressForeground.BorderEnabled = false;
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
        }

        public float Value
        {
            get => 
                this.m_value;
            set => 
                (this.m_value = MathHelper.Clamp(value, 0f, 1f));
        }

        public MyGuiControlPanel PotentialBar =>
            this.m_potentialBar;

        public MyGuiControlPanel ForegroundBar =>
            this.m_progressForeground;

        public MyGuiControlPanel ForegroundBarEndLine =>
            this.m_progressBarLine;
    }
}

