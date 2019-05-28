namespace Sandbox.Game.World
{
    using Sandbox;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Compiler;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ObjectBuilder;
    using VRage.ObjectBuilders;
    using VRage.Plugins;
    using VRage.Scripting;
    using VRage.Utils;

    public class MyScriptManager
    {
        public static MyScriptManager Static;
        private string[] Separators = new string[] { " " };
        public readonly Dictionary<MyModContext, HashSet<MyStringId>> ScriptsPerMod = new Dictionary<MyModContext, HashSet<MyStringId>>();
        public Dictionary<MyStringId, Assembly> Scripts = new Dictionary<MyStringId, Assembly>(MyStringId.Comparer);
        public Dictionary<Type, HashSet<Type>> EntityScripts = new Dictionary<Type, HashSet<Type>>();
        public Dictionary<Tuple<Type, string>, HashSet<Type>> SubEntityScripts = new Dictionary<Tuple<Type, string>, HashSet<Type>>();
        public Dictionary<string, Type> StatScripts = new Dictionary<string, Type>();
        public Dictionary<MyStringId, Type> InGameScripts = new Dictionary<MyStringId, Type>(MyStringId.Comparer);
        public Dictionary<MyStringId, StringBuilder> InGameScriptsCode = new Dictionary<MyStringId, StringBuilder>(MyStringId.Comparer);
        private List<string> m_errors = new List<string>();
        private List<MyScriptCompiler.Message> m_messages = new List<MyScriptCompiler.Message>();
        private List<string> m_cachedFiles = new List<string>();
        private static Dictionary<string, bool> testFiles = new Dictionary<string, bool>();
        private Dictionary<string, string> m_scriptsToSave = new Dictionary<string, string>();

        private void AddAssembly(MyModContext context, MyStringId myStringId, Assembly assembly)
        {
            if (this.Scripts.ContainsKey(myStringId))
            {
                MySandboxGame.Log.WriteLine($"Script already in list {myStringId.ToString()}");
            }
            else
            {
                HashSet<MyStringId> set;
                if (!this.ScriptsPerMod.TryGetValue(context, out set))
                {
                    set = new HashSet<MyStringId>();
                    this.ScriptsPerMod.Add(context, set);
                }
                set.Add(myStringId);
                this.Scripts.Add(myStringId, assembly);
                Type[] types = assembly.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    MyConsole.AddCommand(new MyCommandScript(types[i]));
                }
                this.TryAddEntityScripts(context, assembly);
                this.AddStatScripts(assembly);
            }
        }

        private void AddStatScripts(Assembly assembly)
        {
            Type type = typeof(MyStatLogic);
            foreach (Type type2 in assembly.GetTypes())
            {
                object[] customAttributes = type2.GetCustomAttributes(typeof(MyStatLogicDescriptor), false);
                if ((customAttributes != null) && (customAttributes.Length != 0))
                {
                    string componentName = ((MyStatLogicDescriptor) customAttributes[0]).ComponentName;
                    if (type.IsAssignableFrom(type2) && !this.StatScripts.ContainsKey(componentName))
                    {
                        this.StatScripts.Add(componentName, type2);
                    }
                }
            }
        }

        public void CallScript(string message)
        {
            if (!this.CallScriptInternal(message))
            {
                MyHud.Chat.ShowMessage("Call", "Failed", "Blue");
            }
        }

        private bool CallScriptInternal(string message)
        {
            Assembly assembly;
            if (IlCompiler.Buffer.Length > 0)
            {
                string[] instructions = new string[] { IlCompiler.Buffer.ToString() };
                if (!IlCompiler.Compile(instructions, out assembly, true, true))
                {
                    IlCompiler.Buffer.Clear();
                    return false;
                }
                object obj2 = assembly.GetType("wrapclass").GetMethod("run").Invoke(null, null);
                if (!string.IsNullOrEmpty(message))
                {
                    MyHud.Chat.ShowMessage("returned", obj2.ToString(), "Blue");
                }
                return true;
            }
            string[] strArray = message.Split(this.Separators, StringSplitOptions.RemoveEmptyEntries);
            if (strArray.Length < 3)
            {
                MyAPIGateway.Utilities.ShowNotification("Not enough parameters for script please provide following paramaters : Sriptname Classname MethodName", 0x1388, "White");
                return false;
            }
            if (!this.Scripts.ContainsKey(MyStringId.TryGet(strArray[1])))
            {
                string screenDescription = "";
                foreach (KeyValuePair<MyStringId, Assembly> pair in this.Scripts)
                {
                    screenDescription = screenDescription + pair.Key + "\n";
                }
                MyAPIGateway.Utilities.ShowMissionScreen("Script not found", "", "Available scripts:", screenDescription, null, null);
                return false;
            }
            assembly = this.Scripts[MyStringId.Get(strArray[1])];
            Type type = assembly.GetType(strArray[2]);
            if (type == null)
            {
                string screenDescription = "";
                foreach (Type type2 in assembly.GetTypes())
                {
                    screenDescription = screenDescription + type2.FullName + "\n";
                }
                MyAPIGateway.Utilities.ShowMissionScreen("Class not found", "", "Available classes:", screenDescription, null, null);
                return false;
            }
            MethodInfo method = type.GetMethod(strArray[3]);
            if (method == null)
            {
                string screenDescription = "";
                foreach (MethodInfo info2 in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    screenDescription = screenDescription + info2.Name + "\n";
                }
                MyAPIGateway.Utilities.ShowMissionScreen("Method not found", "", "Available methods:", screenDescription, null, null);
                return false;
            }
            ParameterInfo[] parameters = method.GetParameters();
            List<object> list = new List<object>();
            for (int i = 4; (i < (parameters.Length + 4)) && (i < strArray.Length); i++)
            {
                Type parameterType = parameters[i - 4].ParameterType;
                Type[] types = new Type[] { typeof(string), parameterType.MakeByRefType() };
                MethodInfo info3 = parameterType.GetMethod("TryParse", types);
                if (info3 == null)
                {
                    list.Add(strArray[i]);
                }
                else
                {
                    object obj3 = Activator.CreateInstance(parameterType);
                    object[] objArray = new object[] { strArray[i], obj3 };
                    info3.Invoke(null, objArray);
                    list.Add(objArray[1]);
                }
            }
            if (parameters.Length != list.Count)
            {
                return false;
            }
            object obj4 = method.Invoke(null, list.ToArray());
            if (obj4 != null)
            {
                MyHud.Chat.ShowMessage("return value", obj4.ToString(), "Blue");
            }
            MyHud.Chat.ShowMessage("Call", "Success", "Blue");
            return true;
        }

        private void Compile(IEnumerable<string> scriptFiles, string assemblyName, bool zipped, MyModContext context)
        {
            Assembly result = null;
            bool flag = false;
            if (!zipped)
            {
                if (MyFakes.ENABLE_ROSLYN_SCRIPTS)
                {
                    result = MyScriptCompiler.Static.Compile(MyApiTarget.Mod, assemblyName, (IEnumerable<Script>) (from file in scriptFiles select new Script(file, IlCompiler.UpdateCompatibility(file))), this.m_messages, context.ModName, false).Result;
                    flag = result != null;
                }
                else
                {
                    flag = IlCompiler.CompileFileModAPI(assemblyName, scriptFiles.ToArray<string>(), out result, this.m_errors);
                    this.m_messages.AddRange(from m in this.m_errors select new MyScriptCompiler.Message(TErrorSeverity.Error, m));
                }
                goto TR_0009;
            }
            else
            {
                string path = Path.Combine(Path.GetTempPath(), MyPerGameSettings.BasicGameInfo.GameNameSafe, Path.GetFileName(assemblyName));
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                foreach (string str2 in scriptFiles)
                {
                    try
                    {
                        string str3 = Path.Combine(path, Path.GetFileName(str2));
                        using (StreamReader reader = new StreamReader(MyFileSystem.OpenRead(str2)))
                        {
                            using (StreamWriter writer = new StreamWriter(MyFileSystem.OpenWrite(str3, FileMode.Create)))
                            {
                                writer.Write(reader.ReadToEnd());
                            }
                        }
                        this.m_cachedFiles.Add(str3);
                    }
                    catch (Exception exception)
                    {
                        MySandboxGame.Log.WriteLine(exception);
                        MyDefinitionErrors.Add(context, $"Cannot load {Path.GetFileName(str2)}", TErrorSeverity.Error, true);
                        MyDefinitionErrors.Add(context, exception.Message, TErrorSeverity.Error, true);
                    }
                }
            }
            if (MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                result = MyScriptCompiler.Static.Compile(MyApiTarget.Mod, assemblyName, (IEnumerable<Script>) (from file in this.m_cachedFiles select new Script(file, IlCompiler.UpdateCompatibility(file))), this.m_messages, context.ModName, false).Result;
                flag = result != null;
            }
            else
            {
                flag = IlCompiler.CompileFileModAPI(assemblyName, this.m_cachedFiles.ToArray(), out result, this.m_errors);
                this.m_messages.AddRange(from m in this.m_errors select new MyScriptCompiler.Message(TErrorSeverity.Error, m));
            }
        TR_0009:
            if ((result != null) & flag)
            {
                this.AddAssembly(context, MyStringId.GetOrCompute(assemblyName), result);
            }
            else
            {
                MyDefinitionErrors.Add(context, $"Compilation of {assemblyName} failed:", TErrorSeverity.Error, true);
                MySandboxGame.Log.IncreaseIndent();
                foreach (MyScriptCompiler.Message message in this.m_messages)
                {
                    MyDefinitionErrors.Add(context, message.Text, message.Severity, true);
                }
                MySandboxGame.Log.DecreaseIndent();
                this.m_errors.Clear();
            }
            this.m_cachedFiles.Clear();
        }

        public bool CompileIngameScript(MyStringId id, StringBuilder errors)
        {
            if (MyFakes.ENABLE_SCRIPTS)
            {
                Assembly assembly;
                string[] instructions = new string[] { this.InGameScriptsCode[id].ToString() };
                if (IlCompiler.Compile(instructions, out assembly, false, true))
                {
                    Type type = typeof(MyIngameScript);
                    if (this.InGameScripts.ContainsKey(id))
                    {
                        this.InGameScripts.Remove(id);
                    }
                    foreach (Type type2 in assembly.GetTypes())
                    {
                        if (type.IsAssignableFrom(type2))
                        {
                            this.InGameScripts.Add(id, type2);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public MyObjectBuilder_ScriptManager GetObjectBuilder()
        {
            MyObjectBuilder_ScriptManager manager1 = new MyObjectBuilder_ScriptManager();
            manager1.variables.Dictionary = MyAPIUtilities.Static.Variables;
            return manager1;
        }

        public Type GetScriptType(MyModContext context, string qualifiedTypeName)
        {
            HashSet<MyStringId> set;
            if (this.ScriptsPerMod.TryGetValue(context, out set))
            {
                using (HashSet<MyStringId>.Enumerator enumerator = set.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyStringId current = enumerator.Current;
                        Type type = this.Scripts[current].GetType(qualifiedTypeName);
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
            }
            return null;
        }

        public void Init(MyObjectBuilder_ScriptManager scriptBuilder)
        {
            if (scriptBuilder != null)
            {
                MyAPIUtilities.Static.Variables = scriptBuilder.variables.Dictionary;
            }
            this.LoadData();
        }

        public void LoadData()
        {
            MySandboxGame.Log.WriteLine("MyScriptManager.LoadData() - START");
            MySandboxGame.Log.IncreaseIndent();
            Static = this;
            this.Scripts.Clear();
            this.EntityScripts.Clear();
            this.SubEntityScripts.Clear();
            this.TryAddEntityScripts(MyModContext.BaseGame, MyPlugins.SandboxAssembly);
            this.TryAddEntityScripts(MyModContext.BaseGame, MyPlugins.SandboxGameAssembly);
            if (MySession.Static.CurrentPath != null)
            {
                this.LoadScripts(MySession.Static.CurrentPath, MyModContext.BaseGame);
            }
            if (MySession.Static.Mods != null)
            {
                foreach (MyObjectBuilder_Checkpoint.ModItem item in MySession.Static.Mods)
                {
                    MyModContext mod = new MyModContext();
                    mod.Init(item);
                    try
                    {
                        this.LoadScripts(item.GetPath(), mod);
                    }
                    catch (Exception exception)
                    {
                        MyLog.Default.WriteLine($"Fatal error compiling {mod.ModId}: {mod.ModName}. This item is likely not a mod and should be removed from the mod list.");
                        MyLog.Default.WriteLine(exception);
                        throw;
                    }
                }
            }
            foreach (Assembly assembly in this.Scripts.Values)
            {
                if (MyFakes.ENABLE_TYPES_FROM_MODS)
                {
                    MyGlobalTypeMetadata.Static.RegisterAssembly(assembly);
                }
                MySandboxGame.Log.WriteLine($"Script loaded: {assembly.FullName}");
            }
            MySandboxGame.Log.DecreaseIndent();
            MySandboxGame.Log.WriteLine("MyScriptManager.LoadData() - END");
        }

        private void LoadScripts(string path, MyModContext mod = null)
        {
            if (MyFakes.ENABLE_SCRIPTS)
            {
                string str = Path.Combine(path, "Data", "Scripts");
                IEnumerable<string> files = MyFileSystem.GetFiles(str, "*.cs");
                try
                {
                    if (files.Count<string>() != 0)
                    {
                        bool zipped = MyZipFileProvider.IsZipFile(path);
                        List<string> scriptFiles = new List<string>();
                        char[] separator = new char[] { '\\' };
                        string[] array = files.First<string>().Split(separator);
                        char[] chArray2 = new char[] { '\\' };
                        int length = str.Split(chArray2).Length;
                        if (length >= array.Length)
                        {
                            MySandboxGame.Log.WriteLine(string.Format("\nWARNING: Mod \"{0}\" contains misplaced .cs files ({2}). Scripts are supposed to be at {1}.\n", path, str, files.First<string>()));
                        }
                        else
                        {
                            string str2 = array[length];
                            foreach (string str3 in files)
                            {
                                char[] chArray3 = new char[] { '\\' };
                                array = str3.Split(chArray3);
                                char[] chArray4 = new char[] { '.' };
                                if (array[array.Length - 1].Split(chArray4).Last<string>() == "cs")
                                {
                                    if (array[Array.IndexOf<string>(array, "Scripts") + 1] == str2)
                                    {
                                        scriptFiles.Add(str3);
                                        continue;
                                    }
                                    this.Compile(scriptFiles, $"{mod.ModId}_{str2}", zipped, mod);
                                    scriptFiles.Clear();
                                    str2 = array[length];
                                    scriptFiles.Add(str3);
                                }
                            }
                            this.Compile(scriptFiles.ToArray(), Path.Combine(MyFileSystem.ModsPath, $"{mod.ModId}_{str2}"), zipped, mod);
                            scriptFiles.Clear();
                        }
                    }
                }
                catch (Exception)
                {
                    MySandboxGame.Log.WriteLine($"Failed to load scripts from: {path}");
                }
            }
        }

        private void ReadScripts(string path)
        {
            string str = Path.Combine(path, "Data", "Scripts");
            IEnumerable<string> files = MyFileSystem.GetFiles(str, "*.cs");
            try
            {
                if (files.Count<string>() != 0)
                {
                    foreach (string str2 in files)
                    {
                        try
                        {
                            StreamReader reader = new StreamReader(MyFileSystem.OpenRead(str2));
                            try
                            {
                                this.m_scriptsToSave.Add(str2.Substring(str.Length + 1), reader.ReadToEnd());
                            }
                            finally
                            {
                                if (reader == null)
                                {
                                    continue;
                                }
                                reader.Dispose();
                            }
                        }
                        catch (Exception exception)
                        {
                            MySandboxGame.Log.WriteLine(exception);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MySandboxGame.Log.WriteLine($"Failed to load scripts from: {path}");
            }
        }

        public void SaveData()
        {
            this.WriteScripts(MySession.Static.CurrentPath);
        }

        private void TryAddEntityScripts(MyModContext context, Assembly assembly)
        {
            Type type = typeof(MyGameLogicComponent);
            Type type2 = typeof(MyObjectBuilder_Base);
            foreach (Type type3 in assembly.GetTypes())
            {
                object[] customAttributes = type3.GetCustomAttributes(typeof(MyEntityComponentDescriptor), false);
                if ((customAttributes != null) && (customAttributes.Length != 0))
                {
                    MyEntityComponentDescriptor descriptor = (MyEntityComponentDescriptor) customAttributes[0];
                    try
                    {
                        if (descriptor.EntityUpdate == null)
                        {
                            MyDefinitionErrors.Add(context, "**WARNING!**\r\nScript for " + descriptor.EntityBuilderType.Name + " is using the obsolete MyEntityComponentDescriptor overload!\r\nYou must use the 3 parameter overload to properly register script updates!\r\nThis script will most likely not work as intended!\r\n**WARNING!**", TErrorSeverity.Warning, true);
                        }
                        if ((descriptor.EntityBuilderSubTypeNames == null) || (descriptor.EntityBuilderSubTypeNames.Length == 0))
                        {
                            if (type.IsAssignableFrom(type3) && type2.IsAssignableFrom(descriptor.EntityBuilderType))
                            {
                                HashSet<Type> set2;
                                if (this.EntityScripts.TryGetValue(descriptor.EntityBuilderType, out set2))
                                {
                                    MyDefinitionErrors.Add(context, "Possible entity type script logic collision", TErrorSeverity.Notice, true);
                                }
                                else
                                {
                                    set2 = new HashSet<Type>();
                                    this.EntityScripts.Add(descriptor.EntityBuilderType, set2);
                                }
                                set2.Add(type3);
                            }
                        }
                        else
                        {
                            foreach (string str in descriptor.EntityBuilderSubTypeNames)
                            {
                                if (type.IsAssignableFrom(type3) && type2.IsAssignableFrom(descriptor.EntityBuilderType))
                                {
                                    HashSet<Type> set;
                                    Tuple<Type, string> key = new Tuple<Type, string>(descriptor.EntityBuilderType, str);
                                    if (this.SubEntityScripts.TryGetValue(key, out set))
                                    {
                                        MyDefinitionErrors.Add(context, "Possible entity type script logic collision", TErrorSeverity.Notice, true);
                                    }
                                    else
                                    {
                                        set = new HashSet<Type>();
                                        this.SubEntityScripts.Add(key, set);
                                    }
                                    set.Add(type3);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MySandboxGame.Log.WriteLine("Exception during loading of type : " + type3.Name);
                    }
                }
            }
        }

        protected void UnloadData()
        {
            this.Scripts.Clear();
            this.InGameScripts.Clear();
            this.InGameScriptsCode.Clear();
            this.EntityScripts.Clear();
            this.m_scriptsToSave.Clear();
        }

        private void WriteScripts(string path)
        {
            try
            {
                string str = Path.Combine(path, "Data", "Scripts");
                foreach (KeyValuePair<string, string> pair in this.m_scriptsToSave)
                {
                    StreamWriter writer = new StreamWriter(MyFileSystem.OpenWrite($"{str}\{pair.Key}", FileMode.Create));
                    try
                    {
                        writer.Write(pair.Value);
                    }
                    finally
                    {
                        if (writer == null)
                        {
                            continue;
                        }
                        writer.Dispose();
                    }
                }
            }
            catch (Exception exception)
            {
                MySandboxGame.Log.WriteLine(exception);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScriptManager.<>c <>9 = new MyScriptManager.<>c();
            public static Func<string, Script> <>9__15_2;
            public static Func<string, MyScriptCompiler.Message> <>9__15_3;
            public static Func<string, Script> <>9__15_0;
            public static Func<string, MyScriptCompiler.Message> <>9__15_1;

            internal Script <Compile>b__15_0(string file) => 
                new Script(file, IlCompiler.UpdateCompatibility(file));

            internal MyScriptCompiler.Message <Compile>b__15_1(string m) => 
                new MyScriptCompiler.Message(TErrorSeverity.Error, m);

            internal Script <Compile>b__15_2(string file) => 
                new Script(file, IlCompiler.UpdateCompatibility(file));

            internal MyScriptCompiler.Message <Compile>b__15_3(string m) => 
                new MyScriptCompiler.Message(TErrorSeverity.Error, m);
        }
    }
}

