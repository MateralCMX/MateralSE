namespace Sandbox.Game.GameSystems
{
    using System;
    using VRage.Game.ModAPI;
    using VRageMath;

    public class MyOxygenProviderSystemHelper : IMyOxygenProviderSystem
    {
        void IMyOxygenProviderSystem.AddOxygenGenerator(IMyOxygenProvider provider)
        {
            MyOxygenProviderSystem.AddOxygenGenerator(provider);
        }

        float IMyOxygenProviderSystem.GetOxygenInPoint(Vector3D worldPoint) => 
            MyOxygenProviderSystem.GetOxygenInPoint(worldPoint);

        void IMyOxygenProviderSystem.RemoveOxygenGenerator(IMyOxygenProvider provider)
        {
            MyOxygenProviderSystem.RemoveOxygenGenerator(provider);
        }
    }
}

