namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Postprocess SSAO")]
    internal class MyGuiScreenDebugRenderPostprocessSSAO : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderPostprocessSSAO() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderPostprocessSSAO";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2? captionOffset = null;
            base.AddCaption("Postprocess SSAO", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Use SSAO", MySector.SSAOSettings.Enabled, (Action<MyGuiControlCheckbox>) (state => (MySector.SSAOSettings.Enabled = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Use blur", MySector.SSAOSettings.UseBlur, (Action<MyGuiControlCheckbox>) (state => (MySector.SSAOSettings.UseBlur = state.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show only SSAO", MyRenderProxy.Settings.DisplayAO, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAO = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("MinRadius", MySector.SSAOSettings.Data.MinRadius, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.MinRadius = state.Value)), color);
            color = null;
            this.AddSlider("MaxRadius", MySector.SSAOSettings.Data.MaxRadius, 0f, 1000f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.MaxRadius = state.Value)), color);
            color = null;
            this.AddSlider("RadiusGrowZScale", MySector.SSAOSettings.Data.RadiusGrowZScale, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.RadiusGrowZScale = state.Value)), color);
            color = null;
            this.AddSlider("Falloff", MySector.SSAOSettings.Data.Falloff, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.Falloff = state.Value)), color);
            color = null;
            this.AddSlider("RadiusBias", MySector.SSAOSettings.Data.RadiusBias, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.RadiusBias = state.Value)), color);
            color = null;
            this.AddSlider("Contrast", MySector.SSAOSettings.Data.Contrast, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.Contrast = state.Value)), color);
            color = null;
            this.AddSlider("Normalization", MySector.SSAOSettings.Data.Normalization, 0f, 10f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.Normalization = state.Value)), color);
            color = null;
            this.AddSlider("ColorScale", MySector.SSAOSettings.Data.ColorScale, 0f, 1f, (Action<MyGuiControlSlider>) (state => (MySector.SSAOSettings.Data.ColorScale = state.Value)), color);
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
            public static readonly MyGuiScreenDebugRenderPostprocessSSAO.<>c <>9 = new MyGuiScreenDebugRenderPostprocessSSAO.<>c();
            public static Action<MyGuiControlCheckbox> <>9__1_0;
            public static Action<MyGuiControlCheckbox> <>9__1_1;
            public static Action<MyGuiControlCheckbox> <>9__1_2;
            public static Action<MyGuiControlSlider> <>9__1_3;
            public static Action<MyGuiControlSlider> <>9__1_4;
            public static Action<MyGuiControlSlider> <>9__1_5;
            public static Action<MyGuiControlSlider> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlSlider> <>9__1_8;
            public static Action<MyGuiControlSlider> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;

            internal void <RecreateControls>b__1_0(MyGuiControlCheckbox state)
            {
                MySector.SSAOSettings.Enabled = state.IsChecked;
            }

            internal void <RecreateControls>b__1_1(MyGuiControlCheckbox state)
            {
                MySector.SSAOSettings.UseBlur = state.IsChecked;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.ColorScale = state.Value;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAO = x.IsChecked;
            }

            internal void <RecreateControls>b__1_3(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.MinRadius = state.Value;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.MaxRadius = state.Value;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.RadiusGrowZScale = state.Value;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.Falloff = state.Value;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.RadiusBias = state.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.Contrast = state.Value;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider state)
            {
                MySector.SSAOSettings.Data.Normalization = state.Value;
            }
        }
    }
}

