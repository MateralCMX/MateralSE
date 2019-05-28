namespace Sandbox.Game.GameSystems
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyOxygenRoomLink
    {
        public MyOxygenRoomLink(MyOxygenRoom room)
        {
            this.SetRoom(room);
        }

        private void SetRoom(MyOxygenRoom room)
        {
            this.Room = room;
            this.Room.Link = this;
        }

        public MyOxygenRoom Room { get; set; }
    }
}

