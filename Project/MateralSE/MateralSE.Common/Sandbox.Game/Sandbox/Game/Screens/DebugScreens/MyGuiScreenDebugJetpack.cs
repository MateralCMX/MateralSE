namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Components;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Cosmetics", "Jetpack visual")]
    internal class MyGuiScreenDebugJetpack : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugJetpack() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugJetpack";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Jetpack visual", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Light", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            Vector4? color = null;
            this.AddSlider("Intensity const", MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_BASE, 0f, 1000f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_BASE = slider.Value)), color);
            color = null;
            this.AddSlider("Intensity from thrust length", MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_LENGTH, 0f, 1000f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_LENGTH = slider.Value)), color);
            color = null;
            this.AddSlider("Range from thrust radius", MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_RADIUS, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_RADIUS = slider.Value)), color);
            color = null;
            this.AddSlider("Range from thrust length", MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_LENGTH, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_LENGTH = slider.Value)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            base.AddLabel("Glare", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Intensity const", MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_BASE, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_BASE = slider.Value)), color);
            color = null;
            this.AddSlider("Intensity from thrust length", MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_LENGTH, 0f, 100f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_LENGTH = slider.Value)), color);
            color = null;
            this.AddSlider("Size from thrust radius", MyRenderComponentCharacter.JETPACK_GLARE_SIZE_RADIUS, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_GLARE_SIZE_RADIUS = slider.Value)), color);
            color = null;
            this.AddSlider("Size from thrust length", MyRenderComponentCharacter.JETPACK_GLARE_SIZE_LENGTH, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_GLARE_SIZE_LENGTH = slider.Value)), color);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.02f;
            base.AddLabel("Thrust", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            color = null;
            this.AddSlider("Intensity base", MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY_BASE, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY_BASE = slider.Value)), color);
            color = null;
            this.AddSlider("Intensity", MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY = slider.Value)), color);
            color = null;
            this.AddSlider("Radius", MyRenderComponentCharacter.JETPACK_THRUST_THICKNESS, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_THRUST_THICKNESS = slider.Value)), color);
            color = null;
            this.AddSlider("Length", MyRenderComponentCharacter.JETPACK_THRUST_LENGTH, 0f, 10f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_THRUST_LENGTH = slider.Value)), color);
            color = null;
            this.AddSlider("Offset", MyRenderComponentCharacter.JETPACK_THRUST_OFFSET, -1f, 1f, (Action<MyGuiControlSlider>) (slider => (MyRenderComponentCharacter.JETPACK_THRUST_OFFSET = slider.Value)), color);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.02f;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugJetpack.<>c <>9 = new MyGuiScreenDebugJetpack.<>c();
            public static Action<MyGuiControlSlider> <>9__2_0;
            public static Action<MyGuiControlSlider> <>9__2_1;
            public static Action<MyGuiControlSlider> <>9__2_2;
            public static Action<MyGuiControlSlider> <>9__2_3;
            public static Action<MyGuiControlSlider> <>9__2_4;
            public static Action<MyGuiControlSlider> <>9__2_5;
            public static Action<MyGuiControlSlider> <>9__2_6;
            public static Action<MyGuiControlSlider> <>9__2_7;
            public static Action<MyGuiControlSlider> <>9__2_8;
            public static Action<MyGuiControlSlider> <>9__2_9;
            public static Action<MyGuiControlSlider> <>9__2_10;
            public static Action<MyGuiControlSlider> <>9__2_11;
            public static Action<MyGuiControlSlider> <>9__2_12;

            internal void <RecreateControls>b__2_0(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_BASE = slider.Value;
            }

            internal void <RecreateControls>b__2_1(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_LIGHT_INTENSITY_LENGTH = slider.Value;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_THRUST_THICKNESS = slider.Value;
            }

            internal void <RecreateControls>b__2_11(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_THRUST_LENGTH = slider.Value;
            }

            internal void <RecreateControls>b__2_12(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_THRUST_OFFSET = slider.Value;
            }

            internal void <RecreateControls>b__2_2(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_RADIUS = slider.Value;
            }

            internal void <RecreateControls>b__2_3(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_LIGHT_RANGE_LENGTH = slider.Value;
            }

            internal void <RecreateControls>b__2_4(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_BASE = slider.Value;
            }

            internal void <RecreateControls>b__2_5(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_GLARE_INTENSITY_LENGTH = slider.Value;
            }

            internal void <RecreateControls>b__2_6(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_GLARE_SIZE_RADIUS = slider.Value;
            }

            internal void <RecreateControls>b__2_7(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_GLARE_SIZE_LENGTH = slider.Value;
            }

            internal void <RecreateControls>b__2_8(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY_BASE = slider.Value;
            }

            internal void <RecreateControls>b__2_9(MyGuiControlSlider slider)
            {
                MyRenderComponentCharacter.JETPACK_THRUST_INTENSITY = slider.Value;
            }
        }
    }
}

