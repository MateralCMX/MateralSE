namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_ShipConnector)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyShipConnector), typeof(Sandbox.ModAPI.Ingame.IMyShipConnector) })]
    public class MyShipConnector : MyFunctionalBlock, IMyInventoryOwner, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyShipConnector, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyShipConnector
    {
        private static readonly MyTimeSpan DisconnectSleepTime = MyTimeSpan.FromSeconds(4.0);
        private const float MinStrength = 1E-06f;
        public readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> ThrowOut;
        public readonly VRage.Sync.Sync<bool, SyncDirection.BothWays> CollectAll;
        public readonly VRage.Sync.Sync<float, SyncDirection.BothWays> Strength;
        private readonly VRage.Sync.Sync<State, SyncDirection.FromServer> m_connectionState;
        private MyAttachableConveyorEndpoint m_attachableConveyorEndpoint;
        private int m_update10Counter;
        private bool m_canReloadDummies = true;
        private Vector3 m_connectionPosition;
        private float m_detectorRadius;
        private HkConstraint m_constraint;
        private MyShipConnector m_other;
        private bool m_defferedDisconnect;
        private static HashSet<MySlimBlock> m_tmpBlockSet = new HashSet<MySlimBlock>();
        private int m_manualDisconnectTime;
        private MyPhysicsBody m_connectorDummy;
        private Mode m_connectorMode;
        private bool m_hasConstraint;
        private MyConcurrentHashSet<VRage.Game.Entity.MyEntity> m_detectedFloaters = new MyConcurrentHashSet<VRage.Game.Entity.MyEntity>();
        private MyConcurrentHashSet<VRage.Game.Entity.MyEntity> m_detectedGrids = new MyConcurrentHashSet<VRage.Game.Entity.MyEntity>();
        protected HkConstraint m_connectorConstraint;
        protected HkFixedConstraintData m_connectorConstraintsData;
        protected HkConstraint m_ejectorConstraint;
        protected HkFixedConstraintData m_ejectorConstraintsData;
        private Matrix m_connectorDummyLocal;
        private Vector3 m_connectorCenter;
        private Vector3 m_connectorHalfExtents;
        private bool m_isMaster;
        private bool m_welded;
        private bool m_welding;
        private bool m_isInitOnceBeforeFrameUpdate;
        private long? m_lastAttachedOther;
        private long? m_lastWeldedOther;
        protected static MyTerminalControlButton<MyShipConnector> LockButton;
        protected static MyTerminalControlButton<MyShipConnector> UnlockButton;

        public MyShipConnector()
        {
            this.CreateTerminalControls();
            this.m_connectionState.ValueChanged += o => this.OnConnectionStateChanged();
            this.m_connectionState.AlwaysReject<State, SyncDirection.FromServer>();
            this.m_manualDisconnectTime = -((int) DisconnectSleepTime.Milliseconds);
            this.Strength.Validate = o => (this.Strength >= 0f) && (this.Strength <= 1f);
            if (!Sync.IsServer)
            {
                base.NeedsWorldMatrix = true;
            }
        }

        private void AddConstraint(HkConstraint newConstraint)
        {
            this.m_hasConstraint = true;
            if (newConstraint.RigidBodyA != newConstraint.RigidBodyB)
            {
                base.CubeGrid.Physics.AddConstraint(newConstraint);
            }
        }

        public bool AllowSelfPulling() => 
            false;

        protected override void BeforeDelete()
        {
            this.DisposeBodyConstraint(ref this.m_connectorConstraint, ref this.m_connectorConstraintsData);
            this.DisposeBodyConstraint(ref this.m_ejectorConstraint, ref this.m_ejectorConstraintsData);
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.BeforeDelete();
            this.DisposePhysicsBody(ref this.m_connectorDummy);
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (this.Connected)
            {
                this.Detach(true);
            }
            this.m_lastAttachedOther = null;
            this.m_lastWeldedOther = null;
            base.Closing();
        }

        private Vector3 ConstraintAxisGridSpace()
        {
            Vector3 vector = ((base.Max + base.Min) * base.CubeGrid.GridSize) * 0.5f;
            return Vector3.Normalize(Vector3.DominantAxisProjection(this.ConnectionPosition - vector));
        }

        private Vector3 ConstraintPositionInGridSpace()
        {
            Vector3 vector3;
            Vector3 vector = ((base.Max + base.Min) * base.CubeGrid.GridSize) * 0.5f;
            Vector3 position = Vector3.DominantAxisProjection(this.ConnectionPosition - vector);
            MatrixI matrix = new MatrixI(Vector3I.Zero, base.Orientation.Forward, base.Orientation.Up);
            Vector3.Transform(ref position, ref matrix, out vector3);
            return (vector + position);
        }

        public Vector3D ConstraintPositionWorld() => 
            Vector3D.Transform(this.ConstraintPositionInGridSpace(), base.CubeGrid.PositionComp.WorldMatrix);

        private void CreateBodyConstraint()
        {
            if (this.m_connectorDummy != null)
            {
                this.m_canReloadDummies = false;
                this.m_connectorDummy.Enabled = true;
                this.m_canReloadDummies = true;
                this.CreateBodyConstraint(this.m_connectorDummy, out this.m_connectorConstraintsData, out this.m_connectorConstraint);
                base.CubeGrid.Physics.AddConstraint(this.m_connectorConstraint);
            }
            if (base.Physics != null)
            {
                this.CreateBodyConstraint(base.Physics, out this.m_ejectorConstraintsData, out this.m_ejectorConstraint);
                base.CubeGrid.Physics.AddConstraint(this.m_ejectorConstraint);
            }
            base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnBodyPhysicsChanged);
            base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnBodyPhysicsChanged);
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
            }
        }

        protected void CreateBodyConstraint(MyPhysicsBody body, out HkFixedConstraintData constraintData, out HkConstraint constraint)
        {
            HkRigidBody rigidBody;
            constraintData = new HkFixedConstraintData();
            constraintData.SetSolvingMethod(HkSolvingMethod.MethodStabilized);
            constraintData.SetInertiaStabilizationFactor(1f);
            constraintData.SetInBodySpace(base.PositionComp.LocalMatrix, Matrix.CreateTranslation(-this.m_connectionPosition), base.CubeGrid.Physics, body);
            if ((base.CubeGrid.Physics.RigidBody2 == null) || !base.CubeGrid.Physics.Flags.HasFlag(RigidBodyFlag.RBF_DOUBLED_KINEMATIC))
            {
                rigidBody = base.CubeGrid.Physics.RigidBody;
            }
            else
            {
                rigidBody = base.CubeGrid.Physics.RigidBody2;
            }
            constraint = new HkConstraint(rigidBody, body.RigidBody, constraintData);
            uint collisionFilterInfo = base.CubeGrid.Physics.RigidBody.GetCollisionFilterInfo();
            collisionFilterInfo = HkGroupFilter.CalcFilterInfo(base.CubeGrid.Physics.RigidBody.Layer, HkGroupFilter.GetSystemGroupFromFilterInfo(collisionFilterInfo), 1, 1);
            constraint.WantRuntime = true;
            this.m_canReloadDummies = false;
            body.Enabled = true;
            this.m_canReloadDummies = true;
            body.RigidBody.SetCollisionFilterInfo(collisionFilterInfo);
            base.CubeGrid.Physics.HavokWorld.RefreshCollisionFilterOnEntity(body.RigidBody);
        }

        private void CreateConstraint(MyShipConnector otherConnector)
        {
            this.CreateConstraintNosync(otherConnector);
            if (Sync.IsServer)
            {
                State state = new State {
                    IsMaster = true,
                    OtherEntityId = otherConnector.EntityId,
                    MasterToSlave = null,
                    MasterToSlaveGrid = new MyDeltaTransform?(base.CubeGrid.WorldMatrix * MatrixD.Invert(this.m_other.WorldMatrix))
                };
                this.m_connectionState.Value = state;
                state = new State {
                    IsMaster = false,
                    OtherEntityId = base.EntityId,
                    MasterToSlave = null
                };
                otherConnector.m_connectionState.Value = state;
            }
        }

        private void CreateConstraintNosync(MyShipConnector otherConnector)
        {
            HkHingeConstraintData data = new HkHingeConstraintData();
            data.SetInBodySpace(this.ConstraintPositionInGridSpace(), otherConnector.ConstraintPositionInGridSpace(), this.ConstraintAxisGridSpace(), -otherConnector.ConstraintAxisGridSpace(), base.CubeGrid.Physics, otherConnector.CubeGrid.Physics);
            HkMalleableConstraintData constraintData = new HkMalleableConstraintData();
            constraintData.SetData(data);
            data.ClearHandle();
            data = null;
            constraintData.Strength = this.GetEffectiveStrength(otherConnector);
            HkConstraint newConstraint = new HkConstraint(base.CubeGrid.Physics.RigidBody, otherConnector.CubeGrid.Physics.RigidBody, constraintData);
            this.SetConstraint(otherConnector, newConstraint);
            otherConnector.SetConstraint(this, newConstraint);
            this.AddConstraint(newConstraint);
        }

        private HkBvShape CreateDetectorShape(Vector3 extents, Mode mode)
        {
            if (mode == Mode.Ejector)
            {
                HkPhantomCallbackShape shape = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_EnterEjector), new HkPhantomHandler(this.phantom_LeaveEjector));
                return new HkBvShape((HkShape) new HkBoxShape(extents), (HkShape) shape, HkReferencePolicy.TakeOwnership);
            }
            HkPhantomCallbackShape shape2 = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_EnterConnector), new HkPhantomHandler(this.phantom_LeaveConnector));
            return new HkBvShape((HkShape) new HkSphereShape(extents.AbsMax()), (HkShape) shape2, HkReferencePolicy.TakeOwnership);
        }

        private MyPhysicsBody CreatePhysicsBody(Mode mode, ref Matrix dummyLocal, ref Vector3 center, ref Vector3 halfExtents)
        {
            MyPhysicsBody body = null;
            if ((mode == Mode.Ejector) || Sync.IsServer)
            {
                HkBvShape shape = this.CreateDetectorShape(halfExtents, mode);
                int collisionFilter = (mode != Mode.Connector) ? 0x1a : 0x18;
                body = new MyPhysicsBody(this, RigidBodyFlag.RBF_UNLOCKED_SPEEDS) {
                    IsPhantom = true
                };
                HkMassProperties? massProperties = null;
                body.CreateFromCollisionObject((HkShape) shape, center, dummyLocal, massProperties, collisionFilter);
                body.RigidBody.ContactPointCallbackEnabled = true;
                shape.Base.RemoveReference();
            }
            return body;
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyShipConnector>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyShipConnector> switch3 = new MyTerminalControlOnOffSwitch<MyShipConnector>("ThrowOut", MySpaceTexts.Terminal_ThrowOut, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyShipConnector> switch4 = new MyTerminalControlOnOffSwitch<MyShipConnector>("ThrowOut", MySpaceTexts.Terminal_ThrowOut, tooltip, on, on);
                switch4.Getter = block => (bool) block.ThrowOut;
                MyTerminalControlOnOffSwitch<MyShipConnector> local43 = switch4;
                MyTerminalControlOnOffSwitch<MyShipConnector> local44 = switch4;
                local44.Setter = (block, value) => block.ThrowOut.Value = value;
                MyTerminalControlOnOffSwitch<MyShipConnector> onOff = local44;
                onOff.EnableToggleAction<MyShipConnector>();
                MyTerminalControlFactory.AddControl<MyShipConnector>(onOff);
                tooltip = new MyStringId();
                on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyShipConnector> switch1 = new MyTerminalControlOnOffSwitch<MyShipConnector>("CollectAll", MySpaceTexts.Terminal_CollectAll, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyShipConnector> switch2 = new MyTerminalControlOnOffSwitch<MyShipConnector>("CollectAll", MySpaceTexts.Terminal_CollectAll, tooltip, on, on);
                switch2.Getter = block => (bool) block.CollectAll;
                MyTerminalControlOnOffSwitch<MyShipConnector> local41 = switch2;
                MyTerminalControlOnOffSwitch<MyShipConnector> local42 = switch2;
                local42.Setter = (block, value) => block.CollectAll.Value = value;
                MyTerminalControlOnOffSwitch<MyShipConnector> local6 = local42;
                local6.EnableToggleAction<MyShipConnector>();
                MyTerminalControlFactory.AddControl<MyShipConnector>(local6);
                LockButton = new MyTerminalControlButton<MyShipConnector>("Lock", MySpaceTexts.BlockActionTitle_Lock, MySpaceTexts.Blank, b => b.TryConnect());
                LockButton.Enabled = b => b.IsWorking && b.InConstraint;
                LockButton.Visible = b => b.m_connectorMode == Mode.Connector;
                on = null;
                LockButton.EnableAction<MyShipConnector>(null, on, null).Enabled = b => b.m_connectorMode == Mode.Connector;
                MyTerminalControlFactory.AddControl<MyShipConnector>(LockButton);
                UnlockButton = new MyTerminalControlButton<MyShipConnector>("Unlock", MySpaceTexts.BlockActionTitle_Unlock, MySpaceTexts.Blank, b => b.TryDisconnect());
                UnlockButton.Enabled = b => b.IsWorking && b.InConstraint;
                UnlockButton.Visible = b => b.m_connectorMode == Mode.Connector;
                on = null;
                UnlockButton.EnableAction<MyShipConnector>(null, on, null).Enabled = b => b.m_connectorMode == Mode.Connector;
                MyTerminalControlFactory.AddControl<MyShipConnector>(UnlockButton);
                StringBuilder name = MyTexts.Get(MySpaceTexts.BlockActionTitle_SwitchLock);
                MyTerminalAction<MyShipConnector> action1 = new MyTerminalAction<MyShipConnector>("SwitchLock", name, MyTerminalActionIcons.TOGGLE);
                MyTerminalAction<MyShipConnector> action2 = new MyTerminalAction<MyShipConnector>("SwitchLock", name, MyTerminalActionIcons.TOGGLE);
                action2.Action = b => b.TrySwitch();
                MyTerminalAction<MyShipConnector> local39 = action2;
                MyTerminalAction<MyShipConnector> local40 = action2;
                local40.Writer = (b, sb) => b.WriteLockStateValue(sb);
                MyTerminalAction<MyShipConnector> local37 = local40;
                MyTerminalAction<MyShipConnector> action = local40;
                action.Enabled = b => b.m_connectorMode == Mode.Connector;
                MyTerminalControlFactory.AddAction<MyShipConnector>(action);
                MyTerminalControlSlider<MyShipConnector> slider1 = new MyTerminalControlSlider<MyShipConnector>("Strength", MySpaceTexts.BlockPropertyTitle_Connector_Strength, MySpaceTexts.BlockPropertyDescription_Connector_Strength);
                MyTerminalControlSlider<MyShipConnector> slider2 = new MyTerminalControlSlider<MyShipConnector>("Strength", MySpaceTexts.BlockPropertyTitle_Connector_Strength, MySpaceTexts.BlockPropertyDescription_Connector_Strength);
                slider2.Getter = x => (float) (x.Strength * 100f);
                MyTerminalControlSlider<MyShipConnector> local35 = slider2;
                MyTerminalControlSlider<MyShipConnector> local36 = slider2;
                local36.Setter = (x, v) => x.Strength.Value = v * 0.01f;
                MyTerminalControlSlider<MyShipConnector> slider = local36;
                slider.DefaultValue = 0.00015f;
                slider.SetLogLimits((float) 1E-06f, (float) 1f);
                slider.EnableActions<MyShipConnector>(0.05f, b => b.m_connectorMode == Mode.Connector, null);
                MyTerminalControlSlider<MyShipConnector> local33 = slider;
                MyTerminalControlSlider<MyShipConnector> local34 = slider;
                local34.Enabled = b => b.m_connectorMode == Mode.Connector;
                MyTerminalControlSlider<MyShipConnector> local31 = local34;
                MyTerminalControlSlider<MyShipConnector> local32 = local34;
                local32.Visible = b => b.m_connectorMode == Mode.Connector;
                MyTerminalControlSlider<MyShipConnector> local29 = local32;
                MyTerminalControlSlider<MyShipConnector> local30 = local32;
                local30.SetLimits(x => 0f, x => 100f);
                MyTerminalControlSlider<MyShipConnector> local27 = local30;
                MyTerminalControlSlider<MyShipConnector> control = local30;
                control.Writer = delegate (MyShipConnector x, StringBuilder result) {
                    if (x.Strength <= 1E-06f)
                    {
                        result.Append(MyTexts.Get(MyCommonTexts.Disabled));
                    }
                    else
                    {
                        result.AppendFormatedDecimal("", (float) (x.Strength * 100f), 4, " %");
                    }
                };
                MyTerminalControlFactory.AddControl<MyShipConnector>(control);
            }
        }

        private void CubeGrid_OnBodyPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            this.DisposeBodyConstraint(ref this.m_connectorConstraint, ref this.m_connectorConstraintsData);
            this.DisposeBodyConstraint(ref this.m_ejectorConstraint, ref this.m_ejectorConstraintsData);
            if ((Sync.IsServer && (!this.m_welding && this.InConstraint)) && this.m_hasConstraint)
            {
                this.RemoveConstraint(this.m_other, this.m_constraint);
                this.m_constraint = null;
                this.m_other.m_constraint = null;
                this.m_hasConstraint = false;
                this.m_other.m_hasConstraint = false;
                if (this.m_welded)
                {
                    this.RecreateConstraintInternal();
                }
                else if (((this.m_connectionState.Value.MasterToSlave == null) && (base.CubeGrid.Physics != null)) && (this.m_other.CubeGrid.Physics != null))
                {
                    this.CreateConstraintNosync(this.m_other);
                }
            }
            if (base.CubeGrid.Physics != null)
            {
                MyGridPhysics physics = base.CubeGrid.Physics;
                physics.EnabledChanged = (Action) Delegate.Remove(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
                MyGridPhysics physics2 = base.CubeGrid.Physics;
                physics2.EnabledChanged = (Action) Delegate.Combine(physics2.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void CubeGrid_OnHavokSystemIDChanged(int id)
        {
            MySandboxGame.Static.Invoke(delegate {
                if (base.CubeGrid.Physics != null)
                {
                    this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID);
                }
            }, "MyShipConnector::CubeGrid_OnHavokSystemIDChanged");
        }

        private void CubeGrid_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            this.CubeGrid_OnBodyPhysicsChanged(obj);
        }

        public override void DebugDrawPhysics()
        {
            base.DebugDrawPhysics();
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.DebugDraw();
            }
        }

        public void Detach(bool synchronize = true)
        {
            if (!this.IsMaster)
            {
                if ((this.m_other != null) && this.m_other.IsMaster)
                {
                    if (base.IsWorking && !this.Connected)
                    {
                        base.CubeGrid.Physics.RigidBody.Activate();
                    }
                    this.m_other.Detach(synchronize);
                }
            }
            else if (this.InConstraint && (this.m_other != null))
            {
                if (synchronize && Sync.IsServer)
                {
                    this.m_connectionState.Value = State.DetachedMaster;
                    this.m_other.m_connectionState.Value = State.Detached;
                }
                if (base.IsWorking && !this.Connected)
                {
                    base.CubeGrid.Physics.RigidBody.Activate();
                }
                MyShipConnector other = this.m_other;
                this.DetachInternal();
                if ((MyVisualScriptLogicProvider.ConnectorStateChanged != null) && (this.m_other != null))
                {
                    MyVisualScriptLogicProvider.ConnectorStateChanged(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name, this.m_other.EntityId, this.m_other.CubeGrid.EntityId, this.m_other.Name, this.m_other.CubeGrid.Name, false);
                }
                if (this.m_welded)
                {
                    int num1;
                    this.m_welding = true;
                    this.m_welded = false;
                    other.m_welded = false;
                    this.SetEmissiveStateWorking();
                    other.SetEmissiveStateWorking();
                    this.m_welding = false;
                    if (!Sync.IsServer || other.Closed)
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !other.MarkedForClose;
                    }
                    if ((num1 & synchronize) != 0)
                    {
                        this.TryAttach(new long?(other.EntityId));
                    }
                }
            }
        }

        private void DetachInternal()
        {
            if (!this.IsMaster)
            {
                this.m_other.DetachInternal();
            }
            else if ((this.InConstraint && (this.m_other != null)) && (this.m_other.InConstraint && (this.m_other.m_other != null)))
            {
                MyShipConnector other = this.m_other;
                HkConstraint constraint = this.m_constraint;
                this.Connected = false;
                this.UnsetConstraint();
                other.Connected = false;
                other.UnsetConstraint();
                bool flag = true;
                if (constraint != null)
                {
                    flag = this.RemoveConstraint(other, constraint);
                }
                if (this.Connected)
                {
                    if (flag)
                    {
                        this.RemoveLinks(other);
                    }
                    else
                    {
                        other.RemoveLinks(this);
                    }
                }
            }
        }

        protected void DisposeBodyConstraint(ref HkConstraint constraint, ref HkFixedConstraintData constraintData)
        {
            if (constraint != null)
            {
                base.CubeGrid.Physics.RemoveConstraint(constraint);
                constraint.Dispose();
                constraint = null;
                constraintData = null;
            }
        }

        private void DisposePhysicsBody(MyPhysicsBody body)
        {
            this.DisposePhysicsBody(ref body);
        }

        private void DisposePhysicsBody(ref MyPhysicsBody body)
        {
            if (body != null)
            {
                body.Close();
                body = null;
            }
        }

        private MyShipConnector FindOtherConnector(long? otherConnectorId = new long?())
        {
            MyShipConnector connector = null;
            BoundingSphereD sphere = new BoundingSphereD(this.ConnectionPosition, (double) this.m_detectorRadius);
            if (otherConnectorId != null)
            {
                Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyShipConnector>(otherConnectorId.Value, out connector, false);
            }
            else
            {
                connector = TryFindConnectorInGrid(ref sphere.Transform(base.CubeGrid.PositionComp.WorldMatrix), base.CubeGrid, this);
            }
            if (connector != null)
            {
                return connector;
            }
            using (ConcurrentEnumerator<SpinLockRef.Token, VRage.Game.Entity.MyEntity, HashSet<VRage.Game.Entity.MyEntity>.Enumerator> enumerator = this.m_detectedGrids.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    VRage.Game.Entity.MyEntity current = enumerator.Current;
                    if (!current.MarkedForClose && (current is MyCubeGrid))
                    {
                        MyCubeGrid objA = current as MyCubeGrid;
                        if (!ReferenceEquals(objA, base.CubeGrid))
                        {
                            connector = TryFindConnectorInGrid(ref sphere, objA, this);
                            if (connector != null)
                            {
                                return connector;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void GetBoxFromMatrix(Matrix m, out Vector3 halfExtents, out Vector3 position, out Quaternion orientation)
        {
            halfExtents = Vector3.Zero;
            position = Vector3.Zero;
            orientation = Quaternion.Identity;
        }

        protected float GetEffectiveStrength(MyShipConnector otherConnector)
        {
            float num = 0f;
            if (!this.IsReleasing)
            {
                num = Math.Min((float) this.Strength, (float) otherConnector.Strength);
                if (num < 1E-06f)
                {
                    num = 1E-06f;
                }
            }
            return num;
        }

        private MyShipConnector GetMaster(MyShipConnector first, MyShipConnector second)
        {
            MyCubeGrid cubeGrid = first.CubeGrid;
            MyCubeGrid grid2 = second.CubeGrid;
            if (cubeGrid.IsStatic != grid2.IsStatic)
            {
                if (cubeGrid.IsStatic)
                {
                    return second;
                }
                if (grid2.IsStatic)
                {
                    return first;
                }
            }
            else if (cubeGrid.GridSize != grid2.GridSize)
            {
                if (cubeGrid.GridSizeEnum == MyCubeSize.Large)
                {
                    return second;
                }
                if (grid2.GridSizeEnum == MyCubeSize.Large)
                {
                    return first;
                }
            }
            return ((first.EntityId < second.EntityId) ? second : first);
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyPositionAndOrientation? nullable1;
            State state = this.m_connectionState.Value;
            MyObjectBuilder_ShipConnector objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_ShipConnector;
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.ThrowOut = (bool) this.ThrowOut;
            objectBuilderCubeBlock.CollectAll = (bool) this.CollectAll;
            objectBuilderCubeBlock.Strength = (float) this.Strength;
            objectBuilderCubeBlock.ConnectedEntityId = state.OtherEntityId;
            objectBuilderCubeBlock.IsMaster = new bool?(state.IsMaster);
            MyObjectBuilder_ShipConnector connector2 = objectBuilderCubeBlock;
            if (state.MasterToSlave != null)
            {
                nullable1 = new MyPositionAndOrientation?(state.MasterToSlave.Value);
            }
            else
            {
                nullable1 = null;
            }
            connector2.MasterToSlaveTransform = nullable1;
            MyObjectBuilder_ShipConnector local1 = connector2;
            local1.MasterToSlaveGrid = state.MasterToSlaveGrid;
            return local1;
        }

        public PullInformation GetPullInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = new MyInventoryConstraint("Empty Constraint", null, true);
            return information1;
        }

        public PullInformation GetPushInformation() => 
            null;

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            base.SyncFlag = true;
            float maxRequiredInput = 0.001f;
            if (cubeGrid.GridSizeEnum == MyCubeSize.Small)
            {
                maxRequiredInput *= 0.01f;
            }
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute("Conveyors"), maxRequiredInput, () => base.CheckIsWorking() ? base.ResourceSink.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId) : 0f);
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            MyObjectBuilder_ShipConnector connector = objectBuilder as MyObjectBuilder_ShipConnector;
            Vector3 size = (Vector3) ((base.BlockDefinition.Size * base.CubeGrid.GridSize) * 0.8f);
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = new MyInventory(size.Volume, size, MyInventoryFlags.CanSend | MyInventoryFlags.CanReceive);
                base.Components.Add<MyInventoryBase>(inventory);
                inventory.Init(connector.Inventory);
            }
            this.ThrowOut.SetLocalValue(connector.ThrowOut);
            this.CollectAll.SetLocalValue(connector.CollectAll);
            base.SlimBlock.DeformationRatio = connector.DeformationRatio;
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.UpdateReceiver);
            base.EnabledChanged += new Action<MyTerminalBlock>(this.UpdateReceiver);
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME;
            if (base.CubeGrid.CreatePhysics)
            {
                this.LoadDummies(false);
            }
            this.Strength.SetLocalValue(MathHelper.Clamp(connector.Strength, 0f, 1f));
            if (connector.ConnectedEntityId != 0)
            {
                MyDeltaTransform? nullable1;
                if (connector.MasterToSlaveTransform != null)
                {
                    nullable1 = new MyDeltaTransform?(connector.MasterToSlaveTransform.Value);
                }
                else
                {
                    nullable1 = null;
                }
                MyDeltaTransform? nullable = nullable1;
                if (connector.Connected)
                {
                    MyDeltaTransform transform = new MyDeltaTransform();
                    nullable = new MyDeltaTransform?(transform);
                }
                if (connector.IsMaster == null)
                {
                    connector.IsMaster = new bool?(connector.ConnectedEntityId < base.EntityId);
                }
                this.IsMaster = connector.IsMaster.Value;
                State newValue = new State {
                    IsMaster = connector.IsMaster.Value,
                    OtherEntityId = connector.ConnectedEntityId,
                    MasterToSlave = nullable,
                    MasterToSlaveGrid = connector.MasterToSlaveGrid
                };
                this.m_connectionState.SetLocalValue(newValue);
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                this.m_isInitOnceBeforeFrameUpdate = true;
            }
            if (base.BlockDefinition.EmissiveColorPreset == MyStringHash.NullOrEmpty)
            {
                base.BlockDefinition.EmissiveColorPreset = MyStringHash.GetOrCompute("ConnectBlock");
            }
            base.IsWorkingChanged += new Action<MyCubeBlock>(this.MyShipConnector_IsWorkingChanged);
            base.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.MyShipConnector_OnPhysicsChanged);
            base.AddDebugRenderComponent(new MyDebugRenderCompoonentShipConnector(this));
        }

        private void LoadDummies(bool recreateOnlyConnector = false)
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(base.BlockDefinition.Model).Dummies)
            {
                bool flag = pair.Key.ToLower().Contains("connector");
                bool flag2 = flag || pair.Key.ToLower().Contains("ejector");
                if (flag || flag2)
                {
                    Matrix dummyLocal = Matrix.Normalize(pair.Value.Matrix);
                    this.m_connectionPosition = dummyLocal.Translation;
                    dummyLocal *= base.PositionComp.LocalMatrix;
                    Vector3 halfExtents = pair.Value.Matrix.Scale / 2f;
                    halfExtents = new Vector3(halfExtents.X, halfExtents.Y, halfExtents.Z);
                    this.m_detectorRadius = halfExtents.AbsMax();
                    Vector3 center = pair.Value.Matrix.Translation;
                    if (flag)
                    {
                        MySandboxGame.Static.Invoke(() => this.RecreateConnectorDummy(ref dummyLocal, ref center, ref halfExtents), "MyShipConnector::RecreateConnectorDummy");
                    }
                    if (flag2 && !recreateOnlyConnector)
                    {
                        this.DisposePhysicsBody(base.Physics);
                        base.Physics = this.CreatePhysicsBody(Mode.Ejector, ref dummyLocal, ref center, ref halfExtents);
                    }
                    this.m_connectorMode = !flag ? Mode.Ejector : Mode.Connector;
                    break;
                }
            }
        }

        private void MyShipConnector_IsWorkingChanged(MyCubeBlock obj)
        {
            if ((Sync.IsServer && this.Connected) && (!base.IsFunctional || !base.IsWorking))
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        private void MyShipConnector_OnPhysicsChanged(VRage.Game.Entity.MyEntity obj)
        {
            if ((!base.MarkedForClose && base.CubeGrid.CreatePhysics) && ((this.m_connectorMode == Mode.Connector) && this.m_canReloadDummies))
            {
                this.LoadDummies(true);
            }
        }

        [Event(null, 0x119), Reliable, Broadcast]
        public void NotifyDisconnectTime()
        {
            if (this.m_other != null)
            {
                this.m_manualDisconnectTime = this.m_other.m_manualDisconnectTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.OnAddedToScene(source);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.Activate();
            }
            this.SetEmissiveStateWorking();
            if (base.CubeGrid.Physics != null)
            {
                MyGridPhysics physics = base.CubeGrid.Physics;
                physics.EnabledChanged = (Action) Delegate.Combine(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
        }

        private void OnConnectionStateChanged()
        {
            this.UpdateTerminalVisuals();
            if (!Sync.IsServer)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                if ((this.Connected || this.InConstraint) && (this.m_connectionState.Value.MasterToSlave != null))
                {
                    this.Detach(false);
                }
            }
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            oldGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            oldGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (oldGrid.Physics != null)
            {
                MyGridPhysics physics = oldGrid.Physics;
                physics.EnabledChanged = (Action) Delegate.Remove(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
            if (base.CubeGrid.Physics != null)
            {
                MyGridPhysics physics = base.CubeGrid.Physics;
                physics.EnabledChanged = (Action) Delegate.Combine(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
            base.OnCubeGridChanged(oldGrid);
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnDestroy();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        protected override void OnOwnershipChanged()
        {
            base.OnOwnershipChanged();
            if (this.InConstraint && !this.m_other.FriendlyWithBlock(this))
            {
                this.Detach(true);
            }
        }

        private void OnPhysicsEnabledChanged()
        {
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.Enabled = base.CubeGrid.Physics.Enabled;
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        public override void OnRemovedFromScene(object source)
        {
            this.DisposeBodyConstraint(ref this.m_connectorConstraint, ref this.m_connectorConstraintsData);
            this.DisposeBodyConstraint(ref this.m_ejectorConstraint, ref this.m_ejectorConstraintsData);
            base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            if (base.CubeGrid.Physics != null)
            {
                MyGridPhysics physics = base.CubeGrid.Physics;
                physics.EnabledChanged = (Action) Delegate.Remove(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
            base.OnRemovedFromScene(source);
            if (base.Physics != null)
            {
                MyPhysicsBody physics = base.Physics;
                physics.EnabledChanged = (Action) Delegate.Remove(physics.EnabledChanged, new Action(this.OnPhysicsEnabledChanged));
            }
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.Deactivate();
            }
            if (this.InConstraint)
            {
                long? nullable;
                long? nullable1;
                long? lastAttachedOther;
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                if (this.m_other != null)
                {
                    nullable1 = new long?(this.m_other.EntityId);
                }
                else
                {
                    nullable = null;
                    nullable1 = nullable;
                }
                this.m_lastAttachedOther = nullable1;
                if (this.m_welded)
                {
                    lastAttachedOther = this.m_lastAttachedOther;
                }
                else
                {
                    nullable = null;
                    lastAttachedOther = nullable;
                }
                this.m_lastWeldedOther = lastAttachedOther;
                this.Detach(false);
            }
            base.ResourceSink.IsPoweredChanged -= new Action(this.Receiver_IsPoweredChanged);
        }

        private void phantom_EnterConnector(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            using (allEntities.GetClearToken<VRage.ModAPI.IMyEntity>())
            {
                using (List<VRage.ModAPI.IMyEntity>.Enumerator enumerator = allEntities.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyCubeGrid topMostParent = enumerator.Current.GetTopMostParent(null) as MyCubeGrid;
                        if ((topMostParent != null) && !ReferenceEquals(topMostParent, base.CubeGrid))
                        {
                            this.m_detectedGrids.Add(topMostParent);
                        }
                    }
                }
            }
        }

        private void phantom_EnterEjector(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            bool flag = false;
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            foreach (VRage.ModAPI.IMyEntity entity in allEntities)
            {
                if (entity is MyFloatingObject)
                {
                    flag |= this.m_detectedFloaters.Count == 1;
                    this.m_detectedFloaters.Add((MyFloatingObject) entity);
                }
            }
            allEntities.Clear();
            if (flag)
            {
                this.SetEmissiveStateWorking();
            }
        }

        private void phantom_LeaveConnector(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            foreach (VRage.ModAPI.IMyEntity entity in allEntities)
            {
                this.m_detectedGrids.Remove(entity as MyCubeGrid);
            }
            allEntities.Clear();
        }

        private void phantom_LeaveEjector(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            bool flag = this.m_detectedFloaters.Count == 2;
            List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
            foreach (VRage.ModAPI.IMyEntity entity in allEntities)
            {
                this.m_detectedFloaters.Remove((VRage.Game.Entity.MyEntity) entity);
            }
            allEntities.Clear();
            if (flag)
            {
                this.SetEmissiveStateWorking();
            }
        }

        [Event(null, 0x439), Reliable, Broadcast]
        private void PlayActionSound()
        {
            bool? nullable = null;
            base.m_soundEmitter.PlaySound(base.m_actionSound, false, false, false, false, false, nullable);
        }

        private Vector3 ProjectPerpendicularFromWorld(Vector3 worldPerpAxis)
        {
            Vector3 vector = this.ConstraintAxisGridSpace();
            Vector3 vector1 = Vector3.TransformNormal(worldPerpAxis, base.CubeGrid.PositionComp.WorldMatrixNormalizedInv);
            float num = Vector3.Dot(vector1, vector);
            Vector3.Normalize(vector1 - (num * vector));
            return Vector3.Normalize(vector1 - (num * vector));
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        private void RecreateConnectorDummy(ref Matrix dummyLocal, ref Vector3 center, ref Vector3 halfExtents)
        {
            this.DisposeBodyConstraint(ref this.m_connectorConstraint, ref this.m_connectorConstraintsData);
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.Close();
            }
            this.m_connectorDummyLocal = dummyLocal;
            this.m_connectorCenter = center;
            this.m_connectorHalfExtents = halfExtents;
            this.m_connectorDummy = this.CreatePhysicsBody(Mode.Connector, ref dummyLocal, ref center, ref halfExtents);
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        private void RecreateConstraintInternal()
        {
            if (this.m_constraint != null)
            {
                this.RemoveConstraint(this.m_other, this.m_constraint);
                this.m_constraint = null;
                this.m_other.m_constraint = null;
            }
            HkFixedConstraintData constraintData = new HkFixedConstraintData();
            Matrix pivotA = (Matrix) (MatrixD.CreateWorld(Vector3D.Transform(this.ConnectionPosition, base.CubeGrid.WorldMatrix)) * base.CubeGrid.PositionComp.WorldMatrixNormalizedInv);
            Matrix pivotB = (Matrix) (MatrixD.CreateWorld(Vector3D.Transform(this.ConnectionPosition, base.CubeGrid.WorldMatrix)) * this.m_other.CubeGrid.PositionComp.WorldMatrixNormalizedInv);
            constraintData.SetInBodySpaceInternal(ref pivotA, ref pivotB);
            constraintData.SetSolvingMethod(HkSolvingMethod.MethodStabilized);
            HkConstraint newConstraint = new HkConstraint(base.CubeGrid.Physics.RigidBody, this.m_other.CubeGrid.Physics.RigidBody, constraintData);
            this.SetConstraint(this.m_other, newConstraint);
            this.m_other.SetConstraint(this, newConstraint);
            this.AddConstraint(newConstraint);
        }

        private bool RemoveConstraint(MyShipConnector otherConnector, HkConstraint constraint)
        {
            if (!this.m_hasConstraint)
            {
                if (otherConnector.m_hasConstraint)
                {
                    otherConnector.RemoveConstraint(this, constraint);
                }
                return false;
            }
            if (base.CubeGrid.Physics != null)
            {
                base.CubeGrid.Physics.RemoveConstraint(constraint);
            }
            this.m_hasConstraint = false;
            constraint.Dispose();
            return true;
        }

        private void RemoveLinks(MyShipConnector otherConnector)
        {
            this.m_attachableConveyorEndpoint.Detach(otherConnector.m_attachableConveyorEndpoint);
            if (!ReferenceEquals(base.CubeGrid, otherConnector.CubeGrid))
            {
                this.OnConstraintRemoved(GridLinkTypeEnum.Logical, otherConnector.CubeGrid);
                this.OnConstraintRemoved(GridLinkTypeEnum.Physical, otherConnector.CubeGrid);
                MyFixedGrids.BreakLink(base.CubeGrid, otherConnector.CubeGrid, this);
                MyGridPhysicalHierarchy.Static.BreakLink(base.EntityId, base.CubeGrid, otherConnector.CubeGrid);
            }
            base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
            otherConnector.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
        }

        void IMyConveyorEndpointBlock.InitializeConveyorEndpoint()
        {
            this.m_attachableConveyorEndpoint = new MyAttachableConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_attachableConveyorEndpoint));
        }

        void Sandbox.ModAPI.Ingame.IMyShipConnector.Connect()
        {
            if (this.m_connectorMode == Mode.Connector)
            {
                this.TryConnect();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyShipConnector.Disconnect()
        {
            if (this.m_connectorMode == Mode.Connector)
            {
                this.TryDisconnect();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyShipConnector.ToggleConnect()
        {
            if (this.m_connectorMode == Mode.Connector)
            {
                this.TrySwitch();
            }
        }

        private void SetConstraint(MyShipConnector other, HkConstraint newConstraint)
        {
            this.m_other = other;
            this.m_constraint = newConstraint;
            this.SetEmissiveStateWorking();
        }

        public override bool SetEmissiveStateDamaged() => 
            base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Damaged, base.Render.RenderObjectIDs[0], "Emissive");

        public override bool SetEmissiveStateDisabled() => 
            this.SetEmissiveStateWorking();

        public override bool SetEmissiveStateWorking()
        {
            if (this.InConstraint)
            {
                MyShipConnector other = this;
                if ((this.m_other != null) && this.m_other.IsMaster)
                {
                    other = this.m_other;
                }
                return (!other.Connected ? (!other.IsReleasing ? base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Constraint, base.Render.RenderObjectIDs[0], "Emissive") : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Autolock, base.Render.RenderObjectIDs[0], "Emissive")) : base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Locked, base.Render.RenderObjectIDs[0], "Emissive"));
            }
            if (base.IsWorking)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Working, base.Render.RenderObjectIDs[0], "Emissive");
            }
            if (base.IsFunctional)
            {
                return base.SetEmissiveState(MyCubeBlock.m_emissiveNames.Disabled, base.Render.RenderObjectIDs[0], "Emissive");
            }
            this.SetEmissiveStateDamaged();
            return false;
        }

        private void TryAttach(long? otherConnectorId = new long?())
        {
            MyShipConnector second = this.FindOtherConnector(otherConnectorId);
            if (((second == null) || (!second.FriendlyWithBlock(this) || (base.CubeGrid.Physics == null))) || (second.CubeGrid.Physics == null))
            {
                this.m_connectionState.Value = State.DetachedMaster;
            }
            else
            {
                Vector3D vectord = this.ConstraintPositionWorld();
                Vector3D vectord2 = second.ConstraintPositionWorld();
                (vectord2 - vectord).LengthSquared();
                if (((second.m_connectorMode == Mode.Connector) && second.IsFunctional) && ((vectord2 - vectord).LengthSquared() < 0.34999999403953552))
                {
                    MyShipConnector master = this.GetMaster(this, second);
                    master.IsMaster = true;
                    if (ReferenceEquals(master, this))
                    {
                        this.CreateConstraint(second);
                        second.IsMaster = false;
                    }
                    else
                    {
                        second.CreateConstraint(this);
                        this.IsMaster = false;
                    }
                }
            }
        }

        [Event(null, 0xfb), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        public void TryConnect()
        {
            if (this.InConstraint && !this.Connected)
            {
                if (Sync.IsServer)
                {
                    this.Weld();
                }
                else
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipConnector>(this, x => new Action(x.TryConnect), targetEndpoint);
                }
            }
        }

        [Event(null, 0x107), Reliable, Server(ValidationType.Ownership | ValidationType.Access)]
        public void TryDisconnect()
        {
            if (this.InConstraint && this.Connected)
            {
                EndpointId id;
                this.m_manualDisconnectTime = this.m_other.m_manualDisconnectTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                if (Sync.IsServer)
                {
                    this.Detach(true);
                    id = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipConnector>(this, x => new Action(x.NotifyDisconnectTime), id);
                }
                else
                {
                    id = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipConnector>(this, x => new Action(x.TryDisconnect), id);
                }
            }
        }

        private static MyShipConnector TryFindConnectorInGrid(ref BoundingSphereD sphere, MyCubeGrid grid, MyShipConnector thisConnector = null)
        {
            m_tmpBlockSet.Clear();
            grid.GetBlocksInsideSphere(ref sphere, m_tmpBlockSet, false);
            using (HashSet<MySlimBlock>.Enumerator enumerator = m_tmpBlockSet.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MySlimBlock current = enumerator.Current;
                    if ((current.FatBlock != null) && (current.FatBlock is MyShipConnector))
                    {
                        MyShipConnector fatBlock = current.FatBlock as MyShipConnector;
                        if (!fatBlock.InConstraint && (!ReferenceEquals(fatBlock, thisConnector) && (fatBlock.IsWorking && fatBlock.FriendlyWithBlock(thisConnector))))
                        {
                            m_tmpBlockSet.Clear();
                            return fatBlock;
                        }
                    }
                }
            }
            m_tmpBlockSet.Clear();
            return null;
        }

        private void TryReattachAfterMerge()
        {
            if ((base.Enabled && (!this.InConstraint && (this.m_connectionState.Value.MasterToSlave == null))) && (this.m_lastAttachedOther != null))
            {
                this.TryAttach(this.m_lastAttachedOther);
                if (this.m_lastWeldedOther != null)
                {
                    this.TryConnect();
                }
            }
            this.m_lastAttachedOther = null;
            this.m_lastWeldedOther = null;
        }

        public void TrySwitch()
        {
            if (this.InConstraint)
            {
                if (this.Connected)
                {
                    this.TryDisconnect();
                }
                else
                {
                    this.TryConnect();
                }
            }
        }

        private void TryThrowOutItem()
        {
            float num = (base.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? 2.5f : 0.5f;
            List<MyPhysicalInventoryItem> items = this.GetInventory(0).GetItems();
            int num2 = 0;
            while (true)
            {
                if (num2 < this.GetInventory(0).GetItems().Count)
                {
                    MyPhysicalItemDefinition definition;
                    MyPhysicalInventoryItem item;
                    MatrixD? nullable2;
                    float randomFloat = MyUtils.GetRandomFloat(0f, (base.CubeGrid.GridSizeEnum == MyCubeSize.Large) ? 0.5f : 0.07f);
                    Vector3 vector = MyUtils.GetRandomVector3CircleNormalized();
                    Vector3D position = (Vector3D.Transform(this.ConnectionPosition, base.CubeGrid.PositionComp.WorldMatrix) + ((base.PositionComp.WorldMatrix.Right * vector.X) * randomFloat)) + ((base.PositionComp.WorldMatrix.Up * vector.Z) * randomFloat);
                    if (!MyDefinitionManager.Static.TryGetPhysicalItemDefinition(items[num2].Content.GetId(), out definition))
                    {
                        continue;
                    }
                    float num4 = definition.Size.Max() * 0.5f;
                    position += base.PositionComp.WorldMatrix.Forward * num4;
                    MyFixedPoint a = (MyFixedPoint) (num / definition.Volume);
                    if ((items[num2].Content.TypeId != typeof(MyObjectBuilder_Ore)) && (items[num2].Content.TypeId != typeof(MyObjectBuilder_Ingot)))
                    {
                        a = MyFixedPoint.Ceiling(a);
                    }
                    MyFixedPoint amount = 0;
                    if (items[num2].Amount < a)
                    {
                        num -= ((float) items[num2].Amount) * definition.Volume;
                        amount = items[num2].Amount;
                        item = items[num2];
                        MyFixedPoint? nullable = null;
                        nullable2 = null;
                        this.GetInventory(0).RemoveItems(items[num2].ItemId, nullable, true, false, nullable2);
                        num2++;
                    }
                    else
                    {
                        num = 0f;
                        item = new MyPhysicalInventoryItem(items[num2].GetObjectBuilder()) {
                            Amount = a
                        };
                        amount = a;
                        nullable2 = null;
                        this.GetInventory(0).RemoveItems(items[num2].ItemId, new MyFixedPoint?(a), true, false, nullable2);
                    }
                    if (amount > 0)
                    {
                        MyFloatingObjects.Spawn(item, position, base.PositionComp.WorldMatrix.Forward, base.PositionComp.WorldMatrix.Up, base.CubeGrid.Physics, delegate (VRage.Game.Entity.MyEntity entity) {
                            MyParticleEffect effect;
                            MyPhysicsComponentBase physics = entity.Physics;
                            physics.LinearVelocity += base.PositionComp.WorldMatrix.Forward;
                            if (base.m_soundEmitter != null)
                            {
                                bool? nullable = null;
                                base.m_soundEmitter.PlaySound(base.m_actionSound, false, false, false, false, false, nullable);
                                EndpointId targetEndpoint = new EndpointId();
                                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyShipConnector>(this, x => new Action(x.PlayActionSound), targetEndpoint);
                            }
                            if (MyParticlesManager.TryCreateParticleEffect("Smoke_Collector", entity.WorldMatrix, out effect))
                            {
                                effect.Velocity = base.CubeGrid.Physics.LinearVelocity;
                            }
                        });
                        return;
                    }
                }
                return;
            }
        }

        private void UnsetConstraint()
        {
            this.m_other = null;
            this.m_constraint = null;
            this.SetEmissiveStateWorking();
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (!Sync.IsServer || !base.IsWorking)
            {
                if ((Sync.IsServer && !base.IsWorking) && this.InConstraint)
                {
                    this.Detach(true);
                }
            }
            else
            {
                this.m_update10Counter++;
                if (((this.m_update10Counter % 8) == 0) && base.Enabled)
                {
                    if (this.CollectAll != null)
                    {
                        MyGridConveyorSystem.PullAllRequest(this, this.GetInventory(0), base.OwnerId, true);
                    }
                    if ((!this.InConstraint && (this.ThrowOut != null)) && (this.m_detectedFloaters.Count < 2))
                    {
                        this.TryThrowOutItem();
                    }
                }
                if (((this.m_detectedFloaters.Count == 0) && ((this.m_connectorMode == Mode.Connector) && (((this.m_update10Counter % 4) == 0) && (base.Enabled && !this.InConstraint)))) && (this.m_connectionState.Value.MasterToSlave == null))
                {
                    long? otherConnectorId = null;
                    this.TryAttach(otherConnectorId);
                }
            }
            if ((base.IsWorking && this.InConstraint) && !this.Connected)
            {
                float effectiveStrength = this.GetEffectiveStrength(this.m_other);
                HkMalleableConstraintData constraintData = this.m_constraint.ConstraintData as HkMalleableConstraintData;
                if (((constraintData != null) && (constraintData.Strength != effectiveStrength)) && this.IsMaster)
                {
                    constraintData.Strength = effectiveStrength;
                    base.CubeGrid.Physics.RigidBody.Activate();
                    this.SetEmissiveStateWorking();
                    this.m_other.SetEmissiveStateWorking();
                }
            }
            if ((Sync.IsServer && (this.InConstraint && !this.Connected)) && (this.m_connectorMode == Mode.Connector))
            {
                Vector3D vectord = this.ConstraintPositionWorld();
                if ((this.m_other.ConstraintPositionWorld() - vectord).LengthSquared() > 0.5)
                {
                    this.Detach(true);
                }
            }
            this.UpdateConnectionState();
        }

        private void UpdateConnectionState()
        {
            if (this.m_isInitOnceBeforeFrameUpdate)
            {
                this.m_isInitOnceBeforeFrameUpdate = false;
            }
            else if (((this.m_other == null) && (this.m_connectionState.Value.OtherEntityId != 0)) && Sync.IsServer)
            {
                this.m_connectionState.Value = State.Detached;
            }
            if (this.IsMaster && (base.CubeGrid.Physics != null))
            {
                State state = this.m_connectionState.Value;
                if (state.OtherEntityId == 0)
                {
                    if (this.InConstraint)
                    {
                        this.Detach(false);
                        this.SetEmissiveStateWorking();
                        if (this.m_other != null)
                        {
                            this.m_other.SetEmissiveStateWorking();
                        }
                    }
                }
                else if (state.MasterToSlave == null)
                {
                    MyShipConnector connector;
                    if (this.Connected || (this.InConstraint && (this.m_other.EntityId != state.OtherEntityId)))
                    {
                        this.Detach(false);
                        this.SetEmissiveStateWorking();
                        if (this.m_other != null)
                        {
                            this.m_other.SetEmissiveStateWorking();
                        }
                    }
                    if ((!this.InConstraint && (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyShipConnector>(state.OtherEntityId, out connector, false) && (connector.FriendlyWithBlock(this) && (!connector.Closed && (!connector.MarkedForClose && (base.Physics != null)))))) && (connector.Physics != null))
                    {
                        if ((!Sync.IsServer && (state.MasterToSlaveGrid != null)) && !ReferenceEquals(base.CubeGrid, connector.CubeGrid))
                        {
                            if (base.CubeGrid.IsStatic)
                            {
                                connector.WorldMatrix = MatrixD.Multiply(MatrixD.Invert(state.MasterToSlaveGrid.Value), base.CubeGrid.WorldMatrix);
                            }
                            else
                            {
                                base.CubeGrid.WorldMatrix = MatrixD.Multiply(state.MasterToSlaveGrid.Value, connector.WorldMatrix);
                            }
                        }
                        this.CreateConstraintNosync(connector);
                        this.SetEmissiveStateWorking();
                        if (this.m_other != null)
                        {
                            this.m_other.SetEmissiveStateWorking();
                        }
                    }
                }
                else
                {
                    MyShipConnector connector2;
                    if (this.Connected && (this.m_other.EntityId != state.OtherEntityId))
                    {
                        this.Detach(false);
                        this.SetEmissiveStateWorking();
                        if (this.m_other != null)
                        {
                            this.m_other.SetEmissiveStateWorking();
                        }
                    }
                    Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyShipConnector>(state.OtherEntityId, out connector2, false);
                    if ((!this.Connected && (connector2 != null)) && connector2.FriendlyWithBlock(this))
                    {
                        MatrixD? nullable1;
                        this.m_other = connector2;
                        MyDeltaTransform? masterToSlave = state.MasterToSlave;
                        if ((masterToSlave != null) && masterToSlave.Value.IsZero)
                        {
                            masterToSlave = null;
                        }
                        if ((!Sync.IsServer && (state.MasterToSlaveGrid != null)) && !ReferenceEquals(base.CubeGrid, connector2.CubeGrid))
                        {
                            if (base.CubeGrid.IsStatic)
                            {
                                connector2.WorldMatrix = MatrixD.Multiply(MatrixD.Invert(state.MasterToSlaveGrid.Value), base.CubeGrid.WorldMatrix);
                            }
                            else
                            {
                                base.CubeGrid.WorldMatrix = MatrixD.Multiply(state.MasterToSlaveGrid.Value, connector2.WorldMatrix);
                            }
                        }
                        MyDeltaTransform? nullable2 = masterToSlave;
                        if (nullable2 != null)
                        {
                            nullable1 = new MatrixD?(nullable2.GetValueOrDefault());
                        }
                        else
                        {
                            nullable1 = null;
                        }
                        this.Weld(nullable1);
                        this.SetEmissiveStateWorking();
                        if (this.m_other != null)
                        {
                            this.m_other.SetEmissiveStateWorking();
                        }
                    }
                }
            }
        }

        internal void UpdateHavokCollisionSystemID(int HavokCollisionSystemID)
        {
            if (this.m_connectorDummy != null)
            {
                uint info = HkGroupFilter.CalcFilterInfo(0x18, HavokCollisionSystemID, 1, 1);
                this.m_connectorDummy.RigidBody.SetCollisionFilterInfo(info);
                if (this.m_connectorDummy.HavokWorld != null)
                {
                    this.m_connectorDummy.HavokWorld.RefreshCollisionFilterOnEntity(this.m_connectorDummy.RigidBody);
                }
            }
            if (base.Physics != null)
            {
                uint info = HkGroupFilter.CalcFilterInfo(0x1a, HavokCollisionSystemID, 1, 1);
                base.Physics.RigidBody.SetCollisionFilterInfo(info);
                if (base.Physics.HavokWorld != null)
                {
                    base.Physics.HavokWorld.RefreshCollisionFilterOnEntity(base.Physics.RigidBody);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if ((Sync.IsServer && this.Connected) && (!base.IsFunctional || !base.IsWorking))
            {
                this.m_connectionState.Value = !this.IsMaster ? State.Detached : State.DetachedMaster;
            }
            this.DisposeBodyConstraint(ref this.m_connectorConstraint, ref this.m_connectorConstraintsData);
            this.DisposeBodyConstraint(ref this.m_ejectorConstraint, ref this.m_ejectorConstraintsData);
            if (base.Physics != null)
            {
                base.Physics.Enabled = true;
            }
            if (this.m_connectorDummy != null)
            {
                this.m_connectorDummy.Close();
                this.m_connectorDummy = this.CreatePhysicsBody(Mode.Connector, ref this.m_connectorDummyLocal, ref this.m_connectorCenter, ref this.m_connectorHalfExtents);
            }
            this.CreateBodyConstraint();
            this.UpdateConnectionState();
            this.TryReattachAfterMerge();
            this.UpdateTerminalVisuals();
        }

        private void UpdateReceiver()
        {
            base.ResourceSink.Update();
        }

        private void UpdateReceiver(MyTerminalBlock block)
        {
            base.ResourceSink.Update();
        }

        private void UpdateTerminalVisuals()
        {
            LockButton.UpdateVisual();
            UnlockButton.UpdateVisual();
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        private void Weld()
        {
            MatrixD? masterToSlave = null;
            (this.IsMaster ? this : this.m_other).Weld(masterToSlave);
            if ((MyVisualScriptLogicProvider.ConnectorStateChanged != null) && (this.m_other != null))
            {
                MyVisualScriptLogicProvider.ConnectorStateChanged(base.EntityId, base.CubeGrid.EntityId, base.Name, base.CubeGrid.Name, this.m_other.EntityId, this.m_other.CubeGrid.EntityId, this.m_other.Name, this.m_other.CubeGrid.Name, true);
            }
        }

        private void Weld(MatrixD? masterToSlave)
        {
            this.m_welding = true;
            this.m_welded = true;
            this.m_other.m_welded = true;
            if (masterToSlave == null)
            {
                masterToSlave = new MatrixD?(base.WorldMatrix * MatrixD.Invert(this.m_other.WorldMatrix));
            }
            if (this.m_constraint != null)
            {
                this.RemoveConstraint(this.m_other, this.m_constraint);
                this.m_constraint = null;
                this.m_other.m_constraint = null;
            }
            this.WeldInternal();
            if (Sync.IsServer)
            {
                State state = new State {
                    IsMaster = true,
                    OtherEntityId = this.m_other.EntityId,
                    MasterToSlave = new MyDeltaTransform?(masterToSlave.Value),
                    MasterToSlaveGrid = new MyDeltaTransform?(base.CubeGrid.WorldMatrix * MatrixD.Invert(this.m_other.WorldMatrix))
                };
                this.m_connectionState.Value = state;
                state = new State {
                    IsMaster = false,
                    OtherEntityId = base.EntityId,
                    MasterToSlave = new MyDeltaTransform?(masterToSlave.Value)
                };
                this.m_other.m_connectionState.Value = state;
            }
            this.m_other.m_other = this;
            this.m_welding = false;
        }

        private void WeldInternal()
        {
            if (this.m_attachableConveyorEndpoint.AlreadyAttached())
            {
                this.m_attachableConveyorEndpoint.DetachAll();
            }
            this.m_attachableConveyorEndpoint.Attach(this.m_other.m_attachableConveyorEndpoint);
            this.Connected = true;
            this.m_other.Connected = true;
            this.RecreateConstraintInternal();
            this.SetEmissiveStateWorking();
            this.m_other.SetEmissiveStateWorking();
            if (!ReferenceEquals(base.CubeGrid, this.m_other.CubeGrid))
            {
                this.OnConstraintAdded(GridLinkTypeEnum.Logical, this.m_other.CubeGrid);
                this.OnConstraintAdded(GridLinkTypeEnum.Physical, this.m_other.CubeGrid);
                MyFixedGrids.Link(base.CubeGrid, this.m_other.CubeGrid, this);
                MyGridPhysicalHierarchy.Static.CreateLink(base.EntityId, base.CubeGrid, this.m_other.CubeGrid);
            }
            base.CubeGrid.OnPhysicsChanged -= new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            base.CubeGrid.OnPhysicsChanged += new Action<VRage.Game.Entity.MyEntity>(this.CubeGrid_OnPhysicsChanged);
            base.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
            this.m_other.CubeGrid.GridSystems.ConveyorSystem.FlagForRecomputation();
        }

        public void WriteLockStateValue(StringBuilder sb)
        {
            if (this.InConstraint && this.Connected)
            {
                sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyValue_Locked));
            }
            else if (this.InConstraint)
            {
                sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyValue_ReadyToLock));
            }
            else
            {
                sb.AppendStringBuilder(MyTexts.Get(MySpaceTexts.BlockPropertyValue_Unlocked));
            }
        }

        private bool IsMaster
        {
            get => 
                (Sync.IsServer ? this.m_isMaster : this.m_connectionState.Value.IsMaster);
            set => 
                (this.m_isMaster = value);
        }

        public bool IsReleasing =>
            ((MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_manualDisconnectTime) < DisconnectSleepTime.Milliseconds);

        public bool InConstraint =>
            (this.m_constraint != null);

        public bool Connected { get; set; }

        private Vector3 ConnectionPosition =>
            Vector3.Transform(this.m_connectionPosition, base.PositionComp.LocalMatrix);

        public int DetectedGridCount =>
            this.m_detectedGrids.Count;

        IMyConveyorEndpoint IMyConveyorEndpointBlock.ConveyorEndpoint =>
            this.m_attachableConveyorEndpoint;

        bool Sandbox.ModAPI.Ingame.IMyShipConnector.ThrowOut
        {
            get => 
                ((bool) this.ThrowOut);
            set => 
                (this.ThrowOut.Value = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyShipConnector.CollectAll
        {
            get => 
                ((bool) this.CollectAll);
            set => 
                (this.CollectAll.Value = value);
        }

        float Sandbox.ModAPI.Ingame.IMyShipConnector.PullStrength
        {
            get => 
                ((float) this.Strength);
            set
            {
                if (this.m_connectorMode == Mode.Connector)
                {
                    float single1 = MathHelper.Clamp(value, 1E-06f, 1f);
                    value = single1;
                    this.Strength.Value = value;
                }
            }
        }

        bool Sandbox.ModAPI.Ingame.IMyShipConnector.IsLocked =>
            (base.IsWorking && this.InConstraint);

        bool Sandbox.ModAPI.Ingame.IMyShipConnector.IsConnected =>
            this.Connected;

        MyShipConnectorStatus Sandbox.ModAPI.Ingame.IMyShipConnector.Status
        {
            get
            {
                if (this.Connected)
                {
                    return MyShipConnectorStatus.Connected;
                }
                if (!base.IsWorking || !this.InConstraint)
                {
                    return MyShipConnectorStatus.Unconnected;
                }
                return MyShipConnectorStatus.Connectable;
            }
        }

        Sandbox.ModAPI.Ingame.IMyShipConnector Sandbox.ModAPI.Ingame.IMyShipConnector.OtherConnector =>
            this.m_other;

        Sandbox.ModAPI.IMyShipConnector Sandbox.ModAPI.IMyShipConnector.OtherConnector =>
            this.m_other;

        public bool UseConveyorSystem
        {
            get => 
                true;
            set
            {
            }
        }

        int IMyInventoryOwner.InventoryCount =>
            base.InventoryCount;

        long IMyInventoryOwner.EntityId =>
            base.EntityId;

        bool IMyInventoryOwner.HasInventory =>
            base.HasInventory;

        bool IMyInventoryOwner.UseConveyorSystem
        {
            get => 
                this.UseConveyorSystem;
            set
            {
                throw new NotSupportedException();
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyShipConnector.<>c <>9 = new MyShipConnector.<>c();
            public static MyTerminalValueControl<MyShipConnector, bool>.GetterDelegate <>9__54_0;
            public static MyTerminalValueControl<MyShipConnector, bool>.SetterDelegate <>9__54_1;
            public static MyTerminalValueControl<MyShipConnector, bool>.GetterDelegate <>9__54_2;
            public static MyTerminalValueControl<MyShipConnector, bool>.SetterDelegate <>9__54_3;
            public static Action<MyShipConnector> <>9__54_4;
            public static Func<MyShipConnector, bool> <>9__54_5;
            public static Func<MyShipConnector, bool> <>9__54_6;
            public static Func<MyShipConnector, bool> <>9__54_7;
            public static Action<MyShipConnector> <>9__54_8;
            public static Func<MyShipConnector, bool> <>9__54_9;
            public static Func<MyShipConnector, bool> <>9__54_10;
            public static Func<MyShipConnector, bool> <>9__54_11;
            public static Action<MyShipConnector> <>9__54_12;
            public static MyTerminalControl<MyShipConnector>.WriterDelegate <>9__54_13;
            public static Func<MyShipConnector, bool> <>9__54_14;
            public static MyTerminalValueControl<MyShipConnector, float>.GetterDelegate <>9__54_15;
            public static MyTerminalValueControl<MyShipConnector, float>.SetterDelegate <>9__54_16;
            public static Func<MyShipConnector, bool> <>9__54_17;
            public static Func<MyShipConnector, bool> <>9__54_18;
            public static Func<MyShipConnector, bool> <>9__54_19;
            public static MyTerminalValueControl<MyShipConnector, float>.GetterDelegate <>9__54_20;
            public static MyTerminalValueControl<MyShipConnector, float>.GetterDelegate <>9__54_21;
            public static MyTerminalControl<MyShipConnector>.WriterDelegate <>9__54_22;
            public static Func<MyShipConnector, Action> <>9__59_0;
            public static Func<MyShipConnector, Action> <>9__60_0;
            public static Func<MyShipConnector, Action> <>9__60_1;
            public static Func<MyShipConnector, Action> <>9__96_1;

            internal bool <CreateTerminalControls>b__54_0(MyShipConnector block) => 
                ((bool) block.ThrowOut);

            internal void <CreateTerminalControls>b__54_1(MyShipConnector block, bool value)
            {
                block.ThrowOut.Value = value;
            }

            internal bool <CreateTerminalControls>b__54_10(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal bool <CreateTerminalControls>b__54_11(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal void <CreateTerminalControls>b__54_12(MyShipConnector b)
            {
                b.TrySwitch();
            }

            internal void <CreateTerminalControls>b__54_13(MyShipConnector b, StringBuilder sb)
            {
                b.WriteLockStateValue(sb);
            }

            internal bool <CreateTerminalControls>b__54_14(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal float <CreateTerminalControls>b__54_15(MyShipConnector x) => 
                ((float) (x.Strength * 100f));

            internal void <CreateTerminalControls>b__54_16(MyShipConnector x, float v)
            {
                x.Strength.Value = v * 0.01f;
            }

            internal bool <CreateTerminalControls>b__54_17(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal bool <CreateTerminalControls>b__54_18(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal bool <CreateTerminalControls>b__54_19(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal bool <CreateTerminalControls>b__54_2(MyShipConnector block) => 
                ((bool) block.CollectAll);

            internal float <CreateTerminalControls>b__54_20(MyShipConnector x) => 
                0f;

            internal float <CreateTerminalControls>b__54_21(MyShipConnector x) => 
                100f;

            internal void <CreateTerminalControls>b__54_22(MyShipConnector x, StringBuilder result)
            {
                if (x.Strength <= 1E-06f)
                {
                    result.Append(MyTexts.Get(MyCommonTexts.Disabled));
                }
                else
                {
                    result.AppendFormatedDecimal("", (float) (x.Strength * 100f), 4, " %");
                }
            }

            internal void <CreateTerminalControls>b__54_3(MyShipConnector block, bool value)
            {
                block.CollectAll.Value = value;
            }

            internal void <CreateTerminalControls>b__54_4(MyShipConnector b)
            {
                b.TryConnect();
            }

            internal bool <CreateTerminalControls>b__54_5(MyShipConnector b) => 
                (b.IsWorking && b.InConstraint);

            internal bool <CreateTerminalControls>b__54_6(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal bool <CreateTerminalControls>b__54_7(MyShipConnector b) => 
                (b.m_connectorMode == MyShipConnector.Mode.Connector);

            internal void <CreateTerminalControls>b__54_8(MyShipConnector b)
            {
                b.TryDisconnect();
            }

            internal bool <CreateTerminalControls>b__54_9(MyShipConnector b) => 
                (b.IsWorking && b.InConstraint);

            internal Action <TryConnect>b__59_0(MyShipConnector x) => 
                new Action(x.TryConnect);

            internal Action <TryDisconnect>b__60_0(MyShipConnector x) => 
                new Action(x.NotifyDisconnectTime);

            internal Action <TryDisconnect>b__60_1(MyShipConnector x) => 
                new Action(x.TryDisconnect);

            internal Action <TryThrowOutItem>b__96_1(MyShipConnector x) => 
                new Action(x.PlayActionSound);
        }

        private enum Mode
        {
            Ejector,
            Connector
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct State
        {
            public static readonly MyShipConnector.State Detached;
            public static readonly MyShipConnector.State DetachedMaster;
            public bool IsMaster;
            public long OtherEntityId;
            public MyDeltaTransform? MasterToSlave;
            public MyDeltaTransform? MasterToSlaveGrid;
            static State()
            {
                Detached = new MyShipConnector.State();
                MyShipConnector.State state = new MyShipConnector.State {
                    IsMaster = true
                };
                DetachedMaster = state;
            }
        }
    }
}

