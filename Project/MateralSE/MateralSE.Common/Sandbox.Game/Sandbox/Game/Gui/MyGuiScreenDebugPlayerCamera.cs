namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Utils;
    using VRageMath;

    [MyDebugScreen("Game", "Player Camera")]
    internal class MyGuiScreenDebugPlayerCamera : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugPlayerCamera() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugPlayerShake";

        public override unsafe void RecreateControls(bool constructor)
        {
            Vector4? nullable2;
            base.RecreateControls(constructor);
            Vector2? captionOffset = null;
            base.AddCaption("Player Head Shake", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_scale = 0.7f;
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            if (MySector.MainCamera != null)
            {
                MyCameraSpring cameraSpring = MySector.MainCamera.CameraSpring;
                base.AddLabel("Camera target spring", Color.Yellow.ToVector4(), 1f, null, "Debug");
                nullable2 = null;
                base.AddSlider("Stiffness", 0f, 50f, (Func<float>) (() => cameraSpring.SpringStiffness), (Action<float>) (s => (cameraSpring.SpringStiffness = s)), nullable2);
                nullable2 = null;
                base.AddSlider("Dampening", 0f, 1f, (Func<float>) (() => cameraSpring.SpringDampening), (Action<float>) (s => (cameraSpring.SpringDampening = s)), nullable2);
                nullable2 = null;
                base.AddSlider("CenterMaxVelocity", 0f, 10f, (Func<float>) (() => cameraSpring.SpringMaxVelocity), (Action<float>) (s => (cameraSpring.SpringMaxVelocity = s)), nullable2);
                nullable2 = null;
                base.AddSlider("SpringMaxLength", 0f, 2f, (Func<float>) (() => cameraSpring.SpringMaxLength), (Action<float>) (s => (cameraSpring.SpringMaxLength = s)), nullable2);
            }
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            if (MyThirdPersonSpectator.Static != null)
            {
                base.AddLabel("Third person spectator", Color.Yellow.ToVector4(), 1f, null, "Debug");
                float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
                singlePtr3[0] += 0.01f;
                nullable2 = null;
                captionOffset = null;
                this.AddCheckBox("Debug draw", (Func<bool>) (() => MyThirdPersonSpectator.Static.EnableDebugDraw), (Action<bool>) (s => (MyThirdPersonSpectator.Static.EnableDebugDraw = s)), true, null, nullable2, captionOffset);
                base.AddLabel("Normal spring", Color.Yellow.ToVector4(), 0.7f, null, "Debug");
                nullable2 = null;
                this.AddSlider("Stiffness", 1f, 50000f, (Func<float>) (() => MyThirdPersonSpectator.Static.NormalSpring.Stiffness), (Action<float>) (s => (MyThirdPersonSpectator.Static.NormalSpring.Stiffness = s)), nullable2);
                nullable2 = null;
                this.AddSlider("Damping", 1f, 5000f, (Func<float>) (() => MyThirdPersonSpectator.Static.NormalSpring.Dampening), (Action<float>) (s => (MyThirdPersonSpectator.Static.NormalSpring.Dampening = s)), nullable2);
                nullable2 = null;
                this.AddSlider("Mass", 0.1f, 500f, (Func<float>) (() => MyThirdPersonSpectator.Static.NormalSpring.Mass), (Action<float>) (s => (MyThirdPersonSpectator.Static.NormalSpring.Mass = s)), nullable2);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugPlayerCamera.<>c <>9 = new MyGuiScreenDebugPlayerCamera.<>c();
            public static Func<bool> <>9__1_0;
            public static Action<bool> <>9__1_1;
            public static Func<float> <>9__1_2;
            public static Action<float> <>9__1_3;
            public static Func<float> <>9__1_4;
            public static Action<float> <>9__1_5;
            public static Func<float> <>9__1_6;
            public static Action<float> <>9__1_7;

            internal bool <RecreateControls>b__1_0() => 
                MyThirdPersonSpectator.Static.EnableDebugDraw;

            internal void <RecreateControls>b__1_1(bool s)
            {
                MyThirdPersonSpectator.Static.EnableDebugDraw = s;
            }

            internal float <RecreateControls>b__1_2() => 
                MyThirdPersonSpectator.Static.NormalSpring.Stiffness;

            internal void <RecreateControls>b__1_3(float s)
            {
                MyThirdPersonSpectator.Static.NormalSpring.Stiffness = s;
            }

            internal float <RecreateControls>b__1_4() => 
                MyThirdPersonSpectator.Static.NormalSpring.Dampening;

            internal void <RecreateControls>b__1_5(float s)
            {
                MyThirdPersonSpectator.Static.NormalSpring.Dampening = s;
            }

            internal float <RecreateControls>b__1_6() => 
                MyThirdPersonSpectator.Static.NormalSpring.Mass;

            internal void <RecreateControls>b__1_7(float s)
            {
                MyThirdPersonSpectator.Static.NormalSpring.Mass = s;
            }
        }
    }
}

