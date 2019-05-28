namespace Sandbox.Game.Entities
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;

    internal delegate void SensorFilterHandler(MySensorBase sender, MyEntity detectedEntity, ref bool processEntity);
}

