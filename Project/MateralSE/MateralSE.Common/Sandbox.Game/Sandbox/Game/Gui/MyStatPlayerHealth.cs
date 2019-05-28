namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerHealth : MyStatBase
    {
        public MyStatPlayerHealth()
        {
            base.Id = MyStringHash.GetOrCompute("player_health");
        }

        public override string ToString() => 
            $"{(base.CurrentValue * 100f):0}";

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if ((localCharacter != null) && (localCharacter.StatComp != null))
            {
                base.CurrentValue = localCharacter.StatComp.HealthRatio;
            }
        }
    }
}

