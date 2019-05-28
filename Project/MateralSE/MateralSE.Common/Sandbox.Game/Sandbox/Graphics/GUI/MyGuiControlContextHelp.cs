namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlContextHelp : MyGuiControlBase
    {
        private MyGuiControlLabel m_blockTypeLabel;
        private MyGuiControlLabel m_blockNameLabel;
        private MyGuiControlImage m_blockIconImage;
        private MyGuiControlPanel m_blockTypePanelLarge;
        private MyGuiControlPanel m_blockTypePanelSmall;
        private MyGuiControlLabel m_blockBuiltByLabel;
        private MyGuiControlPanel m_titleBackground;
        private MyGuiControlPanel m_pcuBackground;
        private MyGuiControlImage m_PCUIcon;
        private MyGuiControlLabel m_PCULabel;
        private MyGuiControlMultilineText m_helpText;
        private bool m_progressMode;
        private MyGuiControlBlockInfo.MyControlBlockInfoStyle m_style;
        private float m_smallerFontSize;
        public MyHudBlockInfo BlockInfo;
        public bool ShowBuildInfo;

        public MyGuiControlContextHelp(MyGuiControlBlockInfo.MyControlBlockInfoStyle style, bool progressMode = true, bool largeBlockInfo = true) : base(position, position, colorMask, null, new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds"), true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_smallerFontSize = 0.83f;
            this.ShowBuildInfo = true;
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            this.m_style = style;
            this.m_progressMode = true;
            base.ColorMask = this.m_style.BackgroundColormask;
            position = null;
            position = null;
            this.m_titleBackground = new MyGuiControlPanel(position, position, new VRageMath.Vector4?(Color.Red.ToVector4()), null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_titleBackground.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_titleBackground.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            base.Elements.Add(this.m_titleBackground);
            this.m_blockIconImage = new MyGuiControlImage();
            this.m_blockIconImage.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockIconImage.BackgroundTexture = this.m_progressMode ? null : new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_BG_MEDIUM_DEFAULT.Texture);
            this.m_blockIconImage.Size = this.m_progressMode ? new Vector2(0.066f) : new Vector2(0.04f);
            this.m_blockIconImage.Size *= new Vector2(0.75f, 1f);
            base.Elements.Add(this.m_blockIconImage);
            this.m_blockTypePanelLarge = new MyGuiControlPanel();
            this.m_blockTypePanelLarge.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            base.Elements.Add(this.m_blockTypePanelLarge);
            this.m_blockTypePanelSmall = new MyGuiControlPanel();
            this.m_blockTypePanelSmall.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            base.Elements.Add(this.m_blockTypePanelSmall);
            position = null;
            position = null;
            colorMask = null;
            this.m_blockNameLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_blockNameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockNameLabel.TextScale = 1f * this.baseScale;
            this.m_blockNameLabel.Font = this.m_style.BlockNameLabelFont;
            this.m_blockNameLabel.AutoEllipsis = true;
            base.Elements.Add(this.m_blockNameLabel);
            string text = string.Empty;
            if (style.EnableBlockTypeLabel)
            {
                text = MyTexts.GetString(largeBlockInfo ? MySpaceTexts.HudBlockInfo_LargeShip_Station : MySpaceTexts.HudBlockInfo_SmallShip);
            }
            position = null;
            position = null;
            colorMask = null;
            this.m_blockTypeLabel = new MyGuiControlLabel(position, position, text, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_blockTypeLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockTypeLabel.TextScale = 1f * this.baseScale;
            this.m_blockTypeLabel.Font = "White";
            base.Elements.Add(this.m_blockTypeLabel);
            position = null;
            position = null;
            colorMask = null;
            this.m_blockBuiltByLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_blockBuiltByLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_blockBuiltByLabel.TextScale = this.m_smallerFontSize * this.baseScale;
            this.m_blockBuiltByLabel.Font = this.m_style.InstalledRequiredLabelFont;
            base.Elements.Add(this.m_blockBuiltByLabel);
            position = null;
            position = null;
            this.m_pcuBackground = new MyGuiControlPanel(position, position, new VRageMath.Vector4?((VRageMath.Vector4) new Color(0.21f, 0.26f, 0.3f)), null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_pcuBackground.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_pcuBackground.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            this.m_pcuBackground.Size = new Vector2(0.225f, 0.03f);
            base.Elements.Add(this.m_pcuBackground);
            position = null;
            colorMask = null;
            string[] textures = new string[] { @"Textures\GUI\PCU.png" };
            this.m_PCUIcon = new MyGuiControlImage(position, new Vector2(0.022f, 0.029f), colorMask, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_PCUIcon.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            base.Elements.Add(this.m_PCUIcon);
            this.m_PCULabel = new MyGuiControlLabel();
            this.m_PCULabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            base.Elements.Add(this.m_PCULabel);
            position = null;
            position = null;
            colorMask = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, colorMask, "Blue", 0.68f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            text1.Name = "HelpText";
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_helpText = text1;
            base.Elements.Add(this.m_helpText);
            base.Size = new Vector2(0.225f, 0.32f);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            this.m_pcuBackground.Visible = false;
            this.m_PCUIcon.Visible = false;
            this.m_PCULabel.Visible = false;
            if (this.BlockInfo != null)
            {
                this.Reposition();
                this.m_blockNameLabel.TextToDraw.Clear();
                if (this.BlockInfo.BlockName != null)
                {
                    this.m_blockNameLabel.TextToDraw.Append(this.BlockInfo.BlockName);
                }
                this.m_blockNameLabel.TextToDraw.ToUpper();
                this.m_blockNameLabel.Autowrap(0.25f);
                this.m_blockIconImage.SetTextures(this.BlockInfo.BlockIcons);
                if (!this.ShowBuildInfo)
                {
                    this.m_blockBuiltByLabel.Visible = false;
                }
                else
                {
                    this.m_blockBuiltByLabel.Visible = true;
                    this.m_blockBuiltByLabel.Position = this.m_blockNameLabel.Position + new Vector2(0f, this.m_blockNameLabel.Size.Y + 0f);
                    this.m_blockBuiltByLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                    this.m_blockBuiltByLabel.TextScale = 0.6f;
                    this.m_blockBuiltByLabel.TextToDraw.Clear();
                    MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.BlockInfo.BlockBuiltBy);
                    if (identity != null)
                    {
                        this.m_blockBuiltByLabel.TextToDraw.Append(MyTexts.GetString(MyCommonTexts.BuiltBy));
                        this.m_blockBuiltByLabel.TextToDraw.Append(": ");
                        this.m_blockBuiltByLabel.TextToDraw.Append(identity.DisplayName);
                    }
                }
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha * MySandboxGame.Config.HUDBkOpacity);
        }

        public void RecalculateSize()
        {
            this.m_helpText.Position = ((-base.Size / 2f) + new Vector2(0f, this.m_titleBackground.Size.Y)) + new Vector2(0.01f, 0.01f);
            this.m_helpText.Size = new Vector2(base.Size.X, (base.Size.Y - this.m_titleBackground.Size.Y) - 0.006f);
            this.m_helpText.RefreshText(false);
            this.m_helpText.Text.Clear();
            this.m_helpText.Text.Append(this.BlockInfo.ContextHelp);
            this.m_helpText.Parse();
        }

        private unsafe void Reposition()
        {
            this.RecalculateSize();
            Vector2 vector = -base.Size / 2f;
            Vector2 vector2 = new Vector2(base.Size.X / 2f, -base.Size.Y / 2f);
            Vector2 vector3 = new Vector2(-base.Size.X / 2f, base.Size.Y / 2f);
            Vector2 vector4 = vector + (this.m_progressMode ? new Vector2(0.06f, 0f) : new Vector2(0.036f, 0f));
            float num = 0.072f * this.baseScale;
            Vector2 vector5 = (new Vector2(0.0035f) * new Vector2(0.75f, 1f)) * this.baseScale;
            if (!this.m_progressMode)
            {
                float* singlePtr1 = (float*) ref vector5.Y;
                singlePtr1[0] *= 1f;
            }
            this.m_titleBackground.Position = vector;
            this.m_titleBackground.ColorMask = this.m_style.TitleBackgroundColor;
            num = !this.m_progressMode ? ((Math.Abs((float) (vector.Y - this.m_blockIconImage.Position.Y)) + this.m_blockIconImage.Size.Y) + 0.003f) : (Math.Abs((float) (vector.Y - this.m_blockIconImage.Position.Y)) + this.m_blockIconImage.Size.Y);
            this.m_titleBackground.Size = new Vector2(vector2.X - this.m_titleBackground.Position.X, num + 0.003f);
            if (this.m_progressMode)
            {
                this.m_blockNameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                float num2 = 0.16f;
                float scale = 0.81f;
                Vector2 vector6 = MyGuiManager.MeasureString(this.m_blockNameLabel.Font, this.m_blockNameLabel.TextToDraw, scale);
                this.m_blockNameLabel.TextScale = (vector6.X <= num2) ? scale : (scale * (num2 / vector6.X));
                this.m_blockNameLabel.Size = new Vector2(vector2.X - ((this.m_blockIconImage.Position.X + this.m_blockIconImage.Size.X) + 0.004f), this.m_blockNameLabel.Size.Y);
                this.m_blockNameLabel.Position = new Vector2(vector4.X, this.m_blockIconImage.Position.Y + 0.022f);
                this.m_blockTypeLabel.Visible = false;
                if (!this.ShowBuildInfo)
                {
                    this.m_blockTypePanelLarge.Visible = false;
                    this.m_blockTypePanelSmall.Visible = false;
                }
                else
                {
                    this.m_blockTypePanelLarge.Position = vector2 + new Vector2(-0.005f, 0.032f);
                    this.m_blockTypePanelLarge.Size = this.m_progressMode ? new Vector2(0.05f) : new Vector2(0.04f);
                    this.m_blockTypePanelLarge.Size *= new Vector2(0.75f, 1f);
                    this.m_blockTypePanelLarge.BackgroundTexture = MyGuiConstants.TEXTURE_HUD_GRID_LARGE;
                    this.m_blockTypePanelLarge.Visible = true;
                    this.m_blockTypePanelLarge.Enabled = this.BlockInfo.GridSize == MyCubeSize.Large;
                    this.m_blockTypePanelSmall.Position = vector2 + new Vector2(-0.005f, 0.032f);
                    this.m_blockTypePanelSmall.Size = this.m_progressMode ? new Vector2(0.05f) : new Vector2(0.04f);
                    this.m_blockTypePanelSmall.Size *= new Vector2(0.75f, 1f);
                    this.m_blockTypePanelSmall.BackgroundTexture = MyGuiConstants.TEXTURE_HUD_GRID_SMALL;
                    this.m_blockTypePanelSmall.Visible = true;
                    this.m_blockTypePanelSmall.Enabled = this.BlockInfo.GridSize == MyCubeSize.Small;
                }
            }
            if (!this.ShowBuildInfo)
            {
                this.m_pcuBackground.Visible = false;
                this.m_PCUIcon.Visible = false;
                this.m_PCULabel.Visible = false;
            }
            else
            {
                this.m_pcuBackground.Visible = true;
                this.m_PCUIcon.Visible = true;
                this.m_PCULabel.Visible = true;
                this.m_pcuBackground.Position = vector3 + new Vector2(0f, -0.03f);
                this.m_PCUIcon.Position = vector3 + new Vector2(0.0085f, -0.03f);
                this.m_PCULabel.Position = this.m_PCUIcon.Position + new Vector2(0.035f, 0.003f);
                this.m_PCULabel.Text = "PCU: " + this.BlockInfo.PCUCost.ToString();
            }
            this.m_blockIconImage.Position = vector + new Vector2(0.005f, 0.005f);
        }

        private float baseScale =>
            0.83f;

        private float itemHeight =>
            (0.037f * this.baseScale);

        public bool ShowJustTitle
        {
            set
            {
                if (value)
                {
                    base.Size = new Vector2(0.225f, 0.1f);
                    this.m_helpText.Visible = false;
                }
                else
                {
                    base.Size = new Vector2(0.225f, 0.32f);
                    this.m_helpText.Visible = true;
                }
            }
        }
    }
}

