namespace Sandbox.Game.Replication
{
    using Sandbox.Game.Replication.History;
    using Sandbox.Game.Replication.StateGroups;
    using System;
    using System.Collections.Generic;
    using VRage.Network;

    public class MyFloatingObjectReplicable : MyEntityReplicableBaseEvent<MyFloatingObject>
    {
        private static readonly MyPredictedSnapshotSyncSetup m_settings;
        private MyPropertySyncStateGroup m_propertySync;

        static MyFloatingObjectReplicable()
        {
            MyPredictedSnapshotSyncSetup setup1 = new MyPredictedSnapshotSyncSetup();
            setup1.ProfileName = "FloatingObject";
            setup1.ApplyPosition = true;
            setup1.ApplyRotation = true;
            setup1.ApplyPhysicsAngular = false;
            setup1.ApplyPhysicsLinear = true;
            setup1.ExtrapolationSmoothing = true;
            setup1.MaxPositionFactor = 100f;
            setup1.MaxLinearFactor = 100f;
            setup1.MaxRotationFactor = 100f;
            setup1.MaxAngularFactor = 1f;
            setup1.IterationsFactor = 0.3f;
            m_settings = setup1;
        }

        protected override IMyStateGroup CreatePhysicsGroup() => 
            new MySimplePhysicsStateGroup(base.Instance, this, m_settings);

        public override void GetStateGroups(List<IMyStateGroup> resultList)
        {
            base.GetStateGroups(resultList);
            if ((this.m_propertySync != null) && (this.m_propertySync.PropertyCount > 0))
            {
                resultList.Add(this.m_propertySync);
            }
        }

        protected override void OnHook()
        {
            base.OnHook();
            this.m_propertySync = new MyPropertySyncStateGroup(this, base.Instance.SyncType);
        }
    }
}

