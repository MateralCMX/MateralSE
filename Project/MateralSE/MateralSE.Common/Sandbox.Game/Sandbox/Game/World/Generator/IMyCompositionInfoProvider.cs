namespace Sandbox.Game.World.Generator
{
    using System;
    using VRage.Game;

    internal interface IMyCompositionInfoProvider
    {
        void Close();

        IMyCompositeDeposit[] Deposits { get; }

        IMyCompositeShape[] FilledShapes { get; }

        IMyCompositeShape[] RemovedShapes { get; }

        MyVoxelMaterialDefinition DefaultMaterial { get; }
    }
}

