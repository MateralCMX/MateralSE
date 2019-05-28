namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Render", "Environment Shadows")]
    internal class MyGuiScreenDebugRenderEnvironmentShadows : MyGuiScreenDebugBase
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;

        public MyGuiScreenDebugRenderEnvironmentShadows() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderEnvironmentShadows";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Environment Shadows", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            MySunProperties sunProperties = MySector.SunProperties;
            base.AddLabel("Sun", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("Time of day", 0f, MySession.Static.Settings.SunRotationIntervalMinutes, () => MyTimeOfDayHelper.TimeOfDay, new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDay), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("Shadow fadeout", MySector.SunProperties.EnvironmentLight.ShadowFadeoutMultiplier, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.EnvironmentLight.ShadowFadeoutMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Env Shadow fadeout", MySector.SunProperties.EnvironmentLight.EnvShadowFadeoutMultiplier, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.EnvironmentLight.EnvShadowFadeoutMultiplier = x.Value)), color);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Ambient Occlusion", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("IndirectLight", MySector.SunProperties.EnvironmentLight.AOIndirectLight, 0f, 2f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AOIndirectLight = v.Value)), color);
            color = null;
            this.AddSlider("DirLight", MySector.SunProperties.EnvironmentLight.AODirLight, 0f, 2f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AODirLight = v.Value)), color);
            color = null;
            this.AddSlider("AOPointLight", MySector.SunProperties.EnvironmentLight.AOPointLight, 0f, 2f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AOPointLight = v.Value)), color);
            color = null;
            this.AddSlider("AOSpotLight", MySector.SunProperties.EnvironmentLight.AOSpotLight, 0f, 2f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.AOSpotLight = v.Value)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderEnvironmentShadows.<>c <>9 = new MyGuiScreenDebugRenderEnvironmentShadows.<>c();
            public static Func<float> <>9__3_0;
            public static Action<MyGuiControlSlider> <>9__3_1;
            public static Action<MyGuiControlSlider> <>9__3_2;
            public static Action<MyGuiControlSlider> <>9__3_3;
            public static Action<MyGuiControlSlider> <>9__3_4;
            public static Action<MyGuiControlSlider> <>9__3_5;
            public static Action<MyGuiControlSlider> <>9__3_6;

            internal float <RecreateControls>b__3_0() => 
                MyTimeOfDayHelper.TimeOfDay;

            internal void <RecreateControls>b__3_1(MyGuiControlSlider x)
            {
                MySector.SunProperties.EnvironmentLight.ShadowFadeoutMultiplier = x.Value;
            }

            internal void <RecreateControls>b__3_2(MyGuiControlSlider x)
            {
                MySector.SunProperties.EnvironmentLight.EnvShadowFadeoutMultiplier = x.Value;
            }

            internal void <RecreateControls>b__3_3(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AOIndirectLight = v.Value;
            }

            internal void <RecreateControls>b__3_4(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AODirLight = v.Value;
            }

            internal void <RecreateControls>b__3_5(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AOPointLight = v.Value;
            }

            internal void <RecreateControls>b__3_6(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.AOSpotLight = v.Value;
            }
        }
    }
}

