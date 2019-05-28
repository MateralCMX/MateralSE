namespace Sandbox.Game.WorldEnvironment
{
    using System;
    using VRageMath;

    public interface IMy2DClipmapNodeHandler
    {
        void Close();
        void Init(IMy2DClipmapManager parent, int x, int y, int lod, ref BoundingBox2D bounds);
        void InitJoin(IMy2DClipmapNodeHandler[] children);
        unsafe void Split(BoundingBox2D* childBoxes, ref IMy2DClipmapNodeHandler[] children);
    }
}

