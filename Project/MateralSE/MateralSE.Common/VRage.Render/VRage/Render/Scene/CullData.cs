namespace VRage.Render.Scene
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct CullData
    {
        public int IterationOffset;
        public int ActiveActorsLastFrame;
        public MyCullResultsBase ActiveResults;
        public List<MyBruteCullData> CulledActors;
        public List<MyBruteCullData> ActiveActors;
        public static CullData Create() => 
            new CullData { 
                IterationOffset = 0,
                ActiveResults = null,
                ActiveActorsLastFrame = 0,
                CulledActors = new List<MyBruteCullData>(),
                ActiveActors = new List<MyBruteCullData>()
            };
    }
}

