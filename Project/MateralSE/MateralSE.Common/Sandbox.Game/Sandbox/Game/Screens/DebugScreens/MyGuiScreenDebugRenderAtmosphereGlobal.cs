namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [MyDebugScreen("Render", "Atmosphere Global")]
    public class MyGuiScreenDebugRenderAtmosphereGlobal : MyGuiScreenDebugBase
    {
        private static bool m_atmosphereEnabled = true;

        public MyGuiScreenDebugRenderAtmosphereGlobal() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void EnableAtmosphere(bool enabled)
        {
            m_atmosphereEnabled = enabled;
            MyRenderProxy.EnableAtmosphere(m_atmosphereEnabled);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector4? nullable2;
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Atmosphere", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            if (MySession.Static.GetComponent<MySectorWeatherComponent>() != null)
            {
                nullable2 = null;
                captionOffset = null;
                this.AddCheckBox("Enable Sun Rotation", (Func<bool>) (() => MySession.Static.GetComponent<MySectorWeatherComponent>().Enabled), (Action<bool>) (x => (MySession.Static.GetComponent<MySectorWeatherComponent>().Enabled = x)), true, null, nullable2, captionOffset);
                nullable2 = null;
                this.AddSlider("Time of day", 0f, (MySession.Static == null) ? 1f : MySession.Static.Settings.SunRotationIntervalMinutes, () => MyTimeOfDayHelper.TimeOfDay, new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDay), nullable2);
                nullable2 = null;
                this.AddSlider("Sun Speed", 0.5f, 60f, (Func<float>) (() => MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval), (Action<float>) (f => (MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval = f)), nullable2);
            }
            nullable2 = null;
            captionOffset = null;
            this.AddCheckBox("Enable atmosphere", (Func<bool>) (() => m_atmosphereEnabled), (Action<bool>) (b => this.EnableAtmosphere(b)), true, null, nullable2, captionOffset);
            nullable2 = null;
            this.AddSlider("Atmosphere Intensity", MySector.PlanetProperties.AtmosphereIntensityMultiplier, 0.1f, 150f, delegate (MyGuiControlSlider f) {
                MySector.PlanetProperties.AtmosphereIntensityMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }, nullable2);
            nullable2 = null;
            this.AddSlider("Atmosphere Intensity in Ambient", MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier, 0.1f, 150f, delegate (MyGuiControlSlider f) {
                MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }, nullable2);
            nullable2 = null;
            this.AddSlider("Atmosphere Desaturation in Ambient", MySector.PlanetProperties.AtmosphereDesaturationFactorForward, 0f, 1f, delegate (MyGuiControlSlider f) {
                MySector.PlanetProperties.AtmosphereDesaturationFactorForward = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }, nullable2);
            nullable2 = null;
            this.AddSlider("Clouds Intensity", MySector.PlanetProperties.CloudsIntensityMultiplier, 0.5f, 150f, delegate (MyGuiControlSlider f) {
                MySector.PlanetProperties.CloudsIntensityMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }, nullable2);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderPlanetSettings settings = new MyRenderPlanetSettings {
                AtmosphereIntensityMultiplier = MySector.PlanetProperties.AtmosphereIntensityMultiplier,
                AtmosphereIntensityAmbientMultiplier = MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier,
                AtmosphereDesaturationFactorForward = MySector.PlanetProperties.AtmosphereDesaturationFactorForward,
                CloudsIntensityMultiplier = MySector.PlanetProperties.CloudsIntensityMultiplier
            };
            MyRenderProxy.UpdatePlanetSettings(ref settings);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderAtmosphereGlobal.<>c <>9 = new MyGuiScreenDebugRenderAtmosphereGlobal.<>c();
            public static Func<bool> <>9__2_0;
            public static Action<bool> <>9__2_1;
            public static Func<float> <>9__2_2;
            public static Func<float> <>9__2_3;
            public static Action<float> <>9__2_4;
            public static Func<bool> <>9__2_5;
            public static Action<MyGuiControlSlider> <>9__2_7;
            public static Action<MyGuiControlSlider> <>9__2_8;
            public static Action<MyGuiControlSlider> <>9__2_9;
            public static Action<MyGuiControlSlider> <>9__2_10;

            internal bool <RecreateControls>b__2_0() => 
                MySession.Static.GetComponent<MySectorWeatherComponent>().Enabled;

            internal void <RecreateControls>b__2_1(bool x)
            {
                MySession.Static.GetComponent<MySectorWeatherComponent>().Enabled = x;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlSlider f)
            {
                MySector.PlanetProperties.CloudsIntensityMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }

            internal float <RecreateControls>b__2_2() => 
                MyTimeOfDayHelper.TimeOfDay;

            internal float <RecreateControls>b__2_3() => 
                MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval;

            internal void <RecreateControls>b__2_4(float f)
            {
                MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval = f;
            }

            internal bool <RecreateControls>b__2_5() => 
                MyGuiScreenDebugRenderAtmosphereGlobal.m_atmosphereEnabled;

            internal void <RecreateControls>b__2_7(MyGuiControlSlider f)
            {
                MySector.PlanetProperties.AtmosphereIntensityMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }

            internal void <RecreateControls>b__2_8(MyGuiControlSlider f)
            {
                MySector.PlanetProperties.AtmosphereIntensityAmbientMultiplier = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }

            internal void <RecreateControls>b__2_9(MyGuiControlSlider f)
            {
                MySector.PlanetProperties.AtmosphereDesaturationFactorForward = f.Value;
                MyRenderProxy.SetSettingsDirty();
            }
        }
    }
}

