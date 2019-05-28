namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Components;
    using VRage.Game.Models;
    using VRageMath;

    public static class MyRenderComponentExtensions
    {
        public static unsafe void CalculateBlockDepthBias(this MyRenderComponent renderComponent, MyCubeBlock block)
        {
            if (block.Hierarchy != null)
            {
                MyCompoundCubeBlock entity = block.Hierarchy.Parent.Entity as MyCompoundCubeBlock;
                if (entity != null)
                {
                    bool* flagPtr = (bool*) stackalloc byte[0x40];
                    foreach (MySlimBlock block3 in entity.GetBlocks())
                    {
                        if (block3.FatBlock == null)
                        {
                            continue;
                        }
                        if (!ReferenceEquals(block3.FatBlock, block))
                        {
                            MyRenderComponentBase render = block3.FatBlock.Render;
                            if (render != null)
                            {
                                *((sbyte*) (flagPtr + render.DepthBias)) = 1;
                            }
                        }
                    }
                    int num = 0;
                    MyModel modelStorage = renderComponent.ModelStorage as MyModel;
                    if (modelStorage != null)
                    {
                        Vector3 center = modelStorage.BoundingSphere.Center;
                        MatrixI matrix = new MatrixI(block.SlimBlock.Orientation);
                        Vector3 result = new Vector3();
                        Vector3.Transform(ref center, ref matrix, out result);
                        if (result.LengthSquared() > 0.5f)
                        {
                            num = (Math.Abs(result.X) <= Math.Abs(result.Y)) ? ((Math.Abs(result.Z) <= Math.Abs(result.Y)) ? ((result.Y > 0f) ? 6 : 8) : ((result.Z > 0f) ? 10 : 12)) : ((Math.Abs(result.X) <= Math.Abs(result.Z)) ? ((result.Z > 0f) ? 10 : 12) : ((result.X > 0f) ? 2 : 4));
                        }
                    }
                    for (int i = num; i < 0x40; i++)
                    {
                        if (*(((byte*) (flagPtr + i))) == 0)
                        {
                            renderComponent.DepthBias = (byte) i;
                            return;
                        }
                    }
                }
            }
        }
    }
}

