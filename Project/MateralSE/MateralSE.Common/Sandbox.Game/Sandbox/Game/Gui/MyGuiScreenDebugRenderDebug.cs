namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Debug")]
    internal class MyGuiScreenDebugRenderDebug : MyGuiScreenDebugBase
    {
        private List<MyGuiControlCheckbox> m_cbs;

        public MyGuiScreenDebugRenderDebug() : base(nullable, false)
        {
            this.m_cbs = new List<MyGuiControlCheckbox>();
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderDebug";

        private void PrintUsedFileTexturesIntoLog(MyGuiControlButton sender)
        {
            MyRenderProxy.PrintAllFileTexturesIntoLog();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Debug", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            this.AddSlider("Worker thread count", 1f, 5f, (Func<float>) (() => ((float) MyRenderProxy.Settings.RenderThreadCount)), (Action<float>) (f => (MyRenderProxy.Settings.RenderThreadCount = (int) f)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Force IC", MyRenderProxy.Settings.ForceImmediateContext, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.ForceImmediateContext = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Render thread high priority", MyRenderProxy.Settings.RenderThreadHighPriority, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.RenderThreadHighPriority = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Force Slow CPU", MyRenderProxy.Settings.ForceSlowCPU, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.ForceSlowCPU = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddCheckBox("Total parrot view", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_MODEL_INFO)), Array.Empty<ParameterExpression>())), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Debug missing file textures", MyRenderProxy.Settings.UseDebugMissingFileTextures, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.UseDebugMissingFileTextures = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            base.AddButton("Print textures log", new Action<MyGuiControlButton>(this.PrintUsedFileTexturesIntoLog), null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Skip global RO WM update", MyRenderProxy.Settings.SkipGlobalROWMUpdate, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.SkipGlobalROWMUpdate = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("HQ Depth", MyRenderProxy.Settings.User.HqDepth, delegate (MyGuiControlCheckbox x) {
                MyRenderProxy.Settings.User.HqDepth = x.IsChecked;
                MyRenderProxy.SetSettingsDirty();
            }, true, null, color, captionOffset);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderDebug.<>c <>9 = new MyGuiScreenDebugRenderDebug.<>c();
            public static Func<float> <>9__2_0;
            public static Action<float> <>9__2_1;
            public static Action<MyGuiControlCheckbox> <>9__2_2;
            public static Action<MyGuiControlCheckbox> <>9__2_3;
            public static Action<MyGuiControlCheckbox> <>9__2_4;
            public static Action<MyGuiControlCheckbox> <>9__2_6;
            public static Action<MyGuiControlCheckbox> <>9__2_7;
            public static Action<MyGuiControlCheckbox> <>9__2_8;

            internal float <RecreateControls>b__2_0() => 
                ((float) MyRenderProxy.Settings.RenderThreadCount);

            internal void <RecreateControls>b__2_1(float f)
            {
                MyRenderProxy.Settings.RenderThreadCount = (int) f;
            }

            internal void <RecreateControls>b__2_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.ForceImmediateContext = x.IsChecked;
            }

            internal void <RecreateControls>b__2_3(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.RenderThreadHighPriority = x.IsChecked;
            }

            internal void <RecreateControls>b__2_4(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.ForceSlowCPU = x.IsChecked;
            }

            internal void <RecreateControls>b__2_6(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.UseDebugMissingFileTextures = x.IsChecked;
            }

            internal void <RecreateControls>b__2_7(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.SkipGlobalROWMUpdate = x.IsChecked;
            }

            internal void <RecreateControls>b__2_8(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.User.HqDepth = x.IsChecked;
                MyRenderProxy.SetSettingsDirty();
            }
        }
    }
}

