namespace Sandbox.Game.World
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRageMath;

    public class MyPlayer : IMyPlayer
    {
        public const int BUILD_COLOR_SLOTS_COUNT = 14;
        private MyNetworkClient m_client;
        private MyIdentity m_identity;
        [CompilerGenerated]
        private Action<MyPlayer, MyIdentity> IdentityChanged;
        private int m_selectedBuildColorSlot;
        private static readonly List<Vector3> m_buildColorDefaults = new List<Vector3>();
        private List<Vector3> m_buildColorHSVSlots = new List<Vector3>();
        private bool m_forceRealPlayer;
        public HashSet<long> Grids = new HashSet<long>();
        public List<long> CachedControllerId;

        public event Action<MyPlayer, MyIdentity> IdentityChanged
        {
            [CompilerGenerated] add
            {
                Action<MyPlayer, MyIdentity> identityChanged = this.IdentityChanged;
                while (true)
                {
                    Action<MyPlayer, MyIdentity> a = identityChanged;
                    Action<MyPlayer, MyIdentity> action3 = (Action<MyPlayer, MyIdentity>) Delegate.Combine(a, value);
                    identityChanged = Interlocked.CompareExchange<Action<MyPlayer, MyIdentity>>(ref this.IdentityChanged, action3, a);
                    if (ReferenceEquals(identityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyPlayer, MyIdentity> identityChanged = this.IdentityChanged;
                while (true)
                {
                    Action<MyPlayer, MyIdentity> source = identityChanged;
                    Action<MyPlayer, MyIdentity> action3 = (Action<MyPlayer, MyIdentity>) Delegate.Remove(source, value);
                    identityChanged = Interlocked.CompareExchange<Action<MyPlayer, MyIdentity>>(ref this.IdentityChanged, action3, source);
                    if (ReferenceEquals(identityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<IMyPlayer, IMyIdentity> IMyPlayer.IdentityChanged
        {
            add
            {
                this.IdentityChanged += this.GetDelegate(value);
            }
            remove
            {
                this.IdentityChanged -= this.GetDelegate(value);
            }
        }

        static MyPlayer()
        {
            InitDefaultColors();
        }

        public MyPlayer(MyNetworkClient client, PlayerId id)
        {
            this.m_client = client;
            this.Id = id;
            this.Controller = new MyEntityController(this);
        }

        public void AcquireControls()
        {
            if (this.Controller.ControlledEntity != null)
            {
                this.Controller.ControlledEntity.ControllerInfo.AcquireControls();
            }
            this.Controller.SetCamera();
        }

        public void AddGrid(long gridEntityId)
        {
            this.Grids.Add(gridEntityId);
        }

        public void ChangeOrSwitchToColor(Vector3 color)
        {
            for (int i = 0; i < 14; i++)
            {
                if (this.m_buildColorHSVSlots[i] == color)
                {
                    this.m_selectedBuildColorSlot = i;
                    return;
                }
            }
            this.SelectedBuildColor = color;
        }

        private Action<MyPlayer, MyIdentity> GetDelegate(Action<IMyPlayer, IMyIdentity> value) => 
            ((Action<MyPlayer, MyIdentity>) Delegate.CreateDelegate(typeof(Action<MyPlayer, MyIdentity>), value.Target, value.Method));

        public MyObjectBuilder_Player GetObjectBuilder()
        {
            MyObjectBuilder_Player player1 = new MyObjectBuilder_Player();
            player1.DisplayName = this.DisplayName;
            player1.IdentityId = this.Identity.IdentityId;
            player1.Connected = true;
            player1.ForceRealPlayer = this.m_forceRealPlayer;
            MyObjectBuilder_Player player = player1;
            if (!IsColorsSetToDefaults(this.m_buildColorHSVSlots))
            {
                player.BuildColorSlots = new List<Vector3>();
                foreach (Vector3 vector in this.m_buildColorHSVSlots)
                {
                    player.BuildColorSlots.Add(vector);
                }
            }
            return player;
        }

        public static MyPlayer GetPlayerFromCharacter(MyCharacter character)
        {
            if (character == null)
            {
                return null;
            }
            if ((character.ControllerInfo == null) || (character.ControllerInfo.Controller == null))
            {
                return null;
            }
            return character.ControllerInfo.Controller.Player;
        }

        public static MyPlayer GetPlayerFromWeapon(IMyGunBaseUser gunUser)
        {
            if (gunUser == null)
            {
                return null;
            }
            MyCharacter owner = gunUser.Owner as MyCharacter;
            return ((owner == null) ? null : GetPlayerFromCharacter(owner));
        }

        public Vector3D GetPosition()
        {
            if ((this.Controller.ControlledEntity == null) || (this.Controller.ControlledEntity.Entity == null))
            {
                return Vector3D.Zero;
            }
            return this.Controller.ControlledEntity.Entity.PositionComp.GetPosition();
        }

        public static MyRelationsBetweenPlayerAndBlock GetRelationBetweenPlayers(long playerId1, long playerId2)
        {
            if (playerId1 == playerId2)
            {
                return MyRelationsBetweenPlayerAndBlock.Owner;
            }
            IMyFaction objA = MySession.Static.Factions.TryGetPlayerFaction(playerId1);
            IMyFaction objB = MySession.Static.Factions.TryGetPlayerFaction(playerId2);
            if ((objA == null) || (objB == null))
            {
                return MyRelationsBetweenPlayerAndBlock.Enemies;
            }
            return (!ReferenceEquals(objA, objB) ? ((MySession.Static.Factions.GetRelationBetweenFactions(objA.FactionId, objB.FactionId) != MyRelationsBetweenFactions.Neutral) ? MyRelationsBetweenPlayerAndBlock.Enemies : MyRelationsBetweenPlayerAndBlock.Neutral) : MyRelationsBetweenPlayerAndBlock.FactionShare);
        }

        public static MyRelationsBetweenPlayers GetRelationsBetweenPlayers(long playerId1, long playerId2)
        {
            if ((playerId1 == 0) || (playerId2 == 0))
            {
                return MyRelationsBetweenPlayers.Neutral;
            }
            if (playerId1 == playerId2)
            {
                return MyRelationsBetweenPlayers.Self;
            }
            IMyFaction objA = MySession.Static.Factions.TryGetPlayerFaction(playerId1);
            IMyFaction objB = MySession.Static.Factions.TryGetPlayerFaction(playerId2);
            if ((objA == null) || (objB == null))
            {
                return MyRelationsBetweenPlayers.Enemies;
            }
            return (!ReferenceEquals(objA, objB) ? ((MySession.Static.Factions.GetRelationBetweenFactions(objA.FactionId, objB.FactionId) != MyRelationsBetweenFactions.Neutral) ? MyRelationsBetweenPlayers.Enemies : MyRelationsBetweenPlayers.Neutral) : MyRelationsBetweenPlayers.Allies);
        }

        public MyRelationsBetweenPlayerAndBlock GetRelationTo(long playerId) => 
            ((this.Identity != null) ? GetRelationBetweenPlayers(this.Identity.IdentityId, playerId) : MyRelationsBetweenPlayerAndBlock.Enemies);

        public void Init(MyObjectBuilder_Player objectBuilder)
        {
            this.DisplayName = objectBuilder.DisplayName;
            this.Identity = Sync.Players.TryGetIdentity(objectBuilder.IdentityId);
            this.m_forceRealPlayer = objectBuilder.ForceRealPlayer;
            if (this.m_buildColorHSVSlots.Count < 14)
            {
                int count = this.m_buildColorHSVSlots.Count;
                for (int i = 0; i < (14 - count); i++)
                {
                    this.m_buildColorHSVSlots.Add(MyRenderComponentBase.OldBlackToHSV);
                }
            }
            if ((objectBuilder.BuildColorSlots == null) || (objectBuilder.BuildColorSlots.Count == 0))
            {
                this.SetDefaultColors();
            }
            else if (objectBuilder.BuildColorSlots.Count == 14)
            {
                this.m_buildColorHSVSlots = objectBuilder.BuildColorSlots;
            }
            else if (objectBuilder.BuildColorSlots.Count > 14)
            {
                this.m_buildColorHSVSlots = new List<Vector3>(14);
                for (int i = 0; i < 14; i++)
                {
                    this.m_buildColorHSVSlots.Add(objectBuilder.BuildColorSlots[i]);
                }
            }
            else
            {
                this.m_buildColorHSVSlots = objectBuilder.BuildColorSlots;
                for (int i = this.m_buildColorHSVSlots.Count - 1; i < 14; i++)
                {
                    this.m_buildColorHSVSlots.Add(MyRenderComponentBase.OldBlackToHSV);
                }
            }
            if (Sync.IsServer && (this.Id.SerialId == 0))
            {
                if (MyCubeBuilder.AllPlayersColors == null)
                {
                    MyCubeBuilder.AllPlayersColors = new Dictionary<PlayerId, List<Vector3>>();
                }
                if (!MyCubeBuilder.AllPlayersColors.ContainsKey(this.Id))
                {
                    MyCubeBuilder.AllPlayersColors.Add(this.Id, this.m_buildColorHSVSlots);
                }
                else
                {
                    MyCubeBuilder.AllPlayersColors.TryGetValue(this.Id, out this.m_buildColorHSVSlots);
                }
            }
        }

        private static void InitDefaultColors()
        {
            if (m_buildColorDefaults.Count < 14)
            {
                int count = m_buildColorDefaults.Count;
                for (int j = 0; j < (14 - count); j++)
                {
                    m_buildColorDefaults.Add(MyRenderComponentBase.OldBlackToHSV);
                }
            }
            m_buildColorDefaults[0] = MyRenderComponentBase.OldGrayToHSV;
            m_buildColorDefaults[1] = MyRenderComponentBase.OldRedToHSV;
            m_buildColorDefaults[2] = MyRenderComponentBase.OldGreenToHSV;
            m_buildColorDefaults[3] = MyRenderComponentBase.OldBlueToHSV;
            m_buildColorDefaults[4] = MyRenderComponentBase.OldYellowToHSV;
            m_buildColorDefaults[5] = MyRenderComponentBase.OldWhiteToHSV;
            m_buildColorDefaults[6] = MyRenderComponentBase.OldBlackToHSV;
            for (int i = 7; i < 14; i++)
            {
                m_buildColorDefaults[i] = m_buildColorDefaults[i - 7] + new Vector3(0f, 0.15f, 0.2f);
            }
        }

        public static bool IsColorsSetToDefaults(List<Vector3> colors)
        {
            if (colors.Count != 14)
            {
                return false;
            }
            for (int i = 0; i < 14; i++)
            {
                if (colors[i] != m_buildColorDefaults[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void ReleaseControls()
        {
            this.Controller.SaveCamera();
            if (this.Controller.ControlledEntity != null)
            {
                this.Controller.ControlledEntity.ControllerInfo.ReleaseControls();
            }
        }

        public void RemoveGrid(long gridEntityId)
        {
            this.Grids.Remove(gridEntityId);
        }

        public void SetBuildColorSlots(List<Vector3> newColors)
        {
            for (int i = 0; i < 14; i++)
            {
                this.m_buildColorHSVSlots[i] = MyRenderComponentBase.OldBlackToHSV;
            }
            for (int j = 0; j < Math.Min(newColors.Count, 14); j++)
            {
                this.m_buildColorHSVSlots[j] = newColors[j];
            }
            if ((MyCubeBuilder.AllPlayersColors != null) && MyCubeBuilder.AllPlayersColors.Remove(this.Id))
            {
                MyCubeBuilder.AllPlayersColors.Add(this.Id, this.m_buildColorHSVSlots);
            }
        }

        public void SetDefaultColors()
        {
            for (int i = 0; i < 14; i++)
            {
                this.m_buildColorHSVSlots[i] = m_buildColorDefaults[i];
            }
        }

        public unsafe void SpawnAt(MatrixD worldMatrix, Vector3 velocity, MyEntity spawnedBy, MyBotDefinition botDefinition, bool findFreePlace = true, string modelName = null, Color? color = new Color?())
        {
            if (Sync.IsServer && (this.Identity != null))
            {
                Vector3? nullable1;
                if (color != null)
                {
                    nullable1 = new Vector3?(color.Value.ToVector3());
                }
                else
                {
                    nullable1 = null;
                }
                bool useInventory = this.Id.SerialId == 0;
                MyCharacter newCharacter = MyCharacter.CreateCharacter(worldMatrix, velocity, this.Identity.DisplayName, modelName ?? this.Identity.Model, nullable1, botDefinition, false, false, null, useInventory, this.Identity.IdentityId, true);
                if (findFreePlace)
                {
                    float radius = (newCharacter.Render.GetModel().BoundingBox.Size.Length() / 2f) * 0.9f;
                    Vector3 up = (Vector3) worldMatrix.Up;
                    up.Normalize();
                    Vector3 vector2 = up * (radius + 0.01f);
                    MatrixD xd = worldMatrix;
                    xd.Translation = worldMatrix.Translation + vector2;
                    MatrixD* xdPtr1 = (MatrixD*) ref xd;
                    Vector3D? nullable2 = MyEntities.FindFreePlace(ref (MatrixD) ref xdPtr1, (Vector3) xd.GetDirectionVector(Base6Directions.Direction.Up), radius, 200, 15, 0.2f);
                    if (nullable2 == null)
                    {
                        MatrixD* xdPtr2 = (MatrixD*) ref xd;
                        nullable2 = MyEntities.FindFreePlace(ref (MatrixD) ref xdPtr2, (Vector3) xd.GetDirectionVector(Base6Directions.Direction.Right), radius, 200, 15, 0.2f);
                        if (nullable2 == null)
                        {
                            nullable2 = MyEntities.FindFreePlace(worldMatrix.Translation + vector2, radius, 200, 15, 0.2f);
                        }
                    }
                    if (nullable2 != null)
                    {
                        worldMatrix.Translation = nullable2.Value - vector2;
                        newCharacter.PositionComp.SetWorldMatrix(worldMatrix, null, false, true, true, false, false, false);
                    }
                }
                Sync.Players.SetPlayerCharacter(this, newCharacter, spawnedBy);
                Sync.Players.RevivePlayer(this);
            }
        }

        public void SpawnIntoCharacter(MyCharacter character)
        {
            Sync.Players.SetPlayerCharacter(this, character, null);
            Sync.Players.RevivePlayer(this);
        }

        void IMyPlayer.AddGrid(long gridEntityId)
        {
            this.AddGrid(gridEntityId);
        }

        void IMyPlayer.ChangeOrSwitchToColor(Vector3 color)
        {
            this.ChangeOrSwitchToColor(color);
        }

        Vector3D IMyPlayer.GetPosition() => 
            this.GetPosition();

        MyRelationsBetweenPlayerAndBlock IMyPlayer.GetRelationTo(long playerId) => 
            this.GetRelationTo(playerId);

        void IMyPlayer.RemoveGrid(long gridEntityId)
        {
            this.RemoveGrid(gridEntityId);
        }

        void IMyPlayer.SetDefaultColors()
        {
            this.SetDefaultColors();
        }

        void IMyPlayer.SpawnAt(MatrixD worldMatrix, Vector3 velocity, IMyEntity spawnedBy)
        {
            Color? color = null;
            this.SpawnAt(worldMatrix, velocity, (MyEntity) spawnedBy, null, true, null, color);
        }

        void IMyPlayer.SpawnAt(MatrixD worldMatrix, Vector3 velocity, IMyEntity spawnedBy, bool findFreePlace, string modelName, Color? color)
        {
            this.SpawnAt(worldMatrix, velocity, (MyEntity) spawnedBy, null, findFreePlace, modelName, color);
        }

        void IMyPlayer.SpawnIntoCharacter(IMyCharacter character)
        {
            this.SpawnIntoCharacter((MyCharacter) character);
        }

        public MyNetworkClient Client =>
            this.m_client;

        public MyIdentity Identity
        {
            get => 
                this.m_identity;
            set
            {
                this.m_identity = value;
                if (this.IdentityChanged != null)
                {
                    this.IdentityChanged(this, value);
                }
            }
        }

        public MyEntityController Controller { get; private set; }

        public string DisplayName { get; private set; }

        public int SelectedBuildColorSlot
        {
            get => 
                this.m_selectedBuildColorSlot;
            set => 
                (this.m_selectedBuildColorSlot = MathHelper.Clamp(value, 0, this.m_buildColorHSVSlots.Count - 1));
        }

        public Vector3 SelectedBuildColor
        {
            get => 
                this.m_buildColorHSVSlots[this.m_selectedBuildColorSlot];
            set => 
                (this.m_buildColorHSVSlots[this.m_selectedBuildColorSlot] = value);
        }

        public static int SelectedColorSlot =>
            ((MySession.Static.LocalHumanPlayer != null) ? MySession.Static.LocalHumanPlayer.SelectedBuildColorSlot : 0);

        public static Vector3 SelectedColor =>
            ((MySession.Static.LocalHumanPlayer != null) ? MySession.Static.LocalHumanPlayer.SelectedBuildColor : m_buildColorDefaults[0]);

        public static ListReader<Vector3> ColorSlots =>
            ((MySession.Static.LocalHumanPlayer != null) ? MySession.Static.LocalHumanPlayer.BuildColorSlots : new ListReader<Vector3>(m_buildColorDefaults));

        public static ListReader<Vector3> DefaultBuildColorSlots =>
            m_buildColorDefaults;

        public List<Vector3> BuildColorSlots
        {
            get => 
                this.m_buildColorHSVSlots;
            set => 
                this.SetBuildColorSlots(value);
        }

        public bool IsLocalPlayer =>
            ReferenceEquals(this.m_client, Sync.Clients.LocalClient);

        public bool IsRemotePlayer =>
            !ReferenceEquals(this.m_client, Sync.Clients.LocalClient);

        public bool IsRealPlayer =>
            (this.m_forceRealPlayer || (this.Id.SerialId == 0));

        public bool IsBot =>
            !this.IsRealPlayer;

        public bool IsImmortal =>
            (this.IsRealPlayer && (this.Id.SerialId != 0));

        public MyCharacter Character =>
            this.Identity.Character;

        public PlayerId Id { get; protected set; }

        public List<long> RespawnShip =>
            ((this.m_identity != null) ? this.m_identity.RespawnShips : null);

        IMyNetworkClient IMyPlayer.Client =>
            this.Client;

        HashSet<long> IMyPlayer.Grids =>
            this.Grids;

        IMyEntityController IMyPlayer.Controller =>
            this.Controller;

        string IMyPlayer.DisplayName =>
            this.DisplayName;

        ulong IMyPlayer.SteamUserId =>
            this.Id.SteamId;

        long IMyPlayer.PlayerID =>
            this.Identity.IdentityId;

        long IMyPlayer.IdentityId =>
            this.Identity.IdentityId;

        bool IMyPlayer.IsAdmin =>
            MySession.Static.IsUserAdmin(this.Id.SteamId);

        bool IMyPlayer.IsPromoted =>
            MySession.Static.IsUserSpaceMaster(this.Id.SteamId);

        MyPromoteLevel IMyPlayer.PromoteLevel =>
            MySession.Static.GetUserPromoteLevel(this.Id.SteamId);

        IMyCharacter IMyPlayer.Character =>
            this.Character;

        Vector3 IMyPlayer.SelectedBuildColor
        {
            get => 
                this.SelectedBuildColor;
            set => 
                (this.SelectedBuildColor = value);
        }

        int IMyPlayer.SelectedBuildColorSlot
        {
            get => 
                this.SelectedBuildColorSlot;
            set => 
                (this.SelectedBuildColorSlot = value);
        }

        bool IMyPlayer.IsBot =>
            this.IsBot;

        IMyIdentity IMyPlayer.Identity =>
            this.Identity;

        ListReader<long> IMyPlayer.RespawnShip =>
            ((this.m_identity != null) ? ((ListReader<long>) this.m_identity.RespawnShips) : ((ListReader<long>) 0));

        List<Vector3> IMyPlayer.BuildColorSlots
        {
            get => 
                this.BuildColorSlots;
            set => 
                (this.BuildColorSlots = value);
        }

        ListReader<Vector3> IMyPlayer.DefaultBuildColorSlots =>
            DefaultBuildColorSlots;

        [StructLayout(LayoutKind.Sequential)]
        public struct PlayerId : IComparable<MyPlayer.PlayerId>
        {
            public ulong SteamId;
            public int SerialId;
            public static readonly PlayerIdComparerType Comparer;
            public bool IsValid =>
                (this.SteamId != 0L);
            public PlayerId(ulong steamId) : this(steamId, 0)
            {
            }

            public PlayerId(ulong steamId, int serialId)
            {
                this.SteamId = steamId;
                this.SerialId = serialId;
            }

            public static bool operator ==(MyPlayer.PlayerId a, MyPlayer.PlayerId b) => 
                ((a.SteamId == b.SteamId) && (a.SerialId == b.SerialId));

            public static bool operator !=(MyPlayer.PlayerId a, MyPlayer.PlayerId b) => 
                !(a == b);

            public override string ToString() => 
                (this.SteamId.ToString() + ":" + this.SerialId.ToString());

            public override bool Equals(object obj) => 
                ((obj is MyPlayer.PlayerId) && (((MyPlayer.PlayerId) obj) == this));

            public override int GetHashCode() => 
                ((this.SteamId.GetHashCode() * 0x23b) ^ this.SerialId.GetHashCode());

            public int CompareTo(MyPlayer.PlayerId other) => 
                ((this.SteamId >= other.SteamId) ? ((this.SteamId <= other.SteamId) ? ((this.SerialId >= other.SerialId) ? ((this.SerialId <= other.SerialId) ? 0 : 1) : -1) : 1) : -1);

            public static unsafe MyPlayer.PlayerId operator ++(MyPlayer.PlayerId id)
            {
                int* numPtr1 = (int*) ref id.SerialId;
                numPtr1[0]++;
                return id;
            }

            public static unsafe MyPlayer.PlayerId operator --(MyPlayer.PlayerId id)
            {
                int* numPtr1 = (int*) ref id.SerialId;
                numPtr1[0]--;
                return id;
            }

            static PlayerId()
            {
                Comparer = new PlayerIdComparerType();
            }
            public class PlayerIdComparerType : IEqualityComparer<MyPlayer.PlayerId>
            {
                public bool Equals(MyPlayer.PlayerId left, MyPlayer.PlayerId right) => 
                    (left == right);

                public int GetHashCode(MyPlayer.PlayerId playerId) => 
                    playerId.GetHashCode();
            }
        }
    }
}

