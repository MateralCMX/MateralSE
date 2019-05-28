namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Utils;

    public static class MyPerformanceCounter
    {
        public const int NoSplit = 4;
        private static MyPerCameraDraw PerCameraDraw0 = new MyPerCameraDraw();
        private static MyPerCameraDraw PerCameraDraw1 = new MyPerCameraDraw();
        public static MyPerCameraDraw PerCameraDrawRead = PerCameraDraw0;
        public static MyPerCameraDraw PerCameraDrawWrite = PerCameraDraw0;
        public static MyPerAppLifetime PerAppLifetime = new MyPerAppLifetime();
        public static bool LogFiles = false;

        public static void Restart(string name)
        {
            PerCameraDrawRead.CustomTimers.Remove(name);
            PerCameraDrawWrite.CustomTimers.Remove(name);
            PerCameraDrawRead.StartTimer(name);
            PerCameraDrawWrite.StartTimer(name);
        }

        public static void Stop(string name)
        {
            PerCameraDrawRead.StopTimer(name);
            PerCameraDrawWrite.StopTimer(name);
        }

        internal static void SwitchCounters()
        {
            if (ReferenceEquals(PerCameraDrawRead, PerCameraDraw0))
            {
                PerCameraDrawRead = PerCameraDraw1;
                PerCameraDrawWrite = PerCameraDraw0;
            }
            else
            {
                PerCameraDrawRead = PerCameraDraw0;
                PerCameraDrawWrite = PerCameraDraw1;
            }
        }

        public class MyPerAppLifetime
        {
            public int Textures2DCount;
            public int Textures2DSizeInPixels;
            public double Textures2DSizeInMb;
            public int NonMipMappedTexturesCount;
            public int NonDxtCompressedTexturesCount;
            public int DxtCompressedTexturesCount;
            public int TextureCubesCount;
            public int TextureCubesSizeInPixels;
            public double TextureCubesSizeInMb;
            public int ModelsCount;
            public int MyModelsCount;
            public int MyModelsMeshesCount;
            public int MyModelsVertexesCount;
            public int MyModelsTrianglesCount;
            public int ModelVertexBuffersSize;
            public int ModelIndexBuffersSize;
            public int VoxelVertexBuffersSize;
            public int VoxelIndexBuffersSize;
            public int MyModelsFilesSize;
            public List<string> LoadedTextureFiles = new List<string>();
            public List<string> LoadedModelFiles = new List<string>();
        }

        public class MyPerCameraDraw
        {
            public readonly Dictionary<string, MyPerformanceCounter.Timer> CustomTimers = new Dictionary<string, MyPerformanceCounter.Timer>(5);
            public readonly Dictionary<string, float> CustomCounters = new Dictionary<string, float>(5);
            private long m_gcMemory;
            private readonly List<string> m_tmpKeys = new List<string>();

            public MyPerCameraDraw()
            {
                MyUtils.GetMaxValueFromEnum<MyLodTypeEnum>();
            }

            public void ClearCustomCounters()
            {
                this.m_tmpKeys.Clear();
            }

            public void Reset()
            {
                this.ClearCustomCounters();
                this.GcMemory = GC.GetTotalMemory(false);
            }

            public void SetCounter(string name, float count)
            {
                this.CustomCounters[name] = count;
            }

            public void StartTimer(string name)
            {
                MyPerformanceCounter.Timer timer;
                this.CustomTimers.TryGetValue(name, out timer);
                timer.Start();
                this.CustomTimers[name] = timer;
            }

            public void StopTimer(string name)
            {
                MyPerformanceCounter.Timer timer;
                if (this.CustomTimers.TryGetValue(name, out timer))
                {
                    timer.Stop();
                    this.CustomTimers[name] = timer;
                }
            }

            public long GcMemory
            {
                get => 
                    Interlocked.Read(ref this.m_gcMemory);
                set => 
                    Interlocked.Exchange(ref this.m_gcMemory, value);
            }

            public float this[string name]
            {
                get
                {
                    float num;
                    if (!this.CustomCounters.TryGetValue(name, out num))
                    {
                        num = 0f;
                    }
                    return num;
                }
                set => 
                    (this.CustomCounters[name] = value);
            }

            public List<string> SortedCounterKeys
            {
                get
                {
                    this.m_tmpKeys.Clear();
                    return this.m_tmpKeys;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Timer
        {
            private static Stopwatch m_timer;
            public static readonly MyPerformanceCounter.Timer Empty;
            public long StartTime;
            public long Runtime;
            static Timer()
            {
                MyPerformanceCounter.Timer timer = new MyPerformanceCounter.Timer {
                    Runtime = 0L,
                    StartTime = 0x7fffffffffffffffL
                };
                Empty = timer;
                m_timer = new Stopwatch();
                m_timer.Start();
            }

            public float RuntimeMs =>
                ((float) ((((double) this.Runtime) / ((double) Stopwatch.Frequency)) * 1000.0));
            public void Start()
            {
                this.StartTime = m_timer.ElapsedTicks;
            }

            public void Stop()
            {
                this.Runtime += m_timer.ElapsedTicks - this.StartTime;
                this.StartTime = 0x7fffffffffffffffL;
            }

            private bool IsRunning =>
                (this.StartTime != 0x7fffffffffffffffL);
        }
    }
}

