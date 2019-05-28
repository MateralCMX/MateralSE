namespace Sandbox.Definitions
{
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Game.Definitions;

    [MyDefinitionType(typeof(MyObjectBuilder_ComponentGroupDefinition), (Type) null)]
    public class MyComponentGroupDefinition : MyDefinitionBase
    {
        private MyObjectBuilder_ComponentGroupDefinition m_postprocessBuilder;
        private List<MyComponentDefinition> m_components = new List<MyComponentDefinition>();

        public MyComponentDefinition GetComponentDefinition(int amount)
        {
            if ((amount <= 0) || (amount > this.m_components.Count))
            {
                return null;
            }
            return this.m_components[amount - 1];
        }

        public int GetComponentNumber() => 
            this.m_components.Count;

        protected override void Init(MyObjectBuilder_DefinitionBase builder)
        {
            base.Init(builder);
            this.m_postprocessBuilder = builder as MyObjectBuilder_ComponentGroupDefinition;
        }

        public void Postprocess()
        {
            bool flag = true;
            int amount = 0;
            foreach (MyObjectBuilder_ComponentGroupDefinition.Component component in this.m_postprocessBuilder.Components)
            {
                if (component.Amount > amount)
                {
                    amount = component.Amount;
                }
            }
            for (int i = 0; i < amount; i++)
            {
                this.m_components.Add(null);
            }
            foreach (MyObjectBuilder_ComponentGroupDefinition.Component component2 in this.m_postprocessBuilder.Components)
            {
                MyComponentDefinition definition;
                MyDefinitionId defId = new MyDefinitionId(typeof(MyObjectBuilder_Component), component2.SubtypeId);
                MyDefinitionManager.Static.TryGetDefinition<MyComponentDefinition>(defId, out definition);
                if (definition == null)
                {
                    flag = false;
                }
                this.SetComponentDefinition(component2.Amount, definition);
            }
            for (int j = 0; j < this.m_components.Count; j++)
            {
                if (this.m_components[j] == null)
                {
                    flag = false;
                }
            }
            if (!flag)
            {
                this.m_components.Clear();
            }
        }

        public void SetComponentDefinition(int amount, MyComponentDefinition definition)
        {
            if ((amount > 0) && (amount <= this.m_components.Count))
            {
                this.m_components[amount - 1] = definition;
            }
        }

        public bool IsValid =>
            (this.m_components.Count != 0);
    }
}

