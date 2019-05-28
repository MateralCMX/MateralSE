namespace Sandbox.Game.Replication.History
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRageMath;

    [StructLayout(LayoutKind.Sequential)]
    public struct MySnapshot
    {
        public long ParentId;
        public bool SkippedParent;
        public bool Active;
        public bool InheritRotation;
        public Vector3D Position;
        public Quaternion Rotation;
        public Vector3 LinearVelocity;
        public Vector3 AngularVelocity;
        public Vector3 Pivot;
        public static bool ApplyReset;
        public MySnapshot(BitStream stream)
        {
            this = new MySnapshot();
            this.Read(stream);
        }

        public unsafe MySnapshot(MyEntity entity, bool localPhysics = false, bool inheritRotation = true)
        {
            int isActive;
            MyEntity parent = GetParent(entity, out this.SkippedParent);
            if ((entity.Physics == null) || (entity.Physics.RigidBody == null))
            {
                isActive = 1;
            }
            else
            {
                isActive = (int) entity.Physics.RigidBody.IsActive;
            }
            this.Active = (bool) isActive;
            this.InheritRotation = inheritRotation;
            this.LinearVelocity = Vector3.Zero;
            this.AngularVelocity = Vector3.Zero;
            this.Pivot = Vector3.Zero;
            MatrixD worldMatrix = entity.WorldMatrix;
            MyCubeGrid grid = entity as MyCubeGrid;
            if (parent == null)
            {
                Vector3 center;
                this.ParentId = 0L;
                if ((entity.Physics == null) || (entity.Physics.RigidBody == null))
                {
                    center = entity.PositionComp.LocalAABB.Center;
                }
                else
                {
                    center = entity.Physics.CenterOfMassLocal;
                }
                this.Pivot = center;
                Vector3D.Transform(ref this.Pivot, ref worldMatrix, out this.Position);
                Quaternion.CreateFromRotationMatrix(ref worldMatrix, out this.Rotation);
                this.Rotation.Normalize();
                if (entity.Physics != null)
                {
                    this.LinearVelocity = entity.Physics.LinearVelocity;
                    this.AngularVelocity = entity.Physics.AngularVelocity;
                }
            }
            else
            {
                this.ParentId = parent.EntityId;
                MatrixD worldMatrixInvScaled = new MatrixD();
                if (!this.InheritRotation)
                {
                    MatrixD* xdPtr2 = (MatrixD*) ref worldMatrix;
                    xdPtr2.Translation -= parent.PositionComp.GetPosition();
                }
                else
                {
                    worldMatrixInvScaled = parent.PositionComp.WorldMatrixInvScaled;
                    MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                    MatrixD.Multiply(ref (MatrixD) ref xdPtr1, ref worldMatrixInvScaled, out worldMatrix);
                }
                this.Position = worldMatrix.Translation;
                Quaternion.CreateFromRotationMatrix(ref worldMatrix, out this.Rotation);
                this.Rotation.Normalize();
                if ((grid == null) || !MyFakes.SNAPSHOTS_MECHANICAL_PIVOTS)
                {
                    Vector3 center;
                    if ((entity.Physics == null) || (entity.Physics.RigidBody == null))
                    {
                        center = entity.PositionComp.LocalAABB.Center;
                    }
                    else
                    {
                        center = entity.Physics.CenterOfMassLocal;
                    }
                    this.Pivot = center;
                    Vector3D.Transform(ref this.Pivot, ref worldMatrix, out this.Position);
                }
                else
                {
                    MyMechanicalConnectionBlockBase entityConnectingToParent = MyGridPhysicalHierarchy.Static.GetEntityConnectingToParent(grid) as MyMechanicalConnectionBlockBase;
                    if (entityConnectingToParent != null)
                    {
                        Vector3? constraintPosition = entityConnectingToParent.GetConstraintPosition(grid, false);
                        if (constraintPosition != null)
                        {
                            this.Pivot = constraintPosition.Value;
                            Vector3D.Transform(ref this.Pivot, ref worldMatrix, out this.Position);
                        }
                    }
                }
                if ((entity.Physics != null) && (parent.Physics != null))
                {
                    this.LinearVelocity = entity.Physics.LinearVelocityLocal;
                    this.AngularVelocity = entity.Physics.AngularVelocity;
                    if (!localPhysics)
                    {
                        Vector3 linearVelocity;
                        if (!this.InheritRotation)
                        {
                            linearVelocity = parent.Physics.LinearVelocity;
                        }
                        else
                        {
                            Vector3D position = entity.PositionComp.GetPosition();
                            parent.Physics.GetVelocityAtPointLocal(ref position, out linearVelocity);
                        }
                        this.LinearVelocity -= linearVelocity;
                    }
                }
            }
        }

        public static MyEntity GetParent(MyEntity entity, out bool skipped)
        {
            skipped = false;
            if (MyFakes.WORLD_SNAPSHOTS)
            {
                skipped = true;
                return null;
            }
            MyEntity entityById = null;
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid == null)
            {
                MyCharacter character = entity as MyCharacter;
                if (character != null)
                {
                    entityById = MyEntities.GetEntityById(character.ClosestParentId, false);
                }
            }
            else if (grid.ClosestParentId != 0)
            {
                entityById = MyEntities.GetEntityById(grid.ClosestParentId, false);
            }
            else if (MyGridPhysicalHierarchy.Static.GetNodeChainLength(grid) < 4)
            {
                entityById = MyGridPhysicalHierarchy.Static.GetParent(grid);
            }
            else
            {
                skipped = true;
            }
            if ((entityById == null) || (!entityById.MarkedForClose && !entityById.Closed))
            {
                return entityById;
            }
            return null;
        }

        public void Diff(ref MySnapshot value, out MySnapshot ss)
        {
            if (this.ParentId != value.ParentId)
            {
                ss = new MySnapshot();
            }
            else
            {
                Quaternion quaternion = Quaternion.Inverse(this.Rotation);
                ss.Active = this.Active;
                ss.ParentId = this.ParentId;
                ss.SkippedParent = this.SkippedParent;
                ss.InheritRotation = this.InheritRotation;
                Vector3D.Subtract(ref this.Position, ref value.Position, out ss.Position);
                Quaternion.Multiply(ref quaternion, ref value.Rotation, out ss.Rotation);
                Vector3.Subtract(ref this.LinearVelocity, ref value.LinearVelocity, out ss.LinearVelocity);
                Vector3.Subtract(ref this.AngularVelocity, ref value.AngularVelocity, out ss.AngularVelocity);
                Vector3.Subtract(ref this.Pivot, ref value.Pivot, out ss.Pivot);
            }
        }

        public void Scale(float factor)
        {
            this.ScaleTransform(factor);
            this.LinearVelocity *= factor;
            this.AngularVelocity *= factor;
        }

        private void ScaleTransform(float factor)
        {
            Vector3 vector;
            float num;
            this.Rotation.GetAxisAngle(out vector, out num);
            Quaternion.CreateFromAxisAngle(ref vector, num * factor, out this.Rotation);
            this.Position *= factor;
            this.Pivot *= factor;
        }

        public bool CheckThresholds(float posSq, float rotSq, float linearSq, float angularSq) => 
            ((this.Position.LengthSquared() > posSq) || ((Math.Abs((float) (this.Rotation.W - 1f)) > rotSq) || ((this.LinearVelocity.LengthSquared() > linearSq) || (this.AngularVelocity.LengthSquared() > angularSq))));

        public void Add(ref MySnapshot value)
        {
            if (this.ParentId == value.ParentId)
            {
                this.Active = value.Active;
                this.InheritRotation = value.InheritRotation;
                this.Position += value.Position;
                this.Pivot += value.Pivot;
                this.Rotation *= Quaternion.Inverse(value.Rotation);
                this.Rotation.Normalize();
                this.LinearVelocity += value.LinearVelocity;
                this.AngularVelocity += value.AngularVelocity;
            }
        }

        public void GetMatrix(MyEntity entity, out MatrixD mat, bool applyPosition = true, bool applyRotation = true)
        {
            MatrixD worldMatrix = entity.WorldMatrix;
            this.GetMatrix(out mat, ref worldMatrix, applyPosition, applyRotation);
        }

        public void GetMatrix(out MatrixD mat, ref MatrixD originalWorldMat, bool applyPosition = true, bool applyRotation = true)
        {
            MyEntity entityById = null;
            if (this.ParentId != 0)
            {
                entityById = MyEntities.GetEntityById(this.ParentId, false);
            }
            if (entityById == null)
            {
                if (applyRotation)
                {
                    MatrixD.CreateFromQuaternion(ref this.Rotation, out mat);
                }
                else
                {
                    mat = originalWorldMat;
                }
                if (!applyPosition)
                {
                    mat.Translation = originalWorldMat.Translation;
                }
                else
                {
                    mat.Translation = this.Position;
                    mat.Translation = Vector3D.Transform(-this.Pivot, ref mat);
                }
            }
            else
            {
                MatrixD worldMatrix = entityById.WorldMatrix;
                if (applyPosition & applyRotation)
                {
                    MatrixD.CreateFromQuaternion(ref this.Rotation, out mat);
                    mat.Translation = this.Position;
                    if (this.InheritRotation)
                    {
                        MatrixD.Multiply(ref mat, ref worldMatrix, out mat);
                    }
                    else
                    {
                        mat.Translation += worldMatrix.Translation;
                    }
                }
                else if (!applyPosition)
                {
                    MatrixD.CreateFromQuaternion(ref this.Rotation, out mat);
                    if (this.InheritRotation)
                    {
                        MatrixD.Multiply(ref mat, ref worldMatrix, out mat);
                    }
                    mat.Translation = originalWorldMat.Translation;
                }
                else
                {
                    mat = originalWorldMat;
                    if (!this.InheritRotation)
                    {
                        mat.Translation = worldMatrix.Translation + this.Position;
                    }
                    else
                    {
                        Vector3D vectord;
                        Vector3D.Transform(ref this.Position, ref worldMatrix, out vectord);
                        mat.Translation = vectord;
                    }
                }
                mat.Translation = Vector3D.Transform(-this.Pivot, ref mat);
            }
        }

        public Vector3 GetLinearVelocity(bool local)
        {
            MyEntity objA = null;
            Vector3 vector;
            if (this.ParentId != 0)
            {
                objA = MyEntities.GetEntityById(this.ParentId, false);
            }
            if (ReferenceEquals(objA, null) | local)
            {
                return this.LinearVelocity;
            }
            if (!this.InheritRotation)
            {
                return (objA.Physics.LinearVelocity + this.LinearVelocity);
            }
            Vector3D position = objA.PositionComp.GetPosition();
            objA.Physics.GetVelocityAtPointLocal(ref position, out vector);
            return (vector + this.LinearVelocity);
        }

        public Vector3 GetAngularVelocity(bool local) => 
            this.AngularVelocity;

        public void ApplyPhysics(MyEntity entity, bool angular = true, bool linear = true, bool local = false)
        {
            if (entity.Physics != null)
            {
                if (!this.Active)
                {
                    entity.Physics.LinearVelocity = Vector3.Zero;
                    entity.Physics.AngularVelocity = Vector3.Zero;
                }
                else
                {
                    if (linear)
                    {
                        entity.Physics.LinearVelocity = this.GetLinearVelocity(local);
                    }
                    if (angular)
                    {
                        entity.Physics.AngularVelocity = this.GetAngularVelocity(local);
                    }
                }
                HkRigidBody rigidBody = entity.Physics.RigidBody;
                if (((rigidBody != null) && !rigidBody.IsFixed) && (rigidBody.IsActive != this.Active))
                {
                    if (this.Active)
                    {
                        rigidBody.Activate();
                    }
                    else
                    {
                        rigidBody.Deactivate();
                    }
                }
            }
        }

        public void Lerp(ref MySnapshot value, float factor, out MySnapshot ss)
        {
            ss.Active = (factor > 1f) ? value.Active : ((factor < 0f) ? this.Active : (this.Active || value.Active));
            ss.ParentId = (this.ParentId == value.ParentId) ? this.ParentId : -1L;
            ss.SkippedParent = this.SkippedParent;
            ss.InheritRotation = this.InheritRotation;
            Vector3D.Lerp(ref this.Position, ref value.Position, (double) factor, out ss.Position);
            Vector3.Lerp(ref this.Pivot, ref value.Pivot, factor, out ss.Pivot);
            Quaternion.Slerp(ref this.Rotation, ref value.Rotation, factor, out ss.Rotation);
            Vector3.Lerp(ref this.LinearVelocity, ref value.LinearVelocity, factor, out ss.LinearVelocity);
            Vector3.Lerp(ref this.AngularVelocity, ref value.AngularVelocity, factor, out ss.AngularVelocity);
            ss.Rotation.Normalize();
        }

        public void Write(BitStream stream)
        {
            stream.WriteVariantSigned(this.ParentId);
            stream.WriteBool(this.Active);
            stream.WriteBool(this.InheritRotation);
            stream.Write(this.Position);
            stream.Write(this.Pivot);
            stream.WriteQuaternion(this.Rotation);
            if (this.Active)
            {
                stream.Write(this.LinearVelocity);
                stream.Write(this.AngularVelocity);
            }
        }

        private void Read(BitStream stream)
        {
            this.ParentId = stream.ReadInt64Variant();
            this.Active = stream.ReadBool();
            this.InheritRotation = stream.ReadBool();
            this.Position = stream.ReadVector3D();
            this.Pivot = stream.ReadVector3();
            this.Rotation = stream.ReadQuaternion();
            if (this.Active)
            {
                this.LinearVelocity = stream.ReadVector3();
                this.AngularVelocity = stream.ReadVector3();
            }
            else
            {
                this.LinearVelocity = Vector3.Zero;
                this.AngularVelocity = Vector3.Zero;
            }
        }

        public override string ToString() => 
            (" pos " + this.Position.ToString("N3") + " linVel " + this.LinearVelocity.ToString("N3"));

        public bool SanityCheck() => 
            ((this.Position.LengthSquared() < 250000.0) && ((this.AngularVelocity.LengthSquared() < 250000f) && (this.LinearVelocity.LengthSquared() < 160000f)));

        static MySnapshot()
        {
            ApplyReset = true;
        }
    }
}

