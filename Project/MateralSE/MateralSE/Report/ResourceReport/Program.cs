﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace MateralSE.Report.ResourceReport
{
    public class Program : MyGridProgram
    {
        //CustomData:主要文本面板名称1&0|主要文本面板名称2|
        /// <summary>
        /// 矿石字典
        /// </summary>
        private static readonly Dictionary<string, ItemDefaultModel> OreDic = new Dictionary<string, ItemDefaultModel>
        {
            ["Ice"] = new ItemDefaultModel("冰", 100000)
        };
        /// <summary>
        /// 矿锭字典
        /// </summary>
        private static readonly Dictionary<string, ItemDefaultModel> IngotDic = new Dictionary<string, ItemDefaultModel>
        {
            ["Stone"] = new ItemDefaultModel("碎石", 40000),
            ["Iron"] = new ItemDefaultModel("铁锭", 200000),
            ["Silicon"] = new ItemDefaultModel("硅片", 100000),
            ["Nickel"] = new ItemDefaultModel("镍锭", 100000),
            ["Cobalt"] = new ItemDefaultModel("钴锭", 60000),
            ["Magnesium"] = new ItemDefaultModel("镁粉", 1000),
            ["Gold"] = new ItemDefaultModel("金锭", 1000),
            ["Silver"] = new ItemDefaultModel("银锭", 10000),
            //["Platinum"] = new ItemDefaultModel("铂锭", 1000),
            //["Uranium"] = new ItemDefaultModel("铀棒", 1000)
        };
        /// <summary>
        /// 组件字典
        /// </summary>
        private static readonly Dictionary<string, ItemDefaultModel> ComponentDic = new Dictionary<string, ItemDefaultModel>
        {
            ["SteelPlate"] = new ItemDefaultModel("钢板", 20000),
            ["Construction"] = new ItemDefaultModel("结构零件", 20000),
            ["InteriorPlate"] = new ItemDefaultModel("内衬板", 20000),
            ["Computer"] = new ItemDefaultModel("计算机", 20000),
            ["SmallTube"] = new ItemDefaultModel("小钢管", 20000),
            ["LargeTube"] = new ItemDefaultModel("大钢管", 10000),
            ["Display"] = new ItemDefaultModel("显示器", 10000),
            ["Motor"] = new ItemDefaultModel("马达", 20000),
            ["MetalGrid"] = new ItemDefaultModel("金属网格", 20000),
            ["PowerCell"] = new ItemDefaultModel("动力电池", 20000),
            ["BulletproofGlass"] = new ItemDefaultModel("防弹玻璃", 10000),
            ["Girder"] = new ItemDefaultModel("梁", 10000),
            ["Medical"] = new ItemDefaultModel("医疗零件", 200),
            ["SolarCell"] = new ItemDefaultModel("太阳能电池板", 10000),
            ["Detector"] = new ItemDefaultModel("探测器零件", 1000),
            //["Thrust"] = new ItemDefaultModel("推进器零件", 1000),
            ["RadioCommunication"] = new ItemDefaultModel("无线电零件", 1000),
            ["Explosives"] = new ItemDefaultModel("爆炸物", 1000),
            ["Superconductor"] = new ItemDefaultModel("超导体", 1000),
            ["Reactor"] = new ItemDefaultModel("反应堆零件", 1000),
            ["GravityGenerator"] = new ItemDefaultModel("重力发生器零件", 500),
            ["Canvas"] = new ItemDefaultModel("帆布", 200),
        };
        /// <summary>
        /// 弹药字典
        /// </summary>
        private static readonly Dictionary<string, ItemDefaultModel> AmmoMagazineDic = new Dictionary<string, ItemDefaultModel>
        {
            ["NATO_5p56x45mm"] = new ItemDefaultModel("5.56x45mm弹夹", 1000),
            ["NATO_25x184mm"] = new ItemDefaultModel("25X184mm子弹", 1000),
            //["Missile200mm"] = new ItemDefaultModel("导弹", 1000)
        };
        private InventoryModel _inventory;
        private TextSurfacesModel _mainTextSurfaces;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        public void Main()
        {
            InitBlocks();
            string mainTextSurfaceText = GetMainTextSurfaceText();
            _mainTextSurfaces.WriteText(mainTextSurfaceText);
        }
        /// <summary>
        /// 获得主要文本面板文本
        /// </summary>
        /// <returns></returns>
        private string GetMainTextSurfaceText()
        {
            if (_inventory.Items.All(m => !m.IsAlarm)) return "库存充足";
            return _inventory.Items.Where(m => m.IsAlarm).OrderBy(m=>m.Ratio).Aggregate("", (current, item) => current + $"{item.Name}：{item.AmountText}[ {item.LackAmountText} ]\r\n");
        }
        /// <summary>
        /// 初始化方块
        /// </summary>
        private void InitBlocks()
        {
            var _config = new ConfigModel(Me.CustomData);
            InitTextSurface(_config.MainTextSurfaceNames);
            InitItems();
        }
        /// <summary>
        /// 初始化主要文本面板
        /// </summary>
        /// <param name="mainTextSurfaceNames"></param>
        private void InitTextSurface(IReadOnlyCollection<string> mainTextSurfaceNames)
        {
            var textSurfaces = new List<IMyTextSurface>();
            foreach (string mainTextSurfaceName in mainTextSurfaceNames)
            {
                string[] trueNames = mainTextSurfaceName.Split('&');
                if (trueNames.Length == 1)
                {
                    var textSurface = GridTerminalSystem.GetBlockWithName(trueNames[0]) as IMyTextSurface;
                    textSurfaces.Add(textSurface);
                }
                else if (trueNames.Length == 2)
                {
                    var index = Convert.ToInt32(trueNames[1]);
                    var textSurfaceProvider = GridTerminalSystem.GetBlockWithName(trueNames[0]) as IMyTextSurfaceProvider;
                    if (textSurfaceProvider != null && textSurfaceProvider.SurfaceCount >= index)
                    {
                        textSurfaces.Add(textSurfaceProvider.GetSurface(index));
                    }
                }
            }
            textSurfaces.Add(Me.GetSurface(0));
            _mainTextSurfaces = new TextSurfacesModel(textSurfaces);
        }
        /// <summary>
        /// 初始化物品
        /// </summary>
        private void InitItems()
        {
            _inventory = new InventoryModel();
            IEnumerable<IMyTerminalBlock> terminalBlocks = GetHasInventoryBlocks();
            foreach (IMyTerminalBlock terminalBlock in terminalBlocks)
            {
                for (var i = 0; i < terminalBlock.InventoryCount; i++)
                {
                    IMyInventory inventory = terminalBlock.GetInventory(i);
                    var inventoryItems = new List<MyInventoryItem>();
                    inventory.GetItems(inventoryItems);
                    foreach (MyInventoryItem inventoryItem in inventoryItems)
                    {
                        _inventory.Add(inventoryItem);
                    }
                }
            }
        }
        /// <summary>
        /// 获取拥有库存的方块组
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IMyTerminalBlock> GetHasInventoryBlocks()
        {
            var terminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(terminalBlocks, m => m.HasInventory);
            return terminalBlocks;
        }
        /// <summary>
        /// 配置模型
        /// </summary>
        public class ConfigModel
        {
            /// <summary>
            /// 主要文本面板名称
            /// </summary>
            public string[] MainTextSurfaceNames { get; }
            public ConfigModel(string argument)
            {
                string[] arguments = argument.Split(',');
                for (var i = 0; i < arguments.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            MainTextSurfaceNames = arguments[i].Split('|');
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// 库存模型
        /// </summary>
        private class InventoryModel
        {
            public List<ItemModel> Items { get; } = new List<ItemModel>();
            /// <summary>
            /// 新增一个物品
            /// </summary>
            /// <param name="inventoryItem"></param>
            public void Add(MyInventoryItem inventoryItem)
            {
                ItemModel item = Items.FirstOrDefault(m => m.TypeID == inventoryItem.Type.TypeId && m.SubtypeID == inventoryItem.Type.SubtypeId);
                item?.AddAmount(inventoryItem.Amount);
            }
            /// <summary>
            /// 构造函数
            /// </summary>
            public InventoryModel()
            {
                foreach (KeyValuePair<string, ItemDefaultModel> item in OreDic)
                {
                    Items.Add(new ItemModel("MyObjectBuilder_Ore", item.Key, item.Value));
                }
                foreach (KeyValuePair<string, ItemDefaultModel> item in IngotDic)
                {
                    Items.Add(new ItemModel("MyObjectBuilder_Ingot", item.Key, item.Value));
                }
                foreach (KeyValuePair<string, ItemDefaultModel> item in ComponentDic)
                {
                    Items.Add(new ItemModel("MyObjectBuilder_Component", item.Key, item.Value));
                }
                foreach (KeyValuePair<string, ItemDefaultModel> item in AmmoMagazineDic)
                {
                    Items.Add(new ItemModel("MyObjectBuilder_AmmoMagazine", item.Key, item.Value));
                }
            }
        }
        /// <summary>
        /// 物品模型
        /// </summary>
        private class ItemModel
        {
            /// <summary>
            /// 类型ID
            /// </summary>
            public string TypeID { get; }
            /// <summary>
            /// 子类型ID
            /// </summary>
            public string SubtypeID { get; }
            /// <summary>
            /// 名称
            /// </summary>
            public string Name => _defaultModel.Name;
            /// <summary>
            /// 数量
            /// </summary>
            public float Amount { get; private set; }
            /// <summary>
            /// 数量文本
            /// </summary>
            public string AmountText
            {
                get
                {
                    if (Amount < 1000)
                    {
                        return $"{Amount:N2}";
                    }
                    return $"{Amount / 1000:N2}K";
                }
            }
            /// <summary>
            /// 缺少数量
            /// </summary>
            public float LackAmount => _defaultModel.AlarmValue - Amount;
            /// <summary>
            /// 缺少数量文本
            /// </summary>
            public string LackAmountText
            {
                get
                {
                    if (LackAmount < 1000)
                    {
                        return $"{LackAmount:N2}";
                    }
                    return $"{LackAmount / 1000:N2}K";
                }
            }
            /// <summary>
            /// 比例
            /// </summary>
            public float Ratio => _defaultModel.AlarmValue > 0 ? Amount / _defaultModel.AlarmValue : 0;
            /// <summary>
            /// 是否报警
            /// </summary>
            public bool IsAlarm => Amount <= _defaultModel.AlarmValue;
            private readonly ItemDefaultModel _defaultModel;
            /// <summary>
            /// 添加数量
            /// </summary>
            /// <param name="number"></param>
            public void AddAmount(MyFixedPoint number)
            {
                Amount += number.RawValue / 1000000f;
            }
            /// <summary>
            /// 构造方法
            /// </summary>
            public ItemModel(string typeID, string subtypeID, ItemDefaultModel defaultModel)
            {
                TypeID = typeID;
                SubtypeID = subtypeID;
                _defaultModel = defaultModel;
                Amount = 0;
            }
        }
        /// <summary>
        /// 物品默认模型
        /// </summary>
        private class ItemDefaultModel
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// 预警值
            /// </summary>
            public float AlarmValue { get; }
            public ItemDefaultModel(string name, long alarmValue)
            {
                Name = name;
                AlarmValue = alarmValue;
            }
        }
        /// <summary>
        /// 文本面板模型
        /// </summary>
        public class TextSurfacesModel
        {
            private readonly ICollection<IMyTextSurface> _textSurfaces;
            public TextSurfacesModel(ICollection<IMyTextSurface> textSurfaces)
            {
                _textSurfaces = textSurfaces;
            }
            public void WriteText(string text)
            {
                foreach (IMyTextSurface textSurface in _textSurfaces)
                {
                    textSurface.WriteText(text);
                }
            }
        }
    }
}
