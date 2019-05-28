namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Xml.Serialization;
    using VRage.Utils;

    public class MyRankedServers
    {
        private static XmlSerializer m_serializer = new XmlSerializer(typeof(MyRankedServers));

        public MyRankedServers()
        {
            this.Servers = new List<MyRankServer>();
        }

        private static void DownloadChangelog(string url, Action<MyRankedServers> completedCallback)
        {
            MyRankedServers servers = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;
                    using (StringReader reader = new StringReader(client.DownloadString(new Uri(url))))
                    {
                        servers = m_serializer.Deserialize(reader) as MyRankedServers;
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine("Error while downloading ranked servers: " + exception.ToString());
                return;
            }
            if (completedCallback != null)
            {
                completedCallback(servers);
            }
        }

        public static void LoadAsync(string url, Action<MyRankedServers> completedCallback)
        {
            Task.Run(() => DownloadChangelog(url, completedCallback));
        }

        public static void SaveTestData()
        {
            MyRankedServers o = new MyRankedServers();
            MyRankServer item = new MyRankServer();
            item.Address = "10.20.0.26:27016";
            item.Rank = 1;
            o.Servers.Add(item);
            using (FileStream stream = System.IO.File.OpenWrite("rankedServers.xml"))
            {
                m_serializer.Serialize((Stream) stream, o);
            }
        }

        public List<MyRankServer> Servers { get; set; }
    }
}

