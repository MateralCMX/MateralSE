namespace Sandbox.Engine.Physics
{
    using Havok;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Library.Utils;

    internal abstract class PhysicsStepOptimizerBase : IPhysicsStepOptimizer
    {
        protected PhysicsStepOptimizerBase()
        {
        }

        public abstract void DisableOptimizations();
        public abstract void EnableOptimizations(List<MyTuple<HkWorld, MyTimeSpan>> timings);
        protected static void ForEveryActivePhysicsBodyOfType<TBody>(HkWorld world, Action<TBody> action) where TBody: class
        {
            using (HashSet<HkRigidBody>.Enumerator enumerator = world.ActiveRigidBodies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TBody userObject = enumerator.Current.UserObject as TBody;
                    if (userObject != null)
                    {
                        action(userObject);
                    }
                }
            }
        }

        protected static void ForEverySignificantWorld(List<MyTuple<HkWorld, MyTimeSpan>> timings, Action<HkWorld> action)
        {
            MyTimeSpan span;
            double num = 0.0;
            foreach (MyTuple<HkWorld, MyTimeSpan> tuple in timings)
            {
                span = tuple.Item2;
                num += span.Milliseconds;
            }
            double num2 = num / ((double) timings.Count);
            foreach (MyTuple<HkWorld, MyTimeSpan> tuple2 in timings)
            {
                span = tuple2.Item2;
                if (span.Milliseconds >= num2)
                {
                    action(tuple2.Item1);
                }
            }
        }

        public abstract void Unload();
    }
}

