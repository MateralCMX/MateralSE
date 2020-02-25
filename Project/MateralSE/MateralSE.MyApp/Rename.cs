using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;

namespace MateralSE.MyApp
{
    public class Rename : MyGridProgram
    {
        public void Main(string argument, UpdateType updateSource)
        {
            var batteryBlocks = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteryBlocks);
            foreach (IMyBatteryBlock batteryBlock in batteryBlocks)
            {
                if (batteryBlock.CustomName.Contains("电池-dc-"))
                {
                    batteryBlock.CustomName = "基地-电池";
                }
            }
        }
    }
}
