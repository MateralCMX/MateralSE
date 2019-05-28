namespace VRage.Game.Components
{
    using System;

    [Flags]
    public enum MyUpdateOrder
    {
        BeforeSimulation = 1,
        Simulation = 2,
        AfterSimulation = 4,
        NoUpdate = 0
    }
}

