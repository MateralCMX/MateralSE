namespace VRage.Compiler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Timers;
    using System.Xml.Serialization;

    public class IlChecker
    {
        public static Dictionary<Type, HashSet<MemberInfo>> AllowedOperands = new Dictionary<Type, HashSet<MemberInfo>>();
        public static Dictionary<Assembly, HashSet<string>> AllowedNamespacesCommon = new Dictionary<Assembly, HashSet<string>>();
        public static Dictionary<Assembly, HashSet<string>> AllowedNamespacesModAPI = new Dictionary<Assembly, HashSet<string>>();

        static IlChecker()
        {
            if (AllowedOperands == null)
            {
                AllowedOperands = new Dictionary<Type, HashSet<MemberInfo>>();
            }
            AllowedOperands.Add(typeof(object), null);
            AllowedOperands.Add(typeof(IDisposable), null);
            AllowNamespaceOfTypeCommon(typeof(IEnumerator));
            AllowNamespaceOfTypeCommon(typeof(IEnumerable<>));
            AllowNamespaceOfTypeCommon(typeof(HashSet<>));
            AllowNamespaceOfTypeCommon(typeof(Queue<>));
            AllowNamespaceOfTypeCommon(typeof(ListExtensions));
            AllowNamespaceOfTypeCommon(typeof(Enumerable));
            AllowNamespaceOfTypeCommon(typeof(StringBuilder));
            AllowNamespaceOfTypeCommon(typeof(Regex));
            AllowNamespaceOfTypeModAPI(typeof(Timer));
            AllowNamespaceOfTypeCommon(typeof(Calendar));
            AllowedOperands.Add(typeof(StringBuilder), null);
            AllowedOperands.Add(typeof(string), null);
            AllowedOperands.Add(typeof(Math), null);
            AllowedOperands.Add(typeof(Enum), null);
            AllowedOperands.Add(typeof(int), null);
            AllowedOperands.Add(typeof(short), null);
            AllowedOperands.Add(typeof(long), null);
            AllowedOperands.Add(typeof(uint), null);
            AllowedOperands.Add(typeof(ushort), null);
            AllowedOperands.Add(typeof(ulong), null);
            AllowedOperands.Add(typeof(double), null);
            AllowedOperands.Add(typeof(float), null);
            AllowedOperands.Add(typeof(bool), null);
            AllowedOperands.Add(typeof(char), null);
            AllowedOperands.Add(typeof(byte), null);
            AllowedOperands.Add(typeof(sbyte), null);
            AllowedOperands.Add(typeof(decimal), null);
            AllowedOperands.Add(typeof(DateTime), null);
            AllowedOperands.Add(typeof(TimeSpan), null);
            AllowedOperands.Add(typeof(Array), null);
            AllowedOperands.Add(typeof(DateTimeOffset), null);
            AllowedOperands.Add(typeof(XmlElementAttribute), null);
            AllowedOperands.Add(typeof(XmlAttributeAttribute), null);
            AllowedOperands.Add(typeof(XmlArrayAttribute), null);
            AllowedOperands.Add(typeof(XmlArrayItemAttribute), null);
            AllowedOperands.Add(typeof(XmlAnyAttributeAttribute), null);
            AllowedOperands.Add(typeof(XmlAnyElementAttribute), null);
            AllowedOperands.Add(typeof(XmlAnyElementAttributes), null);
            AllowedOperands.Add(typeof(XmlArrayItemAttributes), null);
            AllowedOperands.Add(typeof(XmlAttributeEventArgs), null);
            AllowedOperands.Add(typeof(XmlAttributeOverrides), null);
            AllowedOperands.Add(typeof(XmlAttributes), null);
            AllowedOperands.Add(typeof(XmlChoiceIdentifierAttribute), null);
            AllowedOperands.Add(typeof(XmlElementAttributes), null);
            AllowedOperands.Add(typeof(XmlElementEventArgs), null);
            AllowedOperands.Add(typeof(XmlEnumAttribute), null);
            AllowedOperands.Add(typeof(XmlIgnoreAttribute), null);
            AllowedOperands.Add(typeof(XmlIncludeAttribute), null);
            AllowedOperands.Add(typeof(XmlRootAttribute), null);
            AllowedOperands.Add(typeof(XmlTextAttribute), null);
            AllowedOperands.Add(typeof(XmlTypeAttribute), null);
            HashSet<MemberInfo> set = new HashSet<MemberInfo> {
                typeof(MemberInfo).GetProperty("Name").GetGetMethod()
            };
            AllowedOperands.Add(typeof(MemberInfo), set);
            AllowedOperands.Add(typeof(RuntimeHelpers), null);
            AllowedOperands.Add(typeof(Stream), null);
            AllowedOperands.Add(typeof(TextWriter), null);
            AllowedOperands.Add(typeof(TextReader), null);
            AllowedOperands.Add(typeof(BinaryReader), null);
            AllowedOperands.Add(typeof(BinaryWriter), null);
            set = new HashSet<MemberInfo> {
                typeof(Type).GetMethod("GetTypeFromHandle")
            };
            AllowedOperands.Add(typeof(Type), set);
            Type type = typeof(Type).Assembly.GetType("System.RuntimeType");
            HashSet<MemberInfo> set2 = new HashSet<MemberInfo> {
                type.GetMethod("op_Inequality")
            };
            Type[] types = new Type[] { typeof(BindingFlags) };
            set2.Add(type.GetMethod("GetFields", types));
            AllowedOperands[type] = set2;
            set2 = new HashSet<MemberInfo>();
            Type[] typeArray2 = new Type[] { typeof(BindingFlags) };
            set2.Add(typeof(Type).GetMethod("GetFields", typeArray2));
            set2.Add(typeof(Type).GetMethod("IsEquivalentTo"));
            set2.Add(typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static));
            set2.Add(typeof(Type).GetMethod("op_Equality"));
            AllowedOperands[typeof(Type)] = set2;
            Type type2 = typeof(Type).Assembly.GetType("System.Reflection.RtFieldInfo");
            HashSet<MemberInfo> set1 = new HashSet<MemberInfo>();
            set1.Add(type2.GetMethod("UnsafeGetValue", BindingFlags.NonPublic | BindingFlags.Instance));
            AllowedOperands[type2] = set1;
            AllowedOperands[typeof(NullReferenceException)] = null;
            AllowedOperands[typeof(ArgumentException)] = null;
            AllowedOperands[typeof(ArgumentNullException)] = null;
            AllowedOperands[typeof(InvalidOperationException)] = null;
            AllowedOperands[typeof(FormatException)] = null;
            AllowedOperands.Add(typeof(Exception), null);
            AllowedOperands.Add(typeof(DivideByZeroException), null);
            AllowedOperands.Add(typeof(InvalidCastException), null);
            AllowedOperands.Add(typeof(FileNotFoundException), null);
            AllowedOperands.Add(typeof(NotSupportedException), null);
            Type type3 = typeof(MethodInfo).Assembly.GetType("System.Reflection.RuntimeMethodInfo");
            set2 = new HashSet<MemberInfo> {
                typeof(ValueType).GetMethod("Equals"),
                typeof(ValueType).GetMethod("GetHashCode"),
                typeof(ValueType).GetMethod("ToString"),
                typeof(ValueType).GetMethod("CanCompareBits", BindingFlags.NonPublic | BindingFlags.Static),
                typeof(ValueType).GetMethod("FastEqualsCheck", BindingFlags.NonPublic | BindingFlags.Static)
            };
            AllowedOperands[typeof(ValueType)] = set2;
            Type type4 = typeof(Environment);
            set2 = new HashSet<MemberInfo>();
            Type[] typeArray3 = new Type[] { typeof(string), typeof(object[]) };
            set2.Add(type4.GetMethod("GetResourceString", BindingFlags.NonPublic | BindingFlags.Static, null, typeArray3, null));
            Type[] typeArray4 = new Type[] { typeof(string) };
            set2.Add(type4.GetMethod("GetResourceString", BindingFlags.NonPublic | BindingFlags.Static, null, typeArray4, null));
            set2.Add(type4.GetProperty("CurrentManagedThreadId", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
            set2.Add(type4.GetProperty("NewLine", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
            set2.Add(type4.GetProperty("ProcessorCount", BindingFlags.Public | BindingFlags.Static).GetGetMethod());
            AllowedOperands[type4] = set2;
            AllowedOperands[typeof(Path)] = null;
            AllowedOperands[typeof(Random)] = null;
            AllowedOperands[typeof(Convert)] = null;
            AllowedOperands.Add(typeof(Nullable<>), null);
            AllowedOperands.Add(typeof(StringComparer), null);
            AllowedOperands.Add(typeof(IComparable<>), null);
            AllowedOperands.Add(typeof(BitConverter), null);
            AllowedOperands.Add(typeof(FlagsAttribute), null);
        }

        private static bool AddNamespaceOfTypeToDictionary(Type type, Dictionary<Assembly, HashSet<string>> targetDictionary)
        {
            if (!IsTypeValid(type))
            {
                return false;
            }
            if (targetDictionary == null)
            {
                return false;
            }
            if (!targetDictionary.ContainsKey(type.Assembly) || (targetDictionary[type.Assembly] == null))
            {
                targetDictionary.Add(type.Assembly, new HashSet<string>());
            }
            targetDictionary[type.Assembly].Add(type.Namespace);
            return true;
        }

        public static bool AllowNamespaceOfTypeCommon(Type type)
        {
            if (AllowedNamespacesCommon == null)
            {
                AllowedNamespacesCommon = new Dictionary<Assembly, HashSet<string>>();
            }
            return AddNamespaceOfTypeToDictionary(type, AllowedNamespacesCommon);
        }

        public static bool AllowNamespaceOfTypeModAPI(Type type)
        {
            if (AllowedNamespacesModAPI == null)
            {
                AllowedNamespacesModAPI = new Dictionary<Assembly, HashSet<string>>();
            }
            return AddNamespaceOfTypeToDictionary(type, AllowedNamespacesModAPI);
        }

        private static bool CheckGenericType(Type declType, MemberInfo memberInfo, bool isIngameScript)
        {
            if (!CheckTypeAndMember(declType, isIngameScript, memberInfo))
            {
                return false;
            }
            if (memberInfo != null)
            {
                foreach (Type type in memberInfo.DeclaringType.GetGenericArguments())
                {
                    if (!type.IsGenericParameter && !CheckTypeAndMember(type, isIngameScript, null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool CheckIl(List<IlReader.IlInstruction> instructions, out Type failed, bool isIngameScript, Dictionary<Type, HashSet<MemberInfo>> allowedTypes = null)
        {
            failed = null;
            foreach (KeyValuePair<Type, HashSet<MemberInfo>> pair in allowedTypes)
            {
                if (!AllowedOperands.Contains<KeyValuePair<Type, HashSet<MemberInfo>>>(pair))
                {
                    AllowedOperands.Add(pair.Key, pair.Value);
                }
            }
            using (List<IlReader.IlInstruction>.Enumerator enumerator2 = instructions.GetEnumerator())
            {
                while (true)
                {
                    bool flag;
                    if (!enumerator2.MoveNext())
                    {
                        break;
                    }
                    IlReader.IlInstruction current = enumerator2.Current;
                    MethodInfo operand = current.Operand as MethodInfo;
                    if ((operand != null) && HasMethodInvalidAtrributes(operand.Attributes))
                    {
                        flag = false;
                    }
                    else
                    {
                        if (CheckMember(current.Operand as MemberInfo, isIngameScript) && (current.OpCode != OpCodes.Calli))
                        {
                            continue;
                        }
                        failed = ((MemberInfo) current.Operand).DeclaringType;
                        flag = false;
                    }
                    return flag;
                }
            }
            return true;
        }

        private static bool CheckMember(MemberInfo memberInfo, bool isIngameScript) => 
            ((memberInfo != null) ? CheckTypeAndMember(memberInfo.DeclaringType, isIngameScript, memberInfo) : true);

        private static bool CheckNamespace(Type type, bool isIngameScript)
        {
            if (type == null)
            {
                return false;
            }
            bool flag = AllowedNamespacesCommon.ContainsKey(type.Assembly) && AllowedNamespacesCommon[type.Assembly].Contains(type.Namespace);
            if (!flag && !isIngameScript)
            {
                flag = AllowedNamespacesModAPI.ContainsKey(type.Assembly) && AllowedNamespacesModAPI[type.Assembly].Contains(type.Namespace);
            }
            return flag;
        }

        private static bool CheckOperand(Type type, MemberInfo memberInfo, Dictionary<Type, HashSet<MemberInfo>> op) => 
            ((op != null) ? (op.ContainsKey(type) && ((memberInfo == null) || ((op[type] == null) || op[type].Contains(memberInfo)))) : false);

        public static bool CheckTypeAndMember(Type type, bool isIngameScript, MemberInfo memberInfo = null)
        {
            if (type != null)
            {
                if (IsDelegate(type))
                {
                    return true;
                }
                if ((type.IsGenericTypeDefinition || !type.IsGenericType) || !CheckGenericType(type.GetGenericTypeDefinition(), memberInfo, isIngameScript))
                {
                    return (CheckNamespace(type, isIngameScript) || CheckOperand(type, memberInfo, AllowedOperands));
                }
            }
            return true;
        }

        public static void Clear()
        {
            AllowedOperands.Clear();
            AllowedNamespacesCommon.Clear();
            AllowedNamespacesModAPI.Clear();
        }

        public static bool HasMethodInvalidAtrributes(MethodAttributes Attributes) => 
            ((Attributes & (MethodAttributes.PinvokeImpl | MethodAttributes.UnmanagedExport)) != MethodAttributes.PrivateScope);

        private static bool IsDelegate(Type type)
        {
            Type type2 = typeof(MulticastDelegate);
            return (type2.IsAssignableFrom(type.BaseType) || ((type == type2) || (type == type2.BaseType)));
        }

        public static bool IsMethodFromParent(Type classType, MethodBase method) => 
            classType.IsSubclassOf(method.DeclaringType);

        private static bool IsTypeValid(Type type) => 
            ((type != null) ? ((type.Assembly != null) ? (type.Namespace != null) : false) : false);
    }
}

