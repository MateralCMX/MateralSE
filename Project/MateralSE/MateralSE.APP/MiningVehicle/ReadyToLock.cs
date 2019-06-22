using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;

namespace MateralSE.APP.MiningVehicle
{
    /// <summary>
    /// 准备锁定
    /// </summary>
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public class ReadyToLock
    {
        public void Main(string argument, UpdateType updateSource)
        {
            //argument:"pistonName1,pistonName2,pistonName3,...|TimerBlockName"
            var arguments = argument.Split('|');
            var pistonNames = arguments[0].Split(',');
            foreach (string pistonName in pistonNames)
            {
                var piston = (IMyPistonBase) GridTerminalSystem.GetBlockWithName(pistonName);
                piston.Velocity = 0.025f;
            }
            //启动检测起落架定时器
            var timerBlock = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName(arguments[1]);
            timerBlock.StartCountdown();
        }
    }
}
