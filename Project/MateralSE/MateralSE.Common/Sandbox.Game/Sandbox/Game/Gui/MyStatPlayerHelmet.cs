namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerHelmet : MyStatBase
    {
        public MyStatPlayerHelmet()
        {
            base.Id = MyStringHash.GetOrCompute("player_helmet");
        }

        public override void Update()
        {
            if (MySession.Static != null)
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if ((localCharacter != null) && (localCharacter.OxygenComponent != null))
                {
                    this.CurrentValue = localCharacter.OxygenComponent.HelmetEnabled ? 1f : 0f;
                }
            }
        }
    }
}

