namespace Sandbox.Game.AI.Pathfinding
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyDefinitionType(typeof(MyObjectBuilder_BlockNavigationDefinition), (Type) null)]
    public class MyBlockNavigationDefinition : MyDefinitionBase
    {
        private MyGridNavigationMesh m_mesh = null;
        private static StringBuilder m_tmpStringBuilder = new StringBuilder();
        private static MyObjectBuilder_BlockNavigationDefinition m_tmpDefaultOb = new MyObjectBuilder_BlockNavigationDefinition();

        public MyBlockNavigationDefinition()
        {
            this.NoEntry = false;
        }

        public static void CreateDefaultTriangles(MyObjectBuilder_BlockNavigationDefinition ob)
        {
            Vector3I size = (Vector3I) ob.Size;
            Vector3I center = (Vector3I) ob.Center;
            int num = 4 * (((size.X * size.Y) + (size.X * size.Z)) + (size.Y * size.Z));
            ob.Triangles = new MyObjectBuilder_BlockNavigationDefinition.Triangle[num];
            int index = 0;
            Vector3 vector = ((Vector3) ((size * 0.5f) - center)) - Vector3.Half;
            int num3 = 0;
            while (num3 < 6)
            {
                Base6Directions.Direction right;
                Base6Directions.Direction up;
                Base6Directions.Direction direction = Base6Directions.EnumDirections[num3];
                Vector3 vector2 = vector;
                switch (direction)
                {
                    case Base6Directions.Direction.Backward:
                        right = Base6Directions.Direction.Right;
                        up = Base6Directions.Direction.Up;
                        vector2 += new Vector3(-0.5f, -0.5f, 0.5f) * size;
                        break;

                    case Base6Directions.Direction.Left:
                        right = Base6Directions.Direction.Backward;
                        up = Base6Directions.Direction.Up;
                        vector2 += new Vector3(-0.5f, -0.5f, -0.5f) * size;
                        break;

                    case Base6Directions.Direction.Right:
                        right = Base6Directions.Direction.Forward;
                        up = Base6Directions.Direction.Up;
                        vector2 += new Vector3(0.5f, -0.5f, 0.5f) * size;
                        break;

                    case Base6Directions.Direction.Up:
                        right = Base6Directions.Direction.Right;
                        up = Base6Directions.Direction.Forward;
                        vector2 += new Vector3(-0.5f, 0.5f, 0.5f) * size;
                        break;

                    case Base6Directions.Direction.Down:
                        right = Base6Directions.Direction.Right;
                        up = Base6Directions.Direction.Backward;
                        vector2 += new Vector3(-0.5f, -0.5f, -0.5f) * size;
                        break;

                    default:
                        right = Base6Directions.Direction.Left;
                        up = Base6Directions.Direction.Up;
                        vector2 += new Vector3(0.5f, -0.5f, -0.5f) * size;
                        break;
                }
                Vector3 vector3 = Base6Directions.GetVector(right);
                Vector3 vector4 = Base6Directions.GetVector(up);
                int num4 = size.AxisValue(Base6Directions.GetAxis(up));
                int num5 = size.AxisValue(Base6Directions.GetAxis(right));
                int num6 = 0;
                while (true)
                {
                    if (num6 >= num4)
                    {
                        num3++;
                        break;
                    }
                    int num7 = 0;
                    while (true)
                    {
                        if (num7 >= num5)
                        {
                            vector2 = (vector2 - (vector3 * num5)) + vector4;
                            num6++;
                            break;
                        }
                        MyObjectBuilder_BlockNavigationDefinition.Triangle triangle = new MyObjectBuilder_BlockNavigationDefinition.Triangle {
                            Points = new SerializableVector3[] { 
                                vector2,
                                vector2 + vector3,
                                vector2 + vector4
                            }
                        };
                        index++;
                        ob.Triangles[index] = triangle;
                        triangle = new MyObjectBuilder_BlockNavigationDefinition.Triangle {
                            Points = new SerializableVector3[3]
                        };
                        triangle.Points[0] = vector2 + vector3;
                        triangle.Points[1] = (vector2 + vector3) + vector4;
                        triangle.Points[2] = vector2 + vector4;
                        index++;
                        ob.Triangles[index] = triangle;
                        vector2 += vector3;
                        num7++;
                    }
                }
            }
        }

        public static MyObjectBuilder_BlockNavigationDefinition GetDefaultObjectBuilder(MyCubeBlockDefinition blockDefinition)
        {
            m_tmpStringBuilder.Clear();
            m_tmpStringBuilder.Append("Default_");
            m_tmpStringBuilder.Append(blockDefinition.Size.X);
            m_tmpStringBuilder.Append("_");
            m_tmpStringBuilder.Append(blockDefinition.Size.Y);
            m_tmpStringBuilder.Append("_");
            m_tmpStringBuilder.Append(blockDefinition.Size.Z);
            m_tmpDefaultOb.Id = (SerializableDefinitionId) new MyDefinitionId(typeof(MyObjectBuilder_BlockNavigationDefinition), m_tmpStringBuilder.ToString());
            m_tmpDefaultOb.Size = blockDefinition.Size;
            m_tmpDefaultOb.Center = blockDefinition.Center;
            return m_tmpDefaultOb;
        }

        protected override unsafe void Init(MyObjectBuilder_DefinitionBase ob)
        {
            base.Init(ob);
            MyObjectBuilder_BlockNavigationDefinition definition = ob as MyObjectBuilder_BlockNavigationDefinition;
            if (ob != null)
            {
                if (definition.NoEntry || (definition.Triangles == null))
                {
                    this.NoEntry = true;
                }
                else
                {
                    this.NoEntry = false;
                    MyGridNavigationMesh mesh = new MyGridNavigationMesh(null, null, definition.Triangles.Length, null);
                    Vector3I max = (Vector3I) ((definition.Size - Vector3I.One) - definition.Center);
                    Vector3I min = (Vector3I) -definition.Center;
                    MyObjectBuilder_BlockNavigationDefinition.Triangle[] triangles = definition.Triangles;
                    int index = 0;
                    while (index < triangles.Length)
                    {
                        Vector3I vectori6;
                        Vector3I vectori7;
                        MyObjectBuilder_BlockNavigationDefinition.Triangle triangle1 = triangles[index];
                        Vector3 a = (Vector3) triangle1.Points[0];
                        Vector3 b = (Vector3) triangle1.Points[1];
                        Vector3 c = (Vector3) triangle1.Points[2];
                        MyNavigationTriangle tri = mesh.AddTriangle(ref a, ref b, ref c);
                        Vector3 vector4 = ((((a + b) + c) / 3f) - a) * 0.0001f;
                        Vector3 vector5 = ((((a + b) + c) / 3f) - b) * 0.0001f;
                        Vector3 vector6 = ((((a + b) + c) / 3f) - c) * 0.0001f;
                        Vector3I result = Vector3I.Round(a + vector4);
                        Vector3I vectori4 = Vector3I.Round(b + vector5);
                        Vector3I vectori5 = Vector3I.Round(c + vector6);
                        Vector3I* vectoriPtr1 = (Vector3I*) ref result;
                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr1, ref min, ref max, out result);
                        Vector3I* vectoriPtr2 = (Vector3I*) ref vectori4;
                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr2, ref min, ref max, out vectori4);
                        Vector3I* vectoriPtr3 = (Vector3I*) ref vectori5;
                        Vector3I.Clamp(ref (Vector3I) ref vectoriPtr3, ref min, ref max, out vectori5);
                        Vector3I.Min(ref result, ref vectori4, out vectori6);
                        Vector3I* vectoriPtr4 = (Vector3I*) ref vectori6;
                        Vector3I.Min(ref (Vector3I) ref vectoriPtr4, ref vectori5, out vectori6);
                        Vector3I.Max(ref result, ref vectori4, out vectori7);
                        Vector3I* vectoriPtr5 = (Vector3I*) ref vectori7;
                        Vector3I.Max(ref (Vector3I) ref vectoriPtr5, ref vectori5, out vectori7);
                        Vector3I gridPos = vectori6;
                        Vector3I_RangeIterator iterator = new Vector3I_RangeIterator(ref vectori6, ref vectori7);
                        while (true)
                        {
                            if (!iterator.IsValid())
                            {
                                index++;
                                break;
                            }
                            mesh.RegisterTriangle(tri, ref gridPos);
                            iterator.GetNext(out gridPos);
                        }
                    }
                    this.m_mesh = mesh;
                }
            }
        }

        public MyGridNavigationMesh Mesh =>
            this.m_mesh;

        public bool NoEntry { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct SizeAndCenter
        {
            private Vector3I Size;
            private Vector3I Center;
            public SizeAndCenter(Vector3I size, Vector3I center)
            {
                this.Size = size;
                this.Center = center;
            }

            public bool Equals(MyBlockNavigationDefinition.SizeAndCenter other) => 
                ((other.Size == this.Size) && (other.Center == this.Center));

            public override bool Equals(object obj) => 
                ((obj != null) ? (!(obj.GetType() != typeof(MyBlockNavigationDefinition.SizeAndCenter)) ? this.Equals((MyBlockNavigationDefinition.SizeAndCenter) obj) : false) : false);

            public override int GetHashCode() => 
                ((this.Size.GetHashCode() * 0x60000005) + this.Center.GetHashCode());
        }
    }
}

