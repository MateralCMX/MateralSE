namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public static class MySimpleObjectDraw
    {
        private static readonly MyStringId ID_CONTAINER_BORDER = MyStringId.GetOrCompute("ContainerBorder");
        private static readonly MyStringId ID_GIZMO_DRAW_LINE = MyStringId.GetOrCompute("GizmoDrawLine");
        private static FaceInfo[] temporary_faces = new FaceInfo[6];
        private static List<LineD> m_lineBuffer = new List<LineD>(0x7d0);
        private static readonly List<Vector3D> m_verticesBuffer = new List<Vector3D>(0x7d0);

        public static void DrawAttachedTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, uint renderObjectID, ref MatrixD worldToLocal, MySimpleObjectRasterizer rasterization, int wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false)
        {
            DrawAttachedTransparentBox(ref worldMatrix, ref localbox, ref color, renderObjectID, ref worldToLocal, rasterization, new Vector3I(wireDivideRatio), lineWidth, faceMaterial, lineMaterial, onlyFrontFaces, MyBillboard.BlendTypeEnum.Standard);
        }

        public static void DrawAttachedTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, uint renderObjectID, ref MatrixD worldToLocal, MySimpleObjectRasterizer rasterization, Vector3I wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, MyBillboard.BlendTypeEnum blendType = 0)
        {
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_001D;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_001D:
            if ((rasterization == MySimpleObjectRasterizer.Solid) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                MyQuadD dd;
                localbox.Min;
                localbox.Max;
                MatrixD identity = MatrixD.Identity;
                identity.Forward = worldMatrix.Forward;
                identity.Up = worldMatrix.Up;
                identity.Right = worldMatrix.Right;
                float width = ((float) (localbox.Max.X - localbox.Min.X)) / 2f;
                float height = ((float) (localbox.Max.Y - localbox.Min.Y)) / 2f;
                float num3 = ((float) (localbox.Max.Z - localbox.Min.Z)) / 2f;
                Vector3D normal = Vector3D.TransformNormal(Vector3D.Forward, identity);
                Vector3D center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + (normal * num3);
                Vector3D vectord1 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - (normal * num3);
                Vector3D vectord3 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
                MatrixD matrix = Matrix.CreateRotationY(MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3D.TransformNormal(Vector3D.Left, (MatrixD) worldMatrix);
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + (normal * width);
                Vector3D vectord4 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - (normal * width);
                Vector3D vectord5 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
                matrix = MatrixD.CreateRotationX((double) MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3D.TransformNormal(Vector3D.Up, (MatrixD) worldMatrix);
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + (normal * height);
                Vector3D vectord6 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - (normal * height);
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    Vector3D.Transform(ref dd.Point0, ref worldToLocal, out dd.Point0);
                    Vector3D.Transform(ref dd.Point1, ref worldToLocal, out dd.Point1);
                    Vector3D.Transform(ref dd.Point2, ref worldToLocal, out dd.Point2);
                    Vector3D.Transform(ref dd.Point3, ref worldToLocal, out dd.Point3);
                    MyTransparentGeometry.AddAttachedQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, renderObjectID, blendType, null);
                }
            }
            if ((rasterization == MySimpleObjectRasterizer.Wireframe) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                Vector4 vctColor = (Vector4) (color * 1.3f);
                DrawAttachedWireFramedBox(ref worldMatrix, ref localbox, renderObjectID, ref worldToLocal, ref vctColor, lineWidth, wireDivideRatio, lineMaterial, onlyFrontFaces, blendType);
            }
        }

        private static void DrawAttachedWireFramedBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, uint renderObjectID, ref MatrixD worldToLocal, ref Vector4 vctColor, float fThickRatio, Vector3I wireDivideRatio, MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, MyBillboard.BlendTypeEnum blendType = 0)
        {
            if (lineMaterial != null)
            {
                MyStringId? nullable = lineMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_0035;
                }
            }
            lineMaterial = new MyStringId?(MyTransparentMaterials.ErrorMaterial.Id);
        TR_0035:
            m_lineBuffer.Clear();
            MatrixD identity = MatrixD.Identity;
            identity.Forward = worldMatrix.Forward;
            identity.Up = worldMatrix.Up;
            identity.Right = worldMatrix.Right;
            Vector3D.Dot(identity.Forward, MyTransparentGeometry.Camera.Forward);
            Vector3D.Dot(identity.Right, MyTransparentGeometry.Camera.Forward);
            Vector3D.Dot(identity.Up, MyTransparentGeometry.Camera.Forward);
            Vector3D forward = identity.Forward;
            Vector3D right = identity.Right;
            Vector3D up = identity.Up;
            float x = (float) localbox.Size.X;
            float y = (float) localbox.Size.Y;
            float z = (float) localbox.Size.Z;
            Vector3D center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (forward * (z * 0.5f));
            Vector3D vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (forward * (z * 0.5f));
            Vector3D min = localbox.Min;
            Vector3D vctEnd = min + (Vector3.Up * y);
            Vector3D vctSideStep = Vector3.Right * (x / ((float) wireDivideRatio.X));
            Vector3D vectord1 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix);
            if (!onlyFrontFaces || FaceVisible(center, forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min += Vector3.Backward * z;
            vctEnd = min + (Vector3.Up * y);
            if (!onlyFrontFaces || FaceVisible(vectord5, -forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Right * x);
            vctSideStep = Vector3.Up * (y / ((float) wireDivideRatio.Y));
            if (!onlyFrontFaces || FaceVisible(center, forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min += Vector3.Backward * z;
            vctEnd += Vector3.Backward * z;
            if (!onlyFrontFaces || FaceVisible(vectord5, -forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            Matrix.CreateRotationY(MathHelper.ToRadians((float) 90f)) * worldMatrix;
            center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (right * (x * 0.5f));
            vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (right * (x * 0.5f));
            min = localbox.Min;
            vctEnd = min + (Vector3.Backward * z);
            vctSideStep = Vector3.Up * (y / ((float) wireDivideRatio.Y));
            Vector3D vectord9 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix);
            if (!onlyFrontFaces || FaceVisible(center, -right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min = localbox.Min + (Vector3.Right * x);
            vctEnd = min + (Vector3.Backward * z);
            if (!onlyFrontFaces || FaceVisible(vectord5, right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Up * y);
            vctSideStep = Vector3.Backward * (z / ((float) wireDivideRatio.Z));
            if (!onlyFrontFaces || FaceVisible(center, -right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min += Vector3.Right * x;
            vctEnd += Vector3.Right * x;
            if (!onlyFrontFaces || FaceVisible(vectord5, right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (up * (y * 0.5f));
            vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (up * (y * 0.5f));
            min = localbox.Min;
            vctEnd = min + (Vector3.Right * x);
            vctSideStep = Vector3.Backward * (z / ((float) wireDivideRatio.Z));
            if (!onlyFrontFaces || FaceVisible(center, -up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min += Vector3.Up * y;
            vctEnd += Vector3.Up * y;
            if (!onlyFrontFaces || FaceVisible(vectord5, up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Backward * z);
            vctSideStep = Vector3.Right * (x / ((float) wireDivideRatio.X));
            if (!onlyFrontFaces || FaceVisible(center, -up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min += Vector3.Up * y;
            vctEnd += Vector3.Up * y;
            if (!onlyFrontFaces || FaceVisible(vectord5, up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            Vector3 vector = new Vector3(localbox.Max.X - localbox.Min.X, localbox.Max.Y - localbox.Min.Y, localbox.Max.Z - localbox.Min.Z);
            float thickness = MathHelper.Max(1f, MathHelper.Min(MathHelper.Min(vector.X, vector.Y), vector.Z)) * fThickRatio;
            foreach (LineD ed in m_lineBuffer)
            {
                MyTransparentGeometry.AddLineBillboard(lineMaterial.Value, vctColor, ed.From, renderObjectID, ref worldToLocal, (Vector3) ed.Direction, (float) ed.Length, thickness, blendType, -1, 1f, null);
            }
        }

        public static void DrawLine(Vector3D start, Vector3D end, MyStringId? material, ref Vector4 color, float thickness, MyBillboard.BlendTypeEnum blendtype = 0)
        {
            Vector3 vec = (Vector3) (end - start);
            float length = vec.Length();
            if (length > 0.1f)
            {
                vec = MyUtils.Normalize(vec);
                MyStringId? nullable = material;
                MyTransparentGeometry.AddLineBillboard((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE, color, start, vec, length, thickness, blendtype, -1, 1f, null);
            }
        }

        public static void DrawTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, MySimpleObjectRasterizer rasterization, int wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            DrawTransparentBox(ref worldMatrix, ref localbox, ref color, ref color, rasterization, new Vector3I(wireDivideRatio), lineWidth, faceMaterial, lineMaterial, onlyFrontFaces, customViewProjection, blendType, intensity, persistentBillboards);
        }

        public static void DrawTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, ref Color frontFaceColor, MySimpleObjectRasterizer rasterization, int wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            DrawTransparentBox(ref worldMatrix, ref localbox, ref color, ref frontFaceColor, rasterization, new Vector3I(wireDivideRatio), lineWidth, faceMaterial, lineMaterial, onlyFrontFaces, customViewProjection, blendType, intensity, persistentBillboards);
        }

        public static void DrawTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, ref Color frontFaceColor, MySimpleObjectRasterizer rasterization, Vector3I wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_001D;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_001D:
            if ((rasterization == MySimpleObjectRasterizer.Solid) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                MyQuadD dd;
                localbox.Min;
                localbox.Max;
                MatrixD identity = MatrixD.Identity;
                identity.Forward = worldMatrix.Forward;
                identity.Up = worldMatrix.Up;
                identity.Right = worldMatrix.Right;
                float width = ((float) (localbox.Max.X - localbox.Min.X)) / 2f;
                float height = ((float) (localbox.Max.Y - localbox.Min.Y)) / 2f;
                float num3 = ((float) (localbox.Max.Z - localbox.Min.Z)) / 2f;
                Vector3D normal = Vector3D.TransformNormal(Vector3.Forward, identity) * num3;
                Vector3D center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                Vector3D vectord1 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) frontFaceColor, ref center, customViewProjection, blendType, persistentBillboards);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                Vector3D vectord3 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, persistentBillboards);
                }
                MatrixD matrix = MatrixD.CreateRotationY((double) MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3.TransformNormal(Vector3.Left, worldMatrix) * width;
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                Vector3D vectord4 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, persistentBillboards);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                Vector3D vectord5 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, persistentBillboards);
                }
                matrix = Matrix.CreateRotationX(MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3.TransformNormal(Vector3.Up, worldMatrix) * ((localbox.Max.Y - localbox.Min.Y) / 2.0);
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                Vector3D vectord6 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!onlyFrontFaces || FaceVisible(center, normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, persistentBillboards);
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                if (!onlyFrontFaces || FaceVisible(center, -normal))
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, persistentBillboards);
                }
            }
            if ((rasterization == MySimpleObjectRasterizer.Wireframe) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                Color color2 = color * 1.3f;
                DrawWireFramedBox(ref worldMatrix, ref localbox, ref color2, lineWidth, wireDivideRatio, lineMaterial, onlyFrontFaces, customViewProjection, blendType, intensity, persistentBillboards);
            }
        }

        public static void DrawTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color faceX_P, ref Color faceY_P, ref Color faceZ_P, ref Color faceX_N, ref Color faceY_N, ref Color faceZ_N, ref Color wire, MySimpleObjectRasterizer rasterization, int wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            DrawTransparentBox(ref worldMatrix, ref localbox, ref faceX_P, ref faceY_P, ref faceZ_P, ref faceX_N, ref faceY_N, ref faceZ_N, ref wire, rasterization, new Vector3I(wireDivideRatio), lineWidth, faceMaterial, lineMaterial, onlyFrontFaces, customViewProjection, blendType, intensity, persistentBillboards);
        }

        public static void DrawTransparentBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color faceX_P, ref Color faceY_P, ref Color faceZ_P, ref Color faceX_N, ref Color faceY_N, ref Color faceZ_N, ref Color wire, MySimpleObjectRasterizer rasterization, Vector3I wireDivideRatio, float lineWidth = 1f, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_002B;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_002B:
            if ((rasterization == MySimpleObjectRasterizer.Solid) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                MyQuadD dd;
                localbox.Min;
                localbox.Max;
                MatrixD identity = MatrixD.Identity;
                identity.Forward = worldMatrix.Forward;
                identity.Up = worldMatrix.Up;
                identity.Right = worldMatrix.Right;
                float width = ((float) (localbox.Max.X - localbox.Min.X)) / 2f;
                float height = ((float) (localbox.Max.Y - localbox.Min.Y)) / 2f;
                float num3 = ((float) (localbox.Max.Z - localbox.Min.Z)) / 2f;
                Vector3D normal = Vector3D.TransformNormal(Vector3.Forward, identity) * num3;
                Vector3D center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                bool flag = FaceVisibleRelative(center, normal);
                Vector3D vectord1 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[0].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    temporary_faces[0].Front = new bool?(flag);
                    temporary_faces[0].Quad = dd;
                    temporary_faces[0].Pos = center;
                    temporary_faces[0].Col = faceZ_N;
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                flag = FaceVisibleRelative(center, -normal);
                Vector3D vectord3 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[1].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                    temporary_faces[1].Front = new bool?(flag);
                    temporary_faces[1].Quad = dd;
                    temporary_faces[1].Pos = center;
                    temporary_faces[1].Col = faceZ_P;
                }
                MatrixD matrix = MatrixD.CreateRotationY((double) MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3.TransformNormal(Vector3.Left, worldMatrix) * width;
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                flag = FaceVisibleRelative(center, normal);
                Vector3D vectord4 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[2].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    temporary_faces[2].Front = new bool?(flag);
                    temporary_faces[2].Quad = dd;
                    temporary_faces[2].Pos = center;
                    temporary_faces[2].Col = faceX_N;
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                flag = FaceVisibleRelative(center, -normal);
                Vector3D vectord5 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[3].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                    temporary_faces[3].Front = new bool?(flag);
                    temporary_faces[3].Quad = dd;
                    temporary_faces[3].Pos = center;
                    temporary_faces[3].Col = faceX_P;
                }
                matrix = Matrix.CreateRotationX(MathHelper.ToRadians((float) 90f)) * worldMatrix;
                normal = Vector3.TransformNormal(Vector3.Up, worldMatrix) * ((localbox.Max.Y - localbox.Min.Y) / 2.0);
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) + normal;
                flag = FaceVisibleRelative(center, normal);
                Vector3D vectord6 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[4].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    temporary_faces[4].Front = new bool?(flag);
                    temporary_faces[4].Quad = dd;
                    temporary_faces[4].Pos = center;
                    temporary_faces[4].Col = faceY_P;
                }
                center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, identity)) - normal;
                flag = FaceVisibleRelative(center, -normal);
                if (!(!onlyFrontFaces | flag))
                {
                    temporary_faces[5].Front = null;
                }
                else
                {
                    MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                    temporary_faces[5].Front = new bool?(flag);
                    temporary_faces[5].Quad = dd;
                    temporary_faces[5].Pos = center;
                    temporary_faces[5].Col = faceY_N;
                }
                int index = 0;
                while (true)
                {
                    if (index >= 6)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            if ((temporary_faces[i].Front != null) && temporary_faces[i].Front.Value)
                            {
                                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref temporary_faces[i].Quad, (Vector4) temporary_faces[i].Col, ref temporary_faces[i].Pos, customViewProjection, blendType, persistentBillboards);
                            }
                        }
                        break;
                    }
                    if ((temporary_faces[index].Front != null) && !temporary_faces[index].Front.Value)
                    {
                        MyTransparentGeometry.AddQuad(faceMaterial.Value, ref temporary_faces[index].Quad, (Vector4) temporary_faces[index].Col, ref temporary_faces[index].Pos, customViewProjection, blendType, persistentBillboards);
                    }
                    index++;
                }
            }
            if ((rasterization == MySimpleObjectRasterizer.Wireframe) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
            {
                Color color = wire * 1.3f;
                DrawWireFramedBox(ref worldMatrix, ref localbox, ref color, lineWidth, wireDivideRatio, lineMaterial, onlyFrontFaces, customViewProjection, blendType, intensity, persistentBillboards);
            }
        }

        public static unsafe void DrawTransparentCapsule(ref MatrixD worldMatrix, float radius, float height, ref Color color, int wireDivideRatio, MyStringId? faceMaterial = new MyStringId?(), int customViewProjectionMatrix = -1, MyBillboard.BlendTypeEnum blendType = 0)
        {
            float num;
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_000C;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_000C:
            num = height * 0.5f;
            Vector3D translation = worldMatrix.Translation;
            MatrixD xd = MatrixD.CreateRotationX(-1.570796012878418);
            xd.Translation = new Vector3D(0.0, (double) num, 0.0);
            xd *= worldMatrix;
            m_verticesBuffer.Clear();
            MyMeshHelper.GenerateSphere(ref xd, radius, wireDivideRatio, m_verticesBuffer);
            Vector3D zero = Vector3D.Zero;
            int num2 = m_verticesBuffer.Count / 2;
            for (int i = 0; i < num2; i += 4)
            {
                MyQuadD dd;
                dd.Point0 = m_verticesBuffer[i + 1];
                dd.Point1 = m_verticesBuffer[i + 3];
                dd.Point2 = m_verticesBuffer[i + 2];
                dd.Point3 = m_verticesBuffer[i];
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref zero, customViewProjectionMatrix, blendType, null);
            }
            MatrixD xd2 = MatrixD.CreateRotationX(-1.570796012878418);
            xd2.Translation = new Vector3D(0.0, (double) -num, 0.0);
            xd2 *= worldMatrix;
            m_verticesBuffer.Clear();
            MyMeshHelper.GenerateSphere(ref xd2, radius, wireDivideRatio, m_verticesBuffer);
            for (int j = num2; j < m_verticesBuffer.Count; j += 4)
            {
                MyQuadD dd2;
                dd2.Point0 = m_verticesBuffer[j + 1];
                dd2.Point1 = m_verticesBuffer[j + 3];
                dd2.Point2 = m_verticesBuffer[j + 2];
                dd2.Point3 = m_verticesBuffer[j];
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd2, (Vector4) color, ref zero, customViewProjectionMatrix, blendType, null);
            }
            float num3 = 6.283185f / ((float) wireDivideRatio);
            float num4 = 0f;
            for (int k = 0; k < wireDivideRatio; k++)
            {
                MyQuadD dd3;
                num4 = k * num3;
                float num8 = (float) (radius * Math.Cos((double) num4));
                float num9 = (float) (radius * Math.Sin((double) num4));
                dd3.Point0.X = num8;
                dd3.Point0.Z = num9;
                dd3.Point3.X = num8;
                dd3.Point3.Z = num9;
                num4 = (k + 1) * num3;
                num8 = (float) (radius * Math.Cos((double) num4));
                num9 = (float) (radius * Math.Sin((double) num4));
                dd3.Point1.X = num8;
                dd3.Point1.Z = num9;
                dd3.Point2.X = num8;
                dd3.Point2.Z = num9;
                dd3.Point0.Y = -num;
                dd3.Point1.Y = -num;
                dd3.Point2.Y = num;
                dd3.Point3.Y = num;
                MyQuadD* ddPtr1 = (MyQuadD*) ref dd3;
                ddPtr1->Point0 = Vector3D.Transform(dd3.Point0, (MatrixD) worldMatrix);
                MyQuadD* ddPtr2 = (MyQuadD*) ref dd3;
                ddPtr2->Point1 = Vector3D.Transform(dd3.Point1, (MatrixD) worldMatrix);
                MyQuadD* ddPtr3 = (MyQuadD*) ref dd3;
                ddPtr3->Point2 = Vector3D.Transform(dd3.Point2, (MatrixD) worldMatrix);
                MyQuadD* ddPtr4 = (MyQuadD*) ref dd3;
                ddPtr4->Point3 = Vector3D.Transform(dd3.Point3, (MatrixD) worldMatrix);
                Vector3D vctPos = (((dd3.Point0 + dd3.Point1) + dd3.Point2) + dd3.Point3) * 0.25;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd3, (Vector4) color, ref vctPos, customViewProjectionMatrix, blendType, null);
            }
        }

        public static void DrawTransparentCone(ref MatrixD worldMatrix, float radius, float height, ref Color color, int wireDivideRatio, MyStringId? faceMaterial = new MyStringId?(), int customViewProjectionMatrix = -1)
        {
            DrawTransparentCone(worldMatrix.Translation, (Vector3) (worldMatrix.Forward * height), (Vector3) (worldMatrix.Up * radius), color, wireDivideRatio, faceMaterial, customViewProjectionMatrix);
        }

        private static void DrawTransparentCone(Vector3D apexPosition, Vector3 directionVector, Vector3 baseVector, Color color, int wireDivideRatio, MyStringId? faceMaterial = new MyStringId?(), int customViewProjectionMatrix = -1)
        {
            if ((faceMaterial == null) || (faceMaterial.Value == MyStringId.NullOrEmpty))
            {
                faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
            }
            Vector3 axis = directionVector;
            axis.Normalize();
            Vector3D vectord = apexPosition;
            float num = (float) (6.2831853071795862 / ((double) wireDivideRatio));
            for (int i = 0; i < wireDivideRatio; i++)
            {
                MyQuadD dd;
                float angle = i * num;
                float num4 = (i + 1) * num;
                Vector3D vectord2 = (apexPosition + directionVector) + Vector3.Transform(baseVector, Matrix.CreateFromAxisAngle(axis, angle));
                Vector3D vectord3 = (apexPosition + directionVector) + Vector3.Transform(baseVector, Matrix.CreateFromAxisAngle(axis, num4));
                dd.Point0 = vectord2;
                dd.Point1 = vectord3;
                dd.Point2 = vectord;
                dd.Point3 = vectord;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref Vector3D.Zero, -1, MyBillboard.BlendTypeEnum.Standard, null);
            }
        }

        public static void DrawTransparentCuboid(ref MatrixD worldMatrix, MyCuboid cuboid, ref Vector4 vctColor, bool bWireFramed, float thickness, MyStringId? lineMaterial = new MyStringId?())
        {
            foreach (Line local1 in cuboid.UniqueLines)
            {
                Vector3D start = Vector3D.Transform(local1.From, worldMatrix);
                Vector3D end = Vector3D.Transform(local1.To, worldMatrix);
                MyStringId? nullable = lineMaterial;
                DrawLine(start, end, new MyStringId?((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE), ref vctColor, thickness, MyBillboard.BlendTypeEnum.Standard);
            }
        }

        public static void DrawTransparentCylinder(ref MatrixD worldMatrix, float radius1, float radius2, float length, ref Vector4 vctColor, bool bWireFramed, int wireDivideRatio, float thickness, MyStringId? lineMaterial = new MyStringId?())
        {
            Vector3D zero = Vector3.Zero;
            Vector3D position = Vector3.Zero;
            Vector3D start = Vector3.Zero;
            Vector3D vectord4 = Vector3.Zero;
            float num = 360f / ((float) wireDivideRatio);
            float degrees = 0f;
            for (int i = 0; i <= wireDivideRatio; i++)
            {
                degrees = i * num;
                zero.X = (float) (radius1 * Math.Cos((double) MathHelper.ToRadians(degrees)));
                zero.Y = length / 2f;
                zero.Z = (float) (radius1 * Math.Sin((double) MathHelper.ToRadians(degrees)));
                position.X = (float) (radius2 * Math.Cos((double) MathHelper.ToRadians(degrees)));
                position.Y = -length / 2f;
                position.Z = (float) (radius2 * Math.Sin((double) MathHelper.ToRadians(degrees)));
                zero = Vector3D.Transform(zero, (MatrixD) worldMatrix);
                position = Vector3D.Transform(position, (MatrixD) worldMatrix);
                MyStringId? nullable = lineMaterial;
                DrawLine(position, zero, new MyStringId?((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE), ref vctColor, thickness, MyBillboard.BlendTypeEnum.Standard);
                if (i > 0)
                {
                    nullable = lineMaterial;
                    DrawLine(vectord4, position, new MyStringId?((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE), ref vctColor, thickness, MyBillboard.BlendTypeEnum.Standard);
                    nullable = lineMaterial;
                    DrawLine(start, zero, new MyStringId?((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE), ref vctColor, thickness, MyBillboard.BlendTypeEnum.Standard);
                }
                vectord4 = position;
                start = zero;
            }
        }

        public static void DrawTransparentPyramid(ref Vector3D start, ref MyQuad backQuad, ref Vector4 vctColor, int divideRatio, float thickness, MyStringId? lineMaterial = new MyStringId?())
        {
            m_lineBuffer.Clear();
            GenerateLines(start, backQuad.Point0, backQuad.Point1, ref m_lineBuffer, divideRatio);
            GenerateLines(start, backQuad.Point1, backQuad.Point2, ref m_lineBuffer, divideRatio);
            GenerateLines(start, backQuad.Point2, backQuad.Point3, ref m_lineBuffer, divideRatio);
            GenerateLines(start, backQuad.Point3, backQuad.Point0, ref m_lineBuffer, divideRatio);
            foreach (LineD ed in m_lineBuffer)
            {
                Vector3 vec = (Vector3) (ed.To - ed.From);
                float length = vec.Length();
                if (length > 0.1f)
                {
                    vec = MyUtils.Normalize(vec);
                    MyStringId? nullable = lineMaterial;
                    MyTransparentGeometry.AddLineBillboard((nullable != null) ? nullable.GetValueOrDefault() : ID_GIZMO_DRAW_LINE, vctColor, ed.From, vec, length, thickness, MyBillboard.BlendTypeEnum.Standard, -1, 1f, null);
                }
            }
        }

        public static unsafe void DrawTransparentRamp(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, MyStringId? faceMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0)
        {
            MyQuadD dd;
            MatrixD xd;
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_0013;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_0013:
            xd = MatrixD.Identity;
            xd.Forward = worldMatrix.Forward;
            xd.Up = worldMatrix.Up;
            xd.Right = worldMatrix.Right;
            float width = ((float) (localbox.Max.X - localbox.Min.X)) / 2f;
            float height = ((float) (localbox.Max.Y - localbox.Min.Y)) / 2f;
            float num3 = ((float) (localbox.Max.Z - localbox.Min.Z)) / 2f;
            Vector3D normal = Vector3D.TransformNormal(Vector3D.Forward, xd) * num3;
            Vector3D center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd)) - normal;
            Vector3D vectord1 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd);
            if (!onlyFrontFaces || FaceVisible(center, -normal))
            {
                MyUtils.GenerateQuad(out dd, ref center, width, height, ref worldMatrix);
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, null);
            }
            MatrixD matrix = MatrixD.CreateRotationY((double) MathHelper.ToRadians((float) 90f)) * worldMatrix;
            normal = Vector3.TransformNormal(Vector3.Left, worldMatrix) * width;
            center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd)) + normal;
            Vector3D vectord5 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd);
            if (!onlyFrontFaces || FaceVisible(center, normal))
            {
                MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                MyQuadD* ddPtr1 = (MyQuadD*) ref dd;
                ddPtr1->Point3 = dd.Point0;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, null);
            }
            center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd)) - normal;
            Vector3D vectord6 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd);
            if (!onlyFrontFaces || FaceVisible(center, -normal))
            {
                MyUtils.GenerateQuad(out dd, ref center, num3, height, ref matrix);
                MyQuadD* ddPtr2 = (MyQuadD*) ref dd;
                ddPtr2->Point3 = dd.Point0;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, null);
            }
            Vector3D one = Vector3D.One;
            Vector3D vectord4 = Vector3D.One;
            matrix = Matrix.CreateRotationX(MathHelper.ToRadians((float) 90f)) * worldMatrix;
            normal = Vector3.TransformNormal(Vector3.Up, worldMatrix) * ((localbox.Max.Y - localbox.Min.Y) / 2.0);
            center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd)) - normal;
            Vector3D vectord7 = worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd);
            if (!onlyFrontFaces || FaceVisible(center, -normal))
            {
                MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                one = dd.Point1;
                vectord4 = dd.Point2;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, null);
            }
            center = (worldMatrix.Translation + Vector3D.Transform(localbox.Center, xd)) + normal;
            if (!onlyFrontFaces || FaceVisible(center, normal))
            {
                MyUtils.GenerateQuad(out dd, ref center, width, num3, ref matrix);
                dd.Point1 = one;
                dd.Point2 = vectord4;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref center, customViewProjection, blendType, null);
            }
        }

        public static unsafe void DrawTransparentRoundedCorner(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, MyStringId? faceMaterial = new MyStringId?(), int customViewProjection = -1)
        {
            MyQuadD dd;
            if (faceMaterial != null)
            {
                MyStringId? nullable = faceMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_0004;
                }
            }
            faceMaterial = new MyStringId?(ID_CONTAINER_BORDER);
        TR_0004:
            dd.Point0 = localbox.Min;
            dd.Point0.Z = localbox.Max.Z;
            dd.Point1 = localbox.Max;
            dd.Point1.Y = localbox.Min.Y;
            dd.Point2 = localbox.Max;
            dd.Point3 = localbox.Max;
            dd.Point3.X = localbox.Min.X;
            MyQuadD* ddPtr1 = (MyQuadD*) ref dd;
            ddPtr1->Point0 = Vector3D.Transform(dd.Point0, (MatrixD) worldMatrix);
            MyQuadD* ddPtr2 = (MyQuadD*) ref dd;
            ddPtr2->Point1 = Vector3D.Transform(dd.Point1, (MatrixD) worldMatrix);
            MyQuadD* ddPtr3 = (MyQuadD*) ref dd;
            ddPtr3->Point2 = Vector3D.Transform(dd.Point2, (MatrixD) worldMatrix);
            MyQuadD* ddPtr4 = (MyQuadD*) ref dd;
            ddPtr4->Point3 = Vector3D.Transform(dd.Point3, (MatrixD) worldMatrix);
            Vector3D vctPos = (((dd.Point0 + dd.Point1) + dd.Point2) + dd.Point3) * 0.25;
            MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref vctPos, customViewProjection, MyBillboard.BlendTypeEnum.Standard, null);
            dd.Point0 = localbox.Min;
            dd.Point0.X = localbox.Max.X;
            dd.Point1 = localbox.Max;
            dd.Point1.Z = localbox.Min.Z;
            dd.Point2 = localbox.Max;
            dd.Point3 = localbox.Max;
            dd.Point3.Y = localbox.Min.Y;
            MyQuadD* ddPtr5 = (MyQuadD*) ref dd;
            ddPtr5->Point0 = Vector3D.Transform(dd.Point0, (MatrixD) worldMatrix);
            MyQuadD* ddPtr6 = (MyQuadD*) ref dd;
            ddPtr6->Point1 = Vector3D.Transform(dd.Point1, (MatrixD) worldMatrix);
            MyQuadD* ddPtr7 = (MyQuadD*) ref dd;
            ddPtr7->Point2 = Vector3D.Transform(dd.Point2, (MatrixD) worldMatrix);
            MyQuadD* ddPtr8 = (MyQuadD*) ref dd;
            ddPtr8->Point3 = Vector3D.Transform(dd.Point3, (MatrixD) worldMatrix);
            vctPos = (((dd.Point0 + dd.Point1) + dd.Point2) + dd.Point3) * 0.25;
            MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref vctPos, customViewProjection, MyBillboard.BlendTypeEnum.Standard, null);
            float num = 0.1570796f;
            float num2 = 0f;
            float num3 = (float) (localbox.Max.X - localbox.Min.X);
            float num4 = num3 * 0.5f;
            Vector3D vectord2 = (dd.Point2 + dd.Point3) * 0.5;
            Vector3D translation = worldMatrix.Translation;
            worldMatrix.Translation = vectord2;
            for (int i = 20; i < 30; i++)
            {
                num2 = i * num;
                float num6 = (float) (num3 * Math.Cos((double) num2));
                float num7 = (float) (num3 * Math.Sin((double) num2));
                dd.Point0.X = num6;
                dd.Point0.Z = num7;
                dd.Point3.X = num6;
                dd.Point3.Z = num7;
                num2 = (i + 1) * num;
                num6 = (float) (num3 * Math.Cos((double) num2));
                num7 = (float) (num3 * Math.Sin((double) num2));
                dd.Point1.X = num6;
                dd.Point1.Z = num7;
                dd.Point2.X = num6;
                dd.Point2.Z = num7;
                dd.Point0.Y = -num4;
                dd.Point1.Y = -num4;
                dd.Point2.Y = num4;
                dd.Point3.Y = num4;
                MyQuadD* ddPtr9 = (MyQuadD*) ref dd;
                ddPtr9->Point0 = Vector3D.Transform(dd.Point0, (MatrixD) worldMatrix);
                MyQuadD* ddPtr10 = (MyQuadD*) ref dd;
                ddPtr10->Point1 = Vector3D.Transform(dd.Point1, (MatrixD) worldMatrix);
                MyQuadD* ddPtr11 = (MyQuadD*) ref dd;
                ddPtr11->Point2 = Vector3D.Transform(dd.Point2, (MatrixD) worldMatrix);
                MyQuadD* ddPtr12 = (MyQuadD*) ref dd;
                ddPtr12->Point3 = Vector3D.Transform(dd.Point3, (MatrixD) worldMatrix);
                vctPos = (((dd.Point0 + dd.Point1) + dd.Point2) + dd.Point3) * 0.25;
                MyTransparentGeometry.AddQuad(faceMaterial.Value, ref dd, (Vector4) color, ref vctPos, customViewProjection, MyBillboard.BlendTypeEnum.Standard, null);
            }
            worldMatrix.Translation = translation;
        }

        public static void DrawTransparentSphere(List<Vector3D> verticesBuffer, float radius, ref Color color, MySimpleObjectRasterizer rasterization, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), float lineThickness = -1f, int customViewProjectionMatrix = -1, List<MyBillboard> persistentBillboards = null, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f)
        {
            Vector3D zero = Vector3D.Zero;
            float thickness = radius * 0.01f;
            if (lineThickness > -1f)
            {
                thickness = lineThickness;
            }
            int num2 = 0;
            for (num2 = 0; num2 < verticesBuffer.Count; num2 += 4)
            {
                MyQuadD dd;
                dd.Point0 = verticesBuffer[num2 + 1];
                dd.Point1 = verticesBuffer[num2 + 3];
                dd.Point2 = verticesBuffer[num2 + 2];
                dd.Point3 = verticesBuffer[num2];
                if ((rasterization == MySimpleObjectRasterizer.Solid) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
                {
                    MyStringId? nullable = faceMaterial;
                    List<MyBillboard> list = persistentBillboards;
                    MyTransparentGeometry.AddQuad((nullable != null) ? nullable.GetValueOrDefault() : ID_CONTAINER_BORDER, ref dd, (Vector4) color, ref zero, customViewProjectionMatrix, blendType, list);
                }
                if ((rasterization == MySimpleObjectRasterizer.Wireframe) || (rasterization == MySimpleObjectRasterizer.SolidAndWireframe))
                {
                    Vector3D origin = dd.Point0;
                    Vector3 vec = (Vector3) (dd.Point1 - origin);
                    float length = vec.Length();
                    if (length > 0.1f)
                    {
                        MyTransparentGeometry.AddLineBillboard(lineMaterial.Value, (Vector4) color, origin, MyUtils.Normalize(vec), length, thickness, blendType, customViewProjectionMatrix, intensity, persistentBillboards);
                    }
                    origin = dd.Point1;
                    vec = (Vector3) (dd.Point2 - origin);
                    length = vec.Length();
                    if (length > 0.1f)
                    {
                        MyTransparentGeometry.AddLineBillboard(lineMaterial.Value, (Vector4) color, origin, MyUtils.Normalize(vec), length, thickness, blendType, customViewProjectionMatrix, intensity, persistentBillboards);
                    }
                }
            }
        }

        public static void DrawTransparentSphere(ref MatrixD worldMatrix, float radius, ref Color color, MySimpleObjectRasterizer rasterization, int wireDivideRatio, MyStringId? faceMaterial = new MyStringId?(), MyStringId? lineMaterial = new MyStringId?(), float lineThickness = -1f, int customViewProjectionMatrix = -1, List<MyBillboard> persistentBillboards = null, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f)
        {
            if (lineMaterial != null)
            {
                MyStringId? nullable = lineMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_0000;
                }
            }
            lineMaterial = new MyStringId?(MyTransparentMaterials.ErrorMaterial.Id);
        TR_0000:
            m_verticesBuffer.Clear();
            MyMeshHelper.GenerateSphere(ref worldMatrix, radius, wireDivideRatio, m_verticesBuffer);
            DrawTransparentSphere(m_verticesBuffer, radius, ref color, rasterization, faceMaterial, lineMaterial, lineThickness, customViewProjectionMatrix, persistentBillboards, blendType, intensity);
        }

        private static void DrawWireFramedBox(ref MatrixD worldMatrix, ref BoundingBoxD localbox, ref Color color, float fThickRatio, Vector3I wireDivideRatio, MyStringId? lineMaterial = new MyStringId?(), bool onlyFrontFaces = false, int customViewProjection = -1, MyBillboard.BlendTypeEnum blendType = 0, float intensity = 1f, List<MyBillboard> persistentBillboards = null)
        {
            if (lineMaterial != null)
            {
                MyStringId? nullable = lineMaterial;
                MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
                if (!((nullable != null) ? ((nullable != null) ? (nullable.GetValueOrDefault() == nullOrEmpty) : true) : false))
                {
                    goto TR_0035;
                }
            }
            lineMaterial = new MyStringId?(MyTransparentMaterials.ErrorMaterial.Id);
        TR_0035:
            m_lineBuffer.Clear();
            MatrixD identity = MatrixD.Identity;
            identity.Forward = worldMatrix.Forward;
            identity.Up = worldMatrix.Up;
            identity.Right = worldMatrix.Right;
            Vector3D.Dot(identity.Forward, MyTransparentGeometry.Camera.Forward);
            Vector3D.Dot(identity.Right, MyTransparentGeometry.Camera.Forward);
            Vector3D.Dot(identity.Up, MyTransparentGeometry.Camera.Forward);
            Vector3D forward = identity.Forward;
            Vector3D right = identity.Right;
            Vector3D up = identity.Up;
            float x = (float) localbox.Size.X;
            float y = (float) localbox.Size.Y;
            float z = (float) localbox.Size.Z;
            Vector3D center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (forward * (z * 0.5f));
            Vector3D vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (forward * (z * 0.5f));
            Vector3D min = localbox.Min;
            Vector3D vctEnd = min + (Vector3.Up * y);
            Vector3D vctSideStep = Vector3.Right * (x / ((float) wireDivideRatio.X));
            Vector3D vectord1 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix);
            if (!onlyFrontFaces || FaceVisible(center, forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min += Vector3.Backward * z;
            vctEnd = min + (Vector3.Up * y);
            if (!onlyFrontFaces || FaceVisible(vectord5, -forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Right * x);
            vctSideStep = Vector3.Up * (y / ((float) wireDivideRatio.Y));
            if (!onlyFrontFaces || FaceVisible(center, forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min += Vector3.Backward * z;
            vctEnd += Vector3.Backward * z;
            if (!onlyFrontFaces || FaceVisible(vectord5, -forward))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            Matrix.CreateRotationY(MathHelper.ToRadians((float) 90f)) * worldMatrix;
            center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (right * (x * 0.5f));
            vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (right * (x * 0.5f));
            min = localbox.Min;
            vctEnd = min + (Vector3.Backward * z);
            vctSideStep = Vector3.Up * (y / ((float) wireDivideRatio.Y));
            Vector3D vectord9 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix);
            if (!onlyFrontFaces || FaceVisible(center, -right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min = localbox.Min + (Vector3.Right * x);
            vctEnd = min + (Vector3.Backward * z);
            if (!onlyFrontFaces || FaceVisible(vectord5, right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Y);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Up * y);
            vctSideStep = Vector3.Backward * (z / ((float) wireDivideRatio.Z));
            if (!onlyFrontFaces || FaceVisible(center, -right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min += Vector3.Right * x;
            vctEnd += Vector3.Right * x;
            if (!onlyFrontFaces || FaceVisible(vectord5, right))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            center = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) - (up * (y * 0.5f));
            vectord5 = Vector3D.Transform(localbox.Center, (MatrixD) worldMatrix) + (up * (y * 0.5f));
            min = localbox.Min;
            vctEnd = min + (Vector3.Right * x);
            vctSideStep = Vector3.Backward * (z / ((float) wireDivideRatio.Z));
            if (!onlyFrontFaces || FaceVisible(center, -up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min += Vector3.Up * y;
            vctEnd += Vector3.Up * y;
            if (!onlyFrontFaces || FaceVisible(vectord5, up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.Z);
            }
            min = localbox.Min;
            vctEnd = min + (Vector3.Backward * z);
            vctSideStep = Vector3.Right * (x / ((float) wireDivideRatio.X));
            if (!onlyFrontFaces || FaceVisible(center, -up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            min += Vector3.Up * y;
            vctEnd += Vector3.Up * y;
            if (!onlyFrontFaces || FaceVisible(vectord5, up))
            {
                GenerateLines(min, vctEnd, ref vctSideStep, ref worldMatrix, ref m_lineBuffer, wireDivideRatio.X);
            }
            Vector3 vector = new Vector3(localbox.Max.X - localbox.Min.X, localbox.Max.Y - localbox.Min.Y, localbox.Max.Z - localbox.Min.Z);
            float thickness = MathHelper.Max(1f, MathHelper.Min(MathHelper.Min(vector.X, vector.Y), vector.Z)) * fThickRatio;
            foreach (LineD ed in m_lineBuffer)
            {
                MyTransparentGeometry.AddLineBillboard(lineMaterial.Value, (Vector4) color, ed.From, (Vector3) ed.Direction, (float) ed.Length, thickness, blendType, customViewProjection, intensity, persistentBillboards);
            }
        }

        public static bool FaceVisible(Vector3D center, Vector3D normal) => 
            (Vector3D.Dot(Vector3D.Normalize(center - MyTransparentGeometry.Camera.Translation), normal) < 0.0);

        public static bool FaceVisibleRelative(Vector3D center, Vector3D normal) => 
            (Vector3D.Dot(Vector3D.Normalize(center), normal) < 0.0);

        private static void GenerateLines(Vector3D start, Vector3D end1, Vector3D end2, ref List<LineD> lineBuffer, int divideRatio)
        {
            Vector3D vectord = (end2 - end1) / ((double) divideRatio);
            for (int i = 0; i < divideRatio; i++)
            {
                LineD item = new LineD(start, end1 + (i * vectord));
                lineBuffer.Add(item);
            }
        }

        private static void GenerateLines(Vector3D vctStart, Vector3D vctEnd, ref Vector3D vctSideStep, ref MatrixD worldMatrix, ref List<LineD> lineBuffer, int divideRatio)
        {
            for (int i = 0; i <= divideRatio; i++)
            {
                Vector3D from = Vector3D.Transform(vctStart, (MatrixD) worldMatrix);
                Vector3D to = Vector3D.Transform(vctEnd, (MatrixD) worldMatrix);
                if (lineBuffer.Count < lineBuffer.Capacity)
                {
                    LineD item = new LineD(from, to);
                    lineBuffer.Add(item);
                    vctStart += vctSideStep;
                    vctEnd += vctSideStep;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FaceInfo
        {
            public bool? Front;
            public MyQuadD Quad;
            public Vector3D Pos;
            public Color Col;
        }
    }
}

