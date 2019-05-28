namespace Sandbox.Engine.Analytics
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Management;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Scripting;
    using VRage.Utils;
    using VRage.Win32;
    using VRageMath;
    using VRageRender;

    public abstract class MyAnalyticsHelper
    {
        public const string ANALYTICS_CONDITION_STRING = "WINDOWS";
        private static MyDamageInformation m_lastDamageInformation;
        private static bool m_scenarioFlag;
        private static bool m_loadingStarted;
        private static int ReportChecksActivityStart;
        private static int ReportChecksActivityEnd;
        private static int ReportChecksLastMinute;
        private static ConcurrentDictionary<MyTuple<string, int>, byte> m_reportedBugs;
        private DateTime m_gameplayStartTime = DateTime.UtcNow;
        private bool m_isSessionStarted;
        private bool m_isSessionEnded;
        private bool m_firstRun;
        protected MyGameEntryEnum m_entry;
        private DateTime m_loadingStartedAt;
        private Dictionary<string, object> m_defaultSessionData;

        static MyAnalyticsHelper()
        {
            MyDamageInformation information = new MyDamageInformation {
                Type = MyStringHash.NullOrEmpty
            };
            m_lastDamageInformation = information;
            m_loadingStarted = false;
            ReportChecksActivityStart = 0;
            ReportChecksActivityEnd = 0;
            ReportChecksLastMinute = DateTime.UtcNow.Minute;
            m_reportedBugs = new ConcurrentDictionary<MyTuple<string, int>, byte>();
        }

        protected MyAnalyticsHelper()
        {
        }

        protected Dictionary<string, object> CopyDefaultSessionData()
        {
            if ((this.m_defaultSessionData == null) || (this.m_defaultSessionData.Count == 0))
            {
                return new Dictionary<string, object>();
            }
            return new Dictionary<string, object>(this.m_defaultSessionData);
        }

        [Conditional("WINDOWS")]
        public void EndSession()
        {
            if (!this.m_isSessionEnded)
            {
                try
                {
                    Dictionary<string, object> sessionData = this.CopyDefaultSessionData();
                    MyAnalyticsManager.Instance.EndSession(sessionData);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        protected virtual Dictionary<string, object> GetGameplayEndData()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            try
            {
                MyFpsManager.PrepareMinMax();
                dictionary["game_duration"] = (uint) MySession.Static.ElapsedPlayTime.TotalSeconds;
                dictionary["entire_world_duration"] = (uint) MySession.Static.ElapsedGameTime.TotalSeconds;
                dictionary["fps_average"] = (uint) (((double) MyFpsManager.GetSessionTotalFrames()) / MySession.Static.ElapsedPlayTime.TotalSeconds);
                dictionary["fps_minimum"] = (uint) MyFpsManager.GetMinSessionFPS();
                dictionary["fps_maximum"] = (uint) MyFpsManager.GetMaxSessionFPS();
                dictionary["ups_average"] = (uint) (((double) MyGameStats.Static.UpdateCount) / MySession.Static.ElapsedPlayTime.TotalSeconds);
                dictionary["simspeed_client_average"] = (float) (((double) MySession.Static.SessionSimSpeedPlayer) / MySession.Static.ElapsedPlayTime.TotalSeconds);
                dictionary["simspeed_server_average"] = (float) (((double) MySession.Static.SessionSimSpeedServer) / MySession.Static.ElapsedPlayTime.TotalSeconds);
            }
            catch (Exception exception)
            {
                dictionary["failed_to_get_data"] = exception.Message + "\n" + exception.StackTrace;
            }
            return dictionary;
        }

        protected virtual Dictionary<string, object> GetGameplayStartData(MyGameEntryEnum entry, MyObjectBuilder_SessionSettings settings)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            try
            {
                dictionary["entry"] = entry.ToString();
                dictionary["game_mode"] = settings.GameMode.ToString();
                dictionary["online_mode"] = settings.OnlineMode.ToString();
                dictionary["world_type"] = MySession.Static.Scenario.Id.SubtypeName;
                dictionary["worldName"] = MySession.Static.Name;
                dictionary["server_is_dedicated"] = (MyMultiplayer.Static == null) ? ((object) 0) : ((object) MyMultiplayer.Static.HostName.Equals("Dedicated server"));
                dictionary["server_name"] = (MyMultiplayer.Static != null) ? MyMultiplayer.Static.HostName : MySession.Static.LocalHumanPlayer.DisplayName;
                dictionary["server_max_number_of_players"] = (MyMultiplayer.Static != null) ? MyMultiplayer.Static.MemberLimit : 1;
                dictionary["server_current_number_of_players"] = (MyMultiplayer.Static != null) ? MyMultiplayer.Static.MemberCount : 1;
                dictionary["is_hosting_player"] = (MyMultiplayer.Static != null) ? ((object) MyMultiplayer.Static.IsServer) : ((object) 1);
                if (MyMultiplayer.Static == null)
                {
                    dictionary["multiplayer_type"] = "Off-line";
                }
                else if (((MySession.Static == null) || (MySession.Static.LocalCharacter == null)) || !MyMultiplayer.Static.HostName.Equals(MySession.Static.LocalCharacter.DisplayNameText))
                {
                    dictionary["multiplayer_type"] = !MyMultiplayer.Static.HostName.Equals("Dedicated server") ? "Client" : "Dedicated server";
                }
                else
                {
                    dictionary["multiplayer_type"] = "Host";
                }
                dictionary["active_mods"] = GetModList();
                dictionary["active_mods_count"] = MySession.Static.Mods.Count;
                long num = (long) Math.Ceiling((DateTime.UtcNow - this.m_loadingStartedAt).TotalSeconds);
                dictionary["loading_duration"] = num;
            }
            catch (Exception exception)
            {
                dictionary["failed_to_get_data"] = exception.Message + "\n" + exception.StackTrace;
            }
            return dictionary;
        }

        private static string GetModList()
        {
            string str = string.Empty;
            foreach (MyObjectBuilder_Checkpoint.ModItem item in MySession.Static.Mods)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str + ", ";
                }
                str = str + item.FriendlyName.Replace(",", "");
            }
            return str;
        }

        public static MyPlanetNamesData GetPlanetNames(Vector3D position)
        {
            MyPlanetNamesData data = new MyPlanetNamesData();
            MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            if ((closestPlanet == null) || (vector.LengthSquared() <= 0f))
            {
                data.planetName = "";
                data.planetType = "";
            }
            else
            {
                data.planetName = closestPlanet.StorageName;
                data.planetType = closestPlanet.Generator.FolderName;
            }
            return data;
        }

        protected virtual Dictionary<string, object> GetSessionData()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            try
            {
                dictionary["game_version"] = MyPerGameSettings.BasicGameInfo.GameVersion.ToString();
                dictionary["game_branch"] = MyGameService.BranchNameFriendly;
                string str = new ManagementObjectSearcher(@"root\CIMV2", "SELECT Name FROM Win32_Processor").Get().Cast<ManagementObject>().First<ManagementObject>()["Name"].ToString();
                dictionary["cpu_info"] = str;
                dictionary["cpu_number_of_cores"] = Environment.ProcessorCount;
                WinApi.MEMORYSTATUSEX lpBuffer = new WinApi.MEMORYSTATUSEX();
                WinApi.GlobalMemoryStatusEx(lpBuffer);
                dictionary["ram_size"] = (lpBuffer.ullTotalPhys / ((ulong) 0x400L)) / ((ulong) 0x400L);
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MyAdapterInfo info = MyVideoSettingsManager.Adapters[MyVideoSettingsManager.CurrentDeviceSettings.AdapterOrdinal];
                    dictionary["gpu_name"] = info.Name;
                    dictionary["gpu_memory"] = (info.VRAM / ((ulong) 0x400L)) / ((ulong) 0x400L);
                    dictionary["gpu_driver_version"] = info.DriverVersion;
                }
                dictionary["os_info"] = Environment.OSVersion.VersionString;
                dictionary["os_platform"] = Environment.Is64BitOperatingSystem ? "64bit" : "32bit";
                dictionary["dx11_support"] = Sandbox.Engine.Platform.Game.IsDedicated ? ((object) 1) : ((object) MyDirectXHelper.IsDx11Supported());
                dictionary["is_first_run"] = this.m_firstRun;
                dictionary["is_dedicated"] = Sandbox.Engine.Platform.Game.IsDedicated;
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    dictionary["display_resolution"] = MySandboxGame.Config.ScreenWidth.ToString() + " x " + MySandboxGame.Config.ScreenHeight.ToString();
                    dictionary["display_window_mode"] = MyVideoSettingsManager.CurrentDeviceSettings.WindowMode.ToString();
                    dictionary["graphics_anisotropic_filtering"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AnisotropicFiltering.ToString();
                    dictionary["graphics_antialiasing_mode"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AntialiasingMode.ToString();
                    dictionary["graphics_shadow_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.ShadowQuality.ToString();
                    dictionary["graphics_texture_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.TextureQuality.ToString();
                    dictionary["graphics_voxel_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelQuality.ToString();
                    dictionary["graphics_grass_density_factor"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.GrassDensityFactor;
                    dictionary["graphics_grass_draw_distance"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.GrassDrawDistance;
                    dictionary["graphics_flares_intensity"] = MyVideoSettingsManager.CurrentGraphicsSettings.FlaresIntensity;
                    dictionary["graphics_voxel_shader_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.VoxelShaderQuality;
                    dictionary["graphics_alphamasked_shader_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AlphaMaskedShaderQuality;
                    dictionary["graphics_atmosphere_shader_quality"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.AtmosphereShaderQuality;
                    dictionary["graphics_distance_fade"] = MyVideoSettingsManager.CurrentGraphicsSettings.PerformanceSettings.RenderSettings.DistanceFade;
                    dictionary["audio_music_volume"] = MySandboxGame.Config.MusicVolume;
                    dictionary["audio_sound_volume"] = MySandboxGame.Config.GameVolume;
                    dictionary["audio_mute_when_not_in_focus"] = MySandboxGame.Config.EnableMuteWhenNotInFocus;
                }
            }
            catch (Exception exception)
            {
                dictionary["failed_to_get_data"] = exception.Message + "\n" + exception.StackTrace;
            }
            return dictionary;
        }

        private static bool IsReportedPlayer(MyEntity entity)
        {
            if (entity != null)
            {
                IMyControllableEntity entity2 = entity as IMyControllableEntity;
                if ((entity2 == null) || !entity2.ControllerInfo.IsLocallyControlled())
                {
                    return ((entity.Parent != null) && IsReportedPlayer(entity.Parent));
                }
            }
            return true;
        }

        protected virtual void RegisterEventsInVisualScripting()
        {
        }

        [Conditional("WINDOWS")]
        public static void ReportActivityEnd(MyEntity sourceEntity, string activityName)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && !SanityCheckAmountPerMinute(ReportChecksActivityEnd, 60))
            {
                try
                {
                    if (IsReportedPlayer(sourceEntity))
                    {
                        if (((MySession.Static == null) || (MySession.Static.LocalCharacter == null)) || (MySession.Static.LocalCharacter.PositionComp == null))
                        {
                            MyAnalyticsManager.Instance.ReportActivityEnd(activityName, "", "", MyPhysics.SimulationRatio, Sync.ServerSimulationRatio);
                        }
                        else
                        {
                            MyPlanetNamesData planetNames = GetPlanetNames(MySession.Static.LocalCharacter.PositionComp.GetPosition());
                            MyAnalyticsManager.Instance.ReportActivityEnd(activityName, planetNames.planetName, planetNames.planetType, MyPhysics.SimulationRatio, Sync.ServerSimulationRatio);
                        }
                        ReportChecksActivityEnd++;
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportActivityStart(MyEntity sourceEntity, string activityName, string activityFocus, string activityType, string activityItemUsage, bool expectActivityEnd = true)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && !SanityCheckAmountPerMinute(ReportChecksActivityStart, 60))
            {
                try
                {
                    if (IsReportedPlayer(sourceEntity))
                    {
                        if (((MySession.Static == null) || (MySession.Static.LocalCharacter == null)) || (MySession.Static.LocalCharacter.PositionComp == null))
                        {
                            MyAnalyticsManager.Instance.ReportActivityStart(activityName, activityFocus, activityType, activityItemUsage, expectActivityEnd, "", "", MyPhysics.SimulationRatio, Sync.ServerSimulationRatio);
                        }
                        else
                        {
                            MyPlanetNamesData planetNames = GetPlanetNames(MySession.Static.LocalCharacter.PositionComp.GetPosition());
                            MyAnalyticsManager.Instance.ReportActivityStart(activityName, activityFocus, activityType, activityItemUsage, expectActivityEnd, planetNames.planetName, planetNames.planetType, MyPhysics.SimulationRatio, Sync.ServerSimulationRatio);
                        }
                        ReportChecksActivityStart++;
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportActivityStartIf(bool condition, MyEntity sourceEntity, string activityName, string activityFocus, string activityType, string activityItemUsage, bool expectActivityEnd = true)
        {
            try
            {
                if (condition)
                {
                    ReportActivityStart(sourceEntity, activityName, activityFocus, activityType, activityItemUsage, expectActivityEnd);
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportBug(string data, string ticket = null, bool firstTimeOnly = true, [CallerFilePath] string file = "", [CallerLineNumber] int line = -1)
        {
            if (!firstTimeOnly || m_reportedBugs.TryAdd(MyTuple.Create<string, int>(file, line), 0))
            {
                object[] objArray1 = new object[] { "[", file, ":", line, "]", data };
                string msg = string.Concat(objArray1);
                if (ticket != null)
                {
                    msg = "[" + ticket + "]" + msg;
                }
                if (string.IsNullOrEmpty(file))
                {
                    msg = data;
                }
                MyLog.Default.WriteLine(msg);
                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData.Add("data", msg);
                ReportEvent(MyAnalyticsProgressionStatus.BugReport, eventData, 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportEvent(MyAnalyticsProgressionStatus status, Dictionary<string, object> eventData, double timestamp = 0.0)
        {
            try
            {
                MyAnalyticsManager.Instance.ReportEvent(status, eventData, timestamp);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
            }
        }

        [Conditional("WINDOWS")]
        public void ReportGameCrash(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(exception.Message);
            builder.AppendLine();
            builder.AppendLine(exception.StackTrace.ToString());
            Dictionary<string, object> eventData = new Dictionary<string, object> {
                ["exception"] = builder.ToString()
            };
            ReportEvent(MyAnalyticsProgressionStatus.GameCrash, eventData, 0.0);
        }

        [Conditional("WINDOWS")]
        public void ReportGameplayEnd()
        {
            if (this.m_isSessionStarted)
            {
                ReportUsedScriptNamespaces();
                ReportEvent(MyAnalyticsProgressionStatus.WorldEnd, this.GetGameplayEndData(), 0.0);
            }
        }

        [Conditional("WINDOWS")]
        public void ReportGameplayStart(MyObjectBuilder_SessionSettings settings)
        {
            if (this.m_isSessionStarted)
            {
                Dictionary<string, object> gameplayStartData = this.GetGameplayStartData(this.m_entry, settings);
                this.m_gameplayStartTime = DateTime.UtcNow;
                ReportEvent(MyAnalyticsProgressionStatus.WorldStart, gameplayStartData, 0.0);
                this.ReportMods();
            }
        }

        [Conditional("WINDOWS")]
        public void ReportGameQuit(string reason)
        {
            if (this.m_isSessionStarted)
            {
                Dictionary<string, object> eventData = new Dictionary<string, object> {
                    ["reason"] = reason,
                    ["game_duration"] = (MySandboxGame.TotalTimeInMilliseconds / 0x3e8).ToString()
                };
                ReportEvent(MyAnalyticsProgressionStatus.GameQuit, eventData, 0.0);
            }
        }

        private void ReportMods()
        {
            try
            {
                for (int i = 0; i < MySession.Static.Mods.Count; i++)
                {
                    MyAnalyticsManager.Instance.ReportModLoaded(MySession.Static.Mods[i].FriendlyName);
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
            }
        }

        public void ReportMouseClick(string screen, Vector2 position, uint seconds)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                try
                {
                    MyAnalyticsManager.Instance.ReportScreenMouseClick(screen, position.X, position.Y, seconds);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportPlayerDeath(bool isLocallyControlled, ulong playerSteamId)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                try
                {
                    if (isLocallyControlled)
                    {
                        int isVanilla;
                        string deathCause = m_lastDamageInformation.Type.String;
                        bool flag = false;
                        bool flag2 = false;
                        if ((m_lastDamageInformation.Type != MyStringHash.NullOrEmpty) && (m_lastDamageInformation.AttackerId != 0))
                        {
                            if (m_lastDamageInformation.Type == MyDamageType.Suicide)
                            {
                                flag2 = true;
                            }
                            else
                            {
                                MyEntity entity = null;
                                MyEntities.TryGetEntityById(m_lastDamageInformation.AttackerId, out entity, false);
                                IMyControllableEntity entity2 = entity as IMyControllableEntity;
                                if (entity2 == null)
                                {
                                    if (((entity is IMyGunBaseUser) || (entity is IMyHandheldGunObject<MyToolBase>)) || (entity is IMyHandheldGunObject<MyGunBase>))
                                    {
                                        flag = true;
                                    }
                                }
                                else
                                {
                                    MyEntityController controller = entity2.ControllerInfo.Controller;
                                    if (controller != null)
                                    {
                                        if (controller.Player.Id.SteamId != playerSteamId)
                                        {
                                            flag = true;
                                        }
                                        else
                                        {
                                            flag2 = true;
                                        }
                                    }
                                }
                            }
                        }
                        string deathType = !flag ? (!flag2 ? (!(m_lastDamageInformation.Type == MyDamageType.Destruction) ? ((m_lastDamageInformation.Type == MyDamageType.Environment) ? "environment" : "unknown") : "cockpit_destruction") : "self_inflicted") : "pvp";
                        MyPlanetNamesData planetNames = GetPlanetNames(MySession.Static.LocalCharacter.PositionComp.GetPosition());
                        bool campaign = (MyCampaignManager.Static != null) && MyCampaignManager.Static.IsCampaignRunning;
                        if (!campaign || (MyCampaignManager.Static.ActiveCampaign == null))
                        {
                            isVanilla = 0;
                        }
                        else
                        {
                            isVanilla = (int) MyCampaignManager.Static.ActiveCampaign.IsVanilla;
                        }
                        MyAnalyticsManager.Instance.ReportPlayerDeath(deathType, deathCause, planetNames.planetName, planetNames.planetType, campaign, (bool) isVanilla, MySession.Static.Settings.GameMode.ToString(), GetModList(), MySession.Static.Mods.Count);
                    }
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public static void ReportServerStatus()
        {
            if (((MyMultiplayer.Static != null) && MyMultiplayer.Static.IsServer) && (MyMultiplayer.Static.MemberCount > 1))
            {
                try
                {
                    int gridCount = 0;
                    int blockCount = 0;
                    int movingGridsCount = 0;
                    MyConcurrentHashSet<MyEntity> entities = MyEntities.GetEntities();
                    foreach (MyEntity entity in entities)
                    {
                        if (!(entity is MyCubeGrid))
                        {
                            continue;
                        }
                        gridCount++;
                        blockCount += (entity as MyCubeGrid).BlocksCount;
                        if (((entity as MyCubeGrid).Physics != null) && ((entity as MyCubeGrid).Physics.LinearVelocity != Vector3.Zero))
                        {
                            movingGridsCount++;
                        }
                    }
                    MyAnalyticsManager.Instance.ReportServerStatus(MyMultiplayer.Static.MemberCount, MyMultiplayer.Static.MemberLimit, Sync.ServerSimulationRatio, entities.Count, gridCount, blockCount, movingGridsCount, MyMultiplayer.Static.HostName, MySession.Static.Scenario.Id.SubtypeName, MySession.Static.Name, (uint) MySession.Static.ElapsedGameTime.TotalSeconds);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public virtual void ReportToolbarSwitch(int page)
        {
        }

        [Conditional("WINDOWS")]
        public static void ReportUsedScriptNamespaces()
        {
            try
            {
                MySandboxGame.Log.WriteLineAndConsole("Used namespaces in scripts:");
                foreach (KeyValuePair<string, int> pair in MyScriptCompiler.UsedNamespaces)
                {
                    MyAnalyticsManager.Instance.ReportUsedNamespace(pair.Key, pair.Value);
                }
                MyScriptCompiler.UsedNamespaces.Clear();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
            }
        }

        private static bool SanityCheckAmountPerMinute(int reportCount, int limit)
        {
            if (DateTime.UtcNow.Minute != ReportChecksLastMinute)
            {
                ReportChecksLastMinute = DateTime.UtcNow.Minute;
                ReportChecksActivityStart = 0;
                ReportChecksActivityEnd = 0;
            }
            return (reportCount >= limit);
        }

        private static bool SanityCheckOnePerMinute(ref int lastInstance)
        {
            int num = (DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute;
            if (num == lastInstance)
            {
                return true;
            }
            lastInstance = num;
            return false;
        }

        [Conditional("WINDOWS")]
        public static void SetLastDamageInformation(MyDamageInformation lastDamageInformation)
        {
            try
            {
                MyStringHash hash = new MyStringHash();
                if (!(lastDamageInformation.Type == hash))
                {
                    m_lastDamageInformation = lastDamageInformation;
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLine(exception);
            }
        }

        [Conditional("WINDOWS")]
        public void StartSessionAndIdentifyPlayer(bool firstTimeRun)
        {
            this.m_firstRun = firstTimeRun;
            if (!this.m_isSessionStarted)
            {
                try
                {
                    this.m_defaultSessionData = this.GetSessionData();
                    Dictionary<string, object> identificationData = this.CopyDefaultSessionData();
                    if (Sandbox.Engine.Platform.Game.IsDedicated)
                    {
                        MyAnalyticsManager.Instance.IdentifyPlayer("0", "Dedicated Server", false, identificationData);
                    }
                    else
                    {
                        MyAnalyticsManager.Instance.IdentifyPlayer(MyGameService.UserId.ToString(), MyGameService.UserName, MyGameService.IsOnline, identificationData);
                    }
                    Dictionary<string, object> sessionData = this.CopyDefaultSessionData();
                    MyAnalyticsManager.Instance.StartSession(sessionData);
                    this.m_isSessionStarted = true;
                    MyGuiScreenBase.MouseClickEvent += new Action<string, Vector2, uint>(this.ReportMouseClick);
                    MyLog.Default.WriteLine("Analytics helper process start reported");
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLine(exception);
                }
            }
        }

        [Conditional("WINDOWS")]
        public void StoreLoadingStartTime()
        {
            this.m_loadingStartedAt = DateTime.UtcNow;
        }
    }
}

