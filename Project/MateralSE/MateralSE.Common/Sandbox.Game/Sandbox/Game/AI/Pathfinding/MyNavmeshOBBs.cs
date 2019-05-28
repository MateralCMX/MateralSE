namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRageMath;
    using VRageRender;

    public class MyNavmeshOBBs
    {
        private const int NEIGHBOUR_OVERLAP_TILES = 2;
        private MyOrientedBoundingBoxD?[][] m_obbs;
        private float m_tileHalfSize;
        private float m_tileHalfHeight;
        private MyPlanet m_planet;
        private int m_middleCoord;

        public MyNavmeshOBBs(MyPlanet planet, Vector3D centerPoint, Vector3D forwardDirection, int obbsPerLine, int tileSize, int tileHeight)
        {
            this.m_planet = planet;
            this.OBBsPerLine = obbsPerLine;
            if ((this.OBBsPerLine % 2) == 0)
            {
                this.OBBsPerLine++;
            }
            this.m_middleCoord = (this.OBBsPerLine - 1) / 2;
            this.m_tileHalfSize = tileSize * 0.5f;
            this.m_tileHalfHeight = tileHeight * 0.5f;
            this.m_obbs = new MyOrientedBoundingBoxD?[this.OBBsPerLine][];
            for (int i = 0; i < this.OBBsPerLine; i++)
            {
                this.m_obbs[i] = new MyOrientedBoundingBoxD?[this.OBBsPerLine];
            }
            this.Initialize(centerPoint, forwardDirection);
            this.BaseOBB = this.GetBaseOBB();
        }

        public void Clear()
        {
            for (int i = 0; i < this.m_obbs.Length; i++)
            {
                Array.Clear(this.m_obbs[i], 0, this.m_obbs.Length);
            }
            Array.Clear(this.m_obbs, 0, this.m_obbs.Length);
            this.m_obbs = null;
        }

        private MyOrientedBoundingBoxD CreateOBB(Vector3D center, Vector3D perpedicularVector)
        {
            Vector3D vectord = -Vector3D.Normalize(MyGravityProviderSystem.CalculateTotalGravityInPoint(center));
            return new MyOrientedBoundingBoxD(center, new Vector3D((double) this.m_tileHalfSize, (double) this.m_tileHalfHeight, (double) this.m_tileHalfSize), Quaternion.CreateFromForwardUp((Vector3) perpedicularVector, (Vector3) vectord));
        }

        public void DebugDraw()
        {
            int index = 0;
            while (index < this.m_obbs.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_obbs[0].Length)
                    {
                        index++;
                        break;
                    }
                    if (this.m_obbs[index][num2] != null)
                    {
                        MyRenderProxy.DebugDrawOBB(this.m_obbs[index][num2].Value, Color.Red, 0f, true, false, false);
                    }
                    num2++;
                }
            }
            MyRenderProxy.DebugDrawOBB(this.BaseOBB, Color.White, 0f, true, false, false);
            if (this.m_obbs[0][0] != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_obbs[0][0].Value.Center, 5f, Color.Yellow, 0f, true, false, true, false);
            }
            if (this.m_obbs[0][this.OBBsPerLine - 1] != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_obbs[0][this.OBBsPerLine - 1].Value.Center, 5f, Color.Green, 0f, true, false, true, false);
            }
            if (this.m_obbs[this.OBBsPerLine - 1][this.OBBsPerLine - 1] != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_obbs[this.OBBsPerLine - 1][this.OBBsPerLine - 1].Value.Center, 5f, Color.Blue, 0f, true, false, true, false);
            }
            if (this.m_obbs[this.OBBsPerLine - 1][0] != null)
            {
                MyRenderProxy.DebugDrawSphere(this.m_obbs[this.OBBsPerLine - 1][0].Value.Center, 5f, Color.White, 0f, true, false, true, false);
            }
            MyOrientedBoundingBoxD? nullable = this.m_obbs[0][0];
            MyOrientedBoundingBoxD? nullable2 = this.m_obbs[this.OBBsPerLine - 1][0];
            MyOrientedBoundingBoxD? nullable3 = this.m_obbs[this.OBBsPerLine - 1][this.OBBsPerLine - 1];
            MyRenderProxy.DebugDrawSphere(GetOBBCorner(nullable.Value, OBBCorner.LowerBackLeft), 5f, Color.White, 0f, true, false, true, false);
            MyRenderProxy.DebugDrawSphere(GetOBBCorner(nullable2.Value, OBBCorner.LowerBackRight), 5f, Color.White, 0f, true, false, true, false);
            MyRenderProxy.DebugDrawSphere(GetOBBCorner(nullable3.Value, OBBCorner.LowerFrontRight), 5f, Color.White, 0f, true, false, true, false);
        }

        private unsafe void Fill(double angle)
        {
            Vector2I currentIndex = new Vector2I(this.m_middleCoord, 0);
            for (int i = 0; i < this.OBBsPerLine; i++)
            {
                MyOrientedBoundingBoxD xd;
                if (this.m_obbs[currentIndex.Y][currentIndex.X] != null)
                {
                    xd = this.m_obbs[currentIndex.Y][currentIndex.X].Value;
                }
                else
                {
                    xd = this.CreateOBB(this.NewTransformedPoint(this.CenterOBB.Center, this.CenterOBB.Orientation.Forward, ((float) angle) * (i - this.m_middleCoord)), this.CenterOBB.Orientation.Forward);
                }
                this.FillOBBHorizontalLine(xd, currentIndex, angle);
                int* numPtr1 = (int*) ref currentIndex.Y;
                numPtr1[0]++;
            }
        }

        private void FillOBBHorizontalLine(MyOrientedBoundingBoxD lineCenterOBB, Vector2I currentIndex, double angle)
        {
            this.m_obbs[currentIndex.Y][currentIndex.X] = new MyOrientedBoundingBoxD?(lineCenterOBB);
            for (int i = 0; i < this.OBBsPerLine; i++)
            {
                if (i != currentIndex.X)
                {
                    this.m_obbs[currentIndex.Y][i] = new MyOrientedBoundingBoxD?(this.CreateOBB(this.NewTransformedPoint(lineCenterOBB.Center, lineCenterOBB.Orientation.Right, (float) (angle * (i - this.m_middleCoord))), lineCenterOBB.Orientation.Right));
                }
            }
        }

        private MyOrientedBoundingBoxD GetBaseOBB()
        {
            MyOrientedBoundingBoxD? nullable = this.m_obbs[0][0];
            MyOrientedBoundingBoxD? nullable2 = this.m_obbs[this.OBBsPerLine - 1][0];
            MyOrientedBoundingBoxD? nullable3 = this.m_obbs[this.OBBsPerLine - 1][this.OBBsPerLine - 1];
            Vector3D oBBCorner = GetOBBCorner(nullable3.Value, OBBCorner.LowerFrontRight);
            double x = (GetOBBCorner(nullable.Value, OBBCorner.LowerBackLeft) - GetOBBCorner(nullable2.Value, OBBCorner.LowerBackRight)).Length() / 2.0;
            return new MyOrientedBoundingBoxD((GetOBBCorner(nullable.Value, OBBCorner.LowerBackLeft) + oBBCorner) / 2.0, new Vector3D(x, 0.01, x), this.CenterOBB.Orientation);
        }

        private MyOrientedBoundingBoxD GetCenterOBB(Vector3D initialPoint, Vector3D forwardDirection, out double angle)
        {
            Vector3D center = this.m_planet.PositionComp.WorldAABB.Center;
            double num = (initialPoint - center).Length();
            double num2 = Math.Asin(((double) this.m_tileHalfSize) / num);
            angle = num2 * 2.0;
            return this.CreateOBB(initialPoint, forwardDirection);
        }

        public List<OBBCoords> GetIntersectedOBB(LineD line)
        {
            int num2;
            Dictionary<OBBCoords, double> dictionary = new Dictionary<OBBCoords, double>();
            int index = 0;
            goto TR_000C;
        TR_0002:
            num2++;
        TR_0009:
            while (true)
            {
                if (num2 >= this.m_obbs[0].Length)
                {
                    index++;
                    break;
                }
                if (!this.m_obbs[index][num2].Value.Contains(ref line.From) && !this.m_obbs[index][num2].Value.Contains(ref line.To))
                {
                    MyOrientedBoundingBoxD xd = this.m_obbs[index][num2].Value;
                    if (xd.Intersects(ref line) == null)
                    {
                        goto TR_0002;
                    }
                }
                OBBCoords key = new OBBCoords {
                    OBB = this.m_obbs[index][num2].Value,
                    Coords = new Vector2I(index, num2)
                };
                dictionary.Add(key, Vector3D.Distance(line.From, this.m_obbs[index][num2].Value.Center));
                goto TR_0002;
            }
        TR_000C:
            while (true)
            {
                if (index >= this.m_obbs.Length)
                {
                    return (from kvp in dictionary
                        orderby kvp.Value
                        select kvp.Key).ToList<OBBCoords>();
                }
                num2 = 0;
                break;
            }
            goto TR_0009;
        }

        public MyOrientedBoundingBoxD? GetOBB(Vector3D worldPosition)
        {
            MyOrientedBoundingBoxD?[][] obbs = this.m_obbs;
            int index = 0;
            while (index < obbs.Length)
            {
                MyOrientedBoundingBoxD?[] nullableArray2 = obbs[index];
                int num2 = 0;
                while (true)
                {
                    if (num2 >= nullableArray2.Length)
                    {
                        index++;
                        break;
                    }
                    MyOrientedBoundingBoxD? nullable = nullableArray2[num2];
                    MyOrientedBoundingBoxD xd = nullable.Value;
                    if (xd.Contains(ref worldPosition))
                    {
                        return nullable;
                    }
                    num2++;
                }
            }
            return null;
        }

        public MyOrientedBoundingBoxD? GetOBB(int coordX, int coordY)
        {
            if (((coordX >= 0) && ((coordX < this.OBBsPerLine) && (coordY >= 0))) && (coordY < this.OBBsPerLine))
            {
                return this.m_obbs[coordX][coordY];
            }
            return null;
        }

        public OBBCoords? GetOBBCoord(Vector3D worldPosition)
        {
            int index = 0;
            while (index < this.m_obbs.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 >= this.m_obbs[0].Length)
                    {
                        index++;
                        break;
                    }
                    MyOrientedBoundingBoxD xd = this.m_obbs[index][num2].Value;
                    if (xd.Contains(ref worldPosition))
                    {
                        return new OBBCoords?(new OBBCoords { 
                            OBB = xd,
                            Coords = new Vector2I(index, num2)
                        });
                    }
                    num2++;
                }
            }
            return null;
        }

        public OBBCoords? GetOBBCoord(int coordX, int coordY)
        {
            if (((coordX < 0) || ((coordX >= this.OBBsPerLine) || (coordY < 0))) || (coordY >= this.OBBsPerLine))
            {
                return null;
            }
            return new OBBCoords?(new OBBCoords { 
                OBB = this.m_obbs[coordX][coordY].Value,
                Coords = new Vector2I(coordX, coordY)
            });
        }

        public static Vector3D GetOBBCorner(MyOrientedBoundingBoxD obb, OBBCorner corner)
        {
            Vector3D[] corners = new Vector3D[8];
            obb.GetCorners(corners, 0);
            return corners[(int) corner];
        }

        private void Initialize(Vector3D initialPoint, Vector3D forwardDirection)
        {
            double num;
            this.CenterOBB = this.GetCenterOBB(initialPoint, forwardDirection, out num);
            this.m_obbs[this.m_middleCoord][this.m_middleCoord] = new MyOrientedBoundingBoxD?(this.CenterOBB);
            this.Fill(num);
            double angle = num * Math.Max((2 * this.m_middleCoord) - 1, 1);
            this.SetNeigboursCenter(angle);
        }

        private Vector3D NewTransformedPoint(Vector3D point, Vector3 rotationVector, float angle)
        {
            Vector3D center = this.m_planet.PositionComp.WorldAABB.Center;
            Quaternion rotation = Quaternion.CreateFromAxisAngle(rotationVector, angle);
            return (Vector3D.Transform(point - center, rotation) + center);
        }

        private void SetNeigboursCenter(double angle)
        {
            this.NeighboursCenters = new List<Vector3D>();
            Vector3D item = this.NewTransformedPoint(this.CenterOBB.Center, this.CenterOBB.Orientation.Forward, (float) angle);
            Vector3D vectord2 = this.NewTransformedPoint(this.CenterOBB.Center, this.CenterOBB.Orientation.Forward, -((float) angle));
            Vector3D vectord3 = this.NewTransformedPoint(this.CenterOBB.Center, this.CenterOBB.Orientation.Right, (float) angle);
            Vector3D vectord4 = this.NewTransformedPoint(this.CenterOBB.Center, this.CenterOBB.Orientation.Right, -((float) angle));
            this.NeighboursCenters.Add(item);
            this.NeighboursCenters.Add(vectord2);
            this.NeighboursCenters.Add(vectord3);
            this.NeighboursCenters.Add(vectord4);
            Vector3D vectord5 = this.NewTransformedPoint(vectord4, this.CenterOBB.Orientation.Forward, -((float) angle));
            Vector3D vectord6 = this.NewTransformedPoint(vectord4, this.CenterOBB.Orientation.Forward, (float) angle);
            Vector3D vectord7 = this.NewTransformedPoint(vectord3, this.CenterOBB.Orientation.Forward, -((float) angle));
            Vector3D vectord8 = this.NewTransformedPoint(vectord3, this.CenterOBB.Orientation.Forward, (float) angle);
            this.NeighboursCenters.Add(vectord5);
            this.NeighboursCenters.Add(vectord6);
            this.NeighboursCenters.Add(vectord7);
            this.NeighboursCenters.Add(vectord8);
        }

        public int OBBsPerLine { get; private set; }

        public MyOrientedBoundingBoxD BaseOBB { get; private set; }

        public MyOrientedBoundingBoxD CenterOBB
        {
            get => 
                this.m_obbs[this.m_middleCoord][this.m_middleCoord].Value;
            private set => 
                (this.m_obbs[this.m_middleCoord][this.m_middleCoord] = new MyOrientedBoundingBoxD?(value));
        }

        public List<Vector3D> NeighboursCenters { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyNavmeshOBBs.<>c <>9 = new MyNavmeshOBBs.<>c();
            public static Func<KeyValuePair<MyNavmeshOBBs.OBBCoords, double>, double> <>9__27_0;
            public static Func<KeyValuePair<MyNavmeshOBBs.OBBCoords, double>, MyNavmeshOBBs.OBBCoords> <>9__27_1;

            internal double <GetIntersectedOBB>b__27_0(KeyValuePair<MyNavmeshOBBs.OBBCoords, double> d) => 
                d.Value;

            internal MyNavmeshOBBs.OBBCoords <GetIntersectedOBB>b__27_1(KeyValuePair<MyNavmeshOBBs.OBBCoords, double> kvp) => 
                kvp.Key;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct OBBCoords
        {
            public Vector2I Coords;
            public MyOrientedBoundingBoxD OBB;
        }

        public enum OBBCorner
        {
            UpperFrontLeft,
            UpperBackLeft,
            LowerBackLeft,
            LowerFrontLeft,
            UpperFrontRight,
            UpperBackRight,
            LowerBackRight,
            LowerFrontRight
        }
    }
}

