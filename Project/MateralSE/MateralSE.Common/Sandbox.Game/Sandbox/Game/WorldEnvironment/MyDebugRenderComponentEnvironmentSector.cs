namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities.Planet;
    using System;
    using VRage.ModAPI;

    internal class MyDebugRenderComponentEnvironmentSector : MyDebugRenderComponent
    {
        public MyDebugRenderComponentEnvironmentSector(IMyEntity entity) : base(entity)
        {
        }

        public override void DebugDraw()
        {
            if (MyPlanetEnvironmentSessionComponent.DebugDrawSectors)
            {
                ((MyEnvironmentSector) base.Entity).DebugDraw();
            }
        }
    }
}

