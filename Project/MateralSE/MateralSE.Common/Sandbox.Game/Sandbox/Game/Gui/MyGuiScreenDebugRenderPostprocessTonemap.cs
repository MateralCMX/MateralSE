namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Postprocess Tonemap")]
    internal class MyGuiScreenDebugRenderPostprocessTonemap : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderPostprocessTonemap() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderPostprocessTonemap";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Postprocess Tonemap", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("Tonemapping", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Enable", (Func<bool>) (() => MyPostprocessSettingsWrapper.Settings.EnableTonemapping), (Action<bool>) (b => (MyPostprocessSettingsWrapper.Settings.EnableTonemapping = b)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display HDR Test", MyRenderProxy.Settings.DisplayHDRTest, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayHDRTest = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Constant Luminance", 0.0001f, 2f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.ConstantLuminance), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.ConstantLuminance = f)), color);
            color = null;
            this.AddSlider("Exposure", -5f, 5f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.LuminanceExposure), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.LuminanceExposure = f)), color);
            color = null;
            this.AddSlider("White Point", MyPostprocessSettingsWrapper.Settings.Data.WhitePoint, 0f, 15f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.WhitePoint = x.Value)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("Grain Size", (float) MyPostprocessSettingsWrapper.Settings.Data.GrainSize, 0f, 5f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.GrainSize = (int) x.Value)), color);
            color = null;
            this.AddSlider("Grain Amount", MyPostprocessSettingsWrapper.Settings.Data.GrainAmount, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.GrainAmount = x.Value)), color);
            color = null;
            this.AddSlider("Grain Strength", MyPostprocessSettingsWrapper.Settings.Data.GrainStrength, 0f, 0.5f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.GrainStrength = x.Value)), color);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            color = null;
            this.AddSlider("Chromatic Factor", MyPostprocessSettingsWrapper.Settings.Data.ChromaticFactor, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.ChromaticFactor = x.Value)), color);
            color = null;
            this.AddSlider("Chromatic Iterations", (float) MyPostprocessSettingsWrapper.Settings.ChromaticIterations, 1f, 15f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.ChromaticIterations = (int) x.Value)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            color = null;
            this.AddSlider("Vignette Start", MyPostprocessSettingsWrapper.Settings.Data.VignetteStart, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.VignetteStart = x.Value)), color);
            color = null;
            this.AddSlider("Vignette Length", MyPostprocessSettingsWrapper.Settings.Data.VignetteLength, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.VignetteLength = x.Value)), color);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            color = null;
            this.AddSlider("Saturation", 0f, 5f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.Saturation), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.Saturation = f)), color);
            color = null;
            this.AddSlider("Brightness", 0f, 5f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.Brightness), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.Brightness = f)), color);
            color = null;
            this.AddSlider("Brightness Factor R", 0f, 1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorR), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorR = f)), color);
            color = null;
            this.AddSlider("Brightness Factor G", 0f, 1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorG), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorG = f)), color);
            color = null;
            this.AddSlider("Brightness Factor B", 0f, 1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorB), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorB = f)), color);
            color = null;
            this.AddSlider("Contrast", 0f, 2f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.Contrast), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.Contrast = f)), color);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.01f;
            color = null;
            this.AddSlider("Vibrance", -1f, 1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.Vibrance), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.Vibrance = f)), color);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.01f;
            base.AddLabel("Sepia", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.AddColor("Light Color", (Color) MyPostprocessSettingsWrapper.Settings.Data.LightColor, (Action<MyGuiControlColor>) (v => (MyPostprocessSettingsWrapper.Settings.Data.LightColor = (Vector3) v.Color)));
            this.AddColor("Dark Color", (Color) MyPostprocessSettingsWrapper.Settings.Data.DarkColor, (Action<MyGuiControlColor>) (v => (MyPostprocessSettingsWrapper.Settings.Data.DarkColor = (Vector3) v.Color)));
            color = null;
            this.AddSlider("Sepia Strength", 0f, 1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength = f)), color);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderPostprocessTonemap.<>c <>9 = new MyGuiScreenDebugRenderPostprocessTonemap.<>c();
            public static Func<bool> <>9__1_0;
            public static Action<bool> <>9__1_1;
            public static Action<MyGuiControlCheckbox> <>9__1_2;
            public static Func<float> <>9__1_3;
            public static Action<float> <>9__1_4;
            public static Func<float> <>9__1_5;
            public static Action<float> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlSlider> <>9__1_8;
            public static Action<MyGuiControlSlider> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;
            public static Action<MyGuiControlSlider> <>9__1_11;
            public static Action<MyGuiControlSlider> <>9__1_12;
            public static Action<MyGuiControlSlider> <>9__1_13;
            public static Action<MyGuiControlSlider> <>9__1_14;
            public static Func<float> <>9__1_15;
            public static Action<float> <>9__1_16;
            public static Func<float> <>9__1_17;
            public static Action<float> <>9__1_18;
            public static Func<float> <>9__1_19;
            public static Action<float> <>9__1_20;
            public static Func<float> <>9__1_21;
            public static Action<float> <>9__1_22;
            public static Func<float> <>9__1_23;
            public static Action<float> <>9__1_24;
            public static Func<float> <>9__1_25;
            public static Action<float> <>9__1_26;
            public static Func<float> <>9__1_27;
            public static Action<float> <>9__1_28;
            public static Action<MyGuiControlColor> <>9__1_29;
            public static Action<MyGuiControlColor> <>9__1_30;
            public static Func<float> <>9__1_31;
            public static Action<float> <>9__1_32;

            internal bool <RecreateControls>b__1_0() => 
                MyPostprocessSettingsWrapper.Settings.EnableTonemapping;

            internal void <RecreateControls>b__1_1(bool b)
            {
                MyPostprocessSettingsWrapper.Settings.EnableTonemapping = b;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.GrainStrength = x.Value;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.ChromaticFactor = x.Value;
            }

            internal void <RecreateControls>b__1_12(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.ChromaticIterations = (int) x.Value;
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.VignetteStart = x.Value;
            }

            internal void <RecreateControls>b__1_14(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.VignetteLength = x.Value;
            }

            internal float <RecreateControls>b__1_15() => 
                MyPostprocessSettingsWrapper.Settings.Data.Saturation;

            internal void <RecreateControls>b__1_16(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.Saturation = f;
            }

            internal float <RecreateControls>b__1_17() => 
                MyPostprocessSettingsWrapper.Settings.Data.Brightness;

            internal void <RecreateControls>b__1_18(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.Brightness = f;
            }

            internal float <RecreateControls>b__1_19() => 
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorR;

            internal void <RecreateControls>b__1_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayHDRTest = x.IsChecked;
            }

            internal void <RecreateControls>b__1_20(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorR = f;
            }

            internal float <RecreateControls>b__1_21() => 
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorG;

            internal void <RecreateControls>b__1_22(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorG = f;
            }

            internal float <RecreateControls>b__1_23() => 
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorB;

            internal void <RecreateControls>b__1_24(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BrightnessFactorB = f;
            }

            internal float <RecreateControls>b__1_25() => 
                MyPostprocessSettingsWrapper.Settings.Data.Contrast;

            internal void <RecreateControls>b__1_26(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.Contrast = f;
            }

            internal float <RecreateControls>b__1_27() => 
                MyPostprocessSettingsWrapper.Settings.Data.Vibrance;

            internal void <RecreateControls>b__1_28(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.Vibrance = f;
            }

            internal void <RecreateControls>b__1_29(MyGuiControlColor v)
            {
                MyPostprocessSettingsWrapper.Settings.Data.LightColor = (Vector3) v.Color;
            }

            internal float <RecreateControls>b__1_3() => 
                MyPostprocessSettingsWrapper.Settings.Data.ConstantLuminance;

            internal void <RecreateControls>b__1_30(MyGuiControlColor v)
            {
                MyPostprocessSettingsWrapper.Settings.Data.DarkColor = (Vector3) v.Color;
            }

            internal float <RecreateControls>b__1_31() => 
                MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength;

            internal void <RecreateControls>b__1_32(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.SepiaStrength = f;
            }

            internal void <RecreateControls>b__1_4(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.ConstantLuminance = f;
            }

            internal float <RecreateControls>b__1_5() => 
                MyPostprocessSettingsWrapper.Settings.Data.LuminanceExposure;

            internal void <RecreateControls>b__1_6(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.LuminanceExposure = f;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.WhitePoint = x.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.GrainSize = (int) x.Value;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.GrainAmount = x.Value;
            }
        }
    }
}

