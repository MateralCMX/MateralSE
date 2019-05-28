namespace VRage.Game.VisualScripting
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using VRage.Collections;
    using VRage.Game.ObjectBuilders.VisualScripting;
    using VRage.Game.VisualScripting.Missions;
    using VRage.ObjectBuilders;

    public class MyVSStateMachineManager
    {
        private readonly CachingList<MyVSStateMachine> m_runningMachines = new CachingList<MyVSStateMachine>();
        private readonly Dictionary<string, MyObjectBuilder_ScriptSM> m_machineDefinitions = new Dictionary<string, MyObjectBuilder_ScriptSM>();
        [CompilerGenerated]
        private Action<MyVSStateMachine> StateMachineStarted;

        public event Action<MyVSStateMachine> StateMachineStarted
        {
            [CompilerGenerated] add
            {
                Action<MyVSStateMachine> stateMachineStarted = this.StateMachineStarted;
                while (true)
                {
                    Action<MyVSStateMachine> a = stateMachineStarted;
                    Action<MyVSStateMachine> action3 = (Action<MyVSStateMachine>) Delegate.Combine(a, value);
                    stateMachineStarted = Interlocked.CompareExchange<Action<MyVSStateMachine>>(ref this.StateMachineStarted, action3, a);
                    if (ReferenceEquals(stateMachineStarted, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action<MyVSStateMachine> stateMachineStarted = this.StateMachineStarted;
                while (true)
                {
                    Action<MyVSStateMachine> source = stateMachineStarted;
                    Action<MyVSStateMachine> action3 = (Action<MyVSStateMachine>) Delegate.Remove(source, value);
                    stateMachineStarted = Interlocked.CompareExchange<Action<MyVSStateMachine>>(ref this.StateMachineStarted, action3, source);
                    if (ReferenceEquals(stateMachineStarted, source))
                    {
                        return;
                    }
                }
            }
        }

        public string AddMachine(string filePath)
        {
            MyObjectBuilder_VSFiles files;
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_VSFiles>(filePath, out files) || (files.StateMachine == null))
            {
                return null;
            }
            if (this.m_machineDefinitions.ContainsKey(files.StateMachine.Name))
            {
                return null;
            }
            this.m_machineDefinitions.Add(files.StateMachine.Name, files.StateMachine);
            return files.StateMachine.Name;
        }

        public void Dispose()
        {
            using (List<MyVSStateMachine>.Enumerator enumerator = this.m_runningMachines.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Dispose();
                }
            }
            this.m_runningMachines.Clear();
        }

        public MyObjectBuilder_ScriptStateMachineManager GetObjectBuilder()
        {
            this.m_runningMachines.ApplyChanges();
            MyObjectBuilder_ScriptStateMachineManager manager1 = new MyObjectBuilder_ScriptStateMachineManager();
            manager1.ActiveStateMachines = new List<MyObjectBuilder_ScriptStateMachineManager.CursorStruct>();
            MyObjectBuilder_ScriptStateMachineManager manager = manager1;
            foreach (MyVSStateMachine machine in this.m_runningMachines)
            {
                machine.ApplyChangesToCursors();
                List<MyStateMachineCursor> activeCursors = machine.ActiveCursors;
                MyObjectBuilder_ScriptSMCursor[] cursorArray = new MyObjectBuilder_ScriptSMCursor[activeCursors.Count];
                int index = 0;
                while (true)
                {
                    if (index >= activeCursors.Count)
                    {
                        MyObjectBuilder_ScriptStateMachineManager.CursorStruct item = new MyObjectBuilder_ScriptStateMachineManager.CursorStruct {
                            Cursors = cursorArray,
                            StateMachineName = machine.Name
                        };
                        manager.ActiveStateMachines.Add(item);
                        break;
                    }
                    MyObjectBuilder_ScriptSMCursor cursor1 = new MyObjectBuilder_ScriptSMCursor();
                    cursor1.NodeName = activeCursors[index].Node.Name;
                    cursorArray[index] = cursor1;
                    index++;
                }
            }
            return manager;
        }

        public bool Restore(string machineName, IEnumerable<MyObjectBuilder_ScriptSMCursor> cursors)
        {
            MyObjectBuilder_ScriptSM tsm;
            if (!this.m_machineDefinitions.TryGetValue(machineName, out tsm))
            {
                return false;
            }
            MyObjectBuilder_ScriptSM ob = new MyObjectBuilder_ScriptSM();
            ob.Name = tsm.Name;
            ob.Nodes = tsm.Nodes;
            ob.Transitions = tsm.Transitions;
            MyVSStateMachine entity = new MyVSStateMachine();
            long? ownerId = null;
            entity.Init(ob, ownerId);
            using (IEnumerator<MyObjectBuilder_ScriptSMCursor> enumerator = cursors.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyObjectBuilder_ScriptSMCursor current = enumerator.Current;
                    if (entity.RestoreCursor(current.NodeName) == null)
                    {
                        return false;
                    }
                }
            }
            this.m_runningMachines.Add(entity);
            if (this.StateMachineStarted != null)
            {
                this.StateMachineStarted(entity);
            }
            return true;
        }

        public bool Run(string machineName, long ownerId = 0L)
        {
            MyObjectBuilder_ScriptSM tsm;
            if (!this.m_machineDefinitions.TryGetValue(machineName, out tsm))
            {
                return false;
            }
            MyVSStateMachine entity = new MyVSStateMachine();
            entity.Init(tsm, new long?(ownerId));
            this.m_runningMachines.Add(entity);
            if (MyVisualScriptLogicProvider.MissionStarted != null)
            {
                MyVisualScriptLogicProvider.MissionStarted(entity.Name);
            }
            if (this.StateMachineStarted != null)
            {
                this.StateMachineStarted(entity);
            }
            return true;
        }

        public void Update()
        {
            this.m_runningMachines.ApplyChanges();
            foreach (MyVSStateMachine machine in this.m_runningMachines)
            {
                machine.Update();
                if (machine.ActiveCursorCount == 0)
                {
                    this.m_runningMachines.Remove(machine, false);
                    if (MyVisualScriptLogicProvider.MissionFinished != null)
                    {
                        MyVisualScriptLogicProvider.MissionFinished(machine.Name);
                    }
                }
            }
        }

        public IEnumerable<MyVSStateMachine> RunningMachines =>
            this.m_runningMachines;

        public Dictionary<string, MyObjectBuilder_ScriptSM> MachineDefinitions =>
            this.m_machineDefinitions;
    }
}

