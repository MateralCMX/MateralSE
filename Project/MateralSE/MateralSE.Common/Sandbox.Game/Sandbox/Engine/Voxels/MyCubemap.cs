namespace Sandbox.Engine.Voxels
{
    using System;
    using VRage.Utils;

    public class MyCubemap : MyWrappedCubemap<MyCubemapData<byte>>
    {
        public MyCubemap(params MyCubemapData<byte>[] faces)
        {
            if (faces.Length != 6)
            {
                MyDebug.FailRelease("When loading cubemap exactly 6 faces are expected.");
            }
            base.m_faces = faces;
            base.m_resolution = faces[0].Resolution;
            base.PrepareSides();
        }
    }
}

