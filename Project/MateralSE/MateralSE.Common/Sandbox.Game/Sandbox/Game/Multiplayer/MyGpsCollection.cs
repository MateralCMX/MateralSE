namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using VRage;
    using VRage.Audio;
    using VRage.Data.Audio;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRageMath;

    [StaticEventOwner]
    public class MyGpsCollection : IMyGpsCollection
    {
        [CompilerGenerated]
        private Action<long> ListChanged;
        [CompilerGenerated]
        private Action<long, int> GpsChanged;
        private Dictionary<long, Dictionary<int, MyGps>> m_playerGpss = new Dictionary<long, Dictionary<int, MyGps>>();
        private StringBuilder m_NamingSearch = new StringBuilder();
        private long lastPlayerId;
        private static readonly int PARSE_MAX_COUNT = 20;
        private static readonly string m_ScanPattern = @"GPS:([^:]{0,32}):([\d\.-]*):([\d\.-]*):([\d\.-]*):";
        private static readonly string m_ScanPatternExtended = @"GPS:([^:]{0,32}):([\d\.-]*):([\d\.-]*):([\d\.-]*):(.*)";
        private static List<IMyGps> reusableList = new List<IMyGps>();

        public event Action<long, int> GpsChanged
        {
            [CompilerGenerated] add
            {
                Action<long, int> gpsChanged = this.GpsChanged;
                while (true)
                {
                    Action<long, int> a = gpsChanged;
                    Action<long, int> action3 = (Action<long, int>) Delegate.Combine(a, value);
                    gpsChanged = Interlocked.CompareExchange<Action<long, int>>(ref this.GpsChanged, action3, a);
                    if (ReferenceEquals(gpsChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long, int> gpsChanged = this.GpsChanged;
                while (true)
                {
                    Action<long, int> source = gpsChanged;
                    Action<long, int> action3 = (Action<long, int>) Delegate.Remove(source, value);
                    gpsChanged = Interlocked.CompareExchange<Action<long, int>>(ref this.GpsChanged, action3, source);
                    if (ReferenceEquals(gpsChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<long> ListChanged
        {
            [CompilerGenerated] add
            {
                Action<long> listChanged = this.ListChanged;
                while (true)
                {
                    Action<long> a = listChanged;
                    Action<long> action3 = (Action<long>) Delegate.Combine(a, value);
                    listChanged = Interlocked.CompareExchange<Action<long>>(ref this.ListChanged, action3, a);
                    if (ReferenceEquals(listChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<long> listChanged = this.ListChanged;
                while (true)
                {
                    Action<long> source = listChanged;
                    Action<long> action3 = (Action<long>) Delegate.Remove(source, value);
                    listChanged = Interlocked.CompareExchange<Action<long>>(ref this.ListChanged, action3, source);
                    if (ReferenceEquals(listChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public bool AddPlayerGps(long identityId, ref MyGps gps)
        {
            if (gps != null)
            {
                Dictionary<int, MyGps> dictionary;
                MyGps gps2;
                if (!this.m_playerGpss.TryGetValue(identityId, out dictionary))
                {
                    dictionary = new Dictionary<int, MyGps>();
                    this.m_playerGpss.Add(identityId, dictionary);
                }
                if (!dictionary.ContainsKey(gps.Hash))
                {
                    dictionary.Add(gps.Hash, gps);
                    return true;
                }
                dictionary.TryGetValue(gps.Hash, out gps2);
                if (gps2.DiscardAt != null)
                {
                    gps2.SetDiscardAt();
                }
            }
            return false;
        }

        [Event(null, 0x16c), Reliable, Server]
        private static void AlwaysVisibleRequest(long identityId, int gpsHash, bool alwaysVisible)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(identityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Dictionary<int, MyGps> dictionary;
                if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, int, bool>(s => new Action<long, int, bool>(MyGpsCollection.AlwaysVisibleSuccess), identityId, gpsHash, alwaysVisible, targetEndpoint, position);
                }
            }
        }

        [Event(null, 0x17b), Reliable, ServerInvoked, Broadcast]
        private static void AlwaysVisibleSuccess(long identityId, int gpsHash, bool alwaysVisible)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary) && dictionary.TryGetValue(gpsHash, out gps))
            {
                gps.AlwaysVisible = alwaysVisible;
                gps.ShowOnHud |= alwaysVisible;
                gps.DiscardAt = null;
                Action<long, int> gpsChanged = MySession.Static.Gpss.GpsChanged;
                if (gpsChanged != null)
                {
                    gpsChanged(identityId, gpsHash);
                }
                if (identityId == MySession.Static.LocalPlayerId)
                {
                    if (gps.ShowOnHud)
                    {
                        MyHud.GpsMarkers.RegisterMarker(gps);
                    }
                    else
                    {
                        MyHud.GpsMarkers.UnregisterMarker(gps);
                    }
                }
            }
        }

        public void ChangeAlwaysVisible(long identityId, int gpsHash, bool alwaysVisible)
        {
            this.SendChangeAlwaysVisible(identityId, gpsHash, alwaysVisible);
        }

        public void ChangeColor(long identityId, int gpsHash, Color color)
        {
            this.SendChangeColor(identityId, gpsHash, color);
        }

        public void ChangeShowOnHud(long identityId, int gpsHash, bool show)
        {
            this.SendChangeShowOnHud(identityId, gpsHash, show);
        }

        [Event(null, 0x1a6), Reliable, Server]
        private static void ColorRequest(long identityId, int gpsHash, Color color)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(identityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Dictionary<int, MyGps> dictionary;
                if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, int, Color>(s => new Action<long, int, Color>(MyGpsCollection.ColorSuccess), identityId, gpsHash, color, targetEndpoint, position);
                }
            }
        }

        [Event(null, 0x1b5), Reliable, ServerInvoked, Broadcast]
        private static void ColorSuccess(long identityId, int gpsHash, Color color)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary) && dictionary.TryGetValue(gpsHash, out gps))
            {
                gps.GPSColor = color;
                gps.DiscardAt = null;
                Action<long, int> gpsChanged = MySession.Static.Gpss.GpsChanged;
                if (gpsChanged != null)
                {
                    gpsChanged(identityId, gpsHash);
                }
            }
        }

        [Event(null, 0xad), Reliable, Server]
        private static void DeleteRequest(long identityId, int gpsHash)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(identityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Dictionary<int, MyGps> dictionary;
                if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary) && dictionary.ContainsKey(gpsHash))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MyGpsCollection.DeleteSuccess), identityId, gpsHash, targetEndpoint, position);
                }
            }
        }

        [Event(null, 0xbd), Reliable, ServerInvoked, Broadcast]
        private static void DeleteSuccess(long identityId, int gpsHash)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary) && dictionary.TryGetValue(gpsHash, out gps))
            {
                if (gps.ShowOnHud)
                {
                    MyHud.GpsMarkers.UnregisterMarker(gps);
                }
                dictionary.Remove(gpsHash);
                gps.Close();
                Action<long> listChanged = MySession.Static.Gpss.ListChanged;
                if (listChanged != null)
                {
                    listChanged(identityId);
                }
            }
        }

        public void DiscardOld()
        {
            List<int> list = new List<int>();
            foreach (KeyValuePair<long, Dictionary<int, MyGps>> pair in this.m_playerGpss)
            {
                foreach (KeyValuePair<int, MyGps> pair2 in pair.Value)
                {
                    TimeSpan? discardAt = pair2.Value.DiscardAt;
                    if ((discardAt != null) && (TimeSpan.Compare(MySession.Static.ElapsedPlayTime, pair2.Value.DiscardAt.Value) > 0))
                    {
                        list.Add(pair2.Value.Hash);
                    }
                }
                foreach (int num in list)
                {
                    MyGps ins = pair.Value[num];
                    if (ins.ShowOnHud)
                    {
                        MyHud.GpsMarkers.UnregisterMarker(ins);
                    }
                    pair.Value.Remove(num);
                }
                list.Clear();
            }
        }

        public bool ExistsForPlayer(long id)
        {
            Dictionary<int, MyGps> dictionary;
            return this.m_playerGpss.TryGetValue(id, out dictionary);
        }

        public MyGps GetGps(int hash)
        {
            using (Dictionary<long, Dictionary<int, MyGps>>.ValueCollection.Enumerator enumerator = MySession.Static.Gpss.m_playerGpss.Values.GetEnumerator())
            {
                while (true)
                {
                    MyGps gps;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.TryGetValue(hash, out gps))
                    {
                        return gps;
                    }
                }
            }
            return null;
        }

        public MyGps GetGpsByEntityId(long identityId, long entityId)
        {
            Dictionary<int, MyGps> dictionary;
            if (this.m_playerGpss.TryGetValue(identityId, out dictionary))
            {
                using (Dictionary<int, MyGps>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyGps current = enumerator.Current;
                        if (current.EntityId == entityId)
                        {
                            return current;
                        }
                    }
                }
            }
            return null;
        }

        public IMyGps GetGpsByName(long identityId, string gpsName)
        {
            Dictionary<int, MyGps> dictionary;
            if (this.m_playerGpss.TryGetValue(identityId, out dictionary))
            {
                using (Dictionary<int, MyGps>.ValueCollection.Enumerator enumerator = dictionary.Values.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyGps current = enumerator.Current;
                        if (current.Name == gpsName)
                        {
                            return current;
                        }
                    }
                }
            }
            return null;
        }

        public void GetGpsList(long identityId, List<IMyGps> list)
        {
            Dictionary<int, MyGps> dictionary;
            if (this.m_playerGpss.TryGetValue(identityId, out dictionary))
            {
                foreach (MyGps gps in dictionary.Values)
                {
                    list.Add(gps);
                }
            }
        }

        public void GetNameForNewCurrent(StringBuilder name)
        {
            Dictionary<int, MyGps> dictionary;
            int num = 0;
            name.Clear().Append(MySession.Static.LocalHumanPlayer.DisplayName).Append(" #");
            if (this.m_playerGpss.TryGetValue(MySession.Static.LocalPlayerId, out dictionary))
            {
                foreach (KeyValuePair<int, MyGps> pair in dictionary)
                {
                    if (pair.Value.Name.StartsWith(name.ToString()))
                    {
                        int num2;
                        this.m_NamingSearch.Clear().Append(pair.Value.Name, name.Length, pair.Value.Name.Length - name.Length);
                        try
                        {
                            num2 = int.Parse(this.m_NamingSearch.ToString());
                        }
                        catch (SystemException)
                        {
                            continue;
                        }
                        if (num2 > num)
                        {
                            num = num2;
                        }
                    }
                }
            }
            num++;
            name.Append(num);
        }

        public unsafe MyObjectBuilder_Gps.Entry GetObjectBuilderEntry(MyGps gps)
        {
            MyObjectBuilder_Gps.Entry* entryPtr1;
            MyObjectBuilder_Gps.Entry entry = new MyObjectBuilder_Gps.Entry {
                name = gps.Name,
                description = gps.Description,
                coords = gps.Coords
            };
            entryPtr1->isFinal = gps.DiscardAt == null;
            entryPtr1 = (MyObjectBuilder_Gps.Entry*) ref entry;
            entry.showOnHud = gps.ShowOnHud;
            entry.alwaysVisible = gps.AlwaysVisible;
            entry.color = gps.GPSColor;
            entry.entityId = gps.EntityId;
            entry.DisplayName = gps.DisplayName;
            entry.isObjective = gps.IsObjective;
            return entry;
        }

        public void LoadGpss(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (MyFakes.ENABLE_GPS && (checkpoint.Gps != null))
            {
                foreach (KeyValuePair<long, MyObjectBuilder_Gps> pair in checkpoint.Gps.Dictionary)
                {
                    using (List<MyObjectBuilder_Gps.Entry>.Enumerator enumerator2 = pair.Value.Entries.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            Dictionary<int, MyGps> dictionary;
                            MyGps gps = new MyGps(enumerator2.Current);
                            if (!this.m_playerGpss.TryGetValue(pair.Key, out dictionary))
                            {
                                dictionary = new Dictionary<int, MyGps>();
                                this.m_playerGpss.Add(pair.Key, dictionary);
                            }
                            if (!dictionary.ContainsKey(gps.GetHashCode()))
                            {
                                dictionary.Add(gps.GetHashCode(), gps);
                                if ((gps.ShowOnHud && (pair.Key == MySession.Static.LocalPlayerId)) && (MySession.Static.LocalPlayerId != 0))
                                {
                                    MyHud.GpsMarkers.RegisterMarker(gps);
                                }
                            }
                        }
                    }
                }
            }
        }

        [Event(null, 0xe4), Reliable, Server]
        private static void ModifyRequest(ModifyMsg msg)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(msg.IdentityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Dictionary<int, MyGps> dictionary;
                if (MySession.Static.Gpss.m_playerGpss.TryGetValue(msg.IdentityId, out dictionary) && dictionary.ContainsKey(msg.Hash))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<ModifyMsg>(s => new Action<ModifyMsg>(MyGpsCollection.ModifySuccess), msg, targetEndpoint, position);
                }
            }
        }

        [Event(null, 0xf4), Reliable, ServerInvoked, Broadcast]
        private static void ModifySuccess(ModifyMsg msg)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(msg.IdentityId, out dictionary) && dictionary.TryGetValue(msg.Hash, out gps))
            {
                gps.Name = msg.Name;
                gps.Description = msg.Description;
                gps.Coords = msg.Coords;
                gps.DiscardAt = null;
                gps.GPSColor = msg.GPSColor;
                Action<long, int> gpsChanged = MySession.Static.Gpss.GpsChanged;
                if (gpsChanged != null)
                {
                    gpsChanged(msg.IdentityId, gps.Hash);
                }
                dictionary.Remove(gps.Hash);
                MyHud.GpsMarkers.UnregisterMarker(gps);
                gps.UpdateHash();
                if (!dictionary.ContainsKey(gps.Hash))
                {
                    dictionary.Add(gps.Hash, gps);
                }
                else
                {
                    MyGps gps2;
                    dictionary.TryGetValue(gps.Hash, out gps2);
                    MyHud.GpsMarkers.UnregisterMarker(gps2);
                    dictionary.Remove(gps.Hash);
                    dictionary.Add(gps.Hash, gps);
                    Action<long> listChanged = MySession.Static.Gpss.ListChanged;
                    if (listChanged != null)
                    {
                        listChanged(msg.IdentityId);
                    }
                }
                if ((msg.IdentityId == MySession.Static.LocalPlayerId) && gps.ShowOnHud)
                {
                    MyHud.GpsMarkers.RegisterMarker(gps);
                }
            }
        }

        [Event(null, 0x6d), Reliable, Server, Broadcast]
        private static void OnAddGps(AddMsg msg)
        {
            if ((Sync.IsServer && !MyEventContext.Current.IsLocallyInvoked) && (MySession.Static.Players.TryGetSteamId(msg.IdentityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                MyEventContext.ValidationFailed();
            }
            else
            {
                MyGps gps = new MyGps {
                    Name = msg.Name,
                    DisplayName = msg.DisplayName,
                    Description = msg.Description,
                    Coords = msg.Coords,
                    ShowOnHud = msg.ShowOnHud,
                    AlwaysVisible = msg.AlwaysVisible
                };
                gps.DiscardAt = null;
                gps.GPSColor = msg.GPSColor;
                gps.IsContainerGPS = msg.IsContainerGPS;
                gps.IsObjective = msg.IsObjective;
                if (!msg.IsFinal)
                {
                    gps.SetDiscardAt();
                }
                if (msg.EntityId != 0)
                {
                    MyEntity entityById = MyEntities.GetEntityById(msg.EntityId, false);
                    if (entityById != null)
                    {
                        gps.SetEntity(entityById);
                    }
                    else
                    {
                        gps.SetEntityId(msg.EntityId);
                    }
                }
                gps.UpdateHash();
                if ((MySession.Static.Gpss.AddPlayerGps(msg.IdentityId, ref gps) && gps.ShowOnHud) && (msg.IdentityId == MySession.Static.LocalPlayerId))
                {
                    MyHud.GpsMarkers.RegisterMarker(gps);
                    if (msg.PlaySoundOnCreation)
                    {
                        MyCueId cueId = MySoundPair.GetCueId("HudGPSNotification3");
                        MyAudio.Static.PlaySound(cueId, null, MySoundDimensions.D2, false, false);
                    }
                }
                Action<long> listChanged = MySession.Static.Gpss.ListChanged;
                if (listChanged != null)
                {
                    listChanged(msg.IdentityId);
                }
            }
        }

        private void ParseChat(ulong steamUserId, string messageText, ChatChannel channel, long targetId, string customAuthorName = null)
        {
            StringBuilder desc = new StringBuilder();
            desc.Append(MyTexts.GetString(MySpaceTexts.TerminalTab_GPS_FromChatDescPrefix)).Append(MyMultiplayer.Static.GetMemberName(steamUserId));
            this.ScanText(messageText, desc);
        }

        public static bool ParseOneGPS(string input, StringBuilder name, ref Vector3D coords)
        {
            using (IEnumerator enumerator = Regex.Matches(input, m_ScanPattern).GetEnumerator())
            {
                while (true)
                {
                    double num;
                    double num2;
                    double num3;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Match current = (Match) enumerator.Current;
                    try
                    {
                        num = Math.Round(double.Parse(current.Groups[2].Value, CultureInfo.InvariantCulture), 2);
                        num2 = Math.Round(double.Parse(current.Groups[3].Value, CultureInfo.InvariantCulture), 2);
                        num3 = Math.Round(double.Parse(current.Groups[4].Value, CultureInfo.InvariantCulture), 2);
                    }
                    catch (SystemException)
                    {
                        continue;
                    }
                    name.Clear().Append(current.Groups[1].Value);
                    coords.X = num;
                    coords.Y = num2;
                    coords.Z = num3;
                    return true;
                }
            }
            return false;
        }

        public static bool ParseOneGPSExtended(string input, StringBuilder name, ref Vector3D coords, StringBuilder additionalData)
        {
            using (IEnumerator enumerator = Regex.Matches(input, m_ScanPatternExtended).GetEnumerator())
            {
                while (true)
                {
                    double num;
                    double num2;
                    double num3;
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    Match current = (Match) enumerator.Current;
                    try
                    {
                        num = Math.Round(double.Parse(current.Groups[2].Value, CultureInfo.InvariantCulture), 2);
                        num2 = Math.Round(double.Parse(current.Groups[3].Value, CultureInfo.InvariantCulture), 2);
                        num3 = Math.Round(double.Parse(current.Groups[4].Value, CultureInfo.InvariantCulture), 2);
                    }
                    catch (SystemException)
                    {
                        continue;
                    }
                    name.Clear().Append(current.Groups[1].Value);
                    coords.X = num;
                    coords.Y = num2;
                    coords.Z = num3;
                    additionalData.Clear();
                    if ((current.Groups.Count == 6) && !string.IsNullOrWhiteSpace(current.Groups[5].Value))
                    {
                        additionalData.Append(current.Groups[5].Value);
                    }
                    return true;
                }
            }
            return false;
        }

        internal void RegisterChat(MyMultiplayerBase multiplayer)
        {
            if (!Sync.IsDedicated && MyFakes.ENABLE_GPS)
            {
                multiplayer.ChatMessageReceived += new Action<ulong, string, ChatChannel, long, string>(this.ParseChat);
            }
        }

        private void RemovePlayerGps(int gpsHash)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(MySession.Static.LocalPlayerId, out dictionary) && dictionary.TryGetValue(gpsHash, out gps))
            {
                if (gps.ShowOnHud)
                {
                    MyHud.GpsMarkers.UnregisterMarker(gps);
                }
                dictionary.Remove(gpsHash);
                Action<long> listChanged = MySession.Static.Gpss.ListChanged;
                if (listChanged != null)
                {
                    listChanged(MySession.Static.LocalPlayerId);
                }
            }
        }

        public void SaveGpss(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (MyFakes.ENABLE_GPS)
            {
                this.DiscardOld();
                if (checkpoint.Gps == null)
                {
                    checkpoint.Gps = new SerializableDictionary<long, MyObjectBuilder_Gps>();
                }
                foreach (KeyValuePair<long, Dictionary<int, MyGps>> pair in this.m_playerGpss)
                {
                    MyObjectBuilder_Gps gps;
                    if (!checkpoint.Gps.Dictionary.TryGetValue(pair.Key, out gps))
                    {
                        gps = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Gps>();
                    }
                    if (gps.Entries == null)
                    {
                        gps.Entries = new List<MyObjectBuilder_Gps.Entry>();
                    }
                    foreach (KeyValuePair<int, MyGps> pair2 in pair.Value)
                    {
                        if (pair2.Value.IsLocal)
                        {
                            continue;
                        }
                        if ((!Sync.IsServer || (pair2.Value.EntityId == 0)) || (MyEntities.GetEntityById(pair2.Value.EntityId, false) != null))
                        {
                            gps.Entries.Add(this.GetObjectBuilderEntry(pair2.Value));
                        }
                    }
                    checkpoint.Gps.Dictionary.Add(pair.Key, gps);
                }
            }
        }

        public int ScanText(string input, string desc = null)
        {
            int num = 0;
            foreach (Match match in Regex.Matches(input, m_ScanPattern))
            {
                double num2;
                double num3;
                double num4;
                string str = match.Groups[1].Value;
                try
                {
                    num2 = Math.Round(double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture), 2);
                    num3 = Math.Round(double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture), 2);
                    num4 = Math.Round(double.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture), 2);
                }
                catch (SystemException)
                {
                    continue;
                }
                MyGps gps1 = new MyGps();
                gps1.Name = str;
                gps1.Description = desc;
                gps1.Coords = new Vector3D(num2, num3, num4);
                gps1.ShowOnHud = false;
                MyGps gps = gps1;
                gps.UpdateHash();
                MySession.Static.Gpss.SendAddGps(MySession.Static.LocalPlayerId, ref gps, 0L, true);
                num++;
                if (num == PARSE_MAX_COUNT)
                {
                    break;
                }
            }
            return num;
        }

        public int ScanText(string input, StringBuilder desc) => 
            this.ScanText(input, desc.ToString());

        public unsafe void SendAddGps(long identityId, ref MyGps gps, long entityId = 0L, bool playSoundOnCreation = true)
        {
            if (identityId != 0)
            {
                AddMsg* msgPtr1;
                AddMsg msg = new AddMsg {
                    IdentityId = identityId,
                    Name = gps.Name,
                    DisplayName = gps.DisplayName,
                    Description = gps.Description,
                    Coords = gps.Coords,
                    ShowOnHud = gps.ShowOnHud
                };
                msgPtr1->IsFinal = gps.DiscardAt == null;
                msgPtr1 = (AddMsg*) ref msg;
                msg.AlwaysVisible = gps.AlwaysVisible;
                msg.EntityId = entityId;
                msg.GPSColor = gps.GPSColor;
                msg.IsContainerGPS = gps.IsContainerGPS;
                msg.PlaySoundOnCreation = playSoundOnCreation;
                msg.IsObjective = gps.IsObjective;
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<AddMsg>(s => new Action<AddMsg>(MyGpsCollection.OnAddGps), msg, targetEndpoint, position);
            }
        }

        private void SendChangeAlwaysVisible(long identityId, int gpsHash, bool alwaysVisible)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, bool>(s => new Action<long, int, bool>(MyGpsCollection.AlwaysVisibleRequest), identityId, gpsHash, alwaysVisible, targetEndpoint, position);
        }

        private void SendChangeColor(long identityId, int gpsHash, Color color)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, Color>(s => new Action<long, int, Color>(MyGpsCollection.ColorRequest), identityId, gpsHash, color, targetEndpoint, position);
        }

        private void SendChangeShowOnHud(long identityId, int gpsHash, bool show)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int, bool>(s => new Action<long, int, bool>(MyGpsCollection.ShowOnHudRequest), identityId, gpsHash, show, targetEndpoint, position);
        }

        public void SendDelete(long identityId, int gpsHash)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, int>(s => new Action<long, int>(MyGpsCollection.DeleteRequest), identityId, gpsHash, targetEndpoint, position);
        }

        public void SendModifyGps(long identityId, MyGps gps)
        {
            ModifyMsg msg = new ModifyMsg {
                IdentityId = identityId,
                Name = gps.Name,
                Description = gps.Description,
                Coords = gps.Coords,
                Hash = gps.Hash,
                GPSColor = gps.GPSColor
            };
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<ModifyMsg>(s => new Action<ModifyMsg>(MyGpsCollection.ModifyRequest), msg, targetEndpoint, position);
        }

        [Event(null, 0x132), Reliable, Server]
        private static void ShowOnHudRequest(long identityId, int gpsHash, bool show)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && (MySession.Static.Players.TryGetSteamId(identityId) != MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                Dictionary<int, MyGps> dictionary;
                if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary))
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Vector3D? position = null;
                    MyMultiplayer.RaiseStaticEvent<long, int, bool>(s => new Action<long, int, bool>(MyGpsCollection.ShowOnHudSuccess), identityId, gpsHash, show, targetEndpoint, position);
                }
            }
        }

        [Event(null, 0x141), Reliable, ServerInvoked, Broadcast]
        private static void ShowOnHudSuccess(long identityId, int gpsHash, bool show)
        {
            Dictionary<int, MyGps> dictionary;
            MyGps gps;
            if (MySession.Static.Gpss.m_playerGpss.TryGetValue(identityId, out dictionary) && dictionary.TryGetValue(gpsHash, out gps))
            {
                gps.ShowOnHud = show;
                if (!show)
                {
                    gps.AlwaysVisible = false;
                }
                gps.DiscardAt = null;
                Action<long, int> gpsChanged = MySession.Static.Gpss.GpsChanged;
                if (gpsChanged != null)
                {
                    gpsChanged(identityId, gpsHash);
                }
                if (identityId == MySession.Static.LocalPlayerId)
                {
                    if (gps.ShowOnHud)
                    {
                        MyHud.GpsMarkers.RegisterMarker(gps);
                    }
                    else
                    {
                        MyHud.GpsMarkers.UnregisterMarker(gps);
                    }
                }
            }
        }

        internal void UnregisterChat(MyMultiplayerBase multiplayer)
        {
            if (!Sync.IsDedicated && MyFakes.ENABLE_GPS)
            {
                multiplayer.ChatMessageReceived -= new Action<ulong, string, ChatChannel, long, string>(this.ParseChat);
            }
        }

        public void updateForHud()
        {
            if (this.lastPlayerId != MySession.Static.LocalPlayerId)
            {
                Dictionary<int, MyGps> dictionary;
                if (this.m_playerGpss.TryGetValue(this.lastPlayerId, out dictionary))
                {
                    foreach (KeyValuePair<int, MyGps> pair in dictionary)
                    {
                        MyHud.GpsMarkers.UnregisterMarker(pair.Value);
                    }
                }
                this.lastPlayerId = MySession.Static.LocalPlayerId;
                if (this.m_playerGpss.TryGetValue(this.lastPlayerId, out dictionary))
                {
                    foreach (KeyValuePair<int, MyGps> pair2 in dictionary)
                    {
                        if (pair2.Value.ShowOnHud)
                        {
                            MyHud.GpsMarkers.RegisterMarker(pair2.Value);
                        }
                    }
                }
            }
            this.DiscardOld();
        }

        void IMyGpsCollection.AddGps(long identityId, IMyGps gps)
        {
            MyGps gps2 = (MyGps) gps;
            this.SendAddGps(identityId, ref gps2, 0L, true);
        }

        void IMyGpsCollection.AddLocalGps(IMyGps gps)
        {
            MyGps gps2 = (MyGps) gps;
            gps2.IsLocal = true;
            if (this.AddPlayerGps(MySession.Static.LocalPlayerId, ref gps2) && gps.ShowOnHud)
            {
                MyHud.GpsMarkers.RegisterMarker(gps2);
            }
        }

        IMyGps IMyGpsCollection.Create(string name, string description, Vector3D coords, bool showOnHud, bool temporary)
        {
            MyGps gps = new MyGps {
                Name = name,
                Description = description,
                Coords = coords,
                ShowOnHud = showOnHud,
                GPSColor = new Color(0x75, 0xc9, 0xf1)
            };
            if (temporary)
            {
                gps.SetDiscardAt();
            }
            else
            {
                gps.DiscardAt = null;
            }
            gps.UpdateHash();
            return gps;
        }

        List<IMyGps> IMyGpsCollection.GetGpsList(long identityId)
        {
            reusableList.Clear();
            this.GetGpsList(identityId, reusableList);
            return reusableList;
        }

        void IMyGpsCollection.ModifyGps(long identityId, IMyGps gps)
        {
            MyGps gps2 = (MyGps) gps;
            this.SendModifyGps(identityId, gps2);
        }

        void IMyGpsCollection.RemoveGps(long identityId, int gpsHash)
        {
            this.SendDelete(identityId, gpsHash);
        }

        void IMyGpsCollection.RemoveGps(long identityId, IMyGps gps)
        {
            this.SendDelete(identityId, (gps as MyGps).Hash);
        }

        void IMyGpsCollection.RemoveLocalGps(int gpsHash)
        {
            this.RemovePlayerGps(gpsHash);
        }

        void IMyGpsCollection.RemoveLocalGps(IMyGps gps)
        {
            this.RemovePlayerGps(gps.Hash);
        }

        void IMyGpsCollection.SetShowOnHud(long identityId, int gpsHash, bool show)
        {
            this.SendChangeShowOnHud(identityId, gpsHash, show);
        }

        void IMyGpsCollection.SetShowOnHud(long identityId, IMyGps gps, bool show)
        {
            this.SendChangeShowOnHud(identityId, (gps as MyGps).Hash, show);
        }

        public Dictionary<int, MyGps> this[long id] =>
            this.m_playerGpss[id];

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGpsCollection.<>c <>9 = new MyGpsCollection.<>c();
            public static Func<IMyEventOwner, Action<MyGpsCollection.AddMsg>> <>9__5_0;
            public static Func<IMyEventOwner, Action<long, int>> <>9__7_0;
            public static Func<IMyEventOwner, Action<long, int>> <>9__8_0;
            public static Func<IMyEventOwner, Action<MyGpsCollection.ModifyMsg>> <>9__10_0;
            public static Func<IMyEventOwner, Action<MyGpsCollection.ModifyMsg>> <>9__11_0;
            public static Func<IMyEventOwner, Action<long, int, bool>> <>9__20_0;
            public static Func<IMyEventOwner, Action<long, int, bool>> <>9__21_0;
            public static Func<IMyEventOwner, Action<long, int, bool>> <>9__24_0;
            public static Func<IMyEventOwner, Action<long, int, bool>> <>9__25_0;
            public static Func<IMyEventOwner, Action<long, int, Color>> <>9__28_0;
            public static Func<IMyEventOwner, Action<long, int, Color>> <>9__29_0;

            internal Action<long, int, bool> <AlwaysVisibleRequest>b__25_0(IMyEventOwner s) => 
                new Action<long, int, bool>(MyGpsCollection.AlwaysVisibleSuccess);

            internal Action<long, int, Color> <ColorRequest>b__29_0(IMyEventOwner s) => 
                new Action<long, int, Color>(MyGpsCollection.ColorSuccess);

            internal Action<long, int> <DeleteRequest>b__8_0(IMyEventOwner s) => 
                new Action<long, int>(MyGpsCollection.DeleteSuccess);

            internal Action<MyGpsCollection.ModifyMsg> <ModifyRequest>b__11_0(IMyEventOwner s) => 
                new Action<MyGpsCollection.ModifyMsg>(MyGpsCollection.ModifySuccess);

            internal Action<MyGpsCollection.AddMsg> <SendAddGps>b__5_0(IMyEventOwner s) => 
                new Action<MyGpsCollection.AddMsg>(MyGpsCollection.OnAddGps);

            internal Action<long, int, bool> <SendChangeAlwaysVisible>b__24_0(IMyEventOwner s) => 
                new Action<long, int, bool>(MyGpsCollection.AlwaysVisibleRequest);

            internal Action<long, int, Color> <SendChangeColor>b__28_0(IMyEventOwner s) => 
                new Action<long, int, Color>(MyGpsCollection.ColorRequest);

            internal Action<long, int, bool> <SendChangeShowOnHud>b__20_0(IMyEventOwner s) => 
                new Action<long, int, bool>(MyGpsCollection.ShowOnHudRequest);

            internal Action<long, int> <SendDelete>b__7_0(IMyEventOwner s) => 
                new Action<long, int>(MyGpsCollection.DeleteRequest);

            internal Action<MyGpsCollection.ModifyMsg> <SendModifyGps>b__10_0(IMyEventOwner s) => 
                new Action<MyGpsCollection.ModifyMsg>(MyGpsCollection.ModifyRequest);

            internal Action<long, int, bool> <ShowOnHudRequest>b__21_0(IMyEventOwner s) => 
                new Action<long, int, bool>(MyGpsCollection.ShowOnHudSuccess);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AddMsg
        {
            public long IdentityId;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string Name;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string DisplayName;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string Description;
            public Vector3D Coords;
            public bool ShowOnHud;
            public bool IsFinal;
            public bool AlwaysVisible;
            public long EntityId;
            public Color GPSColor;
            public bool IsContainerGPS;
            public bool PlaySoundOnCreation;
            public bool IsObjective;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModifyMsg
        {
            public long IdentityId;
            public int Hash;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string Name;
            [Serialize(MyObjectFlags.DefaultZero)]
            public string Description;
            public Vector3D Coords;
            public Color GPSColor;
        }
    }
}

