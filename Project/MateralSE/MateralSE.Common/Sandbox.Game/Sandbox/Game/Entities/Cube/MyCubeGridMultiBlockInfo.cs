namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyCubeGridMultiBlockInfo
    {
        private static List<MyMultiBlockDefinition.MyMultiBlockPartDefinition> m_tmpPartDefinitions = new List<MyMultiBlockDefinition.MyMultiBlockPartDefinition>();
        public int MultiBlockId;
        public MyMultiBlockDefinition MultiBlockDefinition;
        public MyCubeBlockDefinition MainBlockDefinition;
        public HashSet<MySlimBlock> Blocks = new HashSet<MySlimBlock>();

        public bool CanAddBlock(ref Vector3I otherGridPositionMin, ref Vector3I otherGridPositionMax, MyBlockOrientation otherOrientation, MyCubeBlockDefinition otherDefinition)
        {
            MatrixI xi;
            bool flag2;
            if (!this.GetTransform(out xi))
            {
                return true;
            }
            try
            {
                MatrixI xi2;
                MatrixI.Invert(ref xi, out xi2);
                Vector3I vectori1 = Vector3I.Transform(otherGridPositionMin, ref xi2);
                Vector3I vectori = Vector3I.Transform(otherGridPositionMax, ref xi2);
                Vector3I minB = Vector3I.Min(vectori1, vectori);
                Vector3I maxB = Vector3I.Max(vectori1, vectori);
                if (Vector3I.BoxIntersects(ref this.MultiBlockDefinition.Min, ref this.MultiBlockDefinition.Max, ref minB, ref maxB))
                {
                    MatrixI xi4;
                    MatrixI leftMatrix = new MatrixI(otherOrientation);
                    MatrixI.Multiply(ref leftMatrix, ref xi2, out xi4);
                    MyBlockOrientation orientation = new MyBlockOrientation(xi4.Forward, xi4.Up);
                    m_tmpPartDefinitions.Clear();
                    MyMultiBlockDefinition.MyMultiBlockPartDefinition[] blockDefinitions = this.MultiBlockDefinition.BlockDefinitions;
                    int index = 0;
                    while (true)
                    {
                        if (index < blockDefinitions.Length)
                        {
                            MyMultiBlockDefinition.MyMultiBlockPartDefinition item = blockDefinitions[index];
                            if (Vector3I.BoxIntersects(ref item.Min, ref item.Max, ref minB, ref maxB))
                            {
                                if (!(minB == maxB))
                                {
                                    break;
                                }
                                if (!(item.Min == item.Max))
                                {
                                    break;
                                }
                                m_tmpPartDefinitions.Add(item);
                            }
                            index++;
                            continue;
                        }
                        if (m_tmpPartDefinitions.Count == 0)
                        {
                            flag2 = true;
                        }
                        else
                        {
                            bool flag = true;
                            foreach (MyMultiBlockDefinition.MyMultiBlockPartDefinition definition2 in m_tmpPartDefinitions)
                            {
                                MyCubeBlockDefinition definition3;
                                if (!MyDefinitionManager.Static.TryGetCubeBlockDefinition(definition2.Id, out definition3))
                                {
                                    continue;
                                }
                                if (definition3 != null)
                                {
                                    flag &= MyCompoundCubeBlock.CanAddBlocks(definition3, new MyBlockOrientation(definition2.Forward, definition2.Up), otherDefinition, orientation);
                                    if (!flag)
                                    {
                                        break;
                                    }
                                }
                            }
                            flag2 = flag;
                        }
                        return flag2;
                    }
                    flag2 = false;
                }
                else
                {
                    flag2 = true;
                }
            }
            finally
            {
                m_tmpPartDefinitions.Clear();
            }
            return flag2;
        }

        public bool GetBoundingBox(out Vector3I min, out Vector3I max)
        {
            MatrixI xi;
            min = new Vector3I();
            max = new Vector3I();
            if (!this.GetTransform(out xi))
            {
                return false;
            }
            Vector3I vectori = Vector3I.Transform(this.MultiBlockDefinition.Min, xi);
            Vector3I vectori2 = Vector3I.Transform(this.MultiBlockDefinition.Max, xi);
            min = Vector3I.Min(vectori, vectori2);
            max = Vector3I.Max(vectori, vectori2);
            return true;
        }

        public bool GetMissingBlocks(out MatrixI transform, List<int> multiBlockIndices)
        {
            for (int i = 0; i < this.MultiBlockDefinition.BlockDefinitions.Length; i++)
            {
                if (!this.Blocks.Any<MySlimBlock>(b => (b.MultiBlockIndex == i)))
                {
                    multiBlockIndices.Add(i);
                }
            }
            return this.GetTransform(out transform);
        }

        public float GetTotalMaxIntegrity()
        {
            float num = 0f;
            foreach (MySlimBlock block in this.Blocks)
            {
                num += block.MaxIntegrity;
            }
            return num;
        }

        public bool GetTransform(out MatrixI transform)
        {
            transform = new MatrixI();
            if (this.Blocks.Count != 0)
            {
                MySlimBlock block = this.Blocks.First<MySlimBlock>();
                if (block.MultiBlockIndex < this.MultiBlockDefinition.BlockDefinitions.Length)
                {
                    MyMultiBlockDefinition.MyMultiBlockPartDefinition definition = this.MultiBlockDefinition.BlockDefinitions[block.MultiBlockIndex];
                    transform = MatrixI.CreateRotation(definition.Forward, definition.Up, block.Orientation.Forward, block.Orientation.Up);
                    transform.Translation = block.Position - Vector3I.TransformNormal(definition.Min, ref transform);
                    return true;
                }
            }
            return false;
        }

        public bool IsFractured()
        {
            using (HashSet<MySlimBlock>.Enumerator enumerator = this.Blocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    if (enumerator.Current.GetFractureComponent() != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

