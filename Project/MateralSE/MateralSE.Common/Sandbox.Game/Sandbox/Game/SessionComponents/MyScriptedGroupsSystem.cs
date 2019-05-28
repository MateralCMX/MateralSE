namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Game.Components;
    using VRage.Game.Systems;
    using VRage.Network;
    using VRage.Plugins;
    using VRage.Utils;

    [StaticEventOwner, MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, Priority=300)]
    public class MyScriptedGroupsSystem : MySessionComponentBase
    {
        private static MyScriptedGroupsSystem Static;
        private Queue<MyStringHash> m_scriptsQueue;
        private Dictionary<MyStringHash, MyGroupScriptBase> m_groupScripts;
        private Dictionary<MyStringHash, MyScriptedGroupDefinition> m_definitions;

        public override void LoadData()
        {
            base.LoadData();
            this.m_scriptsQueue = new Queue<MyStringHash>();
            this.m_groupScripts = new Dictionary<MyStringHash, MyGroupScriptBase>(MyStringHash.Comparer);
            this.m_definitions = new Dictionary<MyStringHash, MyScriptedGroupDefinition>(MyStringHash.Comparer);
            this.LoadScripts(MyPlugins.GameAssembly);
            this.LoadScripts(MyPlugins.SandboxGameAssembly);
            foreach (MyScriptedGroupDefinition definition in MyDefinitionManager.Static.GetScriptedGroupDefinitions())
            {
                this.m_definitions[definition.Id.SubtypeId] = definition;
            }
            Static = this;
        }

        private void LoadScripts(Assembly assembly)
        {
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                int index = 0;
                while (index < types.Length)
                {
                    Type type = types[index];
                    object[] customAttributes = type.GetCustomAttributes(false);
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= customAttributes.Length)
                        {
                            index++;
                            break;
                        }
                        object obj2 = customAttributes[num2];
                        if (obj2 is MyScriptedSystemAttribute)
                        {
                            MyStringHash orCompute = MyStringHash.GetOrCompute((obj2 as MyScriptedSystemAttribute).ScriptName);
                            this.m_groupScripts[orCompute] = Activator.CreateInstance(type) as MyGroupScriptBase;
                        }
                        num2++;
                    }
                }
            }
        }

        private void RunScriptInternal(MyStringHash scriptName)
        {
            this.m_scriptsQueue.Enqueue(scriptName);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public override void UpdateAfterSimulation()
        {
            base.UpdateAfterSimulation();
            if (this.m_scriptsQueue.Count > 0)
            {
                while (this.m_scriptsQueue.Count > 0)
                {
                    MyStringHash key = this.m_scriptsQueue.Dequeue();
                    MyGroupScriptBase base2 = null;
                    MyScriptedGroupDefinition definition = null;
                    if (this.m_definitions.TryGetValue(key, out definition) && this.m_groupScripts.TryGetValue(definition.Script, out base2))
                    {
                        base2.ProcessObjects(definition.ListReader);
                    }
                }
            }
        }

        public override bool IsRequiredByGame =>
            ((MyPerGameSettings.Game == GameEnum.ME_GAME) && Sync.IsServer);
    }
}

