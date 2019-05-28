namespace Sandbox.Game.EntityComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Weapons.Guns;
    using Sandbox.Game.WorldEnvironment;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRageMath;

    public class MyCasterComponent : MyEntityComponentBase
    {
        private MySlimBlock m_hitBlock;
        private MyCubeGrid m_hitCubeGrid;
        private MyCharacter m_hitCharacter;
        private IMyDestroyableObject m_hitDestroaybleObj;
        private MyFloatingObject m_hitFloatingObject;
        private MyEnvironmentSector m_hitEnvironmentSector;
        private int m_environmentItem;
        private Vector3D m_hitPosition;
        private double m_distanceToHitSq;
        private MyDrillSensorBase m_caster;
        private Vector3D m_pointOfReference;
        private bool m_isPointOfRefSet;

        public MyCasterComponent(MyDrillSensorBase caster)
        {
            this.m_caster = caster;
        }

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
        }

        public override void OnBeforeRemovedFromContainer()
        {
            base.OnBeforeRemovedFromContainer();
        }

        public void OnWorldPosChanged(ref MatrixD newTransform)
        {
            MatrixD worldMatrix = newTransform;
            this.m_caster.OnWorldPositionChanged(ref worldMatrix);
            Dictionary<long, MyDrillSensorBase.DetectionInfo> entitiesInRange = this.m_caster.EntitiesInRange;
            float maxValue = float.MaxValue;
            VRage.Game.Entity.MyEntity entity = null;
            int itemId = 0;
            if (!this.m_isPointOfRefSet)
            {
                this.m_pointOfReference = worldMatrix.Translation;
            }
            if ((entitiesInRange != null) && (entitiesInRange.Count > 0))
            {
                foreach (MyDrillSensorBase.DetectionInfo info in entitiesInRange.Values)
                {
                    float num3 = (float) Vector3D.DistanceSquared(info.DetectionPoint, this.m_pointOfReference);
                    if ((info.Entity.Physics != null) && (info.Entity.Physics.Enabled && (num3 < maxValue)))
                    {
                        entity = info.Entity;
                        itemId = info.ItemId;
                        this.m_distanceToHitSq = num3;
                        this.m_hitPosition = info.DetectionPoint;
                        maxValue = num3;
                    }
                }
            }
            this.m_hitCubeGrid = entity as MyCubeGrid;
            this.m_hitBlock = null;
            this.m_hitDestroaybleObj = entity as IMyDestroyableObject;
            this.m_hitFloatingObject = entity as MyFloatingObject;
            this.m_hitCharacter = entity as MyCharacter;
            this.m_hitEnvironmentSector = entity as MyEnvironmentSector;
            this.m_environmentItem = itemId;
            if (this.m_hitCubeGrid != null)
            {
                Vector3I vectori;
                MatrixD worldMatrixNormalizedInv = this.m_hitCubeGrid.PositionComp.WorldMatrixNormalizedInv;
                this.m_hitCubeGrid.FixTargetCube(out vectori, (Vector3) (Vector3D.Transform(this.m_hitPosition, worldMatrixNormalizedInv) / ((double) this.m_hitCubeGrid.GridSize)));
                this.m_hitBlock = this.m_hitCubeGrid.GetCubeBlock(vectori);
            }
        }

        public void SetPointOfReference(Vector3D pointOfRef)
        {
            this.m_pointOfReference = pointOfRef;
            this.m_isPointOfRefSet = true;
        }

        public override string ComponentTypeDebugString =>
            "MyBlockInfoComponent";

        public MySlimBlock HitBlock =>
            this.m_hitBlock;

        public MyCubeGrid HitCubeGrid =>
            this.m_hitCubeGrid;

        public Vector3D HitPosition =>
            this.m_hitPosition;

        public IMyDestroyableObject HitDestroyableObj =>
            this.m_hitDestroaybleObj;

        public MyFloatingObject HitFloatingObject =>
            this.m_hitFloatingObject;

        public MyEnvironmentSector HitEnvironmentSector =>
            this.m_hitEnvironmentSector;

        public int EnvironmentItem =>
            this.m_environmentItem;

        public MyCharacter HitCharacter =>
            this.m_hitCharacter;

        public double DistanceToHitSq =>
            this.m_distanceToHitSq;

        public Vector3D PointOfReference =>
            this.m_pointOfReference;

        public MyDrillSensorBase Caster =>
            this.m_caster;
    }
}

