namespace Sandbox.Game.Screens.DebugScreens
{
    using Havok;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Gui;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Network;
    using VRageMath;

    [MyDebugScreen("VRage", "Physics")]
    public class MyGuiScreenDebugPhysics : MyGuiScreenDebugBase
    {
        public MyGuiScreenDebugPhysics() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugPhysics";

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.BackgroundColor = new Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Physics", new Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            Vector4? captionTextColor = null;
            captionOffset = null;
            base.AddCaption("Debug Draw", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Shapes", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SHAPES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Inertia tensors", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_INERTIA_TENSORS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Clusters", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_CLUSTERS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Forces", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_FORCES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Friction", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_FRICTION)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Constraints", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_CONSTRAINTS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Simulation islands", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_SIMULATION_ISLANDS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Motion types", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_PHYSICS_MOTION_TYPES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Velocities", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_VELOCITIES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Velocities interpolated", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_INTERPOLATED_VELOCITIES)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("TOI optimized grids", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyDebugDrawSettings.DEBUG_DRAW_TOI_OPTIMIZED_GRIDS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddSubcaption("Hk scheduling", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Havok multithreading", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_HAVOK_MULTITHREADING)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Parallel scheduling", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.ENABLE_HAVOK_PARALLEL_SCHEDULING)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            this.AddButton("Set on server", x => MyPhysics.CommitSchedulingSettingToServer(), null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            this.AddButton("Record VDB", delegate (MyGuiControlButton x) {
                MyPhysics.SyncVDBCamera = true;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(_ => new Action<string>(MyPhysics.ControlVDBRecording), DateTime.Now.ToString() + ".hkm", targetEndpoint, position);
            }, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            this.AddButton("Stop VDB recording", delegate (MyGuiControlButton x) {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(_ => new Action<string>(MyPhysics.ControlVDBRecording), null, targetEndpoint, position);
            }, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddSubcaption("Physics options", captionTextColor, captionOffset, 0.8f);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Enable Welding", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.WELD_LANDING_GEARS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Weld pistons", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.WELD_PISTONS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Wheel softness", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.WHEEL_SOFTNESS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset).SetToolTip("Needs to be true at world load.");
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Suspension power ratio", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.SUSPENSION_POWER_RATIO)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            base.AddCheckBox("Two step simulations", null, MemberHelper.GetMember<bool>(Expression.Lambda<Func<bool>>(Expression.Field(null, fieldof(MyFakes.TWO_STEP_SIMULATIONS)), Array.Empty<ParameterExpression>())), true, null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            this.AddButton("Start VDB", x => HkVDB.Start(), null, captionTextColor, captionOffset);
            captionTextColor = null;
            captionOffset = null;
            this.AddButton("Force cluster reorder", x => MyFakes.FORCE_CLUSTER_REORDER = true, null, captionTextColor, captionOffset);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugPhysics.<>c <>9 = new MyGuiScreenDebugPhysics.<>c();
            public static Action<MyGuiControlButton> <>9__2_13;
            public static Func<IMyEventOwner, Action<string>> <>9__2_23;
            public static Action<MyGuiControlButton> <>9__2_14;
            public static Func<IMyEventOwner, Action<string>> <>9__2_24;
            public static Action<MyGuiControlButton> <>9__2_15;
            public static Action<MyGuiControlButton> <>9__2_21;
            public static Action<MyGuiControlButton> <>9__2_22;

            internal void <RecreateControls>b__2_13(MyGuiControlButton x)
            {
                MyPhysics.CommitSchedulingSettingToServer();
            }

            internal void <RecreateControls>b__2_14(MyGuiControlButton x)
            {
                MyPhysics.SyncVDBCamera = true;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(_ => new Action<string>(MyPhysics.ControlVDBRecording), DateTime.Now.ToString() + ".hkm", targetEndpoint, position);
            }

            internal void <RecreateControls>b__2_15(MyGuiControlButton x)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string>(_ => new Action<string>(MyPhysics.ControlVDBRecording), null, targetEndpoint, position);
            }

            internal void <RecreateControls>b__2_21(MyGuiControlButton x)
            {
                HkVDB.Start();
            }

            internal void <RecreateControls>b__2_22(MyGuiControlButton x)
            {
                MyFakes.FORCE_CLUSTER_REORDER = true;
            }

            internal Action<string> <RecreateControls>b__2_23(IMyEventOwner _) => 
                new Action<string>(MyPhysics.ControlVDBRecording);

            internal Action<string> <RecreateControls>b__2_24(IMyEventOwner _) => 
                new Action<string>(MyPhysics.ControlVDBRecording);
        }
    }
}

