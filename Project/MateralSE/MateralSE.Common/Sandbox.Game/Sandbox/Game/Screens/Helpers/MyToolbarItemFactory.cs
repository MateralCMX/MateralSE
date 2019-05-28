namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using VRage.Game;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.ModAPI.Ingame;
    using VRage.Game.ObjectBuilders;
    using VRage.ObjectBuilders;
    using VRage.Plugins;

    public static class MyToolbarItemFactory
    {
        private static MyObjectFactory<MyToolbarItemDescriptor, MyToolbarItem> m_objectFactory = new MyObjectFactory<MyToolbarItemDescriptor, MyToolbarItem>();

        static MyToolbarItemFactory()
        {
            m_objectFactory.RegisterFromAssembly(Assembly.GetAssembly(typeof(MyToolbarItem)));
            m_objectFactory.RegisterFromAssembly(MyPlugins.GameAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.SandboxAssembly);
            m_objectFactory.RegisterFromAssembly(MyPlugins.UserAssemblies);
        }

        public static MyObjectBuilder_ToolbarItem CreateObjectBuilder(MyToolbarItem item) => 
            m_objectFactory.CreateObjectBuilder<MyObjectBuilder_ToolbarItem>(item);

        public static MyToolbarItem CreateToolbarItem(MyObjectBuilder_ToolbarItem data)
        {
            MyToolbarItem item = m_objectFactory.CreateInstance(data.TypeId);
            return (item.Init(data) ? item : null);
        }

        public static MyToolbarItem CreateToolbarItemFromInventoryItem(IMyInventoryItem inventoryItem)
        {
            MyDefinitionBase base2;
            MyDefinitionId definitionId = inventoryItem.GetDefinitionId();
            if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(definitionId, out base2) && ((base2 is MyPhysicalItemDefinition) || (base2 is MyCubeBlockDefinition)))
            {
                MyObjectBuilder_ToolbarItem data = ObjectBuilderFromDefinition(base2);
                if (data is MyObjectBuilder_ToolbarItemMedievalWeapon)
                {
                    (data as MyObjectBuilder_ToolbarItemMedievalWeapon).ItemId = new uint?(inventoryItem.ItemId);
                }
                if ((data != null) && !(data is MyObjectBuilder_ToolbarItemEmpty))
                {
                    return CreateToolbarItem(data);
                }
            }
            return null;
        }

        public static string[] GetIconForTerminalGroup(MyBlockGroup group)
        {
            string[] icons = new string[] { @"Textures\GUI\Icons\GroupIcon.dds" };
            bool flag = false;
            HashSet<MyTerminalBlock> blocks = group.Blocks;
            if ((blocks != null) && (blocks.Count != 0))
            {
                MyDefinitionBase blockDefinition = blocks.FirstElement<MyTerminalBlock>().BlockDefinition;
                using (HashSet<MyTerminalBlock>.Enumerator enumerator = blocks.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.BlockDefinition.Equals(blockDefinition))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    icons = blockDefinition.Icons;
                }
            }
            return icons;
        }

        public static MyObjectBuilder_ToolbarItem ObjectBuilderFromDefinition(MyDefinitionBase defBase)
        {
            switch (defBase)
            {
                case (MyUsableItemDefinition _):
                {
                    MyObjectBuilder_ToolbarItemUsable local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemUsable>();
                    local1.DefinitionId = (SerializableDefinitionId) defBase.Id;
                    return local1;
                    break;
                }
            }
            if ((defBase is MyPhysicalItemDefinition) && (defBase.Id.TypeId == typeof(MyObjectBuilder_PhysicalGunObject)))
            {
                MyObjectBuilder_ToolbarItemWeapon weapon = null;
                weapon = (MyPerGameSettings.Game != GameEnum.ME_GAME) ? MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemWeapon>() : ((MyObjectBuilder_ToolbarItemWeapon) MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemMedievalWeapon>());
                weapon.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return weapon;
            }
            if (defBase is MyCubeBlockDefinition)
            {
                MyObjectBuilder_ToolbarItemCubeBlock local2 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemCubeBlock>();
                local2.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local2;
            }
            if (defBase is MyAnimationDefinition)
            {
                MyObjectBuilder_ToolbarItemAnimation local3 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemAnimation>();
                local3.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local3;
            }
            if (defBase is MyVoxelHandDefinition)
            {
                MyObjectBuilder_ToolbarItemVoxelHand local4 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemVoxelHand>();
                local4.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local4;
            }
            if (defBase is MyPrefabThrowerDefinition)
            {
                MyObjectBuilder_ToolbarItemPrefabThrower local5 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemPrefabThrower>();
                local5.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local5;
            }
            if (defBase is MyBotDefinition)
            {
                MyObjectBuilder_ToolbarItemBot local6 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemBot>();
                local6.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local6;
            }
            if (defBase is MyAiCommandDefinition)
            {
                MyObjectBuilder_ToolbarItemAiCommand local7 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemAiCommand>();
                local7.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local7;
            }
            if (defBase.Id.TypeId == typeof(MyObjectBuilder_RopeDefinition))
            {
                MyObjectBuilder_ToolbarItemRope local8 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemRope>();
                local8.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local8;
            }
            if (defBase is MyAreaMarkerDefinition)
            {
                MyObjectBuilder_ToolbarItemAreaMarker local9 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemAreaMarker>();
                local9.DefinitionId = (SerializableDefinitionId) defBase.Id;
                return local9;
            }
            if (!(defBase is MyGridCreateToolDefinition))
            {
                return new MyObjectBuilder_ToolbarItemEmpty();
            }
            MyObjectBuilder_ToolbarItemCreateGrid local10 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemCreateGrid>();
            local10.DefinitionId = (SerializableDefinitionId) defBase.Id;
            return local10;
        }

        public static MyObjectBuilder_ToolbarItemTerminalBlock TerminalBlockObjectBuilderFromBlock(MyTerminalBlock block)
        {
            MyObjectBuilder_ToolbarItemTerminalBlock local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemTerminalBlock>();
            local1.BlockEntityId = block.EntityId;
            local1._Action = null;
            return local1;
        }

        public static MyObjectBuilder_ToolbarItemTerminalGroup TerminalGroupObjectBuilderFromGroup(MyBlockGroup group)
        {
            MyObjectBuilder_ToolbarItemTerminalGroup local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemTerminalGroup>();
            local1.GroupName = group.Name.ToString();
            local1._Action = null;
            return local1;
        }

        public static MyObjectBuilder_ToolbarItemWeapon WeaponObjectBuilder() => 
            MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemWeapon>();
    }
}

