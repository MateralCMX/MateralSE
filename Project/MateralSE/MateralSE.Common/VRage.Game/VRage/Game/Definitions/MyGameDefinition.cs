namespace VRage.Game.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VRage.Game;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_GameDefinition), typeof(MyGameDefinition.Postprocess))]
    public class MyGameDefinition : MyDefinitionBase
    {
        public static readonly MyDefinitionId Default = new MyDefinitionId(typeof(MyObjectBuilder_GameDefinition), "Default");
        public Dictionary<string, MyDefinitionId?> SessionComponents;
        public static readonly MyGameDefinition DefaultDefinition;

        static MyGameDefinition()
        {
            MyGameDefinition definition1 = new MyGameDefinition();
            definition1.Id = Default;
            definition1.SessionComponents = new Dictionary<string, MyDefinitionId?>();
            DefaultDefinition = definition1;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_GameDefinition definition = (MyObjectBuilder_GameDefinition) builder;
            if (definition.InheritFrom != null)
            {
                MyGameDefinition definition2 = MyDefinitionManagerBase.Static.GetLoadingSet().GetDefinition<MyGameDefinition>(new MyDefinitionId(typeof(MyObjectBuilder_GameDefinition), definition.InheritFrom));
                if (definition2 != null)
                {
                    this.SessionComponents = new Dictionary<string, MyDefinitionId?>(definition2.SessionComponents);
                }
                else
                {
                    object[] args = new object[] { definition.InheritFrom, definition.SubtypeId };
                    MyLog.Default.Error("Could not find parent definition {0} for game definition {1}.", args);
                }
            }
            if (this.SessionComponents == null)
            {
                this.SessionComponents = new Dictionary<string, MyDefinitionId?>();
            }
            foreach (MyObjectBuilder_GameDefinition.Comp comp in definition.SessionComponents)
            {
                if (comp.Type != null)
                {
                    this.SessionComponents[comp.ComponentName] = new MyDefinitionId(MyObjectBuilderType.Parse(comp.Type), comp.Subtype);
                    continue;
                }
                this.SessionComponents[comp.ComponentName] = null;
            }
            if (definition.Default)
            {
                this.SetDefault();
            }
        }

        private void SetDefault()
        {
            MyGameDefinition definition1 = new MyGameDefinition();
            definition1.SessionComponents = this.SessionComponents;
            definition1.Id = Default;
            MyGameDefinition def = definition1;
            MyDefinitionManagerBase.Static.GetLoadingSet().AddOrRelaceDefinition(def);
        }

        private class Postprocess : MyDefinitionPostprocessor
        {
            public override void AfterLoaded(ref MyDefinitionPostprocessor.Bundle definitions)
            {
            }

            public override void AfterPostprocess(MyDefinitionSet set, Dictionary<MyStringHash, MyDefinitionBase> definitions)
            {
                if (!set.ContainsDefinition(MyGameDefinition.Default))
                {
                    set.GetDefinitionsOfType<MyGameDefinition>().First<MyGameDefinition>().SetDefault();
                }
            }
        }
    }
}

