namespace Sandbox.Game.Entities
{
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Components;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Voxels;
    using VRage.ObjectBuilders;
    using VRage.Voxels;
    using VRageMath;

    internal class MyVoxelPhysics : MyVoxelBase
    {
        private MyPlanet m_parent;

        public MyVoxelPhysics()
        {
            base.AddDebugRenderComponent(new MyDebugRenderComponentVoxelMap(this));
        }

        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            base.m_storage = null;
        }

        public override int GetOrePriority() => 
            0;

        public override void Init(MyObjectBuilder_EntityBase builder, IMyStorage storage)
        {
        }

        public void Init(IMyStorage storage, MatrixD worldMatrix, Vector3I storageMin, Vector3I storageMax, MyPlanet parent)
        {
            this.m_parent = parent;
            long num = (((((storageMin.X * 0x18dL) ^ storageMin.Y) * 0x18dL) ^ storageMin.Z) * 0x18dL) ^ parent.EntityId;
            base.EntityId = MyEntityIdentifier.ConstructId(MyEntityIdentifier.ID_OBJECT_TYPE.VOXEL_PHYSICS, num & 0xffffffffffffffL);
            base.Init(null);
            this.InitVoxelMap(worldMatrix, base.Size, false);
            this.Valid = true;
        }

        public void Init(IMyStorage storage, Vector3D positionMinCorner, Vector3I storageMin, Vector3I storageMax, MyPlanet parent)
        {
            this.PositionLeftBottomCorner = positionMinCorner;
            base.m_storageMax = storageMax;
            base.m_storageMin = storageMin;
            base.m_storage = storage;
            base.SizeInMetres = (Vector3) (base.Size * 1f);
            base.SizeInMetresHalf = base.SizeInMetres / 2f;
            MatrixD worldMatrix = MatrixD.CreateTranslation(positionMinCorner + base.SizeInMetresHalf);
            this.Init(storage, worldMatrix, storageMin, storageMax, parent);
        }

        protected override void InitVoxelMap(MatrixD worldMatrix, Vector3I size, bool useOffset = true)
        {
            base.InitVoxelMap(worldMatrix, size, useOffset);
            this.Physics = new MyVoxelPhysicsBody(this, 1.5f, 7f, false);
            this.Physics.Enabled = true;
        }

        public void OnStorageChanged(Vector3I minChanged, Vector3I maxChanged, MyStorageDataTypeFlags dataChanged)
        {
            Vector3I vectori1 = Vector3I.Clamp(minChanged, base.m_storageMin, base.m_storageMax);
            minChanged = vectori1;
            Vector3I vectori2 = Vector3I.Clamp(maxChanged, base.m_storageMin, base.m_storageMax);
            maxChanged = vectori2;
            if (((dataChanged & MyStorageDataTypeFlags.Content) != MyStorageDataTypeFlags.None) && (this.Physics != null))
            {
                this.Physics.InvalidateRange(minChanged, maxChanged);
                base.RaisePhysicsChanged();
            }
        }

        public void PrefetchShapeOnRay(ref LineD ray)
        {
            if (this.Physics != null)
            {
                this.Physics.PrefetchShapeOnRay(ref ray);
            }
        }

        public void RefreshPhysics(IMyStorage storage)
        {
            base.m_storage = storage;
            this.OnStorageChanged(base.m_storageMin, base.m_storageMax, MyStorageDataTypeFlags.Content);
            this.Valid = true;
        }

        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (this.Physics != null)
            {
                this.Physics.UpdateAfterSimulation10();
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (this.Physics != null)
            {
                this.Physics.UpdateBeforeSimulation10();
            }
        }

        internal MyVoxelPhysicsBody Physics
        {
            get => 
                (base.Physics as MyVoxelPhysicsBody);
            set => 
                (base.Physics = value);
        }

        public override MyVoxelBase RootVoxel =>
            this.m_parent;

        public bool Valid { get; set; }

        public MyPlanet Parent =>
            this.m_parent;
    }
}

