using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;

namespace MateralSE.APP.MiningVehicle
{
    /// <summary>
    /// 锁定起落架
    /// </summary>
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    public class StartMining
    {
        public void Main(string argument, UpdateType updateSource)
        {
            ////argument:"LandingGearName1,LandingGearName2,LandingGearName3,...|TimerBlockName"
            //var arguments = argument.Split('|');
            //var landingGearNames = arguments[0].Split(',');
            //var lockedCount = 0;
            //foreach (string pistonName in landingGearNames)
            //{
            //    var landingGear = (IMyLandingGear)GridTerminalSystem.GetBlockWithName(pistonName);
            //    if (!landingGear.AutoLock)
            //    {
            //        landingGear.ResetAutoLock();
            //    }
            //    if (landingGear.IsLocked)
            //    {
            //        lockedCount++;
            //    }
            //}
            //if (lockedCount == landingGearNames.Length)
            //{
            //    //启动挖矿
            //    var timerBlock = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName(arguments[1]);
            //    timerBlock.StartCountdown();
            //}
        }
    }
}
