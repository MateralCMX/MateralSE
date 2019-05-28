namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyVoxelBrush
    {
        void CutOut(MyVoxelBase map);
        void Draw(ref Color color);
        void Fill(MyVoxelBase map, byte matId);
        BoundingBoxD GetBoundaries();
        List<MyGuiControlBase> GetGuiControls();
        BoundingBoxD GetWorldBoundaries();
        void Paint(MyVoxelBase map, byte matId);
        BoundingBoxD PeekWorldBoundingBox(ref Vector3D targetPosition);
        void Revert(MyVoxelBase map);
        void SetPosition(ref Vector3D targetPosition);
        void SetRotation(ref MatrixD rotationMat);

        float MinScale { get; }

        float MaxScale { get; }

        bool AutoRotate { get; }

        string SubtypeName { get; }
    }
}

