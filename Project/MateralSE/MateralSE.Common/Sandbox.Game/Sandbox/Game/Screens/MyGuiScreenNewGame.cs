namespace Sandbox.Game.Screens
{
    using ParallelTasks;
    using Sandbox;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Localization;
    using VRage.Game.ObjectBuilders.Campaign;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenNewGame : MyGuiScreenBase
    {
        private MyGuiControlScreenSwitchPanel m_screenSwitchPanel;
        private MyGuiControlList m_campaignList;
        private MyGuiControlRadioButtonGroup m_campaignTypesGroup;
        private MyObjectBuilder_Campaign m_selectedCampaign;
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
        private MyGuiControlMultilineText m_descriptionMultilineText;
        private MyGuiControlPanel m_descriptionPanel;
        private MyGuiControlButton m_publishButton;
        private MyGuiControlRotatingWheel m_asyncLoadingWheel;
        private Task m_refreshTask;
        private float MARGIN_TOP;
        private float MARGIN_BOTTOM;
        private float MARGIN_LEFT_INFO;
        private float MARGIN_RIGHT;
        private float MARGIN_LEFT_LIST;

        public MyGuiScreenNewGame() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.878f, 0.97f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.MARGIN_TOP = 0.22f;
            this.MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            this.MARGIN_LEFT_INFO = 15f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void AddCampaignButton(MyObjectBuilder_Campaign campaign, bool isLocalMod = false, bool isWorkshopMod = false)
        {
            string name = campaign.Name;
            MyLocalizationContext context = MyLocalization.Static[campaign.Name];
            if (context != null)
            {
                StringBuilder builder = context["Name"];
                if (builder != null)
                {
                    name = builder.ToString();
                }
            }
            MyGuiControlContentButton button1 = new MyGuiControlContentButton(name, this.GetImagePath(campaign));
            button1.UserData = campaign;
            button1.IsLocalMod = isLocalMod;
            button1.IsWorkshopMod = isWorkshopMod;
            button1.Key = this.m_campaignTypesGroup.Count;
            MyGuiControlContentButton radioButton = button1;
            this.m_campaignTypesGroup.Add(radioButton);
            this.m_campaignList.Controls.Add(radioButton);
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
            parent1.Size = new Vector2(this.m_campaignList.Size.X, label.Size.Y);
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
            this.m_campaignList.Controls.Add(parent);
        }

        private void CampaignDoubleClick(MyGuiControlRadioButton obj)
        {
            this.StartSelectedWorld();
        }

        private void CampaignSelectionChanged(MyGuiControlRadioButtonGroup args)
        {
            MyGuiControlContentButton selectedButton = args.SelectedButton as MyGuiControlContentButton;
            if (selectedButton != null)
            {
                MyObjectBuilder_Campaign userData = selectedButton.UserData as MyObjectBuilder_Campaign;
                if (userData != null)
                {
                    string name = string.IsNullOrEmpty(userData.ModFolderPath) ? userData.Name : Path.Combine(userData.ModFolderPath, userData.Name);
                    MyCampaignManager.Static.ReloadMenuLocalization(name);
                    string str2 = null;
                    MyLocalizationContext context = null;
                    if (string.IsNullOrEmpty(userData.DescriptionLocalizationFile))
                    {
                        str2 = userData.Name;
                        context = MyLocalization.Static[str2];
                    }
                    else
                    {
                        Dictionary<string, string> pathToContextTranslator = MyLocalization.Static.PathToContextTranslator;
                        string key = string.IsNullOrEmpty(userData.ModFolderPath) ? Path.Combine(MyFileSystem.ContentPath, userData.DescriptionLocalizationFile) : Path.Combine(userData.ModFolderPath, userData.DescriptionLocalizationFile);
                        if (pathToContextTranslator.ContainsKey(key))
                        {
                            str2 = pathToContextTranslator[key];
                        }
                        if (!string.IsNullOrEmpty(str2))
                        {
                            context = MyLocalization.Static[str2];
                        }
                    }
                    if (context == null)
                    {
                        this.m_nameText.Text = userData.Name;
                        this.m_descriptionMultilineText.Text = new StringBuilder(userData.Description);
                    }
                    else
                    {
                        StringBuilder builder = context["Name"];
                        this.m_nameText.Text = (builder == null) ? "name" : builder.ToString();
                        this.m_descriptionMultilineText.Text = context["Description"];
                    }
                    if ((userData != null) && userData.IsMultiplayer)
                    {
                        this.m_onlineMode.Enabled = true;
                    }
                    else
                    {
                        this.m_onlineMode.Enabled = false;
                        this.m_onlineMode.SelectItemByIndex(0);
                    }
                    this.m_authorText.Text = userData.Author;
                    this.m_maxPlayersSlider.Enabled = this.m_onlineMode.Enabled && (this.m_onlineMode.GetSelectedIndex() > 0);
                    this.m_selectedCampaign = userData;
                }
            }
        }

        public override string GetFriendlyName() => 
            "New Game";

        private string GetImagePath(MyObjectBuilder_Campaign campaign)
        {
            string imagePath = campaign.ImagePath;
            if (string.IsNullOrEmpty(campaign.ImagePath))
            {
                return string.Empty;
            }
            if (!campaign.IsVanilla)
            {
                imagePath = (campaign.ModFolderPath != null) ? Path.Combine(campaign.ModFolderPath, campaign.ImagePath) : string.Empty;
                if (!MyFileSystem.FileExists(imagePath))
                {
                    imagePath = Path.Combine(MyFileSystem.ContentPath, campaign.ImagePath);
                }
            }
            return imagePath;
        }

        private void InitCampaignList()
        {
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(this.MARGIN_LEFT_LIST, this.MARGIN_TOP);
            this.m_campaignTypesGroup = new MyGuiControlRadioButtonGroup();
            this.m_campaignTypesGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.CampaignSelectionChanged);
            this.m_campaignTypesGroup.MouseDoubleClick += new Action<MyGuiControlRadioButton>(this.CampaignDoubleClick);
            MyGuiControlList list1 = new MyGuiControlList();
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            list1.Position = vector;
            list1.Size = new Vector2(MyGuiConstants.LISTBOX_WIDTH, (base.m_size.Value.Y - this.MARGIN_TOP) - 0.048f);
            this.m_campaignList = list1;
            this.Controls.Add(this.m_campaignList);
        }

        private unsafe void InitRightSide()
        {
            int num = 5;
            Vector2 topLeft = (-base.m_size.Value / 2f) + new Vector2(((this.MARGIN_LEFT_LIST + this.m_campaignList.Size.X) + this.MARGIN_LEFT_INFO) + 0.012f, this.MARGIN_TOP - 0.011f);
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
            MyGuiControlRating rating1 = new MyGuiControlRating(10, 10);
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
            color = null;
            sortOrder = null;
            this.m_publishButton = new MyGuiControlButton(new Vector2?(vector4 - new Vector2(vector5.X + 0.0245f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector5), color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.LoadScreenButtonPublish), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnPublishButtonOnClick), GuiSounds.MouseClick, 1f, sortOrder, false);
            this.m_publishButton.Visible = true;
            this.m_publishButton.Enabled = MyFakes.ENABLE_WORKSHOP_PUBLISH;
            this.m_descriptionPanel.Size = new Vector2(this.m_descriptionPanel.Size.X, this.m_descriptionPanel.Size.Y + MyGuiConstants.BACK_BUTTON_SIZE.Y);
            this.m_descriptionMultilineText.Size = new Vector2(this.m_descriptionMultilineText.Size.X, this.m_descriptionMultilineText.Size.Y + MyGuiConstants.BACK_BUTTON_SIZE.Y);
            this.Controls.Add(this.m_publishButton);
            this.Controls.Add(control);
            base.CloseButtonEnabled = true;
        }

        private void m_onlineMode_ItemSelected()
        {
            this.m_maxPlayersSlider.Enabled = this.m_onlineMode.Enabled && (this.m_onlineMode.GetSelectedIndex() > 0);
        }

        private void OnCancelButtonClick(MyGuiControlButton myGuiControlButton)
        {
            this.CloseScreen();
        }

        private void OnOkButtonClicked(MyGuiControlButton myGuiControlButton)
        {
            this.StartSelectedWorld();
        }

        private void OnPublishButtonOnClick(MyGuiControlButton myGuiControlButton)
        {
            if (this.m_selectedCampaign != null)
            {
                MyCampaignManager.Static.SwitchCampaign(this.m_selectedCampaign.Name, this.m_selectedCampaign.IsVanilla, this.m_selectedCampaign.PublishedFileId, this.m_selectedCampaign.ModFolderPath);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, new StringBuilder(string.Format(MyTexts.GetString(MyCommonTexts.MessageBoxTextDoYouWishToPublishCampaign), MySession.Platform, MySession.PlatformLinkAgreement)), MyTexts.Get(MyCommonTexts.MessageBoxCaptionDoYouWishToPublishCampaign), okButtonText, okButtonText, okButtonText, okButtonText, e => MyCampaignManager.Static.PublishActive(), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
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
            this.m_screenSwitchPanel = new MyGuiControlScreenSwitchPanel(this, MyTexts.Get(MyCommonTexts.NewGameScreen_Description));
            this.InitCampaignList();
            this.InitRightSide();
            this.RefreshCampaignList();
            this.m_refreshTask = MyCampaignManager.Static.RefreshModData();
            captionOffset = null;
            this.m_asyncLoadingWheel = new MyGuiControlRotatingWheel(new Vector2((base.m_size.Value.X / 2f) - 0.077f, (-base.m_size.Value.Y / 2f) + 0.108f), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, captionOffset, 1.5f);
            this.Controls.Add(this.m_asyncLoadingWheel);
        }

        private void RefreshCampaignList()
        {
            List<MyObjectBuilder_Campaign> source = MyCampaignManager.Static.Campaigns.ToList<MyObjectBuilder_Campaign>();
            List<MyObjectBuilder_Campaign> list2 = new List<MyObjectBuilder_Campaign>();
            List<MyObjectBuilder_Campaign> list3 = new List<MyObjectBuilder_Campaign>();
            List<MyObjectBuilder_Campaign> list4 = new List<MyObjectBuilder_Campaign>();
            List<MyObjectBuilder_Campaign> list5 = new List<MyObjectBuilder_Campaign>();
            MyObjectBuilder_Campaign item = source.FirstOrDefault<MyObjectBuilder_Campaign>(x => x.Name == "The First Jump");
            if (item != null)
            {
                source.Remove(item);
                source.Insert(0, item);
            }
            foreach (MyObjectBuilder_Campaign campaign2 in source)
            {
                if (campaign2.IsVanilla && !campaign2.IsDebug)
                {
                    list2.Add(campaign2);
                    continue;
                }
                if (campaign2.IsLocalMod)
                {
                    list3.Add(campaign2);
                }
                else if (!campaign2.IsVanilla || !campaign2.IsDebug)
                {
                    list4.Add(campaign2);
                }
                else
                {
                    list5.Add(campaign2);
                }
            }
            this.m_campaignList.Controls.Clear();
            this.m_campaignTypesGroup.Clear();
            foreach (MyObjectBuilder_Campaign campaign3 in list2)
            {
                this.AddCampaignButton(campaign3, false, false);
            }
            if (MySandboxGame.Config.ExperimentalMode)
            {
                if (list4.Count > 0)
                {
                    this.AddSeparator(MyTexts.Get(MyCommonTexts.Workshop).ToString());
                }
                foreach (MyObjectBuilder_Campaign campaign4 in list4)
                {
                    this.AddCampaignButton(campaign4, false, true);
                }
            }
            if (MySandboxGame.Config.ExperimentalMode)
            {
                if (list3.Count > 0)
                {
                    this.AddSeparator(MyTexts.Get(MyCommonTexts.Local).ToString());
                }
                foreach (MyObjectBuilder_Campaign campaign5 in list3)
                {
                    this.AddCampaignButton(campaign5, true, false);
                }
            }
            if (this.m_campaignList.Controls.Count > 0)
            {
                this.m_campaignTypesGroup.SelectByIndex(0);
            }
        }

        public override bool RegisterClicks() => 
            true;

        private AsyncCampaingLoader RunRefreshAsync() => 
            new AsyncCampaingLoader();

        private void StartSelectedWorld()
        {
            if (this.m_selectedCampaign != null)
            {
                MyCampaignManager.Static.SwitchCampaign(this.m_selectedCampaign.Name, this.m_selectedCampaign.IsVanilla, this.m_selectedCampaign.PublishedFileId, this.m_selectedCampaign.ModFolderPath);
                MySpaceAnalytics.Instance.ReportScenarioStart(this.m_selectedCampaign.Name);
                MyOnlineModeEnum selectedKey = (MyOnlineModeEnum) ((int) this.m_onlineMode.GetSelectedKey());
                MyCampaignManager.Static.RunNewCampaign(this.m_selectedCampaign.Name, selectedKey, (int) this.m_maxPlayersSlider.Value);
            }
        }

        public override bool Update(bool hasFocus)
        {
            this.m_publishButton.Visible = (this.m_selectedCampaign != null) && this.m_selectedCampaign.IsLocalMod;
            if (!this.m_refreshTask.valid || !this.m_refreshTask.IsComplete)
            {
                MyGuiControlButton controlByName = (MyGuiControlButton) this.m_screenSwitchPanel.Controls.GetControlByName("CampaignButton");
                base.FocusedControl = controlByName;
            }
            else
            {
                this.m_refreshTask.valid = false;
                this.m_asyncLoadingWheel.Visible = false;
                this.RefreshCampaignList();
            }
            return base.Update(hasFocus);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenNewGame.<>c <>9 = new MyGuiScreenNewGame.<>c();
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__34_0;
            public static Func<MyObjectBuilder_Campaign, bool> <>9__39_0;

            internal void <OnPublishButtonOnClick>b__34_0(MyGuiScreenMessageBox.ResultEnum e)
            {
                MyCampaignManager.Static.PublishActive();
            }

            internal bool <RefreshCampaignList>b__39_0(MyObjectBuilder_Campaign x) => 
                (x.Name == "The First Jump");
        }

        private class AsyncCampaingLoader : IMyAsyncResult
        {
            public AsyncCampaingLoader()
            {
                this.Task = MyCampaignManager.Static.RefreshModData();
            }

            public bool IsCompleted =>
                (this.Task.IsComplete || !this.Task.valid);

            public ParallelTasks.Task Task { get; private set; }
        }
    }
}

