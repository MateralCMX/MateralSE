namespace Sandbox.Game.Replication.StateGroups
{
    using Sandbox;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Replication.History;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Entity;
    using VRage.Library.Collections;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRageMath;

    public class MyEntityPhysicsStateGroup : MyEntityPhysicsStateGroupBase
    {
        private static readonly MyPredictedSnapshotSyncSetup m_settings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_controlledNewParentSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_wheelSettings;
        private static readonly MyPredictedSnapshotSyncSetup m_carSettings;
        public static readonly MyEntityPhysicsStateGroupBase.ParentingSetup GridParentingSetup;
        private readonly MyPredictedSnapshotSync m_predictedSync;
        private readonly IMySnapshotSync m_animatedSync;
        private MyTimeSpan m_lastParentTime;
        private long m_lastParentId;
        private bool m_inheritRotation;
        private const float NEW_PARENT_TIMEOUT = 3f;
        private MyTimeSpan m_lastAnimatedTime;
        private const float SYNC_CHANGE_TIMEOUT = 0.1f;

        static MyEntityPhysicsStateGroup()
        {
            MyPredictedSnapshotSyncSetup setup1 = new MyPredictedSnapshotSyncSetup();
            setup1.ProfileName = "GeneralGrid";
            setup1.ApplyPosition = true;
            setup1.ApplyRotation = true;
            setup1.ApplyPhysicsAngular = true;
            setup1.ApplyPhysicsLinear = true;
            setup1.ExtrapolationSmoothing = true;
            setup1.IsControlled = false;
            setup1.ApplyPhysicsLocal = false;
            setup1.MaxPositionFactor = 100f;
            setup1.MaxLinearFactor = 100f;
            setup1.MaxRotationFactor = 100f;
            setup1.MaxAngularFactor = 1f;
            setup1.IterationsFactor = 1f;
            m_settings = setup1;
            MyPredictedSnapshotSyncSetup setup2 = new MyPredictedSnapshotSyncSetup();
            setup2.ProfileName = "ControlledGrid";
            setup2.ApplyPosition = true;
            setup2.ApplyRotation = true;
            setup2.ApplyPhysicsAngular = true;
            setup2.ApplyPhysicsLinear = true;
            setup2.ExtrapolationSmoothing = true;
            setup2.InheritRotation = false;
            setup2.ApplyPhysicsLocal = true;
            setup2.IsControlled = true;
            setup2.MaxPositionFactor = 100f;
            setup2.MaxLinearFactor = 100f;
            setup2.MaxRotationFactor = 100f;
            setup2.MaxAngularFactor = 10f;
            setup2.MinAngularFactor = 1000f;
            setup2.IterationsFactor = 1f;
            m_controlledSettings = setup2;
            MyPredictedSnapshotSyncSetup setup3 = new MyPredictedSnapshotSyncSetup();
            setup3.ProfileName = "ControlledGridNewParent";
            setup3.ApplyPosition = true;
            setup3.ApplyRotation = true;
            setup3.ApplyPhysicsAngular = true;
            setup3.ApplyPhysicsLinear = true;
            setup3.ExtrapolationSmoothing = true;
            setup3.IsControlled = true;
            setup3.ApplyPhysicsLocal = true;
            setup3.MaxPositionFactor = 100f;
            setup3.MaxLinearFactor = 100f;
            setup3.MaxRotationFactor = 100f;
            setup3.MaxAngularFactor = 10f;
            setup3.MinAngularFactor = 1000f;
            setup3.IterationsFactor = 5f;
            setup3.InheritRotation = false;
            m_controlledNewParentSettings = setup3;
            MyPredictedSnapshotSyncSetup setup4 = new MyPredictedSnapshotSyncSetup();
            setup4.ProfileName = "Wheel";
            setup4.ApplyPosition = false;
            setup4.ApplyRotation = false;
            setup4.ApplyPhysicsAngular = true;
            setup4.ApplyPhysicsLinear = false;
            setup4.ExtrapolationSmoothing = true;
            setup4.IsControlled = false;
            setup4.UpdateAlways = false;
            setup4.MaxPositionFactor = 1f;
            setup4.MaxLinearFactor = 1f;
            setup4.MaxRotationFactor = 1f;
            setup4.MaxAngularFactor = 1f;
            setup4.IterationsFactor = 1f;
            m_wheelSettings = setup4;
            MyPredictedSnapshotSyncSetup setup5 = new MyPredictedSnapshotSyncSetup();
            setup5.ProfileName = "Car";
            setup5.ApplyPosition = true;
            setup5.ApplyRotation = true;
            setup5.ApplyPhysicsAngular = true;
            setup5.ApplyPhysicsLinear = false;
            setup5.ExtrapolationSmoothing = true;
            setup5.InheritRotation = false;
            setup5.ApplyPhysicsLocal = true;
            setup5.IsControlled = true;
            setup5.MaxPositionFactor = 100f;
            setup5.MaxLinearFactor = 100f;
            setup5.MaxRotationFactor = 100f;
            setup5.MaxAngularFactor = 10f;
            setup5.MinAngularFactor = 1000f;
            setup5.IterationsFactor = 1f;
            m_carSettings = setup5;
            MyEntityPhysicsStateGroupBase.ParentingSetup setup6 = new MyEntityPhysicsStateGroupBase.ParentingSetup();
            setup6.MaxParentDistance = 100f;
            setup6.MinParentSpeed = 30f;
            setup6.MaxParentAcceleration = 6f;
            setup6.MinInsideParentSpeed = 20f;
            setup6.MaxParentDisconnectDistance = 100f;
            setup6.MinDisconnectParentSpeed = 25f;
            setup6.MaxDisconnectParentAcceleration = 30f;
            setup6.MinDisconnectInsideParentSpeed = 10f;
            GridParentingSetup = setup6;
        }

        public MyEntityPhysicsStateGroup(MyEntity entity, IMyReplicable ownerReplicable) : base(entity, ownerReplicable, false)
        {
            this.m_predictedSync = new MyPredictedSnapshotSync(base.Entity);
            this.m_animatedSync = new MyAnimatedSnapshotSync(base.Entity);
            base.m_snapshotSync = this.m_animatedSync;
        }

        public override void ClientUpdate(MyTimeSpan clientTimestamp)
        {
            MyPredictedSnapshotSyncSetup settings;
            MyCubeGrid entity = base.Entity as MyCubeGrid;
            if (!((entity != null) && entity.IsClientPredicted))
            {
                if (!ReferenceEquals(base.m_snapshotSync, this.m_animatedSync))
                {
                    this.m_animatedSync.Reset(false);
                }
                base.m_snapshotSync = this.m_animatedSync;
                settings = m_settings;
            }
            else
            {
                if (!ReferenceEquals(base.m_snapshotSync, this.m_predictedSync))
                {
                    this.m_lastAnimatedTime = MySandboxGame.Static.SimulationTime;
                }
                base.m_snapshotSync = this.m_predictedSync;
                if (this.m_inheritRotation)
                {
                    entity.ClosestParentId = 0L;
                }
                else
                {
                    long parentId = this.m_predictedSync.GetParentId();
                    if (parentId != -1L)
                    {
                        entity.ClosestParentId = parentId;
                    }
                }
                bool flag = MySandboxGame.Static.SimulationTime < (this.m_lastParentTime + MyTimeSpan.FromSeconds(3.0));
                if (MySandboxGame.Static.SimulationTime < (this.m_lastAnimatedTime + MyTimeSpan.FromSeconds(0.10000000149011612)))
                {
                    this.m_predictedSync.Reset(true);
                }
                settings = entity.IsClientPredictedWheel ? m_wheelSettings : (entity.IsClientPredictedCar ? m_carSettings : (flag ? m_controlledNewParentSettings : m_controlledSettings));
            }
            long num = base.m_snapshotSync.Update(clientTimestamp, settings);
            if (entity != null)
            {
                if (this.m_inheritRotation)
                {
                    entity.ClosestParentId = 0L;
                }
                else if (num != -1L)
                {
                    entity.ClosestParentId = num;
                }
                entity.ForceDisablePrediction = MyPredictedSnapshotSync.ForceAnimated;
            }
            base.Entity.LastSnapshotFlags = settings;
        }

        public override void Serialize(BitStream stream, Endpoint forClient, MyTimeSpan serverTimestamp, MyTimeSpan lastClientTimestamp, byte packetId, int maxBitPosition, HashSet<string> cachedData)
        {
            if (stream.Writing)
            {
                bool inheritRotation = this.UpdateEntitySupport();
                MySnapshot snapshot = new MySnapshot(base.Entity, false, inheritRotation);
                base.m_forcedWorldSnapshots = snapshot.SkippedParent;
                snapshot.Write(stream);
                stream.WriteBool(true);
                base.Entity.SerializeControls(stream);
            }
            else
            {
                MatrixD xd;
                MySnapshot item = new MySnapshot(stream);
                this.m_inheritRotation = item.InheritRotation;
                item.GetMatrix(base.Entity, out xd, true, true);
                if (item.ParentId != this.m_lastParentId)
                {
                    this.m_lastParentId = item.ParentId;
                    if (base.m_supportInited)
                    {
                        this.m_lastParentTime = MySandboxGame.Static.SimulationTime;
                    }
                    else
                    {
                        base.m_supportInited = true;
                    }
                }
                this.m_animatedSync.Read(ref item, serverTimestamp);
                this.m_predictedSync.Read(ref item, lastClientTimestamp);
                if (stream.ReadBool())
                {
                    base.Entity.DeserializeControls(stream, false);
                }
            }
        }

        private bool UpdateEntitySupport()
        {
            MyCubeGrid entity = base.Entity as MyCubeGrid;
            if (entity != null)
            {
                if (entity.IsClientPredicted)
                {
                    entity.ClosestParentId = base.UpdateParenting(GridParentingSetup, entity.ClosestParentId);
                    return false;
                }
                entity.ClosestParentId = 0L;
            }
            return true;
        }
    }
}

