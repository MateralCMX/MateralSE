namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    public class MyBBSetSampler
    {
        private IntervalSampler m_sampler;
        private BoundingBoxD m_bBox;

        public MyBBSetSampler(Vector3D min, Vector3D max)
        {
            Vector3D vectord = Vector3D.Max(min, max);
            Vector3D vectord2 = Vector3D.Min(min, max);
            this.m_bBox = new BoundingBoxD(vectord2, vectord);
            this.m_sampler = new IntervalSampler(vectord2.X, vectord.X, (vectord.Y - vectord2.Y) * (vectord.Z - vectord2.Z), Base6Directions.Axis.LeftRight);
        }

        public Vector3D Sample()
        {
            Vector3D vectord;
            IntervalSampler childSampler = this.m_sampler;
            vectord.X = childSampler.Sample(out childSampler);
            vectord.Y = (childSampler == null) ? MyUtils.GetRandomDouble(this.m_bBox.Min.Y, this.m_bBox.Max.Y) : childSampler.Sample(out childSampler);
            vectord.Z = (childSampler == null) ? MyUtils.GetRandomDouble(this.m_bBox.Min.Z, this.m_bBox.Max.Z) : childSampler.Sample(out childSampler);
            return vectord;
        }

        public void SubtractBB(ref BoundingBoxD bb)
        {
            if (this.m_bBox.Intersects(ref bb))
            {
                BoundingBoxD xd = this.m_bBox.Intersect(bb);
                this.m_sampler.Subtract(ref this.m_bBox, ref xd);
            }
        }

        public bool Valid =>
            ((this.m_sampler != null) ? (this.m_sampler.TotalWeight > 0.0) : (this.m_bBox.Volume > 0.0));

        private class IntervalSampler
        {
            private Base6Directions.Axis m_axis;
            private double m_min;
            private double m_max;
            private double m_weightMult;
            private List<SamplingEntry> m_entries;
            private double m_totalWeight;

            private IntervalSampler(MyBBSetSampler.IntervalSampler other, double t, bool clone)
            {
                this.m_min = other.m_min;
                this.m_max = other.m_max;
                this.m_axis = other.m_axis;
                this.m_weightMult = other.m_weightMult;
                this.m_totalWeight = other.m_totalWeight;
                this.m_entries = new List<SamplingEntry>(other.m_entries);
                for (int i = 0; i < other.m_entries.Count; i++)
                {
                    this.m_entries[i] = new SamplingEntry(other.m_entries[i]);
                }
                this.Multiply(t);
                if (!clone)
                {
                    other.Multiply(1.0 - t);
                }
            }

            public IntervalSampler(double min, double max, double weightMultiplier, Base6Directions.Axis axis)
            {
                this.m_min = min;
                this.m_max = max;
                this.m_axis = axis;
                this.m_weightMult = weightMultiplier;
                this.m_totalWeight = weightMultiplier * (this.m_max - this.m_min);
                this.m_entries = new List<SamplingEntry>();
                this.m_entries.Add(new SamplingEntry(this.m_max, null, this.m_totalWeight));
            }

            private unsafe void Multiply(double t)
            {
                this.m_weightMult *= t;
                this.m_totalWeight *= t;
                for (int i = 0; i < this.m_entries.Count; i++)
                {
                    SamplingEntry entry = this.m_entries[i];
                    double* numPtr1 = (double*) ref entry.CumulativeWeight;
                    numPtr1[0] *= t;
                    this.m_entries[i] = entry;
                    if (entry.Sampler != null)
                    {
                        entry.Sampler.Multiply(t);
                    }
                }
            }

            public double Sample(out MyBBSetSampler.IntervalSampler childSampler)
            {
                double randomDouble = MyUtils.GetRandomDouble(0.0, this.TotalWeight);
                double min = this.m_min;
                double cumulativeWeight = 0.0;
                for (int i = 0; i < this.m_entries.Count; i++)
                {
                    if (this.m_entries[i].CumulativeWeight >= randomDouble)
                    {
                        childSampler = this.m_entries[i].Sampler;
                        double num5 = this.m_entries[i].CumulativeWeight - cumulativeWeight;
                        double num6 = (randomDouble - cumulativeWeight) / num5;
                        return ((num6 * this.m_entries[i].UpperLimit) + ((1.0 - num6) * min));
                    }
                    min = this.m_entries[i].UpperLimit;
                    cumulativeWeight = this.m_entries[i].CumulativeWeight;
                }
                childSampler = null;
                return this.m_max;
            }

            private void SelectMinMax(ref BoundingBoxD bb, Base6Directions.Axis axis, out double min, out double max)
            {
                if (axis == Base6Directions.Axis.UpDown)
                {
                    min = bb.Min.Y;
                    max = bb.Max.Y;
                }
                else if (axis == Base6Directions.Axis.ForwardBackward)
                {
                    min = bb.Min.Z;
                    max = bb.Max.Z;
                }
                else
                {
                    min = bb.Min.X;
                    max = bb.Max.X;
                }
            }

            public unsafe void Subtract(ref BoundingBoxD originalBox, ref BoundingBoxD bb)
            {
                double num;
                double num2;
                this.SelectMinMax(ref bb, this.m_axis, out num, out num2);
                bool flag = false;
                double min = this.m_min;
                double prevCumulativeWeight = 0.0;
                int index = 0;
                while (true)
                {
                    while (true)
                    {
                        if (index >= this.m_entries.Count)
                        {
                            this.m_totalWeight = prevCumulativeWeight;
                            return;
                        }
                        SamplingEntry oldEntry = this.m_entries[index];
                        if (!flag)
                        {
                            if (oldEntry.UpperLimit >= num)
                            {
                                if (oldEntry.UpperLimit == num)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    if (min == num)
                                    {
                                        flag = true;
                                        index--;
                                        break;
                                    }
                                    flag = true;
                                    SamplingEntry item = SamplingEntry.Divide(ref oldEntry, min, prevCumulativeWeight, this.m_weightMult, num);
                                    this.m_entries[index] = oldEntry;
                                    this.m_entries.Insert(index, item);
                                    oldEntry = item;
                                }
                            }
                        }
                        else if (min >= num2)
                        {
                            if (oldEntry.Sampler != null)
                            {
                                SamplingEntry* entryPtr3 = (SamplingEntry*) ref oldEntry;
                                entryPtr3->CumulativeWeight = prevCumulativeWeight + oldEntry.Sampler.TotalWeight;
                            }
                            else if (oldEntry.Full)
                            {
                                oldEntry.CumulativeWeight = prevCumulativeWeight;
                            }
                            else
                            {
                                SamplingEntry* entryPtr2 = (SamplingEntry*) ref oldEntry;
                                entryPtr2->CumulativeWeight = prevCumulativeWeight + ((oldEntry.UpperLimit - min) * this.m_weightMult);
                            }
                            this.m_entries[index] = oldEntry;
                        }
                        else
                        {
                            if (oldEntry.UpperLimit > num2)
                            {
                                SamplingEntry item = SamplingEntry.Divide(ref oldEntry, min, prevCumulativeWeight, this.m_weightMult, num2);
                                this.m_entries[index] = oldEntry;
                                this.m_entries.Insert(index, item);
                                oldEntry = item;
                            }
                            if (oldEntry.UpperLimit <= num2)
                            {
                                if (oldEntry.Sampler == null)
                                {
                                    if (this.m_axis == Base6Directions.Axis.ForwardBackward)
                                    {
                                        oldEntry.Full = true;
                                        oldEntry.CumulativeWeight = prevCumulativeWeight;
                                    }
                                    else if (!oldEntry.Full)
                                    {
                                        double num6;
                                        double num7;
                                        Base6Directions.Axis axis = (this.m_axis == Base6Directions.Axis.LeftRight) ? Base6Directions.Axis.UpDown : Base6Directions.Axis.ForwardBackward;
                                        this.SelectMinMax(ref originalBox, axis, out num6, out num7);
                                        double num8 = this.m_max - this.m_min;
                                        double num11 = num7 - num6;
                                        oldEntry.Sampler = new MyBBSetSampler.IntervalSampler(num6, num7, ((this.m_weightMult * num8) * ((oldEntry.UpperLimit - min) / num8)) / num11, axis);
                                    }
                                }
                                if (oldEntry.Sampler != null)
                                {
                                    oldEntry.Sampler.Subtract(ref originalBox, ref bb);
                                    SamplingEntry* entryPtr1 = (SamplingEntry*) ref oldEntry;
                                    entryPtr1->CumulativeWeight = prevCumulativeWeight + oldEntry.Sampler.TotalWeight;
                                }
                                this.m_entries[index] = oldEntry;
                            }
                        }
                        min = oldEntry.UpperLimit;
                        prevCumulativeWeight = oldEntry.CumulativeWeight;
                        break;
                    }
                    index++;
                }
            }

            public double TotalWeight =>
                this.m_totalWeight;

            [StructLayout(LayoutKind.Sequential)]
            private struct SamplingEntry
            {
                public double UpperLimit;
                public double CumulativeWeight;
                public bool Full;
                public MyBBSetSampler.IntervalSampler Sampler;
                public SamplingEntry(double limit, MyBBSetSampler.IntervalSampler sampler, double weight)
                {
                    this.UpperLimit = limit;
                    this.Sampler = sampler;
                    this.CumulativeWeight = weight;
                    this.Full = false;
                }

                public SamplingEntry(MyBBSetSampler.IntervalSampler.SamplingEntry other)
                {
                    this.UpperLimit = other.UpperLimit;
                    this.CumulativeWeight = other.CumulativeWeight;
                    this.Full = other.Full;
                    if (other.Sampler == null)
                    {
                        this.Sampler = null;
                    }
                    else
                    {
                        this.Sampler = new MyBBSetSampler.IntervalSampler(other.Sampler, 1.0, true);
                    }
                }

                public static unsafe MyBBSetSampler.IntervalSampler.SamplingEntry Divide(ref MyBBSetSampler.IntervalSampler.SamplingEntry oldEntry, double prevUpperLimit, double prevCumulativeWeight, double weightMult, double newUpperLimit)
                {
                    MyBBSetSampler.IntervalSampler.SamplingEntry entry = new MyBBSetSampler.IntervalSampler.SamplingEntry {
                        UpperLimit = newUpperLimit
                    };
                    double num = newUpperLimit - prevUpperLimit;
                    double num2 = oldEntry.UpperLimit - newUpperLimit;
                    double t = num / (num + num2);
                    entry.Full = oldEntry.Full;
                    if (oldEntry.Sampler != null)
                    {
                        entry.Sampler = new MyBBSetSampler.IntervalSampler(oldEntry.Sampler, t, false);
                        MyBBSetSampler.IntervalSampler.SamplingEntry* entryPtr1 = (MyBBSetSampler.IntervalSampler.SamplingEntry*) ref entry;
                        entryPtr1->CumulativeWeight = prevCumulativeWeight + entry.Sampler.TotalWeight;
                        oldEntry.CumulativeWeight = entry.CumulativeWeight + oldEntry.Sampler.TotalWeight;
                    }
                    else
                    {
                        entry.Sampler = null;
                        if (oldEntry.Full)
                        {
                            entry.CumulativeWeight = oldEntry.CumulativeWeight = prevCumulativeWeight;
                        }
                        else
                        {
                            entry.CumulativeWeight = prevCumulativeWeight + (weightMult * num);
                            oldEntry.CumulativeWeight = entry.CumulativeWeight + (weightMult * num2);
                        }
                    }
                    return entry;
                }
            }
        }
    }
}

