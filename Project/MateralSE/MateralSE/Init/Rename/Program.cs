using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI.Ingame;

namespace MateralSE.Init.Rename
{
    /// <summary>
    /// 重命名
    /// </summary>
    public class Program : MyGridProgram
    {
        //CustomData:旧名字,新名字
        public void Main()
        {
            try
            {
                Handler();
            }
            catch (Exception exception)
            {
                Echo(exception.Message);
            }
        }
        private void Handler()
        {
            string[] arguments = Me.CustomData.Split(',');
            if (arguments.Length != 2) throw new ArgumentException("参数错误");
            string oldName = arguments[0];
            string newName = arguments[1];
            var index = 1;
            var terminalBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(terminalBlocks);
            foreach (IMyTerminalBlock terminalBlock in terminalBlocks.Where(m => m.CustomName.StartsWith(oldName)))
            {
                terminalBlock.CustomName = $"{newName}-{index++}";
            }
        }
    }
}
