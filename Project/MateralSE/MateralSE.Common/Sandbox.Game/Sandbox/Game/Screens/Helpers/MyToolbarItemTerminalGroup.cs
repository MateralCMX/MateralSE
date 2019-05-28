namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemTerminalGroup))]
    internal class MyToolbarItemTerminalGroup : MyToolbarItemActions, IMyToolbarItemEntity
    {
        private static HashSet<Type> tmpBlockTypes = new HashSet<Type>();
        private static List<MyTerminalBlock> m_tmpBlocks = new List<MyTerminalBlock>();
        private static StringBuilder m_tmpStringBuilder = new StringBuilder();
        private StringBuilder m_groupName;
        private long m_blockEntityId;
        private bool m_wasValid;

        public override bool Activate()
        {
            bool flag;
            ListReader<MyTerminalBlock> blocks = this.GetBlocks();
            ITerminalAction action = this.FindAction(this.GetActions(blocks, out flag), base.ActionId);
            if (action == null)
            {
                return false;
            }
            try
            {
                foreach (MyTerminalBlock block in blocks)
                {
                    m_tmpBlocks.Add(block);
                }
                foreach (MyTerminalBlock block2 in m_tmpBlocks)
                {
                    if (block2 == null)
                    {
                        continue;
                    }
                    if (block2.IsFunctional)
                    {
                        action.Apply(block2);
                    }
                }
            }
            finally
            {
                m_tmpBlocks.Clear();
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type != MyToolbarType.Character) && (type != MyToolbarType.Spectator));

        public bool CompareEntityIds(long id) => 
            (this.m_blockEntityId == id);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            MyToolbarItemTerminalGroup group = obj as MyToolbarItemTerminalGroup;
            return ((group != null) && ((this.m_blockEntityId == group.m_blockEntityId) && (this.m_groupName.Equals(group.m_groupName) && (base.ActionId == group.ActionId))));
        }

        private ITerminalAction FindAction(ListReader<ITerminalAction> actions, string name)
        {
            using (List<ITerminalAction>.Enumerator enumerator = actions.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ITerminalAction current = enumerator.Current;
                    if (current.Id == name)
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static Type FindBaseClass(Type[] types, Type baseKnownCommonType)
        {
            Type key = types[0];
            Dictionary<Type, int> dictionary = new Dictionary<Type, int> {
                { 
                    baseKnownCommonType,
                    types.Length
                }
            };
            int index = 0;
            while (index < types.Length)
            {
                key = types[index];
                while (true)
                {
                    if (key == baseKnownCommonType)
                    {
                        index++;
                        break;
                    }
                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = 1;
                    }
                    else
                    {
                        Dictionary<Type, int> dictionary2 = dictionary;
                        Type type2 = key;
                        dictionary2[type2] += 1;
                    }
                    key = key.BaseType;
                }
            }
            key = types[0];
            while (dictionary[key] != types.Length)
            {
                key = key.BaseType;
            }
            return key;
        }

        private MyTerminalBlock FirstFunctional(ListReader<MyTerminalBlock> blocks, VRage.Game.Entity.MyEntity owner, long playerID)
        {
            using (List<MyTerminalBlock>.Enumerator enumerator = blocks.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyTerminalBlock current = enumerator.Current;
                    if (current.IsFunctional && (current.HasPlayerAccess(playerID) || current.HasPlayerAccess((owner as MyTerminalBlock).OwnerId)))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        private ListReader<ITerminalAction> GetActions(ListReader<MyTerminalBlock> blocks, out bool genericType)
        {
            ListReader<ITerminalAction> validActions;
            try
            {
                bool flag = true;
                foreach (MyTerminalBlock block in blocks)
                {
                    flag &= block is MyFunctionalBlock;
                    tmpBlockTypes.Add(block.GetType());
                }
                if (tmpBlockTypes.Count == 1)
                {
                    genericType = false;
                    validActions = this.GetValidActions(blocks.ItemAt(0).GetType(), blocks);
                }
                else if ((tmpBlockTypes.Count == 0) || !flag)
                {
                    genericType = true;
                    validActions = ListReader<ITerminalAction>.Empty;
                }
                else
                {
                    genericType = true;
                    Type blockType = FindBaseClass(tmpBlockTypes.ToArray<Type>(), typeof(MyFunctionalBlock));
                    validActions = this.GetValidActions(blockType, blocks);
                }
            }
            finally
            {
                tmpBlockTypes.Clear();
            }
            return validActions;
        }

        private ListReader<MyTerminalBlock> GetBlocks()
        {
            MyCubeBlock block;
            Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(this.m_blockEntityId, out block, false);
            if (block != null)
            {
                MyCubeGrid cubeGrid = block.CubeGrid;
                if ((cubeGrid == null) || (cubeGrid.GridSystems.TerminalSystem == null))
                {
                    return ListReader<MyTerminalBlock>.Empty;
                }
                using (List<MyBlockGroup>.Enumerator enumerator = cubeGrid.GridSystems.TerminalSystem.BlockGroups.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        MyBlockGroup current = enumerator.Current;
                        if (current.Name.Equals(this.m_groupName))
                        {
                            return current.Blocks.ToList<MyTerminalBlock>();
                        }
                    }
                }
            }
            return ListReader<MyTerminalBlock>.Empty;
        }

        public override int GetHashCode() => 
            ((((this.m_blockEntityId.GetHashCode() * 0x18d) ^ this.m_groupName.GetHashCode()) * 0x18d) ^ base.ActionId.GetHashCode());

        public override MyObjectBuilder_ToolbarItem GetObjectBuilder()
        {
            MyObjectBuilder_ToolbarItemTerminalGroup group1 = (MyObjectBuilder_ToolbarItemTerminalGroup) MyToolbarItemFactory.CreateObjectBuilder(this);
            group1.GroupName = this.m_groupName.ToString();
            group1.BlockEntityId = this.m_blockEntityId;
            group1._Action = base.ActionId;
            return group1;
        }

        private ListReader<ITerminalAction> GetValidActions(Type blockType, ListReader<MyTerminalBlock> blocks)
        {
            UniqueListReader<ITerminalAction> actions = MyTerminalControlFactory.GetActions(blockType);
            List<ITerminalAction> list = new List<ITerminalAction>();
            foreach (ITerminalAction action in actions)
            {
                if (action.IsValidForGroups())
                {
                    bool flag = false;
                    foreach (MyTerminalBlock block in blocks)
                    {
                        if (action.IsEnabled(block))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        list.Add(action);
                    }
                }
            }
            return list;
        }

        public override bool Init(MyObjectBuilder_ToolbarItem objBuilder)
        {
            base.WantsToBeActivated = false;
            base.WantsToBeSelected = false;
            base.ActivateOnClick = true;
            MyObjectBuilder_ToolbarItemTerminalGroup group = (MyObjectBuilder_ToolbarItemTerminalGroup) objBuilder;
            base.SetDisplayName(group.GroupName);
            if (group.BlockEntityId == 0)
            {
                this.m_wasValid = false;
                return false;
            }
            this.m_blockEntityId = group.BlockEntityId;
            this.m_groupName = new StringBuilder(group.GroupName);
            this.m_wasValid = true;
            base.SetAction(group._Action);
            return true;
        }

        public override ListReader<ITerminalAction> PossibleActions(MyToolbarType toolbarType) => 
            this.AllActions;

        public override MyToolbarItem.ChangeInfo Update(VRage.Game.Entity.MyEntity owner, long playerID = 0L)
        {
            bool flag;
            string[] icons;
            ListReader<MyTerminalBlock> blocks = this.GetBlocks();
            ITerminalAction action = this.FindAction(this.GetActions(blocks, out flag), base.ActionId);
            MyTerminalBlock block = this.FirstFunctional(blocks, owner, playerID);
            if (!flag)
            {
                icons = blocks.ItemAt(0).BlockDefinition.Icons;
            }
            else
            {
                icons = new string[] { @"Textures\GUI\Icons\GroupIcon.dds" };
            }
            MyToolbarItem.ChangeInfo info = (((MyToolbarItem.ChangeInfo) this) | (base.Update(owner, playerID) | this.SetEnabled((action != null) && (block != null))).SetIcons(icons)) | this.SetSubIcon(action?.Icon);
            if ((action == null) || this.m_wasValid)
            {
                if (action == null)
                {
                    this.m_wasValid = false;
                }
            }
            else
            {
                m_tmpStringBuilder.Clear();
                m_tmpStringBuilder.AppendStringBuilder(this.m_groupName);
                m_tmpStringBuilder.Append(" - ");
                m_tmpStringBuilder.Append(action.Name);
                info |= base.SetDisplayName(m_tmpStringBuilder.ToString());
                m_tmpStringBuilder.Clear();
                this.m_wasValid = true;
            }
            if ((action != null) && (blocks.Count > 0))
            {
                m_tmpStringBuilder.Clear();
                action.WriteValue(block ?? blocks.ItemAt(0), m_tmpStringBuilder);
                info |= base.SetIconText(m_tmpStringBuilder);
                m_tmpStringBuilder.Clear();
            }
            return info;
        }

        public override ListReader<ITerminalAction> AllActions
        {
            get
            {
                bool flag;
                return this.GetActions(this.GetBlocks(), out flag);
            }
        }
    }
}

