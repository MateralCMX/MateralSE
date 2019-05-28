namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRageMath;

    public class MyBreakableEnvironmentProxy : IMyEnvironmentModuleProxy
    {
        private const int BrokenItemLifeSpan = 0x4e20;
        private int m_scheduledBreaksCount;
        private ConcurrentDictionary<int, BreakAtData> m_scheduledBreaks = new ConcurrentDictionary<int, BreakAtData>();
        private readonly Action m_BreakAtDelegate;
        private MyEnvironmentSector m_sector;

        public MyBreakableEnvironmentProxy()
        {
            this.m_BreakAtDelegate = new Action(this.BreakAtInvoke);
        }

        public void BreakAt(int itemId, Vector3D hitpos, Vector3D hitnormal, double impactEnergy)
        {
            double num1 = MathHelper.Clamp(impactEnergy, 0.0, this.ItemResilience(itemId) * 10.0);
            impactEnergy = num1;
            Impact eventData = new Impact(hitpos, hitnormal, impactEnergy);
            this.m_sector.RaiseItemEvent<MyBreakableEnvironmentProxy, Impact>(this, itemId, eventData, false);
            this.DisableItemAndCreateDebris(ref eventData, itemId);
        }

        private void BreakAtInvoke()
        {
            foreach (BreakAtData data in this.m_scheduledBreaks.Values)
            {
                this.BreakAt(data.itemId, data.hitpos, data.hitnormal, data.impactEnergy);
            }
            this.m_scheduledBreaks.Clear();
            this.m_scheduledBreaksCount = 0;
        }

        public void Close()
        {
            this.m_sector.OnContactPoint -= new MySectorContactEvent(this.SectorOnContactPoint);
        }

        public void CommitLodChange(int lodBefore, int lodAfter)
        {
        }

        public void CommitPhysicsChange(bool enabled)
        {
        }

        private unsafe MyEntity CreateDebris(int itemId)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_sector.DataView.Items[itemId];
            Vector3D vectord = info.Position + this.m_sector.SectorCenter;
            MyPhysicalModelDefinition modelForId = this.m_sector.Owner.GetModelForId(info.ModelIndex);
            string modelAsset = modelForId.Model.Insert(modelForId.Model.Length - 4, "_broken");
            bool flag = false;
            string model = modelForId.Model;
            if (MyModels.GetModelOnlyData(modelAsset) != null)
            {
                flag = true;
                model = modelAsset;
            }
            MyEntity entity1 = MyDebris.Static.CreateTreeDebris(model);
            MyDebrisBase.MyDebrisBaseLogic gameLogic = (MyDebrisBase.MyDebrisBaseLogic) entity1.GameLogic;
            gameLogic.LifespanInMiliseconds = 0x4e20;
            MatrixD position = MatrixD.CreateFromQuaternion(info.Rotation);
            MatrixD* xdPtr1 = (MatrixD*) ref position;
            xdPtr1.Translation = vectord + (position.Up * (flag ? ((double) 0) : ((double) 5)));
            gameLogic.Start(position, Vector3.Zero, false);
            return entity1;
        }

        public void DebugDraw()
        {
        }

        private void DisableItemAndCreateDebris(ref Impact imp, int itemId)
        {
            if (this.m_sector.GetModelIndex(itemId) >= 0)
            {
                MyParticleEffect effect;
                MyParticlesManager.TryCreateParticleEffect("Tree Destruction", MatrixD.CreateTranslation(imp.Position), out effect);
                if (this.m_sector.LodLevel <= 1)
                {
                    MyEntity entity = this.CreateDebris(itemId);
                    if (entity != null)
                    {
                        float mass = entity.Physics.Mass;
                        Vector3D vectord = (Vector3D) (((((float) Math.Sqrt(imp.Energy / ((double) mass))) / (0.01666667f * MyFakes.SIMULATION_SPEED)) * 0.8f) * imp.Normal);
                        Vector3D vectord2 = entity.Physics.CenterOfMassWorld + (entity.WorldMatrix.Up * (imp.Position - entity.Physics.CenterOfMassWorld).Length());
                        Vector3? torque = null;
                        float? maxSpeed = null;
                        entity.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE, new Vector3?((Vector3) vectord), new Vector3D?(vectord2), torque, maxSpeed, true, false);
                    }
                }
                this.m_sector.EnableItem(itemId, false);
            }
        }

        public void HandleSyncEvent(int item, object data, bool fromClient)
        {
            Impact imp = (Impact) data;
            this.DisableItemAndCreateDebris(ref imp, item);
        }

        public void Init(MyEnvironmentSector sector, List<int> items)
        {
            this.m_sector = sector;
            if (Sync.IsServer)
            {
                this.m_sector.OnContactPoint += new MySectorContactEvent(this.SectorOnContactPoint);
            }
        }

        private double ItemResilience(int itemId) => 
            200000.0;

        public void OnItemChange(int item, short newModel)
        {
        }

        public void OnItemChangeBatch(List<int> items, int offset, short newModel)
        {
        }

        private void SectorOnContactPoint(int itemId, MyEntity other, ref MyPhysics.MyContactPointEvent e)
        {
            if (this.m_sector.DataView.Items[itemId].ModelIndex >= 0)
            {
                float num = Math.Abs(e.ContactPointEvent.SeparatingVelocity);
                if (((((other != null) && (other.Physics != null)) && !(other is MyFloatingObject)) && !(other is IMyHandheldGunObject<MyDeviceBase>)) && ((other.Physics.RigidBody == null) || (other.Physics.RigidBody.Layer != 20)))
                {
                    float mass;
                    MyCubeGrid localGrid = other as MyCubeGrid;
                    if (localGrid != null)
                    {
                        mass = MyGridPhysicalGroupData.GetGroupSharedProperties(localGrid, false).Mass;
                    }
                    else
                    {
                        mass = MyDestructionHelper.MassFromHavok(other.Physics.Mass);
                    }
                    double impactEnergy = (num * num) * mass;
                    if (impactEnergy > this.ItemResilience(itemId))
                    {
                        int num4 = Interlocked.Increment(ref this.m_scheduledBreaksCount);
                        Vector3D position = e.Position;
                        if (this.m_scheduledBreaks.TryAdd(itemId, new BreakAtData(itemId, position, e.ContactPointEvent.ContactPoint.Normal, impactEnergy)) && (num4 == 1))
                        {
                            MySandboxGame.Static.Invoke(this.m_BreakAtDelegate, "MyBreakableEnvironmentProxy::BreakAt");
                        }
                    }
                    if (other is MyMeteor)
                    {
                        this.m_sector.EnableItem(itemId, false);
                    }
                }
            }
        }

        public long SectorId =>
            this.m_sector.SectorId;

        [StructLayout(LayoutKind.Sequential)]
        private struct BreakAtData
        {
            public readonly int itemId;
            public readonly Vector3D hitpos;
            public readonly Vector3D hitnormal;
            public readonly double impactEnergy;
            public BreakAtData(int itemId, Vector3D hitpos, Vector3D hitnormal, double impactEnergy)
            {
                this.itemId = itemId;
                this.hitpos = hitpos;
                this.hitnormal = hitnormal;
                this.impactEnergy = impactEnergy;
            }
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        private struct Impact
        {
            public Vector3D Position;
            public Vector3D Normal;
            public double Energy;
            public Impact(Vector3D position, Vector3D normal, double energy)
            {
                this.Position = position;
                this.Normal = normal;
                this.Energy = energy;
            }
        }
    }
}

