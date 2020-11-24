using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;

namespace MateralSE.Report.InventoryReport
{
    public class Program : MyGridProgram
    {
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }
        public void Main()
        {
            Echo("Hello World!");
        }
    }
}
