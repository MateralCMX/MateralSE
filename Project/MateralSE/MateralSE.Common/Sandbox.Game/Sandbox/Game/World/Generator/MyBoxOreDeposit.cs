namespace Sandbox.Game.World.Generator
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Voxels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRageMath;

    internal class MyBoxOreDeposit : MyCompositeShapeOreDeposit
    {
        private MyCsgBox m_boxShape;

        public MyBoxOreDeposit(MyCsgShapeBase baseShape, MyVoxelMaterialDefinition material) : base(baseShape, material)
        {
            this.m_boxShape = (MyCsgBox) baseShape;
        }

        public override void DebugDraw(ref MatrixD translation, Color materialColor)
        {
        }

        public override MyVoxelMaterialDefinition GetMaterialForPosition(ref Vector3 pos, float lodSize)
        {
            List<MyVoxelMaterialDefinition> list = MyDefinitionManager.Static.GetVoxelMaterialDefinitions().ToList<MyVoxelMaterialDefinition>();
            float num = 2f * this.m_boxShape.HalfExtents;
            return list[(int) (MathHelper.Clamp((float) (((pos - this.m_boxShape.Center()) + this.m_boxShape.HalfExtents).X / num), (float) 0f, (float) 1f) * (list.Count - 1))];
        }
    }
}

