namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyGridConnectivityTest
    {
        void GetConnectedBlocks(Vector3I minI, Vector3I maxI, Dictionary<Vector3I, ConnectivityResult> outConnectedCubeBlocks);
    }
}

