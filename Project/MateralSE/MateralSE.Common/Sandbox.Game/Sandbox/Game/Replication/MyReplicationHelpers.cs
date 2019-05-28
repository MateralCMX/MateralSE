namespace Sandbox.Game.Replication
{
    using System;
    using System.Runtime.InteropServices;

    public static class MyReplicationHelpers
    {
        public static float RampPriority(float priority, int frameCountWithoutSync, float updateOncePer, float rampAmount = 0.5f, bool alsoRampDown = true)
        {
            if (frameCountWithoutSync < updateOncePer)
            {
                return (alsoRampDown ? 0f : priority);
            }
            float num = (frameCountWithoutSync - updateOncePer) / updateOncePer;
            if (num > 1f)
            {
                float num2 = (num - 1f) * rampAmount;
                priority *= num2;
            }
            return priority;
        }
    }
}

