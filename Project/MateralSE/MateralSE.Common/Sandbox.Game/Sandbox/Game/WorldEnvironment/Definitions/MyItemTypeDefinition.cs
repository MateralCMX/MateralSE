namespace Sandbox.Game.WorldEnvironment.Definitions
{
    using Sandbox.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.ObjectBuilders;
    using VRage.Utils;

    public class MyItemTypeDefinition
    {
        public string Name;
        public int LodFrom;
        public int LodTo;
        public Module StorageModule;
        public Module[] ProxyModules;

        public MyItemTypeDefinition(MyEnvironmentItemTypeDefinition def)
        {
            this.Name = def.Name;
            this.LodFrom = (def.LodFrom == -1) ? 15 : def.LodFrom;
            this.LodTo = def.LodTo;
            if (def.Provider != null)
            {
                MyProceduralEnvironmentModuleDefinition definition = MyDefinitionManager.Static.GetDefinition<MyProceduralEnvironmentModuleDefinition>(def.Provider.Value);
                if (definition != null)
                {
                    this.StorageModule.Type = definition.ModuleType;
                    this.StorageModule.Definition = def.Provider.Value;
                }
                else
                {
                    object[] args = new object[] { def.Provider.Value };
                    MyLog.Default.Error("Could not find module definition for type {0}.", args);
                }
            }
            if (def.Proxies != null)
            {
                List<Module> list = new List<Module>();
                SerializableDefinitionId[] proxies = def.Proxies;
                int index = 0;
                while (true)
                {
                    if (index >= proxies.Length)
                    {
                        list.Capacity = list.Count;
                        this.ProxyModules = list.GetInternalArray<Module>();
                        break;
                    }
                    SerializableDefinitionId subtypeId = proxies[index];
                    MyEnvironmentModuleProxyDefinition definition = MyDefinitionManager.Static.GetDefinition<MyEnvironmentModuleProxyDefinition>(subtypeId);
                    if (definition == null)
                    {
                        object[] args = new object[] { subtypeId };
                        MyLog.Default.Error("Could not find proxy module definition for type {0}.", args);
                    }
                    else
                    {
                        Module item = new Module {
                            Type = definition.ModuleType,
                            Definition = subtypeId
                        };
                        list.Add(item);
                    }
                    index++;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Module
        {
            public System.Type Type;
            public MyDefinitionId Definition;
        }
    }
}

