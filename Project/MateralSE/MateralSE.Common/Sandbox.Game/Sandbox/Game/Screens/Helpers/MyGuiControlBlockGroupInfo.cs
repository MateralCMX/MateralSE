namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    [MyGuiControlType(typeof(MyObjectBuilder_GuiControlBlockGroupInfo))]
    public class MyGuiControlBlockGroupInfo : MyGuiControlStackPanel
    {
        private MyGuiControlLabel m_title;
        private MyGuiControlGrid m_blockVariantGrid;
        private MyGuiControlMultilineText m_helpText;
        private MyGuiControlBlockInfo m_componentsInfo;
        private MyGuiControlPanel m_helpTextBackground;
        private MyGuiControlButton m_buttonDlcStore;
        private MyGuiControlPanel m_componentsBackground;
        private MyGuiControlButton m_blockTypeIconSmall;
        private MyGuiControlButton m_blockTypeIconLarge;
        private MyCubeSize m_userSizeChoice;
        private float m_blockNameOriginalOffset;

        private void AddBlockVariantDefinition(Span<MyCubeBlockDefinition> primary, Span<MyCubeBlockDefinition> secondary)
        {
            for (int i = 0; i < primary.Count; i++)
            {
                MyCubeBlockDefinition originalDefinition = primary[i];
                if (originalDefinition != null)
                {
                    MyCubeBlockDefinition definition2;
                    primary[i] = null;
                    GetPairDefinition(secondary, originalDefinition, out definition2);
                    this.AddItemVariantDefinition(originalDefinition, definition2);
                }
            }
        }

        private void AddItemVariantDefinition(MyCubeBlockDefinition primary, MyCubeBlockDefinition secondary)
        {
            if (primary != null)
            {
                MyCubeBlockDefinition definition1 = secondary;
            }
            string toolTip = null;
            string[] icons = null;
            if (!IsAllowed(primary))
            {
                primary = null;
            }
            else
            {
                icons = primary.Icons;
                toolTip = primary.DisplayNameText;
            }
            if (!IsAllowed(secondary))
            {
                secondary = null;
            }
            else
            {
                icons = secondary.Icons;
                toolTip = secondary.DisplayNameText;
            }
            if ((primary != null) || (secondary != null))
            {
                this.m_blockVariantGrid.Add(new MyGuiGridItem(icons, null, toolTip, delegate {
                    bool smallExists = false;
                    bool largeExists = false;
                    MyCubeBlockDefinition[] definitions = new MyCubeBlockDefinition[] { primary, secondary };
                    foreach (MyCubeBlockDefinition definition in definitions)
                    {
                        if (definition != null)
                        {
                            if (definition.CubeSize == MyCubeSize.Small)
                            {
                                smallExists = true;
                            }
                            else
                            {
                                largeExists = true;
                            }
                        }
                    }
                    this.UpdateSizeIcons(smallExists, largeExists);
                    this.SetBlockDetail(definitions);
                }, true), 0);
            }
        }

        private void ClearGrid()
        {
            this.m_blockVariantGrid.SelectedIndex = null;
            this.m_blockVariantGrid.SetItemsToDefault();
            this.m_blockVariantGrid.RowsCount = 2;
        }

        private static MyGuiControlBlockInfo CreateBlockInfoControl()
        {
            MyGuiControlBlockInfo.MyControlBlockInfoStyle style = new MyGuiControlBlockInfo.MyControlBlockInfoStyle {
                BackgroundColormask = new VRageMath.Vector4(0.1333333f, 0.1803922f, 0.2039216f, 1f),
                BlockNameLabelFont = "Blue",
                EnableBlockTypeLabel = false,
                ComponentsLabelText = MySpaceTexts.HudBlockInfo_Components,
                ComponentsLabelFont = "Blue",
                InstalledRequiredLabelText = MySpaceTexts.HudBlockInfo_Installed_Required,
                InstalledRequiredLabelFont = "Blue",
                RequiredLabelText = MyCommonTexts.HudBlockInfo_Required,
                IntegrityLabelFont = "White",
                IntegrityBackgroundColor = new VRageMath.Vector4(0.3058824f, 0.454902f, 0.5372549f, 1f),
                IntegrityForegroundColor = new VRageMath.Vector4(0.5f, 0.1f, 0.1f, 1f),
                IntegrityForegroundColorOverCritical = new VRageMath.Vector4(0.4627451f, 0.6509804f, 0.7529412f, 1f),
                LeftColumnBackgroundColor = new VRageMath.Vector4(0f, 0f, 1f, 0f),
                TitleBackgroundColor = new VRageMath.Vector4(0.2078431f, 0.2666667f, 0.2980392f, 1f),
                TitleSeparatorColor = new VRageMath.Vector4(0.4117647f, 0.4666667f, 0.5176471f, 1f),
                ComponentLineMissingFont = "Red",
                ComponentLineAllMountedFont = "White",
                ComponentLineAllInstalledFont = "Blue",
                ComponentLineDefaultFont = "Blue",
                ComponentLineDefaultColor = new VRageMath.Vector4(0.6f, 0.6f, 0.6f, 1f),
                ShowAvailableComponents = false,
                EnableBlockTypePanel = false,
                HiddenPCU = false,
                HiddenHeader = true
            };
            MyGuiControlBlockInfo info1 = new MyGuiControlBlockInfo(style, false, true);
            info1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            info1.BlockInfo = new MyHudBlockInfo();
            info1.BackgroundTexture = null;
            return info1;
        }

        public override void Draw(float transitionAlpha, float backgroundTransitionAlpha)
        {
            Vector2 positionAbsoluteTopLeft = base.GetPositionAbsoluteTopLeft();
            MyGuiConstants.TEXTURE_WBORDER_LIST.Draw(positionAbsoluteTopLeft, base.Size, ApplyColorMaskModifiers(base.ColorMask, base.Enabled, backgroundTransitionAlpha), 1f);
            base.Draw(transitionAlpha, backgroundTransitionAlpha);
        }

        public void ForEachChild(Action<MyGuiControlStackPanel, MyGuiControlBase> action)
        {
            this.ForEachChildRecursive(this, action);
        }

        private void ForEachChildRecursive(MyGuiControlStackPanel parent, Action<MyGuiControlStackPanel, MyGuiControlBase> action)
        {
            foreach (MyGuiControlBase base2 in parent.GetControls(true))
            {
                action(parent, base2);
                MyGuiControlStackPanel panel = base2 as MyGuiControlStackPanel;
                if (panel != null)
                {
                    this.ForEachChildRecursive(panel, action);
                }
            }
        }

        private static void GetPairDefinition(Span<MyCubeBlockDefinition> array, MyCubeBlockDefinition originalDefinition, out MyCubeBlockDefinition pairDefinition)
        {
            for (int i = 0; i < array.Count; i++)
            {
                MyCubeBlockDefinition definition = array[i];
                if ((definition != null) && (definition.BlockPairName == originalDefinition.BlockPairName))
                {
                    array[i] = null;
                    pairDefinition = definition;
                    return;
                }
            }
            pairDefinition = null;
        }

        public override unsafe void Init(MyObjectBuilder_GuiControlBase builder)
        {
            base.Init(builder);
            this.m_userSizeChoice = MyCubeBuilder.Static.CubeBuilderState.CubeSizeMode;
            this.m_title = new MyGuiControlLabel();
            this.m_title.Size = new Vector2(0.77f, 1f);
            MyGuiControlButton.StyleDefinition definition1 = new MyGuiControlButton.StyleDefinition();
            definition1.NormalFont = "White";
            definition1.HighlightFont = "White";
            definition1.NormalTexture = MyGuiConstants.TEXTURE_HUD_GRID_LARGE_FIT;
            definition1.HighlightTexture = MyGuiConstants.TEXTURE_HUD_GRID_LARGE_FIT;
            MyGuiControlButton.StyleDefinition buttonStyle = definition1;
            MyGuiControlButton.StyleDefinition definition3 = new MyGuiControlButton.StyleDefinition();
            definition3.NormalFont = "White";
            definition3.HighlightFont = "White";
            definition3.NormalTexture = MyGuiConstants.TEXTURE_HUD_GRID_SMALL_FIT;
            definition3.HighlightTexture = MyGuiConstants.TEXTURE_HUD_GRID_SMALL_FIT;
            MyGuiControlButton.StyleDefinition definition2 = definition3;
            this.m_blockTypeIconLarge = new MyGuiControlButton();
            this.m_blockTypeIconLarge.SetCustomStyle(buttonStyle);
            this.m_blockTypeIconLarge.Size = new Vector2(0f, 0.7f);
            Thickness thickness = new Thickness(0.01f, 0.15f, 0f, 0f);
            this.m_blockTypeIconLarge.Margin = thickness;
            this.m_blockTypeIconSmall = new MyGuiControlButton();
            this.m_blockTypeIconSmall.SetCustomStyle(definition2);
            this.m_blockTypeIconSmall.Size = this.m_blockTypeIconLarge.Size;
            thickness.Left = 0.05f;
            this.m_blockTypeIconSmall.Margin = thickness;
            this.m_blockTypeIconSmall.ClickCallbackRespectsEnabledState = false;
            this.m_blockTypeIconLarge.ClickCallbackRespectsEnabledState = false;
            this.m_blockTypeIconLarge.ButtonClicked += new Action<MyGuiControlButton>(this.OnSizeSelectorClicked);
            this.m_blockTypeIconSmall.ButtonClicked += new Action<MyGuiControlButton>(this.OnSizeSelectorClicked);
            MyGuiControlStackPanel control = new MyGuiControlStackPanel {
                Orientation = MyGuiOrientation.Horizontal,
                Size = new Vector2(0.95f, 0.06f),
                Margin = new Thickness(0.025f, 0f, 0f, 0f)
            };
            control.Add(this.m_title);
            control.Add(this.m_blockTypeIconSmall);
            control.Add(this.m_blockTypeIconLarge);
            base.Add(control);
            this.m_blockVariantGrid = new MyGuiControlGrid();
            this.m_blockVariantGrid.VisualStyle = MyGuiControlGridStyleEnum.BlockInfo;
            this.m_blockVariantGrid.RowsCount = 2;
            this.m_blockVariantGrid.ColumnsCount = 8;
            this.m_blockVariantGrid.Size = new Vector2(1f, 0.143f);
            this.m_blockVariantGrid.Margin = new Thickness(0.013f, 0f, 0f, 0f);
            this.m_blockVariantGrid.ItemSelected += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.OnBlockVariantSelected);
            base.Add(this.m_blockVariantGrid);
            this.m_helpTextBackground = new MyGuiControlPanel();
            this.m_helpTextBackground.Size = new Vector2(0.95f, 0.29f);
            this.m_helpTextBackground.Margin = new Thickness(0.025f, 0f, 0f, 0.01f);
            this.m_helpTextBackground.ColorMask = new VRageMath.Vector4(0.1333333f, 0.1803922f, 0.2039216f, 0.9f);
            this.m_helpTextBackground.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? backgroundColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            this.m_helpText = new MyGuiControlMultilineText(position, position, backgroundColor, "Blue", 0.64f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, visibleLinesCount, false, false, null, textPadding);
            this.m_helpText.Size = new Vector2(1f, 1f);
            this.m_helpText.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            base.Add(this.m_helpTextBackground);
            position = null;
            position = null;
            backgroundColor = null;
            visibleLinesCount = null;
            this.m_buttonDlcStore = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.BlocksScreen_DLCStore), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnDLCStoreClick), GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_buttonDlcStore.Visible = false;
            this.m_componentsBackground = new MyGuiControlPanel();
            this.m_componentsBackground.Size = new Vector2(0.95f, 0.484f);
            this.m_componentsBackground.Margin = new Thickness(0.025f, 0f, 0f, 0f);
            this.m_componentsBackground.ColorMask = new VRageMath.Vector4(0.1333333f, 0.1803922f, 0.2039216f, 0.9f);
            this.m_componentsBackground.BackgroundTexture = new MyGuiCompositeTexture(@"Textures\GUI\Blank.dds");
            base.Add(this.m_componentsBackground);
            this.m_componentsInfo = CreateBlockInfoControl();
            this.ForEachChild(delegate (MyGuiControlStackPanel parent, MyGuiControlBase control) {
                Vector2 size = parent.Size;
                Vector2 vector2 = control.Size * size;
                if (vector2.X == 0f)
                {
                    Vector2* vectorPtr1 = (Vector2*) ref vector2;
                    vectorPtr1->X = vector2.Y;
                }
                else if (vector2.Y == 0f)
                {
                    Vector2* vectorPtr2 = (Vector2*) ref vector2;
                    vectorPtr2->Y = vector2.X;
                }
                if (control is MyGuiControlButton)
                {
                    vector2 *= new Vector2(0.75f, 1f);
                }
                control.Size = vector2;
                control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                Thickness margin = control.Margin;
                control.Margin = new Thickness(margin.Left * size.X, margin.Top * size.Y, margin.Right * size.X, margin.Bottom * size.Y);
            });
        }

        private static bool IsAllowed(MyDefinitionBase blockDefinition) => 
            (((blockDefinition != null) && (blockDefinition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS)) && (blockDefinition.AvailableInSurvival || !MySession.Static.SurvivalMode));

        private void OnBlockVariantSelected(MyGuiControlGrid _, MyGuiControlGrid.EventArgs args)
        {
            this.RecreateDetail();
        }

        private void OnDLCStoreClick(MyGuiControlButton button)
        {
            MyDLCs.MyDLC userData = button.UserData as MyDLCs.MyDLC;
            if (userData != null)
            {
                MyGameService.OpenOverlayUrl(userData.URL);
            }
        }

        private void OnSizeSelectorClicked(MyGuiControlButton x)
        {
            MyGuiControlButton button = ReferenceEquals(x, this.m_blockTypeIconLarge) ? this.m_blockTypeIconSmall : this.m_blockTypeIconLarge;
            if (button.Visible)
            {
                x.Enabled = !x.Enabled;
                button.Enabled = !x.Enabled;
                this.m_userSizeChoice = this.m_blockTypeIconLarge.Enabled ? MyCubeSize.Large : MyCubeSize.Small;
                this.RecreateDetail();
            }
        }

        private void RecreateDetail()
        {
            MyGuiGridItem selectedItem = this.m_blockVariantGrid.SelectedItem;
            if (selectedItem == null)
            {
                this.m_blockVariantGrid.SelectedIndex = 0;
            }
            else
            {
                ((Action) selectedItem.UserData)();
            }
        }

        public void RegisterAllControls(MyGuiControls controls)
        {
            this.ForEachChild((_, x) => controls.Add(x));
            controls.Add(this.m_helpText);
            controls.Add(this.m_buttonDlcStore);
            controls.Add(this.m_componentsInfo);
        }

        private void SetBlockDetail(MyCubeBlockDefinition[] definitions)
        {
            foreach (MyCubeBlockDefinition definition in definitions)
            {
                if (definition != null)
                {
                    this.SetTexts(definition);
                    MyGuiControlButton button = (definition.CubeSize == MyCubeSize.Large) ? this.m_blockTypeIconLarge : this.m_blockTypeIconSmall;
                    if (button.Enabled && button.Visible)
                    {
                        this.m_componentsInfo.BlockInfo.LoadDefinition(definition, true);
                        return;
                    }
                }
            }
        }

        public void SetBlockGroup(MyCubeBlockDefinitionGroup group)
        {
            this.ClearGrid();
            this.SetBlockModeEnabled(true);
            this.AddItemVariantDefinition(group.Small, group.Large);
            int length = 0;
            MyDefinitionId[] blockStages = null;
            MyDefinitionId[] blockStages = null;
            if (group.Small != null)
            {
                blockStages = group.Small.BlockStages;
                if (blockStages != null)
                {
                    length = blockStages.Length;
                }
            }
            if (group.Large != null)
            {
                blockStages = group.Large.BlockStages;
                if (blockStages != null)
                {
                    length = Math.Max(length, blockStages.Length);
                }
            }
            MyDefinitionId[] lhs = blockStages;
            MyDefinitionId[] rhs = blockStages;
            MyCubeBlockDefinition[] array = new MyCubeBlockDefinition[length * 2];
            Span<MyCubeBlockDefinition> primary = array.Span<MyCubeBlockDefinition>(0, new int?(length));
            Span<MyCubeBlockDefinition> secondary = array.Span<MyCubeBlockDefinition>(length, new int?(length));
            if (this.m_userSizeChoice == MyCubeSize.Large)
            {
                MyUtils.Swap<MyDefinitionId[]>(ref lhs, ref rhs);
            }
            for (int i = 0; i < length; i++)
            {
                if ((lhs != null) && (i < lhs.Length))
                {
                    MyCubeBlockDefinition definition;
                    MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(lhs[i], out definition);
                    primary[i] = definition;
                }
                if ((rhs != null) && (i < rhs.Length))
                {
                    MyCubeBlockDefinition definition2;
                    MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(rhs[i], out definition2);
                    secondary[i] = definition2;
                }
            }
            this.AddBlockVariantDefinition(primary, secondary);
            this.AddBlockVariantDefinition(secondary, primary);
            this.m_blockVariantGrid.SelectedIndex = 0;
        }

        public void SetBlockModeEnabled(bool enabled)
        {
            this.m_componentsInfo.Visible = enabled;
            this.m_blockTypeIconLarge.Visible = false;
            this.m_blockTypeIconSmall.Visible = false;
            this.m_componentsBackground.Visible = enabled;
        }

        public void SetGeneralDefinition(MyDefinitionBase definition)
        {
            this.ClearGrid();
            this.SetBlockModeEnabled(false);
            this.m_blockVariantGrid.Add(new MyGuiGridItem(definition.Icons, null, definition.DisplayNameText, () => this.SetGeneralDefinitionDetail(definition), true), 0);
            this.m_blockVariantGrid.SelectedIndex = 0;
        }

        private void SetGeneralDefinitionDetail(MyDefinitionBase definition)
        {
            this.SetTexts(definition);
        }

        private void SetTexts(MyDefinitionBase definition)
        {
            StringBuilder text = (definition.DisplayNameEnum != null) ? MyTexts.Get(definition.DisplayNameEnum.Value) : new StringBuilder(definition.DisplayNameText);
            Vector2 vector = MyGuiManager.MeasureString(this.m_title.Font, text, 1f);
            float num = Math.Min((float) (this.m_title.Size.X / vector.X), (float) 1f);
            Vector2 size = this.m_title.Size;
            this.m_title.TextToDraw = text;
            this.m_title.TextScale = num / MyGuiManager.LanguageTextScale;
            this.m_title.Size = size;
            this.m_title.PositionY = this.m_blockNameOriginalOffset + ((size.Y - (vector.Y * this.m_title.TextScaleWithLanguage)) / 2f);
            MyDLCs.MyDLC firstMissingDefinitionDLC = MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId);
            this.m_helpText.Text = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(definition.DescriptionText))
            {
                this.m_helpText.AppendText(definition.DescriptionText);
            }
            if (firstMissingDefinitionDLC != null)
            {
                if (!string.IsNullOrWhiteSpace(definition.DescriptionText))
                {
                    this.m_helpText.AppendText("\n");
                    this.m_helpText.AppendText("\n");
                }
                this.m_helpText.AppendImage(firstMissingDefinitionDLC.Icon, new Vector2(20f, 20f) / MyGuiConstants.GUI_OPTIMAL_SIZE, (VRageMath.Vector4) Color.White);
                this.m_helpText.AppendText("     ");
                this.m_helpText.AppendText(MyDLCs.GetRequiredDLCTooltip(firstMissingDefinitionDLC.AppId));
            }
            this.m_buttonDlcStore.Visible = firstMissingDefinitionDLC != null;
            this.m_buttonDlcStore.UserData = firstMissingDefinitionDLC;
        }

        public override void UpdateArrange()
        {
            base.UpdateArrange();
            this.ForEachChild(delegate (MyGuiControlStackPanel _, MyGuiControlBase child) {
                MyGuiControlStackPanel panel = child as MyGuiControlStackPanel;
                if (panel != null)
                {
                    panel.UpdateArrange();
                }
            });
            this.m_helpText.Size = this.m_helpTextBackground.Size * 0.9f;
            this.m_helpText.Position = this.m_helpTextBackground.Position + (this.m_helpTextBackground.Size * 0.05f);
            this.m_buttonDlcStore.Position = (this.m_helpTextBackground.Position + (this.m_helpTextBackground.Size / 2f)) + new Vector2(0f, 0.07f);
            this.m_blockNameOriginalOffset = this.m_title.PositionY;
            this.m_componentsInfo.Size = this.m_componentsBackground.Size;
            this.m_componentsInfo.Position = this.m_componentsBackground.Position;
        }

        private void UpdateSizeIcons(bool smallExists, bool largeExists)
        {
            this.m_blockTypeIconSmall.Visible = smallExists;
            this.m_blockTypeIconLarge.Visible = largeExists;
            MyGuiControlButton blockTypeIconSmall = this.m_blockTypeIconSmall;
            MyGuiControlButton blockTypeIconLarge = this.m_blockTypeIconLarge;
            if (this.m_userSizeChoice == MyCubeSize.Large)
            {
                MyUtils.Swap<MyGuiControlButton>(ref blockTypeIconSmall, ref blockTypeIconLarge);
            }
            if (!blockTypeIconSmall.Visible)
            {
                MyUtils.Swap<MyGuiControlButton>(ref blockTypeIconSmall, ref blockTypeIconLarge);
            }
            blockTypeIconSmall.Enabled = true;
            blockTypeIconLarge.Enabled = false;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiControlBlockGroupInfo.<>c <>9 = new MyGuiControlBlockGroupInfo.<>c();
            public static Action<MyGuiControlStackPanel, MyGuiControlBase> <>9__11_0;
            public static Action<MyGuiControlStackPanel, MyGuiControlBase> <>9__19_0;

            internal unsafe void <Init>b__11_0(MyGuiControlStackPanel parent, MyGuiControlBase control)
            {
                Vector2 size = parent.Size;
                Vector2 vector2 = control.Size * size;
                if (vector2.X == 0f)
                {
                    Vector2* vectorPtr1 = (Vector2*) ref vector2;
                    vectorPtr1->X = vector2.Y;
                }
                else if (vector2.Y == 0f)
                {
                    Vector2* vectorPtr2 = (Vector2*) ref vector2;
                    vectorPtr2->Y = vector2.X;
                }
                if (control is MyGuiControlButton)
                {
                    vector2 *= new Vector2(0.75f, 1f);
                }
                control.Size = vector2;
                control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                Thickness margin = control.Margin;
                control.Margin = new Thickness(margin.Left * size.X, margin.Top * size.Y, margin.Right * size.X, margin.Bottom * size.Y);
            }

            internal void <UpdateArrange>b__19_0(MyGuiControlStackPanel _, MyGuiControlBase child)
            {
                MyGuiControlStackPanel panel = child as MyGuiControlStackPanel;
                if (panel != null)
                {
                    panel.UpdateArrange();
                }
            }
        }
    }
}

