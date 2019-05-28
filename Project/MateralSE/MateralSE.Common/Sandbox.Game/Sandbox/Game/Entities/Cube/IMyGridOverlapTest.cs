namespace Sandbox.Game.Entities.Cube
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    internal interface IMyGridOverlapTest
    {
        void GetBlocks(Vector3I minI, Vector3I maxI, Dictionary<Vector3I, OverlapResult> outOverlappedBlocks);
    }
}

