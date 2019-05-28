namespace Sandbox.Game.World
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyIdentity : IMyIdentity
    {
        private MyBlockLimits m_blockLimits;
        private static readonly Dictionary<string, short> EmptyBlockTypeLimitDictionary = new Dictionary<string, short>();
        public List<long> RespawnShips;
        [CompilerGenerated]
        private Action<MyCharacter, MyCharacter> CharacterChanged;
        [CompilerGenerated]
        private Action<MyFaction, MyFaction> FactionChanged;

        public event Action<MyCharacter, MyCharacter> CharacterChanged
        {
            [CompilerGenerated] add
            {
                Action<MyCharacter, MyCharacter> characterChanged = this.CharacterChanged;
                while (true)
                {
                    Action<MyCharacter, MyCharacter> a = characterChanged;
                    Action<MyCharacter, MyCharacter> action3 = (Action<MyCharacter, MyCharacter>) Delegate.Combine(a, value);
                    characterChanged = Interlocked.CompareExchange<Action<MyCharacter, MyCharacter>>(ref this.CharacterChanged, action3, a);
                    if (ReferenceEquals(characterChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyCharacter, MyCharacter> characterChanged = this.CharacterChanged;
                while (true)
                {
                    Action<MyCharacter, MyCharacter> source = characterChanged;
                    Action<MyCharacter, MyCharacter> action3 = (Action<MyCharacter, MyCharacter>) Delegate.Remove(source, value);
                    characterChanged = Interlocked.CompareExchange<Action<MyCharacter, MyCharacter>>(ref this.CharacterChanged, action3, source);
                    if (ReferenceEquals(characterChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<MyFaction, MyFaction> FactionChanged
        {
            [CompilerGenerated] add
            {
                Action<MyFaction, MyFaction> factionChanged = this.FactionChanged;
                while (true)
                {
                    Action<MyFaction, MyFaction> a = factionChanged;
                    Action<MyFaction, MyFaction> action3 = (Action<MyFaction, MyFaction>) Delegate.Combine(a, value);
                    factionChanged = Interlocked.CompareExchange<Action<MyFaction, MyFaction>>(ref this.FactionChanged, action3, a);
                    if (ReferenceEquals(factionChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyFaction, MyFaction> factionChanged = this.FactionChanged;
                while (true)
                {
                    Action<MyFaction, MyFaction> source = factionChanged;
                    Action<MyFaction, MyFaction> action3 = (Action<MyFaction, MyFaction>) Delegate.Remove(source, value);
                    factionChanged = Interlocked.CompareExchange<Action<MyFaction, MyFaction>>(ref this.FactionChanged, action3, source);
                    if (ReferenceEquals(factionChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        event Action<IMyCharacter, IMyCharacter> IMyIdentity.CharacterChanged
        {
            add
            {
                this.CharacterChanged += this.GetDelegate(value);
            }
            remove
            {
                this.CharacterChanged -= this.GetDelegate(value);
            }
        }

        private MyIdentity(MyObjectBuilder_Identity objectBuilder)
        {
            MyEntity entity;
            Vector3? nullable2;
            Vector3? nullable1;
            this.RespawnShips = new List<long>();
            SerializableVector3? colorMask = objectBuilder.ColorMask;
            if (colorMask != null)
            {
                nullable1 = new Vector3?(colorMask.GetValueOrDefault());
            }
            else
            {
                nullable2 = null;
                nullable1 = nullable2;
            }
            this.Init(objectBuilder.DisplayName, MyEntityIdentifier.FixObsoleteIdentityType(objectBuilder.IdentityId), objectBuilder.Model, nullable1, objectBuilder.BlockLimitModifier, new DateTime?(objectBuilder.LastLoginTime), new DateTime?(objectBuilder.LastLogoutTime));
            MyEntityIdentifier.MarkIdUsed(this.IdentityId);
            if (objectBuilder.ColorMask != null)
            {
                Vector3? nullable3;
                colorMask = objectBuilder.ColorMask;
                if (colorMask != null)
                {
                    nullable3 = new Vector3?(colorMask.GetValueOrDefault());
                }
                else
                {
                    nullable2 = null;
                    nullable3 = nullable2;
                }
                this.ColorMask = nullable3;
            }
            this.IsDead = true;
            MyEntities.TryGetEntityById(objectBuilder.CharacterEntityId, out entity, false);
            if (entity is MyCharacter)
            {
                this.Character = entity as MyCharacter;
            }
            if (objectBuilder.SavedCharacters != null)
            {
                this.SavedCharacters = objectBuilder.SavedCharacters;
            }
            if (objectBuilder.RespawnShips != null)
            {
                this.RespawnShips = objectBuilder.RespawnShips;
            }
            this.LastDeathPosition = objectBuilder.LastDeathPosition;
        }

        private MyIdentity(string name, long identityId, string model, Vector3? colorMask)
        {
            this.RespawnShips = new List<long>();
            long num1 = MyEntityIdentifier.FixObsoleteIdentityType(identityId);
            identityId = num1;
            DateTime? loginTime = null;
            loginTime = null;
            this.Init(name, identityId, model, colorMask, 0, loginTime, loginTime);
            MyEntityIdentifier.MarkIdUsed(identityId);
        }

        private MyIdentity(string name, MyEntityIdentifier.ID_OBJECT_TYPE identityType, string model = null, Vector3? colorMask = new Vector3?())
        {
            this.RespawnShips = new List<long>();
            this.IdentityId = MyEntityIdentifier.AllocateId(identityType, MyEntityIdentifier.ID_ALLOCATION_METHOD.SERIAL_START_WITH_1);
            DateTime? loginTime = null;
            loginTime = null;
            this.Init(name, this.IdentityId, model, colorMask, 0, loginTime, loginTime);
        }

        public void ChangeCharacter(MyCharacter character)
        {
            MyCharacter character2 = this.Character;
            if (character2 != null)
            {
                character2.OnClosing -= new Action<MyEntity>(this.OnCharacterClosing);
                character2.CharacterDied -= new Action<MyCharacter>(this.OnCharacterDied);
            }
            this.Character = character;
            if (character != null)
            {
                character.OnClosing += new Action<MyEntity>(this.OnCharacterClosing);
                character.CharacterDied += new Action<MyCharacter>(this.OnCharacterDied);
                this.SaveModelAndColorFromCharacter();
                this.IsDead = character.IsDead;
                if (!this.SavedCharacters.Contains(character.EntityId))
                {
                    this.SavedCharacters.Add(character.EntityId);
                    character.OnClosing += new Action<MyEntity>(this.OnSavedCharacterClosing);
                }
            }
            this.CharacterChanged.InvokeIfNotNull<MyCharacter, MyCharacter>(character2, this.Character);
        }

        private Action<MyCharacter, MyCharacter> GetDelegate(Action<IMyCharacter, IMyCharacter> value) => 
            ((Action<MyCharacter, MyCharacter>) Delegate.CreateDelegate(typeof(Action<MyCharacter, MyCharacter>), value.Target, value.Method));

        public int GetInitialPCU() => 
            MyBlockLimits.GetInitialPCU(this.IdentityId);

        public MyObjectBuilder_Identity GetObjectBuilder()
        {
            SerializableVector3? nullable1;
            MyObjectBuilder_Identity identity1 = new MyObjectBuilder_Identity();
            identity1.IdentityId = this.IdentityId;
            identity1.DisplayName = this.DisplayName;
            identity1.CharacterEntityId = (this.Character == null) ? 0L : this.Character.EntityId;
            MyObjectBuilder_Identity local1 = identity1;
            local1.Model = this.Model;
            Vector3? colorMask = this.ColorMask;
            MyObjectBuilder_Identity identity2 = local1;
            if (colorMask != null)
            {
                nullable1 = new SerializableVector3?(colorMask.GetValueOrDefault());
            }
            else
            {
                nullable1 = null;
            }
            identity2.ColorMask = nullable1;
            MyObjectBuilder_Identity local2 = identity2;
            local2.BlockLimitModifier = this.BlockLimits.BlockLimitModifier;
            local2.LastLoginTime = this.LastLoginTime;
            local2.LastLogoutTime = this.LastLogoutTime;
            local2.SavedCharacters = this.SavedCharacters;
            local2.RespawnShips = this.RespawnShips;
            local2.LastDeathPosition = this.LastDeathPosition;
            return local2;
        }

        private void Init(string name, long identityId, string model, Vector3? colormask, int blockLimitModifier = 0, DateTime? loginTime = new DateTime?(), DateTime? logoutTime = new DateTime?())
        {
            DateTime? nullable;
            this.DisplayName = name;
            this.IdentityId = identityId;
            this.IsDead = true;
            this.Model = model;
            this.ColorMask = colormask;
            this.m_blockLimits = new MyBlockLimits(this.GetInitialPCU(), blockLimitModifier);
            if (MySession.Static.Players.IdentityIsNpc(identityId))
            {
                this.LastLoginTime = DateTime.Now;
            }
            else
            {
                nullable = loginTime;
                this.LastLoginTime = (nullable != null) ? nullable.GetValueOrDefault() : DateTime.Now;
            }
            nullable = logoutTime;
            this.LastLogoutTime = (nullable != null) ? nullable.GetValueOrDefault() : DateTime.Now;
            this.SavedCharacters = new HashSet<long>();
        }

        public void LogRespawnTime()
        {
            this.LastRespawnTime = MySession.Static.ElapsedGameTime;
        }

        private void OnCharacterClosing(MyEntity character)
        {
            this.Character.OnClosing -= new Action<MyEntity>(this.OnCharacterClosing);
            this.Character.CharacterDied -= new Action<MyCharacter>(this.OnCharacterDied);
            this.Character = null;
        }

        private void OnCharacterDied(MyCharacter character)
        {
            this.LastDeathPosition = new Vector3D?(character.PositionComp.GetPosition());
        }

        private void OnSavedCharacterClosing(MyEntity character)
        {
            character.OnClosing -= new Action<MyEntity>(this.OnSavedCharacterClosing);
            this.SavedCharacters.Remove(character.EntityId);
        }

        public void PerformFirstSpawn()
        {
            this.FirstSpawnDone = true;
        }

        public void RaiseFactionChanged(MyFaction oldFaction, MyFaction newFaction)
        {
            this.FactionChanged.InvokeIfNotNull<MyFaction, MyFaction>(oldFaction, newFaction);
        }

        private void SaveModelAndColorFromCharacter()
        {
            this.Model = this.Character.ModelName;
            this.ColorMask = new Vector3?(this.Character.ColorMask);
        }

        public void SetColorMask(Vector3 color)
        {
            this.ColorMask = new Vector3?(color);
        }

        public void SetDead(bool dead)
        {
            this.IsDead = dead;
        }

        public void SetDisplayName(string name)
        {
            this.DisplayName = name;
        }

        public long IdentityId { get; private set; }

        public string DisplayName { get; private set; }

        public MyCharacter Character { get; private set; }

        public HashSet<long> SavedCharacters { get; private set; }

        public string Model { get; private set; }

        public Vector3? ColorMask { get; private set; }

        public bool IsDead { get; private set; }

        public Vector3D? LastDeathPosition { get; private set; }

        public TimeSpan LastRespawnTime { get; private set; }

        public bool FirstSpawnDone { get; private set; }

        public DateTime LastLoginTime { get; set; }

        public DateTime LastLogoutTime { get; set; }

        public MyBlockLimits BlockLimits
        {
            get
            {
                if (MyPirateAntennas.GetPiratesId() == this.IdentityId)
                {
                    return MySession.Static.PirateBlockLimits;
                }
                switch (MySession.Static.BlockLimitsEnabled)
                {
                    case MyBlockLimitsEnabledEnum.GLOBALLY:
                        return MySession.Static.GlobalBlockLimits;

                    case MyBlockLimitsEnabledEnum.PER_FACTION:
                    {
                        MyFaction faction = MySession.Static.Factions.TryGetPlayerFaction(this.IdentityId) as MyFaction;
                        return ((faction == null) ? MyBlockLimits.Empty : faction.BlockLimits);
                    }
                }
                return this.m_blockLimits;
            }
        }

        long IMyIdentity.PlayerId =>
            this.IdentityId;

        long IMyIdentity.IdentityId =>
            this.IdentityId;

        string IMyIdentity.DisplayName =>
            this.DisplayName;

        string IMyIdentity.Model =>
            this.Model;

        Vector3? IMyIdentity.ColorMask =>
            this.ColorMask;

        bool IMyIdentity.IsDead =>
            this.IsDead;

        public class Friend
        {
            public virtual MyIdentity CreateNewIdentity(MyObjectBuilder_Identity objectBuilder) => 
                new MyIdentity(objectBuilder);

            public virtual MyIdentity CreateNewIdentity(string name, string model = null, Vector3? colorMask = new Vector3?()) => 
                new MyIdentity(name, MyEntityIdentifier.ID_OBJECT_TYPE.IDENTITY, model, colorMask);

            public virtual MyIdentity CreateNewIdentity(string name, long identityId, string model, Vector3? colorMask) => 
                new MyIdentity(name, identityId, model, colorMask);
        }
    }
}

