namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;

    public class MyHudChangedInventoryItems
    {
        private const int GROUP_ITEMS_COUNT = 6;
        private const float TIME_TO_REMOVE_ITEM_SEC = 5f;
        private List<MyItemInfo> m_items = new List<MyItemInfo>();

        public MyHudChangedInventoryItems()
        {
            this.Visible = false;
        }

        public void AddChangedPhysicalInventoryItem(MyPhysicalInventoryItem intentoryItem, MyFixedPoint changedAmount, bool added)
        {
            MyDefinitionBase itemDefinition = intentoryItem.GetItemDefinition();
            if (itemDefinition != null)
            {
                if (changedAmount < 0)
                {
                    changedAmount = -changedAmount;
                }
                MyItemInfo item = new MyItemInfo {
                    DefinitionId = itemDefinition.Id,
                    Icons = itemDefinition.Icons,
                    TotalAmount = intentoryItem.Amount,
                    ChangedAmount = changedAmount,
                    Added = added
                };
                this.AddItem(item);
            }
        }

        private unsafe void AddItem(MyItemInfo item)
        {
            double totalSeconds = MySession.Static.ElapsedGameTime.TotalSeconds;
            if (this.m_items.Count > 0)
            {
                MyItemInfo info = this.m_items[this.m_items.Count - 1];
                if ((info.DefinitionId == item.DefinitionId) && (info.Added == item.Added))
                {
                    MyFixedPoint* pointPtr1 = (MyFixedPoint*) ref info.ChangedAmount;
                    pointPtr1[0] += item.ChangedAmount;
                    info.TotalAmount = item.TotalAmount;
                    if (this.m_items.Count <= 6)
                    {
                        info.RemoveTime = totalSeconds + 5.0;
                    }
                    this.m_items[this.m_items.Count - 1] = info;
                    return;
                }
                if (this.m_items.Count >= 6)
                {
                    int num2 = this.m_items.Count - 6;
                    totalSeconds = Math.Max(totalSeconds, this.m_items[num2].AddTime + 5.0);
                }
            }
            item.AddTime = totalSeconds;
            item.RemoveTime = totalSeconds + 5.0;
            this.m_items.Add(item);
        }

        public void Clear()
        {
            this.m_items.Clear();
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Show()
        {
            this.Visible = true;
        }

        public void Update()
        {
            double totalSeconds = MySession.Static.ElapsedGameTime.TotalSeconds;
            for (int i = this.m_items.Count - 1; i >= 0; i--)
            {
                ListReader<MyItemInfo> items = this.Items;
                if ((totalSeconds - items[i].RemoveTime) > 0.0)
                {
                    this.m_items.RemoveAt(i);
                }
            }
        }

        public ListReader<MyItemInfo> Items =>
            new ListReader<MyItemInfo>(this.m_items);

        public bool Visible { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyItemInfo
        {
            public MyDefinitionId DefinitionId;
            public string[] Icons;
            public MyFixedPoint ChangedAmount;
            public MyFixedPoint TotalAmount;
            public bool Added;
            public double AddTime;
            public double RemoveTime;
        }
    }
}

