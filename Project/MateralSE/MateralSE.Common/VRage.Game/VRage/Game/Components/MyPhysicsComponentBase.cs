namespace VRage.Game.Components
{
    using Havok;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.Game.SessionComponents;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;

    [MyComponentType(typeof(MyPhysicsComponentBase))]
    public abstract class MyPhysicsComponentBase : MyEntityComponentBase
    {
        protected Vector3 m_lastLinearVelocity;
        protected Vector3 m_lastAngularVelocity;
        private Vector3 m_linearVelocity;
        private Vector3 m_angularVelocity;
        private Vector3 m_supportNormal;
        public ushort ContactPointDelay = 0xffff;
        public Action EnabledChanged;
        public RigidBodyFlag Flags;
        public bool IsPhantom;
        protected bool m_enabled;

        public abstract event Action<MyPhysicsComponentBase, bool> OnBodyActiveStateChanged;

        protected MyPhysicsComponentBase()
        {
        }

        public abstract void Activate();
        public abstract void AddForce(MyPhysicsForceType type, Vector3? force, Vector3D? position, Vector3? torque, float? maxSpeed = new float?(), bool applyImmediately = true, bool activeOnly = false);
        public abstract void ApplyImpulse(Vector3 dir, Vector3D pos);
        public abstract void Clear();
        private void ClearAccelerations()
        {
            this.LinearAcceleration = Vector3.Zero;
            this.AngularAcceleration = Vector3.Zero;
        }

        public abstract void ClearSpeed();
        public virtual void Close()
        {
            this.Deactivate();
            this.CloseRigidBody();
        }

        protected abstract void CloseRigidBody();
        public abstract Vector3D ClusterToWorld(Vector3 clusterPos);
        public abstract void CreateCharacterCollision(Vector3 center, float characterWidth, float characterHeight, float crouchHeight, float ladderHeight, float headSize, float headHeight, MatrixD worldTransform, float mass, ushort collisionLayer, bool isOnlyVertical, float maxSlope, float maxLimit, float maxSpeedRelativeToShip, bool networkProxy, float? maxForce);
        public abstract void Deactivate();
        public abstract void DebugDraw();
        public override void Deserialize(MyObjectBuilder_ComponentBase baseBuilder)
        {
            MyObjectBuilder_PhysicsComponentBase base2 = baseBuilder as MyObjectBuilder_PhysicsComponentBase;
            this.LinearVelocity = (Vector3) base2.LinearVelocity;
            this.AngularVelocity = (Vector3) base2.AngularVelocity;
        }

        public abstract void ForceActivate();
        public virtual MyStringHash GetMaterialAt(Vector3D worldPos) => 
            this.MaterialType;

        public abstract Vector3 GetVelocityAtPoint(Vector3D worldPos);
        public abstract void GetVelocityAtPointLocal(ref Vector3D worldPos, out Vector3 linearVelocity);
        public abstract MatrixD GetWorldMatrix();
        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            this.Definition = definition as MyPhysicsComponentDefinitionBase;
            if (this.Definition != null)
            {
                this.Flags = this.Definition.RigidBodyFlags;
                if (this.Definition.LinearDamping != null)
                {
                    this.LinearDamping = this.Definition.LinearDamping.Value;
                }
                if (this.Definition.AngularDamping != null)
                {
                    this.AngularDamping = this.Definition.AngularDamping.Value;
                }
            }
        }

        public override bool IsSerialized() => 
            ((this.Definition != null) && this.Definition.Serialize);

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.Entity = base.Container.Entity;
            if ((this.Definition != null) && (this.Definition.UpdateFlags != 0))
            {
                MyPhysicsComponentSystem.Static.Register(this);
            }
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            if (((this.Definition != null) && (this.Definition.UpdateFlags != 0)) && (MyPhysicsComponentSystem.Static != null))
            {
                MyPhysicsComponentSystem.Static.Unregister(this);
            }
        }

        public abstract void OnWorldPositionChanged(object source);
        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_PhysicsComponentBase base1 = MyComponentFactory.CreateObjectBuilder(this) as MyObjectBuilder_PhysicsComponentBase;
            base1.LinearVelocity = this.LinearVelocity;
            base1.AngularVelocity = this.AngularVelocity;
            return base1;
        }

        private void SetActualSpeedsAsPrevious()
        {
            this.m_lastLinearVelocity = this.LinearVelocity;
            this.m_lastAngularVelocity = this.AngularVelocity;
        }

        public void SetSpeeds(Vector3 linear, Vector3 angular)
        {
            this.LinearVelocity = linear;
            this.AngularVelocity = angular;
            this.ClearAccelerations();
            this.SetActualSpeedsAsPrevious();
        }

        public void UpdateAccelerations()
        {
            Vector3 linearVelocity = this.LinearVelocity;
            this.LinearAcceleration = (linearVelocity - this.m_lastLinearVelocity) * 60f;
            this.m_lastLinearVelocity = linearVelocity;
            Vector3 angularVelocity = this.AngularVelocity;
            this.AngularAcceleration = (angularVelocity - this.m_lastAngularVelocity) * 60f;
            this.m_lastAngularVelocity = angularVelocity;
        }

        public abstract void UpdateFromSystem();
        public abstract Vector3D WorldToCluster(Vector3D worldPos);

        public bool ReportAllContacts
        {
            get => 
                (this.ContactPointDelay == 0);
            set => 
                (this.ContactPointDelay = value ? ((ushort) 0) : ((ushort) 0xffff));
        }

        public IMyEntity Entity { get; protected set; }

        public bool CanUpdateAccelerations { get; set; }

        public MyStringHash MaterialType { get; set; }

        public virtual bool IsStatic =>
            ((this.Flags & RigidBodyFlag.RBF_STATIC) == RigidBodyFlag.RBF_STATIC);

        public virtual bool IsKinematic =>
            (((this.Flags & RigidBodyFlag.RBF_KINEMATIC) == RigidBodyFlag.RBF_KINEMATIC) || ((this.Flags & RigidBodyFlag.RBF_DOUBLED_KINEMATIC) == RigidBodyFlag.RBF_DOUBLED_KINEMATIC));

        public virtual bool Enabled
        {
            get => 
                this.m_enabled;
            set
            {
                if (this.m_enabled != value)
                {
                    this.m_enabled = value;
                    if (this.EnabledChanged != null)
                    {
                        this.EnabledChanged();
                    }
                    if (!value)
                    {
                        this.Deactivate();
                    }
                    else if (this.Entity.InScene)
                    {
                        this.Activate();
                    }
                }
            }
        }

        public bool PlayCollisionCueEnabled { get; set; }

        public abstract float Mass { get; }

        public Vector3 Center { get; set; }

        public virtual Vector3 LinearVelocity
        {
            get => 
                this.m_linearVelocity;
            set => 
                (this.m_linearVelocity = value);
        }

        public virtual Vector3 LinearVelocityLocal
        {
            get => 
                this.m_linearVelocity;
            set => 
                (this.m_linearVelocity = value);
        }

        public virtual Vector3 AngularVelocity
        {
            get => 
                this.m_angularVelocity;
            set => 
                (this.m_angularVelocity = value);
        }

        public virtual Vector3 AngularVelocityLocal
        {
            get => 
                this.m_angularVelocity;
            set => 
                (this.m_angularVelocity = value);
        }

        public virtual Vector3 SupportNormal
        {
            get => 
                this.m_supportNormal;
            set => 
                (this.m_supportNormal = value);
        }

        public virtual Vector3 LinearAcceleration { get; protected set; }

        public virtual Vector3 AngularAcceleration { get; protected set; }

        public abstract float LinearDamping { get; set; }

        public abstract float AngularDamping { get; set; }

        public abstract float Speed { get; }

        public abstract float Friction { get; set; }

        public abstract HkRigidBody RigidBody { get; protected set; }

        public abstract HkRigidBody RigidBody2 { get; protected set; }

        public abstract HkdBreakableBody BreakableBody { get; set; }

        public abstract bool IsMoving { get; }

        public abstract Vector3 Gravity { get; set; }

        public MyPhysicsComponentDefinitionBase Definition { get; private set; }

        public abstract bool IsActive { get; }

        public abstract bool HasRigidBody { get; }

        public abstract Vector3 CenterOfMassLocal { get; }

        public abstract Vector3D CenterOfMassWorld { get; }

        public virtual bool IsInWorld { get; protected set; }

        public virtual bool ShapeChangeInProgress { get; set; }

        public override string ComponentTypeDebugString =>
            "Physics";
    }
}

