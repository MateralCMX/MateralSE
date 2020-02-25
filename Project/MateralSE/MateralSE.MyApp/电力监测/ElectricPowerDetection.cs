using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MateralSE.MyApp.电力监测
{
    public class ElectricPowerDetection
    {
        private BatteryModel _baseBatteryModel;
        private BatteryModel _otherBatteryModel;

        public void Init(List<IMyBatteryBlock> baseBatteryBlocks, List<IMyBatteryBlock> otherBatteryBlocks = null)
        {
            _baseBatteryModel = new BatteryModel(baseBatteryBlocks);
            if (otherBatteryBlocks != null)
            {
                _otherBatteryModel = new BatteryModel(otherBatteryBlocks);
            }
        }
        public string GetText()
        {
            string lcdText = $"主要电池：{_baseBatteryModel.StoredPowerSHCollagen}%\r\n";
            if (_otherBatteryModel != null)
            {
                lcdText += $"其他电池：{_otherBatteryModel.StoredPowerSHCollagen}%\r\n";
            }
            lcdText += $"主要电池：{_baseBatteryModel.BatteryCount}颗\r\n";
            if (_otherBatteryModel != null)
            {
                lcdText += $"其他电池：{_otherBatteryModel.BatteryCount}颗\r\n";
            }
            return lcdText;
        }
        public void AutomaticManagement()
        {
            _baseBatteryModel.ChangeMode(_otherBatteryModel.CurrentStoredPowerCount < _otherBatteryModel.MaxStoredPowerCount ? ChargeMode.Auto : ChargeMode.Recharge);
        }
    }
    public class BatteryModel
    {
        private ChargeMode upMode;
        private readonly List<IMyBatteryBlock> _batteryBlocks;
        public float CurrentStoredPowerCount { get; }
        public float MaxStoredPowerCount { get; }
        public double StoredPowerSHCollagen { get; }
        public int BatteryCount => _batteryBlocks.Count;
        public BatteryModel(List<IMyBatteryBlock> batteryBlocks)
        {
            _batteryBlocks = batteryBlocks;
            CurrentStoredPowerCount = batteryBlocks.Sum(m => m.CurrentStoredPower);
            MaxStoredPowerCount = batteryBlocks.Sum(m => m.MaxStoredPower);
            StoredPowerSHCollagen = Math.Round(CurrentStoredPowerCount / MaxStoredPowerCount * 100, 2);
        }
        public void ChangeMode(ChargeMode chargeMode)
        {
            if (upMode == chargeMode) return;
            upMode = chargeMode;
            foreach (IMyBatteryBlock batteryBlock in _batteryBlocks)
            {
                batteryBlock.ChargeMode = chargeMode;
            }
        }
    }
}
