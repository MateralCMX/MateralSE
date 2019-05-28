namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Draw")]
    internal class MyGuiScreenDebugRenderDraw : MyGuiScreenDebugBase
    {
        private List<MyGuiControlCheckbox> m_cbs;

        public MyGuiScreenDebugRenderDraw() : base(nullable, false)
        {
            this.m_cbs = new List<MyGuiControlCheckbox>();
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderDraw";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Draw", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Draw IDs", MyRenderProxy.Settings.DisplayIDs, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayIDs = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw AABBs", MyRenderProxy.Settings.DisplayAabbs, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAabbs = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw Tree AABBs", MyRenderProxy.Settings.DisplayTreeAabbs, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayTreeAabbs = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw Wireframe", MyRenderProxy.Settings.Wireframe, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.Wireframe = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw transparency heat map", MyRenderProxy.Settings.DisplayTransparencyHeatMap, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayTransparencyHeatMap = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw transparency heat map in grayscale", MyRenderProxy.Settings.DisplayTransparencyHeatMapInGrayscale, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayTransparencyHeatMapInGrayscale = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Scene objects", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw non-merge-instanced", MyRenderProxy.Settings.DrawNonMergeInstanced, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawNonMergeInstanced = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw merge-instanced", MyRenderProxy.Settings.DrawMergeInstanced, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawMergeInstanced = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw groups", MyRenderProxy.Settings.DrawGroups, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawGroups = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw standard meshes", MyRenderProxy.Settings.DrawMeshes, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawMeshes = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw standard instanced meshes", MyRenderProxy.Settings.DrawInstancedMeshes, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawInstancedMeshes = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw glass", MyRenderProxy.Settings.DrawGlass, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawGlass = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw alphamasked", MyRenderProxy.Settings.DrawAlphamasked, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawAlphamasked = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw billboards", MyRenderProxy.Settings.DrawBillboards, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawBillboards = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw impostors", MyRenderProxy.Settings.DrawImpostors, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawImpostors = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Draw voxels", MyRenderProxy.Settings.DrawVoxels, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawVoxels = x.IsChecked)), true, null, color, captionOffset);
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderDraw.<>c <>9 = new MyGuiScreenDebugRenderDraw.<>c();
            public static Action<MyGuiControlCheckbox> <>9__2_0;
            public static Action<MyGuiControlCheckbox> <>9__2_1;
            public static Action<MyGuiControlCheckbox> <>9__2_2;
            public static Action<MyGuiControlCheckbox> <>9__2_3;
            public static Action<MyGuiControlCheckbox> <>9__2_4;
            public static Action<MyGuiControlCheckbox> <>9__2_5;
            public static Action<MyGuiControlCheckbox> <>9__2_6;
            public static Action<MyGuiControlCheckbox> <>9__2_7;
            public static Action<MyGuiControlCheckbox> <>9__2_8;
            public static Action<MyGuiControlCheckbox> <>9__2_9;
            public static Action<MyGuiControlCheckbox> <>9__2_10;
            public static Action<MyGuiControlCheckbox> <>9__2_11;
            public static Action<MyGuiControlCheckbox> <>9__2_12;
            public static Action<MyGuiControlCheckbox> <>9__2_13;
            public static Action<MyGuiControlCheckbox> <>9__2_14;
            public static Action<MyGuiControlCheckbox> <>9__2_15;

            internal void <RecreateControls>b__2_0(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayIDs = x.IsChecked;
            }

            internal void <RecreateControls>b__2_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAabbs = x.IsChecked;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawInstancedMeshes = x.IsChecked;
            }

            internal void <RecreateControls>b__2_11(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawGlass = x.IsChecked;
            }

            internal void <RecreateControls>b__2_12(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawAlphamasked = x.IsChecked;
            }

            internal void <RecreateControls>b__2_13(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawBillboards = x.IsChecked;
            }

            internal void <RecreateControls>b__2_14(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawImpostors = x.IsChecked;
            }

            internal void <RecreateControls>b__2_15(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawVoxels = x.IsChecked;
            }

            internal void <RecreateControls>b__2_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayTreeAabbs = x.IsChecked;
            }

            internal void <RecreateControls>b__2_3(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.Wireframe = x.IsChecked;
            }

            internal void <RecreateControls>b__2_4(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayTransparencyHeatMap = x.IsChecked;
            }

            internal void <RecreateControls>b__2_5(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayTransparencyHeatMapInGrayscale = x.IsChecked;
            }

            internal void <RecreateControls>b__2_6(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawNonMergeInstanced = x.IsChecked;
            }

            internal void <RecreateControls>b__2_7(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawMergeInstanced = x.IsChecked;
            }

            internal void <RecreateControls>b__2_8(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawGroups = x.IsChecked;
            }

            internal void <RecreateControls>b__2_9(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawMeshes = x.IsChecked;
            }
        }
    }
}

