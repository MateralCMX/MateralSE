namespace VRage.Render.Scene
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Render.Scene.Components;
    using VRage.Render11.Common;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Messages;

    public class MyActor
    {
        public bool EnableAabbUpdateBasedOnChildren;
        private int m_perFrameUpdateCounter;
        public readonly bool[] OccludedState;
        public readonly long[] FrameInView;
        [CompilerGenerated]
        private Action OnMove;
        private BoundingBoxD m_aabb;
        private MyIDTracker<MyActor> m_ID;
        private bool m_visible;
        private readonly MyIndexedComponentContainer<MyActorComponent> m_components;
        private bool m_relativeTransformValid;
        private Matrix m_relativeTransform;
        private BoundingBox? m_localAabb;
        public MatrixD LastWorldMatrix;
        private int m_worldMatrixIndex;
        private volatile bool m_worldMatrixDirty;
        private MatrixD m_worldMatrixInv;
        private bool m_worldMatrixInvDirty;
        private bool m_dirtyProxy;
        private bool m_root;
        private int m_cullTreeId;
        private MyManualCullTreeData m_cullTreeData;
        private int m_childTreeId;
        private int m_globalTreeId;
        private int m_globalFarTreeId;
        private List<MyActor> m_children;
        private bool m_visibilityUpdates;
        private float m_relativeForwardScale;
        [CompilerGenerated]
        private Action<MyActor> OnDestruct;

        public event Action<MyActor> OnDestruct
        {
            [CompilerGenerated] add
            {
                Action<MyActor> onDestruct = this.OnDestruct;
                while (true)
                {
                    Action<MyActor> a = onDestruct;
                    Action<MyActor> action3 = (Action<MyActor>) Delegate.Combine(a, value);
                    onDestruct = Interlocked.CompareExchange<Action<MyActor>>(ref this.OnDestruct, action3, a);
                    if (ReferenceEquals(onDestruct, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyActor> onDestruct = this.OnDestruct;
                while (true)
                {
                    Action<MyActor> source = onDestruct;
                    Action<MyActor> action3 = (Action<MyActor>) Delegate.Remove(source, value);
                    onDestruct = Interlocked.CompareExchange<Action<MyActor>>(ref this.OnDestruct, action3, source);
                    if (ReferenceEquals(onDestruct, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action OnMove
        {
            [CompilerGenerated] add
            {
                Action onMove = this.OnMove;
                while (true)
                {
                    Action a = onMove;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onMove = Interlocked.CompareExchange<Action>(ref this.OnMove, action3, a);
                    if (ReferenceEquals(onMove, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onMove = this.OnMove;
                while (true)
                {
                    Action source = onMove;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onMove = Interlocked.CompareExchange<Action>(ref this.OnMove, action3, source);
                    if (ReferenceEquals(onMove, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyActor()
        {
            this.OccludedState = new bool[0x13];
            this.FrameInView = new long[0x13];
            this.m_components = new MyIndexedComponentContainer<MyActorComponent>();
            throw new Exception("Invalid constructor");
        }

        public MyActor(MyScene scene)
        {
            this.OccludedState = new bool[0x13];
            this.FrameInView = new long[0x13];
            this.m_components = new MyIndexedComponentContainer<MyActorComponent>();
            this.Scene = scene;
        }

        private void Add(MyActor child)
        {
            child.Parent = this;
            this.m_children.Add(child);
            if (!child.m_relativeTransformValid)
            {
                Matrix m = (Matrix) (child.WorldMatrix * this.WorldMatrixInv);
                child.SetRelativeTransform(ref m);
            }
            child.OnParentSet();
            child.InvalidateCullTreeData();
        }

        public void AddComponent<T>(MyActorComponent component) where T: MyActorComponent
        {
            this.AddComponent(typeof(T), component);
        }

        public void AddComponent(Type t, MyActorComponent component)
        {
            component.Assign(this);
            this.m_components.Add(t, component);
        }

        public void AddLocalAabb(BoundingBox localAabb)
        {
            if (this.m_localAabb != null)
            {
                this.m_localAabb = new BoundingBox?(this.m_localAabb.Value.Include(localAabb));
            }
            else
            {
                this.m_localAabb = new BoundingBox?(localAabb);
            }
            this.UpdateVolumeExtent();
            this.OnMatrixChange();
        }

        private void AddProxy(MyChildCullTreeData data)
        {
            bool flag = (this.m_globalTreeId == -1) && (this.m_globalFarTreeId == -1);
            if (this.Parent != null)
            {
                if (!flag)
                {
                    this.RemoveProxy();
                }
                if (this.m_childTreeId == -1)
                {
                    BoundingBox aabb = this.m_localAabb.Value.Transform(this.m_relativeTransform);
                    this.m_aabb = aabb;
                    this.m_childTreeId = this.Parent.AddProxy(ref aabb, data, 0);
                }
            }
            else if (flag)
            {
                this.m_aabb = this.m_localAabb.Value.Transform(this.WorldMatrix);
                if (data.FarCull)
                {
                    this.m_globalFarTreeId = this.Scene.DynamicRenderablesFarDBVH.AddProxy(ref this.m_aabb, data.Add, 0, true);
                }
                else
                {
                    this.m_globalTreeId = this.Scene.DynamicRenderablesDBVH.AddProxy(ref this.m_aabb, data.Add, 0, true);
                }
                this.UpdateVolumeExtent();
            }
        }

        private int AddProxy(ref BoundingBox aabb, MyChildCullTreeData userData, uint flags)
        {
            MyChildCullTreeData data = this.Scene.CompileCullData(userData);
            data.Add(this.m_cullTreeData.All, true);
            int key = this.m_cullTreeData.Children.AddProxy(ref aabb, data.Add, flags, true);
            MyBruteCullData data2 = new MyBruteCullData {
                Aabb = new MyCullAABB(aabb),
                UserData = data
            };
            this.m_cullTreeData.BruteCull.Add(key, data2);
            for (int i = 0; i < this.m_cullTreeData.RenderCullData.Length; i++)
            {
                this.m_cullTreeData.RenderCullData[i].CulledActors.Add(data2);
            }
            this.MoveProxy();
            return key;
        }

        public BoundingBoxD CalculateAabb() => 
            this.m_localAabb.Value.Transform(this.WorldMatrix);

        public float CalculateCameraDistanceSquared()
        {
            if (this.Parent == null)
            {
                return (float) this.m_aabb.DistanceSquared(this.Scene.Environment.CameraPosition);
            }
            return this.m_localAabb.Value.DistanceSquared((Vector3) Vector3D.Transform(this.Scene.Environment.CameraPosition, this.WorldMatrixInv));
        }

        public float CalculateCameraDistanceSquaredFast() => 
            ((float) (this.WorldMatrix.Translation - this.Scene.Environment.CameraPosition).LengthSquared());

        public void Construct(string debugName)
        {
            this.m_components.Clear();
            this.m_cullTreeId = -1;
            this.m_childTreeId = -1;
            this.m_children = new List<MyActor>();
            this.m_dirtyProxy = false;
            this.m_root = false;
            this.m_globalTreeId = -1;
            this.m_globalFarTreeId = -1;
            this.m_visible = true;
            this.m_visibilityUpdates = false;
            this.DebugName = debugName;
            MyUtils.Init<MyIDTracker<MyActor>>(ref this.m_ID);
            this.m_ID.Clear();
            this.m_localAabb = null;
            this.WorldMatrix = MatrixD.Identity;
            this.m_relativeTransformValid = false;
        }

        public void Destroy(bool fadeOut = false)
        {
            bool flag = true;
            if (fadeOut && this.m_visible)
            {
                for (int i = 0; i < this.m_components.Count; i++)
                {
                    flag &= this.m_components[i].StartFadeOut();
                }
            }
            if (flag)
            {
                this.Destruct();
            }
            else
            {
                this.Scene.Updater.DestroyIn(this, MyTimeSpan.FromSeconds((double) this.Scene.FadeOutTime));
            }
        }

        public void Destruct()
        {
            if (this.m_ID != null)
            {
                this.SetParent(null);
                if (this.OnDestruct != null)
                {
                    this.OnDestruct(this);
                }
                this.OnDestruct = null;
                while (this.m_children.Count > 0)
                {
                    this.m_children[0].SetParent(null);
                }
                this.m_children = null;
                for (int i = 0; i < this.m_components.Count; i++)
                {
                    this.m_components[i].OnRemove(this);
                }
                this.m_components.Clear();
                this.RemoveProxy();
                if (this.m_cullTreeData != null)
                {
                    this.Scene.FreeGroupData(this.m_cullTreeData);
                    this.m_cullTreeData = null;
                }
                if (this.m_cullTreeId != -1)
                {
                    this.Scene.ManualCullTree.RemoveProxy(this.m_cullTreeId);
                    this.m_cullTreeId = -1;
                }
                if (this.m_ID.Value != null)
                {
                    this.m_ID.Deregister();
                }
                this.m_ID = null;
                this.OnMove = null;
                for (int j = 0; j < this.OccludedState.Length; j++)
                {
                    this.OccludedState[j] = false;
                }
                this.Scene.Updater.RemoveFromUpdates(this);
            }
        }

        public T GetComponent<T>() where T: MyActorComponent => 
            this.m_components.TryGetComponent<T>();

        public Color GetDebugColor()
        {
            Color magenta = Color.Magenta;
            for (int i = 0; i < this.m_components.Count; i++)
            {
                magenta = this.m_components[i].DebugColor;
                if (magenta != Color.Magenta)
                {
                    return ((this.Parent != null) ? magenta : Vector4.Lerp(magenta.ToVector4().ToLinearRGB(), (Vector4) Color.Magenta, 0.5f).ToSRGB());
                }
            }
            return magenta;
        }

        public void InvalidateCullTreeData()
        {
            this.RemoveProxy();
            if (this.m_visible)
            {
                bool flag = false;
                MyChildCullTreeData data = null;
                int num = 0;
                while (true)
                {
                    if (num >= this.m_components.Count)
                    {
                        if (data != null)
                        {
                            this.AddProxy(data);
                        }
                        break;
                    }
                    MyChildCullTreeData cullTreeData = this.m_components[num].GetCullTreeData();
                    if (cullTreeData != null)
                    {
                        if (data == null)
                        {
                            data = cullTreeData;
                        }
                        else
                        {
                            if (!flag)
                            {
                                flag = true;
                                MyChildCullTreeData data1 = new MyChildCullTreeData();
                                data1.Add = data.Add;
                                data1.Remove = data.Remove;
                                data1.DebugColor = data.DebugColor;
                                data = data1;
                            }
                            data.FarCull |= cullTreeData.FarCull;
                            data.Add = (Action<MyCullResultsBase, bool>) Delegate.Combine(data.Add, cullTreeData.Add);
                            data.Remove = (Action<MyCullResultsBase>) Delegate.Combine(data.Remove, cullTreeData.Remove);
                            data.DebugColor = (Func<Color>) Delegate.Combine(data.DebugColor, cullTreeData.DebugColor);
                        }
                    }
                    num++;
                }
            }
        }

        public bool IsOccluded(int viewId) => 
            ((this.FrameInView[viewId] == (MyScene.FrameCounter - 1L)) && this.OccludedState[viewId]);

        public bool IsRoot() => 
            this.m_root;

        private void MoveProxy()
        {
            if (this.IsVisible)
            {
                if (this.m_root)
                {
                    this.Scene.Updater.AddToNextUpdate(this);
                    this.m_dirtyProxy = true;
                }
                else if (this.Parent != null)
                {
                    if (this.m_childTreeId != -1)
                    {
                        BoundingBox aabb = this.m_localAabb.Value.Transform(this.m_relativeTransform);
                        this.Parent.MoveProxy(this.m_childTreeId, ref aabb);
                        this.m_aabb = aabb;
                    }
                }
                else if (this.m_globalTreeId != -1)
                {
                    this.m_aabb = this.m_localAabb.Value.Transform(this.WorldMatrix);
                    this.Scene.DynamicRenderablesDBVH.MoveProxy(this.m_globalTreeId, ref this.m_aabb, Vector3.Zero);
                }
                else if (this.m_globalFarTreeId != -1)
                {
                    this.m_aabb = this.m_localAabb.Value.Transform(this.WorldMatrix);
                    this.Scene.DynamicRenderablesFarDBVH.MoveProxy(this.m_globalFarTreeId, ref this.m_aabb, Vector3.Zero);
                }
            }
        }

        private void MoveProxy(int id, ref BoundingBox aabb)
        {
            this.m_cullTreeData.Children.MoveProxy(id, ref aabb, Vector3.Zero);
            MyBruteCullData data = this.m_cullTreeData.BruteCull[id];
            data.Aabb.Reset(ref aabb);
            this.m_cullTreeData.BruteCull[id] = data;
            this.Scene.Updater.AddToNextUpdate(this);
            this.m_dirtyProxy = true;
        }

        private void OnMatrixChange()
        {
            if (this.m_root)
            {
                using (List<MyActor>.Enumerator enumerator = this.m_children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.SetWorldMatrixDirty();
                    }
                    goto TR_0000;
                }
            }
            if (this.Parent != null)
            {
                this.SetWorldMatrixDirty();
            }
        TR_0000:
            this.OnMove.InvokeIfNotNull();
            this.MoveProxy();
        }

        private void OnParentRemoved()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].OnParentRemoved();
            }
        }

        private void OnParentSet()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].OnParentSet();
            }
        }

        private void OnUpdateBeforeDraw()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                MyActorComponent component = this.m_components[i];
                if (!component.NeedsPerFrameUpdate)
                {
                    component.OnUpdateBeforeDraw();
                }
            }
        }

        private void Remove(MyActor child)
        {
            child.OnParentRemoved();
            child.RemoveProxy();
            this.m_children.Remove(child);
        }

        public void RemoveComponent<T>(MyActorComponent component) where T: MyActorComponent
        {
            this.RemoveComponent(typeof(T), component);
        }

        public void RemoveComponent(Type t, MyActorComponent component)
        {
            this.RemoveProxy();
            component.OnRemove(this);
            this.m_components.Remove(t);
            this.InvalidateCullTreeData();
        }

        private void RemoveProxy()
        {
            if (this.Parent != null)
            {
                if (this.m_childTreeId != -1)
                {
                    this.Parent.RemoveProxy(this.m_childTreeId);
                }
                this.m_childTreeId = -1;
            }
            if (this.m_globalTreeId != -1)
            {
                this.Scene.DynamicRenderablesDBVH.RemoveProxy(this.m_globalTreeId);
                this.m_globalTreeId = -1;
            }
            if (this.m_globalFarTreeId != -1)
            {
                this.Scene.DynamicRenderablesFarDBVH.RemoveProxy(this.m_globalFarTreeId);
                this.m_globalFarTreeId = -1;
            }
        }

        private void RemoveProxy(int id)
        {
            MyChildCullTreeData userData = this.m_cullTreeData.BruteCull[id].UserData;
            userData.Remove(this.m_cullTreeData.All);
            this.m_cullTreeData.Children.RemoveProxy(id);
            this.m_cullTreeData.BruteCull.Remove(id);
            Predicate<MyBruteCullData> match = x => ReferenceEquals(x.UserData, userData);
            for (int i = 0; i < this.m_cullTreeData.RenderCullData.Length; i++)
            {
                int index = this.m_cullTreeData.RenderCullData[i].CulledActors.FindIndex(match);
                if (index >= 0)
                {
                    this.m_cullTreeData.RenderCullData[i].CulledActors.RemoveAtFast<MyBruteCullData>(index);
                }
                else
                {
                    index = this.m_cullTreeData.RenderCullData[i].ActiveActors.FindIndex(match);
                    this.m_cullTreeData.RenderCullData[i].ActiveActors.RemoveAtFast<MyBruteCullData>(index);
                    userData.Remove(this.m_cullTreeData.RenderCullData[i].ActiveResults);
                }
            }
            this.MoveProxy();
        }

        public void SetID(uint id)
        {
            this.m_ID.Register(id, this);
        }

        public void SetLocalAabb(BoundingBox localAabb)
        {
            this.m_localAabb = new BoundingBox?(localAabb);
            this.UpdateVolumeExtent();
            this.OnMatrixChange();
        }

        public void SetMatrix(ref MatrixD matrix)
        {
            this.WorldMatrix = matrix;
            this.OnMatrixChange();
        }

        public void SetParent(MyActor parent)
        {
            if (!ReferenceEquals(parent, this.Parent))
            {
                if (this.Parent != null)
                {
                    this.Parent.Remove(this);
                    this.m_worldMatrixDirty = false;
                }
                this.Parent = parent;
                if (this.Parent != null)
                {
                    this.Parent.Add(this);
                }
            }
        }

        public void SetRelativeTransform(ref Matrix m)
        {
            this.m_relativeTransform = m;
            this.m_relativeForwardScale = this.m_relativeTransform.Forward.Length();
            this.m_relativeTransformValid = true;
            this.OnMatrixChange();
        }

        public void SetRoot(bool state)
        {
            if (!this.m_root)
            {
                this.m_root = state;
                this.m_cullTreeData = this.Scene.AllocateGroupData();
                this.m_cullTreeData.Actor = this;
            }
        }

        public void SetTransforms(MyRenderObjectUpdateData data)
        {
            if (data.LocalMatrix != null)
            {
                this.m_relativeTransform = data.LocalMatrix.Value;
                this.m_relativeForwardScale = this.m_relativeTransform.Forward.Length();
                this.m_relativeTransformValid = true;
            }
            else if (data.WorldMatrix != null)
            {
                this.WorldMatrix = data.WorldMatrix.Value;
            }
            if (data.LocalAABB != null)
            {
                this.m_localAabb = new BoundingBox?(data.LocalAABB.Value);
                this.UpdateVolumeExtent();
            }
            this.OnMatrixChange();
        }

        public void SetVisibility(bool visibility)
        {
            if (this.m_visible != visibility)
            {
                this.m_visible = visibility;
                int num = 0;
                while (true)
                {
                    if (num >= this.m_components.Count)
                    {
                        this.InvalidateCullTreeData();
                        break;
                    }
                    this.m_components[num].OnVisibilityChange();
                    num++;
                }
            }
        }

        public void SetVisibilityUpdates(bool state)
        {
            this.m_visibilityUpdates = state;
        }

        public void SetWorldMatrixDirty()
        {
            if (this.Parent != null)
            {
                this.m_worldMatrixDirty = true;
            }
            this.m_worldMatrixIndex++;
        }

        public void UpdateBeforeDraw()
        {
            this.OnUpdateBeforeDraw();
            this.UpdateProxy();
        }

        public void UpdateComponent(MyRenderMessageUpdateComponent message)
        {
            UpdateData data = message.Data;
            MyActorComponent component = this.m_components.TryGetComponent(data.ComponentType);
            if (message.Type == MyRenderMessageUpdateComponent.UpdateType.Delete)
            {
                if (component != null)
                {
                    this.RemoveComponent(data.ComponentType, component);
                }
            }
            else
            {
                if (component == null)
                {
                    component = this.Scene.ComponentFactory.Create(data.ComponentType);
                    this.AddComponent(data.ComponentType, component);
                }
                ((MyRenderDirectComponent) component).Update(data);
            }
        }

        private void UpdateProxy()
        {
            if (this.m_dirtyProxy && this.IsVisible)
            {
                this.m_dirtyProxy = false;
                if (this.m_root)
                {
                    this.m_aabb = ((this.m_cullTreeData.Children.GetRoot() != -1) ? this.m_cullTreeData.Children.GetAabb(this.m_cullTreeData.Children.GetRoot()) : new BoundingBox(Vector3.Zero, Vector3.Zero)).Transform(this.WorldMatrix);
                    if (this.m_cullTreeId == -1)
                    {
                        this.m_cullTreeId = this.Scene.ManualCullTree.AddProxy(ref this.m_aabb, this.m_cullTreeData, 0, true);
                    }
                    else
                    {
                        this.Scene.ManualCullTree.MoveProxy(this.m_cullTreeId, ref this.m_aabb, Vector3.Zero);
                    }
                }
            }
        }

        private void UpdateVolumeExtent()
        {
            Vector3 extents;
            if (this.Parent != null)
            {
                extents = this.m_localAabb.Value.Extents;
            }
            else
            {
                extents = (Vector3) this.m_aabb.Extents;
            }
            Vector3 vector = extents;
            this.VolumeExtent = Math.Max(Math.Max(vector.X, vector.Y), vector.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateWorldMatrix()
        {
            if (this.m_worldMatrixDirty)
            {
                if (this.m_relativeTransformValid)
                {
                    MatrixD.Multiply(ref this.m_relativeTransform, ref this.Parent.LastWorldMatrix, out this.LastWorldMatrix);
                    this.WorldMatrixForwardScale = this.Parent.WorldMatrixForwardScale * this.m_relativeForwardScale;
                }
                else
                {
                    this.LastWorldMatrix = this.Parent.WorldMatrix;
                    this.WorldMatrixForwardScale = this.Parent.WorldMatrixForwardScale;
                }
                this.m_worldMatrixDirty = false;
                this.m_worldMatrixInvDirty = true;
                this.m_worldMatrixIndex++;
            }
        }

        public string DebugName { get; private set; }

        public uint ID =>
            this.m_ID.ID;

        public bool IsVisible =>
            this.m_visible;

        public bool IsDestroyed =>
            ReferenceEquals(this.m_ID, null);

        public bool VisibilityUpdates =>
            this.m_visibilityUpdates;

        public MyScene Scene { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public MatrixD WorldMatrix
        {
            get
            {
                this.UpdateWorldMatrix();
                return this.LastWorldMatrix;
            }
            private set
            {
                this.LastWorldMatrix = value;
                this.m_worldMatrixDirty = false;
                this.m_worldMatrixInvDirty = true;
                this.m_worldMatrixIndex++;
                this.WorldMatrixForwardScale = (float) this.LastWorldMatrix.Forward.Length();
            }
        }

        public float WorldMatrixForwardScale { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Matrix RelativeTransform =>
            this.m_relativeTransform;

        public int WorldMatrixIndex =>
            this.m_worldMatrixIndex;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public MatrixD WorldMatrixInv
        {
            get
            {
                if (this.m_worldMatrixInvDirty || this.m_worldMatrixDirty)
                {
                    this.m_worldMatrixInvDirty = false;
                    this.m_worldMatrixInv = MatrixD.Invert(this.WorldMatrix);
                }
                return this.m_worldMatrixInv;
            }
        }

        public bool DirtyProxy =>
            this.m_dirtyProxy;

        public bool HasLocalAabb =>
            (this.m_localAabb != null);

        public MyActor Parent { get; private set; }

        public ListReader<MyActor> Children =>
            this.m_children;

        public BoundingBox LocalAabb =>
            this.m_localAabb.Value;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public BoundingBoxD WorldAabb =>
            this.m_aabb;

        public bool AlwaysUpdate
        {
            get => 
                (this.m_perFrameUpdateCounter > 0);
            set
            {
                if (value)
                {
                    if (this.m_perFrameUpdateCounter == 0)
                    {
                        this.Scene.Updater.AddToAlwaysUpdate(this);
                    }
                    this.m_perFrameUpdateCounter++;
                }
                else
                {
                    this.m_perFrameUpdateCounter--;
                    if (this.m_perFrameUpdateCounter == 0)
                    {
                        this.Scene.Updater.RemoveFromAlwaysUpdate(this);
                    }
                }
            }
        }

        public float VolumeExtent { get; private set; }
    }
}

