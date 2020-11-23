using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace MateralSE.MyApp.容量监测
{
    public class VolumeDetection
    {
        private float _maxVolume;
        private float _currentVolume;
        private double _volumeFraction;
        public List<IMyTerminalBlock> TerminalBlocks { get; private set; }
        public void UpdateInfo(List<IMyTerminalBlock> terminalBlocks)
        {
            TerminalBlocks = terminalBlocks;
            _maxVolume = 0;
            _currentVolume = 0;
            foreach (IMyTerminalBlock inventoryBlock in TerminalBlocks)
            {
                for (var i = 0; i < inventoryBlock.InventoryCount; i++)
                {
                    IMyInventory inventory = inventoryBlock.GetInventory(i);
                    _maxVolume += inventory.MaxVolume.RawValue;
                    _currentVolume += inventory.CurrentVolume.RawValue;
                }
            }
            _volumeFraction = Math.Round(_currentVolume / _maxVolume * 100, 2);
        }
        public string GetText()
        {
            string lcdText = $"容量：{_volumeFraction}%\r\n";
            return lcdText;
        }
    }
}
