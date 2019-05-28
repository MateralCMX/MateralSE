namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenPerformanceWarnings : MyGuiScreenBase
    {
        private MyGuiControlList m_warningsList;
        private MyGuiControlCheckbox m_showWarningsCheckBox;
        private MyGuiControlCheckbox m_showAllCheckBox;
        private MyGuiControlCheckbox m_showAllBlockLimitsCheckBox;
        private MyGuiControlButton m_okButton;
        private Dictionary<MyStringId, WarningLine> m_warningLines;
        internal WarningArea m_areaTitleGraphics;
        internal WarningArea m_areaTitleBlocks;
        internal WarningArea m_areaTitleOther;
        internal WarningArea m_areaTitleUnsafeGrids;
        internal WarningArea m_areaTitleBlockLimits;
        internal WarningArea m_areaTitleServer;
        internal WarningArea m_areaTitlePerformance;
        internal WarningArea m_areaTitleGeneral;
        private int m_refreshCounter;
        private static bool m_showAll;
        private static bool m_showAllBlockLimits;

        public MyGuiScreenPerformanceWarnings() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.8436f, 0.97f), false, null, 0f, 0f)
        {
            this.m_warningLines = new Dictionary<MyStringId, WarningLine>();
            this.m_areaTitleGraphics = new WarningArea(MyCommonTexts.PerformanceWarningIssuesGraphics, MySessionComponentWarningSystem.Category.Graphics, true, false, false);
            this.m_areaTitleBlocks = new WarningArea(MyCommonTexts.PerformanceWarningIssuesBlocks, MySessionComponentWarningSystem.Category.Blocks, true, false, false);
            this.m_areaTitleOther = new WarningArea(MyCommonTexts.PerformanceWarningIssuesOther, MySessionComponentWarningSystem.Category.Other, false, false, false);
            this.m_areaTitleUnsafeGrids = new WarningArea(MyCommonTexts.PerformanceWarningIssuesUnsafeGrids, MySessionComponentWarningSystem.Category.UnsafeGrids, false, true, false);
            this.m_areaTitleBlockLimits = new WarningArea(MyCommonTexts.PerformanceWarningIssuesBlockBuildingLimits, MySessionComponentWarningSystem.Category.BlockLimits, false, true, false);
            this.m_areaTitleServer = new WarningArea(MyCommonTexts.PerformanceWarningIssuesServer, MySessionComponentWarningSystem.Category.Server, false, false, true);
            this.m_areaTitlePerformance = new WarningArea(MyCommonTexts.PerformanceWarningIssues, MySessionComponentWarningSystem.Category.Performance, false, false, false);
            this.m_areaTitleGeneral = new WarningArea(MyCommonTexts.PerformanceWarningIssuesGeneral, MySessionComponentWarningSystem.Category.General, false, false, false);
            this.m_refreshCounter = 120;
            base.EnabledBackgroundFade = true;
            base.CloseButtonEnabled = true;
            this.RecreateControls(true);
        }

        private void CreateBlockLimitsWarnings()
        {
            if (MySession.Static != null)
            {
                DateTime? nullable;
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(MySession.Static.LocalPlayerId);
                if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.NONE)
                {
                    nullable = null;
                    WarningLine line1 = new WarningLine(MyTexts.GetString(MyCommonTexts.WorldSettings_BlockLimits), MyTexts.GetString(MyCommonTexts.Disabled), this.m_areaTitleBlockLimits, nullable);
                }
                if (identity != null)
                {
                    if ((MySession.Static.MaxBlocksPerPlayer > 0) && ((identity.BlockLimits.BlocksBuilt >= identity.BlockLimits.MaxBlocks) || m_showAllBlockLimits))
                    {
                        nullable = null;
                        WarningLine line2 = new WarningLine(MyTexts.GetString(MyCommonTexts.PerformanceWarningBlocks), $"{identity.BlockLimits.BlocksBuilt}/{identity.BlockLimits.MaxBlocks} {MyTexts.GetString(MyCommonTexts.PerformanceWarningBlocksBuilt)}", this.m_areaTitleBlockLimits, nullable);
                    }
                    MyBlockLimits blockLimits = identity.BlockLimits;
                    if (MySession.Static.BlockLimitsEnabled == MyBlockLimitsEnabledEnum.PER_FACTION)
                    {
                        MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(identity.IdentityId);
                        if (playerFaction != null)
                        {
                            blockLimits = playerFaction.BlockLimits;
                        }
                    }
                    if ((MySession.Static.TotalPCU > -1) && ((blockLimits.PCU == 0) || m_showAllBlockLimits))
                    {
                        nullable = null;
                        WarningLine line3 = new WarningLine("PCU", $"{blockLimits.PCU} {MyTexts.GetString(MyCommonTexts.PerformanceWarningPCUAvailable)}", this.m_areaTitleBlockLimits, nullable);
                    }
                    foreach (KeyValuePair<string, short> pair in MySession.Static.BlockTypeLimits)
                    {
                        MyBlockLimits.MyTypeLimitData data;
                        identity.BlockLimits.BlockTypeBuilt.TryGetValue(pair.Key, out data);
                        MyCubeBlockDefinitionGroup group = MyDefinitionManager.Static.TryGetDefinitionGroup(pair.Key);
                        if ((group != null) && ((data != null) && ((data.BlocksBuilt >= MySession.Static.GetBlockTypeLimit(pair.Key)) || m_showAllBlockLimits)))
                        {
                            nullable = null;
                            WarningLine line4 = new WarningLine(group.Any.DisplayNameText, $"{data.BlocksBuilt}/{MySession.Static.GetBlockTypeLimit(pair.Key)} {MyTexts.GetString(MyCommonTexts.PerformanceWarningBlocksBuilt)}", this.m_areaTitleBlockLimits, nullable);
                        }
                    }
                }
            }
        }

        private void CreateNonProfilerWarnings()
        {
            DateTime? nullable;
            if (MySessionComponentWarningSystem.Static != null)
            {
                DateTime now = DateTime.Now;
                foreach (MySessionComponentWarningSystem.Warning warning in MySessionComponentWarningSystem.Static.CurrentWarnings)
                {
                    if ((m_showAll || (warning.Time == null)) || (warning.Time.Value.Subtract(now).Seconds < 5))
                    {
                        WarningLine line1 = new WarningLine(warning.Title, warning.Description, this.GetWarningAreaForCategory(warning.Category), warning.Time);
                    }
                }
            }
            if (MyFakes.PUBLIC_BETA_MP_TEST)
            {
                nullable = null;
                WarningLine line2 = new WarningLine("Public Beta Test build", "You are playing on experimental Public Beta Test build.", this.m_areaTitleServer, nullable);
            }
            if ((MySession.Static != null) && (MySession.Static.MultiplayerLastMsg > 3.0))
            {
                string description = string.Format(MyTexts.GetString(MyCommonTexts.Multiplayer_LastMsg), (int) MySession.Static.MultiplayerLastMsg);
                nullable = null;
                WarningLine line3 = new WarningLine(MyTexts.GetString(MyCommonTexts.PerformanceWarningIssuesServer_Response), description, this.m_areaTitleServer, nullable);
            }
            if ((((MySession.Static != null) && MySession.Static.IsSettingsExperimental()) || MySandboxGame.Config.ExperimentalMode) || MyDebugDrawSettings.DEBUG_DRAW_SERVER_WARNINGS)
            {
                nullable = null;
                WarningLine line4 = new WarningLine(MyTexts.GetString(MyCommonTexts.GeneralWarningIssues_Experimental), MyTexts.GetString(MyCommonTexts.General_Experimental), this.m_areaTitlePerformance, nullable);
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenPerformanceWarnings";

        private WarningArea GetWarningAreaForCategory(MySessionComponentWarningSystem.Category category)
        {
            switch (category)
            {
                case MySessionComponentWarningSystem.Category.Graphics:
                    return this.m_areaTitleGraphics;

                case MySessionComponentWarningSystem.Category.Blocks:
                    return this.m_areaTitleBlocks;

                case MySessionComponentWarningSystem.Category.Other:
                    return this.m_areaTitleOther;

                case MySessionComponentWarningSystem.Category.UnsafeGrids:
                    return this.m_areaTitleServer;

                case MySessionComponentWarningSystem.Category.BlockLimits:
                    return this.m_areaTitleBlockLimits;

                case MySessionComponentWarningSystem.Category.Server:
                    return this.m_areaTitleServer;

                case MySessionComponentWarningSystem.Category.Performance:
                    return this.m_areaTitlePerformance;

                case MySessionComponentWarningSystem.Category.General:
                    return this.m_areaTitleGeneral;
            }
            return this.m_areaTitleOther;
        }

        private void KeepInListChanged(MyGuiControlCheckbox obj)
        {
            m_showAll = obj.IsChecked;
        }

        private void KeepInListChangedBlockLimits(MyGuiControlCheckbox obj)
        {
            m_showAllBlockLimits = obj.IsChecked;
            this.m_areaTitleBlockLimits.Warnings.Clear();
            this.CreateBlockLimitsWarnings();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        private void OnOkButtonClicked(MyGuiControlButton obj)
        {
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyTexts.GetString(MyCommonTexts.PerformanceWarningHelpHeader), captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.87f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.87f, 0f, captionTextColor);
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.87f) / 2f, (base.m_size.Value.Y / 2f) - 0.847f), base.m_size.Value.X * 0.87f, 0f, captionTextColor);
            this.Controls.Add(control);
            captionTextColor = null;
            this.m_warningsList = new MyGuiControlList(new Vector2(0f, -0.05f), new Vector2(0.731f, 0.685f), captionTextColor, null, MyGuiControlListStyleEnum.Default);
            Vector2? size = null;
            captionTextColor = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2(-0.365f, 0.329f), size, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_EnablePerformanceWarnings), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            captionTextColor = null;
            this.m_showWarningsCheckBox = new MyGuiControlCheckbox(new Vector2((label.Position.X + label.Size.X) + 0.025f, 0.329f), captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsEnablePerformanceWarnings), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_showWarningsCheckBox.IsChecked = MySandboxGame.Config.EnablePerformanceWarnings;
            this.m_showWarningsCheckBox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_showWarningsCheckBox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.ShowWarningsChanged));
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2(this.m_showWarningsCheckBox.PositionX + 0.07f, 0.329f), size, MyTexts.GetString(MyCommonTexts.PerformanceWarningShowAll), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            captionTextColor = null;
            this.m_showAllCheckBox = new MyGuiControlCheckbox(new Vector2((label2.Position.X + label2.Size.X) + 0.025f, 0.329f), captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipPerformanceWarningShowAll), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_showAllCheckBox.IsChecked = m_showAll;
            this.m_showAllCheckBox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_showAllCheckBox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.KeepInListChanged));
            size = null;
            captionTextColor = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2(this.m_showAllCheckBox.PositionX + 0.07f, 0.329f), size, MyTexts.GetString(MyCommonTexts.PerformanceWarningShowAllBlockLimits), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            captionTextColor = null;
            this.m_showAllBlockLimitsCheckBox = new MyGuiControlCheckbox(new Vector2((label3.Position.X + label3.Size.X) + 0.025f, 0.329f), captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipPerformanceWarningShowAllBlockLimits), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_showAllBlockLimitsCheckBox.IsChecked = m_showAllBlockLimits;
            this.m_showAllBlockLimitsCheckBox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_showAllBlockLimitsCheckBox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.KeepInListChangedBlockLimits));
            StringBuilder contents = new StringBuilder(MyTexts.GetString(MyCommonTexts.PerformanceWarningInfoText));
            captionTextColor = null;
            int? visibleLinesCount = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text = new MyGuiControlMultilineText(new Vector2(-0.365f, 0.381f), new Vector2(0.4f, 0.2f), captionTextColor, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, contents, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            };
            size = null;
            captionTextColor = null;
            contents = MyTexts.Get(MyCommonTexts.Close);
            visibleLinesCount = null;
            this.m_okButton = new MyGuiControlButton(new Vector2(0.281f, 0.415f), MyGuiControlButtonStyleEnum.Default, size, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Close), contents, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, null, GuiSounds.MouseClick, 1f, visibleLinesCount, false);
            this.m_okButton.ButtonClicked += new Action<MyGuiControlButton>(this.OnOkButtonClicked);
            this.Controls.Add(this.m_warningsList);
            this.Controls.Add(label);
            this.Controls.Add(this.m_showWarningsCheckBox);
            this.Controls.Add(label2);
            this.Controls.Add(this.m_showAllCheckBox);
            this.Controls.Add(label3);
            this.Controls.Add(this.m_showAllBlockLimitsCheckBox);
            this.Controls.Add(text);
            this.Controls.Add(this.m_okButton);
        }

        private void Refresh()
        {
            float num = this.m_warningsList.GetScrollBar().Visible ? this.m_warningsList.GetScrollBar().Value : 0f;
            this.m_warningsList.Controls.Clear();
            if (this.m_refreshCounter < 60f)
            {
                this.m_refreshCounter++;
            }
            else
            {
                this.m_areaTitleOther.Warnings.Clear();
                this.m_areaTitleServer.Warnings.Clear();
                this.m_areaTitleBlocks.Warnings.Clear();
                this.m_areaTitleGeneral.Warnings.Clear();
                this.m_areaTitleGraphics.Warnings.Clear();
                this.m_areaTitleBlockLimits.Warnings.Clear();
                this.m_areaTitlePerformance.Warnings.Clear();
                this.CreateNonProfilerWarnings();
                this.CreateBlockLimitsWarnings();
                this.m_warningLines.Clear();
                foreach (MySimpleProfiler.PerformanceWarning warning in MySimpleProfiler.CurrentWarnings.Values)
                {
                    WarningLine line;
                    if (((warning.Time < 300f) || m_showAll) && !this.m_warningLines.TryGetValue(warning.Block.DisplayStringId, out line))
                    {
                        line = new WarningLine(warning, this);
                        this.m_warningLines.Add(warning.Block.DisplayStringId, line);
                    }
                }
                this.m_refreshCounter = 0;
            }
            this.m_areaTitleGraphics.Add(this.m_warningsList, m_showAll);
            this.m_areaTitleBlocks.Add(this.m_warningsList, m_showAll);
            this.m_areaTitleOther.Add(this.m_warningsList, m_showAll);
            this.m_areaTitleUnsafeGrids.Add(this.m_warningsList, m_showAll);
            this.m_areaTitleBlockLimits.Add(this.m_warningsList, m_showAll);
            this.m_areaTitleServer.Add(this.m_warningsList, m_showAll);
            this.m_areaTitlePerformance.Add(this.m_warningsList, m_showAll);
            this.m_warningsList.GetScrollBar().Value = num;
        }

        private void ShowWarningsChanged(MyGuiControlCheckbox obj)
        {
            MySandboxGame.Config.EnablePerformanceWarnings = obj.IsChecked;
        }

        public override bool Update(bool hasFocus)
        {
            this.Refresh();
            return base.Update(hasFocus);
        }

        internal class WarningArea
        {
            internal List<MyGuiScreenPerformanceWarnings.WarningLine> Warnings = new List<MyGuiScreenPerformanceWarnings.WarningLine>();
            private MyGuiControlParent m_header = new MyGuiControlParent();
            private MyGuiControlPanel m_titleBackground;
            private MyGuiControlLabel m_title;
            private MyGuiControlLabel m_lastOccurence;
            private MyGuiControlSeparatorList m_separator;
            private MyGuiControlButton m_refButton;

            public WarningArea(MyStringId name, MySessionComponentWarningSystem.Category areaType, bool refButton, bool unsafeGrid, bool serverMessage)
            {
                Vector2? position = null;
                position = null;
                VRageMath.Vector4? backgroundColor = null;
                this.m_titleBackground = new MyGuiControlPanel(position, position, backgroundColor, @"Textures\GUI\Controls\item_highlight_dark.dds", null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                position = null;
                position = null;
                backgroundColor = null;
                this.m_title = new MyGuiControlLabel(position, position, MyTexts.GetString(name), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.m_separator = new MyGuiControlSeparatorList();
                backgroundColor = null;
                this.m_separator.AddHorizontal(new Vector2(-0.45f, 0.018f), 0.9f, 0f, backgroundColor);
                this.m_title.Position = new Vector2(-0.33f, 0f);
                this.m_titleBackground.Size = new Vector2(this.m_titleBackground.Size.X, 0.035f);
                this.m_header.Size = new Vector2(this.m_header.Size.X, this.m_titleBackground.Size.Y);
                if (!unsafeGrid)
                {
                    position = null;
                    position = null;
                    backgroundColor = null;
                    this.m_lastOccurence = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.PerformanceWarningLastOccurrence), backgroundColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                    this.m_lastOccurence.Position = new Vector2(0.33f, 0f);
                }
                if (refButton)
                {
                    int? nullable3;
                    if (areaType == MySessionComponentWarningSystem.Category.Graphics)
                    {
                        position = null;
                        position = null;
                        backgroundColor = null;
                        nullable3 = null;
                        this.m_refButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ToolbarButton, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenCaptionGraphicsOptions), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGraphics()), GuiSounds.MouseClick, 1f, nullable3, false);
                    }
                    else if (areaType == MySessionComponentWarningSystem.Category.Blocks)
                    {
                        position = null;
                        position = null;
                        backgroundColor = null;
                        nullable3 = null;
                        this.m_refButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ToolbarButton, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_Cleanup), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenAdminMenu()), GuiSounds.MouseClick, 1f, nullable3, false);
                    }
                    else if (areaType == MySessionComponentWarningSystem.Category.Other)
                    {
                        position = null;
                        position = null;
                        backgroundColor = null;
                        nullable3 = null;
                        this.m_refButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.ToolbarButton, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenCaptionGraphicsOptions), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, sender => MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGame()), GuiSounds.MouseClick, 1f, nullable3, false);
                    }
                }
            }

            public void Add(MyGuiControlList list, bool showAll)
            {
                this.m_header.Position = Vector2.Zero;
                if (this.m_header.Controls.Count == 0)
                {
                    this.m_header.Controls.Add(this.m_titleBackground);
                    this.m_header.Controls.Add(this.m_title);
                    this.m_header.Controls.Add(this.m_separator);
                    if (this.m_lastOccurence != null)
                    {
                        this.m_header.Controls.Add(this.m_lastOccurence);
                    }
                }
                bool flag = false;
                this.Warnings.Sort(delegate (MyGuiScreenPerformanceWarnings.WarningLine x, MyGuiScreenPerformanceWarnings.WarningLine y) {
                    if ((x.Warning != null) || (y.Warning != null))
                    {
                        return (x.Warning != null) ? ((y.Warning != null) ? (x.Warning.Time - y.Warning.Time) : x.Warning.Time) : -y.Warning.Time;
                    }
                    return 0;
                });
                foreach (MyGuiScreenPerformanceWarnings.WarningLine line in this.Warnings)
                {
                    if (((line.Warning == null) || (line.Warning.Time < 300f)) | showAll)
                    {
                        if (!flag)
                        {
                            list.Controls.Add(this.m_header);
                            flag = true;
                        }
                        line.Prepare();
                        list.Controls.Add(line.Parent);
                    }
                }
                if (flag)
                {
                    if (this.m_refButton != null)
                    {
                        list.Controls.Add(this.m_refButton);
                    }
                    MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
                    VRageMath.Vector4? color = null;
                    control.AddHorizontal(new Vector2(-0.45f, 0f), 0.9f, 0f, color);
                    control.Size = new Vector2(1f, 0.005f);
                    control.ColorMask = new VRageMath.Vector4(0f, 0f, 0f, 0f);
                    list.Controls.Add(control);
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyGuiScreenPerformanceWarnings.WarningArea.<>c <>9 = new MyGuiScreenPerformanceWarnings.WarningArea.<>c();
                public static Action<MyGuiControlButton> <>9__7_0;
                public static Action<MyGuiControlButton> <>9__7_1;
                public static Action<MyGuiControlButton> <>9__7_2;
                public static Comparison<MyGuiScreenPerformanceWarnings.WarningLine> <>9__8_0;

                internal void <.ctor>b__7_0(MyGuiControlButton sender)
                {
                    MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGraphics());
                }

                internal void <.ctor>b__7_1(MyGuiControlButton sender)
                {
                    MyGuiSandbox.AddScreen(new MyGuiScreenAdminMenu());
                }

                internal void <.ctor>b__7_2(MyGuiControlButton sender)
                {
                    MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGame());
                }

                internal int <Add>b__8_0(MyGuiScreenPerformanceWarnings.WarningLine x, MyGuiScreenPerformanceWarnings.WarningLine y)
                {
                    if ((x.Warning != null) || (y.Warning != null))
                    {
                        return ((x.Warning != null) ? ((y.Warning != null) ? (x.Warning.Time - y.Warning.Time) : x.Warning.Time) : -y.Warning.Time);
                    }
                    return 0;
                }
            }
        }

        internal class WarningLine
        {
            public MySimpleProfiler.PerformanceWarning Warning;
            private MyGuiControlLabel m_name;
            private MyGuiControlMultilineText m_description;
            public MyGuiControlParent Parent;
            private MyGuiControlSeparatorList m_separator;
            private MyGuiControlLabel m_time;

            public WarningLine(MySimpleProfiler.PerformanceWarning warning, MyGuiScreenPerformanceWarnings screen)
            {
                this.Parent = new MyGuiControlParent();
                string displayName = warning.Block.DisplayName;
                Vector2? size = null;
                VRageMath.Vector4? colorMask = null;
                this.m_name = new MyGuiControlLabel(new Vector2(-0.33f, 0f), size, displayName, colorMask, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                if (this.m_name.Size.X > 0.14f)
                {
                    this.m_name.Text = this.Truncate(displayName, 15, "..");
                }
                this.m_name.SetToolTip(displayName);
                this.m_description = new MyGuiControlMultilineText();
                this.m_description.Position = new Vector2(-0.18f, 0f);
                this.m_description.Size = new Vector2(0.45f, 0.2f);
                this.m_description.Text = new StringBuilder(string.IsNullOrEmpty(warning.Block.Description.String) ? "" : MyTexts.GetString(warning.Block.Description));
                this.m_description.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.Size = new Vector2(0.45f, this.m_description.TextSize.Y);
                this.Parent.Size = new Vector2(this.Parent.Size.X, this.m_description.Size.Y);
                this.m_separator = new MyGuiControlSeparatorList();
                colorMask = null;
                this.m_separator.AddVertical(new Vector2(-0.19f, (-this.Parent.Size.Y / 2f) - 0.006f), this.Parent.Size.Y + 0.016f, 0f, colorMask);
                colorMask = null;
                this.m_separator.AddVertical(new Vector2(0.26f, (-this.Parent.Size.Y / 2f) - 0.006f), this.Parent.Size.Y + 0.016f, 0f, colorMask);
                size = null;
                colorMask = null;
                this.m_time = new MyGuiControlLabel(new Vector2(0.33f, 0f), size, null, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                switch (warning.Block.Type)
                {
                    case MySimpleProfiler.ProfilingBlockType.GPU:
                    case MySimpleProfiler.ProfilingBlockType.RENDER:
                        screen.m_areaTitleGraphics.Warnings.Add(this);
                        break;

                    case MySimpleProfiler.ProfilingBlockType.MOD:
                    case MySimpleProfiler.ProfilingBlockType.OTHER:
                        screen.m_areaTitleOther.Warnings.Add(this);
                        break;

                    case MySimpleProfiler.ProfilingBlockType.BLOCK:
                        screen.m_areaTitleBlocks.Warnings.Add(this);
                        break;

                    default:
                        break;
                }
                this.Warning = warning;
            }

            public WarningLine(string name, string description, MyGuiScreenPerformanceWarnings.WarningArea area, DateTime? time = new DateTime?())
            {
                this.Parent = new MyGuiControlParent();
                string text = name;
                Vector2? size = null;
                VRageMath.Vector4? colorMask = null;
                this.m_name = new MyGuiControlLabel(new Vector2(-0.33f, 0f), size, text, colorMask, 0.8f, "Red", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                if (this.m_name.Size.X > 0.14f)
                {
                    this.m_name.Text = this.Truncate(text, 15, "..");
                }
                this.m_name.SetToolTip(text);
                this.m_name.ShowTooltipWhenDisabled = true;
                this.m_description = new MyGuiControlMultilineText();
                this.m_description.Position = new Vector2(-0.18f, 0f);
                this.m_description.Size = new Vector2(0.45f, 0.2f);
                this.m_description.Text = new StringBuilder(description);
                this.m_description.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                this.m_description.Size = new Vector2(0.45f, this.m_description.TextSize.Y);
                this.m_separator = new MyGuiControlSeparatorList();
                this.Parent.Size = new Vector2(this.Parent.Size.X, this.m_description.Size.Y);
                colorMask = null;
                this.m_separator.AddVertical(new Vector2(-0.19f, (-this.Parent.Size.Y / 2f) - 0.006f), this.Parent.Size.Y + 0.016f, 0f, colorMask);
                colorMask = null;
                this.m_separator.AddVertical(new Vector2(0.35f, (-this.Parent.Size.Y / 2f) - 0.006f), this.Parent.Size.Y + 0.016f, 0f, colorMask);
                size = null;
                colorMask = null;
                this.m_time = new MyGuiControlLabel(new Vector2(0.33f, 0f), size, null, colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
                if (time != null)
                {
                    colorMask = null;
                    this.m_separator.AddVertical(new Vector2(0.26f, (-this.Parent.Size.Y / 2f) - 0.006f), this.Parent.Size.Y + 0.016f, 0f, colorMask);
                    TimeSpan span = (TimeSpan) (DateTime.Now - time.Value);
                    this.m_time.Text = $"{span.Hours}:{span.Minutes:00}:{span.Seconds:00}";
                }
                area.Warnings.Add(this);
            }

            public void Prepare()
            {
                this.Parent.Position = Vector2.Zero;
                if (this.Warning != null)
                {
                    TimeSpan span = TimeSpan.FromSeconds((double) ((int) (this.Warning.Time * 0.01666667f)));
                    this.m_time.Text = $"{span.Hours}:{span.Minutes:00}:{span.Seconds:00}";
                }
                if (this.Parent.Controls.Count == 0)
                {
                    this.Parent.Controls.Add(this.m_name);
                    this.Parent.Controls.Add(this.m_description);
                    this.Parent.Controls.Add(this.m_separator);
                    this.Parent.Controls.Add(this.m_time);
                }
            }

            private string Truncate(string input, int maxLenght, string tooLongSuffix) => 
                ((input.Length >= maxLenght) ? (input.Substring(0, maxLenght - tooLongSuffix.Length) + tooLongSuffix) : input);
        }
    }
}

