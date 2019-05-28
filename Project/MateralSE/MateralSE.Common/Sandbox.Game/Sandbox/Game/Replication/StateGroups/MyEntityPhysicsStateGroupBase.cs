namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Replication.History;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    public abstract class MyEntityPhysicsStateGroupBase : IMyStateGroup, IMyNetObject, IMyEventOwner
    {
        protected IMySnapshotSync m_snapshotSync;
        protected bool m_forcedWorldSnapshots;
        private bool m_physicsActive;
        private const float MIN_SIZE = 10f;
        private const float MIN_ACCELERATION = 5f;
        private readonly List<MyEntity> m_tmpEntityResults = new List<MyEntity>();
        protected bool m_supportInited;
        protected long m_lastSupportId;

        public MyEntityPhysicsStateGroupBase(MyEntity entity, IMyReplicable ownerReplicable, bool createSync = true)
        {
            this.Entity = entity;
            this.Owner = ownerReplicable;
            if (!Sync.IsServer & createSync)
            {
                this.m_snapshotSync = new MyAnimatedSnapshotSync(this.Entity);
            }
            if (Sync.IsServer)
            {
                this.Entity.AddedToScene += new Action<MyEntity>(this.RegisterPhysics);
            }
        }

        private void ActiveStateChanged(MyPhysicsComponentBase physics, bool active)
        {
            this.m_physicsActive = active;
            if (active)
            {
                MyMultiplayer.GetReplicationServer().AddToDirtyGroups(this);
            }
        }

        public abstract void ClientUpdate(MyTimeSpan clientTimestamp);
        public void CreateClientData(MyClientStateBase forClient)
        {
        }

        public virtual void Destroy()
        {
            if (this.Entity.Physics != null)
            {
                this.Entity.Physics.OnBodyActiveStateChanged -= new Action<MyPhysicsComponentBase, bool>(this.ActiveStateChanged);
            }
            if (!Sync.IsServer)
            {
                this.m_snapshotSync.Destroy();
            }
        }

        public void DestroyClientData(MyClientStateBase forClient)
        {
        }

        public void ForceSend(MyClientStateBase clientData)
        {
        }

        public MyStreamProcessingState IsProcessingForClient(Endpoint forClient) => 
            MyStreamProcessingState.None;

        public virtual bool IsStillDirty(Endpoint forClient) => 
            this.m_physicsActive;

        public void OnAck(MyClientStateBase forClient, byte packetId, bool delivered)
        {
        }

        private void OnPhysicsComponentChanged(MyPhysicsComponentBase oldComponent, MyPhysicsComponentBase newComponent)
        {
            if (oldComponent != null)
            {
                oldComponent.OnBodyActiveStateChanged -= new Action<MyPhysicsComponentBase, bool>(this.ActiveStateChanged);
            }
            if (newComponent != null)
            {
                this.m_physicsActive = newComponent.IsActive;
                newComponent.OnBodyActiveStateChanged += new Action<MyPhysicsComponentBase, bool>(this.ActiveStateChanged);
            }
        }

        private void RegisterPhysics(MyEntity obj)
        {
            this.Entity.AddedToScene -= new Action<MyEntity>(this.RegisterPhysics);
            if (this.Entity.Physics != null)
            {
                this.m_physicsActive = this.Entity.Physics.IsActive;
                this.Entity.Physics.OnBodyActiveStateChanged += new Action<MyPhysicsComponentBase, bool>(this.ActiveStateChanged);
                this.Entity.OnPhysicsComponentChanged += new Action<MyPhysicsComponentBase, MyPhysicsComponentBase>(this.OnPhysicsComponentChanged);
            }
        }

        public void Reset(bool reinit, MyTimeSpan clientTimestamp)
        {
            this.m_snapshotSync.Reset(reinit);
            if (reinit)
            {
                this.ClientUpdate(clientTimestamp);
                this.m_snapshotSync.Reset(reinit);
            }
        }

        public virtual void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if (!stream.Writing)
            {
                MySnapshot item = new MySnapshot(stream);
                this.m_snapshotSync.Read(ref item, serverTimestamp);
                if (stream.ReadBool())
                {
                    this.Entity.DeserializeControls(stream, false);
                }
            }
            else
            {
                new MySnapshot(this.Entity, false, true).Write(stream);
                bool isControlled = this.IsControlled;
                stream.WriteBool(isControlled);
                if (isControlled)
                {
                    this.Entity.SerializeControls(stream);
                }
            }
        }

        protected long UpdateParenting(ParentingSetup parentingSetup, long currentParentId)
        {
            List<MyEntity> tmpEntityResults = this.m_tmpEntityResults;
            MyCubeGrid grid = null;
            BoundingBoxD worldAABB = this.Entity.PositionComp.WorldAABB;
            MyEntities.GetTopMostEntitiesInBox(ref worldAABB.Inflate((double) parentingSetup.MaxParentDisconnectDistance), tmpEntityResults, MyEntityQueryType.Dynamic);
            bool flag = false;
            float maxValue = float.MaxValue;
            float num2 = float.MaxValue;
            BoundingBox localAABB = this.Entity.PositionComp.LocalAABB;
            float num3 = localAABB.Size.LengthSquared();
            float num4 = this.Entity.Physics.LinearVelocity.LengthSquared();
            foreach (MyEntity entity in tmpEntityResults)
            {
                if (entity.EntityId == this.Entity.EntityId)
                {
                    continue;
                }
                MyCubeGrid grid2 = entity as MyCubeGrid;
                if ((grid2 != null) && ((grid2.Physics != null) && (grid2.BlocksCount > 1)))
                {
                    float num6 = grid2.PositionComp.LocalAABB.Size.LengthSquared();
                    if (num6 > num3)
                    {
                        bool flag2 = currentParentId == grid2.EntityId;
                        if (grid2.PositionComp.WorldAABB.Contains(this.Entity.PositionComp.WorldAABB) == ContainmentType.Contains)
                        {
                            if (!flag)
                            {
                                grid = null;
                                flag = true;
                            }
                            float num7 = flag2 ? parentingSetup.MinDisconnectInsideParentSpeed : parentingSetup.MinInsideParentSpeed;
                            if ((grid2.Physics.GetVelocityAtPoint(this.Entity.PositionComp.GetPosition()).LengthSquared() >= (num7 * num7)) && (num6 < maxValue))
                            {
                                maxValue = num6;
                                grid = grid2;
                            }
                            continue;
                        }
                        if (!flag)
                        {
                            float num8 = flag2 ? parentingSetup.MinDisconnectParentSpeed : parentingSetup.MinParentSpeed;
                            float num9 = num8 / 2f;
                            if (num4 >= (num9 * num9))
                            {
                                float num10 = flag2 ? parentingSetup.MaxParentDisconnectDistance : parentingSetup.MaxParentDistance;
                                float num11 = flag2 ? parentingSetup.MaxDisconnectParentAcceleration : parentingSetup.MaxParentAcceleration;
                                float num12 = (float) grid2.PositionComp.WorldAABB.DistanceSquared(this.Entity.PositionComp.GetPosition());
                                if ((num12 <= (num10 * num10)) && ((grid2.Physics.GetVelocityAtPoint(this.Entity.PositionComp.GetPosition()).LengthSquared() >= (num8 * num8)) && ((grid2.Physics.LinearAcceleration.LengthSquared() <= (num11 * num11)) && (num12 < num2))))
                                {
                                    num2 = num12;
                                    grid = grid2;
                                }
                            }
                        }
                    }
                }
            }
            tmpEntityResults.Clear();
            long num5 = (grid != null) ? grid.EntityId : 0L;
            if ((!this.m_supportInited || (this.m_lastSupportId == num5)) || (num5 == 0))
            {
                currentParentId = num5;
            }
            this.m_lastSupportId = num5;
            this.m_supportInited = true;
            return currentParentId;
        }

        public IMyReplicable Owner { get; private set; }

        public MyEntity Entity { get; private set; }

        public bool IsHighPriority
        {
            get
            {
                if (this.m_forcedWorldSnapshots)
                {
                    return true;
                }
                if ((this.Entity.PositionComp.LocalAABB.Size.LengthSquared() <= 100f) || (this.Entity.Physics == null))
                {
                    return false;
                }
                return (this.Entity.Physics.LinearAcceleration.LengthSquared() > 25f);
            }
        }

        public bool IsStreaming =>
            false;

        public bool NeedsUpdate =>
            true;

        protected bool IsControlled =>
            (Sync.Players.GetControllingPlayer(this.Entity) != null);

        protected bool IsControlledLocally =>
            ReferenceEquals(MySession.Static.TopMostControlledEntity, this.Entity);

        public bool IsValid =>
            !this.Entity.MarkedForClose;

        public class ParentingSetup
        {
            public float MaxParentDistance;
            public float MinParentSpeed;
            public float MaxParentAcceleration;
            public float MinInsideParentSpeed;
            public float MaxParentDisconnectDistance;
            public float MinDisconnectParentSpeed;
            public float MaxDisconnectParentAcceleration;
            public float MinDisconnectInsideParentSpeed;
        }
    }
}

