namespace VRage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MyCommandHandler
    {
        private Dictionary<string, MyCommand> m_commands = new Dictionary<string, MyCommand>();

        public void AddCommand(MyCommand command)
        {
            if (this.m_commands.ContainsKey(command.Prefix()))
            {
                this.m_commands.Remove(command.Prefix());
            }
            this.m_commands.Add(command.Prefix(), command);
        }

        public bool ContainsCommand(string command) => 
            this.m_commands.ContainsKey(command);

        public string GetCommandKey(string input) => 
            (input.Contains(".") ? input.Substring(0, input.IndexOf(".")) : null);

        public string GetCommandMethod(string input)
        {
            try
            {
                return input.Substring(input.IndexOf(".") + 1);
            }
            catch
            {
                return null;
            }
        }

        public StringBuilder Handle(string input)
        {
            MyCommand command;
            List<string> args = this.SplitArgs(input);
            if (args.Count <= 0)
            {
                return new StringBuilder("Error: Empty string");
            }
            string str = args[0];
            string commandKey = this.GetCommandKey(str);
            if (commandKey == null)
            {
                return new StringBuilder().AppendFormat("Error: Invalid method syntax '{0}'", input);
            }
            args.RemoveAt(0);
            if (!this.m_commands.TryGetValue(commandKey, out command))
            {
                return new StringBuilder().AppendFormat("Error: Unknown command {0}\n", commandKey);
            }
            string commandMethod = this.GetCommandMethod(str);
            if (commandMethod == null)
            {
                return new StringBuilder().AppendFormat("Error: Invalid method syntax '{0}'", input);
            }
            if (commandMethod == "")
            {
                return new StringBuilder("Error: Empty Method");
            }
            try
            {
                return new StringBuilder().Append(commandKey).Append(".").Append(commandMethod).Append(": ").Append(command.Execute(commandMethod, args));
            }
            catch (MyConsoleInvalidArgumentsException)
            {
                return new StringBuilder().AppendFormat("Error: Invalid Argument for method {0}.{1}", commandKey, commandMethod);
            }
            catch (MyConsoleMethodNotFoundException)
            {
                return new StringBuilder().AppendFormat("Error: Command {0} does not contain method {1}", commandKey, commandMethod);
            }
        }

        public void RemoveAllCommands()
        {
            this.m_commands.Clear();
        }

        public List<string> SplitArgs(string input)
        {
            char[] separator = new char[] { '"' };
            return (from element in input.Split(separator).Select<string, string[]>(delegate (string element, int index) {
                if ((index % 2) != 0)
                {
                    return new string[] { element };
                }
                char[] chArray1 = new char[] { ' ' };
                return element.Split(chArray1, StringSplitOptions.RemoveEmptyEntries);
            }) select element).ToList<string>();
        }

        public bool TryGetCommand(string commandName, out MyCommand command)
        {
            if (!this.m_commands.ContainsKey(commandName))
            {
                command = null;
                return false;
            }
            command = this.m_commands[commandName];
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyCommandHandler.<>c <>9 = new MyCommandHandler.<>c();
            public static Func<string, int, string[]> <>9__3_0;
            public static Func<string[], IEnumerable<string>> <>9__3_1;

            internal string[] <SplitArgs>b__3_0(string element, int index)
            {
                if ((index % 2) != 0)
                {
                    return new string[] { element };
                }
                char[] separator = new char[] { ' ' };
                return element.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            internal IEnumerable<string> <SplitArgs>b__3_1(string[] element) => 
                element;
        }
    }
}

