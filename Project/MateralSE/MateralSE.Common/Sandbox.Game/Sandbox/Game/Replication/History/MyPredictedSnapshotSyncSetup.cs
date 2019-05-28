namespace Sandbox.Game.Replication.History
{
    using System;

    public class MyPredictedSnapshotSyncSetup : MySnapshotSyncSetup
    {
        public float MaxPositionFactor;
        public float MinPositionFactor = 1f;
        public float MaxLinearFactor;
        public float MinLinearFactor = 1f;
        public float MaxRotationFactor;
        public float MaxAngularFactor;
        public float MinAngularFactor = 1f;
        public float IterationsFactor;
        public bool UpdateAlways;
        public bool AllowForceStop;
        public bool IsControlled;
        public bool Smoothing = true;
        private MyPredictedSnapshotSyncSetup m_notSmoothed;

        public MyPredictedSnapshotSyncSetup NotSmoothed
        {
            get
            {
                if (this.m_notSmoothed == null)
                {
                    this.m_notSmoothed = base.MemberwiseClone() as MyPredictedSnapshotSyncSetup;
                    this.m_notSmoothed.MaxPositionFactor = Math.Min(1f, this.MaxPositionFactor);
                    this.m_notSmoothed.MaxLinearFactor = Math.Min(1f, this.MaxLinearFactor);
                    this.m_notSmoothed.MaxRotationFactor = Math.Min(1f, this.MaxRotationFactor);
                    this.m_notSmoothed.MaxAngularFactor = Math.Min(1f, this.MaxAngularFactor);
                    this.m_notSmoothed.Smoothing = false;
                }
                return this.m_notSmoothed;
            }
        }
    }
}

