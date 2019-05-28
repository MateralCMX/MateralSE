namespace VRage.Compiler
{
    using Microsoft.CSharp;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.FileSystem;

    public class IlCompiler
    {
        public static CompilerParameters Options;
        public static string CompatibilityUsings = "using VRage;\r\nusing VRage.Game.Components;\r\nusing VRage.ObjectBuilders;\r\nusing VRage.ModAPI;\r\nusing VRage.Game.ModAPI;\r\nusing Sandbox.Common.ObjectBuilders;\r\nusing VRage.Game;\r\nusing Sandbox.ModAPI;\r\nusing VRage.Game.ModAPI.Interfaces;\r\nusing SpaceEngineers.Game.ModAPI;\r\n#line 1\r\n";
        private static CSharpCodeProvider m_cp = new CSharpCodeProvider();
        private static IlReader m_reader = new IlReader();
        private static Dictionary<string, string> m_compatibilityChanges;
        private static StringBuilder m_cache;
        private const string invokeWrapper = "public static class wrapclass{{ public static object run() {{ {0} return null;}} }}";
        public static StringBuilder Buffer;

        static IlCompiler()
        {
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
            dictionary1.Add("using VRage.Common.Voxels;", "");
            dictionary1.Add("VRage.Common.Voxels.", "");
            dictionary1.Add("Sandbox.ModAPI.IMyEntity", "VRage.ModAPI.IMyEntity");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.MyObjectBuilder_EntityBase", "VRage.ObjectBuilders.MyObjectBuilder_EntityBase");
            dictionary1.Add("Sandbox.Common.MyEntityUpdateEnum", "VRage.ModAPI.MyEntityUpdateEnum");
            dictionary1.Add("using Sandbox.Common.ObjectBuilders.Serializer;", "");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.Serializer.", "");
            dictionary1.Add("Sandbox.Common.MyMath", "VRageMath.MyMath");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.VRageData.SerializableVector3I", "VRage.SerializableVector3I");
            dictionary1.Add("VRage.Components", "VRage.Game.Components");
            dictionary1.Add("using Sandbox.Common.ObjectBuilders.VRageData;", "");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.MyOnlineModeEnum", "VRage.Game.MyOnlineModeEnum");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.Definitions.MyDamageType", "VRage.Game.MyDamageType");
            dictionary1.Add("Sandbox.Common.ObjectBuilders.VRageData.SerializableBlockOrientation", "VRage.Game.SerializableBlockOrientation");
            dictionary1.Add("Sandbox.Common.MySessionComponentDescriptor", "VRage.Game.Components.MySessionComponentDescriptor");
            dictionary1.Add("Sandbox.Common.MyUpdateOrder", "VRage.Game.Components.MyUpdateOrder");
            dictionary1.Add("Sandbox.Common.MySessionComponentBase", "VRage.Game.Components.MySessionComponentBase");
            dictionary1.Add("Sandbox.Common.MyFontEnum", "VRage.Game.MyFontEnum");
            dictionary1.Add("Sandbox.Common.MyRelationsBetweenPlayerAndBlock", "VRage.Game.MyRelationsBetweenPlayerAndBlock");
            dictionary1.Add("Sandbox.Common.Components", "VRage.Game.Components");
            dictionary1.Add("using Sandbox.Common.Input;", "");
            dictionary1.Add("using Sandbox.Common.ModAPI;", "");
            m_compatibilityChanges = dictionary1;
            m_cache = new StringBuilder();
            Buffer = new StringBuilder();
            string[] assemblyNames = new string[13];
            assemblyNames[0] = "System.Xml.dll";
            assemblyNames[1] = "Sandbox.Game.dll";
            assemblyNames[2] = "Sandbox.Common.dll";
            assemblyNames[3] = "Sandbox.Graphics.dll";
            assemblyNames[4] = "VRage.dll";
            assemblyNames[5] = "VRage.Library.dll";
            assemblyNames[6] = "VRage.Math.dll";
            assemblyNames[7] = "VRage.Game.dll";
            assemblyNames[8] = "VRage.Render.dll";
            assemblyNames[9] = "System.Core.dll";
            assemblyNames[10] = "System.dll";
            assemblyNames[11] = "SpaceEngineers.ObjectBuilders.dll";
            assemblyNames[12] = "SpaceEngineers.Game.dll";
            Options = new CompilerParameters(assemblyNames);
            Options.GenerateInMemory = true;
        }

        private static bool CheckResultInternal(out Assembly assembly, List<string> errors, CompilerResults result, bool isIngameScript)
        {
            assembly = null;
            if (result.Errors.HasErrors)
            {
                IEnumerator enumerator = result.Errors.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if ((enumerator.Current as CompilerError).IsWarning)
                    {
                        continue;
                    }
                    errors.Add((enumerator.Current as CompilerError).ToString());
                }
                return false;
            }
            Assembly compiledAssembly = result.CompiledAssembly;
            Dictionary<Type, HashSet<MemberInfo>> allowedTypes = new Dictionary<Type, HashSet<MemberInfo>>();
            Type[] types = compiledAssembly.GetTypes();
            int index = 0;
            while (true)
            {
                if (index < types.Length)
                {
                    Type key = types[index];
                    allowedTypes.Add(key, null);
                    index++;
                    continue;
                }
                List<MethodBase> list = new List<MethodBase>();
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                Type[] typeArray2 = compiledAssembly.GetTypes();
                int num2 = 0;
                while (true)
                {
                    if (num2 >= typeArray2.Length)
                    {
                        assembly = compiledAssembly;
                        return true;
                    }
                    Type classType = typeArray2[num2];
                    list.Clear();
                    list.AddArray<MethodBase>(classType.GetMethods(bindingAttr));
                    MethodBase[] constructors = classType.GetConstructors(bindingAttr);
                    list.AddArray<MethodBase>(constructors);
                    using (List<MethodBase>.Enumerator enumerator2 = list.GetEnumerator())
                    {
                        while (true)
                        {
                            bool flag;
                            if (!enumerator2.MoveNext())
                            {
                                break;
                            }
                            MethodBase current = enumerator2.Current;
                            if (IlChecker.IsMethodFromParent(classType, current))
                            {
                                if (IlChecker.CheckTypeAndMember(current.DeclaringType, isIngameScript, null))
                                {
                                    continue;
                                }
                                errors.Add($"Class {classType.Name} derives from class {current.DeclaringType.Name} that is not allowed in script");
                                flag = false;
                            }
                            else
                            {
                                Type type;
                                if (IlChecker.CheckIl(m_reader.ReadInstructions(current), out type, isIngameScript, allowedTypes) && !IlChecker.HasMethodInvalidAtrributes(current.Attributes))
                                {
                                    continue;
                                }
                                errors.Add($"Type {(type == null) ? "FIXME" : type.ToString()} used in {current.Name} not allowed in script");
                                flag = false;
                            }
                            return flag;
                        }
                    }
                    num2++;
                }
            }
        }

        public static bool Compile(string[] instructions, out Assembly assembly, bool isIngameScript, bool wrap = true)
        {
            assembly = null;
            m_cache.Clear();
            if (wrap)
            {
                m_cache.AppendFormat("public static class wrapclass{{ public static object run() {{ {0} return null;}} }}", (object[]) instructions);
            }
            else
            {
                m_cache.Append(instructions[0]);
            }
            Options.TempFiles = new TempFileCollection(null, false);
            string[] sources = new string[] { m_cache.ToString() };
            CompilerResults results = m_cp.CompileAssemblyFromSource(Options, sources);
            if (results.Errors.HasErrors)
            {
                return false;
            }
            assembly = results.CompiledAssembly;
            Dictionary<Type, HashSet<MemberInfo>> allowedTypes = new Dictionary<Type, HashSet<MemberInfo>>();
            foreach (Type type2 in assembly.GetTypes())
            {
                allowedTypes.Add(type2, null);
            }
            Type[] types = assembly.GetTypes();
            int index = 0;
            while (index < types.Length)
            {
                Type type3 = types[index];
                MethodInfo[] methods = type3.GetMethods();
                int num3 = 0;
                while (true)
                {
                    Type type;
                    if (num3 >= methods.Length)
                    {
                        index++;
                        break;
                    }
                    MethodInfo method = methods[num3];
                    if ((type3 != typeof(MulticastDelegate)) && !IlChecker.CheckIl(m_reader.ReadInstructions(method), out type, isIngameScript, allowedTypes))
                    {
                        assembly = null;
                        return false;
                    }
                    num3++;
                }
            }
            return true;
        }

        public static bool Compile(string assemblyName, string[] fileContents, out Assembly assembly, List<string> errors, bool isIngameScript)
        {
            Options.OutputAssembly = assemblyName;
            Options.TempFiles = new TempFileCollection(null, false);
            CompilerResults result = m_cp.CompileAssemblyFromSource(Options, fileContents);
            return CheckResultInternal(out assembly, errors, result, isIngameScript);
        }

        public static bool CompileFileModAPI(string assemblyName, string[] files, out Assembly assembly, List<string> errors)
        {
            Options.OutputAssembly = assemblyName;
            Options.GenerateInMemory = true;
            Options.TempFiles = new TempFileCollection(null, false);
            Options.IncludeDebugInformation = false;
            string[] sources = UpdateCompatibility(files);
            CompilerResults result = m_cp.CompileAssemblyFromSource(Options, sources);
            return CheckResultInternal(out assembly, errors, result, false);
        }

        public static bool CompileStringIngame(string assemblyName, string[] source, out Assembly assembly, List<string> errors)
        {
            Options.OutputAssembly = assemblyName;
            Options.GenerateInMemory = true;
            Options.GenerateExecutable = false;
            Options.IncludeDebugInformation = false;
            Options.TempFiles = new TempFileCollection(null, false);
            CompilerResults result = m_cp.CompileAssemblyFromSource(Options, source);
            return CheckResultInternal(out assembly, errors, result, true);
        }

        public static string[] UpdateCompatibility(string[] files)
        {
            string[] strArray = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string filename = files[i];
                strArray[i] = UpdateCompatibility(filename);
            }
            return strArray;
        }

        public static string UpdateCompatibility(string filename)
        {
            using (Stream stream = MyFileSystem.OpenRead(filename))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string str = reader.ReadToEnd().Insert(0, CompatibilityUsings);
                        foreach (KeyValuePair<string, string> pair in m_compatibilityChanges)
                        {
                            str = str.Replace(pair.Key, pair.Value);
                        }
                        return str;
                    }
                }
            }
            return null;
        }
    }
}

