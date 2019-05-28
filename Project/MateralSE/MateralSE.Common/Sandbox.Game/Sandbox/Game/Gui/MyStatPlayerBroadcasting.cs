namespace Sandbox.Game.GUI
{
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatPlayerBroadcasting : MyStatBase
    {
        public MyStatPlayerBroadcasting()
        {
            base.Id = MyStringHash.GetOrCompute("player_broadcasting");
        }

        public override void Update()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if ((localCharacter == null) || (localCharacter.RadioBroadcaster == null))
            {
                base.CurrentValue = 0f;
            }
            else
            {
                this.CurrentValue = localCharacter.RadioBroadcaster.Enabled ? 1f : 0f;
            }
        }
    }
}

