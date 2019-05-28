namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MyRenderInstanceInfo
    {
        public readonly uint InstanceBufferId;
        public readonly int InstanceStart;
        public readonly int InstanceCount;
        public readonly float MaxViewDistance;
        public readonly MyInstanceFlagsEnum Flags;
        public bool CastShadows =>
            (((int) (this.Flags & MyInstanceFlagsEnum.CastShadows)) != 0);
        public bool ShowLod1 =>
            (((int) (this.Flags & MyInstanceFlagsEnum.ShowLod1)) != 0);
        public bool EnableColorMask =>
            (((int) (this.Flags & MyInstanceFlagsEnum.EnableColorMask)) != 0);
        public MyRenderInstanceInfo(uint instanceBufferId, int instanceStart, int instanceCount, float maxViewDistance, MyInstanceFlagsEnum flags)
        {
            this.Flags = flags;
            this.InstanceBufferId = instanceBufferId;
            this.InstanceStart = instanceStart;
            this.InstanceCount = instanceCount;
            this.MaxViewDistance = maxViewDistance;
        }

        public MyRenderInstanceInfo(uint instanceBufferId, int instanceStart, int instanceCount, bool castShadows, bool showLod1, float maxViewDistance, bool enableColorMaskHsv)
        {
            this.Flags = ((castShadows ? MyInstanceFlagsEnum.CastShadows : ((MyInstanceFlagsEnum) 0)) | (showLod1 ? MyInstanceFlagsEnum.ShowLod1 : ((MyInstanceFlagsEnum) 0))) | (enableColorMaskHsv ? MyInstanceFlagsEnum.EnableColorMask : ((MyInstanceFlagsEnum) 0));
            this.InstanceBufferId = instanceBufferId;
            this.InstanceStart = instanceStart;
            this.InstanceCount = instanceCount;
            this.MaxViewDistance = maxViewDistance;
        }
    }
}

