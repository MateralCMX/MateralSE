namespace Sandbox.Game.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.ObjectBuilders;

    public class MyConstructionStockpile
    {
        private List<MyStockpileItem> m_items = new List<MyStockpileItem>();
        private static List<MyStockpileItem> m_syncItems = new List<MyStockpileItem>();

        public bool AddItems(int count, MyObjectBuilder_PhysicalObject physicalObject)
        {
            int num = 0;
            using (List<MyStockpileItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (enumerator.MoveNext() && !enumerator.Current.Content.CanStack(physicalObject))
                {
                    num++;
                }
            }
            if (num == this.m_items.Count)
            {
                if (count >= 0x7fffffff)
                {
                    return false;
                }
                MyStockpileItem item = new MyStockpileItem {
                    Amount = count,
                    Content = physicalObject
                };
                this.m_items.Add(item);
                this.AddSyncItem(item);
                return true;
            }
            if ((this.m_items[num].Amount + count) >= 0x7fffffffL)
            {
                return false;
            }
            MyStockpileItem item2 = new MyStockpileItem {
                Amount = this.m_items[num].Amount + count,
                Content = this.m_items[num].Content
            };
            this.m_items[num] = item2;
            MyStockpileItem diffItem = new MyStockpileItem {
                Content = this.m_items[num].Content,
                Amount = count
            };
            this.AddSyncItem(diffItem);
            return true;
        }

        public bool AddItems(int count, MyDefinitionId contentId, MyItemFlags flags = 0)
        {
            MyObjectBuilder_PhysicalObject physicalObject = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) contentId);
            if (physicalObject == null)
            {
                return false;
            }
            physicalObject.Flags = flags;
            return this.AddItems(count, physicalObject);
        }

        private void AddSyncItem(MyStockpileItem diffItem)
        {
            int num = 0;
            using (List<MyStockpileItem>.Enumerator enumerator = m_syncItems.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyStockpileItem current = enumerator.Current;
                    if (!current.Content.CanStack(diffItem.Content))
                    {
                        num++;
                        continue;
                    }
                    MyStockpileItem item2 = new MyStockpileItem {
                        Amount = current.Amount + diffItem.Amount,
                        Content = current.Content
                    };
                    m_syncItems[num] = item2;
                    return;
                }
            }
            m_syncItems.Add(diffItem);
        }

        internal unsafe void Change(List<MyStockpileItem> items)
        {
            int count = this.m_items.Count;
            foreach (MyStockpileItem item in items)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 < count)
                    {
                        if (!this.m_items[num2].Content.CanStack(item.Content))
                        {
                            num2++;
                            continue;
                        }
                        MyStockpileItem item2 = this.m_items[num2];
                        int* numPtr1 = (int*) ref item2.Amount;
                        numPtr1[0] += item.Amount;
                        this.m_items[num2] = item2;
                    }
                    if (num2 == count)
                    {
                        MyStockpileItem item3 = new MyStockpileItem {
                            Amount = item.Amount,
                            Content = item.Content
                        };
                        this.m_items.Add(item3);
                    }
                    break;
                }
            }
            for (int i = this.m_items.Count - 1; i >= 0; i--)
            {
                if (this.m_items[i].Amount == 0)
                {
                    this.m_items.RemoveAtFast<MyStockpileItem>(i);
                }
            }
        }

        public void ClearSyncList()
        {
            m_syncItems.Clear();
        }

        public int GetItemAmount(MyDefinitionId contentId, MyItemFlags flags = 0)
        {
            using (List<MyStockpileItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyStockpileItem current = enumerator.Current;
                    if (current.Content.CanStack(contentId.TypeId, contentId.SubtypeId, flags))
                    {
                        return current.Amount;
                    }
                }
            }
            return 0;
        }

        public List<MyStockpileItem> GetItems() => 
            this.m_items;

        public MyObjectBuilder_ConstructionStockpile GetObjectBuilder()
        {
            MyObjectBuilder_ConstructionStockpile stockpile = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ConstructionStockpile>();
            stockpile.Items = new MyObjectBuilder_StockpileItem[this.m_items.Count];
            for (int i = 0; i < this.m_items.Count; i++)
            {
                MyStockpileItem item = this.m_items[i];
                MyObjectBuilder_StockpileItem item2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_StockpileItem>();
                item2.Amount = item.Amount;
                item2.PhysicalContent = item.Content;
                stockpile.Items[i] = item2;
            }
            return stockpile;
        }

        public List<MyStockpileItem> GetSyncList() => 
            m_syncItems;

        public void Init(MyObjectBuilder_ConstructionStockpile objectBuilder)
        {
            this.m_items.Clear();
            if (objectBuilder != null)
            {
                foreach (MyObjectBuilder_StockpileItem item in objectBuilder.Items)
                {
                    if (item.Amount > 0)
                    {
                        MyStockpileItem item2 = new MyStockpileItem {
                            Amount = item.Amount,
                            Content = item.PhysicalContent
                        };
                        this.m_items.Add(item2);
                    }
                }
            }
        }

        public void Init(MyObjectBuilder_Inventory objectBuilder)
        {
            this.m_items.Clear();
            if (objectBuilder != null)
            {
                foreach (MyObjectBuilder_InventoryItem item in objectBuilder.Items)
                {
                    if (item.Amount > 0)
                    {
                        MyStockpileItem item2 = new MyStockpileItem {
                            Amount = (int) item.Amount,
                            Content = item.PhysicalContent
                        };
                        this.m_items.Add(item2);
                    }
                }
            }
        }

        public bool IsEmpty() => 
            (this.m_items.Count == 0);

        public bool RemoveItems(int count, MyObjectBuilder_PhysicalObject physicalObject) => 
            this.RemoveItems(count, physicalObject.GetId(), physicalObject.Flags);

        public bool RemoveItems(int count, MyDefinitionId id, MyItemFlags flags = 0)
        {
            int index = 0;
            using (List<MyStockpileItem>.Enumerator enumerator = this.m_items.GetEnumerator())
            {
                while (enumerator.MoveNext() && !enumerator.Current.Content.CanStack(id.TypeId, id.SubtypeId, flags))
                {
                    index++;
                }
            }
            return this.RemoveItemsInternal(index, count);
        }

        private unsafe bool RemoveItemsInternal(int index, int count)
        {
            if (index >= this.m_items.Count)
            {
                return false;
            }
            if (this.m_items[index].Amount == count)
            {
                MyStockpileItem item = this.m_items[index];
                MyStockpileItem* itemPtr1 = (MyStockpileItem*) ref item;
                itemPtr1->Amount = -item.Amount;
                this.AddSyncItem(item);
                this.m_items.RemoveAt(index);
                return true;
            }
            if (count >= this.m_items[index].Amount)
            {
                return false;
            }
            MyStockpileItem item2 = new MyStockpileItem {
                Amount = this.m_items[index].Amount - count,
                Content = this.m_items[index].Content
            };
            this.m_items[index] = item2;
            MyStockpileItem diffItem = new MyStockpileItem {
                Content = item2.Content,
                Amount = -count
            };
            this.AddSyncItem(diffItem);
            return true;
        }
    }
}

