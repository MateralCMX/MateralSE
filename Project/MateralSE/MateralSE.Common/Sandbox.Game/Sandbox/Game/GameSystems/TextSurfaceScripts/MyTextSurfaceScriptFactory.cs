namespace Sandbox.Game.GameSystems.TextSurfaceScripts
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Plugins;
    using VRage.Utils;
    using VRageMath;

    public class MyTextSurfaceScriptFactory
    {
        private Dictionary<string, ScriptInfo> m_scripts = new Dictionary<string, ScriptInfo>();

        public MyTextSurfaceScriptFactory()
        {
            this.m_scripts.Clear();
            this.RegisterFromAssembly(Assembly.GetExecutingAssembly());
            foreach (IPlugin plugin in MyPlugins.Plugins)
            {
                this.RegisterFromAssembly(plugin.GetType().Assembly);
            }
            this.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public IMyTextSurfaceScript CreateScript(string id, IMyTextSurface surface, IMyCubeBlock block, Vector2 size)
        {
            ScriptInfo info;
            if (!this.Scripts.TryGetValue(id, out info))
            {
                return null;
            }
            object[] args = new object[] { surface, block, size };
            return (IMyTextSurfaceScript) Activator.CreateInstance(info.Type, args);
        }

        public void RegisterFromAssembly(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                foreach (Assembly assembly in assemblies)
                {
                    this.RegisterFromAssembly(assembly);
                }
            }
        }

        public void RegisterFromAssembly(Assembly assembly)
        {
            foreach (TypeInfo info in assembly.DefinedTypes)
            {
                if (!info.ImplementedInterfaces.Contains<Type>(typeof(IMyTextSurfaceScript)))
                {
                    continue;
                }
                if (info != typeof(MyTextSurfaceScriptBase))
                {
                    MyTextSurfaceScriptAttribute customAttribute = info.GetCustomAttribute<MyTextSurfaceScriptAttribute>();
                    if (customAttribute != null)
                    {
                        ScriptInfo info2 = new ScriptInfo {
                            Id = customAttribute.Id,
                            DisplayName = MyStringId.GetOrCompute(customAttribute.DisplayName),
                            Type = info.AsType()
                        };
                        this.m_scripts[customAttribute.Id] = info2;
                    }
                }
            }
        }

        public DictionaryReader<string, ScriptInfo> Scripts =>
            this.m_scripts;

        [StructLayout(LayoutKind.Sequential)]
        public struct ScriptInfo
        {
            public string Id;
            public MyStringId DisplayName;
            public System.Type Type;
        }
    }
}

