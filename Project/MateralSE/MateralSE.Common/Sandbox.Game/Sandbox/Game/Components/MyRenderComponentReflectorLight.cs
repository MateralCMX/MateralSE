namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyRenderComponentReflectorLight : MyRenderComponentLight
    {
        private const float RADIUS_TO_CONE_MULTIPLIER = 0.25f;
        private const float SMALL_LENGTH_MULTIPLIER = 0.5f;
        private MyReflectorLight m_reflectorLight;

        public override unsafe void AddRenderObjects()
        {
            BoundingBox* boxPtr1;
            base.AddRenderObjects();
            BoundingBox localAABB = this.m_reflectorLight.PositionComp.LocalAABB;
            boxPtr1.Inflate(this.m_reflectorLight.IsLargeLight ? 3f : 1f);
            float num = this.m_reflectorLight.ReflectorRadiusBounds.Max * 0.25f;
            if (!this.m_reflectorLight.IsLargeLight)
            {
                num *= 0.5f;
            }
            boxPtr1 = (BoundingBox*) ref localAABB;
            MatrixD? worldMatrix = null;
            Matrix? localMatrix = null;
            MyRenderProxy.UpdateRenderObject(base.m_renderObjectIDs[0], worldMatrix, new BoundingBox?(localAABB.Include(new Vector3(0f, 0f, -num))), -1, localMatrix);
        }

        public override void Draw()
        {
            base.Draw();
            if (this.m_reflectorLight.IsReflectorEnabled)
            {
                this.DrawReflectorCone();
            }
        }

        private void DrawReflectorCone()
        {
            if (!((MySession.Static.CameraController is MyCockpit) ? ((MyCockpit) MySession.Static.CameraController).IsInFirstPersonView : false) && !string.IsNullOrEmpty(this.m_reflectorLight.ReflectorConeMaterial))
            {
                Matrix matrix;
                MatrixD worldMatrix = this.m_reflectorLight.WorldMatrix;
                float num2 = MathHelper.Saturate((float) (1f - ((float) Math.Pow((double) Math.Abs(Vector3.Dot(Vector3.Normalize(MySector.MainCamera.Position - worldMatrix.Translation), (Vector3) worldMatrix.Forward)), 30.0))));
                this.m_reflectorLight.GetLocalMatrix(out matrix);
                uint renderObjectID = this.m_reflectorLight.CubeGrid.Render.GetRenderObjectID();
                Vector3D translation = matrix.Translation;
                Vector3D forward = matrix.Forward;
                float length = Math.Max((float) 15f, (float) (this.m_reflectorLight.ReflectorRadius * 0.25f));
                if (!this.m_reflectorLight.IsLargeLight)
                {
                    length *= 0.5f;
                }
                Color color = this.m_reflectorLight.Color;
                float n = (this.m_reflectorLight.CurrentLightPower * this.m_reflectorLight.Intensity) * 0.8f;
                translation += (forward * this.m_reflectorLight.CubeGrid.GridSize) * 0.5;
                MyTransparentGeometry.AddLocalLineBillboard(MyStringId.GetOrCompute(this.m_reflectorLight.ReflectorConeMaterial), (color.ToVector4() * num2) * MathHelper.Saturate(n), translation, renderObjectID, (Vector3) forward, length, this.m_reflectorLight.IsLargeLight ? 11f : 2.5f, MyBillboard.BlendTypeEnum.AdditiveBottom, -1, 1f, null);
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.m_reflectorLight = base.Container.Entity as MyReflectorLight;
        }
    }
}

