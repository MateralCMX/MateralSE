namespace VRage.Game.Definitions.Animation
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;

    [MyDefinitionType(typeof(MyObjectBuilder_AnimationControllerDefinition), typeof(MyAnimationControllerDefinitionPostprocess))]
    public class MyAnimationControllerDefinition : MyDefinitionBase
    {
        public List<MyObjectBuilder_AnimationLayer> Layers = new List<MyObjectBuilder_AnimationLayer>();
        public List<MyObjectBuilder_AnimationSM> StateMachines = new List<MyObjectBuilder_AnimationSM>();
        public List<MyObjectBuilder_AnimationFootIkChain> FootIkChains = new List<MyObjectBuilder_AnimationFootIkChain>();
        public List<string> IkIgnoredBones = new List<string>();

        public void Clear()
        {
            this.Layers.Clear();
            this.StateMachines.Clear();
            this.FootIkChains.Clear();
            this.IkIgnoredBones.Clear();
        }

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            string descriptionString;
            string displayNameString;
            MyObjectBuilder_AnimationControllerDefinition local1 = MyDefinitionManagerBase.GetObjectFactory().CreateObjectBuilder<MyObjectBuilder_AnimationControllerDefinition>(this);
            local1.Id = (SerializableDefinitionId) base.Id;
            MyObjectBuilder_AnimationControllerDefinition local3 = local1;
            if (base.DescriptionEnum == null)
            {
                descriptionString = base.DescriptionString;
            }
            else
            {
                descriptionString = base.DescriptionEnum.Value.ToString();
            }
            local3.Description = descriptionString;
            if (base.DisplayNameEnum == null)
            {
                displayNameString = base.DisplayNameString;
            }
            else
            {
                displayNameString = base.DisplayNameEnum.Value.ToString();
            }
            local3.DisplayName = displayNameString;
            MyObjectBuilder_AnimationControllerDefinition local2 = local3;
            local2.Icons = base.Icons;
            local2.Public = base.Public;
            local2.Enabled = base.Enabled;
            local2.AvailableInSurvival = base.AvailableInSurvival;
            local2.StateMachines = this.StateMachines.ToArray();
            local2.Layers = this.Layers.ToArray();
            local2.FootIkChains = this.FootIkChains.ToArray();
            local2.IkIgnoredBones = this.IkIgnoredBones.ToArray();
            return local2;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AnimationControllerDefinition definition = builder as MyObjectBuilder_AnimationControllerDefinition;
            if (definition.Layers != null)
            {
                this.Layers.AddRange(definition.Layers);
            }
            if (definition.StateMachines != null)
            {
                this.StateMachines.AddRange(definition.StateMachines);
            }
            if (definition.FootIkChains != null)
            {
                this.FootIkChains.AddRange(definition.FootIkChains);
            }
            if (definition.IkIgnoredBones != null)
            {
                this.IkIgnoredBones.AddRange(definition.IkIgnoredBones);
            }
        }
    }
}

