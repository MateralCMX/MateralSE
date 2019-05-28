namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Engine.Physics;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;

    public delegate void MySectorContactEvent(int itemId, MyEntity other, ref MyPhysics.MyContactPointEvent evt);
}

