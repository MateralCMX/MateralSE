using MateralSE.MyApp.容量监测;
using MateralSE.MyApp.电力监测;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using MateralSE.MyApp.资源监测;
using VRageMath;

namespace MateralSE.MyApp.求生车
{
    public class Program : MyGridProgram
    {
        private ElectricPowerDetection _electricPowerDetection;
        private VolumeDetection _volumeDetection;
        private ResourceDetection _resourceDetection;
        private IMyTextSurface _cockpitLeftLCDPanel;
        private IMyTextSurface _cockpitRightLCDPanel;
        private readonly Dictionary<string, ItemModel> _resourceDictionary = new Dictionary<string, ItemModel>
        {
            ["Iron"] = new ItemModel("铁锭", "MyObjectBuilder_Ingot", 1000),
            ["Silicon"] = new ItemModel("硅片", "MyObjectBuilder_Ingot", 400),
            ["Nickel"] = new ItemModel("镍锭", "MyObjectBuilder_Ingot", 400),
            ["Ice"] = new ItemModel("冰", "MyObjectBuilder_Ore", 1000)
        };
        public void Main(string argument, UpdateType updateSource)
        {
            if (_electricPowerDetection == null) _electricPowerDetection = new ElectricPowerDetection();
            if (_resourceDetection == null) _resourceDetection = new ResourceDetection(_resourceDictionary);
            if (_volumeDetection == null) _volumeDetection = new VolumeDetection();
            var batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
            _electricPowerDetection.Init(batteryBlocks);
            var terminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(terminalBlocks);
            _volumeDetection.UpdateInfo(terminalBlocks);
            if (_cockpitLeftLCDPanel == null || _cockpitRightLCDPanel == null)
            {
                var cockpit = (IMyCockpit)GridTerminalSystem.GetBlockWithName("主驾驶舱");
                _cockpitLeftLCDPanel = cockpit.GetSurface(1);
                _cockpitRightLCDPanel = cockpit.GetSurface(2);
            }
            string lcdText = _electricPowerDetection.GetText();
            lcdText += _volumeDetection.GetText();
            _cockpitLeftLCDPanel.WriteText(lcdText);
            Color lcdFontColor;
            lcdText = _resourceDetection.GetText(out lcdFontColor);
            _cockpitRightLCDPanel.FontColor = lcdFontColor;
            _cockpitRightLCDPanel.WriteText(lcdText);
        }
    }
}
