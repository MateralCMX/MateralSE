namespace VRage.Game.Entity.UseObject
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using VRage.FileSystem;
    using VRage.ModAPI;
    using VRage.Plugins;
    using VRageRender.Import;

    [PreloadRequired]
    public static class MyUseObjectFactory
    {
        private static Dictionary<string, Type> m_useObjectTypesByDummyName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        static MyUseObjectFactory()
        {
            RegisterAssemblyTypes(Assembly.GetExecutingAssembly());
            RegisterAssemblyTypes(MyPlugins.GameAssembly);
            RegisterAssemblyTypes(MyPlugins.SandboxAssembly);
            RegisterAssemblyTypes(MyPlugins.UserAssemblies);
            RegisterAssemblyTypes(Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, "Sandbox.Game.dll")));
        }

        [Conditional("DEBUG")]
        private static void AssertHasCorrectCtor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
            for (int i = 0; i < constructors.Length; i++)
            {
                ParameterInfo[] parameters = constructors[i].GetParameters();
                if (((parameters.Length == 4) && ((parameters[0].ParameterType == typeof(IMyEntity)) && ((parameters[1].ParameterType == typeof(string)) && (parameters[2].ParameterType == typeof(MyModelDummy))))) && (parameters[3].ParameterType == typeof(uint)))
                {
                    return;
                }
            }
        }

        public static IMyUseObject CreateUseObject(string detectorName, IMyEntity owner, string dummyName, MyModelDummy dummyData, uint shapeKey)
        {
            Type type;
            if (!m_useObjectTypesByDummyName.TryGetValue(detectorName, out type) || (type == null))
            {
                return null;
            }
            object[] args = new object[] { owner, dummyName, dummyData, shapeKey };
            return (IMyUseObject) Activator.CreateInstance(type, args);
        }

        private static void RegisterAssemblyTypes(Assembly[] assemblies)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    RegisterAssemblyTypes(assemblyArray[i]);
                }
            }
        }

        private static void RegisterAssemblyTypes(Assembly assembly)
        {
            if (assembly != null)
            {
                Type type = typeof(IMyUseObject);
                foreach (Type type2 in assembly.GetTypes())
                {
                    if (type.IsAssignableFrom(type2))
                    {
                        MyUseObjectAttribute[] customAttributes = (MyUseObjectAttribute[]) type2.GetCustomAttributes(typeof(MyUseObjectAttribute), false);
                        if (!customAttributes.IsNullOrEmpty<MyUseObjectAttribute>())
                        {
                            foreach (MyUseObjectAttribute attribute in customAttributes)
                            {
                                m_useObjectTypesByDummyName[attribute.DummyName] = type2;
                            }
                        }
                    }
                }
            }
        }
    }
}

