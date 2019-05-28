namespace Sandbox.Game.Weapons
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;
    using VRage.ModAPI;
    using VRageMath;

    public delegate void MyCustomHitMaterialMethod(ref Vector3D hitPoint, ref Vector3 normal, ref Vector3D direction, IMyEntity entity, MySurfaceImpactEnum surfaceImpact, MyEntity weapon, float scale);
}

