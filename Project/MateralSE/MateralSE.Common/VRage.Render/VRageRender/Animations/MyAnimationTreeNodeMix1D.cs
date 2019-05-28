namespace VRageRender.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRageMath;

    public class MyAnimationTreeNodeMix1D : MyAnimationTreeNode
    {
        private float? m_lastKnownParamValue;
        private int m_lastKnownFrameCounter = -1;
        public List<MyParameterNodeMapping> ChildMappings = new List<MyParameterNodeMapping>(4);
        public MyStringId ParameterName;
        public bool Circular;
        public float Sensitivity = 1f;
        public float MaxChange = float.PositiveInfinity;

        private float ComputeParamValue(ref MyAnimationUpdateData data)
        {
            float num4;
            float paramValueBinding = this.ChildMappings[0].ParamValueBinding;
            float num2 = this.ChildMappings[this.ChildMappings.Count - 1].ParamValueBinding;
            float num3 = 0.001f * (num2 - paramValueBinding);
            data.Controller.Variables.GetValue(this.ParameterName, out num4);
            if ((this.m_lastKnownParamValue == null) || ((data.Controller.FrameCounter - this.m_lastKnownFrameCounter) > 1))
            {
                this.m_lastKnownParamValue = new float?(num4);
            }
            else if (!this.Circular)
            {
                float single1 = num4 - this.m_lastKnownParamValue.Value;
                float num11 = ((single1 * single1) <= (this.MaxChange * this.MaxChange)) ? MathHelper.Lerp(this.m_lastKnownParamValue.Value, num4, this.Sensitivity) : num4;
                if (((this.m_lastKnownParamValue.Value - num11) * (this.m_lastKnownParamValue.Value - num11)) > (num3 * num3))
                {
                    this.m_lastKnownParamValue = new float?(num11);
                }
            }
            else
            {
                float num5 = this.m_lastKnownParamValue.Value;
                float num6 = num4 - this.m_lastKnownParamValue.Value;
                num6 *= num6;
                float num7 = num6;
                float num8 = num4 - ((this.m_lastKnownParamValue.Value + num2) - paramValueBinding);
                if ((num8 * num8) < num6)
                {
                    num5 = (this.m_lastKnownParamValue.Value + num2) - paramValueBinding;
                    num7 = num8 * num8;
                }
                float num9 = num4 - ((this.m_lastKnownParamValue.Value - num2) + paramValueBinding);
                if ((num9 * num9) < num7)
                {
                    num5 = (this.m_lastKnownParamValue.Value - num2) + paramValueBinding;
                    num7 = num9 * num9;
                }
                float num10 = (num7 <= (this.MaxChange * this.MaxChange)) ? MathHelper.Lerp(num5, num4, this.Sensitivity) : num4;
                while (true)
                {
                    if (num10 >= paramValueBinding)
                    {
                        while (true)
                        {
                            if (num10 <= num2)
                            {
                                if (((this.m_lastKnownParamValue.Value - num10) * (this.m_lastKnownParamValue.Value - num10)) > (num3 * num3))
                                {
                                    this.m_lastKnownParamValue = new float?(num10);
                                }
                                break;
                            }
                            num10 -= num2 - paramValueBinding;
                        }
                        break;
                    }
                    num10 += num2 - paramValueBinding;
                }
            }
            return this.m_lastKnownParamValue.Value;
        }

        public override float GetLocalTimeNormalized()
        {
            using (List<MyParameterNodeMapping>.Enumerator enumerator = this.ChildMappings.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyParameterNodeMapping current = enumerator.Current;
                    if (current.Child != null)
                    {
                        return current.Child.GetLocalTimeNormalized();
                    }
                }
            }
            return 0f;
        }

        private void PushLocalTimeToSlaves(int masterIndex)
        {
            float normalizedTime = (this.ChildMappings[masterIndex].Child != null) ? this.ChildMappings[masterIndex].Child.GetLocalTimeNormalized() : 0f;
            for (int i = 0; i < this.ChildMappings.Count; i++)
            {
                if ((i != masterIndex) && (this.ChildMappings[i].Child != null))
                {
                    this.ChildMappings[i].Child.SetLocalTimeNormalized(normalizedTime);
                }
            }
        }

        public override void SetLocalTimeNormalized(float normalizedTime)
        {
            foreach (MyParameterNodeMapping mapping in this.ChildMappings)
            {
                if (mapping.Child != null)
                {
                    mapping.Child.SetLocalTimeNormalized(normalizedTime);
                }
            }
        }

        public override unsafe void Update(ref MyAnimationUpdateData data)
        {
            if (this.ChildMappings.Count == 0)
            {
                data.AddVisitedTreeNodesPathPoint(-1);
            }
            else
            {
                float num = this.ComputeParamValue(ref data);
                int masterIndex = -1;
                int num3 = this.ChildMappings.Count - 1;
                while (true)
                {
                    if (num3 >= 0)
                    {
                        if (this.ChildMappings[num3].ParamValueBinding > num)
                        {
                            num3--;
                            continue;
                        }
                        masterIndex = num3;
                    }
                    if (masterIndex == -1)
                    {
                        if (this.ChildMappings[0].Child == null)
                        {
                            data.BonesResult = data.Controller.ResultBonesPool.Alloc();
                        }
                        else
                        {
                            data.AddVisitedTreeNodesPathPoint(1);
                            this.ChildMappings[0].Child.Update(ref data);
                            this.PushLocalTimeToSlaves(0);
                        }
                    }
                    else if (masterIndex == (this.ChildMappings.Count - 1))
                    {
                        if (this.ChildMappings[masterIndex].Child == null)
                        {
                            data.BonesResult = data.Controller.ResultBonesPool.Alloc();
                        }
                        else
                        {
                            data.AddVisitedTreeNodesPathPoint((this.ChildMappings.Count - 1) + 1);
                            this.ChildMappings[masterIndex].Child.Update(ref data);
                            this.PushLocalTimeToSlaves(masterIndex);
                        }
                    }
                    else
                    {
                        int num4 = masterIndex + 1;
                        float num5 = this.ChildMappings[num4].ParamValueBinding - this.ChildMappings[masterIndex].ParamValueBinding;
                        float amount = (num - this.ChildMappings[masterIndex].ParamValueBinding) / num5;
                        if (amount > 0.5f)
                        {
                            masterIndex++;
                            num4--;
                            amount = 1f - amount;
                        }
                        if (amount < 0.001f)
                        {
                            amount = 0f;
                        }
                        else if (amount > 0.999f)
                        {
                            amount = 1f;
                        }
                        MyAnimationTreeNode child = this.ChildMappings[masterIndex].Child;
                        MyAnimationTreeNode node2 = this.ChildMappings[num4].Child;
                        if ((child == null) || (amount >= 1f))
                        {
                            data.BonesResult = data.Controller.ResultBonesPool.Alloc();
                        }
                        else
                        {
                            data.AddVisitedTreeNodesPathPoint(masterIndex + 1);
                            child.Update(ref data);
                            this.PushLocalTimeToSlaves(masterIndex);
                        }
                        MyAnimationUpdateData data2 = data;
                        if ((node2 == null) || (amount <= 0f))
                        {
                            MyAnimationUpdateData* dataPtr1 = (MyAnimationUpdateData*) ref data2;
                            dataPtr1->BonesResult = data2.Controller.ResultBonesPool.Alloc();
                        }
                        else
                        {
                            data2.DeltaTimeInSeconds = 0.0;
                            data2.AddVisitedTreeNodesPathPoint(num4 + 1);
                            node2.Update(ref data2);
                            data.VisitedTreeNodesCounter = data2.VisitedTreeNodesCounter;
                        }
                        int index = 0;
                        while (true)
                        {
                            if (index >= data.BonesResult.Count)
                            {
                                data.Controller.ResultBonesPool.Free(data2.BonesResult);
                                break;
                            }
                            if (data.LayerBoneMask[index])
                            {
                                data.BonesResult[index].Rotation = Quaternion.Slerp(data.BonesResult[index].Rotation, data2.BonesResult[index].Rotation, amount);
                                data.BonesResult[index].Translation = Vector3.Lerp(data.BonesResult[index].Translation, data2.BonesResult[index].Translation, amount);
                            }
                            index++;
                        }
                    }
                    this.m_lastKnownFrameCounter = data.Controller.FrameCounter;
                    data.AddVisitedTreeNodesPathPoint(-1);
                    return;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyParameterNodeMapping
        {
            public float ParamValueBinding;
            public MyAnimationTreeNode Child;
        }
    }
}

