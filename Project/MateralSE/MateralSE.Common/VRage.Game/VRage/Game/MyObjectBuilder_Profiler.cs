namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using VRage.FileSystem;
    using VRage.ObjectBuilders;
    using VRage.Profiler;

    [XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyObjectBuilder_Profiler : MyObjectBuilder_Base
    {
        public List<MyObjectBuilder_ProfilerBlock> ProfilingBlocks;
        public List<MyProfilerBlockKey> RootBlocks;
        public List<MyProfiler.TaskInfo> Tasks;
        public int[] TotalCalls;
        public long[] CommitTimes;
        public string CustomName = "";
        public string AxisName = "";
        public bool ShallowProfile;

        public static MyObjectBuilder_Profiler GetObjectBuilder(MyProfiler profiler)
        {
            MyProfiler.MyProfilerObjectBuilderInfo objectBuilderInfo = profiler.GetObjectBuilderInfo();
            MyObjectBuilder_Profiler profiler2 = new MyObjectBuilder_Profiler {
                ProfilingBlocks = new List<MyObjectBuilder_ProfilerBlock>()
            };
            foreach (KeyValuePair<MyProfilerBlockKey, MyProfilerBlock> pair in objectBuilderInfo.ProfilingBlocks)
            {
                profiler2.ProfilingBlocks.Add(MyObjectBuilder_ProfilerBlock.GetObjectBuilder(pair.Value, profiler.AllocationProfiling));
            }
            profiler2.RootBlocks = new List<MyProfilerBlockKey>();
            foreach (MyProfilerBlock block in objectBuilderInfo.RootBlocks)
            {
                profiler2.RootBlocks.Add(block.Key);
            }
            profiler2.Tasks = objectBuilderInfo.Tasks;
            profiler2.TotalCalls = objectBuilderInfo.TotalCalls;
            profiler2.CustomName = objectBuilderInfo.CustomName;
            profiler2.AxisName = objectBuilderInfo.AxisName;
            profiler2.ShallowProfile = objectBuilderInfo.ShallowProfile;
            profiler2.CommitTimes = objectBuilderInfo.CommitTimes;
            return profiler2;
        }

        public static MyProfiler Init(MyObjectBuilder_Profiler objectBuilder)
        {
            MyProfiler.MyProfilerObjectBuilderInfo profiler = new MyProfiler.MyProfilerObjectBuilderInfo {
                ProfilingBlocks = new Dictionary<MyProfilerBlockKey, MyProfilerBlock>()
            };
            foreach (MyObjectBuilder_ProfilerBlock block in objectBuilder.ProfilingBlocks)
            {
                profiler.ProfilingBlocks.Add(block.Key, new MyProfilerBlock());
            }
            using (List<MyObjectBuilder_ProfilerBlock>.Enumerator enumerator = objectBuilder.ProfilingBlocks.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    MyObjectBuilder_ProfilerBlock.Init(enumerator.Current, profiler);
                }
            }
            profiler.RootBlocks = new List<MyProfilerBlock>();
            foreach (MyProfilerBlockKey key in objectBuilder.RootBlocks)
            {
                profiler.RootBlocks.Add(profiler.ProfilingBlocks[key]);
            }
            profiler.TotalCalls = objectBuilder.TotalCalls;
            profiler.CustomName = objectBuilder.CustomName;
            profiler.AxisName = objectBuilder.AxisName;
            profiler.ShallowProfile = objectBuilder.ShallowProfile;
            profiler.Tasks = objectBuilder.Tasks;
            profiler.CommitTimes = objectBuilder.CommitTimes;
            MyProfiler profiler1 = new MyProfiler(false, profiler.CustomName, profiler.AxisName, false, 0x3e8);
            profiler1.Init(profiler);
            return profiler1;
        }

        public static void LoadFromFile(int index)
        {
            try
            {
                MyObjectBuilder_Profiler profiler;
                MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Profiler>(Path.Combine(MyFileSystem.UserDataPath, "Profiler-" + index), out profiler);
                MyRenderProfiler.SelectedProfiler = Init(profiler);
            }
            catch
            {
            }
        }

        public static void SaveToFile(int index)
        {
            try
            {
                MyObjectBuilder_Profiler objectBuilder = GetObjectBuilder(MyRenderProfiler.SelectedProfiler);
                MyObjectBuilderSerializer.SerializeXML(Path.Combine(MyFileSystem.UserDataPath, "Profiler-" + index), true, objectBuilder, null);
            }
            catch
            {
            }
        }
    }
}

