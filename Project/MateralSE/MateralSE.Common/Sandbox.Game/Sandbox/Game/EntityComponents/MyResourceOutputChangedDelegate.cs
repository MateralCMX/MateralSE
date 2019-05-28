namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public delegate void MyResourceOutputChangedDelegate(MyDefinitionId changedResourceId, float oldOutput, MyResourceSourceComponent source);
}

