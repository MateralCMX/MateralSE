namespace Sandbox.Game.Entities.Blocks
{
    using ProtoBuf;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Screens.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Serialization;

    [StructLayout(LayoutKind.Sequential), ProtoContract]
    public struct ToolbarItem
    {
        [ProtoMember(15)]
        public long EntityID;
        [ProtoMember(0x11), Serialize(MyObjectFlags.DefaultZero)]
        public string GroupName;
        [ProtoMember(20), Serialize(MyObjectFlags.DefaultZero)]
        public string Action;
        [ProtoMember(0x17), Serialize(MyObjectFlags.DefaultZero)]
        public List<MyObjectBuilder_ToolbarItemActionParameter> Parameters;
        [Serialize(MyObjectFlags.DefaultZero)]
        public SerializableDefinitionId? GunId;
        public static ToolbarItem FromItem(MyToolbarItem item)
        {
            ToolbarItem item2 = new ToolbarItem {
                EntityID = 0L
            };
            if (item is MyToolbarItemTerminalBlock)
            {
                MyObjectBuilder_ToolbarItemTerminalBlock objectBuilder = item.GetObjectBuilder() as MyObjectBuilder_ToolbarItemTerminalBlock;
                item2.EntityID = objectBuilder.BlockEntityId;
                item2.Action = objectBuilder._Action;
                item2.Parameters = objectBuilder.Parameters;
            }
            else if (!(item is MyToolbarItemTerminalGroup))
            {
                if (item is MyToolbarItemWeapon)
                {
                    item2.GunId = new SerializableDefinitionId?((item.GetObjectBuilder() as MyObjectBuilder_ToolbarItemWeapon).DefinitionId);
                }
            }
            else
            {
                MyObjectBuilder_ToolbarItemTerminalGroup objectBuilder = item.GetObjectBuilder() as MyObjectBuilder_ToolbarItemTerminalGroup;
                item2.EntityID = objectBuilder.BlockEntityId;
                item2.Action = objectBuilder._Action;
                item2.GroupName = objectBuilder.GroupName;
                item2.Parameters = objectBuilder.Parameters;
            }
            return item2;
        }

        public static MyToolbarItem ToItem(ToolbarItem msgItem)
        {
            MyToolbarItem item = null;
            if (msgItem.GunId != null)
            {
                MyObjectBuilder_ToolbarItemWeapon data = MyToolbarItemFactory.WeaponObjectBuilder();
                data.defId = msgItem.GunId.Value;
                item = MyToolbarItemFactory.CreateToolbarItem(data);
            }
            else if (string.IsNullOrEmpty(msgItem.GroupName))
            {
                MyTerminalBlock block;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyTerminalBlock>(msgItem.EntityID, out block, false))
                {
                    MyObjectBuilder_ToolbarItemTerminalBlock data = MyToolbarItemFactory.TerminalBlockObjectBuilderFromBlock(block);
                    data._Action = msgItem.Action;
                    data.Parameters = msgItem.Parameters;
                    item = MyToolbarItemFactory.CreateToolbarItem(data);
                }
            }
            else
            {
                MyCubeBlock block2;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById<MyCubeBlock>(msgItem.EntityID, out block2, false))
                {
                    string groupName = msgItem.GroupName;
                    MyBlockGroup group = block2.CubeGrid.GridSystems.TerminalSystem.BlockGroups.Find(x => x.Name.ToString() == groupName);
                    if (group != null)
                    {
                        MyObjectBuilder_ToolbarItemTerminalGroup data = MyToolbarItemFactory.TerminalGroupObjectBuilderFromGroup(group);
                        data._Action = msgItem.Action;
                        data.Parameters = msgItem.Parameters;
                        data.BlockEntityId = msgItem.EntityID;
                        item = MyToolbarItemFactory.CreateToolbarItem(data);
                    }
                }
            }
            return item;
        }

        public bool ShouldSerializeParameters() => 
            ((this.Parameters != null) && (this.Parameters.Count > 0));
    }
}

