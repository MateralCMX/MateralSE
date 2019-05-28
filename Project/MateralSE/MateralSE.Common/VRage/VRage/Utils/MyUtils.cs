namespace VRage.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using VRage;
    using VRage.Library.Utils;
    using VRageMath;

    public static class MyUtils
    {
        private const int HashSeed = -2128831035;
        public static readonly StringBuilder EmptyStringBuilder = new StringBuilder();
        public static readonly Matrix ZeroMatrix = new Matrix(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
        private static readonly string[] BYTE_SIZE_PREFIX = new string[] { "", "K", "M", "G", "T" };
        private static readonly List<char> m_splitBuffer = new List<char>(0x10);
        public const string C_CRLF = "\r\n";
        public const string OPEN_SQUARE_BRACKET = "U+005B";
        public const string CLOSED_SQUARE_BRACKET = "U+005D";
        public static Tuple<string, float>[] DefaultNumberSuffix = new Tuple<string, float>[] { new Tuple<string, float>("k", 1000f), new Tuple<string, float>("m", 1000000f), new Tuple<string, float>("g", 1E+09f), new Tuple<string, float>("b", 1E+09f) };
        [ThreadStatic]
        private static Random m_secretRandom;
        private static byte[] m_randomBuffer = new byte[8];

        private static void AddEdge(int i0, int i1, Dictionary<Edge, int> edgeCounts)
        {
            Edge key = new Edge {
                I0 = i0,
                I1 = i1
            };
            key.GetHashCode();
            if (edgeCounts.ContainsKey(key))
            {
                edgeCounts[key] += 1;
            }
            else
            {
                edgeCounts[key] = 1;
            }
        }

        public static Vector2 AlignCoord(Vector2 coordScreen, Vector2 size, MyGuiDrawAlignEnum drawAlignEnum)
        {
            switch (drawAlignEnum)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return coordScreen;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (coordScreen + (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (coordScreen + (size * new Vector2(0f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (coordScreen + (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return (coordScreen + (size * new Vector2(0.5f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (coordScreen + (size * new Vector2(0.5f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (coordScreen + (size * new Vector2(1f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (coordScreen + (size * new Vector2(1f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (coordScreen + (size * new Vector2(1f, 1f)));
            }
            throw new ArgumentOutOfRangeException("drawAlignEnum", drawAlignEnum, null);
        }

        public static string AlignIntToRight(int value, int charsCount, char ch)
        {
            string str = value.ToString();
            int length = str.Length;
            return ((length <= charsCount) ? (new string(ch, charsCount - length) + str) : str);
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(double f)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Vector3? vec)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(float f)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Matrix matrix)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(MatrixD matrix)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Quaternion q)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Vector2 vec)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Vector3 vec)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValid(Vector3D vec)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertIsValidOrZero(Matrix matrix)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertLengthValid(ref Vector3 vec)
        {
        }

        [Conditional("DEBUG")]
        public static void AssertLengthValid(ref Vector3D vec)
        {
        }

        public static void CheckFloatValues(object graph, string name, ref double? min, ref double? max)
        {
            float num;
            double? nullable;
            double num2;
            int frameCount = new StackTrace().FrameCount;
            if (graph == null)
            {
                return;
            }
            if (graph is float)
            {
                num = (float) graph;
                if (float.IsInfinity(num))
                {
                    goto TR_0001;
                }
                else if (!float.IsNaN(num))
                {
                    if (min != 0)
                    {
                        nullable = min;
                        if (!((num < nullable.GetValueOrDefault()) & (nullable != null)))
                        {
                            goto TR_0029;
                        }
                    }
                    min = new double?((double) num);
                }
                else
                {
                    goto TR_0001;
                }
            }
            else
            {
                goto TR_0025;
            }
            goto TR_0029;
        TR_0001:
            throw new InvalidOperationException("Invalid value: " + name);
        TR_0002:
            throw new InvalidOperationException("Invalid value: " + name);
        TR_0019:
            if ((!graph.GetType().IsPrimitive && !(graph is string)) && !(graph is DateTime))
            {
                if (graph is IEnumerable)
                {
                    using (IEnumerator enumerator = (graph as IEnumerable).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            CheckFloatValues(enumerator.Current, name + "[]", ref min, ref max);
                        }
                    }
                    return;
                }
                foreach (FieldInfo info in graph.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    CheckFloatValues(info.GetValue(graph), name + "." + info.Name, ref min, ref max);
                }
                foreach (PropertyInfo info2 in graph.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    CheckFloatValues(info2.GetValue(graph, null), name + "." + info2.Name, ref min, ref max);
                }
            }
            return;
        TR_001D:
            if (max != 0)
            {
                nullable = max;
                if (!((num2 > nullable.GetValueOrDefault()) & (nullable != null)))
                {
                    goto TR_0019;
                }
            }
            max = new double?(num2);
            goto TR_0019;
        TR_0025:
            if (!(graph is double))
            {
                goto TR_0019;
            }
            else
            {
                num2 = (double) graph;
                if (double.IsInfinity(num2))
                {
                    goto TR_0002;
                }
                else if (!double.IsNaN(num2))
                {
                    if (min != 0)
                    {
                        nullable = min;
                        if (!((num2 < nullable.GetValueOrDefault()) & (nullable != null)))
                        {
                            goto TR_001D;
                        }
                    }
                    min = new double?(num2);
                }
                else
                {
                    goto TR_0002;
                }
            }
            goto TR_001D;
        TR_0029:
            if (max != 0)
            {
                nullable = max;
                if (!((num > nullable.GetValueOrDefault()) & (nullable != null)))
                {
                    goto TR_0025;
                }
            }
            max = new double?((double) num);
            goto TR_0025;
        }

        [Conditional("DEBUG")]
        public static void CheckMainThread()
        {
        }

        public static void CopyDirectory(string source, string destination)
        {
            if (Directory.Exists(source))
            {
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                string[] files = Directory.GetFiles(source);
                for (int i = 0; i < files.Length; i++)
                {
                    string path = files[i];
                    string fileName = Path.GetFileName(path);
                    string destFileName = Path.Combine(destination, fileName);
                    File.Copy(path, destFileName, true);
                }
            }
        }

        public static void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }

        public static void DeserializeValue(XmlReader reader, out Vector3 value)
        {
            Vector3 vector = new Vector3 {
                X = reader.ReadElementContentAsFloat(),
                Y = reader.ReadElementContentAsFloat(),
                Z = reader.ReadElementContentAsFloat()
            };
            value = vector;
        }

        public static void DeserializeValue(XmlReader reader, out Vector4 value)
        {
            Vector4 vector = new Vector4 {
                W = reader.ReadElementContentAsFloat(),
                X = reader.ReadElementContentAsFloat(),
                Y = reader.ReadElementContentAsFloat(),
                Z = reader.ReadElementContentAsFloat()
            };
            value = vector;
        }

        public static string FormatByteSizePrefix(ref double byteSize)
        {
            long num = 1L;
            for (int i = 0; i < BYTE_SIZE_PREFIX.Length; i++)
            {
                num *= 0x400L;
                if (byteSize < num)
                {
                    byteSize /= (double) (num / 0x400L);
                    return BYTE_SIZE_PREFIX[i];
                }
            }
            return string.Empty;
        }

        public static Color[] GenerateBoxColors()
        {
            List<Color> list = new List<Color>();
            float amount = 0f;
            while (amount < 1f)
            {
                float num2 = 0f;
                while (true)
                {
                    if (num2 >= 1f)
                    {
                        amount += 0.2f;
                        break;
                    }
                    float num3 = 0f;
                    while (true)
                    {
                        if (num3 >= 1f)
                        {
                            num2 += 0.33f;
                            break;
                        }
                        float x = MathHelper.Lerp(0.5f, 0.5833333f, amount);
                        float y = MathHelper.Lerp(0.4f, 0.9f, num2);
                        float z = MathHelper.Lerp(0.4f, 1f, num3);
                        list.Add(new Vector3(x, y, z).HSVtoColor());
                        num3 += 0.33f;
                    }
                }
            }
            int? count = null;
            list.ShuffleList<Color>(0, count);
            return list.ToArray();
        }

        public static void GenerateQuad(out MyQuadD quad, ref Vector3D position, float width, float height, ref MatrixD matrix)
        {
            Vector3D vectord = matrix.Left * width;
            Vector3D vectord2 = matrix.Up * height;
            quad.Point0 = (position + vectord) + vectord2;
            quad.Point1 = (position + vectord) - vectord2;
            quad.Point2 = (position - vectord) - vectord2;
            quad.Point3 = (position - vectord) + vectord2;
        }

        public static float GetAngleBetweenVectors(Vector3 vectorA, Vector3 vectorB)
        {
            float num = Vector3.Dot(vectorA, vectorB);
            if ((num > 1f) && (num <= 1.0001f))
            {
                num = 1f;
            }
            if ((num < -1f) && (num >= -1.0001f))
            {
                num = -1f;
            }
            return (float) Math.Acos((double) num);
        }

        public static float GetAngleBetweenVectorsAndNormalise(Vector3 vectorA, Vector3 vectorB) => 
            GetAngleBetweenVectors(Vector3.Normalize(vectorA), Vector3.Normalize(vectorB));

        public static float GetAngleBetweenVectorsForSphereCollision(Vector3 vector1, Vector3 vector2)
        {
            float f = (float) Math.Acos((double) (Vector3.Dot(vector1, vector2) / (vector1.Length() * vector2.Length())));
            return (!float.IsNaN(f) ? f : 0f);
        }

        public static bool GetBillboardQuadAdvancedRotated(out MyQuadD quad, Vector3D position, float radius, float angle, Vector3D cameraPosition) => 
            GetBillboardQuadAdvancedRotated(out quad, position, radius, radius, angle, cameraPosition);

        public static bool GetBillboardQuadAdvancedRotated(out MyQuadD quad, Vector3D position, float radiusX, float radiusY, float angle, Vector3D cameraPosition)
        {
            Vector3D vectord;
            Vector3D vectord2;
            Vector3D forward;
            Vector3D vectord4;
            Vector3D vectord5;
            Vector3D vectord6;
            vectord.X = position.X - cameraPosition.X;
            vectord.Y = position.Y - cameraPosition.Y;
            vectord.Z = position.Z - cameraPosition.Z;
            if (vectord.LengthSquared() <= 9.9999997473787516E-06)
            {
                quad = new MyQuadD();
                return false;
            }
            vectord = Normalize(vectord);
            Vector3D.Reject(ref Vector3D.Up, ref vectord, out vectord2);
            if (vectord2.LengthSquared() <= 9.9999994396249292E-11)
            {
                forward = Vector3D.Forward;
            }
            else
            {
                Normalize(ref vectord2, out forward);
            }
            Vector3D.Cross(ref forward, ref vectord, out vectord4);
            vectord4 = Normalize(vectord4);
            float num = (float) Math.Cos((double) angle);
            float num2 = (float) Math.Sin((double) angle);
            vectord5.X = ((radiusX * num) * vectord4.X) + ((radiusY * num2) * forward.X);
            vectord5.Y = ((radiusX * num) * vectord4.Y) + ((radiusY * num2) * forward.Y);
            vectord5.Z = ((radiusX * num) * vectord4.Z) + ((radiusY * num2) * forward.Z);
            vectord6.X = ((-radiusX * num2) * vectord4.X) + ((radiusY * num) * forward.X);
            vectord6.Y = ((-radiusX * num2) * vectord4.Y) + ((radiusY * num) * forward.Y);
            vectord6.Z = ((-radiusX * num2) * vectord4.Z) + ((radiusY * num) * forward.Z);
            quad.Point0.X = (position.X + vectord5.X) + vectord6.X;
            quad.Point0.Y = (position.Y + vectord5.Y) + vectord6.Y;
            quad.Point0.Z = (position.Z + vectord5.Z) + vectord6.Z;
            quad.Point1.X = (position.X - vectord5.X) + vectord6.X;
            quad.Point1.Y = (position.Y - vectord5.Y) + vectord6.Y;
            quad.Point1.Z = (position.Z - vectord5.Z) + vectord6.Z;
            quad.Point2.X = (position.X - vectord5.X) - vectord6.X;
            quad.Point2.Y = (position.Y - vectord5.Y) - vectord6.Y;
            quad.Point2.Z = (position.Z - vectord5.Z) - vectord6.Z;
            quad.Point3.X = (position.X + vectord5.X) - vectord6.X;
            quad.Point3.Y = (position.Y + vectord5.Y) - vectord6.Y;
            quad.Point3.Z = (position.Z + vectord5.Z) - vectord6.Z;
            return true;
        }

        public static void GetBillboardQuadOriented(out MyQuadD quad, ref Vector3D position, float width, float height, ref Vector3 leftVector, ref Vector3 upVector)
        {
            Vector3D vectord = leftVector * width;
            Vector3D vectord2 = upVector * height;
            quad.Point0 = (position + vectord2) + vectord;
            quad.Point1 = (position + vectord2) - vectord;
            quad.Point2 = (position - vectord2) - vectord;
            quad.Point3 = (position - vectord2) + vectord;
        }

        public static bool? GetBoolFromString(string s)
        {
            bool flag;
            if (bool.TryParse(s, out flag))
            {
                return new bool?(flag);
            }
            return null;
        }

        public static bool GetBoolFromString(string s, bool defaultValue)
        {
            bool? boolFromString = GetBoolFromString(s);
            return ((boolFromString != null) ? boolFromString.GetValueOrDefault() : defaultValue);
        }

        public static unsafe BoundingSphereD GetBoundingSphereFromBoundingBox(ref BoundingBoxD box)
        {
            BoundingSphereD ed;
            ed.Center = (box.Max + box.Min) / 2.0;
            BoundingSphereD* edPtr1 = (BoundingSphereD*) ref ed;
            edPtr1->Radius = Vector3D.Distance(ed.Center, box.Max);
            return ed;
        }

        public static byte? GetByteFromString(string s)
        {
            byte num;
            if (byte.TryParse(s, out num))
            {
                return new byte?(num);
            }
            return null;
        }

        public static Vector3 GetCartesianCoordinatesFromSpherical(float angleHorizontal, float angleVertical, float radius)
        {
            angleVertical = 1.570796f - angleVertical;
            angleHorizontal = 3.141593f - angleHorizontal;
            return new Vector3((float) ((radius * Math.Sin((double) angleVertical)) * Math.Sin((double) angleHorizontal)), (float) (radius * Math.Cos((double) angleVertical)), (float) ((radius * Math.Sin((double) angleVertical)) * Math.Cos((double) angleHorizontal)));
        }

        public static int GetClampInt(int value, int min, int max) => 
            ((value >= min) ? ((value <= max) ? value : max) : min);

        public static Vector3 GetClosestPointOnLine(ref Vector3 linePointA, ref Vector3 linePointB, ref Vector3 point)
        {
            float dist = 0f;
            return GetClosestPointOnLine(ref linePointA, ref linePointB, ref point, out dist);
        }

        public static Vector3D GetClosestPointOnLine(ref Vector3D linePointA, ref Vector3D linePointB, ref Vector3D point)
        {
            double dist = 0.0;
            return GetClosestPointOnLine(ref linePointA, ref linePointB, ref point, out dist);
        }

        public static Vector3 GetClosestPointOnLine(ref Vector3 linePointA, ref Vector3 linePointB, ref Vector3 point, out float dist)
        {
            Vector3 vector2 = Normalize(linePointB - linePointA);
            float num = Vector3.Distance(linePointA, linePointB);
            float num2 = Vector3.Dot(vector2, point - linePointA);
            dist = num2;
            if (num2 <= 0f)
            {
                return linePointA;
            }
            if (num2 >= num)
            {
                return linePointB;
            }
            Vector3 vector3 = vector2 * num2;
            return (linePointA + vector3);
        }

        public static Vector3D GetClosestPointOnLine(ref Vector3D linePointA, ref Vector3D linePointB, ref Vector3D point, out double dist)
        {
            Vector3D vectord2 = Normalize(linePointB - linePointA);
            double num = Vector3D.Distance(linePointA, linePointB);
            double num2 = Vector3D.Dot(vectord2, point - linePointA);
            dist = num2;
            if (num2 <= 0.0)
            {
                return linePointA;
            }
            if (num2 >= num)
            {
                return linePointB;
            }
            Vector3D vectord3 = vectord2 * num2;
            return (linePointA + vectord3);
        }

        public static Vector2 GetCoordAligned(Vector2 coordScreen, Vector2 size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return coordScreen;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (coordScreen - (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (coordScreen - (size * new Vector2(0f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (coordScreen - (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return (coordScreen - (size * 0.5f));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (coordScreen - (size * new Vector2(0.5f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (coordScreen - (size * new Vector2(1f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (coordScreen - (size * new Vector2(1f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (coordScreen - size);
            }
            throw new InvalidBranchException();
        }

        public static Vector2 GetCoordAlignedFromCenter(Vector2 coordCenter, Vector2 size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return (coordCenter + (size * new Vector2(-0.5f, -0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (coordCenter + (size * new Vector2(-0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (coordCenter + (size * new Vector2(-0.5f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (coordCenter + (size * new Vector2(0f, -0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return coordCenter;

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (coordCenter + (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (coordCenter + (size * new Vector2(0.5f, -0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (coordCenter + (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (coordCenter + (size * new Vector2(0.5f, 0.5f)));
            }
            throw new InvalidBranchException();
        }

        public static Vector2 GetCoordAlignedFromRectangle(ref RectangleF rect, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return rect.Position;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (rect.Position + (rect.Size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (rect.Position + (rect.Size * new Vector2(0f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (rect.Position + (rect.Size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return (rect.Position + (rect.Size * 0.5f));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (rect.Position + (rect.Size * new Vector2(0.5f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (rect.Position + (rect.Size * new Vector2(1f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (rect.Position + (rect.Size * new Vector2(1f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (rect.Position + (rect.Size * 1f));
            }
            throw new InvalidBranchException();
        }

        public static Vector2 GetCoordAlignedFromTopLeft(Vector2 topLeft, Vector2 size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return topLeft;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (topLeft + (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (topLeft + (size * new Vector2(0f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (topLeft + (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return (topLeft + (size * new Vector2(0.5f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (topLeft + (size * new Vector2(0.5f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (topLeft + (size * new Vector2(1f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (topLeft + (size * new Vector2(1f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (topLeft + (size * new Vector2(1f, 1f)));
            }
            return topLeft;
        }

        public static Vector2 GetCoordCenterFromAligned(Vector2 alignedCoord, Vector2 size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return (alignedCoord + (size * 0.5f));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (alignedCoord + (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (alignedCoord + (size * new Vector2(0.5f, -0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (alignedCoord + (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return alignedCoord;

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (alignedCoord - (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (alignedCoord + (size * new Vector2(-0.5f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (alignedCoord - (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (alignedCoord - (size * 0.5f));
            }
            throw new InvalidBranchException();
        }

        public static Vector2 GetCoordTopLeftFromAligned(Vector2 alignedCoord, Vector2 size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return alignedCoord;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return (alignedCoord - (size * new Vector2(0f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return (alignedCoord - (size * new Vector2(0f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return (alignedCoord - (size * new Vector2(0.5f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return (alignedCoord - (size * 0.5f));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return (alignedCoord - (size * new Vector2(0.5f, 1f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return (alignedCoord - (size * new Vector2(1f, 0f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return (alignedCoord - (size * new Vector2(1f, 0.5f)));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return (alignedCoord - size);
            }
            throw new InvalidBranchException();
        }

        public static Vector2I GetCoordTopLeftFromAligned(Vector2I alignedCoord, Vector2I size, MyGuiDrawAlignEnum drawAlign)
        {
            switch (drawAlign)
            {
                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP:
                    return alignedCoord;

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER:
                    return new Vector2I(alignedCoord.X, alignedCoord.Y - (size.Y / 2));

                case MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM:
                    return new Vector2I(alignedCoord.X, alignedCoord.Y - size.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP:
                    return new Vector2I(alignedCoord.X - (size.X / 2), alignedCoord.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER:
                    return new Vector2I(alignedCoord.X - (size.X / 2), alignedCoord.Y - (size.Y / 2));

                case MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM:
                    return new Vector2I(alignedCoord.X - (size.X / 2), alignedCoord.Y - size.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP:
                    return new Vector2I(alignedCoord.X - size.X, alignedCoord.Y);

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER:
                    return new Vector2I(alignedCoord.X - size.X, alignedCoord.Y - (size.Y / 2));

                case MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM:
                    return new Vector2I(alignedCoord.X - size.X, alignedCoord.Y - size.Y);
            }
            throw new InvalidBranchException();
        }

        public static Vector3? GetEdgeSphereCollision(ref Vector3 sphereCenter, float sphereRadius, ref MyTriangle_Vertices triangle)
        {
            Vector3 vector = GetClosestPointOnLine(ref triangle.Vertex0, ref triangle.Vertex1, ref sphereCenter);
            if (Vector3.Distance(vector, sphereCenter) < sphereRadius)
            {
                return new Vector3?(vector);
            }
            vector = GetClosestPointOnLine(ref triangle.Vertex1, ref triangle.Vertex2, ref sphereCenter);
            if (Vector3.Distance(vector, sphereCenter) < sphereRadius)
            {
                return new Vector3?(vector);
            }
            vector = GetClosestPointOnLine(ref triangle.Vertex2, ref triangle.Vertex0, ref sphereCenter);
            if (Vector3.Distance(vector, sphereCenter) < sphereRadius)
            {
                return new Vector3?(vector);
            }
            return null;
        }

        public static float? GetFloatFromString(string s)
        {
            float num;
            if (float.TryParse(s, NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out num))
            {
                return new float?(num);
            }
            return null;
        }

        public static float GetFloatFromString(string s, float defaultValue)
        {
            float? floatFromString = GetFloatFromString(s);
            return ((floatFromString == null) ? defaultValue : floatFromString.Value);
        }

        public static unsafe int GetHash(double d, int hash = -2128831035)
        {
            if (d != 0.0)
            {
                ulong num = *((ulong*) &d);
                int num1 = HashStep((int) num, HashStep((int) (num >> 0x20), hash));
                hash = num1;
            }
            return hash;
        }

        public static int GetHash(string str, int hash = -2128831035)
        {
            if (str != null)
            {
                int num = 0;
                while (true)
                {
                    if (num >= (str.Length - 1))
                    {
                        if ((str.Length & 1) != 0)
                        {
                            int num2 = HashStep(str[num], hash);
                            hash = num2;
                        }
                        break;
                    }
                    int num1 = HashStep((str[num] << 0x10) + str[num + 1], hash);
                    hash = num1;
                    num += 2;
                }
            }
            return hash;
        }

        public static int GetHash(string str, int start, int length, int hash = -2128831035)
        {
            if (str == null)
            {
                return 0;
            }
            if (length < 0)
            {
                length = str.Length - start;
            }
            if (length <= 0)
            {
                return 0;
            }
            int num = (start + length) - 1;
            int num2 = start;
            while (num2 < num)
            {
                int num1 = HashStep((str[num2] << 0x10) + str[num2 + 1], hash);
                hash = num1;
                num2 += 2;
            }
            if ((length & 1) != 0)
            {
                int num3 = HashStep(str[num2], hash);
                hash = num3;
            }
            return hash;
        }

        public static int GetHashUpperCase(string str, int start, int length, int hash = -2128831035)
        {
            if (str == null)
            {
                return 0;
            }
            if (length < 0)
            {
                length = str.Length - start;
            }
            if (length <= 0)
            {
                return 0;
            }
            int num = (start + length) - 1;
            int num2 = start;
            while (num2 < num)
            {
                int num1 = HashStep((char.ToUpperInvariant(str[num2]) << 0x10) + char.ToUpperInvariant(str[num2 + 1]), hash);
                hash = num1;
                num2 += 2;
            }
            if ((length & 1) != 0)
            {
                int num3 = HashStep(char.ToUpperInvariant(str[num2]), hash);
                hash = num3;
            }
            return hash;
        }

        public static bool GetInsidePolygonForSphereCollision(ref Vector3 point, ref MyTriangle_Vertices triangle) => 
            ((((0f + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex0 - point, triangle.Vertex1 - point)) + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex1 - point, triangle.Vertex2 - point)) + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex2 - point, triangle.Vertex0 - point)) >= 6.2203541591948124);

        public static bool GetInsidePolygonForSphereCollision(ref Vector3D point, ref MyTriangle_Vertices triangle) => 
            ((((0f + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex0 - point, triangle.Vertex1 - point)) + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex1 - point, triangle.Vertex2 - point)) + GetAngleBetweenVectorsForSphereCollision(triangle.Vertex2 - point, triangle.Vertex0 - point)) >= 6.2203541591948124);

        public static int? GetInt32FromString(string s)
        {
            int num;
            if (int.TryParse(s, out num))
            {
                return new int?(num);
            }
            return null;
        }

        public static int? GetIntFromString(string s)
        {
            int num;
            if (int.TryParse(s, out num))
            {
                return new int?(num);
            }
            return null;
        }

        public static int GetIntFromString(string s, int defaultValue)
        {
            int? intFromString = GetIntFromString(s);
            return ((intFromString == null) ? defaultValue : intFromString.Value);
        }

        public static double GetLargestDistanceToSphere(ref Vector3D from, ref BoundingSphereD sphere) => 
            (Vector3D.Distance(from, sphere.Center) + sphere.Radius);

        public static float? GetLineBoundingBoxIntersection(ref Line line, ref BoundingBox boundingBox)
        {
            Ray ray = new Ray(line.From, line.Direction);
            float? nullable = boundingBox.Intersects(ray);
            if ((nullable != null) && (nullable.Value <= line.Length))
            {
                return new float?(nullable.Value);
            }
            return null;
        }

        public static float? GetLineTriangleIntersection(ref Line line, ref MyTriangle_Vertices triangle)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            float num;
            Vector3.Subtract(ref triangle.Vertex1, ref triangle.Vertex0, out vector);
            Vector3.Subtract(ref triangle.Vertex2, ref triangle.Vertex0, out vector2);
            Vector3.Cross(ref line.Direction, ref vector2, out vector3);
            Vector3.Dot(ref vector, ref vector3, out num);
            if ((num <= -1.401298E-45f) || (num >= float.Epsilon))
            {
                Vector3 vector4;
                float num3;
                Vector3 vector5;
                float num4;
                float num5;
                float num2 = 1f / num;
                Vector3.Subtract(ref line.From, ref triangle.Vertex0, out vector4);
                Vector3.Dot(ref vector4, ref vector3, out num3);
                num3 *= num2;
                if ((num3 < 0f) || (num3 > 1f))
                {
                    return null;
                }
                Vector3.Cross(ref vector4, ref vector, out vector5);
                Vector3.Dot(ref line.Direction, ref vector5, out num4);
                num4 *= num2;
                if ((num4 < 0f) || ((num3 + num4) > 1f))
                {
                    return null;
                }
                Vector3.Dot(ref vector2, ref vector5, out num5);
                num5 *= num2;
                if (num5 < 0f)
                {
                    return null;
                }
                if (num5 <= line.Length)
                {
                    return new float?(num5);
                }
            }
            return null;
        }

        public static int GetMaxValueFromEnum<T>()
        {
            Array values = Enum.GetValues(typeof(T));
            int num = -2147483648;
            Type underlyingType = Enum.GetUnderlyingType(typeof(T));
            if (underlyingType == typeof(byte))
            {
                foreach (byte num2 in values)
                {
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                return num;
            }
            if (underlyingType == typeof(short))
            {
                foreach (short num3 in values)
                {
                    if (num3 > num)
                    {
                        num = num3;
                    }
                }
                return num;
            }
            if (underlyingType == typeof(ushort))
            {
                foreach (ushort num4 in values)
                {
                    if (num4 > num)
                    {
                        num = num4;
                    }
                }
                return num;
            }
            if (!(underlyingType == typeof(int)))
            {
                throw new InvalidBranchException();
            }
            else
            {
                foreach (int num5 in values)
                {
                    if (num5 > num)
                    {
                        num = num5;
                    }
                }
                return num;
            }
            return num;
        }

        public static Vector3 GetNormalVectorFromTriangle(ref MyTriangle_Vertices inputTriangle) => 
            Vector3.Normalize(Vector3.Cross(inputTriangle.Vertex2 - inputTriangle.Vertex0, inputTriangle.Vertex1 - inputTriangle.Vertex0));

        public static void GetOpenBoundaries(Vector3[] vertices, int[] indices, List<Vector3> openBoundaries)
        {
            Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
            int index = 0;
            while (index < vertices.Length)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 < index)
                    {
                        if (!IsEqual(vertices[num2], vertices[index]))
                        {
                            num2++;
                            continue;
                        }
                        if (!dictionary.ContainsKey(num2))
                        {
                            dictionary[num2] = new List<int>();
                        }
                        dictionary[num2].Add(index);
                    }
                    index++;
                    break;
                }
            }
            foreach (KeyValuePair<int, List<int>> pair in dictionary)
            {
                foreach (int num3 in pair.Value)
                {
                    for (int j = 0; j < indices.Length; j++)
                    {
                        if (indices[j] == num3)
                        {
                            indices[j] = pair.Key;
                        }
                    }
                }
            }
            Dictionary<Edge, int> edgeCounts = new Dictionary<Edge, int>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                AddEdge(indices[i], indices[i + 1], edgeCounts);
                AddEdge(indices[i + 1], indices[i + 2], edgeCounts);
                AddEdge(indices[i + 2], indices[i], edgeCounts);
            }
            openBoundaries.Clear();
            foreach (KeyValuePair<Edge, int> pair2 in edgeCounts)
            {
                if (pair2.Value == 1)
                {
                    openBoundaries.Add(vertices[pair2.Key.I0]);
                    openBoundaries.Add(vertices[pair2.Key.I1]);
                }
            }
        }

        public static double GetPointLineDistance(ref Vector3D linePointA, ref Vector3D linePointB, ref Vector3D point)
        {
            Vector3D vectord = linePointB - linePointA;
            return (Vector3D.Cross(vectord, point - linePointA).Length() / vectord.Length());
        }

        public static void GetPolyLineQuad(out MyQuadD retQuad, ref MyPolyLineD polyLine, Vector3D cameraPosition)
        {
            Vector3D vectord = Normalize(cameraPosition - polyLine.Point0);
            Vector3D vectord2 = GetVector3Scaled(Vector3D.Cross(polyLine.LineDirectionNormalized, vectord), polyLine.Thickness);
            retQuad.Point0 = polyLine.Point0 - vectord2;
            retQuad.Point1 = polyLine.Point1 - vectord2;
            retQuad.Point2 = polyLine.Point1 + vectord2;
            retQuad.Point3 = polyLine.Point0 + vectord2;
        }

        public static Vector3 GetRandomBorderPosition(ref BoundingBox box) => 
            ((Vector3) GetRandomBorderPosition(ref (ref BoundingBoxD) ref box));

        public static Vector3D GetRandomBorderPosition(ref BoundingBoxD box)
        {
            Vector3D size = box.Size;
            double num = 2.0 / box.SurfaceArea;
            double num2 = (size.X * size.Y) * num;
            double num3 = (size.X * size.Z) * num;
            double num4 = (1.0 - num2) - num3;
            double num5 = Instance.NextDouble();
            if (num5 < num2)
            {
                size.Z = (num5 >= (num2 * 0.5)) ? box.Max.Z : box.Min.Z;
                size.X = GetRandomDouble(box.Min.X, box.Max.X);
                size.Y = GetRandomDouble(box.Min.Y, box.Max.Y);
                return size;
            }
            num5 -= num2;
            if (num5 < num3)
            {
                size.Y = (num5 >= (num3 * 0.5)) ? box.Max.Y : box.Min.Y;
                size.X = GetRandomDouble(box.Min.X, box.Max.X);
                size.Z = GetRandomDouble(box.Min.Z, box.Max.Z);
                return size;
            }
            size.X = ((num5 - num4) >= (num4 * 0.5)) ? box.Max.X : box.Min.X;
            size.Y = GetRandomDouble(box.Min.Y, box.Max.Y);
            size.Z = GetRandomDouble(box.Min.Z, box.Max.Z);
            return size;
        }

        public static Vector3 GetRandomBorderPosition(ref BoundingSphere sphere) => 
            (sphere.Center + (GetRandomVector3Normalized() * sphere.Radius));

        public static Vector3D GetRandomBorderPosition(ref BoundingSphereD sphere) => 
            (sphere.Center + (GetRandomVector3Normalized() * ((float) sphere.Radius)));

        public static Vector3D GetRandomDiscPosition(ref Vector3D center, double radius, ref Vector3D tangent, ref Vector3D bitangent)
        {
            double num = Math.Sqrt((GetRandomDouble(0.0, 1.0) * radius) * radius);
            double randomDouble = GetRandomDouble(0.0, 6.2831859588623047);
            return (center + (num * ((Math.Cos(randomDouble) * tangent) + (Math.Sin(randomDouble) * bitangent))));
        }

        public static Vector3D GetRandomDiscPosition(ref Vector3D center, double minRadius, double maxRadius, ref Vector3D tangent, ref Vector3D bitangent)
        {
            double num = Math.Sqrt(GetRandomDouble(minRadius * minRadius, maxRadius * maxRadius));
            double randomDouble = GetRandomDouble(0.0, 6.2831859588623047);
            return (center + (num * ((Math.Cos(randomDouble) * tangent) + (Math.Sin(randomDouble) * bitangent))));
        }

        public static double GetRandomDouble(double minValue, double maxValue) => 
            ((Instance.NextDouble() * (maxValue - minValue)) + minValue);

        public static float GetRandomFloat() => 
            ((float) Instance.NextDouble());

        public static float GetRandomFloat(float minValue, float maxValue) => 
            ((MyRandom.Instance.NextFloat() * (maxValue - minValue)) + minValue);

        public static int GetRandomInt(int maxValue) => 
            Instance.Next(maxValue);

        public static int GetRandomInt(int minValue, int maxValue) => 
            Instance.Next(minValue, maxValue);

        public static T GetRandomItem<T>(this T[] list) => 
            list[GetRandomInt(list.Length)];

        public static T GetRandomItemFromList<T>(this List<T> list) => 
            list[GetRandomInt(list.Count)];

        public static long GetRandomLong()
        {
            Instance.NextBytes(m_randomBuffer);
            return BitConverter.ToInt64(m_randomBuffer, 0);
        }

        public static Vector3 GetRandomPerpendicularVector(ref Vector3 axis) => 
            ((Vector3) GetRandomPerpendicularVector(ref (ref Vector3D) ref axis));

        public static Vector3D GetRandomPerpendicularVector(ref Vector3D axis)
        {
            Vector3D vectord2;
            Vector3D vectord = Vector3D.CalculatePerpendicularVector((Vector3D) axis);
            Vector3D.Cross(ref axis, ref vectord, out vectord2);
            double randomDouble = GetRandomDouble(0.0, 6.2831859588623047);
            return (Vector3D) ((Math.Cos(randomDouble) * vectord) + (Math.Sin(randomDouble) * vectord2));
        }

        public static Vector3 GetRandomPosition(ref BoundingBox box) => 
            (box.Center + (GetRandomVector3() * box.HalfExtents));

        public static Vector3D GetRandomPosition(ref BoundingBoxD box) => 
            (box.Center + (GetRandomVector3() * box.HalfExtents));

        public static float GetRandomRadian() => 
            GetRandomFloat(0f, 6.283186f);

        public static float GetRandomSign() => 
            ((float) Math.Sign((float) (((float) Instance.NextDouble()) - 0.5f)));

        public static TimeSpan GetRandomTimeSpan(TimeSpan begin, TimeSpan end)
        {
            long randomLong = GetRandomLong();
            return new TimeSpan(begin.Ticks + (randomLong % (end.Ticks - begin.Ticks)));
        }

        public static Vector3 GetRandomVector3() => 
            new Vector3(GetRandomFloat(-1f, 1f), GetRandomFloat(-1f, 1f), GetRandomFloat(-1f, 1f));

        public static Vector3 GetRandomVector3CircleNormalized()
        {
            float randomRadian = GetRandomRadian();
            return new Vector3((float) Math.Sin((double) randomRadian), 0f, (float) Math.Cos((double) randomRadian));
        }

        public static Vector3D GetRandomVector3D() => 
            new Vector3D(GetRandomDouble(-1.0, 1.0), GetRandomDouble(-1.0, 1.0), GetRandomDouble(-1.0, 1.0));

        public static Vector3 GetRandomVector3HemisphereNormalized(Vector3 normal)
        {
            Vector3 vector = GetRandomVector3Normalized();
            return ((Vector3.Dot(vector, normal) >= 0f) ? vector : -vector);
        }

        public static Vector3 GetRandomVector3MaxAngle(float maxAngle)
        {
            float randomFloat = GetRandomFloat(-maxAngle, maxAngle);
            float angle = GetRandomFloat(0f, 6.283185f);
            return -new Vector3(MyMath.FastSin(randomFloat) * MyMath.FastCos(angle), MyMath.FastSin(randomFloat) * MyMath.FastSin(angle), MyMath.FastCos(randomFloat));
        }

        public static Vector3 GetRandomVector3Normalized()
        {
            float randomRadian = GetRandomRadian();
            float randomFloat = GetRandomFloat(-1f, 1f);
            float num3 = (float) Math.Sqrt(1.0 - (randomFloat * randomFloat));
            return new Vector3(num3 * Math.Cos((double) randomRadian), num3 * Math.Sin((double) randomRadian), (double) randomFloat);
        }

        public static double GetSmallestDistanceToSphere(ref Vector3D from, ref BoundingSphereD sphere) => 
            (Vector3D.Distance(from, sphere.Center) - sphere.Radius);

        public static double GetSmallestDistanceToSphereAlwaysPositive(ref Vector3D from, ref BoundingSphereD sphere)
        {
            double smallestDistanceToSphere = GetSmallestDistanceToSphere(ref from, ref sphere);
            if (smallestDistanceToSphere < 0.0)
            {
                smallestDistanceToSphere = 0.0;
            }
            return smallestDistanceToSphere;
        }

        public static MySpherePlaneIntersectionEnum GetSpherePlaneIntersection(ref BoundingSphere sphere, ref Plane plane, out float distanceFromPlaneToSphere)
        {
            float d = plane.D;
            distanceFromPlaneToSphere = (((plane.Normal.X * sphere.Center.X) + (plane.Normal.Y * sphere.Center.Y)) + (plane.Normal.Z * sphere.Center.Z)) + d;
            return ((Math.Abs(distanceFromPlaneToSphere) >= sphere.Radius) ? ((distanceFromPlaneToSphere < sphere.Radius) ? MySpherePlaneIntersectionEnum.BEHIND : MySpherePlaneIntersectionEnum.FRONT) : MySpherePlaneIntersectionEnum.INTERSECTS);
        }

        public static MySpherePlaneIntersectionEnum GetSpherePlaneIntersection(ref BoundingSphereD sphere, ref PlaneD plane, out double distanceFromPlaneToSphere)
        {
            double d = plane.D;
            distanceFromPlaneToSphere = (((plane.Normal.X * sphere.Center.X) + (plane.Normal.Y * sphere.Center.Y)) + (plane.Normal.Z * sphere.Center.Z)) + d;
            return ((Math.Abs(distanceFromPlaneToSphere) >= sphere.Radius) ? ((distanceFromPlaneToSphere < sphere.Radius) ? MySpherePlaneIntersectionEnum.BEHIND : MySpherePlaneIntersectionEnum.FRONT) : MySpherePlaneIntersectionEnum.INTERSECTS);
        }

        public static Vector3? GetSphereTriangleIntersection(ref BoundingSphere sphere, ref Plane trianglePlane, ref MyTriangle_Vertices triangle)
        {
            float num;
            if (GetSpherePlaneIntersection(ref sphere, ref trianglePlane, out num) == MySpherePlaneIntersectionEnum.INTERSECTS)
            {
                Vector3 vector2;
                Vector3 vector = trianglePlane.Normal * num;
                vector2.X = sphere.Center.X - vector.X;
                vector2.Y = sphere.Center.Y - vector.Y;
                vector2.Z = sphere.Center.Z - vector.Z;
                if (GetInsidePolygonForSphereCollision(ref vector2, ref triangle))
                {
                    return new Vector3?(vector2);
                }
                Vector3? nullable = GetEdgeSphereCollision(ref sphere.Center, sphere.Radius / 1f, ref triangle);
                if (nullable != null)
                {
                    return new Vector3?(nullable.Value);
                }
            }
            return null;
        }

        public static Vector3D GetTransformNormalNormalized(Vector3D vec, ref MatrixD matrix)
        {
            Vector3D vectord;
            Vector3D.TransformNormal(ref vec, ref matrix, out vectord);
            return Normalize(vectord);
        }

        public static Vector3D GetVector3Scaled(Vector3D originalVector, float newLength)
        {
            if (newLength == 0f)
            {
                return Vector3D.Zero;
            }
            double num = originalVector.Length();
            if (num == 0.0)
            {
                return Vector3D.Zero;
            }
            double num2 = ((double) newLength) / num;
            return new Vector3D(originalVector.X * num2, originalVector.Y * num2, originalVector.Z * num2);
        }

        private static int HashStep(int value, int hash)
        {
            hash ^= value;
            hash *= 0x1000193;
            return hash;
        }

        public static bool HasValidLength(Vector3 vec) => 
            (vec.Length() > 1E-06f);

        public static bool HasValidLength(Vector3D vec) => 
            (vec.Length() > 9.9999999747524271E-07);

        public static T Init<T>(ref T location) where T: class, new()
        {
            ref T localRef1 = location;
            ref T localRef2 = localRef1;
            if (((T) localRef1) == null)
            {
                ref T local1 = localRef1;
                localRef2 = location = Activator.CreateInstance<T>();
            }
            return localRef2;
        }

        public static void InterlockedMax(ref long storage, long value)
        {
            for (long i = Interlocked.Read(ref storage); value > i; i = Interlocked.Read(ref storage))
            {
                Interlocked.CompareExchange(ref storage, value, i);
            }
        }

        public static bool IsEqual(float value1, float value2) => 
            IsZero((float) (value1 - value2), 1E-05f);

        public static bool IsEqual(Matrix value1, Matrix value2) => 
            (IsZero(value1.Left - value2.Left, 1E-05f) && (IsZero(value1.Up - value2.Up, 1E-05f) && (IsZero(value1.Forward - value2.Forward, 1E-05f) && IsZero(value1.Translation - value2.Translation, 1E-05f))));

        public static bool IsEqual(Quaternion value1, Quaternion value2) => 
            (IsZero((float) (value1.X - value2.X), 1E-05f) && (IsZero((float) (value1.Y - value2.Y), 1E-05f) && (IsZero((float) (value1.Z - value2.Z), 1E-05f) && IsZero((float) (value1.W - value2.W), 1E-05f))));

        public static bool IsEqual(QuaternionD value1, QuaternionD value2) => 
            (IsZero((double) (value1.X - value2.X), 1E-05f) && (IsZero((double) (value1.Y - value2.Y), 1E-05f) && (IsZero((double) (value1.Z - value2.Z), 1E-05f) && IsZero((double) (value1.W - value2.W), 1E-05f))));

        public static bool IsEqual(Vector2 value1, Vector2 value2) => 
            (IsZero((float) (value1.X - value2.X), 1E-05f) && IsZero((float) (value1.Y - value2.Y), 1E-05f));

        public static bool IsEqual(Vector3 value1, Vector3 value2) => 
            (IsZero((float) (value1.X - value2.X), 1E-05f) && (IsZero((float) (value1.Y - value2.Y), 1E-05f) && IsZero((float) (value1.Z - value2.Z), 1E-05f)));

        public static bool IsLineIntersectingBoundingSphere(ref LineD line, ref BoundingSphereD boundingSphere)
        {
            RayD ray = new RayD(ref line.From, ref line.Direction);
            double? nullable = boundingSphere.Intersects(ray);
            return ((nullable != null) ? (nullable.Value <= line.Length) : false);
        }

        public static bool IsValid(double f) => 
            (!double.IsNaN(f) && !double.IsInfinity(f));

        public static bool IsValid(Vector3? vec) => 
            ((vec == null) || (IsValid(vec.Value.X) && (IsValid(vec.Value.Y) && IsValid(vec.Value.Z))));

        public static bool IsValid(float f) => 
            (!float.IsNaN(f) && !float.IsInfinity(f));

        public static bool IsValid(Matrix matrix)
        {
            if ((!matrix.Up.IsValid() || !matrix.Left.IsValid()) || !matrix.Forward.IsValid())
            {
                return false;
            }
            return (matrix.Translation.IsValid() && (matrix != Matrix.Zero));
        }

        public static bool IsValid(MatrixD matrix)
        {
            if ((!matrix.Up.IsValid() || !matrix.Left.IsValid()) || !matrix.Forward.IsValid())
            {
                return false;
            }
            return (matrix.Translation.IsValid() && (matrix != MatrixD.Zero));
        }

        public static bool IsValid(Quaternion q) => 
            (IsValid(q.X) && (IsValid(q.Y) && (IsValid(q.Z) && (IsValid(q.W) && !IsZero(q, 1E-05f)))));

        public static bool IsValid(Vector2 vec) => 
            (IsValid(vec.X) && IsValid(vec.Y));

        public static bool IsValid(Vector3 vec) => 
            (IsValid(vec.X) && (IsValid(vec.Y) && IsValid(vec.Z)));

        public static bool IsValid(Vector3D vec) => 
            (IsValid(vec.X) && (IsValid(vec.Y) && IsValid(vec.Z)));

        public static bool IsValidNormal(Vector3 vec)
        {
            float num = vec.LengthSquared();
            return (vec.IsValid() && ((num > 0.999f) && (num < 1.001f)));
        }

        public static bool IsValidOrZero(Matrix matrix) => 
            (IsValid(matrix.Up) && (IsValid(matrix.Left) && (IsValid(matrix.Forward) && IsValid(matrix.Translation))));

        public static bool IsWrongTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2) => 
            (((vertex2 - vertex0).LengthSquared() <= 9.999999E-11f) || (((vertex1 - vertex0).LengthSquared() <= 9.999999E-11f) || ((vertex1 - vertex2).LengthSquared() <= 9.999999E-11f)));

        public static bool IsZero(Vector4 value) => 
            (IsZero(value.X, 1E-05f) && (IsZero(value.Y, 1E-05f) && (IsZero(value.Z, 1E-05f) && IsZero(value.W, 1E-05f))));

        public static bool IsZero(double value, float epsilon = 1E-05f) => 
            ((value > -epsilon) && (value < epsilon));

        public static bool IsZero(float value, float epsilon = 1E-05f) => 
            ((value > -epsilon) && (value < epsilon));

        public static bool IsZero(Vector3 value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && IsZero(value.Z, epsilon)));

        public static bool IsZero(ref Quaternion value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && (IsZero(value.Z, epsilon) && IsZero(value.W, epsilon))));

        public static bool IsZero(ref Vector3 value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && IsZero(value.Z, epsilon)));

        public static bool IsZero(Vector3D value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && IsZero(value.Z, epsilon)));

        public static bool IsZero(ref Vector3D value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && IsZero(value.Z, epsilon)));

        public static bool IsZero(Quaternion value, float epsilon = 1E-05f) => 
            (IsZero(value.X, epsilon) && (IsZero(value.Y, epsilon) && (IsZero(value.Z, epsilon) && IsZero(value.W, epsilon))));

        public static Vector3D LinePlaneIntersection(Vector3D planePoint, Vector3 planeNormal, Vector3D lineStart, Vector3 lineDir)
        {
            double num = Vector3D.Dot(planePoint - lineStart, planeNormal);
            return (lineStart + (lineDir * (num / ((double) Vector3.Dot(lineDir, planeNormal)))));
        }

        public static Vector3 Normalize(Vector3 vec) => 
            Vector3.Normalize(vec);

        public static Vector3D Normalize(Vector3D vec) => 
            Vector3D.Normalize(vec);

        public static void Normalize(ref Matrix m, out Matrix normalized)
        {
            normalized = Matrix.CreateWorld(m.Translation, Normalize(m.Forward), Normalize(m.Up));
        }

        public static void Normalize(ref MatrixD m, out MatrixD normalized)
        {
            normalized = MatrixD.CreateWorld(m.Translation, Normalize(m.Forward), Normalize(m.Up));
        }

        public static void Normalize(ref Vector3 vec, out Vector3 normalized)
        {
            Vector3.Normalize(ref vec, out normalized);
        }

        public static void Normalize(ref Vector3D vec, out Vector3D normalized)
        {
            Vector3D.Normalize(ref vec, out normalized);
        }

        public static TCollection PrepareCollection<TCollection, TElement>(ref TCollection collection) where TCollection: class, ICollection<TElement>, new()
        {
            if (((TCollection) collection) == null)
            {
                collection = Activator.CreateInstance<TCollection>();
            }
            else if (collection.Count != 0)
            {
                collection.Clear();
            }
            return collection;
        }

        public static ClearCollectionToken<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> ReuseCollection<TKey, TValue>(ref Dictionary<TKey, TValue> collection) => 
            ReuseCollection<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>(ref collection);

        public static ClearCollectionToken<HashSet<TElement>, TElement> ReuseCollection<TElement>(ref HashSet<TElement> collection) => 
            ReuseCollection<HashSet<TElement>, TElement>(ref collection);

        public static ClearCollectionToken<List<TElement>, TElement> ReuseCollection<TElement>(ref List<TElement> collection) => 
            ReuseCollection<List<TElement>, TElement>(ref collection);

        public static ClearCollectionToken<TCollection, TElement> ReuseCollection<TCollection, TElement>(ref TCollection collection) where TCollection: class, ICollection<TElement>, new()
        {
            PrepareCollection<TCollection, TElement>(ref collection);
            return new ClearCollectionToken<TCollection, TElement>(collection);
        }

        public static ClearRangeToken<TElement> ReuseCollectionNested<TElement>(ref List<TElement> collection)
        {
            if (collection == null)
            {
                collection = new List<TElement>();
            }
            return new ClearRangeToken<TElement>(collection);
        }

        public static void RotationMatrixToYawPitchRoll(ref Matrix mx, out float yaw, out float pitch, out float roll)
        {
            float num = mx.M32;
            if (num > 1f)
            {
                num = 1f;
            }
            else if (num < -1f)
            {
                num = -1f;
            }
            pitch = (float) Math.Asin((double) -num);
            float num2 = 0.001f;
            if (((float) Math.Cos((double) pitch)) > num2)
            {
                roll = (float) Math.Atan2((double) mx.M12, (double) mx.M22);
                yaw = (float) Math.Atan2((double) mx.M31, (double) mx.M33);
            }
            else
            {
                roll = (float) Math.Atan2((double) -mx.M21, (double) mx.M11);
                yaw = 0f;
            }
        }

        public static void SerializeValue(XmlWriter writer, Vector3 v)
        {
            string[] textArray1 = new string[] { v.X.ToString(CultureInfo.InvariantCulture), " ", v.Y.ToString(CultureInfo.InvariantCulture), " ", v.Z.ToString(CultureInfo.InvariantCulture) };
            writer.WriteValue(string.Concat(textArray1));
        }

        public static void SerializeValue(XmlWriter writer, Vector4 v)
        {
            string[] textArray1 = new string[] { v.X.ToString(CultureInfo.InvariantCulture), " ", v.Y.ToString(CultureInfo.InvariantCulture), " ", v.Z.ToString(CultureInfo.InvariantCulture), " ", v.W.ToString(CultureInfo.InvariantCulture) };
            writer.WriteValue(string.Concat(textArray1));
        }

        public static void ShuffleList<T>(this IList<T> list, int offset = 0, int? count = new int?())
        {
            int? nullable = count;
            int num = (nullable != null) ? nullable.GetValueOrDefault() : (list.Count - offset);
            while (num > 1)
            {
                num--;
                int randomInt = GetRandomInt(num + 1);
                T local = list[offset + randomInt];
                list[offset + randomInt] = list[offset + num];
                list[offset + num] = local;
            }
        }

        public static void SplitStringBuilder(StringBuilder destination, StringBuilder source, string splitSeparator)
        {
            int length = source.Length;
            int num2 = splitSeparator.Length;
            int num3 = 0;
            for (int i = 0; i < length; i++)
            {
                char item = source[i];
                if (item == splitSeparator[num3])
                {
                    if ((num3 + 1) != num2)
                    {
                        m_splitBuffer.Add(item);
                    }
                    else
                    {
                        destination.AppendLine();
                        m_splitBuffer.Clear();
                        num3 = 0;
                    }
                }
                else
                {
                    if (num3 > 0)
                    {
                        foreach (char ch2 in m_splitBuffer)
                        {
                            destination.Append(ch2);
                        }
                        m_splitBuffer.Clear();
                        num3 = 0;
                    }
                    destination.Append(item);
                }
            }
            foreach (char ch3 in m_splitBuffer)
            {
                destination.Append(ch3);
            }
            m_splitBuffer.Clear();
        }

        public static string StripInvalidChars(string filename) => 
            Path.GetInvalidFileNameChars().Aggregate<char, string>(filename, (current, c) => current.Replace(c.ToString(), string.Empty));

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T local = lhs;
            lhs = rhs;
            rhs = local;
        }

        public static bool TryParseWithSuffix(this string text, NumberStyles numberStyle, IFormatProvider formatProvider, out float value, Tuple<string, float>[] suffix = null)
        {
            foreach (Tuple<string, float> tuple in suffix ?? DefaultNumberSuffix)
            {
                if (text.EndsWith(tuple.Item1, StringComparison.InvariantCultureIgnoreCase))
                {
                    value *= tuple.Item2;
                    return float.TryParse(text.Substring(0, text.Length - tuple.Item1.Length), numberStyle, formatProvider, out value);
                }
            }
            return float.TryParse(text, out value);
        }

        public static string UpdateControlsFromNotificationFriendly(this string text) => 
            text.Replace("U+005B", "[").Replace("U+005D", "]");

        public static string UpdateControlsToNotificationFriendly(this string text) => 
            text.Replace("[", "U+005B").Replace("]", "U+005D");

        public static void VectorPlaneRotation(Vector3D xVector, Vector3D yVector, out Vector3D xOut, out Vector3D yOut, float angle)
        {
            Vector3D vectord = (xVector * Math.Cos((double) angle)) + (yVector * Math.Sin((double) angle));
            Vector3D vectord2 = (xVector * Math.Cos(angle + 1.5707963267948966)) + (yVector * Math.Sin(angle + 1.5707963267948966));
            xOut = vectord;
            yOut = vectord2;
        }

        public static Thread MainThread
        {
            [CompilerGenerated]
            get => 
                <MainThread>k__BackingField;
            [CompilerGenerated]
            set => 
                (<MainThread>k__BackingField = value);
        }

        private static Random Instance
        {
            get
            {
                if (m_secretRandom == null)
                {
                    m_secretRandom = !MyRandom.EnableDeterminism ? new Random() : new Random(1);
                }
                return m_secretRandom;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyUtils.<>c <>9 = new MyUtils.<>c();
            public static Func<string, char, string> <>9__18_0;

            internal string <StripInvalidChars>b__18_0(string current, char c) => 
                current.Replace(c.ToString(), string.Empty);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ClearCollectionToken<TCollection, TElement> : IDisposable where TCollection: class, ICollection<TElement>, new()
        {
            public readonly TCollection Collection;
            public ClearCollectionToken(TCollection collection)
            {
                this.Collection = collection;
            }

            public void Dispose()
            {
                this.Collection.Clear();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ClearRangeToken<T> : IDisposable
        {
            public readonly int Begin;
            public readonly List<T> Collection;
            public ClearRangeToken(List<T> collection)
            {
                this.Collection = collection;
                this.Begin = collection.Count;
            }

            public void Dispose()
            {
                int count = this.Collection.Count - this.Begin;
                this.Collection.RemoveRange(this.Begin, count);
            }

            public void Add(T element)
            {
                this.Collection.Add(element);
            }

            public OffsetEnumerator<T> GetEnumerator() => 
                new OffsetEnumerator<T>(this.Collection, this.Begin);
            [StructLayout(LayoutKind.Sequential)]
            public struct OffsetEnumerator : IEnumerator<T>, IDisposable, IEnumerator
            {
                private readonly int End;
                private int Index;
                private readonly List<T> List;
                public T Current =>
                    this.List[this.Index];
                object IEnumerator.Current =>
                    this.List[this.Index];
                public OffsetEnumerator(List<T> list, int begin)
                {
                    this.List = list;
                    this.End = list.Count;
                    this.Index = begin - 1;
                }

                public bool MoveNext()
                {
                    this.Index++;
                    return (this.Index < this.End);
                }

                public void Dispose()
                {
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Edge : IEquatable<MyUtils.Edge>
        {
            public int I0;
            public int I1;
            public bool Equals(MyUtils.Edge other) => 
                Equals(other.GetHashCode(), this.GetHashCode());

            public override int GetHashCode() => 
                ((this.I0 < this.I1) ? ((this.I0.GetHashCode() * 0x18d) ^ this.I1.GetHashCode()) : ((this.I1.GetHashCode() * 0x18d) ^ this.I0.GetHashCode()));
        }
    }
}

