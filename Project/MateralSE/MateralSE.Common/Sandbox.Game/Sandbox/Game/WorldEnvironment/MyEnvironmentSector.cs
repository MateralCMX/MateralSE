namespace Sandbox.Game.WorldEnvironment
{
    using Havok;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.World;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyEnvironmentSector : MyEntity
    {
        private int m_parallelWorksInProgress;
        private MyProceduralEnvironmentDefinition m_environment;
        private MyInstancedRenderSector m_render;
        private IMyEnvironmentOwner m_owner;
        private IMyEnvironmentDataProvider m_provider;
        private MyConcurrentQueue<LodHEntry> m_lodHistory = new MyConcurrentQueue<LodHEntry>();
        private Vector3D m_sectorCenter;
        private BoundingBox2I m_dataRange;
        [CompilerGenerated]
        private Action OnPhysicsClose;
        private Dictionary<int, HkShape> m_modelsToShapes;
        private CompoundInstancedShape m_activeShape;
        private CompoundInstancedShape m_newShape;
        private bool m_togglePhysics;
        private bool m_recalculateShape;
        private int m_lodSwitchedFrom = -1;
        private volatile int m_currentLod = -1;
        private volatile int m_lodToSwitch = -1;
        private List<short> m_modelToItem;
        private readonly Dictionary<System.Type, Module> m_modules = new Dictionary<System.Type, Module>();
        private bool m_modulesPendingUpdate;
        private HashSet<MySectorContactEvent> m_contactListeners;
        private int m_hasParallelWorkPending;
        [CompilerGenerated]
        private Action<MyEnvironmentSector, int> OnLodCommit;
        [CompilerGenerated]
        private Action<MyEnvironmentSector, bool> OnPhysicsCommit;

        public event MySectorContactEvent OnContactPoint
        {
            add
            {
                if (this.m_contactListeners == null)
                {
                    this.m_contactListeners = new HashSet<MySectorContactEvent>();
                }
                if (((this.m_contactListeners.Count == 0) && (base.Physics != null)) && (base.Physics.RigidBody != null))
                {
                    base.Physics.RigidBody.ContactPointCallbackEnabled = true;
                }
                this.m_contactListeners.Add(value);
            }
            remove
            {
                if (this.m_contactListeners != null)
                {
                    this.m_contactListeners.Remove(value);
                    if (((this.m_contactListeners.Count == 0) && (base.Physics != null)) && (base.Physics.RigidBody != null))
                    {
                        base.Physics.RigidBody.ContactPointCallbackEnabled = false;
                    }
                }
            }
        }

        public event Action<MyEnvironmentSector, int> OnLodCommit
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentSector, int> onLodCommit = this.OnLodCommit;
                while (true)
                {
                    Action<MyEnvironmentSector, int> a = onLodCommit;
                    Action<MyEnvironmentSector, int> action3 = (Action<MyEnvironmentSector, int>) Delegate.Combine(a, value);
                    onLodCommit = Interlocked.CompareExchange<Action<MyEnvironmentSector, int>>(ref this.OnLodCommit, action3, a);
                    if (ReferenceEquals(onLodCommit, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentSector, int> onLodCommit = this.OnLodCommit;
                while (true)
                {
                    Action<MyEnvironmentSector, int> source = onLodCommit;
                    Action<MyEnvironmentSector, int> action3 = (Action<MyEnvironmentSector, int>) Delegate.Remove(source, value);
                    onLodCommit = Interlocked.CompareExchange<Action<MyEnvironmentSector, int>>(ref this.OnLodCommit, action3, source);
                    if (ReferenceEquals(onLodCommit, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action OnPhysicsClose
        {
            [CompilerGenerated] add
            {
                Action onPhysicsClose = this.OnPhysicsClose;
                while (true)
                {
                    Action a = onPhysicsClose;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onPhysicsClose = Interlocked.CompareExchange<Action>(ref this.OnPhysicsClose, action3, a);
                    if (ReferenceEquals(onPhysicsClose, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onPhysicsClose = this.OnPhysicsClose;
                while (true)
                {
                    Action source = onPhysicsClose;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onPhysicsClose = Interlocked.CompareExchange<Action>(ref this.OnPhysicsClose, action3, source);
                    if (ReferenceEquals(onPhysicsClose, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyEnvironmentSector, bool> OnPhysicsCommit
        {
            [CompilerGenerated] add
            {
                Action<MyEnvironmentSector, bool> onPhysicsCommit = this.OnPhysicsCommit;
                while (true)
                {
                    Action<MyEnvironmentSector, bool> a = onPhysicsCommit;
                    Action<MyEnvironmentSector, bool> action3 = (Action<MyEnvironmentSector, bool>) Delegate.Combine(a, value);
                    onPhysicsCommit = Interlocked.CompareExchange<Action<MyEnvironmentSector, bool>>(ref this.OnPhysicsCommit, action3, a);
                    if (ReferenceEquals(onPhysicsCommit, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEnvironmentSector, bool> onPhysicsCommit = this.OnPhysicsCommit;
                while (true)
                {
                    Action<MyEnvironmentSector, bool> source = onPhysicsCommit;
                    Action<MyEnvironmentSector, bool> action3 = (Action<MyEnvironmentSector, bool>) Delegate.Remove(source, value);
                    onPhysicsCommit = Interlocked.CompareExchange<Action<MyEnvironmentSector, bool>>(ref this.OnPhysicsCommit, action3, source);
                    if (ReferenceEquals(onPhysicsCommit, source))
                    {
                        return;
                    }
                }
            }
        }

        public unsafe void BuildInstanceBuffers(int lod)
        {
            short* numPtr;
            short[] pinned numArray;
            Sandbox.Game.WorldEnvironment.ItemInfo* infoPtr;
            Sandbox.Game.WorldEnvironment.ItemInfo[] pinned infoArray;
            Dictionary<short, List<MyInstanceData>> dictionary = new Dictionary<short, List<MyInstanceData>>();
            this.m_modelToItem = new List<short>(this.DataView.Items.Count);
            Vector3D vectord = this.SectorCenter - this.m_render.WorldMatrix.Translation;
            int count = this.DataView.Items.Count;
            if (((numArray = this.m_modelToItem.GetInternalArray<short>()) == null) || (numArray.Length == 0))
            {
                numPtr = null;
            }
            else
            {
                numPtr = numArray;
            }
            if (((infoArray = this.DataView.Items.GetInternalArray<Sandbox.Game.WorldEnvironment.ItemInfo>()) == null) || (infoArray.Length == 0))
            {
                infoPtr = null;
            }
            else
            {
                infoPtr = infoArray;
            }
            for (int i = 0; i < count; i++)
            {
                if (infoPtr[i].ModelIndex >= 0)
                {
                    List<MyInstanceData> list;
                    Matrix matrix;
                    if (!dictionary.TryGetValue(infoPtr[i].ModelIndex, out list))
                    {
                        list = new List<MyInstanceData>();
                        dictionary[infoPtr[i].ModelIndex] = list;
                    }
                    Matrix.CreateFromQuaternion(ref infoPtr[i].Rotation, out matrix);
                    matrix.Translation = infoPtr[i].Position + vectord;
                    numPtr[i] = (short) list.Count;
                    list.Add(new MyInstanceData(matrix));
                }
            }
            infoArray = null;
            numArray = null;
            this.m_modelToItem.SetSize<short>(count);
            foreach (KeyValuePair<short, List<MyInstanceData>> pair in dictionary)
            {
                MyPhysicalModelDefinition modelForId = this.m_owner.GetModelForId(pair.Key);
                if (modelForId != null)
                {
                    int id = MyModel.GetId(modelForId.Model);
                    this.m_render.AddInstances(id, pair.Value);
                }
            }
        }

        private unsafe void BuildShape()
        {
            Sandbox.Game.WorldEnvironment.ItemInfo* infoPtr;
            Sandbox.Game.WorldEnvironment.ItemInfo[] pinned infoArray;
            this.FetchData(0);
            CompoundInstancedShape shape = new CompoundInstancedShape();
            if (this.m_modelsToShapes == null)
            {
                this.m_modelsToShapes = new Dictionary<int, HkShape>();
            }
            int count = this.DataView.Items.Count;
            if (((infoArray = this.DataView.Items.GetInternalArray<Sandbox.Game.WorldEnvironment.ItemInfo>()) == null) || (infoArray.Length == 0))
            {
                infoPtr = null;
            }
            else
            {
                infoPtr = infoArray;
            }
            for (int i = 0; i < count; i++)
            {
                short modelIndex = infoPtr[i].ModelIndex;
                if ((modelIndex >= 0) && (this.Owner.GetModelForId(modelIndex) != null))
                {
                    HkShape shape2;
                    if (!this.m_modelsToShapes.TryGetValue(modelIndex, out shape2))
                    {
                        MyModel modelOnlyData = MyModels.GetModelOnlyData(this.Owner.GetModelForId(modelIndex).Model);
                        HkShape[] havokCollisionShapes = modelOnlyData.HavokCollisionShapes;
                        if (havokCollisionShapes != null)
                        {
                            if (havokCollisionShapes.Length != 0)
                            {
                                shape2 = (havokCollisionShapes.Length != 1) ? ((HkShape) new HkListShape(havokCollisionShapes, HkReferencePolicy.TakeOwnership)) : havokCollisionShapes[0];
                            }
                            else
                            {
                                object[] args = new object[] { modelOnlyData.AssetName };
                                MyLog.Default.Warning("Model {0} has an empty list of shapes, something wrong with export?", args);
                            }
                        }
                        this.m_modelsToShapes[modelIndex] = shape2;
                    }
                    shape.AddInstance(i, ref (Sandbox.Game.WorldEnvironment.ItemInfo) ref (infoPtr + i), shape2);
                }
            }
            infoArray = null;
            shape.Bake();
            this.m_newShape = shape;
        }

        public void CancelParallel()
        {
        }

        public void Close()
        {
            this.CloseInternal(false);
        }

        private void CloseInternal(bool entityClosing)
        {
            if (this.m_render != null)
            {
                this.m_render.DetachEnvironment(this);
            }
            if (this.DataView != null)
            {
                this.DataView.Close();
                this.DataView = null;
            }
            using (Dictionary<System.Type, Module>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Proxy.Close();
                }
            }
            this.HasPhysics = false;
            this.m_currentLod = -1;
            base.Close();
            this.IsClosed = true;
        }

        protected override void Closing()
        {
            this.CloseInternal(true);
        }

        public void DebugDraw()
        {
            Vector3D vectord;
            if ((this.LodLevel < 0) && !this.HasPhysics)
            {
                return;
            }
            Color red = Color.Red;
            if (ReferenceEquals(MyPlanetEnvironmentSessionComponent.ActiveSector, this))
            {
                red = Color.LimeGreen;
                if (this.DataView == null)
                {
                    goto TR_0004;
                }
                else
                {
                    if (MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorItems)
                    {
                        for (int i = 0; i < this.DataView.Items.Count; i++)
                        {
                            MyRuntimeEnvironmentItemInfo info2;
                            Sandbox.Game.WorldEnvironment.ItemInfo info = this.DataView.Items[i];
                            this.Owner.GetDefinition((ushort) info.DefinitionIndex, out info2);
                            MyRenderProxy.DebugDrawText3D(info.Position + this.SectorCenter, $"{info2.Type.Name} i{i} m{info.ModelIndex} d{info.DefinitionIndex}", red, 0.7f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        }
                    }
                    if (!MyPlanetEnvironmentSessionComponent.DebugDrawActiveSectorProvider)
                    {
                        goto TR_0004;
                    }
                    else
                    {
                        using (List<MyLogicalEnvironmentSectorBase>.Enumerator enumerator = this.DataView.LogicalSectors.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.DebugDraw(this.DataView.Lod);
                            }
                            goto TR_0004;
                        }
                    }
                }
            }
            if (this.HasPhysics && (this.LodLevel == -1))
            {
                red = Color.RoyalBlue;
            }
        TR_0004:
            vectord = (this.Bounds[4] + this.Bounds[7]) / 2.0;
            if (ReferenceEquals(MyPlanetEnvironmentSessionComponent.ActiveSector, this) || (Vector3D.DistanceSquared(vectord, MySector.MainCamera.Position) < (MyPlanetEnvironmentSessionComponent.DebugDrawDistance * MyPlanetEnvironmentSessionComponent.DebugDrawDistance)))
            {
                MyRenderProxy.DebugDrawText3D(vectord, this.ToString(), red, 1f, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            MyRenderProxy.DebugDraw6FaceConvex(this.Bounds, red, 1f, true, false, false);
        }

        public void DoParallelWork()
        {
            try
            {
                if ((Interlocked.Increment(ref this.m_parallelWorksInProgress) <= 1) && (Interlocked.Exchange(ref this.m_hasParallelWorkPending, 0) == 1))
                {
                    this.HasParallelWorkPending = false;
                    if (base.Closed)
                    {
                        this.m_lodToSwitch = this.m_currentLod;
                        this.m_togglePhysics = false;
                    }
                    else
                    {
                        bool flag = false;
                        if (this.m_lodToSwitch != this.m_currentLod)
                        {
                            flag = true;
                            if (this.m_lodToSwitch == -1)
                            {
                                this.m_render.Close();
                            }
                            else
                            {
                                this.FetchData(this.m_lodToSwitch);
                                this.BuildInstanceBuffers(this.m_lodToSwitch);
                            }
                            this.m_lodSwitchedFrom = this.m_currentLod;
                        }
                        if ((this.m_togglePhysics && !this.HasPhysics) || (this.HasPhysics && this.m_recalculateShape))
                        {
                            flag = true;
                            this.BuildShape();
                        }
                        this.HasSerialWorkPending = true;
                        if (flag)
                        {
                            this.Owner.ScheduleWork(this, false);
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this.m_parallelWorksInProgress);
            }
        }

        public bool DoSerialWork()
        {
            if (base.Closed)
            {
                return false;
            }
            if (this.HasParallelWorkPending)
            {
                return false;
            }
            bool flag = false;
            if (this.m_togglePhysics || (this.m_lodSwitchedFrom != this.m_lodToSwitch))
            {
                foreach (KeyValuePair<System.Type, Module> pair in this.m_modules)
                {
                    if (this.m_lodSwitchedFrom != this.m_lodToSwitch)
                    {
                        pair.Value.Proxy.CommitLodChange(this.m_lodSwitchedFrom, this.m_lodToSwitch);
                    }
                    if (this.m_togglePhysics)
                    {
                        pair.Value.Proxy.CommitPhysicsChange(!this.HasPhysics);
                    }
                }
                flag = true;
            }
            this.m_currentLod = this.m_lodToSwitch;
            if ((this.m_lodSwitchedFrom != this.m_currentLod) && (this.m_lodToSwitch == this.m_currentLod))
            {
                this.RaiseOnLodCommitEvent(this.m_currentLod);
            }
            if (this.m_togglePhysics)
            {
                this.RaiseOnPhysicsCommitEvent(this.HasPhysics);
            }
            if (((this.m_render != null) && this.m_render.HasChanges()) && (this.m_lodToSwitch == this.m_currentLod))
            {
                this.m_render.CommitChangesToRenderer();
                flag = true;
                this.m_lodSwitchedFrom = this.m_currentLod;
            }
            if (this.m_togglePhysics)
            {
                if (this.HasPhysics)
                {
                    base.Physics.Enabled = false;
                    this.HasPhysics = false;
                    this.m_togglePhysics = false;
                }
                else if (this.m_newShape != null)
                {
                    this.PreparePhysicsBody();
                    flag = true;
                    this.HasPhysics = true;
                    this.m_togglePhysics = false;
                }
            }
            if (this.m_recalculateShape)
            {
                this.m_recalculateShape = false;
                if (this.HasPhysics && (this.m_newShape != null))
                {
                    this.PreparePhysicsBody();
                }
            }
            this.HasSerialWorkPending = false;
            return flag;
        }

        public void EnableItem(int itemId, bool enabled)
        {
            MyLogicalEnvironmentSectorBase base2;
            int num;
            this.DataView.GetLogicalSector(itemId, out num, out base2);
            base2.EnableItem(num, enabled);
        }

        public void EnablePhysics(bool physics)
        {
            if (!base.Closed)
            {
                bool flag = this.HasPhysics != physics;
                if ((flag != this.m_togglePhysics) & flag)
                {
                    if ((this.m_activeShape == null) || this.m_recalculateShape)
                    {
                        if (Interlocked.Exchange(ref this.m_hasParallelWorkPending, 1) == 0)
                        {
                            this.Owner.ScheduleWork(this, true);
                        }
                    }
                    else
                    {
                        if (base.Physics != null)
                        {
                            base.Physics.Enabled = physics;
                        }
                        flag = false;
                        this.HasPhysics = physics;
                        if (!physics)
                        {
                            Action onPhysicsClose = this.OnPhysicsClose;
                            if (onPhysicsClose != null)
                            {
                                onPhysicsClose();
                            }
                        }
                    }
                }
                this.m_togglePhysics = flag;
            }
        }

        private unsafe void FetchData(int lodToSwitch)
        {
            MyEnvironmentDataView dataView = this.DataView;
            if ((dataView == null) || (dataView.Lod != lodToSwitch))
            {
                Sandbox.Game.WorldEnvironment.ItemInfo* infoPtr;
                Sandbox.Game.WorldEnvironment.ItemInfo[] pinned infoArray;
                this.DataView = this.m_provider.GetItemView(lodToSwitch, ref this.m_dataRange.Min, ref this.m_dataRange.Max, ref this.m_sectorCenter);
                this.DataView.Listener = this;
                if (dataView != null)
                {
                    dataView.Close();
                }
                using (Dictionary<System.Type, Module>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Proxy.Close();
                    }
                }
                this.m_modules.Clear();
                int count = this.DataView.Items.Count;
                if (((infoArray = this.DataView.Items.GetInternalArray<Sandbox.Game.WorldEnvironment.ItemInfo>()) == null) || (infoArray.Length == 0))
                {
                    infoPtr = null;
                }
                else
                {
                    infoPtr = infoArray;
                }
                for (int i = 0; i < count; i++)
                {
                    if (infoPtr[i].DefinitionIndex != -1)
                    {
                        MyItemTypeDefinition.Module[] proxyModules = this.m_environment.Items[infoPtr[i].DefinitionIndex].Type.ProxyModules;
                        if (proxyModules != null)
                        {
                            foreach (MyItemTypeDefinition.Module module in proxyModules)
                            {
                                Module module2;
                                if (!this.m_modules.TryGetValue(module.Type, out module2))
                                {
                                    module2 = new Module((IMyEnvironmentModuleProxy) Activator.CreateInstance(module.Type)) {
                                        Definition = module.Definition
                                    };
                                    this.m_modules[module.Type] = module2;
                                }
                                module2.Items.Add(i);
                            }
                        }
                    }
                }
                infoArray = null;
                foreach (KeyValuePair<System.Type, Module> pair in this.m_modules)
                {
                    pair.Value.Proxy.Init(this, pair.Value.Items);
                    pair.Value.Items = null;
                }
            }
        }

        public override int GetHashCode() => 
            this.SectorId.GetHashCode();

        public int GetItemFromShapeKey(uint shapekey)
        {
            uint num;
            int num2;
            this.m_activeShape.Shape.DecomposeShapeKey(shapekey, out num2, out num);
            return this.m_activeShape.GetItemId(num2);
        }

        public void GetItemInfo(int itemId, out uint renderObjectId, out int instanceIndex)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo info = this.DataView.Items[itemId];
            int id = MyModel.GetId(this.m_owner.GetModelForId(info.ModelIndex).Model);
            renderObjectId = this.m_render.GetRenderEntity(id);
            instanceIndex = this.m_modelToItem[itemId];
        }

        public short GetModelIndex(int itemId)
        {
            int num;
            MyLogicalEnvironmentSectorBase base2;
            Sandbox.Game.WorldEnvironment.ItemInfo info;
            this.DataView.GetLogicalSector(itemId, out num, out base2);
            base2.GetItem(num, out info);
            return info.ModelIndex;
        }

        public T GetModule<T>() where T: class, IMyEnvironmentModuleProxy
        {
            Module module;
            this.m_modules.TryGetValue(typeof(T), out module);
            return ((module != null) ? ((T) module.Proxy) : null);
        }

        public IMyEnvironmentModuleProxy GetModule(System.Type moduleType)
        {
            Module module;
            this.m_modules.TryGetValue(moduleType, out module);
            return ((module != null) ? module.Proxy : null);
        }

        public T GetModuleForDefinition<T>(MyRuntimeEnvironmentItemInfo itemEnvDefinition) where T: class, IMyEnvironmentModuleProxy
        {
            Module module;
            MyItemTypeDefinition.Module[] proxyModules = itemEnvDefinition.Type.ProxyModules;
            if ((proxyModules == null) || !proxyModules.Any<MyItemTypeDefinition.Module>(x => typeof(T).IsAssignableFrom(x.Type)))
            {
                return default(T);
            }
            this.m_modules.TryGetValue(typeof(T), out module);
            return ((module != null) ? ((T) module.Proxy) : null);
        }

        public void Init(IMyEnvironmentOwner owner, ref MyEnvironmentSectorParameters parameters)
        {
            this.SectorCenter = parameters.Center;
            this.Bounds = parameters.Bounds;
            this.m_dataRange = parameters.DataRange;
            this.m_environment = (MyProceduralEnvironmentDefinition) parameters.Environment;
            this.EnvironmentDefinition = parameters.Environment;
            this.m_owner = owner;
            this.m_provider = parameters.Provider;
            Vector3D center = parameters.Center;
            owner.ProjectPointToSurface(ref center);
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                this.m_render = new MyInstancedRenderSector($"{owner}:Sector(0x{parameters.SectorId:X})", MatrixD.CreateTranslation(center));
            }
            this.SectorId = parameters.SectorId;
            BoundingBoxD xd = BoundingBoxD.CreateInvalid();
            for (int i = 0; i < 8; i++)
            {
                xd.Include(this.Bounds[i]);
            }
            base.PositionComp.SetPosition(parameters.Center, null, false, true);
            base.PositionComp.WorldAABB = xd;
            base.AddDebugRenderComponent(new MyDebugRenderComponentEnvironmentSector(this));
            base.GameLogic = new MyNullGameLogicComponent();
            base.Save = false;
            this.IsClosed = false;
        }

        public void OnItemChange(int index, short newModelIndex)
        {
            if ((this.m_currentLod != -1) || this.HasPhysics)
            {
                using (Dictionary<System.Type, Module>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Proxy.OnItemChange(index, newModelIndex);
                    }
                }
                if (this.m_currentLod != -1)
                {
                    this.UpdateItemModel(index, newModelIndex);
                    this.m_render.CommitChangesToRenderer();
                }
                if (this.HasPhysics)
                {
                    this.UpdateItemShape(index, newModelIndex);
                }
                else if (newModelIndex >= 0)
                {
                    this.m_recalculateShape = true;
                }
            }
        }

        public void OnItemsChange(int sector, List<int> indices, short newModelIndex)
        {
            if ((this.m_currentLod != -1) || this.HasPhysics)
            {
                int offset = this.DataView.SectorOffsets[sector];
                int num2 = ((sector < (this.DataView.SectorOffsets.Count - 1)) ? this.DataView.SectorOffsets[sector + 1] : this.DataView.Items.Count) - offset;
                int index = 0;
                while (true)
                {
                    if (index >= indices.Count)
                    {
                        List<int>.Enumerator enumerator2;
                        using (Dictionary<System.Type, Module>.ValueCollection.Enumerator enumerator = this.m_modules.Values.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current.Proxy.OnItemChangeBatch(indices, offset, newModelIndex);
                            }
                        }
                        if (this.m_currentLod != -1)
                        {
                            using (enumerator2 = indices.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    int num4 = enumerator2.Current + offset;
                                    this.UpdateItemModel(num4, newModelIndex);
                                }
                            }
                            this.m_render.CommitChangesToRenderer();
                        }
                        if (this.HasPhysics)
                        {
                            using (enumerator2 = indices.GetEnumerator())
                            {
                                while (enumerator2.MoveNext())
                                {
                                    int num5 = enumerator2.Current + offset;
                                    this.UpdateItemShape(num5, newModelIndex);
                                }
                                break;
                            }
                        }
                        if (newModelIndex > 0)
                        {
                            this.m_recalculateShape = true;
                        }
                        break;
                    }
                    if (indices[index] >= num2)
                    {
                        indices.RemoveAtFast<int>(index);
                        index--;
                    }
                    index++;
                }
            }
        }

        private void Physics_onContactPoint(ref MyPhysics.MyContactPointEvent evt)
        {
            MyPhysicsBody physicsBody = evt.ContactPointEvent.GetPhysicsBody(0);
            if (physicsBody != null)
            {
                int bodyIdx = ReferenceEquals(physicsBody.Entity, this) ? 0 : 1;
                uint shapeKey = evt.ContactPointEvent.GetShapeKey(bodyIdx);
                if (shapeKey != uint.MaxValue)
                {
                    MyPhysicsBody body2 = evt.ContactPointEvent.GetPhysicsBody(1 ^ bodyIdx);
                    if (body2 != null)
                    {
                        IMyEntity entity = body2.Entity;
                        int itemFromShapeKey = this.GetItemFromShapeKey(shapeKey);
                        using (HashSet<MySectorContactEvent>.Enumerator enumerator = this.m_contactListeners.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                enumerator.Current(itemFromShapeKey, (MyEntity) entity, ref evt);
                            }
                        }
                    }
                }
            }
        }

        private void PreparePhysicsBody()
        {
            this.m_activeShape = this.m_newShape;
            this.m_newShape = null;
            if (base.Physics != null)
            {
                base.Physics.Close();
            }
            base.Physics = new MyPhysicsBody(this, RigidBodyFlag.RBF_STATIC);
            HkMassProperties properties = new HkMassProperties();
            MyPhysicsBody physics = (MyPhysicsBody) base.Physics;
            physics.CreateFromCollisionObject((HkShape) this.m_activeShape.Shape, Vector3.Zero, base.PositionComp.WorldMatrix, new HkMassProperties?(properties), 15);
            physics.ContactPointCallback += new MyPhysicsBody.PhysicsContactHandler(this.Physics_onContactPoint);
            physics.IsStaticForCluster = true;
            if ((this.m_contactListeners != null) && (this.m_contactListeners.Count != 0))
            {
                base.Physics.RigidBody.ContactPointCallbackEnabled = true;
            }
            base.Physics.Enabled = true;
        }

        public void RaiseItemEvent<TModule>(TModule module, int item, bool fromClient = false) where TModule: IMyEnvironmentModuleProxy
        {
            this.RaiseItemEvent<TModule, object>(module, item, null, fromClient);
        }

        public void RaiseItemEvent<TModule, TArgument>(TModule module, int item, TArgument eventData, bool fromClient = false) where TModule: IMyEnvironmentModuleProxy
        {
            int num;
            MyLogicalEnvironmentSectorBase base2;
            MyDefinitionId definition = this.m_modules[typeof(TModule)].Definition;
            this.DataView.GetLogicalSector(item, out num, out base2);
            base2.RaiseItemEvent<TArgument>(num, ref definition, eventData, fromClient);
        }

        public void RaiseOnLodCommitEvent(int lod)
        {
            if (this.OnLodCommit != null)
            {
                this.OnLodCommit(this, lod);
            }
        }

        public void RaiseOnPhysicsCommitEvent(bool enabled)
        {
            if (this.OnPhysicsCommit != null)
            {
                this.OnPhysicsCommit(this, enabled);
            }
        }

        [Conditional("DEBUG")]
        private void RecordHistory(int lod, bool set)
        {
            if (this.m_lodHistory.Count > 10)
            {
                this.m_lodHistory.Dequeue();
            }
            LodHEntry instance = new LodHEntry {
                Lod = lod,
                Set = set,
                Trace = new StackTrace()
            };
            this.m_lodHistory.Enqueue(instance);
        }

        public void SetLod(int lod)
        {
            if (!base.Closed && ((lod != this.m_currentLod) || (lod != this.m_lodToSwitch)))
            {
                if (Interlocked.Exchange(ref this.m_hasParallelWorkPending, 1) == 0)
                {
                    this.Owner.ScheduleWork(this, true);
                }
                this.m_lodToSwitch = lod;
                if (this.m_render != null)
                {
                    this.m_render.Lod = this.m_lodToSwitch;
                }
            }
        }

        public override string ToString()
        {
            long sectorId = this.SectorId;
            int num = (int) (sectorId & 0xffffffL);
            long num5 = sectorId >> 0x18;
            int num2 = (int) (num5 & 0xffffffL);
            long num6 = num5 >> 0x18;
            int num3 = (int) (num6 & 7L);
            int num4 = (int) ((num6 >> 3) & 0xffL);
            object[] objArray1 = new object[7];
            objArray1[0] = num;
            objArray1[1] = num2;
            objArray1[2] = num3;
            objArray1[3] = num4;
            objArray1[4] = this.LodLevel;
            objArray1[5] = this.HasPhysics ? " p" : "";
            object[] local1 = objArray1;
            object[] args = objArray1;
            args[6] = (this.DataView != null) ? this.DataView.Items.Count : 0;
            return string.Format("S(x{0} y{1} f{2} l{3}({4}) c{6} {5})", args);
        }

        private void UpdateItemModel(int index, short newModelIndex)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo info = this.DataView.Items[index];
            if (info.ModelIndex != newModelIndex)
            {
                if (this.m_currentLod == this.m_lodToSwitch)
                {
                    if ((info.ModelIndex >= 0) && (this.m_owner.GetModelForId(info.ModelIndex) != null))
                    {
                        int id = MyModel.GetId(this.m_owner.GetModelForId(info.ModelIndex).Model);
                        this.m_render.RemoveInstance(id, this.m_modelToItem[index]);
                        this.m_modelToItem[index] = -1;
                    }
                    if ((newModelIndex >= 0) && (this.m_owner.GetModelForId(newModelIndex) != null))
                    {
                        Matrix matrix;
                        int id = MyModel.GetId(this.m_owner.GetModelForId(newModelIndex).Model);
                        Vector3D vectord = this.SectorCenter - this.m_render.WorldMatrix.Translation;
                        Matrix.CreateFromQuaternion(ref info.Rotation, out matrix);
                        matrix.Translation = info.Position + vectord;
                        MyInstanceData data = new MyInstanceData(matrix);
                        this.m_modelToItem[index] = this.m_render.AddInstance(id, ref data);
                    }
                }
                info.ModelIndex = newModelIndex;
                this.DataView.Items[index] = info;
            }
        }

        private void UpdateItemShape(int index, short newModelIndex)
        {
            int num;
            if ((this.m_activeShape != null) && this.m_activeShape.TryGetInstance(index, out num))
            {
                this.m_activeShape.Shape.EnableInstance(num, newModelIndex >= 0);
            }
            else if (!this.m_recalculateShape)
            {
                this.m_recalculateShape = true;
                if (Interlocked.Exchange(ref this.m_hasParallelWorkPending, 1) == 0)
                {
                    this.Owner.ScheduleWork(this, true);
                }
            }
        }

        public Vector3D SectorCenter
        {
            get => 
                this.m_sectorCenter;
            private set => 
                (this.m_sectorCenter = value);
        }

        public Vector3D[] Bounds { get; private set; }

        public MyWorldEnvironmentDefinition EnvironmentDefinition { get; private set; }

        public bool IsLoaded =>
            true;

        public bool IsClosed { get; private set; }

        public int LodLevel =>
            this.m_currentLod;

        public bool HasPhysics { get; private set; }

        public bool IsPinned { get; internal set; }

        public bool IsPendingLodSwitch =>
            (this.m_currentLod != this.m_lodToSwitch);

        public bool IsPendingPhysicsToggle =>
            this.m_togglePhysics;

        public bool HasSerialWorkPending { get; private set; }

        public bool HasParallelWorkPending
        {
            get => 
                (Interlocked.CompareExchange(ref this.m_hasParallelWorkPending, 0, 0) == 1);
            private set => 
                Interlocked.Exchange(ref this.m_hasParallelWorkPending, value ? 1 : 0);
        }

        public bool HasParallelWorkInProgress =>
            (Volatile.Read(ref this.m_parallelWorksInProgress) > 0);

        public long SectorId { get; private set; }

        public MyEnvironmentDataView DataView { get; private set; }

        public IMyEnvironmentOwner Owner =>
            this.m_owner;

        [Serializable, CompilerGenerated]
        private sealed class <>c__97<T> where T: class, IMyEnvironmentModuleProxy
        {
            public static readonly MyEnvironmentSector.<>c__97<T> <>9;
            public static Func<MyItemTypeDefinition.Module, bool> <>9__97_0;

            static <>c__97()
            {
                MyEnvironmentSector.<>c__97<T>.<>9 = new MyEnvironmentSector.<>c__97<T>();
            }

            internal bool <GetModuleForDefinition>b__97_0(MyItemTypeDefinition.Module x) => 
                typeof(T).IsAssignableFrom(x.Type);
        }

        private class CompoundInstancedShape
        {
            public HkStaticCompoundShape Shape = new HkStaticCompoundShape(HkReferencePolicy.TakeOwnership);
            private readonly Dictionary<int, int> m_itemToShapeInstance = new Dictionary<int, int>();
            private readonly Dictionary<int, int> m_shapeInstanceToItem = new Dictionary<int, int>();
            private bool m_baked;

            public void AddInstance(int itemId, ref Sandbox.Game.WorldEnvironment.ItemInfo item, HkShape shape)
            {
                if (!shape.IsZero)
                {
                    Matrix matrix;
                    Matrix.CreateFromQuaternion(ref item.Rotation, out matrix);
                    matrix.Translation = item.Position;
                    int num = this.Shape.AddInstance(shape, matrix);
                    this.m_itemToShapeInstance[itemId] = num;
                    this.m_shapeInstanceToItem[num] = itemId;
                }
            }

            public void Bake()
            {
                this.Shape.Bake();
                this.m_baked = true;
            }

            public int GetItemId(int shapeInstance) => 
                this.m_shapeInstanceToItem[shapeInstance];

            public bool TryGetInstance(int itemId, out int shapeInstance) => 
                this.m_itemToShapeInstance.TryGetValue(itemId, out shapeInstance);

            public bool TryGetItemId(int shapeInstance, out int itemId) => 
                this.m_shapeInstanceToItem.TryGetValue(shapeInstance, out itemId);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LodHEntry
        {
            public int Lod;
            public bool Set;
            public StackTrace Trace;
            public override string ToString() => 
                $"{(this.Set ? "Set" : "Requested")} {this.Lod} @ {this.Trace.GetFrame(1)}";
        }

        private class Module
        {
            public readonly IMyEnvironmentModuleProxy Proxy;
            public List<int> Items = new List<int>();
            public MyDefinitionId Definition;

            public Module(IMyEnvironmentModuleProxy proxy)
            {
                this.Proxy = proxy;
            }
        }
    }
}

