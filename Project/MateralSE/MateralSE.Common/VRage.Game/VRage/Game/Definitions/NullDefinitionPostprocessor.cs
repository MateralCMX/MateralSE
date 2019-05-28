namespace VRage.Game.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    public class NullDefinitionPostprocessor : MyDefinitionPostprocessor
    {
        public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
        {
        }

        public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
        {
        }
    }
}

