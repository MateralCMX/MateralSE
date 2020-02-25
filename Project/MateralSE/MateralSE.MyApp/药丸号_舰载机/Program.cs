using MateralSE.MyApp.电力监测;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using MateralSE.MyApp.容量监测;

namespace MateralSE.MyApp.药丸号_舰载机
{
    public class Program : MyGridProgram
    {
        private ElectricPowerDetection _electricPowerDetection;
        private IMyTextSurface _cockpitLeftTextPanel;
        private VolumeDetection _volumeDetection;
        public void Main(string argument, UpdateType updateSource)
        {
            var batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
            if (_cockpitLeftTextPanel == null)
            {
                var cockpit = (IMyCockpit)GridTerminalSystem.GetBlockWithName("主驾驶舱");
                _cockpitLeftTextPanel = cockpit.GetSurface(1);
            }
            if (_electricPowerDetection == null) _electricPowerDetection = new ElectricPowerDetection();
            _electricPowerDetection.Init(batteryBlocks);
            var terminalBlocks = new List<IMyTerminalBlock>();
            var tempBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(tempBlocks);
            terminalBlocks.AddRange(tempBlocks);
            GridTerminalSystem.GetBlocksOfType<IMyCockpit>(tempBlocks);
            terminalBlocks.AddRange(tempBlocks);
            GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(tempBlocks);
            terminalBlocks.AddRange(tempBlocks);
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(tempBlocks);
            terminalBlocks.AddRange(tempBlocks);
            if (_volumeDetection == null) _volumeDetection = new VolumeDetection(terminalBlocks);
            _volumeDetection.UpdateInfo();
            string lcdText = _electricPowerDetection.GetText();
            lcdText +=_volumeDetection.GetText();
            _cockpitLeftTextPanel.WriteText(lcdText);
        }
    }
}
