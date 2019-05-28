namespace Sandbox.Engine.Utils
{
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Gui;
    using VRage.Serialization;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyConfig : MyConfigBase, IMyConfig
    {
        private readonly string DX9_RENDER_QUALITY;
        private readonly string MODEL_QUALITY;
        private readonly string VOXEL_QUALITY;
        private readonly string FIELD_OF_VIEW;
        private readonly string ENABLE_DAMAGE_EFFECTS;
        private readonly string SCREEN_WIDTH;
        private readonly string SCREEN_HEIGHT;
        private readonly string FULL_SCREEN;
        private readonly string VIDEO_ADAPTER;
        private readonly string DISABLE_UPDATE_DRIVER_NOTIFICATION;
        private readonly string VERTICAL_SYNC;
        private readonly string REFRESH_RATE;
        private readonly string FLARES_INTENSITY;
        private readonly string GAME_VOLUME;
        private readonly string MUSIC_VOLUME_OLD;
        private readonly string MUSIC_VOLUME;
        private readonly string VOICE_CHAT_VOLUME;
        private readonly string LANGUAGE;
        private readonly string SKIN;
        private readonly string EXPERIMENTAL_MODE;
        private readonly string CONTROLS_HINTS;
        private readonly string GOODBOT_HINTS;
        private readonly string ROTATION_HINTS;
        private readonly string SHOW_CROSSHAIR;
        private readonly string ENABLE_STEAM_CLOUD;
        private readonly string CONTROLS_GENERAL;
        private readonly string CONTROLS_BUTTONS;
        private readonly string SCREENSHOT_SIZE_MULTIPLIER;
        private readonly string FIRST_TIME_RUN;
        private readonly string FIRST_VT_TIME_RUN;
        private readonly string FIRST_TIME_TUTORIALS;
        private readonly string SYNC_RENDERING;
        private readonly string NEED_SHOW_BATTLE_TUTORIAL_QUESTION;
        private readonly string DEBUG_INPUT_COMPONENTS;
        private readonly string DEBUG_INPUT_COMPONENTS_INFO;
        private readonly string MINIMAL_HUD;
        private readonly string HUD_STATE;
        private readonly string MEMORY_LIMITS;
        private readonly string CUBE_BUILDER_USE_SYMMETRY;
        private readonly string CUBE_BUILDER_BUILDING_MODE;
        private readonly string CUBE_BUILDER_ALIGN_TO_DEFAULT;
        private readonly string MULTIPLAYER_SHOWCOMPATIBLE;
        private readonly string COMPRESS_SAVE_GAMES;
        private readonly string SHOW_PLAYER_NAMES_ON_HUD;
        private readonly string RELEASING_ALT_RESETS_CAMERA;
        private readonly string ENABLE_PERFORMANCE_WARNINGS_TEMP;
        private readonly string LAST_CHECKED_VERSION;
        private readonly string WINDOW_MODE;
        private readonly string MOUSE_CAPTURE;
        private readonly string HUD_WARNINGS;
        private readonly string DYNAMIC_MUSIC;
        private readonly string SHIP_SOUNDS_SPEED;
        private readonly string HQTARGET;
        private readonly string ANTIALIASING_MODE;
        private readonly string SHADOW_MAP_RESOLUTION;
        private readonly string AMBIENT_OCCLUSION_ENABLED;
        private readonly string POSTPROCESSING_ENABLED;
        private readonly string MULTITHREADED_RENDERING;
        private readonly string TEXTURE_QUALITY;
        private readonly string SHADER_QUALITY;
        private readonly string ANISOTROPIC_FILTERING;
        private readonly string FOLIAGE_DETAILS;
        private readonly string GRASS_DENSITY;
        private readonly string GRASS_DRAW_DISTANCE;
        private readonly string VEGETATION_DISTANCE;
        private readonly string GRAPHICS_RENDERER;
        private readonly string ENABLE_VOICE_CHAT;
        private readonly string ENABLE_MUTE_WHEN_NOT_IN_FOCUS;
        private readonly string ENABLE_REVERB;
        private readonly string UI_TRANSPARENCY;
        private readonly string UI_BK_TRANSPARENCY;
        private readonly string HUD_BK_TRANSPARENCY;
        private readonly string TUTORIALS_FINISHED;
        private readonly string MUTED_PLAYERS;
        private readonly string DONT_SEND_VOICE_PLAYERS;
        private readonly string LOW_MEM_SWITCH_TO_LOW;
        private readonly string NEWSLETTER_CURRENT_STATUS;
        private readonly string SERVER_SEARCH_SETTINGS;
        private readonly string ENABLE_DOPPLER;
        private readonly string WELCOMESCREEN_CURRENT_STATUS;
        private readonly string DEBUG_OVERRIDE_AUTOSAVE;
        private readonly string GDPR_CONSENT;
        private readonly string GDPR_CONSENT_SENT;
        private readonly string SHOW_CHAT_TIMESTAMP;
        private const char m_numberSeparator = ',';
        private HashSet<ulong> m_mutedPlayers;
        private bool m_mutedPlayersInited;
        private HashSet<ulong> m_dontSendVoicePlayers;
        private bool m_dontSendVoicePlayersInited;

        public MyConfig(string fileName) : base(fileName)
        {
            this.DX9_RENDER_QUALITY = "RenderQuality";
            this.MODEL_QUALITY = "ModelQuality";
            this.VOXEL_QUALITY = "VoxelQuality";
            this.FIELD_OF_VIEW = "FieldOfView";
            this.ENABLE_DAMAGE_EFFECTS = "EnableDamageEffects";
            this.SCREEN_WIDTH = "ScreenWidth";
            this.SCREEN_HEIGHT = "ScreenHeight";
            this.FULL_SCREEN = "FullScreen";
            this.VIDEO_ADAPTER = "VideoAdapter";
            this.DISABLE_UPDATE_DRIVER_NOTIFICATION = "DisableUpdateDriverNotification";
            this.VERTICAL_SYNC = "VerticalSync";
            this.REFRESH_RATE = "RefreshRate";
            this.FLARES_INTENSITY = "FlaresIntensity";
            this.GAME_VOLUME = "GameVolume";
            this.MUSIC_VOLUME_OLD = "MusicVolume";
            this.MUSIC_VOLUME = "Music_Volume";
            this.VOICE_CHAT_VOLUME = "VoiceChatVolume";
            this.LANGUAGE = "Language";
            this.SKIN = "Skin";
            this.EXPERIMENTAL_MODE = "ExperimentalMode";
            this.CONTROLS_HINTS = "ControlsHints";
            this.GOODBOT_HINTS = "GoodBotHints";
            this.ROTATION_HINTS = "RotationHints";
            this.SHOW_CROSSHAIR = "ShowCrosshair";
            this.ENABLE_STEAM_CLOUD = "EnableSteamCloud";
            this.CONTROLS_GENERAL = "ControlsGeneral";
            this.CONTROLS_BUTTONS = "ControlsButtons";
            this.SCREENSHOT_SIZE_MULTIPLIER = "ScreenshotSizeMultiplier";
            this.FIRST_TIME_RUN = "FirstTimeRun";
            this.FIRST_VT_TIME_RUN = "FirstVTTimeRun";
            this.FIRST_TIME_TUTORIALS = "FirstTimeTutorials";
            this.SYNC_RENDERING = "SyncRendering";
            this.NEED_SHOW_BATTLE_TUTORIAL_QUESTION = "NeedShowBattleTutorialQuestion";
            this.DEBUG_INPUT_COMPONENTS = "DebugInputs";
            this.DEBUG_INPUT_COMPONENTS_INFO = "DebugComponentsInfo";
            this.MINIMAL_HUD = "MinimalHud";
            this.HUD_STATE = "HudState";
            this.MEMORY_LIMITS = "MemoryLimits";
            this.CUBE_BUILDER_USE_SYMMETRY = "CubeBuilderUseSymmetry";
            this.CUBE_BUILDER_BUILDING_MODE = "CubeBuilderBuildingMode";
            this.CUBE_BUILDER_ALIGN_TO_DEFAULT = "CubeBuilderAlignToDefault";
            this.MULTIPLAYER_SHOWCOMPATIBLE = "MultiplayerShowCompatible";
            this.COMPRESS_SAVE_GAMES = "CompressSaveGames";
            this.SHOW_PLAYER_NAMES_ON_HUD = "ShowPlayerNamesOnHud";
            this.RELEASING_ALT_RESETS_CAMERA = "ReleasingAltResetsCamera";
            this.ENABLE_PERFORMANCE_WARNINGS_TEMP = "EnablePerformanceWarningsTempV2";
            this.LAST_CHECKED_VERSION = "LastCheckedVersion";
            this.WINDOW_MODE = "WindowMode";
            this.MOUSE_CAPTURE = "CaptureMouse";
            this.HUD_WARNINGS = "HudWarnings";
            this.DYNAMIC_MUSIC = "EnableDynamicMusic";
            this.SHIP_SOUNDS_SPEED = "ShipSoundsAreBasedOnSpeed";
            this.HQTARGET = "HQTarget";
            this.ANTIALIASING_MODE = "AntialiasingMode";
            this.SHADOW_MAP_RESOLUTION = "ShadowMapResolution";
            this.AMBIENT_OCCLUSION_ENABLED = "AmbientOcclusionEnabled";
            this.POSTPROCESSING_ENABLED = "PostProcessingEnabled";
            this.MULTITHREADED_RENDERING = "MultithreadedRendering";
            this.TEXTURE_QUALITY = "TextureQuality";
            this.SHADER_QUALITY = "ShaderQuality";
            this.ANISOTROPIC_FILTERING = "AnisotropicFiltering";
            this.FOLIAGE_DETAILS = "FoliageDetails";
            this.GRASS_DENSITY = "GrassDensity";
            this.GRASS_DRAW_DISTANCE = "GrassDrawDistance";
            this.VEGETATION_DISTANCE = "TreeViewDistance";
            this.GRAPHICS_RENDERER = "GraphicsRenderer";
            this.ENABLE_VOICE_CHAT = "VoiceChat";
            this.ENABLE_MUTE_WHEN_NOT_IN_FOCUS = "EnableMuteWhenNotInFocus";
            this.ENABLE_REVERB = "EnableReverb";
            this.UI_TRANSPARENCY = "UiTransparency";
            this.UI_BK_TRANSPARENCY = "UiBkTransparency";
            this.HUD_BK_TRANSPARENCY = "HUDBkTransparency";
            this.TUTORIALS_FINISHED = "TutorialsFinished";
            this.MUTED_PLAYERS = "MutedPlayers";
            this.DONT_SEND_VOICE_PLAYERS = "DontSendVoicePlayers";
            this.LOW_MEM_SWITCH_TO_LOW = "LowMemSwitchToLow";
            this.NEWSLETTER_CURRENT_STATUS = "NewsletterCurrentStatus";
            this.SERVER_SEARCH_SETTINGS = "ServerSearchSettings";
            this.ENABLE_DOPPLER = "EnableDoppler";
            this.WELCOMESCREEN_CURRENT_STATUS = "WelcomeScreenCurrentStatus";
            this.DEBUG_OVERRIDE_AUTOSAVE = "DebugOverrideAutosave";
            this.GDPR_CONSENT = "GDPRConsent";
            this.GDPR_CONSENT_SENT = "GDPRConsentSent";
            this.SHOW_CHAT_TIMESTAMP = "ShowChatTimestamp";
            this.m_mutedPlayers = new HashSet<ulong>();
            this.m_dontSendVoicePlayers = new HashSet<ulong>();
        }

        private static object Decode64AndDeserialize(string p)
        {
            if ((p == null) || (p.Length == 0))
            {
                return null;
            }
            return new BinaryFormatter().Deserialize(new MemoryStream(Convert.FromBase64String(p)));
        }

        private HashSet<ulong> GetSeparatedValues(string key, ref HashSet<ulong> cache, ref bool cacheInitedFlag)
        {
            if (cacheInitedFlag)
            {
                return cache;
            }
            string parameterValue = "";
            if (!base.m_values.Dictionary.ContainsKey(key))
            {
                base.m_values.Dictionary.Add(key, "");
            }
            else
            {
                parameterValue = base.GetParameterValue(key);
            }
            HashSet<ulong> set = new HashSet<ulong>();
            char[] separator = new char[] { ',' };
            foreach (string str2 in parameterValue.Split(separator))
            {
                if (str2.Length > 0)
                {
                    set.Add(Convert.ToUInt64(str2));
                }
            }
            cache = set;
            cacheInitedFlag = true;
            return set;
        }

        public bool IsSetToLowQuality()
        {
            MyTextureAnisoFiltering? anisotropicFiltering = this.AnisotropicFiltering;
            MyTextureAnisoFiltering nONE = MyTextureAnisoFiltering.NONE;
            if ((((MyTextureAnisoFiltering) anisotropicFiltering.GetValueOrDefault()) == nONE) & (anisotropicFiltering != null))
            {
                MyAntialiasingMode? antialiasingMode = this.AntialiasingMode;
                MyAntialiasingMode mode = MyAntialiasingMode.NONE;
                if ((((MyAntialiasingMode) antialiasingMode.GetValueOrDefault()) == mode) & (antialiasingMode != null))
                {
                    MyShadowsQuality? shadowQuality = this.ShadowQuality;
                    MyShadowsQuality lOW = MyShadowsQuality.LOW;
                    if ((((MyShadowsQuality) shadowQuality.GetValueOrDefault()) == lOW) & (shadowQuality != null))
                    {
                        MyTextureQuality? textureQuality = this.TextureQuality;
                        MyTextureQuality quality2 = MyTextureQuality.LOW;
                        if ((((MyTextureQuality) textureQuality.GetValueOrDefault()) == quality2) & (textureQuality != null))
                        {
                            MyRenderQualityEnum? modelQuality = this.ModelQuality;
                            MyRenderQualityEnum enum2 = MyRenderQualityEnum.LOW;
                            if ((((MyRenderQualityEnum) modelQuality.GetValueOrDefault()) == enum2) & (modelQuality != null))
                            {
                                modelQuality = this.VoxelQuality;
                                enum2 = MyRenderQualityEnum.LOW;
                                if ((((MyRenderQualityEnum) modelQuality.GetValueOrDefault()) == enum2) & (modelQuality != null))
                                {
                                    float? grassDrawDistance = this.GrassDrawDistance;
                                    float num = 0f;
                                    return ((grassDrawDistance.GetValueOrDefault() == num) & (grassDrawDistance != null));
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static string SerialiazeAndEncod64(object p)
        {
            if (p == null)
            {
                return "";
            }
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, p);
            return Convert.ToBase64String(serializationStream.GetBuffer());
        }

        private void SetSeparatedValues(string key, HashSet<ulong> value, ref HashSet<ulong> cache, ref bool cacheInitedFlag)
        {
            cache = value;
            string str = "";
            foreach (ulong num in value)
            {
                str = str + num.ToString() + ",";
            }
            base.SetParameterValue(key, str);
        }

        public void SetToLowQuality()
        {
            this.AnisotropicFiltering = 0;
            this.AntialiasingMode = 0;
            this.ShadowQuality = 0;
            this.TextureQuality = 0;
            this.ModelQuality = 0;
            this.VoxelQuality = 0;
            this.GrassDrawDistance = 0f;
        }

        internal void SetToMediumQuality()
        {
            MyPerformanceSettings settings3 = new MyPerformanceSettings();
            MyRenderSettings1 settings4 = new MyRenderSettings1 {
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
                DistanceFade = 100f
            };
            settings3.RenderSettings = settings4;
            settings3.EnableDamageEffects = true;
            MyGraphicsSettings currentGraphicsSettings = MyVideoSettingsManager.CurrentGraphicsSettings;
            currentGraphicsSettings.PerformanceSettings = settings3;
            MyVideoSettingsManager.Apply(currentGraphicsSettings);
            MyVideoSettingsManager.SaveCurrentSettings();
        }

        public bool FirstTimeRun
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.FIRST_TIME_RUN), true);
            set => 
                base.SetParameterValue(this.FIRST_TIME_RUN, new bool?(value));
        }

        public bool FirstVTTimeRun
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.FIRST_VT_TIME_RUN), true);
            set => 
                base.SetParameterValue(this.FIRST_VT_TIME_RUN, new bool?(value));
        }

        public bool FirstTimeTutorials
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.FIRST_TIME_TUTORIALS), true);
            set => 
                base.SetParameterValue(this.FIRST_TIME_TUTORIALS, new bool?(value));
        }

        public bool SyncRendering
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.SYNC_RENDERING), false);
            set => 
                base.SetParameterValue(this.SYNC_RENDERING, new bool?(value));
        }

        public bool NeedShowBattleTutorialQuestion
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.NEED_SHOW_BATTLE_TUTORIAL_QUESTION), true);
            set => 
                base.SetParameterValue(this.NEED_SHOW_BATTLE_TUTORIAL_QUESTION, new bool?(value));
        }

        public MyRenderQualityEnum? ModelQuality
        {
            get => 
                base.GetOptionalEnum<MyRenderQualityEnum>(this.MODEL_QUALITY);
            set => 
                base.SetOptionalEnum<MyRenderQualityEnum>(this.MODEL_QUALITY, value);
        }

        public MyRenderQualityEnum? VoxelQuality
        {
            get => 
                base.GetOptionalEnum<MyRenderQualityEnum>(this.VOXEL_QUALITY);
            set => 
                base.SetOptionalEnum<MyRenderQualityEnum>(this.VOXEL_QUALITY, value);
        }

        public float? GrassDensityFactor
        {
            get
            {
                float floatFromString = MyUtils.GetFloatFromString(base.GetParameterValue(this.GRASS_DENSITY), -1f);
                if (floatFromString >= 0f)
                {
                    return new float?(floatFromString);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    base.SetParameterValue(this.GRASS_DENSITY, value.Value);
                }
                else
                {
                    base.m_values.Dictionary.Remove(this.GRASS_DENSITY);
                }
            }
        }

        public float? GrassDrawDistance
        {
            get
            {
                float floatFromString = MyUtils.GetFloatFromString(base.GetParameterValue(this.GRASS_DRAW_DISTANCE), -1f);
                if (floatFromString >= 0f)
                {
                    return new float?(floatFromString);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    base.SetParameterValue(this.GRASS_DRAW_DISTANCE, value.Value);
                }
                else
                {
                    base.m_values.Dictionary.Remove(this.GRASS_DRAW_DISTANCE);
                }
            }
        }

        public float? VegetationDrawDistance
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.VEGETATION_DISTANCE));
            set
            {
                if (value != null)
                {
                    base.SetParameterValue(this.VEGETATION_DISTANCE, value.Value);
                }
                else
                {
                    base.m_values.Dictionary.Remove(this.VEGETATION_DISTANCE);
                }
            }
        }

        public float FieldOfView
        {
            get
            {
                float? floatFromString = MyUtils.GetFloatFromString(base.GetParameterValue(this.FIELD_OF_VIEW));
                return ((floatFromString == null) ? MyConstants.FIELD_OF_VIEW_CONFIG_DEFAULT : MathHelper.ToRadians(floatFromString.Value));
            }
            set => 
                base.SetParameterValue(this.FIELD_OF_VIEW, MathHelper.ToDegrees(value));
        }

        public bool? HqTarget
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.HQTARGET));
            set => 
                base.SetParameterValue(this.HQTARGET, value);
        }

        public MyAntialiasingMode? AntialiasingMode
        {
            get => 
                base.GetOptionalEnum<MyAntialiasingMode>(this.ANTIALIASING_MODE);
            set => 
                base.SetOptionalEnum<MyAntialiasingMode>(this.ANTIALIASING_MODE, value);
        }

        public MyShadowsQuality? ShadowQuality
        {
            get => 
                base.GetOptionalEnum<MyShadowsQuality>(this.SHADOW_MAP_RESOLUTION);
            set => 
                base.SetOptionalEnum<MyShadowsQuality>(this.SHADOW_MAP_RESOLUTION, value);
        }

        public bool? AmbientOcclusionEnabled
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.AMBIENT_OCCLUSION_ENABLED));
            set => 
                base.SetParameterValue(this.AMBIENT_OCCLUSION_ENABLED, value);
        }

        public bool PostProcessingEnabled
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.POSTPROCESSING_ENABLED), true);
            set => 
                base.SetParameterValue(this.POSTPROCESSING_ENABLED, new bool?(value));
        }

        public MyTextureQuality? TextureQuality
        {
            get => 
                base.GetOptionalEnum<MyTextureQuality>(this.TEXTURE_QUALITY);
            set => 
                base.SetOptionalEnum<MyTextureQuality>(this.TEXTURE_QUALITY, value);
        }

        public MyRenderQualityEnum? ShaderQuality
        {
            get => 
                base.GetOptionalEnum<MyRenderQualityEnum>(this.SHADER_QUALITY);
            set => 
                base.SetOptionalEnum<MyRenderQualityEnum>(this.SHADER_QUALITY, value);
        }

        public MyTextureAnisoFiltering? AnisotropicFiltering
        {
            get => 
                base.GetOptionalEnum<MyTextureAnisoFiltering>(this.ANISOTROPIC_FILTERING);
            set => 
                base.SetOptionalEnum<MyTextureAnisoFiltering>(this.ANISOTROPIC_FILTERING, value);
        }

        public int? ScreenWidth
        {
            get => 
                MyUtils.GetInt32FromString(base.GetParameterValue(this.SCREEN_WIDTH));
            set => 
                base.SetParameterValue(this.SCREEN_WIDTH, value);
        }

        public int? ScreenHeight
        {
            get => 
                MyUtils.GetInt32FromString(base.GetParameterValue(this.SCREEN_HEIGHT));
            set => 
                base.SetParameterValue(this.SCREEN_HEIGHT, value);
        }

        public int VideoAdapter
        {
            get => 
                MyUtils.GetIntFromString(base.GetParameterValue(this.VIDEO_ADAPTER), 0);
            set => 
                base.SetParameterValue(this.VIDEO_ADAPTER, value);
        }

        public bool DisableUpdateDriverNotification
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.DISABLE_UPDATE_DRIVER_NOTIFICATION), false);
            set => 
                base.SetParameterValue(this.DISABLE_UPDATE_DRIVER_NOTIFICATION, new bool?(value));
        }

        public MyWindowModeEnum WindowMode
        {
            get
            {
                string parameterValue = base.GetParameterValue(this.WINDOW_MODE);
                byte? byteFromString = null;
                if (!string.IsNullOrEmpty(parameterValue))
                {
                    byteFromString = MyUtils.GetByteFromString(parameterValue);
                }
                else
                {
                    bool? boolFromString = MyUtils.GetBoolFromString(base.GetParameterValue(this.FULL_SCREEN));
                    if (boolFromString != null)
                    {
                        base.RemoveParameterValue(this.FULL_SCREEN);
                        byte?* nullablePtr1 = (byte?*) new byte?(boolFromString.Value ? ((byte) 2) : ((byte) 0));
                        nullablePtr1 = (byte?*) ref byteFromString;
                        base.SetParameterValue(this.WINDOW_MODE, byteFromString.Value);
                    }
                }
                if ((byteFromString == null) || !Enum.IsDefined(typeof(MyWindowModeEnum), byteFromString))
                {
                    return MyWindowModeEnum.Fullscreen;
                }
                return byteFromString.Value;
            }
            set => 
                base.SetParameterValue(this.WINDOW_MODE, (int) value);
        }

        public bool CaptureMouse
        {
            get => 
                !base.GetParameterValue(this.MOUSE_CAPTURE).Equals("False");
            set => 
                base.SetParameterValue(this.MOUSE_CAPTURE, value.ToString());
        }

        public bool VerticalSync
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.VERTICAL_SYNC), false);
            set => 
                base.SetParameterValue(this.VERTICAL_SYNC, new bool?(value));
        }

        public int RefreshRate
        {
            get => 
                MyUtils.GetIntFromString(base.GetParameterValue(this.REFRESH_RATE), 0);
            set => 
                base.SetParameterValue(this.REFRESH_RATE, value);
        }

        public float FlaresIntensity
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.FLARES_INTENSITY), 1f);
            set => 
                base.SetParameterValue(this.FLARES_INTENSITY, value);
        }

        public bool? EnableDamageEffects
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_DAMAGE_EFFECTS));
            set => 
                base.SetParameterValue(this.ENABLE_DAMAGE_EFFECTS, value);
        }

        public float GameVolume
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.GAME_VOLUME), 1f);
            set => 
                base.SetParameterValue(this.GAME_VOLUME, value);
        }

        public float MusicVolume
        {
            get
            {
                float? floatFromString = MyUtils.GetFloatFromString(base.GetParameterValue(this.MUSIC_VOLUME_OLD));
                if (floatFromString != null)
                {
                    if (floatFromString.Value != 1f)
                    {
                        base.SetParameterValue(this.MUSIC_VOLUME, floatFromString.Value);
                    }
                    base.RemoveParameterValue(this.MUSIC_VOLUME_OLD);
                }
                return MyUtils.GetFloatFromString(base.GetParameterValue(this.MUSIC_VOLUME), 0.5f);
            }
            set => 
                base.SetParameterValue(this.MUSIC_VOLUME, value);
        }

        public float VoiceChatVolume
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.VOICE_CHAT_VOLUME), 1f);
            set => 
                base.SetParameterValue(this.VOICE_CHAT_VOLUME, value);
        }

        public bool ExperimentalMode
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.EXPERIMENTAL_MODE), true);
            set => 
                base.SetParameterValue(this.EXPERIMENTAL_MODE, new bool?(value));
        }

        public bool ControlsHints
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.CONTROLS_HINTS), true);
            set => 
                base.SetParameterValue(this.CONTROLS_HINTS, new bool?(value));
        }

        public bool GoodBotHints
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.GOODBOT_HINTS), true);
            set => 
                base.SetParameterValue(this.GOODBOT_HINTS, new bool?(value));
        }

        public bool RotationHints
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ROTATION_HINTS), true);
            set => 
                base.SetParameterValue(this.ROTATION_HINTS, new bool?(value));
        }

        public bool ShowCrosshair
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.SHOW_CROSSHAIR), true);
            set => 
                base.SetParameterValue(this.SHOW_CROSSHAIR, new bool?(value));
        }

        public bool ShowChatTimestamp
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.SHOW_CHAT_TIMESTAMP), true);
            set => 
                base.SetParameterValue(this.SHOW_CHAT_TIMESTAMP, new bool?(value));
        }

        public bool EnableSteamCloud
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_STEAM_CLOUD), false);
            set => 
                base.SetParameterValue(this.ENABLE_STEAM_CLOUD, new bool?(value));
        }

        public float ScreenshotSizeMultiplier
        {
            get
            {
                if (string.IsNullOrEmpty(base.GetParameterValue(this.SCREENSHOT_SIZE_MULTIPLIER)))
                {
                    base.SetParameterValue(this.SCREENSHOT_SIZE_MULTIPLIER, (float) 1f);
                    base.Save();
                }
                return MyUtils.GetFloatFromString(base.GetParameterValue(this.SCREENSHOT_SIZE_MULTIPLIER), 1f);
            }
            set => 
                base.SetParameterValue(this.SCREENSHOT_SIZE_MULTIPLIER, value);
        }

        public MyLanguagesEnum Language
        {
            get
            {
                byte? byteFromString = MyUtils.GetByteFromString(base.GetParameterValue(this.LANGUAGE));
                if ((byteFromString == null) || !Enum.IsDefined(typeof(MyLanguagesEnum), byteFromString))
                {
                    return MyLanguage.GetOsLanguageCurrentOfficial();
                }
                return byteFromString.Value;
            }
            set => 
                base.SetParameterValue(this.LANGUAGE, (int) value);
        }

        public string Skin
        {
            get
            {
                if (string.IsNullOrEmpty(base.GetParameterValue(this.SKIN)))
                {
                    base.SetParameterValue(this.SKIN, "Default");
                    base.Save();
                }
                return base.GetParameterValue(this.SKIN);
            }
            set => 
                base.SetParameterValue(this.SKIN, value);
        }

        public SerializableDictionary<string, object> ControlsGeneral
        {
            get
            {
                if (!base.m_values.Dictionary.ContainsKey(this.CONTROLS_GENERAL))
                {
                    base.m_values.Dictionary.Add(this.CONTROLS_GENERAL, new SerializableDictionary<string, object>());
                }
                return base.GetParameterValueDictionary(this.CONTROLS_GENERAL);
            }
            set
            {
            }
        }

        public SerializableDictionary<string, object> ControlsButtons
        {
            get
            {
                if (!base.m_values.Dictionary.ContainsKey(this.CONTROLS_BUTTONS))
                {
                    base.m_values.Dictionary.Add(this.CONTROLS_BUTTONS, new SerializableDictionary<string, object>());
                }
                return base.GetParameterValueDictionary(this.CONTROLS_BUTTONS);
            }
            set
            {
            }
        }

        public SerializableDictionary<string, MyDebugInputData> DebugInputComponents
        {
            get
            {
                if (!base.m_values.Dictionary.ContainsKey(this.DEBUG_INPUT_COMPONENTS))
                {
                    base.m_values.Dictionary.Add(this.DEBUG_INPUT_COMPONENTS, new SerializableDictionary<string, MyDebugInputData>());
                }
                else if (!(base.m_values.Dictionary[this.DEBUG_INPUT_COMPONENTS] is SerializableDictionary<string, MyDebugInputData>))
                {
                    base.m_values.Dictionary[this.DEBUG_INPUT_COMPONENTS] = new SerializableDictionary<string, MyDebugInputData>();
                }
                return base.GetParameterValueT<SerializableDictionary<string, MyDebugInputData>>(this.DEBUG_INPUT_COMPONENTS);
            }
            set
            {
            }
        }

        public MyDebugComponent.MyDebugComponentInfoState DebugComponentsInfo
        {
            get
            {
                int? intFromString = MyUtils.GetIntFromString(base.GetParameterValue(this.DEBUG_INPUT_COMPONENTS_INFO));
                if ((intFromString == null) || !Enum.IsDefined(typeof(MyDebugComponent.MyDebugComponentInfoState), intFromString))
                {
                    return MyDebugComponent.MyDebugComponentInfoState.EnabledInfo;
                }
                return intFromString.Value;
            }
            set => 
                base.SetParameterValue(this.DEBUG_INPUT_COMPONENTS_INFO, (int) value);
        }

        public bool MinimalHud
        {
            get => 
                (MyHud.IsHudMinimal || MyHud.MinimalHud);
            set => 
                (this.HudState = value ? 0 : 1);
        }

        public int HudState
        {
            get => 
                MyUtils.GetIntFromString(base.GetParameterValue(this.HUD_STATE), 1);
            set => 
                base.SetParameterValue(this.HUD_STATE, value);
        }

        public bool MemoryLimits
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.MEMORY_LIMITS), true);
            set => 
                base.SetParameterValue(this.MEMORY_LIMITS, new bool?(value));
        }

        public bool CubeBuilderUseSymmetry
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.CUBE_BUILDER_USE_SYMMETRY), true);
            set => 
                base.SetParameterValue(this.CUBE_BUILDER_USE_SYMMETRY, new bool?(value));
        }

        public int CubeBuilderBuildingMode
        {
            get => 
                MyUtils.GetIntFromString(base.GetParameterValue(this.CUBE_BUILDER_BUILDING_MODE), 0);
            set => 
                base.SetParameterValue(this.CUBE_BUILDER_BUILDING_MODE, value);
        }

        public bool CubeBuilderAlignToDefault
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.CUBE_BUILDER_ALIGN_TO_DEFAULT), true);
            set => 
                base.SetParameterValue(this.CUBE_BUILDER_ALIGN_TO_DEFAULT, new bool?(value));
        }

        public bool MultiplayerShowCompatible
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.MULTIPLAYER_SHOWCOMPATIBLE), true);
            set => 
                base.SetParameterValue(this.MULTIPLAYER_SHOWCOMPATIBLE, new bool?(value));
        }

        public bool CompressSaveGames
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.COMPRESS_SAVE_GAMES), MyFakes.GAME_SAVES_COMPRESSED_BY_DEFAULT);
            set => 
                base.SetParameterValue(this.COMPRESS_SAVE_GAMES, new bool?(value));
        }

        public bool EnablePerformanceWarnings
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_PERFORMANCE_WARNINGS_TEMP), true);
            set => 
                base.SetParameterValue(this.ENABLE_PERFORMANCE_WARNINGS_TEMP, new bool?(value));
        }

        public int LastCheckedVersion
        {
            get => 
                MyUtils.GetIntFromString(base.GetParameterValue(this.LAST_CHECKED_VERSION), 0);
            set => 
                base.SetParameterValue(this.LAST_CHECKED_VERSION, value);
        }

        public float UIOpacity
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.UI_TRANSPARENCY), 1f);
            set => 
                base.SetParameterValue(this.UI_TRANSPARENCY, value);
        }

        public float UIBkOpacity
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.UI_BK_TRANSPARENCY), 1f);
            set => 
                base.SetParameterValue(this.UI_BK_TRANSPARENCY, value);
        }

        public float HUDBkOpacity
        {
            get => 
                MyUtils.GetFloatFromString(base.GetParameterValue(this.HUD_BK_TRANSPARENCY), 0.6f);
            set => 
                base.SetParameterValue(this.HUD_BK_TRANSPARENCY, value);
        }

        public List<string> TutorialsFinished
        {
            get
            {
                if (!base.m_values.Dictionary.ContainsKey(this.TUTORIALS_FINISHED))
                {
                    base.m_values.Dictionary.Add(this.TUTORIALS_FINISHED, new List<string>());
                }
                return base.GetParameterValueT<List<string>>(this.TUTORIALS_FINISHED);
            }
            set
            {
            }
        }

        public bool HudWarnings
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.HUD_WARNINGS), true);
            set => 
                base.SetParameterValue(this.HUD_WARNINGS, new bool?(value));
        }

        public bool EnableVoiceChat
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_VOICE_CHAT), true);
            set => 
                base.SetParameterValue(this.ENABLE_VOICE_CHAT, new bool?(value));
        }

        public bool EnableMuteWhenNotInFocus
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_MUTE_WHEN_NOT_IN_FOCUS), true);
            set => 
                base.SetParameterValue(this.ENABLE_MUTE_WHEN_NOT_IN_FOCUS, new bool?(value));
        }

        public bool EnableDynamicMusic
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.DYNAMIC_MUSIC), true);
            set => 
                base.SetParameterValue(this.DYNAMIC_MUSIC, new bool?(value));
        }

        public bool ShipSoundsAreBasedOnSpeed
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.SHIP_SOUNDS_SPEED), true);
            set => 
                base.SetParameterValue(this.SHIP_SOUNDS_SPEED, new bool?(value));
        }

        public bool EnableReverb
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_REVERB), true);
            set => 
                base.SetParameterValue(this.ENABLE_REVERB, new bool?(value));
        }

        public MyStringId GraphicsRenderer
        {
            get
            {
                string parameterValue = base.GetParameterValue(this.GRAPHICS_RENDERER);
                if (string.IsNullOrEmpty(parameterValue))
                {
                    return MyPerGameSettings.DefaultGraphicsRenderer;
                }
                MyStringId id = MyStringId.TryGet(parameterValue);
                return (!(id == MyStringId.NullOrEmpty) ? id : MyPerGameSettings.DefaultGraphicsRenderer);
            }
            set => 
                base.SetParameterValue(this.GRAPHICS_RENDERER, value.ToString());
        }

        public MyObjectBuilder_ServerFilterOptions ServerSearchSettings
        {
            get
            {
                object obj2;
                base.m_values.Dictionary.TryGetValue(this.SERVER_SEARCH_SETTINGS, out obj2);
                return (obj2 as MyObjectBuilder_ServerFilterOptions);
            }
            set => 
                (base.m_values.Dictionary[this.SERVER_SEARCH_SETTINGS] = value);
        }

        public bool EnableDoppler
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.ENABLE_DOPPLER), true);
            set => 
                base.SetParameterValue(this.ENABLE_DOPPLER, new bool?(value));
        }

        public HashSet<ulong> MutedPlayers
        {
            get => 
                this.GetSeparatedValues(this.MUTED_PLAYERS, ref this.m_mutedPlayers, ref this.m_mutedPlayersInited);
            set => 
                this.SetSeparatedValues(this.MUTED_PLAYERS, value, ref this.m_mutedPlayers, ref this.m_mutedPlayersInited);
        }

        public HashSet<ulong> DontSendVoicePlayers
        {
            get => 
                this.GetSeparatedValues(this.DONT_SEND_VOICE_PLAYERS, ref this.m_dontSendVoicePlayers, ref this.m_dontSendVoicePlayersInited);
            set => 
                this.SetSeparatedValues(this.DONT_SEND_VOICE_PLAYERS, value, ref this.m_dontSendVoicePlayers, ref this.m_dontSendVoicePlayersInited);
        }

        public LowMemSwitch LowMemSwitchToLow
        {
            get => 
                ((LowMemSwitch) MyUtils.GetIntFromString(base.GetParameterValue(this.LOW_MEM_SWITCH_TO_LOW), 0));
            set => 
                base.SetParameterValue(this.LOW_MEM_SWITCH_TO_LOW, (int) value);
        }

        public NewsletterStatus NewsletterCurrentStatus
        {
            get => 
                ((NewsletterStatus) MyUtils.GetIntFromString(base.GetParameterValue(this.NEWSLETTER_CURRENT_STATUS), 1));
            set => 
                base.SetParameterValue(this.NEWSLETTER_CURRENT_STATUS, (int) value);
        }

        public WelcomeScreenStatus WelcomScreenCurrentStatus
        {
            get => 
                ((WelcomeScreenStatus) MyUtils.GetIntFromString(base.GetParameterValue(this.WELCOMESCREEN_CURRENT_STATUS), 0));
            set => 
                base.SetParameterValue(this.WELCOMESCREEN_CURRENT_STATUS, (int) value);
        }

        public bool DebugOverrideAutosave
        {
            get => 
                MyUtils.GetBoolFromString(base.GetParameterValue(this.DEBUG_OVERRIDE_AUTOSAVE), false);
            set => 
                base.SetParameterValue(this.DEBUG_OVERRIDE_AUTOSAVE, new bool?(value));
        }

        MyTextureAnisoFiltering? IMyConfig.AnisotropicFiltering =>
            this.AnisotropicFiltering;

        MyAntialiasingMode? IMyConfig.AntialiasingMode =>
            this.AntialiasingMode;

        bool IMyConfig.ControlsHints =>
            this.ControlsHints;

        int IMyConfig.CubeBuilderBuildingMode =>
            this.CubeBuilderBuildingMode;

        bool IMyConfig.CubeBuilderUseSymmetry =>
            this.CubeBuilderUseSymmetry;

        bool IMyConfig.EnableDamageEffects
        {
            get
            {
                if (this.EnableDamageEffects != null)
                {
                    return this.EnableDamageEffects.Value;
                }
                return true;
            }
        }

        float IMyConfig.FieldOfView =>
            this.FieldOfView;

        float IMyConfig.GameVolume =>
            this.GameVolume;

        bool IMyConfig.HudWarnings =>
            this.HudWarnings;

        MyLanguagesEnum IMyConfig.Language =>
            this.Language;

        bool IMyConfig.MemoryLimits =>
            this.MemoryLimits;

        int IMyConfig.HudState =>
            this.HudState;

        float IMyConfig.MusicVolume =>
            this.MusicVolume;

        int IMyConfig.RefreshRate =>
            this.RefreshRate;

        MyGraphicsRenderer IMyConfig.GraphicsRenderer =>
            (!(MyStringId.TryGet(base.GetParameterValue(this.GRAPHICS_RENDERER)) == MySandboxGame.DirectX11RendererKey) ? MyGraphicsRenderer.NONE : MyGraphicsRenderer.DX11);

        bool IMyConfig.RotationHints =>
            this.RotationHints;

        int? IMyConfig.ScreenHeight =>
            this.ScreenHeight;

        int? IMyConfig.ScreenWidth =>
            this.ScreenWidth;

        MyShadowsQuality? IMyConfig.ShadowQuality =>
            this.ShadowQuality;

        bool IMyConfig.ShowCrosshair =>
            this.ShowCrosshair;

        MyTextureQuality? IMyConfig.TextureQuality =>
            this.TextureQuality;

        bool IMyConfig.VerticalSync =>
            this.VerticalSync;

        int IMyConfig.VideoAdapter =>
            this.VideoAdapter;

        MyWindowModeEnum IMyConfig.WindowMode =>
            this.WindowMode;

        bool IMyConfig.CaptureMouse =>
            this.CaptureMouse;

        bool? IMyConfig.AmbientOcclusionEnabled =>
            this.AmbientOcclusionEnabled;

        DictionaryReader<string, object> IMyConfig.ControlsButtons =>
            this.ControlsButtons.Dictionary;

        DictionaryReader<string, object> IMyConfig.ControlsGeneral =>
            this.ControlsGeneral.Dictionary;

        HashSetReader<ulong> IMyConfig.DontSendVoicePlayers =>
            this.DontSendVoicePlayers;

        bool IMyConfig.EnableDynamicMusic =>
            this.EnableDynamicMusic;

        bool IMyConfig.EnableMuteWhenNotInFocus =>
            this.EnableMuteWhenNotInFocus;

        bool IMyConfig.EnablePerformanceWarnings =>
            this.EnablePerformanceWarnings;

        bool IMyConfig.EnableReverb =>
            this.EnableReverb;

        bool IMyConfig.EnableVoiceChat =>
            this.EnableVoiceChat;

        bool IMyConfig.FirstTimeRun =>
            this.FirstTimeRun;

        float IMyConfig.FlaresIntensity =>
            this.FlaresIntensity;

        float? IMyConfig.GrassDensityFactor =>
            this.GrassDensityFactor;

        float? IMyConfig.GrassDrawDistance =>
            this.GrassDrawDistance;

        float IMyConfig.HUDBkOpacity =>
            this.HUDBkOpacity;

        MyRenderQualityEnum? IMyConfig.ModelQuality =>
            this.ModelQuality;

        HashSetReader<ulong> IMyConfig.MutedPlayers =>
            this.MutedPlayers;

        float IMyConfig.ScreenshotSizeMultiplier =>
            this.ScreenshotSizeMultiplier;

        MyRenderQualityEnum? IMyConfig.ShaderQuality =>
            this.ShaderQuality;

        bool IMyConfig.ShipSoundsAreBasedOnSpeed =>
            this.ShipSoundsAreBasedOnSpeed;

        string IMyConfig.Skin =>
            this.Skin;

        float IMyConfig.UIBkOpacity =>
            this.UIBkOpacity;

        float IMyConfig.UIOpacity =>
            this.UIOpacity;

        float? IMyConfig.VegetationDrawDistance =>
            this.VegetationDrawDistance;

        float IMyConfig.VoiceChatVolume =>
            this.VoiceChatVolume;

        MyRenderQualityEnum? IMyConfig.VoxelQuality =>
            this.VoxelQuality;

        public bool? GDPRConsent
        {
            get
            {
                string parameterValue = base.GetParameterValue(this.GDPR_CONSENT);
                if (!string.IsNullOrWhiteSpace(parameterValue))
                {
                    return new bool?(MyUtils.GetBoolFromString(parameterValue, false));
                }
                return null;
            }
            set => 
                base.SetParameterValue(this.GDPR_CONSENT, new bool?(value.Value));
        }

        public bool? GDPRConsentSent
        {
            get
            {
                string parameterValue = base.GetParameterValue(this.GDPR_CONSENT_SENT);
                if (!string.IsNullOrWhiteSpace(parameterValue))
                {
                    return new bool?(MyUtils.GetBoolFromString(parameterValue, false));
                }
                return null;
            }
            set => 
                base.SetParameterValue(this.GDPR_CONSENT_SENT, new bool?(value.Value));
        }

        public enum LowMemSwitch
        {
            ARMED,
            TRIGGERED,
            USER_SAID_NO
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct MyDebugInputData
        {
            [ProtoMember(0x2bf)]
            public bool Enabled;
            [ProtoMember(0x2c2)]
            public string SerializedData;
            public object Data
            {
                get => 
                    MyConfig.Decode64AndDeserialize(this.SerializedData);
                set => 
                    (this.SerializedData = MyConfig.SerialiazeAndEncod64(value));
            }
            public bool ShouldSerializeData() => 
                false;
        }

        public enum NewsletterStatus
        {
            Unknown,
            NoFeedback,
            NotInterested,
            EmailNotConfirmed,
            EmailConfirmed
        }

        public enum WelcomeScreenStatus
        {
            NotSeen,
            AlreadySeen
        }
    }
}

