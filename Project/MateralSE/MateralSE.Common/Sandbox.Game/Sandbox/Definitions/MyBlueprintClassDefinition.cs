namespace Sandbox.Definitions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_BlueprintClassDefinition), (Type) null)]
    public class MyBlueprintClassDefinition : MyDefinitionBase, IEnumerable<MyBlueprintDefinitionBase>, IEnumerable
    {
        public string HighlightIcon;
        public string InputConstraintIcon;
        public string OutputConstraintIcon;
        public string ProgressBarSoundCue;
        private SortedSet<MyBlueprintDefinitionBase> m_blueprints;

        public void AddBlueprint(MyBlueprintDefinitionBase blueprint)
        {
            if (!this.m_blueprints.Contains(blueprint))
            {
                this.m_blueprints.Add(blueprint);
            }
        }

        public bool ContainsBlueprint(MyBlueprintDefinitionBase blueprint) => 
            this.m_blueprints.Contains(blueprint);

        public IEnumerator<MyBlueprintDefinitionBase> GetEnumerator() => 
            this.m_blueprints.GetEnumerator();

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_BlueprintClassDefinition definition = builder as MyObjectBuilder_BlueprintClassDefinition;
            this.HighlightIcon = definition.HighlightIcon;
            this.InputConstraintIcon = definition.InputConstraintIcon;
            this.OutputConstraintIcon = definition.OutputConstraintIcon;
            this.ProgressBarSoundCue = definition.ProgressBarSoundCue;
            this.m_blueprints = new SortedSet<MyBlueprintDefinitionBase>(SubtypeComparer.Static);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.m_blueprints.GetEnumerator();

        private class SubtypeComparer : IComparer<MyBlueprintDefinitionBase>
        {
            public static MyBlueprintClassDefinition.SubtypeComparer Static = new MyBlueprintClassDefinition.SubtypeComparer();

            public int Compare(MyBlueprintDefinitionBase x, MyBlueprintDefinitionBase y) => 
                x.Id.SubtypeName.CompareTo(y.Id.SubtypeName);
        }
    }
}

