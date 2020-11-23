using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace MateralSE.Report.PowerReport
{
    /// <summary>
    /// 电量报表
    /// </summary>
    public class Program : MyGridProgram
    {
        //CustomData:主要文本面板名称1|主要文本面板名称2|,主要电池前缀名
        private const string _mainName = "主要电池";
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
        /// 初始化主要文本面板
        /// </summary>
        /// <param name="mainTextSurfaceNames"></param>
        private void InitTextSurface(IEnumerable<string> mainTextSurfaceNames)
        {
            List<IMyTextSurface> mainTextSurfaces = mainTextSurfaceNames.Select(lcdName => GridTerminalSystem.GetBlockWithName(lcdName) as IMyTextSurface).ToList();
            mainTextSurfaces.Add(Me.GetSurface(0));
            _mainTextSurfaces = new TextSurfacesModel(mainTextSurfaces);
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
            InitTextSurface(_config.MainTextSurfaceNames);
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
        /// <summary>
        /// 电池方块组模型
        /// </summary>
        public class BatteryBlocksModel
        {
            private readonly ICollection<IMyBatteryBlock> _batteryBlocks;
            /// <summary>
            /// 总电量
            /// </summary>
            public float TotalPower => _batteryBlocks.Sum(m => m.MaxStoredPower);
            /// <summary>
            /// 当前电量
            /// </summary>
            public float CurrentPower => _batteryBlocks.Sum(m => m.CurrentStoredPower);
            /// <summary>
            /// 比例
            /// </summary>
            public float Ratio => TotalPower > 0 ? CurrentPower / TotalPower : 0;
            /// <summary>
            /// 电池数量
            /// </summary>
            public int BatteryCount => _batteryBlocks.Count;
            public BatteryBlocksModel(ICollection<IMyBatteryBlock> batteryBlocks)
            {
                _batteryBlocks = batteryBlocks;
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
