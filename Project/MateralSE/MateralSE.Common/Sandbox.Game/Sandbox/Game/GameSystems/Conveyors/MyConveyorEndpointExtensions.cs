namespace Sandbox.Game.GameSystems.Conveyors
{
    using Sandbox.Engine.Utils;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal static class MyConveyorEndpointExtensions
    {
        public static void DebugDraw(this IMyConveyorEndpoint endpoint)
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_CONVEYORS)
            {
                Vector3 pointTo = new Vector3();
                for (int i = 0; i < endpoint.GetLineCount(); i++)
                {
                    ConveyorLinePosition position = endpoint.GetPosition(i);
                    Vector3 vector2 = new Vector3(position.LocalGridPosition) + (0.5f * new Vector3(position.VectorDirection));
                    pointTo += vector2;
                }
                pointTo = (Vector3) Vector3.Transform((pointTo * endpoint.CubeBlock.CubeGrid.GridSize) / ((float) endpoint.GetLineCount()), endpoint.CubeBlock.CubeGrid.WorldMatrix);
                for (int j = 0; j < endpoint.GetLineCount(); j++)
                {
                    ConveyorLinePosition position = endpoint.GetPosition(j);
                    MyConveyorLine conveyorLine = endpoint.GetConveyorLine(j);
                    Vector3 pointFrom = (Vector3) Vector3.Transform((new Vector3(position.LocalGridPosition) + (0.5f * new Vector3(position.VectorDirection))) * endpoint.CubeBlock.CubeGrid.GridSize, endpoint.CubeBlock.CubeGrid.WorldMatrix);
                    Vector3 worldCoord = (Vector3) Vector3.Transform((new Vector3(position.LocalGridPosition) + (0.4f * new Vector3(position.VectorDirection))) * endpoint.CubeBlock.CubeGrid.GridSize, endpoint.CubeBlock.CubeGrid.WorldMatrix);
                    Vector3 vector5 = Vector3.TransformNormal((Vector3) ((position.VectorDirection * endpoint.CubeBlock.CubeGrid.GridSize) * 0.5f), endpoint.CubeBlock.CubeGrid.WorldMatrix);
                    Color colorFrom = conveyorLine.IsFunctional ? Color.Orange : Color.DarkRed;
                    colorFrom = conveyorLine.IsWorking ? Color.GreenYellow : colorFrom;
                    EndpointDebugShape shape = EndpointDebugShape.SHAPE_SPHERE;
                    float num3 = 1f;
                    float num4 = 0.05f;
                    if ((conveyorLine.GetEndpoint(0) == null) || (conveyorLine.GetEndpoint(1) == null))
                    {
                        if (conveyorLine.Type == MyObjectBuilder_ConveyorLine.LineType.SMALL_LINE)
                        {
                            num3 = 0.2f;
                            num4 = 0.015f;
                            shape = EndpointDebugShape.SHAPE_SPHERE;
                        }
                        else
                        {
                            num3 = 0.1f;
                            num4 = 0.015f;
                            shape = EndpointDebugShape.SHAPE_CAPSULE;
                        }
                    }
                    else if (conveyorLine.Type == MyObjectBuilder_ConveyorLine.LineType.SMALL_LINE)
                    {
                        num3 = 1f;
                        num4 = 0.05f;
                        shape = EndpointDebugShape.SHAPE_SPHERE;
                    }
                    else
                    {
                        num3 = 0.2f;
                        num4 = 0.05f;
                        shape = EndpointDebugShape.SHAPE_CAPSULE;
                    }
                    MyRenderProxy.DebugDrawLine3D(pointFrom, pointFrom + (vector5 * num3), colorFrom, colorFrom, true, false);
                    if (shape == EndpointDebugShape.SHAPE_SPHERE)
                    {
                        MyRenderProxy.DebugDrawSphere(pointFrom, num4 * endpoint.CubeBlock.CubeGrid.GridSize, colorFrom.ToVector3(), 1f, false, false, true, false);
                    }
                    else if (shape == EndpointDebugShape.SHAPE_CAPSULE)
                    {
                        MyRenderProxy.DebugDrawCapsule(pointFrom - (vector5 * num3), pointFrom + (vector5 * num3), num4 * endpoint.CubeBlock.CubeGrid.GridSize, colorFrom, false, false, false);
                    }
                    if (MyDebugDrawSettings.DEBUG_DRAW_CONVEYORS_LINE_IDS)
                    {
                        MyRenderProxy.DebugDrawText3D(worldCoord, conveyorLine.GetHashCode().ToString(), colorFrom, 0.6f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
                    }
                    MyRenderProxy.DebugDrawLine3D(pointFrom, pointTo, colorFrom, colorFrom, false, false);
                }
            }
        }

        private enum EndpointDebugShape
        {
            SHAPE_SPHERE,
            SHAPE_CAPSULE
        }
    }
}

