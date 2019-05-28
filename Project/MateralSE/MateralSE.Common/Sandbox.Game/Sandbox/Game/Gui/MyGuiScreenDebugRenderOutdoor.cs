namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;
    using VRageRender;

    [MyDebugScreen("Render", "Outdoor")]
    internal class MyGuiScreenDebugRenderOutdoor : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugRenderOutdoor() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugRenderOutdoor";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Outdoor", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            Vector4? color = null;
            captionOffset = null;
            this.AddCheckBox("Freeze terrain queries", MyRenderProxy.Settings.FreezeTerrainQueries, (Action<MyGuiControlCheckbox>) (x => (MyRenderProxy.Settings.FreezeTerrainQueries = x.IsChecked)), true, null, color, captionOffset);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.AddLabel("Grass", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Grass maximum draw distance", MyRenderProxy.Settings.User.GrassDrawDistance, 0f, 1000f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.User.GrassDrawDistance = x.Value)), color);
            color = null;
            this.AddSlider("Scaling near distance", MyRenderProxy.Settings.GrassGeometryScalingNearDistance, 0f, 1000f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GrassGeometryScalingNearDistance = x.Value)), color);
            color = null;
            this.AddSlider("Scaling far distance", MyRenderProxy.Settings.GrassGeometryScalingFarDistance, 0f, 1000f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GrassGeometryScalingFarDistance = x.Value)), color);
            color = null;
            this.AddSlider("Scaling factor", MyRenderProxy.Settings.GrassGeometryDistanceScalingFactor, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.GrassGeometryDistanceScalingFactor = x.Value)), color);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Wind", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Strength", MyRenderProxy.Settings.WindStrength, 0f, 10f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.WindStrength = x.Value)), color);
            color = null;
            this.AddSlider("Azimuth", MyRenderProxy.Settings.WindAzimuth, 0f, 360f, (Action<MyGuiControlSlider>) (x => (MyRenderProxy.Settings.WindAzimuth = x.Value)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            base.AddLabel("Lights", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
        }

        protected override void ValueChanged(MyGuiControlBase sender)
        {
            MyRenderProxy.SetSettingsDirty();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugRenderOutdoor.<>c <>9 = new MyGuiScreenDebugRenderOutdoor.<>c();
            public static Action<MyGuiControlCheckbox> <>9__1_0;
            public static Action<MyGuiControlSlider> <>9__1_1;
            public static Action<MyGuiControlSlider> <>9__1_2;
            public static Action<MyGuiControlSlider> <>9__1_3;
            public static Action<MyGuiControlSlider> <>9__1_4;
            public static Action<MyGuiControlSlider> <>9__1_5;
            public static Action<MyGuiControlSlider> <>9__1_6;

            internal void <RecreateControls>b__1_0(MyGuiControlCheckbox x)
            {
                MyRenderProxy.Settings.FreezeTerrainQueries = x.IsChecked;
            }

            internal void <RecreateControls>b__1_1(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.User.GrassDrawDistance = x.Value;
            }

            internal void <RecreateControls>b__1_2(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GrassGeometryScalingNearDistance = x.Value;
            }

            internal void <RecreateControls>b__1_3(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GrassGeometryScalingFarDistance = x.Value;
            }

            internal void <RecreateControls>b__1_4(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.GrassGeometryDistanceScalingFactor = x.Value;
            }

            internal void <RecreateControls>b__1_5(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.WindStrength = x.Value;
            }

            internal void <RecreateControls>b__1_6(MyGuiControlSlider x)
            {
                MyRenderProxy.Settings.WindAzimuth = x.Value;
            }
        }
    }
}

