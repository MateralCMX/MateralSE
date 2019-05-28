namespace Sandbox.Game.Entities.Inventory
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.SessionComponents;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Generics;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyComponentCombiner
    {
        private MyDynamicObjectPool<List<int>> m_listAllocator = new MyDynamicObjectPool<List<int>>(2);
        private Dictionary<MyDefinitionId, List<int>> m_groups = new Dictionary<MyDefinitionId, List<int>>();
        private Dictionary<int, int> m_presentItems = new Dictionary<int, int>();
        private int m_totalItemCounter;
        private int m_solvedItemCounter;
        private List<MyComponentChange> m_solution = new List<MyComponentChange>();
        private static Dictionary<MyDefinitionId, MyFixedPoint> m_componentCounts = new Dictionary<MyDefinitionId, MyFixedPoint>();

        private void AddChangeToSolution(MyDefinitionId removedComponentId, MyDefinitionId addedComponentId, int numChanged)
        {
            for (int i = 0; i < this.m_solution.Count; i++)
            {
                MyComponentChange change = this.m_solution[i];
                if ((change.IsChange() || change.IsAddition()) && (change.ToAdd == removedComponentId))
                {
                    int num2 = change.Amount - numChanged;
                    int amount = Math.Min(numChanged, change.Amount);
                    numChanged -= change.Amount;
                    if (num2 <= 0)
                    {
                        this.m_solution.RemoveAtFast<MyComponentChange>(i);
                    }
                    else
                    {
                        change.Amount = num2;
                        this.m_solution[i] = change;
                    }
                    if (change.IsChange())
                    {
                        this.m_solution.Add(MyComponentChange.CreateChange(change.ToRemove, addedComponentId, amount));
                    }
                    else
                    {
                        this.m_solution.Add(MyComponentChange.CreateAddition(addedComponentId, amount));
                    }
                    if (numChanged <= 0)
                    {
                        break;
                    }
                }
            }
            if (numChanged > 0)
            {
                this.m_solution.Add(MyComponentChange.CreateChange(removedComponentId, addedComponentId, numChanged));
            }
        }

        public void AddItem(MyDefinitionId groupId, int itemValue, int amount)
        {
            List<int> list = null;
            MyComponentGroupDefinition componentGroup = MyDefinitionManager.Static.GetComponentGroup(groupId);
            if (componentGroup != null)
            {
                if (!this.m_groups.TryGetValue(groupId, out list))
                {
                    list = this.m_listAllocator.Allocate();
                    list.Clear();
                    int num = 0;
                    while (true)
                    {
                        if (num > componentGroup.GetComponentNumber())
                        {
                            this.m_groups.Add(groupId, list);
                            break;
                        }
                        list.Add(0);
                        num++;
                    }
                }
                List<int> list2 = list;
                int num2 = itemValue;
                list2[num2] += amount;
                this.m_totalItemCounter += amount;
            }
        }

        private void AddPresentItems(int itemValue, int addCount)
        {
            int num = 0;
            this.m_presentItems.TryGetValue(itemValue, out num);
            num += addCount;
            this.m_presentItems[itemValue] = num;
        }

        private void AddRemovalToSolution(MyDefinitionId removedComponentId, int removeCount)
        {
            for (int i = 0; i < this.m_solution.Count; i++)
            {
                MyComponentChange change = this.m_solution[i];
                if ((change.IsChange() || change.IsAddition()) && (change.ToAdd == removedComponentId))
                {
                    int num2 = change.Amount - removeCount;
                    int amount = Math.Min(removeCount, change.Amount);
                    removeCount -= change.Amount;
                    if (num2 <= 0)
                    {
                        this.m_solution.RemoveAtFast<MyComponentChange>(i);
                    }
                    else
                    {
                        change.Amount = num2;
                        this.m_solution[i] = change;
                    }
                    if (change.IsChange())
                    {
                        this.m_solution.Add(MyComponentChange.CreateRemoval(change.ToRemove, amount));
                    }
                    if (removeCount <= 0)
                    {
                        break;
                    }
                }
            }
            if (removeCount > 0)
            {
                this.m_solution.Add(MyComponentChange.CreateRemoval(removedComponentId, removeCount));
            }
        }

        public bool CanCombineItems(MyInventoryBase inventory, DictionaryReader<MyDefinitionId, int> items)
        {
            bool flag = true;
            this.Clear();
            inventory.CountItems(m_componentCounts);
            foreach (KeyValuePair<MyDefinitionId, int> pair in items)
            {
                int amount = 0;
                int num2 = pair.Value;
                MyComponentGroupDefinition groupForComponent = null;
                groupForComponent = MyDefinitionManager.Static.GetGroupForComponent(pair.Key, out amount);
                if (groupForComponent != null)
                {
                    this.AddItem(groupForComponent.Id, amount, num2);
                    continue;
                }
                if ((MySessionComponentEquivalency.Static != null) && MySessionComponentEquivalency.Static.HasEquivalents(pair.Key))
                {
                    if (MySessionComponentEquivalency.Static.IsProvided(m_componentCounts, pair.Key, pair.Value))
                    {
                        continue;
                    }
                    flag = false;
                }
                else
                {
                    MyFixedPoint point;
                    if (!m_componentCounts.TryGetValue(pair.Key, out point))
                    {
                        flag = false;
                    }
                    else
                    {
                        if (point >= num2)
                        {
                            continue;
                        }
                        flag = false;
                    }
                }
                break;
            }
            if (flag)
            {
                flag &= this.Solve(m_componentCounts);
            }
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                if (!flag)
                {
                    MyRenderProxy.DebugDrawText2D(new Vector2(0f, 0f), "Can not build", Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                else
                {
                    List<MyComponentChange> changes = null;
                    this.GetSolution(out changes);
                    float y = 0f;
                    foreach (MyComponentChange change in changes)
                    {
                        string text = "";
                        if (change.IsAddition())
                        {
                            string[] textArray1 = new string[] { text, "+ ", change.Amount.ToString(), "x", change.ToAdd.ToString() };
                            text = string.Concat(textArray1);
                            MyRenderProxy.DebugDrawText2D(new Vector2(0f, y), text, Color.Green, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 20f;
                            continue;
                        }
                        if (change.IsRemoval())
                        {
                            string[] textArray2 = new string[] { text, "- ", change.Amount.ToString(), "x", change.ToRemove.ToString() };
                            text = string.Concat(textArray2);
                            MyRenderProxy.DebugDrawText2D(new Vector2(0f, y), text, Color.Red, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                            y += 20f;
                            continue;
                        }
                        string[] textArray3 = new string[] { text, "- ", change.Amount.ToString(), "x", change.ToRemove.ToString() };
                        text = string.Concat(textArray3);
                        MyRenderProxy.DebugDrawText2D(new Vector2(0f, y), text, Color.Orange, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 20f;
                        text = "";
                        string[] textArray4 = new string[] { text, "+ ", change.Amount.ToString(), "x", change.ToAdd.ToString() };
                        text = string.Concat(textArray4);
                        MyRenderProxy.DebugDrawText2D(new Vector2(0f, y), text, Color.Orange, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        y += 20f;
                    }
                }
            }
            return flag;
        }

        public void Clear()
        {
            foreach (KeyValuePair<MyDefinitionId, List<int>> pair in this.m_groups)
            {
                pair.Value.Clear();
                this.m_listAllocator.Deallocate(pair.Value);
            }
            this.m_groups.Clear();
            this.m_totalItemCounter = 0;
            this.m_solvedItemCounter = 0;
            m_componentCounts.Clear();
        }

        public MyFixedPoint GetItemAmountCombined(MyInventoryBase inventory, MyDefinitionId contentId)
        {
            if (inventory == null)
            {
                return 0;
            }
            int amount = 0;
            MyComponentGroupDefinition groupForComponent = MyDefinitionManager.Static.GetGroupForComponent(contentId, out amount);
            if (groupForComponent == null)
            {
                return (amount + inventory.GetItemAmount(contentId, MyItemFlags.None, true));
            }
            this.Clear();
            inventory.CountItems(m_componentCounts);
            this.AddItem(groupForComponent.Id, amount, 0x7fffffff);
            this.Solve(m_componentCounts);
            return this.GetSolvedItemCount();
        }

        public void GetSolution(out List<MyComponentChange> changes)
        {
            changes = this.m_solution;
        }

        private int GetSolvedItemCount() => 
            this.m_solvedItemCounter;

        public void RemoveItemsCombined(MyInventoryBase inventory, DictionaryReader<MyDefinitionId, int> toRemove)
        {
            this.Clear();
            foreach (KeyValuePair<MyDefinitionId, int> pair in toRemove)
            {
                int amount = 0;
                MyComponentGroupDefinition groupForComponent = MyDefinitionManager.Static.GetGroupForComponent(pair.Key, out amount);
                if (groupForComponent != null)
                {
                    this.AddItem(groupForComponent.Id, amount, pair.Value);
                    continue;
                }
                if ((MySessionComponentEquivalency.Static != null) && MySessionComponentEquivalency.Static.HasEquivalents(pair.Key))
                {
                    HashSet<MyDefinitionId> equivalents = MySessionComponentEquivalency.Static.GetEquivalents(pair.Key);
                    if (equivalents == null)
                    {
                        continue;
                    }
                    int num2 = pair.Value;
                    foreach (MyDefinitionId id in equivalents)
                    {
                        if (num2 <= 0)
                        {
                            break;
                        }
                        num2 -= (int) inventory.RemoveItemsOfType(num2, id, MyItemFlags.None, false);
                    }
                    continue;
                }
                inventory.RemoveItemsOfType(pair.Value, pair.Key, MyItemFlags.None, false);
            }
            inventory.CountItems(m_componentCounts);
            this.Solve(m_componentCounts);
            inventory.ApplyChanges(this.m_solution);
        }

        public bool Solve(Dictionary<MyDefinitionId, MyFixedPoint> componentCounts)
        {
            this.m_solution.Clear();
            this.m_solvedItemCounter = 0;
            foreach (KeyValuePair<MyDefinitionId, List<int>> pair in this.m_groups)
            {
                MyComponentGroupDefinition componentGroup = MyDefinitionManager.Static.GetComponentGroup(pair.Key);
                List<int> list = pair.Value;
                this.UpdatePresentItems(componentGroup, componentCounts);
                int itemValue = 1;
                while (true)
                {
                    if (itemValue > componentGroup.GetComponentNumber())
                    {
                        int componentNumber = componentGroup.GetComponentNumber();
                        while (true)
                        {
                            if (componentNumber < 1)
                            {
                                for (int i = 1; i <= componentGroup.GetComponentNumber(); i++)
                                {
                                    int num8 = list[i];
                                    if (num8 > 0)
                                    {
                                        int num9 = this.TryCreatingItemsByMerge(componentGroup, i, num8);
                                        list[i] = num8 - num9;
                                        this.m_solvedItemCounter += num9;
                                    }
                                }
                                break;
                            }
                            int itemCount = list[componentNumber];
                            int num6 = this.TryCreatingItemsBySplit(componentGroup, componentNumber, itemCount);
                            list[componentNumber] = itemCount - num6;
                            this.m_solvedItemCounter += num6;
                            componentNumber--;
                        }
                        break;
                    }
                    int removeCount = list[itemValue];
                    int num3 = this.TryRemovePresentItems(itemValue, removeCount);
                    if (num3 > 0)
                    {
                        this.AddRemovalToSolution(componentGroup.GetComponentDefinition(itemValue).Id, num3);
                        list[itemValue] = Math.Max(0, removeCount - num3);
                    }
                    this.m_solvedItemCounter += num3;
                    itemValue++;
                }
            }
            return (this.m_totalItemCounter == this.m_solvedItemCounter);
        }

        private int SplitHelper(MyComponentGroupDefinition group, int splitItemValue, int resultItemValue, int numItemsToSplit, int splitCount)
        {
            int itemValue = splitItemValue - (splitCount * resultItemValue);
            MyDefinitionId removedComponentId = group.GetComponentDefinition(splitItemValue).Id;
            if (itemValue == 0)
            {
                this.AddRemovalToSolution(removedComponentId, numItemsToSplit);
            }
            else
            {
                this.AddPresentItems(itemValue, numItemsToSplit);
                this.AddChangeToSolution(removedComponentId, group.GetComponentDefinition(itemValue).Id, numItemsToSplit);
            }
            return (splitCount * numItemsToSplit);
        }

        private int TryCreatingItemsByMerge(MyComponentGroupDefinition group, int itemValue, int itemCount)
        {
            int num;
            List<int> item = this.m_listAllocator.Allocate();
            item.Clear();
            int num2 = 0;
            while (true)
            {
                if (num2 <= group.GetComponentNumber())
                {
                    item.Add(0);
                    num2++;
                    continue;
                }
                num = 0;
                int num3 = 0;
                while (true)
                {
                    if (num3 >= itemCount)
                    {
                        break;
                    }
                    int num4 = itemValue;
                    int key = itemValue - 1;
                    while (true)
                    {
                        if (key >= 1)
                        {
                            int num6 = 0;
                            if (this.m_presentItems.TryGetValue(key, out num6))
                            {
                                int num7 = Math.Min(num4 / key, num6);
                                if (num7 > 0)
                                {
                                    num4 -= key * num7;
                                    num6 -= num7;
                                    List<int> list2 = item;
                                    int num8 = key;
                                    list2[num8] += num7;
                                }
                            }
                            key--;
                            continue;
                        }
                        if (num4 != itemValue)
                        {
                            if (num4 != 0)
                            {
                                for (int i = num4 + 1; i <= group.GetComponentNumber(); i++)
                                {
                                    int num10 = 0;
                                    this.m_presentItems.TryGetValue(i, out num10);
                                    if (num10 > item[i])
                                    {
                                        MyDefinitionId removedComponentId = group.GetComponentDefinition(i).Id;
                                        this.AddChangeToSolution(removedComponentId, group.GetComponentDefinition(i - num4).Id, 1);
                                        this.TryRemovePresentItems(i, 1);
                                        this.AddPresentItems(i - num4, 1);
                                        num4 = 0;
                                        break;
                                    }
                                }
                            }
                            if (num4 == 0)
                            {
                                num++;
                                for (int i = 1; i <= group.GetComponentNumber(); i++)
                                {
                                    if (item[i] > 0)
                                    {
                                        MyDefinitionId id = group.GetComponentDefinition(i).Id;
                                        this.TryRemovePresentItems(i, item[i]);
                                        this.AddRemovalToSolution(id, item[i]);
                                        item[i] = 0;
                                    }
                                }
                                break;
                            }
                            if (num4 <= 0)
                            {
                                break;
                            }
                        }
                        break;
                    }
                    num3++;
                }
                break;
            }
            this.m_listAllocator.Deallocate(item);
            return num;
        }

        private int TryCreatingItemsBySplit(MyComponentGroupDefinition group, int itemValue, int itemCount)
        {
            int num = 0;
            for (int i = itemValue + 1; i <= group.GetComponentNumber(); i++)
            {
                int splitCount = i / itemValue;
                int num4 = itemCount / splitCount;
                int num5 = itemCount % splitCount;
                int num7 = this.TryRemovePresentItems(i, num4 + ((num5 == 0) ? 0 : 1));
                if (num7 > 0)
                {
                    int numItemsToSplit = Math.Min(num7, num4);
                    if (numItemsToSplit != 0)
                    {
                        int num9 = this.SplitHelper(group, i, itemValue, numItemsToSplit, splitCount);
                        num += num9;
                        itemCount -= num9;
                    }
                    if ((num7 - num4) > 0)
                    {
                        int num10 = this.SplitHelper(group, i, itemValue, 1, num5);
                        num += num10;
                        itemCount -= num10;
                    }
                }
            }
            return num;
        }

        private int TryRemovePresentItems(int itemValue, int removeCount)
        {
            int num = 0;
            this.m_presentItems.TryGetValue(itemValue, out num);
            if (num > removeCount)
            {
                this.m_presentItems[itemValue] = num - removeCount;
                return removeCount;
            }
            this.m_presentItems.Remove(itemValue);
            return num;
        }

        private void UpdatePresentItems(MyComponentGroupDefinition group, Dictionary<MyDefinitionId, MyFixedPoint> componentCounts)
        {
            this.m_presentItems.Clear();
            for (int i = 1; i <= group.GetComponentNumber(); i++)
            {
                MyComponentDefinition componentDefinition = group.GetComponentDefinition(i);
                MyFixedPoint point = 0;
                componentCounts.TryGetValue(componentDefinition.Id, out point);
                this.m_presentItems[i] = (int) point;
            }
        }
    }
}

