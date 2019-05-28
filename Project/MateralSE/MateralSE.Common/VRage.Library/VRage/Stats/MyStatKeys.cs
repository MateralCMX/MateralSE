namespace VRage.Stats
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MyStatKeys
    {
        private static Dictionary<StatKeysEnum, MyNamePriorityPair> m_collection;

        static MyStatKeys()
        {
            MyNamePriorityPair pair = new MyNamePriorityPair {
                Name = "Frame",
                Priority = 0x44c
            };
            Dictionary<StatKeysEnum, MyNamePriorityPair> dictionary1 = new Dictionary<StatKeysEnum, MyNamePriorityPair>();
            dictionary1.Add(StatKeysEnum.Frame, pair);
            pair = new MyNamePriorityPair {
                Name = "FPS",
                Priority = 0x3e8
            };
            dictionary1.Add(StatKeysEnum.FPS, pair);
            pair = new MyNamePriorityPair {
                Name = "UPS",
                Priority = 900
            };
            dictionary1.Add(StatKeysEnum.UPS, pair);
            pair = new MyNamePriorityPair {
                Name = "Simulation speed",
                Priority = 800
            };
            dictionary1.Add(StatKeysEnum.SimSpeed, pair);
            pair = new MyNamePriorityPair {
                Name = "Simulation CPU Load: {0}% {3:0.00}ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.SimCpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Thread CPU Load: {0}% {3:0.00}ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ThreadCpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Render CPU Load: {0}% {3:0.00}ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.RenderCpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Render GPU Load: {0}% {3:0.00}ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.RenderGpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Server simulation speed",
                Priority = 700
            };
            dictionary1.Add(StatKeysEnum.ServerSimSpeed, pair);
            pair = new MyNamePriorityPair {
                Name = "Server simulation CPU Load: {0}%",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ServerSimCpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Server thread CPU Load: {0}%",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ServerThreadCpuLoad, pair);
            pair = new MyNamePriorityPair {
                Name = "Up: {0.##} kB/s",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.Up, pair);
            pair = new MyNamePriorityPair {
                Name = "Down: {0.##} kB/s",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.Down, pair);
            pair = new MyNamePriorityPair {
                Name = "Server Up: {0.##} kB/s",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ServerUp, pair);
            pair = new MyNamePriorityPair {
                Name = "Server Down: {0.##} kB/s",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ServerDown, pair);
            pair = new MyNamePriorityPair {
                Name = "Roundtrip: {0}ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.Roundtrip, pair);
            pair = new MyNamePriorityPair {
                Name = "Frame time: {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.FrameTime, pair);
            pair = new MyNamePriorityPair {
                Name = "Frame avg time: {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.FrameAvgTime, pair);
            pair = new MyNamePriorityPair {
                Name = "Frame min time: {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.FrameMinTime, pair);
            pair = new MyNamePriorityPair {
                Name = "Frame max time: {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.FrameMaxTime, pair);
            pair = new MyNamePriorityPair {
                Name = "Update lag (per s)",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.UpdateLag, pair);
            pair = new MyNamePriorityPair {
                Name = "GC Memory",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.GcMemory, pair);
            pair = new MyNamePriorityPair {
                Name = "Process memory",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ProcessMemory, pair);
            pair = new MyNamePriorityPair {
                Name = "Active particle effects",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ActiveParticleEffs, pair);
            pair = new MyNamePriorityPair {
                Name = "Physics worlds count: {0}",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.PhysWorldCount, pair);
            pair = new MyNamePriorityPair {
                Name = "Active rigid bodies: {0}",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.ActiveRigBodies, pair);
            pair = new MyNamePriorityPair {
                Name = "Physics step time (sum): {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.PhysStepTimeSum, pair);
            pair = new MyNamePriorityPair {
                Name = "Physics step time (avg): {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.PhysStepTimeAvg, pair);
            pair = new MyNamePriorityPair {
                Name = "Physics step time (max): {0} ms",
                Priority = 0
            };
            dictionary1.Add(StatKeysEnum.PhysStepTimeMax, pair);
            m_collection = dictionary1;
        }

        public static string GetName(StatKeysEnum key)
        {
            MyNamePriorityPair pair;
            return (m_collection.TryGetValue(key, out pair) ? pair.Name : string.Empty);
        }

        public static void GetNameAndPriority(StatKeysEnum key, out string name, out int priority)
        {
            MyNamePriorityPair pair;
            if (!m_collection.TryGetValue(key, out pair))
            {
                name = string.Empty;
                priority = 0;
            }
            else
            {
                name = pair.Name;
                priority = pair.Priority;
            }
        }

        public static int GetPriority(StatKeysEnum key)
        {
            MyNamePriorityPair pair;
            return (m_collection.TryGetValue(key, out pair) ? pair.Priority : 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MyNamePriorityPair
        {
            public string Name;
            public int Priority;
        }

        public enum StatKeysEnum
        {
            None,
            Frame,
            FPS,
            UPS,
            SimSpeed,
            SimCpuLoad,
            ThreadCpuLoad,
            RenderCpuLoad,
            RenderGpuLoad,
            ServerSimSpeed,
            ServerSimCpuLoad,
            ServerThreadCpuLoad,
            Up,
            Down,
            ServerUp,
            ServerDown,
            Roundtrip,
            FrameTime,
            FrameAvgTime,
            FrameMinTime,
            FrameMaxTime,
            UpdateLag,
            GcMemory,
            ProcessMemory,
            ActiveParticleEffs,
            PhysWorldCount,
            ActiveRigBodies,
            PhysStepTimeSum,
            PhysStepTimeAvg,
            PhysStepTimeMax
        }
    }
}

