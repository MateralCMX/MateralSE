namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRageMath;

    public abstract class MyRespawnComponentBase : MySessionComponentBase
    {
        [CompilerGenerated]
        private static Action<MyPlayer> RespawnRequested;

        public static  event Action<MyPlayer> RespawnRequested
        {
            [CompilerGenerated] add
            {
                Action<MyPlayer> respawnRequested = RespawnRequested;
                while (true)
                {
                    Action<MyPlayer> a = respawnRequested;
                    Action<MyPlayer> action3 = (Action<MyPlayer>) Delegate.Combine(a, value);
                    respawnRequested = Interlocked.CompareExchange<Action<MyPlayer>>(ref RespawnRequested, action3, a);
                    if (ReferenceEquals(respawnRequested, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlayer> respawnRequested = RespawnRequested;
                while (true)
                {
                    Action<MyPlayer> source = respawnRequested;
                    Action<MyPlayer> action3 = (Action<MyPlayer>) Delegate.Remove(source, value);
                    respawnRequested = Interlocked.CompareExchange<Action<MyPlayer>>(ref RespawnRequested, action3, source);
                    if (ReferenceEquals(respawnRequested, source))
                    {
                        return;
                    }
                }
            }
        }

        protected MyRespawnComponentBase()
        {
        }

        public abstract void AfterRemovePlayer(MyPlayer player);
        public abstract void CloseRespawnScreen();
        public abstract void CloseRespawnScreenNow();
        public abstract MyIdentity CreateNewIdentity(string identityName, MyPlayer.PlayerId playerId, string modelName, bool initialPlayer = false);
        public abstract bool HandleRespawnRequest(bool joinGame, bool newIdentity, long respawnEntityId, string respawnShipId, MyPlayer.PlayerId playerId, Vector3D? spawnPosition, SerializableDefinitionId? botDefinitionId, bool realPlayer, string modelName, Color color);
        public abstract void InitFromCheckpoint(MyObjectBuilder_Checkpoint checkpoint);
        public abstract bool IsInRespawnScreen();
        protected static void NotifyRespawnRequested(MyPlayer player)
        {
            if (RespawnRequested != null)
            {
                RespawnRequested(player);
            }
        }

        public void ResetPlayerIdentity(MyPlayer player, string modelName, Color color)
        {
            if ((player.Identity != null) && MySession.Static.Settings.PermanentDeath.Value)
            {
                if (!player.Identity.IsDead)
                {
                    Sync.Players.KillPlayer(player);
                }
                IMyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(player.Identity.IdentityId);
                if (faction != null)
                {
                    MyFactionCollection.KickMember(faction.FactionId, player.Identity.IdentityId);
                }
                MySession.Static.ChatSystem.ChatHistory.ClearNonGlobalHistory();
                MyIdentity identity = Sync.Players.CreateNewIdentity(player.DisplayName, modelName, new Vector3?((Vector3) color), false);
                player.Identity = identity;
            }
        }

        public abstract void SaveToCheckpoint(MyObjectBuilder_Checkpoint checkpoint);
        public abstract void SetNoRespawnText(StringBuilder text, int timeSec);
        public abstract void SetupCharacterDefault(MyPlayer player, MyWorldGenerator.Args args);
        public abstract void SetupCharacterFromStarts(MyPlayer player, MyWorldGeneratorStartingStateBase[] playerStarts, MyWorldGenerator.Args args);

        protected static bool ShowPermaWarning
        {
            [CompilerGenerated]
            get => 
                <ShowPermaWarning>k__BackingField;
            [CompilerGenerated]
            set => 
                (<ShowPermaWarning>k__BackingField = value);
        }
    }
}

