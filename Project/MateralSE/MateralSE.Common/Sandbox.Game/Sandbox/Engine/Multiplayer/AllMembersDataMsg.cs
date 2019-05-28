namespace Sandbox.Engine.Multiplayer
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct AllMembersDataMsg
    {
        public List<MyObjectBuilder_Identity> Identities;
        public List<MyPlayerCollection.AllPlayerData> Players;
        public List<MyObjectBuilder_Faction> Factions;
        public List<MyObjectBuilder_Client> Clients;
    }
}

