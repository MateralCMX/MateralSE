using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace MateralSE.MyApp.资源监测
{
    public class Debug : MyGridProgram
    {
        private IMyTextSurface _textPanel;
        private List<IMyTerminalBlock> _terminalBlocks = new List<IMyTerminalBlock>();
        private readonly ResourceDetection _resourceDetection = new ResourceDetection(new Dictionary<string, ItemModel>
        {
            ["Iron"] = new ItemModel("铁锭", "MyObjectBuilder_Ingot", 10000),
            ["Silicon"] = new ItemModel("硅片", "MyObjectBuilder_Ingot", 6000),
            ["Nickel"] = new ItemModel("镍锭", "MyObjectBuilder_Ingot", 6000),
            ["Cobalt"] = new ItemModel("钴锭", "MyObjectBuilder_Ingot", 6000),
            ["Silver"] = new ItemModel("银锭", "MyObjectBuilder_Ingot", 1000),
            ["Gold"] = new ItemModel("金锭", "MyObjectBuilder_Ingot", 500),
            ["Magnesium"] = new ItemModel("镁粉", "MyObjectBuilder_Ingot", 2000),
            ["Platinum"] = new ItemModel("铂金锭", "MyObjectBuilder_Ingot", 100),
            ["Uranium"] = new ItemModel("铀棒", "MyObjectBuilder_Ingot", 1000),
            ["Stone"] = new ItemModel("碎石", "MyObjectBuilder_Ingot", 1000),
            ["Ice"] = new ItemModel("冰", "MyObjectBuilder_Ore", 1000),
            ["SteelPlate"] = new ItemModel("钢板", "MyObjectBuilder_Component", 2000),
            ["InteriorPlate"] = new ItemModel("内衬板", "MyObjectBuilder_Component", 2000),
            ["Construction"] = new ItemModel("结构零件", "MyObjectBuilder_Component", 2000),
            ["Computer"] = new ItemModel("计算机", "MyObjectBuilder_Component", 2000),
            ["MetalGrid"] = new ItemModel("金属网格", "MyObjectBuilder_Component", 2000),
            ["Motor"] = new ItemModel("马达", "MyObjectBuilder_Component", 2000),
            ["SmallTube"] = new ItemModel("小型钢管", "MyObjectBuilder_Component", 2000),
            ["LargeTube"] = new ItemModel("大型钢管", "MyObjectBuilder_Component", 2000),
            ["Display"] = new ItemModel("显示器", "MyObjectBuilder_Component", 2000),
            ["BulletproofGlass"] = new ItemModel("防弹玻璃", "MyObjectBuilder_Component", 1000),
            ["PowerCell"] = new ItemModel("动力电池", "MyObjectBuilder_Component", 1000),
            ["Superconductor"] = new ItemModel("超导体", "MyObjectBuilder_Component", 500),
            ["Girder"] = new ItemModel("梁", "MyObjectBuilder_Component", 2000),
            ["Thrust"] = new ItemModel("推进器零件", "MyObjectBuilder_Component", 1000),
            ["Detector"] = new ItemModel("探测器零件", "MyObjectBuilder_Component", 500),
            ["RadioCommunication"] = new ItemModel("无线电零件", "MyObjectBuilder_Component", 500),
            ["GravityGenerator"] = new ItemModel("重力发生器零件", "MyObjectBuilder_Component", 100),
            ["Reactor"] = new ItemModel("反应堆零件", "MyObjectBuilder_Component", 1000),
            ["SolarCell"] = new ItemModel("太阳能电池板", "MyObjectBuilder_Component", 500),
            ["Explosives"] = new ItemModel("爆炸物", "MyObjectBuilder_Component", 500),
            ["Canvas"] = new ItemModel("帆布", "MyObjectBuilder_Component", 20),
            ["Medical"] = new ItemModel("医疗零件", "MyObjectBuilder_Component", 50),
            ["ZoneChip"] = new ItemModel("区域筹码", "MyObjectBuilder_Component", 100),
            ["Missile200mm"] = new ItemModel("200mm导弹收纳箱", "MyObjectBuilder_AmmoMagazine", 100),
            ["NATO_25x184mm"] = new ItemModel("25x184纳托制式弹收纳箱", "MyObjectBuilder_AmmoMagazine", 100),
            ["NATO_5p56x45mm"] = new ItemModel("5.56x45纳托制式弹弹夹", "MyObjectBuilder_AmmoMagazine", 40)
        });
        public void Main(string argument, UpdateType updateSource)
        {
            if (_textPanel == null) _textPanel = Me.GetSurface(0);
            if (_terminalBlocks.Count == 0)
            {
                IMyTerminalBlock terminalBlock = GridTerminalSystem.GetBlockWithName("资源统计容器");
                _terminalBlocks.Add(terminalBlock);
                _terminalBlocks = _terminalBlocks.Where(m => m.HasInventory).ToList();
            }
            _resourceDetection.UpdateInventory(_terminalBlocks, item =>
            {
                Echo($"{item.Type.SubtypeId},{item.Type.TypeId},{item.Amount}");
            });
            Color lcdColor;
            string lcdText = _resourceDetection.GetText(out lcdColor);
            _textPanel.FontColor = lcdColor;
            _textPanel.WriteText(lcdText);
        }
    }
}
