namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "GBuffer Debug")]
    internal class MyGuiScreenDebugRenderGBufferDebug : MyGuiScreenDebugBase
    {
        private List<MyGuiControlCheckbox> m_cbs;
        private bool m_radioUpdate;

        public MyGuiScreenDebugRenderGBufferDebug() : base(nullable, false)
        {
            this.m_cbs = new List<MyGuiControlCheckbox>();
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderGBufferDebug";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("GBuffer Debug", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Gbuffer", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.m_cbs.Clear();
            Vector4? color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Base color", MyRenderProxy.Settings.DisplayGbufferColor, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferColor = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Albedo", MyRenderProxy.Settings.DisplayGbufferAlbedo, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferAlbedo = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Normals", MyRenderProxy.Settings.DisplayGbufferNormal, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferNormal = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Normals view", MyRenderProxy.Settings.DisplayGbufferNormalView, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferNormalView = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Glossiness", MyRenderProxy.Settings.DisplayGbufferGlossiness, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferGlossiness = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Metalness", MyRenderProxy.Settings.DisplayGbufferMetalness, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferMetalness = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("NDotL", MyRenderProxy.Settings.DisplayNDotL, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayNDotL = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("LOD", MyRenderProxy.Settings.DisplayGbufferLOD, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferLOD = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Mipmap", MyRenderProxy.Settings.DisplayMipmap, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayMipmap = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Ambient occlusion", MyRenderProxy.Settings.DisplayGbufferAO, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayGbufferAO = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Emissive", MyRenderProxy.Settings.DisplayEmissive, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayEmissive = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Edge mask", MyRenderProxy.Settings.DisplayEdgeMask, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayEdgeMask = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Depth", MyRenderProxy.Settings.DisplayDepth, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayDepth = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Stencil", MyRenderProxy.Settings.DisplayStencil, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayStencil = x.IsChecked)), true, null, color, captionOffset));
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Reprojection test", MyRenderProxy.Settings.DisplayReprojectedDepth, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayReprojectedDepth = x.IsChecked)), true, null, color, captionOffset));
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            base.AddLabel("Environment light", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Ambient diffuse", MyRenderProxy.Settings.DisplayAmbientDiffuse, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAmbientDiffuse = x.IsChecked)), true, null, color, captionOffset));
            color = null;
            captionOffset = null;
            this.m_cbs.Add(this.AddCheckBox("Ambient specular", MyRenderProxy.Settings.DisplayAmbientSpecular, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayAmbientSpecular = x.IsChecked)), true, null, color, captionOffset));
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
            if (!this.m_radioUpdate)
            {
                this.m_radioUpdate = true;
                foreach (MyGuiControlCheckbox checkbox in this.m_cbs)
                {
                    if (!ReferenceEquals(checkbox, sender))
                    {
                        checkbox.IsChecked = false;
                    }
                }
                this.m_radioUpdate = false;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderGBufferDebug.<>c <>9 = new MyGuiScreenDebugRenderGBufferDebug.<>c();
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
            public static Action<MyGuiControlCheckbox> <>9__2_16;

            internal void <RecreateControls>b__2_0(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferColor = x.IsChecked;
            }

            internal void <RecreateControls>b__2_1(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferAlbedo = x.IsChecked;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayEmissive = x.IsChecked;
            }

            internal void <RecreateControls>b__2_11(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayEdgeMask = x.IsChecked;
            }

            internal void <RecreateControls>b__2_12(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayDepth = x.IsChecked;
            }

            internal void <RecreateControls>b__2_13(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayStencil = x.IsChecked;
            }

            internal void <RecreateControls>b__2_14(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayReprojectedDepth = x.IsChecked;
            }

            internal void <RecreateControls>b__2_15(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAmbientDiffuse = x.IsChecked;
            }

            internal void <RecreateControls>b__2_16(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayAmbientSpecular = x.IsChecked;
            }

            internal void <RecreateControls>b__2_2(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferNormal = x.IsChecked;
            }

            internal void <RecreateControls>b__2_3(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferNormalView = x.IsChecked;
            }

            internal void <RecreateControls>b__2_4(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferGlossiness = x.IsChecked;
            }

            internal void <RecreateControls>b__2_5(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferMetalness = x.IsChecked;
            }

            internal void <RecreateControls>b__2_6(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayNDotL = x.IsChecked;
            }

            internal void <RecreateControls>b__2_7(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferLOD = x.IsChecked;
            }

            internal void <RecreateControls>b__2_8(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayMipmap = x.IsChecked;
            }

            internal void <RecreateControls>b__2_9(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayGbufferAO = x.IsChecked;
            }
        }
    }
}

