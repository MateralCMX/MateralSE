namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ComponentSubstitutionDefinition), (Type) null)]
    public class MyComponentSubstitutionDefinition : MyDefinitionBase
    {
        public MyDefinitionId RequiredComponent;
        public Dictionary<MyDefinitionId, int> ProvidingComponents = new Dictionary<MyDefinitionId, int>(10);

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            MyObjectBuilder_ComponentSubstitutionDefinition definition = builder as MyObjectBuilder_ComponentSubstitutionDefinition;
            this.RequiredComponent = definition.RequiredComponentId;
            if (definition.ProvidingComponents != null)
            {
                foreach (MyObjectBuilder_ComponentSubstitutionDefinition.ProvidingComponent component in definition.ProvidingComponents)
                {
                    this.ProvidingComponents[component.Id] = component.Amount;
                }
            }
        }

        public bool IsProvidedByComponent(MyDefinitionId componentId, int accessibleAmount, out int providedCount)
        {
            int num = 0;
            providedCount = 0;
            if (this.RequiredComponent == componentId)
            {
                providedCount = accessibleAmount;
                return true;
            }
            if (!this.ProvidingComponents.TryGetValue(componentId, out num) || (num > accessibleAmount))
            {
                return false;
            }
            providedCount = accessibleAmount / num;
            return true;
        }

        public bool IsProvidedByComponents(Dictionary<MyDefinitionId, MyFixedPoint> m_componentCounts, out int providedCount)
        {
            int num = 0;
            foreach (KeyValuePair<MyDefinitionId, MyFixedPoint> pair in m_componentCounts)
            {
                int num2;
                if (this.IsProvidedByComponent(pair.Key, (int) pair.Value, out num2))
                {
                    num += num2;
                }
            }
            providedCount = num;
            return (providedCount >= 0);
        }
    }
}

