namespace Sandbox.Game.Entities
{
    using Havok;
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Multiplayer;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyCubeBlockType(typeof(MyObjectBuilder_Door)), MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyDoor), typeof(Sandbox.ModAPI.Ingame.IMyDoor) })]
    public class MyDoor : MyDoorBase, Sandbox.ModAPI.IMyDoor, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDoor
    {
        private const float CLOSED_DISSASEMBLE_RATIO = 3.3f;
        private static readonly float EPSILON = 1E-09f;
        private MySoundPair m_openSound;
        private MySoundPair m_closeSound;
        private float m_currOpening = 0f;
        private float m_currSpeed = 0f;
        private float m_openingSpeed;
        private int m_lastUpdateTime;
        private bool m_physicsInitiated;
        private MyEntitySubpart m_leftSubpart;
        private MyEntitySubpart m_rightSubpart;
        private HkFixedConstraintData m_leftConstraintData;
        private HkConstraint m_leftConstraint;
        private HkFixedConstraintData m_rightConstraintData;
        private HkConstraint m_rightConstraint;
        public float MaxOpen = 1.2f;
        [CompilerGenerated]
        private Action<bool> DoorStateChanged;
        [CompilerGenerated]
        private Action<Sandbox.ModAPI.IMyDoor, bool> OnDoorStateChanged;

        public event Action<bool> DoorStateChanged
        {
            [CompilerGenerated] add
            {
                Action<bool> doorStateChanged = this.DoorStateChanged;
                while (true)
                {
                    Action<bool> a = doorStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Combine(a, value);
                    doorStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.DoorStateChanged, action3, a);
                    if (ReferenceEquals(doorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<bool> doorStateChanged = this.DoorStateChanged;
                while (true)
                {
                    Action<bool> source = doorStateChanged;
                    Action<bool> action3 = (Action<bool>) Delegate.Remove(source, value);
                    doorStateChanged = Interlocked.CompareExchange<Action<bool>>(ref this.DoorStateChanged, action3, source);
                    if (ReferenceEquals(doorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<Sandbox.ModAPI.IMyDoor, bool> OnDoorStateChanged
        {
            [CompilerGenerated] add
            {
                Action<Sandbox.ModAPI.IMyDoor, bool> onDoorStateChanged = this.OnDoorStateChanged;
                while (true)
                {
                    Action<Sandbox.ModAPI.IMyDoor, bool> a = onDoorStateChanged;
                    Action<Sandbox.ModAPI.IMyDoor, bool> action3 = (Action<Sandbox.ModAPI.IMyDoor, bool>) Delegate.Combine(a, value);
                    onDoorStateChanged = Interlocked.CompareExchange<Action<Sandbox.ModAPI.IMyDoor, bool>>(ref this.OnDoorStateChanged, action3, a);
                    if (ReferenceEquals(onDoorStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<Sandbox.ModAPI.IMyDoor, bool> onDoorStateChanged = this.OnDoorStateChanged;
                while (true)
                {
                    Action<Sandbox.ModAPI.IMyDoor, bool> source = onDoorStateChanged;
                    Action<Sandbox.ModAPI.IMyDoor, bool> action3 = (Action<Sandbox.ModAPI.IMyDoor, bool>) Delegate.Remove(source, value);
                    onDoorStateChanged = Interlocked.CompareExchange<Action<Sandbox.ModAPI.IMyDoor, bool>>(ref this.OnDoorStateChanged, action3, source);
                    if (ReferenceEquals(onDoorStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyDoor()
        {
            base.m_open.AlwaysReject<bool, SyncDirection.BothWays>();
            base.m_open.ValueChanged += x => this.OnStateChange();
        }

        protected override void BeforeDelete()
        {
            base.DisposeSubpartConstraint(ref this.m_leftConstraint, ref this.m_leftConstraintData);
            base.DisposeSubpartConstraint(ref this.m_rightConstraint, ref this.m_rightConstraintData);
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.BeforeDelete();
        }

        protected override bool CheckIsWorking() => 
            (base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId) && base.CheckIsWorking());

        protected override void Closing()
        {
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (base.m_soundEmitter != null)
            {
                base.m_soundEmitter.StopSound(true, true);
            }
            base.Closing();
        }

        private void ComponentStack_IsFunctionalChanged()
        {
            base.ResourceSink.Update();
        }

        private void CreateConstraint(MyEntitySubpart subpart, ref HkConstraint constraint, ref HkFixedConstraintData constraintData)
        {
            if (subpart != null)
            {
                bool flag = !Sync.IsServer;
                if (subpart.Physics == null)
                {
                    HkShape[] havokCollisionShapes = subpart.ModelCollision.HavokCollisionShapes;
                    if ((havokCollisionShapes != null) && (havokCollisionShapes.Length != 0))
                    {
                        MyPhysicsBody body = new MyPhysicsBody(subpart, flag ? RigidBodyFlag.RBF_STATIC : (RigidBodyFlag.RBF_UNLOCKED_SPEEDS | RigidBodyFlag.RBF_DOUBLED_KINEMATIC)) {
                            IsSubpart = true
                        };
                        subpart.Physics = body;
                        HkShape shape = havokCollisionShapes[0];
                        MyPositionComponentBase positionComp = subpart.PositionComp;
                        Vector3 center = positionComp.LocalVolume.Center;
                        HkMassProperties properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(positionComp.LocalAABB.HalfExtents, 100f);
                        int collisionFilter = base.CubeGrid.IsStatic ? 9 : 0x10;
                        body.CreateFromCollisionObject(shape, center, positionComp.WorldMatrix, new HkMassProperties?(properties), collisionFilter);
                    }
                }
                if (flag)
                {
                    subpart.Physics.Enabled = true;
                }
                else
                {
                    base.CreateSubpartConstraint(subpart, out constraintData, out constraint);
                    base.CubeGrid.Physics.AddConstraint(constraint);
                    constraint.SetVirtualMassInverse(Vector4.Zero, Vector4.One);
                }
            }
        }

        private void CreateConstraints()
        {
            this.UpdateSlidingDoorsPosition();
            this.CreateConstraint(this.m_leftSubpart, ref this.m_leftConstraint, ref this.m_leftConstraintData);
            this.CreateConstraint(this.m_rightSubpart, ref this.m_rightConstraint, ref this.m_rightConstraintData);
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
        }

        private void CubeGrid_OnHavokSystemIDChanged(int id)
        {
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, true);
            }
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_Door objectBuilderCubeBlock = (MyObjectBuilder_Door) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.State = base.Open;
            objectBuilderCubeBlock.Opening = this.m_currOpening;
            objectBuilderCubeBlock.OpenSound = this.m_openSound.ToString();
            objectBuilderCubeBlock.CloseSound = this.m_closeSound.ToString();
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            MyStringHash orCompute;
            this.m_physicsInitiated = false;
            MyDoorDefinition blockDefinition = base.BlockDefinition as MyDoorDefinition;
            if (blockDefinition != null)
            {
                this.MaxOpen = blockDefinition.MaxOpen;
                this.m_openSound = new MySoundPair(blockDefinition.OpenSound, true);
                this.m_closeSound = new MySoundPair(blockDefinition.CloseSound, true);
                orCompute = MyStringHash.GetOrCompute(blockDefinition.ResourceSinkGroup);
                this.m_openingSpeed = blockDefinition.OpeningSpeed;
            }
            else
            {
                this.MaxOpen = 1.2f;
                this.m_openSound = new MySoundPair("BlockDoorSmallOpen", true);
                this.m_closeSound = new MySoundPair("BlockDoorSmallClose", true);
                orCompute = MyStringHash.GetOrCompute("Doors");
                this.m_openingSpeed = 1f;
            }
            MyResourceSinkComponent sinkComp = new MyResourceSinkComponent(1);
            sinkComp.Init(orCompute, 3E-05f, delegate {
                if (!this.Enabled || !this.IsFunctional)
                {
                    return 0f;
                }
                return sinkComp.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            });
            sinkComp.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink = sinkComp;
            base.Init(builder, cubeGrid);
            base.NeedsWorldMatrix = false;
            MyObjectBuilder_Door door = (MyObjectBuilder_Door) builder;
            base.m_open.SetLocalValue(door.State);
            if (door.Opening != -1f)
            {
                this.m_currOpening = MathHelper.Clamp(door.Opening, 0f, this.MaxOpen);
            }
            else
            {
                this.m_currOpening = base.IsFunctional ? 0f : this.MaxOpen;
                base.m_open.SetLocalValue(!base.IsFunctional);
            }
            if (!base.Enabled || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.UpdateSlidingDoorsPosition();
            }
            this.OnStateChange();
            if (((base.m_open != null) && base.Open) && (this.m_currOpening == this.MaxOpen))
            {
                this.UpdateSlidingDoorsPosition();
            }
            sinkComp.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            if (!Sync.IsServer)
            {
                base.NeedsWorldMatrix = true;
            }
        }

        private void InitSubparts()
        {
            Matrix? nullable;
            base.DisposeSubpartConstraint(ref this.m_leftConstraint, ref this.m_leftConstraintData);
            base.DisposeSubpartConstraint(ref this.m_rightConstraint, ref this.m_rightConstraintData);
            base.Subparts.TryGetValue("DoorLeft", out this.m_leftSubpart);
            base.Subparts.TryGetValue("DoorRight", out this.m_rightSubpart);
            MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
            if (this.m_leftSubpart != null)
            {
                nullable = null;
                this.m_leftSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                this.m_leftSubpart.NeedsWorldMatrix = false;
                this.m_leftSubpart.InvalidateOnMove = false;
            }
            if (this.m_rightSubpart != null)
            {
                nullable = null;
                this.m_rightSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                this.m_rightSubpart.NeedsWorldMatrix = false;
                this.m_rightSubpart.InvalidateOnMove = false;
            }
            if (base.CubeGrid.Projector != null)
            {
                this.UpdateSlidingDoorsPosition();
            }
            else if (!base.CubeGrid.CreatePhysics)
            {
                this.UpdateSlidingDoorsPosition();
            }
            else
            {
                if ((this.m_leftSubpart != null) && (this.m_leftSubpart.Physics != null))
                {
                    this.m_leftSubpart.Physics.Close();
                    this.m_leftSubpart.Physics = null;
                }
                if ((this.m_rightSubpart != null) && (this.m_rightSubpart.Physics != null))
                {
                    this.m_rightSubpart.Physics.Close();
                    this.m_rightSubpart.Physics = null;
                }
                this.CreateConstraints();
                this.m_physicsInitiated = true;
                this.UpdateSlidingDoorsPosition();
            }
        }

        public override void OnAddedToScene(object source)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            base.OnAddedToScene(source);
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            base.OnBuildSuccess(builtBy, instantBuild);
        }

        public override void OnCubeGridChanged(MyCubeGrid oldGrid)
        {
            oldGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, true);
            }
            if (base.InScene)
            {
                Matrix? nullable;
                MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
                if (this.m_leftSubpart != null)
                {
                    nullable = null;
                    this.m_leftSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                }
                if (this.m_rightSubpart != null)
                {
                    nullable = null;
                    this.m_rightSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                }
            }
            base.OnCubeGridChanged(oldGrid);
        }

        protected override void OnEnabledChanged()
        {
            base.ResourceSink.Update();
            base.OnEnabledChanged();
        }

        public override void OnModelChange()
        {
            base.OnModelChange();
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void OnRemovedFromScene(object source)
        {
            base.DisposeSubpartConstraint(ref this.m_leftConstraint, ref this.m_leftConstraintData);
            base.DisposeSubpartConstraint(ref this.m_rightConstraint, ref this.m_rightConstraintData);
            base.OnRemovedFromScene(source);
        }

        private void OnStateChange()
        {
            if ((this.m_leftSubpart != null) || (this.m_rightSubpart != null))
            {
                this.m_currSpeed = (base.m_open != null) ? this.m_openingSpeed : -this.m_openingSpeed;
                base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
                this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
                this.UpdateCurrentOpening();
                this.UpdateSlidingDoorsPosition();
                if (base.m_open != null)
                {
                    this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                    this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
                }
            }
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
        }

        private void RecreateConstraints(VRage.Game.Entity.MyEntity obj, bool refreshInPlace)
        {
            if ((((((obj != null) && !obj.MarkedForClose) && (obj.GetPhysicsBody() != null)) && (!obj.IsPreview && (base.CubeGrid.Projector == null))) && ((this.m_leftSubpart == null) || (!this.m_leftSubpart.MarkedForClose && !this.m_leftSubpart.Closed))) && ((this.m_rightSubpart == null) || (!this.m_rightSubpart.MarkedForClose && !this.m_rightSubpart.Closed)))
            {
                Matrix? nullable;
                MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
                if (this.m_leftSubpart != null)
                {
                    nullable = null;
                    this.m_leftSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                }
                if (this.m_rightSubpart != null)
                {
                    nullable = null;
                    this.m_rightSubpart.Render.SetParent(0, orAddCell.ParentCullObject, nullable);
                }
                base.DisposeSubpartConstraint(ref this.m_leftConstraint, ref this.m_leftConstraintData);
                base.DisposeSubpartConstraint(ref this.m_rightConstraint, ref this.m_rightConstraintData);
                if ((base.InScene && (base.CubeGrid.Physics != null)) && (base.CubeGrid.Physics.IsInWorld || ((MyPhysicsBody) base.CubeGrid.Physics).IsInWorldWelded()))
                {
                    this.CreateConstraints();
                }
                if (obj.Physics != null)
                {
                    this.UpdateHavokCollisionSystemID(obj.GetPhysicsBody().HavokCollisionSystemID, refreshInPlace);
                }
                this.UpdateSlidingDoorsPosition();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.CloseDoor()
        {
            if (base.IsWorking && ((((Sandbox.ModAPI.Ingame.IMyDoor) this).Status - 2) > DoorStatus.Open))
            {
                ((Sandbox.ModAPI.Ingame.IMyDoor) this).ToggleDoor();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.OpenDoor()
        {
            if (base.IsWorking && (((Sandbox.ModAPI.Ingame.IMyDoor) this).Status > DoorStatus.Open))
            {
                ((Sandbox.ModAPI.Ingame.IMyDoor) this).ToggleDoor();
            }
        }

        void Sandbox.ModAPI.Ingame.IMyDoor.ToggleDoor()
        {
            if (base.IsWorking)
            {
                base.SetOpenRequest(!base.Open, base.OwnerId);
            }
        }

        private void StartSound(MySoundPair cuePair)
        {
            if ((base.m_soundEmitter != null) && (((base.m_soundEmitter.Sound == null) || !base.m_soundEmitter.Sound.IsPlaying) || ((base.m_soundEmitter.SoundId != cuePair.Arcade) && (base.m_soundEmitter.SoundId != cuePair.Realistic))))
            {
                base.m_soundEmitter.StopSound(true, true);
                bool? nullable = null;
                base.m_soundEmitter.PlaySingleSound(cuePair, true, false, false, nullable);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if ((base.CubeGrid.Physics != null) && (((this.m_currOpening != 0f) && (this.m_currOpening <= this.MaxOpen)) || (this.m_currSpeed != 0f)))
            {
                this.UpdateSlidingDoorsPosition();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if ((base.Open && (this.m_currOpening == this.MaxOpen)) || (!base.Open && (this.m_currOpening == 0f)))
            {
                if (((base.m_soundEmitter != null) && (base.m_soundEmitter.IsPlaying && base.m_soundEmitter.Loop)) && ((base.BlockDefinition.DamagedSound == null) || (base.m_soundEmitter.SoundId != base.BlockDefinition.DamagedSound.SoundId)))
                {
                    base.m_soundEmitter.StopSound(false, true);
                }
                if (this.m_physicsInitiated && !base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
                this.m_currSpeed = 0f;
                if (base.m_open == null)
                {
                    this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                    this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
                }
            }
            else
            {
                if (((base.m_soundEmitter != null) && base.Enabled) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
                {
                    if (base.Open)
                    {
                        this.StartSound(this.m_openSound);
                    }
                    else
                    {
                        this.StartSound(this.m_closeSound);
                    }
                }
                base.UpdateBeforeSimulation();
                this.UpdateCurrentOpening();
                this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
        }

        private void UpdateCurrentOpening()
        {
            if (((this.m_currSpeed != 0f) && base.Enabled) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                float num = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime)) / 1000f;
                float num2 = this.m_currSpeed * num;
                this.m_currOpening = MathHelper.Clamp(this.m_currOpening + num2, 0f, this.MaxOpen);
            }
        }

        internal void UpdateHavokCollisionSystemID(int havokCollisionSystemID, bool refreshInPlace)
        {
            MyEntitySubpart[] subpartArray1 = new MyEntitySubpart[] { this.m_rightSubpart, this.m_leftSubpart };
            foreach (MyEntitySubpart subpart in subpartArray1)
            {
                if (subpart != null)
                {
                    SetupDoorSubpart(subpart, havokCollisionSystemID, refreshInPlace);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.InitSubparts();
            this.RecreateConstraints(base.CubeGrid, false);
        }

        private unsafe void UpdateSlidingDoorsPosition()
        {
            if (base.CubeGrid.Physics != null)
            {
                Vector3 vector;
                bool flag = !Sync.IsServer;
                float x = this.m_currOpening * 0.65f;
                if (this.m_leftSubpart != null)
                {
                    Matrix matrix;
                    vector = new Vector3(-x, 0f, 0f);
                    Matrix.CreateTranslation(ref vector, out matrix);
                    Matrix renderLocal = matrix * base.PositionComp.LocalMatrix;
                    Matrix identity = Matrix.Identity;
                    identity.Translation = new Vector3(0.35f, 0f, 0f);
                    Matrix* matrixPtr1 = (Matrix*) ref identity;
                    Matrix.Multiply(ref (Matrix) ref matrixPtr1, ref matrix, out identity);
                    this.m_leftSubpart.PositionComp.SetLocalMatrix(ref identity, flag ? null : this.m_leftSubpart.Physics, true, ref renderLocal, true);
                    if (this.m_leftConstraintData != null)
                    {
                        Matrix matrix4;
                        if (base.CubeGrid.Physics != null)
                        {
                            base.CubeGrid.Physics.RigidBody.Activate();
                        }
                        this.m_leftSubpart.Physics.RigidBody.Activate();
                        vector = new Vector3(x, 0f, 0f);
                        Matrix.CreateTranslation(ref vector, out matrix4);
                        this.m_leftConstraintData.SetInBodySpace(base.PositionComp.LocalMatrix, matrix4, base.CubeGrid.Physics, (MyPhysicsBody) this.m_leftSubpart.Physics);
                    }
                }
                if (this.m_rightSubpart != null)
                {
                    Matrix matrix5;
                    vector = new Vector3(x, 0f, 0f);
                    Matrix.CreateTranslation(ref vector, out matrix5);
                    Matrix renderLocal = matrix5 * base.PositionComp.LocalMatrix;
                    Matrix identity = Matrix.Identity;
                    identity.Translation = new Vector3(-0.35f, 0f, 0f);
                    Matrix* matrixPtr2 = (Matrix*) ref identity;
                    Matrix.Multiply(ref (Matrix) ref matrixPtr2, ref matrix5, out identity);
                    this.m_rightSubpart.PositionComp.SetLocalMatrix(ref identity, flag ? null : this.m_rightSubpart.Physics, true, ref renderLocal, true);
                    if (this.m_rightConstraintData != null)
                    {
                        if (base.CubeGrid.Physics != null)
                        {
                            base.CubeGrid.Physics.RigidBody.Activate();
                        }
                        this.m_rightSubpart.Physics.RigidBody.Activate();
                        Matrix pivotB = Matrix.CreateTranslation(new Vector3(-x, 0f, 0f));
                        this.m_rightConstraintData.SetInBodySpace(base.PositionComp.LocalMatrix, pivotB, base.CubeGrid.Physics, (MyPhysicsBody) this.m_rightSubpart.Physics);
                    }
                }
            }
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            this.UpdateSlidingDoorsPosition();
        }

        public override float DisassembleRatio =>
            (base.DisassembleRatio * 3.3f);

        DoorStatus Sandbox.ModAPI.Ingame.IMyDoor.Status =>
            ((base.m_open == null) ? ((this.m_currOpening < EPSILON) ? DoorStatus.Closed : DoorStatus.Closing) : (((this.MaxOpen - this.m_currOpening) < EPSILON) ? DoorStatus.Open : DoorStatus.Opening));

        public float OpenRatio =>
            (this.m_currOpening / this.MaxOpen);

        bool Sandbox.ModAPI.IMyDoor.IsFullyClosed =>
            (this.m_currOpening < EPSILON);
    }
}

