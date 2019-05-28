namespace VRage.Game.Components
{
    using System;

    [Flags]
    public enum RigidBodyFlag
    {
        RBF_DEFAULT = 0,
        RBF_KINEMATIC = 2,
        RBF_STATIC = 4,
        RBF_DISABLE_COLLISION_RESPONSE = 0x40,
        RBF_DOUBLED_KINEMATIC = 0x80,
        RBF_BULLET = 0x100,
        RBF_DEBRIS = 0x200,
        RBF_KEYFRAMED_REPORTING = 0x400,
        RBF_UNLOCKED_SPEEDS = 0x800
    }
}

