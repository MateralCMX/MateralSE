namespace Sandbox.Game.Gui
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Utils;
    using VRageMath;

    [MyDebugScreen("Game", "Player Shake")]
    internal class MyGuiScreenDebugPlayerShake : MyGuiScreenDebugBase
    {
        private float m_forceShake;

        public MyGuiScreenDebugPlayerShake() : base(nullable, false)
        {
            this.m_forceShake = 5f;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugPlayerShake";

        private void OnForceShakeClick(MyGuiControlButton button)
        {
            if (MySector.MainCamera != null)
            {
                MySector.MainCamera.CameraShake.AddShake(this.m_forceShake);
            }
        }

        public override unsafe void RecreateControls(bool constructor)
        {
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
                MyCameraShake cameraShake = MySector.MainCamera.CameraShake;
                base.AddLabel("Camera shake", Color.Yellow.ToVector4(), 1f, null, "Debug");
                Vector4? color = null;
                base.AddSlider("MaxShake", 0f, 50f, (Func<float>) (() => cameraShake.MaxShake), (Action<float>) (s => (cameraShake.MaxShake = s)), color);
                color = null;
                base.AddSlider("MaxShakePosX", 0f, 3f, (Func<float>) (() => cameraShake.MaxShakePosX), (Action<float>) (s => (cameraShake.MaxShakePosX = s)), color);
                color = null;
                base.AddSlider("MaxShakePosY", 0f, 3f, (Func<float>) (() => cameraShake.MaxShakePosY), (Action<float>) (s => (cameraShake.MaxShakePosY = s)), color);
                color = null;
                base.AddSlider("MaxShakePosZ", 0f, 3f, (Func<float>) (() => cameraShake.MaxShakePosZ), (Action<float>) (s => (cameraShake.MaxShakePosZ = s)), color);
                color = null;
                base.AddSlider("MaxShakeDir", 0f, 1f, (Func<float>) (() => cameraShake.MaxShakeDir), (Action<float>) (s => (cameraShake.MaxShakeDir = s)), color);
                color = null;
                base.AddSlider("Reduction", 0f, 1f, (Func<float>) (() => cameraShake.Reduction), (Action<float>) (s => (cameraShake.Reduction = s)), color);
                color = null;
                base.AddSlider("Dampening", 0f, 1f, (Func<float>) (() => cameraShake.Dampening), (Action<float>) (s => (cameraShake.Dampening = s)), color);
                color = null;
                base.AddSlider("OffConstant", 0f, 1f, (Func<float>) (() => cameraShake.OffConstant), (Action<float>) (s => (cameraShake.OffConstant = s)), color);
                color = null;
                base.AddSlider("DirReduction", 0f, 2f, (Func<float>) (() => cameraShake.DirReduction), (Action<float>) (s => (cameraShake.DirReduction = s)), color);
                float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
                singlePtr2[0] += 0.01f;
                base.AddLabel("Maximum shakes", Color.Yellow.ToVector4(), 1f, null, "Debug");
                color = null;
                this.AddSlider("Character damage", 0f, 5000f, (Func<float>) (() => MyCharacter.MAX_SHAKE_DAMAGE), (Action<float>) (s => (MyCharacter.MAX_SHAKE_DAMAGE = s)), color);
                color = null;
                this.AddSlider("Grid damage", 0f, 5000f, (Func<float>) (() => MyCockpit.MAX_SHAKE_DAMAGE), (Action<float>) (s => (MyCockpit.MAX_SHAKE_DAMAGE = s)), color);
                color = null;
                this.AddSlider("Explosion shake time", 0f, 5000f, (Func<float>) (() => MyExplosionsConstants.CAMERA_SHAKE_TIME_MS), (Action<float>) (s => (MyExplosionsConstants.CAMERA_SHAKE_TIME_MS = s)), color);
                color = null;
                this.AddSlider("Grinder max shake", 0f, 50f, (Func<float>) (() => MyAngleGrinder.GRINDER_MAX_SHAKE), (Action<float>) (s => (MyAngleGrinder.GRINDER_MAX_SHAKE = s)), color);
                color = null;
                this.AddSlider("Rifle max shake", 0f, 50f, (Func<float>) (() => MyAutomaticRifleGun.RIFLE_MAX_SHAKE), (Action<float>) (s => (MyAutomaticRifleGun.RIFLE_MAX_SHAKE = s)), color);
                color = null;
                this.AddSlider("Rifle FOV shake", 0f, 1f, (Func<float>) (() => MyAutomaticRifleGun.RIFLE_FOV_SHAKE), (Action<float>) (s => (MyAutomaticRifleGun.RIFLE_FOV_SHAKE = s)), color);
                color = null;
                this.AddSlider("Drill max shake", 0f, 50f, (Func<float>) (() => MyDrillBase.DRILL_MAX_SHAKE), (Action<float>) (s => (MyDrillBase.DRILL_MAX_SHAKE = s)), color);
                float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
                singlePtr3[0] += 0.01f;
                base.AddLabel("Testing", Color.Yellow.ToVector4(), 1f, null, "Debug");
                color = null;
                base.AddSlider("Shake", 0f, 50f, (Func<float>) (() => this.m_forceShake), (Action<float>) (s => (this.m_forceShake = s)), color);
                color = null;
                captionOffset = null;
                base.AddButton("Force shake", new Action<MyGuiControlButton>(this.OnForceShakeClick), null, color, captionOffset);
            }
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.01f;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugPlayerShake.<>c <>9 = new MyGuiScreenDebugPlayerShake.<>c();
            public static Func<float> <>9__2_18;
            public static Action<float> <>9__2_19;
            public static Func<float> <>9__2_20;
            public static Action<float> <>9__2_21;
            public static Func<float> <>9__2_22;
            public static Action<float> <>9__2_23;
            public static Func<float> <>9__2_24;
            public static Action<float> <>9__2_25;
            public static Func<float> <>9__2_26;
            public static Action<float> <>9__2_27;
            public static Func<float> <>9__2_28;
            public static Action<float> <>9__2_29;
            public static Func<float> <>9__2_30;
            public static Action<float> <>9__2_31;

            internal float <RecreateControls>b__2_18() => 
                MyCharacter.MAX_SHAKE_DAMAGE;

            internal void <RecreateControls>b__2_19(float s)
            {
                MyCharacter.MAX_SHAKE_DAMAGE = s;
            }

            internal float <RecreateControls>b__2_20() => 
                MyCockpit.MAX_SHAKE_DAMAGE;

            internal void <RecreateControls>b__2_21(float s)
            {
                MyCockpit.MAX_SHAKE_DAMAGE = s;
            }

            internal float <RecreateControls>b__2_22() => 
                MyExplosionsConstants.CAMERA_SHAKE_TIME_MS;

            internal void <RecreateControls>b__2_23(float s)
            {
                MyExplosionsConstants.CAMERA_SHAKE_TIME_MS = s;
            }

            internal float <RecreateControls>b__2_24() => 
                MyAngleGrinder.GRINDER_MAX_SHAKE;

            internal void <RecreateControls>b__2_25(float s)
            {
                MyAngleGrinder.GRINDER_MAX_SHAKE = s;
            }

            internal float <RecreateControls>b__2_26() => 
                MyAutomaticRifleGun.RIFLE_MAX_SHAKE;

            internal void <RecreateControls>b__2_27(float s)
            {
                MyAutomaticRifleGun.RIFLE_MAX_SHAKE = s;
            }

            internal float <RecreateControls>b__2_28() => 
                MyAutomaticRifleGun.RIFLE_FOV_SHAKE;

            internal void <RecreateControls>b__2_29(float s)
            {
                MyAutomaticRifleGun.RIFLE_FOV_SHAKE = s;
            }

            internal float <RecreateControls>b__2_30() => 
                MyDrillBase.DRILL_MAX_SHAKE;

            internal void <RecreateControls>b__2_31(float s)
            {
                MyDrillBase.DRILL_MAX_SHAKE = s;
            }
        }
    }
}

