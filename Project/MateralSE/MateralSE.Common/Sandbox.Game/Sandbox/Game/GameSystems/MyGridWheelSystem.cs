namespace Sandbox.Game.GameSystems
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using VRage.Game.Entity;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner]
    public class MyGridWheelSystem
    {
        public Vector3 AngularVelocity;
        private const int JUMP_FULL_CHARGE_TIME = 600;
        private bool m_wheelsChanged = false;
        private float m_maxRequiredPowerInput;
        private readonly MyCubeGrid m_grid;
        private readonly HashSet<MyMotorSuspension> m_wheels = new HashSet<MyMotorSuspension>();
        private readonly HashSet<MyMotorSuspension> m_wheelsNeedingUpdate = new HashSet<MyMotorSuspension>();
        private readonly MyResourceSinkComponent m_sinkComp;
        private bool m_handbrake;
        private bool m_brake;
        private ulong m_jumpStartTime;
        private bool m_lastJumpInput;
        public WheelJumpSate m_jumpState;
        [CompilerGenerated]
        private Action<MyCubeGrid> OnMotorUnregister;
        private int m_consecutiveCorrectionFrames;
        private Vector3 m_lastPhysicsAngularVelocityLateral;

        public event Action<MyCubeGrid> OnMotorUnregister
        {
            [CompilerGenerated] add
            {
                Action<MyCubeGrid> onMotorUnregister = this.OnMotorUnregister;
                while (true)
                {
                    Action<MyCubeGrid> a = onMotorUnregister;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Combine(a, value);
                    onMotorUnregister = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnMotorUnregister, action3, a);
                    if (ReferenceEquals(onMotorUnregister, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCubeGrid> onMotorUnregister = this.OnMotorUnregister;
                while (true)
                {
                    Action<MyCubeGrid> source = onMotorUnregister;
                    Action<MyCubeGrid> action3 = (Action<MyCubeGrid>) Delegate.Remove(source, value);
                    onMotorUnregister = Interlocked.CompareExchange<Action<MyCubeGrid>>(ref this.OnMotorUnregister, action3, source);
                    if (ReferenceEquals(onMotorUnregister, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGridWheelSystem(MyCubeGrid grid)
        {
            this.m_grid = grid;
            this.m_sinkComp = new MyResourceSinkComponent(1);
            this.m_sinkComp.Init(MyStringHash.GetOrCompute("Utility"), this.m_maxRequiredPowerInput, () => this.m_maxRequiredPowerInput);
            this.m_sinkComp.IsPoweredChanged += new Action(this.ReceiverIsPoweredChanged);
            grid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.OnGridPhysicsChanged);
        }

        public bool HasWorkingWheels(bool propulsion)
        {
            using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_wheels.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyMotorSuspension current = enumerator.Current;
                    if (current.IsWorking)
                    {
                        bool flag;
                        if (!propulsion)
                        {
                            flag = true;
                        }
                        else
                        {
                            if (current.RotorGrid == null)
                            {
                                continue;
                            }
                            if (current.RotorAngularVelocity.LengthSquared() <= 2f)
                            {
                                continue;
                            }
                            flag = true;
                        }
                        return flag;
                    }
                }
            }
            return false;
        }

        internal void InitControl(VRage.Game.Entity.MyEntity controller)
        {
            using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_wheels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.InitControl(controller);
                }
            }
        }

        [Event(null, 0x1a7), Reliable, Server, Broadcast]
        public static void InvokeJumpInternal(long gridId, bool initiate)
        {
            MyCubeGrid entityById = (MyCubeGrid) Sandbox.Game.Entities.MyEntities.GetEntityById(gridId, false);
            if (entityById != null)
            {
                if (Sync.IsServer && !MyEventContext.Current.IsLocallyInvoked)
                {
                    MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entityById);
                    if (((controllingPlayer == null) || (controllingPlayer.Client.SteamUserId != MyEventContext.Current.Sender.Value)) && !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value))
                    {
                        int num1;
                        controllingPlayer = MySession.Static.Players.GetPreviousControllingPlayer(entityById);
                        if ((controllingPlayer == null) || (controllingPlayer.Client.SteamUserId != MyEventContext.Current.Sender.Value))
                        {
                            num1 = (int) !MySession.Static.IsUserAdmin(MyEventContext.Current.Sender.Value);
                        }
                        else
                        {
                            num1 = 0;
                        }
                        bool kick = (bool) num1;
                        (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, kick, null, true);
                        MyEventContext.ValidationFailed();
                        return;
                    }
                }
                MyWheel.WheelExplosionLog(entityById, null, "InvokeJump" + initiate.ToString());
                MyGridWheelSystem wheelSystem = entityById.GridSystems.WheelSystem;
                if (!initiate)
                {
                    wheelSystem.m_jumpState = WheelJumpSate.Pushing;
                }
                else
                {
                    wheelSystem.m_jumpState = WheelJumpSate.Charging;
                    wheelSystem.m_jumpStartTime = MySandboxGame.Static.SimulationFrameCounter;
                }
                using (HashSet<MyMotorSuspension>.Enumerator enumerator = wheelSystem.Wheels.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnSuspensionJumpStateUpdated();
                    }
                }
            }
        }

        private static bool IsUsed(MyMotorSuspension motor) => 
            (motor.Enabled && motor.IsFunctional);

        private bool LateralCorrectionLogic(ref Vector3 gridDownNormal, ref Vector3 lateralCorrectionNormal, ref Vector3 linVelocityNormal)
        {
            Color? nullable;
            if (!Sync.IsServer && !this.m_grid.IsClientPredicted)
            {
                return false;
            }
            MyGridPhysics physics = this.m_grid.Physics;
            bool flag = false;
            MatrixD worldMatrix = this.m_grid.WorldMatrix;
            Vector3.TransformNormal(ref gridDownNormal, ref worldMatrix, out gridDownNormal);
            gridDownNormal = Vector3.ProjectOnPlane(ref gridDownNormal, ref linVelocityNormal);
            lateralCorrectionNormal = Vector3.ProjectOnPlane(ref lateralCorrectionNormal, ref linVelocityNormal);
            Vector3 vector = Vector3.Cross(gridDownNormal, linVelocityNormal);
            gridDownNormal.Normalize();
            lateralCorrectionNormal.Normalize();
            Vector3 vector2 = Vector3.ProjectOnVector(ref physics.AngularVelocity, ref linVelocityNormal);
            float num = vector2.Length();
            if (num > this.m_lastPhysicsAngularVelocityLateral.Length())
            {
                flag = true;
            }
            float num3 = Vector3.Dot(lateralCorrectionNormal, vector) * Math.Max(1, this.m_consecutiveCorrectionFrames);
            if (MyDebugDrawSettings.DEBUG_DRAW_WHEEL_SYSTEMS)
            {
                nullable = null;
                Vector3D translation = worldMatrix.Translation;
                MyRenderProxy.DebugDrawArrow3DDir(translation, lateralCorrectionNormal * 5f, Color.Yellow, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(translation, gridDownNormal * 5f, Color.Pink, nullable, false, 0.1, null, 0.5f, false);
                nullable = null;
                MyRenderProxy.DebugDrawArrow3DDir(translation, vector2 * 5f, Color.Red, nullable, false, 0.1, null, 0.5f, false);
            }
            this.m_lastPhysicsAngularVelocityLateral = vector2;
            if (Math.Abs((float) ((num3 * num) * num)) > 0.02f)
            {
                Vector3 vector3 = linVelocityNormal * num3;
                if (Vector3.Dot(vector3, vector2) > 0f)
                {
                    vector3 = Vector3.ClampToSphere(vector3, vector2.Length());
                    if (MyDebugDrawSettings.DEBUG_DRAW_WHEEL_SYSTEMS)
                    {
                        nullable = null;
                        MyRenderProxy.DebugDrawArrow3DDir(worldMatrix.Translation - (gridDownNormal * 5f), vector3 * 100f, Color.Red, nullable, false, 0.1, null, 0.5f, false);
                    }
                    physics.AngularVelocity -= vector3;
                    flag = true;
                }
            }
            return flag;
        }

        private void MotorEnabledChanged(MyTerminalBlock obj)
        {
            this.m_wheelsChanged = true;
        }

        public void OnBlockNeedsUpdateChanged(MyMotorSuspension motor)
        {
            if (!motor.NeedsPerFrameUpdate)
            {
                this.m_wheelsNeedingUpdate.Remove(motor);
            }
            else
            {
                if (this.m_wheelsNeedingUpdate.Count == 0)
                {
                    this.m_grid.MarkForUpdate();
                }
                this.m_wheelsNeedingUpdate.Add(motor);
            }
        }

        private void OnGridPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            if ((this.m_grid.GridSystems != null) && (this.m_grid.GridSystems.ControlSystem != null))
            {
                MyShipController shipController = this.m_grid.GridSystems.ControlSystem.GetShipController();
                if (shipController != null)
                {
                    this.InitControl(shipController);
                }
            }
        }

        private void ReceiverIsPoweredChanged()
        {
            using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_wheels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.UpdateIsWorking();
                }
            }
        }

        private void RecomputeWheelParameters()
        {
            this.m_wheelsChanged = false;
            this.m_maxRequiredPowerInput = 0f;
            foreach (MyMotorSuspension suspension in this.m_wheels)
            {
                if (IsUsed(suspension))
                {
                    this.m_maxRequiredPowerInput += suspension.RequiredPowerInput;
                }
            }
            this.m_sinkComp.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, this.m_maxRequiredPowerInput);
            this.m_sinkComp.Update();
        }

        public void Register(MyMotorSuspension motor)
        {
            this.m_wheels.Add(motor);
            this.OnBlockNeedsUpdateChanged(motor);
            this.m_wheelsChanged = true;
            motor.EnabledChanged += new Action<MyTerminalBlock>(this.MotorEnabledChanged);
            if (Sync.IsServer)
            {
                motor.Brake = this.m_handbrake;
            }
            this.m_grid.MarkForUpdate();
        }

        internal void ReleaseControl(VRage.Game.Entity.MyEntity controller)
        {
            using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_wheels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.ReleaseControl(controller);
                }
            }
            this.UpdateJumpControlState(false, false);
        }

        public void SetWheelJumpStrengthRatioIfJumpEngaged(ref float strength, float defaultStrength)
        {
            WheelJumpSate jumpState = this.m_jumpState;
            if (jumpState == WheelJumpSate.Charging)
            {
                strength = 0.0001f;
            }
            else if (jumpState == WheelJumpSate.Pushing)
            {
                float num2 = Math.Min((float) 1f, (float) (((float) (MySandboxGame.Static.SimulationFrameCounter - this.m_jumpStartTime)) / 600f));
                strength = defaultStrength + ((1f - defaultStrength) * num2);
            }
        }

        public void Unregister(MyMotorSuspension motor)
        {
            if (motor != null)
            {
                if ((motor.RotorGrid != null) && (this.OnMotorUnregister != null))
                {
                    this.OnMotorUnregister(motor.RotorGrid);
                }
                this.m_wheels.Remove(motor);
                this.m_wheelsNeedingUpdate.Remove(motor);
                this.m_wheelsChanged = true;
                motor.EnabledChanged -= new Action<MyTerminalBlock>(this.MotorEnabledChanged);
                this.m_grid.MarkForUpdate();
            }
        }

        public void UpdateBeforeSimulation()
        {
            if (this.m_wheelsChanged)
            {
                this.RecomputeWheelParameters();
            }
            if ((this.m_jumpState == WheelJumpSate.Pushing) || (this.m_jumpState == WheelJumpSate.Restore))
            {
                MyWheel.WheelExplosionLog(this.m_grid, null, "JumpUpdate: " + this.m_jumpState);
                using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.Wheels.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.OnSuspensionJumpStateUpdated();
                    }
                }
                WheelJumpSate jumpState = this.m_jumpState;
                if (jumpState == WheelJumpSate.Pushing)
                {
                    this.m_jumpState = WheelJumpSate.Restore;
                }
                else if (jumpState == WheelJumpSate.Restore)
                {
                    this.m_jumpState = WheelJumpSate.Idle;
                }
            }
            MyGridPhysics physics = this.m_grid.Physics;
            if (physics != null)
            {
                float num;
                Vector3 linearVelocity = physics.LinearVelocity;
                if ((linearVelocity.Normalize() / MyGridPhysics.GetShipMaxLinearVelocity(this.m_grid.GridSizeEnum)) > 1f)
                {
                    num = 1f;
                }
                bool forward = this.AngularVelocity.Z < 0f;
                bool backwards = this.AngularVelocity.Z > 0f;
                bool flag3 = forward | backwards;
                if (this.m_wheels.Count > 0)
                {
                    int num1;
                    int num2 = 0;
                    bool flag4 = !this.m_grid.GridSystems.GyroSystem.HasOverrideInput;
                    Vector3 zero = Vector3.Zero;
                    Vector3 groundNormal = Vector3.Zero;
                    foreach (MyMotorSuspension suspension in this.m_wheels)
                    {
                        suspension.AxleFrictionLogic(num, flag3 & (suspension.IsWorking && suspension.Propulsion));
                        suspension.Update();
                        if (flag4 && suspension.LateralCorrectionLogicInfo(ref groundNormal, ref zero))
                        {
                            num2++;
                        }
                        if (suspension.IsWorking)
                        {
                            if (suspension.Steering)
                            {
                                suspension.Steer(this.AngularVelocity.X, num);
                            }
                            if (suspension.Propulsion)
                            {
                                suspension.UpdatePropulsion(forward, backwards);
                            }
                        }
                    }
                    if ((num2 == 0) || Vector3.IsZero(ref zero))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !Vector3.IsZero(ref groundNormal);
                    }
                    bool flag5 = false;
                    if (flag4 & num1)
                    {
                        flag5 = this.LateralCorrectionLogic(ref zero, ref groundNormal, ref linearVelocity);
                    }
                    if (!flag5)
                    {
                        this.m_consecutiveCorrectionFrames = (int) (this.m_consecutiveCorrectionFrames * 0.8f);
                    }
                    else if (this.m_consecutiveCorrectionFrames < 10)
                    {
                        this.m_consecutiveCorrectionFrames++;
                    }
                }
            }
        }

        private void UpdateBrake()
        {
            using (HashSet<MyMotorSuspension>.Enumerator enumerator = this.m_wheels.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Brake = this.m_brake | this.m_handbrake;
                }
            }
        }

        public void UpdateJumpControlState(bool isCharging, bool sync)
        {
            if (isCharging && (this.m_grid.GridSystems.ResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true) != MyResourceStateEnum.Ok))
            {
                isCharging = false;
            }
            bool lastJumpInput = this.m_lastJumpInput;
            if ((sync || Sync.IsServer) && (lastJumpInput != isCharging))
            {
                bool flag2 = !lastJumpInput;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<long, bool>(s => new Action<long, bool>(MyGridWheelSystem.InvokeJumpInternal), this.m_grid.EntityId, flag2, targetEndpoint, position);
            }
            this.m_lastJumpInput = isCharging;
        }

        public HashSet<MyMotorSuspension> Wheels =>
            this.m_wheels;

        public int WheelCount =>
            this.m_wheels.Count;

        public bool HandBrake
        {
            get => 
                this.m_handbrake;
            set
            {
                if (this.m_handbrake != value)
                {
                    this.m_handbrake = value;
                    if (Sync.IsServer)
                    {
                        this.UpdateBrake();
                    }
                }
            }
        }

        public bool Brake
        {
            set
            {
                if (this.m_brake != value)
                {
                    this.m_brake = value;
                    this.UpdateBrake();
                }
            }
        }

        public bool NeedsPerFrameUpdate =>
            ((this.m_wheelsChanged || (this.m_wheelsNeedingUpdate.Count > 0)) || ((this.m_grid.Physics != null) && (this.m_grid.Physics.LinearVelocity.LengthSquared() > 0.1f)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGridWheelSystem.<>c <>9 = new MyGridWheelSystem.<>c();
            public static Func<IMyEventOwner, Action<long, bool>> <>9__44_0;

            internal Action<long, bool> <UpdateJumpControlState>b__44_0(IMyEventOwner s) => 
                new Action<long, bool>(MyGridWheelSystem.InvokeJumpInternal);
        }

        public enum WheelJumpSate
        {
            Idle,
            Charging,
            Pushing,
            Restore
        }
    }
}

