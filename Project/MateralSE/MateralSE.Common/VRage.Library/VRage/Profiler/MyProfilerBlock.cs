namespace VRage.Profiler
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Library.Utils;

    public class MyProfilerBlock
    {
        public int NumCalls;
        public int Allocated;
        public float CustomValue;
        public MyTimeSpan Elapsed;
        public int ForceOrder;
        public string TimeFormat;
        public string ValueFormat;
        public string CallFormat;
        private bool m_isOptimized;
        public MyProfilerBlock Parent;
        public List<MyProfilerBlock> Children = new List<MyProfilerBlock>();
        public float AverageMilliseconds;
        public OptimizableDataCache RawAllocations;
        public OptimizableDataCache RawMilliseconds;
        public int[] NumCallsArray = new int[MyProfiler.MAX_FRAMES];
        public float[] CustomValues = new float[MyProfiler.MAX_FRAMES];
        private int m_beginThreadId;
        private ulong m_beginAllocationStamp;
        private long m_measureStartTimestamp;
        public BlockTypes BlockType;

        public MyProfilerBlock()
        {
            this.RawMilliseconds = new TimeCache(this, null);
            this.RawAllocations = new AllocationCache(this, null);
        }

        public void Clear()
        {
            this.Reset();
            this.NumCalls = 0;
            this.Allocated = 0;
            this.CustomValue = 0f;
        }

        internal void Dump(StringBuilder sb, int frame)
        {
            if (this.NumCallsArray[frame] >= 0.01)
            {
                sb.Append($"<Block Name="{this.Name}">
");
                sb.Append($"<Time>{this.RawMilliseconds[frame]}</Time>
<Calls>{this.NumCallsArray[frame]}</Calls>
");
                foreach (MyProfilerBlock block in this.Children)
                {
                    block.Dump(sb, frame);
                }
                sb.Append("</Block>\n");
            }
        }

        public MyProfilerBlock Duplicate(int id, MyProfilerBlock parent)
        {
            MyProfilerBlock block = new MyProfilerBlock();
            block.Init(this.GetObjectBuilderInfo(true), id, parent);
            return block;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void End(bool memoryProfiling, MyTimeSpan? customTime = new MyTimeSpan?())
        {
            MyTimeSpan span;
            if (memoryProfiling)
            {
                this.Allocated += (int) (MyManagedAllocationReader.GetThreadAllocationStamp() - this.m_beginAllocationStamp);
            }
            if (customTime != null)
            {
                span = customTime.Value;
            }
            else
            {
                span = MyTimeSpan.FromTicks(Stopwatch.GetTimestamp() - this.m_measureStartTimestamp);
            }
            this.Elapsed += span;
        }

        private static void ForceInvalidateSelfAndParentsOptimizationsRecursive(MyProfilerBlock block)
        {
            while (block != null)
            {
                int frame = 0;
                while (true)
                {
                    if (frame >= MyProfiler.MAX_FRAMES)
                    {
                        block = block.Parent;
                        break;
                    }
                    block.RawAllocations.InvalidateFrameOptimizations(frame);
                    block.RawMilliseconds.InvalidateFrameOptimizations(frame);
                    frame++;
                }
            }
        }

        public DataReader GetAllocationsReader(bool useOptimizations) => 
            new DataReader(this.RawAllocations, useOptimizations);

        public DataReader GetMillisecondsReader(bool useOptimizations) => 
            new DataReader(this.RawMilliseconds, useOptimizations);

        public MyProfilerBlockObjectBuilderInfo GetObjectBuilderInfo(bool serializeAllocations)
        {
            MyProfilerBlockObjectBuilderInfo info1 = new MyProfilerBlockObjectBuilderInfo();
            info1.Id = this.Id;
            info1.Key = this.Key;
            info1.Parent = this.Parent;
            info1.Children = this.Children;
            info1.CallFormat = this.CallFormat;
            info1.TimeFormat = this.TimeFormat;
            info1.ValueFormat = this.ValueFormat;
            info1.CustomValues = this.CustomValues;
            info1.NumCallsArray = this.NumCallsArray;
            info1.Milliseconds = this.RawMilliseconds.RawData;
            info1.Allocations = serializeAllocations ? this.RawAllocations.RawData : null;
            return info1;
        }

        public void Init(MyProfilerBlockObjectBuilderInfo data)
        {
            this.Id = data.Id;
            this.Key = data.Key;
            this.CallFormat = data.CallFormat;
            this.TimeFormat = data.TimeFormat;
            this.ValueFormat = data.ValueFormat;
            this.CustomValues = data.CustomValues;
            this.NumCallsArray = data.NumCallsArray;
            this.Parent = data.Parent;
            this.Children = data.Children;
            this.RawMilliseconds = new TimeCache(this, data.Milliseconds);
            this.RawAllocations = new AllocationCache(this, data.Allocations);
        }

        private void Init(MyProfilerBlockObjectBuilderInfo data, int id, MyProfilerBlock parent)
        {
            this.Init(data);
            this.Children = new List<MyProfilerBlock>();
            this.CustomValues = (float[]) data.CustomValues.Clone();
            this.NumCallsArray = (int[]) data.NumCallsArray.Clone();
            this.RawMilliseconds = new TimeCache(this, (float[]) data.Milliseconds.Clone());
            this.RawAllocations = new AllocationCache(this, (float[]) data.Allocations.Clone());
            this.Id = id;
            this.BlockType = BlockTypes.Added;
            if (parent != null)
            {
                this.Parent = parent;
                MyProfilerBlockKey key = this.Key;
                key.ParentId = parent.Id;
                this.Key = key;
                parent.Children.Add(this);
            }
        }

        public void Invert()
        {
            this.Allocated = -this.Allocated;
            this.Elapsed -= this.Elapsed;
            this.AverageMilliseconds -= this.AverageMilliseconds;
            for (int i = 0; i < MyProfiler.MAX_FRAMES; i++)
            {
                this.RawAllocations[i] = -this.RawAllocations[i];
                this.RawMilliseconds[i] = -this.RawMilliseconds[i];
            }
            this.BlockType = BlockTypes.Inverted;
        }

        public void Reset()
        {
            this.m_measureStartTimestamp = Stopwatch.GetTimestamp();
            this.Elapsed = MyTimeSpan.Zero;
            this.Allocated = 0;
        }

        public void SetBlockData(ref MyProfilerBlockKey key, int blockId, int forceOrder = 0x7fffffff, bool isDeepTreeRoot = true)
        {
            this.Id = blockId;
            this.Key = key;
            this.ForceOrder = forceOrder;
            this.IsDeepTreeRoot = isDeepTreeRoot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start(bool memoryProfiling)
        {
            this.NumCalls++;
            this.m_measureStartTimestamp = Stopwatch.GetTimestamp();
            if (memoryProfiling)
            {
                this.m_beginAllocationStamp = MyManagedAllocationReader.GetThreadAllocationStamp();
                this.m_beginThreadId = Thread.CurrentThread.ManagedThreadId;
            }
        }

        public void SubtractFrom(MyProfilerBlock otherBlock)
        {
            this.NumCalls = otherBlock.NumCalls - this.NumCalls;
            this.Allocated = otherBlock.Allocated - this.Allocated;
            this.CustomValue = otherBlock.CustomValue - this.CustomValue;
            this.Elapsed = otherBlock.Elapsed - this.Elapsed;
            this.AverageMilliseconds = otherBlock.AverageMilliseconds - this.AverageMilliseconds;
            for (int i = 0; i < MyProfiler.MAX_FRAMES; i++)
            {
                this.RawAllocations[i] = otherBlock.RawAllocations[i] - this.RawAllocations[i];
                this.RawMilliseconds[i] = otherBlock.RawMilliseconds[i] - this.RawMilliseconds[i];
                this.NumCallsArray[i] = otherBlock.NumCallsArray[i] - this.NumCallsArray[i];
                this.CustomValues[i] = otherBlock.CustomValues[i] - this.CustomValues[i];
            }
            this.BlockType = BlockTypes.Diffed;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Key.Name, " (", this.NumCalls, " calls)" };
            return string.Concat(objArray1);
        }

        public int Id { get; private set; }

        public MyProfilerBlockKey Key { get; private set; }

        public string Name =>
            this.Key.Name;

        public bool IsOptimized
        {
            get => 
                this.m_isOptimized;
            set
            {
                if (this.m_isOptimized != value)
                {
                    this.m_isOptimized = value;
                    ForceInvalidateSelfAndParentsOptimizationsRecursive(this);
                }
            }
        }

        public bool IsDeepTreeRoot { get; private set; }

        private sealed class AllocationCache : MyProfilerBlock.OptimizableDataCache
        {
            public AllocationCache(MyProfilerBlock block, float[] data = null) : base(block, data)
            {
            }

            protected override MyProfilerBlock.OptimizableDataCache GetBlockData(MyProfilerBlock block) => 
                block.RawAllocations;
        }

        public enum BlockTypes
        {
            Normal,
            Diffed,
            Inverted,
            Added
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DataReader
        {
            private readonly bool m_useOptimizations;
            private readonly MyProfilerBlock.OptimizableDataCache Data;
            public DataReader(MyProfilerBlock.OptimizableDataCache data, bool useOptimizations)
            {
                this.Data = data;
                this.m_useOptimizations = useOptimizations;
            }

            public float this[int frame]
            {
                get
                {
                    float num = this.Data[frame];
                    if (this.m_useOptimizations)
                    {
                        num -= this.Data.GetOptimizedCutout(frame);
                    }
                    return num;
                }
            }
        }

        public class MyProfilerBlockObjectBuilderInfo
        {
            public int Id;
            public MyProfilerBlockKey Key;
            public string TimeFormat;
            public string ValueFormat;
            public string CallFormat;
            public int[] NumCallsArray;
            public float[] Allocations;
            public float[] Milliseconds;
            public float[] CustomValues;
            public MyProfilerBlock Parent;
            public List<MyProfilerBlock> Children;
        }

        public abstract class OptimizableDataCache
        {
            public readonly float[] RawData;
            private readonly bool[] m_valid;
            private readonly float[] m_optimizedCutout;
            private readonly MyProfilerBlock m_block;

            protected OptimizableDataCache(MyProfilerBlock mBlock, float[] data = null)
            {
                float[] singleArray1 = data;
                this.m_block = mBlock;
                this.m_valid = new bool[MyProfiler.MAX_FRAMES];
                this.RawData = data ?? new float[MyProfiler.MAX_FRAMES];
                this.m_optimizedCutout = new float[MyProfiler.MAX_FRAMES];
            }

            protected abstract MyProfilerBlock.OptimizableDataCache GetBlockData(MyProfilerBlock block);
            public float GetOptimizedCutout(int frame)
            {
                if (!this.m_valid[frame])
                {
                    float num = 0f;
                    if (this.m_block.IsOptimized)
                    {
                        num = this.RawData[frame];
                    }
                    else
                    {
                        foreach (MyProfilerBlock block in this.m_block.Children)
                        {
                            num += this.GetBlockData(block).GetOptimizedCutout(frame);
                        }
                    }
                    this.m_valid[frame] = true;
                    this.m_optimizedCutout[frame] = num;
                }
                return this.m_optimizedCutout[frame];
            }

            public void InvalidateFrameOptimizations(int frame)
            {
                this.m_valid[frame] = false;
            }

            public float this[int frame]
            {
                get => 
                    this.RawData[frame];
                set
                {
                    this.RawData[frame] = value;
                    this.InvalidateFrameOptimizations(frame);
                }
            }
        }

        private sealed class TimeCache : MyProfilerBlock.OptimizableDataCache
        {
            public TimeCache(MyProfilerBlock block, float[] data = null) : base(block, data)
            {
            }

            protected override MyProfilerBlock.OptimizableDataCache GetBlockData(MyProfilerBlock block) => 
                block.RawMilliseconds;
        }
    }
}

