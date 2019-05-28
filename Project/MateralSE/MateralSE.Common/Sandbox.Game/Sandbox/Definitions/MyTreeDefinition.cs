namespace Sandbox.Definitions
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_TreeDefinition), typeof(MyPhysicalModelDefinition.Postprocessor))]
    public class MyTreeDefinition : MyEnvironmentItemDefinition
    {
        public float BranchesStartHeight;
        public float HitPoints;
        public string CutEffect;
        public string FallSound;
        public string BreakSound;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_TreeDefinition definition = builder as MyObjectBuilder_TreeDefinition;
            this.BranchesStartHeight = definition.BranchesStartHeight;
            this.HitPoints = definition.HitPoints;
            this.CutEffect = definition.CutEffect;
            this.FallSound = definition.FallSound;
            this.BreakSound = definition.BreakSound;
        }
    }
}

