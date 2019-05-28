namespace Sandbox.Game.Gui
{
    using Sandbox.ModAPI.Interfaces;
    using Sandbox.ModAPI.Interfaces.Terminal;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage;
    using VRage.Collections;

    public static class MyTerminalControlFactory
    {
        private static Dictionary<Type, BlockData> m_controls = new Dictionary<Type, BlockData>();
        private static FastResourceLock m_controlsLock = new FastResourceLock();

        public static void AddAction<TBase, TBlock>(MyTerminalAction<TBase> Action) where TBase: MyTerminalBlock where TBlock: TBase
        {
            GetList<TBlock>().Actions.Add(Action);
        }

        public static void AddAction<TBlock>(MyTerminalAction<TBlock> Action) where TBlock: MyTerminalBlock
        {
            GetList<TBlock>().Actions.Add(Action);
        }

        public static void AddAction<TBlock>(int index, MyTerminalAction<TBlock> Action) where TBlock: MyTerminalBlock
        {
            GetList<TBlock>().Actions.Insert(index, Action);
        }

        private static void AddActions<TBlock>(MyTerminalControl<TBlock> block) where TBlock: MyTerminalBlock
        {
            if (block.Actions != null)
            {
                MyTerminalAction<TBlock>[] actions = block.Actions;
                for (int i = 0; i < actions.Length; i++)
                {
                    AddAction<TBlock>(actions[i]);
                }
            }
        }

        private static void AddActions<TBlock>(int index, MyTerminalControl<TBlock> block) where TBlock: MyTerminalBlock
        {
            if (block.Actions != null)
            {
                foreach (MyTerminalAction<TBlock> action in block.Actions)
                {
                    index++;
                    AddAction<TBlock>(index, action);
                }
            }
        }

        public static void AddActions(Type blockType, ITerminalControl control)
        {
            if (control.Actions != null)
            {
                foreach (Sandbox.Game.Gui.ITerminalAction action in control.Actions)
                {
                    GetList(blockType).Actions.Add(action);
                }
            }
        }

        public static void AddBaseClass<TBlock, TBase>() where TBlock: TBase where TBase: MyTerminalBlock
        {
            AddBaseClass(typeof(TBase), GetList<TBlock>());
        }

        private static void AddBaseClass(Type baseClass, BlockData resultList)
        {
            BlockData data;
            MethodInfo method = baseClass.GetMethod("CreateTerminalControls", BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, new object[0]);
            }
            if (m_controls.TryGetValue(baseClass, out data))
            {
                foreach (ITerminalControl control in data.Controls.Items)
                {
                    resultList.Controls.Add(control);
                }
                foreach (Sandbox.Game.Gui.ITerminalAction action in data.Actions.Items)
                {
                    resultList.Actions.Add(action);
                }
            }
        }

        public static void AddControl<TBase, TBlock>(MyTerminalControl<TBase> control) where TBase: MyTerminalBlock where TBlock: TBase
        {
            GetList<TBlock>().Controls.Add(control);
            AddActions<TBase>(control);
        }

        public static void AddControl<TBlock>(MyTerminalControl<TBlock> control) where TBlock: MyTerminalBlock
        {
            GetList<TBlock>().Controls.Add(control);
            AddActions<TBlock>(control);
        }

        public static void AddControl<TBlock>(int index, MyTerminalControl<TBlock> control) where TBlock: MyTerminalBlock
        {
            GetList<TBlock>().Controls.Insert(index, control);
            AddActions<TBlock>(index, control);
        }

        public static void AddControl(Type blockType, ITerminalControl control)
        {
            GetList(blockType).Controls.Add(control);
        }

        public static bool AreControlsCreated<TBlock>() => 
            m_controls.ContainsKey(typeof(TBlock));

        public static bool AreControlsCreated(Type blockType) => 
            m_controls.ContainsKey(blockType);

        public static void EnsureControlsAreCreated(Type blockType)
        {
            MethodInfo method = blockType.GetMethod("CreateTerminalControls", BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, new object[0]);
            }
        }

        public static void GetActions<TBlock>(List<MyTerminalAction<TBlock>> resultList) where TBlock: MyTerminalBlock
        {
            foreach (Sandbox.Game.Gui.ITerminalAction action in GetList<TBlock>().Actions.Items)
            {
                resultList.Add((MyTerminalAction<TBlock>) action);
            }
        }

        public static UniqueListReader<Sandbox.Game.Gui.ITerminalAction> GetActions(Type blockType) => 
            GetList(blockType).Actions.Items;

        public static void GetActions(Type blockType, List<Sandbox.Game.Gui.ITerminalAction> resultList)
        {
            foreach (Sandbox.Game.Gui.ITerminalAction action in GetList(blockType).Actions.Items)
            {
                resultList.Add(action);
            }
        }

        public static void GetControls<TBlock>(List<MyTerminalControl<TBlock>> resultList) where TBlock: MyTerminalBlock
        {
            foreach (ITerminalControl control in GetList<TBlock>().Controls.Items)
            {
                resultList.Add((MyTerminalControl<TBlock>) control);
            }
        }

        public static UniqueListReader<ITerminalControl> GetControls(Type blockType) => 
            GetList(blockType).Controls.Items;

        public static void GetControls(Type blockType, List<ITerminalControl> resultList)
        {
            foreach (ITerminalControl control in GetList(blockType).Controls.Items)
            {
                resultList.Add(control);
            }
        }

        private static BlockData GetList<TBlock>() => 
            GetList(typeof(TBlock));

        internal static BlockData GetList(Type type)
        {
            BlockData data;
            if (!m_controls.TryGetValue(type, out data))
            {
                data = InitializeControls(type);
            }
            return data;
        }

        public static void GetValueControls(Type blockType, List<ITerminalProperty> resultList)
        {
            foreach (ITerminalProperty property in GetList(blockType).Controls.Items)
            {
                if (property != null)
                {
                    resultList.Add(property);
                }
            }
        }

        public static void GetValueControls<TBlock>(Type blockType, List<ITerminalProperty> resultList) where TBlock: MyTerminalBlock
        {
            foreach (ITerminalProperty property in GetList<TBlock>().Controls.Items)
            {
                if (property != null)
                {
                    resultList.Add(property);
                }
            }
        }

        internal static BlockData InitializeControls(Type type)
        {
            BlockData resultList = new BlockData();
            Type type2 = type;
            using (m_controlsLock.AcquireExclusiveUsing())
            {
                m_controls[type2] = resultList;
            }
            for (Type type3 = type.BaseType; type3 != null; type3 = type3.BaseType)
            {
                AddBaseClass(type3, resultList);
            }
            return resultList;
        }

        public static void RemoveAllBaseClass<TBlock>() where TBlock: MyTerminalBlock
        {
            BlockData list = GetList<TBlock>();
            for (Type type = typeof(TBlock).BaseType; type != null; type = type.BaseType)
            {
                RemoveBaseClass(type, list);
            }
        }

        public static void RemoveBaseClass<TBlock, TBase>() where TBlock: TBase where TBase: MyTerminalBlock
        {
            RemoveBaseClass(typeof(TBase), GetList<TBlock>());
        }

        private static void RemoveBaseClass(Type baseClass, BlockData resultList)
        {
            BlockData data;
            if (m_controls.TryGetValue(baseClass, out data))
            {
                foreach (ITerminalControl control in data.Controls.Items)
                {
                    resultList.Controls.Remove(control);
                }
                foreach (Sandbox.Game.Gui.ITerminalAction action in data.Actions.Items)
                {
                    resultList.Actions.Remove(action);
                }
            }
        }

        public static void RemoveControl<TBlock>(IMyTerminalControl item)
        {
            RemoveControl(typeof(TBlock), item);
        }

        public static void RemoveControl(Type blockType, IMyTerminalControl controlItem)
        {
            MyUniqueList<ITerminalControl> controls = GetList(blockType).Controls;
            foreach (ITerminalControl control2 in controls)
            {
                if (ReferenceEquals(control2, (ITerminalControl) controlItem))
                {
                    controls.Remove(control2);
                    break;
                }
            }
            ITerminalControl control = (ITerminalControl) controlItem;
            if (control.Actions != null)
            {
                foreach (Sandbox.Game.Gui.ITerminalAction action in control.Actions)
                {
                    GetList(blockType).Actions.Remove(action);
                }
            }
        }

        public static void Unload()
        {
            m_controls.Clear();
        }

        internal class BlockData
        {
            public MyUniqueList<ITerminalControl> Controls = new MyUniqueList<ITerminalControl>();
            public MyUniqueList<Sandbox.Game.Gui.ITerminalAction> Actions = new MyUniqueList<Sandbox.Game.Gui.ITerminalAction>();
        }
    }
}

