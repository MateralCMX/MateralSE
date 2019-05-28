namespace Sandbox.Game.World.Generator
{
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using System;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyCompositeShapeOreDeposit
    {
        public readonly MyCsgShapeBase Shape;
        protected readonly MyVoxelMaterialDefinition m_material;

        public MyCompositeShapeOreDeposit(MyCsgShapeBase shape, MyVoxelMaterialDefinition material)
        {
            this.Shape = shape;
            this.m_material = material;
        }

        public virtual void DebugDraw(ref MatrixD translation, Color materialColor)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_ASTEROID_ORES)
            {
                this.Shape.DebugDraw(ref translation, materialColor);
                MyRenderProxy.DebugDrawText3D((Matrix.CreateTranslation(this.Shape.Center()) * translation).Translation, this.m_material.Id.SubtypeName, Color.White, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        public virtual MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize) => 
            this.m_material;
    }
}

