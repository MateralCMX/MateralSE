namespace Sandbox.Game.Weapons
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;

    public delegate void MyCustomHitParticlesMethod(ref Vector3D hitPoint, ref Vector3 normal, ref Vector3D direction, IMyEntity entity, MyEntity weapon, float scale, MyEntity ownerEntity = null);
}

