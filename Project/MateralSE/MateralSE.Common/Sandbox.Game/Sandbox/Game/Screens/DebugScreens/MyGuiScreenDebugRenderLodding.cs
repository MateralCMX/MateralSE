namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Gui;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Lodding")]
    public class MyGuiScreenDebugRenderLodding : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderLodding() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderLodding";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Lodding", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("The new pipeline - lod shift", (Vector4) Color.White, 1f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("GBuffer", (float) MySector.Lodding.CurrentSettings.GBuffer.LodShift, 0f, 6f, delegate (MyGuiControlSlider x) {
                MySector.Lodding.CurrentSettings.GBuffer.LodShift = (int) Math.Round((double) x.Value);
                MySector.Lodding.CurrentSettings.GBuffer.LodShiftVisible = (int) Math.Round((double) x.Value);
            }, color);
            if (MySector.Lodding.CurrentSettings.CascadeDepths.Length >= 3)
            {
                color = null;
                this.AddSlider("CSM_0 Visible in gbuffer", (float) MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShiftVisible, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShiftVisible = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_0", (float) MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShift, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShift = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_1 Visible in gbuffer", (float) MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShiftVisible, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShiftVisible = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_1", (float) MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShift, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShift = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_2 Visible in gbuffer", (float) MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShiftVisible, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShiftVisible = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_2", (float) MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShift, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShift = (int) Math.Round((double) x.Value))), color);
            }
            color = null;
            this.AddSlider("Single depth visible in gbuffer", (float) MySector.Lodding.CurrentSettings.SingleDepth.LodShiftVisible, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.SingleDepth.LodShiftVisible = (int) Math.Round((double) x.Value))), color);
            color = null;
            this.AddSlider("Single depth", (float) MySector.Lodding.CurrentSettings.SingleDepth.LodShift, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.SingleDepth.LodShift = (int) Math.Round((double) x.Value))), color);
            base.AddLabel("The new pipeline - min lod", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.AddSlider("GBuffer", (float) MySector.Lodding.CurrentSettings.GBuffer.MinLod, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.GBuffer.MinLod = (int) Math.Round((double) x.Value))), color);
            if (MySector.Lodding.CurrentSettings.CascadeDepths.Length >= 3)
            {
                color = null;
                this.AddSlider("CSM_0", (float) MySector.Lodding.CurrentSettings.CascadeDepths[0].MinLod, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[0].MinLod = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_1", (float) MySector.Lodding.CurrentSettings.CascadeDepths[1].MinLod, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[1].MinLod = (int) Math.Round((double) x.Value))), color);
                color = null;
                this.AddSlider("CSM_2", (float) MySector.Lodding.CurrentSettings.CascadeDepths[2].MinLod, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.CascadeDepths[2].MinLod = (int) Math.Round((double) x.Value))), color);
            }
            color = null;
            this.AddSlider("Single depth", (float) MySector.Lodding.CurrentSettings.SingleDepth.MinLod, 0f, 6f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.SingleDepth.MinLod = (int) Math.Round((double) x.Value))), color);
            base.AddLabel("The new pipeline - global", (Vector4) Color.White, 1f, null, "Debug");
            color = null;
            this.AddSlider("Object distance mult", MySector.Lodding.CurrentSettings.Global.ObjectDistanceMult, 0.01f, 8f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.ObjectDistanceMult = x.Value)), color);
            color = null;
            this.AddSlider("Object distance add", MySector.Lodding.CurrentSettings.Global.ObjectDistanceAdd, -100f, 100f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.ObjectDistanceAdd = x.Value)), color);
            color = null;
            this.AddSlider("Min transition in seconds", MySector.Lodding.CurrentSettings.Global.MinTransitionInSeconds, 0f, 2f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.MinTransitionInSeconds = x.Value)), color);
            color = null;
            this.AddSlider("Max transition in seconds", MySector.Lodding.CurrentSettings.Global.MaxTransitionInSeconds, 0f, 2f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.MaxTransitionInSeconds = x.Value)), color);
            color = null;
            this.AddSlider("Transition dead zone - const", MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneConst, 0f, 2f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneConst = x.Value)), color);
            color = null;
            this.AddSlider("Transition dead zone - dist mult", MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneDistanceMult, 0f, 2f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneDistanceMult = x.Value)), color);
            color = null;
            this.AddSlider("Lod histeresis ratio", MySector.Lodding.CurrentSettings.Global.HisteresisRatio, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.HisteresisRatio = x.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Update lods", (Func<bool>) (() => MySector.Lodding.CurrentSettings.Global.IsUpdateEnabled), (Action<bool>) (x => (MySector.Lodding.CurrentSettings.Global.IsUpdateEnabled = x)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Display lod", MyRenderProxy.Settings.DisplayGbufferLOD, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferLOD = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Enable lod selection", MySector.Lodding.CurrentSettings.Global.EnableLodSelection, (Action<MyGuiControlCheckbox>) (x => (MySector.Lodding.CurrentSettings.Global.EnableLodSelection = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Lod selection", (float) MySector.Lodding.CurrentSettings.Global.LodSelection, 0f, 5f, (Action<MyGuiControlSlider>) (x => (MySector.Lodding.CurrentSettings.Global.LodSelection = (int) Math.Round((double) x.Value))), color);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
            MyRenderProxy.UpdateNewLoddingSettings(MySector.Lodding.CurrentSettings);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderLodding.<>c <>9 = new MyGuiScreenDebugRenderLodding.<>c();
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
            public static Action<MyGuiControlSlider> <>9__1_15;
            public static Action<MyGuiControlSlider> <>9__1_16;
            public static Action<MyGuiControlSlider> <>9__1_17;
            public static Action<MyGuiControlSlider> <>9__1_18;
            public static Action<MyGuiControlSlider> <>9__1_19;
            public static Action<MyGuiControlSlider> <>9__1_20;
            public static Func<bool> <>9__1_21;
            public static Action<bool> <>9__1_22;
            public static Action<MyGuiControlCheckbox> <>9__1_23;
            public static Action<MyGuiControlCheckbox> <>9__1_24;
            public static Action<MyGuiControlSlider> <>9__1_25;

            internal void <RecreateControls>b__1_0(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.GBuffer.LodShift = (int) Math.Round((double) x.Value);
                MySector.Lodding.CurrentSettings.GBuffer.LodShiftVisible = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_1(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShiftVisible = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[0].MinLod = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_11(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[1].MinLod = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_12(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[2].MinLod = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.SingleDepth.MinLod = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_14(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.ObjectDistanceMult = x.Value;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.ObjectDistanceAdd = x.Value;
            }

            internal void <RecreateControls>b__1_16(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.MinTransitionInSeconds = x.Value;
            }

            internal void <RecreateControls>b__1_17(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.MaxTransitionInSeconds = x.Value;
            }

            internal void <RecreateControls>b__1_18(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneConst = x.Value;
            }

            internal void <RecreateControls>b__1_19(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.TransitionDeadZoneDistanceMult = x.Value;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[0].LodShift = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_20(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.HisteresisRatio = x.Value;
            }

            internal bool <RecreateControls>b__1_21() => 
                MySector.Lodding.CurrentSettings.Global.IsUpdateEnabled;

            internal void <RecreateControls>b__1_22(bool x)
            {
                MySector.Lodding.CurrentSettings.Global.IsUpdateEnabled = x;
            }

            internal void <RecreateControls>b__1_23(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferLOD = x.IsChecked;
            }

            internal void <RecreateControls>b__1_24(MyGuiControlCheckbox x)
            {
                MySector.Lodding.CurrentSettings.Global.EnableLodSelection = x.IsChecked;
            }

            internal void <RecreateControls>b__1_25(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.Global.LodSelection = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_3(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShiftVisible = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_4(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[1].LodShift = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_5(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShiftVisible = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.CascadeDepths[2].LodShift = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.SingleDepth.LodShiftVisible = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.SingleDepth.LodShift = (int) Math.Round((double) x.Value);
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider x)
            {
                MySector.Lodding.CurrentSettings.GBuffer.MinLod = (int) Math.Round((double) x.Value);
            }
        }
    }
}

