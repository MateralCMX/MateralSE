namespace Sandbox.Engine.Physics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Generics;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;
    using VRageRender.Animations;

    public class MyRagdollAnimWeightBlendingHelper
    {
        private const string RAGDOLL_WEIGHT_VARIABLE_PREFIX = "rd_weight_";
        private const string RAGDOLL_BLEND_TIME_VARIABLE_PREFIX = "rd_blend_time_";
        private const string RAGDOLL_DEFAULT_BLEND_TIME_VARIABLE_NAME = "rd_default_blend_time";
        private const float DEFAULT_BLEND_TIME = 2.5f;
        private static readonly MyGameTimer TIMER = new MyGameTimer();
        private BoneData[] m_boneIndexToData;
        private float m_defaultBlendTime = 0.8f;
        private MyStringId m_defautlBlendTimeId;

        public unsafe void BlendWeight(ref float weight, MyCharacterBone bone, IMyVariableStorage<float> controllerVariables)
        {
            if (this.m_boneIndexToData.Length > bone.Index)
            {
                float num;
                float num2;
                BoneData data = this.m_boneIndexToData[bone.Index];
                if (!controllerVariables.GetValue(this.m_defautlBlendTimeId, out this.m_defaultBlendTime))
                {
                    this.m_defaultBlendTime = 2.5f;
                }
                if (!controllerVariables.GetValue(data.WeightId, out num) || (num < 0f))
                {
                    num = -1f;
                }
                if (!controllerVariables.GetValue(data.BlendTimeId, out num2) || (num2 < 0f))
                {
                    num2 = -1f;
                }
                if ((num < 0f) || (num2 < 0f))
                {
                    float maxValue = float.MaxValue;
                    float num6 = float.MaxValue;
                    LayerData[] layers = data.Layers;
                    int index = 0;
                    while (true)
                    {
                        float num8;
                        float num9;
                        if (index >= layers.Length)
                        {
                            if (num < 0f)
                            {
                                if (maxValue == float.MaxValue)
                                {
                                    return;
                                }
                                num = maxValue;
                            }
                            if (num2 < 0f)
                            {
                                num2 = (num6 == float.MaxValue) ? this.m_defaultBlendTime : num6;
                            }
                            break;
                        }
                        LayerData data2 = layers[index];
                        if (controllerVariables.GetValue(data2.LayerId, out num8))
                        {
                            maxValue = Math.Min(maxValue, num8);
                        }
                        if (controllerVariables.GetValue(data2.LayerBlendTimeId, out num9))
                        {
                            num6 = Math.Min(num6, num9);
                        }
                        index++;
                    }
                }
                double totalMilliseconds = TIMER.ElapsedTimeSpan.TotalMilliseconds;
                data.BlendTimeMs = num2 * 1000f;
                if (num != data.TargetWeight)
                {
                    data.StartedMs = totalMilliseconds;
                    BoneData* dataPtr1 = (BoneData*) ref data;
                    dataPtr1->StartingWeight = (data.PrevWeight == -1f) ? weight : data.PrevWeight;
                    data.TargetWeight = num;
                }
                double amount = MathHelper.Clamp((double) ((totalMilliseconds - data.StartedMs) / data.BlendTimeMs), (double) 0.0, (double) 1.0);
                weight = (float) MathHelper.Lerp((double) data.StartingWeight, (double) data.TargetWeight, amount);
                data.PrevWeight = weight;
                this.m_boneIndexToData[bone.Index] = data;
            }
        }

        public unsafe void Init(MyCharacterBone[] bones, MyAnimationController controller)
        {
            List<MyAnimationStateMachine> list = new List<MyAnimationStateMachine>(controller.GetLayerCount());
            for (int i = 0; i < controller.GetLayerCount(); i++)
            {
                list.Add(controller.GetLayerByIndex(i));
            }
            this.m_boneIndexToData = new BoneData[bones.Length];
            foreach (MyCharacterBone bone in bones)
            {
                BoneData* dataPtr1;
                BoneData data = new BoneData {
                    WeightId = MyStringId.GetOrCompute("rd_weight_" + bone.Name),
                    BlendTimeId = MyStringId.GetOrCompute("rd_blend_time_" + bone.Name),
                    BlendTimeMs = -1.0,
                    StartingWeight = 0f,
                    TargetWeight = 0f,
                    PrevWeight = 0f
                };
                dataPtr1->Layers = (from layer in list
                    where layer.BoneMask[bone.Index]
                    select new LayerData { 
                        LayerId = MyStringId.GetOrCompute("rd_weight_" + layer.Name),
                        LayerBlendTimeId = MyStringId.GetOrCompute("rd_blend_time_" + layer.Name)
                    }).ToArray<LayerData>();
                dataPtr1 = (BoneData*) ref data;
                this.m_boneIndexToData[bone.Index] = data;
            }
            this.m_defautlBlendTimeId = MyStringId.GetOrCompute("rd_default_blend_time");
            this.Initialized = true;
        }

        public void ResetWeights()
        {
            if (this.m_boneIndexToData != null)
            {
                for (int i = 0; i < this.m_boneIndexToData.Length; i++)
                {
                    this.m_boneIndexToData[i].PrevWeight = 0f;
                    this.m_boneIndexToData[i].TargetWeight = 0f;
                }
            }
        }

        public bool Initialized { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyRagdollAnimWeightBlendingHelper.<>c <>9 = new MyRagdollAnimWeightBlendingHelper.<>c();
            public static Func<MyAnimationStateMachine, MyRagdollAnimWeightBlendingHelper.LayerData> <>9__14_1;

            internal MyRagdollAnimWeightBlendingHelper.LayerData <Init>b__14_1(MyAnimationStateMachine layer) => 
                new MyRagdollAnimWeightBlendingHelper.LayerData { 
                    LayerId = MyStringId.GetOrCompute("rd_weight_" + layer.Name),
                    LayerBlendTimeId = MyStringId.GetOrCompute("rd_blend_time_" + layer.Name)
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BoneData
        {
            public MyStringId WeightId;
            public MyStringId BlendTimeId;
            public double BlendTimeMs;
            public double StartedMs;
            public float StartingWeight;
            public float TargetWeight;
            public float PrevWeight;
            public MyRagdollAnimWeightBlendingHelper.LayerData[] Layers;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LayerData
        {
            public MyStringId LayerId;
            public MyStringId LayerBlendTimeId;
        }
    }
}

