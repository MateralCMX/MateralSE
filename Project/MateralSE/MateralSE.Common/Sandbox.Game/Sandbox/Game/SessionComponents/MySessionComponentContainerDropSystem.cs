namespace Sandbox.Game.SessionComponents
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Components.Session;
    using VRage.Game.Definitions.SessionComponents;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.Utils;
    using VRageMath;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation | MyUpdateOrder.BeforeSimulation, 0x378, typeof(MyObjectBuilder_SessionComponentContainerDropSystem), (System.Type) null)]
    public class MySessionComponentContainerDropSystem : MySessionComponentBase
    {
        private readonly Random random = new Random();
        private readonly int DESPAWN_SMOKE_TIME = 15;
        private static readonly short ONE_MINUTE = 60;
        private static readonly short TWO_MINUTES = 120;
        private readonly string m_dropTriggerName = "Special Content";
        private static MySoundPair m_explosionSound = new MySoundPair("WepSmallWarheadExpl", false);
        private MyContainerDropSystemDefinition m_definition;
        private int m_counter;
        private uint m_containerIdSmall = 1;
        private uint m_containerIdLarge = 1;
        private List<MyContainerGPS> m_delayedGPSForRemoval;
        private List<MyEntityForRemoval> m_delayedEntitiesForRemoval;
        private List<MyPlayerContainerData> m_playerData = new List<MyPlayerContainerData>();
        private Dictionary<MyTuple<SpawnType, bool>, List<MyDropContainerDefinition>> m_dropContainerLists;
        private MyTuple<SpawnType, bool> m_keyPersonalSpace;
        private MyTuple<SpawnType, bool> m_keyPersonalAtmosphere;
        private MyTuple<SpawnType, bool> m_keyPersonalMoon;
        private MyTuple<SpawnType, bool> m_keyCompetetiveSpace;
        private MyTuple<SpawnType, bool> m_keyCompetetiveAtmosphere;
        private MyTuple<SpawnType, bool> m_keyCompetetiveMoon;
        private bool m_hasNewItems;
        private List<MyGameInventoryItem> m_newGameItems;
        private Dictionary<VRage.Game.Entity.MyEntity, MyParticleEffect> m_smokeParticles = new Dictionary<VRage.Game.Entity.MyEntity, MyParticleEffect>();
        private Dictionary<MyGps, MyEntityForRemoval> m_gpsList = new Dictionary<MyGps, MyEntityForRemoval>();
        private List<MyGps> m_gpsToRemove = new List<MyGps>();
        private bool m_nothingDropped;
        private bool m_enableWindowPopups = true;
        private int m_minDropContainerRespawnTime;
        private int m_maxDropContainerRespawnTime;

        private void AddSmoke(VRage.Game.Entity.MyEntity entity, MyParticleEffect effect)
        {
            this.m_smokeParticles[entity] = effect;
        }

        [Event(null, 0x391), Reliable, Server]
        public static void CompetetiveContainerOpened(string name, int time, long playerId, Color color)
        {
            RemoveGPS(name, playerId);
            MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().RegisterDelayedGPSRemovalInternal(name, time);
            ModifyGPSColorForAll(name, color);
        }

        public void ContainerDestroyed(MyContainerDropComponent container)
        {
            if (container.Competetive)
            {
                RemoveGPS(container.GPSName, 0L);
            }
            else
            {
                for (int i = 0; i < this.m_playerData.Count; i++)
                {
                    if (this.m_playerData[i].PlayerId == container.Owner)
                    {
                        this.m_playerData[i].Active = true;
                    }
                }
                RemoveGPS(container.GPSName, container.Owner);
            }
        }

        public void ContainerOpened(MyContainerDropComponent container, long playerId)
        {
            if (container.Entity != null)
            {
                EndpointId id;
                Vector3D? nullable;
                if (container.Competetive)
                {
                    MyGameService.TriggerCompetitiveContainer();
                    if (Sync.IsServer)
                    {
                        CompetetiveContainerOpened(container.GPSName, this.m_definition.CompetetiveContainerGPSTimeOut, playerId, this.m_definition.CompetetiveContainerGPSColorClaimed);
                    }
                    else
                    {
                        id = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int, long, Color>(x => new Action<string, int, long, Color>(MySessionComponentContainerDropSystem.CompetetiveContainerOpened), container.GPSName, this.m_definition.CompetetiveContainerGPSTimeOut, playerId, this.m_definition.CompetetiveContainerGPSColorClaimed, id, nullable);
                    }
                }
                else
                {
                    MyGameService.TriggerPersonalContainer();
                    if (Sync.IsServer)
                    {
                        RemoveGPS(container.GPSName, container.Owner);
                    }
                    else
                    {
                        id = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, long>(x => new Action<string, long>(MySessionComponentContainerDropSystem.RemoveGPS), container.GPSName, container.Owner, id, nullable);
                    }
                    for (int i = 0; i < this.m_playerData.Count; i++)
                    {
                        if (this.m_playerData[i].PlayerId == playerId)
                        {
                            this.m_playerData[i].Active = true;
                        }
                    }
                }
                if (container.Entity != null)
                {
                    id = new EndpointId();
                    nullable = null;
                    Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySessionComponentContainerDropSystem.RemoveContainerDropComponent), container.Entity.EntityId, id, nullable);
                }
            }
        }

        private Vector3D FindNewSpawnPosition(bool personal, out bool validSpawn, out MyPlanet planet, Vector3D basePosition)
        {
            validSpawn = false;
            planet = null;
            Vector3D zero = Vector3D.Zero;
            float minValue = personal ? this.m_definition.PersonalContainerDistMin : this.m_definition.CompetetiveContainerDistMin;
            float maxValue = personal ? this.m_definition.PersonalContainerDistMax : this.m_definition.CompetetiveContainerDistMax;
            for (int i = 15; i > 0; i--)
            {
                zero = (MyUtils.GetRandomVector3Normalized() * MyUtils.GetRandomFloat(minValue, maxValue)) + basePosition;
                if (this.IsSpawnPositionFree(zero, 50.0))
                {
                    if (MyGravityProviderSystem.CalculateNaturalGravityInPoint(zero) == Vector3D.Zero)
                    {
                        validSpawn = true;
                        break;
                    }
                    planet = MyGamePruningStructure.GetClosestPlanet(zero);
                    zero = this.GetPlanetarySpawnPosition(zero, planet);
                    if (this.IsSpawnPositionFree(zero, 50.0))
                    {
                        validSpawn = true;
                        break;
                    }
                }
            }
            return zero;
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            MyObjectBuilder_SessionComponentContainerDropSystem objectBuilder = base.GetObjectBuilder() as MyObjectBuilder_SessionComponentContainerDropSystem;
            objectBuilder.PlayerData = new List<PlayerContainerData>();
            foreach (MyPlayerContainerData data in this.m_playerData)
            {
                objectBuilder.PlayerData.Add(new PlayerContainerData(data.PlayerId, data.Timer, data.Active, data.Competetive, (data.Container != null) ? data.Container.EntityId : 0L));
            }
            objectBuilder.GPSForRemoval = this.m_delayedGPSForRemoval;
            objectBuilder.EntitiesForRemoval = this.m_delayedEntitiesForRemoval;
            objectBuilder.ContainerIdSmall = this.m_containerIdSmall;
            objectBuilder.ContainerIdLarge = this.m_containerIdLarge;
            return objectBuilder;
        }

        private Vector3D GetPlanetarySpawnPosition(Vector3D position, MyPlanet planet)
        {
            if (planet == null)
            {
                return position;
            }
            Vector3D vectord = MyGravityProviderSystem.CalculateNaturalGravityInPoint(position);
            return (planet.GetClosestSurfacePointGlobal(ref position) - (Vector3D.Normalize(vectord) * (planet.HasAtmosphere ? ((double) 0x7d0) : ((double) 10))));
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            if (Sync.IsServer)
            {
                MyObjectBuilder_SessionComponentContainerDropSystem system = sessionComponent as MyObjectBuilder_SessionComponentContainerDropSystem;
                this.m_delayedGPSForRemoval = (system.GPSForRemoval != null) ? system.GPSForRemoval : new List<MyContainerGPS>();
                this.m_delayedEntitiesForRemoval = (system.EntitiesForRemoval != null) ? system.EntitiesForRemoval : new List<MyEntityForRemoval>();
                this.m_containerIdSmall = system.ContainerIdSmall;
                this.m_containerIdLarge = system.ContainerIdLarge;
                if (system.PlayerData != null)
                {
                    foreach (PlayerContainerData data in system.PlayerData)
                    {
                        this.m_playerData.Add(new MyPlayerContainerData(data.PlayerId, data.Timer, data.Active, data.Competetive, data.ContainerId));
                    }
                }
            }
            if (MyGameService.IsActive)
            {
                MyGameService.ItemsAdded += new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
                MyGameService.NoItemsRecieved += new EventHandler(this.MyGameService_NoItemsRecieved);
            }
            this.m_minDropContainerRespawnTime = MySession.Static.MinDropContainerRespawnTime;
            this.m_maxDropContainerRespawnTime = MySession.Static.MaxDropContainerRespawnTime;
            if (this.m_minDropContainerRespawnTime > this.m_maxDropContainerRespawnTime)
            {
                MyLog.Default.WriteLine("MinDropContainerRespawnTime is higher than MaxDropContainerRespawnTime. Clamping to Max.");
                this.m_minDropContainerRespawnTime = this.m_maxDropContainerRespawnTime;
            }
        }

        public override void InitFromDefinition(MySessionComponentDefinition definition)
        {
            base.InitFromDefinition(definition);
            this.m_definition = definition as MyContainerDropSystemDefinition;
            DictionaryReader<string, MyDropContainerDefinition> dropContainerDefinitions = MyDefinitionManager.Static.GetDropContainerDefinitions();
            this.m_dropContainerLists = new Dictionary<MyTuple<SpawnType, bool>, List<MyDropContainerDefinition>>();
            this.m_keyPersonalSpace = new MyTuple<SpawnType, bool>(SpawnType.Space, false);
            this.m_keyPersonalAtmosphere = new MyTuple<SpawnType, bool>(SpawnType.Atmosphere, false);
            this.m_keyPersonalMoon = new MyTuple<SpawnType, bool>(SpawnType.Moon, false);
            this.m_keyCompetetiveSpace = new MyTuple<SpawnType, bool>(SpawnType.Space, true);
            this.m_keyCompetetiveAtmosphere = new MyTuple<SpawnType, bool>(SpawnType.Atmosphere, true);
            this.m_keyCompetetiveMoon = new MyTuple<SpawnType, bool>(SpawnType.Moon, true);
            this.m_dropContainerLists[this.m_keyPersonalSpace] = new List<MyDropContainerDefinition>();
            this.m_dropContainerLists[this.m_keyPersonalAtmosphere] = new List<MyDropContainerDefinition>();
            this.m_dropContainerLists[this.m_keyPersonalMoon] = new List<MyDropContainerDefinition>();
            this.m_dropContainerLists[this.m_keyCompetetiveSpace] = new List<MyDropContainerDefinition>();
            this.m_dropContainerLists[this.m_keyCompetetiveAtmosphere] = new List<MyDropContainerDefinition>();
            this.m_dropContainerLists[this.m_keyCompetetiveMoon] = new List<MyDropContainerDefinition>();
            foreach (KeyValuePair<string, MyDropContainerDefinition> pair in dropContainerDefinitions)
            {
                if (pair.Value.Priority <= 0f)
                {
                    continue;
                }
                if (pair.Value.Prefab != null)
                {
                    if (pair.Value.SpawnRules.CanBePersonal)
                    {
                        if (pair.Value.SpawnRules.CanSpawnInSpace)
                        {
                            this.m_dropContainerLists[this.m_keyPersonalSpace].Add(pair.Value);
                        }
                        if (pair.Value.SpawnRules.CanSpawnInAtmosphere)
                        {
                            this.m_dropContainerLists[this.m_keyPersonalAtmosphere].Add(pair.Value);
                        }
                        if (pair.Value.SpawnRules.CanSpawnOnMoon)
                        {
                            this.m_dropContainerLists[this.m_keyPersonalMoon].Add(pair.Value);
                        }
                    }
                    if (pair.Value.SpawnRules.CanBeCompetetive)
                    {
                        if (pair.Value.SpawnRules.CanSpawnInSpace)
                        {
                            this.m_dropContainerLists[this.m_keyCompetetiveSpace].Add(pair.Value);
                        }
                        if (pair.Value.SpawnRules.CanSpawnInAtmosphere)
                        {
                            this.m_dropContainerLists[this.m_keyCompetetiveAtmosphere].Add(pair.Value);
                        }
                        if (pair.Value.SpawnRules.CanSpawnOnMoon)
                        {
                            this.m_dropContainerLists[this.m_keyCompetetiveMoon].Add(pair.Value);
                        }
                    }
                }
            }
        }

        private bool IsSpawnPositionFree(Vector3D position, double size)
        {
            BoundingSphereD sphere = new BoundingSphereD(position, size);
            List<VRage.Game.Entity.MyEntity> result = new List<VRage.Game.Entity.MyEntity>();
            MyGamePruningStructure.GetAllEntitiesInSphere(ref sphere, result, MyEntityQueryType.Both);
            bool flag = true;
            using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator = result.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!(enumerator.Current is MyPlanet))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            return flag;
        }

        public static void ModifyGPSColorForAll(string name, Color color)
        {
            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
            {
                MyGps gpsByName = MySession.Static.Gpss.GetGpsByName(player.Identity.IdentityId, name) as MyGps;
                if (gpsByName != null)
                {
                    gpsByName.GPSColor = color;
                    MySession.Static.Gpss.SendModifyGps(player.Identity.IdentityId, gpsByName);
                }
            }
        }

        private void MyGameService_ItemsAdded(object sender, MyGameItemsEventArgs e)
        {
            this.m_newGameItems = e.NewItems;
            this.m_hasNewItems = (this.m_newGameItems != null) && (this.m_newGameItems.Count > 0);
            if (this.m_newGameItems.Count == 1)
            {
                this.m_newGameItems[0].IsNew = true;
            }
        }

        private void MyGameService_NoItemsRecieved(object sender, System.EventArgs e)
        {
            this.m_nothingDropped = true;
        }

        private static MyParticleEffect PlayParticle(VRage.Game.Entity.MyEntity entity, string particleName)
        {
            MyParticleEffect effect = null;
            if (MyParticlesManager.TryCreateParticleEffect(particleName, entity.WorldMatrix, out effect))
            {
                effect.Play();
            }
            return effect;
        }

        [Event(null, 0x1ac), Reliable, Server, Broadcast]
        private static void PlayParticleBroadcast(long entityId, string particleName, bool smoke)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyParticleEffect effect = PlayParticle(entity, particleName);
                if (smoke)
                {
                    if (effect != null)
                    {
                        MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().AddSmoke(entity, effect);
                    }
                }
                else
                {
                    MyEntity3DSoundEmitter emitter = MyAudioComponent.TryGetSoundEmitter();
                    if (emitter != null)
                    {
                        emitter.SetPosition(new Vector3D?(entity.PositionComp.GetPosition()));
                        emitter.Entity = entity;
                        bool? nullable = null;
                        emitter.PlaySound(m_explosionSound, false, false, false, false, false, nullable);
                    }
                }
            }
        }

        public void RegisterDelayedGPSRemovalInternal(string name, int time)
        {
            this.m_delayedGPSForRemoval.Add(new MyContainerGPS(time, name));
        }

        [Event(null, 0x399), Reliable, Server, Broadcast]
        private static void RemoveContainerDropComponent(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MyContainerDropComponent component = entity.Components.Get<MyContainerDropComponent>();
                MySessionComponentContainerDropSystem system = MySession.Static.GetComponent<MySessionComponentContainerDropSystem>();
                if ((system != null) && (component != null))
                {
                    system.RemoveDelayedRemovalEntity(component.GridEntityId);
                    if (component.GridEntityId == 0)
                    {
                        MyCubeBlock block = entity as MyCubeBlock;
                        if ((block != null) && (block.CubeGrid != null))
                        {
                            block.CubeGrid.ChangePowerProducerState(MyMultipleEnabledEnum.AllDisabled, -1L);
                        }
                    }
                    else
                    {
                        VRage.Game.Entity.MyEntity entity2;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(component.GridEntityId, out entity2, false))
                        {
                            MyCubeGrid grid = entity2 as MyCubeGrid;
                            if (grid != null)
                            {
                                grid.ChangePowerProducerState(MyMultipleEnabledEnum.AllDisabled, -1L);
                            }
                        }
                    }
                }
                if (entity.Components != null)
                {
                    entity.Components.Remove<MyContainerDropComponent>();
                }
            }
        }

        private void RemoveDelayedRemovalEntity(long entityId)
        {
            if (this.m_delayedEntitiesForRemoval != null)
            {
                MyEntityForRemoval item = this.m_delayedEntitiesForRemoval.FirstOrDefault<MyEntityForRemoval>(e => e.EntityId == entityId);
                if (item != null)
                {
                    this.m_delayedEntitiesForRemoval.Remove(item);
                }
            }
        }

        [Event(null, 0x349), Reliable, Server]
        public static void RemoveGPS(string name, long playerId = 0L)
        {
            if (playerId == 0)
            {
                foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                {
                    IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(player.Identity.IdentityId, name);
                    if (gpsByName != null)
                    {
                        MySession.Static.Gpss.SendDelete(player.Identity.IdentityId, gpsByName.Hash);
                    }
                }
            }
            else
            {
                IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(playerId, name);
                if (gpsByName != null)
                {
                    MySession.Static.Gpss.SendDelete(playerId, gpsByName.Hash);
                }
            }
        }

        [Event(null, 0x32c), Reliable, Server, Broadcast]
        private static void ShowNotificationSync(string message, int showTime, string font, long playerId)
        {
            if (Sync.IsValidEventOnServer && !MyEventContext.Current.IsLocallyInvoked)
            {
                (Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                MyEventContext.ValidationFailed();
            }
            else if ((MyAPIGateway.Utilities != null) && ((playerId == 0) || (playerId == MySession.Static.LocalPlayerId)))
            {
                MyAPIGateway.Utilities.ShowNotification(message, showTime, font);
            }
        }

        private bool SpawnContainerDrop(MyPlayerContainerData playerData)
        {
            bool flag2;
            MyPlanet planet;
            List<MyDropContainerDefinition> list;
            int num1;
            Vector3 forward;
            bool flag = false;
            ICollection<MyPlayer> players = Sync.Players.GetOnlinePlayers();
            Vector3D zero = Vector3D.Zero;
            foreach (MyPlayer player in players)
            {
                if ((player.Identity.IdentityId == playerData.PlayerId) && (player.Controller.ControlledEntity != null))
                {
                    flag = true;
                    zero = player.Controller.ControlledEntity.Entity.PositionComp.GetPosition();
                    break;
                }
            }
            if (!flag)
            {
                playerData.Competetive = true;
                return true;
            }
            if (!Sync.MultiplayerActive || (Sync.Players.GetOnlinePlayerCount() <= 1))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) (MyUtils.GetRandomFloat() <= 0.95f);
            }
            bool personal = (bool) num1;
            playerData.Competetive = !personal;
            Vector3D globalPos = this.FindNewSpawnPosition(personal, out flag2, out planet, zero);
            if (!flag2)
            {
                return false;
            }
            Vector3D gpsPosition = globalPos;
            Vector3D vectord3 = MyGravityProviderSystem.CalculateNaturalGravityInPoint(gpsPosition);
            if (planet != null)
            {
                gpsPosition = planet.GetClosestSurfacePointGlobal(ref globalPos);
            }
            if ((planet == null) || (vectord3 == Vector3D.Zero))
            {
                list = this.m_dropContainerLists[personal ? this.m_keyPersonalSpace : this.m_keyCompetetiveSpace];
            }
            else
            {
                list = !planet.HasAtmosphere ? this.m_dropContainerLists[personal ? this.m_keyPersonalMoon : this.m_keyCompetetiveMoon] : this.m_dropContainerLists[personal ? this.m_keyPersonalAtmosphere : this.m_keyCompetetiveAtmosphere];
            }
            MyDropContainerDefinition definition = null;
            if (list.Count == 0)
            {
                return false;
            }
            if (list.Count == 1)
            {
                definition = list[0];
            }
            else
            {
                float maxValue = 0f;
                foreach (MyDropContainerDefinition definition2 in list)
                {
                    maxValue += definition2.Priority;
                }
                float randomFloat = MyUtils.GetRandomFloat(0f, maxValue);
                foreach (MyDropContainerDefinition definition3 in list)
                {
                    if (randomFloat <= definition3.Priority)
                    {
                        definition = definition3;
                        break;
                    }
                    randomFloat -= definition3.Priority;
                }
            }
            if (definition == null)
            {
                return false;
            }
            List<MyCubeGrid> resultGridList = new List<MyCubeGrid>();
            Stack<Action> stack = new Stack<Action>();
            stack.Push(delegate {
                MyEntityForRemoval removal;
                string str;
                string str3;
                List<long> list;
                playerData.Container = null;
                MyCubeGrid grid = (resultGridList.Count > 0) ? resultGridList[0] : null;
                if (grid != null)
                {
                    foreach (MyTerminalBlock block in grid.GetFatBlocks<MyTerminalBlock>())
                    {
                        if ((block != null) && ((block.CustomName != null) ? block.CustomName.ToString() : string.Empty).Equals(this.m_dropTriggerName))
                        {
                            playerData.Container = block;
                            break;
                        }
                    }
                }
                if (grid == null)
                {
                    goto TR_0000;
                }
                else if (playerData.Container != null)
                {
                    grid.IsRespawnGrid = true;
                    removal = new MyEntityForRemoval(playerData.Competetive ? this.m_definition.CompetetiveContainerGridTimeOut : this.m_definition.PersonalContainerGridTimeOut, grid.EntityId);
                    this.m_delayedEntitiesForRemoval.Add(removal);
                    str = playerData.Competetive ? MyTexts.GetString(MySpaceTexts.ContainerDropSystemContainerLarge) : MyTexts.GetString(MySpaceTexts.ContainerDropSystemContainerSmall);
                    string str2 = string.Format(MyTexts.GetString(MySpaceTexts.ContainerDropSystemContainerWasDetected), str);
                    str3 = str + " ";
                    if (personal)
                    {
                        str3 = str3 + this.m_containerIdSmall.ToString();
                        this.m_containerIdSmall++;
                    }
                    else
                    {
                        str3 = str3 + this.m_containerIdLarge.ToString();
                        this.m_containerIdLarge++;
                    }
                    MyContainerDropComponent component = new MyContainerDropComponent(playerData.Competetive, str3, playerData.PlayerId, this.m_definition.ContainerAudioCue) {
                        GridEntityId = grid.EntityId
                    };
                    playerData.Container.Components.Add(typeof(MyContainerDropComponent), component);
                    playerData.Container.ChangeOwner(0L, MyOwnershipShareModeEnum.All);
                    list = new List<long>();
                    if (playerData.Competetive)
                    {
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int, string, long>(s => new Action<string, int, string, long>(MySessionComponentContainerDropSystem.ShowNotificationSync), str2, 0x1388, "Blue".ToString(), 0L, targetEndpoint, position);
                        foreach (MyPlayer player in players)
                        {
                            list.Add(player.Identity.IdentityId);
                        }
                    }
                    else
                    {
                        ShowNotificationSync(str2, 0x1388, "Blue".ToString(), playerData.PlayerId);
                        list.Add(playerData.PlayerId);
                    }
                }
                else
                {
                    goto TR_0000;
                }
                Color color = playerData.Competetive ? this.m_definition.CompetetiveContainerGPSColorFree : this.m_definition.PersonalContainerGPSColor;
                foreach (long num in list)
                {
                    MyGps gps1 = new MyGps();
                    gps1.ShowOnHud = true;
                    gps1.Name = str3;
                    gps1.DisplayName = str;
                    gps1.DiscardAt = null;
                    gps1.Coords = gpsPosition;
                    gps1.Description = "";
                    gps1.AlwaysVisible = true;
                    gps1.GPSColor = color;
                    gps1.IsContainerGPS = true;
                    MyGps key = gps1;
                    this.m_gpsList.Add(key, removal);
                    MySession.Static.Gpss.SendAddGps(num, ref key, playerData.Container.EntityId, true);
                }
                return;
            TR_0000:
                playerData.Active = true;
            });
            Vector3 up = (vectord3 != Vector3.Zero) ? (Vector3.Normalize(vectord3) * -1f) : Vector3.Normalize(MyUtils.GetRandomVector3());
            if ((up == Vector3.Left) || (up == Vector3.Right))
            {
                forward = Vector3.Forward;
            }
            else
            {
                forward = Vector3.Right;
            }
            Stack<Action> callbacks = stack;
            Vector3 initialAngularVelocity = new Vector3();
            MyPrefabManager.Static.SpawnPrefab(resultGridList, definition.Prefab.Id.SubtypeName, globalPos, Vector3.Normalize(Vector3.Cross(up, forward)), up, (Vector3) vectord3, initialAngularVelocity, MyTexts.GetString(MySpaceTexts.ContainerDropSystemBeaconText), null, SpawningOptions.SpawnRandomCargo, 0L, true, callbacks);
            return true;
        }

        [Event(null, 0x198), Reliable, Server, Broadcast]
        private static void StopSmoke(long entityId)
        {
            VRage.Game.Entity.MyEntity entity;
            if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false))
            {
                MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().StopSmoke(entity);
            }
        }

        private void StopSmoke(VRage.Game.Entity.MyEntity entity)
        {
            if (this.m_smokeParticles.ContainsKey(entity))
            {
                this.m_smokeParticles[entity].Stop(true);
                this.m_smokeParticles.Remove(entity);
            }
        }

        protected override void UnloadData()
        {
            if (MyGameService.IsActive)
            {
                MyGameService.ItemsAdded -= new EventHandler<MyGameItemsEventArgs>(this.MyGameService_ItemsAdded);
                MyGameService.NoItemsRecieved -= new EventHandler(this.MyGameService_NoItemsRecieved);
            }
            base.UnloadData();
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            this.UpdateSmokeParticles();
            if ((this.m_counter % 60) == 0)
            {
                foreach (KeyValuePair<MyGps, MyEntityForRemoval> pair in this.m_gpsList)
                {
                    EndpointId id;
                    Vector3D? nullable;
                    if (pair.Value.TimeLeft <= TWO_MINUTES)
                    {
                        id = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int>(x => new Action<string, int>(MySessionComponentContainerDropSystem.UpdateGPSRemainingTime), pair.Key.Name, pair.Value.TimeLeft, id, nullable);
                    }
                    else if ((pair.Value.TimeLeft % ONE_MINUTE) == 0x3b)
                    {
                        id = new EndpointId();
                        nullable = null;
                        Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<string, int>(x => new Action<string, int>(MySessionComponentContainerDropSystem.UpdateGPSRemainingTime), pair.Key.Name, pair.Value.TimeLeft, id, nullable);
                    }
                    if (pair.Value.TimeLeft <= 0)
                    {
                        this.m_gpsToRemove.Add(pair.Key);
                    }
                }
                foreach (MyGps gps in this.m_gpsToRemove)
                {
                    this.m_gpsList.Remove(gps);
                }
                this.m_gpsToRemove.Clear();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (MySession.Static.EnableContainerDrops && !MySandboxGame.IsPaused)
            {
                int counter = this.m_counter;
                this.m_counter = counter + 1;
                if ((counter % 60) == 0)
                {
                    if ((this.EnableWindowPopups && this.m_hasNewItems) && (this.m_newGameItems != null))
                    {
                        this.m_hasNewItems = false;
                        object[] args = new object[] { this.m_newGameItems };
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNewGameItems>(args));
                        this.m_newGameItems.Clear();
                    }
                    if (this.EnableWindowPopups && this.m_nothingDropped)
                    {
                        MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNoGameItemDrop>(Array.Empty<object>()));
                        this.m_nothingDropped = false;
                    }
                    if (Sync.IsServer)
                    {
                        this.UpdateContainerSpawner();
                        this.UpdateGPSRemoval();
                        this.UpdateContainerEntityRemoval();
                        int timer = this.random.Next(this.m_minDropContainerRespawnTime, this.m_maxDropContainerRespawnTime);
                        if ((this.m_playerData.Count == 0) && !Sandbox.Engine.Platform.Game.IsDedicated)
                        {
                            this.m_playerData.Add(new MyPlayerContainerData(MySession.Static.LocalPlayerId, timer, true, false, 0L));
                        }
                        if (this.m_counter >= 0xe10)
                        {
                            this.m_counter = 1;
                            foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                            {
                                bool flag = false;
                                int num3 = 0;
                                while (true)
                                {
                                    if (num3 < this.m_playerData.Count)
                                    {
                                        if (this.m_playerData[num3].PlayerId != player.Identity.IdentityId)
                                        {
                                            num3++;
                                            continue;
                                        }
                                        flag = true;
                                    }
                                    if (!flag)
                                    {
                                        this.m_playerData.Add(new MyPlayerContainerData(player.Identity.IdentityId, timer, true, false, 0L));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateContainerEntityRemoval()
        {
            if (this.m_delayedEntitiesForRemoval != null)
            {
                for (int i = 0; i < this.m_delayedEntitiesForRemoval.Count; i++)
                {
                    EndpointId id;
                    Vector3D? nullable;
                    MyEntityForRemoval removal = this.m_delayedEntitiesForRemoval[i];
                    removal.TimeLeft--;
                    if (removal.TimeLeft > 0f)
                    {
                        VRage.Game.Entity.MyEntity entity2;
                        if (((removal.TimeLeft == this.DESPAWN_SMOKE_TIME) && Sandbox.Game.Entities.MyEntities.TryGetEntityById(removal.EntityId, out entity2, false)) && !this.m_smokeParticles.ContainsKey(entity2))
                        {
                            id = new EndpointId();
                            nullable = null;
                            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long, string, bool>(s => new Action<long, string, bool>(MySessionComponentContainerDropSystem.PlayParticleBroadcast), removal.EntityId, "Smoke_Container", true, id, nullable);
                        }
                    }
                    else
                    {
                        VRage.Game.Entity.MyEntity entity;
                        if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(removal.EntityId, out entity, false))
                        {
                            id = new EndpointId();
                            nullable = null;
                            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long, string, bool>(s => new Action<long, string, bool>(MySessionComponentContainerDropSystem.PlayParticleBroadcast), removal.EntityId, "Explosion_Missile", false, id, nullable);
                            id = new EndpointId();
                            nullable = null;
                            Sandbox.Engine.Multiplayer.MyMultiplayer.RaiseStaticEvent<long>(s => new Action<long>(MySessionComponentContainerDropSystem.StopSmoke), removal.EntityId, id, nullable);
                            entity.Close();
                        }
                        this.m_delayedEntitiesForRemoval.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void UpdateContainerSpawner()
        {
            for (int i = 0; i < this.m_playerData.Count; i++)
            {
                MyPlayerContainerData playerData = this.m_playerData[i];
                if (playerData.ContainerId != 0)
                {
                    VRage.Game.Entity.MyEntity entity = null;
                    Sandbox.Game.Entities.MyEntities.TryGetEntityByName(this.m_dropTriggerName, out entity);
                    playerData.Container = entity as MyTerminalBlock;
                    playerData.ContainerId = 0L;
                }
                if (!playerData.Active)
                {
                    if ((playerData.Container != null) && ((playerData.Container.Closed || !playerData.Container.InScene) || !playerData.Container.Components.Contains(typeof(MyContainerDropComponent))))
                    {
                        playerData.Container = null;
                        playerData.Active = true;
                    }
                }
                else
                {
                    playerData.Timer--;
                    if (playerData.Timer <= 0)
                    {
                        bool flag = this.SpawnContainerDrop(playerData);
                        int num2 = this.random.Next(this.m_minDropContainerRespawnTime, this.m_maxDropContainerRespawnTime);
                        playerData.Timer = flag ? num2 : ONE_MINUTE;
                        playerData.Active = !flag || playerData.Competetive;
                    }
                }
            }
        }

        [Event(null, 0x35e), Reliable, Server, Broadcast]
        public static void UpdateGPSRemainingTime(string gpsName, int remainingTime)
        {
            if (Sync.IsServer && !MyEventContext.Current.IsLocallyInvoked)
            {
                (Sandbox.Engine.Multiplayer.MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
                MyEventContext.ValidationFailed();
            }
            else
            {
                foreach (MyPlayer player in Sync.Players.GetOnlinePlayers())
                {
                    IMyGps gpsByName = MySession.Static.Gpss.GetGpsByName(player.Identity.IdentityId, gpsName);
                    if (gpsByName != null)
                    {
                        string str = string.Empty;
                        if (remainingTime >= TWO_MINUTES)
                        {
                            int num = remainingTime / ONE_MINUTE;
                            str = string.Format(MyTexts.GetString(MyCommonTexts.GpsContainerRemainingTimeMins), num);
                        }
                        else if (remainingTime >= ONE_MINUTE)
                        {
                            int num2 = remainingTime / ONE_MINUTE;
                            int num3 = remainingTime % ONE_MINUTE;
                            str = (num3 != 1) ? string.Format(MyTexts.GetString(MyCommonTexts.GpsContainerRemainingTimeMinSecs), num2, num3) : string.Format(MyTexts.GetString(MyCommonTexts.GpsContainerRemainingTimeMinSec), num2, num3);
                        }
                        else if ((remainingTime <= 1) || (remainingTime >= ONE_MINUTE))
                        {
                            str = string.Format(MyTexts.GetString(MyCommonTexts.GpsContainerRemainingTimeSec), remainingTime);
                        }
                        else
                        {
                            str = string.Format(MyTexts.GetString(MyCommonTexts.GpsContainerRemainingTimeSecs), remainingTime);
                        }
                        gpsByName.ContainerRemainingTime = str;
                    }
                }
            }
        }

        private void UpdateGPSRemoval()
        {
            if (this.m_delayedGPSForRemoval != null)
            {
                for (int i = 0; i < this.m_delayedGPSForRemoval.Count; i++)
                {
                    MyContainerGPS rgps = this.m_delayedGPSForRemoval[i];
                    rgps.TimeLeft--;
                    if (rgps.TimeLeft <= 0f)
                    {
                        RemoveGPS(rgps.GPSName, 0L);
                        this.m_delayedGPSForRemoval.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        private void UpdateSmokeParticles()
        {
            foreach (KeyValuePair<VRage.Game.Entity.MyEntity, MyParticleEffect> pair in this.m_smokeParticles)
            {
                pair.Value.WorldMatrix = pair.Key.WorldMatrix;
            }
        }

        public bool EnableWindowPopups
        {
            get => 
                this.m_enableWindowPopups;
            set => 
                (this.m_enableWindowPopups = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySessionComponentContainerDropSystem.<>c <>9 = new MySessionComponentContainerDropSystem.<>c();
            public static Func<IMyEventOwner, Action<string, int>> <>9__40_0;
            public static Func<IMyEventOwner, Action<string, int>> <>9__40_1;
            public static Func<IMyEventOwner, Action<long, string, bool>> <>9__44_0;
            public static Func<IMyEventOwner, Action<long>> <>9__44_1;
            public static Func<IMyEventOwner, Action<long, string, bool>> <>9__44_2;
            public static Func<IMyEventOwner, Action<string, int, long, Color>> <>9__53_0;
            public static Func<IMyEventOwner, Action<string, long>> <>9__53_1;
            public static Func<IMyEventOwner, Action<long>> <>9__53_2;
            public static Func<IMyEventOwner, Action<string, int, string, long>> <>9__54_1;

            internal Action<string, int, long, Color> <ContainerOpened>b__53_0(IMyEventOwner x) => 
                new Action<string, int, long, Color>(MySessionComponentContainerDropSystem.CompetetiveContainerOpened);

            internal Action<string, long> <ContainerOpened>b__53_1(IMyEventOwner x) => 
                new Action<string, long>(MySessionComponentContainerDropSystem.RemoveGPS);

            internal Action<long> <ContainerOpened>b__53_2(IMyEventOwner s) => 
                new Action<long>(MySessionComponentContainerDropSystem.RemoveContainerDropComponent);

            internal Action<string, int, string, long> <SpawnContainerDrop>b__54_1(IMyEventOwner s) => 
                new Action<string, int, string, long>(MySessionComponentContainerDropSystem.ShowNotificationSync);

            internal Action<string, int> <UpdateAfterSimulation>b__40_0(IMyEventOwner x) => 
                new Action<string, int>(MySessionComponentContainerDropSystem.UpdateGPSRemainingTime);

            internal Action<string, int> <UpdateAfterSimulation>b__40_1(IMyEventOwner x) => 
                new Action<string, int>(MySessionComponentContainerDropSystem.UpdateGPSRemainingTime);

            internal Action<long, string, bool> <UpdateContainerEntityRemoval>b__44_0(IMyEventOwner s) => 
                new Action<long, string, bool>(MySessionComponentContainerDropSystem.PlayParticleBroadcast);

            internal Action<long> <UpdateContainerEntityRemoval>b__44_1(IMyEventOwner s) => 
                new Action<long>(MySessionComponentContainerDropSystem.StopSmoke);

            internal Action<long, string, bool> <UpdateContainerEntityRemoval>b__44_2(IMyEventOwner s) => 
                new Action<long, string, bool>(MySessionComponentContainerDropSystem.PlayParticleBroadcast);
        }

        private class MyPlayerContainerData
        {
            public long PlayerId;
            public int Timer;
            public bool Active;
            public bool Competetive;
            public MyTerminalBlock Container;
            public long ContainerId;

            public MyPlayerContainerData(long playerId, int timer, bool active, bool competetive, long cargoId)
            {
                this.PlayerId = playerId;
                this.Timer = timer;
                this.Active = active;
                this.Competetive = competetive;
                this.ContainerId = cargoId;
            }
        }

        private enum SpawnType
        {
            Space,
            Atmosphere,
            Moon
        }
    }
}

