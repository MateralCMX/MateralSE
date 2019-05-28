namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ToolItemDefinition), (Type) null)]
    public class MyToolItemDefinition : MyPhysicalItemDefinition
    {
        public Sandbox.Definitions.MyVoxelMiningDefinition[] VoxelMinings;
        public List<Sandbox.Definitions.MyToolActionDefinition> PrimaryActions = new List<Sandbox.Definitions.MyToolActionDefinition>();
        public List<Sandbox.Definitions.MyToolActionDefinition> SecondaryActions = new List<Sandbox.Definitions.MyToolActionDefinition>();
        public float HitDistance;

        private void CopyActions(MyObjectBuilder_ToolItemDefinition.MyToolActionDefinition[] sourceActions, List<Sandbox.Definitions.MyToolActionDefinition> targetList)
        {
            if ((sourceActions != null) && (sourceActions.Length != 0))
            {
                for (int i = 0; i < sourceActions.Length; i++)
                {
                    Sandbox.Definitions.MyToolActionDefinition item = new Sandbox.Definitions.MyToolActionDefinition {
                        Name = MyStringId.GetOrCompute(sourceActions[i].Name),
                        StartTime = sourceActions[i].StartTime,
                        EndTime = sourceActions[i].EndTime,
                        Efficiency = sourceActions[i].Efficiency,
                        StatsEfficiency = sourceActions[i].StatsEfficiency,
                        SwingSound = sourceActions[i].SwingSound,
                        SwingSoundStart = sourceActions[i].SwingSoundStart,
                        HitStart = sourceActions[i].HitStart,
                        HitDuration = sourceActions[i].HitDuration,
                        HitSound = sourceActions[i].HitSound,
                        CustomShapeRadius = sourceActions[i].CustomShapeRadius,
                        Crosshair = sourceActions[i].Crosshair
                    };
                    if (sourceActions[i].HitConditions != null)
                    {
                        item.HitConditions = new MyToolHitCondition[sourceActions[i].HitConditions.Length];
                        for (int j = 0; j < item.HitConditions.Length; j++)
                        {
                            item.HitConditions[j].EntityType = sourceActions[i].HitConditions[j].EntityType;
                            item.HitConditions[j].Animation = sourceActions[i].HitConditions[j].Animation;
                            item.HitConditions[j].AnimationTimeScale = sourceActions[i].HitConditions[j].AnimationTimeScale;
                            item.HitConditions[j].StatsAction = sourceActions[i].HitConditions[j].StatsAction;
                            item.HitConditions[j].StatsActionIfHit = sourceActions[i].HitConditions[j].StatsActionIfHit;
                            item.HitConditions[j].StatsModifier = sourceActions[i].HitConditions[j].StatsModifier;
                            item.HitConditions[j].StatsModifierIfHit = sourceActions[i].HitConditions[j].StatsModifierIfHit;
                            item.HitConditions[j].Component = sourceActions[i].HitConditions[j].Component;
                        }
                    }
                    targetList.Add(item);
                }
            }
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ToolItemDefinition definition = builder as MyObjectBuilder_ToolItemDefinition;
            if ((definition.VoxelMinings != null) && (definition.VoxelMinings.Length != 0))
            {
                this.VoxelMinings = new Sandbox.Definitions.MyVoxelMiningDefinition[definition.VoxelMinings.Length];
                for (int i = 0; i < definition.VoxelMinings.Length; i++)
                {
                    this.VoxelMinings[i].MinedOre = definition.VoxelMinings[i].MinedOre;
                    this.VoxelMinings[i].HitCount = definition.VoxelMinings[i].HitCount;
                    this.VoxelMinings[i].PhysicalItemId = definition.VoxelMinings[i].PhysicalItemId;
                    this.VoxelMinings[i].RemovedRadius = definition.VoxelMinings[i].RemovedRadius;
                    this.VoxelMinings[i].OnlyApplyMaterial = definition.VoxelMinings[i].OnlyApplyMaterial;
                }
            }
            this.CopyActions(definition.PrimaryActions, this.PrimaryActions);
            this.CopyActions(definition.SecondaryActions, this.SecondaryActions);
            this.HitDistance = definition.HitDistance;
        }
    }
}

