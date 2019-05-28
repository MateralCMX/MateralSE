namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenOptionsGame : MyGuiScreenBase
    {
        private MyGuiControlCombobox m_languageCombobox;
        private MyGuiControlCombobox m_skinCombobox;
        private MyGuiControlCombobox m_buildingModeCombobox;
        private MyGuiControlCheckbox m_experimentalCheckbox;
        private MyGuiControlCheckbox m_controlHintsCheckbox;
        private MyGuiControlCheckbox m_goodbotHintsCheckbox;
        private MyGuiControlCheckbox m_rotationHintsCheckbox;
        private MyGuiControlCheckbox m_crosshairCheckbox;
        private MyGuiControlCheckbox m_cloudCheckbox;
        private MyGuiControlCheckbox m_anonymousActivityCheckbox;
        private MyGuiControlSlider m_UIOpacitySlider;
        private MyGuiControlSlider m_UIBkOpacitySlider;
        private MyGuiControlSlider m_HUDBkOpacitySlider;
        private MyGuiControlButton m_localizationWebButton;
        private MyGuiControlLabel m_skinLabel;
        private MyGuiControlLabel m_skinWarningLabel;
        private MyGuiControlLabel m_localizationWarningLabel;
        private OptionsGameSettings m_settings;
        private bool m_languangeChanged;
        private MyGuiControlElementGroup m_elementGroup;
        private MyLanguagesEnum m_loadedLanguage;

        public MyGuiScreenOptionsGame() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.8587787f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            OptionsGameSettings settings = new OptionsGameSettings {
                UIOpacity = 1f,
                UIBkOpacity = 1f,
                HUDBkOpacity = 0.6f
            };
            this.m_settings = settings;
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        private void checkboxChanged(MyGuiControlCheckbox obj)
        {
            if (!ReferenceEquals(obj, this.m_experimentalCheckbox))
            {
                if (ReferenceEquals(obj, this.m_controlHintsCheckbox))
                {
                    this.m_settings.ControlHints = obj.IsChecked;
                }
            }
            else if (obj.IsChecked)
            {
                StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder(MyTexts.GetString(MyCommonTexts.MessageBoxTextConfirmExperimental)), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                    if (retval == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        this.m_settings.ExperimentalMode = obj.IsChecked;
                    }
                    else
                    {
                        this.m_settings.ExperimentalMode = false;
                        obj.IsChecked = false;
                    }
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            if ((this.m_rotationHintsCheckbox != null) && ReferenceEquals(obj, this.m_rotationHintsCheckbox))
            {
                this.m_settings.RotationHints = obj.IsChecked;
            }
            else if (ReferenceEquals(obj, this.m_crosshairCheckbox))
            {
                this.m_settings.ShowCrosshair = obj.IsChecked;
            }
            else if (ReferenceEquals(obj, this.m_cloudCheckbox))
            {
                this.m_settings.EnableSteamCloud = obj.IsChecked;
            }
            else if (ReferenceEquals(obj, this.m_goodbotHintsCheckbox))
            {
                this.m_settings.GoodBotHints = obj.IsChecked;
            }
            else if (ReferenceEquals(obj, this.m_anonymousActivityCheckbox))
            {
                MySandboxGame.Config.GDPRConsent = new bool?(obj.IsChecked);
                MySandboxGame.Config.Save();
                ConsentSenderGDPR.TrySendConsent();
            }
        }

        private void DoChanges()
        {
            MySandboxGame.Config.ExperimentalMode = this.m_experimentalCheckbox.IsChecked;
            MySandboxGame.Config.ShowCrosshair = this.m_crosshairCheckbox.IsChecked;
            MySandboxGame.Config.EnableSteamCloud = this.m_cloudCheckbox.IsChecked;
            MySandboxGame.Config.UIOpacity = this.m_UIOpacitySlider.Value;
            MySandboxGame.Config.UIBkOpacity = this.m_UIBkOpacitySlider.Value;
            MySandboxGame.Config.HUDBkOpacity = this.m_HUDBkOpacitySlider.Value;
            MyLanguage.CurrentLanguage = (MyLanguagesEnum) ((byte) this.m_languageCombobox.GetSelectedKey());
            if (this.m_loadedLanguage != MyLanguage.CurrentLanguage)
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextRestartNeededAfterLanguageSwitch), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                MyScreenManager.RecreateControls();
            }
            if (this.m_skinCombobox != null)
            {
                MyGuiSkinManager.Static.SelectSkin((int) this.m_skinCombobox.GetSelectedKey());
            }
            MyCubeBuilder.BuildingMode = (MyCubeBuilder.BuildingModeEnum) ((int) this.m_buildingModeCombobox.GetSelectedKey());
            MySandboxGame.Config.ControlsHints = this.m_controlHintsCheckbox.IsChecked;
            MySandboxGame.Config.GoodBotHints = this.m_goodbotHintsCheckbox.IsChecked;
            if (this.m_rotationHintsCheckbox != null)
            {
                MySandboxGame.Config.RotationHints = this.m_rotationHintsCheckbox.IsChecked;
            }
            if (MyGuiScreenHudSpace.Static != null)
            {
                MyGuiScreenHudSpace.Static.RegisterAlphaMultiplier(VisualStyleCategory.Background, this.m_HUDBkOpacitySlider.Value);
            }
            MySandboxGame.Config.Save();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptionsGame";

        private void LocalizationWebButtonClicked(MyGuiControlButton obj)
        {
            StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder().AppendFormat(MyTexts.GetString(MyCommonTexts.MessageBoxTextOpenBrowser), MyPerGameSettings.GameWebUrl), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum retval) {
                if ((retval == MyGuiScreenMessageBox.ResultEnum.YES) && !MyBrowserHelper.OpenInternetBrowser(MyPerGameSettings.LocalizationWebUrl))
                {
                    StringBuilder messageText = new StringBuilder();
                    messageText.AppendFormat(MyTexts.GetString(MyCommonTexts.TitleFailedToStartInternetBrowser), MyPerGameSettings.LocalizationWebUrl);
                    MyStringId? nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    Vector2? nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.TitleFailedToStartInternetBrowser), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        private void m_buildingModeCombobox_ItemSelected()
        {
            this.m_settings.BuildingMode = (MyCubeBuilder.BuildingModeEnum) ((int) this.m_buildingModeCombobox.GetSelectedKey());
        }

        private void m_elementGroup_HighlightChanged(MyGuiControlElementGroup obj)
        {
            foreach (MyGuiControlBase base2 in this.m_elementGroup)
            {
                if (base2.HasFocus && !ReferenceEquals(obj.SelectedElement, base2))
                {
                    base.FocusedControl = obj.SelectedElement;
                    break;
                }
            }
        }

        private void m_languageCombobox_ItemSelected()
        {
            this.m_settings.Language = (MyLanguagesEnum) ((byte) this.m_languageCombobox.GetSelectedKey());
            if (MyTexts.Languages[this.m_settings.Language].IsCommunityLocalized)
            {
                this.m_localizationWarningLabel.ColorMask = Color.Red.ToVector4();
            }
            else
            {
                this.m_localizationWarningLabel.ColorMask = Color.White.ToVector4();
            }
        }

        public void OnCancelClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        public void OnOkClick(MyGuiControlButton sender)
        {
            this.DoChanges();
            this.CloseScreen();
        }

        public override void RecreateControls(bool constructor)
        {
            int? nullable3;
            int num1;
            base.RecreateControls(constructor);
            this.m_elementGroup = new MyGuiControlElementGroup();
            this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
            VRageMath.Vector4? captionTextColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionGameOptions, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            captionTextColor = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(control);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            captionTextColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
            this.Controls.Add(list2);
            MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            MyGuiDrawAlignEnum enum3 = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            Vector2 vector = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2 vector2 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            float x = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float num2 = 25f;
            float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 0.5f;
            Vector2 vector3 = new Vector2(0f, 0.045f);
            float num5 = 0f;
            Vector2 vector4 = new Vector2(0f, 0.008f);
            Vector2 vector5 = (((base.m_size.Value / 2f) - vector) * new Vector2(-1f, -1f)) + new Vector2(0f, y);
            Vector2 vector6 = (((base.m_size.Value / 2f) - vector) * new Vector2(1f, -1f)) + new Vector2(0f, y);
            Vector2 vector7 = ((base.m_size.Value / 2f) - vector2) * new Vector2(0f, 1f);
            Vector2 vector8 = new Vector2(vector6.X - (x + 0.0015f), vector6.Y);
            num5 -= 0.045f;
            Vector2? position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.Language), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label1.Position = (vector5 + (num5 * vector3)) + vector4;
            label1.OriginAlign = originAlign;
            MyGuiControlLabel label = label1;
            MyGuiControlCombobox combobox1 = new MyGuiControlCombobox();
            combobox1.Position = vector6 + (num5 * vector3);
            combobox1.OriginAlign = enum3;
            this.m_languageCombobox = combobox1;
            this.m_languageCombobox.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsGame_Language));
            foreach (MyLanguagesEnum enum4 in MyLanguage.SupportedLanguages)
            {
                DictionaryReader<MyLanguagesEnum, MyTexts.LanguageDescription> languages = MyTexts.Languages;
                MyTexts.LanguageDescription local1 = languages[enum4];
                string name = local1.Name;
                if (local1.IsCommunityLocalized)
                {
                    name = name + " *";
                }
                nullable3 = null;
                this.m_languageCombobox.AddItem((long) enum4, name, nullable3, null);
            }
            this.m_languageCombobox.CustomSortItems((a, b) => a.Key.CompareTo(b.Key));
            this.m_languageCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_languageCombobox_ItemSelected);
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label13 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_LocalizationWarning), captionTextColor, 0.578f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label13.Position = (vector6 + (num5 * vector3)) - new Vector2(x - 0.005f, 0f);
            label13.OriginAlign = originAlign;
            this.m_localizationWarningLabel = label13;
            position = null;
            captionTextColor = null;
            StringBuilder text = MyTexts.Get(MyCommonTexts.ScreenOptionsGame_MoreInfo);
            nullable3 = null;
            this.m_localizationWebButton = new MyGuiControlButton(new Vector2?((vector6 + (num5 * vector3)) - new Vector2((x - 0.008f) - this.m_localizationWarningLabel.Size.X, 0f)), MyGuiControlButtonStyleEnum.Default, position, captionTextColor, originAlign, null, text, 0.6f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.LocalizationWebButtonClicked), GuiSounds.MouseClick, 1f, nullable3, false);
            this.m_localizationWebButton.VisualStyle = MyGuiControlButtonStyleEnum.ClickableText;
            num5 += 0.83f;
            if (MyFakes.ENABLE_NON_PUBLIC_GUI_ELEMENTS && (MyGuiSkinManager.Static.SkinCount > 0))
            {
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label14 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_Skin), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label14.Position = vector5 + (num5 * vector3);
                label14.OriginAlign = originAlign;
                this.m_skinLabel = label14;
                MyGuiControlCombobox combobox2 = new MyGuiControlCombobox();
                combobox2.Position = vector6 + (num5 * vector3);
                combobox2.OriginAlign = enum3;
                this.m_skinCombobox = combobox2;
                foreach (KeyValuePair<int, MyGuiSkinDefinition> pair in MyGuiSkinManager.Static.AvailableSkins)
                {
                    nullable3 = null;
                    this.m_skinCombobox.AddItem((long) pair.Key, pair.Value.DisplayNameText, nullable3, null);
                }
                this.m_skinCombobox.SelectItemByKey((long) MyGuiSkinManager.Static.CurrentSkinId, true);
                num5 += 0.65f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label15 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_SkinWarning), captionTextColor, 0.578f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label15.Position = vector6 + (num5 * vector3);
                label15.OriginAlign = enum3;
                this.m_skinWarningLabel = label15;
                num5 += 0.8f;
            }
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label16 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_BuildingMode), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label16.Position = (vector5 + (num5 * vector3)) + vector4;
            label16.OriginAlign = originAlign;
            MyGuiControlLabel label2 = label16;
            MyGuiControlCombobox combobox3 = new MyGuiControlCombobox();
            combobox3.Position = vector6 + (num5 * vector3);
            combobox3.OriginAlign = enum3;
            this.m_buildingModeCombobox = combobox3;
            this.m_buildingModeCombobox.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsGame_BuildingMode));
            nullable3 = null;
            MyStringId? toolTip = null;
            this.m_buildingModeCombobox.AddItem(0L, MyCommonTexts.ScreenOptionsGame_SingleBlock, nullable3, toolTip);
            nullable3 = null;
            toolTip = null;
            this.m_buildingModeCombobox.AddItem(1L, MyCommonTexts.ScreenOptionsGame_Line, nullable3, toolTip);
            nullable3 = null;
            toolTip = null;
            this.m_buildingModeCombobox.AddItem(2L, MyCommonTexts.ScreenOptionsGame_Plane, nullable3, toolTip);
            this.m_buildingModeCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_buildingModeCombobox_ItemSelected);
            num5 += 1.26f;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label17 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ExperimentalMode), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label17.Position = (vector5 + (num5 * vector3)) + vector4;
            label17.OriginAlign = originAlign;
            MyGuiControlLabel label3 = label17;
            label3.Enabled = ReferenceEquals(MyGuiScreenGamePlay.Static, null);
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsExperimentalMode), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox1.Position = vector8 + (num5 * vector3);
            checkbox1.OriginAlign = originAlign;
            this.m_experimentalCheckbox = checkbox1;
            this.m_experimentalCheckbox.Enabled = ReferenceEquals(MyGuiScreenGamePlay.Static, null);
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label18 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ShowControlsHints), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label18.Position = (vector5 + (num5 * vector3)) + vector4;
            label18.OriginAlign = originAlign;
            MyGuiControlLabel label4 = label18;
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsShowControlsHints), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox2.Position = vector8 + (num5 * vector3);
            checkbox2.OriginAlign = originAlign;
            this.m_controlHintsCheckbox = checkbox2;
            this.m_controlHintsCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_controlHintsCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label19 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ShowGoodBotHints), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label19.Position = (vector5 + (num5 * vector3)) + vector4;
            label19.OriginAlign = originAlign;
            MyGuiControlLabel label5 = label19;
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsShowGoodBotHints), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox3.Position = vector8 + (num5 * vector3);
            checkbox3.OriginAlign = originAlign;
            this.m_goodbotHintsCheckbox = checkbox3;
            this.m_goodbotHintsCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_goodbotHintsCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            MyGuiControlLabel label6 = null;
            if (MyFakes.ENABLE_ROTATION_HINTS)
            {
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label20 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ShowRotationHints), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label20.Position = (vector5 + (num5 * vector3)) + vector4;
                label20.OriginAlign = originAlign;
                label6 = label20;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox4 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsShowRotationHints), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox4.Position = vector8 + (num5 * vector3);
                checkbox4.OriginAlign = originAlign;
                this.m_rotationHintsCheckbox = checkbox4;
                this.m_rotationHintsCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_rotationHintsCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            }
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label21 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ShowCrosshair), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label21.Position = (vector5 + (num5 * vector3)) + vector4;
            label21.OriginAlign = originAlign;
            MyGuiControlLabel label7 = label21;
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox5 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsShowCrosshair), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox5.Position = vector8 + (num5 * vector3);
            checkbox5.OriginAlign = originAlign;
            this.m_crosshairCheckbox = checkbox5;
            this.m_crosshairCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_crosshairCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label22 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.EnableSteamCloud), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label22.Position = (vector5 + (num5 * vector3)) + vector4;
            label22.OriginAlign = originAlign;
            MyGuiControlLabel label8 = label22;
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox6 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsEnableSteamCloud), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox6.Position = vector8 + (num5 * vector3);
            checkbox6.OriginAlign = originAlign;
            this.m_cloudCheckbox = checkbox6;
            this.m_cloudCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_cloudCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            num5++;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label23 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.AnonymousActivityTracking_Caption), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label23.Position = (vector5 + (num5 * vector3)) + vector4;
            label23.OriginAlign = originAlign;
            MyGuiControlLabel label9 = label23;
            position = null;
            captionTextColor = null;
            MyGuiControlCheckbox checkbox7 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGame_AnonymousActivityTracking), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            checkbox7.Position = vector8 + (num5 * vector3);
            checkbox7.OriginAlign = originAlign;
            this.m_anonymousActivityCheckbox = checkbox7;
            bool? gDPRConsent = MySandboxGame.Config.GDPRConsent;
            bool flag = true;
            if (!((gDPRConsent.GetValueOrDefault() == flag) & (gDPRConsent != null)))
            {
                num1 = (int) (MySandboxGame.Config.GDPRConsent == null);
            }
            else
            {
                num1 = 1;
            }
            this.m_anonymousActivityCheckbox.IsChecked = (bool) num1;
            this.m_anonymousActivityCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_anonymousActivityCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label24 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_ReleasingAltResetsCamera), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label24.Position = vector5 + (num5 * vector3);
            label24.OriginAlign = originAlign;
            MyGuiControlLabel label25 = label24;
            num5 += 1.35f;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label26 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_UIOpacity), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label26.Position = (vector5 + (num5 * vector3)) + vector4;
            label26.OriginAlign = originAlign;
            MyGuiControlLabel label10 = label26;
            position = null;
            string str2 = MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsUIOpacity);
            captionTextColor = null;
            MyGuiControlSlider slider1 = new MyGuiControlSlider(position, 0.1f, 1f, 0.29f, 1f, captionTextColor, null, 1, 0.8f, 0f, "White", str2, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider1.Position = vector6 + (num5 * vector3);
            slider1.OriginAlign = enum3;
            slider1.Size = new Vector2(x, 0f);
            this.m_UIOpacitySlider = slider1;
            this.m_UIOpacitySlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_UIOpacitySlider.ValueChanged, new Action<MyGuiControlSlider>(this.sliderChanged));
            num5 += 1.08f;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label27 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_UIBkOpacity), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label27.Position = (vector5 + (num5 * vector3)) + vector4;
            label27.OriginAlign = originAlign;
            MyGuiControlLabel label11 = label27;
            position = null;
            str2 = MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsUIBkOpacity);
            captionTextColor = null;
            MyGuiControlSlider slider2 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, 1f, captionTextColor, null, 1, 0.8f, 0f, "White", str2, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider2.Position = vector6 + (num5 * vector3);
            slider2.OriginAlign = enum3;
            slider2.Size = new Vector2(x, 0f);
            this.m_UIBkOpacitySlider = slider2;
            this.m_UIBkOpacitySlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_UIBkOpacitySlider.ValueChanged, new Action<MyGuiControlSlider>(this.sliderChanged));
            num5 += 1.08f;
            position = null;
            position = null;
            captionTextColor = null;
            MyGuiControlLabel label28 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsGame_HUDBkOpacity), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            label28.Position = (vector5 + (num5 * vector3)) + vector4;
            label28.OriginAlign = originAlign;
            MyGuiControlLabel label12 = label28;
            position = null;
            str2 = MyTexts.GetString(MyCommonTexts.ToolTipGameOptionsHUDBkOpacity);
            captionTextColor = null;
            MyGuiControlSlider slider3 = new MyGuiControlSlider(position, 0f, 1f, 0.29f, 1f, captionTextColor, null, 1, 0.8f, 0f, "White", str2, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            slider3.Position = vector6 + (num5 * vector3);
            slider3.OriginAlign = enum3;
            slider3.Size = new Vector2(x, 0f);
            this.m_HUDBkOpacitySlider = slider3;
            this.m_HUDBkOpacitySlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_HUDBkOpacitySlider.ValueChanged, new Action<MyGuiControlSlider>(this.sliderChanged));
            position = null;
            position = null;
            captionTextColor = null;
            nullable3 = null;
            MyGuiControlButton button = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, nullable3, false);
            button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            position = null;
            position = null;
            captionTextColor = null;
            nullable3 = null;
            MyGuiControlButton button2 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, nullable3, false);
            button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            button.Position = vector7 + (new Vector2(-num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            button2.Position = vector7 + (new Vector2(num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            this.Controls.Add(label);
            this.Controls.Add(this.m_languageCombobox);
            this.Controls.Add(this.m_localizationWebButton);
            this.Controls.Add(this.m_localizationWarningLabel);
            if (MyFakes.ENABLE_NON_PUBLIC_GUI_ELEMENTS && (MyGuiSkinManager.Static.SkinCount > 0))
            {
                this.Controls.Add(this.m_skinLabel);
                this.Controls.Add(this.m_skinCombobox);
                this.Controls.Add(this.m_skinWarningLabel);
            }
            this.Controls.Add(label2);
            this.Controls.Add(this.m_buildingModeCombobox);
            this.Controls.Add(label3);
            this.Controls.Add(label4);
            this.Controls.Add(label5);
            this.Controls.Add(this.m_experimentalCheckbox);
            this.Controls.Add(this.m_controlHintsCheckbox);
            this.Controls.Add(this.m_goodbotHintsCheckbox);
            if (MyFakes.ENABLE_ROTATION_HINTS)
            {
                this.Controls.Add(label6);
                this.Controls.Add(this.m_rotationHintsCheckbox);
            }
            this.Controls.Add(label7);
            this.Controls.Add(this.m_crosshairCheckbox);
            this.Controls.Add(label8);
            this.Controls.Add(this.m_cloudCheckbox);
            this.Controls.Add(label9);
            this.Controls.Add(this.m_anonymousActivityCheckbox);
            this.Controls.Add(label10);
            this.Controls.Add(this.m_UIOpacitySlider);
            this.Controls.Add(label11);
            this.Controls.Add(this.m_UIBkOpacitySlider);
            this.Controls.Add(label12);
            this.Controls.Add(this.m_HUDBkOpacitySlider);
            this.Controls.Add(button);
            this.m_elementGroup.Add(button);
            this.Controls.Add(button2);
            this.m_elementGroup.Add(button2);
            this.UpdateControls(constructor);
            this.m_experimentalCheckbox.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_experimentalCheckbox.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.checkboxChanged));
            base.FocusedControl = button;
            base.CloseButtonEnabled = true;
        }

        private void sliderChanged(MyGuiControlSlider obj)
        {
            if (ReferenceEquals(obj, this.m_UIOpacitySlider))
            {
                this.m_settings.UIOpacity = obj.Value;
                base.m_guiTransition = obj.Value;
            }
            else if (ReferenceEquals(obj, this.m_UIBkOpacitySlider))
            {
                this.m_settings.UIBkOpacity = obj.Value;
                base.m_backgroundTransition = obj.Value;
            }
            else if (ReferenceEquals(obj, this.m_HUDBkOpacitySlider))
            {
                this.m_settings.HUDBkOpacity = obj.Value;
            }
        }

        private void UpdateControls(bool constructor)
        {
            if (!constructor)
            {
                this.m_languageCombobox.SelectItemByKey((long) this.m_settings.Language, true);
                this.m_buildingModeCombobox.SelectItemByKey((long) this.m_settings.BuildingMode, true);
                this.m_controlHintsCheckbox.IsChecked = this.m_settings.ControlHints;
                this.m_goodbotHintsCheckbox.IsChecked = this.m_settings.GoodBotHints;
                this.m_experimentalCheckbox.IsChecked = this.m_settings.ExperimentalMode;
                if (this.m_rotationHintsCheckbox != null)
                {
                    this.m_rotationHintsCheckbox.IsChecked = this.m_settings.RotationHints;
                }
                this.m_crosshairCheckbox.IsChecked = this.m_settings.ShowCrosshair;
                this.m_cloudCheckbox.IsChecked = this.m_settings.EnableSteamCloud;
                this.m_UIOpacitySlider.Value = this.m_settings.UIOpacity;
                this.m_UIBkOpacitySlider.Value = this.m_settings.UIBkOpacity;
                this.m_HUDBkOpacitySlider.Value = this.m_settings.HUDBkOpacity;
            }
            else
            {
                this.m_languageCombobox.SelectItemByKey((long) MySandboxGame.Config.Language, true);
                this.m_loadedLanguage = (MyLanguagesEnum) ((byte) this.m_languageCombobox.GetSelectedKey());
                this.m_buildingModeCombobox.SelectItemByKey((long) MyCubeBuilder.BuildingMode, true);
                this.m_controlHintsCheckbox.IsChecked = MySandboxGame.Config.ControlsHints;
                this.m_goodbotHintsCheckbox.IsChecked = MySandboxGame.Config.GoodBotHints;
                this.m_experimentalCheckbox.IsChecked = MySandboxGame.Config.ExperimentalMode;
                if (this.m_rotationHintsCheckbox != null)
                {
                    this.m_rotationHintsCheckbox.IsChecked = MySandboxGame.Config.RotationHints;
                }
                this.m_crosshairCheckbox.IsChecked = MySandboxGame.Config.ShowCrosshair;
                this.m_cloudCheckbox.IsChecked = MySandboxGame.Config.EnableSteamCloud;
                this.m_UIOpacitySlider.Value = MySandboxGame.Config.UIOpacity;
                this.m_UIBkOpacitySlider.Value = MySandboxGame.Config.UIBkOpacity;
                this.m_HUDBkOpacitySlider.Value = MySandboxGame.Config.HUDBkOpacity;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenOptionsGame.<>c <>9 = new MyGuiScreenOptionsGame.<>c();
            public static Comparison<MyGuiControlCombobox.Item> <>9__23_0;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__28_0;

            internal void <LocalizationWebButtonClicked>b__28_0(MyGuiScreenMessageBox.ResultEnum retval)
            {
                if ((retval == MyGuiScreenMessageBox.ResultEnum.YES) && !MyBrowserHelper.OpenInternetBrowser(MyPerGameSettings.LocalizationWebUrl))
                {
                    StringBuilder messageText = new StringBuilder();
                    messageText.AppendFormat(MyTexts.GetString(MyCommonTexts.TitleFailedToStartInternetBrowser), MyPerGameSettings.LocalizationWebUrl);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageText, MyTexts.Get(MyCommonTexts.TitleFailedToStartInternetBrowser), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }

            internal int <RecreateControls>b__23_0(MyGuiControlCombobox.Item a, MyGuiControlCombobox.Item b) => 
                a.Key.CompareTo(b.Key);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OptionsGameSettings
        {
            public MyLanguagesEnum Language;
            public MyCubeBuilder.BuildingModeEnum BuildingMode;
            public MyStringId SkinId;
            public bool ExperimentalMode;
            public bool ControlHints;
            public bool GoodBotHints;
            public bool RotationHints;
            public bool ShowCrosshair;
            public bool EnableSteamCloud;
            public bool EnablePrediction;
            public bool ShowPlayerNamesOnHud;
            public bool EnablePerformanceWarnings;
            public float UIOpacity;
            public float UIBkOpacity;
            public float HUDBkOpacity;
        }
    }
}

