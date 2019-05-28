namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyAnimationInverseKinematics
    {
        private readonly List<MyAnimationIkChainExt> m_feet = new List<MyAnimationIkChainExt>(2);
        private bool[] m_ignoredBonesTable;
        private readonly HashSet<string> m_ignoredBoneNames = new HashSet<string>();
        private float m_characterDirDownOffset;
        private const float m_characterDirDownOffsetMin = -0.3f;
        private const float m_characterDirDownOffsetMax = 0.2f;
        private float m_characterDirDownOffsetSmoothness = 0.5f;
        private float m_currentFeetIkInfluence;
        private const float m_poleVectorChangeSmoothness = 0.85f;
        private const int m_offsetFilteringSampleCount = 30;
        private int m_offsetFilteringCursor;
        private float m_filteredOffsetValue;
        private readonly List<float> m_offsetFiltering = new List<float>(30);
        private static readonly int[] m_boneIndicesPreallocated = new int[0x40];
        public static MatrixD DebugTransform;
        private static bool m_showDebugDrawings = false;
        private readonly List<Matrix> m_ignoredBonesBackup = new List<Matrix>(8);
        private float m_rootBoneVerticalOffset;

        private void BackupIgnoredBones(MyCharacterBone[] characterBones)
        {
            this.m_ignoredBonesBackup.Clear();
            for (int i = 0; i < characterBones.Length; i++)
            {
                if (this.m_ignoredBonesTable[i])
                {
                    this.m_ignoredBonesBackup.Add(characterBones[i].AbsoluteTransform);
                }
            }
        }

        public void Clear()
        {
            this.m_characterDirDownOffset = 0f;
            this.m_feet.Clear();
            this.m_ignoredBoneNames.Clear();
            this.ClearCharacterOffsetFilteringSamples();
        }

        public void ClearCharacterOffsetFilteringSamples()
        {
            this.m_offsetFiltering.Clear();
        }

        private unsafe void MoveTheBodyDown(MyCharacterBone[] characterBones, bool allowMoving)
        {
            if (!allowMoving)
            {
                this.m_filteredOffsetValue = 0f;
            }
            else
            {
                if (this.m_offsetFiltering.Count != 30)
                {
                    this.m_offsetFiltering.Add(this.m_characterDirDownOffset);
                }
                else
                {
                    int offsetFilteringCursor = this.m_offsetFilteringCursor;
                    this.m_offsetFilteringCursor = offsetFilteringCursor + 1;
                    this.m_offsetFiltering[offsetFilteringCursor] = this.m_characterDirDownOffset;
                    if (this.m_offsetFilteringCursor == 30)
                    {
                        this.m_offsetFilteringCursor = 0;
                    }
                }
                float minValue = float.MinValue;
                foreach (float num3 in this.m_offsetFiltering)
                {
                    minValue = Math.Max(minValue, num3);
                }
                this.m_filteredOffsetValue = minValue;
            }
            if (this.m_offsetFilteringCursor >= 30)
            {
                this.m_offsetFilteringCursor = 0;
            }
            MyCharacterBone parent = characterBones[0];
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            Vector3 translation = parent.Translation;
            this.m_rootBoneVerticalOffset = this.m_filteredOffsetValue * this.m_currentFeetIkInfluence;
            float* singlePtr1 = (float*) ref translation.Y;
            singlePtr1[0] += this.m_rootBoneVerticalOffset;
            parent.Translation = translation;
            parent.ComputeAbsoluteTransform(true);
        }

        private void RecreateIgnoredBonesTableIfNeeded(MyCharacterBone[] characterBones)
        {
            if ((this.m_ignoredBonesTable == null) && (characterBones != null))
            {
                this.m_ignoredBonesTable = new bool[characterBones.Length];
                for (int i = 0; i < characterBones.Length; i++)
                {
                    this.m_ignoredBonesTable[i] = this.m_ignoredBoneNames.Contains(characterBones[i].Name);
                }
            }
        }

        public void RegisterFootBone(string boneName, int boneChainLength, bool alignBoneWithTerrain)
        {
            MyAnimationIkChainExt item = new MyAnimationIkChainExt();
            item.BoneIndex = -1;
            item.BoneName = boneName;
            item.ChainLength = boneChainLength;
            item.AlignBoneWithTerrain = alignBoneWithTerrain;
            this.m_feet.Add(item);
        }

        public void RegisterIgnoredBone(string boneName)
        {
            this.m_ignoredBoneNames.Add(boneName);
            this.m_ignoredBonesTable = null;
        }

        public void ResetIkInfluence()
        {
            this.m_currentFeetIkInfluence = 0f;
        }

        private void RestoreIgnoredBones(MyCharacterBone[] characterBones)
        {
            int num = 0;
            for (int i = 0; i < characterBones.Length; i++)
            {
                if (this.m_ignoredBonesTable[i])
                {
                    num++;
                    characterBones[i].SetCompleteTransformFromAbsoluteMatrix(this.m_ignoredBonesBackup[num], false);
                    characterBones[i].ComputeAbsoluteTransform(true);
                }
            }
        }

        public unsafe void SolveFeet(bool enabled, MyCharacterBone[] characterBones, bool allowMovingWithBody)
        {
            float num2;
            bool flag;
            this.m_currentFeetIkInfluence = MathHelper.Clamp((float) (this.m_currentFeetIkInfluence + (enabled ? 0.1f : -0.1f)), (float) 0f, (float) 1f);
            if (this.m_currentFeetIkInfluence <= 0f)
            {
                return;
            }
            else if (((this.TerrainHeightProvider != null) && ((characterBones != null) && (characterBones.Length != 0))) && (this.m_feet.Count != 0))
            {
                this.RecreateIgnoredBonesTableIfNeeded(characterBones);
                this.BackupIgnoredBones(characterBones);
                this.MoveTheBodyDown(characterBones, allowMovingWithBody);
                this.RestoreIgnoredBones(characterBones);
                float referenceTerrainHeight = this.TerrainHeightProvider.GetReferenceTerrainHeight();
                num2 = 0.2f;
                flag = false;
                using (List<MyAnimationIkChainExt>.Enumerator enumerator = this.m_feet.GetEnumerator())
                {
                    while (true)
                    {
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                MyAnimationIkChainExt current = enumerator.Current;
                                if (current.BoneIndex == -1)
                                {
                                    MyCharacterBone[] boneArray = characterBones;
                                    int index = 0;
                                    do
                                    {
                                        if (index < boneArray.Length)
                                        {
                                            MyCharacterBone bone4 = boneArray[index];
                                            if (bone4.Name != current.BoneName)
                                            {
                                                index++;
                                                continue;
                                            }
                                            current.BoneIndex = bone4.Index;
                                        }
                                    }
                                    while (current.BoneIndex == -1);
                                }
                                MyCharacterBone bone = characterBones[current.BoneIndex];
                                MyCharacterBone parent = bone;
                                MyCharacterBone bone3 = bone;
                                int num5 = 0;
                                while (true)
                                {
                                    if (num5 >= current.ChainLength)
                                    {
                                        float lastTerrainHeight;
                                        Vector3 vector2;
                                        parent.ComputeAbsoluteTransform(true);
                                        Vector3 translation = bone.AbsoluteTransform.Translation;
                                        Vector3 boneRigPosition = bone.GetAbsoluteRigTransform().Translation;
                                        if (this.TerrainHeightProvider.GetTerrainHeight(translation, boneRigPosition, out lastTerrainHeight, out vector2))
                                        {
                                            vector2 = Vector3.Lerp(current.LastTerrainNormal, vector2, 0.2f);
                                        }
                                        else
                                        {
                                            lastTerrainHeight = current.LastTerrainHeight;
                                            vector2 = Vector3.Lerp(current.LastTerrainNormal, Vector3.Up, 0.1f);
                                            current.LastTerrainHeight *= 0.9f;
                                        }
                                        current.LastTerrainHeight = lastTerrainHeight;
                                        current.LastTerrainNormal = vector2;
                                        float y = translation.Y;
                                        float single1 = lastTerrainHeight - referenceTerrainHeight;
                                        float num7 = single1 + ((translation.Y - this.m_filteredOffsetValue) / vector2.Y);
                                        translation.Y = Math.Min(bone3.AbsoluteTransform.Translation.Y, num7);
                                        Vector3* vectorPtr1 = (Vector3*) ref translation;
                                        vectorPtr1->Y = MathHelper.Lerp(y, translation.Y, this.m_currentFeetIkInfluence);
                                        num2 = MathHelper.Clamp(single1, -0.3f, num2);
                                        flag = true;
                                        if (y > translation.Y)
                                        {
                                            translation.Y = y;
                                        }
                                        if (translation.Y < (boneRigPosition.Y + this.m_filteredOffsetValue))
                                        {
                                            translation.Y = boneRigPosition.Y + this.m_filteredOffsetValue;
                                        }
                                        SolveIkTwoBones(characterBones, current, ref translation, ref vector2, false);
                                        break;
                                    }
                                    bone3 = parent;
                                    parent = parent.Parent;
                                    num5++;
                                }
                            }
                            else
                            {
                                goto TR_0003;
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        TR_0003:
            if (!flag)
            {
                num2 = 0f;
            }
            this.m_characterDirDownOffset = MathHelper.Lerp(num2, this.m_characterDirDownOffset, (this.m_characterDirDownOffsetSmoothness * this.m_offsetFiltering.Count) / 30f);
        }

        public static bool SolveIkCcd(MyCharacterBone[] characterBones, int boneIndex, int chainLength, ref Vector3D finalPosition)
        {
            Vector3 vector = (Vector3) finalPosition;
            int num = 0;
            int num2 = 50;
            float num3 = 2.5E-05f;
            MyCharacterBone bone = characterBones[boneIndex];
            MyCharacterBone parent = bone;
            int[] boneIndicesPreallocated = m_boneIndicesPreallocated;
            int index = 0;
            while (true)
            {
                if (index < chainLength)
                {
                    if (parent != null)
                    {
                        boneIndicesPreallocated[index] = parent.Index;
                        parent = parent.Parent;
                        index++;
                        continue;
                    }
                    chainLength = index;
                }
                Vector3 translation = bone.AbsoluteTransform.Translation;
                float num5 = 1f / ((float) Vector3D.DistanceSquared(translation, vector));
                while (true)
                {
                    int num6 = 0;
                    while (true)
                    {
                        if (num6 >= chainLength)
                        {
                            num++;
                            if ((num < num2) && (Vector3D.DistanceSquared(translation, vector) > num3))
                            {
                                break;
                            }
                            return (Vector3D.DistanceSquared(translation, vector) <= num3);
                        }
                        MyCharacterBone bone3 = characterBones[boneIndicesPreallocated[num6]];
                        bone.ComputeAbsoluteTransform(true);
                        Matrix absoluteTransform = bone3.AbsoluteTransform;
                        Vector3 vector3 = absoluteTransform.Translation;
                        translation = bone.AbsoluteTransform.Translation;
                        double num7 = Vector3D.DistanceSquared(translation, vector);
                        if (num7 > num3)
                        {
                            Vector3 vector4 = translation - vector3;
                            Vector3 v = vector - vector3;
                            double num8 = vector4.LengthSquared();
                            double num9 = v.LengthSquared();
                            double num10 = vector4.Dot(v);
                            if ((num10 < 0.0) || ((num10 * num10) < ((num8 * num9) * 0.99998998641967773)))
                            {
                                Matrix matrix3;
                                Vector3 toVector = Vector3.Lerp(vector4, v, 1f / ((num5 * ((float) num7)) + 1f));
                                Matrix.CreateRotationFromTwoVectors(ref vector4, ref toVector, out matrix3);
                                Matrix matrix = Matrix.Normalize(absoluteTransform);
                                Matrix identity = Matrix.Identity;
                                if (bone3.Parent != null)
                                {
                                    identity = bone3.Parent.AbsoluteTransform;
                                }
                                identity = Matrix.Normalize(identity);
                                bone3.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Multiply(matrix.GetOrientation() * matrix3, Matrix.Invert(bone3.BindTransform * identity)));
                                bone3.ComputeAbsoluteTransform(true);
                            }
                        }
                        num6++;
                    }
                }
            }
        }

        public static unsafe bool SolveIkTwoBones(MyCharacterBone[] characterBones, MyAnimationIkChainExt ikChain, ref Vector3 finalPosition, ref Vector3 finalNormal, bool fromBindPose)
        {
            Vector3 left;
            Vector3 vector7;
            Vector2 vector14;
            Vector2* vectorPtr1;
            int boneIndex = ikChain.BoneIndex;
            float min = MathHelper.ToRadians(ikChain.MinEndPointRotation);
            float max = MathHelper.ToRadians(ikChain.MaxEndPointRotation);
            if (!ikChain.LastPoleVector.IsValid())
            {
                left = Vector3.Left;
            }
            int chainLength = ikChain.ChainLength;
            bool alignBoneWithTerrain = ikChain.AlignBoneWithTerrain;
            MyCharacterBone bone = characterBones[boneIndex];
            if (bone == null)
            {
                return false;
            }
            MyCharacterBone parent = bone.Parent;
            for (int i = 2; i < chainLength; i++)
            {
                parent = parent.Parent;
            }
            if (parent == null)
            {
                return false;
            }
            MyCharacterBone bone3 = parent.Parent;
            if (bone3 == null)
            {
                return false;
            }
            if (fromBindPose)
            {
                bone3.SetCompleteBindTransform();
                parent.SetCompleteBindTransform();
                bone.SetCompleteBindTransform();
                bone3.ComputeAbsoluteTransform(true);
            }
            Matrix absoluteTransform = bone.AbsoluteTransform;
            Vector3 translation = bone3.AbsoluteTransform.Translation;
            Vector3 position = parent.AbsoluteTransform.Translation;
            Vector3 vector4 = bone.AbsoluteTransform.Translation;
            Vector3 vector5 = position - translation;
            Vector3 vector6 = finalPosition - translation;
            Vector3 vector8 = vector4 - translation;
            Vector3.Cross(ref vector5, ref vector8, out vector7);
            vector7.Normalize();
            vector7 = Vector3.Normalize(Vector3.Lerp(vector7, left, 0.85f));
            Vector3 vector9 = Vector3.Normalize(vector6);
            Vector3 vector10 = Vector3.Normalize(Vector3.Cross(vector9, vector7));
            float x = vector10.Dot(ref vector5);
            Vector2 vector11 = new Vector2(x, vector9.Dot(ref vector5));
            float single2 = vector10.Dot(ref vector8);
            float single3 = vector10.Dot(ref vector6);
            Vector2 vector12 = new Vector2(single3, vector9.Dot(ref vector6));
            Vector2 vector13 = new Vector2(vector10.Dot(ref finalNormal), vector9.Dot(ref finalNormal));
            float num5 = vector11.Length();
            float num6 = (new Vector2(single2, vector9.Dot(ref vector8)) - vector11).Length();
            float num7 = vector12.Length();
            if ((num5 + num6) <= num7)
            {
                vector12 = (Vector2) (((num5 + num6) * vector12) / num7);
            }
            vector14.Y = (((vector12.Y * vector12.Y) - (num6 * num6)) + (num5 * num5)) / (2f * vector12.Y);
            float num9 = (num5 * num5) - (vector14.Y * vector14.Y);
            vectorPtr1->X = (float) Math.Sqrt((num9 > 0f) ? ((double) num9) : ((double) 0f));
            vectorPtr1 = (Vector2*) ref vector14;
            Vector3 vector15 = (translation + (vector10 * vector14.X)) + (vector9 * vector14.Y);
            Vector3 secondVector = finalPosition - vector15;
            Vector3 vector18 = (vector10 * vector13.X) + (vector9 * vector13.Y);
            vector18.Normalize();
            Matrix absoluteMatrix = bone3.AbsoluteTransform;
            Quaternion rotation = Quaternion.CreateFromTwoVectors(vector5, vector15 - translation);
            Matrix* matrixPtr1 = (Matrix*) ref absoluteMatrix;
            matrixPtr1.Right = Vector3.Transform(absoluteMatrix.Right, rotation);
            Matrix* matrixPtr2 = (Matrix*) ref absoluteMatrix;
            matrixPtr2.Up = Vector3.Transform(absoluteMatrix.Up, rotation);
            Matrix* matrixPtr3 = (Matrix*) ref absoluteMatrix;
            matrixPtr3.Forward = Vector3.Transform(absoluteMatrix.Forward, rotation);
            bone3.SetCompleteTransformFromAbsoluteMatrix(ref absoluteMatrix, true);
            bone3.ComputeAbsoluteTransform(true);
            Matrix matrix3 = parent.AbsoluteTransform;
            Quaternion quaternion2 = Quaternion.CreateFromTwoVectors(bone.AbsoluteTransform.Translation - parent.AbsoluteTransform.Translation, secondVector);
            Matrix* matrixPtr4 = (Matrix*) ref matrix3;
            matrixPtr4.Right = Vector3.Transform(matrix3.Right, quaternion2);
            Matrix* matrixPtr5 = (Matrix*) ref matrix3;
            matrixPtr5.Up = Vector3.Transform(matrix3.Up, quaternion2);
            Matrix* matrixPtr6 = (Matrix*) ref matrix3;
            matrixPtr6.Forward = Vector3.Transform(matrix3.Forward, quaternion2);
            parent.SetCompleteTransformFromAbsoluteMatrix(ref matrix3, true);
            parent.ComputeAbsoluteTransform(true);
            if (ikChain.EndBoneTransform != null)
            {
                MatrixD xd = ikChain.EndBoneTransform.Value * MatrixD.Invert(bone.BindTransform * bone.Parent.AbsoluteTransform);
                bone.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.Normalize((Matrix) xd.GetOrientation()));
                bone.Translation = (Vector3) xd.Translation;
                bone.ComputeAbsoluteTransform(true);
            }
            else if (alignBoneWithTerrain)
            {
                Matrix identity;
                Vector3 vector20;
                Vector3.Cross(ref vector18, ref Vector3.Up, out vector20);
                float epsilon = 0.2f;
                if (!MyUtils.IsValid(vector20) || MyUtils.IsZero(vector20, epsilon))
                {
                    identity = Matrix.Identity;
                }
                else
                {
                    float angleBetweenVectors = MyUtils.GetAngleBetweenVectors(vector18, Vector3.Up);
                    if (vector20.Dot(vector7) > 0f)
                    {
                        angleBetweenVectors = -angleBetweenVectors;
                    }
                    Matrix.CreateFromAxisAngle(ref vector7, MathHelper.Clamp(angleBetweenVectors, min, max), out identity);
                }
                ikChain.LastAligningRotationMatrix = Matrix.Lerp(ikChain.LastAligningRotationMatrix, identity, ikChain.AligningSmoothness);
                Matrix matrix6 = absoluteTransform.GetOrientation() * ikChain.LastAligningRotationMatrix;
                matrix6.Translation = bone.AbsoluteTransform.Translation;
                bone.SetCompleteTransformFromAbsoluteMatrix(ref matrix6, true);
                bone.ComputeAbsoluteTransform(true);
            }
            if (m_showDebugDrawings)
            {
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(translation, ref DebugTransform), Vector3D.Transform(position, ref DebugTransform), Color.Yellow, Color.Red, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(position, ref DebugTransform), Vector3D.Transform(vector4, ref DebugTransform), Color.Yellow, Color.Red, false, false);
                MyRenderProxy.DebugDrawSphere(Vector3D.Transform(finalPosition, ref DebugTransform), 0.05f, Color.Cyan, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(position, ref DebugTransform), Vector3D.Transform(position + vector7, ref DebugTransform), Color.PaleGreen, Color.PaleGreen, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(translation, ref DebugTransform), Vector3D.Transform(translation + vector10, ref DebugTransform), Color.White, Color.White, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(translation, ref DebugTransform), Vector3D.Transform(translation + vector9, ref DebugTransform), Color.White, Color.White, false, false);
                MyRenderProxy.DebugDrawSphere(Vector3D.Transform(vector15, ref DebugTransform), 0.05f, Color.Green, 1f, false, false, true, false);
                MyRenderProxy.DebugDrawAxis(bone3.AbsoluteTransform * DebugTransform, 0.5f, false, false, false);
                MyRenderProxy.DebugDrawLine3D(Vector3D.Transform(finalPosition, ref DebugTransform), Vector3D.Transform(finalPosition + vector18, ref DebugTransform), Color.Black, Color.LightBlue, false, false);
                MyRenderProxy.DebugDrawArrow3D(Vector3D.Transform(position, ref DebugTransform), Vector3D.Transform(vector15, ref DebugTransform), Color.Green, new Color?(Color.White), false, 0.1, null, 0.5f, false);
            }
            ikChain.LastPoleVector = vector7;
            return true;
        }

        public ListReader<MyAnimationIkChainExt> Feet =>
            this.m_feet;

        public IMyTerrainHeightProvider TerrainHeightProvider { get; set; }

        public float RootBoneVerticalOffset =>
            this.m_rootBoneVerticalOffset;
    }
}

