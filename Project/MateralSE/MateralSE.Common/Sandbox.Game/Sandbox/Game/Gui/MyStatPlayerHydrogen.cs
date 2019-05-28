namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerHydrogen : MyStatBase
    {
        public MyStatPlayerHydrogen()
        {
            base.Id = MyStringHash.GetOrCompute("player_hydrogen");
        }

        public override string ToString() => 
            $"{(base.CurrentValue * 100f):0}";

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if ((localCharacter != null) && (localCharacter.OxygenComponent != null))
            {
                base.CurrentValue = localCharacter.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId);
            }
        }
    }
}

