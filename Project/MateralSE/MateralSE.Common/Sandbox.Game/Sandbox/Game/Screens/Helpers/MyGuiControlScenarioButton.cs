namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlScenarioButton : MyGuiControlRadioButton
    {
        private MyGuiControlLabel m_titleLabel;
        private MyGuiControlImage m_previewImage;

        public MyGuiControlScenarioButton(MyScenarioDefinition scenario) : base(position, position, MyDefinitionManager.Static.GetScenarioDefinitions().IndexOf(scenario), colorMask)
        {
            Vector2? position = null;
            position = null;
            Vector4? colorMask = null;
            base.VisualStyle = MyGuiControlRadioButtonStyleEnum.ScenarioButton;
            base.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.Scenario = scenario;
            position = null;
            position = null;
            colorMask = null;
            this.m_titleLabel = new MyGuiControlLabel(position, position, scenario.DisplayNameText, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            position = null;
            position = null;
            colorMask = null;
            this.m_previewImage = new MyGuiControlImage(position, position, colorMask, null, scenario.Icons, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            MyGuiSizedTexture texture = new MyGuiSizedTexture {
                SizePx = new Vector2(229f, 128f)
            };
            this.m_previewImage.Size = texture.SizeGui;
            this.m_previewImage.BorderEnabled = true;
            this.m_previewImage.BorderColor = MyGuiConstants.THEMED_GUI_LINE_BORDER.ToVector4();
            base.SetToolTip(scenario.DescriptionText);
            base.Size = new Vector2(Math.Max(this.m_titleLabel.Size.X, this.m_previewImage.Size.X), this.m_titleLabel.Size.Y + this.m_previewImage.Size.Y);
            base.Elements.Add(this.m_titleLabel);
            base.Elements.Add(this.m_previewImage);
        }

        protected override void OnHasHighlightChanged()
        {
            base.OnHasHighlightChanged();
            if (base.HasHighlight)
            {
                this.m_titleLabel.Font = "White";
                this.m_previewImage.BorderColor = Vector4.One;
            }
            else
            {
                this.m_titleLabel.Font = "Blue";
                this.m_previewImage.BorderColor = MyGuiConstants.THEMED_GUI_LINE_BORDER.ToVector4();
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            this.UpdatePositions();
        }

        private void UpdatePositions()
        {
            this.m_titleLabel.Position = base.Size * -0.5f;
            this.m_previewImage.Position = this.m_titleLabel.Position + new Vector2(0f, this.m_titleLabel.Size.Y);
        }

        public string Title =>
            this.m_titleLabel.Text.ToString();

        public MyScenarioDefinition Scenario { get; private set; }
    }
}

