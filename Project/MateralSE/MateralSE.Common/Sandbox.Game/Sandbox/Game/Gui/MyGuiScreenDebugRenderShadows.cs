namespace Sandbox.Game.Gui
{
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Shadows")]
    internal class MyGuiScreenDebugRenderShadows : MyGuiScreenDebugBase
    {
        private int m_selectedVolume;
        private MyGuiControlCheckbox m_checkboxHigherRange;
        private MyGuiControlSlider m_sliderFullCoveredDepth;
        private MyGuiControlSlider m_sliderExtCoveredDepth;
        private MyGuiControlSlider m_sliderShadowNormalOffset;

        public MyGuiScreenDebugRenderShadows() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderShadows";

        private float GetSelectedVolume() => 
            ((float) this.m_selectedVolume);

        private void OnChangeSmallObjectsThreshold(MyGuiControlSlider slider)
        {
            float num = slider.Value;
            for (int i = 0; i < MySector.ShadowSettings.Cascades.Length; i++)
            {
                MySector.ShadowSettings.Cascades[i].SkippingSmallObjectThreshold = num;
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Shadows", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Setup", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Enable Shadows", (Func<bool>) (() => MyRenderProxy.Settings.EnableShadows), (Action<bool>) (newValue => (MyRenderProxy.Settings.EnableShadows = newValue)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Enable Shadow Blur", (Func<bool>) (() => MySector.ShadowSettings.Data.EnableShadowBlur), (Action<bool>) (newValue => (MySector.ShadowSettings.Data.EnableShadowBlur = newValue)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Force per-frame updating", MySector.ShadowSettings.Data.UpdateCascadesEveryFrame, (Action<MyGuiControlCheckbox>) (x => (MySector.ShadowSettings.Data.UpdateCascadesEveryFrame = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Shadow cascade usage based skip", MyRenderProxy.Settings.ShadowCascadeUsageBasedSkip, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.ShadowCascadeUsageBasedSkip = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Use Occlusion culling", !MyRenderProxy.Settings.DisableShadowCascadeOcclusionQueries, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisableShadowCascadeOcclusionQueries = !x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Max base shadow cascade distance", MySector.ShadowSettings.Data.ShadowCascadeMaxDistance, 1f, 20000f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.ShadowCascadeMaxDistance = x.Value)), color);
            color = null;
            this.AddSlider("Back offset", MySector.ShadowSettings.Data.ShadowCascadeZOffset, 1f, 50000f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.ShadowCascadeZOffset = x.Value)), color);
            color = null;
            this.AddSlider("Spread factor", MySector.ShadowSettings.Data.ShadowCascadeSpreadFactor, 0f, 2f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.ShadowCascadeSpreadFactor = x.Value)), color);
            color = null;
            this.AddSlider("LightDirectionChangeDelayMultiplier", MySector.ShadowSettings.Data.LightDirectionChangeDelayMultiplier, 0f, 180f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.LightDirectionChangeDelayMultiplier = x.Value)), color);
            color = null;
            this.AddSlider("LightDirectionDifferenceThreshold", MySector.ShadowSettings.Data.LightDirectionDifferenceThreshold, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.LightDirectionDifferenceThreshold = x.Value)), color);
            color = null;
            base.AddSlider("Small objects threshold (broken)", 0f, 0f, 1000f, new Action<MyGuiControlSlider>(this.OnChangeSmallObjectsThreshold), color);
            color = null;
            this.m_sliderShadowNormalOffset = base.AddSlider("Shadow normal offset", MySector.ShadowSettings.Cascades[this.m_selectedVolume].ShadowNormalOffset, 0f, 1f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Cascades[this.m_selectedVolume].ShadowNormalOffset = x.Value)), color);
            color = null;
            MyGuiControlSlider slider1 = this.AddSlider("ZBias", MySector.ShadowSettings.Data.ZBias, 0f, 0.02f, (Action<MyGuiControlSlider>) (x => (MySector.ShadowSettings.Data.ZBias = x.Value)), color);
            slider1.LabelDecimalPlaces = 9;
            float zBias = MySector.ShadowSettings.Data.ZBias;
            slider1.Value = -1f;
            slider1.Value = zBias;
            base.AddLabel("Debug", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show shadows", MyRenderProxy.Settings.DisplayShadowsWithDebug, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayShadowsWithDebug = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show cascade splits", MyRenderProxy.Settings.DisplayShadowSplitsWithDebug, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayShadowSplitsWithDebug = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show cascade splits for particles", MyRenderProxy.Settings.DisplayParticleShadowSplitsWithDebug, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayParticleShadowSplitsWithDebug = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show cascade volumes", MyRenderProxy.Settings.DisplayShadowVolumes, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DisplayShadowVolumes = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show cascade textures", MyRenderProxy.Settings.DrawCascadeShadowTextures, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawCascadeShadowTextures = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Show spot textures", MyRenderProxy.Settings.DrawSpotShadowTextures, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.DrawSpotShadowTextures = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Zoom to cascade texture", (float) MyRenderProxy.Settings.ZoomCascadeTextureIndex, -1f, 8f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.ZoomCascadeTextureIndex = (int) x.Value)), color);
            color = null;
            captionOffset = null;
            this.AddCheckBox("Freeze camera", MyRenderProxy.Settings.ShadowCameraFrozen, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.ShadowCameraFrozen = x.IsChecked)), true, null, color, captionOffset);
            for (int i = 0; i < MySector.ShadowSettings.Data.CascadesCount; i++)
            {
                int captureIndex = i;
                color = null;
                captionOffset = null;
                base.AddCheckBox("Freeze cascade " + i.ToString(), MySector.ShadowSettings.ShadowCascadeFrozen[captureIndex], delegate (MyGuiControlCheckbox x) {
                    bool[] shadowCascadeFrozen = MySector.ShadowSettings.ShadowCascadeFrozen;
                    shadowCascadeFrozen[captureIndex] = x.IsChecked;
                    MySector.ShadowSettings.ShadowCascadeFrozen = shadowCascadeFrozen;
                }, true, null, color, captionOffset);
            }
        }

        private void SetSelectedVolume(float value)
        {
            int num = MathHelper.Clamp((int) Math.Floor((double) value), 0, MySector.ShadowSettings.Data.CascadesCount - 1);
            if (this.m_selectedVolume != num)
            {
                this.m_selectedVolume = num;
                MyShadowsSettings.Cascade cascade = MySector.ShadowSettings.Cascades[this.m_selectedVolume];
                this.m_checkboxHigherRange.IsChecked = true;
                this.m_sliderFullCoveredDepth.Value = cascade.FullCoverageDepth;
                this.m_sliderExtCoveredDepth.Value = cascade.ExtendedCoverageDepth;
                this.m_sliderShadowNormalOffset.Value = cascade.ShadowNormalOffset;
            }
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
            MyRenderProxy.UpdateShadowsSettings(MySector.ShadowSettings);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderShadows.<>c <>9 = new MyGuiScreenDebugRenderShadows.<>c();
            public static Func<bool> <>9__1_0;
            public static Action<bool> <>9__1_1;
            public static Func<bool> <>9__1_2;
            public static Action<bool> <>9__1_3;
            public static Action<MyGuiControlCheckbox> <>9__1_4;
            public static Action<MyGuiControlCheckbox> <>9__1_5;
            public static Action<MyGuiControlCheckbox> <>9__1_6;
            public static Action<MyGuiControlSlider> <>9__1_7;
            public static Action<MyGuiControlSlider> <>9__1_8;
            public static Action<MyGuiControlSlider> <>9__1_9;
            public static Action<MyGuiControlSlider> <>9__1_10;
            public static Action<MyGuiControlSlider> <>9__1_11;
            public static Action<MyGuiControlSlider> <>9__1_13;
            public static Action<MyGuiControlCheckbox> <>9__1_14;
            public static Action<MyGuiControlCheckbox> <>9__1_15;
            public static Action<MyGuiControlCheckbox> <>9__1_16;
            public static Action<MyGuiControlCheckbox> <>9__1_17;
            public static Action<MyGuiControlCheckbox> <>9__1_18;
            public static Action<MyGuiControlCheckbox> <>9__1_19;
            public static Action<MyGuiControlSlider> <>9__1_20;
            public static Action<MyGuiControlCheckbox> <>9__1_21;

            internal bool <RecreateControls>b__1_0() => 
                MyRenderProxy.Settings.EnableShadows;

            internal void <RecreateControls>b__1_1(bool newValue)
            {
                MyRenderProxy.Settings.EnableShadows = newValue;
            }

            internal void <RecreateControls>b__1_10(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.LightDirectionChangeDelayMultiplier = x.Value;
            }

            internal void <RecreateControls>b__1_11(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.LightDirectionDifferenceThreshold = x.Value;
            }

            internal void <RecreateControls>b__1_13(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.ZBias = x.Value;
            }

            internal void <RecreateControls>b__1_14(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayShadowsWithDebug = x.IsChecked;
            }

            internal void <RecreateControls>b__1_15(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayShadowSplitsWithDebug = x.IsChecked;
            }

            internal void <RecreateControls>b__1_16(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayParticleShadowSplitsWithDebug = x.IsChecked;
            }

            internal void <RecreateControls>b__1_17(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisplayShadowVolumes = x.IsChecked;
            }

            internal void <RecreateControls>b__1_18(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawCascadeShadowTextures = x.IsChecked;
            }

            internal void <RecreateControls>b__1_19(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DrawSpotShadowTextures = x.IsChecked;
            }

            internal bool <RecreateControls>b__1_2() => 
                MySector.ShadowSettings.Data.EnableShadowBlur;

            internal void <RecreateControls>b__1_20(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.ZoomCascadeTextureIndex = (int) x.Value;
            }

            internal void <RecreateControls>b__1_21(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.ShadowCameraFrozen = x.IsChecked;
            }

            internal void <RecreateControls>b__1_3(bool newValue)
            {
                MySector.ShadowSettings.Data.EnableShadowBlur = newValue;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlCheckbox x)
            {
                MySector.ShadowSettings.Data.UpdateCascadesEveryFrame = x.IsChecked;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.ShadowCascadeUsageBasedSkip = x.IsChecked;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.DisableShadowCascadeOcclusionQueries = !x.IsChecked;
            }

            internal void <RecreateControls>b__1_7(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.ShadowCascadeMaxDistance = x.Value;
            }

            internal void <RecreateControls>b__1_8(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.ShadowCascadeZOffset = x.Value;
            }

            internal void <RecreateControls>b__1_9(MyGuiControlSlider x)
            {
                MySector.ShadowSettings.Data.ShadowCascadeSpreadFactor = x.Value;
            }
        }
    }
}

