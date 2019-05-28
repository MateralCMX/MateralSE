namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Culling")]
    internal class MyGuiScreenDebugRenderCulling : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderCulling() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderCulling";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Culling", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            this.AddSlider("CullGroupsThreshold", 0f, 1000f, (Func<float>) (() => ((float) MyRenderProxy.Settings.CullGroupsThreshold)), (Action<float>) (f => (MyRenderProxy.Settings.CullGroupsThreshold = (int) f)), color);
            color = null;
            this.AddSlider("CullTreeFallbackThreshold", 0f, 1f, (Func<float>) (() => MyRenderProxy.Settings.IncrementalCullingTreeFallbackThreshold), (Action<float>) (x => (MyRenderProxy.Settings.IncrementalCullingTreeFallbackThreshold = x)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("UseIncrementalCulling", (Func<bool>) (() => MyRenderProxy.Settings.UseIncrementalCulling), (Action<bool>) (x => (MyRenderProxy.Settings.UseIncrementalCulling = x)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("IncrementalCullFrames", 1f, 100f, (Func<float>) (() => ((float) MyRenderProxy.Settings.IncrementalCullFrames)), (Action<float>) (x => (MyRenderProxy.Settings.IncrementalCullFrames = (int) x)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Occlusion", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.AddCheckBox("Skip occlusion queries", MyRenderProxy.Settings.IgnoreOcclusionQueries, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.IgnoreOcclusionQueries = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Disable occlusion queries", MyRenderProxy.Settings.DisableOcclusionQueries, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisableOcclusionQueries = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw occlusion queries debug", MyRenderProxy.Settings.DrawOcclusionQueriesDebug, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawOcclusionQueriesDebug = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw group occlusion queries debug", MyRenderProxy.Settings.DrawGroupOcclusionQueriesDebug, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawGroupOcclusionQueriesDebug = x.IsChecked)), true, null, color, captionOffset);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderCulling.<>c <>9 = new MyGuiScreenDebugRenderCulling.<>c();
            public static Func<float> <>9__1_0;
            public static Action<float> <>9__1_1;
            public static Func<float> <>9__1_2;
            public static Action<float> <>9__1_3;
            public static Func<bool> <>9__1_4;
            public static Action<bool> <>9__1_5;
            public static Func<float> <>9__1_6;
            public static Action<float> <>9__1_7;
            public static Action<MyGuiControlCheckbox> <>9__1_8;
            public static Action<MyGuiControlCheckbox> <>9__1_9;
            public static Action<MyGuiControlCheckbox> <>9__1_10;
            public static Action<MyGuiControlCheckbox> <>9__1_11;

            internal float <RecreateControls>b__1_0() => 
                ((float) MyRenderProxy.Settings.CullGroupsThreshold);

            internal void <RecreateControls>b__1_1(float f)
            {
                MyRenderProxy.Settings.CullGroupsThreshold = (int) f;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawOcclusionQueriesDebug = x.IsChecked;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawGroupOcclusionQueriesDebug = x.IsChecked;
            }

            internal float <RecreateControls>b__1_2() => 
                MyRenderProxy.Settings.IncrementalCullingTreeFallbackThreshold;

            internal void <RecreateControls>b__1_3(float x)
            {
                MyRenderProxy.Settings.IncrementalCullingTreeFallbackThreshold = x;
            }

            internal bool <RecreateControls>b__1_4() => 
                MyRenderProxy.Settings.UseIncrementalCulling;

            internal void <RecreateControls>b__1_5(bool x)
            {
                MyRenderProxy.Settings.UseIncrementalCulling = x;
            }

            internal float <RecreateControls>b__1_6() => 
                ((float) MyRenderProxy.Settings.IncrementalCullFrames);

            internal void <RecreateControls>b__1_7(float x)
            {
                MyRenderProxy.Settings.IncrementalCullFrames = (int) x;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.IgnoreOcclusionQueries = x.IsChecked;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisableOcclusionQueries = x.IsChecked;
            }
        }
    }
}

