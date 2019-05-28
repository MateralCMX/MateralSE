namespace VRageRender.Messages
{
    using System;
    using VRageMath;
    using VRageRender;
    using VRageRender.Import;

    public class MyRenderMessageCreateRenderEntityAtmosphere : MyRenderMessageBase
    {
        public uint ID;
        public string DebugName;
        public string Model;
        public MatrixD WorldMatrix;
        public MyMeshDrawTechnique Technique;
        public RenderFlags Flags;
        public VRageRender.CullingOptions CullingOptions;
        public float MaxViewDistance;
        public float AtmosphereRadius;
        public float PlanetRadius;
        public Vector3 AtmosphereWavelengths;
        public bool FadeIn;

        public override string ToString() => 
            (this.DebugName ?? (string.Empty + ", " + this.Model));

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.CreateRenderEntityAtmosphere;
    }
}

