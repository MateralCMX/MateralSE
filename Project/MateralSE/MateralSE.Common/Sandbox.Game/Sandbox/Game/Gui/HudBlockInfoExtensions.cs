namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;

    public static class HudBlockInfoExtensions
    {
        public static void AddComponentsForBlock(this MyHudBlockInfo blockInfo, MyCubeBlockDefinition definition)
        {
            for (int i = 0; i < definition.Components.Length; i++)
            {
                MyCubeBlockDefinition.Component component = definition.Components[i];
                MyHudBlockInfo.ComponentInfo item = new MyHudBlockInfo.ComponentInfo {
                    DefinitionId = component.Definition.Id,
                    ComponentName = component.Definition.DisplayNameText,
                    Icons = component.Definition.Icons,
                    TotalCount = component.Count
                };
                blockInfo.Components.Add(item);
            }
        }

        public static void InitBlockInfo(this MyHudBlockInfo blockInfo, MyCubeBlockDefinition definition)
        {
            blockInfo.BlockName = definition.DisplayNameText;
            blockInfo.SetContextHelp(definition);
            blockInfo.PCUCost = definition.PCU;
            blockInfo.BlockIcons = definition.Icons;
            blockInfo.BlockIntegrity = 0f;
            blockInfo.CriticalComponentIndex = definition.CriticalGroup;
            blockInfo.CriticalIntegrity = definition.CriticalIntegrityRatio;
            blockInfo.OwnershipIntegrity = definition.OwnershipIntegrityRatio;
            blockInfo.MissingComponentIndex = -1;
            blockInfo.GridSize = definition.CubeSize;
            blockInfo.Components.Clear();
            blockInfo.BlockBuiltBy = 0L;
        }

        public static void LoadDefinition(this MyHudBlockInfo blockInfo, MyCubeBlockDefinition definition, bool merge = true)
        {
            blockInfo.InitBlockInfo(definition);
            if (definition.MultiBlock == null)
            {
                blockInfo.AddComponentsForBlock(definition);
            }
            else
            {
                MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_MultiBlockDefinition), definition.MultiBlock);
                MyMultiBlockDefinition definition2 = MyDefinitionManager.Static.TryGetMultiBlockDefinition(id);
                if (definition2 != null)
                {
                    foreach (MyMultiBlockDefinition.MyMultiBlockPartDefinition definition3 in definition2.BlockDefinitions)
                    {
                        MyCubeBlockDefinition definition4 = null;
                        MyDefinitionManager.Static.TryGetDefinition<MyCubeBlockDefinition>(definition3.Id, out definition4);
                        if (definition4 != null)
                        {
                            blockInfo.AddComponentsForBlock(definition4);
                        }
                    }
                }
            }
            if (merge)
            {
                blockInfo.MergeSameComponents();
            }
        }

        public static void LoadDefinition(this MyHudBlockInfo blockInfo, MyCubeBlockDefinition definition, DictionaryReader<MyDefinitionId, int> materials, bool merge = true)
        {
            blockInfo.InitBlockInfo(definition);
            foreach (KeyValuePair<MyDefinitionId, int> pair in materials)
            {
                MyDefinitionBase base2 = MyDefinitionManager.Static.GetDefinition(pair.Key);
                MyHudBlockInfo.ComponentInfo item = new MyHudBlockInfo.ComponentInfo();
                if (base2 != null)
                {
                    item.DefinitionId = base2.Id;
                    item.ComponentName = base2.DisplayNameText;
                    item.Icons = base2.Icons;
                    item.TotalCount = pair.Value;
                }
                else
                {
                    MyPhysicalItemDefinition definition2 = null;
                    if (!MyDefinitionManager.Static.TryGetPhysicalItemDefinition(pair.Key, out definition2))
                    {
                        continue;
                    }
                    item.ComponentName = definition2.DisplayNameText;
                    item.Icons = definition2.Icons;
                    item.DefinitionId = definition2.Id;
                    item.TotalCount = 1;
                }
                blockInfo.Components.Add(item);
            }
            if (merge)
            {
                blockInfo.MergeSameComponents();
            }
        }

        public static unsafe void MergeSameComponents(this MyHudBlockInfo blockInfo)
        {
            int index = blockInfo.Components.Count - 1;
            while (index >= 0)
            {
                int num2 = index - 1;
                while (true)
                {
                    if (num2 >= 0)
                    {
                        if (!(blockInfo.Components[index].DefinitionId == blockInfo.Components[num2].DefinitionId))
                        {
                            num2--;
                            continue;
                        }
                        MyHudBlockInfo.ComponentInfo info = blockInfo.Components[num2];
                        int* numPtr1 = (int*) ref info.TotalCount;
                        numPtr1[0] += blockInfo.Components[index].TotalCount;
                        int* numPtr2 = (int*) ref info.MountedCount;
                        numPtr2[0] += blockInfo.Components[index].MountedCount;
                        int* numPtr3 = (int*) ref info.StockpileCount;
                        numPtr3[0] += blockInfo.Components[index].StockpileCount;
                        blockInfo.Components[num2] = info;
                        blockInfo.Components.RemoveAt(index);
                    }
                    index--;
                    break;
                }
            }
        }
    }
}

