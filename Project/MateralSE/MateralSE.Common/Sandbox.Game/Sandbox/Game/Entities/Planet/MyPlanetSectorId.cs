namespace Sandbox.Game.Entities.Planet
{
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRageMath;

    public class MyPlanetSectorId
    {
        private const long CoordMask = 0xffffffL;
        private const int CoordBits = 0x18;
        private const int FaceOffset = 0x30;
        private const long FaceMask = 7L;
        private const int FaceBits = 3;
        private const long LodMask = 0xffL;
        private const int LodBits = 8;
        private const int LodOffset = 0x33;

        private MyPlanetSectorId()
        {
        }

        public static Vector3I DecomposeSectorId(long sectorID) => 
            new Vector3I((float) ((int) (sectorID & 0xffffffL)), (float) ((sectorID >> 0x18) & 0xffffffL), (float) ((sectorID >> 0x30) & 7L));

        public static int GetFace(long packedSectorId) => 
            ((int) ((packedSectorId >> 0x30) & 7L));

        public static long MakeSectorEntityId(int x, int y, int lod, int face, long parentId) => 
            MyEntityIdentifier.ConstructIdFromString(MyEntityIdentifier.ID_OBJECT_TYPE.PLANET_ENVIRONMENT_SECTOR, $"P({parentId})S(x{x}, y{y}, f{face}, l{lod})");

        public static long MakeSectorId(int x, int y, int face, int lod = 0) => 
            ((((x & 0xffffffL) | ((y & 0xffffffL) << 0x18)) | ((face & 7L) << 0x30)) | ((lod & 0xffL) << 0x33));
    }
}

