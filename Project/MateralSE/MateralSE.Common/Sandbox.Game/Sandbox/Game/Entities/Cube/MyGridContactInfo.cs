namespace Sandbox.Game.Entities.Cube
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyGridContactInfo
    {
        public HkContactPointEvent Event;
        public readonly Vector3D ContactPosition;
        public MyCubeGrid m_currentEntity;
        public VRage.Game.Entity.MyEntity m_collidingEntity;
        private MySlimBlock m_currentBlock;
        private MySlimBlock m_otherBlock;
        private bool m_voxelSurfaceMaterialInitialized;
        private MyVoxelMaterialDefinition m_voxelSurfaceMaterial;
        public float ImpulseMultiplier;
        public MyCubeGrid CurrentEntity =>
            this.m_currentEntity;
        public VRage.Game.Entity.MyEntity CollidingEntity =>
            this.m_collidingEntity;
        public MySlimBlock OtherBlock =>
            this.m_otherBlock;
        public MyVoxelMaterialDefinition VoxelSurfaceMaterial
        {
            get
            {
                if (!this.m_voxelSurfaceMaterialInitialized)
                {
                    this.ReadVoxelSurfaceMaterial();
                    this.m_voxelSurfaceMaterialInitialized = true;
                }
                return this.m_voxelSurfaceMaterial;
            }
        }
        private ContactFlags Flags
        {
            get => 
                ((ContactFlags) this.Event.ContactProperties.UserData.AsUint);
            set => 
                (this.Event.ContactProperties.UserData = HkContactUserData.UInt((uint) value));
        }
        public bool EnableDeformation
        {
            get => 
                ((this.Flags & ContactFlags.Deformation) != 0);
            set => 
                this.SetFlag(ContactFlags.Deformation, value);
        }
        public bool RubberDeformation
        {
            get => 
                ((this.Flags & ContactFlags.RubberDeformation) != 0);
            set => 
                this.SetFlag(ContactFlags.RubberDeformation, value);
        }
        public bool EnableParticles
        {
            get => 
                ((this.Flags & ContactFlags.Particles) != 0);
            set => 
                this.SetFlag(ContactFlags.Particles, value);
        }
        public MyGridContactInfo(ref HkContactPointEvent evnt, MyCubeGrid grid) : this(ref evnt, grid, evnt.GetOtherEntity(grid) as VRage.Game.Entity.MyEntity)
        {
        }

        public MyGridContactInfo(ref HkContactPointEvent evnt, MyCubeGrid grid, VRage.Game.Entity.MyEntity collidingEntity)
        {
            this.Event = evnt;
            this.ContactPosition = grid.Physics.ClusterToWorld(evnt.ContactPoint.Position);
            this.m_currentEntity = grid;
            this.m_collidingEntity = collidingEntity;
            this.m_currentBlock = null;
            this.m_otherBlock = null;
            this.ImpulseMultiplier = 1f;
            this.m_voxelSurfaceMaterial = null;
            this.m_voxelSurfaceMaterialInitialized = false;
        }

        public bool IsKnown =>
            ((this.Flags & ContactFlags.Known) != 0);
        public void HandleEvents()
        {
            if ((this.Flags & ContactFlags.Known) == 0)
            {
                this.Flags |= ContactFlags.Particles | ContactFlags.Deformation | ContactFlags.Known;
                this.m_currentBlock = GetContactBlock(this.CurrentEntity, this.ContactPosition, this.Event.ContactPoint.NormalAndDistance.W);
                MyCubeGrid collidingEntity = this.CollidingEntity as MyCubeGrid;
                if (collidingEntity != null)
                {
                    this.m_otherBlock = GetContactBlock(collidingEntity, this.ContactPosition, this.Event.ContactPoint.NormalAndDistance.W);
                }
                if ((this.m_currentBlock != null) && (this.m_currentBlock.FatBlock != null))
                {
                    this.m_currentBlock.FatBlock.ContactPointCallback(ref this);
                    this.ImpulseMultiplier *= this.m_currentBlock.BlockDefinition.PhysicalMaterial.CollisionMultiplier;
                }
                if ((this.m_otherBlock != null) && (this.m_otherBlock.FatBlock != null))
                {
                    this.SwapEntities();
                    this.Event.ContactPoint.Flip();
                    this.ImpulseMultiplier *= this.m_currentBlock.BlockDefinition.PhysicalMaterial.CollisionMultiplier;
                    this.m_currentBlock.FatBlock.ContactPointCallback(ref this);
                    this.SwapEntities();
                    this.Event.ContactPoint.Flip();
                }
            }
        }

        private void SetFlag(ContactFlags flag, bool value)
        {
            this.Flags = value ? (this.Flags | flag) : (this.Flags & ~flag);
        }

        private void SwapEntities()
        {
            MyCubeGrid currentEntity = this.m_currentEntity;
            this.m_currentEntity = (MyCubeGrid) this.m_collidingEntity;
            this.m_collidingEntity = currentEntity;
            MySlimBlock currentBlock = this.m_currentBlock;
            this.m_currentBlock = this.m_otherBlock;
            this.m_otherBlock = currentBlock;
        }

        private static unsafe MySlimBlock GetContactBlock(MyCubeGrid grid, Vector3D worldPosition, float graceDistance)
        {
            Vector3D vectord;
            Vector3I vectori6;
            HashSet<MySlimBlock> cubeBlocks = grid.CubeBlocks;
            if (cubeBlocks.Count == 1)
            {
                return cubeBlocks.FirstElement<MySlimBlock>();
            }
            MatrixD worldMatrixNormalizedInv = grid.PositionComp.WorldMatrixNormalizedInv;
            Vector3D.Transform(ref worldPosition, ref worldMatrixNormalizedInv, out vectord);
            MySlimBlock block = null;
            float maxValue = float.MaxValue;
            if (cubeBlocks.Count < 10)
            {
                foreach (MySlimBlock block2 in cubeBlocks)
                {
                    Vector3D vectord3 = ((Vector3D) (block2.Position * grid.GridSize)) - vectord;
                    float num3 = (float) vectord3.LengthSquared();
                    if (num3 < maxValue)
                    {
                        maxValue = num3;
                        block = block2;
                    }
                }
                return block;
            }
            bool flag = false;
            Vector3 linearVelocity = grid.Physics.LinearVelocity;
            float num2 = MyGridPhysics.ShipMaxLinearVelocity();
            if (linearVelocity.LengthSquared() > ((num2 * num2) * 100f))
            {
                flag = true;
                linearVelocity /= linearVelocity.Length() * num2;
            }
            float single1 = Math.Max(Math.Abs(graceDistance), grid.GridSize * 0.2f);
            graceDistance = single1;
            graceDistance++;
            Vector3D vectord2 = Vector3D.TransformNormal(linearVelocity * 0.01666667f, worldMatrixNormalizedInv);
            Vector3I vectori = Vector3I.Round(((vectord + graceDistance) + vectord2) / ((double) grid.GridSize));
            Vector3I vectori2 = Vector3I.Round(((vectord + graceDistance) - vectord2) / ((double) grid.GridSize));
            Vector3I vectori3 = Vector3I.Round(((vectord - graceDistance) + vectord2) / ((double) grid.GridSize));
            Vector3I vectori1 = Vector3I.Round(((vectord - graceDistance) - vectord2) / ((double) grid.GridSize));
            Vector3I vectori4 = Vector3I.Min(Vector3I.Min(Vector3I.Min(vectori1, vectori), vectori2), vectori3);
            Vector3I vectori5 = Vector3I.Max(Vector3I.Max(Vector3I.Max(vectori1, vectori), vectori2), vectori3);
            vectori6.X = vectori4.X;
            while (vectori6.X <= vectori5.X)
            {
                vectori6.Y = vectori4.Y;
                while (true)
                {
                    if (vectori6.Y > vectori5.Y)
                    {
                        int* numPtr3 = (int*) ref vectori6.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori6.Z = vectori4.Z;
                    while (true)
                    {
                        if (vectori6.Z > vectori5.Z)
                        {
                            int* numPtr2 = (int*) ref vectori6.Y;
                            numPtr2[0]++;
                            break;
                        }
                        MySlimBlock cubeBlock = grid.GetCubeBlock(vectori6);
                        if (cubeBlock != null)
                        {
                            float num4 = (float) ((vectori6 * grid.GridSize) - vectord).LengthSquared();
                            if (num4 < maxValue)
                            {
                                maxValue = num4;
                                block = cubeBlock;
                                if (flag)
                                {
                                    return block;
                                }
                            }
                        }
                        int* numPtr1 = (int*) ref vectori6.Z;
                        numPtr1[0]++;
                    }
                }
            }
            return block;
        }

        private void ReadVoxelSurfaceMaterial()
        {
            MyVoxelPhysicsBody physics = this.m_collidingEntity.Physics as MyVoxelPhysicsBody;
            if (physics != null)
            {
                int bodyIndex = ReferenceEquals(this.Event.GetPhysicsBody(0), physics) ? 0 : 1;
                uint num2 = physics.GetHitTriangleMaterial(ref this.Event, bodyIndex);
                if (num2 != uint.MaxValue)
                {
                    this.m_voxelSurfaceMaterial = MyDefinitionManager.Static.GetVoxelMaterialDefinition((byte) num2);
                }
            }
        }
        [Flags]
        public enum ContactFlags
        {
            Known = 1,
            Deformation = 8,
            Particles = 0x10,
            RubberDeformation = 0x20,
            PredictedCollision = 0x40,
            PredictedCollision_Disabled = 0x80
        }
    }
}

