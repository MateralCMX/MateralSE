namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
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
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenWorldSettings : MyGuiScreenBase
    {
        public static MyGuiScreenWorldSettings Static;
        internal MyGuiScreenAdvancedWorldSettings Advanced;
        internal MyGuiScreenWorldGeneratorSettings WorldGenerator;
        internal MyGuiScreenMods ModsScreen;
        protected bool m_isNewGame;
        private string m_sessionPath;
        protected MyObjectBuilder_SessionSettings m_settings;
        private MyGuiScreenWorldSettings m_parent;
        private List<MyObjectBuilder_Checkpoint.ModItem> m_mods;
        private MyObjectBuilder_Checkpoint m_checkpoint;
        private MyGuiControlTextbox m_nameTextbox;
        private MyGuiControlTextbox m_descriptionTextbox;
        private MyGuiControlCombobox m_onlineMode;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlButton m_survivalModeButton;
        private MyGuiControlButton m_creativeModeButton;
        private MyGuiControlButton m_worldGeneratorButton;
        private MyGuiControlSlider m_maxPlayersSlider;
        private MyGuiControlCheckbox m_autoSave;
        private MyGuiControlLabel m_maxPlayersLabel;
        private MyGuiControlLabel m_autoSaveLabel;
        private MyGuiControlList m_scenarioTypesList;
        private MyGuiControlRadioButtonGroup m_scenarioTypesGroup;
        private MyGuiControlRotatingWheel m_asyncLoadingWheel;
        private IMyAsyncResult m_loadingTask;
        private float MARGIN_TOP;
        private float MARGIN_BOTTOM;
        private float MARGIN_LEFT_INFO;
        private float MARGIN_RIGHT;
        private float MARGIN_LEFT_LIST;
        private bool m_descriptionChanged;

        public MyGuiScreenWorldSettings() : this(null, null)
        {
        }

        public MyGuiScreenWorldSettings(MyObjectBuilder_Checkpoint checkpoint, string path) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(CalcSize(checkpoint)), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.MARGIN_TOP = 0.22f;
            this.MARGIN_BOTTOM = 50f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y;
            this.MARGIN_LEFT_INFO = 29.5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_RIGHT = 81f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            this.MARGIN_LEFT_LIST = 90f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            MySandboxGame.Log.WriteLine("MyGuiScreenWorldSettings.ctor START");
            base.EnabledBackgroundFade = true;
            Static = this;
            this.m_checkpoint = checkpoint;
            if ((checkpoint == null) || (checkpoint.Mods == null))
            {
                this.m_mods = new List<MyObjectBuilder_Checkpoint.ModItem>();
            }
            else
            {
                this.m_mods = checkpoint.Mods;
            }
            this.m_sessionPath = path;
            this.m_isNewGame = ReferenceEquals(checkpoint, null);
            this.RecreateControls(true);
            MySandboxGame.Log.WriteLine("MyGuiScreenWorldSettings.ctor END");
        }

        private void Advanced_OnOkButtonClicked()
        {
            this.Advanced.GetSettings(this.m_settings);
            this.SetSettingsToControls();
        }

        protected virtual unsafe void BuildControls()
        {
            MyGuiControlButton button3;
            VRageMath.Vector4? nullable;
            Vector2? nullable2;
            if (this.m_isNewGame)
            {
                nullable = null;
                nullable2 = null;
                base.AddCaption(MyCommonTexts.ScreenMenuButtonCampaign, nullable, nullable2, 0.8f);
            }
            else
            {
                nullable = null;
                nullable2 = null;
                base.AddCaption(MyCommonTexts.ScreenCaptionEditSettings, nullable, nullable2, 0.8f);
            }
            int num = 0;
            if (this.m_isNewGame)
            {
                this.InitCampaignList();
            }
            Vector2 vector6 = new Vector2(0f, 0.052f);
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(this.m_isNewGame ? ((this.MARGIN_LEFT_LIST + this.m_scenarioTypesList.Size.X) + this.MARGIN_LEFT_INFO) : this.MARGIN_LEFT_LIST, this.m_isNewGame ? (this.MARGIN_TOP + 0.015f) : (this.MARGIN_TOP - 0.105f));
            Vector2 vector5 = (base.m_size.Value / 2f) - vector;
            float* singlePtr1 = (float*) ref vector5.X;
            singlePtr1[0] -= this.MARGIN_RIGHT + 0.005f;
            float* singlePtr2 = (float*) ref vector5.Y;
            singlePtr2[0] -= this.MARGIN_BOTTOM;
            Vector2 vector3 = vector5 * (this.m_isNewGame ? 0.339f : 0.329f);
            Vector2 vector2 = vector + new Vector2(vector3.X, 0f);
            MyGuiControlLabel control = this.MakeLabel(MyCommonTexts.Name);
            MyGuiControlLabel label2 = this.MakeLabel(MyCommonTexts.Description);
            MyGuiControlLabel label3 = this.MakeLabel(MyCommonTexts.WorldSettings_GameMode);
            MyGuiControlLabel label4 = this.MakeLabel(MyCommonTexts.WorldSettings_OnlineMode);
            this.m_maxPlayersLabel = this.MakeLabel(MyCommonTexts.MaxPlayers);
            this.m_autoSaveLabel = this.MakeLabel(MyCommonTexts.WorldSettings_AutoSave);
            nullable2 = null;
            nullable = null;
            this.m_nameTextbox = new MyGuiControlTextbox(nullable2, null, 0x80, nullable, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            nullable2 = null;
            nullable = null;
            this.m_descriptionTextbox = new MyGuiControlTextbox(nullable2, null, 0x1f3f, nullable, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_descriptionTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.OnDescriptionChanged);
            nullable2 = null;
            nullable = null;
            nullable2 = null;
            nullable2 = null;
            nullable = null;
            this.m_onlineMode = new MyGuiControlCombobox(nullable2, new Vector2((vector5 - vector3).X, 0.04f), nullable, nullable2, 10, nullable2, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, nullable);
            float x = this.m_onlineMode.Size.X;
            float? defaultValue = null;
            nullable = null;
            this.m_maxPlayersSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 2f, (float) MyMultiplayerLobby.MAX_PLAYERS, x, defaultValue, nullable, new StringBuilder("{0}").ToString(), 0, 0.8f, 0.028f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, true);
            nullable2 = null;
            nullable = null;
            this.m_autoSave = new MyGuiControlCheckbox(nullable2, nullable, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_autoSave.SetToolTip(new StringBuilder().AppendFormat(MyCommonTexts.ToolTipWorldSettingsAutoSave, 5).ToString());
            nullable2 = null;
            nullable2 = null;
            nullable = null;
            int? buttonIndex = null;
            this.m_creativeModeButton = new MyGuiControlButton(nullable2, MyGuiControlButtonStyleEnum.Small, nullable2, nullable, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.WorldSettings_GameModeCreative), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCreativeClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_creativeModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeCreative);
            nullable2 = null;
            nullable2 = null;
            nullable = null;
            buttonIndex = null;
            this.m_survivalModeButton = new MyGuiControlButton(nullable2, MyGuiControlButtonStyleEnum.Small, nullable2, nullable, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.WorldSettings_GameModeSurvival), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnSurvivalClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_survivalModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeSurvival);
            this.m_onlineMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnOnlineModeSelect);
            buttonIndex = null;
            MyStringId? toolTip = null;
            this.m_onlineMode.AddItem(0L, MyCommonTexts.WorldSettings_OnlineModeOffline, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_onlineMode.AddItem(3L, MyCommonTexts.WorldSettings_OnlineModePrivate, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_onlineMode.AddItem(2L, MyCommonTexts.WorldSettings_OnlineModeFriends, buttonIndex, toolTip);
            buttonIndex = null;
            toolTip = null;
            this.m_onlineMode.AddItem(1L, MyCommonTexts.WorldSettings_OnlineModePublic, buttonIndex, toolTip);
            if (this.m_isNewGame)
            {
                if (MyDefinitionManager.Static.GetScenarioDefinitions().Count == 0)
                {
                    MyDefinitionManager.Static.LoadScenarios();
                }
                this.m_scenarioTypesGroup = new MyGuiControlRadioButtonGroup();
                this.m_scenarioTypesGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.scenario_SelectedChanged);
                this.m_scenarioTypesGroup.MouseDoubleClick += new Action<MyGuiControlRadioButton>(this.OnOkButtonClick);
                nullable2 = null;
                this.m_asyncLoadingWheel = new MyGuiControlRotatingWheel(new Vector2((base.m_size.Value.X / 2f) - 0.077f, (-base.m_size.Value.Y / 2f) + 0.108f), new VRageMath.Vector4?(MyGuiConstants.ROTATING_WHEEL_COLOR), 0.2f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @"Textures\GUI\screens\screen_loading_wheel.dds", true, true, nullable2, 1.5f);
                this.m_loadingTask = this.StartLoadingWorldInfos();
            }
            this.m_nameTextbox.SetToolTip(string.Format(MyTexts.GetString(MyCommonTexts.ToolTipWorldSettingsName), 5, 0x80));
            this.m_nameTextbox.FocusChanged += new Action<MyGuiControlBase, bool>(this.NameFocusChanged);
            this.m_descriptionTextbox.SetToolTip(MyTexts.GetString(MyCommonTexts.ToolTipWorldSettingsDescription));
            this.m_descriptionTextbox.FocusChanged += new Action<MyGuiControlBase, bool>(this.DescriptionFocusChanged);
            this.m_onlineMode.SetToolTip(string.Format(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsOnlineMode), MySession.Platform));
            this.m_onlineMode.HideToolTip();
            this.m_maxPlayersSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxPlayer));
            nullable2 = null;
            nullable2 = null;
            nullable = null;
            buttonIndex = null;
            this.m_worldGeneratorButton = new MyGuiControlButton(nullable2, MyGuiControlButtonStyleEnum.Default, nullable2, nullable, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.WorldSettings_WorldGenerator), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnWorldGeneratorClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.Controls.Add(control);
            this.Controls.Add(this.m_nameTextbox);
            this.Controls.Add(label2);
            this.Controls.Add(this.m_descriptionTextbox);
            this.Controls.Add(label3);
            this.Controls.Add(this.m_creativeModeButton);
            this.Controls.Add(label4);
            this.Controls.Add(this.m_onlineMode);
            this.Controls.Add(this.m_maxPlayersLabel);
            this.Controls.Add(this.m_maxPlayersSlider);
            this.Controls.Add(this.m_autoSaveLabel);
            this.Controls.Add(this.m_autoSave);
            Vector2 vector7 = base.m_size.Value / 2f;
            float* singlePtr3 = (float*) ref vector7.X;
            singlePtr3[0] -= this.MARGIN_RIGHT + 0.004f;
            float* singlePtr4 = (float*) ref vector7.Y;
            singlePtr4[0] -= this.MARGIN_BOTTOM + 0.004f;
            Vector2 vector8 = MyGuiConstants.BACK_BUTTON_SIZE;
            Vector2 vector1 = MyGuiConstants.GENERIC_BUTTON_SPACING;
            Vector2 vector9 = MyGuiConstants.GENERIC_BUTTON_SPACING;
            MyGuiControlButton button = null;
            MyGuiControlButton button2 = null;
            nullable = null;
            StringBuilder text = MyTexts.Get(MySpaceTexts.WorldSettings_Advanced);
            buttonIndex = null;
            button = new MyGuiControlButton(new Vector2?(vector7 - new Vector2(vector8.X + 0.0245f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector8), nullable, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MySpaceTexts.ToolTipNewGameCustomGame_Advanced), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnAdvancedClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            nullable = null;
            text = MyTexts.Get(MyCommonTexts.WorldSettings_Mods);
            buttonIndex = null;
            button2 = new MyGuiControlButton(new Vector2?(vector7 - new Vector2((vector8.X * 2f) + 0.049f, 0f)), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector8), nullable, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, MyTexts.GetString(MySpaceTexts.ToolTipNewGameCustomGame_Mods), text, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnModsClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
            if (MyFakes.ENABLE_WORKSHOP_MODS)
            {
                this.Controls.Add(button2);
            }
            this.Controls.Add(button);
            button2.SetEnabledByExperimental();
            foreach (MyGuiControlBase base2 in this.Controls)
            {
                base2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                if (base2 is MyGuiControlLabel)
                {
                    base2.Position = vector + (vector6 * num);
                    continue;
                }
                num++;
                base2.Position = vector2 + (vector6 * num);
            }
            if (this.m_isNewGame)
            {
                this.Controls.Add(this.m_scenarioTypesList);
            }
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            if (this.m_isNewGame)
            {
                nullable = null;
                list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.38f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.625f, 0f, nullable);
            }
            else
            {
                nullable = null;
                list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.835f, 0f, nullable);
                nullable = null;
                list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.835f, 0f, nullable);
            }
            this.Controls.Add(list);
            if (this.m_isNewGame)
            {
                nullable = null;
                buttonIndex = null;
                button3 = new MyGuiControlButton(new Vector2?(vector7), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector8), nullable, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Start), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                button3.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewGame_Start));
            }
            else
            {
                nullable = null;
                buttonIndex = null;
                button3 = new MyGuiControlButton(new Vector2?(vector7), MyGuiControlButtonStyleEnum.Default, new Vector2?(vector8), nullable, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkButtonClick), GuiSounds.MouseClick, 1f, buttonIndex, false);
                button3.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            }
            this.Controls.Add(button3);
            this.Controls.Add(this.m_survivalModeButton);
            this.m_survivalModeButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_creativeModeButton.PositionX += 0.0025f;
            this.m_creativeModeButton.PositionY += 0.005f;
            this.m_survivalModeButton.Position = this.m_creativeModeButton.Position + new Vector2(this.m_onlineMode.Size.X + 0.0005f, 0f);
            this.m_nameTextbox.Size = this.m_onlineMode.Size;
            this.m_descriptionTextbox.Size = this.m_nameTextbox.Size;
            this.m_maxPlayersSlider.PositionX = this.m_nameTextbox.PositionX - 0.001f;
            this.m_autoSave.PositionX = this.m_maxPlayersSlider.PositionX;
            if (button2 != null)
            {
                button2.PositionX = this.m_maxPlayersSlider.PositionX + 0.003f;
                button2.PositionY += 0.007f;
            }
            if (button != null)
            {
                button.PositionX += (0.0045f + button2.Size.X) + 0.01f;
                button.PositionY = button2.Position.Y;
            }
            if (this.m_isNewGame)
            {
                this.Controls.Add(this.m_asyncLoadingWheel);
            }
            base.CloseButtonEnabled = true;
        }

        public static Vector2 CalcSize(MyObjectBuilder_Checkpoint checkpoint) => 
            new Vector2((checkpoint == null) ? 0.878f : 0.6535714f, (checkpoint == null) ? 0.97f : 0.9398855f);

        private void ChangeWorldSettings()
        {
            this.m_checkpoint.SessionName = this.m_nameTextbox.Text;
            if (this.DescriptionChanged())
            {
                this.m_checkpoint.Description = this.m_descriptionTextbox.Text;
                this.m_descriptionChanged = false;
            }
            this.GetSettingsFromControls();
            this.m_checkpoint.Settings = this.m_settings;
            this.m_checkpoint.Mods = this.m_mods;
            MyLocalCache.SaveCheckpoint(this.m_checkpoint, this.m_sessionPath);
            if (((MySession.Static != null) && (MySession.Static.Name == this.m_checkpoint.SessionName)) && (this.m_sessionPath == MySession.Static.CurrentPath))
            {
                MySession @static = MySession.Static;
                @static.Password = this.GetPassword();
                @static.Description = this.GetDescription();
                @static.Settings = this.m_checkpoint.Settings;
                @static.Mods = this.m_checkpoint.Mods;
            }
            this.CloseScreen();
        }

        private void CheckButton(MyGuiControlButton active, params MyGuiControlButton[] allButtons)
        {
            foreach (MyGuiControlButton button in allButtons)
            {
                if (ReferenceEquals(button, active) && !button.Checked)
                {
                    button.Checked = true;
                }
                else if (!ReferenceEquals(button, active) && button.Checked)
                {
                    button.Checked = false;
                }
            }
        }

        private void CheckButton(float value, params MyGuiControlButton[] allButtons)
        {
            bool flag = false;
            foreach (MyGuiControlButton button in allButtons)
            {
                if (button.UserData is float)
                {
                    if ((((float) button.UserData) == value) && !button.Checked)
                    {
                        flag = true;
                        button.Checked = true;
                    }
                    else if ((((float) button.UserData) != value) && button.Checked)
                    {
                        button.Checked = false;
                    }
                }
            }
            if (!flag)
            {
                allButtons[0].Checked = true;
            }
        }

        private void CheckDx11AndStart()
        {
            if (MySandboxGame.IsDirectX11)
            {
                this.StartNewSandbox();
            }
            else
            {
                MyStringId? nullable;
                Vector2? nullable2;
                if (!MyDirectXHelper.IsDx11Supported())
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartNoDx9SelectDifferent), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                else
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionError);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.QuickstartDX11SwitchQuestion), messageCaption, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnSwitchAnswer), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }
        }

        public override bool CloseScreen()
        {
            if (this.WorldGenerator != null)
            {
                this.WorldGenerator.CloseScreen();
            }
            this.WorldGenerator = null;
            if (this.Advanced != null)
            {
                this.Advanced.CloseScreen();
            }
            this.Advanced = null;
            if (this.ModsScreen != null)
            {
                this.ModsScreen.CloseScreen();
            }
            this.ModsScreen = null;
            Static = null;
            return base.CloseScreen();
        }

        protected virtual MyObjectBuilder_SessionSettings CopySettings(MyObjectBuilder_SessionSettings source) => 
            (source.Clone() as MyObjectBuilder_SessionSettings);

        private bool DescriptionChanged() => 
            this.m_descriptionChanged;

        private void DescriptionFocusChanged(MyGuiControlBase obj, bool focused)
        {
            if (focused && !this.m_descriptionTextbox.IsImeActive)
            {
                this.m_descriptionTextbox.SelectAll();
                this.m_descriptionTextbox.MoveCarriageToEnd();
            }
        }

        protected virtual MyObjectBuilder_SessionSettings GetDefaultSettings() => 
            MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SessionSettings>();

        private string GetDescription() => 
            ((this.m_checkpoint == null) ? this.m_descriptionTextbox.Text : this.m_checkpoint.Description);

        public override string GetFriendlyName() => 
            "MyGuiScreenWorldSettings";

        private MyGameModeEnum GetGameMode() => 
            (this.m_survivalModeButton.Checked ? MyGameModeEnum.Survival : MyGameModeEnum.Creative);

        private string GetPassword()
        {
            if ((this.Advanced == null) || !this.Advanced.IsConfirmed)
            {
                return ((this.m_checkpoint == null) ? "" : this.m_checkpoint.Password);
            }
            return this.Advanced.Password;
        }

        protected virtual void GetSettingsFromControls()
        {
            this.m_settings.OnlineMode = (MyOnlineModeEnum) ((int) this.m_onlineMode.GetSelectedKey());
            if (this.m_checkpoint != null)
            {
                this.m_checkpoint.PreviousEnvironmentHostility = new MyEnvironmentHostilityEnum?(this.m_settings.EnvironmentHostility);
            }
            this.m_settings.MaxPlayers = (short) this.m_maxPlayersSlider.Value;
            this.m_settings.GameMode = this.GetGameMode();
            this.m_settings.ScenarioEditMode = false;
            this.m_settings.AutoSaveInMinutes = this.m_autoSave.IsChecked ? 5 : 0;
        }

        private void InitCampaignList()
        {
            if (MyDefinitionManager.Static.GetScenarioDefinitions().Count == 0)
            {
                MyDefinitionManager.Static.LoadScenarios();
            }
            Vector2 vector = (-base.m_size.Value / 2f) + new Vector2(this.MARGIN_LEFT_LIST, this.MARGIN_TOP);
            this.m_scenarioTypesGroup = new MyGuiControlRadioButtonGroup();
            this.m_scenarioTypesGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.scenario_SelectedChanged);
            this.m_scenarioTypesGroup.MouseDoubleClick += _ => this.OnOkButtonClick(null);
            MyGuiControlList list1 = new MyGuiControlList();
            list1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            list1.Position = vector;
            list1.Size = new Vector2(MyGuiConstants.LISTBOX_WIDTH, (base.m_size.Value.Y - this.MARGIN_TOP) - 0.048f);
            this.m_scenarioTypesList = list1;
        }

        private void LoadValues()
        {
            this.m_nameTextbox.Text = this.m_checkpoint.SessionName ?? "";
            this.m_descriptionTextbox.TextChanged -= new Action<MyGuiControlTextbox>(this.OnDescriptionChanged);
            this.m_descriptionTextbox.Text = string.IsNullOrEmpty(this.m_checkpoint.Description) ? "" : MyTexts.SubstituteTexts(this.m_checkpoint.Description, null);
            this.m_descriptionTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.OnDescriptionChanged);
            this.m_descriptionChanged = false;
            this.m_settings = this.CopySettings(this.m_checkpoint.Settings);
            this.m_mods = this.m_checkpoint.Mods;
            this.SetSettingsToControls();
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void NameFocusChanged(MyGuiControlBase obj, bool focused)
        {
            if (focused && !this.m_nameTextbox.IsImeActive)
            {
                this.m_nameTextbox.SelectAll();
                this.m_nameTextbox.MoveCarriageToEnd();
            }
        }

        private void OnAdvancedClick(object sender)
        {
            this.Advanced = new MyGuiScreenAdvancedWorldSettings(this);
            this.Advanced.UpdateSurvivalState(this.GetGameMode() == MyGameModeEnum.Survival);
            this.Advanced.OnOkButtonClicked += new Action(this.Advanced_OnOkButtonClicked);
            MyGuiSandbox.AddScreen(this.Advanced);
        }

        private void OnCancelButtonClick(object sender)
        {
            this.CloseScreen();
        }

        private void OnCreativeClick(object sender)
        {
            this.UpdateSurvivalState(false);
            this.Settings.EnableCopyPaste = true;
        }

        private void OnDescriptionChanged(MyGuiControlTextbox obj)
        {
            this.m_descriptionChanged = true;
        }

        private void OnLoadingFinished(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
        {
            MyLoadListResult result2 = (MyLoadListResult) result;
            this.m_scenarioTypesGroup.Clear();
            this.m_scenarioTypesList.Clear();
            if (result2.AvailableSaves.Count != 0)
            {
                result2.AvailableSaves.Sort((a, b) => a.Item2.SessionName.CompareTo(b.Item2.SessionName));
            }
            foreach (Tuple<string, MyWorldInfo> tuple in result2.AvailableSaves)
            {
                if (MySandboxGame.Config.ExperimentalMode || !tuple.Item2.IsExperimental)
                {
                    MyGuiControlContentButton button1 = new MyGuiControlContentButton(tuple.Item2.SessionName, Path.Combine(tuple.Item1, "thumb.jpg"));
                    button1.UserData = tuple.Item1;
                    button1.Key = this.m_scenarioTypesGroup.Count;
                    MyGuiControlContentButton radioButton = button1;
                    this.m_scenarioTypesGroup.Add(radioButton);
                    this.m_scenarioTypesList.Controls.Add(radioButton);
                }
            }
            if (this.m_scenarioTypesList.Controls.Count > 0)
            {
                this.m_scenarioTypesGroup.SelectByIndex(0);
            }
            else
            {
                this.SetDefaultValues();
            }
        }

        private void OnModsClick(object sender)
        {
            MyGuiSandbox.AddScreen(new MyGuiScreenMods(this.m_mods));
        }

        private void OnOkButtonClick(object sender)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            bool flag = this.m_nameTextbox.Text.ToString().Replace(':', '-').IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;
            if ((flag || (this.m_nameTextbox.Text.Length < 5)) || (this.m_nameTextbox.Text.Length > 0x80))
            {
                MyStringId id = !flag ? ((this.m_nameTextbox.Text.Length >= 5) ? MyCommonTexts.ErrorNameTooLong : MyCommonTexts.ErrorNameTooShort) : MyCommonTexts.ErrorNameInvalid;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(id), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
            }
            else if (this.m_descriptionTextbox.Text.Length <= 0x1f3f)
            {
                if (this.m_isNewGame)
                {
                    this.CheckDx11AndStart();
                }
                else
                {
                    this.OnOkButtonClickQuestions(0);
                }
            }
            else
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.ErrorDescriptionTooLong), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                screen.SkipTransition = true;
                screen.InstantClose = false;
                MyGuiSandbox.AddScreen(screen);
            }
        }

        private void OnOkButtonClickAnswer(MyGuiScreenMessageBox.ResultEnum answer, int skipQuestions)
        {
            if (answer == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.OnOkButtonClickQuestions(skipQuestions);
            }
        }

        private void OnOkButtonClickQuestions(int skipQuestions)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            if (skipQuestions <= 0)
            {
                bool flag = (this.m_checkpoint.Settings.GameMode == MyGameModeEnum.Survival) && (this.GetGameMode() == MyGameModeEnum.Creative);
                if (((this.m_checkpoint.Settings.GameMode == MyGameModeEnum.Creative) && (this.GetGameMode() == MyGameModeEnum.Survival)) || (!flag && (this.m_checkpoint.Settings.InventorySizeMultiplier > this.m_settings.InventorySizeMultiplier)))
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.HarvestingWarningInventoryMightBeTruncatedAreYouSure), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, x => this.OnOkButtonClickAnswer(x, 1), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                    screen.SkipTransition = true;
                    screen.InstantClose = false;
                    MyGuiSandbox.AddScreen(screen);
                    return;
                }
            }
            if (skipQuestions <= 1)
            {
                int num1;
                if ((this.m_checkpoint.Settings.WorldSizeKm == 0) || (this.m_checkpoint.Settings.WorldSizeKm > this.m_settings.WorldSizeKm))
                {
                    num1 = (int) (this.m_settings.WorldSizeKm != 0);
                }
                else
                {
                    num1 = 0;
                }
                if (num1 != 0)
                {
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiScreenMessageBox screen = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.WorldSettings_WarningChangingWorldSize), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), nullable, nullable, nullable, nullable, x => this.OnOkButtonClickAnswer(x, 2), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2);
                    screen.SkipTransition = true;
                    screen.InstantClose = false;
                    MyGuiSandbox.AddScreen(screen);
                    return;
                }
            }
            this.ChangeWorldSettings();
        }

        private void OnOnlineModeSelect()
        {
            bool flag = this.m_onlineMode.GetSelectedKey() == 0L;
            this.m_maxPlayersSlider.Enabled = !flag;
            this.m_maxPlayersLabel.Enabled = !flag;
            if (!flag && !MySandboxGame.Config.ExperimentalMode)
            {
                this.m_settings.TotalPCU = Math.Min(this.m_settings.TotalPCU, 0xc350);
            }
        }

        private void OnSurvivalClick(object sender)
        {
            this.UpdateSurvivalState(true);
            this.Settings.EnableCopyPaste = false;
        }

        private void OnSwitchAnswer(MyGuiScreenMessageBox.ResultEnum result)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            if (result != MyGuiScreenMessageBox.ResultEnum.YES)
            {
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartSelectDifferent), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
            else
            {
                MySandboxGame.Config.GraphicsRenderer = MySandboxGame.DirectX11RendererKey;
                MySandboxGame.Config.Save();
                MyGuiSandbox.BackToMainMenu();
                nullable = null;
                nullable = null;
                nullable = null;
                nullable = null;
                nullable2 = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.QuickstartDX11PleaseRestartGame), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
            }
        }

        private void OnWorldGeneratorClick(object sender)
        {
            this.WorldGenerator = new MyGuiScreenWorldGeneratorSettings(this);
            this.WorldGenerator.OnOkButtonClicked += new Action(this.WorldGenerator_OnOkButtonClicked);
            MyGuiSandbox.AddScreen(this.WorldGenerator);
        }

        private Vector2 ProjectX(Vector2 vec) => 
            new Vector2(vec.X, 0f);

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.BuildControls();
            if (this.m_isNewGame)
            {
                this.SetDefaultValues();
                MyGuiControlScreenSwitchPanel panel1 = new MyGuiControlScreenSwitchPanel(this, MyTexts.Get(MyCommonTexts.WorldSettingsScreen_Description));
            }
            else
            {
                this.LoadValues();
                this.m_nameTextbox.MoveCarriageToEnd();
                this.m_descriptionTextbox.MoveCarriageToEnd();
            }
        }

        public override bool RegisterClicks() => 
            true;

        private void scenario_SelectedChanged(MyGuiControlRadioButtonGroup group)
        {
            ulong num;
            this.SetDefaultName();
            if (MyFakes.ENABLE_PLANETS)
            {
                this.m_worldGeneratorButton.Enabled = true;
                if (this.m_worldGeneratorButton.Enabled && (this.WorldGenerator != null))
                {
                    this.WorldGenerator.GetSettings(this.m_settings);
                }
            }
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(group.SelectedButton.UserData as string, out num);
            if (checkpoint != null)
            {
                this.m_settings = this.CopySettings(checkpoint.Settings);
                this.SetSettingsToControls();
            }
        }

        private void SetDefaultName()
        {
            if (this.m_scenarioTypesGroup.SelectedButton != null)
            {
                this.m_nameTextbox.Text = ((MyGuiControlContentButton) this.m_scenarioTypesGroup.SelectedButton).Title.ToString() + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                this.m_descriptionTextbox.Text = string.Empty;
            }
        }

        private void SetDefaultValues()
        {
            this.m_settings = this.GetDefaultSettings();
            this.m_settings.EnableToolShake = true;
            this.m_settings.EnableSunRotation = MyPerGameSettings.Game == GameEnum.SE_GAME;
            this.m_settings.VoxelGeneratorVersion = 4;
            this.m_settings.EnableOxygen = true;
            this.m_settings.CargoShipsEnabled = true;
            this.m_mods = new List<MyObjectBuilder_Checkpoint.ModItem>();
            this.SetSettingsToControls();
            this.SetDefaultName();
        }

        protected virtual void SetSettingsToControls()
        {
            this.m_onlineMode.SelectItemByKey((long) this.m_settings.OnlineMode, true);
            this.m_maxPlayersSlider.Value = this.m_settings.MaxPlayers;
            this.UpdateSurvivalState(this.m_settings.GameMode == MyGameModeEnum.Survival);
            this.m_autoSave.IsChecked = this.m_settings.AutoSaveInMinutes != 0;
        }

        private void SetupWorldGeneratorSettings(MyObjectBuilder_Checkpoint checkpoint)
        {
        }

        private IMyAsyncResult StartLoadingWorldInfos()
        {
            string str = "CustomWorlds";
            string customPath = Path.Combine(MyFileSystem.ContentPath, str);
            return (!this.m_isNewGame ? ((IMyAsyncResult) new MyLoadWorldInfoListResult(customPath)) : ((IMyAsyncResult) new MyNewCustomWorldInfoListResult(customPath)));
        }

        private void StartNewSandbox()
        {
            ulong num;
            MyLog.Default.WriteLine("StartNewSandbox - Start");
            this.GetSettingsFromControls();
            string userData = this.m_scenarioTypesGroup.SelectedButton.UserData as string;
            MyObjectBuilder_Checkpoint checkpoint = MyLocalCache.LoadCheckpoint(userData, out num);
            if (checkpoint != null)
            {
                this.GetSettingsFromControls();
                checkpoint.Settings = this.m_settings;
                checkpoint.SessionName = this.m_nameTextbox.Text;
                checkpoint.Password = this.GetPassword();
                checkpoint.Description = this.GetDescription();
                checkpoint.Mods = this.m_mods;
                this.SetupWorldGeneratorSettings(checkpoint);
                MySessionLoader.LoadSingleplayerSession(checkpoint, userData, num, () => MyAsyncSaving.Start(null, Path.Combine(MyFileSystem.SavesPath, checkpoint.SessionName.Replace(':', '-')), false));
            }
        }

        public override bool Update(bool hasFocus)
        {
            if ((this.m_loadingTask != null) && this.m_loadingTask.IsCompleted)
            {
                this.OnLoadingFinished(this.m_loadingTask, null);
                this.m_loadingTask = null;
                this.m_asyncLoadingWheel.Visible = false;
            }
            return base.Update(hasFocus);
        }

        private void UpdateSurvivalState(bool survivalEnabled)
        {
            this.m_creativeModeButton.Checked = !survivalEnabled;
            this.m_survivalModeButton.Checked = survivalEnabled;
        }

        private void WorldGenerator_OnOkButtonClicked()
        {
            this.WorldGenerator.GetSettings(this.m_settings);
            this.SetSettingsToControls();
        }

        public MyObjectBuilder_SessionSettings Settings
        {
            get
            {
                this.GetSettingsFromControls();
                return this.m_settings;
            }
        }

        public MyObjectBuilder_Checkpoint Checkpoint =>
            this.m_checkpoint;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenWorldSettings.<>c <>9 = new MyGuiScreenWorldSettings.<>c();
            public static Comparison<Tuple<string, MyWorldInfo>> <>9__84_0;

            internal int <OnLoadingFinished>b__84_0(Tuple<string, MyWorldInfo> a, Tuple<string, MyWorldInfo> b) => 
                a.Item2.SessionName.CompareTo(b.Item2.SessionName);
        }
    }
}

