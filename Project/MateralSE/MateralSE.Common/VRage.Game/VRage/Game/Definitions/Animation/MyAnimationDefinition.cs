namespace VRage.Game.Definitions.Animation
{
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_AnimationDefinition), (Type) null)]
    public class MyAnimationDefinition : MyDefinitionBase
    {
        public string AnimationModel;
        public string AnimationModelFPS;
        public int ClipIndex;
        public string InfluenceArea;
        public bool AllowInCockpit;
        public bool AllowWithWeapon;
        public bool Loop;
        public string[] SupportedSkeletons;
        public AnimationStatus Status;
        public MyDefinitionId LeftHandItem;
        public AnimationSet[] AnimationSets;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_AnimationDefinition definition = builder as MyObjectBuilder_AnimationDefinition;
            this.AnimationModel = definition.AnimationModel;
            this.AnimationModelFPS = definition.AnimationModelFPS;
            this.ClipIndex = definition.ClipIndex;
            this.InfluenceArea = definition.InfluenceArea;
            this.AllowInCockpit = definition.AllowInCockpit;
            this.AllowWithWeapon = definition.AllowWithWeapon;
            if (!string.IsNullOrEmpty(definition.SupportedSkeletons))
            {
                char[] separator = new char[] { ' ' };
                this.SupportedSkeletons = definition.SupportedSkeletons.Split(separator);
            }
            this.Loop = definition.Loop;
            if (!definition.LeftHandItem.TypeId.IsNull)
            {
                this.LeftHandItem = definition.LeftHandItem;
            }
            this.AnimationSets = definition.AnimationSets;
        }

        public enum AnimationStatus
        {
            Unchecked,
            OK,
            Failed
        }
    }
}

