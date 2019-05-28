namespace Sandbox.Game
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.AI;
    using Sandbox.Game.Audio;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Data.Audio;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders.AI;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Game.SessionComponents;
    using VRage.Game.VisualScripting;
    using VRage.Input;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [StaticEventOwner]
    public static class MyVisualScriptLogicProvider
    {
        private static readonly Dictionary<Vector3I, bool> m_thrustDirections = new Dictionary<Vector3I, bool>();
        private static readonly Dictionary<int, MyHudNotification> m_addedNotificationsById = new Dictionary<int, MyHudNotification>();
        private static int m_notificationIdCounter;
        [Display(Name="Player Left Cockpit", Description="When player leaves cockpit.")]
        public static DoubleKeyPlayerEvent PlayerLeftCockpit;
        [Display(Name="Player Entered Cockpit", Description="When player leaves cockpit.")]
        public static DoubleKeyPlayerEvent PlayerEnteredCockpit;
        [Display(Name="Cutscene Node Event", Description="")]
        public static CutsceneEvent CutsceneNodeEvent;
        [Display(Name="Cutscene Ended", Description="When cutscene ended.")]
        public static CutsceneEvent CutsceneEnded;
        [Display(Name="Player Spawned", Description="When player spawns in the world.")]
        public static SingleKeyPlayerEvent PlayerSpawned;
        [Display(Name="Player Died", Description="When player dies in the world.")]
        public static SingleKeyPlayerEvent PlayerDied;
        [Display(Name="Player Connected", Description="When player connects.")]
        public static SingleKeyPlayerEvent PlayerConnected;
        [Display(Name="Player Disconnected", Description="When player disconnects.")]
        public static SingleKeyPlayerEvent PlayerDisconnected;
        [Display(Name="Player Respawned", Description="When player respawns.")]
        public static SingleKeyPlayerEvent PlayerRespawnRequest;
        [Display(Name="NPC Died", Description="When player dies.")]
        public static SingleKeyEntityNameEvent NPCDied;
        [Display(Name="Player Health Recharging", Description="When player is recharging health.")]
        public static PlayerHealthRechargeEvent PlayerHealthRecharging;
        [Display(Name="Player Suit Recharging", Description="When suit is recharging power/oxygen/hydrogen.")]
        public static PlayerSuitRechargeEvent PlayerSuitRecharging;
        [Display(Name="Timer Block Triggered", Description="When timer block triggers.")]
        public static SingleKeyEntityNameEvent TimerBlockTriggered;
        [Display(Name="Timer Block Triggered Entity Name", Description="")]
        public static SingleKeyEntityNameEvent TimerBlockTriggeredEntityName;
        [Display(Name="Player Picked Up Item", Description="When player picks up item.")]
        public static FloatingObjectPlayerEvent PlayerPickedUp;
        [Display(Name="Player Dropped Item", Description="When player drops item.")]
        public static PlayerItemEvent PlayerDropped;
        [Display(Name="Item Spawned", Description="When item spawns.")]
        public static ItemSpawnedEvent ItemSpawned;
        [Display(Name="Button Pressed Entity Name", Description="When someone press the button.")]
        public static ButtonPanelEvent ButtonPressedEntityName;
        [Display(Name="Button Pressed Terminal Name", Description="When someone press the button.")]
        public static ButtonPanelEvent ButtonPressedTerminalName;
        [Display(Name="Area Trigger Entity Left", Description="When entity leaves area of the trigger.")]
        public static TriggerEventComplex AreaTrigger_EntityLeft;
        [Display(Name="Area Trigger Entity Entered", Description="When entity enters area of the trigger.")]
        public static TriggerEventComplex AreaTrigger_EntityEntered;
        [Display(Name="Area Trigger Left", Description="When player leaves area of the trigger.")]
        public static SingleKeyTriggerEvent AreaTrigger_Left;
        [Display(Name="Area Trigger Entered", Description="When player enters area of the trigger.")]
        public static SingleKeyTriggerEvent AreaTrigger_Entered;
        [Display(Name="Screen Added", Description="When screen is added.")]
        public static ScreenManagerEvent ScreenAdded;
        [Display(Name="Screen Removed", Description="When screen is removed.")]
        public static ScreenManagerEvent ScreenRemoved;
        [Display(Name="Block Destroyed", Description="When block is destroyed.")]
        public static SingleKeyEntityNameGridNameEvent BlockDestroyed;
        [Display(Name="Block Built", Description="When block is build.")]
        public static BlockEvent BlockBuilt;
        [Display(Name="Prefab Spawned", Description="When prefab is spawned.")]
        public static SingleKeyEntityNameEvent PrefabSpawned;
        [Display(Name="Block Functionality Changed", Description="When block function state is changed.")]
        public static BlockFunctionalityChangedEvent BlockFunctionalityChanged;
        [Display(Name="Tool Equipped", Description="When tool is equipped.")]
        public static ToolEquipedEvent ToolEquipped;
        [Display(Name="Landing Gear Unlocked", Description="When landing gear is unlocked.")]
        public static LandingGearUnlockedEvent LandingGearUnlocked;
        [Display(Name="Grid Power Generation State Changed", Description="When grid power generation state is changed.")]
        public static GridPowerGenerationStateChangedEvent GridPowerGenerationStateChanged;
        [Display(Name="Room Fully Pressurized", Description="When room is fully pressurized.")]
        public static RoomFullyPressurizedEvent RoomFullyPressurized;
        [Display(Name="NewBuiltItem", Description="When new item is build.")]
        public static NewBuiltItemEvent NewItemBuilt;
        [Display(Name="WeaponBlockActivated", Description="When gatling gun or missile launcher shoots.")]
        public static WeaponBlockActivatedEvent WeaponBlockActivated;
        [Display(Name="ConnectorStateChanged", Description="When Two connectors dis/connect.")]
        public static ConnectorStateChangedEvent ConnectorStateChanged;
        [Display(Name="GridJumped", Description="When grid uses jumpdrive to jump.")]
        public static GridJumpedEvent GridJumped;
        [Display(Name="ShipDrillDrilled", Description="When drill obtains ore by mining voxels.")]
        public static ShipDrillCollectedEvent ShipDrillCollected;
        [Display(Name="RemoteControlChanged", Description="When remote control block get controlled by player.")]
        public static RemoteControlChangedEvent RemoteControlChanged;
        [Display(Name="ToolbarItemChanged", Description="When an item on a toolbar is changed.")]
        public static ToolbarItemChangedEvent ToolbarItemChanged;
        private static MyStringId MUSIC = MyStringId.GetOrCompute("Music");
        private static MyStringHash DAMAGE_TYPE_SCRIPT = MyStringHash.GetOrCompute("Script");
        public static bool GameIsReady = false;
        private static bool m_registered = false;
        private static bool m_exitGameDialogOpened = false;
        private static readonly Dictionary<long, List<MyTuple<long, int>>> m_playerIdsToHighlightData = new Dictionary<long, List<MyTuple<long, int>>>();
        private static readonly Color DEFAULT_HIGHLIGHT_COLOR = new Color(0, 0x60, 0xd1, 0x19);
        private static readonly MyDefinitionId ElectricityId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");

        [VisualScriptingMiscData("GPS and Highlights", "Adds GPS for specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPS(string name, string description, Vector3D position, Color GPSColor, int disappearsInS = 0, long playerId = -1L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyGps gps1 = new MyGps();
            gps1.ShowOnHud = true;
            gps1.Coords = position;
            gps1.Name = name;
            gps1.Description = description;
            gps1.AlwaysVisible = true;
            MyGps gps = gps1;
            if (disappearsInS > 0)
            {
                gps.DiscardAt = new TimeSpan?(TimeSpan.FromSeconds(MySession.Static.ElapsedPlayTime.TotalSeconds + disappearsInS));
            }
            else
            {
                gps.DiscardAt = null;
            }
            if (GPSColor != Color.Transparent)
            {
                gps.GPSColor = GPSColor;
            }
            MySession.Static.Gpss.SendAddGps(playerId, ref gps, 0L, true);
        }

        [VisualScriptingMiscData("GPS and Highlights", "Adds GPS for all players.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSForAll(string name, string description, Vector3D position, Color GPSColor, int disappearsInS = 0)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    AddGPS(name, description, position, GPSColor, disappearsInS, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Adds GPS for specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSObjective(string name, string description, Vector3D position, Color GPSColor, int disappearsInS = 0, long playerId = -1L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyGps gps1 = new MyGps();
            gps1.ShowOnHud = true;
            gps1.Coords = position;
            gps1.Name = name;
            gps1.Description = description;
            gps1.AlwaysVisible = true;
            gps1.IsObjective = true;
            MyGps gps = gps1;
            if (disappearsInS > 0)
            {
                gps.DiscardAt = new TimeSpan?(TimeSpan.FromSeconds(MySession.Static.ElapsedPlayTime.TotalSeconds + disappearsInS));
            }
            else
            {
                gps.DiscardAt = null;
            }
            if (GPSColor != Color.Transparent)
            {
                gps.GPSColor = GPSColor;
            }
            MySession.Static.Gpss.SendAddGps(playerId, ref gps, 0L, true);
        }

        [VisualScriptingMiscData("GPS and Highlights", "Adds GPS for all players.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSObjectiveForAll(string name, string description, Vector3D position, Color GPSColor, int disappearsInS = 0)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    AddGPSObjective(name, description, position, GPSColor, disappearsInS, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Creates GPS and attach it to entity for all players", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSObjectiveToEntity(string entityName, string GPSName, string GPSDescription, Color GPSColor)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    AddGPSObjectiveToEntity(entityName, GPSName, GPSDescription, GPSColor, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Creates GPS and attach it to entity for local player only.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSObjectiveToEntity(string entityName, string GPSName, string GPSDescription, Color GPSColor, long playerId = -1L)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                if (playerId == -1L)
                {
                    playerId = GetLocalPlayerId();
                }
                MyTuple<string, string> tuple1 = new MyTuple<string, string>(entityName, GPSName);
                MyGps gps1 = new MyGps();
                gps1.ShowOnHud = true;
                gps1.Name = GPSName;
                gps1.Description = GPSDescription;
                gps1.AlwaysVisible = true;
                gps1.IsObjective = true;
                MyGps gps = gps1;
                if (GPSColor != Color.Transparent)
                {
                    gps.GPSColor = GPSColor;
                }
                gps.DiscardAt = null;
                MySession.Static.Gpss.SendAddGps(playerId, ref gps, entity.EntityId, true);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Creates GPS and attach it to entity for local player only.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSToEntity(string entityName, string GPSName, string GPSDescription, Color GPSColor, long playerId = -1L)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                if (playerId == -1L)
                {
                    playerId = GetLocalPlayerId();
                }
                MyTuple<string, string> tuple1 = new MyTuple<string, string>(entityName, GPSName);
                MyGps gps1 = new MyGps();
                gps1.ShowOnHud = true;
                gps1.Name = GPSName;
                gps1.Description = GPSDescription;
                gps1.AlwaysVisible = true;
                MyGps gps = gps1;
                if (GPSColor != Color.Transparent)
                {
                    gps.GPSColor = GPSColor;
                }
                gps.DiscardAt = null;
                MySession.Static.Gpss.SendAddGps(playerId, ref gps, entity.EntityId, true);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Creates GPS and attach it to entity for all players", -10510688), VisualScriptingMember(true, false)]
        public static void AddGPSToEntityForAll(string entityName, string GPSName, string GPSDescription, Color GPSColor)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    AddGPSToEntity(entityName, GPSName, GPSDescription, GPSColor, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("AI", "Adds grid with specific name into drone's targets.", -10510688), VisualScriptingMember(true, false)]
        public static void AddGridToTargetList(string gridName, string targetGridname)
        {
            MyCubeGrid grid;
            MyCubeGrid grid2;
            if (TryGetGrid(gridName, out grid) && TryGetGrid(targetGridname, out grid2))
            {
                grid.TargetingAddId(grid2.EntityId);
            }
        }

        [VisualScriptingMiscData("Notifications", "Adds a new notification for the specific player and returns if of the notification. Returns -1 if no player corresponds to 'playerId'. For 'playerId' equal to 0 use local player.", -10510688), VisualScriptingMember(true, false)]
        public static int AddNotification(string message, string font = "White", long playerId = 0L)
        {
            MyPlayer.PlayerId id2;
            MyStringId orCompute = MyStringId.GetOrCompute(message);
            using (Dictionary<int, MyHudNotification>.Enumerator enumerator = m_addedNotificationsById.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    KeyValuePair<int, MyHudNotification> current = enumerator.Current;
                    if (current.Value.Text == orCompute)
                    {
                        return current.Key;
                    }
                }
            }
            m_notificationIdCounter++;
            int notificationIdCounter = m_notificationIdCounter;
            if (playerId == 0)
            {
                playerId = GetLocalPlayerId();
            }
            if (!MySession.Static.Players.TryGetPlayerId(playerId, out id2))
            {
                return -1;
            }
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<MyStringId, string, int>(s => new Action<MyStringId, string, int>(Sandbox.Game.MyVisualScriptLogicProvider.AddNotificationSync), orCompute, font, notificationIdCounter, new EndpointId(id2.SteamId), position);
            return notificationIdCounter;
        }

        [Event(null, 0x126d), Reliable, Client]
        private static void AddNotificationSync(MyStringId message, string font, int notificationId)
        {
            MyHudNotification notification = new MyHudNotification(message, 0, font, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Normal);
            MyHud.Notifications.Add(notification);
            m_addedNotificationsById.Add(notificationId, notification);
        }

        [VisualScriptingMiscData("Questlog", "Sets detail of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static int AddQuestlogDetail(string questDetailRow = "", bool completePrevious = true, bool useTyping = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, bool, bool, long>(s => new Action<string, bool, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.AddQuestlogDetailSync), questDetailRow, completePrevious, useTyping, num, targetEndpoint, position);
            return (MyHud.Questlog.GetQuestGetails().Length - 1);
        }

        [Event(null, 0x1532), Reliable, Server, Broadcast]
        private static void AddQuestlogDetailSync(string questDetailRow = "", bool completePrevious = true, bool useTyping = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                PlayHudSound(MyGuiSounds.HudQuestlogDetail, playerId);
                MyHud.Questlog.AddDetail(questDetailRow, useTyping, false);
                int num = MyHud.Questlog.GetQuestGetails().Length - 1;
                if (completePrevious)
                {
                    PlayHudSound(MyGuiSounds.HudObjectiveComplete, playerId);
                    MyHud.Questlog.SetCompleted(num - 1, true);
                }
            }
        }

        [VisualScriptingMiscData("Questlog", "Sets objective of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static int AddQuestlogObjective(string questDetailRow = "", bool completePrevious = true, bool useTyping = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, bool, bool, long>(s => new Action<string, bool, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.AddQuestlogObjectiveSync), questDetailRow, completePrevious, useTyping, num, targetEndpoint, position);
            return (MyHud.Questlog.GetQuestGetails().Length - 1);
        }

        [Event(null, 0x1550), Reliable, Server, Broadcast]
        private static void AddQuestlogObjectiveSync(string questDetailRow = "", bool completePrevious = true, bool useTyping = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                PlayHudSound(MyGuiSounds.HudQuestlogDetail, playerId);
                MyHud.Questlog.AddDetail(questDetailRow, useTyping, true);
                int num = MyHud.Questlog.GetQuestGetails().Length - 1;
                if (completePrevious)
                {
                    PlayHudSound(MyGuiSounds.HudObjectiveComplete, playerId);
                    MyHud.Questlog.SetCompleted(num - 1, true);
                }
            }
        }

        [VisualScriptingMiscData("Entity", "Adds item defined by id in specific quantity into inventory of entity.", -10510688), VisualScriptingMember(true, false)]
        public static void AddToInventory(string entityname, MyDefinitionId itemId, int amount = 1)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityname);
            if (entityByName != null)
            {
                MyInventoryBase inventoryBase = entityByName.GetInventoryBase();
                if (inventoryBase != null)
                {
                    MyFixedPoint point = new MyFixedPoint();
                    inventoryBase.AddItems(amount, (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) itemId));
                }
            }
        }

        [VisualScriptingMiscData("Player", "Adds the specified item to the player's inventory.", -10510688), VisualScriptingMember(true, false)]
        public static void AddToPlayersInventory(long playerId = 0L, MyDefinitionId itemId = new MyDefinitionId(), int amount = 1)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                MyInventory inventory = identityFromPlayerId.Character.GetInventory(0);
                if (inventory != null)
                {
                    MyFixedPoint point = new MyFixedPoint();
                    inventory.AddItems(amount, (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) itemId));
                }
            }
        }

        [VisualScriptingMiscData("Questlog", "Returns true if all essential hints have been completed.", -10510688), VisualScriptingMember(true, false)]
        public static bool AreEssentialGoodbotHintsDone()
        {
            HashSet<string> essentialObjectiveIds = MySessionComponentIngameHelp.EssentialObjectiveIds;
            int num = 0;
            foreach (string str in MySandboxGame.Config.TutorialsFinished)
            {
                if (essentialObjectiveIds.Contains(str))
                {
                    num++;
                }
            }
            return (num == essentialObjectiveIds.Count);
        }

        [VisualScriptingMiscData("Factions", "Returns true if specified two factions are enemies.", -10510688), VisualScriptingMember(false, false)]
        public static bool AreFactionsEnemies(string firstFactionTag, string secondFactionTag)
        {
            MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(firstFactionTag, null);
            if (faction == null)
            {
                return false;
            }
            MyFaction faction2 = MySession.Static.Factions.TryGetFactionByTag(firstFactionTag, null);
            return ((faction2 != null) ? MySession.Static.Factions.AreFactionsEnemies(faction.FactionId, faction2.FactionId) : false);
        }

        [VisualScriptingMiscData("AI", "Activates autopilot of specific drone and set all required parameters. Waypoints will not be cleared.", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotActivate(string entityName, FlightMode mode = 2, float speedLimit = 120f, bool collisionAvoidance = true, bool precisionMode = false)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.SetCollisionAvoidance(collisionAvoidance);
                control.SetAutoPilotSpeedLimit(speedLimit);
                control.ChangeFlightMode(mode);
                control.SetDockingMode(precisionMode);
                control.SetAutoPilotEnabled(true);
            }
        }

        [VisualScriptingMiscData("AI", "Adds new waypoint for specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotAddWaypoint(string entityName, Vector3D position, string waypointName = "Waypoint")
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.AddWaypoint(position, waypointName);
            }
        }

        [VisualScriptingMiscData("AI", "Clears all waypoints of specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotClearWaypoints(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.ClearWaypoints();
            }
        }

        [VisualScriptingMiscData("AI", "Enables/disables autopilot of specific drone", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotEnabled(string entityName, bool enabled = true)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.SetAutoPilotEnabled(enabled);
            }
        }

        [VisualScriptingMiscData("AI", "Gets position of curret waypoint of specific drone. If current waypoint exists, returns it position and 'waypointName' will be name of the waypoint. If waypoint does not exists, return current position and 'waypointName' will be empty string.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D AutopilotGetCurrentWaypoint(string entityName, out string waypointName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            waypointName = "";
            if (control == null)
            {
                return Vector3D.Zero;
            }
            if (control.CurrentWaypoint == null)
            {
                return control.PositionComp.GetPosition();
            }
            waypointName = control.CurrentWaypoint.Name;
            return control.CurrentWaypoint.Coords;
        }

        [VisualScriptingMiscData("AI", "Enables drone's autopilot, sets it to one-way go to waypoint and adds that one waypoint. All previous waypoints will be cleared.", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotGoToPosition(string entityName, Vector3D position, string waypointName = "Waypoint", float speedLimit = 120f, bool collisionAvoidance = true, bool precisionMode = false)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.SetCollisionAvoidance(collisionAvoidance);
                control.SetAutoPilotSpeedLimit(speedLimit);
                control.ChangeFlightMode(FlightMode.OneWay);
                control.SetDockingMode(precisionMode);
                control.ClearWaypoints();
                control.AddWaypoint(position, waypointName);
                control.SetAutoPilotEnabled(true);
            }
        }

        [VisualScriptingMiscData("AI", "Adds list of waypoints to specific drone. All waypoints will be called 'waypointName' followed by space and number. (given by order, starts with 1)", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotSetWaypoints(string entityName, List<Vector3D> positions, string waypointName = "Waypoint")
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (control != null)
            {
                control.ClearWaypoints();
                if (positions != null)
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        control.AddWaypoint(positions[i], waypointName + " " + (i + 1).ToString());
                    }
                }
            }
        }

        [VisualScriptingMiscData("AI", "Orders drone to immediately skip current waypoint and go directly to the next one.", -10510688), VisualScriptingMember(true, false)]
        public static void AutopilotSkipCurrentWaypoint(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.CurrentWaypoint != null))
            {
                control.AdvanceWaypoint();
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets specific doors to open/close state. (Doors, SlidingDoors, AirtightDoors)", -10510688), VisualScriptingMember(true, false)]
        public static void ChangeDoorState(string doorBlockName, bool open = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(doorBlockName, out entity))
            {
                if (entity is MyAdvancedDoor)
                {
                    (entity as MyAdvancedDoor).Open = open;
                }
                if (entity is MyAirtightDoorGeneric)
                {
                    (entity as MyAirtightDoorGeneric).ChangeOpenClose(open);
                }
                if (entity is MyDoor)
                {
                    (entity as MyDoor).Open = open;
                }
            }
        }

        [VisualScriptingMiscData("Entity", "Changes ownership of a specific block (if entity is block) or ownership of all functional blocks (if entity is grid) to specific player and modify its/theirs share settings.", -10510688), VisualScriptingMember(true, false)]
        public static bool ChangeOwner(string entityName, long playerId = 0L, bool factionShare = false, bool allShare = false)
        {
            VRage.Game.Entity.MyEntity entity;
            MyOwnershipShareModeEnum none = MyOwnershipShareModeEnum.None;
            if (factionShare)
            {
                none = MyOwnershipShareModeEnum.Faction;
            }
            if (allShare)
            {
                none = MyOwnershipShareModeEnum.All;
            }
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    block.ChangeBlockOwnerRequest(0L, none);
                    if (playerId > 0L)
                    {
                        block.ChangeBlockOwnerRequest(playerId, none);
                    }
                    return true;
                }
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid != null)
                {
                    foreach (MyCubeBlock block2 in grid.GetFatBlocks())
                    {
                        if (block2 is MyLightingBlock)
                        {
                            continue;
                        }
                        if (((block2 is MyFunctionalBlock) || (block2 is MyShipController)) || (block2 is MyTerminalBlock))
                        {
                            grid.ChangeOwnerRequest(grid, block2, 0L, none);
                            if (playerId > 0L)
                            {
                                grid.ChangeOwnerRequest(grid, block2, playerId, none);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        [VisualScriptingMiscData("Toolbar", "Clears all toolbar slots for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void ClearAllToolbarSlots(long playerId = -1L)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearAllToolbarSlotsSync), playerId, targetEndpoint, position);
        }

        [Event(null, 0x174e), Reliable, Server, Broadcast]
        private static void ClearAllToolbarSlotsSync(long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                int currentPage = MyToolbarComponent.CurrentToolbar.CurrentPage;
                int page = 0;
                while (page < MyToolbarComponent.CurrentToolbar.PageCount)
                {
                    MyToolbarComponent.CurrentToolbar.SwitchToPage(page);
                    int slot = 0;
                    while (true)
                    {
                        if (slot >= MyToolbarComponent.CurrentToolbar.SlotCount)
                        {
                            page++;
                            break;
                        }
                        MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, null);
                        slot++;
                    }
                }
                MyToolbarComponent.CurrentToolbar.SwitchToPage(currentPage);
            }
        }

        [VisualScriptingMiscData("Notifications", "Clears all added notifications.", -10510688), VisualScriptingMember(true, false)]
        public static void ClearNotifications(long playerId = -1L)
        {
            if (playerId == 0)
            {
                ClearNotificationSync(-1L);
            }
            else
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearNotificationSync), playerId, targetEndpoint, position);
            }
        }

        [Event(null, 0x12a1), Reliable, Server, Broadcast]
        private static void ClearNotificationSync(long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                MyHud.Notifications.Clear();
                m_notificationIdCounter = 0;
                m_addedNotificationsById.Clear();
            }
        }

        [VisualScriptingMiscData("Toolbar", "Clears the toolbar slot for the player.", -10510688), VisualScriptingMember(true, false)]
        public static void ClearToolbarSlot(int slot, long playerId = -1L)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, long>(s => new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearToolbarSlotSync), slot, playerId, targetEndpoint, position);
        }

        [Event(null, 0x173d), Reliable, Server, Broadcast]
        private static void ClearToolbarSlotSync(int slot, long playerId = -1L)
        {
            if (((slot >= 0) && (slot < MyToolbarComponent.CurrentToolbar.SlotCount)) && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, null);
            }
        }

        [Event(null, 0x14e3), Reliable, Client]
        private static void CloseRespawnScreen()
        {
            Sync.Players.RespawnComponent.CloseRespawnScreenNow();
        }

        [VisualScriptingMiscData("Blocks Specific", "Returns identity Id of player occupying cockpit or 0, if no one is in. ", -10510688), VisualScriptingMember(false, false)]
        public static long CockpitGetPilotId(string cockpitName, out bool occupied)
        {
            VRage.Game.Entity.MyEntity entity;
            occupied = false;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(cockpitName, out entity))
            {
                MyCockpit cockpit = entity as MyCockpit;
                if ((cockpit != null) && (cockpit.Pilot != null))
                {
                    occupied = cockpit.Pilot != null;
                    return cockpit.Pilot.GetPlayerIdentityId();
                }
            }
            return 0L;
        }

        [VisualScriptingMiscData("Blocks Specific", "Forces player into specific Cockpit.", -10510688), VisualScriptingMember(true, false)]
        public static void CockpitInsertPilot(string cockpitName, bool keepOriginalPlayerPosition = true, long playerId = 0L)
        {
            VRage.Game.Entity.MyEntity entity;
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            if ((characterFromPlayerId != null) && Sandbox.Game.Entities.MyEntities.TryGetEntityByName(cockpitName, out entity))
            {
                MyCockpit cockpit = entity as MyCockpit;
                if (cockpit != null)
                {
                    cockpit.RemovePilot();
                    if (characterFromPlayerId.Parent is MyCockpit)
                    {
                        (characterFromPlayerId.Parent as MyCockpit).RemovePilot();
                    }
                    cockpit.AttachPilot(characterFromPlayerId, keepOriginalPlayerPosition, false, false);
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Removes pilot from specific Cockpit.", -10510688), VisualScriptingMember(true, false)]
        public static void CockpitRemovePilot(string cockpitName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(cockpitName, out entity))
            {
                MyCockpit cockpit = entity as MyCockpit;
                if (cockpit != null)
                {
                    cockpit.RemovePilot();
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Colors all blocks of specific grid.", -10510688), VisualScriptingMember(true, false)]
        public static void ColorAllGridBlocks(string gridName, Color color, bool playSound = true)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    grid.ColorGrid(color.ColorToHSVDX11(), playSound, false);
                }
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets color of specific block.", -10510688), VisualScriptingMember(true, false)]
        public static void ColorBlock(string blockName, Color color)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(blockName);
            if (entityByName != null)
            {
                MyCubeBlock block = entityByName as MyCubeBlock;
                if (block != null)
                {
                    Vector3 newHSV = color.ColorToHSVDX11();
                    block.CubeGrid.ChangeColor(block.SlimBlock, newHSV);
                }
            }
        }

        [VisualScriptingMiscData("Triggers", "Creates area trigger at the position of specified entity.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateAreaTriggerOnEntity(string entityName, float radius, string name)
        {
            VRage.Game.Entity.MyEntity entity;
            MyAreaTriggerComponent component = new MyAreaTriggerComponent(name) {
                Radius = radius
            };
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                component.Center = entity.PositionComp.GetPosition();
                component.DefaultTranslation = Vector3D.Zero;
                if (!entity.Components.Contains(typeof(MyTriggerAggregate)))
                {
                    entity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
                }
                entity.Components.Get<MyTriggerAggregate>().AddComponent(component);
            }
        }

        [VisualScriptingMiscData("Triggers", "Creates area trigger at the position.", -10510688), VisualScriptingMember(true, false)]
        public static long CreateAreaTriggerOnPosition(Vector3D position, float radius, string name)
        {
            MyAreaTriggerComponent component = new MyAreaTriggerComponent(name);
            VRage.Game.Entity.MyEntity entity = new VRage.Game.Entity.MyEntity();
            component.Center = position;
            component.Radius = radius;
            entity.PositionComp.SetPosition(position, null, false, true);
            entity.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
            component.DefaultTranslation = Vector3D.Zero;
            Sandbox.Game.Entities.MyEntities.Add(entity, true);
            if (!entity.Components.Contains(typeof(MyTriggerAggregate)))
            {
                entity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
            }
            entity.Components.Get<MyTriggerAggregate>().AddComponent(component);
            return entity.EntityId;
        }

        [VisualScriptingMiscData("Triggers", "Creates area trigger at the relative position to the specified entity.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateAreaTriggerRelativeToEntity(Vector3D position, string entityName, float radius, string name)
        {
            VRage.Game.Entity.MyEntity entity;
            MyAreaTriggerComponent component = new MyAreaTriggerComponent(name) {
                Radius = radius
            };
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                component.Center = position;
                component.DefaultTranslation = position - entity.PositionComp.GetPosition();
                if (!entity.Components.Contains(typeof(MyTriggerAggregate)))
                {
                    entity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
                }
                entity.Components.Get<MyTriggerAggregate>().AddComponent(component);
            }
        }

        [VisualScriptingMiscData("Effects", "Creates explosion at specific point with specified radius, causing damage to everything in range.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateExplosion(Vector3D position, float radius, int damage = 0x1388)
        {
            MyExplosionTypeEnum enum2 = MyExplosionTypeEnum.WARHEAD_EXPLOSION_50;
            if (radius < 2f)
            {
                enum2 = MyExplosionTypeEnum.WARHEAD_EXPLOSION_02;
            }
            else if (radius < 15f)
            {
                enum2 = MyExplosionTypeEnum.WARHEAD_EXPLOSION_15;
            }
            else if (radius < 30f)
            {
                enum2 = MyExplosionTypeEnum.WARHEAD_EXPLOSION_30;
            }
            MyExplosionInfo explosionInfo = new MyExplosionInfo {
                PlayerDamage = 0f,
                Damage = damage,
                ExplosionType = enum2,
                ExplosionSphere = new BoundingSphereD(position, (double) radius),
                LifespanMiliseconds = 700,
                ParticleScale = 1f,
                Direction = new Vector3?(Vector3.Down),
                VoxelExplosionCenter = position,
                ExplosionFlags = MyExplosionFlags.APPLY_DEFORMATION | MyExplosionFlags.CREATE_SHRAPNELS | MyExplosionFlags.CREATE_PARTICLE_EFFECT | MyExplosionFlags.CREATE_DECALS | MyExplosionFlags.APPLY_FORCE_AND_DAMAGE | MyExplosionFlags.AFFECT_VOXELS | MyExplosionFlags.CREATE_DEBRIS,
                VoxelCutoutScale = 1f,
                PlaySound = true,
                ApplyForceAndDamage = true,
                ObjectsRemoveDelayInMiliseconds = 40
            };
            MyExplosions.AddExplosion(ref explosionInfo, true);
        }

        [VisualScriptingMiscData("Factions", "Creates new faction.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateFaction(long founderId, string factionTag, string factionName = "", string factionDescription = "", string factionPrivateText = "")
        {
            MySession.Static.Factions.CreateFaction(founderId, factionTag, factionName, factionDescription, factionPrivateText);
        }

        [VisualScriptingMiscData("Grid", "Creates local blueprint for player.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateLocalBlueprint(string gridName, string blueprintName, string blueprintDisplayName = null)
        {
            string text1 = Path.Combine(MyFileSystem.UserDataPath, "Blueprints", "local");
            string path = Path.Combine(text1, blueprintName, "bp.sbc");
            if (MyFileSystem.DirectoryExists(text1))
            {
                if (blueprintDisplayName == null)
                {
                    blueprintDisplayName = blueprintName;
                }
                VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
                if (entityByName != null)
                {
                    MyCubeGrid grid = entityByName as MyCubeGrid;
                    if (grid != null)
                    {
                        MyClipboardComponent.Static.Clipboard.CopyGrid(grid);
                        MyObjectBuilder_ShipBlueprintDefinition definition = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ShipBlueprintDefinition>();
                        definition.Id = (SerializableDefinitionId) new MyDefinitionId(new MyObjectBuilderType(typeof(MyObjectBuilder_ShipBlueprintDefinition)), MyUtils.StripInvalidChars(blueprintName));
                        definition.CubeGrids = MyClipboardComponent.Static.Clipboard.CopiedGrids.ToArray();
                        definition.RespawnShip = false;
                        definition.DisplayName = blueprintDisplayName;
                        definition.CubeGrids[0].DisplayName = blueprintDisplayName;
                        MyObjectBuilder_Definitions objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Definitions>();
                        objectBuilder.ShipBlueprints = new MyObjectBuilder_ShipBlueprintDefinition[] { definition };
                        MyObjectBuilderSerializer.SerializeXML(path, false, objectBuilder, null);
                        MyClipboardComponent.Static.Clipboard.Deactivate(false);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Effects", "Creates specific particle effect at entity.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateParticleEffectAtEntity(string effectName, string entityName)
        {
            MyParticleEffect effect;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if ((entityByName != null) && MyParticlesManager.TryCreateParticleEffect(effectName, entityByName.WorldMatrix, out effect))
            {
                effect.Loop = false;
            }
        }

        [VisualScriptingMiscData("Effects", "Creates specific particle effect at position.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateParticleEffectAtPosition(string effectName, Vector3D position)
        {
            MyParticleEffect effect;
            if (MyParticlesManager.TryCreateParticleEffect(effectName, MatrixD.CreateWorld(position), out effect))
            {
                effect.Loop = false;
            }
        }

        [VisualScriptingMiscData("Audio", "Creates new 3D sound emitter at entity.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateSoundEmitterAtEntity(string newEmitterId, string entityName)
        {
            if ((MyAudio.Static != null) && (newEmitterId.Length > 0))
            {
                VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
                if (entityByName != null)
                {
                    MyAudioComponent.CreateNewLibraryEmitter(newEmitterId, entityByName);
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Creates new 3D sound emitter at specific location.", -10510688), VisualScriptingMember(true, false)]
        public static void CreateSoundEmitterAtPosition(string newEmitterId, Vector3 position)
        {
            if ((MyAudio.Static != null) && (newEmitterId.Length > 0))
            {
                MyEntity3DSoundEmitter emitter = MyAudioComponent.CreateNewLibraryEmitter(newEmitterId, null);
                if (emitter != null)
                {
                    emitter.SetPosition(new Vector3D?(position));
                }
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Applies damage to specific block from specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void DamageBlock(string entityName, float damage = 0f, long damageOwner = 0L)
        {
            MyCubeBlock entityByName = GetEntityByName(entityName) as MyCubeBlock;
            if (entityByName != null)
            {
                MyHitInfo? hitInfo = null;
                entityByName.SlimBlock.DoDamage(damage, MyDamageType.Destruction, true, hitInfo, damageOwner);
            }
        }

        [VisualScriptingMiscData("Definitions", "Returns true if the type id and subtype id match.", -10510688), VisualScriptingMember(false, false)]
        public static bool DefinitionIdMatch(string typeId, string subtypeId, string matchTypeId, string matchSubtypeId) => 
            (string.Equals(typeId, matchTypeId) && string.Equals(subtypeId, matchSubtypeId));

        [VisualScriptingMiscData("Blocks Generic", "Disables functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void DisableBlock(string blockName)
        {
            SetBlockState(blockName, false);
        }

        private static void DisplayCongratulationScreen(int congratulationMessageId)
        {
            MyScreenManager.AddScreen(new MyGuiScreenCongratulation(congratulationMessageId));
        }

        [VisualScriptingMiscData("Notifications", "Display congratulation screen to playet/s. Use MessageId to select which message should be shown. If player id is 1-, show to all. If it is 0, show to local player. Else it will be used as player identity id.", -10510688), VisualScriptingMember(true, false)]
        public static void DisplayCongratulationScreen(int congratulationMessageId, long playerId)
        {
            if (playerId == 0)
            {
                DisplayCongratulationScreenInternal(congratulationMessageId);
            }
            else
            {
                Vector3D? nullable;
                if (playerId == -1L)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.DisplayCongratulationScreenInternalAll), congratulationMessageId, targetEndpoint, nullable);
                }
                else
                {
                    MyPlayer.PlayerId id;
                    if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                    {
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.DisplayCongratulationScreenInternal), congratulationMessageId, new EndpointId(id.SteamId), nullable);
                    }
                }
            }
        }

        [Event(null, 0x12c3), Reliable, ServerInvoked]
        private static void DisplayCongratulationScreenInternal(int congratulationMessageId)
        {
            DisplayCongratulationScreen(congratulationMessageId);
        }

        [Event(null, 0x12c9), Reliable, ServerInvoked, Broadcast]
        private static void DisplayCongratulationScreenInternalAll(int congratulationMessageId)
        {
            DisplayCongratulationScreen(congratulationMessageId);
        }

        [VisualScriptingMiscData("AI", "Gets AI behavior of specific drone. Returns empty string if drone lacks remote or AI behavior.", -10510688), VisualScriptingMember(false, false)]
        public static string DroneGetCurrentAIBehavior(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control == null) || (control.AutomaticBehaviour == null))
            {
                return "";
            }
            return control.AutomaticBehaviour.ToString();
        }

        private static MyRemoteControl DroneGetRemote(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return null;
            }
            MyRemoteControl fatBlock = entityByName as MyRemoteControl;
            if ((entityByName is MyCubeBlock) && (fatBlock == null))
            {
                entityByName = ((MyCubeBlock) entityByName).CubeGrid;
            }
            if (entityByName is MyCubeGrid)
            {
                foreach (MySlimBlock block in ((MyCubeGrid) entityByName).GetBlocks())
                {
                    if (block.FatBlock is MyRemoteControl)
                    {
                        fatBlock = block.FatBlock as MyRemoteControl;
                        break;
                    }
                }
            }
            return fatBlock;
        }

        [VisualScriptingMiscData("AI", "Gets speed limit of specific drone.", -10510688), VisualScriptingMember(false, false)]
        public static float DroneGetSpeedLimit(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control == null) || (control.AutomaticBehaviour == null))
            {
                return 0f;
            }
            return control.AutomaticBehaviour.SpeedLimit;
        }

        [VisualScriptingMiscData("AI", "Gets count of targets for specific drone. Returns -1 if drone lacks remote or AI behavior.", -10510688), VisualScriptingMember(false, false)]
        public static int DroneGetTargetsCount(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control == null) || (control.AutomaticBehaviour == null))
            {
                return -1;
            }
            return control.AutomaticBehaviour.TargetList.Count;
        }

        [VisualScriptingMiscData("AI", "Gets number of waypoints for specific drone. Returns -1 if drone has no remote or AI behavior.", -10510688), VisualScriptingMember(false, false)]
        public static int DroneGetWaypointCount(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control == null) || (control.AutomaticBehaviour == null))
            {
                return -1;
            }
            return (control.AutomaticBehaviour.WaypointList.Count + (control.AutomaticBehaviour.WaypointActive ? 1 : 0));
        }

        [VisualScriptingMiscData("AI", "Returns true if specific drone has both remote and AI behavior, false otherwise.", -10510688), VisualScriptingMember(false, false)]
        public static bool DroneHasAI(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            return ((control != null) && (control.AutomaticBehaviour != null));
        }

        [VisualScriptingMiscData("AI", "Returns true if drone is in ambush mode, false otherwise.", -10510688), VisualScriptingMember(false, false)]
        public static bool DroneIsInAmbushMode(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            return ((control != null) && ((control.AutomaticBehaviour != null) && control.AutomaticBehaviour.Ambushing));
        }

        [VisualScriptingMiscData("AI", "Returns true if specific drone has both working remoteand have operational AI behavior, false otherwise.", -10510688), VisualScriptingMember(false, false)]
        public static bool DroneIsOperational(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            return ((control != null) && ((control.AutomaticBehaviour != null) && (control.IsWorking && control.AutomaticBehaviour.Operational)));
        }

        private static List<DroneTarget> DroneProcessTargets(List<VRage.Game.Entity.MyEntity> targets)
        {
            List<DroneTarget> list = new List<DroneTarget>();
            switch (targets)
            {
                case (null):
                    break;

                default:
                    foreach (VRage.Game.Entity.MyEntity entity in targets)
                    {
                        if (entity is MyCubeGrid)
                        {
                            foreach (MySlimBlock block in ((MyCubeGrid) entity).GetBlocks())
                            {
                                if (block.FatBlock is MyShipController)
                                {
                                    list.Add(new DroneTarget(block.FatBlock, 8));
                                }
                                if (block.FatBlock is MyReactor)
                                {
                                    list.Add(new DroneTarget(block.FatBlock, 6));
                                }
                                if (block.FatBlock is MyUserControllableGun)
                                {
                                    list.Add(new DroneTarget(block.FatBlock, 10));
                                }
                            }
                            continue;
                        }
                        list.Add(new DroneTarget(entity));
                    }
                    break;
            }
            return list;
        }

        [VisualScriptingMiscData("AI", "Activates/deactivates ambush mode for specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetAmbushMode(string entityName, bool ambushModeOn = true)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.Ambushing = ambushModeOn;
            }
        }

        [VisualScriptingMiscData("AI", "Enables/disables collision avoidance for specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetCollisionAvoidance(string entityName, bool collisionAvoidance = true)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.SetCollisionAvoidance(collisionAvoidance);
                control.AutomaticBehaviour.CollisionAvoidance = collisionAvoidance;
            }
        }

        [VisualScriptingMiscData("AI", "Sets player targeting priority of specific drone. (All player controlled entities will be considered a target if priority is greater than 0)", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetPlayerPriority(string entityName, int priority)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.PlayerPriority = priority;
            }
        }

        [VisualScriptingMiscData("AI", "Sets target prioritization for specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetPrioritizationStyle(string entityName, TargetPrioritization style)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.PrioritizationStyle = style;
            }
        }

        [VisualScriptingMiscData("AI", "Sets origin point of specific drone. (Once non-kamikaze drone has no weapons, it will retreat to that point.)", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetRetreatPosition(string entityName, Vector3D position)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.OriginPoint = position;
            }
        }

        [VisualScriptingMiscData("AI", "Enables/disables if drone should rotate toward it's target.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetRotateToTarget(string entityName, bool rotateToTarget = true)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.RotateToTarget = rotateToTarget;
            }
        }

        [VisualScriptingMiscData("AI", "Sets maximum speed limit of specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetSpeedLimit(string entityName, float speedLimit = 100f)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.SpeedLimit = speedLimit;
                control.SetAutoPilotSpeedLimit(speedLimit);
            }
        }

        [VisualScriptingMiscData("AI", "Sets current target of drone to specific entity.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneSetTarget(string entityName, VRage.Game.Entity.MyEntity target)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if (((control != null) && (control.AutomaticBehaviour != null)) && (target != null))
            {
                control.AutomaticBehaviour.CurrentTarget = target;
            }
        }

        [VisualScriptingMiscData("AI", "Adds specific entity into targets of specific drone. Priority specifies order in which targets will be dealt with (higher is more important).", -10510688), VisualScriptingMember(true, false)]
        public static void DroneTargetAdd(string entityName, VRage.Game.Entity.MyEntity target, int priority = 1)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                if (target is MyCubeGrid)
                {
                    List<VRage.Game.Entity.MyEntity> targets = new List<VRage.Game.Entity.MyEntity>();
                    targets.Add(target);
                    foreach (DroneTarget target2 in DroneProcessTargets(targets))
                    {
                        control.AutomaticBehaviour.TargetAdd(target2);
                    }
                }
                else
                {
                    control.AutomaticBehaviour.TargetAdd(new DroneTarget(target, priority));
                }
            }
        }

        [VisualScriptingMiscData("AI", "Clears all targets of specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneTargetClear(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.TargetClear();
            }
        }

        [VisualScriptingMiscData("AI", "Sets current target of specific drone to none.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneTargetLoseCurrent(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.TargetLoseCurrent();
            }
        }

        [VisualScriptingMiscData("AI", "Removes specific entity from drone's targets", -10510688), VisualScriptingMember(true, false)]
        public static void DroneTargetRemove(string entityName, VRage.Game.Entity.MyEntity target)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.TargetRemove(target);
            }
        }

        [VisualScriptingMiscData("AI", "Adds specific waypoint to specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneWaypointAdd(string entityName, VRage.Game.Entity.MyEntity waypoint)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.WaypointAdd(waypoint);
            }
        }

        [VisualScriptingMiscData("AI", "Deletes all waypoints of specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneWaypointClear(string entityName)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.WaypointClear();
            }
        }

        [VisualScriptingMiscData("AI", "Enables/disables waypoint cycling for specific drone.", -10510688), VisualScriptingMember(true, false)]
        public static void DroneWaypointSetCycling(string entityName, bool cycleWaypoints = true)
        {
            MyRemoteControl control = DroneGetRemote(entityName);
            if ((control != null) && (control.AutomaticBehaviour != null))
            {
                control.AutomaticBehaviour.CycleWaypoints = cycleWaypoints;
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Enables functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void EnableBlock(string blockName)
        {
            SetBlockState(blockName, true);
        }

        [VisualScriptingMiscData("Questlog", "Enables highlight of the questlog for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void EnableHighlight(bool enable = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool, long>(s => new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.EnableHighlightSync), enable, num, targetEndpoint, position);
        }

        [Event(null, 0x15f6), Reliable, Server, Broadcast]
        private static void EnableHighlightSync(bool enable = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MyHud.Questlog.HighlightChanges = enable;
            }
        }

        [VisualScriptingMiscData("Gameplay", "Returns true if access to terminal screen is enabled.", -10510688), VisualScriptingMember(true, false)]
        public static void EnableTerminal(bool flag)
        {
            MyPerGameSettings.GUI.EnableTerminalScreen = flag;
        }

        [VisualScriptingMiscData("G-Screen", "Enables/disables toolbar config screen (G-screen).", -10510688), VisualScriptingMember(true, false)]
        public static void EnableToolbarConfig(bool flag)
        {
            MyPerGameSettings.GUI.EnableToolbarConfigScreen = flag;
        }

        [VisualScriptingMiscData("Cutscenes", "Ends current cutscene. If 'playerId' is -1, apply for all players, otherwise only for specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void EndCutscene(long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.EndCutsceneSync), num, targetEndpoint, position);
        }

        [Event(null, 0x7ee), Reliable, Server, Broadcast]
        private static void EndCutsceneSync(long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                MySession.Static.GetComponent<MySessionComponentCutscenes>().CutsceneEnd(true);
            }
        }

        [VisualScriptingMiscData("Entity", "Returns true if specific entity is present in the world.", -10510688), VisualScriptingMember(false, false)]
        public static bool EntityExists(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            return (!(entityByName is MyCubeGrid) ? (entityByName != null) : (((MyCubeGrid) entityByName).InScene && !((MyCubeGrid) entityByName).MarkedForClose));
        }

        [VisualScriptingMiscData("Entity", "Finds free place around the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static bool FindFreePlace(Vector3D position, out Vector3D newPosition, float radius, int maxTestCount = 20, int testsPerDistance = 5, float stepSize = 1f)
        {
            Vector3D? nullable = Sandbox.Game.Entities.MyEntities.FindFreePlace(position, radius, maxTestCount, testsPerDistance, stepSize);
            newPosition = (nullable != null) ? nullable.Value : Vector3D.Zero;
            return (nullable != null);
        }

        [VisualScriptingMiscData("Gameplay", "Finishes active mission (state Mission Complete) with fadeout (ms).", -10510688), VisualScriptingMember(true, false)]
        public static void FinishMission(string outcome = "Mission Complete", int fadeTimeMs = 0x1388)
        {
            SetMissionOutcome(outcome);
            SessionClose(fadeTimeMs);
        }

        [VisualScriptingMiscData("Environment", "Sets density, multiplier and color of fog.", -10510688), VisualScriptingMember(true, false)]
        public static void FogSetAll(float density, float multiplier, Vector3 color)
        {
            MySector.FogProperties.FogMultiplier = multiplier;
            MySector.FogProperties.FogDensity = density;
            MySector.FogProperties.FogColor = color;
        }

        [VisualScriptingMiscData("Environment", "Sets fog color ", -10510688), VisualScriptingMember(true, false)]
        public static void FogSetColor(Vector3 color)
        {
            MySector.FogProperties.FogColor = color;
        }

        [VisualScriptingMiscData("Environment", "Sets density of fog.", -10510688), VisualScriptingMember(true, false)]
        public static void FogSetDensity(float density)
        {
            MySector.FogProperties.FogDensity = density;
        }

        [VisualScriptingMiscData("Environment", "Sets multiplier of fog.", -10510688), VisualScriptingMember(true, false)]
        public static void FogSetMultiplier(float multiplier)
        {
            MySector.FogProperties.FogMultiplier = multiplier;
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns ids of attached modules. Output parameters will contain additional informations.", -10510688), VisualScriptingMember(false, false)]
        public static List<long> GetBlockAttachedUpgradeModules(string blockName, out int modulesCount, out int workingCount, out int slotsUsed, out int slotsTotal, out int incompatibleCount)
        {
            VRage.Game.Entity.MyEntity entity;
            List<long> list = new List<long>();
            modulesCount = 0;
            workingCount = 0;
            slotsUsed = 0;
            slotsTotal = 0;
            incompatibleCount = 0;
            Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity);
            if (entity != null)
            {
                MyCubeBlock block = entity as MyCubeBlock;
                if (block != null)
                {
                    slotsTotal = block.GetComponent().ConnectionPositions.Count;
                    if (block.CurrentAttachedUpgradeModules != null)
                    {
                        modulesCount = block.CurrentAttachedUpgradeModules.Count;
                        Dictionary<long, MyCubeBlock.AttachedUpgradeModule> currentAttachedUpgradeModules = block.CurrentAttachedUpgradeModules;
                        lock (currentAttachedUpgradeModules)
                        {
                            foreach (MyCubeBlock.AttachedUpgradeModule module in block.CurrentAttachedUpgradeModules.Values)
                            {
                                int num1;
                                list.Add(module.Block.EntityId);
                                slotsUsed += module.SlotCount;
                                incompatibleCount += module.Compatible ? 0 : 1;
                                if (!module.Compatible || !module.Block.IsWorking)
                                {
                                    num1 = 0;
                                }
                                else
                                {
                                    num1 = 1;
                                }
                                workingCount += num1;
                            }
                        }
                    }
                }
            }
            return list;
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns current integrity of block in interval <0;1>.", -10510688), VisualScriptingMember(false, false)]
        public static float GetBlockHealth(string entityName, bool buildIntegrity = true)
        {
            MyCubeBlock entityByName = GetEntityByName(entityName) as MyCubeBlock;
            return ((entityByName == null) ? 0f : (!buildIntegrity ? entityByName.SlimBlock.Integrity : entityByName.SlimBlock.BuildIntegrity));
        }

        private static MyCharacter GetCharacterFromPlayerId(long playerId = 0L)
        {
            if (playerId == 0)
            {
                return MySession.Static.LocalCharacter;
            }
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerId);
            return identity?.Character;
        }

        [VisualScriptingMiscData("Misc", "Creates a new color out of red, green and blue. All values must be in range <0;1>.", -10510688), VisualScriptingMember(false, false)]
        public static Color GetColor(float r = 0f, float g = 0f, float b = 0f)
        {
            float single1 = MathHelper.Clamp(r, 0f, 1f);
            r = single1;
            float single2 = MathHelper.Clamp(g, 0f, 1f);
            g = single2;
            float single3 = MathHelper.Clamp(b, 0f, 1f);
            b = single3;
            return new Color(r, g, b);
        }

        [VisualScriptingMiscData("Misc", "Returns path to where game content is located.", -10510688), VisualScriptingMember(false, false)]
        public static string GetContentPath() => 
            MyFileSystem.ContentPath;

        [VisualScriptingMiscData("GUI", @"Gets GUI element by name from the specific Gui element. You may search through hierarchy of controls by connecting element names with '\'. Such as 'GrandParent\Parent\Child' will return element of name 'Child' that is under element 'Parent' that is under element 'GrandParent' which is in screen. In case specific element was not found, returned element will be the closest parent that was found.", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiControlBase GetControlByName(this MyGuiControlParent control, string controlName)
        {
            MyGuiControlBase controlByName;
            if (string.IsNullOrEmpty(controlName))
            {
                goto TR_0000;
            }
            else if (control != null)
            {
                char[] separator = new char[] { '\\' };
                string[] strArray = controlName.Split(separator);
                controlByName = control.Controls.GetControlByName(strArray[0]);
                for (int i = 1; i < strArray.Length; i++)
                {
                    MyGuiControlParent parent = controlByName as MyGuiControlParent;
                    if (parent != null)
                    {
                        controlByName = parent.Controls.GetControlByName(strArray[i]);
                    }
                    else
                    {
                        MyGuiControlScrollablePanel panel = controlByName as MyGuiControlScrollablePanel;
                        if (panel == null)
                        {
                            if (controlByName != null)
                            {
                                controlByName = controlByName.Elements.GetControlByName(strArray[i]);
                            }
                            break;
                        }
                        controlByName = panel.Controls.GetControlByName(strArray[i]);
                    }
                }
            }
            else
            {
                goto TR_0000;
            }
            return controlByName;
        TR_0000:
            return null;
        }

        [VisualScriptingMiscData("GUI", @"Gets GUI element by name from the specific screen. You may search through hierarchy of controls by connecting element names with '\\'. Such as 'GrandParent\\Parent\\Child' will return element of name 'Child' that is under element 'Parent' that is under element 'GrandParent' which is in screen. In case specific element was not found, returned element will be the closest parent that was found.", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiControlBase GetControlByName(this MyGuiScreenBase screen, string controlName)
        {
            MyGuiControlBase controlByName;
            if (string.IsNullOrEmpty(controlName))
            {
                goto TR_0000;
            }
            else if (screen != null)
            {
                char[] separator = new char[] { '\\' };
                string[] strArray = controlName.Split(separator);
                controlByName = screen.Controls.GetControlByName(strArray[0]);
                for (int i = 1; i < strArray.Length; i++)
                {
                    MyGuiControlParent parent = controlByName as MyGuiControlParent;
                    if (parent != null)
                    {
                        controlByName = parent.Controls.GetControlByName(strArray[i]);
                    }
                    else
                    {
                        MyGuiControlScrollablePanel panel = controlByName as MyGuiControlScrollablePanel;
                        if (panel == null)
                        {
                            if (controlByName != null)
                            {
                                controlByName = controlByName.Elements.GetControlByName(strArray[i]);
                            }
                            break;
                        }
                        controlByName = panel.Controls.GetControlByName(strArray[i]);
                    }
                }
            }
            else
            {
                goto TR_0000;
            }
            return controlByName;
        TR_0000:
            return null;
        }

        [VisualScriptingMiscData("Grid", "Returns count of all blocks of type 'blockId' on specific grid.", -10510688), VisualScriptingMember(false, false)]
        public static int GetCountOfSpecificGridBlocks(string gridName, MyDefinitionId blockId)
        {
            int num = -2;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                num = -1;
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    num = 0;
                    foreach (MyCubeBlock block in grid.GetFatBlocks())
                    {
                        if (block == null)
                        {
                            continue;
                        }
                        if ((block.BlockDefinition != null) && (block.BlockDefinition.Id == blockId))
                        {
                            num++;
                        }
                    }
                }
            }
            return num;
        }

        [VisualScriptingMiscData("Gameplay", "Gets path of the session (game/mission) currently being played.", -10510688), VisualScriptingMember(false, false)]
        public static string GetCurrentSessionPath() => 
            MySession.Static.CurrentPath;

        [VisualScriptingMiscData("Entity", "Returns true if entity has dampeners enabled, false otherwise.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetDampenersEnabled(string entityName)
        {
            MyEntityThrustComponent thrustComponentByEntityName = GetThrustComponentByEntityName(entityName);
            return ((thrustComponentByEntityName != null) ? thrustComponentByEntityName.DampenersEnabled : false);
        }

        [VisualScriptingMiscData("Entity", "Gets typeId and subtypeId out of DefinitionId.", -10510688), VisualScriptingMember(false, false)]
        public static void GetDataFromDefinition(MyDefinitionId definitionId, out string typeId, out string subtypeId)
        {
            typeId = definitionId.TypeId.ToString();
            subtypeId = definitionId.SubtypeId.ToString();
        }

        [VisualScriptingMiscData("Entity", "Gets DefinitionId from typeId and subtypeId", -10510688), VisualScriptingMember(false, false)]
        public static MyDefinitionId GetDefinitionId(string typeId, string subtypeId)
        {
            MyObjectBuilderType type;
            if (!MyObjectBuilderType.TryParse(typeId, out type))
            {
                MyObjectBuilderType.TryParse("MyObjectBuilder_" + typeId, out type);
            }
            return new MyDefinitionId(type, subtypeId);
        }

        [VisualScriptingMiscData("Entity", "Gets specific entity by id.", -10510688), VisualScriptingMember(false, false)]
        public static VRage.Game.Entity.MyEntity GetEntityById(long id)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityById(id, out entity, false) ? entity : null);
        }

        [VisualScriptingMiscData("Entity", "Gets specific entity by name. If there are more entities by same name, the first one created will be taken.", -10510688), VisualScriptingMember(false, false)]
        public static VRage.Game.Entity.MyEntity GetEntityByName(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            if ((name == null) || !Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity))
            {
                return null;
            }
            return entity;
        }

        [VisualScriptingMiscData("Entity", "Gets vector in world coordination system representing entity's direction (e.g. Direction.Forward will return real forward vector of entity in world coordination system.)", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D GetEntityDirection(string entityName, Base6Directions.Direction direction = 0)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return Vector3D.Forward;
            }
            switch (direction)
            {
                case Base6Directions.Direction.Backward:
                    return entityByName.WorldMatrix.Backward;

                case Base6Directions.Direction.Left:
                    return entityByName.WorldMatrix.Left;

                case Base6Directions.Direction.Right:
                    return entityByName.WorldMatrix.Right;

                case Base6Directions.Direction.Up:
                    return entityByName.WorldMatrix.Up;

                case Base6Directions.Direction.Down:
                    return entityByName.WorldMatrix.Down;
            }
            return entityByName.WorldMatrix.Forward;
        }

        [VisualScriptingMiscData("Entity", "Gets entity id from specific entity.", -10510688), VisualScriptingMember(false, false)]
        public static long GetEntityIdFromEntity(VRage.Game.Entity.MyEntity entity) => 
            ((entity != null) ? entity.EntityId : 0L);

        [VisualScriptingMiscData("Entity", "Returns entity id of specific entity ", -10510688), VisualScriptingMember(false, false)]
        public static long GetEntityIdFromName(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) ? entity.EntityId : 0L);
        }

        [VisualScriptingMiscData("Entity", "Gets amount of specific items in inventory of entity. (rounded)", -10510688), VisualScriptingMember(false, false)]
        public static int GetEntityInventoryItemAmount(string entityName, MyDefinitionId itemId) => 
            ((int) Math.Round((double) GetEntityInventoryItemAmountPrecise(entityName, itemId)));

        [VisualScriptingMiscData("Entity", "Gets amount of specific items in inventory of entity.", -10510688), VisualScriptingMember(false, false)]
        public static float GetEntityInventoryItemAmountPrecise(string entityName, MyDefinitionId itemId)
        {
            MyFixedPoint inventoryItemAmount;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                goto TR_0000;
            }
            else
            {
                if (!entityByName.HasInventory && !(entityByName is MyCubeGrid))
                {
                    goto TR_0000;
                }
                inventoryItemAmount = 0;
                if (entityByName is MyCubeGrid)
                {
                    foreach (MyCubeBlock block in ((MyCubeGrid) entityByName).GetFatBlocks())
                    {
                        if (block == null)
                        {
                            continue;
                        }
                        if (block.HasInventory)
                        {
                            inventoryItemAmount += GetInventoryItemAmount(block, itemId);
                        }
                    }
                }
                else
                {
                    inventoryItemAmount = GetInventoryItemAmount(entityByName, itemId);
                }
            }
            return (float) inventoryItemAmount;
        TR_0000:
            return 0f;
        }

        [VisualScriptingMiscData("Entity", "Returns true if entity has item in specific inventory on specific slot. Also return definition id of that item and its amount.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetEntityInventoryItemAtSlot(string entityName, out MyDefinitionId itemId, out float amount, int slot = 0, int inventoryId = 0)
        {
            itemId = new MyDefinitionId();
            amount = 0f;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if ((entityByName != null) && entityByName.HasInventory)
            {
                int num1 = Math.Max(inventoryId, 0);
                inventoryId = num1;
                if (inventoryId >= entityByName.InventoryCount)
                {
                    return false;
                }
                MyInventory inventory = entityByName.GetInventory(inventoryId);
                if (inventory != null)
                {
                    MyPhysicalInventoryItem? itemByIndex = inventory.GetItemByIndex(slot);
                    if (itemByIndex != null)
                    {
                        amount = (float) itemByIndex.Value.Amount;
                        itemId = itemByIndex.Value.Content.GetObjectId();
                        return true;
                    }
                }
            }
            return false;
        }

        [VisualScriptingMiscData("Entity", "Gets name of specific entity defined by id.", -10510688), VisualScriptingMember(false, false)]
        public static string GetEntityName(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false) ? entity.Name : string.Empty);
        }

        [VisualScriptingMiscData("Entity", "Gets position of specific entity.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D GetEntityPosition(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            return ((entityByName == null) ? Vector3D.Zero : entityByName.PositionComp.GetPosition());
        }

        [VisualScriptingMiscData("Entity", "Gets linear velocity of specific entity.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D GetEntitySpeed(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if ((entityByName == null) || (entityByName.Physics == null))
            {
                return Vector3D.Zero;
            }
            return entityByName.Physics.LinearVelocity;
        }

        [VisualScriptingMiscData("Entity", "Breaks and returns world matrix of specific entity.", -10510688), VisualScriptingMember(false, false)]
        public static void GetEntityVectors(string entityName, out Vector3D position, out Vector3D forward, out Vector3D up)
        {
            position = Vector3D.Zero;
            forward = Vector3D.Forward;
            up = Vector3D.Up;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName != null)
            {
                position = entityByName.PositionComp.WorldMatrix.Translation;
                forward = entityByName.PositionComp.WorldMatrix.Forward;
                up = entityByName.PositionComp.WorldMatrix.Up;
            }
        }

        [VisualScriptingMiscData("Entity", "Gets world matrix of specific entity.", -10510688), VisualScriptingMember(false, false)]
        public static MatrixD GetEntityWorldMatrix(VRage.Game.Entity.MyEntity entity) => 
            ((entity != null) ? entity.WorldMatrix : MatrixD.Identity);

        [VisualScriptingMiscData("Factions", "Returns list of all members (of theirs ids) of specific faction.", -10510688), VisualScriptingMember(false, false)]
        public static List<long> GetFactionMembers(string factionTag = "")
        {
            List<long> list = new List<long>();
            MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(factionTag, null);
            if (faction != null)
            {
                foreach (KeyValuePair<long, MyFactionMember> pair in faction.Members)
                {
                    list.Add(pair.Key);
                }
            }
            return list;
        }

        [VisualScriptingMiscData("GUI", "Gets friendly name of the specific screen.", -10510688), VisualScriptingMember(false, false)]
        public static string GetFriendlyName(this MyGuiScreenBase screen) => 
            screen.GetFriendlyName();

        [VisualScriptingMiscData("Grid", "Returns entity id of main cockpit or first cockpit found on grid. Also returns other info such as if cockpit is main or if any cockpit was found.", -10510688), VisualScriptingMember(false, false)]
        public static long GetGridCockpitId(string gridName, out bool isMainCockpit, out bool found, bool checkForEnabledShipControl = true)
        {
            isMainCockpit = false;
            found = true;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    if (grid.MainCockpit != null)
                    {
                        isMainCockpit = true;
                        return grid.MainCockpit.EntityId;
                    }
                    using (MyFatBlockReader<MyCockpit> reader = grid.GetFatBlocks<MyCockpit>().GetEnumerator())
                    {
                        while (true)
                        {
                            if (!reader.MoveNext())
                            {
                                break;
                            }
                            MyCockpit current = reader.Current;
                            if ((current != null) && (!checkForEnabledShipControl || current.EnableShipControl))
                            {
                                return current.EntityId;
                            }
                        }
                    }
                }
            }
            found = false;
            return 0L;
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns grid EntityId of grid that contains block with specific name. Returns 0 if name does not refer to a cube block. (If more entities have same name, only the first one created will be tested.)", -10510688), VisualScriptingMember(false, false)]
        public static long GetGridIdOfBlock(string entityName)
        {
            MyCubeBlock entityByName = GetEntityByName(entityName) as MyCubeBlock;
            return ((entityByName != null) ? entityByName.CubeGrid.EntityId : 0L);
        }

        [VisualScriptingMiscData("Grid", "Returns sums of current integrities, max integrities, block counts.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetGridStatistics(string gridName, out float currentIntegrity, out float maxIntegrity, out int blockCount)
        {
            currentIntegrity = 0f;
            maxIntegrity = 0f;
            blockCount = 0;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    foreach (MySlimBlock block in grid.GetBlocks())
                    {
                        currentIntegrity += block.Integrity;
                        maxIntegrity += block.MaxIntegrity;
                    }
                    blockCount = grid.BlocksCount;
                    return true;
                }
            }
            return false;
        }

        private static MyIdentity GetIdentityFromPlayerId(long playerId = 0L) => 
            ((playerId == 0) ? MySession.Static.LocalHumanPlayer.Identity : MySession.Static.Players.TryGetIdentity(playerId));

        [VisualScriptingMiscData("Grid", "Returns list of all blocks of type 'blockId' on specific grid.", -10510688), VisualScriptingMember(false, false)]
        public static List<long> GetIdListOfSpecificGridBlocks(string gridName, MyDefinitionId blockId)
        {
            List<long> list = new List<long>();
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    foreach (MyCubeBlock block in grid.GetFatBlocks())
                    {
                        if (block == null)
                        {
                            continue;
                        }
                        if ((block.BlockDefinition != null) && (block.BlockDefinition.Id == blockId))
                        {
                            list.Add(block.EntityId);
                        }
                    }
                }
            }
            return list;
        }

        [VisualScriptingMiscData("Grid", "Returns id of first block of type 'blockId' on specific grid.", -10510688), VisualScriptingMember(false, false)]
        public static long GetIdOfFirstSpecificGridBlock(string gridName, MyDefinitionId blockId)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    using (List<MyCubeBlock>.Enumerator enumerator = grid.GetFatBlocks().GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyCubeBlock current = enumerator.Current;
                            if ((current != null) && ((current.BlockDefinition != null) && (current.BlockDefinition.Id == blockId)))
                            {
                                return current.EntityId;
                            }
                        }
                    }
                }
            }
            return 0L;
        }

        [VisualScriptingMiscData("GUI", "Gets whole inventory grid of interacted entity and find index of specific item in it. If no item was found, method will still return inventory grid and index will be set to last index in it (GetItemsCount() - 1). Works only when Terminal screen is opened and focused.", -10510688), VisualScriptingMember(false, false)]
        public static void GetInteractedEntityInventoryItemIndexAndControl(MyDefinitionId itemDefinition, out MyGuiControlBase control, out int index)
        {
            control = null;
            index = -1;
            MyGuiScreenTerminal openedTerminal = GetOpenedTerminal();
            if (openedTerminal != null)
            {
                MyGuiControlInventoryOwner controlByName = openedTerminal.GetControlByName(@"TerminalTabs\PageInventory\RightInventory\MyGuiControlInventoryOwner") as MyGuiControlInventoryOwner;
                if (controlByName != null)
                {
                    using (List<MyGuiControlGrid>.Enumerator enumerator = controlByName.ContentGrids.GetEnumerator())
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MyGuiControlGrid current = enumerator.Current;
                                if (current == null)
                                {
                                    continue;
                                }
                                control = current;
                                index = 0;
                                while (true)
                                {
                                    if (index < current.GetItemsCount())
                                    {
                                        if (!(((MyPhysicalInventoryItem) current.GetItemAt(index).UserData).GetDefinitionId() == itemDefinition))
                                        {
                                            index++;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static MyFixedPoint GetInventoryItemAmount(VRage.Game.Entity.MyEntity entity, MyDefinitionId itemId)
        {
            MyFixedPoint point = 0;
            if ((entity != null) && entity.HasInventory)
            {
                for (int i = 0; i < entity.InventoryCount; i++)
                {
                    MyInventory inventory = entity.GetInventory(i);
                    if (inventory != null)
                    {
                        point += inventory.GetItemAmount(itemId, MyItemFlags.None, false);
                    }
                }
            }
            return point;
        }

        [VisualScriptingMiscData("Blocks Specific", "Gets information about specific landing gear. Returns true if informations were obtained, false if no such Landing gear exists.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetLandingGearInformation(string entityName, out bool locked, out bool inConstraint, out string attachedType, out string attachedName)
        {
            locked = false;
            inConstraint = false;
            attachedType = "";
            attachedName = "";
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return false;
            }
            IMyLandingGear gear = entityByName as IMyLandingGear;
            if (gear == null)
            {
                return false;
            }
            locked = gear.LockMode == LandingGearMode.Locked;
            inConstraint = gear.LockMode == LandingGearMode.ReadyToLock;
            if (locked)
            {
                VRage.Game.Entity.MyEntity attachedEntity = gear.GetAttachedEntity() as VRage.Game.Entity.MyEntity;
                if (attachedEntity != null)
                {
                    attachedType = (attachedEntity is MyCubeBlock) ? "Block" : ((attachedEntity is MyCubeGrid) ? "Grid" : ((attachedEntity is MyVoxelBase) ? "Voxel" : "Other"));
                    attachedName = attachedEntity.Name;
                }
            }
            return true;
        }

        [VisualScriptingMiscData("Blocks Specific", "Gets information about specific landing gear. Returns true if informations were obtained, false if entity is not a Landing gear.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetLandingGearInformationFromEntity(VRage.Game.Entity.MyEntity entity, out bool locked, out bool inConstraint, out string attachedType, out string attachedName)
        {
            locked = false;
            inConstraint = false;
            attachedType = "";
            attachedName = "";
            if (entity == null)
            {
                return false;
            }
            IMyLandingGear gear = entity as IMyLandingGear;
            if (gear == null)
            {
                return false;
            }
            locked = gear.LockMode == LandingGearMode.Locked;
            inConstraint = gear.LockMode == LandingGearMode.ReadyToLock;
            if (locked)
            {
                VRage.Game.Entity.MyEntity attachedEntity = gear.GetAttachedEntity() as VRage.Game.Entity.MyEntity;
                if (attachedEntity != null)
                {
                    attachedType = (attachedEntity is MyCubeBlock) ? "Block" : ((attachedEntity is MyCubeGrid) ? "Grid" : ((attachedEntity is MyVoxelBase) ? "Voxel" : "Other"));
                    attachedName = attachedEntity.Name;
                }
            }
            return true;
        }

        [VisualScriptingMiscData("Factions", "Gets id of local player. Works only on Lobby and clients. On Dedicated server returns 0.", -10510688), VisualScriptingMember(false, false)]
        public static long GetLocalPlayerId() => 
            MySession.Static.LocalPlayerId;

        [VisualScriptingMiscData("Blocks Specific", "Returns merge block status ( -1 - block don't exist, 2 - Locked, 1 - Constrained, 0 - Otherwise).", -10510688), VisualScriptingMember(false, false)]
        public static int GetMergeBlockStatus(string mergeBlockName)
        {
            VRage.Game.Entity.MyEntity entity;
            Sandbox.Game.Entities.MyEntities.TryGetEntityByName(mergeBlockName, out entity);
            if (entity == null)
            {
                return -1;
            }
            MyFunctionalBlock block = entity as MyFunctionalBlock;
            return ((block == null) ? -1 : block.GetBlockSpecificState());
        }

        [VisualScriptingMiscData("Misc", "Returns path to where mods are being stored.", -10510688), VisualScriptingMember(false, false)]
        public static string GetModsPath() => 
            MyFileSystem.ModsPath;

        [VisualScriptingMiscData("Entity", "Returns name of a planet if point is close to a plane (in its natural gravity). Else returns 'Void'. !!!BEWARE 'Void' is just for English as this string is localized. For checking if there really is a planet or not use 'IsPlanetNearby(...)' function as output here might be inconsistent between localizations.", -10510688), VisualScriptingMember(false, false)]
        public static string GetNearestPlanet(Vector3D position)
        {
            if (MyGravityProviderSystem.CalculateNaturalGravityInPoint(position).LengthSquared() > 0f)
            {
                MyPlanet closestPlanet = MyGamePruningStructure.GetClosestPlanet(position);
                if ((closestPlanet != null) && (closestPlanet.Generator != null))
                {
                    return closestPlanet.Generator.FolderName;
                }
            }
            return MyTexts.GetString(MyCommonTexts.Void);
        }

        [VisualScriptingMiscData("Grid", "Gets number of blocks of specified type on the specific grid.", -10510688), VisualScriptingMember(false, false)]
        public static int GetNumberOfGridBlocks(string entityName, string blockTypeId, string blockSubtypeId)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            if (entityByName == null)
            {
                return 0;
            }
            int num = 0;
            bool flag = !string.IsNullOrEmpty(blockTypeId);
            bool flag2 = !string.IsNullOrEmpty(blockSubtypeId);
            foreach (MyCubeBlock block in entityByName.GetFatBlocks())
            {
                if (flag2 & flag)
                {
                    if (block.BlockDefinition.Id.SubtypeName != blockSubtypeId)
                    {
                        continue;
                    }
                    if (block.BlockDefinition.Id.TypeId.ToString() != blockTypeId)
                    {
                        continue;
                    }
                    num++;
                    continue;
                }
                if (flag)
                {
                    if (block.BlockDefinition.Id.TypeId.ToString() != blockTypeId)
                    {
                        continue;
                    }
                    num++;
                    continue;
                }
                if (flag2 && (block.BlockDefinition.Id.SubtypeName == blockSubtypeId))
                {
                    num++;
                }
            }
            return num;
        }

        [VisualScriptingMiscData("Player", "Gets online players.", -10510688), VisualScriptingMember(false, false)]
        public static List<long> GetOnlinePlayers()
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            List<long> list = new List<long>();
            if ((onlinePlayers != null) && (onlinePlayers.Count > 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    list.Add(player.Identity.IdentityId);
                }
            }
            return list;
        }

        [VisualScriptingMiscData("GUI", "Gets currently opened terminal screen. (only if it is focused)", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiScreenTerminal GetOpenedTerminal() => 
            (MyScreenManager.GetScreenWithFocus() as MyGuiScreenTerminal);

        [VisualScriptingMiscData("GUI", "Gets currently opened ToolbarConfig screen (G-Screen). (only if it is focused)", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiScreenToolbarConfigBase GetOpenedToolbarConfig() => 
            (MyScreenManager.GetScreenWithFocus() as MyGuiScreenToolbarConfigBase);

        [VisualScriptingMiscData("Player", "Gets oxygen level at player's position.", -10510688), VisualScriptingMember(false, false)]
        public static float GetOxygenLevelAtPlayersPosition(long playerId = 0L)
        {
            if (MySession.Static.Settings.EnableOxygenPressurization && MySession.Static.Settings.EnableOxygen)
            {
                MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
                if ((characterFromPlayerId != null) && (characterFromPlayerId.OxygenComponent != null))
                {
                    return (float) characterFromPlayerId.OxygenLevelAtCharacterLocation;
                }
            }
            return 1f;
        }

        [VisualScriptingMiscData("Factions", "Gets id of pirate faction.", -10510688), VisualScriptingMember(false, false)]
        public static long GetPirateId() => 
            MyPirateAntennas.GetPiratesId();

        [VisualScriptingMiscData("Player", "Gets player's controlled cube block (grid).", -10510688), VisualScriptingMember(false, false)]
        public static bool GetPlayerControlledBlockData(out string controlType, out long blockId, out string blockName, out long gridId, out string gridName, out bool isRespawnShip, long playerId = 0L)
        {
            controlType = null;
            blockId = 0L;
            blockName = null;
            gridId = 0L;
            gridName = null;
            isRespawnShip = false;
            MyPlayer playerFromPlayerId = GetPlayerFromPlayerId(playerId);
            if ((playerFromPlayerId == null) || !(playerFromPlayerId.Controller.ControlledEntity.Entity is MyCubeBlock))
            {
                return false;
            }
            MyCubeBlock entity = (MyCubeBlock) playerFromPlayerId.Controller.ControlledEntity.Entity;
            controlType = (entity is MyCockpit) ? "Cockpit" : ((entity is MyRemoteControl) ? "Remote" : ((entity is MyUserControllableGun) ? "Turret" : "Other"));
            blockId = entity.EntityId;
            blockName = entity.Name;
            gridId = entity.CubeGrid.EntityId;
            gridName = entity.CubeGrid.Name;
            isRespawnShip = entity.CubeGrid.IsRespawnGrid;
            return true;
        }

        private static MyPlayer GetPlayerFromPlayerId(long playerId = 0L)
        {
            MyPlayer.PlayerId id;
            if (playerId == 0)
            {
                return MySession.Static.LocalHumanPlayer;
            }
            MyPlayer player = null;
            if (!MySession.Static.Players.TryGetPlayerId(playerId, out id))
            {
                return null;
            }
            MySession.Static.Players.TryGetPlayerById(id, out player);
            return player;
        }

        [VisualScriptingMiscData("Player", "Gets energy level of player's suit.", -10510688), VisualScriptingMember(false, false)]
        public static float GetPlayersEnergyLevel(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? -1f : identityFromPlayerId.Character.SuitEnergyLevel);
        }

        [VisualScriptingMiscData("Player", "Gets players entity ID.", -10510688), VisualScriptingMember(false, false)]
        public static long GetPlayersEntityId(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? 0L : identityFromPlayerId.Character.EntityId);
        }

        [VisualScriptingMiscData("Player", "Gets players entity name.", -10510688), VisualScriptingMember(false, false)]
        public static string GetPlayersEntityName(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? null : identityFromPlayerId.Character.Name);
        }

        [VisualScriptingMiscData("Factions", "Gets name of faction, specific player is in.", -10510688), VisualScriptingMember(false, false)]
        public static string GetPlayersFactionName(long playerId = 0L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId) as MyFaction;
            return ((faction != null) ? faction.Name : "");
        }

        [VisualScriptingMiscData("Factions", "Gets tag of faction, specific player is in.", -10510688), VisualScriptingMember(false, false)]
        public static string GetPlayersFactionTag(long playerId = 0L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId) as MyFaction;
            return ((faction != null) ? faction.Tag : "");
        }

        [VisualScriptingMiscData("Player", "Gets player's health.", -10510688), VisualScriptingMember(false, false)]
        public static float GetPlayersHealth(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? -1f : identityFromPlayerId.Character.StatComp.Health.Value);
        }

        [VisualScriptingMiscData("Player", "Gets player's helmet status.", -10510688), VisualScriptingMember(false, false)]
        public static bool GetPlayersHelmetStatus(long playerId = 0L)
        {
            if (!MySession.Static.Settings.EnableOxygenPressurization || !MySession.Static.Settings.EnableOxygen)
            {
                return false;
            }
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            return ((characterFromPlayerId != null) && ((characterFromPlayerId.OxygenComponent != null) && characterFromPlayerId.OxygenComponent.HelmetEnabled));
        }

        [VisualScriptingMiscData("Player", "Gets hydrogen level of player's suit.", -10510688), VisualScriptingMember(false, false)]
        public static float GetPlayersHydrogenLevel(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? -1f : identityFromPlayerId.Character.OxygenComponent.GetGasFillLevel(MyCharacterOxygenComponent.HydrogenId));
        }

        [VisualScriptingMiscData("Player", "Gets player's inventory item amount.", -10510688), VisualScriptingMember(false, false)]
        public static int GetPlayersInventoryItemAmount(long playerId = 0L, MyDefinitionId itemId = new MyDefinitionId()) => 
            ((int) Math.Round((double) GetPlayersInventoryItemAmountPrecise(playerId, itemId)));

        [VisualScriptingMiscData("Player", "Gets player's inventory item amount (precise).", -10510688), VisualScriptingMember(false, false)]
        public static float GetPlayersInventoryItemAmountPrecise(long playerId = 0L, MyDefinitionId itemId = new MyDefinitionId())
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (((identityFromPlayerId == null) || itemId.TypeId.IsNull) || (identityFromPlayerId.Character == null))
            {
                return 0f;
            }
            return (float) GetInventoryItemAmount(identityFromPlayerId.Character, itemId);
        }

        [VisualScriptingMiscData("GUI", "Gets whole inventory grid of player and find index of specific item in it. If no item was found, method will still return inventory grid and index will be set to last index in it (GetItemsCount() - 1). Works only when Terminal screen is opened and focused.", -10510688), VisualScriptingMember(false, false)]
        public static void GetPlayersInventoryItemIndexAndControl(MyDefinitionId itemDefinition, out MyGuiControlBase control, out int index)
        {
            control = null;
            index = -1;
            MyGuiScreenTerminal openedTerminal = GetOpenedTerminal();
            if (openedTerminal != null)
            {
                control = openedTerminal.GetControlByName(@"TerminalTabs\PageInventory\LeftInventory\MyGuiControlInventoryOwner\InventoryGrid");
                MyGuiControlGrid grid = control as MyGuiControlGrid;
                if (grid != null)
                {
                    index = 0;
                    while ((index < grid.GetItemsCount()) && (((MyPhysicalInventoryItem) grid.GetItemAt(index).UserData).GetDefinitionId() != itemDefinition))
                    {
                        index++;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Player", "Gets player's name.", -10510688), VisualScriptingMember(false, false)]
        public static string GetPlayersName(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? "" : identityFromPlayerId.DisplayName);
        }

        [VisualScriptingMiscData("Player", "Gets oxygen level of player's suit.", -10510688), VisualScriptingMember(false, false)]
        public static float GetPlayersOxygenLevel(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId == null) ? -1f : identityFromPlayerId.Character.OxygenComponent.SuitOxygenLevel);
        }

        [VisualScriptingMiscData("Player", "Gets player's position.", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D GetPlayersPosition(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (((identityFromPlayerId == null) || (identityFromPlayerId.Character == null)) || (identityFromPlayerId.Character.PositionComp == null))
            {
                return Vector3D.Zero;
            }
            return identityFromPlayerId.Character.PositionComp.GetPosition();
        }

        [VisualScriptingMiscData("Player", "Gets player's speed (linear velocity).", -10510688), VisualScriptingMember(false, false)]
        public static Vector3D GetPlayersSpeed(long playerId = 0L)
        {
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            return ((characterFromPlayerId == null) ? Vector3D.Zero : characterFromPlayerId.Physics.LinearVelocity);
        }

        [VisualScriptingMiscData("Questlog", "Obsolete. Returns -1.", -10510688), VisualScriptingMember(false, false)]
        public static int GetQuestlogMaxPages() => 
            -1;

        [VisualScriptingMiscData("Questlog", "Obsolete. Returns -1.", -10510688), VisualScriptingMember(false, false), Obsolete]
        public static int GetQuestlogPage() => 
            -1;

        [VisualScriptingMiscData("Questlog", "Obsolete. Returns -1.", -10510688), VisualScriptingMember(false, false)]
        public static int GetQuestlogPageFromMessage(int id = 0) => 
            -1;

        [VisualScriptingMiscData("Misc", "Returns path to where game is being saved.", -10510688), VisualScriptingMember(false, false)]
        public static string GetSavesPath() => 
            MyFileSystem.SavesPath;

        [VisualScriptingMiscData("GUI", "Gets tab on specific index of specified TabControl element.", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiControlTabPage GetTab(this MyGuiControlTabControl tabs, int key) => 
            tabs?.GetTabSubControl(key);

        [VisualScriptingMiscData("GUI", "Gets TabControl elements of specific terminal screen.", -10510688), VisualScriptingMember(false, false)]
        public static MyGuiControlTabControl GetTabs(this MyGuiScreenTerminal terminal) => 
            ((terminal != null) ? (terminal.Controls.GetControlByName("TerminalTabs") as MyGuiControlTabControl) : null);

        private static MyEntityThrustComponent GetThrustComponentByEntityName(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return null;
            }
            MyComponentBase component = null;
            entityByName.Components.TryGet(typeof(MyEntityThrustComponent), out component);
            return (component as MyEntityThrustComponent);
        }

        [VisualScriptingMiscData("GUI", "Gets whole item grid and find index of specific item in it. If no item was found, method will still return the item grid and index will be set to last index in it (GetItemsCount() - 1). Works only when ToolbarConfig screen is opened and focused.", -10510688), VisualScriptingMember(false, false)]
        public static void GetToolbarConfigGridItemIndexAndControl(MyDefinitionId itemDefinition, out MyGuiControlBase control, out int index)
        {
            control = null;
            index = -1;
            MyGuiScreenToolbarConfigBase openedToolbarConfig = GetOpenedToolbarConfig();
            if (openedToolbarConfig != null)
            {
                control = openedToolbarConfig.GetControlByName(@"ScrollablePanel\Grid");
                MyGuiControlGrid grid = control as MyGuiControlGrid;
                if (grid != null)
                {
                    index = 0;
                    while (index < grid.GetItemsCount())
                    {
                        MyGuiGridItem itemAt = grid.GetItemAt(index);
                        if ((itemAt != null) && (itemAt.UserData != null))
                        {
                            MyObjectBuilder_ToolbarItemDefinition itemData = ((MyGuiScreenToolbarConfigBase.GridItemUserData) itemAt.UserData).ItemData as MyObjectBuilder_ToolbarItemDefinition;
                            if ((itemData != null) && (itemData.DefinitionId == itemDefinition))
                            {
                                break;
                            }
                        }
                        index++;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Misc", "Gets name of the control element (keyboard, mouse, gamepad buttons) that is binded to the specific action called 'keyName'. Names are defined in class MyControlsSpace, such as 'STRAFE_LEFT' or 'CUBE_ROTATE_ROLL_POSITIVE'.", -10510688), VisualScriptingMember(false, false)]
        public static string GetUserControlKey(string keyName)
        {
            MyStringId orCompute = MyStringId.GetOrCompute(keyName);
            MyControl gameControl = MyInput.Static.GetGameControl(orCompute);
            return ((gameControl == null) ? "" : gameControl.ToString());
        }

        [VisualScriptingMiscData("Grid", "Returns true if the specific grid has at least one cockpit that enables ship control.", -10510688), VisualScriptingMember(false, false)]
        public static bool HasOperationalCockpit(string gridName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName == null)
            {
                return false;
            }
            MyCubeGrid grid = entityByName as MyCubeGrid;
            if (grid == null)
            {
                return false;
            }
            MyFatBlockReader<MyCockpit> fatBlocks = grid.GetFatBlocks<MyCockpit>();
            bool flag = false;
            using (MyFatBlockReader<MyCockpit> reader2 = fatBlocks.GetEnumerator())
            {
                while (reader2.MoveNext())
                {
                    if (reader2.Current.EnableShipControl)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        [VisualScriptingMiscData("Grid", "Returns true if the specific grid has at least one gyro that is enabled, powered and not-overridden.", -10510688), VisualScriptingMember(false, false)]
        public static bool HasOperationalGyro(string gridName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName == null)
            {
                return false;
            }
            MyCubeGrid grid1 = entityByName as MyCubeGrid;
            MyFatBlockReader<MyGyro> fatBlocks = grid1.GetFatBlocks<MyGyro>();
            bool flag = false;
            foreach (MyGyro gyro in fatBlocks)
            {
                if (!gyro.Enabled)
                {
                    continue;
                }
                if (gyro.IsPowered && !gyro.GyroOverride)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        [VisualScriptingMiscData("Grid", "Returns true if grid has enough power or is in 'adaptable-overload'. (grid is overloaded by adaptable block, that won't cause blackout, such as thrusters or batteries)", -10510688), VisualScriptingMember(false, false)]
        public static bool HasPower(string gridName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName == null)
            {
                return false;
            }
            MyCubeGrid grid = entityByName as MyCubeGrid;
            if (grid == null)
            {
                return false;
            }
            MyResourceStateEnum enum2 = grid.GridSystems.ResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true);
            return ((enum2 == MyResourceStateEnum.Ok) || (enum2 == MyResourceStateEnum.OverloadAdaptible));
        }

        [VisualScriptingMiscData("Grid", "Returns true if entity has thrusters in all directions.", -10510688), VisualScriptingMember(false, false)]
        public static bool HasThrusterInAllDirections(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return false;
            }
            MyCubeGrid grid = entityByName as MyCubeGrid;
            if (grid == null)
            {
                return false;
            }
            ResetThrustDirections();
            foreach (MyThrust thrust in grid.GetFatBlocks<MyThrust>())
            {
                if (!thrust.Enabled)
                {
                    continue;
                }
                if ((Math.Abs(thrust.ThrustOverride) < 0.0001f) && thrust.IsFunctional)
                {
                    m_thrustDirections[thrust.ThrustForwardVector] = true;
                }
            }
            using (Dictionary<Vector3I, bool>.ValueCollection.Enumerator enumerator = m_thrustDirections.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!enumerator.Current)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [VisualScriptingMiscData("Grid", "Returns true if the specified grid has at least one Remote in functional state.", -10510688), VisualScriptingMember(false, false)]
        public static bool HasWorkingRemote(string gridName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if (grid != null)
                {
                    using (MyFatBlockReader<MyRemoteControl> reader2 = grid.GetFatBlocks<MyRemoteControl>().GetEnumerator())
                    {
                        while (true)
                        {
                            if (!reader2.MoveNext())
                            {
                                break;
                            }
                            if (reader2.Current.IsFunctional)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        [VisualScriptingMiscData("GUI", "Highlights specific GUI element in specific screen.", -10510688), VisualScriptingMember(true, false)]
        public static void HighlightGuiControl(string controlName, string activeScreenName)
        {
            foreach (MyGuiScreenBase base2 in MyScreenManager.Screens)
            {
                if (base2.Name == activeScreenName)
                {
                    foreach (MyGuiControlBase base3 in base2.Controls)
                    {
                        if (base3.Name == controlName)
                        {
                            MyGuiScreenHighlight.MyHighlightControl control = new MyGuiScreenHighlight.MyHighlightControl {
                                Control = base3
                            };
                            MyGuiScreenHighlight.HighlightControl(control);
                        }
                    }
                }
            }
        }

        [VisualScriptingMiscData("GUI", "Highlights specific GUI element. If the element is of type MyGuiControlGrid, 'indicies' may be used to select which items should be highlighted. 'customToolTipMessage' can be used for custom tooltip of highlighted element.", -10510688), VisualScriptingMember(true, false)]
        public static void HighlightGuiControl(MyGuiControlBase control, List<int> indicies = null, string customToolTipMessage = null)
        {
            if (control != null)
            {
                MyGuiScreenHighlight.MyHighlightControl control2 = new MyGuiScreenHighlight.MyHighlightControl {
                    Control = control
                };
                if (indicies != null)
                {
                    control2.Indices = indicies.ToArray();
                }
                if (!string.IsNullOrEmpty(customToolTipMessage))
                {
                    control2.CustomToolTips = new MyToolTips(customToolTipMessage);
                }
                MyGuiScreenHighlight.HighlightControl(control2);
            }
        }

        public static void Init()
        {
            MyCubeGrids.BlockBuilt += delegate (MyCubeGrid grid, MySlimBlock block) {
                if (BlockBuilt != null)
                {
                    BlockBuilt(block.BlockDefinition.Id.TypeId.ToString(), block.BlockDefinition.Id.SubtypeName, grid.Name, (block.FatBlock != null) ? block.FatBlock.EntityId : 0L);
                }
            };
            if (!m_registered)
            {
                m_registered = true;
                MySession.OnLoading += delegate {
                    m_addedNotificationsById.Clear();
                    m_playerIdsToHighlightData.Clear();
                };
                Sandbox.Game.Entities.MyEntities.OnEntityAdd += delegate (VRage.Game.Entity.MyEntity entity) {
                    MyCubeGrid grid = entity as MyCubeGrid;
                    if (((grid != null) && (BlockBuilt != null)) && (grid.BlocksCount == 1))
                    {
                        MySlimBlock cubeBlock = grid.GetCubeBlock(Vector3I.Zero);
                        if (cubeBlock != null)
                        {
                            BlockBuilt(cubeBlock.BlockDefinition.Id.TypeId.ToString(), cubeBlock.BlockDefinition.Id.SubtypeName, grid.Name, (cubeBlock.FatBlock != null) ? cubeBlock.FatBlock.EntityId : 0L);
                        }
                    }
                };
                MyScreenManager.ScreenRemoved += delegate (MyGuiScreenBase screen) {
                    if (ScreenRemoved != null)
                    {
                        ScreenRemoved(screen);
                    }
                };
                MyScreenManager.ScreenAdded += delegate (MyGuiScreenBase screen) {
                    if (ScreenAdded != null)
                    {
                        ScreenAdded(screen);
                    }
                };
                MyRespawnComponentBase.RespawnRequested += delegate (MyPlayer player) {
                    if (PlayerRespawnRequest != null)
                    {
                        PlayerRespawnRequest(player.Identity.IdentityId);
                    }
                };
                MyVisualScriptingProxy.RegisterType(typeof(MyGuiSounds));
                MyVisualScriptingProxy.RegisterType(typeof(MyKeys));
                MyVisualScriptingProxy.RegisterType(typeof(FlightMode));
                MyVisualScriptingProxy.RegisterType(typeof(Base6Directions.Direction));
                MyVisualScriptingProxy.WhitelistExtensions(typeof(Sandbox.Game.MyVisualScriptLogicProvider));
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns true if functional block exists and is enabled, otherwise false.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsBlockEnabled(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) && ((entity is MyFunctionalBlock) && (entity as MyFunctionalBlock).Enabled));
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns true if specific cube block exists and is in functional state, otherwise false.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsBlockFunctional(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) && ((entity is MyCubeBlock) && (entity as MyCubeBlock).IsFunctional));
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns true if specific cube block exists and is in functional state, otherwise false. Access block by Id", -10510688), VisualScriptingMember(false, false)]
        public static bool IsBlockFunctionalById(long id)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityById(id, out entity, false) && ((entity is MyCubeBlock) && (entity as MyCubeBlock).IsFunctional));
        }

        [VisualScriptingMiscData("Blocks Specific", "True if block is part of airtight room (Best used for AirVents).", -10510688), VisualScriptingMember(true, false)]
        public static bool IsBlockPositionAirtight(string blockName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity))
            {
                return false;
            }
            Sandbox.ModAPI.IMyFunctionalBlock block = entity as Sandbox.ModAPI.IMyFunctionalBlock;
            return ((block != null) ? block.CubeGrid.IsRoomAtPositionAirtight(block.Position) : false);
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns true if specific functional block exist and is powered, otherwise false.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsBlockPowered(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) && ((entity is MyFunctionalBlock) && (((entity as MyFunctionalBlock).ResourceSink != null) && (entity as MyFunctionalBlock).ResourceSink.IsPowered)));
        }

        [VisualScriptingMiscData("Blocks Generic", "Returns true if specific functional block exists and is working, otherwise false.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsBlockWorking(string name)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) && ((entity is MyFunctionalBlock) && (entity as MyFunctionalBlock).IsWorking));
        }

        [VisualScriptingMiscData("Blocks Specific", "Returns true if specific connector is locked. False if unlocked of no such connector exists.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsConnectorLocked(string connectorName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(connectorName);
            if (entityByName == null)
            {
                return false;
            }
            Sandbox.ModAPI.IMyShipConnector connector = entityByName as Sandbox.ModAPI.IMyShipConnector;
            return ((connector != null) && connector.IsConnected);
        }

        [VisualScriptingMiscData("Blocks Generic", "Return true if 'secondBlock' is reachable from 'firstBlock'. (Can be only onle-way) ", -10510688), VisualScriptingMember(false, false)]
        public static bool IsConveyorConnected(string firstBlock, string secondBlock)
        {
            VRage.Game.Entity.MyEntity entity;
            if (firstBlock.Equals(secondBlock))
            {
                return true;
            }
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(firstBlock, out entity))
            {
                IMyConveyorEndpointBlock block = entity as IMyConveyorEndpointBlock;
                if ((block != null) && Sandbox.Game.Entities.MyEntities.TryGetEntityByName(secondBlock, out entity))
                {
                    IMyConveyorEndpointBlock block2 = entity as IMyConveyorEndpointBlock;
                    return ((block2 != null) && MyGridConveyorSystem.Reachable(block.ConveyorEndpoint, block2.ConveyorEndpoint));
                }
            }
            return false;
        }

        [VisualScriptingMiscData("Gameplay", "Returns true if world is creative.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsCreative() => 
            MySession.Static.CreativeMode;

        [VisualScriptingMiscData("Blocks Specific", "Returns true if specific doors are open false if closed or door does not exist.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsDoorOpen(string doorBlockName)
        {
            VRage.Game.Entity.MyEntity entity;
            return (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(doorBlockName, out entity) && (!(entity is MyAdvancedDoor) ? (!(entity is MyAirtightDoorGeneric) ? ((entity is MyDoor) && (entity as MyDoor).Open) : (entity as MyAirtightDoorGeneric).Open) : (entity as MyAdvancedDoor).Open));
        }

        [VisualScriptingMiscData("Grid", "Returns true if the specified grid has at least one functional gyro, at least one controlling block (cockpit/remote) and thrusters in all directions.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsFlyable(string entityName)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            if (entityByName == null)
            {
                return false;
            }
            MyResourceStateEnum enum2 = entityByName.GridSystems.ResourceDistributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, true);
            if ((enum2 == MyResourceStateEnum.OverloadBlackout) || (enum2 == MyResourceStateEnum.NoPower))
            {
                return false;
            }
            MyFatBlockReader<MyGyro> fatBlocks = entityByName.GetFatBlocks<MyGyro>();
            bool flag = false;
            foreach (MyGyro gyro in fatBlocks)
            {
                if (!gyro.Enabled)
                {
                    continue;
                }
                if (gyro.IsPowered && !gyro.GyroOverride)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }
            MyFatBlockReader<MyShipController> reader2 = entityByName.GetFatBlocks<MyShipController>();
            bool flag2 = false;
            using (MyFatBlockReader<MyShipController> reader5 = reader2.GetEnumerator())
            {
                while (reader5.MoveNext())
                {
                    if (reader5.Current.EnableShipControl)
                    {
                        flag2 = true;
                        break;
                    }
                }
            }
            if (!flag2)
            {
                return false;
            }
            ResetThrustDirections();
            foreach (MyThrust thrust in entityByName.GetFatBlocks<MyThrust>())
            {
                if (!thrust.IsPowered)
                {
                    continue;
                }
                if (thrust.Enabled && (Math.Abs(thrust.ThrustOverride) < 0.0001f))
                {
                    m_thrustDirections[thrust.ThrustForwardVector] = true;
                }
            }
            using (Dictionary<Vector3I, bool>.ValueCollection.Enumerator enumerator = m_thrustDirections.Values.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (!enumerator.Current)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [VisualScriptingMiscData("Gameplay", "Returns true if session is fully loaded.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGameLoaded() => 
            GameIsReady;

        [VisualScriptingMiscData("Grid", "Returns true if grid is marked as destructible.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGridDestructible(string entityName)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            return ((entityByName != null) ? entityByName.DestructibleBlocks : true);
        }

        [VisualScriptingMiscData("Grid", "Returns true if the specific grid is marked as editable.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGridEditable(string entityName)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            return ((entityByName != null) ? entityByName.Editable : true);
        }

        [VisualScriptingMiscData("Grid", "Returns true if any Landing gear of specific grid is in locked state.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsGridLockedWithLandingGear(string gridName)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            return ((entityByName != null) && ((entityByName.GridSystems.LandingSystem.Locked == MyMultipleEnabledEnum.Mixed) || (entityByName.GridSystems.LandingSystem.Locked == MyMultipleEnabledEnum.AllEnabled)));
        }

        [VisualScriptingMiscData("Blocks Specific", "Returns true if Landing gear is locked, false otherwise.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsLandingGearLocked(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return false;
            }
            IMyLandingGear gear = entityByName as IMyLandingGear;
            return ((gear != null) && (gear.LockMode == LandingGearMode.Locked));
        }

        [VisualScriptingMiscData("GUI", "Returns true if specific key was pressed in this frame.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsNewKeyPressed(MyKeys key) => 
            MyInput.Static.IsNewKeyPressed(key);

        [VisualScriptingMiscData("Entity", "Returns true if point is in natural gravity close to planet(eg. if nearest planet exists).", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlanetNearby(Vector3D position) => 
            ((MyGravityProviderSystem.CalculateNaturalGravityInPoint(position).LengthSquared() > 0f) && (MyGamePruningStructure.GetClosestPlanet(position) != null));

        [VisualScriptingMiscData("Player", "Checks if player is dead.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlayerDead(long playerId = 0L)
        {
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            return ((characterFromPlayerId != null) && characterFromPlayerId.IsDead);
        }

        [VisualScriptingMiscData("Player", "Checks if player is in cockpit.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlayerInCockpit(long playerId = 0L, string gridName = null, string cockpitName = null)
        {
            MyPlayer playerFromPlayerId = GetPlayerFromPlayerId(playerId);
            MyCockpit entity = null;
            if (((playerFromPlayerId != null) && (playerFromPlayerId.Controller != null)) && (playerFromPlayerId.Controller.ControlledEntity != null))
            {
                entity = playerFromPlayerId.Controller.ControlledEntity.Entity as MyCockpit;
            }
            return (((entity != null) && (string.IsNullOrEmpty(gridName) || (entity.CubeGrid.Name == gridName))) && (string.IsNullOrEmpty(cockpitName) || (entity.Name == cockpitName)));
        }

        [VisualScriptingMiscData("Player", "Checks if player is controlling something over remote.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlayerInRemote(long playerId = 0L, string gridName = null, string remoteName = null)
        {
            MyPlayer playerFromPlayerId = GetPlayerFromPlayerId(playerId);
            MyRemoteControl entity = null;
            if (((playerFromPlayerId != null) && (playerFromPlayerId.Controller != null)) && (playerFromPlayerId.Controller.ControlledEntity != null))
            {
                entity = playerFromPlayerId.Controller.ControlledEntity.Entity as MyRemoteControl;
            }
            return (((entity != null) && (string.IsNullOrEmpty(gridName) || (entity.CubeGrid.Name == gridName))) && (string.IsNullOrEmpty(remoteName) || (entity.Name == remoteName)));
        }

        [VisualScriptingMiscData("Player", "Checks if player is controlling weapon.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlayerInWeapon(long playerId = 0L, string gridName = null, string weaponName = null)
        {
            MyPlayer playerFromPlayerId = GetPlayerFromPlayerId(playerId);
            MyUserControllableGun entity = null;
            if (((playerFromPlayerId != null) && (playerFromPlayerId.Controller != null)) && (playerFromPlayerId.Controller.ControlledEntity != null))
            {
                entity = playerFromPlayerId.Controller.ControlledEntity.Entity as MyUserControllableGun;
            }
            return (((entity != null) && (string.IsNullOrEmpty(gridName) || (entity.CubeGrid.Name == gridName))) && (string.IsNullOrEmpty(weaponName) || (entity.Name == weaponName)));
        }

        [VisualScriptingMiscData("Player", "Checks if player's jetpack is enabled.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsPlayersJetpackEnabled(long playerId = 0L)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            return ((identityFromPlayerId != null) && ((identityFromPlayerId.Character != null) && ((identityFromPlayerId.Character.JetpackComp != null) && identityFromPlayerId.Character.JetpackComp.TurnedOn)));
        }

        [VisualScriptingMiscData("Gameplay", "Returns true if world is survival.", -10510688), VisualScriptingMember(false, false)]
        public static bool IsSurvival() => 
            MySession.Static.SurvivalMode;

        [VisualScriptingMiscData("Factions", "Kicks specific player from faction he is in.", -10510688), VisualScriptingMember(true, false)]
        public static void KickPlayerFromFaction(long playerId = 0L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(playerId) as MyFaction;
            MyFactionCollection.KickMember((faction != null) ? faction.FactionId : 0L, playerId);
        }

        [VisualScriptingMiscData("Audio", "Sets currently selected category to specific category and play a track from it.", -10510688), VisualScriptingMember(true, false)]
        public static void MusicPlayMusicCategory(string categoryName, bool playAtLeastOnce = true)
        {
            if (MyAudio.Static != null)
            {
                if (MyMusicController.Static == null)
                {
                    MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
                    MyAudio.Static.MusicAllowed = false;
                    MyMusicController.Static.Active = true;
                }
                MyStringId orCompute = MyStringId.GetOrCompute(categoryName);
                if (orCompute.Id != 0)
                {
                    MyMusicController.Static.PlaySpecificMusicCategory(orCompute, playAtLeastOnce);
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Plays specific music cue.", -10510688), VisualScriptingMember(true, false)]
        public static void MusicPlayMusicCue(string cueName, bool playAtLeastOnce = true)
        {
            if (MyAudio.Static != null)
            {
                if (MyMusicController.Static == null)
                {
                    MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
                    MyAudio.Static.MusicAllowed = false;
                    MyMusicController.Static.Active = true;
                }
                MyCueId cueId = MyAudio.Static.GetCueId(cueName);
                if (!cueId.IsNull)
                {
                    MySoundData cue = MyAudio.Static.GetCue(cueId);
                    if ((cue == null) || cue.Category.Equals(MUSIC))
                    {
                        MyMusicController.Static.PlaySpecificMusicTrack(cueId, playAtLeastOnce);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Enables/disables dynamic music category changes.", -10510688), VisualScriptingMember(true, false)]
        public static void MusicSetDynamicMusic(bool enabled)
        {
            if (MyAudio.Static != null)
            {
                if (MyMusicController.Static == null)
                {
                    MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
                    MyAudio.Static.MusicAllowed = false;
                    MyMusicController.Static.Active = true;
                }
                MyMusicController.Static.CanChangeCategoryGlobal = enabled;
            }
        }

        [VisualScriptingMiscData("Audio", "Sets currently selected category to specific music category.", -10510688), VisualScriptingMember(true, false)]
        public static void MusicSetMusicCategory(string categoryName)
        {
            if (MyAudio.Static != null)
            {
                if (MyMusicController.Static == null)
                {
                    MyMusicController.Static = new MyMusicController(MyAudio.Static.GetAllMusicCues());
                    MyAudio.Static.MusicAllowed = false;
                    MyMusicController.Static.Active = true;
                }
                MyStringId orCompute = MyStringId.GetOrCompute(categoryName);
                if (orCompute.Id != 0)
                {
                    MyMusicController.Static.SetSpecificMusicCategory(orCompute);
                }
            }
        }

        [VisualScriptingMiscData("Cutscenes", "Goes to next node in current cutscene. If 'playerId' is -1, apply for all players, otherwise only for specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void NextCutsceneNode(long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(x => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.NextCutsceneNodeSync), num, targetEndpoint, position);
        }

        [Event(null, 0x7df), Reliable, Server, Broadcast]
        private static void NextCutsceneNodeSync(long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                MySession.Static.GetComponent<MySessionComponentCutscenes>().CutsceneNext(true);
            }
        }

        private static void OnExclusiveHighlightAccepted(MyHighlightSystem.MyHighlightData data, int exclusiveKey)
        {
            if (data.Thickness != -1f)
            {
                List<MyTuple<long, int>> local1 = m_playerIdsToHighlightData[data.PlayerId];
                int num = local1.FindIndex(tuple => tuple.Item1 == data.EntityId);
                m_playerIdsToHighlightData[data.PlayerId][num] = new MyTuple<long, int>(local1[num].Item1, exclusiveKey);
                MySession.Static.GetComponent<MyHighlightSystem>().ExclusiveHighlightRejected -= new Action<MyHighlightSystem.MyHighlightData, int>(Sandbox.Game.MyVisualScriptLogicProvider.OnExclusiveHighlightRejected);
            }
        }

        private static void OnExclusiveHighlightRejected(MyHighlightSystem.MyHighlightData data, int exclusiveKey)
        {
            m_playerIdsToHighlightData[data.PlayerId].RemoveAll(tuple => tuple.Item1 == data.EntityId);
            MySession.Static.GetComponent<MyHighlightSystem>().ExclusiveHighlightAccepted -= new Action<MyHighlightSystem.MyHighlightData, int>(Sandbox.Game.MyVisualScriptLogicProvider.OnExclusiveHighlightAccepted);
        }

        [VisualScriptingMiscData("GUI", "Opens steam overlay. If playerID is 0, open it for local player else open it for targeted player.", -10510688), VisualScriptingMember(true, false)]
        public static void OpenSteamOverlay(string url, long playerId = 0L)
        {
            if (playerId == 0)
            {
                OpenSteamOverlaySync(url);
            }
            else
            {
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    Vector3D? position = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string>(s => new Action<string>(Sandbox.Game.MyVisualScriptLogicProvider.OpenSteamOverlaySync), url, new EndpointId(id.SteamId), position);
                }
            }
        }

        [Event(null, 0x10ec), Reliable, Client]
        private static void OpenSteamOverlaySync(string url)
        {
            if (MyGuiSandbox.IsUrlWhitelisted(url))
            {
                MyGameService.OpenOverlayUrl(url);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Resets research for the specific player. If 'playerId' equals -1, resets research for the local player.", -10510688), VisualScriptingMember(true, false)]
        public static void PlayerResearchClear(long playerId = -1L)
        {
            if ((playerId == -1L) && (MySession.Static.LocalCharacter != null))
            {
                playerId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.ResetResearch(playerId);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Resets research for all.", -10510688), VisualScriptingMember(true, false)]
        public static void PlayerResearchClearAll()
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.ResetResearchForAll();
            }
        }

        [VisualScriptingMiscData("G-Screen", "Locks the specific research for the specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void PlayerResearchLock(long playerId, MyDefinitionId itemId)
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.LockResearch(playerId, itemId);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Unlocks the specific research for the specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void PlayerResearchUnlock(long playerId, MyDefinitionId itemId)
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.UnlockResearchDirect(playerId, itemId);
            }
        }

        [VisualScriptingMiscData("Audio", "Plays specific 2D HUD sound.", -10510688), VisualScriptingMember(true, false)]
        public static void PlayHudSound(MyGuiSounds sound = 0, long playerId = 0L)
        {
            if (MyAudio.Static != null)
            {
                MyGuiAudio.PlaySound(sound);
            }
        }

        [VisualScriptingMiscData("Audio", "Plays single sound on emitter attached to specific entity.", -10510688), VisualScriptingMember(true, false)]
        public static void PlaySingleSoundAtEntity(string soundName, string entityName)
        {
            if ((MyAudio.Static != null) && (soundName.Length > 0))
            {
                MySoundPair objA = new MySoundPair(soundName, true);
                if (!ReferenceEquals(objA, MySoundPair.Empty))
                {
                    VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
                    if (entityByName != null)
                    {
                        MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                        if (emitter != null)
                        {
                            emitter.Entity = entityByName;
                            bool? nullable = null;
                            emitter.PlaySound(objA, false, false, false, false, false, nullable);
                        }
                    }
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Plays specific 3D sound at specific point.", -10510688), VisualScriptingMember(true, false)]
        public static void PlaySingleSoundAtPosition(string soundName, Vector3 position)
        {
            if ((MyAudio.Static != null) && (soundName.Length > 0))
            {
                MySoundPair objA = new MySoundPair(soundName, true);
                if (!ReferenceEquals(objA, MySoundPair.Empty))
                {
                    MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                    if (emitter != null)
                    {
                        emitter.SetPosition(new Vector3D?(position));
                        bool? nullable = null;
                        emitter.PlaySound(objA, false, false, false, false, false, nullable);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Plays sound on specific emitter. If 'playIn2D' is true, sound will be forced 2D.", -10510688), VisualScriptingMember(true, false)]
        public static void PlaySound(string EmitterId, string soundName, bool playIn2D = false)
        {
            if ((MyAudio.Static != null) && (EmitterId.Length > 0))
            {
                MySoundPair objA = new MySoundPair(soundName, true);
                if (!ReferenceEquals(objA, MySoundPair.Empty))
                {
                    MyEntity3DSoundEmitter libraryEmitter = MyAudioComponent.GetLibraryEmitter(EmitterId);
                    if (libraryEmitter != null)
                    {
                        bool? nullable = null;
                        libraryEmitter.PlaySound(objA, true, false, playIn2D, false, false, nullable);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Toolbar", "Reloads default settings for the toolbar", -10510688), VisualScriptingMember(true, false)]
        public static void ReloadToolbarDefaults(long playerId = -1L)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ReloadToolbarDefaultsSync), playerId, targetEndpoint, position);
        }

        [Event(null, 0x1777), Reliable, Server, Broadcast]
        private static void ReloadToolbarDefaultsSync(long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                MyToolbarComponent.CurrentToolbar.SetDefaults(true);
            }
        }

        [VisualScriptingMiscData("Triggers", "Removes all area triggers from the specified entity.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveAllTriggersFromEntity(string entityName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                entity.Components.Remove(typeof(MyTriggerAggregate));
            }
        }

        [VisualScriptingMiscData("Entity", "Removes specific entity from world.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveEntity(string entityName)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName != null)
            {
                Sandbox.Game.Entities.MyEntities.RemoveName(entityByName);
                if ((entityByName is MyCubeGrid) || (entityByName is MyFloatingObject))
                {
                    entityByName.Close();
                }
                if (entityByName is MyCubeBlock)
                {
                    MyCubeBlock block = (MyCubeBlock) entityByName;
                    block.CubeGrid.RemoveBlock(block.SlimBlock, true);
                }
            }
        }

        [VisualScriptingMiscData("Entity", "Removes item defined by id in specific quantity from inventory of entity.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveFromEntityInventory(string entityName, MyDefinitionId itemId = new MyDefinitionId(), float amount = 1f)
        {
            MyFixedPoint amountToRemove = (MyFixedPoint) amount;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName != null)
            {
                if (entityByName is MyCubeGrid)
                {
                    foreach (MyCubeBlock block in ((MyCubeGrid) entityByName).GetFatBlocks())
                    {
                        if ((block != null) && block.HasInventory)
                        {
                            amountToRemove -= RemoveInventoryItems(block, itemId, amountToRemove);
                        }
                        if (amountToRemove <= 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    RemoveInventoryItems(entityByName, itemId, amountToRemove);
                }
            }
        }

        [VisualScriptingMiscData("Player", "Removes the specified item from the player's inventory.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveFromPlayersInventory(long playerId = 0L, MyDefinitionId itemId = new MyDefinitionId(), int amount = 1)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                MyInventory inventory = identityFromPlayerId.Character.GetInventory(0);
                if (inventory != null)
                {
                    MyObjectBuilder_PhysicalObject objectBuilder = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) itemId);
                    MyFixedPoint point = new MyFixedPoint();
                    point = amount;
                    MyFixedPoint point2 = inventory.GetItemAmount(itemId, MyItemFlags.None, false);
                    inventory.RemoveItemsOfType((point < point2) ? point : point2, objectBuilder, false, true);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Removes GPS from specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveGPS(string name, long playerId = -1L)
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(playerId, name);
            if (gpsByName != null)
            {
                MySession.Static.Gpss.SendDelete(playerId, gpsByName.Hash);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Removes GPS from all players.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveGPSForAll(string name)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    RemoveGPS(name, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Removes specific GPS from specific entity for local player only. ('GPSDescription' is not used. Cant remove due to backward compatibility.)", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveGPSFromEntity(string entityName, string GPSName, string GPSDescription, long playerId = -1L)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                if (playerId == -1L)
                {
                    playerId = GetLocalPlayerId();
                }
                MyTuple<string, string> tuple1 = new MyTuple<string, string>(entityName, GPSName);
                IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(playerId, GPSName);
                if (gpsByName != null)
                {
                    MySession.Static.Gpss.SendDelete(playerId, gpsByName.Hash);
                }
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Removes specific GPS from specific entity for all players. ('GPSDescription' is not used. Cant remove due to backward compatibility.)", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveGPSFromEntityForAll(string entityName, string GPSName, string GPSDescription)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    RemoveGPSFromEntity(entityName, GPSName, GPSDescription, player.Identity.IdentityId);
                }
            }
        }

        [VisualScriptingMiscData("AI", "Removes specific grid from drone's targets.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveGridFromTargetList(string gridName, string targetGridname)
        {
            MyCubeGrid grid;
            MyCubeGrid grid2;
            if (TryGetGrid(gridName, out grid) && TryGetGrid(targetGridname, out grid2))
            {
                grid.TargetingRemoveId(grid2.EntityId);
            }
        }

        private static MyFixedPoint RemoveInventoryItems(VRage.Game.Entity.MyEntity entity, MyDefinitionId itemId, MyFixedPoint amountToRemove)
        {
            MyFixedPoint point = 0;
            MyFixedPoint b = 0;
            if (((entity != null) && entity.HasInventory) && (amountToRemove > 0))
            {
                for (int i = 0; i < entity.InventoryCount; i++)
                {
                    MyInventory inventory = entity.GetInventory(i);
                    if (inventory != null)
                    {
                        b = inventory.GetItemAmount(itemId, MyItemFlags.None, false);
                        if (b > 0)
                        {
                            b = MyFixedPoint.Min(amountToRemove, b);
                            inventory.RemoveItemsOfType(b, itemId, MyItemFlags.None, false);
                            point += b;
                            amountToRemove -= b;
                        }
                    }
                    if (amountToRemove <= 0)
                    {
                        break;
                    }
                }
            }
            return point;
        }

        [VisualScriptingMiscData("Notifications", "Removes the specific notification referenced by its id from the specific player. If 'playerId' is equal to 0, apply on local player, if -1, apply to all.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveNotification(int messageId, long playerId = -1L)
        {
            if (playerId == 0)
            {
                RemoveNotificationSync(messageId, -1L);
            }
            else
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, long>(s => new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.RemoveNotificationSync), messageId, playerId, targetEndpoint, position);
            }
        }

        [Event(null, 0x1284), Reliable, Server, Broadcast]
        private static void RemoveNotificationSync(int messageId, long playerId = -1L)
        {
            MyHudNotification notification;
            if (((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))) && m_addedNotificationsById.TryGetValue(messageId, out notification))
            {
                MyHud.Notifications.Remove(notification);
                m_addedNotificationsById.Remove(messageId);
            }
        }

        [VisualScriptingMiscData("Questlog", "Removes details of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveQuestlogDetails(long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.RemoveQuestlogDetailsSync), num, targetEndpoint, position);
        }

        [Event(null, 0x15a7), Reliable, Server, Broadcast]
        private static void RemoveQuestlogDetailsSync(long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MyHud.Questlog.CleanDetails();
            }
        }

        [VisualScriptingMiscData("Audio", "Removes specific sound emitter.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveSoundEmitter(string EmitterId)
        {
            if ((MyAudio.Static != null) && (EmitterId.Length > 0))
            {
                MyAudioComponent.RemoveLibraryEmitter(EmitterId);
            }
        }

        [VisualScriptingMiscData("Triggers", "Remove area trigger with the specified name.", -10510688), VisualScriptingMember(true, false)]
        public static void RemoveTrigger(string triggerName)
        {
            if (MySessionComponentTriggerSystem.Static != null)
            {
                MyTriggerComponent component;
                VRage.Game.Entity.MyEntity triggersEntity = MySessionComponentTriggerSystem.Static.GetTriggersEntity(triggerName, out component);
                if ((triggersEntity != null) && (component != null))
                {
                    MyTriggerAggregate aggregate;
                    if (triggersEntity.Components.TryGet<MyTriggerAggregate>(out aggregate))
                    {
                        aggregate.RemoveComponent(component);
                    }
                    else
                    {
                        triggersEntity.Components.Remove(typeof(MyAreaTriggerComponent), component as MyAreaTriggerComponent);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Entity", "Renames specific entity.", -10510688), VisualScriptingMember(true, false)]
        public static void RenameEntity(string oldName, string newName = null)
        {
            if (oldName != newName)
            {
                VRage.Game.Entity.MyEntity entityByName = GetEntityByName(oldName);
                if (entityByName != null)
                {
                    entityByName.Name = newName;
                    Sandbox.Game.Entities.MyEntities.SetEntityName(entityByName, true);
                }
            }
        }

        [VisualScriptingMiscData("Questlog", "Replaces detail of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void ReplaceQuestlogDetail(int id = 0, string newDetail = "", bool useTyping = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, string, bool, long>(s => new Action<int, string, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.ReplaceQuestlogDetailSync), id, newDetail, useTyping, num, targetEndpoint, position);
        }

        [Event(null, 0x1596), Reliable, Server, Broadcast]
        private static void ReplaceQuestlogDetailSync(int id = 0, string newDetail = "", bool useTyping = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MyHud.Questlog.ModifyDetail(id, newDetail, useTyping);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Adds specific item into research.", -10510688), VisualScriptingMember(true, false)]
        public static void ResearchListAddItem(MyDefinitionId itemId)
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.AddRequiredResearch(itemId);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Clears required research list for all.", -10510688), VisualScriptingMember(true, false)]
        public static void ResearchListClear()
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.ClearRequiredResearch();
            }
        }

        [VisualScriptingMiscData("G-Screen", "Removes specific item from research.", -10510688), VisualScriptingMember(true, false)]
        public static void ResearchListRemoveItem(MyDefinitionId itemId)
        {
            if (MySessionComponentResearch.Static != null)
            {
                MySessionComponentResearch.Static.RemoveRequiredResearch(itemId);
            }
        }

        [VisualScriptingMiscData("G-Screen", "[OBSOLETE] Enables/disables research whitelist mode.", -10510688), VisualScriptingMember(true, false)]
        public static void ResearchListWhitelist(bool whitelist)
        {
        }

        private static void ResetThrustDirections()
        {
            if (m_thrustDirections.Count == 0)
            {
                m_thrustDirections.Add(Vector3I.Forward, false);
                m_thrustDirections.Add(Vector3I.Backward, false);
                m_thrustDirections.Add(Vector3I.Left, false);
                m_thrustDirections.Add(Vector3I.Right, false);
                m_thrustDirections.Add(Vector3I.Up, false);
                m_thrustDirections.Add(Vector3I.Down, false);
            }
            else
            {
                m_thrustDirections[Vector3I.Forward] = false;
                m_thrustDirections[Vector3I.Backward] = false;
                m_thrustDirections[Vector3I.Left] = false;
                m_thrustDirections[Vector3I.Right] = false;
                m_thrustDirections[Vector3I.Up] = false;
                m_thrustDirections[Vector3I.Down] = false;
            }
        }

        [VisualScriptingMiscData("Gameplay", "Saves the game.", -10510688), VisualScriptingMember(true, false)]
        public static bool SaveSession()
        {
            if (MyAsyncSaving.InProgress)
            {
                return false;
            }
            MyAsyncSaving.Start(null, null, false);
            return true;
        }

        [VisualScriptingMiscData("Gameplay", "Saves the game under specific name.", -10510688), VisualScriptingMember(true, false)]
        public static bool SaveSessionAs(string saveName)
        {
            if (MyAsyncSaving.InProgress)
            {
                return false;
            }
            MyAsyncSaving.Start(null, MyStatControlText.SubstituteTexts(saveName, null), false);
            return true;
        }

        [VisualScriptingMiscData("Effects", "Sets if screen fade should minimize HUD.", -10510688), VisualScriptingMember(true, false)]
        public static void ScreenColorFadingMinimalizeHUD(bool minimalize)
        {
            MyHud.ScreenEffects.BlackScreenMinimalizeHUD = minimalize;
        }

        [VisualScriptingMiscData("Effects", "Sets target color for screen fading.", -10510688), VisualScriptingMember(true, false)]
        public static void ScreenColorFadingSetColor(Color color)
        {
            MyHud.ScreenEffects.BlackScreenColor = new Color(color, 0f);
        }

        [VisualScriptingMiscData("Effects", "Fades/shows screen over period of time.", -10510688), VisualScriptingMember(true, false)]
        public static void ScreenColorFadingStart(float time = 1f, bool toOpaque = true)
        {
            MyHud.ScreenEffects.FadeScreen(toOpaque ? 0f : 1f, time);
        }

        [VisualScriptingMiscData("Effects", "Switches screen fade state. Screen will un/fade over specified time.", -10510688), VisualScriptingMember(true, false)]
        public static void ScreenColorFadingStartSwitch(float time = 1f)
        {
            MyHud.ScreenEffects.SwitchFadeScreen(time);
        }

        [VisualScriptingMiscData("Notifications", "Sends a scripted chat message under name 'author' to all players (if playerId equal to 0), or to one specific player. In case of singleplayer, message will shown to local player.", -10510688), VisualScriptingMember(true, false)]
        public static void SendChatMessage(string message, string author = "", long playerId = 0L, string font = "Blue")
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static == null)
            {
                MyHud.Chat.multiplayer_ScriptedChatMessageReceived(message, author, font);
            }
            else
            {
                ScriptedChatMsg msg;
                msg.Text = message;
                msg.Author = author;
                msg.Target = playerId;
                msg.Font = font;
                MyMultiplayerBase.SendScriptedChatMessage(ref msg);
            }
        }

        [VisualScriptingMiscData("Gameplay", "Closes active session after the specific time (in ms).", -10510688), VisualScriptingMember(true, false)]
        public static void SessionClose(int fadeTimeMs = 0x2710)
        {
            if (fadeTimeMs < 0)
            {
                fadeTimeMs = 0x2710;
            }
            MyGuiScreenFade fade1 = new MyGuiScreenFade(Color.Black, (uint) fadeTimeMs, 0);
            MyGuiScreenFade screen = new MyGuiScreenFade(Color.Black, (uint) fadeTimeMs, 0);
            screen.Shown += source => MySandboxGame.Static.Invoke(delegate {
                if (MyCampaignManager.Static.IsCampaignRunning)
                {
                    MySession.Static.GetComponent<MyCampaignSessionComponent>().LoadNextCampaignMission();
                }
                else
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
            }, "MyVisualScriptLogicProvider::SessionClose");
            MyHud.MinimalHud = true;
            MyScreenManager.AddScreen(screen);
        }

        [VisualScriptingMiscData("Gameplay", "Displays player the dialog to exit game to main menu (for non-campaign) or continue next campaign mission (for campaign).", -10510688), VisualScriptingMember(true, false)]
        public static void SessionExitGameDialog(string caption, string message)
        {
            if (!m_exitGameDialogOpened)
            {
                m_exitGameDialogOpened = true;
                StringBuilder messageCaption = new StringBuilder(caption);
                MyStringId? cancelButtonText = null;
                cancelButtonText = null;
                cancelButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK, new StringBuilder(message), messageCaption, new MyStringId?(MyCampaignManager.Static.IsCampaignRunning ? MyCommonTexts.ScreenMenuButtonContinue : MyCommonTexts.ScreenMenuButtonExitToMainMenu), cancelButtonText, cancelButtonText, cancelButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                    if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                    {
                        if (MyCampaignManager.Static.IsCampaignRunning)
                        {
                            MySession.Static.GetComponent<MyCampaignSessionComponent>().LoadNextCampaignMission();
                        }
                        else
                        {
                            MySessionLoader.UnloadAndExitToMenu();
                        }
                    }
                    m_exitGameDialogOpened = false;
                }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
            }
        }

        [VisualScriptingMiscData("Gameplay", "Displays reload dialog with specific caption and message to load save defined by path.", -10510688), VisualScriptingMember(true, false)]
        public static void SessionReloadDialog(string caption, string message, string savePath = null)
        {
            StringBuilder messageCaption = new StringBuilder(caption);
            MyStringId? okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            okButtonText = null;
            Vector2? size = null;
            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, new StringBuilder(message), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum result) {
                if (result != MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
                else
                {
                    MyOnlineModeEnum? onlineMode = null;
                    MySessionLoader.LoadSingleplayerSession(savePath ?? MySession.Static.CurrentPath, null, MyCampaignManager.Static.ActiveCampaignName, onlineMode, 0);
                }
            }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
        }

        [VisualScriptingMiscData("Gameplay", "Reloads last checkpoint while displaying message on screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SessionReloadLastCheckpoint(int fadeTimeMs = 0x2710, string message = null, float textScale = 1f, string font = "Blue")
        {
            if (fadeTimeMs < 0)
            {
                fadeTimeMs = 0x2710;
            }
            if (MySession.Static.LocalCharacter != null)
            {
                MySession.Static.LocalCharacter.DeactivateRespawn();
            }
            MyGuiScreenFade screen = new MyGuiScreenFade(Color.Black, (uint) fadeTimeMs, 0);
            screen.Shown += delegate (MyGuiScreenFade fade) {
                MyOnlineModeEnum? onlineMode = null;
                MySessionLoader.LoadSingleplayerSession(MySession.Static.CurrentPath, null, MyCampaignManager.Static.ActiveCampaignName, onlineMode, 0);
                MyHud.MinimalHud = false;
            };
            if (!string.IsNullOrEmpty(message))
            {
                VRageMath.Vector4? backgroundColor = null;
                StringBuilder contents = new StringBuilder(message);
                int? visibleLinesCount = null;
                MyGuiBorderThickness? textPadding = null;
                screen.Controls.Add(new MyGuiControlMultilineText(0.5f, new Vector2(0.6f, 0.3f), backgroundColor, "Red", textScale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, contents, true, true, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, visibleLinesCount, false, false, null, textPadding));
            }
            MyHud.MinimalHud = true;
            MyScreenManager.AddScreen(screen);
        }

        [VisualScriptingMiscData("Questlog", "Sets completed on all quest details for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetAllQuestlogDetailsCompleted(bool completed = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool, long>(s => new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetAllQuestlogDetailsCompletedSync), completed, num, targetEndpoint, position);
        }

        [Event(null, 0x1581), Reliable, Server, Broadcast]
        private static void SetAllQuestlogDetailsCompletedSync(bool completed = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                if (completed)
                {
                    PlayHudSound(MyGuiSounds.HudObjectiveComplete, playerId);
                }
                MyHud.Questlog.SetAllCompleted(completed);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight of specific object for local player. You can set alpha of color too.", -10510688), VisualScriptingMember(true, false)]
        public static void SetAlphaHighlight(string entityName, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), long playerId = -1L, string subPartNames = null, float alpha = 1f)
        {
            Color color2 = color;
            color2.A = (byte) (alpha * 255f);
            SetHighlight(entityName, enabled, thickness, pulseTimeInFrames, color2, playerId, subPartNames);
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight of specific object for all players. You can set alpha of color too.", -10510688), VisualScriptingMember(true, false)]
        public static void SetAlphaHighlightForAll(string entityName, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), string subPartNames = null, float alpha = 1f)
        {
            Color color2 = color;
            color2.A = (byte) (alpha * 255f);
            SetHighlightForAll(entityName, enabled, thickness, pulseTimeInFrames, color2, subPartNames);
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets custom name of specific terminal block.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockCustomName(string blockName, string newName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity) && (entity is MyTerminalBlock))
            {
                (entity as MyTerminalBlock).SetCustomName(newName);
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Enables/disables functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockEnabled(string blockName, bool enabled = true)
        {
            SetBlockState(blockName, enabled);
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets damage multiplier for specific block. (Value above 1 increase damage taken by the block, values in range <0;1> decrease damage taken. )", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockGeneralDamageModifier(string blockName, float modifier = 1f)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity) && (entity is MyCubeBlock))
            {
                ((MyCubeBlock) entity).SlimBlock.BlockGeneralDamageModifier = modifier;
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets block integrity to specific value in range <0;1>. 'damageChange' says if the change is treated as damage or repair (Build integrity won't change in case of damage). 'changeOwner' is id of the one who causes the change.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockHealth(string entityName, float integrity = 1f, bool damageChange = true, long changeOwner = 0L)
        {
            MyCubeBlock entityByName = GetEntityByName(entityName) as MyCubeBlock;
            if (entityByName != null)
            {
                if (damageChange)
                {
                    entityByName.SlimBlock.SetIntegrity(entityByName.SlimBlock.BuildIntegrity, integrity, MyIntegrityChangeEnum.Damage, changeOwner);
                }
                else
                {
                    entityByName.SlimBlock.SetIntegrity(integrity, integrity, MyIntegrityChangeEnum.Repair, changeOwner);
                }
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets whether or not terminal block should be shown in inventory terminal screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockShowInInventory(string blockName, bool showInInventory = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity) && (entity is MyTerminalBlock))
            {
                (entity as MyTerminalBlock).ShowInInventory = showInInventory;
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets whether or not terminal block should be shown in terminal screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockShowInTerminal(string blockName, bool showInTerminal = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity) && (entity is MyTerminalBlock))
            {
                (entity as MyTerminalBlock).ShowInTerminal = showInTerminal;
            }
        }

        [VisualScriptingMiscData("Blocks Generic", "Sets whether or not terminal block should be seen in HUD.", -10510688), VisualScriptingMember(true, false)]
        public static void SetBlockShowOnHUD(string blockName, bool showOnHUD = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity) && (entity is MyTerminalBlock))
            {
                (entity as MyTerminalBlock).ShowOnHUD = showOnHUD;
            }
        }

        private static void SetBlockState(string name, bool state)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(name, out entity) && (entity is MyFunctionalBlock))
            {
                (entity as MyFunctionalBlock).Enabled = state;
            }
        }

        [VisualScriptingMiscData("Gameplay", "[Obsolete, use SetMissionOutcome] Sets the state of campaign. Necessary for transitions between missions in campaign.", -10510688), VisualScriptingMember(true, false)]
        public static void SetCampaignLevelOutcome(string outcome)
        {
            SetMissionOutcome(outcome);
        }

        [VisualScriptingMiscData("Notifications", "[Obsolete] Sets maximum count of messages in chat. [Has no effect anymore as whole history is being kept. Number of shown messages is dependant on number of rows they cover.]", -10510688), VisualScriptingMember(true, false)]
        public static void SetChatMaxMessageCount(int count = 10)
        {
            MyHudChat.MaxMessageCount = count;
        }

        [VisualScriptingMiscData("Notifications", "Sets for how long chat messages should be shown before fading out.", -10510688), VisualScriptingMember(true, false)]
        public static void SetChatMessageDuration(int durationS = 15)
        {
            MyHudChat.MaxMessageTime = durationS * 0x3e8;
        }

        [VisualScriptingMiscData("Misc", "Sets custom image for a loading screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SetCustomLoadingScreenImage(string imagePath)
        {
            MySession.Static.CustomLoadingScreenImage = imagePath;
        }

        [VisualScriptingMiscData("Misc", "Sets custom loading text for a loading screen", -10510688), VisualScriptingMember(true, false)]
        public static void SetCustomLoadingScreenText(string text)
        {
            MySession.Static.CustomLoadingScreenText = text;
        }

        [VisualScriptingMiscData("Misc", "Sets custom skybox for the current game.", -10510688), VisualScriptingMember(true, false)]
        public static void SetCustomSkybox(string skyboxPath)
        {
            MySession.Static.CustomSkybox = skyboxPath;
        }

        [VisualScriptingMiscData("Entity", "Turns dampeners of specific entity on/off.", -10510688), VisualScriptingMember(true, false)]
        public static void SetDampenersEnabled(string entityName, bool state)
        {
            MyEntityThrustComponent thrustComponentByEntityName = GetThrustComponentByEntityName(entityName);
            if (thrustComponentByEntityName != null)
            {
                thrustComponentByEntityName.DampenersEnabled = state;
            }
        }

        public static void SetDisabledByExperimental(this MyGuiControlBase control)
        {
            if (!MySandboxGame.Config.ExperimentalMode)
            {
                control.Enabled = false;
                control.ShowTooltipWhenDisabled = true;
                control.SetToolTip(MyTexts.GetString(MyCommonTexts.ExperimentalRequiredToDisable));
            }
        }

        [VisualScriptingMiscData("AI", "Adds specific drone behavior from preset to a drone. (Extended parameters)", -10510688), VisualScriptingMember(true, false)]
        public static void SetDroneBehaviourAdvanced(string entityName, string presetName = "Default", bool activate = true, bool assignToPirates = true, List<VRage.Game.Entity.MyEntity> waypoints = null, bool cycleWaypoints = false, List<VRage.Game.Entity.MyEntity> targets = null)
        {
            if (!string.IsNullOrEmpty(presetName))
            {
                List<DroneTarget> list = DroneProcessTargets(targets);
                SetDroneBehaviourMethod(entityName, presetName, waypoints, list, activate, assignToPirates, 10, TargetPrioritization.PriorityRandom, 10000f, cycleWaypoints);
            }
        }

        [VisualScriptingMiscData("AI", "Adds specific drone behavior from preset to a drone. (Reduced parameters)", -10510688), VisualScriptingMember(true, false)]
        public static void SetDroneBehaviourBasic(string entityName, string presetName = "Default")
        {
            if (!string.IsNullOrEmpty(presetName))
            {
                SetDroneBehaviourMethod(entityName, presetName, null, null, true, true, 10, TargetPrioritization.PriorityRandom, 10000f, false);
            }
        }

        [VisualScriptingMiscData("AI", "Adds specific drone behavior from preset to a drone. (Full parameters)", -10510688), VisualScriptingMember(true, false)]
        public static void SetDroneBehaviourFull(string entityName, string presetName = "Default", bool activate = true, bool assignToPirates = true, List<VRage.Game.Entity.MyEntity> waypoints = null, bool cycleWaypoints = false, List<VRage.Game.Entity.MyEntity> targets = null, int playerPriority = 10, float maxPlayerDistance = 10000f, TargetPrioritization prioritizationStyle = 2)
        {
            if (!string.IsNullOrEmpty(presetName))
            {
                List<DroneTarget> list = DroneProcessTargets(targets);
                SetDroneBehaviourMethod(entityName, presetName, waypoints, list, activate, assignToPirates, playerPriority, prioritizationStyle, maxPlayerDistance, cycleWaypoints);
            }
        }

        private static void SetDroneBehaviourMethod(string entityName, string presetName, List<VRage.Game.Entity.MyEntity> waypoints, List<DroneTarget> targets, bool activate, bool assignToPirates, int playerPriority, TargetPrioritization prioritizationStyle, float maxPlayerDistance, bool cycleWaypoints)
        {
            MyRemoteControl remoteControl = DroneGetRemote(entityName);
            if (remoteControl != null)
            {
                if (waypoints != null)
                {
                    int index = 0;
                    while (index < waypoints.Count)
                    {
                        if (waypoints[index] == null)
                        {
                            waypoints.RemoveAtFast<VRage.Game.Entity.MyEntity>(index);
                            continue;
                        }
                        index++;
                    }
                }
                if (assignToPirates)
                {
                    remoteControl.CubeGrid.ChangeGridOwnership(GetPirateId(), MyOwnershipShareModeEnum.Faction);
                }
                remoteControl.SetAutomaticBehaviour(new MyDroneAI(remoteControl, presetName, activate, waypoints, targets, playerPriority, prioritizationStyle, maxPlayerDistance, cycleWaypoints));
                if (activate)
                {
                    remoteControl.SetAutoPilotEnabled(true);
                }
            }
        }

        public static void SetEnabledByExperimental(this MyGuiControlBase control)
        {
            if (!MySandboxGame.Config.ExperimentalMode)
            {
                control.Enabled = false;
                control.ShowTooltipWhenDisabled = true;
                control.SetToolTip(MyTexts.GetString(MyCommonTexts.ExperimentalRequired));
            }
        }

        [VisualScriptingMiscData("Entity", "Sets world position of specific entity.", -10510688), VisualScriptingMember(true, false)]
        public static void SetEntityPosition(string entityName, Vector3D position)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName != null)
            {
                entityByName.PositionComp.SetPosition(position, null, false, true);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Sets color of GPS for specific player. If 'playerId' is less or equal to 0, GPS will be modified for local player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGPSColor(string name, Color newColor, long playerId = -1L)
        {
            IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName((playerId > 0L) ? playerId : MySession.Static.LocalPlayerId, name);
            if (gpsByName != null)
            {
                MySession.Static.Gpss.ChangeColor((playerId > 0L) ? playerId : MySession.Static.LocalPlayerId, gpsByName.Hash, newColor);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight for specific entity and creates/deletes GPS attached to it. For local player only.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGPSHighlight(string entityName, string GPSName, string GPSDescription, Color GPSColor, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), long playerId = -1L, string subPartNames = null)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                if (playerId == -1L)
                {
                    playerId = GetLocalPlayerId();
                }
                MyTuple<string, string> tuple1 = new MyTuple<string, string>(entityName, GPSName);
                if (!enabled)
                {
                    IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(playerId, GPSName);
                    if (gpsByName != null)
                    {
                        MySession.Static.Gpss.SendDelete(playerId, gpsByName.Hash);
                    }
                }
                else
                {
                    MyGps gps1 = new MyGps();
                    gps1.ShowOnHud = true;
                    gps1.Name = GPSName;
                    gps1.Description = GPSDescription;
                    gps1.AlwaysVisible = true;
                    gps1.IsObjective = true;
                    MyGps gps = gps1;
                    if (GPSColor != Color.Transparent)
                    {
                        gps.GPSColor = GPSColor;
                    }
                    MySession.Static.Gpss.SendAddGps(playerId, ref gps, entity.EntityId, true);
                }
                SetHighlight(entityName, enabled, thickness, pulseTimeInFrames, color, playerId, subPartNames);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight for specific entity and creates/deletes GPS attached to it. For all players.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGPSHighlightForAll(string entityName, string GPSName, string GPSDescription, Color GPSColor, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), string subPartNames = null)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    SetGPSHighlight(entityName, GPSName, GPSDescription, GPSColor, enabled, thickness, pulseTimeInFrames, color, player.Identity.IdentityId, subPartNames);
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Enables/disables all functional blocks on the specified grid.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridBlocksEnabled(string gridName, bool enabled = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                foreach (MyCubeBlock block in entityByName.GetFatBlocks())
                {
                    if (block is MyFunctionalBlock)
                    {
                        ((MyFunctionalBlock) block).Enabled = enabled;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets all terminal blocks of specified grid to be (not) shown in inventory screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridBlocksShowInInventory(string gridName, bool showInInventory = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                foreach (MyCubeBlock block in entityByName.GetFatBlocks())
                {
                    if (block is MyTerminalBlock)
                    {
                        ((MyTerminalBlock) block).ShowInInventory = showInInventory;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets all terminal blocks of specified grid to be (not) shown in terminal screen.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridBlocksShowInTerminal(string gridName, bool showInTerminal = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                foreach (MyCubeBlock block in entityByName.GetFatBlocks())
                {
                    if (block is MyTerminalBlock)
                    {
                        ((MyTerminalBlock) block).ShowInTerminal = showInTerminal;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets all terminal blocks of specified grid to be (not) shown on HUD.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridBlocksShowOnHUD(string gridName, bool showOnHUD = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                foreach (MyCubeBlock block in entityByName.GetFatBlocks())
                {
                    if (block is MyTerminalBlock)
                    {
                        ((MyTerminalBlock) block).ShowOnHUD = showOnHUD;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Un/Marks the specific grid as destructible.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridDestructible(string entityName, bool destructible = true)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            if (entityByName != null)
            {
                entityByName.DestructibleBlocks = destructible;
            }
        }

        [VisualScriptingMiscData("Grid", "Un/Marks the specific grid as editable.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridEditable(string entityName, bool editable = true)
        {
            MyCubeGrid entityByName = GetEntityByName(entityName) as MyCubeGrid;
            if (entityByName != null)
            {
                entityByName.Editable = editable;
            }
        }

        [VisualScriptingMiscData("Grid", "Sets grid general damage modifier that multiplies all damage received by that grid.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridGeneralDamageModifier(string gridName, float modifier = 1f)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(gridName, out entity) && (entity is MyCubeGrid))
            {
                ((MyCubeGrid) entity).GridGeneralDamageModifier = modifier;
            }
        }

        [VisualScriptingMiscData("Grid", "Sets state of Landing gears for whole grid.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridLandingGearsLock(string gridName, bool gearLock = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                entityByName.GridSystems.LandingSystem.Switch(gearLock);
            }
        }

        [VisualScriptingMiscData("Grid", "Sets grid's power state.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridPowerState(string gridName, bool enabled)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if ((grid != null) && (grid.GridSystems.ResourceDistributor != null))
                {
                    MyMultipleEnabledEnum state = enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
                    foreach (long num in grid.BigOwners)
                    {
                        grid.GridSystems.ResourceDistributor.ChangeSourcesState(MyResourceDistributorComponent.ElectricityId, state, num);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets grid's power state by the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridPowerStateByPlayer(string gridName, bool enabled, long playerId)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(gridName);
            if (entityByName != null)
            {
                MyCubeGrid grid = entityByName as MyCubeGrid;
                if ((grid != null) && (grid.GridSystems.ResourceDistributor != null))
                {
                    MyMultipleEnabledEnum state = enabled ? MyMultipleEnabledEnum.AllEnabled : MyMultipleEnabledEnum.AllDisabled;
                    grid.GridSystems.ResourceDistributor.ChangeSourcesState(MyResourceDistributorComponent.ElectricityId, state, playerId);
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Turns reactors of specific grid on/off.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridReactors(string gridName, bool turnOn = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                long playerId = -1L;
                if ((entityByName.BigOwners != null) && (entityByName.BigOwners.Count > 0))
                {
                    playerId = entityByName.BigOwners[0];
                }
                if (turnOn)
                {
                    entityByName.GridSystems.SyncObject_PowerProducerStateChanged(MyMultipleEnabledEnum.AllEnabled, playerId);
                }
                else
                {
                    entityByName.GridSystems.SyncObject_PowerProducerStateChanged(MyMultipleEnabledEnum.AllDisabled, playerId);
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets the specific grid as static/dynamic.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridStatic(string gridName, bool isStatic = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                if (isStatic)
                {
                    entityByName.RequestConversionToStation();
                }
                else
                {
                    entityByName.RequestConversionToShip(null);
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Enables/disables all weapons(MyUserControllableGun) on the specific grid.", -10510688), VisualScriptingMember(true, false)]
        public static void SetGridWeaponStatus(string gridName, bool enabled = true)
        {
            MyCubeGrid entityByName = GetEntityByName(gridName) as MyCubeGrid;
            if (entityByName != null)
            {
                foreach (MySlimBlock block in entityByName.GetBlocks())
                {
                    if (block.FatBlock is MyUserControllableGun)
                    {
                        ((MyUserControllableGun) block.FatBlock).Enabled = enabled;
                    }
                }
            }
        }

        private static void SetHighlight(MyHighlightSystem.MyHighlightData highlightData, long playerId)
        {
            MyHighlightSystem component = MySession.Static.GetComponent<MyHighlightSystem>();
            bool flag = highlightData.Thickness > -1;
            int exclusiveKey = -1;
            if (m_playerIdsToHighlightData.ContainsKey(playerId))
            {
                exclusiveKey = m_playerIdsToHighlightData[playerId].Find(tuple => tuple.Item1 == highlightData.EntityId).Item2;
                if (exclusiveKey == 0)
                {
                    exclusiveKey = -1;
                }
            }
            if (exclusiveKey != -1)
            {
                if (!flag)
                {
                    m_playerIdsToHighlightData[playerId].RemoveAll(tuple => tuple.Item2 == exclusiveKey);
                }
            }
            else
            {
                if (!flag)
                {
                    return;
                }
                component.ExclusiveHighlightAccepted += new Action<MyHighlightSystem.MyHighlightData, int>(Sandbox.Game.MyVisualScriptLogicProvider.OnExclusiveHighlightAccepted);
                component.ExclusiveHighlightRejected += new Action<MyHighlightSystem.MyHighlightData, int>(Sandbox.Game.MyVisualScriptLogicProvider.OnExclusiveHighlightRejected);
                if (!m_playerIdsToHighlightData.ContainsKey(playerId))
                {
                    m_playerIdsToHighlightData.Add(playerId, new List<MyTuple<long, int>>());
                }
                m_playerIdsToHighlightData[playerId].Add(new MyTuple<long, int>(highlightData.EntityId, -1));
            }
            component.RequestHighlightChangeExclusive(highlightData, exclusiveKey);
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight of specific object for local player.", -10510688), VisualScriptingMember(true, false)]
        public static unsafe void SetHighlight(string entityName, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), long playerId = -1L, string subPartNames = null)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(entityName, out entity))
            {
                MyHighlightSystem.MyHighlightData* dataPtr1;
                MyHighlightSystem.MyHighlightData* dataPtr2;
                Color color2 = new Color();
                if (color == color2)
                {
                    color = DEFAULT_HIGHLIGHT_COLOR;
                }
                if (playerId == -1L)
                {
                    playerId = GetLocalPlayerId();
                }
                MyHighlightSystem.MyHighlightData highlightData = new MyHighlightSystem.MyHighlightData {
                    EntityId = entity.EntityId,
                    OutlineColor = new Color?(color),
                    PulseTimeInFrames = (ulong) pulseTimeInFrames
                };
                dataPtr1->Thickness = enabled ? thickness : -1;
                dataPtr1 = (MyHighlightSystem.MyHighlightData*) ref highlightData;
                highlightData.PlayerId = playerId;
                highlightData.IgnoreUseObjectData = ReferenceEquals(subPartNames, null);
                dataPtr2->SubPartNames = string.IsNullOrEmpty(subPartNames) ? "" : subPartNames;
                dataPtr2 = (MyHighlightSystem.MyHighlightData*) ref highlightData;
                SetHighlight(highlightData, playerId);
            }
        }

        [VisualScriptingMiscData("GPS and Highlights", "Enables/disables highlight of specific object for all players.", -10510688), VisualScriptingMember(true, false)]
        public static void SetHighlightForAll(string entityName, bool enabled = true, int thickness = 10, int pulseTimeInFrames = 120, Color color = new Color(), string subPartNames = null)
        {
            ICollection<MyPlayer> onlinePlayers = MySession.Static.Players.GetOnlinePlayers();
            if ((onlinePlayers != null) && (onlinePlayers.Count != 0))
            {
                foreach (MyPlayer player in onlinePlayers)
                {
                    SetHighlight(entityName, enabled, thickness, pulseTimeInFrames, color, player.Identity.IdentityId, subPartNames);
                }
            }
        }

        [VisualScriptingMiscData("Grid", "Sets projection highlight for the specific projector block.", -10510688), VisualScriptingMember(true, false)]
        public static unsafe void SetHighlightForProjection(string projectorName, bool enabled = true, int thickness = 5, int pulseTimeInFrames = 120, Color color = new Color(), long playerId = -1L)
        {
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(projectorName);
            if ((entityByName != null) && (entityByName is MyProjectorBase))
            {
                MyHighlightSystem.MyHighlightData* dataPtr1;
                Color color2 = new Color();
                if (color == color2)
                {
                    color = Color.Blue;
                }
                color2 = new Color();
                if (color == color2)
                {
                    color = Color.Blue;
                }
                if (playerId == -1L)
                {
                    playerId = MySession.Static.LocalPlayerId;
                }
                MyHighlightSystem.MyHighlightData data2 = new MyHighlightSystem.MyHighlightData {
                    OutlineColor = new Color?(color),
                    PulseTimeInFrames = (ulong) pulseTimeInFrames
                };
                dataPtr1->Thickness = enabled ? thickness : -1;
                dataPtr1 = (MyHighlightSystem.MyHighlightData*) ref data2;
                data2.PlayerId = playerId;
                data2.IgnoreUseObjectData = true;
                MyHighlightSystem.MyHighlightData highlightData = data2;
                using (List<MyCubeGrid>.Enumerator enumerator = ((MyProjectorBase) entityByName).Clipboard.PreviewGrids.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ListReader<MyCubeBlock> fatBlocks = enumerator.Current.GetFatBlocks();
                        foreach (MyCubeBlock block in fatBlocks)
                        {
                            highlightData.EntityId = block.EntityId;
                            SetHighlight(highlightData, highlightData.PlayerId);
                        }
                    }
                }
            }
        }

        [VisualScriptingMiscData("Effects", "Set state of HUD to specific state. 0 - minimal hud.", -10510688), VisualScriptingMember(true, false)]
        public static void SetHudState(int state)
        {
            MyHud.HudState = state;
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets lock state of specific Landing gear.", -10510688), VisualScriptingMember(true, false)]
        public static void SetLandingGearLock(string entityName, bool locked = true)
        {
            IMyLandingGear entityByName = GetEntityByName(entityName) as IMyLandingGear;
            if (entityByName != null)
            {
                entityByName.RequestLock(locked);
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets color of specific Lighting block.", -10510688), VisualScriptingMember(true, false)]
        public static void SetLigtingBlockColor(string lightBlockName, Color color)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(lightBlockName, out entity))
            {
                MyLightingBlock block = entity as MyLightingBlock;
                if (block != null)
                {
                    block.Color = color;
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets intensity of specific Lighting block.", -10510688), VisualScriptingMember(true, false)]
        public static void SetLigtingBlockIntensity(string lightBlockName, float intensity)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(lightBlockName, out entity))
            {
                MyLightingBlock block = entity as MyLightingBlock;
                if (block != null)
                {
                    block.Intensity = intensity;
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets radius of specific Lighting block.", -10510688), VisualScriptingMember(true, false)]
        public static void SetLigtingBlockRadius(string lightBlockName, float radius)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(lightBlockName, out entity))
            {
                MyLightingBlock block = entity as MyLightingBlock;
                if (block != null)
                {
                    block.Radius = radius;
                }
            }
        }

        [VisualScriptingMiscData("Gameplay", "Sets the state of the mission. Necessary for transitions between missions in the scenario.", -10510688), VisualScriptingMember(true, false)]
        public static void SetMissionOutcome(string outcome = "Mission Complete")
        {
            MyCampaignSessionComponent component = MySession.Static.GetComponent<MyCampaignSessionComponent>();
            if (component != null)
            {
                component.CampaignLevelOutcome = outcome;
            }
        }

        [VisualScriptingMiscData("Entity", "Gets name of specific entity defined by id.", -10510688), VisualScriptingMember(true, false)]
        public static void SetName(long entityId, string name)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false) && (GetEntityByName(name) == null))
            {
                entity.Name = name;
                Sandbox.Game.Entities.MyEntities.SetEntityName(entity, true);
            }
        }

        [VisualScriptingMiscData("GUI", "Changes selected page of TabControl element to specific page.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPage(this MyGuiControlTabControl pageControl, int pageNumber)
        {
            pageControl.SelectedPage = pageNumber;
        }

        [VisualScriptingMiscData("Player", "Sets player's damage modifier.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayerGeneralDamageModifier(long playerId = 0L, float modifier = 1f)
        {
            MyCharacter localCharacter = null;
            if (playerId <= 0L)
            {
                localCharacter = MySession.Static.LocalCharacter;
            }
            else
            {
                MyIdentity identity = MySession.Static.Players.TryGetIdentity(playerId);
                if (identity != null)
                {
                    localCharacter = identity.Character;
                }
            }
            if (localCharacter != null)
            {
                localCharacter.CharacterGeneralDamageModifier = modifier;
            }
        }

        [VisualScriptingMiscData("Player", "Sets player's input black list. Enables/Disables specified control of the character.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayerInputBlacklistState(string controlStringId, long playerId = -1L, bool enabled = false)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, long, bool>(s => new Action<string, long, bool>(Sandbox.Game.MyVisualScriptLogicProvider.SetPlayerInputBlacklistStateSync), controlStringId, playerId, enabled, targetEndpoint, position);
        }

        [Event(null, 0x14ef), Reliable, Server, Broadcast]
        private static void SetPlayerInputBlacklistStateSync(string controlStringId, long playerId = -1L, bool enabled = false)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                MyInput.Static.SetControlBlock(MyStringId.GetOrCompute(controlStringId), !enabled);
            }
        }

        [VisualScriptingMiscData("Player", "Sets player's color (HSV).", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersColorInHSV(long playerId = 0L, Vector3 colorHSV = new Vector3())
        {
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            if (characterFromPlayerId != null)
            {
                characterFromPlayerId.ChangeModelAndColor(characterFromPlayerId.ModelName, colorHSV, false, 0L);
            }
        }

        [VisualScriptingMiscData("Player", "Sets player's color (RGB).", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersColorInRGB(long playerId = 0L, Color colorRBG = new Color())
        {
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            if (characterFromPlayerId != null)
            {
                characterFromPlayerId.ChangeModelAndColor(characterFromPlayerId.ModelName, colorRBG.ColorToHSVDX11(), false, 0L);
            }
        }

        [VisualScriptingMiscData("Player", "Sets energy level of the player's suit.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersEnergyLevel(long playerId = 0L, float value = 1f)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                identityFromPlayerId.Character.SuitBattery.ResourceSource.SetRemainingCapacityByType(MyResourceDistributorComponent.ElectricityId, value * 1E-05f);
            }
        }

        [VisualScriptingMiscData("Factions", "Forces join player into a faction specified by tag. Returns false if faction does not exist, true otherwise. If player was in any faction before, he will be removed from that faction.", -10510688), VisualScriptingMember(true, false)]
        public static bool SetPlayersFaction(long playerId = 0L, string factionTag = "")
        {
            if (playerId <= 0L)
            {
                playerId = MySession.Static.LocalPlayerId;
            }
            MyFaction faction = MySession.Static.Factions.TryGetFactionByTag(factionTag, null);
            if (faction == null)
            {
                return false;
            }
            KickPlayerFromFaction(playerId);
            MyFactionCollection.SendJoinRequest(faction.FactionId, playerId);
            return true;
        }

        [VisualScriptingMiscData("Player", "Sets player's health.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersHealth(long playerId = 0L, float value = 100f)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                float num = identityFromPlayerId.Character.StatComp.Health.Value;
                if (value < num)
                {
                    float damage = num - value;
                    identityFromPlayerId.Character.StatComp.DoDamage(damage, new MyDamageInformation(false, damage, DAMAGE_TYPE_SCRIPT, 0L));
                }
                else
                {
                    identityFromPlayerId.Character.StatComp.Health.Value = value;
                }
            }
        }

        [VisualScriptingMiscData("Player", "Sets hydrogen level of the player's suit.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersHydrogenLevel(long playerId = 0L, float value = 1f)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                MyDefinitionId hydrogenId = MyCharacterOxygenComponent.HydrogenId;
                identityFromPlayerId.Character.OxygenComponent.UpdateStoredGasLevel(ref hydrogenId, value);
            }
        }

        [VisualScriptingMiscData("Player", "Sets oxygen level of the player's suit.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersOxygenLevel(long playerId = 0L, float value = 1f)
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                identityFromPlayerId.Character.OxygenComponent.SuitOxygenLevel = value;
            }
        }

        [VisualScriptingMiscData("Player", "Sets player's position.", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersPosition(long playerId = 0L, Vector3D position = new Vector3D())
        {
            MyIdentity identityFromPlayerId = GetIdentityFromPlayerId(playerId);
            if (identityFromPlayerId != null)
            {
                identityFromPlayerId.Character.PositionComp.SetPosition(position, null, false, true);
            }
        }

        [VisualScriptingMiscData("Player", "Sets player's speed (linear velocity).", -10510688), VisualScriptingMember(true, false)]
        public static void SetPlayersSpeed(Vector3D speed = new Vector3D(), long playerId = 0L)
        {
            MyCharacter characterFromPlayerId = GetCharacterFromPlayerId(playerId);
            if (characterFromPlayerId != null)
            {
                if (speed != Vector3D.Zero)
                {
                    float num = Math.Max(characterFromPlayerId.Definition.MaxSprintSpeed, Math.Max(characterFromPlayerId.Definition.MaxRunSpeed, characterFromPlayerId.Definition.MaxBackrunSpeed));
                    float num2 = MyGridPhysics.ShipMaxLinearVelocity() + num;
                    if (speed.LengthSquared() > (num2 * num2))
                    {
                        speed.Normalize();
                        speed *= num2;
                    }
                }
                characterFromPlayerId.Physics.LinearVelocity = (Vector3) speed;
            }
        }

        [VisualScriptingMiscData("Questlog", "Sets title and visibility of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetQuestlog(bool visible = true, string questName = "", long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool, string, long>(s => new Action<bool, string, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogSync), visible, questName, num, targetEndpoint, position);
        }

        [VisualScriptingMiscData("Questlog", "Sets completed of the quest detail for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetQuestlogDetailCompleted(int lineId = 0, bool completed = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, bool, long>(s => new Action<int, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogDetailCompletedSync), lineId, completed, num, targetEndpoint, position);
        }

        [Event(null, 0x156c), Reliable, Server, Broadcast]
        private static void SetQuestlogDetailCompletedSync(int lineId = 0, bool completed = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                if (completed)
                {
                    PlayHudSound(MyGuiSounds.HudObjectiveComplete, playerId);
                }
                MyHud.Questlog.SetCompleted(lineId, completed);
            }
        }

        [VisualScriptingMiscData("Questlog", "Obsolete. Does not do anything.", -10510688), VisualScriptingMember(true, false)]
        public static void SetQuestlogPage(int value = 0, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, long>(s => new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogPageSync), value, num, targetEndpoint, position);
        }

        [Event(null, 0x15b8), Reliable, Server, Broadcast, Obsolete]
        private static void SetQuestlogPageSync(int value = 0, long playerId = -1L)
        {
        }

        [Event(null, 0x1503), Reliable, Server, Broadcast]
        private static void SetQuestlogSync(bool visible = true, string questName = "", long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MySessionComponentIngameHelp component = MySession.Static.GetComponent<MySessionComponentIngameHelp>();
                if (component != null)
                {
                    component.TryCancelObjective();
                }
                if (visible && !MyHud.Questlog.Visible)
                {
                    PlayHudSound(MyGuiSounds.HudGPSNotification3, playerId);
                }
                MyHud.Questlog.QuestTitle = questName;
                MyHud.Questlog.CleanDetails();
                MyHud.Questlog.Visible = visible;
            }
        }

        [VisualScriptingMiscData("Questlog", "Sets title of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetQuestlogTitle(string questName = "", long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, long>(s => new Action<string, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogTitleSync), questName, num, targetEndpoint, position);
        }

        [Event(null, 0x1520), Reliable, Server, Broadcast]
        private static void SetQuestlogTitleSync(string questName = "", long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                MyHud.Questlog.QuestTitle = questName;
            }
        }

        [VisualScriptingMiscData("Questlog", "Sets visible of the quest for the specified player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetQuestlogVisible(bool value = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<bool, long>(s => new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogVisibleSync), value, num, targetEndpoint, position);
        }

        [Event(null, 0x15da), Reliable, Server, Broadcast]
        private static void SetQuestlogVisibleSync(bool value = true, long playerId = -1L)
        {
            if (!Sandbox.Engine.Platform.Game.IsDedicated && ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))))
            {
                if (value && !MyHud.Questlog.Visible)
                {
                    PlayHudSound(MyGuiSounds.HudGPSNotification3, playerId);
                }
                MyHud.Questlog.Visible = value;
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets colors of specific Text panel.", -10510688), VisualScriptingMember(true, false)]
        public static void SetTextPanelColors(string panelName, Color fontColor, Color backgroundColor)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(panelName, out entity))
            {
                MyTextPanel panel = entity as MyTextPanel;
                if (panel != null)
                {
                    if (fontColor != Color.Transparent)
                    {
                        panel.FontColor = fontColor;
                    }
                    if (backgroundColor != Color.Transparent)
                    {
                        panel.BackgroundColor = backgroundColor;
                    }
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets description of specific Text panel.", -10510688), VisualScriptingMember(true, false)]
        public static void SetTextPanelDescription(string panelName, string description, bool publicDescription = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(panelName, out entity))
            {
                MyTextPanel panel = entity as MyTextPanel;
                if (panel != null)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseEvent<MyTextPanel, string, bool>(panel, x => new Action<string, bool>(x.OnChangeDescription), MyStatControlText.SubstituteTexts(description.ToString(), null), publicDescription, targetEndpoint);
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Sets title of specific Text panel.", -10510688), VisualScriptingMember(true, false)]
        public static void SetTextPanelTitle(string panelName, string title, bool publicTitle = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(panelName, out entity))
            {
                MyTextPanel panel = entity as MyTextPanel;
                if (panel != null)
                {
                    if (publicTitle)
                    {
                        panel.PublicDescription = new StringBuilder(title);
                    }
                    else
                    {
                        panel.PrivateDescription = new StringBuilder(title);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Toolbar", "Sets the specified page for the toolbar.", -10510688), VisualScriptingMember(true, false)]
        public static void SetToolbarPage(int page, long playerId = -1L)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, long>(s => new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetToolbarPageSync), page, playerId, targetEndpoint, position);
        }

        [Event(null, 0x1766), Reliable, Server, Broadcast]
        private static void SetToolbarPageSync(int page, long playerId = -1L)
        {
            if (((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))) && ((page >= 0) && (page < MyToolbarComponent.CurrentToolbar.PageCount)))
            {
                MyToolbarComponent.CurrentToolbar.SwitchToPage(page);
            }
        }

        [VisualScriptingMiscData("Toolbar", "Sets item to the specified slot for the player.", -10510688), VisualScriptingMember(true, false)]
        public static void SetToolbarSlotToItem(int slot, MyDefinitionId itemId, long playerId = -1L)
        {
            if (!itemId.TypeId.IsNull)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, MyDefinitionId, long>(s => new Action<int, MyDefinitionId, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetToolbarSlotToItemSync), slot, itemId, playerId, targetEndpoint, position);
            }
        }

        [Event(null, 0x1712), Reliable, Server, Broadcast]
        private static void SetToolbarSlotToItemSync(int slot, MyDefinitionId itemId, long playerId = -1L)
        {
            MyDefinitionBase base2;
            if (((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))) && MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemId, out base2))
            {
                MyToolbarItem item = MyToolbarItemFactory.CreateToolbarItem(MyToolbarItemFactory.ObjectBuilderFromDefinition(base2));
                if (MyToolbarComponent.CurrentToolbar.SelectedSlot != null)
                {
                    int? selectedSlot = MyToolbarComponent.CurrentToolbar.SelectedSlot;
                    int num = slot;
                    if ((selectedSlot.GetValueOrDefault() == num) & (selectedSlot != null))
                    {
                        MyToolbarComponent.CurrentToolbar.Unselect(false);
                    }
                }
                MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, item);
            }
        }

        [VisualScriptingMiscData("GUI", "Sets tooltip of specific GUI element.", -10510688), VisualScriptingMember(true, false)]
        public static void SetTooltip(this MyGuiControlBase control, string text)
        {
            if (control != null)
            {
                control.SetToolTip(text);
            }
        }

        [VisualScriptingMiscData("Effects", "False to force minimize HUD, true to disable force minimization. (Force minimization overrides HUD state without actually changing it so you can revert back safely.)", -10510688), VisualScriptingMember(true, false)]
        public static void ShowHud(bool flag = true)
        {
            MyHud.MinimalHud = !flag;
        }

        [VisualScriptingMiscData("Notifications", "Shows a notification with specific message and font for the specific player for a defined time. If playerId is equal to 0, notification will be show to local player, otherwise it will be shown to specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void ShowNotification(string message, int disappearTimeMs, string font = "White", long playerId = 0L)
        {
            if (playerId == 0)
            {
                if (MyAPIGateway.Utilities != null)
                {
                    MyAPIGateway.Utilities.ShowNotification(message, disappearTimeMs, font);
                }
            }
            else
            {
                MyPlayer.PlayerId id;
                if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                {
                    Vector3D? position = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int, string>(s => new Action<string, int, string>(Sandbox.Game.MyVisualScriptLogicProvider.ShowNotificationSync), message, disappearTimeMs, font, new EndpointId(id.SteamId), position);
                }
            }
        }

        [Event(null, 0x1249), Reliable, Client]
        private static void ShowNotificationSync(string message, int disappearTimeMs, string font = "White")
        {
            if (MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.ShowNotification(message, disappearTimeMs, font);
            }
        }

        [VisualScriptingMiscData("Notifications", "Shows a notification with specific message and font to all players for a defined time.", -10510688), VisualScriptingMember(true, false)]
        public static void ShowNotificationToAll(string message, int disappearTimeMs, string font = "White")
        {
            if (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int, string>(s => new Action<string, int, string>(Sandbox.Game.MyVisualScriptLogicProvider.ShowNotificationToAllSync), message, disappearTimeMs, font, targetEndpoint, position);
            }
            else if (MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.ShowNotification(message, disappearTimeMs, font);
            }
        }

        [Event(null, 0x1250), Reliable, Broadcast, Server]
        private static void ShowNotificationToAllSync(string message, int disappearTimeMs, string font = "White")
        {
            if (MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.ShowNotification(message, disappearTimeMs, font);
            }
        }

        private static void SpawnAlignedToGravityWithOffset(string name, Vector3D position, Vector3D direction, string newGridName, long ownerId = 0L, float gravityOffset = 0f, float gravityRotation = 0f)
        {
            string path = Path.Combine(Path.Combine(MyFileSystem.UserDataPath, "Blueprints", "local"), name, "bp.sbc");
            MyObjectBuilder_ShipBlueprintDefinition[] shipBlueprints = null;
            if (MyFileSystem.FileExists(path))
            {
                MyObjectBuilder_Definitions definitions;
                if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(path, out definitions))
                {
                    return;
                }
                shipBlueprints = definitions.ShipBlueprints;
            }
            if (shipBlueprints != null)
            {
                Vector3D up;
                Vector3 v = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                if (v == Vector3.Zero)
                {
                    v = MyGravityProviderSystem.CalculateArtificialGravityInPoint(position, 1f);
                }
                if (!(v != Vector3.Zero))
                {
                    if (!(direction == Vector3D.Zero))
                    {
                        up = Vector3D.CalculatePerpendicularVector(-direction);
                    }
                    else
                    {
                        direction = Vector3D.Right;
                        up = Vector3D.Up;
                    }
                }
                else
                {
                    v.Normalize();
                    up = -v;
                    position += v * gravityOffset;
                    if (direction == Vector3D.Zero)
                    {
                        direction = Vector3D.CalculatePerpendicularVector(v);
                        if (gravityRotation != 0f)
                        {
                            MatrixD matrix = MatrixD.CreateFromAxisAngle(up, (double) gravityRotation);
                            Vector3D vectord1 = Vector3D.Transform(direction, matrix);
                            direction = vectord1;
                        }
                    }
                }
                List<MyObjectBuilder_CubeGrid> list = new List<MyObjectBuilder_CubeGrid>();
                MyObjectBuilder_ShipBlueprintDefinition[] definitionArray2 = shipBlueprints;
                int index = 0;
                while (index < definitionArray2.Length)
                {
                    MyObjectBuilder_CubeGrid[] cubeGrids = definitionArray2[index].CubeGrids;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= cubeGrids.Length)
                        {
                            index++;
                            break;
                        }
                        MyObjectBuilder_CubeGrid item = cubeGrids[num2];
                        item.CreatePhysics = true;
                        item.EnableSmallToLargeConnections = true;
                        item.PositionAndOrientation = new MyPositionAndOrientation(position, (Vector3) direction, (Vector3) up);
                        MyPositionAndOrientation orientation = item.PositionAndOrientation.Value;
                        orientation.Orientation.Normalize();
                        if (!string.IsNullOrEmpty(newGridName))
                        {
                            item.Name = newGridName;
                        }
                        list.Add(item);
                        num2++;
                    }
                }
                if (!Sandbox.Engine.Platform.Game.IsDedicated)
                {
                    MyHud.PushRotatingWheelVisible();
                }
                MyCubeGrid.RelativeOffset offset = new MyCubeGrid.RelativeOffset {
                    Use = false,
                    RelativeToEntity = false,
                    SpawnerId = 0L,
                    OriginalSpawnPoint = Vector3D.Zero
                };
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? nullable = null;
                Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(s => new Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(MyCubeGrid.TryPasteGrid_Implementation), list, false, Vector3.Zero, false, true, offset, targetEndpoint, nullable);
            }
        }

        [VisualScriptingMiscData("Spawn", "Spawns the bot at the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnBot(string subtypeName, Vector3D position)
        {
            MyBotDefinition definition;
            MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_AnimalBot), subtypeName);
            if (MyDefinitionManager.Static.TryGetBotDefinition(id, out definition) && (definition != null))
            {
                MyAIComponent.Static.SpawnNewBot(definition as MyAgentDefinition, position, false);
            }
        }

        [VisualScriptingMiscData("Spawn", "Spawns the group of prefabs at the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnGroup(string subtypeId, Vector3D position, Vector3D direction, Vector3D up, long ownerId = 0L, string newGridName = null)
        {
            ListReader<MySpawnGroupDefinition> spawnGroupDefinitions = MyDefinitionManager.Static.GetSpawnGroupDefinitions();
            MySpawnGroupDefinition definition = null;
            foreach (MySpawnGroupDefinition definition2 in spawnGroupDefinitions)
            {
                if (definition2.Id.SubtypeName == subtypeId)
                {
                    definition = definition2;
                    break;
                }
            }
            if (definition != null)
            {
                List<MyCubeGrid> tmpGridList = new List<MyCubeGrid>();
                direction.Normalize();
                up.Normalize();
                MatrixD matrix = MatrixD.CreateWorld(position, direction, up);
                Stack<Action> callbacks = new Stack<Action>();
                callbacks.Push(delegate {
                    if ((newGridName != null) && (tmpGridList.Count > 0))
                    {
                        tmpGridList[0].Name = newGridName;
                        Sandbox.Game.Entities.MyEntities.SetEntityName(tmpGridList[0], true);
                    }
                });
                foreach (MySpawnGroupDefinition.SpawnGroupPrefab prefab in definition.Prefabs)
                {
                    Vector3D vectord = Vector3D.Transform((Vector3D) prefab.Position, matrix);
                    Vector3 initialAngularVelocity = new Vector3();
                    MyPrefabManager.Static.SpawnPrefab(tmpGridList, prefab.SubtypeId, vectord, (Vector3) direction, (Vector3) up, (Vector3) (prefab.Speed * direction), initialAngularVelocity, prefab.BeaconText, null, SpawningOptions.RotateFirstCockpitTowardsDirection, ownerId, true, callbacks);
                }
            }
        }

        [VisualScriptingMiscData("Spawn", "Spawns the item at the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnItem(MyDefinitionId itemId, Vector3D position, string inheritsVelocityFrom = "", float amount = 1f)
        {
            MyFixedPoint point = (MyFixedPoint) amount;
            MyObjectBuilder_PhysicalObject content = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) itemId);
            if (content != null)
            {
                VRage.Game.Entity.MyEntity entity;
                MyPhysicsComponentBase component = null;
                if (!string.IsNullOrEmpty(inheritsVelocityFrom) && Sandbox.Game.Entities.MyEntities.TryGetEntityByName(inheritsVelocityFrom, out entity))
                {
                    entity.Components.TryGet<MyPhysicsComponentBase>(out component);
                }
                Vector3D forward = Vector3D.Forward;
                Vector3D up = Vector3D.Up;
                Vector3D vectord3 = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
                if (vectord3 != Vector3D.Zero)
                {
                    up = Vector3D.Normalize(vectord3) * -1.0;
                    forward = (up != Vector3D.Right) ? Vector3D.Cross(up, Vector3D.Right) : Vector3D.Forward;
                }
                MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(point, content, 1f), position, forward, up, component, null);
            }
        }

        [VisualScriptingMiscData("Spawn", "Spawns local blueprint at the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnLocalBlueprint(string name, Vector3D position, Vector3D direction = new Vector3D(), string newGridName = null, long ownerId = 0L)
        {
            SpawnAlignedToGravityWithOffset(name, position, direction, newGridName, ownerId, 0f, 0f);
        }

        [VisualScriptingMiscData("Spawn", "Spawns local blueprint at the specified position and aligned to gravity.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnLocalBlueprintInGravity(string name, Vector3D position, float rotationAngle = 0f, float gravityOffset = 0f, string newGridName = null, long ownerId = 0L)
        {
            Vector3D direction = new Vector3D();
            SpawnAlignedToGravityWithOffset(name, position, direction, newGridName, ownerId, gravityOffset, rotationAngle);
        }

        [VisualScriptingMiscData("Player", "Spawns player on the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnPlayer(MatrixD worldMatrix, Vector3D velocity = new Vector3D(), long playerId = 0L)
        {
            MyPlayer.PlayerId id;
            if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
            {
                MyPlayer playerById = MySession.Static.Players.GetPlayerById(id);
                if (playerById != null)
                {
                    if ((playerById.Character != null) && !playerById.Character.IsDead)
                    {
                        playerById.Character.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                    }
                    else
                    {
                        Color? color = null;
                        playerById.SpawnAt(worldMatrix, (Vector3) velocity, null, null, true, null, color);
                        Vector3D? position = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent(s => new Action(Sandbox.Game.MyVisualScriptLogicProvider.CloseRespawnScreen), new EndpointId(playerById.Id.SteamId), position);
                    }
                }
            }
        }

        [VisualScriptingMiscData("Spawn", "Spawns the prefab at the specified position.", -10510688), VisualScriptingMember(true, false)]
        public static void SpawnPrefab(string prefabName, Vector3D position, Vector3D direction, Vector3D up, long ownerId = 0L, string beaconName = null, string entityName = null)
        {
            if (MyPrefabManager.Static == null)
            {
                MyLog.Default.WriteLine("Spawn Prefab failed. Prefab manager is not initialized.");
            }
            else
            {
                direction.Normalize();
                up.Normalize();
                Vector3 initialLinearVelocity = new Vector3();
                initialLinearVelocity = new Vector3();
                MyPrefabManager.Static.SpawnPrefab(prefabName, position, (Vector3) direction, (Vector3) up, initialLinearVelocity, initialLinearVelocity, beaconName, entityName, SpawningOptions.RotateFirstCockpitTowardsDirection, ownerId, true, null);
            }
        }

        [VisualScriptingMiscData("Cutscenes", "Starts specific cutscene. If 'playerId' is -1, apply for all players, otherwise only for specific player.", -10510688), VisualScriptingMember(true, false)]
        public static void StartCutscene(string cutsceneName, bool registerEvents = true, long playerId = -1L)
        {
            long playerIdentityId;
            if ((playerId != 0) || (MySession.Static.LocalCharacter == null))
            {
                playerIdentityId = playerId;
            }
            else
            {
                playerIdentityId = MySession.Static.LocalCharacter.GetPlayerIdentityId();
            }
            long num = playerIdentityId;
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, bool, long>(x => new Action<string, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.StartCutsceneSync), cutsceneName, registerEvents, num, targetEndpoint, position);
        }

        [Event(null, 0x7ce), Reliable, Server, Broadcast]
        private static void StartCutsceneSync(string cutsceneName, bool registerEvents = true, long playerId = -1L)
        {
            if ((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId)))
            {
                if (((playerId == -1L) && (Sandbox.Engine.Multiplayer.MyMultiplayer.Static != null)) && !Sandbox.Engine.Multiplayer.MyMultiplayer.Static.IsServer)
                {
                    registerEvents = false;
                }
                MySession.Static.GetComponent<MySessionComponentCutscenes>().PlayCutscene(cutsceneName, registerEvents, "");
            }
        }

        [VisualScriptingMiscData("State Machines", "Starts the specified state machine.", -10510688), VisualScriptingMember(true, false)]
        public static void StartStateMachine(string stateMachineName, long ownerId = 0L)
        {
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            if (component != null)
            {
                component.SMManager.Run(stateMachineName, ownerId);
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Calls 'Start' action on specific functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void StartTimerBlock(string blockName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity))
            {
                Sandbox.ModAPI.IMyFunctionalBlock block = entity as Sandbox.ModAPI.IMyFunctionalBlock;
                if (block != null)
                {
                    block.ApplyAction("Start");
                }
            }
        }

        [VisualScriptingMiscData("Audio", "Stops sound played by specific emitter.", -10510688), VisualScriptingMember(true, false)]
        public static void StopSound(string EmitterId, bool forced = false)
        {
            if ((MyAudio.Static != null) && (EmitterId.Length > 0))
            {
                MyEntity3DSoundEmitter libraryEmitter = MyAudioComponent.GetLibraryEmitter(EmitterId);
                if (libraryEmitter != null)
                {
                    libraryEmitter.StopSound(forced, true);
                }
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Calls 'Stop' action on specific functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void StopTimerBlock(string blockName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity))
            {
                Sandbox.ModAPI.IMyFunctionalBlock block = entity as Sandbox.ModAPI.IMyFunctionalBlock;
                if (block != null)
                {
                    block.ApplyAction("Stop");
                }
            }
        }

        [VisualScriptingMiscData("Environment", "Enables/disable sun rotation.", -10510688), VisualScriptingMember(true, false)]
        public static void SunRotationEnabled(bool enabled)
        {
            MySession.Static.GetComponent<MySectorWeatherComponent>().Enabled = enabled;
        }

        [VisualScriptingMiscData("Environment", "Gets current time of day.", -10510688), VisualScriptingMember(false, false)]
        public static float SunRotationGetCurrentTime() => 
            MyTimeOfDayHelper.TimeOfDay;

        [VisualScriptingMiscData("Environment", "Gets length of day.", -10510688), VisualScriptingMember(false, false)]
        public static float SunRotationGetDayLength() => 
            MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval;

        [VisualScriptingMiscData("Environment", "Sets length of day.", -10510688), VisualScriptingMember(true, false)]
        public static void SunRotationSetDayLength(float length)
        {
            MySession.Static.GetComponent<MySectorWeatherComponent>().RotationInterval = length;
        }

        [VisualScriptingMiscData("Environment", "Sets time of day.", -10510688), VisualScriptingMember(true, false)]
        public static void SunRotationSetTime(float time)
        {
            MyTimeOfDayHelper.UpdateTimeOfDay(time);
        }

        [VisualScriptingMiscData("Toolbar", "Switches the specified toolbar slot for the player.", -10510688), VisualScriptingMember(true, false)]
        public static void SwitchToolbarToSlot(int slot, long playerId = -1L)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int, long>(s => new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SwitchToolbarToSlotSync), slot, playerId, targetEndpoint, position);
        }

        [Event(null, 0x1729), Reliable, Server, Broadcast]
        private static void SwitchToolbarToSlotSync(int slot, long playerId = -1L)
        {
            if (((playerId == -1L) || ((MySession.Static.LocalCharacter != null) && (MySession.Static.LocalCharacter.GetPlayerIdentityId() == playerId))) && ((slot >= 0) && (slot < MyToolbarComponent.CurrentToolbar.SlotCount)))
            {
                if ((MyToolbarComponent.CurrentToolbar.SelectedSlot != null) && (MyToolbarComponent.CurrentToolbar.SelectedSlot.Value == slot))
                {
                    MyToolbarComponent.CurrentToolbar.Unselect(false);
                }
                MyToolbarComponent.CurrentToolbar.ActivateItemAtSlot(slot, false, true, true);
            }
        }

        [VisualScriptingMiscData("Misc", "Takes a screenshot and saves it to specific destination.", -10510688), VisualScriptingMember(true, false)]
        public static void TakeScreenshot(string destination, string name)
        {
            string pathToSave = Path.Combine(destination, name, ".png");
            MyRenderProxy.TakeScreenshot(new Vector2(0.5f, 0.5f), pathToSave, false, true, false);
            MyRenderProxy.UnloadTexture(pathToSave);
        }

        [VisualScriptingMiscData("AI", "Sets whitelist targeting mode. If true, entities in whitelist will be considered a target, if false, entities not in whitelist will be considered a target.", -10510688), VisualScriptingMember(true, false)]
        public static void TargetingSetWhitelist(string gridName, bool whitelistMode = true)
        {
            MyCubeGrid grid;
            if (TryGetGrid(gridName, out grid))
            {
                grid.TargetingSetWhitelist(whitelistMode);
            }
        }

        [VisualScriptingMiscData("G-Screen", "Sets group mode of toolbar config screen (G-screen) to Default.", -10510688), VisualScriptingMember(true, false)]
        public static void ToolbarConfigGroupsDefualtBehavior()
        {
            MyGuiScreenToolbarConfigBase.GroupMode = MyGuiScreenToolbarConfigBase.GroupModes.Default;
        }

        [VisualScriptingMiscData("G-Screen", "Sets group mode of toolbar config screen (G-screen) to Hide all.", -10510688), VisualScriptingMember(true, false)]
        public static void ToolbarConfigGroupsHideAll()
        {
            MyGuiScreenToolbarConfigBase.GroupMode = MyGuiScreenToolbarConfigBase.GroupModes.HideAll;
        }

        [VisualScriptingMiscData("G-Screen", "Sets group mode of toolbar config screen (G-screen) to Hide block groups.", -10510688), VisualScriptingMember(true, false)]
        public static void ToolbarConfigGroupsHideBlockGroups()
        {
            MyGuiScreenToolbarConfigBase.GroupMode = MyGuiScreenToolbarConfigBase.GroupModes.HideBlockGroups;
        }

        [VisualScriptingMiscData("G-Screen", "Sets group mode of toolbar config screen (G-screen) to Hide empty groups.", -10510688), VisualScriptingMember(true, false)]
        public static void ToolbarConfigGroupsHideEmpty()
        {
            MyGuiScreenToolbarConfigBase.GroupMode = MyGuiScreenToolbarConfigBase.GroupModes.HideEmpty;
        }

        [VisualScriptingMiscData("Blocks Specific", "Calls 'TriggerNow' action on specific functional block.", -10510688), VisualScriptingMember(true, false)]
        public static void TriggerTimerBlock(string blockName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(blockName, out entity))
            {
                Sandbox.ModAPI.IMyFunctionalBlock block = entity as Sandbox.ModAPI.IMyFunctionalBlock;
                if (block != null)
                {
                    block.ApplyAction("TriggerNow");
                }
            }
        }

        private static bool TryGetGrid(string entityName, out MyCubeGrid grid)
        {
            grid = null;
            VRage.Game.Entity.MyEntity entityByName = GetEntityByName(entityName);
            if (entityByName == null)
            {
                return false;
            }
            if (entityByName is MyCubeGrid)
            {
                grid = (MyCubeGrid) entityByName;
                return true;
            }
            if (!(entityByName is MyCubeBlock))
            {
                return false;
            }
            grid = ((MyCubeBlock) entityByName).CubeGrid;
            return true;
        }

        private static bool TryGetGrid(VRage.Game.Entity.MyEntity entity, out MyCubeGrid grid)
        {
            if (entity is MyCubeGrid)
            {
                grid = (MyCubeGrid) entity;
                return true;
            }
            if (entity is MyCubeBlock)
            {
                grid = ((MyCubeBlock) entity).CubeGrid;
                return true;
            }
            grid = null;
            return false;
        }

        private static void UnlockAchievement(int achievementId)
        {
            if ((achievementId >= 0) && (achievementId < AllowedAchievementsHelper.AllowedAchievements.Count))
            {
                MyGameService.SetAchievement(AllowedAchievementsHelper.AllowedAchievements[achievementId]);
            }
        }

        [VisualScriptingMiscData("Achievements", "Award player achievement. Id ID is -1, unlock to all, if ID is 0, unlock to local player, if anything else, it unlocks to player with that ID", -10510688), VisualScriptingMember(true, false)]
        public static void UnlockAchievementById(int achievementId, long playerId)
        {
            if (playerId == 0)
            {
                UnlockAchievementInternal(achievementId);
            }
            else
            {
                Vector3D? nullable;
                if (playerId == -1L)
                {
                    EndpointId targetEndpoint = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.UnlockAchievementInternalAll), achievementId, targetEndpoint, nullable);
                }
                else
                {
                    MyPlayer.PlayerId id;
                    if (MySession.Static.Players.TryGetPlayerId(playerId, out id))
                    {
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<int>(s => new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.UnlockAchievementInternal), achievementId, new EndpointId(id.SteamId), nullable);
                    }
                }
            }
        }

        [Event(null, 0x17f8), Reliable, ServerInvoked]
        private static void UnlockAchievementInternal(int achievementId)
        {
            UnlockAchievement(achievementId);
        }

        [Event(null, 0x17fe), Reliable, ServerInvoked, Broadcast]
        private static void UnlockAchievementInternalAll(int achievementId)
        {
            UnlockAchievement(achievementId);
        }

        [VisualScriptingMiscData("Blocks Specific", "Turns on/off shooting for specific weapon block (UserControllableGun)", -10510688), VisualScriptingMember(true, false)]
        public static void WeaponSetShooting(string weaponName, bool shooting = true)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(weaponName, out entity) && (entity is MyUserControllableGun))
            {
                (entity as MyUserControllableGun).SetShooting(shooting);
            }
        }

        [VisualScriptingMiscData("Blocks Specific", "Orders specific weapon block (UserControllableGun) to shoot once.", -10510688), VisualScriptingMember(true, false)]
        public static void WeaponShootOnce(string weaponName)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityByName(weaponName, out entity) && (entity is MyUserControllableGun))
            {
                (entity as MyUserControllableGun).ShootFromTerminal((Vector3) entity.WorldMatrix.Forward);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Sandbox.Game.MyVisualScriptLogicProvider.<>c <>9 = new Sandbox.Game.MyVisualScriptLogicProvider.<>c();
            public static Action<MyCubeGrid, MySlimBlock> <>9__48_0;
            public static Action <>9__48_1;
            public static Action<VRage.Game.Entity.MyEntity> <>9__48_2;
            public static Action<MyGuiScreenBase> <>9__48_3;
            public static Action<MyGuiScreenBase> <>9__48_4;
            public static Action<MyPlayer> <>9__48_5;
            public static Func<MyTextPanel, Action<string, bool>> <>9__138_0;
            public static Func<IMyEventOwner, Action<string, bool, long>> <>9__148_0;
            public static Func<IMyEventOwner, Action<long>> <>9__150_0;
            public static Func<IMyEventOwner, Action<long>> <>9__152_0;
            public static Action <>9__216_1;
            public static Action<MyGuiScreenFade> <>9__216_0;
            public static Action<MyGuiScreenFade> <>9__217_0;
            public static Action<MyGuiScreenMessageBox.ResultEnum> <>9__219_0;
            public static Func<IMyEventOwner, Action<string>> <>9__298_0;
            public static Func<IMyEventOwner, Action<string, int, string>> <>9__323_0;
            public static Func<IMyEventOwner, Action<string, int, string>> <>9__324_0;
            public static Func<IMyEventOwner, Action<MyStringId, string, int>> <>9__330_0;
            public static Func<IMyEventOwner, Action<int, long>> <>9__332_0;
            public static Func<IMyEventOwner, Action<long>> <>9__334_0;
            public static Func<IMyEventOwner, Action<int>> <>9__336_0;
            public static Func<IMyEventOwner, Action<int>> <>9__336_1;
            public static Func<IMyEventOwner, Action> <>9__374_0;
            public static Func<IMyEventOwner, Action<string, long, bool>> <>9__376_0;
            public static Func<IMyEventOwner, Action<bool, string, long>> <>9__378_0;
            public static Func<IMyEventOwner, Action<string, long>> <>9__380_0;
            public static Func<IMyEventOwner, Action<string, bool, bool, long>> <>9__382_0;
            public static Func<IMyEventOwner, Action<string, bool, bool, long>> <>9__384_0;
            public static Func<IMyEventOwner, Action<int, bool, long>> <>9__386_0;
            public static Func<IMyEventOwner, Action<bool, long>> <>9__388_0;
            public static Func<IMyEventOwner, Action<int, string, bool, long>> <>9__390_0;
            public static Func<IMyEventOwner, Action<long>> <>9__392_0;
            public static Func<IMyEventOwner, Action<int, long>> <>9__394_0;
            public static Func<IMyEventOwner, Action<bool, long>> <>9__398_0;
            public static Func<IMyEventOwner, Action<bool, long>> <>9__401_0;
            public static Func<IMyEventOwner, Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>> <>9__408_0;
            public static Func<IMyEventOwner, Action<int, MyDefinitionId, long>> <>9__412_0;
            public static Func<IMyEventOwner, Action<int, long>> <>9__414_0;
            public static Func<IMyEventOwner, Action<int, long>> <>9__416_0;
            public static Func<IMyEventOwner, Action<long>> <>9__418_0;
            public static Func<IMyEventOwner, Action<int, long>> <>9__420_0;
            public static Func<IMyEventOwner, Action<long>> <>9__422_0;
            public static Func<IMyEventOwner, Action<int>> <>9__429_0;
            public static Func<IMyEventOwner, Action<int>> <>9__429_1;

            internal Action<MyStringId, string, int> <AddNotification>b__330_0(IMyEventOwner s) => 
                new Action<MyStringId, string, int>(Sandbox.Game.MyVisualScriptLogicProvider.AddNotificationSync);

            internal Action<string, bool, bool, long> <AddQuestlogDetail>b__382_0(IMyEventOwner s) => 
                new Action<string, bool, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.AddQuestlogDetailSync);

            internal Action<string, bool, bool, long> <AddQuestlogObjective>b__384_0(IMyEventOwner s) => 
                new Action<string, bool, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.AddQuestlogObjectiveSync);

            internal Action<long> <ClearAllToolbarSlots>b__418_0(IMyEventOwner s) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearAllToolbarSlotsSync);

            internal Action<long> <ClearNotifications>b__334_0(IMyEventOwner s) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearNotificationSync);

            internal Action<int, long> <ClearToolbarSlot>b__416_0(IMyEventOwner s) => 
                new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.ClearToolbarSlotSync);

            internal Action<int> <DisplayCongratulationScreen>b__336_0(IMyEventOwner s) => 
                new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.DisplayCongratulationScreenInternalAll);

            internal Action<int> <DisplayCongratulationScreen>b__336_1(IMyEventOwner s) => 
                new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.DisplayCongratulationScreenInternal);

            internal Action<bool, long> <EnableHighlight>b__401_0(IMyEventOwner s) => 
                new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.EnableHighlightSync);

            internal Action<long> <EndCutscene>b__152_0(IMyEventOwner x) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.EndCutsceneSync);

            internal void <Init>b__48_0(MyCubeGrid grid, MySlimBlock block)
            {
                if (Sandbox.Game.MyVisualScriptLogicProvider.BlockBuilt != null)
                {
                    Sandbox.Game.MyVisualScriptLogicProvider.BlockBuilt(block.BlockDefinition.Id.TypeId.ToString(), block.BlockDefinition.Id.SubtypeName, grid.Name, (block.FatBlock != null) ? block.FatBlock.EntityId : 0L);
                }
            }

            internal void <Init>b__48_1()
            {
                Sandbox.Game.MyVisualScriptLogicProvider.m_addedNotificationsById.Clear();
                Sandbox.Game.MyVisualScriptLogicProvider.m_playerIdsToHighlightData.Clear();
            }

            internal void <Init>b__48_2(VRage.Game.Entity.MyEntity entity)
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                if (((grid != null) && (Sandbox.Game.MyVisualScriptLogicProvider.BlockBuilt != null)) && (grid.BlocksCount == 1))
                {
                    MySlimBlock cubeBlock = grid.GetCubeBlock(Vector3I.Zero);
                    if (cubeBlock != null)
                    {
                        Sandbox.Game.MyVisualScriptLogicProvider.BlockBuilt(cubeBlock.BlockDefinition.Id.TypeId.ToString(), cubeBlock.BlockDefinition.Id.SubtypeName, grid.Name, (cubeBlock.FatBlock != null) ? cubeBlock.FatBlock.EntityId : 0L);
                    }
                }
            }

            internal void <Init>b__48_3(MyGuiScreenBase screen)
            {
                if (Sandbox.Game.MyVisualScriptLogicProvider.ScreenRemoved != null)
                {
                    Sandbox.Game.MyVisualScriptLogicProvider.ScreenRemoved(screen);
                }
            }

            internal void <Init>b__48_4(MyGuiScreenBase screen)
            {
                if (Sandbox.Game.MyVisualScriptLogicProvider.ScreenAdded != null)
                {
                    Sandbox.Game.MyVisualScriptLogicProvider.ScreenAdded(screen);
                }
            }

            internal void <Init>b__48_5(MyPlayer player)
            {
                if (Sandbox.Game.MyVisualScriptLogicProvider.PlayerRespawnRequest != null)
                {
                    Sandbox.Game.MyVisualScriptLogicProvider.PlayerRespawnRequest(player.Identity.IdentityId);
                }
            }

            internal Action<long> <NextCutsceneNode>b__150_0(IMyEventOwner x) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.NextCutsceneNodeSync);

            internal Action<string> <OpenSteamOverlay>b__298_0(IMyEventOwner s) => 
                new Action<string>(Sandbox.Game.MyVisualScriptLogicProvider.OpenSteamOverlaySync);

            internal Action<long> <ReloadToolbarDefaults>b__422_0(IMyEventOwner s) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.ReloadToolbarDefaultsSync);

            internal Action<int, long> <RemoveNotification>b__332_0(IMyEventOwner s) => 
                new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.RemoveNotificationSync);

            internal Action<long> <RemoveQuestlogDetails>b__392_0(IMyEventOwner s) => 
                new Action<long>(Sandbox.Game.MyVisualScriptLogicProvider.RemoveQuestlogDetailsSync);

            internal Action<int, string, bool, long> <ReplaceQuestlogDetail>b__390_0(IMyEventOwner s) => 
                new Action<int, string, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.ReplaceQuestlogDetailSync);

            internal void <SessionClose>b__216_0(MyGuiScreenFade source)
            {
                MySandboxGame.Static.Invoke(delegate {
                    if (MyCampaignManager.Static.IsCampaignRunning)
                    {
                        MySession.Static.GetComponent<MyCampaignSessionComponent>().LoadNextCampaignMission();
                    }
                    else
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                    }
                }, "MyVisualScriptLogicProvider::SessionClose");
            }

            internal void <SessionClose>b__216_1()
            {
                if (MyCampaignManager.Static.IsCampaignRunning)
                {
                    MySession.Static.GetComponent<MyCampaignSessionComponent>().LoadNextCampaignMission();
                }
                else
                {
                    MySessionLoader.UnloadAndExitToMenu();
                }
            }

            internal void <SessionExitGameDialog>b__219_0(MyGuiScreenMessageBox.ResultEnum result)
            {
                if (result == MyGuiScreenMessageBox.ResultEnum.YES)
                {
                    if (MyCampaignManager.Static.IsCampaignRunning)
                    {
                        MySession.Static.GetComponent<MyCampaignSessionComponent>().LoadNextCampaignMission();
                    }
                    else
                    {
                        MySessionLoader.UnloadAndExitToMenu();
                    }
                }
                Sandbox.Game.MyVisualScriptLogicProvider.m_exitGameDialogOpened = false;
            }

            internal void <SessionReloadLastCheckpoint>b__217_0(MyGuiScreenFade fade)
            {
                MyOnlineModeEnum? onlineMode = null;
                MySessionLoader.LoadSingleplayerSession(MySession.Static.CurrentPath, null, MyCampaignManager.Static.ActiveCampaignName, onlineMode, 0);
                MyHud.MinimalHud = false;
            }

            internal Action<bool, long> <SetAllQuestlogDetailsCompleted>b__388_0(IMyEventOwner s) => 
                new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetAllQuestlogDetailsCompletedSync);

            internal Action<string, long, bool> <SetPlayerInputBlacklistState>b__376_0(IMyEventOwner s) => 
                new Action<string, long, bool>(Sandbox.Game.MyVisualScriptLogicProvider.SetPlayerInputBlacklistStateSync);

            internal Action<bool, string, long> <SetQuestlog>b__378_0(IMyEventOwner s) => 
                new Action<bool, string, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogSync);

            internal Action<int, bool, long> <SetQuestlogDetailCompleted>b__386_0(IMyEventOwner s) => 
                new Action<int, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogDetailCompletedSync);

            internal Action<int, long> <SetQuestlogPage>b__394_0(IMyEventOwner s) => 
                new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogPageSync);

            internal Action<string, long> <SetQuestlogTitle>b__380_0(IMyEventOwner s) => 
                new Action<string, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogTitleSync);

            internal Action<bool, long> <SetQuestlogVisible>b__398_0(IMyEventOwner s) => 
                new Action<bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetQuestlogVisibleSync);

            internal Action<string, bool> <SetTextPanelDescription>b__138_0(MyTextPanel x) => 
                new Action<string, bool>(x.OnChangeDescription);

            internal Action<int, long> <SetToolbarPage>b__420_0(IMyEventOwner s) => 
                new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetToolbarPageSync);

            internal Action<int, MyDefinitionId, long> <SetToolbarSlotToItem>b__412_0(IMyEventOwner s) => 
                new Action<int, MyDefinitionId, long>(Sandbox.Game.MyVisualScriptLogicProvider.SetToolbarSlotToItemSync);

            internal Action<string, int, string> <ShowNotification>b__323_0(IMyEventOwner s) => 
                new Action<string, int, string>(Sandbox.Game.MyVisualScriptLogicProvider.ShowNotificationSync);

            internal Action<string, int, string> <ShowNotificationToAll>b__324_0(IMyEventOwner s) => 
                new Action<string, int, string>(Sandbox.Game.MyVisualScriptLogicProvider.ShowNotificationToAllSync);

            internal Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset> <SpawnAlignedToGravityWithOffset>b__408_0(IMyEventOwner s) => 
                new Action<List<MyObjectBuilder_CubeGrid>, bool, Vector3, bool, bool, MyCubeGrid.RelativeOffset>(MyCubeGrid.TryPasteGrid_Implementation);

            internal Action <SpawnPlayer>b__374_0(IMyEventOwner s) => 
                new Action(Sandbox.Game.MyVisualScriptLogicProvider.CloseRespawnScreen);

            internal Action<string, bool, long> <StartCutscene>b__148_0(IMyEventOwner x) => 
                new Action<string, bool, long>(Sandbox.Game.MyVisualScriptLogicProvider.StartCutsceneSync);

            internal Action<int, long> <SwitchToolbarToSlot>b__414_0(IMyEventOwner s) => 
                new Action<int, long>(Sandbox.Game.MyVisualScriptLogicProvider.SwitchToolbarToSlotSync);

            internal Action<int> <UnlockAchievementById>b__429_0(IMyEventOwner s) => 
                new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.UnlockAchievementInternalAll);

            internal Action<int> <UnlockAchievementById>b__429_1(IMyEventOwner s) => 
                new Action<int>(Sandbox.Game.MyVisualScriptLogicProvider.UnlockAchievementInternal);
        }

        private class AllowedAchievementsHelper
        {
            public static readonly List<string> AllowedAchievements = new List<string>();

            static AllowedAchievementsHelper()
            {
                AllowedAchievements.Add("Promoted_engineer");
                AllowedAchievements.Add("Engineering_degree");
                AllowedAchievements.Add("Planetesphobia");
                AllowedAchievements.Add("Rapid_disassembly");
                AllowedAchievements.Add("It_takes_but_one");
                AllowedAchievements.Add("I_see_dead_drones");
                AllowedAchievements.Add("Bring_it_on");
                AllowedAchievements.Add("Im_doing_my_part");
                AllowedAchievements.Add("Scrap_delivery");
                AllowedAchievements.Add("Joint_operation");
                AllowedAchievements.Add("Flak_fodde");
            }
        }
    }
}

