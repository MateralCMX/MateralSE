namespace Sandbox.Game.Screens.DebugScreens
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("VRage", "Deformation")]
    public class MyGuiScreenDebugDeformation : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugDeformation() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugDeformation";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Deformation", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? color = null;
            this.AddSlider("Break Velocity", 0f, 100f, (Func<float>) (() => MyFakes.DEFORMATION_MINIMUM_VELOCITY), (Action<float>) (v => (MyFakes.DEFORMATION_MINIMUM_VELOCITY = v)), color);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            color = null;
            this.AddSlider("Vertical Offset Ratio", 0f, 5f, (Func<float>) (() => MyFakes.DEFORMATION_OFFSET_RATIO), (Action<float>) (v => (MyFakes.DEFORMATION_OFFSET_RATIO = v)), color);
            color = null;
            this.AddSlider("Vertical Offset Limit", 0f, 100f, (Func<float>) (() => MyFakes.DEFORMATION_OFFSET_MAX), (Action<float>) (v => (MyFakes.DEFORMATION_OFFSET_MAX = v)), color);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.01f;
            color = null;
            this.AddSlider("Velocity Relay", 0f, 1f, (Func<float>) (() => MyFakes.DEFORMATION_VELOCITY_RELAY), (Action<float>) (v => (MyFakes.DEFORMATION_VELOCITY_RELAY = v)), color);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.01f;
            color = null;
            this.AddSlider("Projectile Vertical Offset", 0f, 0.05f, (Func<float>) (() => MyFakes.DEFORMATION_PROJECTILE_OFFSET_RATIO), (Action<float>) (v => (MyFakes.DEFORMATION_PROJECTILE_OFFSET_RATIO = v)), color);
            color = null;
            captionOffset = null;
            base.AddCaption("Simple controls (use on your own risk)", color, captionOffset, 0.8f);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            color = null;
            captionOffset = null;
            this.AddCheckBox("Voxel cutouts enabled", MyFakes.DEFORMATION_EXPLOSIONS, (Action<MyGuiControlCheckbox>) (x => (MyFakes.DEFORMATION_EXPLOSIONS = x.IsChecked)), true, null, color, captionOffset);
            color = null;
            this.AddSlider("Voxel cutouts multiplier", 0f, 15f, (Func<float>) (() => MyFakes.DEFORMATION_VOXEL_CUTOUT_MULTIPLIER), (Action<float>) (v => (MyFakes.DEFORMATION_VOXEL_CUTOUT_MULTIPLIER = v)), color);
            color = null;
            this.AddSlider("Voxel cutout max radius", 0f, 100f, (Func<float>) (() => MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS), (Action<float>) (v => (MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS = v)), color);
            color = null;
            this.AddSlider("Grid damage multiplier", 0f, 10f, (Func<float>) (() => MyFakes.DEFORMATION_DAMAGE_MULTIPLIER), (Action<float>) (v => (MyFakes.DEFORMATION_DAMAGE_MULTIPLIER = v)), color);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugDeformation.<>c <>9 = new MyGuiScreenDebugDeformation.<>c();
            public static Func<float> <>9__2_0;
            public static Action<float> <>9__2_1;
            public static Func<float> <>9__2_2;
            public static Action<float> <>9__2_3;
            public static Func<float> <>9__2_4;
            public static Action<float> <>9__2_5;
            public static Func<float> <>9__2_6;
            public static Action<float> <>9__2_7;
            public static Func<float> <>9__2_8;
            public static Action<float> <>9__2_9;
            public static Action<MyGuiControlCheckbox> <>9__2_10;
            public static Func<float> <>9__2_11;
            public static Action<float> <>9__2_12;
            public static Func<float> <>9__2_13;
            public static Action<float> <>9__2_14;
            public static Func<float> <>9__2_15;
            public static Action<float> <>9__2_16;

            internal float <RecreateControls>b__2_0() => 
                MyFakes.DEFORMATION_MINIMUM_VELOCITY;

            internal void <RecreateControls>b__2_1(float v)
            {
                MyFakes.DEFORMATION_MINIMUM_VELOCITY = v;
            }

            internal void <RecreateControls>b__2_10(MyGuiControlCheckbox x)
            {
                MyFakes.DEFORMATION_EXPLOSIONS = x.IsChecked;
            }

            internal float <RecreateControls>b__2_11() => 
                MyFakes.DEFORMATION_VOXEL_CUTOUT_MULTIPLIER;

            internal void <RecreateControls>b__2_12(float v)
            {
                MyFakes.DEFORMATION_VOXEL_CUTOUT_MULTIPLIER = v;
            }

            internal float <RecreateControls>b__2_13() => 
                MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS;

            internal void <RecreateControls>b__2_14(float v)
            {
                MyFakes.DEFORMATION_VOXEL_CUTOUT_MAX_RADIUS = v;
            }

            internal float <RecreateControls>b__2_15() => 
                MyFakes.DEFORMATION_DAMAGE_MULTIPLIER;

            internal void <RecreateControls>b__2_16(float v)
            {
                MyFakes.DEFORMATION_DAMAGE_MULTIPLIER = v;
            }

            internal float <RecreateControls>b__2_2() => 
                MyFakes.DEFORMATION_OFFSET_RATIO;

            internal void <RecreateControls>b__2_3(float v)
            {
                MyFakes.DEFORMATION_OFFSET_RATIO = v;
            }

            internal float <RecreateControls>b__2_4() => 
                MyFakes.DEFORMATION_OFFSET_MAX;

            internal void <RecreateControls>b__2_5(float v)
            {
                MyFakes.DEFORMATION_OFFSET_MAX = v;
            }

            internal float <RecreateControls>b__2_6() => 
                MyFakes.DEFORMATION_VELOCITY_RELAY;

            internal void <RecreateControls>b__2_7(float v)
            {
                MyFakes.DEFORMATION_VELOCITY_RELAY = v;
            }

            internal float <RecreateControls>b__2_8() => 
                MyFakes.DEFORMATION_PROJECTILE_OFFSET_RATIO;

            internal void <RecreateControls>b__2_9(float v)
            {
                MyFakes.DEFORMATION_PROJECTILE_OFFSET_RATIO = v;
            }
        }
    }
}

