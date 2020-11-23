using MateralSE.MyApp.电力监测;
using MateralSE.MyApp.资源监测;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using MateralSE.MyApp.容量监测;
using VRageMath;

namespace MateralSE.MyApp.药丸号
{
    public class Program : MyGridProgram
    {
        private VolumeDetection _volumeDetection;
        private ElectricPowerDetection _electricPowerDetection;
        private const string BaseBatteryName = "药丸号-电池";
        private IMyTextSurface _electricPowerTextPanel;
        private IMyTextSurface _resourceTextPanel;
        private readonly ResourceDetection _resourceDetection = new ResourceDetection(new Dictionary<string, ItemModel>
        {
            ["Iron"] = new ItemModel("铁锭", "MyObjectBuilder_Ingot", 5000),
            ["Silicon"] = new ItemModel("硅片", "MyObjectBuilder_Ingot", 1000),
            ["Nickel"] = new ItemModel("镍锭", "MyObjectBuilder_Ingot", 1000),
            ["Cobalt"] = new ItemModel("钴锭", "MyObjectBuilder_Ingot", 1000),
            ["Silver"] = new ItemModel("银锭", "MyObjectBuilder_Ingot", 500),
            ["Gold"] = new ItemModel("金锭", "MyObjectBuilder_Ingot", 200),
            ["Magnesium"] = new ItemModel("镁粉", "MyObjectBuilder_Ingot", 500),
            ["Platinum"] = new ItemModel("铂金锭", "MyObjectBuilder_Ingot", 50),
            ["Uranium"] = new ItemModel("铀棒", "MyObjectBuilder_Ingot", 100),
            ["Stone"] = new ItemModel("碎石", "MyObjectBuilder_Ingot", 500),
            ["Ice"] = new ItemModel("冰", "MyObjectBuilder_Ore", 1000),
            ["SteelPlate"] = new ItemModel("钢板", "MyObjectBuilder_Component", 1000),
            ["InteriorPlate"] = new ItemModel("内衬板", "MyObjectBuilder_Component", 1000),
            ["Construction"] = new ItemModel("结构零件", "MyObjectBuilder_Component", 1000),
            ["PowerCell"] = new ItemModel("动力电池", "MyObjectBuilder_Component", 200),
            ["RadioCommunication"] = new ItemModel("无线电零件", "MyObjectBuilder_Component", 100),
            ["Thrust"] = new ItemModel("推进器零件", "MyObjectBuilder_Component", 100),
            ["Detector"] = new ItemModel("探测器零件", "MyObjectBuilder_Component", 100),
            ["LargeTube"] = new ItemModel("大型钢管", "MyObjectBuilder_Component", 200),
            ["SmallTube"] = new ItemModel("小型钢管", "MyObjectBuilder_Component", 200),
            ["Display"] = new ItemModel("显示器", "MyObjectBuilder_Component", 200),
            ["Computer"] = new ItemModel("计算机", "MyObjectBuilder_Component", 500),
            ["MetalGrid"] = new ItemModel("金属网格", "MyObjectBuilder_Component", 200),
            ["BulletproofGlass"] = new ItemModel("防弹玻璃", "MyObjectBuilder_Component", 100),
            ["Motor"] = new ItemModel("马达", "MyObjectBuilder_Component", 500)
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
            if (_electricPowerTextPanel == null) _electricPowerTextPanel = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("电力统计面板");
            if (_electricPowerDetection == null) _electricPowerDetection = new ElectricPowerDetection();
            _electricPowerDetection.Init(baseBatteryBlocks, otherBatteryBlocks);
            var cargoContainers = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers);
            if (_volumeDetection == null) _volumeDetection = new VolumeDetection(cargoContainers);
            _volumeDetection.UpdateInfo();
            lcdText = _electricPowerDetection.GetText();
            lcdText += _volumeDetection.GetText();
            _electricPowerTextPanel.WriteText(lcdText);
            _electricPowerDetection.AutomaticManagement();
        }
    }
}
