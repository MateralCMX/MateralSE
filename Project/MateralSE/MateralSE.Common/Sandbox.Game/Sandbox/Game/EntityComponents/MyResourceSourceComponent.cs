namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyResourceSourceComponent : MyResourceSourceComponentBase
    {
        [CompilerGenerated]
        private MyResourceCapacityRemainingChangedDelegate HasCapacityRemainingChanged;
        [CompilerGenerated]
        private MyResourceCapacityRemainingChangedDelegate ProductionEnabledChanged;
        [CompilerGenerated]
        private MyResourceOutputChangedDelegate OutputChanged;
        [CompilerGenerated]
        private MyResourceOutputChangedDelegate MaxOutputChanged;
        private int m_allocatedTypeCount;
        private PerTypeData[] m_dataPerType;
        private bool m_enabled;
        private readonly StringBuilder m_textCache = new StringBuilder();
        [ThreadStatic]
        private static List<MyResourceSourceInfo> m_singleHelperList;
        private readonly Dictionary<MyDefinitionId, int> m_resourceTypeToIndex = new Dictionary<MyDefinitionId, int>(1, MyDefinitionId.Comparer);
        private readonly List<MyDefinitionId> m_resourceIds = new List<MyDefinitionId>(1);

        public event MyResourceCapacityRemainingChangedDelegate HasCapacityRemainingChanged
        {
            [CompilerGenerated] add
            {
                MyResourceCapacityRemainingChangedDelegate hasCapacityRemainingChanged = this.HasCapacityRemainingChanged;
                while (true)
                {
                    MyResourceCapacityRemainingChangedDelegate a = hasCapacityRemainingChanged;
                    MyResourceCapacityRemainingChangedDelegate delegate4 = (MyResourceCapacityRemainingChangedDelegate) Delegate.Combine(a, value);
                    hasCapacityRemainingChanged = Interlocked.CompareExchange<MyResourceCapacityRemainingChangedDelegate>(ref this.HasCapacityRemainingChanged, delegate4, a);
                    if (ReferenceEquals(hasCapacityRemainingChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyResourceCapacityRemainingChangedDelegate hasCapacityRemainingChanged = this.HasCapacityRemainingChanged;
                while (true)
                {
                    MyResourceCapacityRemainingChangedDelegate source = hasCapacityRemainingChanged;
                    MyResourceCapacityRemainingChangedDelegate delegate4 = (MyResourceCapacityRemainingChangedDelegate) Delegate.Remove(source, value);
                    hasCapacityRemainingChanged = Interlocked.CompareExchange<MyResourceCapacityRemainingChangedDelegate>(ref this.HasCapacityRemainingChanged, delegate4, source);
                    if (ReferenceEquals(hasCapacityRemainingChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event MyResourceOutputChangedDelegate MaxOutputChanged
        {
            [CompilerGenerated] add
            {
                MyResourceOutputChangedDelegate maxOutputChanged = this.MaxOutputChanged;
                while (true)
                {
                    MyResourceOutputChangedDelegate a = maxOutputChanged;
                    MyResourceOutputChangedDelegate delegate4 = (MyResourceOutputChangedDelegate) Delegate.Combine(a, value);
                    maxOutputChanged = Interlocked.CompareExchange<MyResourceOutputChangedDelegate>(ref this.MaxOutputChanged, delegate4, a);
                    if (ReferenceEquals(maxOutputChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyResourceOutputChangedDelegate maxOutputChanged = this.MaxOutputChanged;
                while (true)
                {
                    MyResourceOutputChangedDelegate source = maxOutputChanged;
                    MyResourceOutputChangedDelegate delegate4 = (MyResourceOutputChangedDelegate) Delegate.Remove(source, value);
                    maxOutputChanged = Interlocked.CompareExchange<MyResourceOutputChangedDelegate>(ref this.MaxOutputChanged, delegate4, source);
                    if (ReferenceEquals(maxOutputChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event MyResourceOutputChangedDelegate OutputChanged
        {
            [CompilerGenerated] add
            {
                MyResourceOutputChangedDelegate outputChanged = this.OutputChanged;
                while (true)
                {
                    MyResourceOutputChangedDelegate a = outputChanged;
                    MyResourceOutputChangedDelegate delegate4 = (MyResourceOutputChangedDelegate) Delegate.Combine(a, value);
                    outputChanged = Interlocked.CompareExchange<MyResourceOutputChangedDelegate>(ref this.OutputChanged, delegate4, a);
                    if (ReferenceEquals(outputChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyResourceOutputChangedDelegate outputChanged = this.OutputChanged;
                while (true)
                {
                    MyResourceOutputChangedDelegate source = outputChanged;
                    MyResourceOutputChangedDelegate delegate4 = (MyResourceOutputChangedDelegate) Delegate.Remove(source, value);
                    outputChanged = Interlocked.CompareExchange<MyResourceOutputChangedDelegate>(ref this.OutputChanged, delegate4, source);
                    if (ReferenceEquals(outputChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event MyResourceCapacityRemainingChangedDelegate ProductionEnabledChanged
        {
            [CompilerGenerated] add
            {
                MyResourceCapacityRemainingChangedDelegate productionEnabledChanged = this.ProductionEnabledChanged;
                while (true)
                {
                    MyResourceCapacityRemainingChangedDelegate a = productionEnabledChanged;
                    MyResourceCapacityRemainingChangedDelegate delegate4 = (MyResourceCapacityRemainingChangedDelegate) Delegate.Combine(a, value);
                    productionEnabledChanged = Interlocked.CompareExchange<MyResourceCapacityRemainingChangedDelegate>(ref this.ProductionEnabledChanged, delegate4, a);
                    if (ReferenceEquals(productionEnabledChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                MyResourceCapacityRemainingChangedDelegate productionEnabledChanged = this.ProductionEnabledChanged;
                while (true)
                {
                    MyResourceCapacityRemainingChangedDelegate source = productionEnabledChanged;
                    MyResourceCapacityRemainingChangedDelegate delegate4 = (MyResourceCapacityRemainingChangedDelegate) Delegate.Remove(source, value);
                    productionEnabledChanged = Interlocked.CompareExchange<MyResourceCapacityRemainingChangedDelegate>(ref this.ProductionEnabledChanged, delegate4, source);
                    if (ReferenceEquals(productionEnabledChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyResourceSourceComponent(int initialAllocationSize = 1)
        {
            this.AllocateData(initialAllocationSize);
        }

        private void AllocateData(int allocationSize)
        {
            this.m_dataPerType = new PerTypeData[allocationSize];
            this.m_allocatedTypeCount = allocationSize;
        }

        public override float CurrentOutputByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].CurrentOutput;

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
                    if (base.Entity != null)
                    {
                        MyRenderProxy.DebugDrawText3D(vectord, base.Entity.ToString(), Color.Yellow, (float) num3, true, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
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
                subtypeName = $"{resourceId.SubtypeName} Max:{data.MaxOutput} Current:{data.CurrentOutput} Remaining:{data.RemainingCapacity}";
            }
            MyRenderProxy.DebugDrawLine3D(origin, origin + vectord, Color.White, Color.White, false, false);
            MyRenderProxy.DebugDrawText3D(worldCoord, subtypeName, Color.White, textSize, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, -1, false);
        }

        public override float DefinedOutputByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].DefinedOutput;

        protected int GetTypeIndex(MyDefinitionId resourceTypeId)
        {
            int num = 0;
            if (this.m_resourceTypeToIndex.Count > 1)
            {
                num = this.m_resourceTypeToIndex[resourceTypeId];
            }
            return num;
        }

        public bool HasCapacityRemainingByType(MyDefinitionId resourceTypeId) => 
            (this.IsInfiniteCapacity || (MySession.Static.CreativeMode || this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].HasRemainingCapacity));

        public void Init(MyStringHash sourceGroup, MyResourceSourceInfo sourceResourceData)
        {
            MyUtils.Init<List<MyResourceSourceInfo>>(ref m_singleHelperList);
            m_singleHelperList.Add(sourceResourceData);
            this.Init(sourceGroup, m_singleHelperList);
            m_singleHelperList.Clear();
        }

        public void Init(MyStringHash sourceGroup, List<MyResourceSourceInfo> sourceResourceData)
        {
            this.Group = sourceGroup;
            List<MyResourceSourceInfo> list = sourceResourceData;
            bool local1 = (list != null) && (list.Count != 0);
            int allocationSize = local1 ? list.Count : 1;
            this.Enabled = true;
            if (allocationSize != this.m_allocatedTypeCount)
            {
                this.AllocateData(allocationSize);
            }
            int num2 = 0;
            if (!local1)
            {
                num2++;
                this.m_resourceTypeToIndex.Add(MyResourceDistributorComponent.ElectricityId, num2);
                this.m_resourceIds.Add(MyResourceDistributorComponent.ElectricityId);
            }
            else
            {
                foreach (MyResourceSourceInfo info in list)
                {
                    num2++;
                    this.m_resourceTypeToIndex.Add(info.ResourceTypeId, num2);
                    this.m_resourceIds.Add(info.ResourceTypeId);
                    this.m_dataPerType[num2 - 1].DefinedOutput = info.DefinedOutput;
                    this.SetOutputByType(info.ResourceTypeId, 0f);
                    this.SetMaxOutputByType(info.ResourceTypeId, this.m_dataPerType[this.GetTypeIndex(info.ResourceTypeId)].DefinedOutput);
                    this.SetProductionEnabledByType(info.ResourceTypeId, true);
                    this.m_dataPerType[num2 - 1].ProductionToCapacityMultiplier = (info.ProductionToCapacityMultiplier != 0f) ? info.ProductionToCapacityMultiplier : 1f;
                    if (info.IsInfiniteCapacity)
                    {
                        this.SetRemainingCapacityByType(info.ResourceTypeId, float.PositiveInfinity);
                    }
                }
            }
        }

        public override float MaxOutputByType(MyDefinitionId resourceTypeId)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            return this.MaxOutputLimitedByCapacity(typeIndex);
        }

        private float MaxOutputLimitedByCapacity(int typeIndex) => 
            Math.Min(this.m_dataPerType[typeIndex].MaxOutput, (this.m_dataPerType[typeIndex].RemainingCapacity * this.m_dataPerType[typeIndex].ProductionToCapacityMultiplier) * 60f);

        public void OnProductionEnabledChanged(MyDefinitionId? resId = new MyDefinitionId?())
        {
            if (resId != null)
            {
                if (this.ProductionEnabledChanged != null)
                {
                    this.ProductionEnabledChanged(resId.Value, this);
                }
            }
            else
            {
                foreach (MyDefinitionId id in this.m_resourceIds)
                {
                    if (this.ProductionEnabledChanged != null)
                    {
                        this.ProductionEnabledChanged(id, this);
                    }
                }
            }
        }

        public override bool ProductionEnabledByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].IsProducerEnabled;

        public float ProductionToCapacityMultiplierByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].ProductionToCapacityMultiplier;

        public float RemainingCapacityByType(MyDefinitionId resourceTypeId) => 
            this.m_dataPerType[this.GetTypeIndex(resourceTypeId)].RemainingCapacity;

        internal void SetEnabled(bool newValue, bool fireEvents = true)
        {
            if (this.m_enabled != newValue)
            {
                this.m_enabled = newValue;
                if (fireEvents)
                {
                    MyDefinitionId? resId = null;
                    this.OnProductionEnabledChanged(resId);
                }
                if (!this.m_enabled)
                {
                    foreach (MyDefinitionId id in this.m_resourceIds)
                    {
                        this.SetOutputByType(id, 0f);
                    }
                }
            }
        }

        private void SetHasCapacityRemainingByType(MyDefinitionId resourceTypeId, bool newHasCapacity)
        {
            if (!this.IsInfiniteCapacity)
            {
                int typeIndex = this.GetTypeIndex(resourceTypeId);
                if (this.m_dataPerType[typeIndex].HasRemainingCapacity != newHasCapacity)
                {
                    this.m_dataPerType[typeIndex].HasRemainingCapacity = newHasCapacity;
                    if (this.HasCapacityRemainingChanged != null)
                    {
                        this.HasCapacityRemainingChanged(resourceTypeId, this);
                    }
                    if (!newHasCapacity)
                    {
                        this.m_dataPerType[typeIndex].CurrentOutput = 0f;
                    }
                }
            }
        }

        public void SetMaxOutput(float newMaxOutput)
        {
            this.SetMaxOutputByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key, newMaxOutput);
        }

        public void SetMaxOutputByType(MyDefinitionId resourceTypeId, float newMaxOutput)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            if (this.m_dataPerType[typeIndex].MaxOutput != newMaxOutput)
            {
                float maxOutput = this.m_dataPerType[typeIndex].MaxOutput;
                this.m_dataPerType[typeIndex].MaxOutput = newMaxOutput;
                if (this.MaxOutputChanged != null)
                {
                    this.MaxOutputChanged(resourceTypeId, maxOutput, this);
                }
            }
        }

        public void SetOutput(float newOutput)
        {
            this.SetOutputByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key, newOutput);
        }

        public void SetOutputByType(MyDefinitionId resourceTypeId, float newOutput)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            float currentOutput = this.m_dataPerType[typeIndex].CurrentOutput;
            this.m_dataPerType[typeIndex].CurrentOutput = newOutput;
            if ((currentOutput != newOutput) && (this.OutputChanged != null))
            {
                this.OutputChanged(resourceTypeId, currentOutput, this);
            }
        }

        public void SetProductionEnabledByType(MyDefinitionId resourceTypeId, bool newProducerEnabled)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            this.m_dataPerType[typeIndex].IsProducerEnabled = newProducerEnabled;
            if ((this.m_dataPerType[typeIndex].IsProducerEnabled != newProducerEnabled) && (this.ProductionEnabledChanged != null))
            {
                this.ProductionEnabledChanged(resourceTypeId, this);
            }
            if (!newProducerEnabled)
            {
                this.SetOutputByType(resourceTypeId, 0f);
            }
        }

        public void SetRemainingCapacityByType(MyDefinitionId resourceTypeId, float newRemainingCapacity)
        {
            int typeIndex = this.GetTypeIndex(resourceTypeId);
            float oldOutput = this.MaxOutputLimitedByCapacity(typeIndex);
            this.m_dataPerType[typeIndex].RemainingCapacity = newRemainingCapacity;
            if (this.m_dataPerType[typeIndex].RemainingCapacity != newRemainingCapacity)
            {
                this.SetHasCapacityRemainingByType(resourceTypeId, newRemainingCapacity > 0f);
            }
            if ((this.MaxOutputChanged != null) && (this.MaxOutputLimitedByCapacity(typeIndex) != oldOutput))
            {
                this.MaxOutputChanged(resourceTypeId, oldOutput, this);
            }
        }

        public override string ToString()
        {
            this.m_textCache.Clear();
            this.m_textCache.AppendFormat("Enabled: {0}", this.Enabled).Append("; \n");
            this.m_textCache.Append("Output: ");
            MyValueFormatter.AppendWorkInBestUnit(this.CurrentOutput, this.m_textCache);
            this.m_textCache.Append("; \n");
            this.m_textCache.Append("Max Output: ");
            MyValueFormatter.AppendWorkInBestUnit(this.MaxOutput, this.m_textCache);
            this.m_textCache.Append("; \n");
            this.m_textCache.AppendFormat("ProductionEnabled: {0}", this.ProductionEnabled);
            return this.m_textCache.ToString();
        }

        public MyEntity TemporaryConnectedEntity { get; set; }

        public MyStringHash Group { get; private set; }

        public float CurrentOutput =>
            this.CurrentOutputByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public float MaxOutput =>
            this.MaxOutputByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public float DefinedOutput =>
            this.DefinedOutputByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public bool ProductionEnabled =>
            this.ProductionEnabledByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public float RemainingCapacity =>
            this.RemainingCapacityByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public bool IsInfiniteCapacity =>
            float.IsInfinity(this.RemainingCapacity);

        public float ProductionToCapacityMultiplier =>
            this.ProductionToCapacityMultiplierByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public bool Enabled
        {
            get => 
                this.m_enabled;
            set => 
                this.SetEnabled(value, true);
        }

        public bool HasCapacityRemaining =>
            this.HasCapacityRemainingByType(this.m_resourceTypeToIndex.FirstPair<MyDefinitionId, int>().Key);

        public ListReader<MyDefinitionId> ResourceTypes =>
            new ListReader<MyDefinitionId>(this.m_resourceIds);

        public override string ComponentTypeDebugString =>
            "Resource Source";

        [StructLayout(LayoutKind.Sequential)]
        private struct PerTypeData
        {
            public float CurrentOutput;
            public float MaxOutput;
            public float DefinedOutput;
            public float RemainingCapacity;
            public float ProductionToCapacityMultiplier;
            public bool HasRemainingCapacity;
            public bool IsProducerEnabled;
        }
    }
}

