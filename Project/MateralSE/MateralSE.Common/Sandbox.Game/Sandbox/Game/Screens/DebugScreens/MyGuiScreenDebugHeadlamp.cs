namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Cosmetics", "Headlamp")]
    internal class MyGuiScreenDebugHeadlamp : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugHeadlamp() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugHeadlamp";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Headlamp", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            base.AddLabel("Spot", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.AddColor("Color", MyCharacter.REFLECTOR_COLOR, delegate (MyGuiControlColor v) {
                MyCharacter.REFLECTOR_COLOR = (Vector4) v.Color;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            });
            Vector4? color = null;
            this.AddSlider("Falloff", MyCharacter.REFLECTOR_FALLOFF, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.REFLECTOR_FALLOFF = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Gloss factor", MyCharacter.REFLECTOR_GLOSS_FACTOR, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.REFLECTOR_GLOSS_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Diffuse factor", MyCharacter.REFLECTOR_DIFFUSE_FACTOR, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.REFLECTOR_DIFFUSE_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Intensity", MyCharacter.REFLECTOR_INTENSITY, 0f, 200f, delegate (MyGuiControlSlider slider) {
                MyCharacter.REFLECTOR_INTENSITY = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            base.AddLabel("Point", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
            this.AddColor("Color", MyCharacter.POINT_COLOR, delegate (MyGuiControlColor v) {
                MyCharacter.POINT_COLOR = (Vector4) v.Color;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            });
            color = null;
            this.AddSlider("Falloff", MyCharacter.POINT_FALLOFF, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.POINT_FALLOFF = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Gloss factor", MyCharacter.POINT_GLOSS_FACTOR, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.POINT_GLOSS_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Diffuse factor", MyCharacter.POINT_DIFFUSE_FACTOR, 0f, 5f, delegate (MyGuiControlSlider slider) {
                MyCharacter.POINT_DIFFUSE_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Intensity", MyCharacter.POINT_LIGHT_INTENSITY, 0f, 50f, delegate (MyGuiControlSlider slider) {
                MyCharacter.POINT_LIGHT_INTENSITY = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
            color = null;
            this.AddSlider("Range", MyCharacter.POINT_LIGHT_RANGE, 0f, 10f, delegate (MyGuiControlSlider slider) {
                MyCharacter.POINT_LIGHT_RANGE = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }, color);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugHeadlamp.<>c <>9 = new MyGuiScreenDebugHeadlamp.<>c();
            public static Action<MyGuiControlColor> <>9__2_0;
            public static Action<MyGuiControlSlider> <>9__2_1;
            public static Action<MyGuiControlSlider> <>9__2_2;
            public static Action<MyGuiControlSlider> <>9__2_3;
            public static Action<MyGuiControlSlider> <>9__2_4;
            public static Action<MyGuiControlColor> <>9__2_5;
            public static Action<MyGuiControlSlider> <>9__2_6;
            public static Action<MyGuiControlSlider> <>9__2_7;
            public static Action<MyGuiControlSlider> <>9__2_8;
            public static Action<MyGuiControlSlider> <>9__2_9;
            public static Action<MyGuiControlSlider> <>9__2_10;

            internal void <RecreateControls>b__2_0(MyGuiControlColor v)
            {
                MyCharacter.REFLECTOR_COLOR = (Vector4) v.Color;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_1(MyGuiControlSlider slider)
            {
                MyCharacter.REFLECTOR_FALLOFF = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlSlider slider)
            {
                MyCharacter.POINT_LIGHT_RANGE = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_2(MyGuiControlSlider slider)
            {
                MyCharacter.REFLECTOR_GLOSS_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_3(MyGuiControlSlider slider)
            {
                MyCharacter.REFLECTOR_DIFFUSE_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_4(MyGuiControlSlider slider)
            {
                MyCharacter.REFLECTOR_INTENSITY = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_5(MyGuiControlColor v)
            {
                MyCharacter.POINT_COLOR = (Vector4) v.Color;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_6(MyGuiControlSlider slider)
            {
                MyCharacter.POINT_FALLOFF = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_7(MyGuiControlSlider slider)
            {
                MyCharacter.POINT_GLOSS_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_8(MyGuiControlSlider slider)
            {
                MyCharacter.POINT_DIFFUSE_FACTOR = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }

            internal void <RecreateControls>b__2_9(MyGuiControlSlider slider)
            {
                MyCharacter.POINT_LIGHT_INTENSITY = slider.Value;
                MyCharacter.LIGHT_PARAMETERS_CHANGED = true;
            }
        }
    }
}

