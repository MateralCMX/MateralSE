namespace VRage.Game
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_DefinitionBase), (Type) null)]
    public class MyDefinitionBase
    {
        public MyDefinitionId Id;
        public MyStringId? DisplayNameEnum;
        public MyStringId? DescriptionEnum;
        public string DisplayNameString;
        public string DescriptionString;
        public string DescriptionArgs;
        public string[] Icons;
        public bool Enabled = true;
        public bool Public = true;
        public bool AvailableInSurvival;
        public MyModContext Context;

        public virtual MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            string text1;
            string text2;
            MyObjectBuilder_DefinitionBase local1 = MyDefinitionManagerBase.GetObjectFactory().CreateObjectBuilder<MyObjectBuilder_DefinitionBase>(this);
            local1.Id = (SerializableDefinitionId) this.Id;
            MyObjectBuilder_DefinitionBase local3 = local1;
            if (this.DescriptionEnum == null)
            {
                text1 = this.DescriptionString?.ToString();
            }
            else
            {
                text1 = this.DescriptionEnum.Value.ToString();
            }
            local3.Description = text1;
            if (this.DisplayNameEnum == null)
            {
                text2 = this.DisplayNameString?.ToString();
            }
            else
            {
                text2 = this.DisplayNameEnum.Value.ToString();
            }
            local3.DisplayName = text2;
            MyObjectBuilder_DefinitionBase local2 = local3;
            local2.Icons = this.Icons;
            local2.Public = this.Public;
            local2.Enabled = this.Enabled;
            local2.DescriptionArgs = this.DescriptionArgs;
            local2.AvailableInSurvival = this.AvailableInSurvival;
            return local2;
        }

        protected virtual void Init(MyObjectBuilder_DefinitionBase builder)
        {
            this.Id = builder.Id;
            this.Public = builder.Public;
            this.Enabled = builder.Enabled;
            this.AvailableInSurvival = builder.AvailableInSurvival;
            this.Icons = builder.Icons;
            this.DescriptionArgs = builder.DescriptionArgs;
            this.DLCs = builder.DLCs;
            if ((builder.DisplayName == null) || !builder.DisplayName.StartsWith("DisplayName_"))
            {
                this.DisplayNameString = builder.DisplayName;
            }
            else
            {
                this.DisplayNameEnum = new MyStringId?(MyStringId.GetOrCompute(builder.DisplayName));
            }
            if ((builder.Description == null) || !builder.Description.StartsWith("Description_"))
            {
                this.DescriptionString = builder.Description;
            }
            else
            {
                this.DescriptionEnum = new MyStringId?(MyStringId.GetOrCompute(builder.Description));
            }
        }

        public void Init(MyObjectBuilder_DefinitionBase builder, MyModContext modContext)
        {
            this.Context = modContext;
            this.Init(builder);
        }

        [Obsolete("Prefer to use MyDefinitionPostprocessor instead.")]
        public virtual void Postprocess()
        {
        }

        public void Save(string filepath)
        {
            this.GetObjectBuilder().Save(filepath);
        }

        public override string ToString() => 
            this.Id.ToString();

        public string[] DLCs { get; private set; }

        public virtual string DisplayNameText =>
            ((this.DisplayNameEnum != null) ? MyTexts.GetString(this.DisplayNameEnum.Value) : this.DisplayNameString);

        public virtual string DescriptionText =>
            ((this.DescriptionEnum != null) ? MyTexts.GetString(this.DescriptionEnum.Value) : this.DescriptionString);
    }
}

