namespace Sandbox.Engine.Utils
{
    using ProtoBuf;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    internal class MyVoxelSegmentation
    {
        private HashSet<Vector3I> m_filledVoxels = new HashSet<Vector3I>(new Vector3IEqualityComparer());
        private HashSet<Vector3I> m_selectionList = new HashSet<Vector3I>(new Vector3IEqualityComparer());
        private List<Segment> m_segments = new List<Segment>();
        private List<Segment> m_tmpSegments = new List<Segment>();
        private HashSet<Vector3I> m_usedVoxels = new HashSet<Vector3I>();

        private void AddAllVoxels(Vector3I from, Vector3I to)
        {
            int x = from.X;
            while (x <= to.X)
            {
                int y = from.Y;
                while (true)
                {
                    if (y > to.Y)
                    {
                        x++;
                        break;
                    }
                    int z = from.Z;
                    while (true)
                    {
                        if (z > to.Z)
                        {
                            y++;
                            break;
                        }
                        this.m_filledVoxels.Add(new Vector3I(x, y, z));
                        z++;
                    }
                }
            }
        }

        public void AddInput(Vector3I input)
        {
            this.m_filledVoxels.Add(input);
        }

        private unsafe void AddSegment(ref Vector3I from, ref Vector3I to)
        {
            Vector3I vectori;
            bool flag = false;
            vectori.X = from.X;
            while (vectori.X <= to.X)
            {
                vectori.Y = from.Y;
                while (true)
                {
                    if (vectori.Y > to.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = from.Z;
                    while (true)
                    {
                        if (vectori.Z > to.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        flag = this.m_usedVoxels.Add(vectori) | flag;
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
            if (flag)
            {
                this.m_segments.Add(new Segment(from, to));
            }
        }

        private unsafe bool AllFilled(Vector3I from, Vector3I to)
        {
            Vector3I vectori;
            vectori.X = to.X;
            while (vectori.X >= from.X)
            {
                vectori.Y = to.Y;
                while (true)
                {
                    if (vectori.Y < from.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]--;
                        break;
                    }
                    vectori.Z = to.Z;
                    while (true)
                    {
                        if (vectori.Z < from.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]--;
                            break;
                        }
                        if (!this.m_filledVoxels.Contains(vectori))
                        {
                            return false;
                        }
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]--;
                    }
                }
            }
            return true;
        }

        public void ClearInput()
        {
            this.m_filledVoxels.Clear();
        }

        private void ClipSegments()
        {
            int index = this.m_segments.Count - 1;
            while (true)
            {
                while (true)
                {
                    if (index < 0)
                    {
                        return;
                    }
                    this.m_filledVoxels.Clear();
                    this.AddAllVoxels(this.m_segments[index].Min, this.m_segments[index].Max);
                    for (int i = this.m_segments.Count - 1; i >= 0; i--)
                    {
                        if (index != i)
                        {
                            this.RemoveVoxels(this.m_segments[i].Min, this.m_segments[i].Max);
                            if (this.m_filledVoxels.Count == 0)
                            {
                                break;
                            }
                        }
                    }
                    break;
                }
                if (this.m_filledVoxels.Count == 0)
                {
                    this.m_segments.RemoveAt(index);
                }
                else
                {
                    Segment segment = this.m_segments[index];
                    segment.Replace(this.m_filledVoxels);
                    this.m_segments[index] = segment;
                }
                index--;
            }
        }

        private unsafe void CreateSegments(bool fastMethod)
        {
            this.m_usedVoxels.Clear();
            foreach (Vector3I vectori3 in this.m_filledVoxels)
            {
                if (this.m_usedVoxels.Contains(vectori3))
                {
                    continue;
                }
                Vector3I start = vectori3;
                Vector3I pos = vectori3;
                this.ExpandX(ref start, ref pos);
                this.ExpandY(ref start, ref pos);
                this.ExpandZ(ref start, ref pos);
                this.AddSegment(ref start, ref pos);
                if (!fastMethod)
                {
                    while (pos.X > start.X)
                    {
                        while (true)
                        {
                            if (pos.Y <= start.Y)
                            {
                                int* numPtr2 = (int*) ref pos.X;
                                numPtr2[0]--;
                                pos.Y = start.Y;
                                pos.Z = start.Z;
                                this.ExpandY(ref start, ref pos);
                                this.ExpandZ(ref start, ref pos);
                                this.AddSegment(ref start, ref pos);
                                break;
                            }
                            int* numPtr1 = (int*) ref pos.Y;
                            numPtr1[0]--;
                            pos.Z = start.Z;
                            this.ExpandZ(ref start, ref pos);
                            this.AddSegment(ref start, ref pos);
                        }
                    }
                }
            }
        }

        private void CreateSegmentsExtraSimple()
        {
            while (this.m_filledVoxels.Count > 0)
            {
                HashSet<Vector3I>.Enumerator enumerator = this.m_filledVoxels.GetEnumerator();
                enumerator.MoveNext();
                Vector3I current = enumerator.Current;
                Vector3I pos = current;
                this.ExpandX(ref current, ref pos);
                this.ExpandY(ref current, ref pos);
                this.ExpandZ(ref current, ref pos);
                this.m_segments.Add(new Segment(current, pos));
                int x = current.X;
                while (x <= pos.X)
                {
                    int y = current.Y;
                    while (true)
                    {
                        if (y > pos.Y)
                        {
                            x++;
                            break;
                        }
                        int z = current.Z;
                        while (true)
                        {
                            if (z > pos.Z)
                            {
                                y++;
                                break;
                            }
                            this.m_filledVoxels.Remove(new Vector3I(x, y, z));
                            z++;
                        }
                    }
                }
            }
        }

        private void CreateSegmentsSimple()
        {
            HashSet<Vector3I> selectionList = this.m_selectionList;
            this.m_selectionList = this.m_filledVoxels;
            this.CreateSegmentsSimpleCore();
            this.m_selectionList = selectionList;
        }

        private void CreateSegmentsSimple2()
        {
            this.m_selectionList.Clear();
            foreach (Vector3I vectori in this.m_filledVoxels)
            {
                this.m_selectionList.Add(vectori);
            }
            this.CreateSegmentsSimpleCore();
        }

        private void CreateSegmentsSimpleCore()
        {
            while (this.m_selectionList.Count > 0)
            {
                HashSet<Vector3I>.Enumerator enumerator = this.m_selectionList.GetEnumerator();
                enumerator.MoveNext();
                bool flag = true;
                bool flag2 = true;
                bool flag3 = true;
                bool flag4 = true;
                bool flag5 = true;
                bool flag6 = true;
                Vector3I current = enumerator.Current;
                Vector3I max = current;
                this.m_filledVoxels.Remove(current);
                this.m_selectionList.Remove(current);
                while (true)
                {
                    if (!(((((flag | flag2) | flag3) | flag4) | flag5) | flag6))
                    {
                        this.m_segments.Add(new Segment(current, max));
                        break;
                    }
                    if (flag)
                    {
                        flag = this.ExpandByOnePlusX(ref current, ref max);
                    }
                    if (flag4)
                    {
                        flag4 = this.ExpandByOneMinusX(ref current, ref max);
                    }
                    if (flag2)
                    {
                        flag2 = this.ExpandByOnePlusY(ref current, ref max);
                    }
                    if (flag5)
                    {
                        flag5 = this.ExpandByOneMinusY(ref current, ref max);
                    }
                    if (flag3)
                    {
                        flag3 = this.ExpandByOnePlusZ(ref current, ref max);
                    }
                    if (flag6)
                    {
                        flag6 = this.ExpandByOneMinusZ(ref current, ref max);
                    }
                }
            }
        }

        private int Expand(Vector3I start, ref Vector3I pos, ref Vector3I expand)
        {
            int num = 0;
            while (this.AllFilled((Vector3I) (start + expand), (Vector3I) (pos + expand)))
            {
                start = (Vector3I) (start + expand);
                pos = (Vector3I) (pos + expand);
                num++;
            }
            return num;
        }

        private bool ExpandByOneMinusX(ref Vector3I min, ref Vector3I max)
        {
            int x = min.X - 1;
            int y = min.Y;
            while (y <= max.Y)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        y++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    z++;
                }
            }
            min.X = x;
            int num4 = min.Y;
            while (num4 <= max.Y)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(x, num4, z));
                    z++;
                }
            }
            return true;
        }

        private bool ExpandByOneMinusY(ref Vector3I min, ref Vector3I max)
        {
            int y = min.Y - 1;
            int x = min.X;
            while (x <= max.X)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        x++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    z++;
                }
            }
            min.Y = y;
            int num4 = min.X;
            while (num4 <= max.X)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(num4, y, z));
                    z++;
                }
            }
            return true;
        }

        private bool ExpandByOneMinusZ(ref Vector3I min, ref Vector3I max)
        {
            int z = min.Z - 1;
            int x = min.X;
            while (x <= max.X)
            {
                int y = min.Y;
                while (true)
                {
                    if (y > max.Y)
                    {
                        x++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    y++;
                }
            }
            min.Z = z;
            int num4 = min.X;
            while (num4 <= max.X)
            {
                int y = min.Y;
                while (true)
                {
                    if (y > max.Y)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(num4, y, z));
                    y++;
                }
            }
            return true;
        }

        private bool ExpandByOnePlusX(ref Vector3I min, ref Vector3I max)
        {
            int x = max.X + 1;
            int y = min.Y;
            while (y <= max.Y)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        y++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    z++;
                }
            }
            max.X = x;
            int num4 = min.Y;
            while (num4 <= max.Y)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(x, num4, z));
                    z++;
                }
            }
            return true;
        }

        private bool ExpandByOnePlusY(ref Vector3I min, ref Vector3I max)
        {
            int y = max.Y + 1;
            int x = min.X;
            while (x <= max.X)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        x++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    z++;
                }
            }
            max.Y = y;
            int num4 = min.X;
            while (num4 <= max.X)
            {
                int z = min.Z;
                while (true)
                {
                    if (z > max.Z)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(num4, y, z));
                    z++;
                }
            }
            return true;
        }

        private bool ExpandByOnePlusZ(ref Vector3I min, ref Vector3I max)
        {
            int z = max.Z + 1;
            int x = min.X;
            while (x <= max.X)
            {
                int y = min.Y;
                while (true)
                {
                    if (y > max.Y)
                    {
                        x++;
                        break;
                    }
                    if (!this.m_filledVoxels.Contains(new Vector3I(x, y, z)))
                    {
                        return false;
                    }
                    y++;
                }
            }
            max.Z = z;
            int num4 = min.X;
            while (num4 <= max.X)
            {
                int y = min.Y;
                while (true)
                {
                    if (y > max.Y)
                    {
                        num4++;
                        break;
                    }
                    this.m_selectionList.Remove(new Vector3I(num4, y, z));
                    y++;
                }
            }
            return true;
        }

        private int ExpandX(ref Vector3I start, ref Vector3I pos) => 
            this.Expand(start, ref pos, ref Vector3I.UnitX);

        private int ExpandY(ref Vector3I start, ref Vector3I pos) => 
            this.Expand(start, ref pos, ref Vector3I.UnitY);

        private int ExpandZ(ref Vector3I start, ref Vector3I pos) => 
            this.Expand(start, ref pos, ref Vector3I.UnitZ);

        public List<Segment> FindSegments(MyVoxelSegmentationType segmentationType = 2, int mergeIterations = 1)
        {
            this.m_segments.Clear();
            switch (segmentationType)
            {
                case MyVoxelSegmentationType.ExtraSimple:
                    this.CreateSegmentsExtraSimple();
                    break;

                case MyVoxelSegmentationType.Simple:
                    this.CreateSegmentsSimple();
                    break;

                case MyVoxelSegmentationType.Simple2:
                    this.CreateSegmentsSimple2();
                    break;

                default:
                    this.CreateSegments(segmentationType == MyVoxelSegmentationType.Fast);
                    this.m_segments.Sort(new SegmentSizeComparer());
                    this.RemoveFullyContainedOptimized();
                    this.ClipSegments();
                    for (int i = 0; i < mergeIterations; i++)
                    {
                        this.MergeSegments();
                    }
                    break;
            }
            return this.m_segments;
        }

        private unsafe void MergeSegments()
        {
            int num = 0;
            while (true)
            {
                int num2;
                while (true)
                {
                    if (num >= this.m_segments.Count)
                    {
                        return;
                    }
                    num2 = num + 1;
                    break;
                }
                while (true)
                {
                    if (num2 >= this.m_segments.Count)
                    {
                        num++;
                        break;
                    }
                    Segment segment = this.m_segments[num];
                    Segment segment2 = this.m_segments[num2];
                    int num3 = 0;
                    if ((segment.Min.X == segment2.Min.X) && (segment.Max.X == segment2.Max.X))
                    {
                        num3++;
                    }
                    if ((segment.Min.Y == segment2.Min.Y) && (segment.Max.Y == segment2.Max.Y))
                    {
                        num3++;
                    }
                    if ((segment.Min.Z == segment2.Min.Z) && (segment.Max.Z == segment2.Max.Z))
                    {
                        num3++;
                    }
                    if ((num3 == 2) && (((segment.Min.X == (segment2.Max.X + 1)) || (((segment.Max.X + 1) == segment2.Min.X) || ((segment.Min.Y == (segment2.Max.Y + 1)) || (((segment.Max.Y + 1) == segment2.Min.Y) || (segment.Min.Z == (segment2.Max.Z + 1)))))) || ((segment.Max.Z + 1) == segment2.Min.Z)))
                    {
                        Segment* segmentPtr1 = (Segment*) ref segment;
                        segmentPtr1->Min = Vector3I.Min(segment.Min, segment2.Min);
                        Segment* segmentPtr2 = (Segment*) ref segment;
                        segmentPtr2->Max = Vector3I.Max(segment.Max, segment2.Max);
                        this.m_segments[num] = segment;
                        this.m_segments.RemoveAt(num2);
                        continue;
                    }
                    num2++;
                }
            }
        }

        private void RemoveFullyContained()
        {
            int num = 0;
            while (num < this.m_segments.Count)
            {
                int index = num + 1;
                while (true)
                {
                    if (index >= this.m_segments.Count)
                    {
                        num++;
                        break;
                    }
                    Segment segment = this.m_segments[num];
                    if (segment.Contains(this.m_segments[index]))
                    {
                        this.m_segments.RemoveAt(index);
                        continue;
                    }
                    index++;
                }
            }
        }

        private unsafe void RemoveFullyContainedOptimized()
        {
            this.m_filledVoxels.Clear();
            this.m_tmpSegments.Clear();
            int num = 0;
            while (num < this.m_segments.Count)
            {
                Vector3I vectori;
                bool flag = false;
                Vector3I min = this.m_segments[num].Min;
                Vector3I max = this.m_segments[num].Max;
                vectori.X = min.X;
                while (true)
                {
                    if (vectori.X > max.X)
                    {
                        if (flag)
                        {
                            this.m_tmpSegments.Add(this.m_segments[num]);
                        }
                        num++;
                        break;
                    }
                    vectori.Y = min.Y;
                    while (true)
                    {
                        if (vectori.Y > max.Y)
                        {
                            int* numPtr3 = (int*) ref vectori.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori.Z = min.Z;
                        while (true)
                        {
                            if (vectori.Z > max.Z)
                            {
                                int* numPtr2 = (int*) ref vectori.Y;
                                numPtr2[0]++;
                                break;
                            }
                            flag = this.m_filledVoxels.Add(vectori) | flag;
                            int* numPtr1 = (int*) ref vectori.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
            List<Segment> segments = this.m_segments;
            this.m_segments = this.m_tmpSegments;
            this.m_tmpSegments = segments;
        }

        private void RemoveVoxels(Vector3I from, Vector3I to)
        {
            int x = from.X;
            while (x <= to.X)
            {
                int y = from.Y;
                while (true)
                {
                    if (y > to.Y)
                    {
                        x++;
                        break;
                    }
                    int z = from.Z;
                    while (true)
                    {
                        if (z > to.Z)
                        {
                            y++;
                            break;
                        }
                        this.m_filledVoxels.Remove(new Vector3I(x, y, z));
                        z++;
                    }
                }
            }
        }

        private Vector3I ShiftVector(Vector3I vec) => 
            new Vector3I(vec.Z, vec.X, vec.Y);

        public int InputCount =>
            this.m_filledVoxels.Count;

        private class DescIntComparer : IComparer<int>
        {
            public int Compare(int x, int y) => 
                (y - x);
        }

        [StructLayout(LayoutKind.Sequential), ProtoContract]
        public struct Segment
        {
            [ProtoMember(50)]
            public Vector3I Min;
            [ProtoMember(0x34)]
            public Vector3I Max;
            public Vector3I Size =>
                ((Vector3I) ((this.Max - this.Min) + Vector3I.One));
            public int VoxelCount =>
                ((this.Size.X * this.Size.Y) * this.Size.Z);
            public Segment(Vector3I min, Vector3I max)
            {
                this.Min = min;
                this.Max = max;
            }

            public bool Contains(MyVoxelSegmentation.Segment b) => 
                ((Vector3I.Min(b.Min, this.Min) == this.Min) && (Vector3I.Max(b.Max, this.Max) == this.Max));

            public void Replace(IEnumerable<Vector3I> voxels)
            {
                this.Min = Vector3I.MaxValue;
                this.Max = Vector3I.MinValue;
                foreach (Vector3I vectori in voxels)
                {
                    this.Min = Vector3I.Min(this.Min, vectori);
                    this.Max = Vector3I.Max(this.Max, vectori);
                }
            }
        }

        private class SegmentSizeComparer : IComparer<MyVoxelSegmentation.Segment>
        {
            public int Compare(MyVoxelSegmentation.Segment x, MyVoxelSegmentation.Segment y) => 
                (y.VoxelCount - x.VoxelCount);
        }

        private class Vector3IComparer : IComparer<Vector3I>
        {
            public int Compare(Vector3I x, Vector3I y) => 
                x.CompareTo(y);
        }

        private class Vector3IEqualityComparer : IEqualityComparer<Vector3I>
        {
            public bool Equals(Vector3I v1, Vector3I v2) => 
                ((v1.X == v2.X) && ((v1.Y == v2.Y) && (v1.Z == v2.Z)));

            public int GetHashCode(Vector3I obj) => 
                ((((obj.X * 0x2627) ^ obj.Y) * 0x2627) ^ obj.Z);
        }
    }
}

