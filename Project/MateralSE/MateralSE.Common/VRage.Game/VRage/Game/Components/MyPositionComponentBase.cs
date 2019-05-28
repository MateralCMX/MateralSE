namespace VRage.Game.Components
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageMath.Spatial;

    public abstract class MyPositionComponentBase : MyEntityComponentBase
    {
        public static Action<IMyEntity> OnReportInvalidMatrix;
        protected MatrixD m_worldMatrix = MatrixD.Identity;
        public uint m_worldMatrixCounter;
        public uint m_lastParentWorldMatrixCounter;
        public bool m_worldMatrixDirty;
        protected Matrix m_localMatrix = Matrix.Identity;
        protected BoundingBox m_localAABB;
        protected BoundingSphere m_localVolume;
        protected Vector3 m_localVolumeOffset;
        protected BoundingBoxD m_worldAABB;
        protected BoundingSphereD m_worldVolume;
        protected bool m_worldVolumeDirty;
        protected bool m_worldAABBDirty;
        private float? m_scale;
        [CompilerGenerated]
        private Action<MyPositionComponentBase> OnPositionChanged;
        protected bool m_normalizedInvMatrixDirty = true;
        private MatrixD m_normalizedWorldMatrixInv;
        protected bool m_invScaledMatrixDirty = true;
        private MatrixD m_worldMatrixInvScaled;

        public event Action<MyPositionComponentBase> OnPositionChanged
        {
            [CompilerGenerated] add
            {
                Action<MyPositionComponentBase> onPositionChanged = this.OnPositionChanged;
                while (true)
                {
                    Action<MyPositionComponentBase> a = onPositionChanged;
                    Action<MyPositionComponentBase> action3 = (Action<MyPositionComponentBase>) Delegate.Combine(a, value);
                    onPositionChanged = Interlocked.CompareExchange<Action<MyPositionComponentBase>>(ref this.OnPositionChanged, action3, a);
                    if (ReferenceEquals(onPositionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPositionComponentBase> onPositionChanged = this.OnPositionChanged;
                while (true)
                {
                    Action<MyPositionComponentBase> source = onPositionChanged;
                    Action<MyPositionComponentBase> action3 = (Action<MyPositionComponentBase>) Delegate.Remove(source, value);
                    onPositionChanged = Interlocked.CompareExchange<Action<MyPositionComponentBase>>(ref this.OnPositionChanged, action3, source);
                    if (ReferenceEquals(onPositionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyPositionComponentBase()
        {
        }

        public MatrixD GetOrientation() => 
            this.WorldMatrix.GetOrientation();

        public Vector3D GetPosition() => 
            this.WorldMatrix.Translation;

        public virtual MatrixD GetViewMatrix() => 
            this.WorldMatrixNormalizedInv;

        protected virtual void OnWorldPositionChanged(object source, bool updateChildren = true, bool forceUpdateAllChildren = false)
        {
            this.m_worldVolumeDirty = true;
            this.m_worldAABBDirty = true;
            this.m_normalizedInvMatrixDirty = true;
            this.m_invScaledMatrixDirty = true;
            this.RaiseOnPositionChanged(this);
        }

        protected void RaiseOnPositionChanged(MyPositionComponentBase component)
        {
            this.OnPositionChanged.InvokeIfNotNull<MyPositionComponentBase>(component);
        }

        public void RecalculateWorldMatrixHR(bool updateChildren = false)
        {
            if (base.Entity.Parent != null)
            {
                base.Entity.Parent.PositionComp.RecalculateWorldMatrixHR(false);
                MatrixD worldMatrix = base.Entity.Parent.WorldMatrix;
                MatrixD other = this.m_worldMatrix;
                MatrixD.Multiply(ref this.m_localMatrix, ref worldMatrix, out this.m_worldMatrix);
                this.m_worldMatrixDirty = false;
                if (!this.m_worldMatrix.EqualsFast(ref other, 0.0001))
                {
                    this.m_lastParentWorldMatrixCounter = base.Entity.Parent.PositionComp.m_worldMatrixCounter;
                    this.m_worldMatrixCounter++;
                    this.m_worldVolumeDirty = true;
                    this.m_worldAABBDirty = true;
                    this.m_normalizedInvMatrixDirty = true;
                    this.m_invScaledMatrixDirty = true;
                }
            }
        }

        public bool SetLocalMatrix(ref Matrix localMatrix, object source, bool updateWorld)
        {
            bool flag1 = !this.m_localMatrix.EqualsFast(ref localMatrix, 0.0001f);
            if (flag1)
            {
                this.m_localMatrix = localMatrix;
                this.m_worldMatrixCounter++;
                this.m_worldMatrixDirty = true;
            }
            if (this.NeedsRecalculateWorldMatrix && updateWorld)
            {
                this.UpdateWorldMatrix(source, true, false);
            }
            return flag1;
        }

        public void SetLocalMatrix(ref Matrix localMatrix, object source, bool updateWorld, ref Matrix renderLocal, bool forceUpdateRender = false)
        {
            if (this.SetLocalMatrix(ref localMatrix, source, updateWorld) | forceUpdateRender)
            {
                MyRenderComponentBase render = base.Entity.Render;
                if (render != null)
                {
                    render.UpdateRenderObjectLocal(renderLocal);
                }
            }
        }

        public void SetPosition(Vector3D pos, object source = null, bool forceUpdate = false, bool updateChildren = true)
        {
            if (!MyUtils.IsZero(this.m_worldMatrix.Translation - pos, 1E-05f))
            {
                this.SetWorldMatrix(MatrixD.CreateWorld(pos, this.m_worldMatrix.Forward, this.m_worldMatrix.Up), source, forceUpdate, updateChildren, true, false, false, false);
            }
        }

        public virtual void SetWorldMatrix(MatrixD worldMatrix, object source = null, bool forceUpdate = false, bool updateChildren = true, bool updateLocal = true, bool skipTeleportCheck = false, bool forceUpdateAllChildren = false, bool ignoreAssert = false)
        {
            if ((OnReportInvalidMatrix != null) && !worldMatrix.IsValid())
            {
                OnReportInvalidMatrix(base.Entity);
            }
            else if ((!skipTeleportCheck && base.Entity.InScene) && (Vector3D.DistanceSquared(worldMatrix.Translation, this.WorldMatrix.Translation) > MyClusterTree.IdealClusterSizeHalfSqr.X))
            {
                base.Entity.Teleport(worldMatrix, source, ignoreAssert);
            }
            else if ((base.Entity.Parent == null) || (source == base.Entity.Parent))
            {
                if (this.Scale != null)
                {
                    MyUtils.Normalize(ref worldMatrix, out worldMatrix);
                    worldMatrix = MatrixD.CreateScale((double) this.Scale.Value) * worldMatrix;
                }
                if (forceUpdate || !this.m_worldMatrix.EqualsFast(ref worldMatrix, 1E-06))
                {
                    if (base.Container.Entity.Parent == null)
                    {
                        this.m_worldMatrix = worldMatrix;
                        this.m_localMatrix = (Matrix) worldMatrix;
                    }
                    else if (updateLocal)
                    {
                        MatrixD worldMatrixInvScaled = base.Container.Entity.Parent.PositionComp.WorldMatrixInvScaled;
                        this.m_localMatrix = (Matrix) (worldMatrix * worldMatrixInvScaled);
                    }
                    this.m_worldMatrixCounter++;
                    this.UpdateWorldMatrix(source, updateChildren, forceUpdateAllChildren);
                }
            }
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "worldpos=", this.GetPosition(), ", worldmat=", this.WorldMatrix };
            return string.Concat(objArray1);
        }

        protected virtual void UpdateWorldMatrix(object source = null, bool updateChildren = true, bool forceUpdateAllChildren = false)
        {
            if (base.Container.Entity.Parent == null)
            {
                this.OnWorldPositionChanged(source, updateChildren, forceUpdateAllChildren);
            }
            else
            {
                MatrixD worldMatrix = base.Container.Entity.Parent.WorldMatrix;
                this.UpdateWorldMatrix(ref worldMatrix, source, updateChildren, forceUpdateAllChildren);
            }
        }

        public virtual void UpdateWorldMatrix(ref MatrixD parentWorldMatrix, object source = null, bool updateChildren = true, bool forceUpdateAllChildren = false)
        {
            if (base.Entity.Parent != null)
            {
                MatrixD.Multiply(ref this.m_localMatrix, ref parentWorldMatrix, out this.m_worldMatrix);
                this.m_lastParentWorldMatrixCounter = base.Entity.Parent.PositionComp.m_worldMatrixCounter;
                this.m_worldMatrixCounter++;
                this.OnWorldPositionChanged(source, updateChildren, forceUpdateAllChildren);
            }
        }

        public MatrixD WorldMatrix
        {
            get
            {
                if (this.NeedsRecalculateWorldMatrix)
                {
                    this.RecalculateWorldMatrixHR(false);
                }
                return this.m_worldMatrix;
            }
            set => 
                this.SetWorldMatrix(value, null, false, true, true, false, false, false);
        }

        public Matrix LocalMatrix
        {
            get => 
                this.m_localMatrix;
            set => 
                this.SetLocalMatrix(ref value, null, true);
        }

        public BoundingBoxD WorldAABB
        {
            get
            {
                if (this.m_worldAABBDirty || this.NeedsRecalculateWorldMatrix)
                {
                    MatrixD worldMatrix = this.WorldMatrix;
                    this.m_localAABB.Transform(ref worldMatrix, ref this.m_worldAABB);
                    this.m_worldAABBDirty = false;
                }
                return this.m_worldAABB;
            }
            set
            {
                this.m_worldAABB = value;
                Vector3 result = (Vector3) (value.Center - this.WorldMatrix.Translation);
                MatrixD worldMatrixInvScaled = this.WorldMatrixInvScaled;
                Vector3* vectorPtr1 = (Vector3*) ref result;
                Vector3.TransformNormal(ref (Vector3) ref vectorPtr1, ref worldMatrixInvScaled, out result);
                this.LocalAABB = new BoundingBox(result - value.HalfExtents, result + value.HalfExtents);
                this.m_worldAABBDirty = false;
            }
        }

        public BoundingSphereD WorldVolume
        {
            get
            {
                if (this.m_worldVolumeDirty || this.NeedsRecalculateWorldMatrix)
                {
                    MatrixD worldMatrix = this.WorldMatrix;
                    this.m_worldVolume.Center = Vector3D.Transform(this.m_localVolume.Center, ref worldMatrix);
                    this.m_worldVolume.Radius = this.m_localVolume.Radius;
                    this.m_worldVolumeDirty = false;
                }
                return this.m_worldVolume;
            }
            set
            {
                this.m_worldVolume = value;
                Vector3 result = (Vector3) (value.Center - this.WorldMatrix.Translation);
                MatrixD worldMatrixInvScaled = this.WorldMatrixInvScaled;
                Vector3* vectorPtr1 = (Vector3*) ref result;
                Vector3.TransformNormal(ref (Vector3) ref vectorPtr1, ref worldMatrixInvScaled, out result);
                this.LocalVolume = new BoundingSphere(result, (float) value.Radius);
                this.m_worldVolumeDirty = false;
            }
        }

        public virtual BoundingBox LocalAABB
        {
            get => 
                this.m_localAABB;
            set
            {
                this.m_localAABB = value;
                this.m_localVolume = BoundingSphere.CreateFromBoundingBox(this.m_localAABB);
                this.m_worldVolumeDirty = true;
                this.m_worldAABBDirty = true;
            }
        }

        public BoundingSphere LocalVolume
        {
            get => 
                this.m_localVolume;
            set
            {
                this.m_localVolume = value;
                this.m_localAABB = MyMath.CreateFromInsideRadius(value.Radius);
                this.m_localAABB = this.m_localAABB.Translate(value.Center);
                this.m_worldVolumeDirty = true;
                this.m_worldAABBDirty = true;
            }
        }

        public Vector3 LocalVolumeOffset
        {
            get => 
                this.m_localVolumeOffset;
            set
            {
                this.m_localVolumeOffset = value;
                this.m_worldVolumeDirty = true;
            }
        }

        protected virtual bool ShouldSync =>
            (base.Container.Get<MySyncComponentBase>() != null);

        public float? Scale
        {
            get => 
                this.m_scale;
            set
            {
                float? scale = this.m_scale;
                float? nullable2 = value;
                if (!((scale.GetValueOrDefault() == nullable2.GetValueOrDefault()) & ((scale != null) == (nullable2 != null))))
                {
                    this.m_scale = value;
                    Matrix localMatrix = this.LocalMatrix;
                    if (this.m_scale == null)
                    {
                        Matrix* matrixPtr2 = (Matrix*) ref localMatrix;
                        MyUtils.Normalize(ref (Matrix) ref matrixPtr2, out localMatrix);
                        this.LocalMatrix = localMatrix;
                    }
                    else
                    {
                        MatrixD worldMatrix = this.WorldMatrix;
                        if (base.Container.Entity.Parent == null)
                        {
                            MatrixD* xdPtr1 = (MatrixD*) ref worldMatrix;
                            MyUtils.Normalize(ref (MatrixD) ref xdPtr1, out worldMatrix);
                            this.WorldMatrix = Matrix.CreateScale(this.m_scale.Value) * worldMatrix;
                        }
                        else
                        {
                            Matrix* matrixPtr1 = (Matrix*) ref localMatrix;
                            MyUtils.Normalize(ref (Matrix) ref matrixPtr1, out localMatrix);
                            this.LocalMatrix = Matrix.CreateScale(this.m_scale.Value) * localMatrix;
                        }
                    }
                    this.UpdateWorldMatrix(null, true, false);
                }
            }
        }

        public bool NeedsRecalculateWorldMatrix
        {
            get
            {
                if (this.m_worldMatrixDirty)
                {
                    return true;
                }
                if (base.Entity != null)
                {
                    IMyEntity parent = base.Entity.Parent;
                    uint lastParentWorldMatrixCounter = this.m_lastParentWorldMatrixCounter;
                    while (parent != null)
                    {
                        if (lastParentWorldMatrixCounter < parent.PositionComp.m_worldMatrixCounter)
                        {
                            return true;
                        }
                        lastParentWorldMatrixCounter = parent.PositionComp.m_lastParentWorldMatrixCounter;
                        parent = parent.Parent;
                    }
                }
                return false;
            }
        }

        public MatrixD WorldMatrixNormalizedInv
        {
            get
            {
                if (this.m_normalizedInvMatrixDirty || this.NeedsRecalculateWorldMatrix)
                {
                    MatrixD worldMatrix = this.WorldMatrix;
                    if (!MyUtils.IsZero((double) (worldMatrix.Left.LengthSquared() - 1.0), 1E-05f))
                    {
                        MatrixD.Invert(ref MatrixD.Normalize(worldMatrix), out this.m_normalizedWorldMatrixInv);
                    }
                    else
                    {
                        MatrixD.Invert(ref worldMatrix, out this.m_normalizedWorldMatrixInv);
                    }
                    this.m_normalizedInvMatrixDirty = false;
                    if (this.Scale == null)
                    {
                        this.m_worldMatrixInvScaled = this.m_normalizedWorldMatrixInv;
                        this.m_invScaledMatrixDirty = false;
                    }
                }
                return this.m_normalizedWorldMatrixInv;
            }
            private set => 
                (this.m_normalizedWorldMatrixInv = value);
        }

        public MatrixD WorldMatrixInvScaled
        {
            get
            {
                if (this.m_invScaledMatrixDirty || this.NeedsRecalculateWorldMatrix)
                {
                    MatrixD worldMatrix = this.WorldMatrix;
                    if (!MyUtils.IsZero((double) (worldMatrix.Left.LengthSquared() - 1.0), 1E-05f))
                    {
                        worldMatrix = MatrixD.Normalize(worldMatrix);
                    }
                    if (this.Scale != null)
                    {
                        worldMatrix *= Matrix.CreateScale(this.Scale.Value);
                    }
                    MatrixD.Invert(ref worldMatrix, out this.m_worldMatrixInvScaled);
                    this.m_invScaledMatrixDirty = false;
                    if (this.Scale == null)
                    {
                        this.m_normalizedWorldMatrixInv = this.m_worldMatrixInvScaled;
                        this.m_normalizedInvMatrixDirty = false;
                    }
                }
                return this.m_worldMatrixInvScaled;
            }
            private set => 
                (this.m_worldMatrixInvScaled = value);
        }

        public override string ComponentTypeDebugString =>
            "Position";
    }
}

