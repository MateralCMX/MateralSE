namespace SpaceEngineers.Game
{
    using Sandbox;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Utils;
    using Sandbox.Game.EntityComponents;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using SpaceEngineers.Game.ModAPI;
    using SpaceEngineers.Game.ModAPI.Ingame;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using VRage;
    using VRage.Collections;
    using VRage.Compiler;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.GUI.TextPanel;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ModAPI.Ingame.Utilities;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Scripting;
    using VRageMath;

    public class MySpaceGameCustomInitialization : MySandboxGame.IGameCustomInitialization
    {
        private string GetPrefixedBranchName()
        {
            string branchName = MyGameService.BranchName;
            branchName = !string.IsNullOrEmpty(branchName) ? Regex.Replace(branchName, "[^a-zA-Z0-9_]", "_").ToUpper() : "STABLE";
            return ("BRANCH_" + branchName);
        }

        public void InitIlChecker()
        {
            if (!MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                IlChecker.AllowNamespaceOfTypeModAPI(typeof(SpaceEngineers.Game.ModAPI.IMyButtonPanel));
                IlChecker.AllowNamespaceOfTypeCommon(typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel));
            }
            else
            {
                using (IMyWhitelistBatch batch = MyScriptCompiler.Static.Whitelist.OpenBatch())
                {
                    Type[] types = new Type[] { typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyButtonPanel), typeof(LandingGearMode) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, types);
                    Type[] typeArray2 = new Type[] { typeof(SpaceEngineers.Game.ModAPI.IMyButtonPanel) };
                    batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray2);
                }
            }
        }

        public void InitIlCompiler()
        {
            if (!MyFakes.ENABLE_ROSLYN_SCRIPTS)
            {
                string[] assemblyNames = new string[13];
                assemblyNames[0] = "System.Xml.dll";
                assemblyNames[1] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Game.dll");
                assemblyNames[2] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Common.dll");
                assemblyNames[3] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Graphics.dll");
                assemblyNames[4] = Path.Combine(MyFileSystem.ExePath, "VRage.dll");
                assemblyNames[5] = Path.Combine(MyFileSystem.ExePath, "VRage.Library.dll");
                assemblyNames[6] = Path.Combine(MyFileSystem.ExePath, "VRage.Math.dll");
                assemblyNames[7] = Path.Combine(MyFileSystem.ExePath, "VRage.Game.dll");
                assemblyNames[8] = Path.Combine(MyFileSystem.ExePath, "VRage.Render.dll");
                assemblyNames[9] = "System.Core.dll";
                assemblyNames[10] = "System.dll";
                assemblyNames[11] = Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.ObjectBuilders.dll");
                assemblyNames[12] = Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.Game.dll");
                IlCompiler.Options = new CompilerParameters(assemblyNames);
                IlCompiler.Options.GenerateInMemory = true;
            }
            else
            {
                MyScriptCompiler.Static.IgnoredWarnings.Add("CS0105");
                MyModWatchdog.Init(MySandboxGame.Static.UpdateThread);
                string[] textArray1 = new string[14];
                textArray1[0] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Game.dll");
                textArray1[1] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Common.dll");
                textArray1[2] = Path.Combine(MyFileSystem.ExePath, "Sandbox.Graphics.dll");
                textArray1[3] = Path.Combine(MyFileSystem.ExePath, "VRage.dll");
                textArray1[4] = Path.Combine(MyFileSystem.ExePath, "VRage.Library.dll");
                textArray1[5] = Path.Combine(MyFileSystem.ExePath, "VRage.Math.dll");
                textArray1[6] = Path.Combine(MyFileSystem.ExePath, "VRage.Game.dll");
                textArray1[7] = Path.Combine(MyFileSystem.ExePath, "VRage.Render.dll");
                textArray1[8] = Path.Combine(MyFileSystem.ExePath, "VRage.Input.dll");
                textArray1[9] = Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.ObjectBuilders.dll");
                textArray1[10] = Path.Combine(MyFileSystem.ExePath, "SpaceEngineers.Game.dll");
                textArray1[11] = Path.Combine(MyFileSystem.ExePath, "System.Collections.Immutable.dll");
                textArray1[12] = AppDomain.CurrentDomain.GetAssemblies().First<Assembly>(x => (x.GetName().Name == "System.Runtime")).Location;
                string[] local3 = textArray1;
                string[] assemblyLocations = textArray1;
                assemblyLocations[13] = AppDomain.CurrentDomain.GetAssemblies().First<Assembly>(x => (x.GetName().Name == "System.Collections")).Location;
                MyScriptCompiler.Static.AddReferencedAssemblies(assemblyLocations);
                Type[] types = new Type[14];
                types[0] = typeof(MyTuple);
                types[1] = typeof(Vector2);
                types[2] = typeof(VRage.Game.Game);
                types[3] = typeof(ITerminalAction);
                types[4] = typeof(Sandbox.ModAPI.Ingame.IMyGridTerminalSystem);
                types[5] = typeof(MyModelComponent);
                types[6] = typeof(IMyComponentAggregate);
                types[7] = typeof(ListReader<>);
                types[8] = typeof(MyObjectBuilder_FactionDefinition);
                types[9] = typeof(IMyCubeBlock);
                types[10] = typeof(MyIni);
                types[11] = typeof(System.Collections.Immutable.ImmutableArray);
                types[12] = typeof(SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent);
                types[13] = typeof(MySprite);
                MyScriptCompiler.Static.AddImplicitIngameNamespacesFromTypes(types);
                string[] symbols = new string[] { this.GetPrefixedBranchName(), "STABLE", string.Empty, string.Empty, "VERSION_" + MyFinalBuildConstants.APP_VERSION.Minor, "BUILD_" + MyFinalBuildConstants.APP_VERSION.Build };
                MyScriptCompiler.Static.AddConditionalCompilationSymbols(symbols);
                if (MyFakes.ENABLE_ROSLYN_SCRIPT_DIAGNOSTICS)
                {
                    MyScriptCompiler.Static.DiagnosticOutputPath = Path.Combine(MyFileSystem.UserDataPath, "ScriptDiagnostics");
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MySpaceGameCustomInitialization.<>c <>9 = new MySpaceGameCustomInitialization.<>c();
            public static Func<Assembly, bool> <>9__1_0;
            public static Func<Assembly, bool> <>9__1_1;

            internal bool <InitIlCompiler>b__1_0(Assembly x) => 
                (x.GetName().Name == "System.Runtime");

            internal bool <InitIlCompiler>b__1_1(Assembly x) => 
                (x.GetName().Name == "System.Collections");
        }
    }
}

