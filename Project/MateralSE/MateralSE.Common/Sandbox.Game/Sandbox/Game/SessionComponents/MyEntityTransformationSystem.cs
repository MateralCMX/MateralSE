namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Interfaces;
    using VRage.Groups;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, 900)]
    public class MyEntityTransformationSystem : MySessionComponentBase
    {
        private static float PICKING_RAY_LENGTH = 1000f;
        private static float PLANE_THICKNESS = 0.005f;
        private static bool DEBUG = false;
        private MyEntity m_controlledEntity;
        private readonly Dictionary<MyEntity, int> m_cachedBodyCollisionLayers = new Dictionary<MyEntity, int>();
        private MyOrientedBoundingBoxD m_xBB;
        private MyOrientedBoundingBoxD m_yBB;
        private MyOrientedBoundingBoxD m_zBB;
        private MyOrientedBoundingBoxD m_xPlane;
        private MyOrientedBoundingBoxD m_yPlane;
        private MyOrientedBoundingBoxD m_zPlane;
        private int m_selected;
        private MatrixD m_gizmoMatrix;
        private PlaneD m_dragPlane;
        private bool m_dragActive;
        private bool m_dragOverAxis;
        private Vector3D m_dragStartingPoint;
        private Vector3D m_dragAxis;
        private Vector3D m_dragStartingPosition;
        private bool m_rotationActive;
        private PlaneD m_rotationPlane;
        private Vector3D m_rotationAxis;
        private Vector3D m_rotationStartingPoint;
        private MatrixD m_storedOrientation;
        private MatrixD m_storedScale;
        private Vector3D m_storedTranslation;
        private MatrixD m_storedWorldMatrix;
        private LineD m_lastRay;
        private OperationMode m_previousOperation;
        private readonly List<MyLineSegmentOverlapResult<MyEntity>> m_rayCastResultList = new List<MyLineSegmentOverlapResult<MyEntity>>();
        private bool m_active;
        [CompilerGenerated]
        private Action<MyEntity, MyEntity> ControlledEntityChanged;
        [CompilerGenerated]
        private Action<LineD> RayCasted;

        public event Action<MyEntity, MyEntity> ControlledEntityChanged
        {
            [CompilerGenerated] add
            {
                Action<MyEntity, MyEntity> controlledEntityChanged = this.ControlledEntityChanged;
                while (true)
                {
                    Action<MyEntity, MyEntity> a = controlledEntityChanged;
                    Action<MyEntity, MyEntity> action3 = (Action<MyEntity, MyEntity>) Delegate.Combine(a, value);
                    controlledEntityChanged = Interlocked.CompareExchange<Action<MyEntity, MyEntity>>(ref this.ControlledEntityChanged, action3, a);
                    if (ReferenceEquals(controlledEntityChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyEntity, MyEntity> controlledEntityChanged = this.ControlledEntityChanged;
                while (true)
                {
                    Action<MyEntity, MyEntity> source = controlledEntityChanged;
                    Action<MyEntity, MyEntity> action3 = (Action<MyEntity, MyEntity>) Delegate.Remove(source, value);
                    controlledEntityChanged = Interlocked.CompareExchange<Action<MyEntity, MyEntity>>(ref this.ControlledEntityChanged, action3, source);
                    if (ReferenceEquals(controlledEntityChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public event Action<LineD> RayCasted
        {
            [CompilerGenerated] add
            {
                Action<LineD> rayCasted = this.RayCasted;
                while (true)
                {
                    Action<LineD> a = rayCasted;
                    Action<LineD> action3 = (Action<LineD>) Delegate.Combine(a, value);
                    rayCasted = Interlocked.CompareExchange<Action<LineD>>(ref this.RayCasted, action3, a);
                    if (ReferenceEquals(rayCasted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<LineD> rayCasted = this.RayCasted;
                while (true)
                {
                    Action<LineD> source = rayCasted;
                    Action<LineD> action3 = (Action<LineD>) Delegate.Remove(source, value);
                    rayCasted = Interlocked.CompareExchange<Action<LineD>>(ref this.RayCasted, action3, source);
                    if (ReferenceEquals(rayCasted, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyEntityTransformationSystem()
        {
            this.Active = false;
            this.Mode = CoordinateMode.WorldCoords;
            this.m_selected = -1;
            MySession.Static.CameraAttachedToChanged += (old, @new) => (this.Active = false);
        }

        public void ChangeCoordSystem(bool world)
        {
            this.Mode = !world ? CoordinateMode.LocalCoords : CoordinateMode.WorldCoords;
            if (this.ControlledEntity != null)
            {
                this.UpdateGizmoPosition();
            }
        }

        public void ChangeOperationMode(OperationMode mode)
        {
            this.Operation = mode;
        }

        private void ControlledEntityPositionChanged(MyPositionComponentBase myPositionComponentBase)
        {
            this.UpdateGizmoPosition();
        }

        private LineD CreateRayFromCursorPosition()
        {
            Vector2 mousePosition = MyInput.Static.GetMousePosition();
            LineD ed = base.Session.Camera.WorldLineFromScreen(mousePosition);
            return new LineD(ed.From, ed.From + (ed.Direction * PICKING_RAY_LENGTH));
        }

        public override void Draw()
        {
            if (this.Active)
            {
                if (DEBUG)
                {
                    MyRenderProxy.DebugDrawLine3D(this.m_lastRay.From, this.m_lastRay.To, Color.Green, Color.Green, true, false);
                }
                Vector2 screenCoord = new Vector2(base.Session.Camera.ViewportSize.X * 0.01f, base.Session.Camera.ViewportSize.Y * 0.05f);
                Vector2 vector2 = new Vector2(base.Session.Camera.ViewportSize.Y * 0.11f, 0f);
                Vector2 vector3 = new Vector2(0f, base.Session.Camera.ViewportSize.Y * 0.015f);
                float scale = 0.65f * Math.Min((float) (base.Session.Camera.ViewportSize.X / 1920f), (float) (base.Session.Camera.ViewportSize.Y / 1200f));
                MyRenderProxy.DebugDrawText2D(screenCoord, "Transform:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                switch (this.Operation)
                {
                    case OperationMode.Translation:
                        MyRenderProxy.DebugDrawText2D(screenCoord + vector2, "Translation", Color.Orange, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        break;

                    case OperationMode.Rotation:
                        MyRenderProxy.DebugDrawText2D(screenCoord + vector2, "Rotation", Color.PaleGreen, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        break;

                    case OperationMode.HierarchyAssignment:
                        MyRenderProxy.DebugDrawText2D(screenCoord + vector2, "Hierarchy", Color.CornflowerBlue, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                        break;

                    default:
                        break;
                }
                screenCoord += vector3;
                MyRenderProxy.DebugDrawText2D(screenCoord, "     Coords:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                CoordinateMode mode = this.Mode;
                if (mode == CoordinateMode.LocalCoords)
                {
                    MyRenderProxy.DebugDrawText2D(screenCoord + vector2, "Local", Color.PaleGreen, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                else if (mode == CoordinateMode.WorldCoords)
                {
                    MyRenderProxy.DebugDrawText2D(screenCoord + vector2, "World", Color.Orange, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                screenCoord += 1.5f * vector3;
                MyRenderProxy.DebugDrawText2D(screenCoord, "Cam loc:", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                Vector2 vector1 = new Vector2(base.Session.Camera.ViewportSize.Y * 0.08f, 0f);
                Vector3D position = MyAPIGateway.Session.Camera.Position;
                MyRenderProxy.DebugDrawText2D(screenCoord + vector2, position.X.ToString("0.00"), Color.Crimson, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                MyRenderProxy.DebugDrawText2D((screenCoord + vector2) + (1f * vector3), position.Y.ToString("0.00"), Color.PaleGreen, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                MyRenderProxy.DebugDrawText2D((screenCoord + vector2) + (2f * vector3), position.Z.ToString("0.00"), Color.CornflowerBlue, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                screenCoord += 3.5f * vector3;
                bool flag = this.ControlledEntity != null;
                MyRenderProxy.DebugDrawText2D(screenCoord, flag ? "Selected:" : "No entity selected", Color.Yellow, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                if (flag)
                {
                    Vector3D vectord2 = this.ControlledEntity.PositionComp.GetPosition();
                    MyRenderProxy.DebugDrawText2D(screenCoord + vector2, vectord2.X.ToString("0.00"), Color.Crimson, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D((screenCoord + vector2) + (1f * vector3), vectord2.Y.ToString("0.00"), Color.PaleGreen, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                    MyRenderProxy.DebugDrawText2D((screenCoord + vector2) + (2f * vector3), vectord2.Z.ToString("0.00"), Color.CornflowerBlue, scale, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                if (this.ControlledEntity != null)
                {
                    if (this.ControlledEntity.Parent != null)
                    {
                        for (MyEntity entity = this.ControlledEntity.Parent; entity != null; entity = entity.Parent)
                        {
                            MyRenderProxy.DebugDrawLine3D(this.ControlledEntity.Parent.PositionComp.GetPosition(), this.ControlledEntity.PositionComp.GetPosition(), Color.Orange, Color.Blue, false, false);
                        }
                    }
                    if ((this.Operation == OperationMode.Translation) && !this.DisableTransformation)
                    {
                        Vector3D vectord3 = base.Session.Camera.Position;
                        double num2 = Vector3D.Distance(this.m_xBB.Center, vectord3);
                        double num3 = base.Session.Camera.ProjectionMatrix.Up.LengthSquared();
                        this.m_xBB.HalfExtent = ((Vector3D.One * 0.008) * num2) * num3;
                        this.m_yBB.HalfExtent = ((Vector3D.One * 0.008) * num2) * num3;
                        this.m_zBB.HalfExtent = ((Vector3D.One * 0.008) * num2) * num3;
                        this.DrawOBB(this.m_xBB, Color.Red, 0.5f, 0);
                        this.DrawOBB(this.m_yBB, Color.Green, 0.5f, 1);
                        this.DrawOBB(this.m_zBB, Color.Blue, 0.5f, 2);
                    }
                    if ((this.Operation == OperationMode.HierarchyAssignment) || this.DisableTransformation)
                    {
                        MyRenderProxy.DebugDrawSphere(this.ControlledEntity.PositionComp.WorldVolume.Center, (float) this.ControlledEntity.PositionComp.WorldVolume.Radius, Color.Yellow, 0.2f, true, false, true, false);
                    }
                    else
                    {
                        this.DrawOBB(this.m_xPlane, Color.Red, 0.2f, 3);
                        this.DrawOBB(this.m_yPlane, Color.Green, 0.2f, 4);
                        this.DrawOBB(this.m_zPlane, Color.Blue, 0.2f, 5);
                    }
                }
            }
        }

        private void DrawOBB(MyOrientedBoundingBoxD obb, Color color, float alpha, int identificationIndex)
        {
            if (identificationIndex == this.m_selected)
            {
                MyRenderProxy.DebugDrawOBB(obb, Color.White, 0.2f, false, false, false);
            }
            else
            {
                MyRenderProxy.DebugDrawOBB(obb, color, alpha, false, false, false);
            }
        }

        private void PerformDragg(bool lockToAxis = true)
        {
            LineD ed = this.CreateRayFromCursorPosition();
            Vector3D vectord = this.m_dragPlane.Intersection(ref ed.From, ref ed.Direction) - this.m_dragStartingPoint;
            if (!lockToAxis)
            {
                if (vectord.LengthSquared() < double.Epsilon)
                {
                    return;
                }
                MatrixD worldMatrix = this.ControlledEntity.PositionComp.WorldMatrix;
                worldMatrix.Translation = this.m_dragStartingPosition + vectord;
                this.SetWorldMatrix(ref worldMatrix);
            }
            else
            {
                double num = vectord.Dot(ref this.m_dragAxis);
                if (Math.Abs(num) < double.Epsilon)
                {
                    return;
                }
                MatrixD worldMatrix = this.ControlledEntity.PositionComp.WorldMatrix;
                worldMatrix.Translation = this.m_dragStartingPosition + (this.m_dragAxis * num);
                this.SetWorldMatrix(ref worldMatrix);
            }
            this.UpdateGizmoPosition();
        }

        private void PerformRotation()
        {
            LineD ed = this.CreateRayFromCursorPosition();
            Vector3D vectord = this.m_rotationPlane.Intersection(ref ed.From, ref ed.Direction);
            if (Vector3D.DistanceSquared(this.m_rotationStartingPoint, vectord) >= 9.88131291682493E-323)
            {
                MatrixD xd = MatrixD.CreateFromQuaternion(QuaternionD.CreateFromTwoVectors(this.m_rotationStartingPoint - this.m_gizmoMatrix.Translation, vectord - this.m_gizmoMatrix.Translation));
                MatrixD newWorldMatrix = (this.m_storedOrientation * xd) * this.m_storedScale;
                newWorldMatrix.Translation = this.m_storedTranslation;
                this.SetWorldMatrix(ref newWorldMatrix);
            }
        }

        private void Physics_ClearCollisionLayerCache()
        {
            this.m_cachedBodyCollisionLayers.Clear();
        }

        private void Physics_MoveEntityToNoCollisionLayer(MyEntity entity)
        {
            MyCubeGrid grid = entity as MyCubeGrid;
            if (grid != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in MyCubeGridGroups.Static.Physical.GetGroup(grid).m_members)
                {
                    if (node.NodeData.Parent == null)
                    {
                        this.Physics_MoveEntityToNoCollisionLayerRecursive(node.NodeData);
                    }
                }
            }
            else
            {
                this.Physics_MoveEntityToNoCollisionLayerRecursive(entity);
            }
        }

        private void Physics_MoveEntityToNoCollisionLayerRecursive(MyEntity entity)
        {
            if (entity.Physics != null)
            {
                this.m_cachedBodyCollisionLayers.Add(entity, entity.Physics.RigidBody.Layer);
                entity.Physics.RigidBody.Layer = 0x13;
            }
            foreach (MyHierarchyComponentBase base2 in entity.Hierarchy.Children)
            {
                if (base2.Entity.Physics == null)
                {
                    continue;
                }
                if (base2.Entity.Physics.RigidBody != null)
                {
                    this.Physics_MoveEntityToNoCollisionLayerRecursive((MyEntity) base2.Entity);
                }
            }
        }

        private void Physics_RestorePreviousCollisionLayerState()
        {
            foreach (KeyValuePair<MyEntity, int> pair in this.m_cachedBodyCollisionLayers)
            {
                if (pair.Key.Physics != null)
                {
                    pair.Key.Physics.RigidBody.Layer = pair.Value;
                }
            }
        }

        private bool PickControl()
        {
            Vector3D? nullable2;
            if (this.m_xBB.Intersects(ref this.m_lastRay) != null)
            {
                this.m_selected = 0;
                nullable2 = null;
                this.PrepareDrag(nullable2, new Vector3D?(this.m_gizmoMatrix.Right));
                this.m_dragActive = true;
                return true;
            }
            if (this.m_yBB.Intersects(ref this.m_lastRay) != null)
            {
                this.m_selected = 1;
                nullable2 = null;
                this.PrepareDrag(nullable2, new Vector3D?(this.m_gizmoMatrix.Up));
                this.m_dragActive = true;
                return true;
            }
            if (this.m_zBB.Intersects(ref this.m_lastRay) != null)
            {
                this.m_selected = 2;
                nullable2 = null;
                this.PrepareDrag(nullable2, new Vector3D?(this.m_gizmoMatrix.Backward));
                this.m_dragActive = true;
                return true;
            }
            if (this.m_xPlane.Intersects(ref this.m_lastRay) != null)
            {
                if (this.Operation == OperationMode.Rotation)
                {
                    this.PrepareRotation(this.m_gizmoMatrix.Right);
                    this.m_rotationActive = true;
                }
                else
                {
                    nullable2 = null;
                    this.PrepareDrag(new Vector3D?(this.m_gizmoMatrix.Right), nullable2);
                    this.m_dragActive = true;
                }
                this.m_selected = 3;
                return true;
            }
            if (this.m_yPlane.Intersects(ref this.m_lastRay) != null)
            {
                if (this.Operation == OperationMode.Rotation)
                {
                    this.PrepareRotation(this.m_gizmoMatrix.Up);
                    this.m_rotationActive = true;
                }
                else
                {
                    nullable2 = null;
                    this.PrepareDrag(new Vector3D?(this.m_gizmoMatrix.Up), nullable2);
                    this.m_dragActive = true;
                }
                this.m_selected = 4;
                return true;
            }
            if (this.m_zPlane.Intersects(ref this.m_lastRay) == null)
            {
                return false;
            }
            if (this.Operation == OperationMode.Rotation)
            {
                this.PrepareRotation(this.m_gizmoMatrix.Backward);
                this.m_rotationActive = true;
            }
            else
            {
                nullable2 = null;
                this.PrepareDrag(new Vector3D?(this.m_gizmoMatrix.Backward), nullable2);
                this.m_dragActive = true;
            }
            this.m_selected = 5;
            return true;
        }

        private MyEntity PickEntity()
        {
            MyPhysics.HitInfo? nullable = MyPhysics.CastRay(this.m_lastRay.From, this.m_lastRay.To, 0);
            this.m_rayCastResultList.Clear();
            MyGamePruningStructure.GetAllEntitiesInRay(ref this.m_lastRay, this.m_rayCastResultList, MyEntityQueryType.Both);
            int a = 0;
            for (int i = 0; i < this.m_rayCastResultList.Count; i++)
            {
                if (!(this.m_rayCastResultList[i].Element is MyCubeGrid))
                {
                    this.m_rayCastResultList.Swap<MyLineSegmentOverlapResult<MyEntity>>(a, i);
                    a++;
                }
            }
            if ((this.m_rayCastResultList.Count == 0) && (nullable == null))
            {
                return null;
            }
            MyEntity element = null;
            double maxValue = double.MaxValue;
            foreach (MyLineSegmentOverlapResult<MyEntity> result in this.m_rayCastResultList)
            {
                BoundingBoxD worldAABB = result.Element.PositionComp.WorldAABB;
                if (worldAABB.Intersects(ref this.m_lastRay, out maxValue) && (((result.Element is MyCubeGrid) || (result.Element is MyFloatingObject)) || (result.Element.GetType() == typeof(MyEntity))))
                {
                    element = result.Element;
                    break;
                }
            }
            if ((nullable != null) && (Vector3D.Distance(nullable.Value.Position, this.m_lastRay.From) < maxValue))
            {
                IMyEntity hitEntity = nullable.Value.HkHitInfo.GetHitEntity();
                if ((hitEntity is MyCubeGrid) || (hitEntity is MyFloatingObject))
                {
                    return (MyEntity) hitEntity;
                }
            }
            if (element == null)
            {
                element = this.ControlledEntity;
            }
            return ((element != null) ? element : null);
        }

        private void PrepareDrag(Vector3D? planeNormal, Vector3D? axis)
        {
            if (axis != null)
            {
                Vector3D vectord = base.Session.Camera.Position - this.m_gizmoMatrix.Translation;
                Vector3D vectord2 = Vector3D.Cross(axis.Value, vectord);
                planeNormal = new Vector3D?(Vector3D.Cross(axis.Value, vectord2));
                this.m_dragPlane = new PlaneD(this.m_gizmoMatrix.Translation, planeNormal.Value);
            }
            else if (planeNormal != null)
            {
                this.m_dragPlane = new PlaneD(this.m_gizmoMatrix.Translation, planeNormal.Value);
            }
            this.m_dragStartingPoint = this.m_dragPlane.Intersection(ref this.m_lastRay.From, ref this.m_lastRay.Direction);
            if (axis != null)
            {
                this.m_dragAxis = axis.Value;
            }
            this.m_dragOverAxis = axis != null;
            this.m_dragStartingPosition = this.ControlledEntity.PositionComp.GetPosition();
        }

        private void PrepareRotation(Vector3D axis)
        {
            this.m_rotationAxis = axis;
            this.m_rotationPlane = new PlaneD(this.m_gizmoMatrix.Translation, this.m_rotationAxis);
            this.m_rotationStartingPoint = this.m_rotationPlane.Intersection(ref this.m_lastRay.From, ref this.m_lastRay.Direction);
            MatrixD worldMatrix = this.ControlledEntity.PositionComp.WorldMatrix;
            this.m_storedScale = MatrixD.CreateScale(worldMatrix.Scale);
            this.m_storedTranslation = worldMatrix.Translation;
            this.m_storedOrientation = worldMatrix.GetOrientation();
        }

        public void SetControlledEntity(MyEntity entity)
        {
            if (!ReferenceEquals(this.ControlledEntity, entity))
            {
                if (this.ControlledEntity != null)
                {
                    this.ControlledEntity.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.ControlledEntityPositionChanged);
                    this.Physics_RestorePreviousCollisionLayerState();
                }
                MyEntity controlledEntity = this.ControlledEntity;
                this.m_controlledEntity = entity;
                if (this.ControlledEntityChanged != null)
                {
                    this.ControlledEntityChanged(controlledEntity, entity);
                }
                if (entity != null)
                {
                    this.ControlledEntity.PositionComp.OnPositionChanged += new Action<MyPositionComponentBase>(this.ControlledEntityPositionChanged);
                    this.ControlledEntity.OnClosing += delegate (MyEntity myEntity) {
                        myEntity.PositionComp.OnPositionChanged -= new Action<MyPositionComponentBase>(this.ControlledEntityPositionChanged);
                        this.m_controlledEntity = null;
                    };
                    this.Physics_ClearCollisionLayerCache();
                    this.Physics_MoveEntityToNoCollisionLayer(this.ControlledEntity);
                    this.UpdateGizmoPosition();
                }
            }
        }

        private void SetWorldMatrix(ref MatrixD newWorldMatrix)
        {
            MyCubeGrid controlledEntity = this.ControlledEntity as MyCubeGrid;
            if (controlledEntity != null)
            {
                MatrixD worldMatrixNormalizedInv = controlledEntity.PositionComp.WorldMatrixNormalizedInv;
                controlledEntity.PositionComp.WorldMatrix = newWorldMatrix;
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node node in MyCubeGridGroups.Static.Physical.GetGroup(controlledEntity).m_members)
                {
                    if (node.NodeData.Parent != null)
                    {
                        continue;
                    }
                    if (node.NodeData != controlledEntity)
                    {
                        MatrixD xd2 = (node.NodeData.PositionComp.WorldMatrix * worldMatrixNormalizedInv) * newWorldMatrix;
                        node.NodeData.PositionComp.WorldMatrix = xd2;
                    }
                }
            }
            else if (this.ControlledEntity.Parent != null)
            {
                this.ControlledEntity.PositionComp.SetWorldMatrix(newWorldMatrix, this.ControlledEntity.Parent, true, true, true, false, false, false);
            }
            else
            {
                this.ControlledEntity.PositionComp.WorldMatrix = newWorldMatrix;
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (this.Active)
            {
                if ((this.m_dragActive || this.m_rotationActive) && MyInput.Static.IsNewRightMousePressed())
                {
                    this.SetWorldMatrix(ref this.m_storedWorldMatrix);
                    this.m_dragActive = false;
                    this.m_rotationActive = false;
                    this.m_selected = -1;
                }
                if (!this.DisableTransformation)
                {
                    if (this.m_dragActive)
                    {
                        this.PerformDragg(this.m_dragOverAxis);
                    }
                    if (this.m_rotationActive)
                    {
                        this.PerformRotation();
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.R))
                {
                    switch (this.Operation)
                    {
                        case OperationMode.Translation:
                            this.Operation = OperationMode.Rotation;
                            break;

                        case OperationMode.Rotation:
                            this.Operation = OperationMode.Translation;
                            break;

                        case OperationMode.HierarchyAssignment:
                            this.Operation = this.m_previousOperation;
                            break;

                        default:
                            break;
                    }
                }
                if (MyInput.Static.IsNewKeyPressed(MyKeys.T))
                {
                    CoordinateMode mode = this.Mode;
                    if (mode == CoordinateMode.LocalCoords)
                    {
                        this.Mode = CoordinateMode.WorldCoords;
                    }
                    else if (mode == CoordinateMode.WorldCoords)
                    {
                        this.Mode = CoordinateMode.LocalCoords;
                    }
                    if (this.ControlledEntity != null)
                    {
                        this.UpdateGizmoPosition();
                    }
                }
                if (MyInput.Static.IsNewLeftMousePressed())
                {
                    if (this.DisablePicking)
                    {
                        return;
                    }
                    this.m_lastRay = this.CreateRayFromCursorPosition();
                    if (this.RayCasted != null)
                    {
                        this.RayCasted(this.m_lastRay);
                    }
                    if ((this.ControlledEntity != null) && this.PickControl())
                    {
                        this.m_storedWorldMatrix = this.ControlledEntity.PositionComp.WorldMatrix;
                    }
                    else
                    {
                        this.m_selected = -1;
                        MyEntity entity = this.PickEntity();
                        this.SetControlledEntity(entity);
                    }
                }
                if (MyInput.Static.IsNewLeftMouseReleased())
                {
                    this.m_dragActive = false;
                    this.m_rotationActive = false;
                    this.m_selected = -1;
                }
                if ((this.ControlledEntity != null) && (this.ControlledEntity.Physics != null))
                {
                    this.ControlledEntity.Physics.ClearSpeed();
                }
            }
        }

        private void UpdateGizmoPosition()
        {
            MatrixD worldMatrix = this.ControlledEntity.PositionComp.WorldMatrix;
            double radius = this.ControlledEntity.PositionComp.WorldVolume.Radius;
            if (radius <= 0.0)
            {
                radius++;
            }
            radius += (float) (this.ControlledEntity.PositionComp.WorldVolume.Center - worldMatrix.Translation).Length();
            this.m_gizmoMatrix = MatrixD.Identity;
            if (this.Mode != CoordinateMode.LocalCoords)
            {
                this.m_gizmoMatrix.Translation = worldMatrix.Translation;
            }
            else
            {
                this.m_gizmoMatrix = worldMatrix;
                this.m_gizmoMatrix = MatrixD.Normalize(this.m_gizmoMatrix);
            }
            this.m_xBB.Center = new Vector3D(radius, 0.0, 0.0);
            this.m_yBB.Center = new Vector3D(0.0, radius, 0.0);
            this.m_zBB.Center = new Vector3D(0.0, 0.0, radius);
            this.m_xBB.Orientation = Quaternion.Identity;
            this.m_yBB.Orientation = Quaternion.Identity;
            this.m_zBB.Orientation = Quaternion.Identity;
            this.m_xBB.Transform(this.m_gizmoMatrix);
            this.m_yBB.Transform(this.m_gizmoMatrix);
            this.m_zBB.Transform(this.m_gizmoMatrix);
            this.m_xPlane.Center = new Vector3D((double) (-PLANE_THICKNESS / 2f), radius / 2.0, radius / 2.0);
            this.m_yPlane.Center = new Vector3D(radius / 2.0, (double) (PLANE_THICKNESS / 2f), radius / 2.0);
            this.m_zPlane.Center = new Vector3D(radius / 2.0, radius / 2.0, (double) (PLANE_THICKNESS / 2f));
            this.m_xPlane.HalfExtent = new Vector3D((double) (PLANE_THICKNESS / 2f), radius / 2.0, radius / 2.0);
            this.m_yPlane.HalfExtent = new Vector3D(radius / 2.0, (double) (PLANE_THICKNESS / 2f), radius / 2.0);
            this.m_zPlane.HalfExtent = new Vector3D(radius / 2.0, radius / 2.0, (double) (PLANE_THICKNESS / 2f));
            this.m_xPlane.Orientation = Quaternion.Identity;
            this.m_yPlane.Orientation = Quaternion.Identity;
            this.m_zPlane.Orientation = Quaternion.Identity;
            this.m_xPlane.Transform(this.m_gizmoMatrix);
            this.m_yPlane.Transform(this.m_gizmoMatrix);
            this.m_zPlane.Transform(this.m_gizmoMatrix);
        }

        public bool Active
        {
            get => 
                this.m_active;
            set
            {
                if ((base.Session != null) && ((base.Session.CreativeMode || MySession.Static.IsUserAdmin(Sync.MyId)) || MySession.Static.CreativeToolsEnabled(Sync.MyId)))
                {
                    if (!value)
                    {
                        this.SetControlledEntity(null);
                    }
                    this.m_active = value;
                }
            }
        }

        public bool DisableTransformation { get; set; }

        public CoordinateMode Mode { get; set; }

        public OperationMode Operation { get; set; }

        public MyEntity ControlledEntity =>
            this.m_controlledEntity;

        public bool DisablePicking { get; set; }

        public LineD LastRay =>
            this.m_lastRay;

        public enum CoordinateMode
        {
            LocalCoords,
            WorldCoords
        }

        public enum OperationMode
        {
            Translation,
            Rotation,
            HierarchyAssignment
        }
    }
}

