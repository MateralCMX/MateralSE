using MateralSE.MyApp.电力监测;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using MateralSE.MyApp.容量监测;
using MateralSE.MyApp.资源监测;
using VRageMath;

namespace MateralSE.MyApp.空间站
{
    public class Program : MyGridProgram
    {
        private ElectricPowerDetection _electricPowerDetection;
        private const string BaseBatteryName = "基地-电池";
        private IMyTextSurface _upTextPanel;
        private IMyTextSurface _downTextPanel;
        private VolumeDetection _volumeDetection;
        private IMyTextSurface _resourceTextPanel;
        private readonly ResourceDetection _resourceDetection = new ResourceDetection(new Dictionary<string, ItemModel>
        {
            ["Iron"] = new ItemModel("铁锭", "MyObjectBuilder_Ingot", 20000),
            ["Silicon"] = new ItemModel("硅片", "MyObjectBuilder_Ingot", 10000),
            ["Nickel"] = new ItemModel("镍锭", "MyObjectBuilder_Ingot", 10000),
            ["Cobalt"] = new ItemModel("钴锭", "MyObjectBuilder_Ingot", 10000),
            ["Silver"] = new ItemModel("银锭", "MyObjectBuilder_Ingot", 2000),
            ["Gold"] = new ItemModel("金锭", "MyObjectBuilder_Ingot", 1000),
            ["Magnesium"] = new ItemModel("镁粉", "MyObjectBuilder_Ingot", 10000),
            ["Platinum"] = new ItemModel("铂金锭", "MyObjectBuilder_Ingot", 1000),
            ["Uranium"] = new ItemModel("铀棒", "MyObjectBuilder_Ingot", 2000),
            ["Stone"] = new ItemModel("碎石", "MyObjectBuilder_Ingot", 2000),
            ["Ice"] = new ItemModel("冰", "MyObjectBuilder_Ore", 200000),
            ["SteelPlate"] = new ItemModel("钢板", "MyObjectBuilder_Component", 5000),
            ["InteriorPlate"] = new ItemModel("内衬板", "MyObjectBuilder_Component", 5000),
            ["Construction"] = new ItemModel("结构零件", "MyObjectBuilder_Component", 5000),
            ["Computer"] = new ItemModel("计算机", "MyObjectBuilder_Component", 5000),
            ["MetalGrid"] = new ItemModel("金属网格", "MyObjectBuilder_Component", 5000),
            ["Motor"] = new ItemModel("马达", "MyObjectBuilder_Component", 5000),
            ["SmallTube"] = new ItemModel("小型钢管", "MyObjectBuilder_Component", 5000),
            ["LargeTube"] = new ItemModel("大型钢管", "MyObjectBuilder_Component", 3000),
            ["Display"] = new ItemModel("显示器", "MyObjectBuilder_Component", 5000),
            ["BulletproofGlass"] = new ItemModel("防弹玻璃", "MyObjectBuilder_Component", 1000),
            ["PowerCell"] = new ItemModel("动力电池", "MyObjectBuilder_Component", 1000),
            ["Superconductor"] = new ItemModel("超导体", "MyObjectBuilder_Component", 500),
            ["Girder"] = new ItemModel("梁", "MyObjectBuilder_Component", 1000),
            ["Thrust"] = new ItemModel("推进器零件", "MyObjectBuilder_Component", 500),
            ["Detector"] = new ItemModel("探测器零件", "MyObjectBuilder_Component", 300),
            ["RadioCommunication"] = new ItemModel("无线电零件", "MyObjectBuilder_Component", 300),
            ["GravityGenerator"] = new ItemModel("重力发生器零件", "MyObjectBuilder_Component", 100),
            ["Reactor"] = new ItemModel("反应堆零件", "MyObjectBuilder_Component", 300),
            ["SolarCell"] = new ItemModel("太阳能电池板", "MyObjectBuilder_Component", 200),
            ["Explosives"] = new ItemModel("爆炸物", "MyObjectBuilder_Component", 500),
            ["Medical"] = new ItemModel("医疗零件", "MyObjectBuilder_Component", 100),
            ["Missile200mm"] = new ItemModel("200mm导弹收纳箱", "MyObjectBuilder_AmmoMagazine", 200),
            ["NATO_25x184mm"] = new ItemModel("25x184纳托制式弹收纳箱", "MyObjectBuilder_AmmoMagazine", 200),
            ["NATO_5p56x45mm"] = new ItemModel("5.56x45纳托制式弹弹夹", "MyObjectBuilder_AmmoMagazine", 100)
        });
        public void Main(string argument, UpdateType updateSource)
        {
            if (_resourceTextPanel == null) _resourceTextPanel = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("资源统计面板");
            var terminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(terminalBlocks);
            terminalBlocks = terminalBlocks.Where(m => m.HasInventory).ToList();
            _resourceDetection.UpdateInventory(terminalBlocks);
            Color lcdColor;
            string lcdText = _resourceDetection.GetText(out lcdColor);
            _resourceTextPanel.FontColor = lcdColor;
            _resourceTextPanel.WriteText(lcdText);
            var batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
            List<IMyBatteryBlock> baseBatteryBlocks = batteryBlocks.Where(m => m.CustomName.Equals(BaseBatteryName)).ToList();
            List<IMyBatteryBlock> otherBatteryBlocks = batteryBlocks.Where(m => !m.CustomName.Equals(BaseBatteryName)).ToList();
            if (_upTextPanel == null) _upTextPanel = Me.GetSurface(0);
            if (_downTextPanel == null) _downTextPanel = Me.GetSurface(1);
            if (_electricPowerDetection == null) _electricPowerDetection = new ElectricPowerDetection();
            _electricPowerDetection.Init(baseBatteryBlocks, otherBatteryBlocks);
            _electricPowerDetection.AutomaticManagement();
            terminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(terminalBlocks);
            if (_volumeDetection == null) _volumeDetection = new VolumeDetection(terminalBlocks);
            _volumeDetection.UpdateInfo();
            lcdText = _electricPowerDetection.GetText();
            lcdText += _volumeDetection.GetText();
            _upTextPanel.WriteText(lcdText);
            lcdText = _electricPowerDetection.GetGuidText(out lcdColor);
            _downTextPanel.FontColor = lcdColor;
            _downTextPanel.WriteText(lcdText);
        }
    }
}
