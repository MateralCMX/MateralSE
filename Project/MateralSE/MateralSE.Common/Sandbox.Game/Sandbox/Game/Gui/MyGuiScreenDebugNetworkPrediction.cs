namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Replication.History;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using VRageMath;

    [MyDebugScreen("VRage", "Network Prediction")]
    internal class MyGuiScreenDebugNetworkPrediction : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_animationComboA;
        private MyGuiControlCombobox m_animationComboB;
        private MyGuiControlSlider m_blendSlider;
        private MyGuiControlCombobox m_animationCombo;
        private MyGuiControlCheckbox m_loopCheckbox;
        private const float m_forcedPriority = 1f;

        public MyGuiScreenDebugNetworkPrediction() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugNetworkPrediction";

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_scale = 0.7f;
            base.m_sliderDebugScale = 0.7f;
            Vector2? captionOffset = null;
            base.AddCaption("Network Prediction", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.1f);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            if (MyMultiplayer.Static != null)
            {
                int num1;
                if ((!MyPredictedSnapshotSync.POSITION_CORRECTION || (!MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION || (!MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION || (!MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION || (!MyPredictedSnapshotSync.ROTATION_CORRECTION || !MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION))))) || !MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION)
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION;
                }
                Vector4? color = null;
                captionOffset = null;
                this.AddCheckBox("Apply Corrections", (bool) num1, delegate (MyGuiControlCheckbox x) {
                    MyPredictedSnapshotSync.POSITION_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.ROTATION_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
                    MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
                }, true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Apply Trend Reset", MyPredictedSnapshotSync.ApplyTrend, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.ApplyTrend = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Force animated", MyPredictedSnapshotSync.ForceAnimated, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.ForceAnimated = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Velocity change to reset", MyPredictedSnapshotSync.MIN_VELOCITY_CHANGE_TO_RESET, 0f, 30f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MIN_VELOCITY_CHANGE_TO_RESET = slider.Value)), color);
                color = null;
                this.AddSlider("Delta factor", MyPredictedSnapshotSync.DELTA_FACTOR, 0f, 1f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.DELTA_FACTOR = slider.Value)), color);
                color = null;
                this.AddSlider("Smooth iterations", (float) MyPredictedSnapshotSync.SMOOTH_ITERATIONS, 0f, 1000f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.SMOOTH_ITERATIONS = (int) slider.Value)), color);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Apply Reset", MySnapshot.ApplyReset, (Action<MyGuiControlCheckbox>) (x => (MySnapshot.ApplyReset = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Smooth distance", MyPredictedSnapshotSync.SMOOTH_DISTANCE, 0f, 1000f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.SMOOTH_DISTANCE = (int) slider.Value)), color);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Propagate To Connections", MySnapshotCache.PROPAGATE_TO_CONNECTIONS, (Action<MyGuiControlCheckbox>) (x => (MySnapshotCache.PROPAGATE_TO_CONNECTIONS = x.IsChecked)), true, null, color, captionOffset);
                float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
                singlePtr2[0] += 0.01f;
                color = null;
                captionOffset = null;
                this.AddCheckBox("Position corrections", MyPredictedSnapshotSync.POSITION_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.POSITION_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Smooth position corrections", MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Minimum pos delta", MyPredictedSnapshotSync.MIN_POSITION_DELTA, 0f, 0.5f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MIN_POSITION_DELTA = slider.Value)), color);
                color = null;
                this.AddSlider("Maximum pos delta", MyPredictedSnapshotSync.MAX_POSITION_DELTA, 0f, 5f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MAX_POSITION_DELTA = slider.Value)), color);
                float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
                singlePtr3[0] += 0.01f;
                color = null;
                captionOffset = null;
                this.AddCheckBox("Linear velocity corrections", MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Smooth linear velocity corrections", MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Minimum linVel delta", MyPredictedSnapshotSync.MIN_LINEAR_VELOCITY_DELTA, 0f, 0.5f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MIN_LINEAR_VELOCITY_DELTA = slider.Value)), color);
                color = null;
                this.AddSlider("Maximum linVel delta", MyPredictedSnapshotSync.MAX_LINEAR_VELOCITY_DELTA, 0f, 5f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MAX_LINEAR_VELOCITY_DELTA = slider.Value)), color);
                float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
                singlePtr4[0] += 0.01f;
                color = null;
                captionOffset = null;
                this.AddCheckBox("Rotation corrections", MyPredictedSnapshotSync.ROTATION_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.ROTATION_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Smooth rotation corrections", MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Minimum angle delta", MathHelper.ToDegrees(MyPredictedSnapshotSync.MIN_ROTATION_ANGLE), 0f, 90f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MIN_ROTATION_ANGLE = MathHelper.ToRadians(slider.Value))), color);
                color = null;
                this.AddSlider("Maximum angle delta", MathHelper.ToDegrees(MyPredictedSnapshotSync.MAX_ROTATION_ANGLE), 0f, 90f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MAX_ROTATION_ANGLE = MathHelper.ToRadians(slider.Value))), color);
                float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
                singlePtr5[0] += 0.01f;
                color = null;
                captionOffset = null;
                this.AddCheckBox("Angular velocity corrections", MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                captionOffset = null;
                this.AddCheckBox("Smooth angular velocity corrections", MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION, (Action<MyGuiControlCheckbox>) (x => (MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION = x.IsChecked)), true, null, color, captionOffset);
                color = null;
                this.AddSlider("Minimum angle delta", MyPredictedSnapshotSync.MIN_ANGULAR_VELOCITY_DELTA, 0f, 1f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MIN_ANGULAR_VELOCITY_DELTA = slider.Value)), color);
                color = null;
                this.AddSlider("Maximum angle delta", MyPredictedSnapshotSync.MAX_ANGULAR_VELOCITY_DELTA, 0f, 1f, (Action<MyGuiControlSlider>) (slider => (MyPredictedSnapshotSync.MAX_ANGULAR_VELOCITY_DELTA = slider.Value)), color);
                color = null;
                this.AddSlider("Impulse scale", MyGridPhysics.PREDICTION_IMPULSE_SCALE, 0f, 0.2f, (Action<MyGuiControlSlider>) (slider => (MyGridPhysics.PREDICTION_IMPULSE_SCALE = slider.Value)), color);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugNetworkPrediction.<>c <>9 = new MyGuiScreenDebugNetworkPrediction.<>c();
            public static Action<MyGuiControlCheckbox> <>9__7_0;
            public static Action<MyGuiControlCheckbox> <>9__7_1;
            public static Action<MyGuiControlCheckbox> <>9__7_2;
            public static Action<MyGuiControlSlider> <>9__7_3;
            public static Action<MyGuiControlSlider> <>9__7_4;
            public static Action<MyGuiControlSlider> <>9__7_5;
            public static Action<MyGuiControlCheckbox> <>9__7_6;
            public static Action<MyGuiControlSlider> <>9__7_7;
            public static Action<MyGuiControlCheckbox> <>9__7_8;
            public static Action<MyGuiControlCheckbox> <>9__7_9;
            public static Action<MyGuiControlCheckbox> <>9__7_10;
            public static Action<MyGuiControlSlider> <>9__7_11;
            public static Action<MyGuiControlSlider> <>9__7_12;
            public static Action<MyGuiControlCheckbox> <>9__7_13;
            public static Action<MyGuiControlCheckbox> <>9__7_14;
            public static Action<MyGuiControlSlider> <>9__7_15;
            public static Action<MyGuiControlSlider> <>9__7_16;
            public static Action<MyGuiControlCheckbox> <>9__7_17;
            public static Action<MyGuiControlCheckbox> <>9__7_18;
            public static Action<MyGuiControlSlider> <>9__7_19;
            public static Action<MyGuiControlSlider> <>9__7_20;
            public static Action<MyGuiControlCheckbox> <>9__7_21;
            public static Action<MyGuiControlCheckbox> <>9__7_22;
            public static Action<MyGuiControlSlider> <>9__7_23;
            public static Action<MyGuiControlSlider> <>9__7_24;
            public static Action<MyGuiControlSlider> <>9__7_25;

            internal void <RecreateControls>b__7_0(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.POSITION_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.ROTATION_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
                MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_1(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.ApplyTrend = x.IsChecked;
            }

            internal void <RecreateControls>b__7_10(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.SMOOTH_POSITION_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_11(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MIN_POSITION_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_12(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MAX_POSITION_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_13(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.LINEAR_VELOCITY_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_14(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.SMOOTH_LINEAR_VELOCITY_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_15(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MIN_LINEAR_VELOCITY_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_16(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MAX_LINEAR_VELOCITY_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_17(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.ROTATION_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_18(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.SMOOTH_ROTATION_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_19(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MIN_ROTATION_ANGLE = MathHelper.ToRadians(slider.Value);
            }

            internal void <RecreateControls>b__7_2(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.ForceAnimated = x.IsChecked;
            }

            internal void <RecreateControls>b__7_20(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MAX_ROTATION_ANGLE = MathHelper.ToRadians(slider.Value);
            }

            internal void <RecreateControls>b__7_21(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_22(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.SMOOTH_ANGULAR_VELOCITY_CORRECTION = x.IsChecked;
            }

            internal void <RecreateControls>b__7_23(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MIN_ANGULAR_VELOCITY_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_24(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MAX_ANGULAR_VELOCITY_DELTA = slider.Value;
            }

            internal void <RecreateControls>b__7_25(MyGuiControlSlider slider)
            {
                MyGridPhysics.PREDICTION_IMPULSE_SCALE = slider.Value;
            }

            internal void <RecreateControls>b__7_3(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.MIN_VELOCITY_CHANGE_TO_RESET = slider.Value;
            }

            internal void <RecreateControls>b__7_4(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.DELTA_FACTOR = slider.Value;
            }

            internal void <RecreateControls>b__7_5(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.SMOOTH_ITERATIONS = (int) slider.Value;
            }

            internal void <RecreateControls>b__7_6(MyGuiControlCheckbox x)
            {
                MySnapshot.ApplyReset = x.IsChecked;
            }

            internal void <RecreateControls>b__7_7(MyGuiControlSlider slider)
            {
                MyPredictedSnapshotSync.SMOOTH_DISTANCE = (int) slider.Value;
            }

            internal void <RecreateControls>b__7_8(MyGuiControlCheckbox x)
            {
                MySnapshotCache.PROPAGATE_TO_CONNECTIONS = x.IsChecked;
            }

            internal void <RecreateControls>b__7_9(MyGuiControlCheckbox x)
            {
                MyPredictedSnapshotSync.POSITION_CORRECTION = x.IsChecked;
            }
        }
    }
}

