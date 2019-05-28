namespace Sandbox.Definitions
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    [MyDefinitionType(typeof(MyObjectBuilder_PhysicalModelCollectionDefinition), (Type) null)]
    public class MyPhysicalModelCollectionDefinition : MyDefinitionBase
    {
        public MyDiscreteSampler<MyDefinitionId> Items;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            List<MyDefinitionId> values = new List<MyDefinitionId>();
            List<float> densities = new List<float>();
            foreach (MyPhysicalModelItem item in (builder as MyObjectBuilder_PhysicalModelCollectionDefinition).Items)
            {
                Type type = (Type) MyObjectBuilderType.ParseBackwardsCompatible(item.TypeId);
                MyDefinitionId id = new MyDefinitionId(type, item.SubtypeId);
                values.Add(id);
                densities.Add(item.Weight);
            }
            this.Items = new MyDiscreteSampler<MyDefinitionId>(values, densities);
        }
    }
}

