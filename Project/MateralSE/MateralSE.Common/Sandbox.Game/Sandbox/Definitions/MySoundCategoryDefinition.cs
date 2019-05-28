namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_SoundCategoryDefinition), (Type) null)]
    public class MySoundCategoryDefinition : MyDefinitionBase
    {
        public List<SoundDescription> Sounds;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_SoundCategoryDefinition definition = builder as MyObjectBuilder_SoundCategoryDefinition;
            this.Sounds = new List<SoundDescription>();
            if (definition.Sounds != null)
            {
                foreach (MyObjectBuilder_SoundCategoryDefinition.SoundDesc desc in definition.Sounds)
                {
                    MyStringId orCompute = MyStringId.GetOrCompute(desc.SoundName);
                    if (MyTexts.Exists(orCompute))
                    {
                        this.Sounds.Add(new SoundDescription(desc.Id, desc.SoundName, new MyStringId?(orCompute)));
                    }
                    else
                    {
                        MyStringId? soundNameEnum = null;
                        this.Sounds.Add(new SoundDescription(desc.Id, desc.SoundName, soundNameEnum));
                    }
                }
            }
        }

        public class SoundDescription
        {
            public string SoundId;
            public string SoundName;
            public MyStringId? SoundNameEnum;

            public SoundDescription(string soundId, string soundName, MyStringId? soundNameEnum)
            {
                this.SoundId = soundId;
                this.SoundName = soundName;
                this.SoundNameEnum = soundNameEnum;
            }

            public string SoundText =>
                ((this.SoundNameEnum != null) ? MyTexts.GetString(this.SoundNameEnum.Value) : this.SoundName);
        }
    }
}

