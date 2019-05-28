namespace VRage.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    public static class MyEnumDuplicitiesTester
    {
        private const string m_keenSWHCompanyName = "Keen Software House";

        private static void AssertEnumNotDuplicities(Type enumType, HashSet<object> hashSet)
        {
            hashSet.Clear();
            foreach (object obj2 in Enum.GetValues(enumType))
            {
                if (!hashSet.Add(obj2))
                {
                    object[] objArray1 = new object[] { "Duplicate enum found: ", obj2, " in ", enumType.AssemblyQualifiedName };
                    throw new Exception(string.Concat(objArray1));
                }
            }
        }

        private static void CheckEnumNotDuplicities(string companyName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            List<Assembly> list = new List<Assembly>(assemblies.Length + files.Length);
            foreach (Assembly assembly in assemblies)
            {
                if ((companyName == null) || (GetCompanyNameOfAssembly(assembly) == companyName))
                {
                    list.Add(assembly);
                }
            }
            foreach (string str in files)
            {
                if (!IsLoaded(assemblies, str) && ((companyName == null) || (FileVersionInfo.GetVersionInfo(str).CompanyName == companyName)))
                {
                    list.Add(Assembly.LoadFrom(str));
                }
            }
            HashSet<object> hashSet = new HashSet<object>();
            using (List<Assembly>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TestEnumNotDuplicitiesInAssembly(enumerator.Current, hashSet);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void CheckEnumNotDuplicitiesInRunningApplication()
        {
            CheckEnumNotDuplicities("Keen Software House");
        }

        private static string GetCompanyNameOfAssembly(Assembly assembly)
        {
            AssemblyCompanyAttribute attribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false) as AssemblyCompanyAttribute;
            return ((attribute != null) ? attribute.Company : string.Empty);
        }

        private static bool IsLoaded(Assembly[] assemblies, string assemblyPath)
        {
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.IsDynamic || (!string.IsNullOrEmpty(assembly.Location) && (Path.GetFullPath(assembly.Location) == assemblyPath)))
                {
                    return true;
                }
            }
            return false;
        }

        private static void TestEnumNotDuplicitiesInAssembly(Assembly assembly, HashSet<object> hashSet)
        {
        }
    }
}

