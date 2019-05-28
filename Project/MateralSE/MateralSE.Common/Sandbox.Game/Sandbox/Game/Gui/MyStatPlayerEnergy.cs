namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerEnergy : MyStatBase
    {
        public MyStatPlayerEnergy()
        {
            base.Id = MyStringHash.GetOrCompute("player_energy");
        }

        public override string ToString() => 
            $"{(base.CurrentValue * 100f):0}";

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                base.CurrentValue = localCharacter.SuitEnergyLevel;
            }
        }
    }
}

