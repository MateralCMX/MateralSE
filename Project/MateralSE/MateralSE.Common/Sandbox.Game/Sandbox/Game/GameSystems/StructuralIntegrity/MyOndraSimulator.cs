namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyOndraSimulator : IMyIntegritySimulator
    {
        private static HashSet<Vector3I> m_disconnectHelper = new HashSet<Vector3I>();
        private MyCubeGrid m_grid;
        private Dictionary<Vector3I, Element> Lookup = new Dictionary<Vector3I, Element>(Vector3I.Comparer);
        private HashSet<Element> Elements = new HashSet<Element>();
        private HashSet<Vector3I> TmpDisconnectList = new HashSet<Vector3I>(Vector3I.Comparer);
        private bool m_blocksChanged;
        private float m_breakThreshold = 10f;
        private float m_totalMax;

        public MyOndraSimulator(MyCubeGrid grid)
        {
            this.m_grid = grid;
        }

        public void Add(MySlimBlock block)
        {
            Element element = new Element(MyCubeGrid.IsInVoxels(block, true));
            bool flag1 = MyCubeGrid.IsInVoxels(block, true);
            if (!flag1)
            {
                element.CurrentOffset = 0.05f;
            }
            this.Lookup[block.Position] = element;
            this.m_blocksChanged = true;
        }

        private static void AddNeighbor(Stack<KeyValuePair<Vector3I, Element>> addTo, Dictionary<Vector3I, Element> lookup, Vector3I pos)
        {
            Element element;
            if (lookup.TryGetValue(pos, out element) && !element.IsStatic)
            {
                addTo.Push(new KeyValuePair<Vector3I, Element>(pos, element));
                lookup.Remove(pos);
            }
        }

        private static void AddNeighbors(HashSet<Vector3I> helper, Vector3I pos, Element e)
        {
            if (e.Cubes.Contains(pos) && helper.Add(pos))
            {
                AddNeighbors(helper, (Vector3I) (pos + Vector3I.UnitX), e);
                AddNeighbors(helper, (Vector3I) (pos + Vector3I.UnitY), e);
                AddNeighbors(helper, (Vector3I) (pos + Vector3I.UnitZ), e);
                AddNeighbors(helper, pos - Vector3I.UnitX, e);
                AddNeighbors(helper, pos - Vector3I.UnitY, e);
                AddNeighbors(helper, pos - Vector3I.UnitZ, e);
            }
        }

        private void CheckBlockChanges()
        {
            if (this.m_blocksChanged)
            {
                this.m_blocksChanged = false;
                Dictionary<Vector3I, Element> source = this.Lookup.ToDictionary<KeyValuePair<Vector3I, Element>, Vector3I, Element>(s => s.Key, v => v.Value);
                this.Lookup.Clear();
                Stack<KeyValuePair<Vector3I, Element>> addTo = new Stack<KeyValuePair<Vector3I, Element>>();
                while (source.Count > 0)
                {
                    KeyValuePair<Vector3I, Element> item = source.First<KeyValuePair<Vector3I, Element>>();
                    source.Remove(item.Key);
                    if (!item.Value.IsStatic)
                    {
                        this.Elements.Add(item.Value);
                    }
                    addTo.Push(item);
                    while (addTo.Count > 0)
                    {
                        KeyValuePair<Vector3I, Element> pair2 = addTo.Pop();
                        item.Value.Cubes.Add(pair2.Key);
                        this.Lookup.Add(pair2.Key, item.Value);
                        if (!item.Value.IsStatic)
                        {
                            AddNeighbor(addTo, source, pair2.Key + Vector3I.UnitX);
                            AddNeighbor(addTo, source, pair2.Key + Vector3I.UnitY);
                            AddNeighbor(addTo, source, pair2.Key + Vector3I.UnitZ);
                            AddNeighbor(addTo, source, pair2.Key - Vector3I.UnitX);
                            AddNeighbor(addTo, source, pair2.Key - Vector3I.UnitY);
                            AddNeighbor(addTo, source, pair2.Key - Vector3I.UnitZ);
                        }
                    }
                }
            }
        }

        public void Close()
        {
        }

        public void DebugDraw()
        {
            this.m_totalMax = Math.Max(this.m_totalMax, 0.2f);
            float gridSize = this.m_grid.GridSize;
            float num2 = 0f;
            foreach (KeyValuePair<Vector3I, Element> pair in this.Lookup)
            {
                if (!pair.Value.IsStatic)
                {
                    Color tension = GetTension(pair.Value.MaxDiff, this.m_totalMax);
                    num2 += pair.Value.AbsSum;
                    this.DrawCube(gridSize, pair, ref tension, pair.Value.AbsSum.ToString("0.00"));
                }
            }
            IEnumerable<KeyValuePair<Vector3I, Element>> source = from s in this.Lookup
                where s.Value.IsStatic
                select s;
            if (source.Any<KeyValuePair<Vector3I, Element>>())
            {
                Color black = Color.Black;
                this.DrawCube(gridSize, source.First<KeyValuePair<Vector3I, Element>>(), ref black, num2.ToString());
            }
        }

        private void Disconnect()
        {
            while (this.TmpDisconnectList.Count > 0)
            {
                Vector3I item = this.TmpDisconnectList.First<Vector3I>();
                this.TmpDisconnectList.Remove(item);
                Element e = this.Lookup[item];
                if (e.Cubes.Count != 1)
                {
                    e.Cubes.Remove(item);
                    Element element1 = new Element(false);
                    element1.CurrentOffset = e.CurrentOffset;
                    Element element2 = element1;
                    element2.Cubes.Add(item);
                    this.Elements.Add(element2);
                    this.Lookup[item] = element2;
                    this.TestDisconnect(e);
                }
            }
        }

        public void Draw()
        {
        }

        private void DrawCube(float size, KeyValuePair<Vector3I, Element> c, ref Color color, string text)
        {
            Matrix matrix = (Matrix.CreateScale((float) (size * 1.02f)) * Matrix.CreateTranslation(c.Key * size)) * this.m_grid.WorldMatrix;
            MyRenderProxy.DebugDrawOBB(matrix, color.ToVector3(), 0.5f, true, true, true, false);
            MyRenderProxy.DebugDrawText3D(matrix.Translation, text, (c.Value.Cubes.Count > 1) ? Color.Black : Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
        }

        public void ForceRecalc()
        {
        }

        public float GetSupportedWeight(Vector3I pos) => 
            0f;

        public float GetTension(Vector3I pos) => 
            0f;

        private static Color GetTension(float offset, float max) => 
            ((offset >= (max / 2f)) ? new Color(1f, 1f - ((offset - (max / 2f)) / (max / 2f)), 0f) : new Color(offset / (max / 2f), 1f, 0f));

        public bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB)
        {
            Element element;
            Element element2;
            return (!this.Lookup.TryGetValue(blockA.Position, out element) || (!this.Lookup.TryGetValue(blockB.Position, out element2) || (Math.Max(element.AbsSum, element2.AbsSum) < this.m_breakThreshold)));
        }

        public void Remove(MySlimBlock block)
        {
            this.Lookup.Remove(block.Position);
            this.m_blocksChanged = true;
        }

        public bool Simulate(float deltaTime)
        {
            this.CheckBlockChanges();
            this.Solve_Iterative(0.9f, out this.m_totalMax);
            foreach (MySlimBlock block in this.m_grid.GetBlocks())
            {
                if (this.Lookup[block.Position].AbsSum >= this.m_breakThreshold)
                {
                    this.m_grid.UpdateBlockNeighbours(block);
                }
            }
            return true;
        }

        private void Solve_Iterative(float ratio, out float maxError)
        {
            foreach (Element local1 in this.Elements)
            {
                float num2 = 0.05f;
                local1.TmpOffset = local1.CurrentOffset + num2;
            }
            maxError = 0f;
            float disconnectThreshold = 0.055f;
            foreach (Element element in this.Elements)
            {
                float sum = 0f;
                float absSum = 0f;
                float count = 0f;
                float max = 0f;
                foreach (Vector3I vectori in element.Cubes)
                {
                    this.SumConstraints(element, vectori, (Vector3I) (vectori + Vector3I.UnitX), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    this.SumConstraints(element, vectori, (Vector3I) (vectori + Vector3I.UnitY), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    this.SumConstraints(element, vectori, (Vector3I) (vectori + Vector3I.UnitZ), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    this.SumConstraints(element, vectori, vectori - Vector3I.UnitX, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    this.SumConstraints(element, vectori, vectori - Vector3I.UnitY, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    this.SumConstraints(element, vectori, vectori - Vector3I.UnitZ, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                }
                int num1 = element.Cubes.Count;
                sum += 0;
                count += element.Cubes.Count;
                float num7 = (count > 0f) ? ((-sum / count) * ratio) : 0f;
                float tmpOffset = element.TmpOffset;
                float currentOffset = element.CurrentOffset;
                element.CurrentOffset = element.TmpOffset + num7;
                element.MaxDiff = max;
                element.Sum = sum;
                element.AbsSum = absSum;
                maxError = Math.Max(maxError, max);
            }
            this.Disconnect();
        }

        private void SumConstraints(Element me, Vector3I myPos, Vector3I neighbourPos, float myOffset, ref float sum, ref float absSum, ref float count, ref float max, float disconnectThreshold)
        {
            Element element;
            if (this.Lookup.TryGetValue(neighbourPos, out element) && !ReferenceEquals(element, me))
            {
                float num = myOffset - element.TmpOffset;
                max = Math.Max(num, max);
                sum += num * element.Cubes.Count;
                absSum += Math.Abs(num);
                count += element.Cubes.Count;
                if (num > disconnectThreshold)
                {
                    this.TmpDisconnectList.Add(myPos);
                }
            }
        }

        private void TestDisconnect(Element e)
        {
            while (true)
            {
                if (e.Cubes.Count > 1)
                {
                    try
                    {
                        Vector3I pos = e.Cubes.First<Vector3I>();
                        AddNeighbors(m_disconnectHelper, pos, e);
                        if (e.Cubes.Count != m_disconnectHelper.Count)
                        {
                            Element element1 = new Element(false);
                            element1.CurrentOffset = e.CurrentOffset;
                            Element item = element1;
                            foreach (Vector3I vectori2 in m_disconnectHelper)
                            {
                                e.Cubes.Remove(vectori2);
                                item.Cubes.Add(vectori2);
                                this.Lookup[vectori2] = item;
                            }
                            this.Elements.Add(item);
                            continue;
                        }
                    }
                    finally
                    {
                        m_disconnectHelper.Clear();
                        continue;
                    }
                }
                return;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyOndraSimulator.<>c <>9 = new MyOndraSimulator.<>c();
            public static Func<KeyValuePair<Vector3I, MyOndraSimulator.Element>, bool> <>9__12_0;
            public static Func<KeyValuePair<Vector3I, MyOndraSimulator.Element>, Vector3I> <>9__14_0;
            public static Func<KeyValuePair<Vector3I, MyOndraSimulator.Element>, MyOndraSimulator.Element> <>9__14_1;

            internal Vector3I <CheckBlockChanges>b__14_0(KeyValuePair<Vector3I, MyOndraSimulator.Element> s) => 
                s.Key;

            internal MyOndraSimulator.Element <CheckBlockChanges>b__14_1(KeyValuePair<Vector3I, MyOndraSimulator.Element> v) => 
                v.Value;

            internal bool <DebugDraw>b__12_0(KeyValuePair<Vector3I, MyOndraSimulator.Element> s) => 
                s.Value.IsStatic;
        }

        private class Element
        {
            public bool IsStatic;
            public float CurrentOffset;
            public float TmpOffset;
            public float MaxDiff;
            public float LastDelta;
            public float Sum;
            public float AbsSum;
            public HashSet<Vector3I> Cubes;

            public Element(bool isStatic)
            {
                this.Cubes = new HashSet<Vector3I>();
                this.IsStatic = isStatic;
            }

            public Element(float offset = 0f)
            {
                this.Cubes = new HashSet<Vector3I>();
                this.IsStatic = false;
                this.CurrentOffset = offset;
                this.TmpOffset = offset;
            }
        }
    }
}

