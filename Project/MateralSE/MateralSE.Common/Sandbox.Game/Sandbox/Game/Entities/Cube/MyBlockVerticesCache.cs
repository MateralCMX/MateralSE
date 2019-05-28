namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Game;
    using VRageMath;

    internal class MyBlockVerticesCache
    {
        private const bool ADD_INNER_BONES_TO_CONVEX = false;
        private static List<Vector3>[][] Cache;

        static MyBlockVerticesCache()
        {
            GenerateConvexVertices();
        }

        private static void GenerateConvexVertices()
        {
            List<Vector3> verts = new List<Vector3>(0x1b);
            Array values = Enum.GetValues(typeof(MyCubeTopology));
            Cache = new List<Vector3>[values.Length][];
            foreach (MyCubeTopology topology in values)
            {
                GetTopologySwitch(topology, verts);
                Cache[(int) topology] = new List<Vector3>[0x24];
                Base6Directions.Direction[] enumDirections = Base6Directions.EnumDirections;
                int index = 0;
                while (true)
                {
                    if (index >= enumDirections.Length)
                    {
                        verts.Clear();
                        break;
                    }
                    Base6Directions.Direction dir = enumDirections[index];
                    Base6Directions.Direction[] directionArray2 = Base6Directions.EnumDirections;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= directionArray2.Length)
                        {
                            index++;
                            break;
                        }
                        Base6Directions.Direction direction2 = directionArray2[num2];
                        if ((dir != direction2) && (Base6Directions.GetIntVector(dir) != -Base6Directions.GetIntVector(direction2)))
                        {
                            List<Vector3> list2 = new List<Vector3>(verts.Count);
                            Cache[(int) topology][(int) ((dir * (Base6Directions.Direction.Forward | Base6Directions.Direction.Left | Base6Directions.Direction.Up)) + direction2)] = list2;
                            MyBlockOrientation orientation = new MyBlockOrientation(dir, direction2);
                            foreach (Vector3 vector in verts)
                            {
                                list2.Add(Vector3.TransformNormal(vector, orientation));
                            }
                        }
                        num2++;
                    }
                }
            }
        }

        public static ListReader<Vector3> GetBlockVertices(MyCubeTopology topology, MyBlockOrientation orientation) => 
            new ListReader<Vector3>(Cache[(int) topology][(int) ((orientation.Forward * (Base6Directions.Direction.Forward | Base6Directions.Direction.Left | Base6Directions.Direction.Up)) + orientation.Up)]);

        private static void GetTopologySwitch(MyCubeTopology topology, List<Vector3> verts)
        {
            switch (topology)
            {
                case MyCubeTopology.Box:
                case MyCubeTopology.RoundedSlope:
                    verts.Add(new Vector3(1f, 1f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, 1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    return;

                case MyCubeTopology.Slope:
                case MyCubeTopology.RotatedSlope:
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    return;

                case MyCubeTopology.Corner:
                case MyCubeTopology.RotatedCorner:
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    return;

                case MyCubeTopology.InvCorner:
                    verts.Add(new Vector3(1f, 1f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    return;

                case MyCubeTopology.StandaloneBox:
                    return;

                case MyCubeTopology.RoundSlope:
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, 0.414f, 0.414f));
                    verts.Add(new Vector3(1f, 0.414f, 0.414f));
                    return;

                case MyCubeTopology.RoundCorner:
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-0.414f, 0.414f, -1f));
                    verts.Add(new Vector3(-0.414f, -1f, 0.414f));
                    verts.Add(new Vector3(1f, 0.414f, 0.414f));
                    return;

                case MyCubeTopology.RoundInvCorner:
                    verts.Add(new Vector3(1f, 1f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(0.414f, -0.414f, -1f));
                    verts.Add(new Vector3(0.414f, -1f, -0.414f));
                    verts.Add(new Vector3(1f, -0.414f, -0.414f));
                    return;

                case MyCubeTopology.Slope2Base:
                    verts.Add(new Vector3(1f, 0f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, 0f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    return;

                case MyCubeTopology.Slope2Tip:
                    verts.Add(new Vector3(-1f, 0f, -1f));
                    verts.Add(new Vector3(1f, 0f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    return;

                case MyCubeTopology.Corner2Base:
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(1f, 0f, -1f));
                    verts.Add(new Vector3(1f, -1f, 0f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    return;

                case MyCubeTopology.Corner2Tip:
                    verts.Add(new Vector3(1f, 0f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(0f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    return;

                case MyCubeTopology.InvCorner2Base:
                    verts.Add(new Vector3(1f, 1f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, 1f));
                    verts.Add(new Vector3(1f, 0f, -1f));
                    verts.Add(new Vector3(0f, -1f, -1f));
                    verts.Add(new Vector3(-1f, 1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    return;

                case MyCubeTopology.InvCorner2Tip:
                    verts.Add(new Vector3(1f, 1f, 1f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, 1f, 1f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, 0f, 1f));
                    verts.Add(new Vector3(0f, -1f, 1f));
                    return;

                case MyCubeTopology.HalfBox:
                    verts.Add(new Vector3(1f, 1f, 0f));
                    verts.Add(new Vector3(1f, -1f, 0f));
                    verts.Add(new Vector3(1f, 1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    verts.Add(new Vector3(-1f, 1f, 0f));
                    verts.Add(new Vector3(-1f, -1f, 0f));
                    verts.Add(new Vector3(-1f, 1f, -1f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    return;

                case MyCubeTopology.HalfSlopeBox:
                    verts.Add(new Vector3(-1f, 0f, -1f));
                    verts.Add(new Vector3(1f, 0f, -1f));
                    verts.Add(new Vector3(-1f, -1f, 0f));
                    verts.Add(new Vector3(1f, -1f, 0f));
                    verts.Add(new Vector3(-1f, -1f, -1f));
                    verts.Add(new Vector3(1f, -1f, -1f));
                    return;
            }
        }
    }
}

