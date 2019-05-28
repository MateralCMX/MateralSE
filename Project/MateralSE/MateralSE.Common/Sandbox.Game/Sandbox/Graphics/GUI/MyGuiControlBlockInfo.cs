namespace Sandbox.Graphics.GUI
{
    using Sandbox;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiControlBlockInfo : MyGuiControlBase
    {
        public static bool ShowComponentProgress = true;
        public static bool ShowCriticalComponent = false;
        public static bool ShowCriticalIntegrity = true;
        public static bool ShowOwnershipIntegrity = MyFakes.SHOW_FACTIONS_GUI;
        public static VRageMath.Vector4 CriticalIntegrityColor = Color.Red.ToVector4();
        public static VRageMath.Vector4 CriticalComponentColor = (CriticalIntegrityColor * new VRageMath.Vector4(1f, 1f, 1f, 0.7f));
        public static VRageMath.Vector4 OwnershipIntegrityColor = Color.Blue.ToVector4();
        private MyGuiControlLabel m_blockTypeLabel;
        private MyGuiControlLabel m_blockNameLabel;
        private MyGuiControlLabel m_componentsLabel;
        private MyGuiControlLabel m_installedRequiredLabel;
        private MyGuiControlLabel m_blockBuiltByLabel;
        private MyGuiControlLabel m_integrityLabel;
        private MyGuiControlLabel m_PCULabel;
        private MyGuiControlImage m_blockIconImage;
        private MyGuiControlImage m_PCUIcon;
        private MyGuiControlPanel m_blockTypePanel;
        private MyGuiControlPanel m_pcuBackground;
        private MyGuiControlPanel m_titleBackground;
        private MyGuiControlPanel m_integrityBackground;
        private MyGuiProgressCompositeTextureAdvanced m_integrityForeground;
        private Color m_integrityForegroundColorMask;
        private MyGuiControlLabel m_criticalIntegrityLabel;
        private MyGuiControlLabel m_ownershipIntegrityLabel;
        private MyGuiControlSeparatorList m_separator;
        private List<ComponentLineControl> m_componentLines;
        public MyHudBlockInfo BlockInfo;
        private bool m_progressMode;
        private MyControlBlockInfoStyle m_style;
        private float m_smallerFontSize;

        public MyGuiControlBlockInfo(MyControlBlockInfoStyle style, bool progressMode = true, bool largeBlockInfo = true) : base(position, position, colorMask, null, new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds"), true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER)
        {
            this.m_integrityForegroundColorMask = Color.White;
            this.m_componentLines = new List<ComponentLineControl>(15);
            this.m_smallerFontSize = 0.83f;
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? colorMask = null;
            this.m_style = style;
            this.m_progressMode = progressMode;
            if (this.m_progressMode)
            {
                base.BackgroundTexture = MyGuiConstants.TEXTURE_COMPOSITE_SLOPE_LEFTBOTTOM_30;
            }
            base.ColorMask = this.m_style.BackgroundColormask;
            position = null;
            position = null;
            this.m_titleBackground = new MyGuiControlPanel(position, position, new VRageMath.Vector4?(Color.Red.ToVector4()), null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_titleBackground.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_titleBackground.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            base.Elements.Add(this.m_titleBackground);
            if (this.m_progressMode)
            {
                position = null;
                position = null;
                colorMask = null;
                this.m_integrityLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_integrityLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
                this.m_integrityLabel.Font = this.m_style.IntegrityLabelFont;
                base.Elements.Add(this.m_integrityLabel);
                this.m_integrityBackground = new MyGuiControlPanel();
                this.m_integrityBackground.BackgroundTexture = MyGuiConstants.TEXTURE_COMPOSITE_BLOCKINFO_PROGRESSBAR;
                this.m_integrityBackground.ColorMask = this.m_style.IntegrityBackgroundColor;
                this.m_integrityBackground.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
                base.Elements.Add(this.m_integrityBackground);
                this.m_integrityForeground = new MyGuiProgressCompositeTextureAdvanced(MyGuiConstants.TEXTURE_COMPOSITE_BLOCKINFO_PROGRESSBAR);
                this.m_integrityForeground.IsInverted = true;
                this.m_integrityForeground.Orientation = MyGuiProgressCompositeTexture.BarOrientation.VERTICAL;
                position = null;
                position = null;
                colorMask = null;
                this.m_criticalIntegrityLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.Functional), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_criticalIntegrityLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM;
                this.m_criticalIntegrityLabel.TextScale = 0.4f * this.baseScale;
                this.m_criticalIntegrityLabel.Font = "Blue";
                position = null;
                position = null;
                colorMask = null;
                this.m_ownershipIntegrityLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.Hack), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_ownershipIntegrityLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
                this.m_ownershipIntegrityLabel.TextScale = 0.4f * this.baseScale;
                this.m_ownershipIntegrityLabel.Font = "Blue";
            }
            this.m_blockIconImage = new MyGuiControlImage();
            this.m_blockIconImage.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockIconImage.BackgroundTexture = this.m_progressMode ? null : new MyGuiCompositeTexture(MyGuiConstants.TEXTURE_HUD_BG_MEDIUM_DEFAULT.Texture);
            this.m_blockIconImage.Size = this.m_progressMode ? new Vector2(0.066f) : new Vector2(0.04f);
            this.m_blockIconImage.Size *= new Vector2(0.75f, 1f);
            base.Elements.Add(this.m_blockIconImage);
            this.m_blockTypePanel = new MyGuiControlPanel();
            this.m_blockTypePanel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_blockTypePanel.Size = this.m_progressMode ? new Vector2(0.088f) : new Vector2(0.02f);
            this.m_blockTypePanel.Size *= new Vector2(0.75f, 1f);
            this.m_blockTypePanel.BackgroundTexture = new MyGuiCompositeTexture(largeBlockInfo ? @"Textures\GUI\Icons\HUD 2017\GridSizeLargeFit.png" : @"Textures\GUI\Icons\HUD 2017\GridSizeSmallFit.png");
            base.Elements.Add(this.m_blockTypePanel);
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
            this.m_componentsLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(this.m_style.ComponentsLabelText), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_componentsLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_componentsLabel.TextScale = this.m_smallerFontSize * this.baseScale;
            this.m_componentsLabel.Font = this.m_style.ComponentsLabelFont;
            base.Elements.Add(this.m_componentsLabel);
            position = null;
            position = null;
            colorMask = null;
            this.m_installedRequiredLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(this.m_style.InstalledRequiredLabelText), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_installedRequiredLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_installedRequiredLabel.TextScale = this.m_smallerFontSize * this.baseScale;
            this.m_installedRequiredLabel.Font = this.m_style.InstalledRequiredLabelFont;
            base.Elements.Add(this.m_installedRequiredLabel);
            position = null;
            position = null;
            colorMask = null;
            this.m_blockBuiltByLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_blockBuiltByLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_blockBuiltByLabel.TextScale = this.m_smallerFontSize * this.baseScale;
            this.m_blockBuiltByLabel.Font = this.m_style.InstalledRequiredLabelFont;
            base.Elements.Add(this.m_blockBuiltByLabel);
            if (!this.m_progressMode && !this.m_style.HiddenPCU)
            {
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
            }
            this.m_separator = new MyGuiControlSeparatorList();
            this.EnsureLineControls(this.m_componentLines.Capacity);
            this.Size = this.m_progressMode ? new Vector2(0.225f, 0.4f) : new Vector2(0.225f, 0.4f);
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            if (this.BlockInfo != null)
            {
                this.EnsureLineControls(this.BlockInfo.Components.Count);
                this.Reposition();
                Vector2 vector = new Vector2(-base.Size.X / 2f, 0f);
                int num = 0;
                while (true)
                {
                    if (num >= this.m_componentLines.Count)
                    {
                        this.m_blockNameLabel.TextToDraw.Clear();
                        if (this.BlockInfo.BlockName != null)
                        {
                            this.m_blockNameLabel.TextToDraw.Append(this.BlockInfo.BlockName);
                        }
                        this.m_blockNameLabel.TextToDraw.ToUpper();
                        this.m_blockNameLabel.Autowrap(0.25f);
                        this.m_blockBuiltByLabel.TextToDraw.Clear();
                        MyIdentity identity = MySession.Static.Players.TryGetIdentity(this.BlockInfo.BlockBuiltBy);
                        if (identity != null)
                        {
                            this.m_blockBuiltByLabel.TextToDraw.Append(MyTexts.GetString(MyCommonTexts.BuiltBy));
                            this.m_blockBuiltByLabel.TextToDraw.Append(": ");
                            this.m_blockBuiltByLabel.TextToDraw.Append(identity.DisplayName);
                        }
                        if (this.m_progressMode)
                        {
                            this.m_blockBuiltByLabel.Visible = false;
                        }
                        this.m_blockIconImage.SetTextures(this.BlockInfo.BlockIcons);
                        if (this.BlockInfo.Components.Count == 0)
                        {
                            this.m_installedRequiredLabel.Visible = false;
                            this.m_componentsLabel.Visible = false;
                        }
                        else
                        {
                            this.m_installedRequiredLabel.Visible = true;
                            this.m_componentsLabel.Visible = true;
                        }
                        break;
                    }
                    if (num >= this.BlockInfo.Components.Count)
                    {
                        this.m_componentLines[num].Visible = false;
                    }
                    else
                    {
                        string componentLineDefaultFont;
                        MyHudBlockInfo.ComponentInfo info = this.BlockInfo.Components[num];
                        VRageMath.Vector4 one = VRageMath.Vector4.One;
                        if (!this.m_progressMode || (this.BlockInfo.BlockIntegrity <= 0f))
                        {
                            componentLineDefaultFont = this.m_style.ComponentLineDefaultFont;
                        }
                        else if (this.BlockInfo.MissingComponentIndex == num)
                        {
                            componentLineDefaultFont = this.m_style.ComponentLineMissingFont;
                        }
                        else if (info.MountedCount == info.TotalCount)
                        {
                            componentLineDefaultFont = this.m_style.ComponentLineAllMountedFont;
                        }
                        else if (info.InstalledCount == info.TotalCount)
                        {
                            componentLineDefaultFont = this.m_style.ComponentLineAllInstalledFont;
                        }
                        else
                        {
                            componentLineDefaultFont = this.m_style.ComponentLineDefaultFont;
                            one = this.m_style.ComponentLineDefaultColor;
                        }
                        if (!this.m_progressMode || (this.BlockInfo.BlockIntegrity <= 0f))
                        {
                            this.m_componentLines[num].SetProgress(1f);
                        }
                        else
                        {
                            this.m_componentLines[num].SetProgress(((float) info.MountedCount) / ((float) info.TotalCount));
                        }
                        this.m_componentLines[num].Visible = true;
                        this.m_componentLines[num].NameLabel.Font = componentLineDefaultFont;
                        if (this.m_progressMode)
                        {
                            this.m_componentLines[num].NameLabel.Position = vector + new Vector2(-0.005f, 0f);
                            float x = MyGuiManager.MeasureString(this.m_componentLines[num].NameLabel.Font, this.m_componentLines[num].NameLabel.TextToDraw, this.m_componentLines[num].NameLabel.TextScale).X;
                            this.m_componentLines[num].NameLabel.TextScale = 0.6f;
                        }
                        this.m_componentLines[num].NameLabel.ColorMask = one;
                        this.m_componentLines[num].NameLabel.TextToDraw.Clear();
                        this.m_componentLines[num].NameLabel.TextToDraw.Append(info.ComponentName);
                        this.m_componentLines[num].IconImage.SetTextures(info.Icons);
                        this.m_componentLines[num].NumbersLabel.Font = componentLineDefaultFont;
                        this.m_componentLines[num].NumbersLabel.ColorMask = one;
                        this.m_componentLines[num].NumbersLabel.TextToDraw.Clear();
                        if (this.m_progressMode && (this.BlockInfo.BlockIntegrity > 0f))
                        {
                            this.m_componentLines[num].NumbersLabel.TextToDraw.AppendInt32(info.InstalledCount).Append(" / ").AppendInt32(info.TotalCount);
                            if (this.m_style.ShowAvailableComponents)
                            {
                                this.m_componentLines[num].NumbersLabel.TextToDraw.Append(" / ").AppendInt32(info.AvailableAmount);
                            }
                        }
                        else if (!this.BlockInfo.ShowAvailable)
                        {
                            this.m_componentLines[num].NumbersLabel.TextToDraw.AppendInt32(info.TotalCount);
                        }
                        else
                        {
                            this.m_componentLines[num].NumbersLabel.TextToDraw.AppendInt32(info.TotalCount);
                            if (this.m_style.ShowAvailableComponents)
                            {
                                this.m_componentLines[num].NumbersLabel.TextToDraw.Append(" / ").AppendInt32(info.AvailableAmount);
                            }
                        }
                        float num2 = 1f;
                        if (MyGuiManager.MeasureString(this.m_componentLines[num].NumbersLabel.Font, this.m_componentLines[num].NumbersLabel.TextToDraw, this.m_componentLines[num].NumbersLabel.TextScale).X > 0.06f)
                        {
                            num2 = 0.8f;
                        }
                        this.m_componentLines[num].NumbersLabel.TextScale = 0.6f;
                        this.m_componentLines[num].NumbersLabel.TextScale *= num2;
                        this.m_componentLines[num].NumbersLabel.Size = this.m_componentLines[num].NumbersLabel.GetTextSize();
                        this.m_componentLines[num].IconImage.BorderEnabled = ShowCriticalComponent && (this.BlockInfo.CriticalComponentIndex == num);
                        this.m_componentLines[num].RecalcTextSize(this.m_progressMode);
                    }
                    num++;
                }
            }
            base.Draw(transitionAlpha, backgroundTransitionAlpha * MySandboxGame.Config.HUDBkOpacity);
            if ((this.BlockInfo != null) && (this.m_integrityForeground != null))
            {
                this.m_integrityForeground.Draw(this.BlockInfo.BlockIntegrity, this.m_integrityForegroundColorMask);
            }
            if (this.m_separator != null)
            {
                this.m_separator.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
            if ((ShowCriticalIntegrity && (this.m_criticalIntegrityLabel != null)) && this.m_criticalIntegrityLabel.Visible)
            {
                this.m_criticalIntegrityLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
            if ((ShowOwnershipIntegrity && (this.m_ownershipIntegrityLabel != null)) && this.m_ownershipIntegrityLabel.Visible)
            {
                this.m_ownershipIntegrityLabel.Draw(transitionAlpha, backgroundTransitionAlpha);
            }
        }

        private void EnsureLineControls(int count)
        {
            while (this.m_componentLines.Count < count)
            {
                ComponentLineControl item = new ComponentLineControl((this.m_progressMode ? new Vector2(0.288f, 0.05f) : new Vector2(0.24f, 0.05f)) * new Vector2(1f, this.baseScale), 0.035f * this.baseScale);
                this.m_componentLines.Add(item);
                base.Elements.Add(item);
            }
        }

        public void RecalculateSize()
        {
            if (this.m_progressMode)
            {
                base.Size = new Vector2(base.Size.X, (0.12f * this.baseScale) + (this.itemHeight * (this.BlockInfo.Components.Count - 2)));
            }
            else
            {
                base.Size = new Vector2(base.Size.X, (0.1f * this.baseScale) + (this.itemHeight * (this.BlockInfo.Components.Count + 1)));
                if (this.m_style.HiddenPCU)
                {
                    base.Size -= new Vector2(0f, this.itemHeight);
                }
                if (this.m_style.HiddenHeader)
                {
                    base.Size -= new Vector2(0f, this.itemHeight);
                }
            }
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
            this.m_installedRequiredLabel.TextToDraw = (this.BlockInfo.BlockIntegrity <= 0f) ? (!this.BlockInfo.ShowAvailable ? MyTexts.Get(this.m_style.RequiredLabelText) : MyTexts.Get(this.m_style.RequiredAvailableLabelText)) : MyTexts.Get(this.m_style.RequiredLabelText);
            this.m_titleBackground.Position = vector;
            this.m_titleBackground.ColorMask = this.m_style.TitleBackgroundColor;
            if (!this.m_progressMode && !this.m_style.HiddenHeader)
            {
                num = (Math.Abs((float) (vector.Y - this.m_blockIconImage.Position.Y)) + this.m_blockIconImage.Size.Y) + 0.003f;
            }
            else
            {
                num = 0f;
                this.m_blockIconImage.Visible = false;
                this.m_blockTypeLabel.Visible = false;
                this.m_blockNameLabel.Visible = false;
                this.m_titleBackground.Visible = false;
            }
            this.m_titleBackground.Size = new Vector2(vector2.X - this.m_titleBackground.Position.X, num + 0.003f);
            this.m_separator.Clear();
            if (!this.m_progressMode)
            {
                if (!this.m_style.HiddenPCU)
                {
                    this.m_pcuBackground.Position = vector3 + new Vector2(0f, -0.03f);
                    this.m_pcuBackground.Size = new Vector2(base.Size.X, this.m_pcuBackground.Size.Y);
                    this.m_PCUIcon.Position = vector3 + new Vector2(0.0085f, -0.03f);
                    this.m_PCULabel.Position = this.m_PCUIcon.Position + new Vector2(0.035f, 0.003f);
                    this.m_PCULabel.Text = "PCU: " + this.BlockInfo.PCUCost.ToString();
                }
            }
            else
            {
                this.m_ownershipIntegrityLabel.Visible = false;
                this.m_criticalIntegrityLabel.Visible = false;
                float y = this.itemHeight * this.BlockInfo.Components.Count;
                float x = 0.05f;
                Vector2 vector7 = new Vector2(0.006f, 0.04f);
                Vector2 normalizedCoord = base.GetPositionAbsoluteTopLeft() + vector7;
                Vector2 vector9 = (vector + vector7) + new Vector2(0f, y);
                Vector2 normalizedSize = new Vector2(x, y);
                this.m_integrityBackground.Position = vector9;
                this.m_integrityBackground.Size = normalizedSize;
                this.m_integrityLabel.Position = (vector + vector7) + new Vector2(x / 2f, -0.005f);
                this.m_integrityLabel.TextToDraw.Clear();
                this.m_integrityLabel.TextToDraw.AppendInt32(((int) Math.Floor((double) (this.BlockInfo.BlockIntegrity * 100f)))).Append("%");
                if (this.BlockInfo.BlockIntegrity > 0f)
                {
                    VRageMath.Vector4 vector11 = (this.BlockInfo.BlockIntegrity > this.BlockInfo.CriticalIntegrity) ? this.m_style.IntegrityForegroundColorOverCritical : this.m_style.IntegrityForegroundColor;
                    this.m_integrityForegroundColorMask = vector11;
                    this.m_integrityForeground.Position = new Vector2I(MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(normalizedCoord, false));
                    this.m_integrityForeground.Size = new Vector2I(MyGuiManager.GetScreenSizeFromNormalizedSize(normalizedSize, false));
                    float width = 0.004f;
                    if (ShowCriticalIntegrity)
                    {
                        this.m_separator.AddHorizontal(normalizedCoord + new Vector2(0f, y * (1f - this.BlockInfo.CriticalIntegrity)), x, width, new VRageMath.Vector4?(CriticalIntegrityColor));
                        this.m_criticalIntegrityLabel.Position = normalizedCoord + new Vector2(x / 2f, y * (1f - this.BlockInfo.CriticalIntegrity));
                        this.m_criticalIntegrityLabel.Visible = true;
                    }
                    if (ShowOwnershipIntegrity && (this.BlockInfo.OwnershipIntegrity > 0f))
                    {
                        this.m_separator.AddHorizontal(normalizedCoord + new Vector2(0f, y * (1f - this.BlockInfo.OwnershipIntegrity)), x, width, new VRageMath.Vector4?(OwnershipIntegrityColor));
                        this.m_ownershipIntegrityLabel.Position = normalizedCoord + new Vector2(x / 2f, (y * (1f - this.BlockInfo.OwnershipIntegrity)) + 0.002f);
                        this.m_ownershipIntegrityLabel.Visible = true;
                    }
                }
            }
            if (this.m_progressMode)
            {
                this.m_blockNameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_blockNameLabel.TextScale = 0.81f;
                this.m_blockNameLabel.Size = new Vector2(vector2.X - ((this.m_blockIconImage.Position.X + this.m_blockIconImage.Size.X) + 0.004f), this.m_blockNameLabel.Size.Y);
                this.m_blockNameLabel.Position = new Vector2(vector4.X, this.m_blockIconImage.Position.Y + 0.022f);
                this.m_blockBuiltByLabel.Position = this.m_blockNameLabel.Position + new Vector2(0f, this.m_blockNameLabel.Size.Y + 0f);
                this.m_blockBuiltByLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_blockBuiltByLabel.TextScale = 0.6f;
                this.m_componentsLabel.Position = vector4 + new Vector2(0f, 0.0095f + (num * this.baseScale));
                this.m_installedRequiredLabel.Position = vector2 + new Vector2(-0.011f, 0.0095f + (num * this.baseScale));
                this.m_blockTypeLabel.Visible = false;
                this.m_blockTypePanel.Visible = false;
            }
            else
            {
                this.m_blockTypePanel.Position = vector + new Vector2(0.01f, 0.012f);
                if (this.m_style.EnableBlockTypePanel)
                {
                    this.m_blockTypePanel.Visible = true;
                    this.m_blockNameLabel.Size = new Vector2((this.m_blockTypePanel.Position.X - this.m_blockTypePanel.Size.X) - this.m_blockNameLabel.Position.X, this.m_blockNameLabel.Size.Y);
                }
                else
                {
                    this.m_blockTypePanel.Visible = false;
                    this.m_blockNameLabel.Size = new Vector2((vector2.X - ((this.m_blockIconImage.Position.X + this.m_blockIconImage.Size.X) + 0.004f)) - 0.006f, this.m_blockNameLabel.Size.Y);
                }
                this.m_blockNameLabel.TextScale = 0.95f * this.baseScale;
                this.m_blockNameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
                this.m_blockNameLabel.Position = new Vector2(vector4.X + 0.006f, this.m_blockIconImage.Position.Y + this.m_blockIconImage.Size.Y);
                if (!this.m_style.EnableBlockTypeLabel)
                {
                    this.m_blockNameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    this.m_blockNameLabel.Position -= new Vector2(0f, this.m_blockIconImage.Size.Y * 0.5f);
                }
                this.m_blockTypeLabel.Visible = true;
                this.m_blockTypeLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.m_blockTypeLabel.TextScale = this.m_smallerFontSize * this.baseScale;
                this.m_blockTypeLabel.Position = (this.m_blockIconImage.Position + new Vector2(this.m_blockIconImage.Size.X, 0f)) + new Vector2(0.004f, -0.0025f);
                this.m_componentsLabel.Position = vector4 + new Vector2(0.006f, 0.015f + (num * this.baseScale));
                this.m_installedRequiredLabel.Position = vector2 + new Vector2(-0.011f, 0.015f + (num * this.baseScale));
            }
            this.m_blockIconImage.Position = vector + new Vector2(0.005f, 0.005f);
            Vector2 vector6 = !this.m_progressMode ? (vector + new Vector2(0.008f, (0.012f + this.m_componentsLabel.Size.Y) + (num * this.baseScale))) : (vector4 + new Vector2(0f, (0.012f + this.m_componentsLabel.Size.Y) + (num * this.baseScale)));
            for (int i = 0; i < this.BlockInfo.Components.Count; i++)
            {
                this.m_componentLines[i].Position = vector6 + new Vector2(0f, ((this.BlockInfo.Components.Count - i) - 1) * this.itemHeight);
                this.m_componentLines[i].IconPanelProgress.Visible = ShowComponentProgress;
                this.m_componentLines[i].IconImage.BorderColor = CriticalComponentColor;
                this.m_componentLines[i].NameLabel.TextScale = (this.m_smallerFontSize * this.baseScale) * 0.9f;
                this.m_componentLines[i].NumbersLabel.TextScale = (this.m_smallerFontSize * this.baseScale) * 0.9f;
                this.m_componentLines[i].NumbersLabel.PositionX = this.m_installedRequiredLabel.PositionX - (this.m_installedRequiredLabel.Size.X * 0.1f);
                if (this.m_progressMode)
                {
                    this.m_componentLines[i].IconImage.BackgroundTexture = null;
                    this.m_componentLines[i].NameLabel.PositionX = ((-this.m_componentLines[i].Size.X / 2f) + this.m_componentLines[i].IconImage.Size.X) - 0.006f;
                }
            }
        }

        private float baseScale =>
            (this.m_progressMode ? 1f : 0.83f);

        private float itemHeight =>
            (0.037f * this.baseScale);

        private class ComponentLineControl : MyGuiControlBase
        {
            public MyGuiControlImage IconImage;
            public MyGuiControlPanel IconPanelProgress;
            public MyGuiControlLabel NameLabel;
            public MyGuiControlLabel NumbersLabel;

            public ComponentLineControl(Vector2 size, float iconSize) : base(position, new Vector2?(size), colorMask, null, null, true, false, false, MyGuiControlHighlightType.WHEN_ACTIVE, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
            {
                Vector2? position = null;
                VRageMath.Vector4? colorMask = null;
                Vector2 vector = new Vector2(iconSize) * new Vector2(0.75f, 1f);
                Vector2 vector2 = new Vector2(-base.Size.X / 2f, 0f);
                Vector2 vector3 = new Vector2(base.Size.X / 2f, 0f);
                Vector2 vector4 = vector2 - new Vector2(0f, vector.Y / 2f);
                this.IconImage = new MyGuiControlImage();
                this.IconPanelProgress = new MyGuiControlPanel();
                position = null;
                position = null;
                colorMask = null;
                this.NameLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                position = null;
                position = null;
                colorMask = null;
                this.NumbersLabel = new MyGuiControlLabel(position, position, string.Empty, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.IconImage.Size = vector;
                this.IconImage.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.IconImage.Position = vector4;
                this.IconPanelProgress.Size = vector;
                this.IconPanelProgress.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                this.IconPanelProgress.Position = vector4;
                this.IconPanelProgress.BackgroundTexture = MyGuiConstants.TEXTURE_GUI_BLANK;
                float x = 0.1f;
                this.IconPanelProgress.ColorMask = new VRageMath.Vector4(x, x, x, 0.5f);
                this.NameLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.NameLabel.Position = vector2 + new Vector2(vector.X + 0.01225f, 0f);
                this.NameLabel.AutoEllipsis = true;
                this.NumbersLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
                this.NumbersLabel.Position = vector3 + new Vector2(-0.033f, 0f);
                base.Elements.Add(this.IconImage);
                base.Elements.Add(this.IconPanelProgress);
                base.Elements.Add(this.NameLabel);
                base.Elements.Add(this.NumbersLabel);
            }

            public void RecalcTextSize(bool progressMode)
            {
                this.NameLabel.BorderEnabled = false;
                this.NumbersLabel.BorderEnabled = false;
                float num = 0.002f;
                if (progressMode)
                {
                    Vector2 vector2 = new Vector2(base.Size.X / 2f, 0f);
                    this.NumbersLabel.Position = vector2 + new Vector2(-0.133f, 0f);
                }
                float num2 = ((this.NumbersLabel.Position.X - this.NameLabel.Position.X) - this.NumbersLabel.Size.X) - (2f * num);
                int num3 = 3;
                Vector2 vector = MyGuiManager.MeasureString(this.NameLabel.Font, this.NameLabel.TextToDraw, this.NameLabel.TextScale);
                while ((num3 > 0) && (vector.X > num2))
                {
                    this.NameLabel.TextScale *= 0.9f;
                    vector = MyGuiManager.MeasureString(this.NameLabel.Font, this.NameLabel.TextToDraw, this.NameLabel.TextScale);
                    num3--;
                }
                this.NameLabel.Size = new Vector2(num2 + num, this.NameLabel.Size.Y);
            }

            public void SetProgress(float val)
            {
                this.IconPanelProgress.Size = this.IconImage.Size * new Vector2(1f, 1f - val);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyControlBlockInfoStyle
        {
            public VRageMath.Vector4 BackgroundColormask;
            public string BlockNameLabelFont;
            public MyStringId ComponentsLabelText;
            public string ComponentsLabelFont;
            public MyStringId InstalledRequiredLabelText;
            public string InstalledRequiredLabelFont;
            public MyStringId RequiredAvailableLabelText;
            public MyStringId RequiredLabelText;
            public string IntegrityLabelFont;
            public VRageMath.Vector4 IntegrityBackgroundColor;
            public VRageMath.Vector4 IntegrityForegroundColor;
            public VRageMath.Vector4 IntegrityForegroundColorOverCritical;
            public VRageMath.Vector4 LeftColumnBackgroundColor;
            public VRageMath.Vector4 TitleBackgroundColor;
            public VRageMath.Vector4 TitleSeparatorColor;
            public string ComponentLineMissingFont;
            public string ComponentLineAllMountedFont;
            public string ComponentLineAllInstalledFont;
            public string ComponentLineDefaultFont;
            public VRageMath.Vector4 ComponentLineDefaultColor;
            public bool EnableBlockTypeLabel;
            public bool ShowAvailableComponents;
            public bool EnableBlockTypePanel;
            public bool HiddenPCU;
            public bool HiddenHeader;
        }
    }
}

