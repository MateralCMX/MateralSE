namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Concurrent;
    using VRage.Collections;
    using VRageMath;

    public class MyDirtyRegion
    {
        public ConcurrentQueue<MyCube> PartsToRemove = new ConcurrentQueue<MyCube>();
        public ConcurrentCachingHashSet<Vector3I> Cubes = new ConcurrentCachingHashSet<Vector3I>();

        public void AddCube(Vector3I pos)
        {
            this.Cubes.Add(pos);
        }

        public unsafe void AddCubeRegion(Vector3I min, Vector3I max)
        {
            Vector3I vectori;
            vectori.X = min.X;
            while (vectori.X <= max.X)
            {
                vectori.Y = min.Y;
                while (true)
                {
                    if (vectori.Y > max.Y)
                    {
                        int* numPtr3 = (int*) ref vectori.X;
                        numPtr3[0]++;
                        break;
                    }
                    vectori.Z = min.Z;
                    while (true)
                    {
                        if (vectori.Z > max.Z)
                        {
                            int* numPtr2 = (int*) ref vectori.Y;
                            numPtr2[0]++;
                            break;
                        }
                        this.Cubes.Add(vectori);
                        int* numPtr1 = (int*) ref vectori.Z;
                        numPtr1[0]++;
                    }
                }
            }
        }

        public void Clear()
        {
            this.Cubes.Clear();
        }

        public bool IsDirty
        {
            get
            {
                this.Cubes.ApplyChanges();
                return ((this.Cubes.Count > 0) || !this.PartsToRemove.IsEmpty);
            }
        }
    }
}

