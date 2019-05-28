namespace Sandbox.Game.Entities.Character.Components
{
    using Sandbox.Game.Components;
    using System;
    using System.Collections.Generic;
    using VRage.Utils;

    [Obsolete("Use MyComponentDefinitionBase and MyContainerDefinition to define enabled types of components on entities")]
    public static class MyCharacterComponentTypes
    {
        [Obsolete("Use MyComponentDefinitionBase and MyContainerDefinition to define enabled types of components on entities")]
        private static Dictionary<MyStringId, Tuple<Type, Type>> m_types;

        [Obsolete("Use MyComponentDefinitionBase and MyContainerDefinition to define enabled types of components on entities")]
        public static Dictionary<MyStringId, Tuple<Type, Type>> CharacterComponents
        {
            get
            {
                if (m_types == null)
                {
                    Dictionary<MyStringId, Tuple<Type, Type>> dictionary1 = new Dictionary<MyStringId, Tuple<Type, Type>>();
                    dictionary1.Add(MyStringId.GetOrCompute("RagdollComponent"), new Tuple<Type, Type>(typeof(MyCharacterRagdollComponent), typeof(MyCharacterRagdollComponent)));
                    dictionary1.Add(MyStringId.GetOrCompute("InventorySpawnComponent"), new Tuple<Type, Type>(typeof(MyInventorySpawnComponent), typeof(MyInventorySpawnComponent)));
                    dictionary1.Add(MyStringId.GetOrCompute("CraftingComponent"), new Tuple<Type, Type>(typeof(MyCraftingComponentBasic), typeof(MyCraftingComponentBase)));
                    m_types = dictionary1;
                }
                return m_types;
            }
        }
    }
}

