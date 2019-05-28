namespace Sandbox.Game.Components
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.ModAPI;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyDebugRenderComponent : MyDebugRenderComponentBase
    {
        protected MyEntity Entity;

        public MyDebugRenderComponent(IMyEntity entity)
        {
            this.Entity = (MyEntity) entity;
        }

        public override void DebugDraw()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES)
            {
                this.DebugDrawDummies(this.Entity.Render.GetModel());
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_ENTITY_IDS && ((this.Entity.Parent == null) || !MyDebugDrawSettings.DEBUG_DRAW_ENTITY_IDS_ONLY_ROOT))
            {
                MyRenderProxy.DebugDrawText3D(this.Entity.PositionComp.WorldMatrix.Translation, this.Entity.EntityId.ToString("X16"), Color.White, 0.6f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
            if (MyDebugDrawSettings.DEBUG_DRAW_PHYSICS && (this.Entity.Physics != null))
            {
                this.Entity.Physics.DebugDraw();
            }
        }

        protected void DebugDrawDummies(MyModel model)
        {
            if (model != null)
            {
                float num = 0f;
                Vector3D zero = Vector3D.Zero;
                if (MySector.MainCamera != null)
                {
                    num = MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES_DISTANCE * MyDebugDrawSettings.DEBUG_DRAW_MODEL_DUMMIES_DISTANCE;
                    zero = MySector.MainCamera.WorldMatrix.Translation;
                }
                foreach (KeyValuePair<string, MyModelDummy> pair in model.Dummies)
                {
                    MatrixD matrix = pair.Value.Matrix * this.Entity.PositionComp.WorldMatrix;
                    if ((num == 0f) || (Vector3D.DistanceSquared(zero, matrix.Translation) <= num))
                    {
                        MyRenderProxy.DebugDrawText3D(matrix.Translation, pair.Key, Color.White, 0.7f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                        MyRenderProxy.DebugDrawAxis(MatrixD.Normalize(matrix), 0.1f, false, false, false);
                        MyRenderProxy.DebugDrawOBB(matrix, Vector3.One, 0.1f, false, false, true, false);
                    }
                }
            }
        }

        public override void DebugDrawInvalidTriangles()
        {
            if (this.Entity != null)
            {
                using (List<MyHierarchyComponentBase>.Enumerator enumerator = this.Entity.Hierarchy.Children.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Container.Entity.DebugDrawInvalidTriangles();
                    }
                }
                if (this.Entity.Render.GetModel() != null)
                {
                    int trianglesCount = this.Entity.Render.GetModel().GetTrianglesCount();
                    for (int i = 0; i < trianglesCount; i++)
                    {
                        MyTriangleVertexIndices triangle = this.Entity.Render.GetModel().GetTriangle(i);
                        if (MyUtils.IsWrongTriangle(this.Entity.Render.GetModel().GetVertex(triangle.I0), this.Entity.Render.GetModel().GetVertex(triangle.I1), this.Entity.Render.GetModel().GetVertex(triangle.I2)))
                        {
                            Vector3 pointFrom = (Vector3) Vector3.Transform(this.Entity.Render.GetModel().GetVertex(triangle.I0), this.Entity.PositionComp.WorldMatrix);
                            Vector3 pointTo = (Vector3) Vector3.Transform(this.Entity.Render.GetModel().GetVertex(triangle.I1), this.Entity.PositionComp.WorldMatrix);
                            Vector3 vector3 = (Vector3) Vector3.Transform(this.Entity.Render.GetModel().GetVertex(triangle.I2), this.Entity.PositionComp.WorldMatrix);
                            MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, Color.Purple, Color.Purple, false, false);
                            MyRenderProxy.DebugDrawLine3D(pointTo, vector3, Color.Purple, Color.Purple, false, false);
                            MyRenderProxy.DebugDrawLine3D(vector3, pointFrom, Color.Purple, Color.Purple, false, false);
                            Vector3 vector4 = ((pointFrom + pointTo) + vector3) / 3f;
                            MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitX, Color.Yellow, Color.Yellow, false, false);
                            MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitY, Color.Yellow, Color.Yellow, false, false);
                            MyRenderProxy.DebugDrawLine3D(vector4, vector4 + Vector3.UnitZ, Color.Yellow, Color.Yellow, false, false);
                        }
                    }
                }
            }
        }
    }
}

