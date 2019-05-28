namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Postprocess Eye Adaptation")]
    internal class MyGuiScreenDebugRenderPostprocessEyeAdaptation : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderPostprocessEyeAdaptation() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderPostprocessEyeAdaptation";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Postprocess Eye Adaptation", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("Eye adaptation", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Enable", (Func<bool>) (() => MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation), (Action<bool>) (b => (MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = b)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Tau", 0f, 10f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationTau), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationTau = f)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display Histogram", MyRenderProxy.Settings.DisplayHistogram, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayHistogram = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display HDR intensity", MyRenderProxy.Settings.DisplayHdrIntensity, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayHdrIntensity = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("Histogram Log Min", MyPostprocessSettingsWrapper.Settings.HistogramLogMin, -8f, 8f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramLogMin = x.Value)), color);
            color = null;
            this.AddSlider("Histogram Log Max", MyPostprocessSettingsWrapper.Settings.HistogramLogMax, -8f, 8f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramLogMax = x.Value)), color);
            color = null;
            this.AddSlider("Histogram Filter Min", MyPostprocessSettingsWrapper.Settings.HistogramFilterMin, 0f, 100f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramFilterMin = x.Value)), color);
            color = null;
            this.AddSlider("Histogram Filter Max", MyPostprocessSettingsWrapper.Settings.HistogramFilterMax, 0f, 100f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramFilterMax = x.Value)), color);
            color = null;
            this.AddSlider("Min Eye Adaptation Log Brightness", MyPostprocessSettingsWrapper.Settings.MinEyeAdaptationLogBrightness, -8f, 8f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.MinEyeAdaptationLogBrightness = x.Value)), color);
            color = null;
            this.AddSlider("Max Eye Adaptation Log Brightness", MyPostprocessSettingsWrapper.Settings.MaxEyeAdaptationLogBrightness, -8f, 8f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.MaxEyeAdaptationLogBrightness = x.Value)), color);
            color = null;
            this.AddSlider("Adaptation Speed Up", MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedUp, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedUp = x.Value)), color);
            color = null;
            this.AddSlider("Adaptation Speed Down", MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedDown, 0f, 4f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedDown = x.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Prioritize Screen Center", MyPostprocessSettingsWrapper.Settings.EyeAdaptationPrioritizeScreenCenter, (Action<MyGuiControlCheckbox>) (x => (MyPostprocessSettingsWrapper.Settings.EyeAdaptationPrioritizeScreenCenter = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Histogram Luminance Threshold", MyPostprocessSettingsWrapper.Settings.HistogramLuminanceThreshold, 0f, 0.5f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramLuminanceThreshold = x.Value)), color);
            color = null;
            this.AddSlider("Histogram Skybox Factor", MyPostprocessSettingsWrapper.Settings.HistogramSkyboxFactor, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.HistogramSkyboxFactor = x.Value)), color);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderPostprocessEyeAdaptation.<>c <>9 = new MyGuiScreenDebugRenderPostprocessEyeAdaptation.<>c();
            public static Func<bool> <>9__1_0;
            public static Action<bool> <>9__1_1;
            public static Func<float> <>9__1_2;
            public static Action<float> <>9__1_3;
            public static Action<MyGuiControlCheckbox> <>9__1_4;
            public static Action<MyGuiControlCheckbox> <>9__1_5;
            public static Action<MyGuiControlSlider> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlSlider> <>9__1_8;
            public static Action<MyGuiControlSlider> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;
            public static Action<MyGuiControlSlider> <>9__1_11;
            public static Action<MyGuiControlSlider> <>9__1_12;
            public static Action<MyGuiControlSlider> <>9__1_13;
            public static Action<MyGuiControlCheckbox> <>9__1_14;
            public static Action<MyGuiControlSlider> <>9__1_15;
            public static Action<MyGuiControlSlider> <>9__1_16;

            internal bool <RecreateControls>b__1_0() => 
                MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation;

            internal void <RecreateControls>b__1_1(bool b)
            {
                MyPostprocessSettingsWrapper.Settings.EnableEyeAdaptation = b;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.MinEyeAdaptationLogBrightness = x.Value;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.MaxEyeAdaptationLogBrightness = x.Value;
            }

            internal void <RecreateControls>b__1_12(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedUp = x.Value;
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationSpeedDown = x.Value;
            }

            internal void <RecreateControls>b__1_14(MyGuiControlCheckbox x)
            {
                MyPostprocessSettingsWrapper.Settings.EyeAdaptationPrioritizeScreenCenter = x.IsChecked;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramLuminanceThreshold = x.Value;
            }

            internal void <RecreateControls>b__1_16(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramSkyboxFactor = x.Value;
            }

            internal float <RecreateControls>b__1_2() => 
                MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationTau;

            internal void <RecreateControls>b__1_3(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.EyeAdaptationTau = f;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayHistogram = x.IsChecked;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayHdrIntensity = x.IsChecked;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramLogMin = x.Value;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramLogMax = x.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramFilterMin = x.Value;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.HistogramFilterMax = x.Value;
            }
        }
    }
}

