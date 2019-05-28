namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Postprocess Bloom")]
    internal class MyGuiScreenDebugRenderPostprocessBloom : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderPostprocessBloom() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderPostprocessBloom";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Postprocess Bloom", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            base.AddLabel("Bloom", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Enabled", MyPostprocessSettingsWrapper.Settings.BloomEnabled, (Action<MyGuiControlCheckbox>) (x => (MyPostprocessSettingsWrapper.Settings.BloomEnabled = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display filter", MyRenderProxy.Settings.DisplayBloomFilter, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayBloomFilter = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display min", MyRenderProxy.Settings.DisplayBloomMin, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayBloomMin = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Exposure", 0f, 10f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomExposure), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomExposure = f)), color);
            color = null;
            this.AddSlider("Luma threshold", 0f, 100f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = f)), color);
            color = null;
            this.AddSlider("Emissiveness", 0f, 400f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomEmissiveness), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomEmissiveness = f)), color);
            color = null;
            this.AddSlider("Size", 0f, 10f, (Func<float>) (() => ((float) MyPostprocessSettingsWrapper.Settings.BloomSize)), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.BloomSize = (int) f)), color);
            color = null;
            this.AddSlider("Depth slope", 0f, 5f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomDepthSlope), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomDepthSlope = f)), color);
            color = null;
            this.AddSlider("Depth strength", 0f, 4f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomDepthStrength), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomDepthStrength = f)), color);
            color = null;
            this.AddSlider("Dirt/Bloom Ratio", MyPostprocessSettingsWrapper.Settings.Data.BloomDirtRatio, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MyPostprocessSettingsWrapper.Settings.Data.BloomDirtRatio = x.Value)), color);
            color = null;
            this.AddSlider("Magnitude", 0f, 0.1f, (Func<float>) (() => MyPostprocessSettingsWrapper.Settings.Data.BloomMult), (Action<float>) (f => (MyPostprocessSettingsWrapper.Settings.Data.BloomMult = f)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("High Quality Bloom", MyPostprocessSettingsWrapper.Settings.HighQualityBloom, (Action<MyGuiControlCheckbox>) (x => (MyPostprocessSettingsWrapper.Settings.HighQualityBloom = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("AntiFlicker Filter", MyPostprocessSettingsWrapper.Settings.BloomAntiFlickerFilter, (Action<MyGuiControlCheckbox>) (x => (MyPostprocessSettingsWrapper.Settings.BloomAntiFlickerFilter = x.IsChecked)), true, null, color, captionOffset);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
            MyRenderProxy.UpdateDebugOverrides();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderPostprocessBloom.<>c <>9 = new MyGuiScreenDebugRenderPostprocessBloom.<>c();
            public static Action<MyGuiControlCheckbox> <>9__1_0;
            public static Action<MyGuiControlCheckbox> <>9__1_1;
            public static Action<MyGuiControlCheckbox> <>9__1_2;
            public static Func<float> <>9__1_3;
            public static Action<float> <>9__1_4;
            public static Func<float> <>9__1_5;
            public static Action<float> <>9__1_6;
            public static Func<float> <>9__1_7;
            public static Action<float> <>9__1_8;
            public static Func<float> <>9__1_9;
            public static Action<float> <>9__1_10;
            public static Func<float> <>9__1_11;
            public static Action<float> <>9__1_12;
            public static Func<float> <>9__1_13;
            public static Action<float> <>9__1_14;
            public static Action<MyGuiControlSlider> <>9__1_15;
            public static Func<float> <>9__1_16;
            public static Action<float> <>9__1_17;
            public static Action<MyGuiControlCheckbox> <>9__1_18;
            public static Action<MyGuiControlCheckbox> <>9__1_19;

            internal void <RecreateControls>b__1_0(MyGuiControlCheckbox x)
            {
                MyPostprocessSettingsWrapper.Settings.BloomEnabled = x.IsChecked;
            }

            internal void <RecreateControls>b__1_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayBloomFilter = x.IsChecked;
            }

            internal void <RecreateControls>b__1_10(float f)
            {
                MyPostprocessSettingsWrapper.Settings.BloomSize = (int) f;
            }

            internal float <RecreateControls>b__1_11() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomDepthSlope;

            internal void <RecreateControls>b__1_12(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomDepthSlope = f;
            }

            internal float <RecreateControls>b__1_13() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomDepthStrength;

            internal void <RecreateControls>b__1_14(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomDepthStrength = f;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlSlider x)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomDirtRatio = x.Value;
            }

            internal float <RecreateControls>b__1_16() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomMult;

            internal void <RecreateControls>b__1_17(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomMult = f;
            }

            internal void <RecreateControls>b__1_18(MyGuiControlCheckbox x)
            {
                MyPostprocessSettingsWrapper.Settings.HighQualityBloom = x.IsChecked;
            }

            internal void <RecreateControls>b__1_19(MyGuiControlCheckbox x)
            {
                MyPostprocessSettingsWrapper.Settings.BloomAntiFlickerFilter = x.IsChecked;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayBloomMin = x.IsChecked;
            }

            internal float <RecreateControls>b__1_3() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomExposure;

            internal void <RecreateControls>b__1_4(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomExposure = f;
            }

            internal float <RecreateControls>b__1_5() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold;

            internal void <RecreateControls>b__1_6(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomLumaThreshold = f;
            }

            internal float <RecreateControls>b__1_7() => 
                MyPostprocessSettingsWrapper.Settings.Data.BloomEmissiveness;

            internal void <RecreateControls>b__1_8(float f)
            {
                MyPostprocessSettingsWrapper.Settings.Data.BloomEmissiveness = f;
            }

            internal float <RecreateControls>b__1_9() => 
                ((float) MyPostprocessSettingsWrapper.Settings.BloomSize);
        }
    }
}

