namespace VRage.Plugins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using VRage.Collections;
    using VRage.FileSystem;

    public class MyPlugins : IDisposable
    {
        private static List<IPlugin> m_plugins = new List<IPlugin>();
        private static Assembly m_gamePluginAssembly;
        private static List<Assembly> m_userPluginAssemblies;
        private static Assembly m_sandboxAssembly;
        private static Assembly m_sandboxGameAssembly;
        private static Assembly m_gameObjBuildersPlugin;
        private static Assembly m_gameBaseObjBuildersPlugin;
        private static MyPlugins m_instance;

        private MyPlugins()
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~MyPlugins()
        {
        }

        public static void Load()
        {
            if (m_gamePluginAssembly != null)
            {
                List<Assembly> assemblies = new List<Assembly>();
                assemblies.Add(m_gamePluginAssembly);
                LoadPlugins(assemblies);
            }
            if (m_userPluginAssemblies != null)
            {
                LoadPlugins(m_userPluginAssemblies);
            }
            m_instance = new MyPlugins();
        }

        private static void LoadPlugins(List<Assembly> assemblies)
        {
            using (List<Assembly>.Enumerator enumerator = assemblies.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IEnumerator<Type> enumerator2 = (from s in enumerator.Current.GetTypes()
                        where s.GetInterfaces().Contains<Type>(typeof(IPlugin)) && !s.IsAbstract
                        select s).GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            Type current = enumerator2.Current;
                            try
                            {
                                m_plugins.Add((IPlugin) Activator.CreateInstance(current));
                            }
                            catch (Exception exception)
                            {
                                Trace.Fail("Cannot create instance of '" + current.FullName + "': " + exception.ToString());
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator2 == null)
                        {
                            continue;
                        }
                        enumerator2.Dispose();
                    }
                }
            }
        }

        public static void RegisterBaseGameObjectBuildersAssemblyFile(string gameBaseObjBuildersAssemblyFile)
        {
            if (gameBaseObjBuildersAssemblyFile != null)
            {
                m_gameBaseObjBuildersPlugin = Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, gameBaseObjBuildersAssemblyFile));
            }
        }

        public static void RegisterFromArgs(string[] args)
        {
            m_userPluginAssemblies = null;
            if (args != null)
            {
                List<string> list = new List<string>();
                if (args.Contains<string>("-plugin"))
                {
                    for (int i = args.ToList<string>().IndexOf("-plugin"); ((i + 1) < args.Length) && !args[i + 1].StartsWith("-"); i++)
                    {
                        list.Add(args[i + 1]);
                    }
                }
                if (list.Count > 0)
                {
                    m_userPluginAssemblies = new List<Assembly>(list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        m_userPluginAssemblies.Add(Assembly.LoadFrom(list[i]));
                    }
                }
            }
        }

        public static void RegisterGameAssemblyFile(string gameAssemblyFile)
        {
            if (gameAssemblyFile != null)
            {
                m_gamePluginAssembly = Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, gameAssemblyFile));
            }
        }

        public static void RegisterGameObjectBuildersAssemblyFile(string gameObjBuildersAssemblyFile)
        {
            if (gameObjBuildersAssemblyFile != null)
            {
                m_gameObjBuildersPlugin = Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, gameObjBuildersAssemblyFile));
            }
        }

        public static void RegisterSandboxAssemblyFile(string sandboxAssemblyFile)
        {
            if (sandboxAssemblyFile != null)
            {
                m_sandboxAssembly = Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, sandboxAssemblyFile));
            }
        }

        public static void RegisterSandboxGameAssemblyFile(string sandboxAssemblyFile)
        {
            if (sandboxAssemblyFile != null)
            {
                m_sandboxGameAssembly = Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, sandboxAssemblyFile));
            }
        }

        public static void RegisterUserAssemblyFiles(List<string> userAssemblyFiles)
        {
            if (userAssemblyFiles != null)
            {
                if (m_userPluginAssemblies == null)
                {
                    m_userPluginAssemblies = new List<Assembly>(userAssemblyFiles.Count);
                }
                foreach (string str in userAssemblyFiles)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        m_userPluginAssemblies.Add(Assembly.LoadFrom(str));
                    }
                }
            }
        }

        public static void Unload()
        {
            using (List<IPlugin>.Enumerator enumerator = m_plugins.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Dispose();
                }
            }
            m_plugins.Clear();
            m_instance.Dispose();
            m_instance = null;
            m_gamePluginAssembly = null;
            m_userPluginAssemblies = null;
            m_sandboxAssembly = null;
            m_sandboxGameAssembly = null;
            m_gameObjBuildersPlugin = null;
            m_gameBaseObjBuildersPlugin = null;
        }

        public static bool Loaded =>
            (m_instance != null);

        public static ListReader<IPlugin> Plugins =>
            m_plugins;

        public static Assembly GameAssembly =>
            (GameAssemblyReady ? m_gamePluginAssembly : null);

        public static Assembly GameObjectBuildersAssembly =>
            (GameObjectBuildersAssemblyReady ? m_gameObjBuildersPlugin : null);

        public static Assembly GameBaseObjectBuildersAssembly =>
            (GameBaseObjectBuildersAssemblyReady ? m_gameBaseObjBuildersPlugin : null);

        public static Assembly[] UserAssemblies =>
            (UserAssembliesReady ? m_userPluginAssemblies.ToArray() : null);

        public static Assembly SandboxAssembly =>
            (SandboxAssemblyReady ? m_sandboxAssembly : null);

        public static Assembly SandboxGameAssembly =>
            ((m_sandboxGameAssembly != null) ? m_sandboxGameAssembly : null);

        public static bool GameAssemblyReady =>
            (m_gamePluginAssembly != null);

        public static bool GameObjectBuildersAssemblyReady =>
            (m_gameObjBuildersPlugin != null);

        public static bool GameBaseObjectBuildersAssemblyReady =>
            (m_gameBaseObjBuildersPlugin != null);

        public static bool UserAssembliesReady =>
            (m_userPluginAssemblies != null);

        public static bool SandboxAssemblyReady =>
            (m_sandboxAssembly != null);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPlugins.<>c <>9 = new MyPlugins.<>c();
            public static Func<Type, bool> <>9__42_0;

            internal bool <LoadPlugins>b__42_0(Type s) => 
                (s.GetInterfaces().Contains<Type>(typeof(IPlugin)) && !s.IsAbstract);
        }
    }
}

