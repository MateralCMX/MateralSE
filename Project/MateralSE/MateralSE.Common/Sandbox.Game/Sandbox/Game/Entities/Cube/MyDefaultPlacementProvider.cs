namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRageMath;

    public class MyDefaultPlacementProvider : IMyPlacementProvider
    {
        private int m_lastUpdate;
        private Sandbox.Engine.Physics.MyPhysics.HitInfo? m_hitInfo;
        private MyCubeGrid m_closestGrid;
        private MySlimBlock m_closestBlock;
        private MyVoxelBase m_closestVoxelMap;
        private readonly List<Sandbox.Engine.Physics.MyPhysics.HitInfo> m_tmpHitList = new List<Sandbox.Engine.Physics.MyPhysics.HitInfo>();

        public MyDefaultPlacementProvider(float intersectionDistance)
        {
            this.IntersectionDistance = intersectionDistance;
        }

        public void RayCastGridCells(MyCubeGrid grid, List<Vector3I> outHitPositions, Vector3I gridSizeInflate, float maxDist)
        {
            grid.RayCastCells(this.RayStart, this.RayStart + (this.RayDirection * maxDist), outHitPositions, new Vector3I?(gridSizeInflate), false, true);
        }

        public void UpdatePlacement()
        {
            this.m_lastUpdate = MySession.Static.GameplayFrameCounter;
            this.m_hitInfo = null;
            this.m_closestGrid = null;
            this.m_closestVoxelMap = null;
            LineD ed = new LineD(this.RayStart, this.RayStart + (this.RayDirection * this.IntersectionDistance));
            MyPhysics.CastRay(ed.From, ed.To, this.m_tmpHitList, 0x18);
            if (MySession.Static.ControlledEntity != null)
            {
                this.m_tmpHitList.RemoveAll(hitInfo => ReferenceEquals(hitInfo.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity));
            }
            if (this.m_tmpHitList.Count != 0)
            {
                Sandbox.Engine.Physics.MyPhysics.HitInfo info = this.m_tmpHitList[0];
                if (info.HkHitInfo.GetHitEntity() != null)
                {
                    this.m_closestGrid = info.HkHitInfo.GetHitEntity().GetTopMostParent(null) as MyCubeGrid;
                }
                if (this.m_closestGrid != null)
                {
                    this.m_hitInfo = new Sandbox.Engine.Physics.MyPhysics.HitInfo?(info);
                    if (!this.ClosestGrid.Editable)
                    {
                        this.m_closestGrid = null;
                    }
                }
                else
                {
                    this.m_closestVoxelMap = info.HkHitInfo.GetHitEntity() as MyVoxelBase;
                    if (this.m_closestVoxelMap != null)
                    {
                        this.m_hitInfo = new Sandbox.Engine.Physics.MyPhysics.HitInfo?(info);
                    }
                }
            }
        }

        public Vector3D RayStart
        {
            get
            {
                MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                if ((cameraControllerEnum != MyCameraControllerEnum.Entity) && (cameraControllerEnum != MyCameraControllerEnum.ThirdPersonSpectator))
                {
                    if (MySector.MainCamera != null)
                    {
                        return MySector.MainCamera.Position;
                    }
                }
                else
                {
                    if (MySession.Static.ControlledEntity != null)
                    {
                        return MySession.Static.ControlledEntity.GetHeadMatrix(false, true, false, false).Translation;
                    }
                    if (MySector.MainCamera != null)
                    {
                        return MySector.MainCamera.Position;
                    }
                }
                return Vector3.Zero;
            }
        }

        public Vector3D RayDirection
        {
            get
            {
                MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                if ((cameraControllerEnum != MyCameraControllerEnum.Entity) && (cameraControllerEnum != MyCameraControllerEnum.ThirdPersonSpectator))
                {
                    if (MySector.MainCamera != null)
                    {
                        return MySector.MainCamera.ForwardVector;
                    }
                }
                else
                {
                    if (MySession.Static.ControlledEntity != null)
                    {
                        return MySession.Static.ControlledEntity.GetHeadMatrix(false, true, false, false).Forward;
                    }
                    if (MySector.MainCamera != null)
                    {
                        return MySector.MainCamera.ForwardVector;
                    }
                }
                return Vector3.Forward;
            }
        }

        public Sandbox.Engine.Physics.MyPhysics.HitInfo? HitInfo
        {
            get
            {
                if (MySession.Static.GameplayFrameCounter != this.m_lastUpdate)
                {
                    this.UpdatePlacement();
                }
                return this.m_hitInfo;
            }
        }

        public MyCubeGrid ClosestGrid
        {
            get
            {
                if (MySession.Static.GameplayFrameCounter != this.m_lastUpdate)
                {
                    this.UpdatePlacement();
                }
                return this.m_closestGrid;
            }
        }

        public MyVoxelBase ClosestVoxelMap
        {
            get
            {
                if (MySession.Static.GameplayFrameCounter != this.m_lastUpdate)
                {
                    this.UpdatePlacement();
                }
                return this.m_closestVoxelMap;
            }
        }

        public bool CanChangePlacementObjectSize =>
            false;

        public float IntersectionDistance { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyDefaultPlacementProvider.<>c <>9 = new MyDefaultPlacementProvider.<>c();
            public static Predicate<MyPhysics.HitInfo> <>9__24_0;

            internal bool <UpdatePlacement>b__24_0(MyPhysics.HitInfo hitInfo) => 
                ReferenceEquals(hitInfo.HkHitInfo.GetHitEntity(), MySession.Static.ControlledEntity.Entity);
        }
    }
}

