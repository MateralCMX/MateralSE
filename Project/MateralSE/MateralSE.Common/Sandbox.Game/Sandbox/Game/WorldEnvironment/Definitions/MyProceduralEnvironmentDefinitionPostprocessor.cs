namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;

    public class MyProceduralEnvironmentDefinitionPostprocessor : MyDefinitionPostprocessor
    {
        public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
        {
        }

        public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
        {
            foreach (KeyValuePair<MyStringHash, MyDefinitionBase> pair in definitions)
            {
                ((MyProceduralEnvironmentDefinition) pair.Value).Prepare();
            }
        }
    }
}

