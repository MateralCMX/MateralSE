namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenOptionsDisplay : MyGuiScreenBase
    {
        private MyGuiControlLabel m_labelRecommendAspectRatio;
        private MyGuiControlLabel m_labelUnsupportedAspectRatio;
        private MyGuiControlCombobox m_comboVideoAdapter;
        private MyGuiControlCombobox m_comboResolution;
        private MyGuiControlCombobox m_comboWindowMode;
        private MyGuiControlCheckbox m_checkboxVSync;
        private MyGuiControlCheckbox m_checkboxCaptureMouse;
        private MyGuiControlCombobox m_comboScreenshotMultiplier;
        private MyRenderDeviceSettings m_settingsOld;
        private MyRenderDeviceSettings m_settingsNew;
        private bool m_waitingForConfirmation;
        private bool m_doRevert;
        private MyGuiControlElementGroup m_elementGroup;

        public MyGuiScreenOptionsDisplay() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.5696565f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        public override bool CloseScreen()
        {
            bool flag1 = base.CloseScreen();
            if (flag1)
            {
                bool waitingForConfirmation = this.m_waitingForConfirmation;
            }
            return flag1;
        }

        public override bool Draw()
        {
            if (!base.Draw())
            {
                return false;
            }
            if (this.m_doRevert)
            {
                this.OnVideoModeChanged(MyVideoSettingsManager.Apply(this.m_settingsOld));
                this.m_doRevert = false;
            }
            return true;
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptionsVideo";

        private Vector2I GetResolutionFromKey(long key) => 
            new Vector2I((int) (key >> 0x20), (int) (((ulong) key) & 0xffffffffUL));

        private long GetResolutionKey(Vector2I resolution) => 
            ((resolution.X << 0x20) | resolution.Y);

        private MyAdapterInfo GetSelectedAdapter() => 
            MyVideoSettingsManager.Adapters[(int) this.m_comboVideoAdapter.GetSelectedKey()];

        private Vector2I GetSelectedResolution()
        {
            long selectedKey = this.m_comboResolution.GetSelectedKey();
            return this.GetResolutionFromKey(selectedKey);
        }

        private MyWindowModeEnum GetSelectedWindowMode() => 
            ((MyWindowModeEnum) ((byte) this.m_comboWindowMode.GetSelectedKey()));

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

        public void OnCancelClick(MyGuiControlButton sender)
        {
            this.CloseScreen();
        }

        public void OnMessageBoxAdapterChangeCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MySessionLoader.ExitGame();
            }
        }

        public void OnMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn != MyGuiScreenMessageBox.ResultEnum.YES)
            {
                this.m_doRevert = true;
            }
            else
            {
                if (this.m_settingsNew.NewAdapterOrdinal != this.m_settingsNew.AdapterOrdinal)
                {
                    MySandboxGame.Config.DisableUpdateDriverNotification = false;
                }
                MyVideoSettingsManager.SaveCurrentSettings();
                this.ReadSettingsFromControls(ref this.m_settingsOld);
                this.CloseScreenNow();
                if (this.m_settingsNew.NewAdapterOrdinal != this.m_settingsNew.AdapterOrdinal)
                {
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning);
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextRestartNeededAfterAdapterSwitch), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnMessageBoxAdapterChangeCallback), 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
            }
            this.m_waitingForConfirmation = false;
        }

        public void OnOkClick(MyGuiControlButton sender)
        {
            MySandboxGame.Config.ScreenshotSizeMultiplier = this.m_comboScreenshotMultiplier.GetSelectedKey();
            if (this.ReadSettingsFromControls(ref this.m_settingsNew))
            {
                this.OnVideoModeChangedAndConfirm(MyVideoSettingsManager.Apply(this.m_settingsNew));
            }
            else
            {
                this.CloseScreen();
            }
        }

        private void OnVideoModeChanged(MyVideoSettingsManager.ChangeResult result)
        {
            this.WriteSettingsToControls(this.m_settingsOld);
            this.ReadSettingsFromControls(ref this.m_settingsNew);
        }

        private void OnVideoModeChangedAndConfirm(MyVideoSettingsManager.ChangeResult result)
        {
            MyStringId? nullable;
            Vector2? nullable2;
            switch (result)
            {
                case MyVideoSettingsManager.ChangeResult.Success:
                {
                    this.m_waitingForConfirmation = true;
                    StringBuilder messageCaption = MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm);
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_TIMEOUT, MyTexts.Get(MyCommonTexts.DoYouWantToKeepTheseSettingsXSecondsRemaining), messageCaption, nullable, nullable, nullable, nullable, new Action<MyGuiScreenMessageBox.ResultEnum>(this.OnMessageBoxCallback), 0xea60, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    return;
                }
                case MyVideoSettingsManager.ChangeResult.NothingChanged:
                    break;

                case MyVideoSettingsManager.ChangeResult.Failed:
                    this.m_doRevert = true;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable = null;
                    nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.SorryButSelectedSettingsAreNotSupportedByYourHardware), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                    break;

                default:
                    return;
            }
        }

        private bool ReadSettingsFromControls(ref MyRenderDeviceSettings deviceSettings)
        {
            bool flag = false;
            MyRenderDeviceSettings settings = new MyRenderDeviceSettings {
                AdapterOrdinal = deviceSettings.AdapterOrdinal
            };
            Vector2I selectedResolution = this.GetSelectedResolution();
            settings.BackBufferWidth = selectedResolution.X;
            settings.BackBufferHeight = selectedResolution.Y;
            settings.WindowMode = this.GetSelectedWindowMode();
            settings.NewAdapterOrdinal = (int) this.m_comboVideoAdapter.GetSelectedKey();
            flag |= settings.NewAdapterOrdinal != settings.AdapterOrdinal;
            settings.VSync = this.m_checkboxVSync.IsChecked;
            settings.RefreshRate = 0;
            if (this.m_checkboxCaptureMouse.IsChecked != MySandboxGame.Config.CaptureMouse)
            {
                MySandboxGame.Config.CaptureMouse = this.m_checkboxCaptureMouse.IsChecked;
                MySandboxGame.Static.UpdateMouseCapture();
            }
            foreach (MyDisplayMode mode in MyVideoSettingsManager.Adapters[deviceSettings.AdapterOrdinal].SupportedDisplayModes)
            {
                if (((mode.Width == settings.BackBufferWidth) && (mode.Height == settings.BackBufferHeight)) && (settings.RefreshRate < mode.RefreshRate))
                {
                    settings.RefreshRate = mode.RefreshRate;
                }
            }
            flag = flag || !settings.Equals(ref deviceSettings);
            deviceSettings = settings;
            return flag;
        }

        public override void RecreateControls(bool constructor)
        {
            if (constructor)
            {
                int? nullable3;
                base.RecreateControls(constructor);
                this.m_elementGroup = new MyGuiControlElementGroup();
                this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
                VRageMath.Vector4? captionTextColor = null;
                base.AddCaption(MyCommonTexts.ScreenCaptionDisplay, captionTextColor, new Vector2(0f, 0.003f), 0.8f);
                MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
                captionTextColor = null;
                control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
                this.Controls.Add(control);
                MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
                captionTextColor = null;
                list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.83f, 0f, captionTextColor);
                this.Controls.Add(list2);
                MyGuiDrawAlignEnum enum2 = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
                MyGuiDrawAlignEnum enum3 = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
                Vector2 vector = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
                Vector2 vector2 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
                float num = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
                float x = 25f;
                float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 0.5f;
                float num4 = 0.0015f;
                Vector2 vector3 = new Vector2(0f, 0.045f);
                float num5 = 0f;
                Vector2 vector4 = new Vector2(0f, 0.008f);
                Vector2 vector5 = (((base.m_size.Value / 2f) - vector) * new Vector2(-1f, -1f)) + new Vector2(0f, y);
                Vector2 vector6 = (((base.m_size.Value / 2f) - vector) * new Vector2(1f, -1f)) + new Vector2(0f, y);
                Vector2 vector7 = ((base.m_size.Value / 2f) - vector2) * new Vector2(0f, 1f);
                Vector2 vector8 = new Vector2(vector6.X - (num + num4), vector6.Y);
                num5 -= 0.045f;
                Vector2? position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.VideoAdapter), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label1.Position = (vector5 + (num5 * vector3)) + vector4;
                label1.OriginAlign = enum2;
                MyGuiControlLabel label = label1;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox1 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MyCommonTexts.ToolTipVideoOptionsVideoAdapter), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox1.Position = vector6 + (num5 * vector3);
                combobox1.OriginAlign = enum3;
                this.m_comboVideoAdapter = combobox1;
                int num6 = 0;
                foreach (MyAdapterInfo info in MyVideoSettingsManager.Adapters)
                {
                    num6++;
                    nullable3 = null;
                    this.m_comboVideoAdapter.AddItem((long) num6, new StringBuilder(info.Name), nullable3, null);
                }
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label7 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenOptionsVideo_WindowMode), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label7.Position = (vector5 + (num5 * vector3)) + vector4;
                label7.OriginAlign = enum2;
                MyGuiControlLabel label2 = label7;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox2 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsDisplay_WindowMode), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox2.Position = vector6 + (num5 * vector3);
                combobox2.OriginAlign = enum3;
                this.m_comboWindowMode = combobox2;
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label8 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.VideoMode), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label8.Position = (vector5 + (num5 * vector3)) + vector4;
                label8.OriginAlign = enum2;
                MyGuiControlLabel label3 = label8;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox3 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MyCommonTexts.ToolTipVideoOptionsVideoMode), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox3.Position = vector6 + (num5 * vector3);
                combobox3.OriginAlign = enum3;
                this.m_comboResolution = combobox3;
                num5++;
                position = null;
                position = null;
                MyGuiControlLabel label9 = new MyGuiControlLabel(position, position, null, new VRageMath.Vector4?(MyGuiConstants.LABEL_TEXT_COLOR * 0.9f), 0.578f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label9.Position = new Vector2(vector6.X - (num - num4), vector6.Y) + (num5 * vector3);
                label9.OriginAlign = enum2;
                this.m_labelUnsupportedAspectRatio = label9;
                this.m_labelUnsupportedAspectRatio.Text = $"* {MyTexts.Get(MyCommonTexts.UnsupportedAspectRatio)}";
                num5 += 0.45f;
                position = null;
                position = null;
                MyGuiControlLabel label10 = new MyGuiControlLabel(position, position, null, new VRageMath.Vector4?(MyGuiConstants.LABEL_TEXT_COLOR * 0.9f), 0.578f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label10.Position = new Vector2(vector6.X - (num - num4), vector6.Y) + (num5 * vector3);
                label10.OriginAlign = enum2;
                this.m_labelRecommendAspectRatio = label10;
                num5 += 0.66f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label11 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenshotMultiplier), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label11.Position = (vector5 + (num5 * vector3)) + vector4;
                label11.OriginAlign = enum2;
                MyGuiControlLabel label4 = label11;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox4 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsDisplay_ScreenshotMultiplier), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox4.Position = vector6 + (num5 * vector3);
                combobox4.OriginAlign = enum3;
                this.m_comboScreenshotMultiplier = combobox4;
                num5 += 1.26f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label12 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.VerticalSync), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label12.Position = (vector5 + (num5 * vector3)) + vector4;
                label12.OriginAlign = enum2;
                MyGuiControlLabel label5 = label12;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipVideoOptionsVerticalSync), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox1.Position = vector8 + (num5 * vector3);
                checkbox1.OriginAlign = enum2;
                this.m_checkboxVSync = checkbox1;
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label13 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.CaptureMouse), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label13.Position = (vector5 + (num5 * vector3)) + vector4;
                label13.OriginAlign = enum2;
                MyGuiControlLabel label6 = label13;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MyCommonTexts.ToolTipVideoOptionsCaptureMouse), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox2.Position = vector8 + (num5 * vector3);
                checkbox2.OriginAlign = enum2;
                this.m_checkboxCaptureMouse = checkbox2;
                num5++;
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
                button.Position = vector7 + (new Vector2(-x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                button2.Position = vector7 + (new Vector2(x, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
                this.Controls.Add(label);
                this.Controls.Add(this.m_comboVideoAdapter);
                this.Controls.Add(label3);
                this.Controls.Add(this.m_comboResolution);
                this.Controls.Add(this.m_labelUnsupportedAspectRatio);
                this.Controls.Add(this.m_labelRecommendAspectRatio);
                this.Controls.Add(label2);
                this.Controls.Add(this.m_comboWindowMode);
                this.Controls.Add(label4);
                this.Controls.Add(this.m_comboScreenshotMultiplier);
                this.Controls.Add(label6);
                this.Controls.Add(this.m_checkboxCaptureMouse);
                this.Controls.Add(label5);
                this.Controls.Add(this.m_checkboxVSync);
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                this.Controls.Add(button2);
                this.m_elementGroup.Add(button2);
                this.m_comboVideoAdapter.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.UpdateWindowModeComboBox);
                this.m_comboVideoAdapter.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.UpdateResolutionComboBox);
                this.m_comboWindowMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.UpdateRecommendecAspectRatioLabel);
                this.m_comboWindowMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.UpdateResolutionComboBox);
                this.m_settingsOld = MyVideoSettingsManager.CurrentDeviceSettings;
                this.m_settingsNew = this.m_settingsOld;
                this.WriteSettingsToControls(this.m_settingsOld);
                this.ReadSettingsFromControls(ref this.m_settingsOld);
                this.ReadSettingsFromControls(ref this.m_settingsNew);
                base.FocusedControl = button;
                base.CloseButtonEnabled = true;
            }
        }

        private void SelectResolution(Vector2I resolution)
        {
            int num = 0x7fffffff;
            Vector2I vectori = resolution;
            int index = 0;
            while (true)
            {
                if (index < this.m_comboResolution.GetItemsCount())
                {
                    Vector2I resolutionFromKey = this.GetResolutionFromKey(this.m_comboResolution.GetItemByIndex(index).Key);
                    if (!(resolutionFromKey == resolution))
                    {
                        int num3 = Math.Abs((int) ((resolutionFromKey.X * resolutionFromKey.Y) - (resolution.X * resolution.Y)));
                        if (num3 < num)
                        {
                            num = num3;
                            vectori = resolutionFromKey;
                        }
                        index++;
                        continue;
                    }
                    vectori = resolution;
                }
                this.m_comboResolution.SelectItemByKey(this.GetResolutionKey(vectori), true);
                return;
            }
        }

        private void SelectWindowMode(MyWindowModeEnum mode)
        {
            this.m_comboWindowMode.SelectItemByKey((long) mode, true);
        }

        private void UpdateAdapterComboBox()
        {
            long selectedKey = this.m_comboVideoAdapter.GetSelectedKey();
            this.m_comboVideoAdapter.ClearItems();
            int num2 = 0;
            foreach (MyAdapterInfo info in MyVideoSettingsManager.Adapters)
            {
                num2++;
                int? sortOrder = null;
                this.m_comboVideoAdapter.AddItem((long) num2, new StringBuilder(info.Name), sortOrder, null);
            }
            this.m_comboVideoAdapter.SelectItemByKey(selectedKey, true);
        }

        private void UpdateRecommendecAspectRatioLabel()
        {
            MyAspectRatio recommendedAspectRatio = MyVideoSettingsManager.GetRecommendedAspectRatio((int) this.m_comboVideoAdapter.GetSelectedKey());
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(MyTexts.GetString(MyCommonTexts.RecommendedAspectRatio), recommendedAspectRatio.TextShort);
            this.m_labelRecommendAspectRatio.Text = $"*** {builder}";
        }

        private void UpdateResolutionComboBox()
        {
            Vector2I selectedResolution = this.GetSelectedResolution();
            MyWindowModeEnum selectedWindowMode = this.GetSelectedWindowMode();
            this.m_comboResolution.ClearItems();
            foreach (MyDisplayMode mode in this.GetSelectedAdapter().SupportedDisplayModes)
            {
                Vector2I inResolution = new Vector2I(mode.Width, mode.Height);
                bool flag = true;
                if ((selectedWindowMode == MyWindowModeEnum.Window) && (inResolution != MyRenderProxyUtils.GetFixedWindowResolution(inResolution, this.GetSelectedAdapter())))
                {
                    flag = false;
                }
                if (this.m_comboResolution.TryGetItemByKey(this.GetResolutionKey(inResolution)) != null)
                {
                    flag = false;
                }
                if (flag)
                {
                    MyAspectRatio recommendedAspectRatio = MyVideoSettingsManager.GetRecommendedAspectRatio((int) this.m_comboVideoAdapter.GetSelectedKey());
                    MyAspectRatioEnum closestAspectRatio = MyVideoSettingsManager.GetClosestAspectRatio(((float) inResolution.X) / ((float) inResolution.Y));
                    MyAspectRatio aspectRatio = MyVideoSettingsManager.GetAspectRatio(closestAspectRatio);
                    string textShort = aspectRatio.TextShort;
                    string str2 = aspectRatio.IsSupported ? ((closestAspectRatio == recommendedAspectRatio.AspectRatioEnum) ? " ***" : "") : " *";
                    object[] args = new object[] { inResolution.X, inResolution.Y, textShort, str2 };
                    int? sortOrder = null;
                    this.m_comboResolution.AddItem(this.GetResolutionKey(inResolution), new StringBuilder(string.Format("{0} x {1} - {2}{3}", args)), sortOrder, null);
                }
            }
            this.SelectResolution(selectedResolution);
        }

        private void UpdateScreenshotMultiplierComboBox()
        {
            int selectedKey = (int) this.m_comboScreenshotMultiplier.GetSelectedKey();
            this.m_comboScreenshotMultiplier.ClearItems();
            int? sortOrder = null;
            this.m_comboScreenshotMultiplier.AddItem(1L, "1x", sortOrder, null);
            sortOrder = null;
            this.m_comboScreenshotMultiplier.AddItem(2L, "2x", sortOrder, null);
            sortOrder = null;
            this.m_comboScreenshotMultiplier.AddItem(4L, "4x", sortOrder, null);
            sortOrder = null;
            this.m_comboScreenshotMultiplier.AddItem(8L, "8x", sortOrder, null);
            this.m_comboScreenshotMultiplier.SelectItemByKey((long) selectedKey, true);
        }

        private void UpdateWindowModeComboBox()
        {
            MyWindowModeEnum selectedKey = (MyWindowModeEnum) ((byte) this.m_comboWindowMode.GetSelectedKey());
            this.m_comboWindowMode.ClearItems();
            bool isOutputAttached = this.GetSelectedAdapter().IsOutputAttached;
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_comboWindowMode.AddItem(0L, MyCommonTexts.ScreenOptionsVideo_WindowMode_Window, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            this.m_comboWindowMode.AddItem(1L, MyCommonTexts.ScreenOptionsVideo_WindowMode_FullscreenWindow, sortOrder, toolTip);
            if (isOutputAttached)
            {
                sortOrder = null;
                toolTip = null;
                this.m_comboWindowMode.AddItem(2L, MyCommonTexts.ScreenOptionsVideo_WindowMode_Fullscreen, sortOrder, toolTip);
            }
            if ((selectedKey == MyWindowModeEnum.Fullscreen) && !isOutputAttached)
            {
                selectedKey = MyWindowModeEnum.FullscreenWindow;
            }
            this.m_comboWindowMode.SelectItemByKey((long) selectedKey, true);
        }

        private void WriteSettingsToControls(MyRenderDeviceSettings deviceSettings)
        {
            this.UpdateAdapterComboBox();
            this.m_comboVideoAdapter.SelectItemByKey((long) deviceSettings.NewAdapterOrdinal, false);
            this.UpdateWindowModeComboBox();
            this.UpdateResolutionComboBox();
            this.UpdateScreenshotMultiplierComboBox();
            this.m_comboScreenshotMultiplier.SelectItemByKey((long) ((int) MySandboxGame.Config.ScreenshotSizeMultiplier), true);
            this.SelectResolution(new Vector2I(deviceSettings.BackBufferWidth, deviceSettings.BackBufferHeight));
            this.SelectWindowMode(deviceSettings.WindowMode);
            this.m_checkboxVSync.IsChecked = deviceSettings.VSync;
            this.m_checkboxCaptureMouse.IsChecked = MySandboxGame.Config.CaptureMouse;
        }
    }
}

