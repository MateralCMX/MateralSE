namespace VRageMath
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class MyCuboid
    {
        public MyCuboidSide[] Sides = new MyCuboidSide[6];

        public MyCuboid()
        {
            this.Sides[0] = new MyCuboidSide();
            this.Sides[1] = new MyCuboidSide();
            this.Sides[2] = new MyCuboidSide();
            this.Sides[3] = new MyCuboidSide();
            this.Sides[4] = new MyCuboidSide();
            this.Sides[5] = new MyCuboidSide();
        }

        public void CreateFromSizes(float width1, float depth1, float width2, float depth2, float length)
        {
            float y = length * 0.5f;
            float x = width1 * 0.5f;
            float num3 = width2 * 0.5f;
            float z = depth1 * 0.5f;
            float num5 = depth2 * 0.5f;
            Vector3[] vertices = new Vector3[] { new Vector3(-num3, -y, -num5), new Vector3(num3, -y, -num5), new Vector3(-num3, -y, num5), new Vector3(num3, -y, num5), new Vector3(-x, y, -z), new Vector3(x, y, -z), new Vector3(-x, y, z), new Vector3(x, y, z) };
            this.CreateFromVertices(vertices);
        }

        public void CreateFromVertices(Vector3[] vertices)
        {
            Vector3 vector = new Vector3(float.MaxValue);
            Vector3 vector2 = new Vector3(float.MinValue);
            Vector3[] vectorArray = vertices;
            for (int i = 0; i < vectorArray.Length; i++)
            {
                Vector3 vector1 = vectorArray[i];
                vector = Vector3.Min(vector1, vector);
                vector2 = Vector3.Min(vector1, vector2);
            }
            Line line = new Line(vertices[0], vertices[2], false);
            Line line2 = new Line(vertices[2], vertices[3], false);
            Line line3 = new Line(vertices[3], vertices[1], false);
            Line line4 = new Line(vertices[1], vertices[0], false);
            Line line5 = new Line(vertices[7], vertices[6], false);
            Line line6 = new Line(vertices[6], vertices[4], false);
            Line line7 = new Line(vertices[4], vertices[5], false);
            Line line8 = new Line(vertices[5], vertices[7], false);
            Line line9 = new Line(vertices[4], vertices[0], false);
            Line line10 = new Line(vertices[0], vertices[1], false);
            Line line11 = new Line(vertices[1], vertices[5], false);
            Line line12 = new Line(vertices[5], vertices[4], false);
            Line line13 = new Line(vertices[3], vertices[2], false);
            Line line14 = new Line(vertices[2], vertices[6], false);
            Line line15 = new Line(vertices[6], vertices[7], false);
            Line line16 = new Line(vertices[7], vertices[3], false);
            Line line17 = new Line(vertices[1], vertices[3], false);
            Line line18 = new Line(vertices[3], vertices[7], false);
            Line line19 = new Line(vertices[7], vertices[5], false);
            Line line20 = new Line(vertices[5], vertices[1], false);
            Line line21 = new Line(vertices[0], vertices[4], false);
            Line line22 = new Line(vertices[4], vertices[6], false);
            Line line23 = new Line(vertices[6], vertices[2], false);
            Line line24 = new Line(vertices[2], vertices[0], false);
            this.Sides[0].Lines[0] = line;
            this.Sides[0].Lines[1] = line2;
            this.Sides[0].Lines[2] = line3;
            this.Sides[0].Lines[3] = line4;
            this.Sides[0].CreatePlaneFromLines();
            this.Sides[1].Lines[0] = line5;
            this.Sides[1].Lines[1] = line6;
            this.Sides[1].Lines[2] = line7;
            this.Sides[1].Lines[3] = line8;
            this.Sides[1].CreatePlaneFromLines();
            this.Sides[2].Lines[0] = line9;
            this.Sides[2].Lines[1] = line10;
            this.Sides[2].Lines[2] = line11;
            this.Sides[2].Lines[3] = line12;
            this.Sides[2].CreatePlaneFromLines();
            this.Sides[3].Lines[0] = line13;
            this.Sides[3].Lines[1] = line14;
            this.Sides[3].Lines[2] = line15;
            this.Sides[3].Lines[3] = line16;
            this.Sides[3].CreatePlaneFromLines();
            this.Sides[4].Lines[0] = line17;
            this.Sides[4].Lines[1] = line18;
            this.Sides[4].Lines[2] = line19;
            this.Sides[4].Lines[3] = line20;
            this.Sides[4].CreatePlaneFromLines();
            this.Sides[5].Lines[0] = line21;
            this.Sides[5].Lines[1] = line22;
            this.Sides[5].Lines[2] = line23;
            this.Sides[5].Lines[3] = line24;
            this.Sides[5].CreatePlaneFromLines();
        }

        public MyCuboid CreateTransformed(ref Matrix worldMatrix)
        {
            Vector3[] vertices = new Vector3[8];
            int index = 0;
            foreach (Vector3 vector in this.Vertices)
            {
                vertices[index] = Vector3.Transform(vector, (Matrix) worldMatrix);
                index++;
            }
            MyCuboid cuboid1 = new MyCuboid();
            cuboid1.CreateFromVertices(vertices);
            return cuboid1;
        }

        public BoundingBox GetAABB()
        {
            BoundingBox box = BoundingBox.CreateInvalid();
            foreach (Line local1 in this.UniqueLines)
            {
                Vector3 from = local1.From;
                Vector3 to = local1.To;
                box = box.Include(ref from).Include(ref to);
            }
            return box;
        }

        public unsafe BoundingBox GetLocalAABB()
        {
            BoundingBox aABB = this.GetAABB();
            Vector3 center = aABB.Center;
            Vector3* vectorPtr1 = (Vector3*) ref aABB.Min;
            vectorPtr1[0] -= center;
            Vector3* vectorPtr2 = (Vector3*) ref aABB.Max;
            vectorPtr2[0] -= center;
            return aABB;
        }

        public IEnumerable<Line> UniqueLines
        {
            [IteratorStateMachine(typeof(<get_UniqueLines>d__3))]
            get
            {
                <get_UniqueLines>d__3 d__1 = new <get_UniqueLines>d__3(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        public IEnumerable<Vector3> Vertices
        {
            [IteratorStateMachine(typeof(<get_Vertices>d__5))]
            get
            {
                <get_Vertices>d__5 d__1 = new <get_Vertices>d__5(-2);
                d__1.<>4__this = this;
                return d__1;
            }
        }

        [CompilerGenerated]
        private sealed class <get_UniqueLines>d__3 : IEnumerable<Line>, IEnumerable, IEnumerator<Line>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private Line <>2__current;
            private int <>l__initialThreadId;
            public MyCuboid <>4__this;

            [DebuggerHidden]
            public <get_UniqueLines>d__3(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                MyCuboid cuboid = this.<>4__this;
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[0];
                        this.<>1__state = 1;
                        return true;

                    case 1:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[1];
                        this.<>1__state = 2;
                        return true;

                    case 2:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[2];
                        this.<>1__state = 3;
                        return true;

                    case 3:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[3];
                        this.<>1__state = 4;
                        return true;

                    case 4:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[0];
                        this.<>1__state = 5;
                        return true;

                    case 5:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[1];
                        this.<>1__state = 6;
                        return true;

                    case 6:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[2];
                        this.<>1__state = 7;
                        return true;

                    case 7:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[3];
                        this.<>1__state = 8;
                        return true;

                    case 8:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[2].Lines[0];
                        this.<>1__state = 9;
                        return true;

                    case 9:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[2].Lines[2];
                        this.<>1__state = 10;
                        return true;

                    case 10:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[4].Lines[1];
                        this.<>1__state = 11;
                        return true;

                    case 11:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[5].Lines[2];
                        this.<>1__state = 12;
                        return true;

                    case 12:
                        this.<>1__state = -1;
                        return false;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Line> IEnumerable<Line>.GetEnumerator()
            {
                MyCuboid.<get_UniqueLines>d__3 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MyCuboid.<get_UniqueLines>d__3(0) {
                        <>4__this = this.<>4__this
                    };
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<VRageMath.Line>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            Line IEnumerator<Line>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <get_Vertices>d__5 : IEnumerable<Vector3>, IEnumerable, IEnumerator<Vector3>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private Vector3 <>2__current;
            private int <>l__initialThreadId;
            public MyCuboid <>4__this;

            [DebuggerHidden]
            public <get_Vertices>d__5(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                MyCuboid cuboid = this.<>4__this;
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[2].Lines[1].From;
                        this.<>1__state = 1;
                        return true;

                    case 1:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[2].Lines[1].To;
                        this.<>1__state = 2;
                        return true;

                    case 2:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[1].From;
                        this.<>1__state = 3;
                        return true;

                    case 3:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[0].Lines[1].To;
                        this.<>1__state = 4;
                        return true;

                    case 4:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[2].From;
                        this.<>1__state = 5;
                        return true;

                    case 5:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[1].Lines[2].To;
                        this.<>1__state = 6;
                        return true;

                    case 6:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[3].Lines[2].From;
                        this.<>1__state = 7;
                        return true;

                    case 7:
                        this.<>1__state = -1;
                        this.<>2__current = cuboid.Sides[3].Lines[2].To;
                        this.<>1__state = 8;
                        return true;

                    case 8:
                        this.<>1__state = -1;
                        return false;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<Vector3> IEnumerable<Vector3>.GetEnumerator()
            {
                MyCuboid.<get_Vertices>d__5 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new MyCuboid.<get_Vertices>d__5(0) {
                        <>4__this = this.<>4__this
                    };
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<VRageMath.Vector3>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            Vector3 IEnumerator<Vector3>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

