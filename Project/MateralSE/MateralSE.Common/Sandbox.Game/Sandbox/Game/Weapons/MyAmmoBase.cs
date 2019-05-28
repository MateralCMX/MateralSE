namespace Sandbox.Game.Weapons
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;

    public class MyAmmoBase : MyEntity
    {
        protected Vector3D m_origin;
        protected MyWeaponDefinition m_weaponDefinition;
        protected long m_originEntity;
        private float m_ammoOffsetSize;
        protected bool m_shouldExplode;
        protected bool m_markedToDestroy;
        protected bool m_physicsEnabled;

        protected MyAmmoBase()
        {
            base.Save = false;
        }

        protected override void Closing()
        {
            base.Closing();
            if (this.m_physicsEnabled)
            {
                this.Physics.ContactPointCallback -= new MyPhysicsBody.PhysicsContactHandler(this.OnContactPointCallback);
            }
        }

        protected void Init(MyWeaponPropertiesWrapper weaponProperties, string modelName, bool spherePhysics = true, bool capsulePhysics = false, bool bulletType = false, bool physics = true)
        {
            MyEntityIdentifier.AllocationSuspended = true;
            float? scale = null;
            base.Init(null, modelName, null, scale, null);
            this.m_weaponDefinition = weaponProperties.WeaponDefinition;
            this.m_physicsEnabled = physics;
            if (physics)
            {
                if (spherePhysics)
                {
                    this.InitSpherePhysics(MyMaterialType.AMMO, base.Model, 100f, MyPerGameSettings.DefaultLinearDamping, MyPerGameSettings.DefaultAngularDamping, 0x1b, bulletType ? RigidBodyFlag.RBF_BULLET : RigidBodyFlag.RBF_DEFAULT);
                }
                else if (!capsulePhysics)
                {
                    this.InitBoxPhysics(MyMaterialType.AMMO, base.Model, 1f, MyPerGameSettings.DefaultAngularDamping, 0x1b, bulletType ? RigidBodyFlag.RBF_BULLET : RigidBodyFlag.RBF_DEFAULT);
                }
                else
                {
                    this.InitCapsulePhysics(MyMaterialType.AMMO, new Vector3(0f, 0f, -base.Model.BoundingBox.HalfExtents.Z * 0.8f), new Vector3(0f, 0f, base.Model.BoundingBox.HalfExtents.Z * 0.8f), 0.1f, 10f, 0f, 0f, 0x1b, bulletType ? RigidBodyFlag.RBF_BULLET : RigidBodyFlag.RBF_DEFAULT);
                    this.m_ammoOffsetSize = (base.Model.BoundingBox.HalfExtents.Z * 0.8f) + 0.1f;
                }
                this.Physics.RigidBody.ContactPointCallbackEnabled = true;
                this.Physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.OnContactPointCallback);
            }
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
            base.Render.CastShadows = false;
            MyEntityIdentifier.AllocationSuspended = MyEntityIdentifier.AllocationSuspended;
        }

        public virtual void MarkForDestroy()
        {
            this.m_markedToDestroy = true;
            base.Close();
        }

        private void OnContactPointCallback(ref MyPhysics.MyContactPointEvent value)
        {
            if (value.ContactPointEvent.EventType != HkContactPointEvent.Type.ManifoldAtEndOfStep)
            {
                this.OnContactStart(ref value);
            }
        }

        protected virtual void OnContactStart(ref MyPhysics.MyContactPointEvent value)
        {
        }

        protected void Start(Vector3D position, Vector3D initialVelocity, Vector3D direction)
        {
            this.m_shouldExplode = false;
            this.m_origin = position + (direction * this.m_ammoOffsetSize);
            this.m_markedToDestroy = false;
            MatrixD worldMatrix = MatrixD.CreateWorld(this.m_origin, direction, Vector3D.CalculatePerpendicularVector(direction));
            base.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
            if (this.m_physicsEnabled)
            {
                this.Physics.Clear();
                this.Physics.Enabled = true;
                this.Physics.LinearVelocity = (Vector3) initialVelocity;
            }
            base.NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public MyPhysicsBody Physics =>
            (base.Physics as MyPhysicsBody);
    }
}

