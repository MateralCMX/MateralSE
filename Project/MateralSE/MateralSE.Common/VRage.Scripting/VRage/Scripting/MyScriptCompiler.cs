namespace VRage.Scripting
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Emit;
    using Sandbox.ModAPI;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using VRage.Collections;
    using VRage.Compiler;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Scripting.Analyzers;
    using VRage.Scripting.Rewriters;

    public class MyScriptCompiler
    {
        public static readonly MyScriptCompiler Static = new MyScriptCompiler();
        public static SortedDictionary<string, int> UsedNamespaces = new SortedDictionary<string, int>();
        private readonly List<MetadataReference> m_metadataReferences = new List<MetadataReference>();
        private readonly MyScriptWhitelist m_whitelist;
        private readonly CSharpCompilationOptions m_debugCompilationOptions;
        private readonly CSharpCompilationOptions m_runtimeCompilationOptions;
        private readonly WhitelistDiagnosticAnalyzer m_ingameWhitelistDiagnosticAnalyzer;
        private readonly WhitelistDiagnosticAnalyzer m_modApiWhitelistDiagnosticAnalyzer;
        private readonly HashSet<string> m_assemblyLocations = new HashSet<string>();
        private readonly HashSet<string> m_implicitScriptNamespaces = new HashSet<string>();
        private readonly HashSet<string> m_ignoredWarnings = new HashSet<string>();
        private readonly HashSet<Type> m_unblockableIngameExceptions = new HashSet<Type>();
        private readonly HashSet<string> m_conditionalCompilationSymbols = new HashSet<string>();
        private readonly CSharpParseOptions m_conditionalParseOptions;

        public MyScriptCompiler()
        {
            string[] assemblyLocations = new string[] { base.GetType().Assembly.Location, typeof(int).Assembly.Location, typeof(XmlEntity).Assembly.Location, typeof(HashSet<>).Assembly.Location, typeof(Dictionary<,>).Assembly.Location, typeof(Uri).Assembly.Location };
            this.AddReferencedAssemblies(assemblyLocations);
            Type[] types = new Type[] { typeof(object), typeof(StringBuilder), typeof(IEnumerable), typeof(IEnumerable<>), typeof(Enumerable) };
            this.AddImplicitIngameNamespacesFromTypes(types);
            Type[] typeArray2 = new Type[] { typeof(ScriptOutOfRangeException) };
            this.AddUnblockableIngameExceptions(typeArray2);
            System.Collections.Immutable.ImmutableArray<byte> array = new System.Collections.Immutable.ImmutableArray<byte>();
            bool? nullable = null;
            this.m_debugCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Debug, false, false, null, null, array, nullable, Platform.X64, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false);
            array = new System.Collections.Immutable.ImmutableArray<byte>();
            nullable = null;
            this.m_runtimeCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null, null, null, null, OptimizationLevel.Release, false, false, null, null, array, nullable, Platform.X64, ReportDiagnostic.Default, 4, null, true, false, null, null, null, null, null, false);
            this.m_whitelist = new MyScriptWhitelist(this);
            this.m_ingameWhitelistDiagnosticAnalyzer = new WhitelistDiagnosticAnalyzer(this.m_whitelist, MyWhitelistTarget.Ingame);
            this.m_modApiWhitelistDiagnosticAnalyzer = new WhitelistDiagnosticAnalyzer(this.m_whitelist, MyWhitelistTarget.ModApi);
            this.m_conditionalParseOptions = new CSharpParseOptions(LanguageVersion.CSharp6, DocumentationMode.Parse, SourceCodeKind.Regular, null);
        }

        public void AddConditionalCompilationSymbols(params string[] symbols)
        {
            for (int i = 0; i < symbols.Length; i++)
            {
                string str = symbols[i];
                if (str == null)
                {
                    throw new ArgumentNullException("symbols");
                }
                if (str != string.Empty)
                {
                    this.m_conditionalCompilationSymbols.Add(symbols[i]);
                }
            }
        }

        public void AddImplicitIngameNamespacesFromTypes(params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
                this.m_implicitScriptNamespaces.Add(types[i].Namespace);
            }
        }

        public void AddReferencedAssemblies(params string[] assemblyLocations)
        {
            for (int i = 0; i < assemblyLocations.Length; i++)
            {
                string item = assemblyLocations[i];
                if (item == null)
                {
                    throw new ArgumentNullException("assemblyLocations");
                }
                if (this.m_assemblyLocations.Add(item))
                {
                    MetadataReferenceProperties properties = new MetadataReferenceProperties();
                    this.m_metadataReferences.Add(MetadataReference.CreateFromFile(item, properties, null));
                }
            }
        }

        public void AddUnblockableIngameExceptions(params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type c = types[i];
                if (c == null)
                {
                    throw new ArgumentNullException("types");
                }
                if (!typeof(Exception).IsAssignableFrom(c))
                {
                    throw new ArgumentException(c.FullName + " is not an exception", "types");
                }
                if (c.IsGenericType || c.IsGenericTypeDefinition)
                {
                    throw new ArgumentException("Generic exceptions are not supported", "types");
                }
                this.m_unblockableIngameExceptions.Add(c);
            }
        }

        private void AnalyzeDiagnostics(System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics, List<Message> messages, ref bool success)
        {
            success = success && !System.Linq.ImmutableArrayExtensions.Any<Diagnostic>(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            foreach (Diagnostic diagnostic in from d in System.Linq.ImmutableArrayExtensions.Where<Diagnostic>(diagnostics, d => d.Severity >= DiagnosticSeverity.Warning)
                orderby d.Severity descending
                select d)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Warning)
                {
                    if (!success)
                    {
                        continue;
                    }
                    if (this.m_ignoredWarnings.Contains(diagnostic.Id))
                    {
                        continue;
                    }
                }
                TErrorSeverity severity = (diagnostic.Severity == DiagnosticSeverity.Warning) ? TErrorSeverity.Warning : TErrorSeverity.Error;
                FileLinePositionSpan mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
                messages.Add(new Message(severity, $"{mappedLineSpan.Path}({mappedLineSpan.StartLinePosition.Line + 1},{mappedLineSpan.StartLinePosition.Character}): {severity}: {diagnostic.GetMessage((IFormatProvider) null)}"));
            }
        }

        [AsyncStateMachine(typeof(<Compile>d__37))]
        public Task<Assembly> Compile(MyApiTarget target, string assemblyName, IEnumerable<Script> scripts, List<Message> messages, string friendlyName, bool enableDebugInformation = false)
        {
            <Compile>d__37 d__;
            d__.<>4__this = this;
            d__.target = target;
            d__.assemblyName = assemblyName;
            d__.scripts = scripts;
            d__.messages = messages;
            d__.friendlyName = friendlyName;
            d__.enableDebugInformation = enableDebugInformation;
            d__.<>t__builder = AsyncTaskMethodBuilder<Assembly>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<Compile>d__37>(ref d__);
            return d__.<>t__builder.Task;
        }

        public Task<Assembly> Compile(MyApiTarget target, string assemblyName, Script script, List<Message> messages, string friendlyName, bool enableDebugInformation = false)
        {
            Script[] scripts = new Script[] { script };
            return this.Compile(target, assemblyName, scripts, messages, friendlyName, enableDebugInformation);
        }

        internal CSharpCompilation CreateCompilation(string assemblyFileName, IEnumerable<Script> scripts, bool enableDebugInformation)
        {
            CSharpCompilationOptions debugCompilationOptions;
            if (enableDebugInformation || this.EnableDebugInformation)
            {
                debugCompilationOptions = this.m_debugCompilationOptions;
            }
            else
            {
                debugCompilationOptions = this.m_runtimeCompilationOptions;
            }
            CSharpCompilationOptions options = debugCompilationOptions;
            IEnumerable<SyntaxTree> enumerable = null;
            if (scripts != null)
            {
                CSharpParseOptions parseOptions = this.m_conditionalParseOptions.WithPreprocessorSymbols(this.ConditionalCompilationSymbols);
                enumerable = from s in scripts select CSharpSyntaxTree.ParseText(s.Code, parseOptions, s.Name, Encoding.UTF8, new CancellationToken());
            }
            return CSharpCompilation.Create(MakeAssemblyName(assemblyFileName), enumerable, this.m_metadataReferences, options);
        }

        [AsyncStateMachine(typeof(<EmitDiagnostics>d__39))]
        private Task<bool> EmitDiagnostics(CompilationWithAnalyzers analyticCompilation, EmitResult result, List<Message> messages, bool success)
        {
            <EmitDiagnostics>d__39 d__;
            d__.<>4__this = this;
            d__.analyticCompilation = analyticCompilation;
            d__.result = result;
            d__.messages = messages;
            d__.success = success;
            d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<EmitDiagnostics>d__39>(ref d__);
            return d__.<>t__builder.Task;
        }

        private bool GetDiagnosticsOutputPath(MyApiTarget target, string assemblyName, out string outputPath)
        {
            outputPath = this.DiagnosticOutputPath;
            if (outputPath == null)
            {
                return false;
            }
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            outputPath = Path.Combine(this.DiagnosticOutputPath, target.ToString(), Path.GetFileNameWithoutExtension(assemblyName));
            return true;
        }

        public Script GetIngameScript(string code, string className, string inheritance, string modifiers = "sealed")
        {
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("Argument is null or empty", "className");
            }
            string str = string.Join(";\nusing ", this.m_implicitScriptNamespaces);
            modifiers = modifiers ?? "";
            string text1 = ": " + inheritance;
            inheritance = string.IsNullOrEmpty(inheritance) ? "" : text1;
            code = code ?? "";
            string str2 = "#line 1 \"{2}\"\n";
            return new Script(className, string.Format("using {0};\npublic {1} class {2} {3}{{\n" + str2 + "{4}\n}}\n", new object[] { str, modifiers, className, inheritance, code }));
        }

        private SyntaxTree InjectInstructionCounter(CSharpCompilation compilation, SyntaxTree tree) => 
            new InstructionCountingRewriter(this, compilation, tree).Rewrite();

        private SyntaxTree InjectPerformanceCounters(CSharpCompilation compilation, SyntaxTree syntaxTree, int modId) => 
            PerfCountingRewriter.Rewrite(compilation, syntaxTree, modId);

        private static string MakeAssemblyName(string name) => 
            ((name != null) ? Path.GetFileName(name) : "scripts.dll");

        [AsyncStateMachine(typeof(<WriteDiagnostics>d__44))]
        private Task WriteDiagnostics(MyApiTarget target, string assemblyName, IEnumerable<Message> messages, bool success)
        {
            <WriteDiagnostics>d__44 d__;
            d__.<>4__this = this;
            d__.target = target;
            d__.assemblyName = assemblyName;
            d__.messages = messages;
            d__.success = success;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<WriteDiagnostics>d__44>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<WriteDiagnostics>d__45))]
        private Task WriteDiagnostics(MyApiTarget target, string assemblyName, IList<SyntaxTree> syntaxTrees, string suffix = null)
        {
            <WriteDiagnostics>d__45 d__;
            d__.<>4__this = this;
            d__.target = target;
            d__.assemblyName = assemblyName;
            d__.syntaxTrees = syntaxTrees;
            d__.suffix = suffix;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<WriteDiagnostics>d__45>(ref d__);
            return d__.<>t__builder.Task;
        }

        public HashSetReader<string> AssemblyLocations =>
            this.m_assemblyLocations;

        public HashSetReader<string> ImplicitIngameScriptNamespaces =>
            this.m_implicitScriptNamespaces;

        public HashSetReader<Type> UnblockableIngameExceptions =>
            this.m_unblockableIngameExceptions;

        public HashSetReader<string> ConditionalCompilationSymbols =>
            this.m_conditionalCompilationSymbols;

        public string DiagnosticOutputPath { get; set; }

        public MyScriptWhitelist Whitelist =>
            this.m_whitelist;

        public HashSet<string> IgnoredWarnings =>
            this.m_ignoredWarnings;

        public bool EnableDebugInformation { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScriptCompiler.<>c <>9 = new MyScriptCompiler.<>c();
            public static Func<Diagnostic, bool> <>9__42_0;
            public static Func<Diagnostic, bool> <>9__42_1;
            public static Func<Diagnostic, DiagnosticSeverity> <>9__42_2;

            internal bool <AnalyzeDiagnostics>b__42_0(Diagnostic d) => 
                (d.Severity == DiagnosticSeverity.Error);

            internal bool <AnalyzeDiagnostics>b__42_1(Diagnostic d) => 
                (d.Severity >= DiagnosticSeverity.Warning);

            internal DiagnosticSeverity <AnalyzeDiagnostics>b__42_2(Diagnostic d) => 
                d.Severity;
        }

        [CompilerGenerated]
        private struct <Compile>d__37 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<Assembly> <>t__builder;
            public MyScriptCompiler <>4__this;
            public string friendlyName;
            public MyApiTarget target;
            public string assemblyName;
            public IEnumerable<Script> scripts;
            public bool enableDebugInformation;
            private MyScriptCompiler.<>c__DisplayClass37_0 <>8__1;
            public List<MyScriptCompiler.Message> messages;
            private DiagnosticAnalyzer <whitelistAnalyzer>5__2;
            private bool <injectionFailed>5__3;
            private CSharpCompilation <compilationWithoutInjection>5__4;
            private CompilationWithAnalyzers <analyticCompilation>5__5;
            private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter <>u__1;
            private SyntaxTree[] <newSyntaxTrees>5__6;
            private ConfiguredTaskAwaitable<SyntaxTree[]>.ConfiguredTaskAwaiter <>u__2;
            private MemoryStream <pdbStream>5__7;
            private MemoryStream <assemblyStream>5__8;
            private bool <loadPDBs>5__9;
            private bool <success>5__10;
            private ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter <>u__3;

            private void MoveNext()
            {
                Assembly assembly;
                int num = this.<>1__state;
                MyScriptCompiler compiler = this.<>4__this;
                try
                {
                    ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                    CancellationToken token;
                    switch (num)
                    {
                        case 0:
                            awaiter = this.<>u__1;
                            this.<>u__1 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                            break;

                        case 1:
                            goto TR_0036;

                        case 2:
                            awaiter = this.<>u__1;
                            this.<>u__1 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0028;

                        case 3:
                        case 4:
                        case 5:
                            goto TR_0022;

                        default:
                            this.<>8__1 = new MyScriptCompiler.<>c__DisplayClass37_0();
                            this.<>8__1.<>4__this = this.<>4__this;
                            if (this.friendlyName == null)
                            {
                                this.friendlyName = "<No Name>";
                            }
                            switch (this.target)
                            {
                                case MyApiTarget.None:
                                    this.<whitelistAnalyzer>5__2 = null;
                                    this.<>8__1.syntaxTreeInjector = null;
                                    break;

                                case MyApiTarget.Mod:
                                {
                                    MyScriptCompiler.<>c__DisplayClass37_1 class_;
                                    MyScriptCompiler.<>c__DisplayClass37_0 CS$<>8__locals1 = this.<>8__1;
                                    int modId = MyModWatchdog.AllocateModId(this.friendlyName);
                                    this.<whitelistAnalyzer>5__2 = compiler.m_modApiWhitelistDiagnosticAnalyzer;
                                    CS$<>8__locals1.syntaxTreeInjector = new Func<CSharpCompilation, SyntaxTree, SyntaxTree>(class_.<Compile>b__0);
                                    break;
                                }
                                case MyApiTarget.Ingame:
                                    this.<>8__1.syntaxTreeInjector = new Func<CSharpCompilation, SyntaxTree, SyntaxTree>(compiler.InjectInstructionCounter);
                                    this.<whitelistAnalyzer>5__2 = compiler.m_ingameWhitelistDiagnosticAnalyzer;
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException("target", this.target, "Invalid compilation target");
                            }
                            this.<>8__1.compilation = compiler.CreateCompilation(this.assemblyName, this.scripts, this.enableDebugInformation);
                            awaiter = compiler.WriteDiagnostics(this.target, this.assemblyName, (IList<SyntaxTree>) this.<>8__1.compilation.SyntaxTrees, null).ConfigureAwait(false).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                break;
                            }
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter, ref this);
                            return;
                    }
                    awaiter.GetResult();
                    this.<injectionFailed>5__3 = false;
                    this.<compilationWithoutInjection>5__4 = this.<>8__1.compilation;
                    if (this.<>8__1.syntaxTreeInjector == null)
                    {
                        goto TR_0025;
                    }
                    else
                    {
                        this.<newSyntaxTrees>5__6 = null;
                    }
                    goto TR_0036;
                TR_0022:
                    try
                    {
                        if ((num - 3) > 2)
                        {
                            this.<assemblyStream>5__8 = new MemoryStream();
                        }
                        try
                        {
                            EmitResult result;
                            ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter awaiter3;
                            switch (num)
                            {
                                case 3:
                                    awaiter3 = this.<>u__3;
                                    this.<>u__3 = new ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter();
                                    this.<>1__state = num = -1;
                                    break;

                                case 4:
                                    awaiter = this.<>u__1;
                                    this.<>u__1 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                                    this.<>1__state = num = -1;
                                    goto TR_0015;

                                case 5:
                                    awaiter3 = this.<>u__3;
                                    this.<>u__3 = new ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter();
                                    this.<>1__state = num = -1;
                                    goto TR_0011;

                                default:
                                    this.<loadPDBs>5__9 = false;
                                    token = new CancellationToken();
                                    result = this.<>8__1.compilation.Emit(this.<assemblyStream>5__8, this.<pdbStream>5__7, null, null, null, null, null, token);
                                    this.<success>5__10 = result.Success;
                                    awaiter3 = compiler.EmitDiagnostics(this.<analyticCompilation>5__5, result, this.messages, this.<success>5__10).ConfigureAwait(false).GetAwaiter();
                                    if (awaiter3.IsCompleted)
                                    {
                                        break;
                                    }
                                    this.<>1__state = num = 3;
                                    this.<>u__3 = awaiter3;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter3, ref this);
                                    return;
                            }
                            bool flag = awaiter3.GetResult();
                            this.<success>5__10 = flag;
                            awaiter = compiler.WriteDiagnostics(this.target, this.assemblyName, this.messages, this.<success>5__10).ConfigureAwait(false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                this.<>1__state = num = 4;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter, ref this);
                                return;
                            }
                            goto TR_0015;
                        TR_0010:
                            assembly = null;
                            goto TR_0005;
                        TR_0011:
                            awaiter3.GetResult();
                            goto TR_0010;
                        TR_0015:
                            awaiter.GetResult();
                            this.<pdbStream>5__7.Seek(0L, SeekOrigin.Begin);
                            this.<assemblyStream>5__8.Seek(0L, SeekOrigin.Begin);
                            if (this.<injectionFailed>5__3)
                            {
                                goto TR_0010;
                            }
                            else if (!this.<success>5__10)
                            {
                                token = new CancellationToken();
                                result = this.<compilationWithoutInjection>5__4.Emit(this.<assemblyStream>5__8, null, null, null, null, null, null, token);
                                awaiter3 = compiler.EmitDiagnostics(this.<analyticCompilation>5__5, result, this.messages, false).ConfigureAwait(false).GetAwaiter();
                                if (awaiter3.IsCompleted)
                                {
                                    goto TR_0011;
                                }
                                else
                                {
                                    this.<>1__state = num = 5;
                                    this.<>u__3 = awaiter3;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable<bool>.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter3, ref this);
                                }
                                return;
                            }
                            else
                            {
                                assembly = !this.<loadPDBs>5__9 ? Assembly.Load(this.<assemblyStream>5__8.ToArray()) : Assembly.Load(this.<assemblyStream>5__8.ToArray(), this.<pdbStream>5__7.ToArray());
                            }
                        }
                        finally
                        {
                            if ((num < 0) && (this.<assemblyStream>5__8 != null))
                            {
                                this.<assemblyStream>5__8.Dispose();
                            }
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<pdbStream>5__7 != null))
                        {
                            this.<pdbStream>5__7.Dispose();
                        }
                    }
                    goto TR_0005;
                TR_0025:
                    this.<analyticCompilation>5__5 = null;
                    if (this.<whitelistAnalyzer>5__2 != null)
                    {
                        token = new CancellationToken();
                        this.<analyticCompilation>5__5 = DiagnosticAnalyzerExtensions.WithAnalyzers(this.<>8__1.compilation, System.Collections.Immutable.ImmutableArray.Create<DiagnosticAnalyzer>(this.<whitelistAnalyzer>5__2), null, token);
                        this.<>8__1.compilation = (CSharpCompilation) this.<analyticCompilation>5__5.Compilation;
                    }
                    this.<pdbStream>5__7 = new MemoryStream();
                    goto TR_0022;
                TR_0027:
                    this.<newSyntaxTrees>5__6 = null;
                    goto TR_0025;
                TR_0028:
                    awaiter.GetResult();
                    this.<>8__1.compilation = this.<>8__1.compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(this.<newSyntaxTrees>5__6);
                    goto TR_0027;
                TR_002B:
                    if (this.<injectionFailed>5__3)
                    {
                        goto TR_0027;
                    }
                    else
                    {
                        awaiter = compiler.WriteDiagnostics(this.target, this.assemblyName, this.<newSyntaxTrees>5__6, ".injected").ConfigureAwait(false).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0028;
                        }
                        else
                        {
                            this.<>1__state = num = 2;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter, ref this);
                        }
                    }
                    return;
                TR_0036:
                    try
                    {
                        SyntaxTree[] treeArray;
                        ConfiguredTaskAwaitable<SyntaxTree[]>.ConfiguredTaskAwaiter awaiter2;
                        if (num == 1)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new ConfiguredTaskAwaitable<SyntaxTree[]>.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_002F;
                        }
                        else
                        {
                            System.Collections.Immutable.ImmutableArray<SyntaxTree> syntaxTrees = this.<>8__1.compilation.SyntaxTrees;
                            if (syntaxTrees.Length != 1)
                            {
                                awaiter2 = Task.WhenAll<SyntaxTree>(System.Linq.ImmutableArrayExtensions.Select<SyntaxTree, Task<SyntaxTree>>(syntaxTrees, new Func<SyntaxTree, Task<SyntaxTree>>(this.<>8__1.<Compile>b__1))).ConfigureAwait(false).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto TR_002F;
                                }
                                else
                                {
                                    this.<>1__state = num = 1;
                                    this.<>u__2 = awaiter2;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable<SyntaxTree[]>.ConfiguredTaskAwaiter, MyScriptCompiler.<Compile>d__37>(ref awaiter2, ref this);
                                }
                                return;
                            }
                            else
                            {
                                this.<newSyntaxTrees>5__6 = new SyntaxTree[] { this.<>8__1.syntaxTreeInjector(this.<>8__1.compilation, syntaxTrees[0]) };
                            }
                        }
                        goto TR_002B;
                    TR_002F:
                        treeArray = awaiter2.GetResult();
                        this.<newSyntaxTrees>5__6 = treeArray;
                    }
                    catch
                    {
                        this.<injectionFailed>5__3 = true;
                    }
                    goto TR_002B;
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
                return;
            TR_0005:
                this.<>1__state = -2;
                this.<>t__builder.SetResult(assembly);
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <EmitDiagnostics>d__39 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<bool> <>t__builder;
            public List<MyScriptCompiler.Message> messages;
            public MyScriptCompiler <>4__this;
            public EmitResult result;
            public bool success;
            public CompilationWithAnalyzers analyticCompilation;
            private ConfiguredTaskAwaitable<System.Collections.Immutable.ImmutableArray<Diagnostic>>.ConfiguredTaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                MyScriptCompiler compiler = this.<>4__this;
                try
                {
                    bool flag;
                    ConfiguredTaskAwaitable<System.Collections.Immutable.ImmutableArray<Diagnostic>>.ConfiguredTaskAwaiter awaiter;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new ConfiguredTaskAwaitable<System.Collections.Immutable.ImmutableArray<Diagnostic>>.ConfiguredTaskAwaiter();
                        this.<>1__state = num = -1;
                        goto TR_0005;
                    }
                    else
                    {
                        this.messages.Clear();
                        compiler.AnalyzeDiagnostics(this.result.Diagnostics, this.messages, ref this.success);
                        if (this.analyticCompilation == null)
                        {
                            goto TR_0004;
                        }
                        else
                        {
                            awaiter = this.analyticCompilation.GetAllDiagnosticsAsync().ConfigureAwait(false).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0005;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable<System.Collections.Immutable.ImmutableArray<Diagnostic>>.ConfiguredTaskAwaiter, MyScriptCompiler.<EmitDiagnostics>d__39>(ref awaiter, ref this);
                            }
                        }
                    }
                    return;
                TR_0004:
                    flag = this.success;
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(flag);
                    return;
                TR_0005:
                    compiler.AnalyzeDiagnostics(awaiter.GetResult(), this.messages, ref this.success);
                    goto TR_0004;
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <WriteDiagnostics>d__44 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public MyScriptCompiler <>4__this;
            public MyApiTarget target;
            public string assemblyName;
            public bool success;
            public IEnumerable<MyScriptCompiler.Message> messages;
            private Stream <stream>5__2;
            private StreamWriter <writer>5__3;
            private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                MyScriptCompiler compiler = this.<>4__this;
                try
                {
                    StringBuilder builder;
                    if (num > 1)
                    {
                        string str;
                        if (compiler.GetDiagnosticsOutputPath(this.target, this.assemblyName, out str))
                        {
                            string path = Path.Combine(str, "log.txt");
                            builder = new StringBuilder();
                            builder.AppendLine("Success: " + this.success.ToString());
                            builder.AppendLine();
                            IEnumerator<MyScriptCompiler.Message> enumerator = this.messages.GetEnumerator();
                            try
                            {
                                while (enumerator.MoveNext())
                                {
                                    MyScriptCompiler.Message current = enumerator.Current;
                                    builder.AppendLine(current.Severity + " " + current.Text);
                                }
                            }
                            finally
                            {
                                if ((num < 0) && (enumerator != null))
                                {
                                    enumerator.Dispose();
                                }
                            }
                            this.<stream>5__2 = MyFileSystem.OpenWrite(path, FileMode.Create);
                        }
                        else
                        {
                            goto TR_0002;
                        }
                    }
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                        }
                        else if (num == 1)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0009;
                        }
                        else
                        {
                            this.<writer>5__3 = new StreamWriter(this.<stream>5__2);
                            awaiter = this.<writer>5__3.WriteAsync(builder.ToString()).ConfigureAwait(false).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<WriteDiagnostics>d__44>(ref awaiter, ref this);
                                return;
                            }
                        }
                        awaiter.GetResult();
                        awaiter = this.<writer>5__3.FlushAsync().ConfigureAwait(false).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0009;
                        }
                        else
                        {
                            this.<>1__state = num = 1;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<WriteDiagnostics>d__44>(ref awaiter, ref this);
                        }
                        return;
                    TR_0009:
                        awaiter.GetResult();
                        this.<writer>5__3 = null;
                    }
                    finally
                    {
                        if ((num < 0) && (this.<stream>5__2 != null))
                        {
                            this.<stream>5__2.Dispose();
                        }
                    }
                    this.<stream>5__2 = null;
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                    return;
                }
            TR_0002:
                this.<>1__state = -2;
                this.<>t__builder.SetResult();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [CompilerGenerated]
        private struct <WriteDiagnostics>d__45 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public MyScriptCompiler <>4__this;
            public MyApiTarget target;
            public string assemblyName;
            public string suffix;
            public IList<SyntaxTree> syntaxTrees;
            private string <outputPath>5__2;
            private int <i>5__3;
            private SyntaxTree <syntaxTree>5__4;
            private SyntaxTree <normalizedTree>5__5;
            private ConfiguredTaskAwaitable<SyntaxNode>.ConfiguredTaskAwaiter <>u__1;
            private Stream <stream>5__6;
            private StreamWriter <writer>5__7;
            private ConfiguredTaskAwaitable.ConfiguredTaskAwaiter <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                MyScriptCompiler compiler = this.<>4__this;
                try
                {
                    SyntaxNode node;
                    ConfiguredTaskAwaitable<SyntaxNode>.ConfiguredTaskAwaiter awaiter;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new ConfiguredTaskAwaitable<SyntaxNode>.ConfiguredTaskAwaiter();
                        this.<>1__state = num = -1;
                        goto TR_0019;
                    }
                    else if ((num - 1) <= 1)
                    {
                        goto TR_0016;
                    }
                    else if (compiler.GetDiagnosticsOutputPath(this.target, this.assemblyName, out this.<outputPath>5__2))
                    {
                        this.suffix = this.suffix ?? "";
                        this.<i>5__3 = 0;
                    }
                    else
                    {
                        goto TR_0002;
                    }
                    goto TR_001E;
                TR_0006:
                    this.<stream>5__6 = null;
                    if (this.syntaxTrees is Array)
                    {
                        this.syntaxTrees[this.<i>5__3] = this.<normalizedTree>5__5;
                    }
                    this.<syntaxTree>5__4 = null;
                    this.<normalizedTree>5__5 = null;
                    int num2 = this.<i>5__3;
                    this.<i>5__3 = num2 + 1;
                    goto TR_001E;
                TR_0016:
                    try
                    {
                        ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter2;
                        if (num == 1)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                        }
                        else if (num == 2)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new ConfiguredTaskAwaitable.ConfiguredTaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_000D;
                        }
                        else
                        {
                            this.<writer>5__7 = new StreamWriter(this.<stream>5__6);
                            awaiter2 = this.<writer>5__7.WriteAsync(this.<normalizedTree>5__5.ToString()).ConfigureAwait(false).GetAwaiter();
                            if (!awaiter2.IsCompleted)
                            {
                                this.<>1__state = num = 1;
                                this.<>u__2 = awaiter2;
                                this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<WriteDiagnostics>d__45>(ref awaiter2, ref this);
                                return;
                            }
                        }
                        awaiter2.GetResult();
                        awaiter2 = this.<writer>5__7.FlushAsync().ConfigureAwait(false).GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_000D;
                        }
                        else
                        {
                            this.<>1__state = num = 2;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, MyScriptCompiler.<WriteDiagnostics>d__45>(ref awaiter2, ref this);
                        }
                        return;
                    TR_000D:
                        awaiter2.GetResult();
                        this.<writer>5__7 = null;
                    }
                    finally
                    {
                        if ((num < 0) && (this.<stream>5__6 != null))
                        {
                            this.<stream>5__6.Dispose();
                        }
                    }
                    goto TR_0006;
                TR_0019:
                    node = awaiter.GetResult();
                    string path = Path.Combine(this.<outputPath>5__2, Path.GetFileNameWithoutExtension(this.<syntaxTree>5__4.FilePath) + this.suffix + Path.GetExtension(this.<syntaxTree>5__4.FilePath));
                    if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        path = path + ".cs";
                    }
                    this.<normalizedTree>5__5 = this.<syntaxTree>5__4.WithRootAndOptions(node.NormalizeWhitespace<SyntaxNode>("    ", "\r\n", false), this.<syntaxTree>5__4.Options).WithFilePath(path);
                    this.<stream>5__6 = MyFileSystem.OpenWrite(path, FileMode.Create);
                    goto TR_0016;
                TR_001E:
                    while (true)
                    {
                        if (this.<i>5__3 >= this.syntaxTrees.Count)
                        {
                            break;
                        }
                        this.<syntaxTree>5__4 = this.syntaxTrees[this.<i>5__3];
                        CancellationToken token = new CancellationToken();
                        awaiter = this.<syntaxTree>5__4.GetRootAsync(token).ConfigureAwait(false).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0019;
                        }
                        else
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<ConfiguredTaskAwaitable<SyntaxNode>.ConfiguredTaskAwaiter, MyScriptCompiler.<WriteDiagnostics>d__45>(ref awaiter, ref this);
                        }
                        return;
                    }
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                    return;
                }
            TR_0002:
                this.<>1__state = -2;
                this.<>t__builder.SetResult();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public readonly TErrorSeverity Severity;
            public readonly string Text;
            public Message(TErrorSeverity severity, string text)
            {
                this = new MyScriptCompiler.Message();
                this.Severity = severity;
                this.Text = text;
            }
        }
    }
}

