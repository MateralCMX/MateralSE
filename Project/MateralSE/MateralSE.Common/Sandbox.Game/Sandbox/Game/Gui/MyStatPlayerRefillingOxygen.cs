namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character.Components;
    using System;
    using VRage.Utils;

    public class MyStatPlayerRefillingOxygen : MyStatPlayerGasRefillingBase
    {
        public MyStatPlayerRefillingOxygen()
        {
            base.Id = MyStringHash.GetOrCompute("player_refilling_oxygen");
        }

        protected override float GetGassLevel(MyCharacterOxygenComponent oxygenComp) => 
            oxygenComp.GetGasFillLevel(MyCharacterOxygenComponent.OxygenId);
    }
}

