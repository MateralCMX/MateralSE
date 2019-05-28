namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;

    public class MyComponentStack
    {
        public const float MOUNT_THRESHOLD = 1.525902E-05f;
        private readonly MyCubeBlockDefinition m_blockDefinition;
        private float m_buildIntegrity;
        private float m_integrity;
        private bool m_yieldLastComponent = true;
        private ushort m_topGroupIndex;
        private ushort m_topComponentIndex;
        [CompilerGenerated]
        private Action IsFunctionalChanged;

        public event Action IsFunctionalChanged
        {
            [CompilerGenerated] add
            {
                Action isFunctionalChanged = this.IsFunctionalChanged;
                while (true)
                {
                    Action a = isFunctionalChanged;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    isFunctionalChanged = Interlocked.CompareExchange<Action>(ref this.IsFunctionalChanged, action3, a);
                    if (ReferenceEquals(isFunctionalChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action isFunctionalChanged = this.IsFunctionalChanged;
                while (true)
                {
                    Action source = isFunctionalChanged;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    isFunctionalChanged = Interlocked.CompareExchange<Action>(ref this.IsFunctionalChanged, action3, source);
                    if (ReferenceEquals(isFunctionalChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyComponentStack(MyCubeBlockDefinition BlockDefinition, float integrityPercent, float buildPercent)
        {
            this.m_blockDefinition = BlockDefinition;
            float maxIntegrity = BlockDefinition.MaxIntegrity;
            this.BuildIntegrity = maxIntegrity * buildPercent;
            this.Integrity = maxIntegrity * integrityPercent;
            this.UpdateIndices();
            if (this.Integrity != 0f)
            {
                float topComponentIntegrity = this.GetTopComponentIntegrity();
                if (topComponentIntegrity < 1.525902E-05f)
                {
                    this.Integrity += 1.525902E-05f - topComponentIntegrity;
                }
                if (topComponentIntegrity > BlockDefinition.Components[this.m_topGroupIndex].Definition.MaxIntegrity)
                {
                    this.Integrity -= topComponentIntegrity - BlockDefinition.Components[this.m_topGroupIndex].Definition.MaxIntegrity;
                }
            }
        }

        public void ApplyDamage(float damage, MyConstructionStockpile outputStockpile = null)
        {
            this.UnmountInternal(damage, outputStockpile, true, false);
            float ratio = this.BuildIntegrity / this.Integrity;
            this.UpdateBuildIntegrityDown(ratio);
        }

        private static void CalculateIndicesInternal(float integrity, MyCubeBlockDefinition blockDef, ref int topGroupIndex, ref int topComponentIndex)
        {
            float num = integrity;
            MyCubeBlockDefinition.Component[] components = blockDef.Components;
            int index = 0;
            for (index = 0; index < components.Length; index++)
            {
                float num3 = components[index].Definition.MaxIntegrity * components[index].Count;
                if (num < num3)
                {
                    int num4 = (int) (num / ((float) components[index].Definition.MaxIntegrity));
                    if (((num - (components[index].Definition.MaxIntegrity * num4)) >= 7.629511E-06f) || (num4 == 0))
                    {
                        topGroupIndex = index;
                        topComponentIndex = num4;
                        return;
                    }
                    topGroupIndex = index;
                    topComponentIndex = num4 - 1;
                    return;
                }
                num -= num3;
                if (num < 7.629511E-06f)
                {
                    topGroupIndex = index;
                    topComponentIndex = components[index].Count - 1;
                    return;
                }
            }
        }

        public bool CanContinueBuild(MyInventoryBase inventory, MyConstructionStockpile stockpile)
        {
            if (this.IsFullIntegrity)
            {
                return false;
            }
            if (this.GetTopComponentIntegrity() >= this.m_blockDefinition.Components[this.m_topGroupIndex].Definition.MaxIntegrity)
            {
                int topGroupIndex = this.m_topGroupIndex;
                if (this.m_topComponentIndex == (this.m_blockDefinition.Components[topGroupIndex].Count - 1))
                {
                    topGroupIndex++;
                }
                MyComponentDefinition definition = this.m_blockDefinition.Components[topGroupIndex].Definition;
                if ((stockpile == null) || (stockpile.GetItemAmount(definition.Id, MyItemFlags.None) <= 0))
                {
                    return ((inventory != null) && (MyCubeBuilder.BuildComponent.GetItemAmountCombined(inventory, definition.Id) > 0));
                }
            }
            return true;
        }

        private void CheckFunctionalState(bool oldFunctionalState)
        {
            if ((this.IsFunctional != oldFunctionalState) && (this.IsFunctionalChanged != null))
            {
                this.IsFunctionalChanged();
            }
        }

        private bool CheckOrMountFirstComponent(MyConstructionStockpile stockpile = null)
        {
            if (this.Integrity <= 7.629511E-06f)
            {
                MyComponentDefinition definition = this.m_blockDefinition.Components[0].Definition;
                if ((stockpile != null) && !stockpile.RemoveItems(1, definition.Id, MyItemFlags.None))
                {
                    return false;
                }
                this.Integrity = 1.525902E-05f;
                this.UpdateBuildIntegrityUp();
            }
            return true;
        }

        public void DecreaseMountLevel(float unmountAmount, MyConstructionStockpile outputStockpile = null, bool useDefaultDeconstructEfficiency = false)
        {
            float ratio = this.BuildIntegrity / this.Integrity;
            this.UnmountInternal(unmountAmount, outputStockpile, false, useDefaultDeconstructEfficiency);
            this.UpdateBuildIntegrityDown(ratio);
        }

        public void DestroyCompletely()
        {
            this.BuildIntegrity = 0f;
            this.Integrity = 0f;
            this.UpdateIndices();
        }

        public void DisableLastComponentYield()
        {
            this.m_yieldLastComponent = false;
        }

        private float GetDeconstructionEfficiency(int groupIndex, bool useDefault) => 
            (useDefault ? 1f : this.m_blockDefinition.Components[groupIndex].Definition.DeconstructionEfficiency);

        public GroupInfo GetGroupInfo(int index)
        {
            MyCubeBlockDefinition.Component component = this.m_blockDefinition.Components[index];
            GroupInfo info = new GroupInfo {
                Component = component.Definition,
                TotalCount = component.Count,
                MountedCount = 0,
                AvailableCount = 0,
                Integrity = 0f,
                MaxIntegrity = component.Count * component.Definition.MaxIntegrity
            };
            if (index < this.m_topGroupIndex)
            {
                info.MountedCount = component.Count;
                info.Integrity = component.Count * component.Definition.MaxIntegrity;
            }
            else if (index == this.m_topGroupIndex)
            {
                info.MountedCount = this.m_topComponentIndex + 1;
                info.Integrity = this.GetTopComponentIntegrity() + (this.m_topComponentIndex * component.Definition.MaxIntegrity);
            }
            return info;
        }

        public void GetMissingComponents(Dictionary<string, int> addToDictionary, MyConstructionStockpile availableItems = null)
        {
            Dictionary<string, int> dictionary;
            string str2;
            int topGroupIndex = this.m_topGroupIndex;
            MyCubeBlockDefinition.Component component = this.m_blockDefinition.Components[topGroupIndex];
            int num2 = this.m_topComponentIndex + 1;
            if (this.IsFullyDismounted)
            {
                num2--;
            }
            if (num2 < component.Count)
            {
                string subtypeName = component.Definition.Id.SubtypeName;
                if (!addToDictionary.ContainsKey(subtypeName))
                {
                    addToDictionary[subtypeName] = component.Count - num2;
                }
                else
                {
                    dictionary = addToDictionary;
                    str2 = subtypeName;
                    dictionary[str2] += component.Count - num2;
                }
            }
            topGroupIndex++;
            while (topGroupIndex < this.m_blockDefinition.Components.Length)
            {
                component = this.m_blockDefinition.Components[topGroupIndex];
                string subtypeName = component.Definition.Id.SubtypeName;
                if (!addToDictionary.ContainsKey(subtypeName))
                {
                    addToDictionary[subtypeName] = component.Count;
                }
                else
                {
                    dictionary = addToDictionary;
                    str2 = subtypeName;
                    dictionary[str2] += component.Count;
                }
                topGroupIndex++;
            }
            if (availableItems != null)
            {
                for (topGroupIndex = 0; topGroupIndex < addToDictionary.Keys.Count; topGroupIndex++)
                {
                    string subtypeName = addToDictionary.Keys.ElementAt<string>(topGroupIndex);
                    dictionary = addToDictionary;
                    str2 = subtypeName;
                    dictionary[str2] -= availableItems.GetItemAmount(new MyDefinitionId(typeof(MyObjectBuilder_Component), subtypeName), MyItemFlags.None);
                    if (addToDictionary[subtypeName] <= 0)
                    {
                        addToDictionary.Remove(subtypeName);
                        topGroupIndex--;
                    }
                }
            }
        }

        public void GetMissingInfo(out int groupIndex, out int componentCount)
        {
            if (this.IsFullIntegrity)
            {
                groupIndex = 0;
                componentCount = 0;
            }
            else if (this.GetTopComponentIntegrity() < this.m_blockDefinition.Components[this.m_topGroupIndex].Definition.MaxIntegrity)
            {
                groupIndex = 0;
                componentCount = 0;
            }
            else
            {
                int num = this.m_topComponentIndex + 1;
                groupIndex = this.m_topGroupIndex;
                if (num == this.m_blockDefinition.Components[groupIndex].Count)
                {
                    groupIndex++;
                    num = 0;
                }
                componentCount = this.m_blockDefinition.Components[groupIndex].Count - num;
            }
        }

        public static void GetMountedComponents(MyComponentList addToList, MyObjectBuilder_CubeBlock block)
        {
            int topGroupIndex = 0;
            int topComponentIndex = 0;
            MyCubeBlockDefinition blockDefinition = null;
            MyDefinitionManager.Static.TryGetCubeBlockDefinition(block.GetId(), out blockDefinition);
            if ((blockDefinition != null) && (block != null))
            {
                float integrity = block.IntegrityPercent * blockDefinition.MaxIntegrity;
                CalculateIndicesInternal(integrity, blockDefinition, ref topGroupIndex, ref topComponentIndex);
                if ((topGroupIndex < blockDefinition.Components.Length) && (topComponentIndex < blockDefinition.Components[topGroupIndex].Count))
                {
                    int amount = topComponentIndex;
                    if (integrity >= 1.525902E-05f)
                    {
                        amount++;
                    }
                    for (int i = 0; i < topGroupIndex; i++)
                    {
                        MyCubeBlockDefinition.Component component = blockDefinition.Components[i];
                        addToList.AddMaterial(component.Definition.Id, component.Count, component.Count, false);
                    }
                    MyDefinitionId myDefinitionId = blockDefinition.Components[topGroupIndex].Definition.Id;
                    addToList.AddMaterial(myDefinitionId, amount, amount, false);
                }
            }
        }

        private float GetTopComponentIntegrity()
        {
            float integrity = this.Integrity;
            MyCubeBlockDefinition.Component[] components = this.m_blockDefinition.Components;
            for (int i = 0; i < this.m_topGroupIndex; i++)
            {
                integrity -= components[i].Definition.MaxIntegrity * components[i].Count;
            }
            return (integrity - (components[this.m_topGroupIndex].Definition.MaxIntegrity * this.m_topComponentIndex));
        }

        public void IncreaseMountLevel(float mountAmount, MyConstructionStockpile stockpile = null)
        {
            bool isFunctional = this.IsFunctional;
            this.IncreaseMountLevelInternal(mountAmount, stockpile);
        }

        private void IncreaseMountLevelInternal(float mountAmount, MyConstructionStockpile stockpile = null)
        {
            if (this.CheckOrMountFirstComponent(stockpile))
            {
                float topComponentIntegrity = this.GetTopComponentIntegrity();
                float maxIntegrity = this.m_blockDefinition.Components[this.m_topGroupIndex].Definition.MaxIntegrity;
                int topGroupIndex = this.m_topGroupIndex;
                int topComponentIndex = this.m_topComponentIndex;
                while (mountAmount > 0f)
                {
                    float num5 = maxIntegrity - topComponentIntegrity;
                    if (mountAmount < num5)
                    {
                        this.Integrity += mountAmount;
                        this.UpdateBuildIntegrityUp();
                        return;
                    }
                    this.Integrity += num5 + 1.525902E-05f;
                    mountAmount -= num5 + 1.525902E-05f;
                    if ((topComponentIndex + 1) >= this.m_blockDefinition.Components[this.m_topGroupIndex].Count)
                    {
                        topGroupIndex++;
                        topComponentIndex = 0;
                    }
                    if (topGroupIndex == this.m_blockDefinition.Components.Length)
                    {
                        this.Integrity = this.MaxIntegrity;
                        this.UpdateBuildIntegrityUp();
                        return;
                    }
                    MyComponentDefinition definition = this.m_blockDefinition.Components[topGroupIndex].Definition;
                    if ((stockpile != null) && !stockpile.RemoveItems(1, definition.Id, MyItemFlags.None))
                    {
                        this.Integrity -= 1.525902E-05f;
                        this.UpdateBuildIntegrityUp();
                        return;
                    }
                    this.UpdateBuildIntegrityUp();
                    this.SetTopIndex(topGroupIndex, topComponentIndex);
                    topComponentIntegrity = 1.525902E-05f;
                    maxIntegrity = this.m_blockDefinition.Components[topGroupIndex].Definition.MaxIntegrity;
                }
            }
        }

        internal void SetIntegrity(float buildIntegrity, float integrity)
        {
            this.Integrity = integrity;
            this.BuildIntegrity = buildIntegrity;
            this.UpdateIndices();
        }

        private void SetTopIndex(int newTopGroupIndex, int newTopComponentIndex)
        {
            this.m_topGroupIndex = (ushort) newTopGroupIndex;
            this.m_topComponentIndex = (ushort) newTopComponentIndex;
        }

        private void UnmountInternal(float unmountAmount, MyConstructionStockpile outputStockpile = null, bool damageItems = false, bool useDefaultDeconstructEfficiency = false)
        {
            float topComponentIntegrity = this.GetTopComponentIntegrity();
            int topGroupIndex = this.m_topGroupIndex;
            int topComponentIndex = this.m_topComponentIndex;
            MyObjectBuilder_PhysicalObject physicalObject = null;
            MyObjectBuilder_Ore scrapBuilder = MyFloatingObject.ScrapBuilder;
            while ((unmountAmount * this.GetDeconstructionEfficiency(topGroupIndex, damageItems | useDefaultDeconstructEfficiency)) >= topComponentIntegrity)
            {
                this.Integrity -= topComponentIntegrity;
                unmountAmount -= topComponentIntegrity;
                if ((outputStockpile != null) && MySession.Static.SurvivalMode)
                {
                    bool flag = damageItems && MyFakes.ENABLE_DAMAGED_COMPONENTS;
                    if (!damageItems || (flag && (MyRandom.Instance.NextFloat() <= this.m_blockDefinition.Components[topGroupIndex].Definition.DropProbability)))
                    {
                        physicalObject = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) this.m_blockDefinition.Components[topGroupIndex].DeconstructItem.Id);
                        if (flag)
                        {
                            physicalObject.Flags |= MyItemFlags.Damaged;
                        }
                        if ((this.Integrity > 0f) || this.m_yieldLastComponent)
                        {
                            outputStockpile.AddItems(1, physicalObject);
                        }
                    }
                    MyComponentDefinition definition = this.m_blockDefinition.Components[topGroupIndex].Definition;
                    if (((MyFakes.ENABLE_SCRAP & damageItems) && (MyRandom.Instance.NextFloat() < definition.DropProbability)) && ((this.Integrity > 0f) || this.m_yieldLastComponent))
                    {
                        outputStockpile.AddItems((int) (0.8f * definition.Mass), scrapBuilder);
                    }
                }
                topComponentIndex--;
                if (topComponentIndex < 0)
                {
                    topGroupIndex--;
                    if (topGroupIndex < 0)
                    {
                        this.SetTopIndex(0, 0);
                        this.Integrity = 0f;
                        return;
                    }
                    topComponentIndex = this.m_blockDefinition.Components[topGroupIndex].Count - 1;
                }
                topComponentIntegrity = this.m_blockDefinition.Components[topGroupIndex].Definition.MaxIntegrity;
                this.SetTopIndex(topGroupIndex, topComponentIndex);
            }
            this.Integrity -= unmountAmount * this.GetDeconstructionEfficiency(topGroupIndex, damageItems | useDefaultDeconstructEfficiency);
            topComponentIntegrity -= unmountAmount * this.GetDeconstructionEfficiency(topGroupIndex, damageItems | useDefaultDeconstructEfficiency);
            if (topComponentIntegrity < 1.525902E-05f)
            {
                this.Integrity += 1.525902E-05f - topComponentIntegrity;
                topComponentIntegrity = 1.525902E-05f;
            }
        }

        public void UpdateBuildIntegrityDown(float ratio)
        {
            if (this.BuildIntegrity > (this.Integrity * ratio))
            {
                this.BuildIntegrity = this.Integrity * ratio;
            }
        }

        public void UpdateBuildIntegrityUp()
        {
            if (this.BuildIntegrity < this.Integrity)
            {
                this.BuildIntegrity = this.Integrity;
            }
        }

        private void UpdateIndices()
        {
            int topGroupIndex = 0;
            int topComponentIndex = 0;
            CalculateIndicesInternal(this.Integrity, this.m_blockDefinition, ref topGroupIndex, ref topComponentIndex);
            this.SetTopIndex(topGroupIndex, topComponentIndex);
        }

        public bool? WillFunctionalityRise(float mountAmount)
        {
            bool flag = (this.Integrity + (mountAmount * this.m_blockDefinition.IntegrityPointsPerSec)) > (this.MaxIntegrity * this.m_blockDefinition.CriticalIntegrityRatio);
            if ((this.Integrity > (this.MaxIntegrity * this.m_blockDefinition.CriticalIntegrityRatio)) != flag)
            {
                return new bool?(flag);
            }
            return null;
        }

        public bool YieldLastComponent =>
            this.m_yieldLastComponent;

        public bool IsFullIntegrity =>
            (this.m_integrity >= this.MaxIntegrity);

        public bool IsFullyDismounted =>
            (this.m_integrity < 1.525902E-05f);

        public bool IsDestroyed =>
            (this.m_integrity < 1.525902E-05f);

        public float Integrity
        {
            get => 
                this.m_integrity;
            private set
            {
                if (this.m_integrity != value)
                {
                    bool isFunctional = this.IsFunctional;
                    this.m_integrity = value;
                    this.CheckFunctionalState(isFunctional);
                }
            }
        }

        public float IntegrityRatio =>
            (this.Integrity / this.MaxIntegrity);

        public float MaxIntegrity =>
            this.m_blockDefinition.MaxIntegrity;

        public float BuildRatio =>
            (this.m_buildIntegrity / this.MaxIntegrity);

        public float BuildIntegrity
        {
            get => 
                this.m_buildIntegrity;
            private set
            {
                if (this.m_buildIntegrity != value)
                {
                    bool isFunctional = this.IsFunctional;
                    this.m_buildIntegrity = value;
                    this.CheckFunctionalState(isFunctional);
                }
            }
        }

        public static float NewBlockIntegrity =>
            (MySession.Static.SurvivalMode ? 1.525902E-05f : 1f);

        public bool IsFunctional =>
            (this.IsBuilt && (this.Integrity > (this.MaxIntegrity * this.m_blockDefinition.CriticalIntegrityRatio)));

        public bool IsBuilt =>
            (this.BuildIntegrity >= (this.MaxIntegrity * this.m_blockDefinition.FinalModelThreshold()));

        public int GroupCount =>
            this.m_blockDefinition.Components.Length;

        [StructLayout(LayoutKind.Sequential)]
        public struct GroupInfo
        {
            public int MountedCount;
            public int TotalCount;
            public int AvailableCount;
            public float Integrity;
            public float MaxIntegrity;
            public MyComponentDefinition Component;
        }
    }
}

