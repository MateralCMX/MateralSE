namespace Sandbox.Game.GameSystems.Chat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using VRage.Game.ModAPI;
    using VRage.Utils;

    public class MyChatCommandSystem
    {
        public Dictionary<string, IMyChatCommand> ChatCommands = new Dictionary<string, IMyChatCommand>();
        private static char[] m_separators = new char[] { ' ', '\r', '\n' };

        public MyChatCommandSystem()
        {
            this.ScanAssemblyForCommands(Assembly.GetExecutingAssembly());
        }

        public bool CanHandle(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }
            string[] strArray = message.Split(m_separators, 2, StringSplitOptions.RemoveEmptyEntries);
            return ((strArray.Length != 0) ? this.ChatCommands.ContainsKey(strArray[0]) : false);
        }

        public void Handle(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                IMyChatCommand command;
                string[] strArray = message.Split(m_separators, 2, StringSplitOptions.RemoveEmptyEntries);
                if (this.ChatCommands.TryGetValue(strArray[0], out command))
                {
                    command.Handle(ParseCommand(command, message));
                }
            }
        }

        public static string[] ParseCommand(IMyChatCommand command, string input)
        {
            if (input.Length <= (command.CommandText.Length + 1))
            {
                return null;
            }
            string str = input.Substring(command.CommandText.Length + 1);
            MatchCollection matchs = Regex.Matches(str, "(\"[^\"]+\"|\\S+)");
            string[] strArray = new string[matchs.Count];
            for (int i = 0; i < strArray.Length; i++)
            {
                strArray[i] = matchs[i].Value;
            }
            return strArray;
        }

        public void ScanAssemblyForCommands(Assembly assembly)
        {
            foreach (TypeInfo info in assembly.DefinedTypes)
            {
                if (info.ImplementedInterfaces.Contains<Type>(typeof(IMyChatCommand)))
                {
                    if (info == typeof(MyChatCommand))
                    {
                        continue;
                    }
                    IMyChatCommand command = (IMyChatCommand) Activator.CreateInstance(info);
                    this.ChatCommands.Add(command.CommandText, command);
                    continue;
                }
                foreach (MethodInfo info2 in info.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    ChatCommandAttribute customAttribute = info2.GetCustomAttribute<ChatCommandAttribute>();
                    if ((customAttribute != null) && !customAttribute.DebugCommand)
                    {
                        Action<string[]> action = info2.CreateDelegate<Action<string[]>>();
                        if (action == null)
                        {
                            MyLog.Default.WriteLine("Error creating delegate from " + info.FullName + "." + info2.Name);
                        }
                        else
                        {
                            this.ChatCommands.Add(customAttribute.CommandText, new MyChatCommand(customAttribute.CommandText, customAttribute.HelpText, customAttribute.HelpSimpleText, action, MyPromoteLevel.None));
                        }
                    }
                }
            }
        }
    }
}

