namespace ParallelTasks
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Collections;

    public class DependencyResolver : IDisposable
    {
        private static readonly MyConcurrentArrayBufferPool<int> m_pool = new MyConcurrentArrayBufferPool<int>("DependencySolver");
        private readonly DependencyBatch m_batch;
        private readonly MyTuple<int[], int>[] m_dependencies;

        public DependencyResolver(DependencyBatch batch)
        {
            this.m_batch = batch;
            this.m_dependencies = new MyTuple<int[], int>[500];
        }

        public JobToken Add(Action job) => 
            new JobToken(this.m_batch.Add(job), this);

        private unsafe void AddDependency(int parent, int child)
        {
            int* localPtr1 = ref this.m_dependencies[parent].Item2;
            int num2 = localPtr1[0];
            localPtr1[0] = num2 + 1;
            int index = num2;
            int[] arr = this.m_dependencies[parent].Item1;
            if ((arr == null) || (arr.Length == index))
            {
                Resize(ref arr);
                this.m_dependencies[parent].Item1 = arr;
            }
            arr[index] = child;
        }

        public void Dispose()
        {
            for (int i = 0; i < this.m_dependencies.Length; i++)
            {
                MyTuple<int[], int> tuple = this.m_dependencies[i];
                int num2 = tuple.Item2;
                if (num2 > 0)
                {
                    using (DependencyBatch.StartToken token = this.m_batch.Job(i))
                    {
                        int[] instance = tuple.Item1;
                        int index = 0;
                        while (true)
                        {
                            if (index >= num2)
                            {
                                m_pool.Return(instance);
                                break;
                            }
                            token.Starts(instance[index]);
                            index++;
                        }
                    }
                    this.m_dependencies[i] = new MyTuple<int[], int>();
                }
            }
        }

        private static void Resize(ref int[] arr)
        {
            bool flag = arr == null;
            int bucketId = flag ? 8 : (arr.Length + 1);
            int[] destinationArray = m_pool.Get(bucketId);
            if (!flag)
            {
                Array.Copy(arr, destinationArray, arr.Length);
                m_pool.Return(arr);
            }
            arr = destinationArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JobToken
        {
            private readonly int m_jobId;
            private readonly DependencyResolver m_solver;
            public JobToken(int jobId, DependencyResolver solver)
            {
                this.m_jobId = jobId;
                this.m_solver = solver;
            }

            public DependencyResolver.JobToken Starts(DependencyResolver.JobToken child)
            {
                child.DependsOn(this);
                return this;
            }

            public DependencyResolver.JobToken DependsOn(DependencyResolver.JobToken parent)
            {
                int jobId = this.m_jobId;
                int num2 = parent.m_jobId;
                if (num2 == jobId)
                {
                    throw new Exception("Cannot start/depend on itself");
                }
                this.m_solver.AddDependency(num2, jobId);
                return this;
            }
        }
    }
}

