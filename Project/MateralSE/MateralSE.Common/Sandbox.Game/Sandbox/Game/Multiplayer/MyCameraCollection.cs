namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Network;
    using VRage.Serialization;
    using VRageMath;

    [StaticEventOwner]
    internal class MyCameraCollection
    {
        private Dictionary<MyPlayer.PlayerId, Dictionary<long, MyEntityCameraSettings>> m_entityCameraSettings = new Dictionary<MyPlayer.PlayerId, Dictionary<long, MyEntityCameraSettings>>();
        private List<long> m_entitiesToRemove = new List<long>();
        private Dictionary<MyPlayer.PlayerId, MyEntityCameraSettings> m_lastCharacterSettings = new Dictionary<MyPlayer.PlayerId, MyEntityCameraSettings>();

        private void AddCameraData(MyPlayer.PlayerId pid, long entityId, bool isLocalCharacter, MyEntityCameraSettings data)
        {
            if (!this.ContainsPlayer(pid))
            {
                this.m_entityCameraSettings[pid] = new Dictionary<long, MyEntityCameraSettings>();
            }
            if (this.m_entityCameraSettings[pid].ContainsKey(entityId))
            {
                this.m_entityCameraSettings[pid][entityId] = data;
            }
            else
            {
                this.m_entityCameraSettings[pid].Add(entityId, data);
            }
            if (isLocalCharacter)
            {
                this.m_lastCharacterSettings[pid] = data;
            }
        }

        private void AddCameraData(MyPlayer.PlayerId pid, long entityId, bool isFirstPerson, double distance, Vector2 headAngle, bool isLocalCharacter)
        {
            MyEntityCameraSettings cameraSettings = null;
            if (!this.TryGetCameraSettings(pid, entityId, isLocalCharacter, out cameraSettings))
            {
                MyEntityCameraSettings settings1 = new MyEntityCameraSettings();
                settings1.Distance = distance;
                settings1.IsFirstPerson = isFirstPerson;
                settings1.HeadAngle = new Vector2?(headAngle);
                cameraSettings = settings1;
                this.AddCameraData(pid, entityId, isLocalCharacter, cameraSettings);
            }
            else
            {
                cameraSettings.IsFirstPerson = isFirstPerson;
                if (!isFirstPerson)
                {
                    cameraSettings.Distance = distance;
                    cameraSettings.HeadAngle = new Vector2?(headAngle);
                }
                if (isLocalCharacter)
                {
                    this.m_lastCharacterSettings[pid] = cameraSettings;
                }
            }
        }

        public bool ContainsPlayer(MyPlayer.PlayerId pid) => 
            this.m_entityCameraSettings.ContainsKey(pid);

        public void LoadCameraCollection(MyObjectBuilder_Checkpoint checkpoint)
        {
            this.m_entityCameraSettings = new Dictionary<MyPlayer.PlayerId, Dictionary<long, MyEntityCameraSettings>>();
            SerializableDictionary<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> allPlayersData = checkpoint.AllPlayersData;
            if (allPlayersData != null)
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> pair in allPlayersData.Dictionary)
                {
                    MyPlayer.PlayerId id = new MyPlayer.PlayerId(pair.Key.ClientId, pair.Key.SerialId);
                    this.m_entityCameraSettings[id] = new Dictionary<long, MyEntityCameraSettings>();
                    foreach (CameraControllerSettings settings in pair.Value.EntityCameraData)
                    {
                        Vector2? nullable1;
                        MyEntityCameraSettings settings1 = new MyEntityCameraSettings();
                        settings1.Distance = settings.Distance;
                        SerializableVector2? headAngle = settings.HeadAngle;
                        MyEntityCameraSettings settings3 = settings1;
                        if (headAngle != null)
                        {
                            nullable1 = new Vector2?(headAngle.GetValueOrDefault());
                        }
                        else
                        {
                            nullable1 = null;
                        }
                        settings1.HeadAngle = nullable1;
                        MyEntityCameraSettings local1 = settings1;
                        local1.IsFirstPerson = settings.IsFirstPerson;
                        MyEntityCameraSettings settings2 = local1;
                        this.m_entityCameraSettings[id][settings.EntityId] = settings2;
                    }
                }
            }
        }

        [Event(null, 0x2c), Reliable, Server]
        private static void OnSaveEntityCameraSettings(MyPlayer.PlayerId playerId, long entityId, bool isFirstPerson, double distance, Vector2 headAngle, bool isLocalCharacter)
        {
            MyPlayer.PlayerId pid = new MyPlayer.PlayerId(playerId.SteamId, playerId.SerialId);
            MySession.Static.Cameras.AddCameraData(pid, entityId, isFirstPerson, distance, headAngle, isLocalCharacter);
        }

        public void RequestSaveEntityCameraSettings(MyPlayer.PlayerId pid, long entityId, bool isFirstPerson, double distance, float headAngleX, float headAngleY, bool isLocalCharacter)
        {
            Vector2 vector = new Vector2(headAngleX, headAngleY);
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<MyPlayer.PlayerId, long, bool, double, Vector2, bool>(x => new Action<MyPlayer.PlayerId, long, bool, double, Vector2, bool>(MyCameraCollection.OnSaveEntityCameraSettings), pid, entityId, isFirstPerson, distance, vector, isLocalCharacter, targetEndpoint, position);
        }

        public void SaveCameraCollection(MyObjectBuilder_Checkpoint checkpoint)
        {
            if (checkpoint.AllPlayersData != null)
            {
                foreach (KeyValuePair<MyObjectBuilder_Checkpoint.PlayerId, MyObjectBuilder_Player> pair in checkpoint.AllPlayersData.Dictionary)
                {
                    MyPlayer.PlayerId key = new MyPlayer.PlayerId(pair.Key.ClientId, pair.Key.SerialId);
                    pair.Value.EntityCameraData = new List<CameraControllerSettings>();
                    if (this.m_entityCameraSettings.ContainsKey(key))
                    {
                        this.m_entitiesToRemove.Clear();
                        foreach (KeyValuePair<long, MyEntityCameraSettings> pair2 in this.m_entityCameraSettings[key])
                        {
                            SerializableVector2? nullable1;
                            if (!MyEntities.EntityExists(pair2.Key))
                            {
                                this.m_entitiesToRemove.Add(pair2.Key);
                                continue;
                            }
                            CameraControllerSettings settings1 = new CameraControllerSettings();
                            settings1.Distance = pair2.Value.Distance;
                            settings1.IsFirstPerson = pair2.Value.IsFirstPerson;
                            Vector2? headAngle = pair2.Value.HeadAngle;
                            CameraControllerSettings settings2 = settings1;
                            if (headAngle != null)
                            {
                                nullable1 = new SerializableVector2?(headAngle.GetValueOrDefault());
                            }
                            else
                            {
                                nullable1 = null;
                            }
                            settings1.HeadAngle = nullable1;
                            CameraControllerSettings local1 = settings1;
                            local1.EntityId = pair2.Key;
                            CameraControllerSettings item = local1;
                            pair.Value.EntityCameraData.Add(item);
                        }
                        foreach (long num in this.m_entitiesToRemove)
                        {
                            this.m_entityCameraSettings[key].Remove(num);
                        }
                    }
                }
            }
        }

        public void SaveEntityCameraSettings(MyPlayer.PlayerId pid, long entityId, bool isFirstPerson, double distance, bool isLocalCharacter, float headAngleX, float headAngleY, bool sync = true)
        {
            if (!Sync.IsServer & sync)
            {
                this.RequestSaveEntityCameraSettings(pid, entityId, isFirstPerson, distance, headAngleX, headAngleY, isLocalCharacter);
            }
            Vector2 headAngle = new Vector2(headAngleX, headAngleY);
            this.AddCameraData(pid, entityId, isFirstPerson, distance, headAngle, isLocalCharacter);
        }

        public bool TryGetCameraSettings(MyPlayer.PlayerId pid, long entityId, bool isLocalCharacter, out MyEntityCameraSettings cameraSettings)
        {
            if (Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MySession.Static.Players.GetPlayerById(pid);
            }
            else
            {
                MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            }
            if (this.ContainsPlayer(pid))
            {
                if (this.m_entityCameraSettings[pid].ContainsKey(entityId))
                {
                    return this.m_entityCameraSettings[pid].TryGetValue(entityId, out cameraSettings);
                }
                if (isLocalCharacter && this.m_lastCharacterSettings.ContainsKey(pid))
                {
                    cameraSettings = this.m_lastCharacterSettings[pid];
                    this.m_entityCameraSettings[pid][entityId] = cameraSettings;
                    return true;
                }
            }
            cameraSettings = null;
            return false;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCameraCollection.<>c <>9 = new MyCameraCollection.<>c();
            public static Func<IMyEventOwner, Action<MyPlayer.PlayerId, long, bool, double, Vector2, bool>> <>9__0_0;

            internal Action<MyPlayer.PlayerId, long, bool, double, Vector2, bool> <RequestSaveEntityCameraSettings>b__0_0(IMyEventOwner x) => 
                new Action<MyPlayer.PlayerId, long, bool, double, Vector2, bool>(MyCameraCollection.OnSaveEntityCameraSettings);
        }
    }
}

