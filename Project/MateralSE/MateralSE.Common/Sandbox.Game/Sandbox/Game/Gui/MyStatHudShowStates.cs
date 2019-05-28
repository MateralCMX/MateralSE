namespace Sandbox.Game.GUI
{
    using System;
    using VRage.Utils;

    public class MyStatHudShowStates : MyStatBase
    {
        public MyStatHudShowStates()
        {
            base.Id = MyStringHash.GetOrCompute("hud_show_states");
        }

        public override void Update()
        {
        }
    }
}

