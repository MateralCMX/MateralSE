namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Definitions;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRageMath;

    public interface IMyEnvironmentOwner
    {
        Vector3D[] GetBoundingShape(ref Vector3D worldPos, ref Vector3 basisX, ref Vector3 basisY);
        void GetDefinition(ushort index, out MyRuntimeEnvironmentItemInfo def);
        MyPhysicalModelDefinition GetModelForId(short id);
        short GetModelId(MyPhysicalModelDefinition def);
        MyEnvironmentSector GetSectorById(long packedSectorId);
        MyEnvironmentSector GetSectorForPosition(Vector3D positionWorld);
        int GetSeed();
        void GetSurfaceNormalForPoint(ref Vector3D point, out Vector3D normal);
        void ProjectPointToSurface(ref Vector3D center);
        void QuerySurfaceParameters(Vector3D localOrigin, ref BoundingBoxD queryBounds, List<Vector3> queries, List<MySurfaceParams> results);
        void ScheduleWork(MyEnvironmentSector sector, bool parallel);
        void SetSectorPinned(MyEnvironmentSector sector, bool pinned);

        MyWorldEnvironmentDefinition EnvironmentDefinition { get; }

        MyEntity Entity { get; }
    }
}

