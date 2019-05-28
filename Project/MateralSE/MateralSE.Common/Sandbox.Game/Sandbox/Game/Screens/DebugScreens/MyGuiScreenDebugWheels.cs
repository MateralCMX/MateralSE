namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Gui;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRageMath;

    [MyDebugScreen("VRage", "Wheels")]
    public class MyGuiScreenDebugWheels : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugWheels() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugWheels";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Wheels", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? captionTextColor = null;
            captionOffset = null;
            base.AddCaption("DebugDraw", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Physics", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_WHEEL_PHYSICS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Systems", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_WHEEL_SYSTEMS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Draw voxel contact materials", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VOXEL_CONTACT_MATERIAL)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddSubcaption("Response modifier", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            this.AddSlider("Max accel", 0f, 0.1f, (Func<float>) (() => MyPhysicsConfig.WheelSoftnessVelocity), (Action<float>) (v => (MyPhysicsConfig.WheelSoftnessVelocity = v)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Softness ratio", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.WheelSoftnessRatio), (Action<float>) (v => (MyPhysicsConfig.WheelSoftnessRatio = v)), captionTextColor);
            captionTextColor = null;
            captionOffset = null;
            base.AddSubcaption("Steering model", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            this.AddSlider("Slip countdown", 0f, 100f, (Func<float>) (() => ((float) MyPhysicsConfig.WheelSlipCountdown)), (Action<float>) (x => (MyPhysicsConfig.WheelSlipCountdown = (int) x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Impulse Blending", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.WheelImpulseBlending), (Action<float>) (x => (MyPhysicsConfig.WheelImpulseBlending = x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Impulse Blending", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.WheelImpulseBlending), (Action<float>) (x => (MyPhysicsConfig.WheelImpulseBlending = x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Slip CutAway Ratio", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.WheelSlipCutAwayRatio), (Action<float>) (x => (MyPhysicsConfig.WheelSlipCutAwayRatio = x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Surface Material steering ratio", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.WheelSurfaceMaterialSteerRatio), (Action<float>) (x => (MyPhysicsConfig.WheelSurfaceMaterialSteerRatio = x)), captionTextColor);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Override axle friction", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyPhysicsConfig.OverrideWheelAxleFriction)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            this.AddSlider("Axle friction", 0f, 10000f, (Func<float>) (() => MyPhysicsConfig.WheelAxleFriction), (Action<float>) (x => (MyPhysicsConfig.WheelAxleFriction = x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Artificial breaking", 0f, 10f, (Func<float>) (() => MyPhysicsConfig.ArtificialBrakingMultiplier), (Action<float>) (x => (MyPhysicsConfig.ArtificialBrakingMultiplier = x)), captionTextColor);
            captionTextColor = null;
            this.AddSlider("Artificial breaking CoM stabilization", 0f, 1f, (Func<float>) (() => MyPhysicsConfig.ArtificialBrakingCoMStabilization), (Action<float>) (x => (MyPhysicsConfig.ArtificialBrakingCoMStabilization = x)), captionTextColor);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugWheels.<>c <>9 = new MyGuiScreenDebugWheels.<>c();
            public static Func<float> <>9__2_3;
            public static Action<float> <>9__2_4;
            public static Func<float> <>9__2_5;
            public static Action<float> <>9__2_6;
            public static Func<float> <>9__2_7;
            public static Action<float> <>9__2_8;
            public static Func<float> <>9__2_9;
            public static Action<float> <>9__2_10;
            public static Func<float> <>9__2_11;
            public static Action<float> <>9__2_12;
            public static Func<float> <>9__2_13;
            public static Action<float> <>9__2_14;
            public static Func<float> <>9__2_15;
            public static Action<float> <>9__2_16;
            public static Func<float> <>9__2_18;
            public static Action<float> <>9__2_19;
            public static Func<float> <>9__2_20;
            public static Action<float> <>9__2_21;
            public static Func<float> <>9__2_22;
            public static Action<float> <>9__2_23;

            internal void <RecreateControls>b__2_10(float x)
            {
                MyPhysicsConfig.WheelImpulseBlending = x;
            }

            internal float <RecreateControls>b__2_11() => 
                MyPhysicsConfig.WheelImpulseBlending;

            internal void <RecreateControls>b__2_12(float x)
            {
                MyPhysicsConfig.WheelImpulseBlending = x;
            }

            internal float <RecreateControls>b__2_13() => 
                MyPhysicsConfig.WheelSlipCutAwayRatio;

            internal void <RecreateControls>b__2_14(float x)
            {
                MyPhysicsConfig.WheelSlipCutAwayRatio = x;
            }

            internal float <RecreateControls>b__2_15() => 
                MyPhysicsConfig.WheelSurfaceMaterialSteerRatio;

            internal void <RecreateControls>b__2_16(float x)
            {
                MyPhysicsConfig.WheelSurfaceMaterialSteerRatio = x;
            }

            internal float <RecreateControls>b__2_18() => 
                MyPhysicsConfig.WheelAxleFriction;

            internal void <RecreateControls>b__2_19(float x)
            {
                MyPhysicsConfig.WheelAxleFriction = x;
            }

            internal float <RecreateControls>b__2_20() => 
                MyPhysicsConfig.ArtificialBrakingMultiplier;

            internal void <RecreateControls>b__2_21(float x)
            {
                MyPhysicsConfig.ArtificialBrakingMultiplier = x;
            }

            internal float <RecreateControls>b__2_22() => 
                MyPhysicsConfig.ArtificialBrakingCoMStabilization;

            internal void <RecreateControls>b__2_23(float x)
            {
                MyPhysicsConfig.ArtificialBrakingCoMStabilization = x;
            }

            internal float <RecreateControls>b__2_3() => 
                MyPhysicsConfig.WheelSoftnessVelocity;

            internal void <RecreateControls>b__2_4(float v)
            {
                MyPhysicsConfig.WheelSoftnessVelocity = v;
            }

            internal float <RecreateControls>b__2_5() => 
                MyPhysicsConfig.WheelSoftnessRatio;

            internal void <RecreateControls>b__2_6(float v)
            {
                MyPhysicsConfig.WheelSoftnessRatio = v;
            }

            internal float <RecreateControls>b__2_7() => 
                ((float) MyPhysicsConfig.WheelSlipCountdown);

            internal void <RecreateControls>b__2_8(float x)
            {
                MyPhysicsConfig.WheelSlipCountdown = (int) x;
            }

            internal float <RecreateControls>b__2_9() => 
                MyPhysicsConfig.WheelImpulseBlending;
        }
    }
}

