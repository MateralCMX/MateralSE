namespace Sandbox.Engine.AI
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Game;
    using VRage.Game.AI;
    using VRage.Game.ObjectBuilders.AI;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    [PreloadRequired]
    public static class MyAIActionsParser
    {
        private static bool ENABLE_PARSING = true;
        private static string SERIALIZE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MedievalEngineers", "BehaviorDescriptors.xml");

        static MyAIActionsParser()
        {
            bool flag1 = ENABLE_PARSING;
        }

        public static HashSet<Type> GetAllTypesFromAssemblies()
        {
            HashSet<Type> outputTypes = new HashSet<Type>();
            GetTypesFromAssembly(MyPlugins.SandboxGameAssembly, outputTypes);
            GetTypesFromAssembly(MyPlugins.GameAssembly, outputTypes);
            GetTypesFromAssembly(MyPlugins.UserAssemblies, outputTypes);
            return outputTypes;
        }

        private static void GetTypesFromAssembly(Assembly[] assemblies, HashSet<Type> outputTypes)
        {
            if (assemblies != null)
            {
                Assembly[] assemblyArray = assemblies;
                for (int i = 0; i < assemblyArray.Length; i++)
                {
                    GetTypesFromAssembly(assemblyArray[i], outputTypes);
                }
            }
        }

        private static void GetTypesFromAssembly(Assembly assembly, HashSet<Type> outputTypes)
        {
            if (assembly != null)
            {
                Type[] types = assembly.GetTypes();
                int index = 0;
                while (index < types.Length)
                {
                    Type item = types[index];
                    object[] customAttributes = item.GetCustomAttributes(false);
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= customAttributes.Length)
                        {
                            index++;
                            break;
                        }
                        if (customAttributes[num2] is MyBehaviorDescriptorAttribute)
                        {
                            outputTypes.Add(item);
                        }
                        num2++;
                    }
                }
            }
        }

        private static Dictionary<string, List<MethodInfo>> ParseMethods(HashSet<Type> types)
        {
            Dictionary<string, List<MethodInfo>> dictionary = new Dictionary<string, List<MethodInfo>>();
            foreach (Type local1 in types)
            {
                MyBehaviorDescriptorAttribute customAttribute = local1.GetCustomAttribute<MyBehaviorDescriptorAttribute>();
                foreach (MethodInfo info in local1.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    MyBehaviorTreeActionAttribute attribute2 = info.GetCustomAttribute<MyBehaviorTreeActionAttribute>();
                    if ((attribute2 != null) && (attribute2.ActionType == MyBehaviorTreeActionType.BODY))
                    {
                        bool flag = true;
                        ParameterInfo[] parameters = info.GetParameters();
                        int index = 0;
                        while (true)
                        {
                            if (index < parameters.Length)
                            {
                                ParameterInfo element = parameters[index];
                                BTParamAttribute attribute3 = element.GetCustomAttribute<BTParamAttribute>();
                                BTMemParamAttribute attribute4 = element.GetCustomAttribute<BTMemParamAttribute>();
                                if ((attribute3 != null) || (attribute4 != null))
                                {
                                    index++;
                                    continue;
                                }
                                flag = false;
                            }
                            if (flag)
                            {
                                List<MethodInfo> list = null;
                                if (!dictionary.TryGetValue(customAttribute.DescriptorCategory, out list))
                                {
                                    list = new List<MethodInfo>();
                                    dictionary[customAttribute.DescriptorCategory] = list;
                                }
                                list.Add(info);
                            }
                            break;
                        }
                    }
                }
            }
            return dictionary;
        }

        private static void SerializeToXML(string path, Dictionary<string, List<MethodInfo>> data)
        {
            MyAIBehaviorData objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyAIBehaviorData>();
            objectBuilder.Entries = new MyAIBehaviorData.CategorizedData[data.Count];
            int index = 0;
            foreach (KeyValuePair<string, List<MethodInfo>> pair in data)
            {
                MyAIBehaviorData.CategorizedData data3 = new MyAIBehaviorData.CategorizedData {
                    Category = pair.Key,
                    Descriptors = new MyAIBehaviorData.ActionData[pair.Value.Count]
                };
                int num2 = 0;
                foreach (MethodInfo local1 in pair.Value)
                {
                    MyAIBehaviorData.ActionData data4 = new MyAIBehaviorData.ActionData();
                    MyBehaviorTreeActionAttribute customAttribute = local1.GetCustomAttribute<MyBehaviorTreeActionAttribute>();
                    data4.ActionName = customAttribute.ActionName;
                    data4.ReturnsRunning = customAttribute.ReturnsRunning;
                    ParameterInfo[] parameters = local1.GetParameters();
                    data4.Parameters = new MyAIBehaviorData.ParameterData[parameters.Length];
                    int num3 = 0;
                    ParameterInfo[] infoArray2 = parameters;
                    int num4 = 0;
                    while (true)
                    {
                        if (num4 >= infoArray2.Length)
                        {
                            data3.Descriptors[num2] = data4;
                            num2++;
                            break;
                        }
                        ParameterInfo element = infoArray2[num4];
                        BTMemParamAttribute attribute2 = element.GetCustomAttribute<BTMemParamAttribute>();
                        BTParamAttribute attribute3 = element.GetCustomAttribute<BTParamAttribute>();
                        MyAIBehaviorData.ParameterData data5 = new MyAIBehaviorData.ParameterData {
                            Name = element.Name,
                            TypeFullName = element.ParameterType.FullName
                        };
                        if (attribute2 != null)
                        {
                            data5.MemType = attribute2.MemoryType;
                        }
                        else if (attribute3 != null)
                        {
                            data5.MemType = MyMemoryParameterType.PARAMETER;
                        }
                        data4.Parameters[num3] = data5;
                        num3++;
                        num4++;
                    }
                }
                objectBuilder.Entries[index] = data3;
                index++;
            }
            MyObjectBuilderSerializer.SerializeXML(path, false, objectBuilder, null);
        }
    }
}

