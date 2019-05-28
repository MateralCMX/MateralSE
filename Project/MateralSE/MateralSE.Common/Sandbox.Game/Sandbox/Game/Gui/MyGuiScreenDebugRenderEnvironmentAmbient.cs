namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Environment Ambient")]
    internal class MyGuiScreenDebugRenderEnvironmentAmbient : MyGuiScreenDebugBase
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;
        private MyGuiControlSlider m_resolutionSlider;
        private MyGuiControlSlider m_resolutionFilteredSlider;

        public MyGuiScreenDebugRenderEnvironmentAmbient() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderEnvironmentAmbient";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Environment Ambient", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("Indirect light", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("Diffuse factor", MySector.SunProperties.EnvironmentLight.AmbientDiffuseFactor, 0f, 5f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AmbientDiffuseFactor = v.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show Indirect Diffuse", MyRenderProxy.Settings.DisplayAmbientDiffuse, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAmbientDiffuse = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Specular factor", MySector.SunProperties.EnvironmentLight.AmbientSpecularFactor, 0f, 15f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AmbientSpecularFactor = v.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show Indirect Specular", MyRenderProxy.Settings.DisplayAmbientSpecular, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAmbientSpecular = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Glass Ambient", MySector.SunProperties.EnvironmentLight.GlassAmbient, 0f, 5f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.GlassAmbient = v.Value)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Environment probe", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Distance", MySector.SunProperties.EnvironmentProbe.DrawDistance, 5f, 1000f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentProbe.DrawDistance = v.Value)), color);
            color = null;
            this.m_resolutionSlider = base.AddSlider("Resolution", (float) MySector.SunProperties.EnvMapResolution, 32f, 4096f, delegate (MyGuiControlSlider v) {
                MySector.SunProperties.EnvMapResolution = MathHelper.GetNearestBiggerPowerOfTwo((int) v.Value);
                if (MySector.SunProperties.EnvMapFilteredResolution > MySector.SunProperties.EnvMapResolution)
                {
                    this.m_resolutionFilteredSlider.Value = MySector.SunProperties.EnvMapResolution;
                }
                if (v.Value != MySector.SunProperties.EnvMapResolution)
                {
                    v.Value = MySector.SunProperties.EnvMapResolution;
                }
            }, color);
            color = null;
            this.m_resolutionFilteredSlider = base.AddSlider("Filtered Resolution", (float) MySector.SunProperties.EnvMapFilteredResolution, 32f, 4096f, delegate (MyGuiControlSlider v) {
                MySector.SunProperties.EnvMapFilteredResolution = MathHelper.GetNearestBiggerPowerOfTwo((int) v.Value);
                if (MySector.SunProperties.EnvMapFilteredResolution > MySector.SunProperties.EnvMapResolution)
                {
                    this.m_resolutionSlider.Value = MySector.SunProperties.EnvMapFilteredResolution;
                }
                if (v.Value != MySector.SunProperties.EnvMapFilteredResolution)
                {
                    v.Value = MySector.SunProperties.EnvMapFilteredResolution;
                }
            }, color);
            color = null;
            this.AddSlider("Dim Distance", MySector.SunProperties.EnvironmentLight.ForwardDimDistance, 0.1f, 10f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.ForwardDimDistance = v.Value)), color);
            color = null;
            this.AddSlider("Minimum Ambient", MySector.SunProperties.EnvironmentLight.AmbientForwardPass, 0f, 1f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AmbientForwardPass = v.Value)), color);
            color = null;
            this.AddSlider("Ambient radius", MySector.SunProperties.EnvironmentLight.AmbientRadius, 0f, 100f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AmbientRadius = v.Value)), color);
            color = null;
            this.AddSlider("Ambient Gather radius", MySector.SunProperties.EnvironmentLight.AmbientLightsGatherRadius, 0f, 100f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AmbientLightsGatherRadius = v.Value)), color);
            color = null;
            this.AddSlider("Ambient Gather scale", MySector.SunProperties.EnvironmentProbe.AmbientScale, 0f, 1f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentProbe.AmbientScale = v.Value)), color);
            color = null;
            this.AddSlider("Ambient Gather Min clamp", MySector.SunProperties.EnvironmentProbe.AmbientMinClamp, 0f, 0.1f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentProbe.AmbientMinClamp = v.Value)), color);
            color = null;
            this.AddSlider("Ambient Gather Max clamp", MySector.SunProperties.EnvironmentProbe.AmbientMaxClamp, 0f, 1f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentProbe.AmbientMaxClamp = v.Value)), color);
            color = null;
            this.AddSlider("Atmosphere Intensity", MySector.SunProperties.EnvironmentLight.EnvAtmosphereBrightness, 0f, 5f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.EnvAtmosphereBrightness = v.Value)), color);
            color = null;
            this.AddSlider("Timeout", MySector.SunProperties.EnvironmentProbe.TimeOut, 0f, 10f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentProbe.TimeOut = v.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Render Blocks", MyRenderProxy.Settings.RenderBlocksToEnvProbe, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.RenderBlocksToEnvProbe = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Cubemap mipmap", (float) MyRenderProxy.Settings.DisplayEnvProbeMipLevel, 0f, 30f, (Action<MyGuiControlSlider>) (v => (MyRenderProxy.Settings.DisplayEnvProbeMipLevel = (int) v.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("DebugDisplay", MyRenderProxy.Settings.DisplayEnvProbe, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayEnvProbe = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("DebugDisplayFar", MyRenderProxy.Settings.DisplayEnvProbeFar, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayEnvProbeFar = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Use Intensity display", MyRenderProxy.Settings.DisplayEnvProbeIntensities, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayEnvProbeIntensities = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Skybox", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Screen Intensity", MySector.SunProperties.EnvironmentLight.SkyboxBrightness, 0f, 5f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.SkyboxBrightness = v.Value)), color);
            color = null;
            this.AddSlider("Environment Intensity", MySector.SunProperties.EnvironmentLight.EnvSkyboxBrightness, 0f, 50f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.EnvSkyboxBrightness = v.Value)), color);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderEnvironmentAmbient.<>c <>9 = new MyGuiScreenDebugRenderEnvironmentAmbient.<>c();
            public static Action<MyGuiControlSlider> <>9__5_0;
            public static Action<MyGuiControlCheckbox> <>9__5_1;
            public static Action<MyGuiControlSlider> <>9__5_2;
            public static Action<MyGuiControlCheckbox> <>9__5_3;
            public static Action<MyGuiControlSlider> <>9__5_4;
            public static Action<MyGuiControlSlider> <>9__5_5;
            public static Action<MyGuiControlSlider> <>9__5_8;
            public static Action<MyGuiControlSlider> <>9__5_9;
            public static Action<MyGuiControlSlider> <>9__5_10;
            public static Action<MyGuiControlSlider> <>9__5_11;
            public static Action<MyGuiControlSlider> <>9__5_12;
            public static Action<MyGuiControlSlider> <>9__5_13;
            public static Action<MyGuiControlSlider> <>9__5_14;
            public static Action<MyGuiControlSlider> <>9__5_15;
            public static Action<MyGuiControlSlider> <>9__5_16;
            public static Action<MyGuiControlCheckbox> <>9__5_17;
            public static Action<MyGuiControlSlider> <>9__5_18;
            public static Action<MyGuiControlCheckbox> <>9__5_19;
            public static Action<MyGuiControlCheckbox> <>9__5_20;
            public static Action<MyGuiControlCheckbox> <>9__5_21;
            public static Action<MyGuiControlSlider> <>9__5_22;
            public static Action<MyGuiControlSlider> <>9__5_23;

            internal void <RecreateControls>b__5_0(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AmbientDiffuseFactor = v.Value;
            }

            internal void <RecreateControls>b__5_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAmbientDiffuse = x.IsChecked;
            }

            internal void <RecreateControls>b__5_10(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AmbientRadius = v.Value;
            }

            internal void <RecreateControls>b__5_11(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AmbientLightsGatherRadius = v.Value;
            }

            internal void <RecreateControls>b__5_12(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentProbe.AmbientScale = v.Value;
            }

            internal void <RecreateControls>b__5_13(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentProbe.AmbientMinClamp = v.Value;
            }

            internal void <RecreateControls>b__5_14(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentProbe.AmbientMaxClamp = v.Value;
            }

            internal void <RecreateControls>b__5_15(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.EnvAtmosphereBrightness = v.Value;
            }

            internal void <RecreateControls>b__5_16(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentProbe.TimeOut = v.Value;
            }

            internal void <RecreateControls>b__5_17(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.RenderBlocksToEnvProbe = x.IsChecked;
            }

            internal void <RecreateControls>b__5_18(MyGuiControlSlider v)
            {
                MyRenderProxy.Settings.DisplayEnvProbeMipLevel = (int) v.Value;
            }

            internal void <RecreateControls>b__5_19(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayEnvProbe = x.IsChecked;
            }

            internal void <RecreateControls>b__5_2(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AmbientSpecularFactor = v.Value;
            }

            internal void <RecreateControls>b__5_20(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayEnvProbeFar = x.IsChecked;
            }

            internal void <RecreateControls>b__5_21(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayEnvProbeIntensities = x.IsChecked;
            }

            internal void <RecreateControls>b__5_22(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.SkyboxBrightness = v.Value;
            }

            internal void <RecreateControls>b__5_23(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.EnvSkyboxBrightness = v.Value;
            }

            internal void <RecreateControls>b__5_3(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAmbientSpecular = x.IsChecked;
            }

            internal void <RecreateControls>b__5_4(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.GlassAmbient = v.Value;
            }

            internal void <RecreateControls>b__5_5(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentProbe.DrawDistance = v.Value;
            }

            internal void <RecreateControls>b__5_8(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.ForwardDimDistance = v.Value;
            }

            internal void <RecreateControls>b__5_9(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AmbientForwardPass = v.Value;
            }
        }
    }
}

