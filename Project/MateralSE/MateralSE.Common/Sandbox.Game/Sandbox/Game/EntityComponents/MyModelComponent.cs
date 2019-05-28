namespace Sandbox.Game.EntityComponents
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.Models;
    using VRage.Utils;

    [MyComponentType(typeof(MyModelComponent)), MyComponentBuilder(typeof(MyObjectBuilder_ModelComponent), true)]
    public class MyModelComponent : MyEntityComponentBase
    {
        public static MyStringHash ModelChanged = MyStringHash.GetOrCompute("ModelChanged");

        public override void Init(MyComponentDefinitionBase definition)
        {
            base.Init(definition);
            this.Definition = definition as MyModelComponentDefinition;
        }

        public void InitEntity()
        {
            if (this.Definition != null)
            {
                float? scale = null;
                MyEntity entity = base.Entity as MyEntity;
                entity.Init(new StringBuilder(this.Definition.DisplayNameText), this.Definition.Model, null, scale, null);
                entity.DisplayNameText = this.Definition.DisplayNameText;
            }
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            this.InitEntity();
            if (this.Definition != null)
            {
                this.RaiseEntityEvent(ModelChanged, new MyEntityContainerEventExtensions.ModelChangedParams(this.Definition.Model, this.Definition.Size, this.Definition.Mass, this.Definition.Volume, this.Definition.DisplayNameText, this.Definition.Icons));
            }
        }

        public MyModelComponentDefinition Definition { get; private set; }

        public MyModel Model =>
            ((base.Entity != null) ? (base.Entity as MyEntity).Model : null);

        public MyModel ModelCollision =>
            ((base.Entity != null) ? (base.Entity as MyEntity).ModelCollision : null);

        public override string ComponentTypeDebugString =>
            $"Model Component {((this.Definition != null) ? this.Definition.Model : "invalid")}";
    }
}

