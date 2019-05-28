namespace VRage.Game.Components
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;
    using VRageRender;

    public abstract class MyRenderComponentBase : MyEntityComponentBase
    {
        public static uint[] UNINITIALIZED_IDs = new uint[] { uint.MaxValue };
        public static readonly Vector3 OldRedToHSV = new Vector3(0f, 0f, 0.05f);
        public static readonly Vector3 OldYellowToHSV = new Vector3(0.1222222f, -0.1f, 0.26f);
        public static readonly Vector3 OldBlueToHSV = new Vector3(0.575f, 0f, 0f);
        public static readonly Vector3 OldGreenToHSV = new Vector3(0.3333333f, -0.48f, -0.25f);
        public static readonly Vector3 OldBlackToHSV = new Vector3(0f, -0.96f, -0.5f);
        public static readonly Vector3 OldWhiteToHSV = new Vector3(0f, -0.95f, 0.4f);
        public static readonly Vector3 OldGrayToHSV = new Vector3(0f, -1f, 0f);
        protected bool m_enableColorMaskHsv;
        protected Vector3 m_colorMaskHsv = OldGrayToHSV;
        protected Dictionary<string, MyTextureChange> m_textureChanges;
        protected Color m_diffuseColor = Color.White;
        public int LastMomentUpdateIndex = -1;
        public Action NeedForDrawFromParentChanged;
        public bool FadeIn;
        public bool FadeOut;
        protected uint[] m_parentIDs = UNINITIALIZED_IDs;
        protected uint[] m_renderObjectIDs = UNINITIALIZED_IDs;
        public float Transparency;
        public byte DepthBias;
        private bool m_visibilityUpdates;

        protected MyRenderComponentBase()
        {
        }

        public abstract void AddRenderObjects();
        protected virtual bool CanBeAddedToRender() => 
            true;

        public abstract void Draw();
        public Color GetDiffuseColor() => 
            this.m_diffuseColor;

        public virtual CullingOptions GetRenderCullingOptions() => 
            CullingOptions.Default;

        public virtual RenderFlags GetRenderFlags()
        {
            RenderFlags flags = 0;
            if (this.NearFlag)
            {
                flags |= RenderFlags.Near;
            }
            if (this.CastShadows)
            {
                flags |= RenderFlags.CastShadowsOnLow | RenderFlags.CastShadows;
            }
            if (this.Visible)
            {
                flags |= RenderFlags.Visible;
            }
            if (this.NeedsResolveCastShadow)
            {
                flags |= RenderFlags.NeedsResolveCastShadow;
            }
            if (this.FastCastShadowResolve)
            {
                flags |= RenderFlags.FastCastShadowResolve;
            }
            if (this.SkipIfTooSmall)
            {
                flags |= RenderFlags.SkipIfTooSmall;
            }
            if (this.DrawOutsideViewDistance)
            {
                flags |= RenderFlags.DrawOutsideViewDistance;
            }
            if (this.ShadowBoxLod)
            {
                flags |= RenderFlags.ShadowLodBox;
            }
            if (this.DrawInAllCascades)
            {
                flags |= RenderFlags.DrawInAllCascades;
            }
            if (this.MetalnessColorable)
            {
                flags |= RenderFlags.MetalnessColorable;
            }
            return flags;
        }

        public uint GetRenderObjectID() => 
            ((this.m_renderObjectIDs.Length == 0) ? uint.MaxValue : this.m_renderObjectIDs[0]);

        public virtual void InvalidateRenderObjects()
        {
            if (((base.Container.Entity.Visible || base.Container.Entity.CastShadows) && base.Container.Entity.InScene) && base.Container.Entity.InvalidateOnMove)
            {
                MatrixD worldMatrix = base.Container.Entity.PositionComp.WorldMatrix;
                for (int i = 0; i < this.m_renderObjectIDs.Length; i++)
                {
                    if (this.RenderObjectIDs[i] != uint.MaxValue)
                    {
                        BoundingBox? aabb = null;
                        Matrix? localMatrix = null;
                        MyRenderProxy.UpdateRenderObject(this.RenderObjectIDs[i], new MatrixD?(worldMatrix), aabb, this.LastMomentUpdateIndex, localMatrix);
                    }
                }
            }
        }

        public bool IsChild(int index) => 
            (this.m_parentIDs[index] != uint.MaxValue);

        public bool IsRenderObjectAssigned(int index) => 
            (this.m_renderObjectIDs[index] != uint.MaxValue);

        public abstract bool IsVisible();
        protected void PropagateVisibilityUpdates(bool always = false)
        {
            if (always || this.m_visibilityUpdates)
            {
                foreach (uint num2 in this.m_renderObjectIDs)
                {
                    if (num2 != uint.MaxValue)
                    {
                        MyRenderProxy.SetVisibilityUpdates(num2, this.m_visibilityUpdates);
                    }
                }
            }
        }

        public abstract void ReleaseRenderObjectID(int index);
        public virtual void RemoveRenderObjects()
        {
            for (int i = 0; i < this.m_renderObjectIDs.Length; i++)
            {
                this.ReleaseRenderObjectID(i);
            }
        }

        public void ResizeRenderObjectArray(int newSize)
        {
            Array.Resize<uint>(ref this.m_renderObjectIDs, newSize);
            Array.Resize<uint>(ref this.m_parentIDs, newSize);
            for (int i = this.m_renderObjectIDs.Length; i < newSize; i++)
            {
                this.m_renderObjectIDs[i] = uint.MaxValue;
                this.m_parentIDs[i] = uint.MaxValue;
            }
        }

        public void SetParent(int index, uint cellParentCullObject, Matrix? childToParent = new Matrix?())
        {
            if (this.m_parentIDs == UNINITIALIZED_IDs)
            {
                this.m_parentIDs = new uint[index + 1];
            }
            this.m_parentIDs[index] = cellParentCullObject;
            MyRenderProxy.SetParentCullObject(this.RenderObjectIDs[index], cellParentCullObject, childToParent);
        }

        public abstract void SetRenderObjectID(int index, uint ID);
        public void SetVisibilityUpdates(bool state)
        {
            this.m_visibilityUpdates = state;
            this.PropagateVisibilityUpdates(true);
        }

        public virtual void UpdateRenderEntity(Vector3 colorMaskHSV)
        {
            this.m_colorMaskHsv = colorMaskHSV;
            if (this.m_renderObjectIDs[0] != uint.MaxValue)
            {
                float? dithering = null;
                MyRenderProxy.UpdateRenderEntity(this.m_renderObjectIDs[0], new Color?(this.m_diffuseColor), new Vector3?(this.m_colorMaskHsv), dithering, false);
            }
        }

        public void UpdateRenderObject(bool visible, bool updateChildren = true)
        {
            if (!(!base.Container.Entity.InScene & visible))
            {
                MyHierarchyComponentBase base2 = base.Container.Get<MyHierarchyComponentBase>();
                if (!visible)
                {
                    if (this.m_renderObjectIDs[0] != uint.MaxValue)
                    {
                        this.UpdateRenderObjectVisibility(visible);
                    }
                    this.RemoveRenderObjects();
                }
                else if (((base2 != null) && (this.Visible && ((base2.Parent == null) || base2.Parent.Container.Entity.Visible))) && this.CanBeAddedToRender())
                {
                    if (!this.IsRenderObjectAssigned(0))
                    {
                        this.AddRenderObjects();
                    }
                    else
                    {
                        this.UpdateRenderObjectVisibility(visible);
                    }
                }
                if (updateChildren && (base2 != null))
                {
                    using (List<MyHierarchyComponentBase>.Enumerator enumerator = base2.Children.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyRenderComponentBase component = null;
                            if (enumerator.Current.Container.TryGet<MyRenderComponentBase>(out component))
                            {
                                component.UpdateRenderObject(visible, true);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateRenderObjectLocal(Matrix renderLocalMatrix)
        {
            if (base.Container.Entity.Visible || base.Container.Entity.CastShadows)
            {
                for (int i = 0; i < this.m_renderObjectIDs.Length; i++)
                {
                    if (this.RenderObjectIDs[i] != uint.MaxValue)
                    {
                        MatrixD? worldMatrix = null;
                        BoundingBox? aabb = null;
                        MyRenderProxy.UpdateRenderObject(this.RenderObjectIDs[i], worldMatrix, aabb, this.LastMomentUpdateIndex, new Matrix?(renderLocalMatrix));
                    }
                }
            }
        }

        protected virtual void UpdateRenderObjectVisibility(bool visible)
        {
            foreach (uint num2 in this.m_renderObjectIDs)
            {
                if (num2 != uint.MaxValue)
                {
                    MyRenderProxy.UpdateRenderObjectVisibility(num2, visible, base.Container.Entity.NearFlag);
                }
            }
        }

        private void UpdateRenderObjectVisibilityIncludingChildren(bool visible)
        {
            MyHierarchyComponentBase base2;
            this.UpdateRenderObjectVisibility(visible);
            if (base.Container.TryGet<MyHierarchyComponentBase>(out base2))
            {
                foreach (MyHierarchyComponentBase base3 in base2.Children)
                {
                    MyRenderComponentBase component = null;
                    if (base3.Container.Entity.InScene && base3.Container.TryGet<MyRenderComponentBase>(out component))
                    {
                        component.UpdateRenderObjectVisibilityIncludingChildren(visible);
                    }
                }
            }
        }

        public virtual void UpdateRenderTextureChanges(Dictionary<string, MyTextureChange> skinTextureChanges)
        {
            this.m_textureChanges = skinTextureChanges;
            if (this.m_renderObjectIDs[0] != uint.MaxValue)
            {
                MyRenderProxy.ChangeMaterialTexture(this.m_renderObjectIDs[0], this.m_textureChanges);
            }
        }

        public void UpdateTransparency()
        {
            if (this.m_renderObjectIDs[0] != uint.MaxValue)
            {
                Color? diffuseColor = null;
                Vector3? colorMaskHsv = null;
                MyRenderProxy.UpdateRenderEntity(this.m_renderObjectIDs[0], diffuseColor, colorMaskHsv, new float?(this.Transparency), false);
            }
        }

        public abstract object ModelStorage { get; set; }

        public bool EnableColorMaskHsv
        {
            get => 
                this.m_enableColorMaskHsv;
            set
            {
                this.m_enableColorMaskHsv = value;
                if (this.EnableColorMaskHsv)
                {
                    this.UpdateRenderEntity(this.m_colorMaskHsv);
                    base.Container.Entity.EnableColorMaskForSubparts(value);
                }
            }
        }

        public Vector3 ColorMaskHsv
        {
            get => 
                this.m_colorMaskHsv;
            set
            {
                this.m_colorMaskHsv = value;
                if (this.EnableColorMaskHsv)
                {
                    this.UpdateRenderEntity(this.m_colorMaskHsv);
                    base.Container.Entity.SetColorMaskForSubparts(value);
                }
            }
        }

        public Dictionary<string, MyTextureChange> TextureChanges
        {
            get => 
                this.m_textureChanges;
            set
            {
                this.m_textureChanges = value;
                if (this.EnableColorMaskHsv)
                {
                    this.UpdateRenderTextureChanges(value);
                    base.Container.Entity.SetTextureChangesForSubparts(value);
                }
            }
        }

        public MyPersistentEntityFlags2 PersistentFlags { get; set; }

        public uint[] ParentIDs =>
            this.m_parentIDs;

        public uint[] RenderObjectIDs =>
            this.m_renderObjectIDs;

        public bool Visible
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.Visible) != 0);
            set
            {
                base.Container.Entity.Flags = !value ? (base.Container.Entity.Flags & ~EntityFlags.Visible) : (base.Container.Entity.Flags | EntityFlags.Visible);
                if (base.Container.Entity.Flags != base.Container.Entity.Flags)
                {
                    this.UpdateRenderObjectVisibilityIncludingChildren(value);
                }
            }
        }

        public bool DrawInAllCascades { get; set; }

        public bool MetalnessColorable { get; set; }

        public virtual bool NearFlag
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.Near) != 0);
            set
            {
                MyHierarchyComponentBase base2;
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.Near;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.Near;
                }
                if (value != this.NearFlag)
                {
                    for (int i = 0; i < this.m_renderObjectIDs.Length; i++)
                    {
                        if (this.m_renderObjectIDs[i] != uint.MaxValue)
                        {
                            MyRenderProxy.UpdateRenderObjectVisibility(this.m_renderObjectIDs[i], this.Visible, this.NearFlag);
                        }
                    }
                }
                if (base.Container.TryGet<MyHierarchyComponentBase>(out base2))
                {
                    foreach (MyHierarchyComponentBase base3 in base2.Children)
                    {
                        MyRenderComponentBase component = null;
                        if (base3.Container.Entity.InScene && base3.Container.TryGet<MyRenderComponentBase>(out component))
                        {
                            component.NearFlag = value;
                        }
                    }
                }
            }
        }

        public bool NeedsDrawFromParent
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.NeedsDrawFromParent) != 0);
            set
            {
                if (value != this.NeedsDrawFromParent)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.NeedsDrawFromParent;
                    if (value)
                    {
                        IMyEntity entity2 = base.Container.Entity;
                        entity2.Flags |= EntityFlags.NeedsDrawFromParent;
                    }
                    if (this.NeedForDrawFromParentChanged != null)
                    {
                        this.NeedForDrawFromParentChanged();
                    }
                }
            }
        }

        public bool CastShadows
        {
            get => 
                ((this.PersistentFlags & MyPersistentEntityFlags2.CastShadows) != MyPersistentEntityFlags2.None);
            set
            {
                if (value)
                {
                    this.PersistentFlags |= MyPersistentEntityFlags2.CastShadows;
                }
                else
                {
                    this.PersistentFlags &= ~MyPersistentEntityFlags2.CastShadows;
                }
            }
        }

        public bool NeedsResolveCastShadow
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.NeedsResolveCastShadow) != 0);
            set
            {
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.NeedsResolveCastShadow;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.NeedsResolveCastShadow;
                }
            }
        }

        public bool FastCastShadowResolve
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.FastCastShadowResolve) != 0);
            set
            {
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.FastCastShadowResolve;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.FastCastShadowResolve;
                }
            }
        }

        public bool SkipIfTooSmall
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.SkipIfTooSmall) != 0);
            set
            {
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.SkipIfTooSmall;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.SkipIfTooSmall;
                }
            }
        }

        public bool DrawOutsideViewDistance
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.DrawOutsideViewDistance) != 0);
            set
            {
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.DrawOutsideViewDistance;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.DrawOutsideViewDistance;
                }
            }
        }

        public bool ShadowBoxLod
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.ShadowBoxLod) != 0);
            set
            {
                if (value)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags |= EntityFlags.ShadowBoxLod;
                }
                else
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.ShadowBoxLod;
                }
            }
        }

        public bool OffsetInVertexShader { get; set; }

        public virtual bool NeedsDraw
        {
            get => 
                ((base.Container.Entity.Flags & EntityFlags.NeedsDraw) != 0);
            set
            {
                if (value != this.NeedsDraw)
                {
                    IMyEntity entity = base.Container.Entity;
                    entity.Flags &= ~EntityFlags.NeedsDraw;
                    if (value)
                    {
                        IMyEntity entity2 = base.Container.Entity;
                        entity2.Flags |= EntityFlags.NeedsDraw;
                    }
                }
            }
        }

        public override string ComponentTypeDebugString =>
            "Render";
    }
}

