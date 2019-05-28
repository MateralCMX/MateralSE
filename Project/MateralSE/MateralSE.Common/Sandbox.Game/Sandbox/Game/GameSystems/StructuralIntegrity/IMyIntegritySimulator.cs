namespace Sandbox.Game.GameSystems.StructuralIntegrity
{
    using Sandbox.Game.Entities.Cube;
    using System;
    using VRageMath;

    internal interface IMyIntegritySimulator
    {
        void Add(MySlimBlock block);
        void Close();
        void DebugDraw();
        void Draw();
        void ForceRecalc();
        float GetSupportedWeight(Vector3I pos);
        float GetTension(Vector3I pos);
        bool IsConnectionFine(MySlimBlock blockA, MySlimBlock blockB);
        void Remove(MySlimBlock block);
        bool Simulate(float deltaTime);
    }
}

