namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;

    internal abstract class MyToolbarItemActions : MyToolbarItem
    {
        private string m_actionId;

        protected MyToolbarItemActions()
        {
        }

        public ITerminalAction GetActionOrNull(string id)
        {
            using (List<ITerminalAction>.Enumerator enumerator = this.AllActions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ITerminalAction current = enumerator.Current;
                    if (current.Id == id)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public ITerminalAction GetCurrentAction() => 
            this.GetActionOrNull(this.ActionId);

        public abstract ListReader<ITerminalAction> PossibleActions(MyToolbarType toolbarType);
        protected void SetAction(string action)
        {
            this.ActionId = action;
            if (this.ActionId == null)
            {
                ListReader<ITerminalAction> allActions = this.AllActions;
                if (allActions.Count > 0)
                {
                    this.ActionId = allActions.ItemAt(0).Id;
                }
            }
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            if (this.ActionId == null)
            {
                ListReader<ITerminalAction> allActions = this.AllActions;
                if (allActions.Count > 0)
                {
                    this.ActionId = allActions.ItemAt(0).Id;
                }
            }
            return MyToolbarItem.ChangeInfo.None;
        }

        protected bool ActionChanged { get; set; }

        public string ActionId
        {
            get => 
                this.m_actionId;
            set
            {
                if (((this.m_actionId == null) || !this.m_actionId.Equals(value)) && ((this.m_actionId != null) || (value != null)))
                {
                    this.m_actionId = value;
                    this.ActionChanged = true;
                }
            }
        }

        public abstract ListReader<ITerminalAction> AllActions { get; }
    }
}

