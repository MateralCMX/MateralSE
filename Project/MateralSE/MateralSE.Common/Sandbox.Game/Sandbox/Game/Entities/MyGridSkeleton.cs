namespace Sandbox.Game.Entities
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRageMath;

    internal class MyGridSkeleton
    {
        public readonly ConcurrentDictionary<Vector3I, Vector3> Bones = new ConcurrentDictionary<Vector3I, Vector3>();
        private List<Vector3I> m_tmpRemovedCubes = new List<Vector3I>();
        private HashSet<Vector3I> m_usedBones = new HashSet<Vector3I>();
        private HashSet<Vector3I> m_testedCubes = new HashSet<Vector3I>();
        private static readonly float MAX_BONE_ERROR;
        [ThreadStatic]
        private static List<Vector3I> m_tempAffectedCubes;
        public const int BoneDensity = 2;
        public static readonly Vector3I[] BoneOffsets;

        static unsafe MyGridSkeleton()
        {
            Vector3I vectori;
            m_tempAffectedCubes = new List<Vector3I>();
            MAX_BONE_ERROR = Vector3UByte.Denormalize(new Vector3UByte(0x80, 0x80, 0x80), 1f).X * 0.75f;
            BoneOffsets = new Vector3I[(int) Math.Pow(3.0, 3.0)];
            int index = 0;
            vectori.X = 0;
            while (vectori.X <= 1)
            {
                vectori.Y = 0;
                while (true)
                {
                    if (vectori.Y > 1)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = 0;
                    while (true)
                    {
                        if (vectori.Z > 1)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        index++;
                        BoneOffsets[index] = vectori * 2;
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
            vectori.X = 0;
            while (vectori.X <= 2)
            {
                vectori.Y = 0;
                while (true)
                {
                    if (vectori.Y > 2)
                    {
                        int* numPtr6 = (int*) ref vectori.X;
                        numPtr6[0]++;
                        break;
                    }
                    vectori.Z = 0;
                    while (true)
                    {
                        if (vectori.Z > 2)
                        {
                            int* numPtr5 = (int*) ref vectori.Y;
                            numPtr5[0]++;
                            break;
                        }
                        if (((vectori.X == 1) || (vectori.Y == 1)) || (vectori.Z == 1))
                        {
                            index++;
                            BoneOffsets[index] = vectori;
                        }
                        int* numPtr4 = (int*) ref vectori.Z;
                        numPtr4[0]++;
                    }
                }
            }
        }

        private unsafe void AddUsedBones(Vector3I pos)
        {
            pos *= 2;
            int num = 0;
            while (num <= 2)
            {
                int num2 = 0;
                while (true)
                {
                    if (num2 > 2)
                    {
                        int* numPtr4 = (int*) ref pos.X;
                        numPtr4[0]++;
                        int* numPtr5 = (int*) ref pos.Y;
                        numPtr5[0] -= 3;
                        num++;
                        break;
                    }
                    int num3 = 0;
                    while (true)
                    {
                        if (num3 > 2)
                        {
                            int* numPtr2 = (int*) ref pos.Y;
                            numPtr2[0]++;
                            int* numPtr3 = (int*) ref pos.Z;
                            numPtr3[0] -= 3;
                            num2++;
                            break;
                        }
                        this.m_usedBones.Add(pos);
                        int* numPtr1 = (int*) ref pos.Z;
                        numPtr1[0]++;
                        num3++;
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        private void AssertBone(Vector3 value, float range)
        {
        }

        public void ClearBone(ref Vector3I pos)
        {
            this.Bones.Remove<Vector3I, Vector3>(pos);
        }

        public void CopyTo(MyGridSkeleton target, Vector3I fromGridPosition)
        {
            Vector3I vectori = fromGridPosition * 2;
            foreach (Vector3I vectori2 in BoneOffsets)
            {
                Vector3 vector;
                Vector3I key = (Vector3I) (vectori + vectori2);
                if (this.Bones.TryGetValue(key, out vector))
                {
                    target.Bones[key] = vector;
                }
            }
        }

        public void CopyTo(MyGridSkeleton target, MatrixI transformationMatrix, MyCubeGrid targetGrid)
        {
            MatrixI xi3;
            Matrix matrix;
            MatrixI rightMatrix = new MatrixI(new Vector3I(1, 1, 1), Base6Directions.Direction.Forward, Base6Directions.Direction.Up);
            MatrixI leftMatrix = new MatrixI(new Vector3I(-1, -1, -1), Base6Directions.Direction.Forward, Base6Directions.Direction.Up);
            transformationMatrix.Translation *= 2;
            MatrixI.Multiply(ref leftMatrix, ref transformationMatrix, out xi3);
            MatrixI.Multiply(ref xi3, ref rightMatrix, out transformationMatrix);
            transformationMatrix.GetBlockOrientation().GetMatrix(out matrix);
            foreach (KeyValuePair<Vector3I, Vector3> pair in this.Bones)
            {
                Vector3I vectori2;
                Vector3 vector;
                Vector3I key = pair.Key;
                Vector3I.Transform(ref key, ref transformationMatrix, out vectori2);
                Vector3 vector3 = Vector3.Transform(pair.Value, matrix);
                if (target.Bones.TryGetValue(vectori2, out vector))
                {
                    target.Bones[vectori2] = (vector + vector3) * 0.5f;
                }
                else
                {
                    target.Bones[vectori2] = vector3;
                }
                Vector3I vectori3 = (Vector3I) (vectori2 / 2);
                int x = -1;
                while (x <= 1)
                {
                    int y = -1;
                    while (true)
                    {
                        if (y > 1)
                        {
                            x++;
                            break;
                        }
                        int z = -1;
                        while (true)
                        {
                            if (z > 1)
                            {
                                y++;
                                break;
                            }
                            targetGrid.SetCubeDirty((Vector3I) (vectori3 + new Vector3I(x, y, z)));
                            z++;
                        }
                    }
                }
            }
        }

        public unsafe void CopyTo(MyGridSkeleton target, Vector3I fromGridPosition, Vector3I toGridPosition)
        {
            Vector3I vectori3;
            Vector3I vectori = fromGridPosition * 2;
            Vector3I vectori2 = (Vector3I) (((toGridPosition - fromGridPosition) + Vector3I.One) * 2);
            vectori3.X = 0;
            while (vectori3.X <= vectori2.X)
            {
                vectori3.Y = 0;
                while (true)
                {
                    if (vectori3.Y > vectori2.Y)
                    {
                        int* numPtr3 = (int*) ref vectori3.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori3.Z = 0;
                    while (true)
                    {
                        Vector3 vector;
                        if (vectori3.Z > vectori2.Z)
                        {
                            int* numPtr2 = (int*) ref vectori3.Y;
                            numPtr2[0]++;
                            break;
                        }
                        Vector3I key = (Vector3I) (vectori + vectori3);
                        if (this.Bones.TryGetValue(key, out vector))
                        {
                            target.Bones[key] = vector;
                        }
                        else
                        {
                            target.Bones.Remove<Vector3I, Vector3>(key);
                        }
                        int* numPtr1 = (int*) ref vectori3.Z;
                        numPtr1[0]++;
                    }
                }
            }
        }

        public void Deserialize(List<VRage.Game.BoneInfo> data, float boneRange, float gridSize, bool clear = false)
        {
            if (clear)
            {
                this.Bones.Clear();
            }
            foreach (VRage.Game.BoneInfo info in data)
            {
                this.Bones[(Vector3I) info.BonePosition] = Vector3UByte.Denormalize((Vector3UByte) info.BoneOffset, boneRange);
            }
        }

        public unsafe int DeserializePart(float boneRange, byte[] data, ref int dataIndex, out Vector3I minBone, out Vector3I maxBone)
        {
            minBone = new Vector3I(data, dataIndex);
            dataIndex += 12;
            maxBone = new Vector3I(data, dataIndex);
            dataIndex += 12;
            bool flag = data[dataIndex] > 0;
            dataIndex++;
            Vector3I vectori = (Vector3I) ((maxBone - minBone) + Vector3I.One);
            if (!flag || ((dataIndex + (vectori.Size * 3)) <= data.Length))
            {
                Vector3I vectori2;
                vectori2.X = minBone.X;
                while (vectori2.X <= maxBone.X)
                {
                    vectori2.Y = minBone.Y;
                    while (true)
                    {
                        if (vectori2.Y > maxBone.Y)
                        {
                            int* numPtr3 = (int*) ref vectori2.X;
                            numPtr3[0]++;
                            break;
                        }
                        vectori2.Z = minBone.Z;
                        while (true)
                        {
                            if (vectori2.Z > maxBone.Z)
                            {
                                int* numPtr2 = (int*) ref vectori2.Y;
                                numPtr2[0]++;
                                break;
                            }
                            if (!flag)
                            {
                                this.Bones.Remove<Vector3I, Vector3>(vectori2);
                            }
                            else
                            {
                                this[vectori2] = Vector3UByte.Denormalize(new Vector3UByte(data[dataIndex], data[dataIndex + 1], data[dataIndex + 2]), boneRange);
                                dataIndex += 3;
                            }
                            int* numPtr1 = (int*) ref vectori2.Z;
                            numPtr1[0]++;
                        }
                    }
                }
            }
            return dataIndex;
        }

        private void FixBone(Vector3I bonePosition, float gridSize, float minBoneDist = 0.05f)
        {
            Vector3 vector3;
            Vector3 vector4;
            Vector3 defaultBone = -Vector3.One * gridSize;
            Vector3 vector2 = Vector3.One * gridSize;
            vector3.X = this.TryGetBone(bonePosition - Vector3I.UnitX, ref defaultBone).X;
            vector3.Y = this.TryGetBone(bonePosition - Vector3I.UnitY, ref defaultBone).Y;
            vector3.Z = this.TryGetBone(bonePosition - Vector3I.UnitZ, ref defaultBone).Z;
            vector3 = (vector3 - new Vector3(gridSize / 2f)) + new Vector3(minBoneDist);
            vector4.X = this.TryGetBone((Vector3I) (bonePosition + Vector3I.UnitX), ref vector2).X;
            vector4.Y = this.TryGetBone((Vector3I) (bonePosition + Vector3I.UnitY), ref vector2).Y;
            vector4.Z = this.TryGetBone((Vector3I) (bonePosition + Vector3I.UnitZ), ref vector2).Z;
            this.Bones[bonePosition] = Vector3.Clamp(this.Bones[bonePosition], vector3, (vector4 + new Vector3(gridSize / 2f)) - new Vector3(minBoneDist));
        }

        public void FixBone(Vector3I gridPosition, Vector3I boneOffset, float gridSize, float minBoneDist = 0.05f)
        {
            this.FixBone((Vector3I) ((gridPosition * 2) + boneOffset), minBoneDist, 0.05f);
        }

        public unsafe void GetAffectedCubes(Vector3I cube, Vector3I boneOffset, List<Vector3I> resultList, MyCubeGrid grid)
        {
            Vector3I vectori3;
            Vector3I vectori = boneOffset - Vector3I.One;
            Vector3I vectori2 = Vector3I.Sign(vectori);
            vectori *= vectori2;
            vectori3.X = 0;
            while (vectori3.X <= vectori.X)
            {
                vectori3.Y = 0;
                while (true)
                {
                    if (vectori3.Y > vectori.Y)
                    {
                        int* numPtr3 = (int*) ref vectori3.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori3.Z = 0;
                    while (true)
                    {
                        if (vectori3.Z > vectori.Z)
                        {
                            int* numPtr2 = (int*) ref vectori3.Y;
                            numPtr2[0]++;
                            break;
                        }
                        Vector3I pos = (Vector3I) (cube + (vectori3 * vectori2));
                        if (grid.CubeExists(pos))
                        {
                            resultList.Add(pos);
                        }
                        int* numPtr1 = (int*) ref vectori3.Z;
                        numPtr1[0]++;
                    }
                }
            }
        }

        public void GetBone(ref Vector3I pos, out Vector3 bone)
        {
            if (!this.Bones.TryGetValue(pos, out bone))
            {
                bone = Vector3.Zero;
            }
        }

        public Vector3 GetBone(Vector3I cubePos, Vector3I bonePos)
        {
            Vector3 vector;
            return (this.Bones.TryGetValue((cubePos * 2) + bonePos, out vector) ? vector : Vector3.Zero);
        }

        private Vector3I GetCubeBoneOffset(Vector3I cubePos, Vector3I boneOffset)
        {
            Vector3I zero = Vector3I.Zero;
            if ((boneOffset.X % 2) != 0)
            {
                zero.X = 1;
            }
            else if ((boneOffset.X / 2) != cubePos.X)
            {
                zero.X = 2;
            }
            if ((boneOffset.Y % 2) != 0)
            {
                zero.Y = 1;
            }
            else if ((boneOffset.Y / 2) != cubePos.Y)
            {
                zero.Y = 2;
            }
            if ((boneOffset.Z % 2) != 0)
            {
                zero.Z = 1;
            }
            else if ((boneOffset.Z / 2) != cubePos.Z)
            {
                zero.Z = 2;
            }
            return zero;
        }

        private Vector3I? GetCubeFromBone(Vector3I bone, MyCubeGrid grid)
        {
            Vector3I zero = Vector3I.Zero;
            zero = (Vector3I) (bone / 2);
            if (grid.CubeExists(zero))
            {
                return new Vector3I?(zero);
            }
            int x = -1;
            while (x <= 1)
            {
                int y = -1;
                while (true)
                {
                    if (y > 1)
                    {
                        x++;
                        break;
                    }
                    int z = -1;
                    while (true)
                    {
                        if (z > 1)
                        {
                            y++;
                            break;
                        }
                        Vector3I pos = (Vector3I) (zero + new Vector3I(x, y, z));
                        Vector3I vectori3 = bone - (pos * 2);
                        if (((vectori3.X <= 2) && ((vectori3.Y <= 2) && (vectori3.Z <= 2))) && grid.CubeExists(pos))
                        {
                            return new Vector3I?(pos);
                        }
                        z++;
                    }
                }
            }
            return null;
        }

        private Vector3? GetDefinitionOffset(MySlimBlock cubeBlock, Vector3I bonePos)
        {
            Matrix matrix;
            Matrix matrix2;
            Vector3I vectori2;
            Vector3 vector;
            Vector3I position = bonePos - Vector3I.One;
            cubeBlock.Orientation.GetMatrix(out matrix);
            Matrix.Transpose(ref matrix, out matrix2);
            Vector3I.Transform(ref position, ref matrix2, out vectori2);
            vectori2 = (Vector3I) (vectori2 + Vector3I.One);
            if (cubeBlock.BlockDefinition.Bones.TryGetValue(vectori2, out vector))
            {
                Vector3 vector2;
                Vector3.Transform(ref vector, ref matrix, out vector2);
                return new Vector3?(vector2);
            }
            return null;
        }

        public Vector3 GetDefinitionOffsetWithNeighbours(Vector3I cubePos, Vector3I bonePos, MyCubeGrid grid)
        {
            Vector3I cubeBoneOffset = this.GetCubeBoneOffset(cubePos, bonePos);
            if (m_tempAffectedCubes == null)
            {
                m_tempAffectedCubes = new List<Vector3I>();
            }
            m_tempAffectedCubes.Clear();
            this.GetAffectedCubes(cubePos, cubeBoneOffset, m_tempAffectedCubes, grid);
            Vector3 zero = Vector3.Zero;
            int num = 0;
            foreach (Vector3I vectori2 in m_tempAffectedCubes)
            {
                MySlimBlock cubeBlock = grid.GetCubeBlock(vectori2);
                if ((cubeBlock != null) && (cubeBlock.BlockDefinition.Skeleton != null))
                {
                    Vector3? definitionOffset = this.GetDefinitionOffset(cubeBlock, this.GetCubeBoneOffset(vectori2, bonePos));
                    if (definitionOffset != null)
                    {
                        zero += definitionOffset.Value;
                        num++;
                    }
                }
            }
            return ((num != 0) ? (zero / ((float) num)) : zero);
        }

        public static float GetMaxBoneError(float gridSize) => 
            (MAX_BONE_ERROR * gridSize);

        public bool IsDeformed(Vector3I cube, float ignoredDeformation, MyCubeGrid cubeGrid, bool checkBlockDefinition)
        {
            float num = ignoredDeformation * ignoredDeformation;
            float maxBoneError = GetMaxBoneError(cubeGrid.GridSize);
            maxBoneError *= maxBoneError;
            foreach (Vector3I vectori in BoneOffsets)
            {
                Vector3 vector;
                if (this.Bones.TryGetValue((cube * 2) + vectori, out vector))
                {
                    if (checkBlockDefinition)
                    {
                        if (Math.Abs((float) (this.GetDefinitionOffsetWithNeighbours(cube, (Vector3I) ((cube * 2) + vectori), cubeGrid).LengthSquared() - vector.LengthSquared())) > maxBoneError)
                        {
                            return true;
                        }
                    }
                    else if (vector.LengthSquared() > num)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void MarkCubeRemoved(ref Vector3I pos)
        {
            this.m_tmpRemovedCubes.Add(pos);
        }

        public float MaxDeformation(Vector3I cube, MyCubeGrid cubeGrid)
        {
            float num = 0f;
            float maxBoneError = GetMaxBoneError(cubeGrid.GridSize);
            maxBoneError *= maxBoneError;
            foreach (Vector3I vectori in BoneOffsets)
            {
                Vector3 offset;
                Vector3I key = (Vector3I) ((cube * 2) + vectori);
                Vector3 vector = this.GetDefinitionOffsetWithNeighbours(cube, (Vector3I) ((cube * 2) + vectori), cubeGrid);
                float num4 = offset.LengthSquared();
                float num5 = Math.Abs((float) (vector.LengthSquared() - num4));
                if (num5 > num)
                {
                    num = num5;
                }
                if (!this.Bones.TryGetValue(key, out offset) && (num5 > maxBoneError))
                {
                    this.Bones.AddOrUpdate(key, offset, (k, v) => offset);
                    cubeGrid.AddDirtyBone(cube, vectori);
                }
            }
            return (float) Math.Sqrt((double) num);
        }

        public bool MultiplyBone(ref Vector3I pos, float factor, ref Vector3I cubePos, MyCubeGrid cubeGrid, float epsilon = 0.005f)
        {
            Vector3 vector;
            if (!this.Bones.TryGetValue(pos, out vector))
            {
                return false;
            }
            Vector3 vector2 = this.GetDefinitionOffsetWithNeighbours(cubePos, pos, cubeGrid);
            factor = 1f - factor;
            if (factor < 0.1f)
            {
                factor = 0.1f;
            }
            Vector3 vector3 = Vector3.Lerp(vector, vector2, factor);
            if (vector3.LengthSquared() < (epsilon * epsilon))
            {
                this.Bones.Remove<Vector3I, Vector3>(pos);
            }
            else
            {
                this.Bones[pos] = vector3;
            }
            return true;
        }

        public unsafe void RemoveUnusedBones(MyCubeGrid grid)
        {
            if (this.m_tmpRemovedCubes.Count != 0)
            {
                foreach (Vector3I vectori in this.m_tmpRemovedCubes)
                {
                    if (grid.CubeExists(vectori))
                    {
                        if (this.m_testedCubes.Contains(vectori))
                        {
                            continue;
                        }
                        this.m_testedCubes.Add(vectori);
                        this.AddUsedBones(vectori);
                        continue;
                    }
                    Vector3 vector1 = (vectori * 2) + Vector3I.One;
                    int num = -1;
                    while (num <= 1)
                    {
                        int num2 = -1;
                        while (true)
                        {
                            if (num2 > 1)
                            {
                                num++;
                                break;
                            }
                            int num3 = -1;
                            while (true)
                            {
                                Vector3I vectori2;
                                if (num3 > 1)
                                {
                                    num2++;
                                    break;
                                }
                                vectori2.X = num;
                                vectori2.Y = num2;
                                vectori2.Z = num3;
                                Vector3I pos = (Vector3I) (vectori + vectori2);
                                if (grid.CubeExists(pos) && !this.m_testedCubes.Contains(pos))
                                {
                                    this.m_testedCubes.Add(pos);
                                    this.AddUsedBones(pos);
                                }
                                num3++;
                            }
                        }
                    }
                }
                using (List<Vector3I>.Enumerator enumerator = this.m_tmpRemovedCubes.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Vector3I item = enumerator.Current * 2;
                        int num4 = 0;
                        while (num4 <= 2)
                        {
                            int num5 = 0;
                            while (true)
                            {
                                if (num5 > 2)
                                {
                                    int* numPtr4 = (int*) ref item.X;
                                    numPtr4[0]++;
                                    int* numPtr5 = (int*) ref item.Y;
                                    numPtr5[0] -= 3;
                                    num4++;
                                    break;
                                }
                                int num6 = 0;
                                while (true)
                                {
                                    if (num6 > 2)
                                    {
                                        int* numPtr2 = (int*) ref item.Y;
                                        numPtr2[0]++;
                                        int* numPtr3 = (int*) ref item.Z;
                                        numPtr3[0] -= 3;
                                        num5++;
                                        break;
                                    }
                                    if (!this.m_usedBones.Contains(item))
                                    {
                                        this.ClearBone(ref item);
                                    }
                                    int* numPtr1 = (int*) ref item.Z;
                                    numPtr1[0]++;
                                    num6++;
                                }
                            }
                        }
                    }
                }
                this.m_testedCubes.Clear();
                this.m_usedBones.Clear();
                this.m_tmpRemovedCubes.Clear();
            }
        }

        public void Reset()
        {
            this.Bones.Clear();
        }

        public void Serialize(List<VRage.Game.BoneInfo> result, float boneRange, MyCubeGrid grid)
        {
            VRage.Game.BoneInfo item = new VRage.Game.BoneInfo();
            float maxBoneError = GetMaxBoneError(grid.GridSize);
            maxBoneError *= maxBoneError;
            foreach (KeyValuePair<Vector3I, Vector3> pair in this.Bones)
            {
                Vector3I? cubeFromBone = this.GetCubeFromBone(pair.Key, grid);
                if ((cubeFromBone != null) && (Math.Abs((float) (this.GetDefinitionOffsetWithNeighbours(cubeFromBone.Value, pair.Key, grid).LengthSquared() - pair.Value.LengthSquared())) > maxBoneError))
                {
                    item.BonePosition = pair.Key;
                    item.BoneOffset = Vector3UByte.Normalize(pair.Value, boneRange);
                    if (!Vector3UByte.IsMiddle((Vector3UByte) item.BoneOffset))
                    {
                        result.Add(item);
                    }
                }
            }
        }

        public unsafe bool SerializePart(Vector3I minBone, Vector3I maxBone, float boneRange, List<byte> result)
        {
            Vector3I vectori;
            bool flag = false;
            minBone.ToBytes(result);
            maxBone.ToBytes(result);
            int count = result.Count;
            result.Add(1);
            vectori.X = minBone.X;
            while (vectori.X <= maxBone.X)
            {
                vectori.Y = minBone.Y;
                while (true)
                {
                    if (vectori.Y > maxBone.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = minBone.Z;
                    while (true)
                    {
                        Vector3 vector;
                        if (vectori.Z > maxBone.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        flag |= this.Bones.TryGetValue(vectori, out vector);
                        Vector3UByte num2 = Vector3UByte.Normalize(vector, boneRange);
                        result.Add(num2.X);
                        result.Add(num2.Y);
                        result.Add(num2.Z);
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
            if (!flag)
            {
                result.RemoveRange(count, result.Count - count);
                result.Add(0);
            }
            return flag;
        }

        public void SetBone(ref Vector3I pos, ref Vector3 bone)
        {
            this.Bones[pos] = bone;
        }

        public void SetOrClearBone(ref Vector3I pos, ref Vector3 bone)
        {
            if (bone == Vector3.Zero)
            {
                this.Bones.Remove<Vector3I, Vector3>(pos);
            }
            else
            {
                this.Bones[pos] = bone;
            }
        }

        public bool TryGetBone(ref Vector3I pos, out Vector3 bone) => 
            this.Bones.TryGetValue(pos, out bone);

        private Vector3 TryGetBone(Vector3I bonePosition, ref Vector3 defaultBone)
        {
            Vector3 vector;
            return (!this.Bones.TryGetValue(bonePosition, out vector) ? defaultBone : vector);
        }

        public void Wrap(ref Vector3I cube, ref Vector3I boneOffset)
        {
            Vector3I vectori = (Vector3I) ((cube * 2) + boneOffset);
            cube = Vector3I.Floor((Vector3D) (vectori / 2));
            boneOffset = vectori - (cube * 2);
        }

        public Vector3 this[Vector3I pos]
        {
            get
            {
                Vector3 vector;
                return (!this.Bones.TryGetValue(pos, out vector) ? Vector3.Zero : vector);
            }
            set => 
                (this.Bones[pos] = value);
        }

        public bool NeedsPerFrameUpdate =>
            (this.m_tmpRemovedCubes.Count > 0);
    }
}

