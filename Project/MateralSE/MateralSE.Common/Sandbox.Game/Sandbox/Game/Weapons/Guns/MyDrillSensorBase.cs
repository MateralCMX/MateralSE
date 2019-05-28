namespace Sandbox.Game.Weapons.Guns
{
    using Sandbox;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRageMath;

    public abstract class MyDrillSensorBase
    {
        private const int CacheExpirationFrames = 10;
        protected MyDefinitionBase m_drillDefinition;
        public HashSet<MyEntity> IgnoredEntities = new HashSet<MyEntity>();
        private ulong m_cacheValidTill;
        protected readonly Dictionary<long, DetectionInfo> m_entitiesInRange = new Dictionary<long, DetectionInfo>();
        private Vector3D m_center;
        private Vector3D m_frontPoint;

        public abstract void DebugDraw();
        public abstract void OnWorldPositionChanged(ref MatrixD worldMatrix);
        protected abstract void ReadEntitiesInRange();

        public MyDefinitionBase DrillDefinition =>
            this.m_drillDefinition;

        public Dictionary<long, DetectionInfo> CachedEntitiesInRange =>
            ((MySandboxGame.Static.SimulationFrameCounter < this.m_cacheValidTill) ? this.m_entitiesInRange : this.EntitiesInRange);

        public Dictionary<long, DetectionInfo> EntitiesInRange
        {
            get
            {
                this.m_cacheValidTill = MySandboxGame.Static.SimulationFrameCounter + 10;
                this.ReadEntitiesInRange();
                return this.m_entitiesInRange;
            }
        }

        public Vector3D Center
        {
            get => 
                this.m_center;
            protected set => 
                (this.m_center = value);
        }

        public Vector3D FrontPoint
        {
            get => 
                this.m_frontPoint;
            protected set => 
                (this.m_frontPoint = value);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DetectionInfo
        {
            public readonly MyEntity Entity;
            public readonly Vector3D DetectionPoint;
            public readonly int ItemId;
            public DetectionInfo(MyEntity entity, Vector3D detectionPoint)
            {
                this.Entity = entity;
                this.DetectionPoint = detectionPoint;
                this.ItemId = 0;
            }

            public DetectionInfo(MyEntity entity, Vector3D detectionPoint, int itemid)
            {
                this.Entity = entity;
                this.DetectionPoint = detectionPoint;
                this.ItemId = itemid;
            }
        }
    }
}

