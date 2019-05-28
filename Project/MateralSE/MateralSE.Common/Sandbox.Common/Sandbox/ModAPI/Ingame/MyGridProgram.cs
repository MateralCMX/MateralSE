namespace Sandbox.ModAPI.Ingame
{
    using Sandbox.ModAPI;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public abstract class MyGridProgram : IMyGridProgram
    {
        private string m_storage;
        private readonly Action<string, UpdateType> m_main;
        private readonly Action m_save;
        private Func<IMyIntergridCommunicationSystem> m_IGC_ContextGetter;

        protected MyGridProgram()
        {
            Type type = base.GetType();
            Type[] types = new Type[] { typeof(string), typeof(UpdateType) };
            MethodInfo method = type.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, types, null);
            if (method != null)
            {
                this.m_main = method.CreateDelegate<Action<string, UpdateType>>(this);
            }
            else
            {
                Type[] typeArray2 = new Type[] { typeof(string) };
                method = type.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, typeArray2, null);
                if (method != null)
                {
                    Action<string> main = method.CreateDelegate<Action<string>>(this);
                    this.m_main = (arg, source) => main(arg);
                }
                else
                {
                    method = type.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (method != null)
                    {
                        Action mainWithoutArgument = method.CreateDelegate<Action>(this);
                        this.m_main = (arg, source) => mainWithoutArgument();
                    }
                }
            }
            MethodInfo info2 = type.GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (info2 != null)
            {
                this.m_save = info2.CreateDelegate<Action>(this);
            }
        }

        [Obsolete]
        void IMyGridProgram.Main(string argument)
        {
            if (this.m_main == null)
            {
                throw new InvalidOperationException("No Main method available");
            }
            this.m_main(argument ?? string.Empty, UpdateType.Mod);
        }

        void IMyGridProgram.Main(string argument, UpdateType updateSource)
        {
            if (this.m_main == null)
            {
                throw new InvalidOperationException("No Main method available");
            }
            this.m_main(argument ?? string.Empty, updateSource);
        }

        void IMyGridProgram.Save()
        {
            if (this.m_save != null)
            {
                this.m_save();
            }
        }

        public virtual Sandbox.ModAPI.Ingame.IMyGridTerminalSystem GridTerminalSystem { get; protected set; }

        public virtual Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me { get; protected set; }

        [Obsolete("Use Runtime.TimeSinceLastRun instead")]
        public virtual TimeSpan ElapsedTime { get; protected set; }

        public virtual IMyGridProgramRuntimeInfo Runtime { get; protected set; }

        public virtual string Storage
        {
            get => 
                (this.m_storage ?? "");
            protected set => 
                (this.m_storage = value ?? "");
        }

        public Action<string> Echo { get; protected set; }

        Func<IMyIntergridCommunicationSystem> IMyGridProgram.IGC_ContextGetter
        {
            set => 
                (this.m_IGC_ContextGetter = value);
        }

        public virtual IMyIntergridCommunicationSystem IGC =>
            this.m_IGC_ContextGetter();

        Sandbox.ModAPI.Ingame.IMyGridTerminalSystem IMyGridProgram.GridTerminalSystem
        {
            get => 
                this.GridTerminalSystem;
            set => 
                (this.GridTerminalSystem = value);
        }

        Sandbox.ModAPI.Ingame.IMyProgrammableBlock IMyGridProgram.Me
        {
            get => 
                this.Me;
            set => 
                (this.Me = value);
        }

        TimeSpan IMyGridProgram.ElapsedTime
        {
            get => 
                this.ElapsedTime;
            set => 
                (this.ElapsedTime = value);
        }

        string IMyGridProgram.Storage
        {
            get => 
                this.Storage;
            set => 
                (this.Storage = value);
        }

        Action<string> IMyGridProgram.Echo
        {
            get => 
                this.Echo;
            set => 
                (this.Echo = value);
        }

        IMyGridProgramRuntimeInfo IMyGridProgram.Runtime
        {
            get => 
                this.Runtime;
            set => 
                (this.Runtime = value);
        }

        bool IMyGridProgram.HasMainMethod =>
            (this.m_main != null);

        bool IMyGridProgram.HasSaveMethod =>
            (this.m_save != null);
    }
}

