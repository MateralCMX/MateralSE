namespace Sandbox.ModAPI
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.ModAPI.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Groups;

    public class MyTerminalControlFactoryHelper : IMyTerminalActionsHelper
    {
        private static MyTerminalControlFactoryHelper m_instance;
        private List<Sandbox.Game.Gui.ITerminalAction> m_actionList = new List<Sandbox.Game.Gui.ITerminalAction>();
        private List<ITerminalProperty> m_valueControls = new List<ITerminalProperty>();

        public void GetProperties(Type blockType, List<ITerminalProperty> resultList, Func<ITerminalProperty, bool> collect = null)
        {
            if (typeof(MyTerminalBlock).IsAssignableFrom(blockType))
            {
                MyTerminalControlFactory.GetValueControls(blockType, this.m_valueControls);
                foreach (ITerminalProperty property in this.m_valueControls)
                {
                    if ((collect == null) || collect(property))
                    {
                        resultList.Add(property);
                    }
                }
                this.m_valueControls.Clear();
            }
        }

        public ITerminalProperty GetProperty(string id, Type blockType)
        {
            if (typeof(MyTerminalBlock).IsAssignableFrom(blockType))
            {
                MyTerminalControlFactory.GetValueControls(blockType, this.m_valueControls);
                using (List<ITerminalProperty>.Enumerator enumerator = this.m_valueControls.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        ITerminalProperty current = enumerator.Current;
                        if (current.Id == id)
                        {
                            this.m_valueControls.Clear();
                            return current;
                        }
                    }
                }
                this.m_valueControls.Clear();
            }
            return null;
        }

        void IMyTerminalActionsHelper.GetActions(Type blockType, List<Sandbox.ModAPI.Interfaces.ITerminalAction> resultList, Func<Sandbox.ModAPI.Interfaces.ITerminalAction, bool> collect)
        {
            if (typeof(MyTerminalBlock).IsAssignableFrom(blockType))
            {
                MyTerminalControlFactory.GetActions(blockType, this.m_actionList);
                foreach (Sandbox.Game.Gui.ITerminalAction action in this.m_actionList)
                {
                    if (((collect == null) || collect(action)) && action.IsValidForToolbarType(MyToolbarType.ButtonPanel))
                    {
                        resultList.Add(action);
                    }
                }
                this.m_actionList.Clear();
            }
        }

        Sandbox.ModAPI.Interfaces.ITerminalAction IMyTerminalActionsHelper.GetActionWithName(string name, Type blockType)
        {
            if (typeof(MyTerminalBlock).IsAssignableFrom(blockType))
            {
                MyTerminalControlFactory.GetActions(blockType, this.m_actionList);
                using (List<Sandbox.Game.Gui.ITerminalAction>.Enumerator enumerator = this.m_actionList.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Sandbox.Game.Gui.ITerminalAction current = enumerator.Current;
                        if ((current.Id.ToString() == name) && current.IsValidForToolbarType(MyToolbarType.ButtonPanel))
                        {
                            this.m_actionList.Clear();
                            return current;
                        }
                    }
                }
                this.m_actionList.Clear();
            }
            return null;
        }

        IMyGridTerminalSystem IMyTerminalActionsHelper.GetTerminalSystemForGrid(IMyCubeGrid grid)
        {
            MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Group group = MyCubeGridGroups.Static.Logical.GetGroup(grid as MyCubeGrid);
            if ((group == null) || (group.GroupData == null))
            {
                return null;
            }
            return group.GroupData.TerminalSystem;
        }

        void IMyTerminalActionsHelper.SearchActionsOfName(string name, Type blockType, List<Sandbox.ModAPI.Interfaces.ITerminalAction> resultList, Func<Sandbox.ModAPI.Interfaces.ITerminalAction, bool> collect = null)
        {
            if (typeof(MyTerminalBlock).IsAssignableFrom(blockType))
            {
                MyTerminalControlFactory.GetActions(blockType, this.m_actionList);
                foreach (Sandbox.Game.Gui.ITerminalAction action in this.m_actionList)
                {
                    if (((collect == null) || collect(action)) && (action.Id.ToString().Contains(name) && action.IsValidForToolbarType(MyToolbarType.ButtonPanel)))
                    {
                        resultList.Add(action);
                    }
                }
                this.m_actionList.Clear();
            }
        }

        public static MyTerminalControlFactoryHelper Static
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new MyTerminalControlFactoryHelper();
                }
                return m_instance;
            }
        }
    }
}

