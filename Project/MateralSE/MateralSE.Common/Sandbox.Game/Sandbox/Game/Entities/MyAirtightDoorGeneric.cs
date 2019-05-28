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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Sync;
    using VRage.Utils;
    using VRageMath;

    [MyTerminalInterface(new System.Type[] { typeof(Sandbox.ModAPI.IMyAirtightDoorBase), typeof(Sandbox.ModAPI.Ingame.IMyAirtightDoorBase) })]
    public abstract class MyAirtightDoorGeneric : MyDoorBase, Sandbox.ModAPI.IMyAirtightDoorBase, Sandbox.ModAPI.IMyDoor, Sandbox.ModAPI.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyFunctionalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock, VRage.Game.ModAPI.Ingame.IMyCubeBlock, VRage.Game.ModAPI.Ingame.IMyEntity, Sandbox.ModAPI.IMyTerminalBlock, VRage.Game.ModAPI.IMyCubeBlock, VRage.ModAPI.IMyEntity, Sandbox.ModAPI.Ingame.IMyDoor, Sandbox.ModAPI.Ingame.IMyAirtightDoorBase
    {
        private MySoundPair m_sound;
        private MySoundPair m_openSound;
        private MySoundPair m_closeSound;
        protected float m_currOpening = 0f;
        protected float m_subpartMovementDistance = 2.5f;
        protected float m_openingSpeed = 0.3f;
        protected float m_currSpeed = 0f;
        private int m_lastUpdateTime;
        private static readonly float EPSILON = 1E-09f;
        protected List<MyEntitySubpart> m_subparts = new List<MyEntitySubpart>();
        protected List<HkConstraint> m_subpartConstraints = new List<HkConstraint>();
        protected List<HkFixedConstraintData> m_subpartConstraintsData = new List<HkFixedConstraintData>();
        protected static string[] m_emissiveTextureNames;
        protected Color m_prevEmissiveColor;
        protected float m_prevEmissivity = -1f;
        [CompilerGenerated]
        private Action<bool> DoorStateChanged;
        [CompilerGenerated]
        private Action<Sandbox.ModAPI.IMyDoor, bool> OnDoorStateChanged;
        private HashSet<VRage.ModAPI.IMyEntity> m_children = new HashSet<VRage.ModAPI.IMyEntity>();
        private bool m_updated;
        private bool m_stateChange;

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

        public MyAirtightDoorGeneric()
        {
            base.m_open.ValueChanged += x => this.DoChangeOpenClose();
        }

        protected override void BeforeDelete()
        {
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            this.DisposeConstraints();
            base.BeforeDelete();
        }

        public void ChangeOpenClose(bool open)
        {
            if (open != base.m_open)
            {
                base.m_open.Value = open;
            }
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
            this.UpdateEmissivity(false);
        }

        private void CreateConstraints()
        {
            this.UpdateDoorPosition();
            bool flag = !Sync.IsServer;
            foreach (MyEntitySubpart subpart in this.m_subparts)
            {
                if (((subpart.Physics == null) && (subpart.ModelCollision.HavokCollisionShapes != null)) && (subpart.ModelCollision.HavokCollisionShapes.Length != 0))
                {
                    HkShape shape = subpart.ModelCollision.HavokCollisionShapes[0];
                    subpart.Physics = new MyPhysicsBody(subpart, flag ? RigidBodyFlag.RBF_STATIC : (RigidBodyFlag.RBF_UNLOCKED_SPEEDS | RigidBodyFlag.RBF_DOUBLED_KINEMATIC));
                    HkMassProperties properties = HkInertiaTensorComputer.ComputeBoxVolumeMassProperties(subpart.PositionComp.LocalAABB.HalfExtents, 100f);
                    properties.Volume = subpart.PositionComp.LocalAABB.Volume();
                    subpart.GetPhysicsBody().CreateFromCollisionObject(shape, subpart.PositionComp.LocalVolume.Center, subpart.WorldMatrix, new HkMassProperties?(properties), 9);
                    ((MyPhysicsBody) subpart.Physics).IsSubpart = true;
                }
                if (subpart.Physics != null)
                {
                    if (flag)
                    {
                        subpart.Physics.Enabled = true;
                    }
                    else
                    {
                        HkFixedConstraintData data;
                        HkConstraint constraint;
                        base.CreateSubpartConstraint(subpart, out data, out constraint);
                        this.m_subpartConstraintsData.Add(data);
                        this.m_subpartConstraints.Add(constraint);
                        base.CubeGrid.Physics.AddConstraint(constraint);
                        constraint.SetVirtualMassInverse(Vector4.Zero, Vector4.One);
                    }
                }
            }
            base.CubeGrid.OnHavokSystemIDChanged -= new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            base.CubeGrid.OnHavokSystemIDChanged += new Action<int>(this.CubeGrid_OnHavokSystemIDChanged);
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, false);
            }
        }

        private void CubeGrid_OnHavokSystemIDChanged(int id)
        {
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, true);
            }
        }

        private void DisposeConstraints()
        {
            for (int i = 0; i < this.m_subpartConstraints.Count; i++)
            {
                HkConstraint constraint = this.m_subpartConstraints[i];
                HkFixedConstraintData constraintData = this.m_subpartConstraintsData[i];
                base.DisposeSubpartConstraint(ref constraint, ref constraintData);
            }
            this.m_subpartConstraints.Clear();
            this.m_subpartConstraintsData.Clear();
        }

        internal void DoChangeOpenClose()
        {
            if ((base.Enabled && base.IsWorking) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                if (base.m_soundEmitter != null)
                {
                    base.m_soundEmitter.StopSound(false, true);
                }
                this.OnStateChange();
                base.RaisePropertiesChanged();
            }
        }

        protected virtual void FillSubparts()
        {
        }

        public override bool GetIntersectionWithAABB(ref BoundingBoxD aabb)
        {
            base.Hierarchy.GetChildrenRecursive(this.m_children);
            using (HashSet<VRage.ModAPI.IMyEntity>.Enumerator enumerator = this.m_children.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    VRage.Game.Entity.MyEntity current = (VRage.Game.Entity.MyEntity) enumerator.Current;
                    MyModel model2 = current.Model;
                    if ((model2 != null) && model2.GetTrianglePruningStructure().GetIntersectionWithAABB(current, ref aabb))
                    {
                        return true;
                    }
                }
            }
            MyModel model = base.Model;
            return ((model != null) && model.GetTrianglePruningStructure().GetIntersectionWithAABB(this, ref aabb));
        }

        public override MyObjectBuilder_CubeBlock GetObjectBuilderCubeBlock(bool copy = false)
        {
            MyObjectBuilder_AirtightDoorGeneric objectBuilderCubeBlock = (MyObjectBuilder_AirtightDoorGeneric) base.GetObjectBuilderCubeBlock(copy);
            objectBuilderCubeBlock.Open = (bool) base.m_open;
            objectBuilderCubeBlock.CurrOpening = this.m_currOpening;
            return objectBuilderCubeBlock;
        }

        public override void Init(MyObjectBuilder_CubeBlock builder, MyCubeGrid cubeGrid)
        {
            base.ResourceSink = new MyResourceSinkComponent(1);
            base.ResourceSink.Init(MyStringHash.GetOrCompute(this.BlockDefinition.ResourceSinkGroup), this.BlockDefinition.PowerConsumptionMoving, new Func<float>(this.UpdatePowerInput));
            base.Init(builder, cubeGrid);
            base.NeedsWorldMatrix = false;
            MyObjectBuilder_AirtightDoorGeneric generic = (MyObjectBuilder_AirtightDoorGeneric) builder;
            base.m_open.SetLocalValue(generic.Open);
            this.m_currOpening = MathHelper.Clamp(generic.CurrOpening, 0f, 1f);
            this.m_openingSpeed = this.BlockDefinition.OpeningSpeed;
            this.m_sound = new MySoundPair(this.BlockDefinition.Sound, true);
            this.m_openSound = new MySoundPair(this.BlockDefinition.OpenSound, true);
            this.m_closeSound = new MySoundPair(this.BlockDefinition.CloseSound, true);
            this.m_subpartMovementDistance = this.BlockDefinition.SubpartMovementDistance;
            if (!base.Enabled || !base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                this.UpdateDoorPosition();
            }
            this.OnStateChange();
            base.ResourceSink.IsPoweredChanged += new Action(this.Receiver_IsPoweredChanged);
            base.ResourceSink.Update();
            base.SlimBlock.ComponentStack.IsFunctionalChanged += new Action(this.ComponentStack_IsFunctionalChanged);
            base.ResourceSink.Update();
            if (!Sync.IsServer)
            {
                base.NeedsWorldMatrix = true;
            }
        }

        private void InitSubparts()
        {
            this.FillSubparts();
            MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
            foreach (MyEntitySubpart local1 in this.m_subparts)
            {
                Matrix? childToParent = null;
                local1.Render.SetParent(0, orAddCell.ParentCullObject, childToParent);
                local1.NeedsWorldMatrix = false;
                local1.InvalidateOnMove = false;
            }
            this.UpdateEmissivity(true);
            this.DisposeConstraints();
            if (!base.CubeGrid.CreatePhysics)
            {
                this.UpdateDoorPosition();
            }
            else
            {
                foreach (MyEntitySubpart subpart in this.m_subparts)
                {
                    if (subpart.Physics != null)
                    {
                        subpart.Physics.Close();
                        subpart.Physics = null;
                    }
                }
                if (base.CubeGrid.Projector != null)
                {
                    this.UpdateDoorPosition();
                }
                else
                {
                    this.CreateConstraints();
                    this.UpdateDoorPosition();
                }
            }
        }

        protected bool IsEnoughPower() => 
            ((base.ResourceSink != null) && base.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, this.BlockDefinition.PowerConsumptionMoving));

        public override void OnAddedToScene(object source)
        {
            base.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
            base.OnAddedToScene(source);
            this.UpdateEmissivity(false);
        }

        public override void OnBuildSuccess(long builtBy, bool instantBuild)
        {
            base.ResourceSink.Update();
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, true);
            }
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
                MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
                using (List<MyEntitySubpart>.Enumerator enumerator = this.m_subparts.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Matrix? childToParent = null;
                        enumerator.Current.Render.SetParent(0, orAddCell.ParentCullObject, childToParent);
                    }
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
            this.DisposeConstraints();
            base.OnRemovedFromScene(source);
        }

        private void OnStateChange()
        {
            this.m_currSpeed = (base.m_open == null) ? -this.m_openingSpeed : this.m_openingSpeed;
            base.ResourceSink.Update();
            base.NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
            this.m_lastUpdateTime = MySandboxGame.TotalGamePlayTimeInMilliseconds - 1;
            this.UpdateCurrentOpening();
            this.UpdateDoorPosition();
            if (base.m_open != null)
            {
                this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
            }
            this.m_stateChange = true;
        }

        private void Receiver_IsPoweredChanged()
        {
            base.UpdateIsWorking();
            this.UpdateEmissivity(false);
        }

        private void RecreateConstraints()
        {
            MyCubeGridRenderCell orAddCell = base.CubeGrid.RenderData.GetOrAddCell((Vector3) (base.Position * base.CubeGrid.GridSize), true);
            using (List<MyEntitySubpart>.Enumerator enumerator = this.m_subparts.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyEntitySubpart current = enumerator.Current;
                    if (!current.Closed && !current.MarkedForClose)
                    {
                        Matrix? childToParent = null;
                        current.Render.SetParent(0, orAddCell.ParentCullObject, childToParent);
                        current.NeedsWorldMatrix = false;
                        current.InvalidateOnMove = false;
                        continue;
                    }
                    return;
                }
            }
            this.DisposeConstraints();
            if ((base.InScene && (base.CubeGrid.Physics != null)) && (base.CubeGrid.Physics.IsInWorld || ((MyPhysicsBody) base.CubeGrid.Physics).IsInWorldWelded()))
            {
                this.CreateConstraints();
            }
            if (base.CubeGrid.Physics != null)
            {
                this.UpdateHavokCollisionSystemID(base.CubeGrid.GetPhysicsBody().HavokCollisionSystemID, false);
            }
            this.UpdateDoorPosition();
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
                base.m_open.Value = base.m_open == 0;
            }
        }

        protected void SetEmissive(Color color, float emissivity = 1f, bool force = false)
        {
            if ((base.Render.RenderObjectIDs[0] != uint.MaxValue) && ((force || (color != this.m_prevEmissiveColor)) || (this.m_prevEmissivity != emissivity)))
            {
                string[] emissiveTextureNames = m_emissiveTextureNames;
                int index = 0;
                while (true)
                {
                    if (index >= emissiveTextureNames.Length)
                    {
                        this.m_prevEmissiveColor = color;
                        this.m_prevEmissivity = emissivity;
                        break;
                    }
                    string emissiveName = emissiveTextureNames[index];
                    UpdateNamedEmissiveParts(base.Render.RenderObjectIDs[0], emissiveName, color, emissivity);
                    index++;
                }
            }
        }

        private void StartSound(MySoundPair cuePair)
        {
            if (((base.m_soundEmitter.Sound == null) || !base.m_soundEmitter.Sound.IsPlaying) || ((base.m_soundEmitter.SoundId != cuePair.Arcade) && (base.m_soundEmitter.SoundId != cuePair.Realistic)))
            {
                base.m_soundEmitter.StopSound(true, true);
                bool? nullable = null;
                base.m_soundEmitter.PlaySingleSound(cuePair, true, false, false, nullable);
            }
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (((base.CubeGrid.Physics != null) && (this.m_subparts.Count != 0)) && (((this.m_currSpeed != 0f) && (base.Enabled && base.IsWorking)) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId)))
            {
                this.UpdateDoorPosition();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (this.m_stateChange && (((base.m_open != null) && ((1f - this.m_currOpening) < EPSILON)) || ((base.m_open == null) && (this.m_currOpening < EPSILON))))
            {
                if ((base.m_soundEmitter != null) && base.m_soundEmitter.Loop)
                {
                    base.m_soundEmitter.StopSound(false, true);
                    bool? nullable = null;
                    base.m_soundEmitter.PlaySingleSound(this.m_sound, false, false, true, nullable);
                }
                this.m_currSpeed = 0f;
                if (!base.HasDamageEffect)
                {
                    base.NeedsUpdate &= ~MyEntityUpdateEnum.EACH_FRAME;
                }
                base.ResourceSink.Update();
                base.RaisePropertiesChanged();
                if (base.m_open == null)
                {
                    this.DoorStateChanged.InvokeIfNotNull<bool>((bool) base.m_open);
                    this.OnDoorStateChanged.InvokeIfNotNull<Sandbox.ModAPI.IMyDoor, bool>(this, (bool) base.m_open);
                }
                this.m_stateChange = false;
            }
            if (((base.m_soundEmitter != null) && (base.Enabled && (base.IsWorking && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId)))) && (this.m_currSpeed != 0f))
            {
                if (base.Open)
                {
                    if (this.m_openSound.Equals(MySoundPair.Empty))
                    {
                        this.StartSound(this.m_sound);
                    }
                    else
                    {
                        this.StartSound(this.m_openSound);
                    }
                }
                else if (this.m_closeSound.Equals(MySoundPair.Empty))
                {
                    this.StartSound(this.m_sound);
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

        private void UpdateCurrentOpening()
        {
            if ((base.Enabled && base.IsWorking) && base.ResourceSink.IsPoweredByType(MyResourceDistributorComponent.ElectricityId))
            {
                float num = ((float) (MySandboxGame.TotalGamePlayTimeInMilliseconds - this.m_lastUpdateTime)) / 1000f;
                float num2 = this.m_currSpeed * num;
                this.m_currOpening = MathHelper.Clamp((float) (this.m_currOpening + num2), (float) 0f, (float) 1f);
            }
        }

        protected abstract void UpdateDoorPosition();
        protected virtual void UpdateEmissivity(bool force = false)
        {
        }

        internal void UpdateHavokCollisionSystemID(int havokCollisionSystemID, bool refreshInPlace)
        {
            using (List<MyEntitySubpart>.Enumerator enumerator = this.m_subparts.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    SetupDoorSubpart(enumerator.Current, havokCollisionSystemID, refreshInPlace);
                }
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            this.InitSubparts();
            this.RecreateConstraints();
        }

        protected float UpdatePowerInput()
        {
            if (!base.Enabled || !base.IsFunctional)
            {
                return 0f;
            }
            return ((this.m_currSpeed != 0f) ? this.BlockDefinition.PowerConsumptionMoving : this.BlockDefinition.PowerConsumptionIdle);
        }

        public override void UpdateVisual()
        {
            base.UpdateVisual();
            this.UpdateEmissivity(false);
        }

        protected override void WorldPositionChanged(object source)
        {
            base.WorldPositionChanged(source);
            this.UpdateDoorPosition();
        }

        DoorStatus Sandbox.ModAPI.Ingame.IMyDoor.Status =>
            ((base.m_open == null) ? ((this.m_currOpening < EPSILON) ? DoorStatus.Closed : DoorStatus.Closing) : (((1f - this.m_currOpening) < EPSILON) ? DoorStatus.Open : DoorStatus.Opening));

        public float OpenRatio =>
            this.m_currOpening;

        bool Sandbox.ModAPI.IMyDoor.IsFullyClosed =>
            (this.m_currOpening < EPSILON);

        private MyAirtightDoorGenericDefinition BlockDefinition =>
            ((MyAirtightDoorGenericDefinition) base.BlockDefinition);
    }
}

