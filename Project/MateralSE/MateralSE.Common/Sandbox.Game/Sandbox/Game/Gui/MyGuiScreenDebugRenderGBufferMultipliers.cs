namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "GBuffer Multipliers")]
    internal class MyGuiScreenDebugRenderGBufferMultipliers : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderGBufferMultipliers() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderGBufferMultipliers";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("GBuffer Multipliers", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("Multipliers", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("Albedo *", MySector.SunProperties.TextureMultipliers.AlbedoMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.AlbedoMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Albedo +", MySector.SunProperties.TextureMultipliers.AlbedoShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.AlbedoShift = x.Value)), color);
            color = null;
            this.AddSlider("Metalness *", MySector.SunProperties.TextureMultipliers.MetalnessMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.MetalnessMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Metalness +", MySector.SunProperties.TextureMultipliers.MetalnessShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.MetalnessShift = x.Value)), color);
            color = null;
            this.AddSlider("Gloss *", MySector.SunProperties.TextureMultipliers.GlossMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.GlossMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Gloss +", MySector.SunProperties.TextureMultipliers.GlossShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.GlossShift = x.Value)), color);
            color = null;
            this.AddSlider("AO *", MySector.SunProperties.TextureMultipliers.AoMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.AoMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("AO +", MySector.SunProperties.TextureMultipliers.AoShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.AoShift = x.Value)), color);
            color = null;
            this.AddSlider("Emissive *", MySector.SunProperties.TextureMultipliers.EmissiveMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.EmissiveMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Emissive +", MySector.SunProperties.TextureMultipliers.EmissiveShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.EmissiveShift = x.Value)), color);
            color = null;
            this.AddSlider("Color Mask *", MySector.SunProperties.TextureMultipliers.ColorMaskMultiplier, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.ColorMaskMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Color Mask +", MySector.SunProperties.TextureMultipliers.ColorMaskShift, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.ColorMaskShift = x.Value)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Colorize", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Color Hue", MySector.SunProperties.TextureMultipliers.ColorizeHSV.X, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.ColorizeHSV.X = x.Value)), color);
            color = null;
            this.AddSlider("Color Saturation", MySector.SunProperties.TextureMultipliers.ColorizeHSV.Y, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.ColorizeHSV.Y = x.Value)), color);
            color = null;
            this.AddSlider("Color Value", MySector.SunProperties.TextureMultipliers.ColorizeHSV.Z, -1f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.SunProperties.TextureMultipliers.ColorizeHSV.Z = x.Value)), color);
            base.AddLabel("Glass", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.AddColor("Color", MyRenderProxy.Settings.GlassColorMultiplier, delegate (MyGuiControlColor v) {
                Vector3 vector = (Vector3) v.Color;
                MyRenderProxy.Settings.GlassColorMultiplier.X = vector.X;
                MyRenderProxy.Settings.GlassColorMultiplier.Y = vector.Y;
                MyRenderProxy.Settings.GlassColorMultiplier.Z = vector.Z;
                MyRenderProxy.SetSettingsDirty();
            });
            color = null;
            this.AddSlider("Alpha", MyRenderProxy.Settings.GlassColorMultiplier.W, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GlassColorMultiplier.W = x.Value)), color);
            color = null;
            this.AddSlider("Reflectivity", MyRenderProxy.Settings.GlassReflectivityMultiplier, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GlassReflectivityMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Fresnel", MyRenderProxy.Settings.GlassFresnelMultiplier, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GlassFresnelMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("Gloss", MyRenderProxy.Settings.GlassGlossMultiplier, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GlassGlossMultiplier = x.Value)), color);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderGBufferMultipliers.<>c <>9 = new MyGuiScreenDebugRenderGBufferMultipliers.<>c();
            public static Action<MyGuiControlSlider> <>9__1_0;
            public static Action<MyGuiControlSlider> <>9__1_1;
            public static Action<MyGuiControlSlider> <>9__1_2;
            public static Action<MyGuiControlSlider> <>9__1_3;
            public static Action<MyGuiControlSlider> <>9__1_4;
            public static Action<MyGuiControlSlider> <>9__1_5;
            public static Action<MyGuiControlSlider> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlSlider> <>9__1_8;
            public static Action<MyGuiControlSlider> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;
            public static Action<MyGuiControlSlider> <>9__1_11;
            public static Action<MyGuiControlSlider> <>9__1_12;
            public static Action<MyGuiControlSlider> <>9__1_13;
            public static Action<MyGuiControlSlider> <>9__1_14;
            public static Action<MyGuiControlColor> <>9__1_15;
            public static Action<MyGuiControlSlider> <>9__1_16;
            public static Action<MyGuiControlSlider> <>9__1_17;
            public static Action<MyGuiControlSlider> <>9__1_18;
            public static Action<MyGuiControlSlider> <>9__1_19;

            internal void <RecreateControls>b__1_0(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.AlbedoMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_1(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.AlbedoShift = x.Value;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.ColorMaskMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.ColorMaskShift = x.Value;
            }

            internal void <RecreateControls>b__1_12(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.ColorizeHSV.X = x.Value;
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.ColorizeHSV.Y = x.Value;
            }

            internal void <RecreateControls>b__1_14(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.ColorizeHSV.Z = x.Value;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlColor v)
            {
                Vector3 color = (Vector3) v.Color;
                MyRenderProxy.Settings.GlassColorMultiplier.X = color.X;
                MyRenderProxy.Settings.GlassColorMultiplier.Y = color.Y;
                MyRenderProxy.Settings.GlassColorMultiplier.Z = color.Z;
                MyRenderProxy.SetSettingsDirty();
            }

            internal void <RecreateControls>b__1_16(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GlassColorMultiplier.W = x.Value;
            }

            internal void <RecreateControls>b__1_17(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GlassReflectivityMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_18(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GlassFresnelMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_19(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GlassGlossMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.MetalnessMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_3(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.MetalnessShift = x.Value;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.GlossMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.GlossShift = x.Value;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.AoMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.AoShift = x.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.EmissiveMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider x)
            {
                MySector.SunProperties.TextureMultipliers.EmissiveShift = x.Value;
            }
        }
    }
}

