namespace VRage.GameServices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.CompilerServices;

    public class MyGameServerItem : IMyMultiplayerGame
    {
        public MyGameServerItem()
        {
            this.GameTagList = new List<string>();
        }

        public string GetGameTagByPrefix(string prefix)
        {
            using (List<string>.Enumerator enumerator = this.GameTagList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    string current = enumerator.Current;
                    if (current.StartsWith(prefix))
                    {
                        return current.Substring(prefix.Length, current.Length - prefix.Length);
                    }
                }
            }
            return string.Empty;
        }

        public ulong GetGameTagByPrefixUlong(string prefix)
        {
            ulong num;
            string gameTagByPrefix = this.GetGameTagByPrefix(prefix);
            if (string.IsNullOrEmpty(gameTagByPrefix))
            {
                return 0UL;
            }
            ulong.TryParse(gameTagByPrefix, out num);
            return num;
        }

        public uint AppID { get; set; }

        public int BotPlayers { get; set; }

        public bool DoNotRefresh { get; set; }

        public bool Experimental { get; set; }

        public string GameDescription { get; set; }

        public string GameDir { get; set; }

        public List<string> GameTagList { get; set; }

        public string GameTags { get; set; }

        public bool HadSuccessfulResponse { get; set; }

        public string Map { get; set; }

        public int MaxPlayers { get; set; }

        public string Name { get; set; }

        public IPEndPoint NetAdr { get; set; }

        public bool Password { get; set; }

        public int Ping { get; set; }

        public int Players { get; set; }

        public bool Secure { get; set; }

        public int ServerVersion { get; set; }

        public ulong GameID =>
            this.SteamID;

        public ulong SteamID { get; set; }

        public uint TimeLastPlayed { get; set; }

        public bool IsRanked { get; set; }
    }
}

