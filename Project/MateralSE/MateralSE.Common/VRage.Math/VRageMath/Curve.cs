namespace VRageMath
{
    using System;

    [Serializable]
    public class Curve
    {
        private CurveKeyCollection keys = new CurveKeyCollection();
        private CurveLoopType preLoop;
        private CurveLoopType postLoop;

        private float CalcCycle(float t)
        {
            float num = (t - this.keys[0].position) * this.keys.InvTimeRange;
            if (num < 0.0)
            {
                num--;
            }
            return (float) ((int) num);
        }

        public Curve Clone()
        {
            Curve curve1 = new Curve();
            curve1.preLoop = this.preLoop;
            curve1.postLoop = this.postLoop;
            curve1.keys = this.keys.Clone();
            return curve1;
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            this.ComputeTangent(keyIndex, tangentType, tangentType);
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            if ((this.keys.Count <= keyIndex) || (keyIndex < 0))
            {
                throw new ArgumentOutOfRangeException("keyIndex");
            }
            CurveKey key = this.Keys[keyIndex];
            double position = key.Position;
            float num = (float) position;
            float num2 = (float) position;
            float num3 = (float) position;
            double num11 = key.Value;
            float num4 = (float) num11;
            float num5 = (float) num11;
            float num6 = (float) num11;
            if (keyIndex > 0)
            {
                num3 = this.Keys[keyIndex - 1].Position;
                num6 = this.Keys[keyIndex - 1].Value;
            }
            if ((keyIndex + 1) < this.keys.Count)
            {
                num = this.Keys[keyIndex + 1].Position;
                num4 = this.Keys[keyIndex + 1].Value;
            }
            if (tangentInType != CurveTangent.Smooth)
            {
                key.TangentIn = (tangentInType != CurveTangent.Linear) ? 0f : (num5 - num6);
            }
            else
            {
                float num7 = num - num3;
                float num8 = num4 - num6;
                key.TangentIn = (Math.Abs(num8) >= 1.19209289550781E-07) ? ((num8 * Math.Abs((float) (num3 - num2))) / num7) : 0f;
            }
            if (tangentOutType != CurveTangent.Smooth)
            {
                if (tangentOutType == CurveTangent.Linear)
                {
                    key.TangentOut = num4 - num5;
                }
                else
                {
                    key.TangentOut = 0f;
                }
            }
            else
            {
                float num9 = num - num3;
                float num10 = num4 - num6;
                if (Math.Abs(num10) < 1.19209289550781E-07)
                {
                    key.TangentOut = 0f;
                }
                else
                {
                    key.TangentOut = (num10 * Math.Abs((float) (num - num2))) / num9;
                }
            }
        }

        public void ComputeTangents(CurveTangent tangentType)
        {
            this.ComputeTangents(tangentType, tangentType);
        }

        public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            for (int i = 0; i < this.Keys.Count; i++)
            {
                this.ComputeTangent(i, tangentInType, tangentOutType);
            }
        }

        public float Evaluate(float position)
        {
            if (this.keys.Count == 0)
            {
                return 0f;
            }
            if (this.keys.Count == 1)
            {
                return this.keys[0].internalValue;
            }
            CurveKey key = this.keys[0];
            CurveKey key2 = this.keys[this.keys.Count - 1];
            float t = position;
            float num2 = 0f;
            if (t < key.position)
            {
                if (this.preLoop == CurveLoopType.Constant)
                {
                    return key.internalValue;
                }
                if (this.preLoop == CurveLoopType.Linear)
                {
                    return (key.internalValue - (key.tangentIn * (key.position - t)));
                }
                if (!this.keys.IsCacheAvailable)
                {
                    this.keys.ComputeCacheValues();
                }
                float num4 = this.CalcCycle(t);
                float num5 = t - (key.position + (num4 * this.keys.TimeRange));
                if (this.preLoop == CurveLoopType.Cycle)
                {
                    t = key.position + num5;
                }
                else if (this.preLoop != CurveLoopType.CycleOffset)
                {
                    t = ((((int) num4) & 1) != 0) ? (key2.position - num5) : (key.position + num5);
                }
                else
                {
                    t = key.position + num5;
                    num2 = (key2.internalValue - key.internalValue) * num4;
                }
            }
            else if (key2.position < t)
            {
                if (this.postLoop == CurveLoopType.Constant)
                {
                    return key2.internalValue;
                }
                if (this.postLoop == CurveLoopType.Linear)
                {
                    return (key2.internalValue - (key2.tangentOut * (key2.position - t)));
                }
                if (!this.keys.IsCacheAvailable)
                {
                    this.keys.ComputeCacheValues();
                }
                float num6 = this.CalcCycle(t);
                float num7 = t - (key.position + (num6 * this.keys.TimeRange));
                if (this.postLoop == CurveLoopType.Cycle)
                {
                    t = key.position + num7;
                }
                else if (this.postLoop != CurveLoopType.CycleOffset)
                {
                    t = ((((int) num6) & 1) != 0) ? (key2.position - num7) : (key.position + num7);
                }
                else
                {
                    t = key.position + num7;
                    num2 = (key2.internalValue - key.internalValue) * num6;
                }
            }
            CurveKey key3 = null;
            CurveKey key4 = null;
            return (num2 + Hermite(key3, key4, this.FindSegment(t, ref key3, ref key4)));
        }

        private float FindSegment(float t, ref CurveKey k0, ref CurveKey k1)
        {
            float num = t;
            k0 = this.keys[0];
            int num2 = 1;
            while (true)
            {
                if (num2 < this.keys.Count)
                {
                    k1 = this.keys[num2];
                    if (k1.position < t)
                    {
                        k0 = k1;
                        num2++;
                        continue;
                    }
                    double position = k0.position;
                    double num4 = t;
                    double num5 = k1.position - position;
                    num = 0f;
                    if (num5 > 0.0)
                    {
                        num = (float) ((num4 - position) / num5);
                    }
                }
                return num;
            }
        }

        private static float Hermite(CurveKey k0, CurveKey k1, float t)
        {
            if (k0.Continuity == CurveContinuity.Step)
            {
                return ((t < 1.0) ? k0.internalValue : k1.internalValue);
            }
            float num = t * t;
            float num2 = num * t;
            float internalValue = k1.internalValue;
            float tangentOut = k0.tangentOut;
            float tangentIn = k1.tangentIn;
            return (((float) (((k0.internalValue * (((2.0 * num2) - (3.0 * num)) + 1.0)) + (internalValue * ((-2.0 * num2) + (3.0 * num)))) + (tangentOut * ((num2 - (2.0 * num)) + t)))) + (tangentIn * (num2 - num)));
        }

        public CurveLoopType PreLoop
        {
            get => 
                this.preLoop;
            set => 
                (this.preLoop = value);
        }

        public CurveLoopType PostLoop
        {
            get => 
                this.postLoop;
            set => 
                (this.postLoop = value);
        }

        public CurveKeyCollection Keys =>
            this.keys;

        public bool IsConstant =>
            (this.keys.Count <= 1);
    }
}

