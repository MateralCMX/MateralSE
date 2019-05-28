namespace Sandbox.Engine.Utils
{
    using Havok;
    using Sandbox.Engine.Physics;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Debris;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MyThirdPersonSpectator : MySessionComponentBase
    {
        public static MyThirdPersonSpectator Static;
        public const float MIN_VIEWER_DISTANCE = 2.5f;
        public const float MAX_VIEWER_DISTANCE = 200f;
        public const float DEFAULT_CAMERA_RADIUS_CUBEGRID_MUL = 1.5f;
        public const float DEFAULT_CAMERA_RADIUS_CHARACTER = 0.125f;
        public const float CAMERA_MAX_RAYCAST_DISTANCE = 500f;
        private readonly Vector3D m_initialLookAtDirection = Vector3D.Normalize(new Vector3D(0.0, 5.0, 12.0));
        private readonly Vector3D m_initialLookAtDirectionCharacter = Vector3D.Normalize(new Vector3D(0.0, 0.0, 12.0));
        private const float m_lookAtOffsetY = 0f;
        private const double m_lookAtDefaultLength = 2.5999999046325684;
        private int m_positionSafeZoomingOutDefaultTimeoutMs = 700;
        private bool m_disableSpringThisFrame;
        private float m_currentCameraRadius = 0.125f;
        private bool m_zoomingOutSmoothly;
        private const int SAFE_START_FILTER_FRAME_COUNT = 20;
        private readonly MyAverageFiltering m_safeStartSmoothing = new MyAverageFiltering(20);
        private float m_safeStartSmoothingFiltering = 1f;
        private Vector3D m_lookAt;
        private Vector3D m_clampedlookAt;
        private Vector3D m_transformedLookAt;
        private Vector3D m_target;
        private Vector3D m_lastTarget;
        private MatrixD m_targetOrientation = MatrixD.Identity;
        private Vector3D m_position;
        private Vector3D m_desiredPosition;
        private Vector3D m_positionSafe;
        private float m_positionSafeZoomingOutParam;
        private int m_positionSafeZoomingOutTimeout;
        private float m_lastRaycastDist = float.PositiveInfinity;
        private TimeSpan m_positionCurrentIsSafeSinceTime = TimeSpan.Zero;
        private const int POSITION_IS_SAFE_TIMEOUT_MS = 0x3e8;
        public SpringInfo NormalSpring = new SpringInfo(14000f, 2000f, 94f, 0.05f);
        public SpringInfo NormalSpringCharacter = new SpringInfo(30000f, 2500f, 40f, 0.2f);
        private float m_springChangeTime;
        private SpringInfo m_currentSpring;
        private Vector3 m_velocity;
        private float m_angleVelocity;
        private Quaternion m_orientation;
        private Matrix m_orientationMatrix;
        private readonly List<MyPhysics.HitInfo> m_raycastList = new List<MyPhysics.HitInfo>(0x40);
        private readonly List<HkBodyCollision> m_collisionList = new List<HkBodyCollision>(0x40);
        private readonly List<VRage.Game.Entity.MyEntity> m_entityList = new List<VRage.Game.Entity.MyEntity>();
        private bool m_saveSettings;
        private bool m_debugDraw;
        private bool m_enableDebugDrawTrail;
        private double m_safeMinimumDistance = 2.5;
        private double m_safeMaximumDistance = 200.0;
        private float m_safeMaximumDistanceTimeout;
        private Sandbox.Game.Entities.IMyControllableEntity m_lastControllerEntity;
        private List<Vector3D> m_debugLastSpectatorPositions;
        private List<Vector3D> m_debugLastSpectatorDesiredPositions;
        private float m_lastZoomingOutSpeed;
        private BoundingBoxD m_safeAABB;

        public MyThirdPersonSpectator()
        {
            this.m_currentSpring = this.NormalSpring;
            this.m_lookAt = this.m_initialLookAtDirectionCharacter * 2.5999999046325684;
            this.m_clampedlookAt = this.m_lookAt;
            this.m_saveSettings = false;
            double? newDistance = null;
            this.ResetViewerDistance(newDistance);
        }

        public void CompensateQuickTransformChange(ref MatrixD transformDelta)
        {
            this.m_position = Vector3D.Transform(this.m_position, ref transformDelta);
            this.m_positionSafe = Vector3D.Transform(this.m_positionSafe, ref transformDelta);
            this.m_lastTarget = Vector3D.Transform(this.m_lastTarget, ref transformDelta);
            this.m_target = Vector3D.Transform(this.m_target, ref transformDelta);
            this.m_desiredPosition = Vector3D.Transform(this.m_desiredPosition, ref transformDelta);
        }

        private MyOrientedBoundingBoxD ComputeCompleteSafeOBB(VRage.Game.Entity.MyEntity controlledEntity)
        {
            MyCubeGrid topMostParent = controlledEntity.GetTopMostParent(null) as MyCubeGrid;
            if (topMostParent == null)
            {
                return this.ComputeEntitySafeOBB(controlledEntity);
            }
            MyCubeGrid root = MyGridPhysicalHierarchy.Static.GetRoot(topMostParent);
            return this.ComputeEntitySafeOBB(root);
        }

        private MyOrientedBoundingBoxD ComputeEntitySafeOBB(VRage.Game.Entity.MyEntity controlledEntity)
        {
            MatrixD worldMatrix = controlledEntity.WorldMatrix;
            BoundingBox localAABB = controlledEntity.PositionComp.LocalAABB;
            MyOrientedBoundingBoxD xd2 = new MyOrientedBoundingBoxD(Vector3D.Transform((Vector3D) localAABB.Center, worldMatrix), localAABB.HalfExtents, Quaternion.CreateFromRotationMatrix(worldMatrix));
            MyCubeGrid topMostParent = controlledEntity.GetTopMostParent(null) as MyCubeGrid;
            if (topMostParent != null)
            {
                this.m_safeAABB = topMostParent.PositionComp.WorldAABB;
                MyGridPhysicalHierarchy.Static.ApplyOnChildren(topMostParent, new Action<MyCubeGrid>(this.MergeAABB));
                xd2 = MyOrientedBoundingBoxD.CreateFromBoundingBox(this.m_safeAABB);
            }
            return xd2;
        }

        private void DebugDrawTrail()
        {
            if (!this.m_debugDraw || !this.m_enableDebugDrawTrail)
            {
                this.m_debugLastSpectatorPositions = null;
                this.m_debugLastSpectatorDesiredPositions = null;
            }
            else
            {
                if (this.m_debugLastSpectatorPositions == null)
                {
                    this.m_debugLastSpectatorPositions = new List<Vector3D>(0x400);
                    this.m_debugLastSpectatorDesiredPositions = new List<Vector3D>(0x400);
                }
                this.m_debugLastSpectatorPositions.Add(this.m_position);
                this.m_debugLastSpectatorDesiredPositions.Add(this.m_desiredPosition);
                if (this.m_debugLastSpectatorDesiredPositions.Count > 60)
                {
                    this.m_debugLastSpectatorPositions.RemoveRange(0, 1);
                    this.m_debugLastSpectatorDesiredPositions.RemoveRange(0, 1);
                }
                for (int i = 1; i < this.m_debugLastSpectatorPositions.Count; i++)
                {
                    float num2 = ((float) i) / ((float) this.m_debugLastSpectatorPositions.Count);
                    Color colorFrom = new Color(num2 * num2, 0f, 0f);
                    MyRenderProxy.DebugDrawLine3D(this.m_debugLastSpectatorPositions[i - 1], this.m_debugLastSpectatorPositions[i], colorFrom, colorFrom, true, false);
                    colorFrom = new Color(num2 * num2, num2 * num2, num2 * num2);
                    MyRenderProxy.DebugDrawLine3D(this.m_debugLastSpectatorDesiredPositions[i - 1], this.m_debugLastSpectatorDesiredPositions[i], colorFrom, colorFrom, true, false);
                }
            }
        }

        private MyCameraRaycastResult FindSafeStart(VRage.Game.Entity.MyEntity controlledEntity, LineD line, ref MyOrientedBoundingBoxD safeObb, ref MyOrientedBoundingBoxD safeObbWithCollisionExtents, out Vector3D castStartSafe, out LineD safeOBBLine)
        {
            safeOBBLine = new LineD(safeObbWithCollisionExtents.Center, line.From + ((line.Direction * 2.0) * safeObbWithCollisionExtents.HalfExtent.Length()));
            double? nullable = safeObbWithCollisionExtents.Intersects(ref safeOBBLine);
            castStartSafe = (nullable != null) ? (safeOBBLine.From + (safeOBBLine.Direction * nullable.Value)) : line.From;
            MyCameraRaycastResult ok = MyCameraRaycastResult.Ok;
            if (nullable != null)
            {
                this.m_raycastList.Clear();
                MatrixD transform = MatrixD.CreateTranslation(castStartSafe);
                HkShape shape = (HkShape) new HkSphereShape(this.m_currentCameraRadius);
                MyPhysics.CastShapeReturnContactBodyDatas(line.From, shape, ref transform, 0, 0f, this.m_raycastList, true);
                if (this.EnableDebugDraw)
                {
                    MyDebugDrawHelper.DrawDashedLine(castStartSafe + (0.1f * Vector3.Up), line.From + (0.1f * Vector3.Up), Color.Red);
                    MyRenderProxy.DebugDrawSphere(castStartSafe, this.m_currentCameraRadius, Color.Red, 1f, true, false, true, false);
                }
                MyPhysics.HitInfo? nullable2 = null;
                foreach (MyPhysics.HitInfo info in this.m_raycastList)
                {
                    VRage.Game.Entity.MyEntity hitEntity = info.HkHitInfo.GetHitEntity() as VRage.Game.Entity.MyEntity;
                    HkWorld.HitInfo hkHitInfo = info.HkHitInfo;
                    if (!this.IsEntityFiltered(hitEntity, controlledEntity, info.HkHitInfo.Body, hkHitInfo.GetShapeKey(0)))
                    {
                        nullable2 = new MyPhysics.HitInfo?(info);
                        break;
                    }
                }
                if (nullable2 == null)
                {
                    this.m_collisionList.Clear();
                    MyPhysics.GetPenetrationsShape(shape, ref castStartSafe, ref Quaternion.Identity, this.m_collisionList, 15);
                    foreach (HkBodyCollision collision in this.m_collisionList)
                    {
                        VRage.Game.Entity.MyEntity collisionEntity = collision.GetCollisionEntity() as VRage.Game.Entity.MyEntity;
                        if (!this.IsEntityFiltered(collisionEntity, controlledEntity, collision.Body, collision.ShapeKey))
                        {
                            ok = MyCameraRaycastResult.FoundOccluderNoSpace;
                            break;
                        }
                    }
                }
                shape.RemoveReference();
                if (nullable2 != null)
                {
                    castStartSafe += (nullable2.Value.HkHitInfo.HitFraction - (((double) this.m_currentCameraRadius) / safeOBBLine.Length)) * (line.From - castStartSafe);
                    ok = MyCameraRaycastResult.FoundOccluderNoSpace;
                }
                double d = (castStartSafe - line.From).LengthSquared();
                double num2 = Math.Sqrt(d) - (this.m_currentCameraRadius * 0.01f);
                double num3 = this.m_safeStartSmoothing.Get();
                if (num3 <= d)
                {
                    this.m_safeStartSmoothingFiltering = Math.Min((float) (this.m_safeStartSmoothingFiltering + 0.05f), (float) 1f);
                }
                else
                {
                    this.m_safeStartSmoothingFiltering = Math.Max((float) (this.m_safeStartSmoothingFiltering - 0.025f), (float) 0f);
                    num3 = MathHelper.Lerp(d, num3, (double) this.m_safeStartSmoothingFiltering);
                    double num4 = Math.Sqrt(num3 / d);
                    castStartSafe = line.From + ((castStartSafe - line.From) * num4);
                    d = num3;
                    num2 = Math.Sqrt(d);
                }
                this.m_safeStartSmoothing.Add(d);
                double local1 = (line.To - line.From).LengthSquared();
                if (local1 < (num2 * num2))
                {
                    this.m_position = castStartSafe;
                    this.m_positionSafe = castStartSafe;
                    this.m_positionSafeZoomingOutTimeout = 0;
                }
                if ((local1 * 2.0) < d)
                {
                    this.m_disableSpringThisFrame = true;
                }
            }
            return ok;
        }

        private static VRage.Game.Entity.MyEntity GetControlledEntity(Sandbox.Game.Entities.IMyControllableEntity genericControlledEntity)
        {
            if (genericControlledEntity == null)
            {
                return null;
            }
            MyRemoteControl control = genericControlledEntity as MyRemoteControl;
            VRage.Game.Entity.MyEntity pilot = genericControlledEntity.Entity;
            if (control != null)
            {
                VRage.Game.Entity.MyEntity previousControlledEntity = control.PreviousControlledEntity as VRage.Game.Entity.MyEntity;
                if (previousControlledEntity != null)
                {
                    pilot = previousControlledEntity;
                }
                else if (control.Pilot != null)
                {
                    pilot = control.Pilot;
                }
            }
            while ((pilot != null) && (pilot.Parent is MyCockpit))
            {
                pilot = pilot.Parent;
            }
            return pilot;
        }

        public Vector3D GetCrosshair() => 
            (this.m_target + (this.m_targetOrientation.Forward * 25000.0));

        public double GetViewerDistance() => 
            this.m_clampedlookAt.Length();

        public MatrixD GetViewMatrix() => 
            ((MySession.Static.CameraController != null) ? MatrixD.CreateLookAt(this.m_positionSafe, this.m_target, this.m_targetOrientation.Up) : MatrixD.Identity);

        private void HandleIntersection(VRage.Game.Entity.MyEntity controlledEntity, ref Vector3D lastTargetPos)
        {
            VRage.Game.Entity.MyEntity entity;
            VRage.Game.Entity.MyEntity topMostParent = controlledEntity.GetTopMostParent(null);
            MyCubeGrid grid = (topMostParent ?? controlledEntity) as MyCubeGrid;
            if ((grid != null) && grid.IsStatic)
            {
                entity = controlledEntity;
            }
            Vector3D target = this.m_target;
            Vector3D position = this.m_position;
            double d = (position - this.m_target).LengthSquared();
            if (d > 0.0)
            {
                double num2 = this.m_lookAt.Length() / Math.Sqrt(d);
                position = this.m_target + ((position - this.m_target) * num2);
            }
            LineD line = new LineD(target, position);
            if (line.Length <= 500.0)
            {
                Vector3D vectord3;
                LineD ed2;
                MyOrientedBoundingBoxD safeObb = this.ComputeCompleteSafeOBB(entity);
                MyOrientedBoundingBoxD safeObbWithCollisionExtents = new MyOrientedBoundingBoxD(safeObb.Center, safeObb.HalfExtent + (2f * this.m_currentCameraRadius), safeObb.Orientation);
                MyCameraRaycastResult foundOccluderNoSpace = this.FindSafeStart(controlledEntity, line, ref safeObb, ref safeObbWithCollisionExtents, out vectord3, out ed2);
                if (controlledEntity is MyCharacter)
                {
                    this.m_safeMinimumDistance = this.m_currentCameraRadius;
                }
                else
                {
                    this.m_safeMinimumDistance = (vectord3 - target).Length();
                    this.m_safeMinimumDistance = Math.Max(this.m_safeMinimumDistance, 2.5);
                }
                Vector3D raycastOrigin = (controlledEntity is MyCharacter) ? target : vectord3;
                Vector3D outSafePosition = position;
                if (foundOccluderNoSpace == MyCameraRaycastResult.Ok)
                {
                    if ((position - raycastOrigin).LengthSquared() < (this.m_safeMinimumDistance * this.m_safeMinimumDistance))
                    {
                        position = raycastOrigin + (Vector3D.Normalize(position - raycastOrigin) * this.m_safeMinimumDistance);
                    }
                    foundOccluderNoSpace = this.RaycastOccludingObjects(controlledEntity, ref raycastOrigin, ref position, ref vectord3, ref outSafePosition);
                    if (this.m_safeMaximumDistanceTimeout >= 0f)
                    {
                        this.m_safeMaximumDistanceTimeout -= 16.66667f;
                    }
                    if (foundOccluderNoSpace == MyCameraRaycastResult.Ok)
                    {
                        if (this.m_safeMaximumDistanceTimeout <= 0f)
                        {
                            this.m_safeMaximumDistance = 200.0;
                        }
                    }
                    else if (foundOccluderNoSpace == MyCameraRaycastResult.FoundOccluder)
                    {
                        double num3 = (outSafePosition - target).Length();
                        double num4 = num3 + this.m_currentCameraRadius;
                        if ((this.m_safeMaximumDistanceTimeout <= 0f) || (num4 < this.m_safeMaximumDistance))
                        {
                            this.m_safeMaximumDistance = num4;
                        }
                        if ((num3 < (this.m_safeMaximumDistance + this.m_currentCameraRadius)) && !this.IsCameraForcedWithDelay())
                        {
                            this.m_safeMaximumDistanceTimeout = this.m_positionSafeZoomingOutDefaultTimeoutMs;
                        }
                        this.m_safeMinimumDistance = Math.Min(this.m_safeMinimumDistance, num3);
                        if ((controlledEntity is MyCharacter) && safeObbWithCollisionExtents.Contains(ref outSafePosition))
                        {
                            foundOccluderNoSpace = MyCameraRaycastResult.FoundOccluderNoSpace;
                        }
                    }
                }
                if (this.IsCameraForced())
                {
                    this.m_positionSafe = this.m_target;
                }
                if (foundOccluderNoSpace > MyCameraRaycastResult.FoundOccluder)
                {
                    this.m_positionSafeZoomingOutParam = 1f;
                    this.m_positionCurrentIsSafeSinceTime = TimeSpan.MaxValue;
                    this.m_lastRaycastDist = 0f;
                    this.m_zoomingOutSmoothly = true;
                    this.m_positionSafeZoomingOutTimeout = 0;
                }
                else
                {
                    bool flag = false;
                    if (this.m_positionCurrentIsSafeSinceTime == TimeSpan.MaxValue)
                    {
                        this.ResetInternalTimers();
                        this.ResetSpring();
                        flag = true;
                        this.m_safeMaximumDistanceTimeout = 0f;
                    }
                    this.PerformZoomInOut(vectord3, outSafePosition);
                    if (flag)
                    {
                        this.m_positionCurrentIsSafeSinceTime = MySession.Static.ElapsedGameTime;
                    }
                    this.PerformZoomInOut(vectord3, outSafePosition);
                    if (this.m_positionCurrentIsSafeSinceTime == TimeSpan.MaxValue)
                    {
                        this.m_positionCurrentIsSafeSinceTime = MySession.Static.ElapsedGameTime;
                    }
                }
                if (this.IsCameraForced())
                {
                    this.m_positionSafe = this.m_target;
                }
                if (this.m_debugDraw)
                {
                    MyRenderProxy.DebugDrawArrow3D(ed2.From, ed2.To, Color.White, new Color?(Color.Purple), false, 0.02, null, 0.5f, false);
                    MyRenderProxy.DebugDrawArrow3D(ed2.From, vectord3, Color.White, new Color?(Color.Red), false, 0.02, null, 0.5f, false);
                    MatrixD viewMatrix = MySector.MainCamera.ViewMatrix;
                    MyDebugDrawHelper.DrawNamedPoint(this.m_position, "mpos", new Color?(Color.Gray), new MatrixD?(viewMatrix));
                    MyDebugDrawHelper.DrawNamedPoint(this.m_target, "target", new Color?(Color.Purple), new MatrixD?(viewMatrix));
                    MyDebugDrawHelper.DrawNamedPoint(vectord3, "safeStart", new Color?(Color.Lime), new MatrixD?(viewMatrix));
                    MyDebugDrawHelper.DrawNamedPoint(outSafePosition, "safePosCand", new Color?(Color.Pink), new MatrixD?(viewMatrix));
                    MyDebugDrawHelper.DrawNamedPoint(this.m_positionSafe, "posSafe", new Color?(Color.White), new MatrixD?(viewMatrix));
                    MyRenderProxy.DebugDrawOBB(safeObbWithCollisionExtents, Color.Olive, 0f, false, true, false);
                    MyRenderProxy.DebugDrawOBB(safeObb, Color.OliveDrab, 0f, false, true, false);
                    MyRenderProxy.DebugDrawText3D(safeObbWithCollisionExtents.Center - Vector3D.Transform(safeObbWithCollisionExtents.HalfExtent, safeObbWithCollisionExtents.Orientation), "safeObb", Color.Olive, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 30f), foundOccluderNoSpace.ToString(), Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 50f), this.IsCameraForcedWithDelay() ? (this.IsCameraForced() ? "Forced" : "ForcedDelay") : "Unforced", Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 70f), this.m_zoomingOutSmoothly ? "zooming out" : "ready", Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 90f), "v=" + this.m_velocity.Length().ToString("0.00"), Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 110f), "maxsafedist=" + this.m_safeMaximumDistance, Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D(new Vector2(30f, 130f), "maxsafedisttimeout=" + (0.001f * this.m_safeMaximumDistanceTimeout).ToString("0.0"), (this.m_safeMaximumDistanceTimeout <= 0f) ? Color.Red : Color.Azure, 1f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
            }
        }

        private static bool HasSamePhysicalGroup(IMyEntity entityA, IMyEntity entityB)
        {
            if (ReferenceEquals(entityA, entityB))
            {
                return true;
            }
            MyCubeGrid nodeA = entityA as MyCubeGrid;
            MyCubeGrid nodeB = entityB as MyCubeGrid;
            return ((nodeA != null) && ((nodeB != null) && MyCubeGridGroups.Static.Physical.HasSameGroup(nodeA, nodeB)));
        }

        public bool IsCameraForced() => 
            (this.m_positionCurrentIsSafeSinceTime == TimeSpan.MaxValue);

        public bool IsCameraForcedWithDelay()
        {
            if (!this.IsCameraForced())
            {
                return ((MySession.Static.ElapsedGameTime - this.m_positionCurrentIsSafeSinceTime).TotalMilliseconds < 1000.0);
            }
            return true;
        }

        private bool IsEntityFiltered(VRage.Game.Entity.MyEntity hitEntity, VRage.Game.Entity.MyEntity controlledEntity, HkRigidBody hitRigidBody, uint shapeKey)
        {
            switch (hitRigidBody.UserObject)
            {
                case (((null) || ((null) || (ReferenceEquals(hitEntity, controlledEntity) || !(hitRigidBody.UserObject is MyPhysicsBody)))) || (hitRigidBody.UserObject as MyPhysicsBody).IsPhantom):
                    return true;
                    break;
            }
            if ((shapeKey != uint.MaxValue) && (hitEntity is MyCubeGrid))
            {
                MySlimBlock blockFromShapeKey = ((MyCubeGrid) hitEntity).Physics.Shape.GetBlockFromShapeKey(shapeKey);
                if (blockFromShapeKey != null)
                {
                    MyLadder fatBlock = blockFromShapeKey.FatBlock as MyLadder;
                    if (fatBlock != null)
                    {
                        hitEntity = fatBlock;
                    }
                }
            }
            if (hitEntity is IMyHandheldGunObject<MyDeviceBase>)
            {
                return true;
            }
            if ((hitEntity is MyFloatingObject) || (hitEntity is MyDebrisBase))
            {
                return true;
            }
            if (hitEntity is MyCharacter)
            {
                return true;
            }
            VRage.Game.Entity.MyEntity topMostParent = hitEntity.GetTopMostParent(null);
            VRage.Game.Entity.MyEntity objA = topMostParent ?? hitEntity;
            VRage.Game.Entity.MyEntity entity3 = controlledEntity.GetTopMostParent(null);
            VRage.Game.Entity.MyEntity objB = entity3 ?? controlledEntity;
            if (ReferenceEquals(objA, objB))
            {
                return true;
            }
            MyCubeGrid first = objB as MyCubeGrid;
            MyCubeGrid second = objA as MyCubeGrid;
            if (((first != null) && (second != null)) && MyGridPhysicalHierarchy.Static.InSameHierarchy(first, second))
            {
                return true;
            }
            MyCharacter character = controlledEntity as MyCharacter;
            return ((character != null) && ((character.Ladder != null) && ((hitEntity is MyLadder) && ReferenceEquals(character.Ladder.GetTopMostParent(null), hitEntity.GetTopMostParent(null)))));
        }

        public override void LoadData()
        {
            Static = this;
            base.LoadData();
        }

        private void MergeAABB(MyCubeGrid grid)
        {
            this.m_safeAABB.Include(grid.PositionComp.WorldAABB);
        }

        public void MoveAndRotate(Vector3 moveIndicator, Vector2 rotationIndicator, float rollIndicator)
        {
            this.UpdateZoom();
        }

        private void PerformZoomInOut(Vector3D safeCastStart, Vector3D safePositionCandidate)
        {
            double num = Math.Min((safePositionCandidate - this.m_target).Length(), this.m_safeMaximumDistance);
            double num2 = (this.m_positionSafe - this.m_target).Length();
            if (this.m_disableSpringThisFrame)
            {
                this.m_lastRaycastDist = (float) num;
                this.m_zoomingOutSmoothly = false;
            }
            if (this.IsCameraForced())
            {
                this.m_positionSafeZoomingOutTimeout = 0;
                this.m_positionSafeZoomingOutParam = 0f;
                num2 = 0.0;
                this.m_zoomingOutSmoothly = true;
                this.m_desiredPosition = safeCastStart;
                this.m_position = safeCastStart;
            }
            if (this.m_disableSpringThisFrame || (num <= (num2 + this.m_currentCameraRadius)))
            {
                if ((num < (num2 + this.m_currentCameraRadius)) && (Math.Abs((double) (this.m_lookAt.LengthSquared() - (num * num))) <= 9.9999997473787516E-06))
                {
                    this.m_zoomingOutSmoothly = false;
                }
            }
            else
            {
                this.m_zoomingOutSmoothly = true;
                if ((this.m_lastZoomingOutSpeed <= 1E-05f) && (this.m_positionSafeZoomingOutTimeout <= -this.m_positionSafeZoomingOutDefaultTimeoutMs))
                {
                    this.m_positionSafeZoomingOutTimeout = this.m_positionSafeZoomingOutDefaultTimeoutMs;
                }
            }
            if (!this.m_zoomingOutSmoothly)
            {
                if (!this.m_disableSpringThisFrame)
                {
                    this.m_positionSafeZoomingOutParam = 0f;
                    this.m_lastZoomingOutSpeed = 0f;
                    this.m_positionSafeZoomingOutTimeout = this.m_positionSafeZoomingOutDefaultTimeoutMs;
                }
                if (!this.IsCameraForced())
                {
                    double num7 = Math.Max(Math.Min((safePositionCandidate - this.m_target).Length(), this.m_safeMaximumDistance), this.m_safeMinimumDistance);
                    this.m_positionSafe = this.m_target + (Vector3D.Normalize(safePositionCandidate - this.m_target) * num7);
                }
                this.m_lastRaycastDist = (float) num;
            }
            else
            {
                this.m_positionSafeZoomingOutTimeout -= 0x10;
                if ((this.m_positionSafeZoomingOutTimeout <= 0) && !this.IsCameraForced())
                {
                    double num3 = Math.Min((safePositionCandidate - this.m_target).Length(), this.m_safeMaximumDistance);
                    float num4 = 1f - MathHelper.Clamp((float) (this.m_lastRaycastDist - ((float) num3)), (float) 0.95f, (float) 1f);
                    this.m_lastZoomingOutSpeed = Math.Abs(num4);
                    this.m_positionSafeZoomingOutParam += num4;
                    this.m_positionSafeZoomingOutParam = MathHelper.Clamp(this.m_positionSafeZoomingOutParam, 0f, 1f);
                    double num5 = Math.Min((this.m_positionSafe - this.m_target).Length(), this.m_safeMaximumDistance);
                    Vector3D vectord3 = this.m_target + (Vector3D.Normalize(safePositionCandidate - this.m_target) * num5);
                    this.m_positionSafe = Vector3D.Lerp(vectord3, safePositionCandidate, (double) this.m_positionSafeZoomingOutParam);
                    this.m_lastRaycastDist = (float) num;
                }
                else
                {
                    this.m_lastZoomingOutSpeed = 0f;
                    double num6 = (this.m_positionSafe - this.m_target).Length();
                    if (!this.IsCameraForced())
                    {
                        this.m_positionSafe = this.m_target + (Vector3D.Normalize(safePositionCandidate - this.m_target) * num6);
                    }
                }
            }
        }

        private void ProcessSpringCalculation()
        {
            Vector3D vectord = this.m_position - this.m_desiredPosition;
            Vector3 vector = (Vector3) (((-this.m_currentSpring.Stiffness * vectord) - (this.m_currentSpring.Dampening * this.m_velocity)) / this.m_currentSpring.Mass);
            this.m_velocity += vector * 0.01666667f;
            this.m_position += this.m_velocity * 0.01666667f;
        }

        private unsafe MyCameraRaycastResult RaycastOccludingObjects(VRage.Game.Entity.MyEntity controlledEntity, ref Vector3D raycastOrigin, ref Vector3D raycastEnd, ref Vector3D raycastSafeCameraStart, ref Vector3D outSafePosition)
        {
            Vector3D vectord = raycastEnd - raycastOrigin;
            vectord.Normalize();
            double positiveInfinity = double.PositiveInfinity;
            bool flag = false;
            Vector3D? nullable = null;
            if (controlledEntity is MyCharacter)
            {
                MatrixD worldMatrix = controlledEntity.PositionComp.WorldMatrix;
                Vector3D from = worldMatrix.Translation;
                Vector3D to = from + ((worldMatrix.Up * controlledEntity.PositionComp.LocalAABB.Max.Y) * 1.1499999761581421);
                from += (worldMatrix.Up * controlledEntity.PositionComp.LocalAABB.Max.Y) * 0.85000002384185791;
                this.m_raycastList.Clear();
                MyPhysics.CastRay(from, to, this.m_raycastList, 0);
                if (this.m_debugDraw && !ReferenceEquals(MySession.Static.CameraController, controlledEntity))
                {
                    MyRenderProxy.DebugDrawLine3D(from, to, Color.Red, Color.Red, false, false);
                }
                foreach (MyPhysics.HitInfo info in this.m_raycastList)
                {
                    HkWorld.HitInfo hkHitInfo = info.HkHitInfo;
                    if (!this.IsEntityFiltered((VRage.Game.Entity.MyEntity) info.HkHitInfo.GetHitEntity(), controlledEntity, info.HkHitInfo.Body, hkHitInfo.GetShapeKey(0)))
                    {
                        nullable = new Vector3D?(info.Position);
                        positiveInfinity = 0.0;
                        outSafePosition = raycastOrigin + (vectord * (0f - this.m_currentCameraRadius));
                        flag = true;
                    }
                }
            }
            if (this.m_debugDraw && (nullable != null))
            {
                MatrixD viewMatrix = MySector.MainCamera.ViewMatrix;
                MyDebugDrawHelper.DrawNamedPoint(nullable.Value, "OCCLUDER", new Color?(Color.Red), new MatrixD?(viewMatrix));
                nullable = null;
            }
            bool flag2 = false;
            float num2 = 1f;
            this.m_collisionList.Clear();
            Vector3 halfExtents = new Vector3(this.m_currentCameraRadius, this.m_currentCameraRadius, this.m_currentCameraRadius) * 0.02f;
            Vector3D translation = raycastOrigin + (this.m_currentCameraRadius * vectord);
            MyPhysics.GetPenetrationsBox(ref halfExtents, ref translation, ref Quaternion.Identity, this.m_collisionList, 15);
            if (this.EnableDebugDraw)
            {
                MyRenderProxy.DebugDrawAABB(new BoundingBoxD(translation - halfExtents, translation + halfExtents), Color.Red, 1f, 1f, true, false, false);
            }
            foreach (HkBodyCollision collision in this.m_collisionList)
            {
                VRage.Game.Entity.MyEntity collisionEntity = (VRage.Game.Entity.MyEntity) collision.GetCollisionEntity();
                if (!this.IsEntityFiltered(collisionEntity, controlledEntity, collision.Body, collision.ShapeKey))
                {
                    flag2 = true;
                }
            }
            BoundingBoxD boundingBox = new BoundingBoxD(translation - 9.9999997473787516E-06, translation + 1E-05f);
            this.m_entityList.Clear();
            Sandbox.Game.Entities.MyEntities.GetElementsInBox(ref boundingBox, this.m_entityList);
            foreach (MyVoxelBase base2 in this.m_entityList)
            {
                if ((base2 != null) && base2.IsAnyAabbCornerInside(ref MatrixD.Identity, boundingBox))
                {
                    flag2 = true;
                    break;
                }
            }
            if ((raycastEnd - raycastOrigin).LengthSquared() > (this.m_currentCameraRadius * this.m_currentCameraRadius))
            {
                MatrixD* xdPtr1;
                HkShape shape = (HkShape) new HkSphereShape(this.m_currentCameraRadius);
                MatrixD identity = MatrixD.Identity;
                xdPtr1.Translation = (controlledEntity is MyCharacter) ? raycastOrigin : (raycastOrigin + (this.m_currentCameraRadius * vectord));
                xdPtr1 = (MatrixD*) ref identity;
                double num4 = (identity.Translation - this.m_target).LengthSquared();
                this.m_raycastList.Clear();
                MyPhysics.CastShapeReturnContactBodyDatas(raycastEnd, shape, ref identity, 0, 0f, this.m_raycastList, true);
                if (this.EnableDebugDraw)
                {
                    MyRenderProxy.DebugDrawLine3D(identity.Translation, raycastEnd, Color.Red, Color.Red, true, false);
                    Color? color = null;
                    MatrixD? cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(identity.Translation, "RAY_START", color, cameraViewMatrix);
                    color = null;
                    cameraViewMatrix = null;
                    MyDebugDrawHelper.DrawNamedPoint(raycastEnd, "RAY_END", color, cameraViewMatrix);
                }
                foreach (MyPhysics.HitInfo info3 in this.m_raycastList)
                {
                    if (Vector3.Dot(info3.HkHitInfo.Normal, (Vector3) vectord) > 0f)
                    {
                        continue;
                    }
                    VRage.Game.Entity.MyEntity hitEntity = (VRage.Game.Entity.MyEntity) info3.HkHitInfo.GetHitEntity();
                    if (!this.IsEntityFiltered(hitEntity, controlledEntity, info3.HkHitInfo.Body, info3.HkHitInfo.GetShapeKey(0)) && (num4 <= (info3.Position - this.m_target).LengthSquared()))
                    {
                        float hitFraction = info3.HkHitInfo.HitFraction;
                        Vector3D vectord6 = Vector3D.Lerp(identity.Translation, raycastEnd, Math.Max((double) hitFraction, 0.0001));
                        double num7 = Vector3D.DistanceSquared(identity.Translation, vectord6);
                        if ((hitFraction < num2) && (num7 < positiveInfinity))
                        {
                            nullable = new Vector3D?(info3.Position);
                            outSafePosition = vectord6;
                            positiveInfinity = num7;
                            flag = true;
                            num2 = hitFraction;
                        }
                    }
                }
                shape.RemoveReference();
            }
            if (flag2 && !flag)
            {
                nullable = new Vector3D?(raycastOrigin);
                outSafePosition = raycastOrigin;
                positiveInfinity = 0.0;
                flag = true;
                num2 = 0f;
            }
            if (this.m_debugDraw && (nullable != null))
            {
                MatrixD viewMatrix = MySector.MainCamera.ViewMatrix;
                MyDebugDrawHelper.DrawNamedPoint(nullable.Value, "OCCLUDER", new Color?(Color.Red), new MatrixD?(viewMatrix));
                nullable = null;
            }
            if (controlledEntity is MyCharacter)
            {
                float currentCameraRadius = this.m_currentCameraRadius;
                if (positiveInfinity < (currentCameraRadius * currentCameraRadius))
                {
                    this.m_positionCurrentIsSafeSinceTime = TimeSpan.MaxValue;
                    return MyCameraRaycastResult.FoundOccluderNoSpace;
                }
            }
            else
            {
                float num9 = this.m_currentCameraRadius + (this.IsCameraForced() ? 2.5f : 0.025f);
                if (positiveInfinity < (num9 * num9))
                {
                    this.m_positionCurrentIsSafeSinceTime = TimeSpan.MaxValue;
                    return MyCameraRaycastResult.FoundOccluderNoSpace;
                }
            }
            if (!flag)
            {
                MyPhysics.CastRay(raycastOrigin, raycastSafeCameraStart, this.m_raycastList, 0);
                using (List<MyPhysics.HitInfo>.Enumerator enumerator = this.m_raycastList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.HkHitInfo.GetHitEntity() is MyVoxelBase)
                        {
                            this.m_positionCurrentIsSafeSinceTime = TimeSpan.MaxValue;
                            return MyCameraRaycastResult.FoundOccluderNoSpace;
                        }
                    }
                }
            }
            return (flag ? MyCameraRaycastResult.FoundOccluder : MyCameraRaycastResult.Ok);
        }

        public void RecalibrateCameraPosition(bool isCharacter = false)
        {
            IMyCameraController cameraController = MySession.Static.CameraController;
            if (cameraController is VRage.Game.Entity.MyEntity)
            {
                Sandbox.Game.Entities.IMyControllableEntity controlledEntity = GetControlledEntity(MySession.Static.ControlledEntity) as Sandbox.Game.Entities.IMyControllableEntity;
                if (controlledEntity != null)
                {
                    if (!isCharacter)
                    {
                        MatrixD xd3 = controlledEntity.GetHeadMatrix(true, true, false, false);
                        this.m_targetOrientation = xd3.GetOrientation();
                        this.m_target = xd3.Translation;
                    }
                    VRage.Game.Entity.MyEntity topMostParent = ((VRage.Game.Entity.MyEntity) cameraController).GetTopMostParent(null);
                    if (!topMostParent.Closed)
                    {
                        MatrixD worldMatrixNormalizedInv = topMostParent.PositionComp.WorldMatrixNormalizedInv;
                        MatrixD xd2 = this.m_targetOrientation * worldMatrixNormalizedInv;
                        BoundingBox localAABB = topMostParent.PositionComp.LocalAABB;
                        localAABB.Inflate((float) 1.2f);
                        Vector3D vectord = Vector3D.Normalize(xd2.Backward);
                        double num = Vector3D.Dot(Vector3D.Transform(this.m_target, worldMatrixNormalizedInv) - localAABB.Center, vectord);
                        double num2 = Math.Max((double) (Math.Abs(Vector3D.Dot(localAABB.HalfExtents, vectord)) - num), (double) 2.5999999046325684);
                        double num3 = 2.5999999046325684;
                        if (Math.Abs(vectord.Z) > 0.0001)
                        {
                            num3 = localAABB.HalfExtents.X * 1.5f;
                        }
                        else if (Math.Abs(vectord.X) > 0.0001)
                        {
                            num3 = localAABB.HalfExtents.Z * 1.5f;
                        }
                        this.m_safeMaximumDistance = 200.0;
                        this.m_safeMinimumDistance = 2.5;
                        double num5 = MathHelper.Clamp((double) ((num3 / (2.0 * Math.Tan(MySector.MainCamera.FieldOfView * 0.5))) + num2), (double) 2.5, (double) 200.0);
                        Vector3D lookAt = this.m_initialLookAtDirectionCharacter * num5;
                        this.SetPositionAndLookAt(lookAt);
                    }
                }
            }
        }

        public void ResetInternalTimers()
        {
            this.m_positionCurrentIsSafeSinceTime = TimeSpan.Zero;
        }

        public void ResetSpring()
        {
            this.m_position = this.m_desiredPosition;
            this.m_disableSpringThisFrame = true;
        }

        public bool ResetViewerAngle(Vector2? headAngle)
        {
            if (headAngle == null)
            {
                return false;
            }
            Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity == null)
            {
                return false;
            }
            controlledEntity.HeadLocalXAngle = headAngle.Value.X;
            controlledEntity.HeadLocalYAngle = headAngle.Value.Y;
            return true;
        }

        public bool ResetViewerDistance(double? newDistance = new double?())
        {
            Vector3D initialLookAtDirection;
            if (newDistance == null)
            {
                return false;
            }
            newDistance = new double?(MathHelper.Clamp(newDistance.Value, 2.5, 200.0));
            if ((MySession.Static == null) || !(MySession.Static.ControlledEntity is MyCharacter))
            {
                initialLookAtDirection = this.m_initialLookAtDirection;
            }
            else
            {
                initialLookAtDirection = this.m_initialLookAtDirectionCharacter;
            }
            Vector3D lookAt = initialLookAtDirection * newDistance.Value;
            this.SetPositionAndLookAt(lookAt);
            this.m_disableSpringThisFrame = true;
            this.m_lastRaycastDist = (float) newDistance.Value;
            this.m_safeMaximumDistanceTimeout = 0f;
            this.m_zoomingOutSmoothly = false;
            this.m_positionSafeZoomingOutTimeout = -this.m_positionSafeZoomingOutDefaultTimeoutMs;
            this.m_saveSettings = false;
            this.Update();
            this.UpdateZoom();
            this.SaveSettings();
            return true;
        }

        public void Rotate(Vector2 rotationIndicator, float rollIndicator)
        {
            this.MoveAndRotate(Vector3.Zero, rotationIndicator, rollIndicator);
        }

        public void SaveSettings()
        {
            this.m_saveSettings = true;
        }

        private void SetPositionAndLookAt(Vector3D lookAt)
        {
            Vector3D initialLookAtDirection;
            Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if (controlledEntity == null)
            {
                controlledEntity = MySession.Static.CameraController as Sandbox.Game.Entities.IMyControllableEntity;
                if (controlledEntity == null)
                {
                    return;
                }
            }
            VRage.Game.Entity.MyEntity entity2 = GetControlledEntity(controlledEntity);
            MyPositionComponentBase positionComp = entity2.PositionComp;
            BoundingBox localAABB = positionComp.LocalAABB;
            BoundingBox box2 = positionComp.LocalAABB;
            MatrixD xd = controlledEntity.GetHeadMatrix(true, true, false, false);
            this.m_target = xd.Translation;
            double num = lookAt.Length();
            if ((MySession.Static == null) || ((MySession.Static.ControlledEntity != null) && !(MySession.Static.CameraController is MyCharacter)))
            {
                initialLookAtDirection = this.m_initialLookAtDirection;
            }
            else
            {
                initialLookAtDirection = this.m_initialLookAtDirectionCharacter;
            }
            this.m_lookAt = initialLookAtDirection * num;
            this.m_lastTarget = this.m_target;
            if (entity2.Physics != null)
            {
                this.m_target += entity2.Physics.LinearVelocity * 60f;
            }
            this.m_transformedLookAt = Vector3D.Transform(lookAt, this.m_targetOrientation);
            this.m_positionSafe = this.m_target + this.m_transformedLookAt;
            if (entity2.Physics != null)
            {
                this.m_positionSafe -= entity2.Physics.LinearVelocity * 60f;
            }
            this.m_desiredPosition = this.m_positionSafe;
            this.m_position = this.m_positionSafe;
            this.m_velocity = Vector3.Zero;
            this.m_disableSpringThisFrame = true;
            this.m_safeStartSmoothing.Clear();
            this.m_positionSafeZoomingOutParam = 0f;
            this.m_positionSafeZoomingOutTimeout = 0;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public void Update()
        {
            Sandbox.Game.Entities.IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if ((controlledEntity == null) || (controlledEntity.Entity == null))
            {
                controlledEntity = MySession.Static.CameraController as Sandbox.Game.Entities.IMyControllableEntity;
                if ((controlledEntity == null) || (controlledEntity.Entity == null))
                {
                    return;
                }
            }
            VRage.Game.Entity.MyEntity entity2 = GetControlledEntity(controlledEntity);
            this.m_lastTarget = this.m_target;
            if ((entity2 == null) || (entity2.PositionComp == null))
            {
                MatrixD xd2 = controlledEntity.GetHeadMatrix(true, true, false, false);
                this.m_target = xd2.Translation;
                this.m_targetOrientation = xd2.GetOrientation();
                this.m_transformedLookAt = Vector3D.Transform(this.m_clampedlookAt, this.m_targetOrientation);
                this.m_position = this.m_desiredPosition;
            }
            else
            {
                MyPositionComponentBase positionComp = entity2.PositionComp;
                BoundingBox localAABB = positionComp.LocalAABB;
                BoundingBox box2 = positionComp.LocalAABB;
                MatrixD xd = controlledEntity.GetHeadMatrix(true, true, false, false);
                MyCharacter character = entity2 as MyCharacter;
                this.m_target = xd.Translation;
                if (character != null)
                {
                    this.m_currentCameraRadius = 0.125f;
                    this.m_currentSpring = this.NormalSpringCharacter;
                }
                else
                {
                    this.m_currentSpring = this.NormalSpring;
                    if (entity2 is MyTerminalBlock)
                    {
                        this.m_currentCameraRadius = 0.5f;
                    }
                    else
                    {
                        MyCubeGrid grid = entity2 as MyCubeGrid;
                        if (grid != null)
                        {
                            this.m_currentCameraRadius = grid.GridSize;
                        }
                    }
                }
                if ((character == null) || !character.IsDead)
                {
                    bool flag = !this.m_disableSpringThisFrame & !MyDX9Gui.LookaroundEnabled;
                    this.m_targetOrientation = MatrixD.Lerp(this.m_targetOrientation, xd.GetOrientation(), flag ? ((double) this.m_currentSpring.RotationGain) : ((double) 1f));
                }
                this.m_transformedLookAt = Vector3D.Transform(this.m_clampedlookAt, this.m_targetOrientation);
                this.m_desiredPosition = this.m_target + this.m_transformedLookAt;
                this.m_position += this.m_target - this.m_lastTarget;
                this.m_positionSafe += this.m_target - this.m_lastTarget;
            }
            if (!ReferenceEquals(controlledEntity, this.m_lastControllerEntity))
            {
                this.m_disableSpringThisFrame = true;
                this.m_lastTarget = this.m_target;
                this.m_lastControllerEntity = controlledEntity;
            }
            Vector3D position = this.m_position;
            if (this.m_disableSpringThisFrame)
            {
                this.m_position = this.m_desiredPosition;
                this.m_velocity = Vector3.Zero;
            }
            else
            {
                this.m_position = this.m_desiredPosition;
                this.m_velocity = Vector3.Zero;
            }
            if (entity2 != null)
            {
                if (!entity2.Closed)
                {
                    this.HandleIntersection(entity2, ref this.m_target);
                }
                else
                {
                    this.m_positionCurrentIsSafeSinceTime = TimeSpan.MaxValue;
                }
            }
            if (this.m_saveSettings)
            {
                MySession.Static.SaveControlledEntityCameraSettings(false);
                this.m_saveSettings = false;
            }
            if (this.m_disableSpringThisFrame)
            {
                double amount = 0.8;
                this.m_position = Vector3D.Lerp(position, this.m_desiredPosition, amount);
                this.m_velocity = Vector3.Zero;
                this.m_disableSpringThisFrame = Vector3D.DistanceSquared(this.m_position, this.m_desiredPosition) > (this.m_currentCameraRadius * this.m_currentCameraRadius);
            }
            this.DebugDrawTrail();
        }

        public void UpdateZoom()
        {
            double num = 0.0;
            if (!MyPerGameSettings.ZoomRequiresLookAroundPressed || MyInput.Static.IsGameControlPressed(MyControlsSpace.LOOKAROUND))
            {
                if (MyInput.Static.PreviousMouseScrollWheelValue() < MyInput.Static.MouseScrollWheelValue())
                {
                    num = this.m_lookAt.Length() / 1.2000000476837158;
                }
                else if (MyInput.Static.PreviousMouseScrollWheelValue() > MyInput.Static.MouseScrollWheelValue())
                {
                    num = this.m_lookAt.Length() * 1.2000000476837158;
                    this.m_positionSafeZoomingOutTimeout = -this.m_positionSafeZoomingOutDefaultTimeoutMs;
                }
            }
            bool flag = false;
            if (num <= 0.0)
            {
                double num4 = this.m_lookAt.Length();
                double num5 = MathHelper.Clamp(num4, 2.5, 200.0);
                this.m_lookAt *= num5 / num4;
                this.SaveSettings();
            }
            else
            {
                double num3 = this.m_lookAt.Length();
                num = MathHelper.Clamp(num, Math.Max(this.m_safeMinimumDistance, 2.5), this.m_safeMaximumDistance);
                this.m_lookAt *= num / num3;
                flag = num > num3;
                if (flag)
                {
                    this.m_positionSafeZoomingOutTimeout = 0;
                    this.m_safeMaximumDistanceTimeout = 0f;
                }
                this.SaveSettings();
            }
            this.m_clampedlookAt = this.m_lookAt;
            double num2 = this.m_clampedlookAt.Length();
            this.m_clampedlookAt = (this.m_clampedlookAt * MathHelper.Clamp(num2, this.m_safeMinimumDistance, this.m_safeMaximumDistance)) / num2;
            if (flag && (this.m_lookAt.LengthSquared() < (this.m_safeMinimumDistance * this.m_safeMinimumDistance)))
            {
                this.m_lookAt = this.m_clampedlookAt;
            }
        }

        public bool EnableDebugDraw
        {
            get => 
                this.m_debugDraw;
            set => 
                (this.m_debugDraw = value);
        }

        private enum MyCameraRaycastResult
        {
            Ok,
            FoundOccluder,
            FoundOccluderNoSpace
        }

        public class SpringInfo
        {
            public float Stiffness;
            public float Dampening;
            public float Mass;
            public float RotationGain;

            public SpringInfo(MyThirdPersonSpectator.SpringInfo spring)
            {
                this.Setup(spring);
            }

            public SpringInfo(float stiffness, float dampening, float mass, float rotationGain)
            {
                this.Stiffness = stiffness;
                this.Dampening = dampening;
                this.Mass = mass;
                this.RotationGain = rotationGain;
            }

            public void Setup(MyThirdPersonSpectator.SpringInfo spring)
            {
                this.Stiffness = spring.Stiffness;
                this.Dampening = spring.Dampening;
                this.Mass = spring.Mass;
            }
        }
    }
}

