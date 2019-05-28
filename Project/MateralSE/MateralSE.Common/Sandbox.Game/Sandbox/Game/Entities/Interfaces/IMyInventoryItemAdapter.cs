namespace Sandbox.Game.Entities.Interfaces
{
    using System;
    using VRage;

    public interface IMyInventoryItemAdapter
    {
        float Mass { get; }

        float Volume { get; }

        bool HasIntegralAmounts { get; }

        MyFixedPoint MaxStackAmount { get; }

        string DisplayNameText { get; }

        string[] Icons { get; }

        MyStringId? IconSymbol { get; }
    }
}

