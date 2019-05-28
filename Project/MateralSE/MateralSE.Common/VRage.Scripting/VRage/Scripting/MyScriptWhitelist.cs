namespace VRage.Scripting
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Sandbox.ModAPI;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Timers;
    using System.Xml.Serialization;
    using VRage.Collections;

    public class MyScriptWhitelist : IMyScriptBlacklist
    {
        private readonly HashSet<string> m_ingameBlacklist = new HashSet<string>();
        private readonly MyScriptCompiler m_scriptCompiler;
        private readonly Dictionary<string, MyWhitelistTarget> m_whitelist = new Dictionary<string, MyWhitelistTarget>();

        public MyScriptWhitelist(MyScriptCompiler scriptCompiler)
        {
            this.m_scriptCompiler = scriptCompiler;
            using (IMyWhitelistBatch batch = this.OpenBatch())
            {
                Type[] types = new Type[] { typeof(IEnumerator), typeof(HashSet<>), typeof(Queue<>), typeof(IEnumerator<>), typeof(StringBuilder), typeof(Regex), typeof(Calendar) };
                batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, types);
                Type[] typeArray2 = new Type[] { typeof(Enumerable), typeof(ConcurrentBag<>), typeof(ConcurrentDictionary<,>) };
                batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray2);
                batch.AllowTypes(MyWhitelistTarget.Ingame, (from x in typeof(Enumerable).Assembly.GetTypes()
                    where x.Namespace == "System.Linq"
                    where !x.Name.Contains("parallel", StringComparison.InvariantCultureIgnoreCase)
                    select x).ToArray<Type>());
                Type[] typeArray3 = new Type[] { typeof(Timer) };
                batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeArray3);
                Type[] typeArray4 = new Type[0x12];
                typeArray4[0] = typeof(TraceEventType);
                typeArray4[1] = typeof(AssemblyProductAttribute);
                typeArray4[2] = typeof(AssemblyDescriptionAttribute);
                typeArray4[3] = typeof(AssemblyConfigurationAttribute);
                typeArray4[4] = typeof(AssemblyCompanyAttribute);
                typeArray4[5] = typeof(AssemblyCultureAttribute);
                typeArray4[6] = typeof(AssemblyVersionAttribute);
                typeArray4[7] = typeof(AssemblyFileVersionAttribute);
                typeArray4[8] = typeof(AssemblyCopyrightAttribute);
                typeArray4[9] = typeof(AssemblyTrademarkAttribute);
                typeArray4[10] = typeof(AssemblyTitleAttribute);
                typeArray4[11] = typeof(ComVisibleAttribute);
                typeArray4[12] = typeof(DefaultValueAttribute);
                typeArray4[13] = typeof(SerializableAttribute);
                typeArray4[14] = typeof(GuidAttribute);
                typeArray4[15] = typeof(StructLayoutAttribute);
                typeArray4[0x10] = typeof(LayoutKind);
                typeArray4[0x11] = typeof(Guid);
                batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray4);
                Type[] typeArray5 = new Type[70];
                typeArray5[0] = typeof(object);
                typeArray5[1] = typeof(IDisposable);
                typeArray5[2] = typeof(string);
                typeArray5[3] = typeof(StringComparison);
                typeArray5[4] = typeof(Math);
                typeArray5[5] = typeof(Enum);
                typeArray5[6] = typeof(int);
                typeArray5[7] = typeof(short);
                typeArray5[8] = typeof(long);
                typeArray5[9] = typeof(uint);
                typeArray5[10] = typeof(ushort);
                typeArray5[11] = typeof(ulong);
                typeArray5[12] = typeof(double);
                typeArray5[13] = typeof(float);
                typeArray5[14] = typeof(bool);
                typeArray5[15] = typeof(char);
                typeArray5[0x10] = typeof(byte);
                typeArray5[0x11] = typeof(sbyte);
                typeArray5[0x12] = typeof(decimal);
                typeArray5[0x13] = typeof(DateTime);
                typeArray5[20] = typeof(TimeSpan);
                typeArray5[0x15] = typeof(Array);
                typeArray5[0x16] = typeof(XmlElementAttribute);
                typeArray5[0x17] = typeof(XmlAttributeAttribute);
                typeArray5[0x18] = typeof(XmlArrayAttribute);
                typeArray5[0x19] = typeof(XmlArrayItemAttribute);
                typeArray5[0x1a] = typeof(XmlAnyAttributeAttribute);
                typeArray5[0x1b] = typeof(XmlAnyElementAttribute);
                typeArray5[0x1c] = typeof(XmlAnyElementAttributes);
                typeArray5[0x1d] = typeof(XmlArrayItemAttributes);
                typeArray5[30] = typeof(XmlAttributeEventArgs);
                typeArray5[0x1f] = typeof(XmlAttributeOverrides);
                typeArray5[0x20] = typeof(XmlAttributes);
                typeArray5[0x21] = typeof(XmlChoiceIdentifierAttribute);
                typeArray5[0x22] = typeof(XmlElementAttributes);
                typeArray5[0x23] = typeof(XmlElementEventArgs);
                typeArray5[0x24] = typeof(XmlEnumAttribute);
                typeArray5[0x25] = typeof(XmlIgnoreAttribute);
                typeArray5[0x26] = typeof(XmlIncludeAttribute);
                typeArray5[0x27] = typeof(XmlRootAttribute);
                typeArray5[40] = typeof(XmlTextAttribute);
                typeArray5[0x29] = typeof(XmlTypeAttribute);
                typeArray5[0x2a] = typeof(RuntimeHelpers);
                typeArray5[0x2b] = typeof(BinaryReader);
                typeArray5[0x2c] = typeof(BinaryWriter);
                typeArray5[0x2d] = typeof(NullReferenceException);
                typeArray5[0x2e] = typeof(ArgumentException);
                typeArray5[0x2f] = typeof(ArgumentNullException);
                typeArray5[0x30] = typeof(InvalidOperationException);
                typeArray5[0x31] = typeof(FormatException);
                typeArray5[50] = typeof(Exception);
                typeArray5[0x33] = typeof(DivideByZeroException);
                typeArray5[0x34] = typeof(InvalidCastException);
                typeArray5[0x35] = typeof(FileNotFoundException);
                typeArray5[0x36] = typeof(NotSupportedException);
                typeArray5[0x37] = typeof(Nullable<>);
                typeArray5[0x38] = typeof(StringComparer);
                typeArray5[0x39] = typeof(IEquatable<>);
                typeArray5[0x3a] = typeof(IComparable);
                typeArray5[0x3b] = typeof(IComparable<>);
                typeArray5[60] = typeof(BitConverter);
                typeArray5[0x3d] = typeof(FlagsAttribute);
                typeArray5[0x3e] = typeof(Path);
                typeArray5[0x3f] = typeof(Random);
                typeArray5[0x40] = typeof(Convert);
                typeArray5[0x41] = typeof(StringSplitOptions);
                typeArray5[0x42] = typeof(DateTimeKind);
                typeArray5[0x43] = typeof(MidpointRounding);
                typeArray5[0x44] = typeof(EventArgs);
                typeArray5[0x45] = typeof(Buffer);
                batch.AllowTypes(MyWhitelistTarget.Both, typeArray5);
                Type[] typeArray6 = new Type[] { typeof(Stream), typeof(TextWriter), typeof(TextReader) };
                batch.AllowTypes(MyWhitelistTarget.ModApi, typeArray6);
                MemberInfo[] members = new MemberInfo[] { typeof(MemberInfo).GetProperty("Name") };
                batch.AllowMembers(MyWhitelistTarget.Both, members);
                MemberInfo[] infoArray2 = new MemberInfo[6];
                infoArray2[0] = typeof(Type).GetProperty("FullName");
                infoArray2[1] = typeof(Type).GetMethod("GetTypeFromHandle");
                Type[] typeArray7 = new Type[] { typeof(BindingFlags) };
                infoArray2[2] = typeof(Type).GetMethod("GetFields", typeArray7);
                infoArray2[3] = typeof(Type).GetMethod("IsEquivalentTo");
                infoArray2[4] = typeof(Type).GetMethod("op_Equality");
                infoArray2[5] = typeof(Type).GetMethod("ToString");
                batch.AllowMembers(MyWhitelistTarget.Both, infoArray2);
                MemberInfo[] infoArray3 = new MemberInfo[] { typeof(ValueType).GetMethod("Equals"), typeof(ValueType).GetMethod("GetHashCode"), typeof(ValueType).GetMethod("ToString") };
                batch.AllowMembers(MyWhitelistTarget.Both, infoArray3);
                MemberInfo[] infoArray4 = new MemberInfo[] { typeof(Environment).GetProperty("CurrentManagedThreadId", BindingFlags.Public | BindingFlags.Static), typeof(Environment).GetProperty("NewLine", BindingFlags.Public | BindingFlags.Static), typeof(Environment).GetProperty("ProcessorCount", BindingFlags.Public | BindingFlags.Static) };
                batch.AllowMembers(MyWhitelistTarget.Both, infoArray4);
                Type type = typeof(Type).Assembly.GetType("System.RuntimeType");
                MemberInfo[] infoArray5 = new MemberInfo[2];
                infoArray5[0] = type.GetMethod("op_Inequality");
                Type[] typeArray8 = new Type[] { typeof(BindingFlags) };
                infoArray5[1] = type.GetMethod("GetFields", typeArray8);
                batch.AllowMembers(MyWhitelistTarget.Both, infoArray5);
                batch.AllowMembers(MyWhitelistTarget.Both, (from m in AllDeclaredMembers(typeof(Delegate))
                    where m.Name != "CreateDelegate"
                    select m).ToArray<MemberInfo>());
                Type[] typeArray9 = new Type[0x22];
                typeArray9[0] = typeof(Action);
                typeArray9[1] = typeof(Action<>);
                typeArray9[2] = typeof(Action<,>);
                typeArray9[3] = typeof(Action<,,>);
                typeArray9[4] = typeof(Action<,,,>);
                typeArray9[5] = typeof(Action<,,,,>);
                typeArray9[6] = typeof(Action<,,,,,>);
                typeArray9[7] = typeof(Action<,,,,,,>);
                typeArray9[8] = typeof(Action<,,,,,,,>);
                typeArray9[9] = typeof(Action<,,,,,,,,>);
                typeArray9[10] = typeof(Action<,,,,,,,,,>);
                typeArray9[11] = typeof(Action<,,,,,,,,,,>);
                typeArray9[12] = typeof(Action<,,,,,,,,,,,>);
                typeArray9[13] = typeof(Action<,,,,,,,,,,,,>);
                typeArray9[14] = typeof(Action<,,,,,,,,,,,,,>);
                typeArray9[15] = typeof(Action<,,,,,,,,,,,,,,>);
                typeArray9[0x10] = typeof(Action<,,,,,,,,,,,,,,,>);
                typeArray9[0x11] = typeof(Func<>);
                typeArray9[0x12] = typeof(Func<,>);
                typeArray9[0x13] = typeof(Func<,,>);
                typeArray9[20] = typeof(Func<,,,>);
                typeArray9[0x15] = typeof(Func<,,,,>);
                typeArray9[0x16] = typeof(Func<,,,,,>);
                typeArray9[0x17] = typeof(Func<,,,,,,>);
                typeArray9[0x18] = typeof(Func<,,,,,,,>);
                typeArray9[0x19] = typeof(Func<,,,,,,,,>);
                typeArray9[0x1a] = typeof(Func<,,,,,,,,,>);
                typeArray9[0x1b] = typeof(Func<,,,,,,,,,,>);
                typeArray9[0x1c] = typeof(Func<,,,,,,,,,,,>);
                typeArray9[0x1d] = typeof(Func<,,,,,,,,,,,,>);
                typeArray9[30] = typeof(Func<,,,,,,,,,,,,,>);
                typeArray9[0x1f] = typeof(Func<,,,,,,,,,,,,,,>);
                typeArray9[0x20] = typeof(Func<,,,,,,,,,,,,,,,>);
                typeArray9[0x21] = typeof(Func<,,,,,,,,,,,,,,,,>);
                batch.AllowTypes(MyWhitelistTarget.Both, typeArray9);
            }
        }

        private static IEnumerable<MemberInfo> AllDeclaredMembers(Type type) => 
            (from m in type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                where !IsPropertyMethod(m)
                select m);

        public void Clear()
        {
            this.m_whitelist.Clear();
            this.m_ingameBlacklist.Clear();
        }

        private CSharpCompilation CreateCompilation() => 
            this.m_scriptCompiler.CreateCompilation(null, null, false);

        public HashSetReader<string> GetBlacklistedIngameEntries() => 
            this.m_ingameBlacklist;

        public DictionaryReader<string, MyWhitelistTarget> GetWhitelist() => 
            new DictionaryReader<string, MyWhitelistTarget>(this.m_whitelist);

        private bool IsBlacklisted(ISymbol symbol)
        {
            if (symbol.IsMemberSymbol())
            {
                if (this.m_ingameBlacklist.Contains(symbol.GetWhitelistKey(TypeKeyQuantity.ThisOnly)))
                {
                    return true;
                }
                symbol = symbol.ContainingType;
            }
            for (ITypeSymbol symbol2 = symbol as ITypeSymbol; symbol2 != null; symbol2 = symbol2.ContainingType)
            {
                if (this.m_ingameBlacklist.Contains(symbol2.GetWhitelistKey(TypeKeyQuantity.AllMembers)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsMemberWhitelisted(ISymbol memberSymbol, MyWhitelistTarget target)
        {
            do
            {
                MyWhitelistTarget target2;
                if ((target == MyWhitelistTarget.Ingame) && this.IsBlacklisted(memberSymbol))
                {
                    return false;
                }
                if (this.IsWhitelisted(memberSymbol.ContainingType, target) == TypeKeyQuantity.AllMembers)
                {
                    return true;
                }
                if (this.m_whitelist.TryGetValue(memberSymbol.GetWhitelistKey(TypeKeyQuantity.ThisOnly), out target2) && target2.HasFlag(target))
                {
                    return true;
                }
                if (!memberSymbol.IsOverride)
                {
                    break;
                }
                ISymbol overriddenSymbol = memberSymbol.GetOverriddenSymbol();
                memberSymbol = overriddenSymbol;
            }
            while (memberSymbol != null);
            return false;
        }

        private static bool IsPropertyMethod(MemberInfo memberInfo)
        {
            MethodInfo info = memberInfo as MethodInfo;
            return ((info != null) && (info.IsSpecialName && (info.Name.StartsWith("get_") || info.Name.StartsWith("set_"))));
        }

        private TypeKeyQuantity IsWhitelisted(INamedTypeSymbol typeSymbol, MyWhitelistTarget target)
        {
            MyWhitelistTarget target2;
            if ((target == MyWhitelistTarget.Ingame) && this.IsBlacklisted(typeSymbol))
            {
                return TypeKeyQuantity.None;
            }
            TypeKeyQuantity quantity = this.IsWhitelisted(typeSymbol.ContainingNamespace, target);
            if (quantity == TypeKeyQuantity.AllMembers)
            {
                return quantity;
            }
            if (this.m_whitelist.TryGetValue(((ITypeSymbol) typeSymbol).GetWhitelistKey(TypeKeyQuantity.AllMembers), out target2) && target2.HasFlag(target))
            {
                return TypeKeyQuantity.AllMembers;
            }
            if (!this.m_whitelist.TryGetValue(((ITypeSymbol) typeSymbol).GetWhitelistKey(TypeKeyQuantity.ThisOnly), out target2) || !target2.HasFlag(target))
            {
                return TypeKeyQuantity.None;
            }
            return TypeKeyQuantity.ThisOnly;
        }

        private TypeKeyQuantity IsWhitelisted(INamespaceSymbol namespaceSymbol, MyWhitelistTarget target)
        {
            MyWhitelistTarget target2;
            if (!this.m_whitelist.TryGetValue(namespaceSymbol.GetWhitelistKey(TypeKeyQuantity.AllMembers), out target2) || !target2.HasFlag(target))
            {
                return TypeKeyQuantity.None;
            }
            return TypeKeyQuantity.AllMembers;
        }

        internal bool IsWhitelisted(ISymbol symbol, MyWhitelistTarget target)
        {
            INamedTypeSymbol typeSymbol = symbol as INamedTypeSymbol;
            return ((typeSymbol == null) ? (!symbol.IsMemberSymbol() || this.IsMemberWhitelisted(symbol, target)) : (this.IsWhitelisted(typeSymbol, target) != TypeKeyQuantity.None));
        }

        public IMyWhitelistBatch OpenBatch() => 
            new MyWhitelistBatch(this);

        public IMyScriptBlacklistBatch OpenIngameBlacklistBatch() => 
            new MyScriptBlacklistBatch(this);

        private void Register(MyWhitelistTarget target, INamespaceSymbol symbol, Type type)
        {
            string whitelistKey = symbol.GetWhitelistKey(TypeKeyQuantity.AllMembers);
            if (!this.m_whitelist.ContainsKey(whitelistKey))
            {
                this.m_whitelist.Add(whitelistKey, target);
            }
            else
            {
                object[] objArray1 = new object[] { "Duplicate registration of the whitelist key ", whitelistKey, " retrieved from ", type };
                throw new MyWhitelistException(string.Concat(objArray1));
            }
        }

        private void Register(MyWhitelistTarget target, ITypeSymbol symbol, Type type)
        {
            INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
            if ((containingNamespace != null) && !containingNamespace.IsGlobalNamespace)
            {
                MyWhitelistTarget target2;
                string key = containingNamespace.GetWhitelistKey(TypeKeyQuantity.AllMembers);
                if (this.m_whitelist.TryGetValue(key, out target2) && (target2 >= target))
                {
                    object[] objArray1 = new object[] { "The type ", type, " is covered by the ", key, " rule" };
                    throw new MyWhitelistException(string.Concat(objArray1));
                }
            }
            string whitelistKey = symbol.GetWhitelistKey(TypeKeyQuantity.AllMembers);
            if (!this.m_whitelist.ContainsKey(whitelistKey))
            {
                this.m_whitelist.Add(whitelistKey, target);
            }
            else
            {
                object[] objArray2 = new object[] { "Duplicate registration of the whitelist key ", whitelistKey, " retrieved from ", type };
                throw new MyWhitelistException(string.Concat(objArray2));
            }
        }

        private void RegisterMember(MyWhitelistTarget target, ISymbol symbol, MemberInfo member)
        {
            if ((!(symbol is IEventSymbol) && (!(symbol is IFieldSymbol) && !(symbol is IPropertySymbol))) && !(symbol is IMethodSymbol))
            {
                throw new MyWhitelistException("Unsupported symbol type " + symbol);
            }
            INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
            if ((containingNamespace != null) && !containingNamespace.IsGlobalNamespace)
            {
                MyWhitelistTarget target2;
                string key = containingNamespace.GetWhitelistKey(TypeKeyQuantity.AllMembers);
                if (this.m_whitelist.TryGetValue(key, out target2) && (target2 >= target))
                {
                    object[] objArray1 = new object[] { "The member ", member, " is covered by the ", key, " rule" };
                    throw new MyWhitelistException(string.Concat(objArray1));
                }
            }
            for (INamedTypeSymbol symbol3 = symbol.ContainingType; symbol3 != null; symbol3 = symbol3.ContainingType)
            {
                MyWhitelistTarget target3;
                string key = ((ITypeSymbol) symbol3).GetWhitelistKey(TypeKeyQuantity.AllMembers);
                if (this.m_whitelist.TryGetValue(key, out target3) && (target3 >= target))
                {
                    object[] objArray2 = new object[] { "The member ", member, " is covered by the ", key, " rule" };
                    throw new MyWhitelistException(string.Concat(objArray2));
                }
                key = ((ITypeSymbol) symbol3).GetWhitelistKey(TypeKeyQuantity.ThisOnly);
                if (!this.m_whitelist.TryGetValue(key, out target3) || (target3 < target))
                {
                    this.m_whitelist[key] = target;
                }
            }
            string whitelistKey = symbol.GetWhitelistKey(TypeKeyQuantity.ThisOnly);
            if (!this.m_whitelist.ContainsKey(whitelistKey))
            {
                this.m_whitelist.Add(whitelistKey, target);
            }
            else
            {
                object[] objArray3 = new object[] { "Duplicate registration of the whitelist key ", whitelistKey, " retrieved from ", member };
                throw new MyWhitelistException(string.Concat(objArray3));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyScriptWhitelist.<>c <>9 = new MyScriptWhitelist.<>c();
            public static Func<Type, bool> <>9__3_0;
            public static Func<Type, bool> <>9__3_1;
            public static Func<MemberInfo, bool> <>9__3_2;
            public static Func<MemberInfo, bool> <>9__4_0;

            internal bool <.ctor>b__3_0(Type x) => 
                (x.Namespace == "System.Linq");

            internal bool <.ctor>b__3_1(Type x) => 
                !x.Name.Contains("parallel", StringComparison.InvariantCultureIgnoreCase);

            internal bool <.ctor>b__3_2(MemberInfo m) => 
                (m.Name != "CreateDelegate");

            internal bool <AllDeclaredMembers>b__4_0(MemberInfo m) => 
                !MyScriptWhitelist.IsPropertyMethod(m);
        }

        private abstract class Batch : IDisposable
        {
            private readonly Dictionary<string, IAssemblySymbol> m_assemblyMap;
            private bool m_isDisposed;

            protected Batch(MyScriptWhitelist whitelist)
            {
                this.Whitelist = whitelist;
                CSharpCompilation compilation = this.Whitelist.CreateCompilation();
                this.m_assemblyMap = compilation.get_References().Select<MetadataReference, ISymbol>(new Func<MetadataReference, ISymbol>(compilation.GetAssemblyOrModuleSymbol)).OfType<IAssemblySymbol>().ToDictionary<IAssemblySymbol, string>(symbol => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            }

            [DebuggerNonUserCode]
            protected void AssertVitality()
            {
                if (this.m_isDisposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
            }

            public void Dispose()
            {
                if (!this.m_isDisposed)
                {
                    this.m_isDisposed = true;
                    this.OnDispose();
                    GC.SuppressFinalize(this);
                }
            }

            ~Batch()
            {
                this.m_isDisposed = true;
            }

            protected virtual void OnDispose()
            {
            }

            protected INamedTypeSymbol ResolveTypeSymbol(Type type)
            {
                IAssemblySymbol symbol;
                if (!this.m_assemblyMap.TryGetValue(type.Assembly.FullName, out symbol))
                {
                    throw new MyWhitelistException($"Cannot add {type.FullName} to the batch because {type.Assembly.FullName} has not been added to the compiler.");
                }
                INamedTypeSymbol typeByMetadataName = symbol.GetTypeByMetadataName(type.FullName);
                if (typeByMetadataName == null)
                {
                    throw new MyWhitelistException($"Cannot add {type.FullName} to the batch because its symbol variant could not be found.");
                }
                return typeByMetadataName;
            }

            protected MyScriptWhitelist Whitelist { get; private set; }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyScriptWhitelist.Batch.<>c <>9 = new MyScriptWhitelist.Batch.<>c();
                public static Func<IAssemblySymbol, string> <>9__2_0;

                internal string <.ctor>b__2_0(IAssemblySymbol symbol) => 
                    symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
        }

        private class MyScriptBlacklistBatch : MyScriptWhitelist.Batch, IMyScriptBlacklistBatch, IDisposable
        {
            public MyScriptBlacklistBatch(MyScriptWhitelist whitelist) : base(whitelist)
            {
            }

            public void AddMembers(Type type, params string[] memberNames)
            {
                if (type == null)
                {
                    throw new MyWhitelistException("Must specify the target type");
                }
                if (memberNames.IsNullOrEmpty<string>())
                {
                    throw new MyWhitelistException("Needs at least one member name");
                }
                base.AssertVitality();
                List<string> members = new List<string>();
                this.GetMemberWhitelistKeys(type, memberNames, members);
                for (int i = 0; i < members.Count; i++)
                {
                    string item = members[i];
                    base.Whitelist.m_ingameBlacklist.Add(item);
                }
            }

            public void AddNamespaceOfTypes(params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamespaceSymbol containingNamespace = base.ResolveTypeSymbol(type).ContainingNamespace;
                    if ((containingNamespace != null) && !containingNamespace.IsGlobalNamespace)
                    {
                        base.Whitelist.m_ingameBlacklist.Add(containingNamespace.GetWhitelistKey(TypeKeyQuantity.AllMembers));
                    }
                }
            }

            public void AddTypes(params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamedTypeSymbol symbol = base.ResolveTypeSymbol(type);
                    base.Whitelist.m_ingameBlacklist.Add(((ITypeSymbol) symbol).GetWhitelistKey(TypeKeyQuantity.AllMembers));
                }
            }

            private void GetMemberWhitelistKeys(Type type, string[] memberNames, List<string> members)
            {
                INamedTypeSymbol symbol = base.ResolveTypeSymbol(type);
                int index = 0;
                while (index < memberNames.Length)
                {
                    string str = memberNames[index];
                    int count = members.Count;
                    System.Collections.Immutable.ImmutableArray<ISymbol>.Enumerator enumerator = symbol.GetMembers().GetEnumerator();
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            if (count == members.Count)
                            {
                                throw new MyWhitelistException("Cannot find any members named " + str);
                            }
                            index++;
                            break;
                        }
                        ISymbol current = enumerator.Current;
                        if (current.Name == str)
                        {
                            Accessibility declaredAccessibility = current.DeclaredAccessibility;
                            if ((declaredAccessibility == Accessibility.Protected) || ((declaredAccessibility - 5) <= Accessibility.Private))
                            {
                                members.Add(current.GetWhitelistKey(TypeKeyQuantity.ThisOnly));
                            }
                        }
                    }
                }
            }

            public void RemoveMembers(Type type, params string[] memberNames)
            {
                if (type == null)
                {
                    throw new MyWhitelistException("Must specify the target type");
                }
                if (memberNames.IsNullOrEmpty<string>())
                {
                    throw new MyWhitelistException("Needs at least one member name");
                }
                base.AssertVitality();
                List<string> members = new List<string>();
                this.GetMemberWhitelistKeys(type, memberNames, members);
                for (int i = 0; i < members.Count; i++)
                {
                    string item = members[i];
                    base.Whitelist.m_ingameBlacklist.Remove(item);
                }
            }

            public void RemoveNamespaceOfTypes(params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamespaceSymbol containingNamespace = base.ResolveTypeSymbol(type).ContainingNamespace;
                    if ((containingNamespace != null) && !containingNamespace.IsGlobalNamespace)
                    {
                        base.Whitelist.m_ingameBlacklist.Remove(containingNamespace.GetWhitelistKey(TypeKeyQuantity.AllMembers));
                    }
                }
            }

            public void RemoveTypes(params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamedTypeSymbol symbol = base.ResolveTypeSymbol(type);
                    base.Whitelist.m_ingameBlacklist.Remove(((ITypeSymbol) symbol).GetWhitelistKey(TypeKeyQuantity.AllMembers));
                }
            }
        }

        private class MyWhitelistBatch : MyScriptWhitelist.Batch, IMyWhitelistBatch, IDisposable
        {
            public MyWhitelistBatch(MyScriptWhitelist whitelist) : base(whitelist)
            {
            }

            public void AllowMembers(MyWhitelistTarget target, params MemberInfo[] members)
            {
                if (members.IsNullOrEmpty<MemberInfo>())
                {
                    throw new MyWhitelistException("Needs at least one member");
                }
                base.AssertVitality();
                for (int i = 0; i < members.Length; i++)
                {
                    MemberInfo member = members[i];
                    if (member == null)
                    {
                        throw new MyWhitelistException("Element " + i + " is null");
                    }
                    List<ISymbol> candidates = System.Linq.ImmutableArrayExtensions.Where<ISymbol>(base.ResolveTypeSymbol(member.DeclaringType).GetMembers(), m => m.MetadataName == member.Name).ToList<ISymbol>();
                    MethodInfo info = member as MethodInfo;
                    System.Reflection.ParameterInfo[] methodParameters = null;
                    if (info != null)
                    {
                        methodParameters = info.GetParameters();
                        candidates.RemoveAll(s => ((IMethodSymbol) s).Parameters.Length != methodParameters.Length);
                        if (info.IsGenericMethodDefinition)
                        {
                            candidates.RemoveAll(s => !((IMethodSymbol) s).IsGenericMethod);
                        }
                        else
                        {
                            candidates.RemoveAll(s => ((IMethodSymbol) s).IsGenericMethod);
                        }
                        if (info.IsSpecialName && (info.Name.StartsWith("get_") || info.Name.StartsWith("set_")))
                        {
                            throw new MyWhitelistException("Whitelist the actual properties, not their access methods");
                        }
                    }
                    int count = candidates.Count;
                    if (count == 0)
                    {
                        throw new MyWhitelistException($"Cannot add {member} to the whitelist because its symbol variant could not be found.");
                    }
                    if (count == 1)
                    {
                        base.Whitelist.RegisterMember(target, candidates[0], member);
                    }
                    else
                    {
                        IMethodSymbol symbol = this.FindMethodOverload(candidates, methodParameters);
                        if (symbol == null)
                        {
                            throw new MyWhitelistException($"Cannot add {member} to the whitelist because its symbol variant could not be found.");
                        }
                        base.Whitelist.RegisterMember(target, symbol, member);
                    }
                }
            }

            public void AllowNamespaceOfTypes(MyWhitelistTarget target, params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamespaceSymbol containingNamespace = base.ResolveTypeSymbol(type).ContainingNamespace;
                    if ((containingNamespace != null) && !containingNamespace.IsGlobalNamespace)
                    {
                        base.Whitelist.Register(target, containingNamespace, type);
                    }
                }
            }

            public void AllowTypes(MyWhitelistTarget target, params Type[] types)
            {
                if (types.IsNullOrEmpty<Type>())
                {
                    throw new MyWhitelistException("Needs at least one type");
                }
                base.AssertVitality();
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null)
                    {
                        throw new MyWhitelistException("The type in index " + i + " is null");
                    }
                    INamedTypeSymbol symbol = base.ResolveTypeSymbol(type);
                    base.Whitelist.Register(target, symbol, type);
                }
            }

            private IMethodSymbol FindMethodOverload(IEnumerable<ISymbol> candidates, System.Reflection.ParameterInfo[] methodParameters)
            {
                using (IEnumerator<ISymbol> enumerator = candidates.GetEnumerator())
                {
                    IMethodSymbol current;
                    bool flag;
                    goto TR_001D;
                TR_0005:
                    if (flag)
                    {
                        return current;
                    }
                TR_001D:
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        current = (IMethodSymbol) enumerator.Current;
                        System.Collections.Immutable.ImmutableArray<IParameterSymbol> parameters = current.Parameters;
                        flag = true;
                        int index = 0;
                        while (index < parameters.Length)
                        {
                            System.Reflection.ParameterInfo info1 = methodParameters[index];
                            Type parameterType = info1.ParameterType;
                            IParameterSymbol symbol2 = parameters[index];
                            ITypeSymbol type = symbol2.Type;
                            if (info1.IsOut && (symbol2.RefKind != RefKind.Out))
                            {
                                flag = false;
                            }
                            else
                            {
                                if (parameterType.IsByRef)
                                {
                                    if (symbol2.RefKind != RefKind.Ref)
                                    {
                                        flag = false;
                                        break;
                                    }
                                    parameterType = parameterType.GetElementType();
                                }
                                if (parameterType.IsPointer)
                                {
                                    if (!(type is IPointerTypeSymbol))
                                    {
                                        flag = false;
                                        break;
                                    }
                                    type = ((IPointerTypeSymbol) type).PointedAtType;
                                    parameterType = parameterType.GetElementType();
                                }
                                if (parameterType.IsArray)
                                {
                                    if (!(type is IArrayTypeSymbol))
                                    {
                                        flag = false;
                                        break;
                                    }
                                    type = ((IArrayTypeSymbol) type).ElementType;
                                    parameterType = parameterType.GetElementType();
                                }
                                if (Equals(base.ResolveTypeSymbol(parameterType), type))
                                {
                                    index++;
                                    continue;
                                }
                                flag = false;
                            }
                            break;
                        }
                        goto TR_0005;
                    }
                }
                return null;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly MyScriptWhitelist.MyWhitelistBatch.<>c <>9 = new MyScriptWhitelist.MyWhitelistBatch.<>c();
                public static Predicate<ISymbol> <>9__3_2;
                public static Predicate<ISymbol> <>9__3_3;

                internal bool <AllowMembers>b__3_2(ISymbol s) => 
                    !((IMethodSymbol) s).IsGenericMethod;

                internal bool <AllowMembers>b__3_3(ISymbol s) => 
                    ((IMethodSymbol) s).IsGenericMethod;
            }
        }
    }
}

