namespace Sandbox.Game.Replication.History
{
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Blocks;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.Game.Networking;
    using VRage.Groups;
    using VRageMath;

    public static class MySnapshotCache
    {
        public static long DEBUG_ENTITY_ID;
        public static bool PROPAGATE_TO_CONNECTIONS = true;
        private static readonly Dictionary<MyEntity, MyItem> m_cache = new Dictionary<MyEntity, MyItem>();

        public static void Add(MyEntity entity, ref MySnapshot snapshot, MySnapshotFlags snapshotFlags, bool reset)
        {
            MyItem item = new MyItem {
                Snapshot = snapshot,
                SnapshotFlags = snapshotFlags,
                Reset = reset
            };
            m_cache[entity] = item;
        }

        public static void Apply()
        {
            using (Dictionary<MyEntity, MyItem>.Enumerator enumerator = m_cache.GetEnumerator())
            {
                while (true)
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            KeyValuePair<MyEntity, MyItem> current = enumerator.Current;
                            MyEntity key = current.Key;
                            if (key.Closed)
                            {
                                continue;
                            }
                            if (key.MarkedForClose)
                            {
                                continue;
                            }
                            if (MyFakes.SNAPSHOTCACHE_HIERARCHY)
                            {
                                MyEntity entity = key;
                                while (true)
                                {
                                    bool flag3;
                                    entity = MySnapshot.GetParent(entity, out flag3);
                                    if ((entity == null) || m_cache.ContainsKey(entity))
                                    {
                                        if (entity == null)
                                        {
                                            break;
                                        }
                                        continue;
                                    }
                                }
                            }
                            MyItem item = current.Value;
                            if (item.SnapshotFlags.ApplyPhysicsLinear || item.SnapshotFlags.ApplyPhysicsAngular)
                            {
                                ApplyPhysics(key, item);
                            }
                            bool applyPosition = item.SnapshotFlags.ApplyPosition;
                            bool applyRotation = item.SnapshotFlags.ApplyRotation;
                            if (applyPosition | applyRotation)
                            {
                                MatrixD xd;
                                item.Snapshot.GetMatrix(key, out xd, applyPosition, applyRotation);
                                bool reset = MySnapshot.ApplyReset && item.Reset;
                                MyCubeGrid child = key as MyCubeGrid;
                                if (!MyFakes.SNAPSHOTCACHE_HIERARCHY || (child == null))
                                {
                                    ApplyChildMatrixLite(key, ref xd, reset);
                                }
                                else
                                {
                                    MatrixD xd2;
                                    Vector3 vector;
                                    CalculateDiffs(key, ref xd, out xd2, out vector);
                                    ApplyChildMatrix(child, ref xd, ref xd2, ref vector, reset);
                                }
                            }
                        }
                        else
                        {
                            goto TR_0000;
                        }
                    }
                }
            }
        TR_0000:
            m_cache.Clear();
        }

        private static void ApplyChildMatrix(MyEntity child, ref MatrixD mat, ref MatrixD diffMat, ref Vector3 diffPos, bool reset)
        {
            ApplyChildMatrixLite(child, ref mat, reset);
            if (PROPAGATE_TO_CONNECTIONS)
            {
                PropagateToConnections(child, ref diffMat, ref diffPos, reset);
            }
        }

        private static void ApplyChildMatrixLite(MyEntity child, ref MatrixD mat, bool reset)
        {
            MyCubeGrid grid = child as MyCubeGrid;
            if ((grid == null) || !grid.IsStatic)
            {
                MyEntity cameraController = MySession.Static.CameraController as MyEntity;
                if ((cameraController != null) && (ReferenceEquals(child, cameraController) || ReferenceEquals(child, cameraController.GetTopMostParent(null))))
                {
                    MatrixD transformDelta = child.PositionComp.WorldMatrixInvScaled * mat;
                    MyThirdPersonSpectator.Static.CompensateQuickTransformChange(ref transformDelta);
                }
                child.m_positionResetFromServer = reset;
                child.PositionComp.SetWorldMatrix(mat, MyGridPhysicalHierarchy.Static, false, true, true, false, reset, false);
                if ((grid != null) && grid.InScene)
                {
                    MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node node = MyCubeGridGroups.Static.Mechanical.GetNode(grid);
                    foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> pair in node.ParentLinks)
                    {
                        MyPistonBase entityById = MyEntities.GetEntityById(pair.Key, false) as MyPistonBase;
                        if (entityById != null)
                        {
                            entityById.SetCurrentPosByTopGridMatrix();
                        }
                    }
                    foreach (KeyValuePair<long, MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node> pair2 in node.ChildLinks)
                    {
                        MyPistonBase entityById = MyEntities.GetEntityById(pair2.Key, false) as MyPistonBase;
                        if (entityById != null)
                        {
                            entityById.SetCurrentPosByTopGridMatrix();
                        }
                    }
                }
            }
        }

        private static void ApplyPhysics(MyEntity entity, MyItem value)
        {
            bool applyPhysicsAngular = value.SnapshotFlags.ApplyPhysicsAngular;
            value.Snapshot.ApplyPhysics(entity, applyPhysicsAngular, value.SnapshotFlags.ApplyPhysicsLinear, value.SnapshotFlags.ApplyPhysicsLocal);
        }

        private static void CalculateDiffs(MyEntity entity, ref MatrixD mat, out MatrixD diffMat, out Vector3 diffPos)
        {
            Vector3D vectord;
            Vector3D vectord2;
            Vector3 center;
            MatrixD worldMatrixInvScaled = entity.PositionComp.WorldMatrixInvScaled;
            diffMat = worldMatrixInvScaled * mat;
            if ((entity.Physics == null) || (entity.Physics.RigidBody == null))
            {
                center = entity.PositionComp.LocalAABB.Center;
            }
            else
            {
                center = entity.Physics.CenterOfMassLocal;
            }
            Vector3 position = center;
            Vector3D.Transform(ref position, ref mat, out vectord);
            MatrixD worldMatrix = entity.PositionComp.WorldMatrix;
            Vector3D.Transform(ref position, ref worldMatrix, out vectord2);
            diffPos = (Vector3) (vectord - vectord2);
        }

        private static void CalculateMatrix(MyEntity child, ref MatrixD diffMat, ref Vector3 diffPos, bool inheritRotation, out MatrixD newChildMatrix)
        {
            if (!inheritRotation)
            {
                newChildMatrix = child.WorldMatrix;
                newChildMatrix.Translation += diffPos;
            }
            else
            {
                MyCharacter character = child as MyCharacter;
                if ((character == null) || (character.Gravity.LengthSquared() <= 0.1f))
                {
                    newChildMatrix = child.WorldMatrix * diffMat;
                }
                else if ((character.Physics == null) || (character.Physics.CharacterProxy == null))
                {
                    newChildMatrix = child.WorldMatrix * diffMat;
                }
                else if (!character.Physics.CharacterProxy.Supported)
                {
                    newChildMatrix = child.WorldMatrix;
                    newChildMatrix.Translation += diffPos;
                }
                else
                {
                    Vector3D up = child.WorldMatrix.Up;
                    newChildMatrix = child.WorldMatrix * diffMat;
                    Vector3D v = newChildMatrix.Up;
                    double num = up.Dot(ref v);
                    if (Math.Abs((double) (Math.Abs(num) - 1.0)) > 9.9999997473787516E-05)
                    {
                        Vector3D vectord3;
                        MatrixD xd2;
                        Vector3D.Cross(ref up, ref v, out vectord3);
                        vectord3.Normalize();
                        MatrixD.CreateFromAxisAngle(ref vectord3, -Math.Acos(num), out xd2);
                        Vector3D translation = newChildMatrix.Translation;
                        newChildMatrix.Translation = Vector3D.Zero;
                        MatrixD.Multiply(ref newChildMatrix, ref xd2, out newChildMatrix);
                        newChildMatrix.Translation = translation;
                    }
                }
                newChildMatrix.Orthogonalize();
            }
        }

        private static void PropagateToChild(MyEntity child, MatrixD diffMat, Vector3 diffPos, bool reset)
        {
            bool inheritRotation;
            MyItem item;
            MatrixD mat = new MatrixD();
            bool flag = false;
            if (!m_cache.TryGetValue(child, out item))
            {
                inheritRotation = (child.LastSnapshotFlags == null) || child.LastSnapshotFlags.InheritRotation;
            }
            else
            {
                bool applyPosition = item.SnapshotFlags.ApplyPosition;
                bool applyRotation = item.SnapshotFlags.ApplyRotation;
                inheritRotation = item.SnapshotFlags.InheritRotation;
                if (applyPosition | applyRotation)
                {
                    MatrixD worldMatrix;
                    if (applyPosition != applyRotation)
                    {
                        CalculateMatrix(child, ref diffMat, ref diffPos, inheritRotation, out worldMatrix);
                    }
                    else
                    {
                        worldMatrix = child.WorldMatrix;
                    }
                    item.Snapshot.GetMatrix(out mat, ref worldMatrix, applyPosition, applyRotation);
                    CalculateDiffs(child, ref mat, out diffMat, out diffPos);
                    flag = true;
                }
                if (item.SnapshotFlags.ApplyPhysicsLinear || item.SnapshotFlags.ApplyPhysicsAngular)
                {
                    ApplyPhysics(child, item);
                }
                reset |= item.Reset;
            }
            if (!flag)
            {
                CalculateMatrix(child, ref diffMat, ref diffPos, inheritRotation, out mat);
            }
            ApplyChildMatrix(child, ref mat, ref diffMat, ref diffPos, reset);
        }

        private static void PropagateToConnections(MyEntity grid, ref MatrixD diffMat, ref Vector3 diffPos, bool reset)
        {
            MatrixD localDiffMat = diffMat;
            Vector3 localDiffPos = diffPos;
            MyGridPhysicalHierarchy.Static.ApplyOnAllChildren(grid, child => PropagateToChild(child, localDiffMat, localDiffPos, reset));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MyItem
        {
            public MySnapshot Snapshot;
            public MySnapshotFlags SnapshotFlags;
            public bool Reset;
        }
    }
}

