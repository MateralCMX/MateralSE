namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Gui;
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemTerminalBlock))]
    internal class MyToolbarItemTerminalBlock : MyToolbarItemActions, IMyToolbarItemEntity
    {
        private long m_blockEntityId;
        private bool m_wasValid;
        private bool m_nameChanged;
        private MyTerminalBlock m_block;
        private List<TerminalActionParameter> m_parameters = new List<TerminalActionParameter>();
        private static List<ITerminalAction> m_tmpEnabledActions = new List<ITerminalAction>();
        private static ListReader<ITerminalAction> m_tmpEnabledActionsReader = new ListReader<ITerminalAction>(m_tmpEnabledActions);
        private static StringBuilder m_tmpStringBuilder = new StringBuilder();
        private MyTerminalBlock m_registeredBlock;

        public override bool Activate()
        {
            ITerminalAction currentAction = base.GetCurrentAction();
            if ((this.m_block == null) || (currentAction == null))
            {
                return false;
            }
            currentAction.Apply(this.m_block, this.Parameters);
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type != MyToolbarType.Character) && (type != MyToolbarType.Spectator));

        private void block_CustomNameChanged(MyTerminalBlock obj)
        {
            this.m_nameChanged = true;
        }

        private void block_OnClose(VRage.Game.Entity.MyEntity obj)
        {
            this.UnregisterEvents();
            this.m_block = null;
        }

        public bool CompareEntityIds(long id) => 
            (id == this.m_blockEntityId);

        public override bool Equals(object obj)
        {
            if (!ReferenceEquals(this, obj))
            {
                MyToolbarItemTerminalBlock block = obj as MyToolbarItemTerminalBlock;
                if (((block == null) || (this.m_blockEntityId != block.m_blockEntityId)) || (base.ActionId != block.ActionId))
                {
                    return false;
                }
                if (this.m_parameters.Count != block.Parameters.Count)
                {
                    return false;
                }
                for (int i = 0; i < this.m_parameters.Count; i++)
                {
                    TerminalActionParameter parameter = this.m_parameters[i];
                    TerminalActionParameter parameter2 = block.Parameters[i];
                    if (parameter.TypeCode != parameter2.TypeCode)
                    {
                        return false;
                    }
                    if (!Equals(parameter.Value, parameter2.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private ListReader<ITerminalAction> GetActions(MyToolbarType? type)
        {
            if (this.m_block == null)
            {
                return ListReader<ITerminalAction>.Empty;
            }
            m_tmpEnabledActions.Clear();
            foreach (ITerminalAction action in MyTerminalControls.Static.GetActions(this.m_block))
            {
                if (!action.IsEnabled(this.m_block))
                {
                    continue;
                }
                if ((type == null) || action.IsValidForToolbarType(type.Value))
                {
                    m_tmpEnabledActions.Add(action);
                }
            }
            return m_tmpEnabledActionsReader;
        }

        public override int GetHashCode() => 
            ((this.m_blockEntityId.GetHashCode() * 0x18d) ^ base.ActionId.GetHashCode());

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            MyObjectBuilder_ToolbarItemTerminalBlock block = (MyObjectBuilder_ToolbarItemTerminalBlock) MyToolbarItemFactory.CreateObjectBuilder(this);
            block.BlockEntityId = this.m_blockEntityId;
            block._Action = base.ActionId;
            block.Parameters.Clear();
            foreach (TerminalActionParameter parameter in this.m_parameters)
            {
                block.Parameters.Add(parameter.GetObjectBuilder());
            }
            return block;
        }

        public override bool Init(MyObjectBuilder_ToolbarItem objectBuilder)
        {
            base.WantsToBeActivated = false;
            base.WantsToBeSelected = false;
            base.ActivateOnClick = true;
            this.m_block = null;
            MyObjectBuilder_ToolbarItemTerminalBlock block = (MyObjectBuilder_ToolbarItemTerminalBlock) objectBuilder;
            this.m_blockEntityId = block.BlockEntityId;
            if (this.m_blockEntityId == 0)
            {
                this.m_wasValid = false;
                return false;
            }
            this.TryGetBlock();
            base.SetAction(block._Action);
            if ((block.Parameters != null) && (block.Parameters.Count > 0))
            {
                this.m_parameters.Clear();
                foreach (MyObjectBuilder_ToolbarItemActionParameter parameter in block.Parameters)
                {
                    this.m_parameters.Add(TerminalActionParameter.Deserialize(parameter.Value, parameter.TypeCode));
                }
            }
            return true;
        }

        public override void OnRemovedFromToolbar(MyToolbar toolbar)
        {
            if (this.m_block != null)
            {
                this.UnregisterEvents();
            }
            base.OnRemovedFromToolbar(toolbar);
        }

        public override ListReader<ITerminalAction> PossibleActions(MyToolbarType type) => 
            this.GetActions(new MyToolbarType?(type));

        private void RegisterEvents()
        {
            this.UnregisterEvents();
            this.m_block.CustomNameChanged += new Action<MyTerminalBlock>(this.block_CustomNameChanged);
            this.m_block.OnClose += new Action<VRage.Game.Entity.MyEntity>(this.block_OnClose);
            this.m_registeredBlock = this.m_block;
        }

        private bool TryGetBlock()
        {
            bool flag1 = Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyTerminalBlock>(this.m_blockEntityId, out this.m_block, false);
            if (flag1)
            {
                this.RegisterEvents();
            }
            return flag1;
        }

        private void UnregisterEvents()
        {
            if (this.m_registeredBlock != null)
            {
                this.m_registeredBlock.CustomNameChanged -= new Action<MyTerminalBlock>(this.block_CustomNameChanged);
                this.m_registeredBlock.OnClose -= new Action<VRage.Game.Entity.MyEntity>(this.block_OnClose);
                this.m_registeredBlock = null;
            }
        }

        public override MyToolbarItem.ChangeInfo Update(VRage.Game.Entity.MyEntity owner, long playerID = 0L)
        {
            int num1;
            int num2;
            MyToolbarItem.ChangeInfo info = base.Update(owner, playerID);
            if (this.m_block == null)
            {
                this.TryGetBlock();
            }
            ITerminalAction currentAction = base.GetCurrentAction();
            if ((this.m_block == null) || (currentAction == null))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) MyCubeGridGroups.Static.Physical.HasSameGroup((owner as MyTerminalBlock).CubeGrid, this.m_block.CubeGrid);
            }
            bool flag = (bool) num1;
            if (!flag || !this.m_block.IsFunctional)
            {
                num2 = 0;
            }
            else
            {
                num2 = this.m_block.HasPlayerAccess(playerID) ? 1 : ((int) this.m_block.HasPlayerAccess((owner as MyTerminalBlock).OwnerId));
            }
            info |= this.SetEnabled((bool) num2);
            if (this.m_block != null)
            {
                info |= base.SetIcons(this.m_block.BlockDefinition.Icons);
            }
            if (flag)
            {
                if (!this.m_wasValid || base.ActionChanged)
                {
                    info = ((info | base.SetIcons(this.m_block.BlockDefinition.Icons)) | base.SetSubIcon(currentAction.Icon)) | this.UpdateCustomName(currentAction);
                }
                else if (this.m_nameChanged)
                {
                    info |= this.UpdateCustomName(currentAction);
                }
                m_tmpStringBuilder.Clear();
                currentAction.WriteValue(this.m_block, m_tmpStringBuilder);
                info |= base.SetIconText(m_tmpStringBuilder);
                m_tmpStringBuilder.Clear();
            }
            this.m_wasValid = flag;
            this.m_nameChanged = false;
            base.ActionChanged = false;
            return info;
        }

        private MyToolbarItem.ChangeInfo UpdateCustomName(ITerminalAction action)
        {
            MyToolbarItem.ChangeInfo info;
            try
            {
                m_tmpStringBuilder.Clear();
                m_tmpStringBuilder.AppendStringBuilder(this.m_block.CustomName);
                m_tmpStringBuilder.Append(" - ");
                m_tmpStringBuilder.AppendStringBuilder(action.Name);
                info = base.SetDisplayName(m_tmpStringBuilder.ToString());
            }
            finally
            {
                m_tmpStringBuilder.Clear();
            }
            return info;
        }

        public override ListReader<ITerminalAction> AllActions
        {
            get
            {
                MyToolbarType? type = null;
                return this.GetActions(type);
            }
        }

        public List<TerminalActionParameter> Parameters =>
            this.m_parameters;
    }
}

