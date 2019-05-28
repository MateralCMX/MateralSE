namespace Sandbox.Game.Entities.Blocks
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
    using Sandbox.Game.Entities.Cube;
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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_Collector)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyCollector), typeof(Sandbox.ModAPI.Ingame.IMyCollector) })]
    public class MyCollector : MyFunctionalBlock, IMyConveyorEndpointBlock, Sandbox.ModAPI.IMyCollector, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyCollector, IMyInventoryOwner
    {
        private HkConstraint m_phantomConstraint;
        private VRage.Sync.Sync<bool, SyncDirection.BothWays> m_useConveyorSystem;
        private MyMultilineConveyorEndpoint m_multilineConveyorEndpoint;
        private bool m_isCollecting;
        private readonly MyConcurrentHashSet<MyFloatingObject> m_entitiesToTake = new MyConcurrentHashSet<MyFloatingObject>();

        public MyCollector()
        {
            this.CreateTerminalControls();
        }

        public bool AllowSelfPulling() => 
            false;

        protected override bool CheckIsWorking() => 
            ((base.ResourceSink != null) ? (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking()) : false);

        protected float ComputeRequiredPower()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return this.BlockDefinition.RequiredPowerInput;
        }

        private HkBvShape CreateFieldShape(Vector3 extents)
        {
            HkPhantomCallbackShape shape = new HkPhantomCallbackShape(new HkPhantomHandler(this.phantom_Enter), new HkPhantomHandler(this.phantom_Leave));
            return new HkBvShape((HkShape) new HkBoxShape(extents), (HkShape) shape, HkReferencePolicy.TakeOwnership);
        }

        private void CreatePhantomConstraint()
        {
            if (this.m_phantomConstraint != null)
            {
                this.DisposePhantomContraint(null);
            }
            MyGridPhysics bodyA = base.CubeGrid.Physics;
            MyPhysicsBody physics = base.Physics;
            if (((bodyA != null) && (physics != null)) && physics.Enabled)
            {
                HkFixedConstraintData data = new HkFixedConstraintData();
                data.SetInBodySpace(base.PositionComp.LocalMatrix, Matrix.CreateTranslation(-physics.Center), bodyA, physics);
                this.m_phantomConstraint = new HkConstraint(bodyA.RigidBody, physics.RigidBody, data);
                bodyA.AddConstraint(this.m_phantomConstraint);
            }
        }

        protected override void CreateTerminalControls()
        {
            if (!MyTerminalControlFactory.AreControlsCreated<MyCollector>())
            {
                base.CreateTerminalControls();
                MyStringId tooltip = new MyStringId();
                MyStringId? on = null;
                on = null;
                MyTerminalControlOnOffSwitch<MyCollector> switch1 = new MyTerminalControlOnOffSwitch<MyCollector>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                MyTerminalControlOnOffSwitch<MyCollector> switch2 = new MyTerminalControlOnOffSwitch<MyCollector>("UseConveyor", MySpaceTexts.Terminal_UseConveyorSystem, tooltip, on, on);
                switch2.Getter = x => x.UseConveyorSystem;
                MyTerminalControlOnOffSwitch<MyCollector> local4 = switch2;
                MyTerminalControlOnOffSwitch<MyCollector> local5 = switch2;
                local5.Setter = (x, v) => x.UseConveyorSystem = v;
                MyTerminalControlOnOffSwitch<MyCollector> onOff = local5;
                onOff.EnableToggleAction<MyCollector>();
                MyTerminalControlFactory.AddControl<MyCollector>(onOff);
            }
        }

        private void DisposePhantomContraint(MyCubeGrid oldGrid = null)
        {
            if (this.m_phantomConstraint != null)
            {
                if (oldGrid == null)
                {
                    oldGrid = base.CubeGrid;
                }
                oldGrid.Physics.RemoveConstraint(this.m_phantomConstraint);
                this.m_phantomConstraint.Dispose();
                this.m_phantomConstraint = null;
            }
        }

        private void GetBoxFromMatrix(Matrix m, out Vector3 halfExtents, out Vector3 position, out Quaternion orientation)
        {
            MatrixD matrix = Matrix.Normalize(m) * base.WorldMatrix;
            orientation = Quaternion.CreateFromRotationMatrix(matrix);
            halfExtents = Vector3.Abs(m.Scale) / 2f;
            halfExtents = new Vector3(halfExtents.X, halfExtents.Y, halfExtents.Z);
            position = (Vector3) matrix.Translation;
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Collector objectBuilderCubeBlock = base.GetObjectBuilderCubeBlock(copy) as MyObjectBuilder_Collector;
            objectBuilderCubeBlock.Inventory = this.GetInventory(0).GetObjectBuilder();
            objectBuilderCubeBlock.UseConveyorSystem = (bool) this.m_useConveyorSystem;
            return objectBuilderCubeBlock;
        }

        public PullInformation GetPullInformation() => 
            null;

        public PullInformation GetPushInformation()
        {
            PullInformation information1 = new PullInformation();
            information1.Inventory = this.GetInventory(0);
            information1.OwnerID = base.OwnerId;
            information1.Constraint = new MyInventoryConstraint("Empty constraint", null, true);
            return information1;
        }

        public override void Init(MyObjectBuilder_CubeBlock objectBuilder, MyCubeGrid cubeGrid)
        {
            MyObjectBuilder_Collector collector = objectBuilder as MyObjectBuilder_Collector;
            MyResourceSinkComponent component = new MyResourceSinkComponent(1);
            component.Init(MyStringHash.GetOrCompute(this.BlockDefinition.ResourceSinkGroup), this.BlockDefinition.RequiredPowerInput, new Func<float>(this.ComputeRequiredPower));
            base.ResourceSink = component;
            base.Init(objectBuilder, cubeGrid);
            if (MyFakes.ENABLE_INVENTORY_FIX)
            {
                base.FixSingleInventory();
            }
            if (this.GetInventory(0) == null)
            {
                MyInventory inventory = new MyInventory(this.BlockDefinition.InventorySize.Volume, this.BlockDefinition.InventorySize, MyInventoryFlags.CanSend);
                base.Components.Add<MyInventoryBase>(inventory);
                inventory.Init(collector.Inventory);
            }
            if (Sync.IsServer && base.CubeGrid.CreatePhysics)
            {
                this.LoadDummies();
            }
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.UpdateReceiver);
            base.EnabledChanged += new Action<MyTerminalBlock>(this.UpdateReceiver);
            this.m_useConveyorSystem.SetLocalValue(collector.UseConveyorSystem);
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public void InitializeConveyorEndpoint()
        {
            this.m_multilineConveyorEndpoint = new MyMultilineConveyorEndpoint(this);
            base.AddDebugRenderComponent(new MyDebugRenderComponentDrawConveyorEndpoint(this.m_multilineConveyorEndpoint));
        }

        private void Inventory_ContentChangedCallback(MyInventoryBase inventory)
        {
            if (Sync.IsServer)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        private void LoadDummies()
        {
            foreach (KeyValuePair<string, MyModelDummy> pair in MyModels.GetModelOnlyDummies(this.BlockDefinition.Model).Dummies)
            {
                if (pair.Key.ToLower().Contains("collector"))
                {
                    Vector3 vector;
                    Vector3 vector2;
                    Quaternion quaternion;
                    Matrix m = pair.Value.Matrix;
                    this.GetBoxFromMatrix(m, out vector, out vector2, out quaternion);
                    HkBvShape shape = this.CreateFieldShape(vector);
                    base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_UNLOCKED_SPEEDS);
                    base.Physics.IsPhantom = true;
                    HkMassProperties? massProperties = null;
                    base.Physics.CreateFromCollisionObject((HkShape) shape, m.Translation, base.WorldMatrix, massProperties, 0x1a);
                    base.Physics.Enabled = true;
                    base.Physics.RigidBody.ContactPointCallbackEnabled = false;
                    shape.Base.RemoveReference();
                    break;
                }
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.OnAddedToScene(source);
            if ((this.m_phantomConstraint == null) && this.ShouldHavePhantom)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            base.OnCubeGridChanged(oldGrid);
            this.DisposePhantomContraint(oldGrid);
            if (this.ShouldHavePhantom)
            {
                base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            }
        }

        public override void OnDestroy()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnDestroy();
        }

        protected override void OnInventoryComponentAdded(MyInventoryBase inventory)
        {
            base.OnInventoryComponentAdded(inventory);
            if (this.GetInventory(0) != null)
            {
                this.GetInventory(0).ContentsChanged += new Action<MyInventoryBase>(this.Inventory_ContentChangedCallback);
            }
        }

        protected override void OnInventoryComponentRemoved(MyInventoryBase inventory)
        {
            base.OnInventoryComponentRemoved(inventory);
            MyInventory inventory2 = inventory as MyInventory;
            if (inventory2 != null)
            {
                inventory2.ContentsChanged -= new Action<MyInventoryBase>(this.Inventory_ContentChangedCallback);
            }
        }

        public override void OnRemovedByCubeBuilder()
        {
            base.ReleaseInventory(this.GetInventory(0), false);
            base.OnRemovedByCubeBuilder();
        }

        public override void OnRemovedFromScene(object source)
        {
            this.DisposePhantomContraint(null);
            base.OnRemovedFromScene(source);
        }

        protected override void OnStartWorking()
        {
            base.OnStartWorking();
            if (this.ShouldHavePhantom)
            {
                MyPhysicsBody physics = base.Physics;
                if ((physics != null) && !physics.Enabled)
                {
                    physics.Enabled = true;
                    this.CreatePhantomConstraint();
                }
            }
        }

        protected override void OnStopWorking()
        {
            base.OnStopWorking();
            if (base.Physics != null)
            {
                this.DisposePhantomContraint(null);
                base.Physics.Enabled = false;
            }
        }

        private void phantom_Enter(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            if (Sync.IsServer)
            {
                List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
                foreach (VRage.ModAPI.IMyEntity entity in allEntities)
                {
                    if (entity is MyFloatingObject)
                    {
                        this.m_entitiesToTake.Add(entity as MyFloatingObject);
                        MySandboxGame.Static.Invoke(() => base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME, "MyCollector::NeedsUpdate");
                    }
                }
                allEntities.Clear();
            }
        }

        private void phantom_Leave(HkPhantomCallbackShape shape, HkRigidBody body)
        {
            if (Sync.IsServer && !this.m_isCollecting)
            {
                List<VRage.ModAPI.IMyEntity> allEntities = body.GetAllEntities();
                foreach (VRage.ModAPI.IMyEntity entity in allEntities)
                {
                    this.m_entitiesToTake.Remove(entity as MyFloatingObject);
                }
                allEntities.Clear();
            }
        }

        [Event(null, 0xda), Reliable, Broadcast]
        private void PlayActionSoundAndParticle(Vector3D position)
        {
            MyParticleEffect effect;
            MyParticlesManager.TryCreateParticleEffect("Smoke_Collector", MatrixD.CreateWorld(position, base.WorldMatrix.Down, base.WorldMatrix.Forward), out effect);
            if (base.m_soundEmitter != null)
            {
                bool? nullable = null;
                base.m_soundEmitter.PlaySound(base.m_actionSound, false, false, false, false, false, nullable);
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        private void RigidBody_ContactPointCallback(ref HkContactPointEvent value)
        {
            if (Sync.IsServer)
            {
                VRage.ModAPI.IMyEntity otherEntity = value.GetOtherEntity(this);
                if (otherEntity is MyFloatingObject)
                {
                    this.m_entitiesToTake.Add(otherEntity as MyFloatingObject);
                    MySandboxGame.Static.Invoke(() => base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME, "MyCollector::NeedsUpdate");
                }
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if ((Sync.IsServer && (base.IsWorking && (this.m_useConveyorSystem != null))) && (this.GetInventory(0).GetItems().Count > 0))
            {
                MyGridConveyorSystem.PushAnyRequest(this, this.GetInventory(0), base.OwnerId);
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            if (!Sync.IsServer)
            {
                if (this.m_entitiesToTake.Count > 0)
                {
                    this.m_entitiesToTake.Clear();
                }
            }
            else
            {
                if ((this.m_phantomConstraint == null) && this.ShouldHavePhantom)
                {
                    this.CreatePhantomConstraint();
                }
                if (base.Enabled && base.IsWorking)
                {
                    bool flag = false;
                    this.m_isCollecting = true;
                    foreach (MyFloatingObject obj2 in this.m_entitiesToTake)
                    {
                        this.GetInventory(0).TakeFloatingObject(obj2);
                        flag = true;
                    }
                    this.m_isCollecting = false;
                    if (flag)
                    {
                        MyParticleEffect effect;
                        Vector3D position = this.m_entitiesToTake.ElementAt<MyFloatingObject>(0).PositionComp.GetPosition();
                        MyParticlesManager.TryCreateParticleEffect("Smoke_Collector", MatrixD.CreateWorld(position, base.WorldMatrix.Down, base.WorldMatrix.Forward), out effect);
                        if (base.m_soundEmitter != null)
                        {
                            bool? nullable = null;
                            base.m_soundEmitter.PlaySound(base.m_actionSound, false, false, false, false, false, nullable);
                        }
                        EndpointId targetEndpoint = new EndpointId();
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyCollector, Vector3D>(this, x => new Action<Vector3D>(x.PlayActionSoundAndParticle), position, targetEndpoint);
                    }
                }
            }
        }

        private void UpdateReceiver()
        {
            base.ResourceSink.Update();
        }

        private void UpdateReceiver(MyTerminalBlock block)
        {
            base.ResourceSink.Update();
        }

        VRage.Game.ModAPI.Ingame.IMyInventory IMyInventoryOwner.GetInventory(int index) => 
            this.GetInventory(index);

        public MyPoweredCargoContainerDefinition BlockDefinition =>
            (base.SlimBlock.BlockDefinition as MyPoweredCargoContainerDefinition);

        private bool ShouldHavePhantom =>
            (base.CubeGrid.CreatePhysics && (!base.CubeGrid.IsPreview && Sync.IsServer));

        public IMyConveyorEndpoint ConveyorEndpoint =>
            this.m_multilineConveyorEndpoint;

        private bool UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
        }

        bool Sandbox.ModAPI.Ingame.IMyCollector.UseConveyorSystem
        {
            get => 
                ((bool) this.m_useConveyorSystem);
            set => 
                (this.m_useConveyorSystem.Value = value);
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
            set => 
                (this.UseConveyorSystem = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCollector.<>c <>9 = new MyCollector.<>c();
            public static MyTerminalValueControl<MyCollector, bool>.GetterDelegate <>9__6_0;
            public static MyTerminalValueControl<MyCollector, bool>.SetterDelegate <>9__6_1;
            public static Func<MyCollector, Action<Vector3D>> <>9__20_0;

            internal bool <CreateTerminalControls>b__6_0(MyCollector x) => 
                x.UseConveyorSystem;

            internal void <CreateTerminalControls>b__6_1(MyCollector x, bool v)
            {
                x.UseConveyorSystem = v;
            }

            internal Action<Vector3D> <UpdateOnceBeforeFrame>b__20_0(MyCollector x) => 
                new Action<Vector3D>(x.PlayActionSoundAndParticle);
        }
    }
}

