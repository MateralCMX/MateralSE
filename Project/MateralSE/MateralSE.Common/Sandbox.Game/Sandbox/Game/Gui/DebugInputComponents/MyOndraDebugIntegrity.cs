namespace Sandbox.Game.GUI.DebugInputComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Stats;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;
    using VRageRender.Utils;

    public class MyOndraDebugIntegrity
    {
        private Dictionary<MyCubeGrid, StructureData> m_grids = new Dictionary<MyCubeGrid, StructureData>();
        private List<Vector3I> m_removeList = new List<Vector3I>();
        private static HashSet<Vector3I> m_disconnectHelper = new HashSet<Vector3I>();
        private long startTimestamp;

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

        private static void Disconnect(StructureData str)
        {
            while (str.TmpDisconnectList.Count > 0)
            {
                Vector3I item = str.TmpDisconnectList.First<Vector3I>();
                str.TmpDisconnectList.Remove(item);
                Element e = str.Lookup[item];
                if (e.Cubes.Count != 1)
                {
                    e.Cubes.Remove(item);
                    Element element1 = new Element(false);
                    element1.CurrentOffset = e.CurrentOffset;
                    Element element2 = element1;
                    element2.Cubes.Add(item);
                    str.Elements.Add(element2);
                    str.Lookup[item] = element2;
                    TestDisconnect(str, e);
                }
            }
        }

        private static void DrawCube(StructureData str, float size, KeyValuePair<Vector3I, Element> c, ref Color color, string text)
        {
            Matrix matrix = (Matrix.CreateScale((float) (size * 1.02f)) * Matrix.CreateTranslation((c.Key * size) + new Vector3(0f, -c.Value.CurrentOffset / 20f, 0f))) * str.m_grid.WorldMatrix;
            MyRenderProxy.DebugDrawOBB(matrix, color.ToVector3(), 0.5f, true, true, true, false);
            MyRenderProxy.DebugDrawText3D(matrix.Translation, text, (c.Value.Cubes.Count > 1) ? Color.Black : Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
        }

        private static void DrawMe(StructureData str)
        {
            float num;
            Dictionary<Vector3I, Element> lookup = str.Lookup;
            Solve_Iterative(str, 0.9f, out num);
            num = Math.Max(num, 0.2f);
            float gridSize = str.m_grid.GridSize;
            float num3 = 0f;
            foreach (KeyValuePair<Vector3I, Element> pair in lookup)
            {
                if (!pair.Value.IsStatic)
                {
                    Color tension = GetTension(pair.Value.MaxDiff, num);
                    num3 += pair.Value.AbsSum;
                    DrawCube(str, gridSize, pair, ref tension, pair.Value.AbsSum.ToString("0.00"));
                }
            }
            if ((from s in lookup
                where s.Value.IsStatic
                select s).Any<KeyValuePair<Vector3I, Element>>())
            {
                Color black = Color.Black;
            }
        }

        private static Color GetTension(float offset, float max) => 
            ((offset >= (max / 2f)) ? new Color(1f, 1f - ((offset - (max / 2f)) / (max / 2f)), 0f) : new Color(offset / (max / 2f), 1f, 0f));

        public void Handle()
        {
            if (MySession.Static != null)
            {
                this.Refresh();
                foreach (KeyValuePair<MyCubeGrid, StructureData> pair in this.m_grids)
                {
                    DrawMe(pair.Value);
                }
            }
        }

        private void Refresh()
        {
            double num = ((double) (Stopwatch.GetTimestamp() - this.startTimestamp)) / ((double) Stopwatch.Frequency);
            Stats.Timing.Write("IntegrityRunTime: {0}s", (float) num, MyStatTypeEnum.CurrentValue | MyStatTypeEnum.FormatFlag, 100, 1, -1);
            IEnumerable<MyCubeGrid> enumerable = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>();
            if (this.m_grids.Count == 0)
            {
                this.startTimestamp = Stopwatch.GetTimestamp();
            }
            foreach (KeyValuePair<MyCubeGrid, StructureData> pair in this.m_grids.ToArray<KeyValuePair<MyCubeGrid, StructureData>>())
            {
                if (pair.Value.m_grid.Closed)
                {
                    this.m_grids.Remove(pair.Value.m_grid);
                }
            }
            foreach (MyCubeGrid grid in enumerable)
            {
                StructureData data;
                if (!this.m_grids.TryGetValue(grid, out data))
                {
                    data = new StructureData {
                        m_grid = grid
                    };
                    this.m_grids[grid] = data;
                }
                bool flag = false;
                foreach (KeyValuePair<Vector3I, Element> pair2 in data.Lookup)
                {
                    if (grid.GetCubeBlock(pair2.Key) == null)
                    {
                        flag = true;
                        this.m_removeList.Add(pair2.Key);
                    }
                }
                foreach (Vector3I vectori in this.m_removeList)
                {
                    data.Lookup.Remove(vectori);
                }
                this.m_removeList.Clear();
                foreach (MySlimBlock block in grid.GetBlocks())
                {
                    bool flag2;
                    Element element;
                    MyStringId? displayNameEnum = block.BlockDefinition.DisplayNameEnum;
                    MyStringId orCompute = MyStringId.GetOrCompute("DisplayName_Block_HeavyArmorBlock");
                    if ((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false)
                    {
                        flag2 = true;
                    }
                    else
                    {
                        displayNameEnum = block.BlockDefinition.DisplayNameEnum;
                        orCompute = MyStringId.GetOrCompute("DisplayName_Block_LightArmorBlock");
                        if (!((displayNameEnum != null) ? ((displayNameEnum != null) ? (displayNameEnum.GetValueOrDefault() == orCompute) : true) : false))
                        {
                            continue;
                        }
                        flag2 = false;
                    }
                    if (!data.Lookup.TryGetValue(block.Position, out element))
                    {
                        flag = true;
                        element = new Element(flag2);
                        if (!flag2)
                        {
                            element.CurrentOffset = 0.05f;
                        }
                        data.Lookup[block.Position] = element;
                    }
                }
                if (flag)
                {
                    Dictionary<Vector3I, Element> source = data.Lookup.ToDictionary<KeyValuePair<Vector3I, Element>, Vector3I, Element>(s => s.Key, v => v.Value);
                    data.Lookup.Clear();
                    Stack<KeyValuePair<Vector3I, Element>> addTo = new Stack<KeyValuePair<Vector3I, Element>>();
                    while (source.Count > 0)
                    {
                        KeyValuePair<Vector3I, Element> item = source.First<KeyValuePair<Vector3I, Element>>();
                        source.Remove(item.Key);
                        if (!item.Value.IsStatic)
                        {
                            data.Elements.Add(item.Value);
                        }
                        addTo.Push(item);
                        while (addTo.Count > 0)
                        {
                            KeyValuePair<Vector3I, Element> pair4 = addTo.Pop();
                            item.Value.Cubes.Add(pair4.Key);
                            data.Lookup.Add(pair4.Key, item.Value);
                            if (!item.Value.IsStatic)
                            {
                                AddNeighbor(addTo, source, pair4.Key + Vector3I.UnitX);
                                AddNeighbor(addTo, source, pair4.Key + Vector3I.UnitY);
                                AddNeighbor(addTo, source, pair4.Key + Vector3I.UnitZ);
                                AddNeighbor(addTo, source, pair4.Key - Vector3I.UnitX);
                                AddNeighbor(addTo, source, pair4.Key - Vector3I.UnitY);
                                AddNeighbor(addTo, source, pair4.Key - Vector3I.UnitZ);
                            }
                        }
                    }
                }
            }
        }

        private static void Solve_Iterative(StructureData str, float ratio, out float maxError)
        {
            foreach (Element local1 in str.Elements)
            {
                float num2 = 0.05f;
                local1.TmpOffset = local1.CurrentOffset + num2;
            }
            maxError = 0f;
            float disconnectThreshold = 0.055f;
            foreach (Element element in str.Elements)
            {
                float sum = 0f;
                float absSum = 0f;
                float count = 0f;
                float max = 0f;
                foreach (Vector3I vectori in element.Cubes)
                {
                    SumConstraints(element, vectori, str, (Vector3I) (vectori + Vector3I.UnitX), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    SumConstraints(element, vectori, str, (Vector3I) (vectori + Vector3I.UnitY), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    SumConstraints(element, vectori, str, (Vector3I) (vectori + Vector3I.UnitZ), element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    SumConstraints(element, vectori, str, vectori - Vector3I.UnitX, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    SumConstraints(element, vectori, str, vectori - Vector3I.UnitY, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
                    SumConstraints(element, vectori, str, vectori - Vector3I.UnitZ, element.TmpOffset, ref sum, ref absSum, ref count, ref max, disconnectThreshold);
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
            Disconnect(str);
        }

        private static void SumConstraints(Element me, Vector3I myPos, StructureData str, Vector3I neighbourPos, float myOffset, ref float sum, ref float absSum, ref float count, ref float max, float disconnectThreshold)
        {
            Element element;
            if (str.Lookup.TryGetValue(neighbourPos, out element) && !ReferenceEquals(element, me))
            {
                float num = myOffset - element.TmpOffset;
                max = Math.Max(num, max);
                sum += num * element.Cubes.Count;
                absSum += Math.Abs(num);
                count += element.Cubes.Count;
                if (num > disconnectThreshold)
                {
                    str.TmpDisconnectList.Add(myPos);
                }
            }
        }

        private static void TestDisconnect(StructureData str, Element e)
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
                                str.Lookup[vectori2] = item;
                            }
                            str.Elements.Add(item);
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
            public static readonly MyOndraDebugIntegrity.<>c <>9 = new MyOndraDebugIntegrity.<>c();
            public static Func<KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element>, bool> <>9__8_0;
            public static Func<KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element>, Vector3I> <>9__16_0;
            public static Func<KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element>, MyOndraDebugIntegrity.Element> <>9__16_1;

            internal bool <DrawMe>b__8_0(KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element> s) => 
                s.Value.IsStatic;

            internal Vector3I <Refresh>b__16_0(KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element> s) => 
                s.Key;

            internal MyOndraDebugIntegrity.Element <Refresh>b__16_1(KeyValuePair<Vector3I, MyOndraDebugIntegrity.Element> v) => 
                v.Value;
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

        private class StructureData
        {
            public MyCubeGrid m_grid;
            public Dictionary<Vector3I, MyOndraDebugIntegrity.Element> Lookup = new Dictionary<Vector3I, MyOndraDebugIntegrity.Element>();
            public HashSet<MyOndraDebugIntegrity.Element> Elements = new HashSet<MyOndraDebugIntegrity.Element>();
            public HashSet<Vector3I> TmpDisconnectList = new HashSet<Vector3I>();
        }
    }
}

