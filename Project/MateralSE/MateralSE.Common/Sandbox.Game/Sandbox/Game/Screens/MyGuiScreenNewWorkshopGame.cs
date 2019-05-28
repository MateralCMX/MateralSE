namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Localization;
    using VRage.GameServices;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public class MyGuiScreenNewWorkshopGame : MyGuiScreenBase
    {
        private MyGuiControlScreenSwitchPanel m_screenSwitchPanel;
        public List<MyWorkshopItem> SubscribedWorlds;
        private MyGuiBlueprintScreen_Reworked.SortOption m_sort;
        private bool m_showThumbnails;
        private MyGuiControlButton m_buttonRefresh;
        private MyGuiControlButton m_buttonSorting;
        private MyGuiControlButton m_buttonOpenWorkshop;
        private MyGuiControlButton m_buttonToggleThumbnails;
        private MyGuiControlImage m_iconRefresh;
        private MyGuiControlImage m_iconSorting;
        private MyGuiControlImage m_iconOpenWorkshop;
        private MyGuiControlImage m_iconToggleThumbnails;
        private MyGuiControlSearchBox m_searchBox;
        private MyGuiControlList m_worldList;
        private MyGuiControlRadioButtonGroup m_worldTypesGroup;
        private MyObjectBuilder_Checkpoint m_selectedWorld;
        private MyLayoutTable m_tableLayout;
        private MyGuiControlLabel m_nameLabel;
        private MyGuiControlLabel m_nameText;
        private MyGuiControlLabel m_onlineModeLabel;
        private MyGuiControlCombobox m_onlineMode;
        private MyGuiControlSlider m_maxPlayersSlider;
        private MyGuiControlLabel m_maxPlayersLabel;
        private MyGuiControlLabel m_authorLabel;
        private MyGuiControlLabel m_authorText;
        private MyGuiControlLabel m_ratingLabel;
        private MyGuiControlRating m_ratingDisplay;
        private MyGuiControlMultilineText m_noSubscribedItemsText;
        private MyGuiControlPanel m_noSubscribedItemsPanel;
        private MyGuiControlMultilineText m_descriptionMultilineText;
        private MyGuiControlPanel m_descriptionPanel;
        private MyGuiControlRotatingWheel m_asyncLoadingWheel;
        private float MARGIN_TOP;
        private float MARGIN_BOTTOM;
        private float MARGIN_LEFT_INFO;
        private float MARGIN_RIGHT;
        private float MARGIN_LEFT_LIST;

        public MyGuiScreenNewWorkshopGame() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.878f, 0.97f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.m_showThumbnails = true;
            this.MARGIN_TOP = 0.22f;
            this.MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            this.MARGIN_LEFT_INFO = 15f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void AddSeparator(string sectionName)
        {
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK;
            panel1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            panel1.Position = Vector2.Zero;
            MyGuiControlCompositePanel control = panel1;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = sectionName;
            label1.Font = "Blue";
            label1.PositionX = 0.005f;
            MyGuiControlLabel label = label1;
            float num = 0.003f;
            Color color = MyGuiConstants.THEMED_GUI_LINE_COLOR;
            Vector2? position = null;
            position = null;
            VRageMath.Vector4? backgroundColor = null;
            string[] textures = new string[] { @"Textures\GUI\FogSmall3.dds" };
            MyGuiControlImage image1 = new MyGuiControlImage(position, position, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            image1.Size = new Vector2(label.Size.X + (num * 10f), 0.007f);
            image1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            image1.ColorMask = color.ToVector4();
            image1.Position = new Vector2(-num, label.Size.Y);
            MyGuiControlImage image = image1;
            MyGuiControlParent parent1 = new MyGuiControlParent();
            parent1.Size = new Vector2(this.m_worldList.Size.X, label.Size.Y);
            parent1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            parent1.Position = Vector2.Zero;
            MyGuiControlParent parent = parent1;
            control.Size = parent.Size + new Vector2(-0.035f, 0.01f);
            control.Position -= (parent.Size / 2f) - new Vector2(-0.01f, 0f);
            label.Position -= parent.Size / 2f;
            image.Position -= parent.Size / 2f;
            parent.Controls.Add(control);
            parent.Controls.Add(image);
            parent.Controls.Add(label);
            this.m_worldList.Controls.Add(parent);
        }

        private void AddWorldButton(MyObjectBuilder_Checkpoint world, MyWorkshopItem workshopItem, bool isLocalMod = false, bool isWorkshopMod = false)
        {
            string sessionName = world.SessionName;
            MyLocalizationContext context = MyLocalization.Static[world.SessionName];
            if (context != null)
            {
                StringBuilder builder = context["Name"];
                if (builder != null)
                {
                    sessionName = builder.ToString();
                }
            }
            MyGuiControlContentButton button1 = new MyGuiControlContentButton(sessionName, this.GetImagePath(world));
            button1.UserData = new MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem>(world, workshopItem);
            button1.IsLocalMod = isLocalMod;
            button1.IsWorkshopMod = isWorkshopMod;
            button1.Key = this.m_worldTypesGroup.Count;
            MyGuiControlContentButton control = button1;
            control.SetPreviewVisibility(this.m_showThumbnails);
            control.SetTooltip(sessionName);
            this.m_worldTypesGroup.Add(control);
            this.m_worldList.Controls.Add(control);
        }

        private void ApplyFiltering()
        {
            bool flag = false;
            string[] strArray = new string[0];
            if (this.m_searchBox != null)
            {
                flag = this.m_searchBox.SearchText != "";
                char[] separator = new char[] { ' ' };
                strArray = this.m_searchBox.SearchText.Split(separator);
            }
            foreach (MyGuiControlBase base2 in this.m_worldList.Controls)
            {
                MyGuiControlContentButton button = base2 as MyGuiControlContentButton;
                if (button != null)
                {
                    bool flag2 = true;
                    if (flag)
                    {
                        string str = button.Title.ToLower();
                        foreach (string str2 in strArray)
                        {
                            if (!str.Contains(str2.ToLower()))
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }
                    base2.Visible = flag2;
                }
            }
            this.m_worldList.SetScrollBarPage(0f);
        }

        private IMyAsyncResult beginAction() => 
            new LoadListResult();

        private IMyAsyncResult beginActionLoadSaves() => 
            new MyLoadWorldInfoListResult(null);

        private MyGuiControlButton CreateToolbarButton(Vector2 position, MyStringId tooltip, Action<MyGuiControlButton> onClick)
        {
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            MyGuiControlButton control = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Default, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onClick, GuiSounds.MouseClick, 1f, buttonIndex, false) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP,
                VisualStyle = MyGuiControlButtonStyleEnum.Rectangular,
                ShowTooltipWhenDisabled = true
            };
            control.SetToolTip(tooltip);
            control.Size = new Vector2(0.029f, 0.03358333f);
            this.Controls.Add(control);
            return control;
        }

        private MyGuiControlImage CreateToolbarButtonIcon(MyGuiControlButton button, string texture)
        {
            button.Size = new Vector2(button.Size.X, (button.Size.X * 4f) / 3f);
            float y = 0.95f * Math.Min(button.Size.X, button.Size.Y);
            Vector2? size = new Vector2(y * 0.75f, y);
            VRageMath.Vector4? backgroundColor = null;
            string[] textures = new string[] { texture };
            MyGuiControlImage control = new MyGuiControlImage(new Vector2?(button.Position + new Vector2(-0.0016f, 0.018f)), size, backgroundColor, null, textures, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.Controls.Add(control);
            return control;
        }

        private void endAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            LoadListResult result2 = (LoadListResult) result;
            this.SubscribedWorlds = result2.SubscribedWorlds;
            if (result2.Success)
            {
                this.OnSuccess();
            }
            screen.CloseScreen();
            if (this.SubscribedWorlds != null)
            {
                this.m_noSubscribedItemsPanel.Visible = this.SubscribedWorlds.Count == 0;
                this.m_noSubscribedItemsText.Visible = this.SubscribedWorlds.Count == 0;
            }
            else
            {
                this.m_noSubscribedItemsPanel.Visible = true;
                this.m_noSubscribedItemsText.Visible = true;
            }
        }

        private void endActionLoadSaves(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            screen.CloseScreen();
            MyWorkshopItem world = ((MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem>) screen.UserData).Item2;
            if (Directory.Exists(MyLocalCache.GetSessionSavesPath(MyUtils.StripInvalidChars(world.Title), false, false)))
            {
                this.OverwriteWorldDialog(world);
            }
            else
            {
                MyWorkshop.CreateWorldInstanceAsync(world, MyWorkshop.MyWorkshopPathInfo.CreateWorldInfo(), false, delegate (bool success, string sessionPath) {
                    if (success)
                    {
                        this.OnSuccessStart(sessionPath);
                    }
                    else
                    {
                        StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                        MyStringId? okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        okButtonText = null;
                        Vector2? size = null;
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.WorldFileCouldNotBeLoaded), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                    }
                });
            }
        }

        private void FillList()
        {
            this.m_worldList.Clear();
            this.m_worldTypesGroup.Clear();
            MyStringId? cancelText = null;
            MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginAction), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endAction), null));
        }

        public override string GetFriendlyName() => 
            "New Workshop Game";

        private string GetImagePath(MyObjectBuilder_Checkpoint world)
        {
            string briefingVideo = world.BriefingVideo;
            return (!string.IsNullOrEmpty(world.BriefingVideo) ? briefingVideo : string.Empty);
        }

        private unsafe void InitRightSide()
        {
            int num = 5;
            Vector2 topLeft = (-base.m_size.Value / 2f) + new Vector2(((this.MARGIN_LEFT_LIST + this.m_worldList.Size.X) + this.MARGIN_LEFT_INFO) + 0.012f, this.MARGIN_TOP - 0.011f);
            Vector2 vector2 = base.m_size.Value;
            Vector2 size = new Vector2((vector2.X / 2f) - topLeft.X, ((vector2.Y - this.MARGIN_TOP) - this.MARGIN_BOTTOM) - 0.0345f) - new Vector2(this.MARGIN_RIGHT, 0.12f);
            float num2 = size.X * 0.6f;
            float num3 = size.X - num2;
            float num4 = 0.052f;
            float num5 = size.Y - (num * num4);
            this.m_tableLayout = new MyLayoutTable(this, topLeft, size);
            float[] widthsPx = new float[] { num2 - 0.055f, num3 + 0.055f };
            this.m_tableLayout.SetColumnWidthsNormalized(widthsPx);
            float[] heightsPx = new float[] { num4, num4, num4, num4, num4, num5 };
            this.m_tableLayout.SetRowHeightsNormalized(heightsPx);
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Text = MyTexts.GetString(MyCommonTexts.Name);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_nameLabel = label1;
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_nameText = label2;
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Text = MyTexts.GetString(MyCommonTexts.WorldSettings_Author);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_authorLabel = label3;
            MyGuiControlLabel label4 = new MyGuiControlLabel();
            label4.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_authorText = label4;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Text = MyTexts.GetString(MyCommonTexts.WorldSettings_Rating);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_ratingLabel = label5;
            MyGuiControlRating rating1 = new MyGuiControlRating(8, 10);
            rating1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_ratingDisplay = rating1;
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Text = MyTexts.GetString(MyCommonTexts.WorldSettings_OnlineMode);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
            this.m_onlineModeLabel = label6;
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox();
            combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
            this.m_onlineMode = combobox1;
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_onlineMode.AddItem(0L, MyCommonTexts.WorldSettings_OnlineModeOffline, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(3L, MyCommonTexts.WorldSettings_OnlineModePrivate, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(2L, MyCommonTexts.WorldSettings_OnlineModeFriends, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_onlineMode.AddItem(1L, MyCommonTexts.WorldSettings_OnlineModePublic, sortOrder, toolTip);
            this.m_onlineMode.SelectItemByIndex(0);
            this.m_onlineMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_onlineMode_ItemSelected);
            this.m_onlineMode.Enabled = false;
            float x = this.m_onlineMode.Size.X;
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            this.m_maxPlayersSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 2f, (float) MyMultiplayerLobby.MAX_PLAYERS, x, defaultValue, color, new StringBuilder("{0}").ToString(), 0, 0.8f, 0.028f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, true);
            Vector2? position = null;
            position = null;
            color = null;
            this.m_maxPlayersLabel = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.MaxPlayers), color, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.m_maxPlayersSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxPlayer));
            position = null;
            position = null;
            color = null;
            sortOrder = null;
            MyGuiBorderThickness? textPadding = null;
            MyGuiControlMultilineText text1 = new MyGuiControlMultilineText(position, position, color, "Blue", 0.8f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, sortOrder, false, false, null, textPadding);
            text1.Name = "BriefingMultilineText";
            text1.Position = new Vector2(-0.009f, -0.115f);
            text1.Size = new Vector2(0.419f, 0.412f);
            text1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text1.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_descriptionMultilineText = text1;
            MyGuiControlCompositePanel panel1 = new MyGuiControlCompositePanel();
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            this.m_descriptionPanel = panel1;
            this.m_tableLayout.Add(this.m_nameLabel, MyAlignH.Left, MyAlignV.Center, 0, 0, 1, 1);
            this.m_tableLayout.Add(this.m_authorLabel, MyAlignH.Left, MyAlignV.Center, 1, 0, 1, 1);
            this.m_tableLayout.Add(this.m_onlineModeLabel, MyAlignH.Left, MyAlignV.Center, 2, 0, 1, 1);
            this.m_tableLayout.Add(this.m_maxPlayersLabel, MyAlignH.Left, MyAlignV.Center, 3, 0, 1, 1);
            this.m_tableLayout.Add(this.m_ratingLabel, MyAlignH.Left, MyAlignV.Center, 4, 0, 1, 1);
            this.m_nameLabel.PositionX -= 0.003f;
            this.m_authorLabel.PositionX -= 0.003f;
            this.m_onlineModeLabel.PositionX -= 0.003f;
            this.m_maxPlayersLabel.PositionX -= 0.003f;
            this.m_ratingLabel.PositionX -= 0.003f;
            this.m_tableLayout.AddWithSize(this.m_nameText, MyAlignH.Left, MyAlignV.Center, 0, 1, 1, 1);
            this.m_tableLayout.AddWithSize(this.m_authorText, MyAlignH.Left, MyAlignV.Center, 1, 1, 1, 1);
            this.m_tableLayout.AddWithSize(this.m_onlineMode, MyAlignH.Left, MyAlignV.Center, 2, 1, 1, 1);
            this.m_tableLayout.AddWithSize(this.m_maxPlayersSlider, MyAlignH.Left, MyAlignV.Center, 3, 1, 1, 1);
            this.m_tableLayout.AddWithSize(this.m_ratingDisplay, MyAlignH.Left, MyAlignV.Center, 4, 1, 1, 1);
            this.m_nameText.PositionX -= 0.001f;
            this.m_nameText.Size += new Vector2(0.002f, 0f);
            this.m_onlineMode.PositionX -= 0.002f;
            this.m_onlineMode.PositionY -= 0.005f;
            this.m_maxPlayersSlider.PositionX -= 0.003f;
            this.m_tableLayout.AddWithSize(this.m_descriptionPanel, MyAlignH.Left, MyAlignV.Top, 5, 0, 1, 2);
            this.m_tableLayout.AddWithSize(this.m_descriptionMultilineText, MyAlignH.Left, MyAlignV.Top, 5, 0, 1, 2);
            this.m_descriptionMultilineText.PositionY += 0.012f;
            float num6 = 0.01f;
            this.m_descriptionPanel.Position = new Vector2(this.m_descriptionPanel.PositionX - num6, (this.m_descriptionPanel.PositionY - num6) + 0.012f);
            this.m_descriptionPanel.Size = new Vector2(this.m_descriptionPanel.Size.X + num6, (this.m_descriptionPanel.Size.Y + (num6 * 2f)) - 0.012f);
            Vector2 vector4 = base.m_size.Value / 2f;
            float* singlePtr1 = (float*) ref vector4.X;
            singlePtr1[0] -= this.MARGIN_RIGHT + 0.004f;
            float* singlePtr2 = (float*) ref vector4.Y;
            singlePtr2[0] -= this.MARGIN_BOTTOM + 0.004f;
            Vector2 vector5 = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 vector1 = MyGuiConstants.GENERIC_BUTTON_SPACING;
            Vector2 vector6 = MyGuiConstants.GENERIC_BUTTON_SPACING;
            color = null;
            sortOrder = null;
            MyGuiControlButton control = new MyGuiControlButton(new Vector2?(vector4), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector5), color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Start), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClicked), GuiSounds.MouseClick, 1f, sortOrder, false);
            control.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewGame_Start));
            this.m_descriptionPanel.Size = new Vector2(this.m_descriptionPanel.Size.X, this.m_descriptionPanel.Size.Y + MyGuiConstants.BACK_BUTTON_SIZE.Y);
            this.m_descriptionMultilineText.Size = new Vector2(this.m_descriptionMultilineText.Size.X, this.m_descriptionMultilineText.Size.Y + MyGuiConstants.BACK_BUTTON_SIZE.Y);
            this.Controls.Add(control);
            color = null;
            sortOrder = null;
            MyGuiControlButton button2 = new MyGuiControlButton(new Vector2?(control.Position - new Vector2(control.Size.X + 0.01f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector5), color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.ScreenLoadSubscribedWorldOpenInWorkshop), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOpenInWorkshopClicked), GuiSounds.MouseClick, 1f, sortOrder, false);
            button2.SetToolTip(MyTexts.GetString(MyCommonTexts.ToolTipWorkshopOpenInWorkshop_Steam));
            this.Controls.Add(button2);
            base.CloseButtonEnabled = true;
            MyGuiControlCompositePanel panel2 = new MyGuiControlCompositePanel();
            panel2.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            this.m_noSubscribedItemsPanel = panel2;
            this.m_tableLayout.AddWithSize(this.m_noSubscribedItemsPanel, MyAlignH.Left, MyAlignV.Top, 0, 0, 6, 2);
            this.m_noSubscribedItemsPanel.Position = new Vector2(this.m_descriptionPanel.Position.X, this.m_worldList.Position.Y);
            this.m_noSubscribedItemsPanel.Size = new Vector2(this.m_descriptionPanel.Size.X, this.m_worldList.Size.Y - (1.63f * MyGuiConstants.BACK_BUTTON_SIZE.Y));
            position = null;
            position = null;
            color = null;
            sortOrder = null;
            textPadding = null;
            MyGuiControlMultilineText text2 = new MyGuiControlMultilineText(position, position, color, "Blue", 0.7394595f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, null, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, sortOrder, false, false, null, textPadding);
            text2.Size = new Vector2(this.m_descriptionMultilineText.Size.X, this.m_descriptionMultilineText.Size.Y * 2f);
            text2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text2.TextAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            text2.TextBoxAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_noSubscribedItemsText = text2;
            this.m_tableLayout.AddWithSize(this.m_noSubscribedItemsText, MyAlignH.Left, MyAlignV.Top, 0, 0, 6, 2);
            this.m_noSubscribedItemsText.Position = this.m_noSubscribedItemsPanel.Position + new Vector2(num6);
            this.m_noSubscribedItemsText.Size = this.m_noSubscribedItemsPanel.Size - new Vector2(2f * num6);
            this.m_noSubscribedItemsText.Clear();
            this.m_noSubscribedItemsText.AppendText(MyTexts.Get(MySpaceTexts.ToolTipNewGame_NoWorkshopWorld), "Blue", this.m_noSubscribedItemsText.TextScale, VRageMath.Vector4.One);
            this.m_noSubscribedItemsText.AppendLine();
            this.m_noSubscribedItemsText.AppendLink(MySteamConstants.URL_BROWSE_WORKSHOP_WORLDS, "Space Engineers Steam Workshop");
            this.m_noSubscribedItemsText.AppendLine();
            this.m_noSubscribedItemsText.OnLinkClicked += new LinkClicked(this.OnLinkClicked);
            this.m_noSubscribedItemsText.ScrollbarOffsetV = 1f;
            this.m_noSubscribedItemsPanel.Visible = false;
            this.m_noSubscribedItemsText.Visible = false;
        }

        private void InitWorldList()
        {
            float y = 0.31f;
            float x = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(x, y);
            Vector2 position = new Vector2(-0.366f, -0.261f);
            this.m_buttonRefresh = this.CreateToolbarButton(position, MyCommonTexts.WorldSettings_Tooltip_Refresh, new Action<MyGuiControlButton>(this.OnRefreshClicked));
            this.m_buttonSorting = this.CreateToolbarButton(position + new Vector2(this.m_buttonRefresh.Size.X, 0f), MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButSort, new Action<MyGuiControlButton>(this.OnSortingClicked));
            this.m_buttonToggleThumbnails = this.CreateToolbarButton(position + new Vector2(this.m_buttonRefresh.Size.X * 2f, 0f), MyCommonTexts.WorldSettings_Tooltip_ToggleThumbnails, new Action<MyGuiControlButton>(this.OnToggleThumbnailsClicked));
            this.m_buttonOpenWorkshop = this.CreateToolbarButton(position + new Vector2(this.m_buttonRefresh.Size.X * 3f, 0f), MySpaceTexts.ScreenBlueprintsRew_Tooltip_ButOpenWorkshop, new Action<MyGuiControlButton>(this.OnOpenWorkshopClicked));
            this.m_iconRefresh = this.CreateToolbarButtonIcon(this.m_buttonRefresh, @"Textures\GUI\Icons\Blueprints\Refresh.png");
            this.m_iconSorting = this.CreateToolbarButtonIcon(this.m_buttonSorting, "");
            this.SetIconForSorting();
            this.m_iconToggleThumbnails = this.CreateToolbarButtonIcon(this.m_buttonToggleThumbnails, "");
            this.m_iconOpenWorkshop = this.CreateToolbarButtonIcon(this.m_buttonOpenWorkshop, @"Textures\GUI\Icons\Blueprints\Steam.png");
            this.SetIconForHideThumbnails();
            this.m_worldTypesGroup = new MyGuiControlRadioButtonGroup();
            this.m_worldTypesGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.WorldSelectionChanged);
            this.m_worldTypesGroup.MouseDoubleClick += new Action<MyGuiControlRadioButton>(this.WorldDoubleClick);
            MyGuiControlList list1 = new MyGuiControlList();
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            list1.Position = vector;
            list1.Size = new Vector2(MyGuiConstants.LISTBOX_WIDTH, (base.m_size.Value.Y - y) - 0.048f);
            this.m_worldList = list1;
            this.Controls.Add(this.m_worldList);
            this.m_searchBox = new MyGuiControlSearchBox(new Vector2(-0.382f, -0.22f), new Vector2(this.m_worldList.Size.X, 0.032f), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.OnSearchTextChange);
            this.Controls.Add(this.m_searchBox);
        }

        private void m_onlineMode_ItemSelected()
        {
            this.m_maxPlayersSlider.Enabled = this.m_onlineMode.Enabled && (this.m_onlineMode.GetSelectedIndex() > 0);
        }

        private void OnCancelButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        private void OnLinkClicked(MyGuiControlBase sender, string url)
        {
            MyGuiSandbox.OpenUrlWithFallback(url, "Space Engineers Steam Workshop", false);
        }

        private void OnOkButtonClicked(MyGuiControlButton myGuiControlButton)
        {
            this.StartSelectedWorld();
        }

        private void OnOpenInWorkshopClicked(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback((this.m_selectedWorld != null) ? string.Format(MySteamConstants.URL_WORKSHOP_VIEW_ITEM_FORMAT, this.m_selectedWorld.WorkshopId) : MySteamConstants.URL_BROWSE_WORKSHOP_SCENARIOS, "Steam Workshop", false);
        }

        private void OnOpenWorkshopClicked(MyGuiControlButton button)
        {
            MyGuiSandbox.OpenUrlWithFallback(MySteamConstants.URL_BROWSE_WORKSHOP_SCENARIOS, "Steam Workshop", false);
        }

        private void OnRefreshClicked(MyGuiControlButton button)
        {
            this.m_selectedWorld = null;
            this.m_worldList.Clear();
            this.m_worldTypesGroup.Clear();
            this.FillList();
        }

        private void OnSearchTextChange(string message)
        {
            this.ApplyFiltering();
            if (this.m_worldTypesGroup.Count > 0)
            {
                this.m_worldTypesGroup.SelectByIndex(0);
            }
        }

        private void OnSortingClicked(MyGuiControlButton button)
        {
            switch (this.m_sort)
            {
                case MyGuiBlueprintScreen_Reworked.SortOption.None:
                    this.m_sort = MyGuiBlueprintScreen_Reworked.SortOption.Alphabetical;
                    break;

                case MyGuiBlueprintScreen_Reworked.SortOption.Alphabetical:
                    this.m_sort = MyGuiBlueprintScreen_Reworked.SortOption.CreationDate;
                    break;

                case MyGuiBlueprintScreen_Reworked.SortOption.CreationDate:
                    this.m_sort = MyGuiBlueprintScreen_Reworked.SortOption.UpdateDate;
                    break;

                case MyGuiBlueprintScreen_Reworked.SortOption.UpdateDate:
                    this.m_sort = MyGuiBlueprintScreen_Reworked.SortOption.None;
                    break;

                default:
                    break;
            }
            this.SetIconForSorting();
            this.m_selectedWorld = null;
            this.m_worldList.Clear();
            this.m_worldTypesGroup.Clear();
            this.OnSuccess();
        }

        private void OnSuccess()
        {
            List<string> texturesToLoad = new List<string>();
            this.SortItems(this.SubscribedWorlds);
            foreach (MyWorkshopItem item in this.SubscribedWorlds)
            {
                MyObjectBuilder_Checkpoint world = new MyObjectBuilder_Checkpoint {
                    SessionName = item.Title
                };
                string[] paths = new string[] { item.Folder + @"\thumb.jpg" };
                string path = Path.Combine(paths);
                if (MyFileSystem.FileExists(path))
                {
                    world.BriefingVideo = path;
                    texturesToLoad.Add(path);
                }
                world.Description = item.Description;
                world.WorkshopId = new ulong?(item.Id);
                this.AddWorldButton(world, item, false, false);
            }
            MyRenderProxy.PreloadTextures(texturesToLoad, VRageRender.Messages.TextureType.GUIWithoutPremultiplyAlpha);
            if ((this.m_worldTypesGroup.SelectedIndex == null) && (this.m_worldList.Controls.Count > 0))
            {
                this.m_worldTypesGroup.SelectByIndex(0);
            }
        }

        private void OnSuccessStart(string sessionPath)
        {
            MyOnlineModeEnum? onlineMode = null;
            MySessionLoader.LoadSingleplayerSession(sessionPath, null, null, onlineMode, 0);
        }

        private void OnToggleThumbnailsClicked(MyGuiControlButton button)
        {
            this.m_showThumbnails = !this.m_showThumbnails;
            this.SetIconForHideThumbnails();
            foreach (MyGuiControlContentButton button2 in this.m_worldList.Controls)
            {
                if (button2 != null)
                {
                    button2.SetPreviewVisibility(this.m_showThumbnails);
                }
            }
            this.m_worldList.Recalculate();
        }

        private void OverwriteWorldDialog(MyWorkshopItem world)
        {
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextWorldExistsDownloadOverwrite), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum callbackReturn) {
                if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MyWorkshop.CreateWorldInstanceAsync(world, MyWorkshop.MyWorkshopPathInfo.CreateWorldInfo(), true, delegate (bool success, string sessionPath) {
                        if (success)
                        {
                            this.OnSuccessStart(sessionPath);
                        }
                        else
                        {
                            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                            MyStringId? nullable = null;
                            nullable = null;
                            nullable = null;
                            nullable = null;
                            Vector2? nullable2 = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.WorldFileCouldNotBeLoaded), messageCaption, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                        }
                    });
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            VRageMath.Vector4? captionTextColor = null;
            Vector2? captionOffset = null;
            base.AddCaption(MyCommonTexts.ScreenMenuButtonCampaign, captionTextColor, captionOffset, 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.38f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.625f, 0f, captionTextColor);
            this.Controls.Add(control);
            this.m_screenSwitchPanel = new MyGuiControlScreenSwitchPanel(this, MyTexts.Get(MyCommonTexts.WorkshopScreen_Description));
            this.InitWorldList();
            this.InitRightSide();
            this.FillList();
        }

        public override bool RegisterClicks() => 
            true;

        private void SetIconForHideThumbnails()
        {
            this.m_iconToggleThumbnails.SetTexture(this.m_showThumbnails ? @"Textures\GUI\Icons\Blueprints\ThumbnailsON.png" : @"Textures\GUI\Icons\Blueprints\ThumbnailsOFF.png");
        }

        private void SetIconForSorting()
        {
            switch (this.m_sort)
            {
                case MyGuiBlueprintScreen_Reworked.SortOption.None:
                    this.m_iconSorting.SetTexture(@"Textures\GUI\Icons\Blueprints\NoSorting.png");
                    return;

                case MyGuiBlueprintScreen_Reworked.SortOption.Alphabetical:
                    this.m_iconSorting.SetTexture(@"Textures\GUI\Icons\Blueprints\Alphabetical.png");
                    return;

                case MyGuiBlueprintScreen_Reworked.SortOption.CreationDate:
                    this.m_iconSorting.SetTexture(@"Textures\GUI\Icons\Blueprints\ByCreationDate.png");
                    return;

                case MyGuiBlueprintScreen_Reworked.SortOption.UpdateDate:
                    this.m_iconSorting.SetTexture(@"Textures\GUI\Icons\Blueprints\ByUpdateDate.png");
                    return;
            }
            this.m_iconSorting.SetTexture(@"Textures\GUI\Icons\Blueprints\NoSorting.png");
        }

        private void SortItems(List<MyWorkshopItem> list)
        {
            MyWorkshopItemComparer comparer = null;
            switch (this.m_sort)
            {
                case MyGuiBlueprintScreen_Reworked.SortOption.Alphabetical:
                    comparer = new MyWorkshopItemComparer((x, y) => x.Title.CompareTo(y.Title));
                    break;

                case MyGuiBlueprintScreen_Reworked.SortOption.CreationDate:
                    comparer = new MyWorkshopItemComparer((x, y) => x.TimeCreated.CompareTo(y.TimeCreated));
                    break;

                case MyGuiBlueprintScreen_Reworked.SortOption.UpdateDate:
                    comparer = new MyWorkshopItemComparer((x, y) => x.TimeUpdated.CompareTo(y.TimeUpdated));
                    break;

                default:
                    break;
            }
            if (comparer != null)
            {
                list.Sort(comparer);
            }
        }

        private void StartSelectedWorld()
        {
            if (((this.m_selectedWorld != null) && (this.m_worldTypesGroup.SelectedButton != null)) && (this.m_worldTypesGroup.SelectedButton.UserData != null))
            {
                MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem> userData = (MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem>) this.m_worldTypesGroup.SelectedButton.UserData;
                MyStringId? cancelText = null;
                MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, cancelText, new Func<IMyAsyncResult>(this.beginActionLoadSaves), new Action<IMyAsyncResult, MyGuiScreenProgressAsync>(this.endActionLoadSaves), userData));
            }
        }

        private void WorldDoubleClick(MyGuiControlRadioButton obj)
        {
            this.StartSelectedWorld();
        }

        private void WorldSelectionChanged(MyGuiControlRadioButtonGroup args)
        {
            MyGuiControlContentButton selectedButton = args.SelectedButton as MyGuiControlContentButton;
            if ((selectedButton != null) && (selectedButton.UserData != null))
            {
                MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem> userData = (MyTuple<MyObjectBuilder_Checkpoint, MyWorkshopItem>) selectedButton.UserData;
                string[] paths = new string[] { userData.Item2.Folder + @"\Sandbox.sbc" };
                string path = Path.Combine(paths);
                string displayName = "";
                bool flag = false;
                if (MyFileSystem.FileExists(path))
                {
                    MyObjectBuilder_Checkpoint checkpoint;
                    if (MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Checkpoint>(path, out checkpoint))
                    {
                        MyObjectBuilder_Identity identity = checkpoint.Identities.Find(x => x.CharacterEntityId == checkpoint.ControlledObject);
                        if (identity != null)
                        {
                            displayName = identity.DisplayName;
                        }
                        if (string.IsNullOrEmpty(userData.Item1.Description))
                        {
                            userData.Item1.Description = checkpoint.Briefing;
                        }
                        flag = checkpoint.OnlineMode != MyOnlineModeEnum.OFFLINE;
                    }
                }
                string sessionName = "";
                MyLocalizationContext context = MyLocalization.Static[userData.Item1.SessionName];
                if (context != null)
                {
                    StringBuilder builder = context["Name"];
                    if (builder != null)
                    {
                        sessionName = builder.ToString();
                    }
                    this.m_descriptionMultilineText.Text = context["Description"];
                }
                if (string.IsNullOrEmpty(sessionName))
                {
                    sessionName = userData.Item1.SessionName;
                    this.m_descriptionMultilineText.Text = new StringBuilder(userData.Item1.Description);
                }
                string str4 = sessionName;
                if (str4.Length > 40)
                {
                    str4 = str4.Remove(0x20) + "...";
                }
                this.m_nameText.SetToolTip(sessionName);
                this.m_nameLabel.SetToolTip(sessionName);
                this.m_nameText.Text = str4;
                if (flag)
                {
                    this.m_onlineMode.Enabled = true;
                }
                else
                {
                    this.m_onlineMode.Enabled = false;
                    this.m_onlineMode.SelectItemByIndex(0);
                }
                this.m_authorText.Text = displayName;
                this.m_ratingDisplay.Value = (int) Math.Round((double) (userData.Item2.Score * 10f));
                this.m_maxPlayersSlider.Enabled = this.m_onlineMode.Enabled && (this.m_onlineMode.GetSelectedIndex() > 0);
                this.m_selectedWorld = userData.Item1;
                this.m_descriptionMultilineText.SetScrollbarPageV(0f);
                this.m_descriptionMultilineText.SetScrollbarPageH(0f);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenNewWorkshopGame.<>c <>9 = new MyGuiScreenNewWorkshopGame.<>c();
            public static Func<MyWorkshopItem, MyWorkshopItem, int> <>9__67_0;
            public static Func<MyWorkshopItem, MyWorkshopItem, int> <>9__67_1;
            public static Func<MyWorkshopItem, MyWorkshopItem, int> <>9__67_2;

            internal int <SortItems>b__67_0(MyWorkshopItem x, MyWorkshopItem y) => 
                x.Title.CompareTo(y.Title);

            internal int <SortItems>b__67_1(MyWorkshopItem x, MyWorkshopItem y) => 
                x.TimeCreated.CompareTo(y.TimeCreated);

            internal int <SortItems>b__67_2(MyWorkshopItem x, MyWorkshopItem y) => 
                x.TimeUpdated.CompareTo(y.TimeUpdated);
        }

        private class LoadListResult : IMyAsyncResult
        {
            public bool Success;
            public List<MyWorkshopItem> SubscribedWorlds;

            public LoadListResult()
            {
                this.Task = Parallel.Start(() => this.LoadListAsync(out this.SubscribedWorlds));
            }

            private void LoadListAsync(out List<MyWorkshopItem> list)
            {
                List<MyWorkshopItem> results = new List<MyWorkshopItem>();
                if (!MyWorkshop.GetSubscribedWorldsBlocking(results))
                {
                    list = null;
                }
                else
                {
                    list = results;
                    List<MyWorkshopItem> list3 = new List<MyWorkshopItem>();
                    if (MyWorkshop.GetSubscribedScenariosBlocking(list3) && (list3.Count > 0))
                    {
                        list.InsertRange(list.Count, list3);
                    }
                }
                this.SubscribedWorlds = list;
                this.Success = MyWorkshop.TryUpdateWorldsBlocking(this.SubscribedWorlds, MyWorkshop.MyWorkshopPathInfo.CreateWorldInfo());
            }

            public bool IsCompleted =>
                this.Task.IsComplete;

            public ParallelTasks.Task Task { get; private set; }
        }

        public class MyWorkshopItemComparer : IComparer<MyWorkshopItem>
        {
            private Func<MyWorkshopItem, MyWorkshopItem, int> comparator;

            public MyWorkshopItemComparer(Func<MyWorkshopItem, MyWorkshopItem, int> comp)
            {
                this.comparator = comp;
            }

            public int Compare(MyWorkshopItem x, MyWorkshopItem y) => 
                ((this.comparator == null) ? 0 : this.comparator(x, y));
        }
    }
}

