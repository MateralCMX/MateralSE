namespace Sandbox.Engine.Multiplayer
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Network;
    using VRageMath;

    [StaticEventOwner]
    public abstract class MyClientState : MyClientStateBase
    {
        public readonly Dictionary<long, HashSet<long>> KnownSectors = new Dictionary<long, HashSet<long>>();
        private MyEntity m_positionEntityServer;

        protected MyClientState()
        {
        }

        [Event(null, 0x127), Reliable, Server]
        public static void AddKnownSector(long planetId, long sectorId)
        {
            MyReplicationServer replicationServer = MyMultiplayer.GetReplicationServer();
            if (replicationServer != null)
            {
                MyClientState clientData = (MyClientState) replicationServer.GetClientData(new Endpoint(MyEventContext.Current.Sender, 0));
                if (clientData != null)
                {
                    HashSet<long> set;
                    if (!clientData.KnownSectors.TryGetValue(planetId, out set))
                    {
                        set = new HashSet<long>();
                        clientData.KnownSectors.Add(planetId, set);
                    }
                    set.Add(sectorId);
                }
            }
        }

        private void GetControlledEntity(out MyEntity controlledEntity, out bool hasControl)
        {
            controlledEntity = null;
            hasControl = false;
            if ((!Sync.IsServer && ((base.EndpointId.Index == 0) && (MySession.Static.HasCreativeRights && ReferenceEquals(MySession.Static.CameraController, MySpectatorCameraController.Static)))) && ((MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.UserControlled) || (MySpectatorCameraController.Static.SpectatorCameraMovement == MySpectatorCameraMovementEnum.Orbit)))
            {
                MyCharacter topMostControlledEntity = MySession.Static.TopMostControlledEntity as MyCharacter;
                if ((topMostControlledEntity == null) || !topMostControlledEntity.UpdateRotationsOverride)
                {
                    return;
                }
            }
            foreach (KeyValuePair<long, MyPlayer.PlayerId> pair in Sync.Players.ControlledEntities)
            {
                if (pair.Value == new MyPlayer.PlayerId(base.EndpointId.Id.Value, base.EndpointId.Index))
                {
                    controlledEntity = MyEntities.GetEntityById(pair.Key, false);
                    if (controlledEntity != null)
                    {
                        MyEntity topMostParent = controlledEntity.GetTopMostParent(null);
                        MyPlayer controllingPlayer = Sync.Players.GetControllingPlayer(topMostParent);
                        if ((controllingPlayer != null) && (pair.Value == controllingPlayer.Id))
                        {
                            controlledEntity = topMostParent;
                        }
                        break;
                    }
                }
            }
            if (controlledEntity != null)
            {
                if (Sync.IsServer)
                {
                    hasControl = true;
                }
                else
                {
                    MyPlayer player = this.GetPlayer();
                    hasControl = ReferenceEquals(MySession.Static.LocalHumanPlayer, player);
                }
            }
        }

        private void Read(BitStream stream, bool outOfOrder)
        {
            MyEntity entity;
            this.ReadShared(stream, out entity);
            int bitPosition = stream.BitPosition;
            short num2 = stream.ReadInt16(0x10);
            bool flag = entity != null;
            if (flag)
            {
                MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(entity);
                flag &= (controllingPlayer != null) && (controllingPlayer.Client.SteamUserId == base.EndpointId.Id.Value);
            }
            if (!flag)
            {
                stream.SetBitPositionRead(bitPosition + num2);
            }
            else
            {
                this.ReadInternal(stream, entity);
                entity.DeserializeControls(stream, outOfOrder);
            }
            base.Ping = stream.ReadInt16(0x10);
        }

        protected abstract void ReadInternal(BitStream stream, MyEntity controlledEntity);
        private void ReadShared(BitStream stream, out MyEntity controlledEntity)
        {
            controlledEntity = null;
            bool hasControl = stream.ReadBool();
            if (hasControl)
            {
                MyEntity entity;
                bool flag2 = stream.ReadBool();
                if (!MyEntities.TryGetEntityById(stream.ReadInt64(0x40), out entity, true))
                {
                    goto TR_0003;
                }
                else if (!entity.GetTopMostParent(null).MarkedForClose)
                {
                    this.m_positionEntityServer = entity;
                    if (!flag2)
                    {
                        return;
                    }
                    if (!(entity.SyncObject is MySyncEntity))
                    {
                        return;
                    }
                    controlledEntity = entity;
                }
                else
                {
                    goto TR_0003;
                }
                goto TR_0000;
            }
            else
            {
                if (stream.ReadBool())
                {
                    Vector3D zero = Vector3D.Zero;
                    stream.Serialize(ref zero);
                    this.m_positionEntityServer = null;
                    this.Position = new Vector3D?(zero);
                }
                goto TR_0000;
            }
            goto TR_0003;
        TR_0000:
            this.UpdateConrtolledEntityStates(controlledEntity, hasControl);
            return;
        TR_0003:
            this.m_positionEntityServer = null;
        }

        [Event(null, 320), Reliable, Server]
        public static void RemoveKnownSector(long planetId, long sectorId)
        {
            MyReplicationServer replicationServer = MyMultiplayer.GetReplicationServer();
            if (replicationServer != null)
            {
                HashSet<long> set;
                MyClientState clientData = (MyClientState) replicationServer.GetClientData(new Endpoint(MyEventContext.Current.Sender, 0));
                if ((clientData != null) && clientData.KnownSectors.TryGetValue(planetId, out set))
                {
                    set.Remove(sectorId);
                    if (set.Count == 0)
                    {
                        clientData.KnownSectors.Remove(planetId);
                    }
                }
            }
        }

        public override void ResetControlledEntityControls()
        {
            MyEntity entity;
            bool flag;
            this.GetControlledEntity(out entity, out flag);
            if (entity != null)
            {
                entity.ResetControls();
            }
        }

        public override void Serialize(BitStream stream, bool outOfOrder)
        {
            if (stream.Writing)
            {
                this.Write(stream);
            }
            else
            {
                this.Read(stream, outOfOrder);
            }
        }

        public override void Update()
        {
            MyEntity entity;
            bool flag;
            this.GetControlledEntity(out entity, out flag);
            if (flag && (entity != null))
            {
                entity.ApplyLastControls();
            }
            this.UpdateConrtolledEntityStates(entity, flag);
        }

        private void UpdateConrtolledEntityStates(MyEntity controlledEntity, bool hasControl)
        {
            if (!hasControl || (controlledEntity == null))
            {
                bool flag2;
                base.IsControllingGrid = flag2 = false;
                base.IsControllingCharacter = base.IsControllingJetpack = flag2;
            }
            else
            {
                MyCharacter character = controlledEntity as MyCharacter;
                if (character != null)
                {
                    base.IsControllingCharacter = !character.JetpackRunning;
                    base.IsControllingJetpack = character.JetpackRunning;
                    base.IsControllingGrid = false;
                }
                else
                {
                    base.IsControllingCharacter = false;
                    base.IsControllingJetpack = false;
                    base.IsControllingGrid = controlledEntity is MyCubeGrid;
                }
            }
        }

        private void Write(BitStream stream)
        {
            MyEntity controlledEntity = null;
            bool hasControl = false;
            if (base.PlayerSerialId <= 0)
            {
                this.GetControlledEntity(out controlledEntity, out hasControl);
            }
            else
            {
                MyPlayer playerById = Sync.Players.GetPlayerById(new MyPlayer.PlayerId(base.EndpointId.Id.Value, base.PlayerSerialId));
                if (playerById.Controller.ControlledEntity != null)
                {
                    controlledEntity = playerById.Controller.ControlledEntity.Entity.GetTopMostParent(null);
                    hasControl = true;
                }
            }
            this.WriteShared(stream, controlledEntity ?? MySession.Static.CameraController.Entity, hasControl);
            int bitPosition = stream.BitPosition;
            stream.WriteInt16(0x10, 0x10);
            if (controlledEntity != null)
            {
                this.WriteInternal(stream, controlledEntity);
                controlledEntity.SerializeControls(stream);
                int newBitPosition = stream.BitPosition;
                short num3 = (short) (stream.BitPosition - bitPosition);
                stream.SetBitPositionWrite(bitPosition);
                stream.WriteInt16(num3, 0x10);
                stream.SetBitPositionWrite(newBitPosition);
            }
            stream.WriteInt16(base.Ping, 0x10);
        }

        protected abstract void WriteInternal(BitStream stream, MyEntity controlledEntity);
        private void WriteShared(BitStream stream, MyEntity controlledEntity, bool hasControl)
        {
            stream.WriteBool(controlledEntity != null);
            if (controlledEntity != null)
            {
                stream.WriteInt64(controlledEntity.EntityId, 0x40);
                stream.WriteBool(hasControl);
            }
            else if (!MySpectatorCameraController.Static.Initialized)
            {
                stream.WriteBool(false);
            }
            else
            {
                stream.WriteBool(true);
                Vector3D position = MySpectatorCameraController.Static.Position;
                stream.Serialize(ref position);
            }
        }

        public MyContextKind Context { get; protected set; }

        public MyEntity ContextEntity { get; protected set; }

        public override Vector3D? Position
        {
            get
            {
                if ((this.m_positionEntityServer == null) || this.m_positionEntityServer.Closed)
                {
                    return base.Position;
                }
                return new Vector3D?(this.m_positionEntityServer.WorldMatrix.Translation);
            }
            protected set => 
                (base.Position = value);
        }

        public override IMyReplicable ControlledReplicable
        {
            get
            {
                MyPlayer player = this.GetPlayer();
                if (player == null)
                {
                    return null;
                }
                MyCharacter character = player.Character;
                return ((character != null) ? MyExternalReplicable.FindByObject(character.GetTopMostParent(null)) : null);
            }
        }

        public override IMyReplicable CharacterReplicable
        {
            get
            {
                MyPlayer player = this.GetPlayer();
                if (player == null)
                {
                    return null;
                }
                MyCharacter character = player.Character;
                return ((character != null) ? MyExternalReplicable.FindByObject(character) : null);
            }
        }

        public enum MyContextKind
        {
            None,
            Terminal,
            Inventory,
            Production,
            Building
        }
    }
}

