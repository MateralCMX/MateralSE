namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyResourceSinkComponent : MyResourceSinkComponentBase
    {
        private MyEntity m_tmpConnectedEntity;
        private PerTypeData[] m_dataPerType;
        private readonly Dictionary<MyDefinitionId, int> m_resourceTypeToIndex = new Dictionary<MyDefinitionId, int>(1, MyDefinitionId.Comparer);
        private readonly List<MyDefinitionId> m_resourceIds = new List<MyDefinitionId>(1);
        [ThreadStatic]
        private static List<MyResourceSinkInfo> m_singleHelperList;
        internal MyStringHash Group;
        [CompilerGenerated]
        private MyRequiredResourceChangeDelegate RequiredInputChanged;
        [CompilerGenerated]
        private MyResourceAvailableDelegate ResourceAvailable;
        [CompilerGenerated]
        private MyCurrentResourceInputChangedDelegate CurrentInputChanged;
        [CompilerGenerated]
        private Action IsPoweredChanged;
        [CompilerGenerated]
        private Action<MyResourceSinkComponent, MyDefinitionId> OnAddType;
        [CompilerGenerated]
        private Action<MyResourceSinkComponent, MyDefinitionId> OnRemoveType;

        public event MyCurrentResourceInputChangedDelegate CurrentInputChanged
        {
            [CompilerGenerated] add
            {
                MyCurrentResourceInputChangedDelegate currentInputChanged = this.CurrentInputChanged;
                while (true)
                {
                    MyCurrentResourceInputChangedDelegate a = currentInputChanged;
                    MyCurrentResourceInputChangedDelegate delegate4 = (MyCurrentResourceInputChangedDelegate) Delegate.Combine(a, value);
                    currentInputChanged = Interlocked.CompareExchange<MyCurrentResourceInputChangedDelegate>(ref this.CurrentInputChanged, delegate4, a);
                    if (ReferenceEquals(currentInputChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyCurrentResourceInputChangedDelegate currentInputChanged = this.CurrentInputChanged;
                while (true)
                {
                    MyCurrentResourceInputChangedDelegate source = currentInputChanged;
                    MyCurrentResourceInputChangedDelegate delegate4 = (MyCurrentResourceInputChangedDelegate) Delegate.Remove(source, value);
                    currentInputChanged = Interlocked.CompareExchange<MyCurrentResourceInputChangedDelegate>(ref this.CurrentInputChanged, delegate4, source);
                    if (ReferenceEquals(currentInputChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action IsPoweredChanged
        {
            [CompilerGenerated] add
            {
                Action isPoweredChanged = this.IsPoweredChanged;
                while (true)
                {
                    Action a = isPoweredChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    isPoweredChanged = Interlocked.CompareExchange<Action>(ref this.IsPoweredChanged, action3, a);
                    if (ReferenceEquals(isPoweredChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action isPoweredChanged = this.IsPoweredChanged;
                while (true)
                {
                    Action source = isPoweredChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    isPoweredChanged = Interlocked.CompareExchange<Action>(ref this.IsPoweredChanged, action3, source);
                    if (ReferenceEquals(isPoweredChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyResourceSinkComponent, MyDefinitionId> OnAddType
        {
            [CompilerGenerated] add
            {
                Action<MyResourceSinkComponent, MyDefinitionId> onAddType = this.OnAddType;
                while (true)
                {
                    Action<MyResourceSinkComponent, MyDefinitionId> a = onAddType;
                    Action<MyResourceSinkComponent, MyDefinitionId> action3 = (Action<MyResourceSinkComponent, MyDefinitionId>) Delegate.Combine(a, value);
                    onAddType = Interlocked.CompareExchange<Action<MyResourceSinkComponent, MyDefinitionId>>(ref this.OnAddType, action3, a);
                    if (ReferenceEquals(onAddType, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyResourceSinkComponent, MyDefinitionId> onAddType = this.OnAddType;
                while (true)
                {
                    Action<MyResourceSinkComponent, MyDefinitionId> source = onAddType;
                    Action<MyResourceSinkComponent, MyDefinitionId> action3 = (Action<MyResourceSinkComponent, MyDefinitionId>) Delegate.Remove(source, value);
                    onAddType = Interlocked.CompareExchange<Action<MyResourceSinkComponent, MyDefinitionId>>(ref this.OnAddType, action3, source);
                    if (ReferenceEquals(onAddType, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyResourceSinkComponent, MyDefinitionId> OnRemoveType
        {
            [CompilerGenerated] add
            {
                Action<MyResourceSinkComponent, MyDefinitionId> onRemoveType = this.OnRemoveType;
                while (true)
                {
                    Action<MyResourceSinkComponent, MyDefinitionId> a = onRemoveType;
                    Action<MyResourceSinkComponent, MyDefinitionId> action3 = (Action<MyResourceSinkComponent, MyDefinitionId>) Delegate.Combine(a, value);
                    onRemoveType = Interlocked.CompareExchange<Action<MyResourceSinkComponent, MyDefinitionId>>(ref this.OnRemoveType, action3, a);
                    if (ReferenceEquals(onRemoveType, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyResourceSinkComponent, MyDefinitionId> onRemoveType = this.OnRemoveType;
                while (true)
                {
                    Action<MyResourceSinkComponent, MyDefinitionId> source = onRemoveType;
                    Action<MyResourceSinkComponent, MyDefinitionId> action3 = (Action<MyResourceSinkComponent, MyDefinitionId>) Delegate.Remove(source, value);
                    onRemoveType = Interlocked.CompareExchange<Action<MyResourceSinkComponent, MyDefinitionId>>(ref this.OnRemoveType, action3, source);
                    if (ReferenceEquals(onRemoveType, source))
                    {
                        return;
                    }
                }
            }
        }

        public event MyRequiredResourceChangeDelegate RequiredInputChanged
        {
            [CompilerGenerated] add
            {
                MyRequiredResourceChangeDelegate requiredInputChanged = this.RequiredInputChanged;
                while (true)
                {
                    MyRequiredResourceChangeDelegate a = requiredInputChanged;
                    MyRequiredResourceChangeDelegate delegate4 = (MyRequiredResourceChangeDelegate) Delegate.Combine(a, value);
                    requiredInputChanged = Interlocked.CompareExchange<MyRequiredResourceChangeDelegate>(ref this.RequiredInputChanged, delegate4, a);
                    if (ReferenceEquals(requiredInputChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyRequiredResourceChangeDelegate requiredInputChanged = this.RequiredInputChanged;
                while (true)
                {
                    MyRequiredResourceChangeDelegate source = requiredInputChanged;
                    MyRequiredResourceChangeDelegate delegate4 = (MyRequiredResourceChangeDelegate) Delegate.Remove(source, value);
                    requiredInputChanged = Interlocked.CompareExchange<MyRequiredResourceChangeDelegate>(ref this.RequiredInputChanged, delegate4, source);
                    if (ReferenceEquals(requiredInputChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event MyResourceAvailableDelegate ResourceAvailable
        {
            [CompilerGenerated] add
            {
                MyResourceAvailableDelegate resourceAvailable = this.ResourceAvailable;
                while (true)
                {
                    MyResourceAvailableDelegate a = resourceAvailable;
                    MyResourceAvailableDelegate delegate4 = (MyResourceAvailableDelegate) Delegate.Combine(a, value);
                    resourceAvailable = Interlocked.CompareExchange<MyResourceAvailableDelegate>(ref this.ResourceAvailable, delegate4, a);
                    if (ReferenceEquals(resourceAvailable, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyResourceAvailableDelegate resourceAvailable = this.ResourceAvailable;
                while (true)
                {
                    MyResourceAvailableDelegate source = resourceAvailable;
                    MyResourceAvailableDelegate delegate4 = (MyResourceAvailableDelegate) Delegate.Remove(source, value);
                    resourceAvailable = Interlocked.CompareExchange<MyResourceAvailableDelegate>(ref this.ResourceAvailable, delegate4, source);
                    if (ReferenceEquals(resourceAvailable, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyResourceSinkComponent(int initialAllocationSize = 1)
        {
            this.AllocateData(initialAllocationSize);
        }

        public void AddType(ref MyResourceSinkInfo sinkData)
        {
            if (!this.m_resourceIds.Contains(sinkData.ResourceTypeId) && !this.m_resourceTypeToIndex.ContainsKey(sinkData.ResourceTypeId))
            {
                PerTypeData[] dataArray = new PerTypeData[this.m_resourceIds.Count + 1];
                for (int i = 0; i < this.m_dataPerType.Length; i++)
                {
                    dataArray[i] = this.m_dataPerType[i];
                }
                this.m_dataPerType = dataArray;
                PerTypeData data = new PerTypeData {
                    MaxRequiredInput = sinkData.MaxRequiredInput,
                    RequiredInputFunc = sinkData.RequiredInputFunc
                };
                this.m_dataPerType[this.m_dataPerType.Length - 1] = data;
                this.m_resourceIds.Add(sinkData.ResourceTypeId);
                this.m_resourceTypeToIndex.Add(sinkData.ResourceTypeId, this.m_dataPerType.Length - 1);
                if (this.OnAddType != null)
                {
                    this.OnAddType(this, sinkData.ResourceTypeId);
                }
            }
        }

        private void AllocateData(int allocationSize)
        {
            this.m_dataPerType = new PerTypeData[allocationSize];
        }

        private void ClearAllCallbacks()
        {
            this.RequiredInputChanged = null;
            this.ResourceAvailable = null;
            this.CurrentInputChanged = null;
            this.IsPoweredChanged = null;
            this.OnAddType = null;
            this.OnRemoveType = null;
        }

        public override float CurrentInputByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].CurrentInput;

        public void DebugDraw(Matrix worldMatrix)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_RESOURCE_RECEIVERS)
            {
                double num = 2.5 * 0.045;
                Vector3D vectord = worldMatrix.Translation + worldMatrix.Up;
                Vector3D up = MySector.MainCamera.WorldMatrix.Up;
                Vector3D right = MySector.MainCamera.WorldMatrix.Right;
                double num3 = Math.Atan(2.5 / Math.Max(Vector3D.Distance(vectord, MySector.MainCamera.Position), 0.001));
                if (num3 > 0.27000001072883606)
                {
                    if (this.Entity != null)
                    {
                        MyRenderProxy.DebugDrawText3D(vectord, this.Entity.ToString(), Color.Yellow, (float) num3, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
                    }
                    if ((this.m_resourceIds != null) && (this.m_resourceIds.Count != 0))
                    {
                        Vector3D origin = vectord;
                        int num4 = -1;
                        foreach (MyDefinitionId id in this.m_resourceIds)
                        {
                            origin = vectord + ((num4 * up) * num);
                            this.DebugDrawResource(id, origin, right, (float) num3);
                            num4--;
                        }
                    }
                }
            }
        }

        private void DebugDrawResource(MyDefinitionId resourceId, Vector3D origin, Vector3D rightVector, float textSize)
        {
            Vector3D vectord = (Vector3D) (0.05000000074505806 * rightVector);
            Vector3D worldCoord = (origin + vectord) + (rightVector * 0.014999999664723873);
            int num = 0;
            string subtypeName = resourceId.SubtypeName;
            if (this.m_resourceTypeToIndex.TryGetValue(resourceId, out num))
            {
                PerTypeData data = this.m_dataPerType[num];
                subtypeName = $"{resourceId.SubtypeName} Required:{data.RequiredInput} Current:{data.CurrentInput} Ratio:{data.SuppliedRatio}";
            }
            MyRenderProxy.DebugDrawLine3D(origin, origin + vectord, Color.White, Color.White, false, false);
            MyRenderProxy.DebugDrawText3D(worldCoord, subtypeName, Color.White, textSize, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
        }

        protected int GetTypeIndex(MyDefinitionId resourceTypeId)
        {
            int num = 0;
            this.m_resourceTypeToIndex.TryGetValue(resourceTypeId, out num);
            return num;
        }

        public void Init(MyStringHash group, List<MyResourceSinkInfo> sinkData)
        {
            this.Group = group;
            if (this.m_dataPerType.Length != sinkData.Count)
            {
                this.AllocateData(sinkData.Count);
            }
            this.m_resourceTypeToIndex.Clear();
            this.m_resourceIds.Clear();
            this.ClearAllCallbacks();
            int num = 0;
            for (int i = 0; i < sinkData.Count; i++)
            {
                num++;
                this.m_resourceTypeToIndex.Add(sinkData[i].ResourceTypeId, num);
                this.m_resourceIds.Add(sinkData[i].ResourceTypeId);
                this.m_dataPerType[num - 1].MaxRequiredInput = sinkData[i].MaxRequiredInput;
                this.m_dataPerType[num - 1].RequiredInputFunc = sinkData[i].RequiredInputFunc;
            }
        }

        public void Init(MyStringHash group, MyResourceSinkInfo sinkData)
        {
            using (MyUtils.ReuseCollection<MyResourceSinkInfo>(ref m_singleHelperList))
            {
                m_singleHelperList.Add(sinkData);
                this.Init(group, m_singleHelperList);
            }
        }

        public void Init(MyStringHash group, float maxRequiredInput, Func<float> requiredInputFunc)
        {
            using (MyUtils.ReuseCollection<MyResourceSinkInfo>(ref m_singleHelperList))
            {
                MyResourceSinkInfo item = new MyResourceSinkInfo {
                    ResourceTypeId = MyResourceDistributorComponent.ElectricityId,
                    MaxRequiredInput = maxRequiredInput,
                    RequiredInputFunc = requiredInputFunc
                };
                m_singleHelperList.Add(item);
                this.Init(group, m_singleHelperList);
            }
        }

        public override bool IsPowerAvailable(MyDefinitionId resourceTypeId, float power) => 
            (this.ResourceAvailableByType(resourceTypeId) >= power);

        public override bool IsPoweredByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].IsPowered;

        public override float MaxRequiredInputByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].MaxRequiredInput;

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
            this.ClearAllCallbacks();
        }

        public void RemoveType(ref MyDefinitionId resourceType)
        {
            if (this.m_resourceIds.Contains(resourceType))
            {
                if (this.OnRemoveType != null)
                {
                    this.OnRemoveType(this, resourceType);
                }
                PerTypeData[] dataArray = new PerTypeData[this.m_resourceIds.Count - 1];
                int typeIndex = this.GetTypeIndex(resourceType);
                int index = 0;
                int num3 = 0;
                while (num3 < this.m_dataPerType.Length)
                {
                    if (num3 != typeIndex)
                    {
                        dataArray[index] = this.m_dataPerType[num3];
                    }
                    num3++;
                    index++;
                }
                this.m_dataPerType = dataArray;
                this.m_resourceIds.Remove(resourceType);
                this.m_resourceTypeToIndex.Remove(resourceType);
            }
        }

        public override float RequiredInputByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].RequiredInput;

        public float ResourceAvailableByType(MyDefinitionId resourceTypeId)
        {
            float num = this.CurrentInputByType(resourceTypeId);
            if (this.ResourceAvailable != null)
            {
                num += this.ResourceAvailable(resourceTypeId, this);
            }
            return num;
        }

        public override void SetInputFromDistributor(MyDefinitionId resourceTypeId, float newResourceInput, bool isAdaptible, bool fireEvents = true)
        {
            float num2;
            bool flag;
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            float currentInput = this.m_dataPerType[typeIndex].CurrentInput;
            float requiredInput = this.m_dataPerType[typeIndex].RequiredInput;
            if ((newResourceInput > 0f) || (requiredInput == 0f))
            {
                flag = isAdaptible || (newResourceInput >= requiredInput);
                num2 = (requiredInput <= 0f) ? 1f : (newResourceInput / requiredInput);
            }
            else
            {
                flag = false;
                num2 = 0f;
                if (MyPerGameSettings.Game == GameEnum.ME_GAME)
                {
                    flag = this.m_dataPerType[typeIndex].RequiredInput == 0f;
                    num2 = (this.m_dataPerType[typeIndex].RequiredInput == 0f) ? 1f : 0f;
                }
            }
            bool flag2 = !newResourceInput.IsEqual(this.m_dataPerType[typeIndex].CurrentInput, 1E-06f);
            bool flag3 = flag != this.m_dataPerType[typeIndex].IsPowered;
            this.m_dataPerType[typeIndex].IsPowered = flag;
            this.m_dataPerType[typeIndex].SuppliedRatio = num2;
            this.m_dataPerType[typeIndex].CurrentInput = newResourceInput;
            if (fireEvents)
            {
                if (flag2 && (this.CurrentInputChanged != null))
                {
                    this.CurrentInputChanged(resourceTypeId, currentInput, this);
                }
                if (flag3 && (this.IsPoweredChanged != null))
                {
                    this.IsPoweredChanged();
                }
            }
        }

        public override void SetMaxRequiredInputByType(MyDefinitionId resourceTypeId, float newMaxRequiredInput)
        {
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].MaxRequiredInput = newMaxRequiredInput;
        }

        public override void SetRequiredInputByType(MyDefinitionId resourceTypeId, float newRequiredInput)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            if (this.m_dataPerType[typeIndex].RequiredInput != newRequiredInput)
            {
                float requiredInput = this.m_dataPerType[typeIndex].RequiredInput;
                this.m_dataPerType[typeIndex].RequiredInput = newRequiredInput;
                if (this.RequiredInputChanged != null)
                {
                    this.RequiredInputChanged(resourceTypeId, this, requiredInput, newRequiredInput);
                }
            }
        }

        public override void SetRequiredInputFuncByType(MyDefinitionId resourceTypeId, Func<float> newRequiredInputFunc)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            this.m_dataPerType[typeIndex].RequiredInputFunc = newRequiredInputFunc;
        }

        public override float SuppliedRatioByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].SuppliedRatio;

        public void Update()
        {
            foreach (MyDefinitionId id in this.m_resourceTypeToIndex.Keys)
            {
                this.SetRequiredInputByType(id, this.m_dataPerType[this.GetTypeIndex(id)].RequiredInputFunc());
            }
        }

        public override IMyEntity TemporaryConnectedEntity
        {
            get => 
                this.m_tmpConnectedEntity;
            set => 
                (this.m_tmpConnectedEntity = (MyEntity) value);
        }

        public MyEntity Entity =>
            (base.Entity as MyEntity);

        [Obsolete]
        public float MaxRequiredInput
        {
            get => 
                this.MaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId);
            set => 
                this.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, value);
        }

        [Obsolete]
        public float RequiredInput =>
            this.RequiredInputByType(MyResourceDistributorComponent.ElectricityId);

        [Obsolete]
        public float SuppliedRatio =>
            this.SuppliedRatioByType(MyResourceDistributorComponent.ElectricityId);

        [Obsolete]
        public float CurrentInput =>
            this.CurrentInputByType(MyResourceDistributorComponent.ElectricityId);

        [Obsolete]
        public bool IsPowered =>
            this.IsPoweredByType(MyResourceDistributorComponent.ElectricityId);

        public override ListReader<MyDefinitionId> AcceptedResources =>
            new ListReader<MyDefinitionId>(this.m_resourceIds);

        public override string ComponentTypeDebugString =>
            "Resource Sink";

        [StructLayout(LayoutKind.Sequential)]
        private struct PerTypeData
        {
            public float CurrentInput;
            public float RequiredInput;
            public float MaxRequiredInput;
            public float SuppliedRatio;
            public Func<float> RequiredInputFunc;
            public bool IsPowered;
        }
    }
}

