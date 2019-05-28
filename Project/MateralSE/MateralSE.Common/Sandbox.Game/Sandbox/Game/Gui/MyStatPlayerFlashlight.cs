namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerFlashlight : MyStatBase
    {
        public MyStatPlayerFlashlight()
        {
            base.Id = MyStringHash.GetOrCompute("player_flashlight");
        }

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter != null)
            {
                this.CurrentValue = localCharacter.LightEnabled ? 1f : 0f;
            }
        }
    }
}

