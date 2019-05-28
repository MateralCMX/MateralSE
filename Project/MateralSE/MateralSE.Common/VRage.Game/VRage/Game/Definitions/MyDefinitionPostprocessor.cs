namespace VRage.Game.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public abstract class MyDefinitionPostprocessor
    {
        public Type DefinitionType;
        public static PostprocessorComparer Comparer = new PostprocessorComparer();

        protected MyDefinitionPostprocessor()
        {
        }

        public abstract void AfterLoaded(ref Bundle definitions);
        public abstract void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions);
        public virtual void OverrideBy(ref Bundle currentDefinitions, ref Bundle overrideBySet)
        {
            foreach (KeyValuePair<MyStringHash, MyDefinitionBase> pair in overrideBySet.Definitions)
            {
                if (pair.Value.Enabled)
                {
                    currentDefinitions.Definitions[pair.Key] = pair.Value;
                    continue;
                }
                currentDefinitions.Definitions.Remove(pair.Key);
            }
        }

        public virtual int Priority =>
            500;

        [StructLayout(LayoutKind.Sequential)]
        public struct Bundle
        {
            public MyModContext Context;
            public MyDefinitionSet Set;
            public Dictionary<MyStringHash, MyDefinitionBase> Definitions;
        }

        public class PostprocessorComparer : IComparer<MyDefinitionPostprocessor>
        {
            public int Compare(MyDefinitionPostprocessor x, MyDefinitionPostprocessor y) => 
                (y.Priority - x.Priority);
        }
    }
}

