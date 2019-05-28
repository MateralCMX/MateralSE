namespace VRage.Game
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_ContainerDefinition), (Type) null), XmlSerializerAssembly("VRage.Game.XmlSerializers")]
    public class MyContainerDefinition : MyDefinitionBase
    {
        public List<DefaultComponent> DefaultComponents = new List<DefaultComponent>();
        public EntityFlags? Flags;

        public override MyObjectBuilder_DefinitionBase GetObjectBuilder()
        {
            MyObjectBuilder_ContainerDefinition objectBuilder = (MyObjectBuilder_ContainerDefinition) base.GetObjectBuilder();
            objectBuilder.Flags = this.Flags;
            if ((this.DefaultComponents != null) && (this.DefaultComponents.Count > 0))
            {
                objectBuilder.DefaultComponents = new MyObjectBuilder_ContainerDefinition.DefaultComponentBuilder[this.DefaultComponents.Count];
                int index = 0;
                foreach (DefaultComponent component in this.DefaultComponents)
                {
                    if (!component.BuilderType.IsNull)
                    {
                        objectBuilder.DefaultComponents[index].BuilderType = component.BuilderType.ToString();
                    }
                    if (component.InstanceType != null)
                    {
                        objectBuilder.DefaultComponents[index].InstanceType = component.InstanceType.Name;
                    }
                    if (component.SubtypeId != null)
                    {
                        objectBuilder.DefaultComponents[index].SubtypeId = component.SubtypeId.Value.ToString();
                    }
                    objectBuilder.DefaultComponents[index].ForceCreate = component.ForceCreate;
                    index++;
                }
            }
            return objectBuilder;
        }

        public bool HasDefaultComponent(string component)
        {
            using (List<DefaultComponent>.Enumerator enumerator = this.DefaultComponents.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    DefaultComponent current = enumerator.Current;
                    if ((!current.BuilderType.IsNull && (current.BuilderType.ToString() == component)) || ((current.InstanceType != null) && (current.InstanceType.ToString() == component)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ContainerDefinition definition = builder as MyObjectBuilder_ContainerDefinition;
            this.Flags = definition.Flags;
            if ((definition.DefaultComponents != null) && (definition.DefaultComponents.Length != 0))
            {
                if (this.DefaultComponents == null)
                {
                    this.DefaultComponents = new List<DefaultComponent>();
                }
                foreach (MyObjectBuilder_ContainerDefinition.DefaultComponentBuilder builder2 in definition.DefaultComponents)
                {
                    DefaultComponent item = new DefaultComponent();
                    try
                    {
                        if (builder2.BuilderType != null)
                        {
                            item.BuilderType = MyObjectBuilderType.Parse(builder2.BuilderType);
                        }
                    }
                    catch (Exception)
                    {
                        MyLog.Default.WriteLine($"Container definition error: can not parse defined component type {builder2} for container {base.Id.ToString()}");
                    }
                    try
                    {
                        if (builder2.InstanceType != null)
                        {
                            item.InstanceType = Type.GetType(builder2.InstanceType, true);
                        }
                    }
                    catch (Exception)
                    {
                        MyLog.Default.WriteLine($"Container definition error: can not parse defined component type {builder2} for container {base.Id.ToString()}");
                    }
                    item.ForceCreate = builder2.ForceCreate;
                    if (builder2.SubtypeId != null)
                    {
                        item.SubtypeId = new MyStringHash?(MyStringHash.GetOrCompute(builder2.SubtypeId));
                    }
                    if (item.IsValid())
                    {
                        this.DefaultComponents.Add(item);
                    }
                    else
                    {
                        MyLog.Default.WriteLine($"Defined component {builder2} for container {base.Id.ToString()} is invalid, none builder type or instance type is defined! Skipping it.");
                    }
                }
            }
        }

        public class DefaultComponent
        {
            public MyObjectBuilderType BuilderType = 0;
            public Type InstanceType;
            public bool ForceCreate;
            public MyStringHash? SubtypeId;

            public bool IsValid() => 
                ((this.InstanceType != null) || !this.BuilderType.IsNull);
        }
    }
}

