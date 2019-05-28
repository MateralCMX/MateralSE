namespace Sandbox.Game.Gui
{
    using Sandbox.Game;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("Game", "Character properties")]
    internal class MyGuiScreenDebugCharacterProperties : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugCharacterProperties() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugCharacterProperties";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("System character properties", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            if (MySession.Static.LocalCharacter != null)
            {
                base.AddLabel("Front light", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
                Vector4? color = null;
                this.AddSlider("Reflector Distance CONST", 1f, 500f, () => 35f, delegate (float s) {
                }, color);
                color = null;
                this.AddSlider("Reflector Intensity CONST", 0f, 2f, () => MyCharacter.REFLECTOR_INTENSITY, delegate (float s) {
                }, color);
                float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
                singlePtr1[0] += 0.01f;
                base.AddLabel("Movement", Color.Yellow.ToVector4(), 1.2f, null, "Debug");
                color = null;
                this.AddSlider("Acceleration", 0f, 100f, (Func<float>) (() => MyPerGameSettings.CharacterMovement.WalkAcceleration), (Action<float>) (s => (MyPerGameSettings.CharacterMovement.WalkAcceleration = s)), color);
                color = null;
                this.AddSlider("Decceleration", 0f, 100f, (Func<float>) (() => MyPerGameSettings.CharacterMovement.WalkDecceleration), (Action<float>) (s => (MyPerGameSettings.CharacterMovement.WalkDecceleration = s)), color);
                color = null;
                this.AddSlider("Sprint acceleration", 0f, 100f, (Func<float>) (() => MyPerGameSettings.CharacterMovement.SprintAcceleration), (Action<float>) (s => (MyPerGameSettings.CharacterMovement.SprintAcceleration = s)), color);
                color = null;
                this.AddSlider("Sprint decceleration", 0f, 100f, (Func<float>) (() => MyPerGameSettings.CharacterMovement.SprintDecceleration), (Action<float>) (s => (MyPerGameSettings.CharacterMovement.SprintDecceleration = s)), color);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugCharacterProperties.<>c <>9 = new MyGuiScreenDebugCharacterProperties.<>c();
            public static Func<float> <>9__1_0;
            public static Action<float> <>9__1_1;
            public static Func<float> <>9__1_2;
            public static Action<float> <>9__1_3;
            public static Func<float> <>9__1_4;
            public static Action<float> <>9__1_5;
            public static Func<float> <>9__1_6;
            public static Action<float> <>9__1_7;
            public static Func<float> <>9__1_8;
            public static Action<float> <>9__1_9;
            public static Func<float> <>9__1_10;
            public static Action<float> <>9__1_11;

            internal float <RecreateControls>b__1_0() => 
                35f;

            internal void <RecreateControls>b__1_1(float s)
            {
            }

            internal float <RecreateControls>b__1_10() => 
                MyPerGameSettings.CharacterMovement.SprintDecceleration;

            internal void <RecreateControls>b__1_11(float s)
            {
                MyPerGameSettings.CharacterMovement.SprintDecceleration = s;
            }

            internal float <RecreateControls>b__1_2() => 
                MyCharacter.REFLECTOR_INTENSITY;

            internal void <RecreateControls>b__1_3(float s)
            {
            }

            internal float <RecreateControls>b__1_4() => 
                MyPerGameSettings.CharacterMovement.WalkAcceleration;

            internal void <RecreateControls>b__1_5(float s)
            {
                MyPerGameSettings.CharacterMovement.WalkAcceleration = s;
            }

            internal float <RecreateControls>b__1_6() => 
                MyPerGameSettings.CharacterMovement.WalkDecceleration;

            internal void <RecreateControls>b__1_7(float s)
            {
                MyPerGameSettings.CharacterMovement.WalkDecceleration = s;
            }

            internal float <RecreateControls>b__1_8() => 
                MyPerGameSettings.CharacterMovement.SprintAcceleration;

            internal void <RecreateControls>b__1_9(float s)
            {
                MyPerGameSettings.CharacterMovement.SprintAcceleration = s;
            }
        }
    }
}

