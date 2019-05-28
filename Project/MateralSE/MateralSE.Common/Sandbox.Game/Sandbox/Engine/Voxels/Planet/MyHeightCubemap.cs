namespace Sandbox.Engine.Voxels.Planet
{
    using Sandbox.Engine.Voxels;
    using System;
    using System.Runtime.InteropServices;
    using VRage.Utils;
    using VRage.Voxels;

    public class MyHeightCubemap : MyWrappedCubemap<MyHeightmapFace>, IDisposable
    {
        private GCHandle[] m_faceHandles;

        public MyHeightCubemap(string name, MyHeightmapFace[] faces, int resolution)
        {
            base.m_faces = faces;
            base.m_resolution = resolution;
            base.Name = name;
            base.PrepareSides();
            this.m_faceHandles = new GCHandle[6];
            for (int i = 0; i < 6; i++)
            {
                this.m_faceHandles[i] = GCHandle.Alloc(faces[i].Data, GCHandleType.Pinned);
            }
        }

        public void Dispose()
        {
            if (this.m_faceHandles != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    this.m_faceHandles[i].Free();
                }
                this.m_faceHandles = null;
            }
        }

        ~MyHeightCubemap()
        {
            if (this.m_faceHandles != null)
            {
                MyLog.Default.Critical("Heightmap native handles have not been cleared, this will potentially lead to crippling memory leaks.", Array.Empty<object>());
            }
        }

        public unsafe VrPlanetShape.Mapset GetMapset() => 
            new VrPlanetShape.Mapset { 
                Front = (ushort*) this.m_faceHandles[0].AddrOfPinnedObject().ToPointer(),
                Back = (ushort*) this.m_faceHandles[1].AddrOfPinnedObject().ToPointer(),
                Left = (ushort*) this.m_faceHandles[2].AddrOfPinnedObject().ToPointer(),
                Right = (ushort*) this.m_faceHandles[3].AddrOfPinnedObject().ToPointer(),
                Up = (ushort*) this.m_faceHandles[4].AddrOfPinnedObject().ToPointer(),
                Down = (ushort*) this.m_faceHandles[5].AddrOfPinnedObject().ToPointer(),
                Resolution = base.Resolution
            };
    }
}

