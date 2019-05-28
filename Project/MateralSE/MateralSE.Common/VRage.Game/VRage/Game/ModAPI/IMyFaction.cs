namespace VRage.Game.ModAPI
{
    using System;
    using VRage.Collections;

    public interface IMyFaction
    {
        bool IsEveryoneNpc();
        bool IsFounder(long playerId);
        bool IsLeader(long playerId);
        bool IsMember(long playerId);
        bool IsNeutral(long playerId);

        long FactionId { get; }

        string Tag { get; }

        string Name { get; }

        string Description { get; }

        string PrivateInfo { get; }

        bool AutoAcceptMember { get; }

        bool AutoAcceptPeace { get; }

        bool AcceptHumans { get; }

        long FounderId { get; }

        DictionaryReader<long, MyFactionMember> Members { get; }

        DictionaryReader<long, MyFactionMember> JoinRequests { get; }
    }
}

