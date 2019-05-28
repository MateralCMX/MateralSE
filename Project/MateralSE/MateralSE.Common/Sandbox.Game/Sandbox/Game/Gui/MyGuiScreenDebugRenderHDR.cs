namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "HDR")]
    internal class MyGuiScreenDebugRenderHDR : MyGuiScreenDebugBase
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;

        public MyGuiScreenDebugRenderHDR() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderHDR";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("HDR", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Enable", MyRenderProxy.Settings.HDREnabled, delegate (MyGuiControlCheckbox x) {
                MyRenderProxy.Settings.HDREnabled = x.IsChecked;
                if (MyRenderProxy.Settings.HDREnabled)
                {
                    MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = true;
                    MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = 1f;
                    MySector.PlanetProperties.AtmosphereIntensityMultiplier = 35f;
                    MySector.PlanetProperties.CloudsIntensityMultiplier = 60f;
                    MySector.SunProperties.SunIntensity = 150f;
                    MyPostprocessSettingsWrapper.MarkDirty();
                }
                else
                {
                    MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = false;
                    MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = 0.5f;
                    MySector.SunProperties.SunIntensity = 5f;
                    MySector.PlanetProperties.AtmosphereIntensityMultiplier = 1f;
                    MySector.PlanetProperties.CloudsIntensityMultiplier = 1f;
                    MyPostprocessSettingsWrapper.MarkDirty();
                }
                MyRenderProxy.SetSettingsDirty();
            }, true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("64bit target", MyRenderProxy.Settings.User.HqTarget, delegate (MyGuiControlCheckbox x) {
                MyRenderProxy.Settings.User.HqTarget = x.IsChecked;
                MyRenderProxy.SetSettingsDirty();
            }, true, null, color, captionOffset);
            color = null;
            this.AddSlider("Time of day", 0f, MySession.Static.Settings.SunRotationIntervalMinutes, () => MyTimeOfDayHelper.TimeOfDay, new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDay), color);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderHDR.<>c <>9 = new MyGuiScreenDebugRenderHDR.<>c();
            public static Action<MyGuiControlCheckbox> <>9__3_0;
            public static Action<MyGuiControlCheckbox> <>9__3_1;
            public static Func<float> <>9__3_2;

            internal void <RecreateControls>b__3_0(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.HDREnabled = x.IsChecked;
                if (MyRenderProxy.Settings.HDREnabled)
                {
                    MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = true;
                    MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = 1f;
                    MySector.PlanetProperties.AtmosphereIntensityMultiplier = 35f;
                    MySector.PlanetProperties.CloudsIntensityMultiplier = 60f;
                    MySector.SunProperties.SunIntensity = 150f;
                    MyPostprocessSettingsWrapper.MarkDirty();
                }
                else
                {
                    MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = false;
                    MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = 0.5f;
                    MySector.SunProperties.SunIntensity = 5f;
                    MySector.PlanetProperties.AtmosphereIntensityMultiplier = 1f;
                    MySector.PlanetProperties.CloudsIntensityMultiplier = 1f;
                    MyPostprocessSettingsWrapper.MarkDirty();
                }
                MyRenderProxy.SetSettingsDirty();
            }

            internal void <RecreateControls>b__3_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.User.HqTarget = x.IsChecked;
                MyRenderProxy.SetSettingsDirty();
            }

            internal float <RecreateControls>b__3_2() => 
                MyTimeOfDayHelper.TimeOfDay;
        }
    }
}

