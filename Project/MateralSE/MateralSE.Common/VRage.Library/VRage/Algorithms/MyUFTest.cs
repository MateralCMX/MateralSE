namespace VRage.Algorithms
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class MyUFTest
    {
        public static void Test()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int count = 0x989680;
            MyUnionFind find = new MyUnionFind();
            find.Resize(count);
            for (int i = 0; i < count; i++)
            {
                find.Union(i, i >> 1);
            }
            int num2 = find.Find(0);
            for (int j = 0; j < count; j++)
            {
                if (num2 != find.Find(j))
                {
                    File.AppendAllText(@"C:\Users\daniel.ilha\Desktop\perf.log", "FAIL!\n");
                    Environment.Exit(1);
                }
            }
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            File.AppendAllText(@"C:\Users\daniel.ilha\Desktop\perf.log", $"Test took {elapsedMilliseconds:N}ms
");
        }
    }
}

