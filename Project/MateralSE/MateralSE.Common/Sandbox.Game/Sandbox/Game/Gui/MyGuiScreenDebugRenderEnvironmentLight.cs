namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    [MyDebugScreen("Render", "Environment Light")]
    internal class MyGuiScreenDebugRenderEnvironmentLight : MyGuiScreenDebugBase
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;

        public MyGuiScreenDebugRenderEnvironmentLight() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderEnvironmentLight";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Environment Light", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            MySunProperties sunProperties = MySector.SunProperties;
            base.AddLabel("Sun", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("Time of day", 0f, MySession.Static.Settings.SunRotationIntervalMinutes, () => MyTimeOfDayHelper.TimeOfDay, new Action<float>(MyTimeOfDayHelper.UpdateTimeOfDay), color);
            color = null;
            this.AddSlider("Intensity", MySector.SunProperties.SunIntensity, 0f, 1000f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.SunIntensity = v.Value)), color);
            this.AddColor("Color", (Color) MySector.SunProperties.EnvironmentLight.SunColor, (Action<MyGuiControlColor>) (v => (MySector.SunProperties.EnvironmentLight.SunColor = (Vector3) v.Color)));
            this.AddColor("Specular Color", (Color) MySector.SunProperties.EnvironmentLight.SunSpecularColor, (Action<MyGuiControlColor>) (v => (MySector.SunProperties.EnvironmentLight.SunSpecularColor = (Vector3) v.Color)));
            color = null;
            this.AddSlider("Specular factor", MySector.SunProperties.EnvironmentLight.SunSpecularFactor, 0f, 1f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.SunSpecularFactor = v.Value)), color);
            color = null;
            this.AddSlider("Gloss factor", MySector.SunProperties.EnvironmentLight.SunGlossFactor, 0f, 5f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.SunGlossFactor = v.Value)), color);
            color = null;
            this.AddSlider("Diffuse factor", MySector.SunProperties.EnvironmentLight.SunDiffuseFactor, 0f, 10f, (Action<MyGuiControlSlider>) (v => (MySector.SunProperties.EnvironmentLight.SunDiffuseFactor = v.Value)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderEnvironmentLight.<>c <>9 = new MyGuiScreenDebugRenderEnvironmentLight.<>c();
            public static Func<float> <>9__3_0;
            public static Action<MyGuiControlSlider> <>9__3_1;
            public static Action<MyGuiControlColor> <>9__3_2;
            public static Action<MyGuiControlColor> <>9__3_3;
            public static Action<MyGuiControlSlider> <>9__3_4;
            public static Action<MyGuiControlSlider> <>9__3_5;
            public static Action<MyGuiControlSlider> <>9__3_6;

            internal float <RecreateControls>b__3_0() => 
                MyTimeOfDayHelper.TimeOfDay;

            internal void <RecreateControls>b__3_1(MyGuiControlSlider v)
            {
                MySector.SunProperties.SunIntensity = v.Value;
            }

            internal void <RecreateControls>b__3_2(MyGuiControlColor v)
            {
                MySector.SunProperties.EnvironmentLight.SunColor = (Vector3) v.Color;
            }

            internal void <RecreateControls>b__3_3(MyGuiControlColor v)
            {
                MySector.SunProperties.EnvironmentLight.SunSpecularColor = (Vector3) v.Color;
            }

            internal void <RecreateControls>b__3_4(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.SunSpecularFactor = v.Value;
            }

            internal void <RecreateControls>b__3_5(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.SunGlossFactor = v.Value;
            }

            internal void <RecreateControls>b__3_6(MyGuiControlSlider v)
            {
                MySector.SunProperties.EnvironmentLight.SunDiffuseFactor = v.Value;
            }
        }
    }
}

