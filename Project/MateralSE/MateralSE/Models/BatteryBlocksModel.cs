using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace MateralSE.Models
{
    /// <summary>
    /// 电池方块组模型
    /// </summary>
    public class BatteryBlocksModel
    {
        private readonly ICollection<IMyBatteryBlock> _batteryBlocks;
        /// <summary>
        /// 总电量
        /// </summary>
        public float TotalPower => _batteryBlocks.Sum(m => m.MaxStoredPower);
        /// <summary>
        /// 当前电量
        /// </summary>
        public float CurrentPower => _batteryBlocks.Sum(m => m.CurrentStoredPower);
        /// <summary>
        /// 比例
        /// </summary>
        public float Ratio => TotalPower > 0 ? CurrentPower / TotalPower : 0;
        /// <summary>
        /// 电池数量
        /// </summary>
        public int BatteryCount => _batteryBlocks.Count;
        public BatteryBlocksModel(ICollection<IMyBatteryBlock> batteryBlocks)
        {
            _batteryBlocks = batteryBlocks;
        }
    }
}
