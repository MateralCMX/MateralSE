namespace VRage.Utils
{
    using System;
    using VRageMath;

    public static class MySectorConstants
    {
        public const float SECTOR_SIZE = 50000f;
        public const float SECTOR_SIZE_HALF = 25000f;
        public static readonly Vector3 SECTOR_SIZE_VECTOR3 = new Vector3(50000f, 50000f, 50000f);
        public static readonly float SECTOR_DIAMETER = SECTOR_SIZE_VECTOR3.Length();
        public const float SAFE_SECTOR_SIZE = 50200f;
        public const float SECTOR_SIZE_FOR_PHYS_OBJECTS_SIZE_HALF = 22500f;
        public const float SAFE_SECTOR_SIZE_HALF = 25100f;
        public static readonly BoundingBox SAFE_SECTOR_SIZE_BOUNDING_BOX = new BoundingBox(new Vector3(-25100f, -25100f, -25100f), new Vector3(25100f, 25100f, 25100f));
        public static readonly Vector3[] SAFE_SECTOR_SIZE_BOUNDING_BOX_CORNERS = SAFE_SECTOR_SIZE_BOUNDING_BOX.GetCorners();
        public static readonly BoundingBox SECTOR_SIZE_FOR_PHYS_OBJECTS_BOUNDING_BOX = new BoundingBox(new Vector3(-22500f, -22500f, -22500f), new Vector3(22500f, 22500f, 22500f));
        public static readonly BoundingBox SECTOR_SIZE_BOUNDING_BOX = new BoundingBox(new Vector3(-25000f, -25000f, -25000f), new Vector3(25000f, 25000f, 25000f));
    }
}

