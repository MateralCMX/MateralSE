namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    internal class MyOndraSimulator2 : IMyIntegritySimulator
    {
        private MyCubeGrid m_grid;
        private Dictionary<Vector3I, CubeData> m_cubes = new Dictionary<Vector3I, CubeData>(Vector3I.Comparer);
        private Stack<Vector3I> m_tmpCubes = new Stack<Vector3I>();
        private float m_totalMax;
        private float m_breakThreshold = 10f;
        private bool m_cubeChanged;

        public MyOndraSimulator2(MyCubeGrid grid)
        {
            this.m_grid = grid;
        }

        public void Add(MySlimBlock block)
        {
            CubeData data = new CubeData(MyCubeGrid.IsInVoxels(block, true));
            bool flag1 = MyCubeGrid.IsInVoxels(block, true);
            if (!flag1)
            {
                data.CurrentOffset = 0.05f;
            }
            this.m_cubes[block.Position] = data;
            this.m_cubeChanged = true;
        }

        public void Close()
        {
        }

        public void DebugDraw()
        {
            this.m_totalMax = Math.Max(this.m_totalMax, 0.2f);
            float gridSize = this.m_grid.GridSize;
            foreach (KeyValuePair<Vector3I, CubeData> pair in this.m_cubes)
            {
                Color black = Color.Black;
                if (!pair.Value.IsStatic)
                {
                    black = GetTension(pair.Value.MaxDiff, this.m_totalMax);
                }
                Matrix matrix = (Matrix.CreateScale((float) (gridSize * 1.02f)) * Matrix.CreateTranslation(pair.Key * gridSize)) * this.m_grid.WorldMatrix;
                string text = pair.Value.MaxDiff.ToString("0.00");
                MyRenderProxy.DebugDrawOBB(matrix, black.ToVector3(), 0.5f, true, true, true, false);
                MyRenderProxy.DebugDrawText3D(matrix.Translation, text, pair.Value.Merged ? Color.Black : Color.White, 0.5f, false, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, -1, false);
            }
        }

        public void Draw()
        {
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
            CubeData data;
            CubeData data2;
            return (!this.m_cubes.TryGetValue(blockA.Position, out data) || (!this.m_cubes.TryGetValue(blockB.Position, out data2) || (Math.Abs((float) (data.TmpOffset - data2.TmpOffset)) < this.m_breakThreshold)));
        }

        private static void PropagateNeighbor(Dictionary<Vector3I, CubeData> cubes, Stack<Vector3I> toCheck, Vector3I pos, Vector3I dir)
        {
            CubeData data2;
            CubeData data = cubes[pos];
            if (cubes.TryGetValue(pos + dir, out data2) && (data2.DistanceToStatic > (data.DistanceToStatic + 1)))
            {
                data2.DistanceToStatic = data.DistanceToStatic + 1;
                toCheck.Push(pos + dir);
            }
        }

        private void Refresh()
        {
            if (this.m_cubeChanged)
            {
                this.m_cubes.Clear();
                foreach (KeyValuePair<Vector3I, CubeData> pair in this.m_cubes.ToArray<KeyValuePair<Vector3I, CubeData>>())
                {
                    CubeData data1 = new CubeData(0f);
                    data1.CurrentOffset = pair.Value.CurrentOffset;
                    data1.IsStatic = pair.Value.IsStatic;
                    data1.DistanceToStatic = pair.Value.IsStatic ? 0 : 0x7fffffff;
                    CubeData data = data1;
                    this.m_cubes.Add(pair.Key, data);
                    if (data.IsStatic)
                    {
                        this.m_tmpCubes.Push(pair.Key);
                    }
                }
                while (this.m_tmpCubes.Count > 0)
                {
                    Vector3I pos = this.m_tmpCubes.Pop();
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, Vector3I.UnitX);
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, Vector3I.UnitY);
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, Vector3I.UnitZ);
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, -Vector3I.UnitX);
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, -Vector3I.UnitY);
                    PropagateNeighbor(this.m_cubes, this.m_tmpCubes, pos, -Vector3I.UnitZ);
                }
                this.m_cubeChanged = false;
            }
        }

        public void Remove(MySlimBlock block)
        {
            this.m_cubes.Remove(block.Position);
            this.m_cubeChanged = true;
        }

        public bool Simulate(float deltaTime)
        {
            this.Refresh();
            Solve_Iterative(this.m_cubes, 0.9f, out this.m_totalMax);
            foreach (MySlimBlock block in this.m_grid.GetBlocks())
            {
                CubeData data = this.m_cubes[block.Position];
                if (data.MaxDiff < this.m_breakThreshold)
                {
                    data.FramesOverThreshold = 0;
                    continue;
                }
                data.FramesOverThreshold++;
                if (data.FramesOverThreshold > 5)
                {
                    this.m_grid.UpdateBlockNeighbours(block);
                }
            }
            return true;
        }

        private static void Solve_Iterative(Dictionary<Vector3I, CubeData> cubes, float ratio, out float maxError)
        {
            foreach (KeyValuePair<Vector3I, CubeData> pair in cubes)
            {
                if (!pair.Value.IsStatic)
                {
                    pair.Value.LastDelta = Math.Max(0.5f, pair.Value.LastDelta);
                    float lastDelta = pair.Value.LastDelta;
                    pair.Value.TmpOffset = pair.Value.CurrentOffset + lastDelta;
                }
            }
            maxError = 0f;
            foreach (KeyValuePair<Vector3I, CubeData> pair2 in cubes)
            {
                if (!pair2.Value.IsStatic)
                {
                    int distanceToStatic = pair2.Value.DistanceToStatic;
                    float sum = 0f;
                    float count = 0f;
                    float max = 0f;
                    SumConstraints(pair2.Value, cubes, pair2.Key + Vector3I.UnitX, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    SumConstraints(pair2.Value, cubes, pair2.Key + Vector3I.UnitY, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    SumConstraints(pair2.Value, cubes, pair2.Key + Vector3I.UnitZ, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    SumConstraints(pair2.Value, cubes, pair2.Key - Vector3I.UnitX, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    SumConstraints(pair2.Value, cubes, pair2.Key - Vector3I.UnitY, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    SumConstraints(pair2.Value, cubes, pair2.Key - Vector3I.UnitZ, pair2.Value.TmpOffset, ref sum, ref count, ref max);
                    float num5 = (count > 0f) ? ((-sum / count) * ratio) : 0f;
                    float tmpOffset = pair2.Value.TmpOffset;
                    float currentOffset = pair2.Value.CurrentOffset;
                    pair2.Value.CurrentOffset = pair2.Value.TmpOffset + num5;
                    pair2.Value.MaxDiff = max;
                    pair2.Value.Sum = sum;
                    maxError = Math.Max(maxError, max);
                }
            }
        }

        private static void SumConstraints(CubeData me, Dictionary<Vector3I, CubeData> cubes, Vector3I neighbourPos, float myOffset, ref float sum, ref float count, ref float max)
        {
            CubeData data;
            if (cubes.TryGetValue(neighbourPos, out data) && !ReferenceEquals(data, me))
            {
                float num = myOffset - data.TmpOffset;
                max = Math.Max(num, max);
                sum += num;
                count++;
            }
        }

        private class CubeData
        {
            public bool IsStatic;
            public float CurrentOffset;
            public float TmpOffset;
            public float MaxDiff;
            public float LastMaxDiff;
            public float LastDelta;
            public float Sum;
            public int DistanceToStatic;
            public bool Merged;
            public int FramesOverThreshold;

            public CubeData(bool isStatic)
            {
                this.IsStatic = isStatic;
            }

            public CubeData(float offset = 0f)
            {
                this.IsStatic = false;
                this.CurrentOffset = offset;
                this.TmpOffset = offset;
            }
        }
    }
}

