namespace Sandbox.Game
{
    using ProtoBuf;
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.GameServices;
    using VRage.Utils;

    public class MyCachedServerItem
    {
        public readonly bool AllowedInGroup;
        public readonly MyGameServerItem Server;
        public Dictionary<string, string> Rules;
        private MyServerData m_data;
        private const int RULE_LENGTH = 0x5d;

        public MyCachedServerItem()
        {
            this.m_data = new MyServerData();
        }

        public MyCachedServerItem(MyGameServerItem server)
        {
            this.m_data = new MyServerData();
            this.Server = server;
            this.Rules = null;
            ulong gameTagByPrefixUlong = server.GetGameTagByPrefixUlong("groupId");
            this.AllowedInGroup = (gameTagByPrefixUlong == 0) || MyGameService.IsUserInGroup(gameTagByPrefixUlong);
        }

        public void DeserializeSettings()
        {
            string str = null;
            try
            {
                if (this.Rules.TryGetValue("sc", out str))
                {
                    int num = int.Parse(str);
                    byte[] gzBuffer = new byte[num];
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= Math.Ceiling((double) (((double) num) / 93.0)))
                        {
                            using (MemoryStream stream = new MemoryStream(MyCompression.Decompress(gzBuffer)))
                            {
                                this.m_data = Serializer.Deserialize<MyServerData>(stream);
                            }
                            break;
                        }
                        byte[] sourceArray = Convert.FromBase64String(this.Rules["sc" + num2]);
                        Array.Copy(sourceArray, 0, gzBuffer, num2 * 0x5d, sourceArray.Length);
                        num2++;
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole("Failed to deserialize session settings for server!");
                MyLog.Default.WriteLineAndConsole(str);
                MyLog.Default.WriteLineAndConsole(exception.ToString());
            }
        }

        public static void SendSettingsToSteam()
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated && (MyGameService.GameServer != null))
            {
                byte[] buffer;
                using (MemoryStream stream = new MemoryStream())
                {
                    MyServerData data1 = new MyServerData();
                    data1.Settings = MySession.Static.Settings;
                    data1.ExperimentalMode = MySession.Static.IsSettingsExperimental();
                    data1.Mods = (from m in MySession.Static.Mods select m.PublishedFileId).Distinct<ulong>().ToList<ulong>();
                    MyServerData local2 = data1;
                    MyServerData instance = data1;
                    instance.Description = MySandboxGame.ConfigDedicated?.ServerDescription;
                    Serializer.Serialize<MyServerData>(stream, instance);
                    buffer = MyCompression.Compress(stream.ToArray());
                }
                MyGameService.GameServer.SetKeyValue("sc", buffer.Length.ToString());
                for (int i = 0; i < Math.Ceiling((double) (((double) buffer.Length) / 93.0)); i++)
                {
                    byte[] destinationArray = new byte[0x5d];
                    int length = buffer.Length - (0x5d * i);
                    if (length >= 0x5d)
                    {
                        Array.Copy(buffer, i * 0x5d, destinationArray, 0, 0x5d);
                    }
                    else
                    {
                        destinationArray = new byte[length];
                        Array.Copy(buffer, i * 0x5d, destinationArray, 0, length);
                    }
                    MyGameService.GameServer.SetKeyValue("sc" + i, Convert.ToBase64String(destinationArray));
                }
            }
        }

        public MyObjectBuilder_SessionSettings Settings =>
            this.m_data.Settings;

        public bool ExperimentalMode =>
            this.m_data.ExperimentalMode;

        public string Description =>
            this.m_data.Description;

        public List<ulong> Mods =>
            this.m_data.Mods;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCachedServerItem.<>c <>9 = new MyCachedServerItem.<>c();
            public static Func<MyObjectBuilder_Checkpoint.ModItem, ulong> <>9__16_0;

            internal ulong <SendSettingsToSteam>b__16_0(MyObjectBuilder_Checkpoint.ModItem m) => 
                m.PublishedFileId;
        }

        [ProtoContract]
        public class MyServerData
        {
            [ProtoMember(0x2c)]
            public MyObjectBuilder_SessionSettings Settings;
            [ProtoMember(0x2f)]
            public bool ExperimentalMode;
            [ProtoMember(50)]
            public List<ulong> Mods = new List<ulong>();
            [ProtoMember(0x35)]
            public string Description;
        }
    }
}

