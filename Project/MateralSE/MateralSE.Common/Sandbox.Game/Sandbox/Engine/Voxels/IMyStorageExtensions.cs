namespace Sandbox.Engine.Voxels
{
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.World;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Voxels;
    using VRage.ModAPI;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public static class IMyStorageExtensions
    {
        public static void ClampVoxelCoord(this VRage.ModAPI.IMyStorage self, ref Vector3I voxelCoord, int distance = 1)
        {
            if (self != null)
            {
                Vector3I max = self.Size - distance;
                Vector3I.Clamp(ref voxelCoord, ref Vector3I.Zero, ref max, out voxelCoord);
            }
        }

        public static void DebugDrawChunk(this VRage.Game.Voxels.IMyStorage self, Vector3I start, Vector3I end, Color? c = new Color?())
        {
            if (c == null)
            {
                c = new Color?(Color.Blue);
            }
            BoundingBoxD box = new BoundingBoxD((Vector3D) start, end + 1);
            box.Translate((Vector3D) (-(self.Size * 0.5) - 0.5));
            foreach (MyVoxelBase base2 in from x in MySession.Static.VoxelMaps.Instances
                where ReferenceEquals(x.Storage, self)
                select x)
            {
                MyRenderProxy.DebugDrawOBB(new MyOrientedBoundingBoxD(box, base2.WorldMatrix), c.Value, 0.5f, true, true, false);
            }
        }

        public static MyVoxelGeometry GetGeometry(this VRage.Game.Voxels.IMyStorage self)
        {
            MyStorageBase base2 = self as MyStorageBase;
            return base2?.Geometry;
        }

        public static MyVoxelMaterialDefinition GetMaterialAt(this VRage.Game.Voxels.IMyStorage self, ref Vector3D localCoords)
        {
            Vector3I lodVoxelRangeMin = Vector3D.Floor(localCoords / 1.0);
            MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
            target.Resize(Vector3I.One);
            self.ReadRange(target, MyStorageDataTypeFlags.Material, 0, lodVoxelRangeMin, lodVoxelRangeMin);
            return MyDefinitionManager.Static.GetVoxelMaterialDefinition(target.Material(0));
        }

        public static MyVoxelMaterialDefinition GetMaterialAt(this VRage.Game.Voxels.IMyStorage self, ref Vector3I voxelCoords)
        {
            MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
            target.Resize(Vector3I.One);
            self.ReadRange(target, MyStorageDataTypeFlags.All, 0, voxelCoords, voxelCoords);
            byte materialIndex = target.Material(0);
            return ((materialIndex != 0xff) ? MyDefinitionManager.Static.GetVoxelMaterialDefinition(materialIndex) : null);
        }

        public static ContainmentType Intersect(this VRage.Game.Voxels.IMyStorage self, ref BoundingBox box, bool lazy = true)
        {
            BoundingBoxI xi = new BoundingBoxI(box);
            return self.Intersect(ref xi, 0, lazy);
        }
    }
}

