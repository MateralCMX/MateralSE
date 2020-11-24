using MateralSE.Models;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MateralSE.Report.PowerReport
{
    /// <summary>
    /// 电量报表
    /// </summary>
    [SuppressMessage("ReSharper", "UsePatternMatching")]
    public class Program : MyGridProgram
    {
        //CustomData:主要文本面板名称1&0|主要文本面板名称2|,主要电池前缀名
        private const string _mainName = "基地电池";
        private const string _otherName = "其他电池";
        private BatteryBlocksModel _mainBatteryBlocks;
        private BatteryBlocksModel _otherBatteryBlocks;
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
            var result = $@"{_mainName}电量：{_mainBatteryBlocks.Ratio * 100:N2}%
{_mainName}数量：{_mainBatteryBlocks.BatteryCount}";
            if (_otherBatteryBlocks.BatteryCount > 0)
            {
                result += $@"
{_otherName}电量：{_otherBatteryBlocks.Ratio * 100:N2}%
{_otherName}数量：{_otherBatteryBlocks.BatteryCount}";
            }
            return result;
        }
        /// <summary>
        /// 初始化电池方块
        /// </summary>
        /// <param name="mainBatteryPrefix">主要电池前缀名</param>
        private void InitBatteryBlocks(string mainBatteryPrefix)
        {
            var allBatteryBlocks = new List<IMyBatteryBlock>();
            var mainBatteryBlocks = new List<IMyBatteryBlock>();
            var otherBatteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(allBatteryBlocks);
            foreach (IMyBatteryBlock batteryBlock in allBatteryBlocks)
            {
                if (batteryBlock.CustomName.StartsWith(mainBatteryPrefix))
                {
                    mainBatteryBlocks.Add(batteryBlock);
                }
                else
                {
                    otherBatteryBlocks.Add(batteryBlock);
                }
            }
            _mainBatteryBlocks = new BatteryBlocksModel(mainBatteryBlocks);
            _otherBatteryBlocks = new BatteryBlocksModel(otherBatteryBlocks);
        }
        /// <summary>
        /// 初始化方块
        /// </summary>
        private void InitBlocks()
        {
            var _config = new ConfigModel(Me.CustomData);
            _mainTextSurfaces = TextSurfacesModel.InitTextSurface(_config.MainTextSurfaceNames, GridTerminalSystem, Me);
            InitBatteryBlocks(_config.MainBatteryPrefix);
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
            /// <summary>
            /// 主要电池前缀
            /// </summary>
            public string MainBatteryPrefix { get; }
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
                        case 1:
                            MainBatteryPrefix = arguments[i];
                            break;
                    }
                }
            }
        }
    }
}
