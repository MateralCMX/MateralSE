namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyEnvironmentDataProvider
    {
        void DebugDraw();
        MyEnvironmentDataView GetItemView(int lod, ref Vector2I start, ref Vector2I end, ref Vector3D localOrigin);
        MyLogicalEnvironmentSectorBase GetLogicalSector(long sectorId);
        MyObjectBuilder_EnvironmentDataProvider GetObjectBuilder();

        IEnumerable<MyLogicalEnvironmentSectorBase> LogicalSectors { get; }
    }
}

