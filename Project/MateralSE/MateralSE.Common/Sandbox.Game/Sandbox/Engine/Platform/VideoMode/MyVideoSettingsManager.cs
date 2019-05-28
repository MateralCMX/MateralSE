namespace Sandbox.Engine.Platform.VideoMode
{
    using Sandbox;
    using Sandbox.AppCode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.EntityComponents.Renders;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Management;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    public static class MyVideoSettingsManager
    {
        private static Dictionary<int, MyAspectRatio> m_recommendedAspectRatio;
        private static MyAdapterInfo[] m_adapters;
        private static readonly MyAspectRatio[] m_aspectRatios = new MyAspectRatio[MyUtils.GetMaxValueFromEnum<MyAspectRatioEnum>() + 1];
        private static MyRenderDeviceSettings m_currentDeviceSettings;
        private static bool m_currentDeviceIsTripleHead;
        private static MyGraphicsSettings m_currentGraphicsSettings;
        public static readonly MyDisplayMode[] DebugDisplayModes;

        static MyVideoSettingsManager()
        {
            Action<bool, MyAspectRatioEnum, float, string, bool> action1 = (isTripleHead, aspectRatioEnum, aspectRatioNumber, textShort, isSupported) => m_aspectRatios[(int) aspectRatioEnum] = new MyAspectRatio(isTripleHead, aspectRatioEnum, aspectRatioNumber, textShort, isSupported);
            action1(false, MyAspectRatioEnum.Normal_4_3, 1.333333f, "4:3", true);
            action1(false, MyAspectRatioEnum.Normal_16_9, 1.777778f, "16:9", true);
            action1(false, MyAspectRatioEnum.Normal_16_10, 1.6f, "16:10", true);
            action1(false, MyAspectRatioEnum.Dual_4_3, 2.666667f, "Dual 4:3", true);
            action1(false, MyAspectRatioEnum.Dual_16_9, 3.555556f, "Dual 16:9", true);
            action1(false, MyAspectRatioEnum.Dual_16_10, 3.2f, "Dual 16:10", true);
            action1(true, MyAspectRatioEnum.Triple_4_3, 4f, "Triple 4:3", true);
            action1(true, MyAspectRatioEnum.Triple_16_9, 5.333333f, "Triple 16:9", true);
            action1(true, MyAspectRatioEnum.Triple_16_10, 4.8f, "Triple 16:10", true);
            action1(false, MyAspectRatioEnum.Unsupported_5_4, 1.25f, "5:4", false);
            DebugDisplayModes = new MyDisplayMode[0];
        }

        public static ChangeResult Apply(MyGraphicsSettings settings)
        {
            MySandboxGame.Log.WriteLine("MyVideoModeManager.Apply(MyGraphicsSettings1)");
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                MySandboxGame.Log.WriteLine("Flares Intensity: " + settings.FlaresIntensity);
                MySandboxGame.Log.WriteLine("Field of view: " + settings.FieldOfView);
                MySandboxGame.Log.WriteLine("PostProcessingEnabled: " + settings.PostProcessingEnabled.ToString());
                MySandboxGame.Log.WriteLine("Render.GrassDensityFactor: " + settings.PerformanceSettings.RenderSettings.GrassDensityFactor);
                MySandboxGame.Log.WriteLine("Render.GrassDrawDistance: " + settings.PerformanceSettings.RenderSettings.GrassDrawDistance);
                MySandboxGame.Log.WriteLine("Render.DistanceFade: " + settings.PerformanceSettings.RenderSettings.DistanceFade);
                MySandboxGame.Log.WriteLine("Render.AntialiasingMode: " + settings.PerformanceSettings.RenderSettings.AntialiasingMode);
                MySandboxGame.Log.WriteLine("Render.ShadowQuality: " + settings.PerformanceSettings.RenderSettings.ShadowQuality);
                MySandboxGame.Log.WriteLine("Render.AmbientOcclusionEnabled: " + settings.PerformanceSettings.RenderSettings.AmbientOcclusionEnabled.ToString());
                MySandboxGame.Log.WriteLine("Render.TextureQuality: " + settings.PerformanceSettings.RenderSettings.TextureQuality);
                MySandboxGame.Log.WriteLine("Render.AnisotropicFiltering: " + settings.PerformanceSettings.RenderSettings.AnisotropicFiltering);
                MySandboxGame.Log.WriteLine("Render.VoxelShaderQuality: " + settings.PerformanceSettings.RenderSettings.VoxelShaderQuality);
                MySandboxGame.Log.WriteLine("Render.AlphaMaskedShaderQuality: " + settings.PerformanceSettings.RenderSettings.AlphaMaskedShaderQuality);
                MySandboxGame.Log.WriteLine("Render.AtmosphereShaderQuality: " + settings.PerformanceSettings.RenderSettings.AtmosphereShaderQuality);
                if (!m_currentGraphicsSettings.Equals(ref settings))
                {
                    SetEnableDamageEffects(settings.PerformanceSettings.EnableDamageEffects);
                    SetFov(settings.FieldOfView);
                    SetPostProcessingEnabled(settings.PostProcessingEnabled);
                    if (MyRenderProxy.Settings.FlaresIntensity != settings.FlaresIntensity)
                    {
                        MyRenderProxy.Settings.FlaresIntensity = settings.FlaresIntensity;
                        MyRenderProxy.SetSettingsDirty();
                    }
                    if (!m_currentGraphicsSettings.PerformanceSettings.RenderSettings.Equals(ref settings.PerformanceSettings.RenderSettings))
                    {
                        MyRenderProxy.SwitchRenderSettings(settings.PerformanceSettings.RenderSettings);
                    }
                    if (m_currentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality != settings.PerformanceSettings.RenderSettings.VoxelQuality)
                    {
                        MyRenderComponentVoxelMap.SetLodQuality(settings.PerformanceSettings.RenderSettings.VoxelQuality);
                    }
                    m_currentGraphicsSettings = settings;
                    MySector.Lodding.SelectQuality(settings.PerformanceSettings.RenderSettings.ModelQuality);
                }
                else
                {
                    return ChangeResult.NothingChanged;
                }
            }
            return ChangeResult.Success;
        }

        public static ChangeResult Apply(MyRenderDeviceSettings settings)
        {
            MySandboxGame.Log.WriteLine("MyVideoModeManager.Apply(MyRenderDeviceSettings)");
            using (MySandboxGame.Log.IndentUsing(LoggingOptions.NONE))
            {
                ChangeResult nothingChanged;
                MySandboxGame.Log.WriteLine("VideoAdapter: " + settings.AdapterOrdinal);
                MySandboxGame.Log.WriteLine("Width: " + settings.BackBufferWidth);
                MySandboxGame.Log.WriteLine("Height: " + settings.BackBufferHeight);
                MySandboxGame.Log.WriteLine("RefreshRate: " + settings.RefreshRate);
                MySandboxGame.Log.WriteLine("WindowMode: " + ((settings.WindowMode == MyWindowModeEnum.Fullscreen) ? "Fullscreen" : ((settings.WindowMode == MyWindowModeEnum.Window) ? "Window" : "Fullscreen window")));
                MySandboxGame.Log.WriteLine("VerticalSync: " + settings.VSync.ToString());
                if (settings.Equals(ref m_currentDeviceSettings) && (settings.NewAdapterOrdinal == settings.AdapterOrdinal))
                {
                    nothingChanged = ChangeResult.NothingChanged;
                }
                else if (IsSupportedDisplayMode(settings.AdapterOrdinal, settings.BackBufferWidth, settings.BackBufferHeight, settings.WindowMode))
                {
                    float num;
                    float num2;
                    m_currentDeviceSettings = settings;
                    m_currentDeviceSettings.VSync = settings.VSync;
                    MySandboxGame.Static.SwitchSettings(m_currentDeviceSettings);
                    float aspectRatio = ((float) m_currentDeviceSettings.BackBufferWidth) / ((float) m_currentDeviceSettings.BackBufferHeight);
                    m_currentDeviceIsTripleHead = GetAspectRatio(GetClosestAspectRatio(aspectRatio)).IsTripleHead;
                    GetFovBounds(aspectRatio, out num, out num2);
                    SetFov(MathHelper.Clamp(m_currentGraphicsSettings.FieldOfView, num, num2));
                    SetPostProcessingEnabled(m_currentGraphicsSettings.PostProcessingEnabled);
                    goto TR_0000;
                }
                else
                {
                    nothingChanged = ChangeResult.Failed;
                }
                return nothingChanged;
            }
        TR_0000:
            return ChangeResult.Success;
        }

        public static ChangeResult ApplyVideoSettings(MyRenderDeviceSettings deviceSettings, MyGraphicsSettings graphicsSettings)
        {
            ChangeResult result = Apply(deviceSettings);
            if (result == ChangeResult.Failed)
            {
                return result;
            }
            ChangeResult result2 = Apply(graphicsSettings);
            return ((result == ChangeResult.Success) ? result : result2);
        }

        public static MyAspectRatio GetAspectRatio(MyAspectRatioEnum aspectRatioEnum) => 
            m_aspectRatios[(int) aspectRatioEnum];

        public static MyAspectRatioEnum GetClosestAspectRatio(float aspectRatio)
        {
            MyAspectRatioEnum aspectRatioEnum = MyAspectRatioEnum.Normal_4_3;
            float maxValue = float.MaxValue;
            for (int i = 0; i < m_aspectRatios.Length; i++)
            {
                float num3 = Math.Abs((float) (aspectRatio - m_aspectRatios[i].AspectRatioNumber));
                if (num3 < maxValue)
                {
                    maxValue = num3;
                    aspectRatioEnum = m_aspectRatios[i].AspectRatioEnum;
                }
            }
            return aspectRatioEnum;
        }

        public static void GetFovBounds(out float minRadians, out float maxRadians)
        {
            GetFovBounds(((float) m_currentDeviceSettings.BackBufferWidth) / ((float) m_currentDeviceSettings.BackBufferHeight), out minRadians, out maxRadians);
        }

        public static void GetFovBounds(float aspectRatio, out float minRadians, out float maxRadians)
        {
            minRadians = MyConstants.FIELD_OF_VIEW_CONFIG_MIN;
            if (aspectRatio >= 4.0)
            {
                maxRadians = MyConstants.FIELD_OF_VIEW_CONFIG_MAX_TRIPLE_HEAD;
            }
            else if (aspectRatio >= 2.6666666666666665)
            {
                maxRadians = MyConstants.FIELD_OF_VIEW_CONFIG_MAX_DUAL_HEAD;
            }
            else
            {
                maxRadians = MyConstants.FIELD_OF_VIEW_CONFIG_MAX;
            }
        }

        public static MyGraphicsSettings GetGraphicsSettingsFromConfig(ref MyPerformanceSettings defaults)
        {
            MyGraphicsSettings currentGraphicsSettings = CurrentGraphicsSettings;
            MyConfig config = MySandboxGame.Config;
            currentGraphicsSettings.PerformanceSettings = defaults;
            currentGraphicsSettings.GraphicsRenderer = config.GraphicsRenderer;
            currentGraphicsSettings.FieldOfView = config.FieldOfView;
            currentGraphicsSettings.PostProcessingEnabled = config.PostProcessingEnabled;
            currentGraphicsSettings.FlaresIntensity = config.FlaresIntensity;
            if (config.EnableDamageEffects == null)
            {
                config.EnableDamageEffects = new bool?(defaults.EnableDamageEffects);
            }
            currentGraphicsSettings.PerformanceSettings.EnableDamageEffects = config.EnableDamageEffects.Value;
            float? vegetationDrawDistance = config.VegetationDrawDistance;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.DistanceFade = (vegetationDrawDistance != null) ? vegetationDrawDistance.GetValueOrDefault() : defaults.RenderSettings.DistanceFade;
            vegetationDrawDistance = config.GrassDensityFactor;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.GrassDensityFactor = (vegetationDrawDistance != null) ? vegetationDrawDistance.GetValueOrDefault() : defaults.RenderSettings.GrassDensityFactor;
            vegetationDrawDistance = config.GrassDrawDistance;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.GrassDrawDistance = (vegetationDrawDistance != null) ? vegetationDrawDistance.GetValueOrDefault() : defaults.RenderSettings.GrassDrawDistance;
            MyAntialiasingMode? antialiasingMode = config.AntialiasingMode;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.AntialiasingMode = (antialiasingMode != null) ? antialiasingMode.GetValueOrDefault() : defaults.RenderSettings.AntialiasingMode;
            MyShadowsQuality? shadowQuality = config.ShadowQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.ShadowQuality = (shadowQuality != null) ? shadowQuality.GetValueOrDefault() : defaults.RenderSettings.ShadowQuality;
            bool? ambientOcclusionEnabled = config.AmbientOcclusionEnabled;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.AmbientOcclusionEnabled = (ambientOcclusionEnabled != null) ? ambientOcclusionEnabled.GetValueOrDefault() : defaults.RenderSettings.AmbientOcclusionEnabled;
            MyTextureQuality? textureQuality = config.TextureQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.TextureQuality = (textureQuality != null) ? textureQuality.GetValueOrDefault() : defaults.RenderSettings.TextureQuality;
            MyTextureAnisoFiltering? anisotropicFiltering = config.AnisotropicFiltering;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.AnisotropicFiltering = (anisotropicFiltering != null) ? anisotropicFiltering.GetValueOrDefault() : defaults.RenderSettings.AnisotropicFiltering;
            MyRenderQualityEnum? modelQuality = config.ModelQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.ModelQuality = (modelQuality != null) ? modelQuality.GetValueOrDefault() : defaults.RenderSettings.ModelQuality;
            modelQuality = config.VoxelQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality = (modelQuality != null) ? modelQuality.GetValueOrDefault() : defaults.RenderSettings.VoxelQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.HqDepth = true;
            modelQuality = config.ShaderQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelShaderQuality = (modelQuality != null) ? modelQuality.GetValueOrDefault() : defaults.RenderSettings.VoxelShaderQuality;
            modelQuality = config.ShaderQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.AlphaMaskedShaderQuality = (modelQuality != null) ? modelQuality.GetValueOrDefault() : defaults.RenderSettings.AlphaMaskedShaderQuality;
            modelQuality = config.ShaderQuality;
            currentGraphicsSettings.PerformanceSettings.RenderSettings.AtmosphereShaderQuality = (modelQuality != null) ? modelQuality.GetValueOrDefault() : defaults.RenderSettings.AtmosphereShaderQuality;
            return currentGraphicsSettings;
        }

        public static MyAspectRatio GetRecommendedAspectRatio(int adapterIndex) => 
            m_recommendedAspectRatio[adapterIndex];

        public static MyRenderDeviceSettings? Initialize()
        {
            MyRenderProxy.RequestVideoAdapters();
            MyConfig config = MySandboxGame.Config;
            RunningGraphicsRenderer = config.GraphicsRenderer;
            int? screenWidth = config.ScreenWidth;
            int? screenHeight = config.ScreenHeight;
            int? nullable3 = new int?(config.VideoAdapter);
            if (((nullable3 == null) || (screenWidth == null)) || (screenHeight == null))
            {
                return MyPerGameSettings.DefaultRenderDeviceSettings;
            }
            MyRenderDeviceSettings settings = new MyRenderDeviceSettings {
                AdapterOrdinal = nullable3.Value,
                NewAdapterOrdinal = nullable3.Value,
                BackBufferHeight = screenHeight.Value,
                BackBufferWidth = screenWidth.Value,
                RefreshRate = config.RefreshRate,
                VSync = config.VerticalSync,
                WindowMode = config.WindowMode
            };
            if (MyPerGameSettings.DefaultRenderDeviceSettings != null)
            {
                settings.UseStereoRendering = MyPerGameSettings.DefaultRenderDeviceSettings.Value.UseStereoRendering;
                settings.SettingsMandatory = MyPerGameSettings.DefaultRenderDeviceSettings.Value.SettingsMandatory;
            }
            return new MyRenderDeviceSettings?(settings);
        }

        public static bool IsCurrentAdapterNvidia() => 
            ((m_adapters.Length > m_currentDeviceSettings.AdapterOrdinal) && ((m_currentDeviceSettings.AdapterOrdinal >= 0) && (m_adapters[m_currentDeviceSettings.AdapterOrdinal].VendorId == VendorIds.Nvidia)));

        public static bool IsHardwareCursorUsed()
        {
            if (MyExternalAppBase.Static == null)
            {
                OperatingSystem oSVersion = Environment.OSVersion;
                if (((oSVersion.Platform != PlatformID.Win32NT) || (oSVersion.Version.Major != 6)) || (oSVersion.Version.Minor != 0))
                {
                    return ((oSVersion.Platform != PlatformID.Win32NT) || ((oSVersion.Version.Major != 5) || (oSVersion.Version.Minor != 1)));
                }
            }
            return false;
        }

        private static bool IsSupportedDisplayMode(int videoAdapter, int width, int height, MyWindowModeEnum windowMode)
        {
            bool flag = false;
            if (windowMode != MyWindowModeEnum.Fullscreen)
            {
                flag = true;
            }
            else
            {
                foreach (MyDisplayMode mode in m_adapters[videoAdapter].SupportedDisplayModes)
                {
                    if ((mode.Width == width) && (mode.Height == height))
                    {
                        flag = true;
                    }
                }
            }
            int maxTextureSize = m_adapters[videoAdapter].MaxTextureSize;
            if ((width > maxTextureSize) || (height > maxTextureSize))
            {
                MySandboxGame.Log.WriteLine($"VideoMode {width}x{height} requires texture size which is not supported by this HW (this HW supports max {maxTextureSize})");
                flag = false;
            }
            return flag;
        }

        public static bool IsTripleHead() => 
            m_currentDeviceIsTripleHead;

        public static bool IsTripleHead(Vector2I screenSize) => 
            GetAspectRatio(GetClosestAspectRatio(((float) screenSize.X) / ((float) screenSize.Y))).IsTripleHead;

        private static bool IsVirtualized(string manufacturer, string model)
        {
            manufacturer = manufacturer.ToLower();
            return ((manufacturer == "microsoft corporation") || (manufacturer.Contains("vmware") || ((model == "VirtualBox") || model.ToLower().Contains("virtual"))));
        }

        public static void LogApplicationInformation()
        {
            MySandboxGame.Log.WriteLine("MyVideoModeManager.LogApplicationInformation - START");
            MySandboxGame.Log.IncreaseIndent();
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                MySandboxGame.Log.WriteLine("Assembly.GetName: " + executingAssembly.GetName().ToString());
                MySandboxGame.Log.WriteLine("Assembly.FullName: " + executingAssembly.FullName);
                MySandboxGame.Log.WriteLine("Assembly.Location: " + executingAssembly.Location);
                MySandboxGame.Log.WriteLine("Assembly.ImageRuntimeVersion: " + executingAssembly.ImageRuntimeVersion);
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine("Error occured during enumerating application information. Application will still continue. Detail description: " + exception.ToString());
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyVideoModeManager.LogApplicationInformation - END");
        }

        public static void LogEnvironmentInformation()
        {
            MySandboxGame.Log.WriteLine("MyVideoModeManager.LogEnvironmentInformation - START");
            MySandboxGame.Log.IncreaseIndent();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select Manufacturer, Model from Win32_ComputerSystem");
                if (searcher != null)
                {
                    foreach (ManagementBaseObject obj2 in searcher.Get())
                    {
                        MySandboxGame.Log.WriteLine("Win32_ComputerSystem.Manufacturer: " + obj2["Manufacturer"]);
                        MySandboxGame.Log.WriteLine("Win32_ComputerSystem.Model: " + obj2["Model"]);
                        MySandboxGame.Log.WriteLine("Virtualized: " + IsVirtualized(obj2["Manufacturer"].ToString(), obj2["Model"].ToString()).ToString());
                    }
                }
                ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT Name FROM Win32_Processor");
                if (searcher2 != null)
                {
                    foreach (ManagementObject obj3 in searcher2.Get())
                    {
                        MySandboxGame.Log.WriteLine("Environment.ProcessorName: " + obj3["Name"]);
                    }
                }
                WinApi.MEMORYSTATUSEX lpBuffer = new WinApi.MEMORYSTATUSEX();
                WinApi.GlobalMemoryStatusEx(lpBuffer);
                MySandboxGame.Log.WriteLine("ComputerInfo.TotalPhysicalMemory: " + MyValueFormatter.GetFormatedLong((long) lpBuffer.ullTotalPhys) + " bytes");
                MySandboxGame.Log.WriteLine("ComputerInfo.TotalVirtualMemory: " + MyValueFormatter.GetFormatedLong((long) lpBuffer.ullTotalVirtual) + " bytes");
                MySandboxGame.Log.WriteLine("ComputerInfo.AvailablePhysicalMemory: " + MyValueFormatter.GetFormatedLong((long) lpBuffer.ullAvailPhys) + " bytes");
                MySandboxGame.Log.WriteLine("ComputerInfo.AvailableVirtualMemory: " + MyValueFormatter.GetFormatedLong((long) lpBuffer.ullAvailVirtual) + " bytes");
                ConnectionOptions options = new ConnectionOptions();
                using (ManagementObjectSearcher searcher3 = new ManagementObjectSearcher(new ManagementScope(@"\\localhost", options), new ObjectQuery("select FreeSpace,Size,Name from Win32_LogicalDisk where DriveType=3")))
                {
                    ManagementObjectCollection objects = searcher3.Get();
                    foreach (ManagementObject obj1 in objects)
                    {
                        string formatedLong = MyValueFormatter.GetFormatedLong(Convert.ToInt64(obj1["Size"]));
                        string str2 = MyValueFormatter.GetFormatedLong(Convert.ToInt64(obj1["FreeSpace"]));
                        string str3 = obj1["Name"].ToString();
                        string[] textArray1 = new string[] { "Drive ", str3, " | Capacity: ", formatedLong, " bytes | Free space: ", str2, " bytes" };
                        MySandboxGame.Log.WriteLine(string.Concat(textArray1));
                    }
                    objects.Dispose();
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine("Error occured during enumerating environment information. Application is continuing. Exception: " + exception.ToString());
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyVideoModeManager.LogEnvironmentInformation - END");
        }

        internal static void OnCreatedDeviceSettings(MyRenderMessageCreatedDeviceSettings message)
        {
            m_currentDeviceSettings = message.Settings;
            m_currentDeviceSettings.NewAdapterOrdinal = m_currentDeviceSettings.AdapterOrdinal;
            m_currentDeviceIsTripleHead = GetAspectRatio(GetClosestAspectRatio(((float) m_currentDeviceSettings.BackBufferWidth) / ((float) m_currentDeviceSettings.BackBufferHeight))).IsTripleHead;
        }

        internal static void OnVideoAdaptersResponse(MyRenderMessageVideoAdaptersResponse message)
        {
            MyRenderProxy.Log.WriteLine("MyVideoSettingsManager.OnVideoAdaptersResponse");
            using (MyRenderProxy.Log.IndentUsing(LoggingOptions.NONE))
            {
                MyAdapterInfo info;
                m_adapters = message.Adapters;
                int index = -1;
                info.Priority = 0x3e8;
                try
                {
                    index = MySandboxGame.Static.GameRenderComponent.RenderThread.CurrentAdapter;
                    info = m_adapters[index];
                    GpuUnderMinimum = !info.Has512MBRam;
                }
                catch
                {
                }
                m_recommendedAspectRatio = new Dictionary<int, MyAspectRatio>();
                if (m_adapters.Length == 0)
                {
                    MyRenderProxy.Log.WriteLine("ERROR: Adapters count is 0!");
                }
                for (int i = 0; i < m_adapters.Length; i++)
                {
                    MyAdapterInfo info2 = m_adapters[i];
                    MyRenderProxy.Log.WriteLine($"Adapter {info2}");
                    using (MyRenderProxy.Log.IndentUsing(LoggingOptions.NONE))
                    {
                        m_recommendedAspectRatio.Add(i, GetAspectRatio(GetClosestAspectRatio(((float) info2.DesktopBounds.Width) / ((float) info2.DesktopBounds.Height))));
                        if (info2.SupportedDisplayModes.Length == 0)
                        {
                            MyRenderProxy.Log.WriteLine($"WARNING: Adapter {i} count of supported display modes is 0!");
                        }
                        int maxTextureSize = info2.MaxTextureSize;
                        foreach (MyDisplayMode mode in info2.SupportedDisplayModes)
                        {
                            MyRenderProxy.Log.WriteLine(mode.ToString());
                            if ((mode.Width > maxTextureSize) || (mode.Height > maxTextureSize))
                            {
                                MyRenderProxy.Log.WriteLine($"WARNING: Display mode {mode} requires texture size which is not supported by this HW (this HW supports max {maxTextureSize})");
                            }
                        }
                    }
                    MySandboxGame.ShowIsBetterGCAvailableNotification |= (index != i) && (info.Priority < info2.Priority);
                }
            }
        }

        public static void SaveCurrentSettings()
        {
            WriteCurrentSettingsToConfig();
            MySandboxGame.Config.Save();
        }

        private static void SetEnableDamageEffects(bool enableDamageEffects)
        {
            m_currentGraphicsSettings.PerformanceSettings.EnableDamageEffects = enableDamageEffects;
            MySandboxGame.Static.EnableDamageEffects = enableDamageEffects;
        }

        private static void SetFov(float fov)
        {
            if (m_currentGraphicsSettings.FieldOfView != fov)
            {
                m_currentGraphicsSettings.FieldOfView = fov;
                if (MySector.MainCamera != null)
                {
                    MySector.MainCamera.FieldOfView = fov;
                    if (MySector.MainCamera.Zoom != null)
                    {
                        MySector.MainCamera.Zoom.Update(0.01666667f);
                    }
                }
            }
        }

        private static void SetHardwareCursor(bool useHardwareCursor)
        {
            MySandboxGame.Static.SetMouseVisible(IsHardwareCursorUsed());
            MyGuiSandbox.SetMouseCursorVisibility(IsHardwareCursorUsed(), false);
        }

        private static void SetPostProcessingEnabled(bool enable)
        {
            if (m_currentGraphicsSettings.PostProcessingEnabled != enable)
            {
                m_currentGraphicsSettings.PostProcessingEnabled = enable;
            }
        }

        public static void UpdateRenderSettingsFromConfig(ref MyPerformanceSettings defaults)
        {
            Apply(GetGraphicsSettingsFromConfig(ref defaults));
        }

        public static void WriteCurrentSettingsToConfig()
        {
            MySandboxGame.Config.VideoAdapter = m_currentDeviceSettings.NewAdapterOrdinal;
            MySandboxGame.Config.ScreenWidth = new int?(m_currentDeviceSettings.BackBufferWidth);
            MySandboxGame.Config.ScreenHeight = new int?(m_currentDeviceSettings.BackBufferHeight);
            MySandboxGame.Config.RefreshRate = m_currentDeviceSettings.RefreshRate;
            MySandboxGame.Config.WindowMode = m_currentDeviceSettings.WindowMode;
            MySandboxGame.Config.VerticalSync = m_currentDeviceSettings.VSync;
            MySandboxGame.Config.FieldOfView = m_currentGraphicsSettings.FieldOfView;
            MySandboxGame.Config.PostProcessingEnabled = m_currentGraphicsSettings.PostProcessingEnabled;
            MySandboxGame.Config.FlaresIntensity = m_currentGraphicsSettings.FlaresIntensity;
            MySandboxGame.Config.GraphicsRenderer = m_currentGraphicsSettings.GraphicsRenderer;
            MySandboxGame.Config.EnableDamageEffects = new bool?(m_currentGraphicsSettings.PerformanceSettings.EnableDamageEffects);
            MyRenderSettings1 renderSettings = m_currentGraphicsSettings.PerformanceSettings.RenderSettings;
            MySandboxGame.Config.VegetationDrawDistance = new float?(renderSettings.DistanceFade);
            MySandboxGame.Config.GrassDensityFactor = new float?(renderSettings.GrassDensityFactor);
            MySandboxGame.Config.GrassDrawDistance = new float?(renderSettings.GrassDrawDistance);
            MySandboxGame.Config.AntialiasingMode = new MyAntialiasingMode?(renderSettings.AntialiasingMode);
            MySandboxGame.Config.ShadowQuality = new MyShadowsQuality?(renderSettings.ShadowQuality);
            MySandboxGame.Config.AmbientOcclusionEnabled = new bool?(renderSettings.AmbientOcclusionEnabled);
            MySandboxGame.Config.TextureQuality = new MyTextureQuality?(renderSettings.TextureQuality);
            MySandboxGame.Config.AnisotropicFiltering = new MyTextureAnisoFiltering?(renderSettings.AnisotropicFiltering);
            MySandboxGame.Config.ModelQuality = new MyRenderQualityEnum?(renderSettings.ModelQuality);
            MySandboxGame.Config.VoxelQuality = new MyRenderQualityEnum?(renderSettings.VoxelQuality);
            MySandboxGame.Config.ShaderQuality = new MyRenderQualityEnum?(renderSettings.VoxelShaderQuality);
            MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.ARMED;
        }

        public static MyAdapterInfo[] Adapters =>
            m_adapters;

        public static MyRenderDeviceSettings CurrentDeviceSettings =>
            m_currentDeviceSettings;

        public static MyGraphicsSettings CurrentGraphicsSettings =>
            m_currentGraphicsSettings;

        public static MyStringId RunningGraphicsRenderer
        {
            [CompilerGenerated]
            get => 
                <RunningGraphicsRenderer>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<RunningGraphicsRenderer>k__BackingField = value);
        }

        public static bool GpuUnderMinimum
        {
            [CompilerGenerated]
            get => 
                <GpuUnderMinimum>k__BackingField;
            [CompilerGenerated]
            private set => 
                (<GpuUnderMinimum>k__BackingField = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVideoSettingsManager.<>c <>9 = new MyVideoSettingsManager.<>c();

            internal void <.cctor>b__22_0(bool isTripleHead, MyAspectRatioEnum aspectRatioEnum, float aspectRatioNumber, string textShort, bool isSupported)
            {
                MyVideoSettingsManager.m_aspectRatios[(int) aspectRatioEnum] = new MyAspectRatio(isTripleHead, aspectRatioEnum, aspectRatioNumber, textShort, isSupported);
            }
        }

        public enum ChangeResult
        {
            Success,
            NothingChanged,
            Failed
        }
    }
}

