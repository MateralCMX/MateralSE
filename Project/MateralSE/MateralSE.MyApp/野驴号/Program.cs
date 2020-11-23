using MateralSE.MyApp.容量监测;
using MateralSE.MyApp.电力监测;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace MateralSE.MyApp.野驴号
{
    public sealed class Program : MyGridProgram
    {
        private ElectricPowerDetection _electricPowerDetection;
        private VolumeDetection _volumeDetection;
        private IMyTextSurface _cockpit1LeftLCDPanel;
        private IMyTextSurface _cockpit2LeftLCDPanel;
        public void Main(string argument, UpdateType updateSource)
        {
            if (_electricPowerDetection == null) _electricPowerDetection = new ElectricPowerDetection();
            if (_volumeDetection == null) _volumeDetection = new VolumeDetection();
            var batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
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
            _volumeDetection.UpdateInfo(terminalBlocks);
            if (_cockpit1LeftLCDPanel == null || _cockpit2LeftLCDPanel == null)
            {
                var cockpit = (IMyCockpit)GridTerminalSystem.GetBlockWithName("主驾驶舱1");
                _cockpit1LeftLCDPanel = cockpit.GetSurface(1);
                cockpit = (IMyCockpit)GridTerminalSystem.GetBlockWithName("主驾驶舱2");
                _cockpit2LeftLCDPanel = cockpit.GetSurface(1);
            }
            string lcdText = _electricPowerDetection.GetText();
            lcdText += _volumeDetection.GetText();
            _cockpit1LeftLCDPanel.WriteText(lcdText);
            _cockpit2LeftLCDPanel.WriteText(lcdText);
        }
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
    }
}
