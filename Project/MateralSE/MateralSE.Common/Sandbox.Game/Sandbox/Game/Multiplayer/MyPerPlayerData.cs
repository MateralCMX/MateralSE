namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;

    public class MyPerPlayerData
    {
        private Dictionary<MyPlayer.PlayerId, Dictionary<MyStringId, object>> m_playerDataByPlayerId = new Dictionary<MyPlayer.PlayerId, Dictionary<MyStringId, object>>(MyPlayer.PlayerId.Comparer);

        private Dictionary<MyStringId, object> GetOrAllocatePlayerDataDictionary(MyPlayer.PlayerId playerId)
        {
            Dictionary<MyStringId, object> dictionary = null;
            if (!this.m_playerDataByPlayerId.TryGetValue(playerId, out dictionary))
            {
                dictionary = new Dictionary<MyStringId, object>(MyStringId.Comparer);
                this.m_playerDataByPlayerId[playerId] = dictionary;
            }
            return dictionary;
        }

        public T GetPlayerData<T>(MyPlayer.PlayerId playerId, MyStringId dataId, T defaultValue)
        {
            Dictionary<MyStringId, object> dictionary = null;
            if (!this.m_playerDataByPlayerId.TryGetValue(playerId, out dictionary))
            {
                return defaultValue;
            }
            object obj2 = null;
            return (dictionary.TryGetValue(dataId, out obj2) ? ((T) obj2) : defaultValue);
        }

        public void SetPlayerData<T>(MyPlayer.PlayerId playerId, MyStringId dataId, T data)
        {
            this.GetOrAllocatePlayerDataDictionary(playerId)[dataId] = data;
        }
    }
}

