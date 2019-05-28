namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Postprocess HBAO")]
    internal class MyGuiScreenDebugRenderPostprocessHBAO : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderPostprocessHBAO() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderPostprocessHBAO";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Postprocess HBAO", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Use HBAO", MySector.HBAOSettings.Enabled, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.Enabled = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show only HBAO", MyRenderProxy.Settings.DisplayAO, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAO = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show Normals", MyRenderProxy.Settings.DisplayNormals, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayNormals = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("Radius", MySector.HBAOSettings.Radius, 0f, 5f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.Radius = state.Value)), color);
            color = null;
            this.AddSlider("Bias", MySector.HBAOSettings.Bias, 0f, 0.5f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.Bias = state.Value)), color);
            color = null;
            this.AddSlider("SmallScaleAO", MySector.HBAOSettings.SmallScaleAO, 0f, 4f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.SmallScaleAO = state.Value)), color);
            color = null;
            this.AddSlider("LargeScaleAO", MySector.HBAOSettings.LargeScaleAO, 0f, 4f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.LargeScaleAO = state.Value)), color);
            color = null;
            this.AddSlider("PowerExponent", MySector.HBAOSettings.PowerExponent, 1f, 8f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.PowerExponent = state.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Use Normals", MySector.HBAOSettings.UseGBufferNormals, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.UseGBufferNormals = state.IsChecked)), true, null, color, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("ForegroundAOEnable", MySector.HBAOSettings.ForegroundAOEnable, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.ForegroundAOEnable = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("ForegroundViewDepth", MySector.HBAOSettings.ForegroundViewDepth, 0f, 1000f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.ForegroundViewDepth = state.Value)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("BackgroundAOEnable", MySector.HBAOSettings.BackgroundAOEnable, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.BackgroundAOEnable = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("AdaptToFOV", MySector.HBAOSettings.AdaptToFOV, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.AdaptToFOV = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("BackgroundViewDepth", MySector.HBAOSettings.BackgroundViewDepth, 0f, 1000f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.BackgroundViewDepth = state.Value)), color);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("DepthClampToEdge", MySector.HBAOSettings.DepthClampToEdge, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.DepthClampToEdge = state.IsChecked)), true, null, color, captionOffset);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("DepthThresholdEnable", MySector.HBAOSettings.DepthThresholdEnable, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.DepthThresholdEnable = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("DepthThreshold", MySector.HBAOSettings.DepthThreshold, 0f, 1000f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.DepthThreshold = state.Value)), color);
            color = null;
            this.AddSlider("DepthThresholdSharpness", MySector.HBAOSettings.DepthThresholdSharpness, 0f, 500f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.DepthThresholdSharpness = state.Value)), color);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Use blur", MySector.HBAOSettings.BlurEnable, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.BlurEnable = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Radius 4", MySector.HBAOSettings.BlurRadius4, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.BlurRadius4 = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Sharpness", MySector.HBAOSettings.BlurSharpness, 0f, 100f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.BlurSharpness = state.Value)), color);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Blur Sharpness Function", MySector.HBAOSettings.BlurSharpnessFunctionEnable, (Action<MyGuiControlCheckbox>) (state => (MySector.HBAOSettings.BlurSharpnessFunctionEnable = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("ForegroundScale", MySector.HBAOSettings.BlurSharpnessFunctionForegroundScale, 0f, 100f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.BlurSharpnessFunctionForegroundScale = state.Value)), color);
            color = null;
            this.AddSlider("ForegroundViewDepth", MySector.HBAOSettings.BlurSharpnessFunctionForegroundViewDepth, 0f, 1f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.BlurSharpnessFunctionForegroundViewDepth = state.Value)), color);
            color = null;
            this.AddSlider("BackgroundViewDepth", MySector.HBAOSettings.BlurSharpnessFunctionBackgroundViewDepth, 0f, 1f, (Action<MyGuiControlSlider>) (state => (MySector.HBAOSettings.BlurSharpnessFunctionBackgroundViewDepth = state.Value)), color);
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] += 0.01f;
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderPostprocessHBAO.<>c <>9 = new MyGuiScreenDebugRenderPostprocessHBAO.<>c();
            public static Action<MyGuiControlCheckbox> <>9__1_0;
            public static Action<MyGuiControlCheckbox> <>9__1_1;
            public static Action<MyGuiControlCheckbox> <>9__1_2;
            public static Action<MyGuiControlSlider> <>9__1_3;
            public static Action<MyGuiControlSlider> <>9__1_4;
            public static Action<MyGuiControlSlider> <>9__1_5;
            public static Action<MyGuiControlSlider> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlCheckbox> <>9__1_8;
            public static Action<MyGuiControlCheckbox> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;
            public static Action<MyGuiControlCheckbox> <>9__1_11;
            public static Action<MyGuiControlCheckbox> <>9__1_12;
            public static Action<MyGuiControlSlider> <>9__1_13;
            public static Action<MyGuiControlCheckbox> <>9__1_14;
            public static Action<MyGuiControlCheckbox> <>9__1_15;
            public static Action<MyGuiControlSlider> <>9__1_16;
            public static Action<MyGuiControlSlider> <>9__1_17;
            public static Action<MyGuiControlCheckbox> <>9__1_18;
            public static Action<MyGuiControlCheckbox> <>9__1_19;
            public static Action<MyGuiControlSlider> <>9__1_20;
            public static Action<MyGuiControlCheckbox> <>9__1_21;
            public static Action<MyGuiControlSlider> <>9__1_22;
            public static Action<MyGuiControlSlider> <>9__1_23;
            public static Action<MyGuiControlSlider> <>9__1_24;

            internal void <RecreateControls>b__1_0(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.Enabled = state.IsChecked;
            }

            internal void <RecreateControls>b__1_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAO = x.IsChecked;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.ForegroundViewDepth = state.Value;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.BackgroundAOEnable = state.IsChecked;
            }

            internal void <RecreateControls>b__1_12(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.AdaptToFOV = state.IsChecked;
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.BackgroundViewDepth = state.Value;
            }

            internal void <RecreateControls>b__1_14(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.DepthClampToEdge = state.IsChecked;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.DepthThresholdEnable = state.IsChecked;
            }

            internal void <RecreateControls>b__1_16(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.DepthThreshold = state.Value;
            }

            internal void <RecreateControls>b__1_17(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.DepthThresholdSharpness = state.Value;
            }

            internal void <RecreateControls>b__1_18(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.BlurEnable = state.IsChecked;
            }

            internal void <RecreateControls>b__1_19(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.BlurRadius4 = state.IsChecked;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayNormals = x.IsChecked;
            }

            internal void <RecreateControls>b__1_20(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.BlurSharpness = state.Value;
            }

            internal void <RecreateControls>b__1_21(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.BlurSharpnessFunctionEnable = state.IsChecked;
            }

            internal void <RecreateControls>b__1_22(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.BlurSharpnessFunctionForegroundScale = state.Value;
            }

            internal void <RecreateControls>b__1_23(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.BlurSharpnessFunctionForegroundViewDepth = state.Value;
            }

            internal void <RecreateControls>b__1_24(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.BlurSharpnessFunctionBackgroundViewDepth = state.Value;
            }

            internal void <RecreateControls>b__1_3(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.Radius = state.Value;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.Bias = state.Value;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.SmallScaleAO = state.Value;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.LargeScaleAO = state.Value;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider state)
            {
                MySector.HBAOSettings.PowerExponent = state.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.UseGBufferNormals = state.IsChecked;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlCheckbox state)
            {
                MySector.HBAOSettings.ForegroundAOEnable = state.IsChecked;
            }
        }
    }
}

