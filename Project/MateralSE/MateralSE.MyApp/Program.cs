using Sandbox.ModAPI.Ingame;

namespace MateralSE.MyApp
{
    public sealed class Program : MyGridProgram
    {
        private int m_tick;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            Echo(m_tick++.ToString());
        }
        public static void Main(string[] args)
        {
        }
    }
}
