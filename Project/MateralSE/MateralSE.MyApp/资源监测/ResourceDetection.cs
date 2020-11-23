using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace MateralSE.MyApp.资源监测
{
    public class ResourceDetection
    {
        private readonly Dictionary<string, ItemModel> _resources;
        public ResourceDetection(Dictionary<string, ItemModel> resources)
        {
            _resources = resources;
        }
        public string GetText(out Color lcdColor)
        {
            bool IsSeriousAlarm = _resources.Count(m => m.Value.IsSeriousAlarm) > 0;
            bool IsAlarm = _resources.Count(m => m.Value.IsAlarm) > 0;
            lcdColor = new Color(0, 255, 0);
            string lcdText;
            if (IsAlarm)
            {
                lcdColor.G = Convert.ToByte(IsSeriousAlarm ? 0 : 255);
                lcdColor.R = 255;
                lcdText = string.Join("\r\n", _resources.Where(m => m.Value.IsAlarm).Select(m => m.Value.AlarmText));
            }
            else
            {
                lcdText = "库存充足";
            }
            return lcdText;
        }
        public void UpdateInventory<T>(IEnumerable<T> inventories, Action<MyInventoryItem> itemHandler = null) where T : IMyCubeBlock
        {
            foreach (KeyValuePair<string, ItemModel> resource in _resources)
            {
                resource.Value.Inventory = 0;
            }
            inventories = inventories.Where(m => m.HasInventory).ToList();
            foreach (T inventoryBlock in inventories)
            {
                for (var i = 0; i < inventoryBlock.InventoryCount; i++)
                {
                    var inventoryItems = new List<MyInventoryItem>();
                    inventoryBlock.GetInventory(i).GetItems(inventoryItems);
                    foreach (MyInventoryItem inventoryItem in inventoryItems)
                    {
                        itemHandler?.Invoke(inventoryItem);
                        if (!_resources.Keys.Contains(inventoryItem.Type.SubtypeId)) continue;
                        if (_resources[inventoryItem.Type.SubtypeId].TypeID != inventoryItem.Type.TypeId) continue;
                        _resources[inventoryItem.Type.SubtypeId].Inventory += (float)inventoryItem.Amount;
                    }
                }
            }
        }
    }
    public class ItemModel
    {
        private readonly string _name;
        private readonly int _alertValue;
        public string TypeID { get; }
        public float Inventory { get; set; }
        public bool IsAlarm => Inventory <= _alertValue;
        public bool IsSeriousAlarm => Inventory / _alertValue < 0.25;
        public string AlarmText
        {
            get
            {
                string result = string.Empty;
                if (!IsAlarm) return result;
                string describe = IsSeriousAlarm ? "库存严重不足" : "库存不足";
                result = _alertValue > 999 ? $"{_name}:{Math.Round(Inventory / 1000, 2)}K/{_alertValue / 1000}K[{describe}]" : $"{_name}:{Math.Round(Inventory, 2)}/{_alertValue}[{describe}]";
                return result;
            }
        }
        public ItemModel(string name, string typeID, int alertValue)
        {
            _name = name;
            TypeID = typeID;
            _alertValue = alertValue;
        }
    }
}
