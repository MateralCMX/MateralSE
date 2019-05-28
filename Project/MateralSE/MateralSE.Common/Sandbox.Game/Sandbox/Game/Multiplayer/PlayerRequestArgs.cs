namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Game.World;
    using System;

    public class PlayerRequestArgs
    {
        public Sandbox.Game.World.MyPlayer.PlayerId PlayerId;
        public bool Cancel;

        public PlayerRequestArgs(Sandbox.Game.World.MyPlayer.PlayerId playerId)
        {
            this.PlayerId = playerId;
            this.Cancel = false;
        }
    }
}

