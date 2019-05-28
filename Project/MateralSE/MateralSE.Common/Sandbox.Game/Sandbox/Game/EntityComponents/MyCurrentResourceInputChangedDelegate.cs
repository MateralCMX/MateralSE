namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public delegate void MyCurrentResourceInputChangedDelegate(MyDefinitionId resourceTypeId, float oldInput, MyResourceSinkComponent sink);
}

