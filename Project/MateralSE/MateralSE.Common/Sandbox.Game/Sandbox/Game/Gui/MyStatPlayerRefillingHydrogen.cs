namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character.Components;
    using System;
    using VRage.Utils;

    public class MyStatPlayerRefillingHydrogen : MyStatPlayerGasRefillingBase
    {
        public MyStatPlayerRefillingHydrogen()
        {
            base.Id = MyStringHash.GetOrCompute("player_refilling_hydrogen");
        }

        protected override float GetGassLevel(MyCharacterOxygenComponent oxygenComp) => 
            oxygenComp.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId);
    }
}

