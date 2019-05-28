namespace Sandbox.Definitions
{
    using Sandbox.Game.Entities;
    using System;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_RopeDefinition), (Type) null)]
    public class MyRopeDefinition : MyDefinitionBase
    {
        public bool EnableRayCastRelease;
        public bool IsDefaultCreativeRope;
        public string ColorMetalTexture;
        public string NormalGlossTexture;
        public string AddMapsTexture;
        public MySoundPair AttachSound;
        public MySoundPair DetachSound;
        public MySoundPair WindingSound;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            string text1;
            string text2;
            string text3;
            MyObjectBuilder_RopeDefinition objectBuilder = (MyObjectBuilder_RopeDefinition) base.GetObjectBuilder();
            objectBuilder.EnableRayCastRelease = this.EnableRayCastRelease;
            objectBuilder.IsDefaultCreativeRope = this.IsDefaultCreativeRope;
            objectBuilder.ColorMetalTexture = this.ColorMetalTexture;
            objectBuilder.NormalGlossTexture = this.NormalGlossTexture;
            objectBuilder.AddMapsTexture = this.AddMapsTexture;
            MyObjectBuilder_RopeDefinition definition2 = objectBuilder;
            if (this.AttachSound == null)
            {
                text1 = null;
            }
            else
            {
                text1 = this.AttachSound.SoundId.ToString();
            }
            definition2.AttachSound = text1;
            if (this.DetachSound == null)
            {
                text2 = null;
            }
            else
            {
                text2 = this.DetachSound.SoundId.ToString();
            }
            definition2.DetachSound = text2;
            if (this.WindingSound == null)
            {
                text3 = null;
            }
            else
            {
                text3 = this.WindingSound.SoundId.ToString();
            }
            definition2.WindingSound = text3;
            return definition2;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            MyObjectBuilder_RopeDefinition definition = (MyObjectBuilder_RopeDefinition) builder;
            this.EnableRayCastRelease = definition.EnableRayCastRelease;
            this.IsDefaultCreativeRope = definition.IsDefaultCreativeRope;
            this.ColorMetalTexture = definition.ColorMetalTexture;
            this.NormalGlossTexture = definition.NormalGlossTexture;
            this.AddMapsTexture = definition.AddMapsTexture;
            if (!string.IsNullOrEmpty(definition.AttachSound))
            {
                this.AttachSound = new MySoundPair(definition.AttachSound, true);
            }
            if (!string.IsNullOrEmpty(definition.DetachSound))
            {
                this.DetachSound = new MySoundPair(definition.DetachSound, true);
            }
            if (!string.IsNullOrEmpty(definition.WindingSound))
            {
                this.WindingSound = new MySoundPair(definition.WindingSound, true);
            }
            base.Init(builder);
        }
    }
}

