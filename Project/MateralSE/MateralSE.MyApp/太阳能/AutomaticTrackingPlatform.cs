using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace MateralSE.MyApp.太阳能
{
    /// <summary>
    /// 自动追踪平台
    /// </summary>
    public class AutomaticTrackingPlatform : MyGridProgram
    {
        private IMySolarPanel _solarPanel;
        private float upSolarPanelPanelMaxOutput;
        private IMyMotorBase _motorRotorBottom;
        public void Main(string argument, UpdateType updateSource)
        {
            if (_solarPanel == null)
            {
                _solarPanel = (IMySolarPanel)GridTerminalSystem.GetBlockWithName("太阳能板-基准");
            }
            if (_motorRotorBottom == null)
            {
                _motorRotorBottom = (IMyMotorBase)GridTerminalSystem.GetBlockWithName("太阳能转子");
            }
            if (_solarPanel.MaxOutput < upSolarPanelPanelMaxOutput)
            {
                _motorRotorBottom.ApplyAction("Reverse");
            }
            upSolarPanelPanelMaxOutput = _solarPanel.MaxOutput;
        }
    }
}
