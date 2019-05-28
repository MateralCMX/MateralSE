namespace Sandbox.Game.Entities
{
    using System;

    [Flags]
    public enum MyEntityQueryType : byte
    {
        Static = 1,
        Dynamic = 2,
        Both = 3
    }
}

