namespace Sandbox.Game.Entities.Cube
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;

    public interface IMyBlockAdditionalModelGenerator
    {
        void BlockAddedToMergedGrid(MySlimBlock block);
        void Close();
        void EnableGenerator(bool enable);
        void GenerateBlocks(MySlimBlock generatingBlock);
        MySlimBlock GetGeneratingBlock(MySlimBlock generatedBlock);
        bool Initialize(MyCubeGrid grid, MyCubeSize gridSizeEnum);
        void UpdateAfterGridSpawn(MySlimBlock block);
        void UpdateAfterSimulation();
        void UpdateBeforeSimulation();
    }
}

