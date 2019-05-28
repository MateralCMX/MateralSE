namespace Sandbox.Engine.Physics
{
    using Havok;
    using Sandbox.Engine.Utils;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Components;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    internal static class MyPhysicsDebugDraw
    {
        public static bool DebugDrawFlattenHierarchy = false;
        public static bool HkGridShapeCellDebugDraw = false;
        public static HkGeometry DebugGeometry;
        private static Color[] boxColors = MyUtils.GenerateBoxColors();
        private static List<HkShape> m_tmpShapeList = new List<HkShape>();
        private static Dictionary<string, Vector3D> DebugShapesPositions = new Dictionary<string, Vector3D>();

        public static void DebugDrawAddForce(MyPhysicsBody physics, MyPhysicsForceType type, Vector3? force, Vector3D? position, Vector3? torque, bool persistent = false)
        {
            switch (type)
            {
                case MyPhysicsForceType.APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE:
                {
                    Vector3D pointFrom = position.Value + (physics.LinearVelocity * 0.01666667f);
                    if (force != null)
                    {
                        MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (force.Value * 0.1f), Color.Blue, new Color?(Color.Red), false, 0.1, null, 0.5f, persistent);
                    }
                    if (torque == null)
                    {
                        break;
                    }
                    MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (torque.Value * 0.1f), Color.Blue, new Color?(Color.Purple), false, 0.1, null, 0.5f, persistent);
                    return;
                }
                case MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE:
                    if (physics.RigidBody != null)
                    {
                        Matrix rigidBodyMatrix = physics.RigidBody.GetRigidBodyMatrix();
                        Vector3D pointFrom = physics.CenterOfMassWorld + (physics.LinearVelocity * 0.01666667f);
                        if (force != null)
                        {
                            MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (Vector3.TransformNormal(force.Value, rigidBodyMatrix) * 0.1f), Color.Blue, new Color?(Color.Red), false, 0.1, null, 0.5f, persistent);
                        }
                        if (torque != null)
                        {
                            MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (Vector3.TransformNormal(torque.Value, rigidBodyMatrix) * 0.1f), Color.Blue, new Color?(Color.Purple), false, 0.1, null, 0.5f, persistent);
                            return;
                        }
                    }
                    break;

                case MyPhysicsForceType.APPLY_WORLD_FORCE:
                    if (position != null)
                    {
                        Vector3D pointFrom = position.Value + (physics.LinearVelocity * 0.01666667f);
                        if (force != null)
                        {
                            MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + ((force.Value * 0.01666667f) * 0.1f), Color.Blue, new Color?(Color.Red), false, 0.1, null, 0.5f, persistent);
                        }
                    }
                    break;

                default:
                    return;
            }
        }

        public static void DebugDrawBreakable(HkdBreakableBody bb, Vector3 offset)
        {
            DebugShapesPositions.Clear();
            if (bb != null)
            {
                int shapeIndex = 0;
                Matrix rigidBodyMatrix = bb.GetRigidBody().GetRigidBodyMatrix();
                MatrixD worldMatrix = MatrixD.CreateWorld(rigidBodyMatrix.Translation + offset, rigidBodyMatrix.Forward, rigidBodyMatrix.Up);
                DrawBreakableShape(bb.BreakableShape, worldMatrix, 0.3f, ref shapeIndex, null, false);
                DrawConnections(bb.BreakableShape, worldMatrix, 0.3f, ref shapeIndex, null, false);
            }
        }

        public static void DebugDrawCoordinateSystem(Vector3? position, Vector3? forward, Vector3? side, Vector3? up, float scale = 1f)
        {
            if (position != null)
            {
                Vector3D pointFrom = position.Value;
                if (forward != null)
                {
                    MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (forward.Value * scale), Color.Blue, new Color?(Color.Red), false, 0.1, null, 0.5f, false);
                }
                if (side != null)
                {
                    MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (side.Value * scale), Color.Blue, new Color?(Color.Green), false, 0.1, null, 0.5f, false);
                }
                if (up != null)
                {
                    MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (up.Value * scale), Color.Blue, new Color?(Color.Blue), false, 0.1, null, 0.5f, false);
                }
            }
        }

        public static void DebugDrawVector3(Vector3? position, Vector3? vector, Color color, float scale = 0.01f)
        {
            if (position != null)
            {
                Vector3D pointFrom = position.Value;
                if (vector != null)
                {
                    MyRenderProxy.DebugDrawArrow3D(pointFrom, pointFrom + (vector.Value * scale), color, new Color?(color), false, 0.1, null, 0.5f, false);
                }
            }
        }

        private static void DrawBreakableShape(HkdBreakableShape breakableShape, MatrixD worldMatrix, float alpha, ref int shapeIndex, string customText = null, bool isPhantom = false)
        {
            object[] objArray1 = new object[] { breakableShape.Name, " Strength: ", breakableShape.GetStrenght(), " Static:", breakableShape.IsFixed().ToString() };
            DrawCollisionShape(breakableShape.GetShape(), worldMatrix, alpha, ref shapeIndex, string.Concat(objArray1), false);
            if (!string.IsNullOrEmpty(breakableShape.Name) && (breakableShape.Name != "PineTree175m_v2_001"))
            {
                breakableShape.IsFixed();
            }
            DebugShapesPositions[breakableShape.Name] = worldMatrix.Translation;
            List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
            breakableShape.GetChildren(list);
            Vector3 coM = breakableShape.CoM;
            foreach (HkdShapeInstanceInfo info in list)
            {
                Matrix matrix = (info.GetTransform() * worldMatrix) * Matrix.CreateTranslation(Vector3.Right * 2f);
                DrawBreakableShape(info.Shape, matrix, alpha, ref shapeIndex, null, false);
            }
        }

        public static unsafe void DrawCollisionShape(HkShape shape, MatrixD worldMatrix, float alpha, ref int shapeIndex, string customText = null, bool isPhantom = false)
        {
            Color color = GetShapeColor(shape.ShapeType, ref shapeIndex, isPhantom);
            if (isPhantom)
            {
                alpha *= alpha;
            }
            color.A = (byte) (alpha * 255f);
            bool smooth = true;
            float num = 0.02f;
            float num2 = 1.035f;
            bool flag2 = false;
            HkShapeType shapeType = shape.ShapeType;
            switch (shapeType)
            {
                case HkShapeType.Sphere:
                {
                    float radius = ((HkSphereShape) shape).Radius;
                    MyRenderProxy.DebugDrawSphere(worldMatrix.Translation, radius, color, alpha, true, smooth, true, false);
                    if (isPhantom)
                    {
                        MyRenderProxy.DebugDrawSphere(worldMatrix.Translation, radius, color, 1f, true, false, true, false);
                        MyRenderProxy.DebugDrawSphere(worldMatrix.Translation, radius, color, 1f, true, false, false, false);
                    }
                    flag2 = true;
                    goto TR_0003;
                }
                case HkShapeType.Cylinder:
                {
                    HkCylinderShape shape4 = (HkCylinderShape) shape;
                    MyRenderProxy.DebugDrawCylinder(worldMatrix, shape4.VertexA, shape4.VertexB, shape4.Radius, color, alpha, true, smooth, false);
                    flag2 = true;
                    goto TR_0003;
                }
                case HkShapeType.Triangle:
                {
                    HkTriangleShape shape12 = (HkTriangleShape) shape;
                    MyRenderProxy.DebugDrawTriangle(shape12.Pt0, shape12.Pt1, shape12.Pt2, Color.Green, false, false, false);
                    goto TR_0003;
                }
                case HkShapeType.Box:
                {
                    HkBoxShape shape5 = (HkBoxShape) shape;
                    MyRenderProxy.DebugDrawOBB(MatrixD.CreateScale((shape5.HalfExtents * 2f) + new Vector3(num)) * worldMatrix, color, alpha, true, smooth, true, false);
                    if (isPhantom)
                    {
                        MyRenderProxy.DebugDrawOBB(Matrix.CreateScale((shape5.HalfExtents * 2f) + new Vector3(num)) * worldMatrix, color, 1f, true, false, true, false);
                        MyRenderProxy.DebugDrawOBB(Matrix.CreateScale((shape5.HalfExtents * 2f) + new Vector3(num)) * worldMatrix, color, 1f, true, false, false, false);
                    }
                    flag2 = true;
                    goto TR_0003;
                }
                case HkShapeType.Capsule:
                {
                    HkCapsuleShape shape3 = (HkCapsuleShape) shape;
                    MyRenderProxy.DebugDrawCapsule(Vector3.Transform(shape3.VertexA, worldMatrix), Vector3.Transform(shape3.VertexB, worldMatrix), shape3.Radius, color, true, smooth, false);
                    flag2 = true;
                    goto TR_0003;
                }
                case HkShapeType.ConvexVertices:
                {
                    Vector3 vector;
                    ((HkConvexVerticesShape) shape).GetGeometry(DebugGeometry, out vector);
                    Vector3D vectord2 = Vector3D.Transform(vector, worldMatrix.GetOrientation());
                    MatrixD xd = worldMatrix;
                    xd = MatrixD.CreateScale((double) num2) * xd;
                    MatrixD* xdPtr1 = (MatrixD*) ref xd;
                    xdPtr1.Translation -= vectord2 * (num2 - 1f);
                    DrawGeometry(DebugGeometry, xd, color, true, true);
                    flag2 = true;
                    goto TR_0003;
                }
                case HkShapeType.TriSampledHeightFieldCollection:
                case HkShapeType.TriSampledHeightFieldBvTree:
                case HkShapeType.SampledHeightField:
                case HkShapeType.ExtendedMesh:
                case HkShapeType.Transform:
                case HkShapeType.CompressedMesh:
                case HkShapeType.Collection:
                case HkShapeType.User0:
                case HkShapeType.User1:
                case HkShapeType.User2:
                    goto TR_0003;

                case HkShapeType.List:
                {
                    HkShapeContainerIterator iterator = ((HkListShape) shape).GetIterator();
                    while (iterator.IsValid)
                    {
                        DrawCollisionShape(iterator.CurrentValue, worldMatrix, alpha, ref shapeIndex, customText, false);
                        iterator.Next();
                    }
                    goto TR_0003;
                }
                case HkShapeType.Mopp:
                    DrawCollisionShape((HkShape) ((HkMoppBvTreeShape) shape).ShapeCollection, worldMatrix, alpha, ref shapeIndex, customText, false);
                    goto TR_0003;

                case HkShapeType.ConvexTranslate:
                {
                    HkConvexTranslateShape shape7 = (HkConvexTranslateShape) shape;
                    DrawCollisionShape((HkShape) shape7.ChildShape, Matrix.CreateTranslation(shape7.Translation) * worldMatrix, alpha, ref shapeIndex, customText, false);
                    goto TR_0003;
                }
                case HkShapeType.ConvexTransform:
                {
                    HkConvexTransformShape shape8 = (HkConvexTransformShape) shape;
                    DrawCollisionShape((HkShape) shape8.ChildShape, shape8.Transform * worldMatrix, alpha, ref shapeIndex, customText, false);
                    goto TR_0003;
                }
                case HkShapeType.StaticCompound:
                {
                    HkStaticCompoundShape shape11 = (HkStaticCompoundShape) shape;
                    if (DebugDrawFlattenHierarchy)
                    {
                        HkShapeContainerIterator iterator2 = shape11.GetIterator();
                        while (iterator2.IsValid)
                        {
                            if (shape11.IsShapeKeyEnabled(iterator2.CurrentShapeKey))
                            {
                                object[] objArray1 = new object[4];
                                object[] objArray2 = new object[4];
                                objArray2[0] = customText ?? string.Empty;
                                object[] local2 = objArray2;
                                local2[1] = "-";
                                local2[2] = iterator2.CurrentShapeKey;
                                local2[3] = "-";
                                string str = string.Concat(local2);
                                DrawCollisionShape(iterator2.CurrentValue, worldMatrix, alpha, ref shapeIndex, str, false);
                            }
                            iterator2.Next();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < shape11.InstanceCount; i++)
                        {
                            string str2;
                            bool flag3 = shape11.IsInstanceEnabled(i);
                            if (flag3)
                            {
                                object[] objArray3 = new object[4];
                                object[] objArray4 = new object[4];
                                objArray4[0] = customText ?? string.Empty;
                                object[] local4 = objArray4;
                                local4[1] = "<";
                                local4[2] = i;
                                local4[3] = ">";
                                str2 = string.Concat(local4);
                            }
                            else
                            {
                                object[] objArray5 = new object[4];
                                object[] objArray6 = new object[4];
                                objArray6[0] = customText ?? string.Empty;
                                object[] local6 = objArray6;
                                local6[1] = "(";
                                local6[2] = i;
                                local6[3] = ")";
                                str2 = string.Concat(local6);
                            }
                            if (flag3)
                            {
                                DrawCollisionShape(shape11.GetInstance(i), shape11.GetInstanceTransform(i) * worldMatrix, alpha, ref shapeIndex, str2, false);
                            }
                        }
                    }
                    goto TR_0003;
                }
                case HkShapeType.BvCompressedMesh:
                    break;

                case HkShapeType.BvTree:
                {
                    HkGridShape shape13 = (HkGridShape) shape;
                    if (HkGridShapeCellDebugDraw && !shape13.Base.IsZero)
                    {
                        float cellSize = shape13.CellSize;
                        int shapeInfoCount = shape13.GetShapeInfoCount();
                        for (int i = 0; i < shapeInfoCount; i++)
                        {
                            try
                            {
                                Vector3S vectors;
                                Vector3S vectors2;
                                shape13.GetShapeInfo(i, out vectors, out vectors2, m_tmpShapeList);
                                Vector3 position = (Vector3) (((vectors2 * cellSize) + (vectors * cellSize)) / 2f);
                                Color color2 = color;
                                if (vectors == vectors2)
                                {
                                    color2 = new Color(1f, 0.2f, 0.1f);
                                }
                                MyRenderProxy.DebugDrawOBB((Matrix.CreateScale((((vectors2 * cellSize) - (vectors * cellSize)) + (Vector3.One * cellSize)) + new Vector3(num)) * Matrix.CreateTranslation(position)) * worldMatrix, color2, alpha, true, smooth, true, false);
                            }
                            finally
                            {
                                m_tmpShapeList.Clear();
                            }
                        }
                    }
                    else
                    {
                        MyRenderMessageDebugDrawTriangles msgInterface = MyRenderProxy.PrepareDebugDrawTriangles();
                        try
                        {
                            using (HkShapeBuffer buffer = new HkShapeBuffer())
                            {
                                HkShapeContainerIterator iterator3 = ((HkBvTreeShape) shape).GetIterator(buffer);
                                while (iterator3.IsValid)
                                {
                                    HkShape currentValue = iterator3.CurrentValue;
                                    if (currentValue.ShapeType != HkShapeType.Triangle)
                                    {
                                        DrawCollisionShape(currentValue, worldMatrix, alpha, ref shapeIndex, null, false);
                                    }
                                    else
                                    {
                                        HkTriangleShape shape17 = (HkTriangleShape) currentValue;
                                        msgInterface.AddTriangle(shape17.Pt0, shape17.Pt1, shape17.Pt2);
                                    }
                                    iterator3.Next();
                                }
                                goto TR_0003;
                            }
                        }
                        finally
                        {
                            msgInterface.Color = color;
                            MyRenderProxy.DebugDrawTriangles(msgInterface, new MatrixD?(worldMatrix), false, false, false, false);
                        }
                        break;
                    }
                    goto TR_0003;
                }
                default:
                    if (shapeType == HkShapeType.Bv)
                    {
                        HkBvShape shape19 = (HkBvShape) shape;
                        DrawCollisionShape(shape19.BoundingVolumeShape, worldMatrix, alpha, ref shapeIndex, null, true);
                        DrawCollisionShape(shape19.ChildShape, worldMatrix, alpha, ref shapeIndex, null, false);
                    }
                    else if (shapeType == HkShapeType.PhantomCallback)
                    {
                        MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, "Phantom", Color.Green, 0.75f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                    goto TR_0003;
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_TRIANGLE_PHYSICS)
            {
                ((HkBvCompressedMeshShape) shape).GetGeometry(DebugGeometry);
                DrawGeometry(DebugGeometry, worldMatrix, Color.Green, false, false);
                flag2 = true;
            }
        TR_0003:
            if (flag2 && (customText != null))
            {
                color.A = 0xff;
                MyRenderProxy.DebugDrawText3D(worldMatrix.Translation, customText, color, 0.8f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        private static void DrawConnections(HkdBreakableShape breakableShape, MatrixD worldMatrix, float alpha, ref int shapeIndex, string customText = null, bool isPhantom = false)
        {
            List<HkdConnection> resultList = new List<HkdConnection>();
            breakableShape.GetConnectionList(resultList);
            List<HkdShapeInstanceInfo> list = new List<HkdShapeInstanceInfo>();
            breakableShape.GetChildren(list);
            foreach (HkdConnection connection in resultList)
            {
                Vector3D pointFrom = DebugShapesPositions[connection.ShapeAName];
                Vector3D pointTo = DebugShapesPositions[connection.ShapeBName];
                bool flag = false;
                foreach (HkdShapeInstanceInfo info in list)
                {
                    if ((info.ShapeName == connection.ShapeAName) || (info.ShapeName == connection.ShapeBName))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, Color.White, Color.White, false, false);
                }
            }
        }

        public static void DrawGeometry(HkGeometry geometry, MatrixD worldMatrix, Color color, bool depthRead = false, bool shaded = false)
        {
            MyRenderMessageDebugDrawTriangles msgInterface = MyRenderProxy.PrepareDebugDrawTriangles();
            try
            {
                int triangleIndex = 0;
                while (true)
                {
                    int num2;
                    int num3;
                    int num4;
                    int num5;
                    if (triangleIndex >= geometry.TriangleCount)
                    {
                        for (int i = 0; i < geometry.VertexCount; i++)
                        {
                            msgInterface.AddVertex(geometry.GetVertex(i));
                        }
                        break;
                    }
                    geometry.GetTriangle(triangleIndex, out num2, out num3, out num4, out num5);
                    msgInterface.AddIndex(num2);
                    msgInterface.AddIndex(num3);
                    msgInterface.AddIndex(num4);
                    triangleIndex++;
                }
            }
            finally
            {
                msgInterface.Color = color;
                MyRenderProxy.DebugDrawTriangles(msgInterface, new MatrixD?(worldMatrix), depthRead, shaded, false, false);
            }
        }

        private static Color GetShapeColor(HkShapeType shapeType, ref int shapeIndex, bool isPhantom)
        {
            if (isPhantom)
            {
                return Color.LightGreen;
            }
            switch (shapeType)
            {
                case HkShapeType.Sphere:
                    return Color.White;

                case HkShapeType.Cylinder:
                    return Color.Orange;

                case HkShapeType.Capsule:
                    return Color.Yellow;

                case HkShapeType.ConvexVertices:
                    return Color.Red;
            }
            int num = shapeIndex + 1;
            shapeIndex = num;
            return boxColors[num % (boxColors.Length - 1)];
        }
    }
}

