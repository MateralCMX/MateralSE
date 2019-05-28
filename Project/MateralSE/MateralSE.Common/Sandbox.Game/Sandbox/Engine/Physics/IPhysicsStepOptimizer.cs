namespace Sandbox.Engine.Physics
{
    using System;
    using System.Collections.Generic;

    internal interface IPhysicsStepOptimizer
    {
        void DisableOptimizations();
        void EnableOptimizations(List<MyTuple<HkWorld, MyTimeSpan>> timings);
        void Unload();
    }
}

