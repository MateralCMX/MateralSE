namespace Sandbox.Game.Entities.Inventory
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Utils;

    public static class MyInventoryBaseExtensions
    {
        private static List<MyComponentBase> m_tmpList = new List<MyComponentBase>();

        public static MyInventoryBase GetInventory(this MyEntity entity, MyStringHash inventoryId)
        {
            MyInventoryBase base2 = null;
            MyStringHash hash;
            base2 = entity.Components.Get<MyInventoryBase>();
            if (base2 != null)
            {
                hash = base2.InventoryId;
                if (inventoryId.Equals(MyStringHash.GetOrCompute(hash.ToString())))
                {
                    return base2;
                }
            }
            if (base2 is MyInventoryAggregate)
            {
                m_tmpList.Clear();
                (base2 as MyInventoryAggregate).GetComponentsFlattened(m_tmpList);
                using (List<MyComponentBase>.Enumerator enumerator = m_tmpList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyInventoryBase current = enumerator.Current as MyInventoryBase;
                        hash = current.InventoryId;
                        if (inventoryId.Equals(MyStringHash.GetOrCompute(hash.ToString())))
                        {
                            return current;
                        }
                    }
                }
            }
            return null;
        }
    }
}

