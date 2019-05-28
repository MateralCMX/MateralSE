namespace Sandbox.Game
{
    using System;

    [Flags]
    public enum MyExplosionFlags
    {
        CREATE_DEBRIS = 1,
        AFFECT_VOXELS = 2,
        APPLY_FORCE_AND_DAMAGE = 4,
        CREATE_DECALS = 8,
        FORCE_DEBRIS = 0x10,
        CREATE_PARTICLE_EFFECT = 0x20,
        CREATE_SHRAPNELS = 0x40,
        APPLY_DEFORMATION = 0x80,
        CREATE_PARTICLE_DEBRIS = 0x100
    }
}

