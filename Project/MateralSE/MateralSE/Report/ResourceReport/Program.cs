using MateralSE.Models;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace MateralSE.Report.ResourceReport
{
    public class Program : MyGridProgram
    {
        //CustomData:主要文本面板名称1&0|主要文本面板名称2|
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
            return _inventory.Items.Where(m => m.IsAlarm).OrderBy(m => m.Ratio).Aggregate("", (current, item) => current + $"{item.Name}：{item.AmountText}[ {item.LackAmountText} ]\r\n");
        }
        /// <summary>
        /// 初始化方块
        /// </summary>
        private void InitBlocks()
        {
            var _config = new ConfigModel(Me.CustomData);
            _mainTextSurfaces = TextSurfacesModel.InitTextSurface(_config.MainTextSurfaceNames, GridTerminalSystem, Me);
            InitInventory();
        }
        /// <summary>
        /// 初始化物品
        /// </summary>
        private void InitInventory()
        {
            _inventory = new InventoryModel();
            IEnumerable<IMyTerminalBlock> terminalBlocks = GetHasInventoryBlocks();
            foreach (IMyTerminalBlock terminalBlock in terminalBlocks)
            {
                Echo(terminalBlock.GetType().Name);
                for (var i = 0; i < terminalBlock.InventoryCount; i++)
                {
                    IMyInventory inventory = terminalBlock.GetInventory(i);
                    _inventory.Add(inventory, terminalBlock);
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
    }
}
