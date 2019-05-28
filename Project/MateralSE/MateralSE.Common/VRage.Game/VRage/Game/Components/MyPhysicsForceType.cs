namespace VRage.Game.Components
{
    using System;

    public enum MyPhysicsForceType : byte
    {
        APPLY_WORLD_IMPULSE_AND_WORLD_ANGULAR_IMPULSE = 0,
        ADD_BODY_FORCE_AND_BODY_TORQUE = 1,
        APPLY_WORLD_FORCE = 2
    }
}

