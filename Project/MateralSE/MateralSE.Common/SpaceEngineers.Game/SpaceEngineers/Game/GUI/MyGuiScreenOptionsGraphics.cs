namespace SpaceEngineers.Game.GUI
{
    using Sandbox;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyGuiScreenOptionsGraphics : MyGuiScreenBase
    {
        private static readonly MyPerformanceSettings[] m_presets;
        private bool m_writingSettings;
        private MyGuiControlCombobox m_comboAntialiasing;
        private MyGuiControlCombobox m_comboShadowMapResolution;
        private MyGuiControlCheckbox m_checkboxAmbientOcclusionHBAO;
        private MyGuiControlCheckbox m_checkboxPostProcessing;
        private MyGuiControlCombobox m_comboTextureQuality;
        private MyGuiControlCombobox m_comboShaderQuality;
        private MyGuiControlCombobox m_comboAnisotropicFiltering;
        private MyGuiControlCombobox m_comboGraphicsPresets;
        private MyGuiControlCombobox m_comboModelQuality;
        private MyGuiControlCombobox m_comboVoxelQuality;
        private MyGuiControlSliderBase m_vegetationViewDistance;
        private MyGuiControlSlider m_grassDensitySlider;
        private MyGuiControlSliderBase m_grassDrawDistanceSlider;
        private MyGuiControlSlider m_sliderFov;
        private MyGuiControlSlider m_sliderFlares;
        private MyGuiControlCheckbox m_checkboxEnableDamageEffects;
        private MyGraphicsSettings m_settingsOld;
        private MyGraphicsSettings m_settingsNew;
        private MyGuiControlElementGroup m_elementGroup;

        static MyGuiScreenOptionsGraphics()
        {
            MyPerformanceSettings settings = new MyPerformanceSettings();
            MyRenderSettings1 settings2 = new MyRenderSettings1 {
                AnisotropicFiltering = MyTextureAnisoFiltering.NONE,
                AntialiasingMode = MyAntialiasingMode.NONE,
                ShadowQuality = MyShadowsQuality.LOW,
                AmbientOcclusionEnabled = false,
                TextureQuality = MyTextureQuality.LOW,
                ModelQuality = MyRenderQualityEnum.LOW,
                VoxelQuality = MyRenderQualityEnum.LOW,
                GrassDrawDistance = 50f,
                GrassDensityFactor = 0f,
                HqDepth = true,
                VoxelShaderQuality = MyRenderQualityEnum.LOW,
                AlphaMaskedShaderQuality = MyRenderQualityEnum.LOW,
                AtmosphereShaderQuality = MyRenderQualityEnum.LOW,
                DistanceFade = 500f
            };
            settings.RenderSettings = settings2;
            settings.EnableDamageEffects = false;
            MyPerformanceSettings[] settingsArray1 = new MyPerformanceSettings[3];
            settingsArray1[0] = settings;
            settings = new MyPerformanceSettings();
            settings2 = new MyRenderSettings1 {
                AnisotropicFiltering = MyTextureAnisoFiltering.NONE,
                AntialiasingMode = MyAntialiasingMode.FXAA,
                ShadowQuality = MyShadowsQuality.MEDIUM,
                AmbientOcclusionEnabled = true,
                TextureQuality = MyTextureQuality.MEDIUM,
                ModelQuality = MyRenderQualityEnum.NORMAL,
                VoxelQuality = MyRenderQualityEnum.NORMAL,
                GrassDrawDistance = 160f,
                GrassDensityFactor = 1f,
                HqDepth = true,
                VoxelShaderQuality = MyRenderQualityEnum.NORMAL,
                AlphaMaskedShaderQuality = MyRenderQualityEnum.NORMAL,
                AtmosphereShaderQuality = MyRenderQualityEnum.NORMAL,
                DistanceFade = 1000f
            };
            settings.RenderSettings = settings2;
            settings.EnableDamageEffects = true;
            settingsArray1[1] = settings;
            settings = new MyPerformanceSettings();
            settings2 = new MyRenderSettings1 {
                AnisotropicFiltering = MyTextureAnisoFiltering.ANISO_16,
                AntialiasingMode = MyAntialiasingMode.FXAA,
                ShadowQuality = MyShadowsQuality.HIGH,
                AmbientOcclusionEnabled = true,
                TextureQuality = MyTextureQuality.HIGH,
                ModelQuality = MyRenderQualityEnum.HIGH,
                VoxelQuality = MyRenderQualityEnum.HIGH,
                GrassDrawDistance = 1000f,
                GrassDensityFactor = 3f,
                HqDepth = true,
                VoxelShaderQuality = MyRenderQualityEnum.HIGH,
                AlphaMaskedShaderQuality = MyRenderQualityEnum.HIGH,
                AtmosphereShaderQuality = MyRenderQualityEnum.HIGH,
                DistanceFade = 2000f
            };
            settings.RenderSettings = settings2;
            settings.EnableDamageEffects = true;
            settingsArray1[2] = settings;
            m_presets = settingsArray1;
        }

        public MyGuiScreenOptionsGraphics() : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2(0.6535714f, 0.9379771f), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            base.EnabledBackgroundFade = true;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenOptionsVideo";

        public static MyPerformanceSettings GetPreset(MyRenderQualityEnum adapterQuality) => 
            m_presets[(adapterQuality == MyRenderQualityEnum.LOW) ? 0 : ((adapterQuality == MyRenderQualityEnum.NORMAL) ? 1 : 2)];

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
            MyVideoSettingsManager.Apply(this.m_settingsOld);
            MyVideoSettingsManager.SaveCurrentSettings();
            this.CloseScreen();
        }

        public void OnOkClick(MyGuiControlButton sender)
        {
            if (this.ReadSettingsFromControls(ref this.m_settingsNew))
            {
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MySpaceTexts.MessageBoxTextRestartNeededAfterRendererSwitch), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
            MyVideoSettingsManager.Apply(this.m_settingsNew);
            MyVideoSettingsManager.SaveCurrentSettings();
            this.CloseScreen();
        }

        private void OnPresetSelected()
        {
            PresetEnum selectedKey = (PresetEnum) ((int) this.m_comboGraphicsPresets.GetSelectedKey());
            if (selectedKey != PresetEnum.Custom)
            {
                this.m_settingsNew.PerformanceSettings = m_presets[(int) selectedKey];
                this.WriteSettingsToControls(this.m_settingsNew);
            }
        }

        private void OnSettingsChanged()
        {
            this.m_comboGraphicsPresets.SelectItemByKey(3L, true);
            this.ReadSettingsFromControls(ref this.m_settingsNew);
            this.RefreshPresetCombo(this.m_settingsNew);
        }

        private bool ReadSettingsFromControls(ref MyGraphicsSettings graphicsSettings)
        {
            if (this.m_writingSettings)
            {
                return false;
            }
            MyGraphicsSettings settings2 = new MyGraphicsSettings {
                GraphicsRenderer = graphicsSettings.GraphicsRenderer,
                FieldOfView = MathHelper.ToRadians(this.m_sliderFov.Value),
                PostProcessingEnabled = this.m_checkboxPostProcessing.IsChecked,
                FlaresIntensity = this.m_sliderFlares.Value
            };
            MyPerformanceSettings settings3 = new MyPerformanceSettings {
                EnableDamageEffects = this.m_checkboxEnableDamageEffects.IsChecked,
                RenderSettings = { 
                    AntialiasingMode = (MyAntialiasingMode) ((int) this.m_comboAntialiasing.GetSelectedKey()),
                    AmbientOcclusionEnabled = this.m_checkboxAmbientOcclusionHBAO.IsChecked,
                    ShadowQuality = (MyShadowsQuality) ((int) this.m_comboShadowMapResolution.GetSelectedKey()),
                    TextureQuality = (MyTextureQuality) ((int) this.m_comboTextureQuality.GetSelectedKey()),
                    AnisotropicFiltering = (MyTextureAnisoFiltering) ((int) this.m_comboAnisotropicFiltering.GetSelectedKey()),
                    ModelQuality = (MyRenderQualityEnum) ((int) this.m_comboModelQuality.GetSelectedKey()),
                    VoxelQuality = (MyRenderQualityEnum) ((int) this.m_comboVoxelQuality.GetSelectedKey()),
                    GrassDrawDistance = this.m_grassDrawDistanceSlider.Value,
                    GrassDensityFactor = this.m_grassDensitySlider.Value,
                    VoxelShaderQuality = (MyRenderQualityEnum) ((int) this.m_comboShaderQuality.GetSelectedKey()),
                    AlphaMaskedShaderQuality = (MyRenderQualityEnum) ((int) this.m_comboShaderQuality.GetSelectedKey()),
                    AtmosphereShaderQuality = (MyRenderQualityEnum) ((int) this.m_comboShaderQuality.GetSelectedKey()),
                    HqDepth = true,
                    DistanceFade = this.m_vegetationViewDistance.Value
                }
            };
            settings2.PerformanceSettings = settings3;
            MyGraphicsSettings settings = settings2;
            graphicsSettings = settings;
            return (settings.GraphicsRenderer != graphicsSettings.GraphicsRenderer);
        }

        public override void RecreateControls(bool constructor)
        {
            if (constructor)
            {
                float num6;
                float num7;
                base.RecreateControls(constructor);
                this.m_elementGroup = new MyGuiControlElementGroup();
                this.m_elementGroup.HighlightChanged += new Action<MyGuiControlElementGroup>(this.m_elementGroup_HighlightChanged);
                VRageMath.Vector4? captionTextColor = null;
                base.AddCaption(MyTexts.GetString(MyCommonTexts.ScreenCaptionGraphicsOptions), captionTextColor, new Vector2(0f, 0.003f), 0.8f);
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
                float x = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
                float num2 = 25f;
                float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 0.5f;
                Vector2 vector3 = new Vector2(0f, 0.008f);
                Vector2 vector4 = new Vector2(0f, 0.045f);
                Vector2 vector5 = new Vector2(0.05f, 0f);
                Vector2 vector6 = (((base.m_size.Value / 2f) - vector) * new Vector2(-1f, -1f)) + new Vector2(0f, y);
                Vector2 vector7 = (((base.m_size.Value / 2f) - vector) * new Vector2(1f, -1f)) + new Vector2(0f, y);
                Vector2 vector8 = ((base.m_size.Value / 2f) - vector2) * new Vector2(0f, 1f);
                Vector2 vector9 = new Vector2(vector7.X - (x + 0.0015f), vector7.Y);
                Vector2 vector10 = vector6 + new Vector2(0.255f, 0f);
                Vector2 vector11 = vector9 + new Vector2(0.26f, 0f);
                float num5 = 0f - 0.045f;
                Vector2? position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label1 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_QualityPreset), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label1.Position = (vector6 + (num5 * vector4)) + vector3;
                label1.OriginAlign = enum2;
                MyGuiControlLabel label = label1;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox1 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_QualityPreset), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox1.Position = vector7 + (num5 * vector4);
                combobox1.OriginAlign = enum3;
                this.m_comboGraphicsPresets = combobox1;
                int? sortOrder = null;
                this.m_comboGraphicsPresets.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_QualityPreset_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboGraphicsPresets.AddItem(1L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_QualityPreset_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboGraphicsPresets.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_QualityPreset_High), sortOrder, null);
                sortOrder = null;
                this.m_comboGraphicsPresets.AddItem(3L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_QualityPreset_Custom), sortOrder, null);
                num5++;
                MyGuiControlLabel label2 = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label17 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_ModelQuality), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label17.Position = (vector6 + (num5 * vector4)) + vector3;
                label17.OriginAlign = enum2;
                label2 = label17;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox2 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_ModelQuality), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox2.Position = vector7 + (num5 * vector4);
                combobox2.OriginAlign = enum3;
                this.m_comboModelQuality = combobox2;
                sortOrder = null;
                this.m_comboModelQuality.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboModelQuality.AddItem(1L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboModelQuality.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_High), sortOrder, null);
                sortOrder = null;
                this.m_comboModelQuality.AddItem(3L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Extreme) + " " + MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_PerformanceHeavy), sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label18 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.ScreenGraphicsOptions_ShaderQuality), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label18.Position = (vector6 + (num5 * vector4)) + vector3;
                label18.OriginAlign = enum2;
                MyGuiControlLabel label3 = label18;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox3 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_ShaderQuality), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox3.Position = vector7 + (num5 * vector4);
                combobox3.OriginAlign = enum3;
                this.m_comboShaderQuality = combobox3;
                sortOrder = null;
                this.m_comboShaderQuality.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboShaderQuality.AddItem(1L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboShaderQuality.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_High), sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label19 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.ScreenGraphicsOptions_VoxelQuality), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label19.Position = (vector6 + (num5 * vector4)) + vector3;
                label19.OriginAlign = enum2;
                MyGuiControlLabel label4 = label19;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox4 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_VoxelQuality), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox4.Position = vector7 + (num5 * vector4);
                combobox4.OriginAlign = enum3;
                this.m_comboVoxelQuality = combobox4;
                sortOrder = null;
                this.m_comboVoxelQuality.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboVoxelQuality.AddItem(1L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboVoxelQuality.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_High), sortOrder, null);
                sortOrder = null;
                this.m_comboVoxelQuality.AddItem(3L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_FoliageDetails_Extreme) + " " + MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_PerformanceHeavy), sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label20 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_TextureQuality), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label20.Position = (vector6 + (num5 * vector4)) + vector3;
                label20.OriginAlign = enum2;
                MyGuiControlLabel label5 = label20;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox5 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_TextureQuality), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox5.Position = vector7 + (num5 * vector4);
                combobox5.OriginAlign = enum3;
                this.m_comboTextureQuality = combobox5;
                sortOrder = null;
                this.m_comboTextureQuality.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_TextureQuality_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboTextureQuality.AddItem(1L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_TextureQuality_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboTextureQuality.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_TextureQuality_High), sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label21 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.ScreenGraphicsOptions_ShadowMapResolution), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label21.Position = (vector6 + (num5 * vector4)) + vector3;
                label21.OriginAlign = enum2;
                MyGuiControlLabel label6 = label21;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox6 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_ShadowQuality), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox6.Position = vector7 + (num5 * vector4);
                combobox6.OriginAlign = enum3;
                this.m_comboShadowMapResolution = combobox6;
                sortOrder = null;
                this.m_comboShadowMapResolution.AddItem(3L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_ShadowMapResolution_Disabled), sortOrder, null);
                sortOrder = null;
                this.m_comboShadowMapResolution.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_ShadowMapResolution_Low), sortOrder, null);
                sortOrder = null;
                this.m_comboShadowMapResolution.AddItem(1L, MyTexts.GetString(MySpaceTexts.ScreenGraphicsOptions_ShadowMapResolution_Medium), sortOrder, null);
                sortOrder = null;
                this.m_comboShadowMapResolution.AddItem(2L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_ShadowMapResolution_High), sortOrder, null);
                sortOrder = null;
                this.m_comboShadowMapResolution.AddItem(4L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_ShadowMapResolution_Extreme) + " " + MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_PerformanceHeavy), sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label22 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_AntiAliasing), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label22.Position = (vector6 + (num5 * vector4)) + vector3;
                label22.OriginAlign = enum2;
                MyGuiControlLabel label7 = label22;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox7 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_Antialiasing), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox7.Position = vector7 + (num5 * vector4);
                combobox7.OriginAlign = enum3;
                this.m_comboAntialiasing = combobox7;
                sortOrder = null;
                this.m_comboAntialiasing.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_AntiAliasing_None), sortOrder, null);
                sortOrder = null;
                this.m_comboAntialiasing.AddItem(1L, "FXAA", sortOrder, null);
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label23 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_AnisotropicFiltering), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label23.Position = (vector6 + (num5 * vector4)) + vector3;
                label23.OriginAlign = enum2;
                MyGuiControlLabel label8 = label23;
                position = null;
                position = null;
                captionTextColor = null;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlCombobox combobox8 = new MyGuiControlCombobox(position, position, captionTextColor, position, 10, position, false, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_AnisotropicFiltering), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, captionTextColor);
                combobox8.Position = vector7 + (num5 * vector4);
                combobox8.OriginAlign = enum3;
                this.m_comboAnisotropicFiltering = combobox8;
                sortOrder = null;
                this.m_comboAnisotropicFiltering.AddItem(0L, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_AnisotropicFiltering_Off), sortOrder, null);
                sortOrder = null;
                this.m_comboAnisotropicFiltering.AddItem(1L, "1x", sortOrder, null);
                sortOrder = null;
                this.m_comboAnisotropicFiltering.AddItem(2L, "4x", sortOrder, null);
                sortOrder = null;
                this.m_comboAnisotropicFiltering.AddItem(3L, "8x", sortOrder, null);
                sortOrder = null;
                this.m_comboAnisotropicFiltering.AddItem(4L, "16x", sortOrder, null);
                num5 += 1.05f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label24 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.FieldOfView), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label24.Position = (vector6 + (num5 * vector4)) + vector3;
                label24.OriginAlign = enum2;
                MyGuiControlLabel label9 = label24;
                MyVideoSettingsManager.GetFovBounds(out num6, out num7);
                if (!MySandboxGame.Config.ExperimentalMode)
                {
                    num7 = Math.Min(num7, MyConstants.FIELD_OF_VIEW_CONFIG_MAX_SAFE);
                }
                position = null;
                string toolTip = MyTexts.GetString(MyCommonTexts.ToolTipVideoOptionsFieldOfView);
                captionTextColor = null;
                MyGuiControlSlider slider1 = new MyGuiControlSlider(position, MathHelper.ToDegrees(num6), MathHelper.ToDegrees(num7), 0.29f, new float?(MathHelper.ToDegrees(MySandboxGame.Config.FieldOfView)), captionTextColor, new StringBuilder("{0}").ToString(), 1, 0.8f, 0.07f, "Blue", toolTip, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                slider1.Position = vector7 + (num5 * vector4);
                slider1.OriginAlign = enum3;
                slider1.Size = new Vector2(x, 0f);
                this.m_sliderFov = slider1;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_FOV));
                stringBuilder.Append(" ");
                stringBuilder.AppendFormat(MyCommonTexts.DefaultFOV, MathHelper.ToDegrees(MyConstants.FIELD_OF_VIEW_CONFIG_DEFAULT));
                this.m_sliderFov.SetToolTip(stringBuilder.ToString());
                num5 += 1.1f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label25 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.FlaresIntensity), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label25.Position = (vector6 + (num5 * vector4)) + vector3;
                label25.OriginAlign = enum2;
                MyGuiControlLabel label10 = label25;
                position = null;
                string str2 = MyTexts.GetString(MySpaceTexts.ToolTipFlaresIntensity);
                captionTextColor = null;
                MyGuiControlSlider slider2 = new MyGuiControlSlider(position, 0.1f, 2f, 0.29f, new float?(MySandboxGame.Config.FlaresIntensity), captionTextColor, new StringBuilder("{0}").ToString(), 1, 0.8f, 0.07f, "Blue", str2, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                slider2.Position = vector7 + (num5 * vector4);
                slider2.OriginAlign = enum3;
                slider2.Size = new Vector2(x, 0f);
                this.m_sliderFlares = slider2;
                num5 += 1.1f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label26 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.WorldSettings_GrassDrawDistance), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label26.Position = (vector6 + (num5 * vector4)) + vector3;
                label26.OriginAlign = enum2;
                MyGuiControlLabel label11 = label26;
                position = null;
                float? defaultRatio = null;
                captionTextColor = null;
                MyGuiControlSliderBase base1 = new MyGuiControlSliderBase(position, 0.29f, new MyGuiSliderPropertiesExponential(50f, 5000f, 10f, true), defaultRatio, captionTextColor, 0.8f, 0.07f, "Blue", MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_GrassDrawDistance), MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true);
                base1.Position = vector7 + (num5 * vector4);
                base1.OriginAlign = enum3;
                base1.Size = new Vector2(x, 0f);
                this.m_grassDrawDistanceSlider = base1;
                num5 += 1.1f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label27 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.WorldSettings_GrassDensity), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label27.Position = (vector6 + (num5 * vector4)) + vector3;
                label27.OriginAlign = enum2;
                MyGuiControlLabel label12 = label27;
                position = null;
                str2 = MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_GrassDensity);
                captionTextColor = null;
                MyGuiControlSlider slider3 = new MyGuiControlSlider(position, 0f, 10f, 0.29f, MySandboxGame.Config.GrassDensityFactor, captionTextColor, new StringBuilder("{0}").ToString(), 1, 0.8f, 0.07f, "Blue", str2, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, true);
                slider3.Position = vector7 + (num5 * vector4);
                slider3.OriginAlign = enum3;
                slider3.Size = new Vector2(x, 0f);
                this.m_grassDensitySlider = slider3;
                this.m_grassDensitySlider.SetBounds(0f, 10f);
                num5 += 1.1f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label28 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.WorldSettings_VegetationDistance), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label28.Position = (vector6 + (num5 * vector4)) + vector3;
                label28.OriginAlign = enum2;
                MyGuiControlLabel label13 = label28;
                position = null;
                defaultRatio = null;
                captionTextColor = null;
                MyGuiControlSliderBase base2 = new MyGuiControlSliderBase(position, 0.29f, new MyGuiSliderPropertiesExponential(500f, 10000f, 10f, true), defaultRatio, captionTextColor, 0.8f, 0.07f, "Blue", MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_TreeDrawDistance), MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true);
                base2.Position = vector7 + (num5 * vector4);
                base2.OriginAlign = enum3;
                base2.Size = new Vector2(x, 0f);
                this.m_vegetationViewDistance = base2;
                num5 += 1.1f;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label29 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_AmbientOcclusion), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label29.Position = (vector6 + (num5 * vector4)) + vector3;
                label29.OriginAlign = enum2;
                MyGuiControlLabel label14 = label29;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_AmbientOcclusion), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox1.Position = vector10 + (num5 * vector4);
                checkbox1.OriginAlign = enum2;
                this.m_checkboxAmbientOcclusionHBAO = checkbox1;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label30 = new MyGuiControlLabel(position, position, MyTexts.GetString(MySpaceTexts.EnableDamageEffects), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label30.Position = ((vector9 + vector5) + (num5 * vector4)) + vector3;
                label30.OriginAlign = enum2;
                MyGuiControlLabel label15 = label30;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox2 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipVideoOptionsEnableDamageEffects), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox2.Position = vector11 + (num5 * vector4);
                checkbox2.OriginAlign = enum2;
                this.m_checkboxEnableDamageEffects = checkbox2;
                num5++;
                position = null;
                position = null;
                captionTextColor = null;
                MyGuiControlLabel label31 = new MyGuiControlLabel(position, position, MyTexts.GetString(MyCommonTexts.ScreenGraphicsOptions_PostProcessing), captionTextColor, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                label31.Position = (vector6 + (num5 * vector4)) + vector3;
                label31.OriginAlign = enum2;
                MyGuiControlLabel label16 = label31;
                position = null;
                captionTextColor = null;
                MyGuiControlCheckbox checkbox3 = new MyGuiControlCheckbox(position, captionTextColor, MyTexts.GetString(MySpaceTexts.ToolTipOptionsGraphics_PostProcessing), false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox3.Position = vector10 + (num5 * vector4);
                checkbox3.OriginAlign = enum2;
                checkbox3.IsChecked = MySandboxGame.Config.PostProcessingEnabled;
                this.m_checkboxPostProcessing = checkbox3;
                position = null;
                position = null;
                captionTextColor = null;
                sortOrder = null;
                MyGuiControlButton button = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnOkClick), GuiSounds.MouseClick, 1f, sortOrder, false);
                button.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
                position = null;
                position = null;
                captionTextColor = null;
                sortOrder = null;
                MyGuiControlButton button2 = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, captionTextColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancelClick), GuiSounds.MouseClick, 1f, sortOrder, false);
                button2.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
                button.Position = vector8 + (new Vector2(-num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
                button2.Position = vector8 + (new Vector2(num2, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
                button2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
                this.Controls.Add(label10);
                this.Controls.Add(this.m_sliderFlares);
                this.Controls.Add(label9);
                this.Controls.Add(this.m_sliderFov);
                if (MyVideoSettingsManager.RunningGraphicsRenderer == MySandboxGame.DirectX11RendererKey)
                {
                    this.Controls.Add(label);
                    this.Controls.Add(this.m_comboGraphicsPresets);
                    this.Controls.Add(label7);
                    this.Controls.Add(this.m_comboAntialiasing);
                    this.Controls.Add(label6);
                    this.Controls.Add(this.m_comboShadowMapResolution);
                    this.Controls.Add(label5);
                    this.Controls.Add(this.m_comboTextureQuality);
                    this.Controls.Add(label2);
                    this.Controls.Add(this.m_comboModelQuality);
                    this.Controls.Add(label3);
                    this.Controls.Add(this.m_comboShaderQuality);
                    this.Controls.Add(label4);
                    this.Controls.Add(this.m_comboVoxelQuality);
                    this.Controls.Add(label8);
                    this.Controls.Add(this.m_comboAnisotropicFiltering);
                    if (MyFakes.ENABLE_PLANETS)
                    {
                        this.Controls.Add(label11);
                        this.Controls.Add(this.m_grassDrawDistanceSlider);
                        this.Controls.Add(label12);
                        this.Controls.Add(this.m_grassDensitySlider);
                        this.Controls.Add(label13);
                        this.Controls.Add(this.m_vegetationViewDistance);
                    }
                    this.Controls.Add(label15);
                    this.Controls.Add(this.m_checkboxEnableDamageEffects);
                    this.Controls.Add(label14);
                    this.Controls.Add(this.m_checkboxAmbientOcclusionHBAO);
                    this.Controls.Add(label16);
                    this.Controls.Add(this.m_checkboxPostProcessing);
                }
                this.Controls.Add(button);
                this.m_elementGroup.Add(button);
                this.Controls.Add(button2);
                this.m_elementGroup.Add(button2);
                this.m_settingsOld = MyVideoSettingsManager.CurrentGraphicsSettings;
                this.m_settingsNew = this.m_settingsOld;
                this.WriteSettingsToControls(this.m_settingsOld);
                this.ReadSettingsFromControls(ref this.m_settingsOld);
                this.ReadSettingsFromControls(ref this.m_settingsNew);
                MyGuiControlCombobox.ItemSelectedDelegate delegate2 = new MyGuiControlCombobox.ItemSelectedDelegate(this.OnSettingsChanged);
                Action<MyGuiControlCheckbox> b = checkbox => this.OnSettingsChanged();
                this.m_comboGraphicsPresets.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnPresetSelected);
                this.m_comboAnisotropicFiltering.ItemSelected += delegate2;
                this.m_comboAntialiasing.ItemSelected += delegate2;
                this.m_comboShadowMapResolution.ItemSelected += delegate2;
                this.m_checkboxAmbientOcclusionHBAO.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkboxAmbientOcclusionHBAO.IsCheckedChanged, b);
                this.m_comboVoxelQuality.ItemSelected += delegate2;
                this.m_comboModelQuality.ItemSelected += delegate2;
                this.m_comboTextureQuality.ItemSelected += delegate2;
                this.m_comboShaderQuality.ItemSelected += delegate2;
                this.m_sliderFlares.ValueChanged = slider => this.OnSettingsChanged();
                this.m_checkboxEnableDamageEffects.IsCheckedChanged = b;
                this.m_sliderFov.ValueChanged = slider => this.OnSettingsChanged();
                this.m_checkboxPostProcessing.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_checkboxPostProcessing.IsCheckedChanged, b);
                this.RefreshPresetCombo(this.m_settingsOld);
                base.FocusedControl = button;
                base.CloseButtonEnabled = true;
            }
        }

        private void RefreshPresetCombo(MyGraphicsSettings settings)
        {
            int index = 0;
            while (true)
            {
                if (index < m_presets.Length)
                {
                    MyPerformanceSettings settings2 = m_presets[index];
                    if (!settings2.Equals(settings.PerformanceSettings))
                    {
                        index++;
                        continue;
                    }
                }
                this.m_comboGraphicsPresets.SelectItemByKey((long) index, false);
                return;
            }
        }

        private void WriteSettingsToControls(MyGraphicsSettings graphicsSettings)
        {
            this.m_writingSettings = true;
            this.m_sliderFlares.Value = graphicsSettings.FlaresIntensity;
            this.m_sliderFov.Value = MathHelper.ToDegrees(graphicsSettings.FieldOfView);
            this.m_checkboxPostProcessing.IsChecked = graphicsSettings.PostProcessingEnabled;
            this.m_comboModelQuality.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.ModelQuality, false);
            this.m_comboVoxelQuality.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality, false);
            this.m_grassDrawDistanceSlider.Value = graphicsSettings.PerformanceSettings.RenderSettings.GrassDrawDistance;
            this.m_grassDensitySlider.Value = graphicsSettings.PerformanceSettings.RenderSettings.GrassDensityFactor;
            this.m_vegetationViewDistance.Value = graphicsSettings.PerformanceSettings.RenderSettings.DistanceFade;
            this.m_checkboxEnableDamageEffects.IsChecked = graphicsSettings.PerformanceSettings.EnableDamageEffects;
            this.m_comboAntialiasing.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.AntialiasingMode, false);
            this.m_checkboxAmbientOcclusionHBAO.IsChecked = graphicsSettings.PerformanceSettings.RenderSettings.AmbientOcclusionEnabled;
            this.m_comboShadowMapResolution.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.ShadowQuality, false);
            this.m_comboTextureQuality.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.TextureQuality, false);
            this.m_comboShaderQuality.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.VoxelShaderQuality, false);
            this.m_comboAnisotropicFiltering.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.AnisotropicFiltering, false);
            this.m_comboShaderQuality.SelectItemByKey((long) graphicsSettings.PerformanceSettings.RenderSettings.VoxelShaderQuality, false);
            this.m_writingSettings = false;
        }

        private enum PresetEnum
        {
            Low,
            Medium,
            High,
            Custom
        }
    }
}

