namespace Sandbox.Game.Gui
{
    using Sandbox.Game.Entities.Cube;
    using Sandbox.ModAPI;
    using Sandbox.ModAPI.Ingame;
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.ModAPI.Ingame;

    public class MyTerminalAction<TBlock> : Sandbox.Game.Gui.ITerminalAction, Sandbox.ModAPI.Interfaces.ITerminalAction, IMyTerminalAction where TBlock: MyTerminalBlock
    {
        private readonly string m_id;
        private string m_icon;
        private StringBuilder m_name;
        private List<TerminalActionParameter> m_parameterDefinitions;
        private Action<TBlock> m_action;
        private Action<TBlock, ListReader<TerminalActionParameter>> m_actionWithParameters;
        public Func<TBlock, bool> Enabled;
        public Func<TBlock, bool> Callable;
        public List<MyToolbarType> InvalidToolbarTypes;
        public bool ValidForGroups;
        public MyTerminalControl<TBlock>.WriterDelegate Writer;
        public Action<IList<TerminalActionParameter>, Action<bool>> DoUserParameterRequest;

        public MyTerminalAction(string id, StringBuilder name, string icon)
        {
            this.m_parameterDefinitions = new List<TerminalActionParameter>();
            this.Enabled = b => true;
            this.Callable = b => true;
            this.ValidForGroups = true;
            this.m_id = id;
            this.m_name = name;
            this.m_icon = icon;
        }

        public MyTerminalAction(string id, StringBuilder name, Action<TBlock> action, string icon)
        {
            this.m_parameterDefinitions = new List<TerminalActionParameter>();
            this.Enabled = b => true;
            this.Callable = b => true;
            this.ValidForGroups = true;
            this.m_id = id;
            this.m_name = name;
            this.Action = action;
            this.m_icon = icon;
        }

        public MyTerminalAction(string id, StringBuilder name, Action<TBlock, ListReader<TerminalActionParameter>> action, string icon)
        {
            this.m_parameterDefinitions = new List<TerminalActionParameter>();
            this.Enabled = b => true;
            this.Callable = b => true;
            this.ValidForGroups = true;
            this.m_id = id;
            this.m_name = name;
            this.ActionWithParameters = action;
            this.m_icon = icon;
        }

        public MyTerminalAction(string id, StringBuilder name, Action<TBlock> action, MyTerminalControl<TBlock>.WriterDelegate valueWriter, string icon)
        {
            this.m_parameterDefinitions = new List<TerminalActionParameter>();
            this.Enabled = b => true;
            this.Callable = b => true;
            this.ValidForGroups = true;
            this.m_id = id;
            this.m_name = name;
            this.Action = action;
            this.m_icon = icon;
            this.Writer = valueWriter;
        }

        public MyTerminalAction(string id, StringBuilder name, Action<TBlock, ListReader<TerminalActionParameter>> action, MyTerminalControl<TBlock>.WriterDelegate valueWriter, string icon)
        {
            this.m_parameterDefinitions = new List<TerminalActionParameter>();
            this.Enabled = b => true;
            this.Callable = b => true;
            this.ValidForGroups = true;
            this.m_id = id;
            this.m_name = name;
            this.ActionWithParameters = action;
            this.m_icon = icon;
            this.Writer = valueWriter;
        }

        public MyTerminalAction(string id, StringBuilder name, Action<TBlock> action, MyTerminalControl<TBlock>.WriterDelegate valueWriter, string icon, Func<TBlock, bool> enabled = null, Func<TBlock, bool> callable = null) : this(id, name, action, valueWriter, icon)
        {
            if (enabled != null)
            {
                this.Enabled = enabled;
            }
            if (callable != null)
            {
                this.Callable = callable;
            }
        }

        public void Apply(MyTerminalBlock block)
        {
            TBlock arg = (TBlock) block;
            if (this.Enabled(arg) && this.IsCallable(arg))
            {
                this.m_action(arg);
            }
        }

        public void Apply(MyTerminalBlock block, ListReader<TerminalActionParameter> parameters)
        {
            TBlock arg = (TBlock) block;
            if (this.Enabled(arg) && this.IsCallable(arg))
            {
                this.m_actionWithParameters(arg, parameters);
            }
        }

        public bool IsCallable(MyTerminalBlock block) => 
            ((this.Callable == null) ? true : this.Callable((TBlock) block));

        public bool IsEnabled(MyTerminalBlock block)
        {
            if (!string.IsNullOrEmpty(this.Id) && ((this.Id.Equals("IncreaseWeld speed") || this.Id.Equals("DecreaseWeld speed")) || this.Id.Equals("Force weld")))
            {
                return false;
            }
            return (this.Enabled((TBlock) block) && this.IsCallable((TBlock) block));
        }

        public bool IsValidForGroups() => 
            this.ValidForGroups;

        public bool IsValidForToolbarType(MyToolbarType type) => 
            ((this.InvalidToolbarTypes != null) ? !this.InvalidToolbarTypes.Contains(type) : true);

        public void RequestParameterCollection(IList<TerminalActionParameter> parameters, Action<bool> callback)
        {
            if (parameters == null)
            {
                throw new ArgumentException("parameters");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            Action<IList<TerminalActionParameter>, Action<bool>> doUserParameterRequest = this.DoUserParameterRequest;
            parameters.Clear();
            foreach (TerminalActionParameter parameter in this.ParameterDefinitions)
            {
                parameters.Add(parameter);
            }
            if (doUserParameterRequest == null)
            {
                callback(true);
            }
            else
            {
                doUserParameterRequest(parameters, callback);
            }
        }

        ListReader<TerminalActionParameter> Sandbox.Game.Gui.ITerminalAction.GetParameterDefinitions() => 
            this.m_parameterDefinitions;

        void Sandbox.ModAPI.Interfaces.ITerminalAction.Apply(IMyCubeBlock block)
        {
            if (block is TBlock)
            {
                this.Apply(block as MyTerminalBlock);
            }
        }

        void Sandbox.ModAPI.Interfaces.ITerminalAction.Apply(IMyCubeBlock block, ListReader<TerminalActionParameter> parameters)
        {
            if (block is TBlock)
            {
                this.Apply(block as MyTerminalBlock, parameters);
            }
        }

        bool Sandbox.ModAPI.Interfaces.ITerminalAction.IsEnabled(IMyCubeBlock block) => 
            ((block is TBlock) && this.IsEnabled(block as MyTerminalBlock));

        void Sandbox.ModAPI.Interfaces.ITerminalAction.WriteValue(IMyCubeBlock block, StringBuilder appendTo)
        {
            if (block is TBlock)
            {
                this.WriteValue(block as MyTerminalBlock, appendTo);
            }
        }

        public void WriteValue(MyTerminalBlock block, StringBuilder appendTo)
        {
            if ((this.Writer != null) && this.IsCallable((TBlock) block))
            {
                this.Writer((TBlock) block, appendTo);
            }
        }

        public Action<TBlock> Action
        {
            get => 
                this.m_action;
            set
            {
                this.m_action = value;
                this.m_actionWithParameters = (block, parameters) => base.m_action(block);
            }
        }

        public Action<TBlock, ListReader<TerminalActionParameter>> ActionWithParameters
        {
            get => 
                this.m_actionWithParameters;
            set
            {
                this.m_actionWithParameters = value;
                this.m_action = block => base.m_actionWithParameters(block, new ListReader<TerminalActionParameter>(base.ParameterDefinitions));
            }
        }

        public string Id =>
            this.m_id;

        public string Icon =>
            this.m_icon;

        public StringBuilder Name =>
            this.m_name;

        string Sandbox.ModAPI.Interfaces.ITerminalAction.Id =>
            this.Id;

        string Sandbox.ModAPI.Interfaces.ITerminalAction.Icon =>
            this.Icon;

        StringBuilder Sandbox.ModAPI.Interfaces.ITerminalAction.Name =>
            this.Name;

        public List<TerminalActionParameter> ParameterDefinitions =>
            this.m_parameterDefinitions;

        Func<Sandbox.ModAPI.IMyTerminalBlock, bool> IMyTerminalAction.Enabled
        {
            get
            {
                Func<TBlock, bool> oldEnabled = this.Enabled;
                return x => oldEnabled((TBlock) x);
            }
            set => 
                (this.Enabled = (Func<TBlock, bool>) value);
        }

        List<MyToolbarType> IMyTerminalAction.InvalidToolbarTypes
        {
            get => 
                this.InvalidToolbarTypes;
            set => 
                (this.InvalidToolbarTypes = value);
        }

        bool IMyTerminalAction.ValidForGroups
        {
            get => 
                this.ValidForGroups;
            set => 
                (this.ValidForGroups = value);
        }

        StringBuilder IMyTerminalAction.Name
        {
            get => 
                this.Name;
            set => 
                (this.m_name = value);
        }

        string IMyTerminalAction.Icon
        {
            get => 
                this.Icon;
            set => 
                (this.m_icon = value);
        }

        Action<Sandbox.ModAPI.IMyTerminalBlock> IMyTerminalAction.Action
        {
            get
            {
                Action<TBlock> oldAction = this.Action;
                return delegate (Sandbox.ModAPI.IMyTerminalBlock x) {
                    oldAction((TBlock) x);
                };
            }
            set => 
                (this.Action = (Action<TBlock>) value);
        }

        Action<Sandbox.ModAPI.IMyTerminalBlock, StringBuilder> IMyTerminalAction.Writer
        {
            get
            {
                MyTerminalControl<TBlock>.WriterDelegate oldWriter = this.Writer;
                return delegate (Sandbox.ModAPI.IMyTerminalBlock x, StringBuilder y) {
                    oldWriter((TBlock) x, y);
                };
            }
            set => 
                (this.Writer = new MyTerminalControl<TBlock>.WriterDelegate(value.Invoke));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTerminalAction<TBlock>.<>c <>9;
            public static Func<TBlock, bool> <>9__12_0;
            public static Func<TBlock, bool> <>9__12_1;
            public static Func<TBlock, bool> <>9__13_0;
            public static Func<TBlock, bool> <>9__13_1;
            public static Func<TBlock, bool> <>9__14_0;
            public static Func<TBlock, bool> <>9__14_1;
            public static Func<TBlock, bool> <>9__15_0;
            public static Func<TBlock, bool> <>9__15_1;
            public static Func<TBlock, bool> <>9__16_0;
            public static Func<TBlock, bool> <>9__16_1;

            static <>c()
            {
                MyTerminalAction<TBlock>.<>c.<>9 = new MyTerminalAction<TBlock>.<>c();
            }

            internal bool <.ctor>b__12_0(TBlock b) => 
                true;

            internal bool <.ctor>b__12_1(TBlock b) => 
                true;

            internal bool <.ctor>b__13_0(TBlock b) => 
                true;

            internal bool <.ctor>b__13_1(TBlock b) => 
                true;

            internal bool <.ctor>b__14_0(TBlock b) => 
                true;

            internal bool <.ctor>b__14_1(TBlock b) => 
                true;

            internal bool <.ctor>b__15_0(TBlock b) => 
                true;

            internal bool <.ctor>b__15_1(TBlock b) => 
                true;

            internal bool <.ctor>b__16_0(TBlock b) => 
                true;

            internal bool <.ctor>b__16_1(TBlock b) => 
                true;
        }
    }
}

