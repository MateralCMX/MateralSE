namespace Sandbox.Game.World
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using System;
    using VRage.Game;
    using VRage.Library.Collections;
    using VRageMath;

    public abstract class MyWorldGeneratorStartingStateBase
    {
        public string FactionTag;

        protected MyWorldGeneratorStartingStateBase()
        {
        }

        protected virtual void CreateAndSetPlayerFaction()
        {
            if ((Sync.IsServer && (this.FactionTag != null)) && (MySession.Static.LocalHumanPlayer != null))
            {
                MySession.Static.Factions.TryGetOrCreateFactionByTag(this.FactionTag).AcceptJoin(MySession.Static.LocalHumanPlayer.Identity.IdentityId, false);
            }
        }

        protected Vector3D FixPositionToVoxel(Vector3D position)
        {
            MyVoxelMap map = null;
            using (ConcurrentEnumerator<SpinLockRef.Token, MyEntity, HashSet<MyEntity>.Enumerator> enumerator = MyEntities.GetEntities().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (map != null)
                    {
                        break;
                    }
                }
            }
            float maxVertDistance = 2048f;
            if (map != null)
            {
                Vector3D vectord1 = map.GetPositionOnVoxel(position, maxVertDistance);
                position = vectord1;
            }
            return position;
        }

        public virtual MyObjectBuilder_WorldGeneratorPlayerStartingState GetObjectBuilder()
        {
            MyObjectBuilder_WorldGeneratorPlayerStartingState state1 = MyWorldGenerator.StartingStateFactory.CreateObjectBuilder(this);
            state1.FactionTag = this.FactionTag;
            return state1;
        }

        public abstract Vector3D? GetStartingLocation();
        public virtual void Init(MyObjectBuilder_WorldGeneratorPlayerStartingState builder)
        {
            this.FactionTag = builder.FactionTag;
        }

        public abstract void SetupCharacter(MyWorldGenerator.Args generatorArgs);
    }
}

