namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [MyDebugScreen("Render", "Environment Fog")]
    internal class MyGuiScreenDebugRenderEnvironmentFog : MyGuiScreenDebugBase
    {
        private static float timeOfDay;
        private static TimeSpan? OriginalTime;

        public MyGuiScreenDebugRenderEnvironmentFog() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderEnvironmentFog";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Environment Fog", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            this.AddSlider("Fog multiplier", MySector.FogProperties.FogMultiplier, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.FogProperties.FogMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Fog density", MySector.FogProperties.FogDensity, 0f, 0.01f, (Action<MyGuiControlSlider>) (x => (MySector.FogProperties.FogDensity = x.Value)), color);
            this.AddColor("Fog color", (Color) MySector.FogProperties.FogColor, (Action<MyGuiControlColor>) (x => (MySector.FogProperties.FogColor = (Vector3) x.Color)));
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderFogSettings settings = new MyRenderFogSettings {
                FogMultiplier = MySector.FogProperties.FogMultiplier,
                FogColor = MySector.FogProperties.FogColor,
                FogDensity = MySector.FogProperties.FogDensity
            };
            MyRenderProxy.UpdateFogSettings(ref settings);
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderEnvironmentFog.<>c <>9 = new MyGuiScreenDebugRenderEnvironmentFog.<>c();
            public static Action<MyGuiControlSlider> <>9__3_0;
            public static Action<MyGuiControlSlider> <>9__3_1;
            public static Action<MyGuiControlColor> <>9__3_2;

            internal void <RecreateControls>b__3_0(MyGuiControlSlider x)
            {
                MySector.FogProperties.FogMultiplier = x.Value;
            }

            internal void <RecreateControls>b__3_1(MyGuiControlSlider x)
            {
                MySector.FogProperties.FogDensity = x.Value;
            }

            internal void <RecreateControls>b__3_2(MyGuiControlColor x)
            {
                MySector.FogProperties.FogColor = (Vector3) x.Color;
            }
        }
    }
}

