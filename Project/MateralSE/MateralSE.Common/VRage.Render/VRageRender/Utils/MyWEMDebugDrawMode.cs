namespace VRageRender.Utils
{
    using System;

    [Flags]
    public enum MyWEMDebugDrawMode
    {
        NONE = 0,
        LINES = 1,
        EDGES = 2,
        LINES_DEPTH = 4,
        FACES = 8,
        VERTICES = 0x10,
        VERTICES_DETAILED = 0x20,
        NORMALS = 0x40
    }
}

