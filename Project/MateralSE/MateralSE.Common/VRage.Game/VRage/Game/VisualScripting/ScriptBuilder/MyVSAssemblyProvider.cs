namespace VRage.Game.VisualScripting.ScriptBuilder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VRage.Game.VisualScripting;
    using VRage.Plugins;

    public static class MyVSAssemblyProvider
    {
        private static MyVSPreprocessor m_defaultPreprocessor = new MyVSPreprocessor();
        private static MyVSCompiler m_defaultCompiler;
        private static bool m_firstRun = true;

        public static List<IMyLevelScript> GetLevelScriptInstances()
        {
            if ((m_defaultCompiler == null) || (m_defaultCompiler.Assembly == null))
            {
                return null;
            }
            return m_defaultCompiler.GetLevelScriptInstances();
        }

        public static Type GetType(string typeName) => 
            m_defaultCompiler.Assembly?.GetType(typeName);

        public static void Init(IEnumerable<string> fileNames, string localModPath)
        {
            if (m_firstRun)
            {
                MyVSCompiler.DependencyCollector.CollectReferences(MyPlugins.GameAssembly);
                m_firstRun = false;
            }
            m_defaultPreprocessor.Clear();
            foreach (string str in fileNames)
            {
                m_defaultPreprocessor.AddFile(str, localModPath);
            }
            List<string> sourceFiles = new List<string>();
            string[] fileSet = m_defaultPreprocessor.FileSet;
            MyVisualScriptBuilder builder = new MyVisualScriptBuilder();
            foreach (string str2 in fileSet)
            {
                builder.ScriptFilePath = str2;
                if (builder.Load() && builder.Build())
                {
                    sourceFiles.Add(Path.Combine(Path.GetTempPath(), builder.ScriptName + ".cs"));
                    File.WriteAllText(sourceFiles[sourceFiles.Count - 1], builder.Syntax);
                }
            }
            m_defaultCompiler = new MyVSCompiler("MyVSDefaultAssembly", sourceFiles);
            if ((fileSet.Length != 0) && m_defaultCompiler.Compile())
            {
                m_defaultCompiler.LoadAssembly();
            }
            using (List<string>.Enumerator enumerator2 = sourceFiles.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    File.Delete(enumerator2.Current);
                }
            }
        }
    }
}

