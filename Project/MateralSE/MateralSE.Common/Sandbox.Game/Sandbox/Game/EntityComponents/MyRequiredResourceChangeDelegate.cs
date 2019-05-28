namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public delegate void MyRequiredResourceChangeDelegate(MyDefinitionId changedResourceTypeId, MyResourceSinkComponent sink, float oldRequirement, float newRequirement);
}

