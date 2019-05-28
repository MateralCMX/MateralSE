namespace Sandbox.Game.AI.Navigation
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using VRageMath;

    public abstract class MySteeringBase
    {
        public MySteeringBase(MyBotNavigation parent, float weight)
        {
            this.Weight = weight;
            this.Parent = parent;
        }

        public abstract void AccumulateCorrection(ref Vector3 correction, ref float weight);
        public virtual void Cleanup()
        {
        }

        [Conditional("DEBUG")]
        public virtual void DebugDraw()
        {
        }

        public abstract string GetName();
        public virtual void Update()
        {
        }

        public float Weight { get; protected set; }

        public MyBotNavigation Parent { get; private set; }
    }
}

