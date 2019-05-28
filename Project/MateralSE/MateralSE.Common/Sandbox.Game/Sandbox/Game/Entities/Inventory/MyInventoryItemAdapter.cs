namespace Sandbox.Game.Entities.Inventory
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Game.Entities.Interfaces;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Game.ModAPI.Ingame;

    public class MyInventoryItemAdapter : IMyInventoryItemAdapter
    {
        [ThreadStatic]
        private static MyInventoryItemAdapter m_static = new MyInventoryItemAdapter();
        private MyPhysicalItemDefinition m_physItem;
        private MyCubeBlockDefinition m_blockDef;

        public void Adapt(IMyInventoryItem inventoryItem)
        {
            this.m_physItem = null;
            this.m_blockDef = null;
            MyObjectBuilder_PhysicalObject content = inventoryItem.Content as MyObjectBuilder_PhysicalObject;
            if (content != null)
            {
                this.Adapt(content.GetObjectId());
            }
            else
            {
                this.Adapt(inventoryItem.GetDefinitionId());
            }
        }

        public void Adapt(MyDefinitionId itemDefinition)
        {
            if (!MyDefinitionManager.Static.TryGetPhysicalItemDefinition(itemDefinition, out this.m_physItem))
            {
                MyDefinitionManager.Static.TryGetCubeBlockDefinition(itemDefinition, out this.m_blockDef);
            }
        }

        public bool TryAdapt(MyDefinitionId itemDefinition)
        {
            this.m_physItem = null;
            this.m_blockDef = null;
            return (!MyDefinitionManager.Static.TryGetPhysicalItemDefinition(itemDefinition, out this.m_physItem) ? MyDefinitionManager.Static.TryGetCubeBlockDefinition(itemDefinition, out this.m_blockDef) : true);
        }

        public static MyInventoryItemAdapter Static
        {
            get
            {
                if (m_static == null)
                {
                    m_static = new MyInventoryItemAdapter();
                }
                return m_static;
            }
        }

        public float Mass
        {
            get
            {
                if (this.m_physItem != null)
                {
                    return this.m_physItem.Mass;
                }
                if (this.m_blockDef == null)
                {
                    return 0f;
                }
                if ((MyDestructionData.Static == null) || !Sync.IsServer)
                {
                    return this.m_blockDef.Mass;
                }
                return MyDestructionHelper.MassFromHavok(MyDestructionData.Static.GetBlockMass(this.m_blockDef.Model, this.m_blockDef));
            }
        }

        public float Volume
        {
            get
            {
                if (this.m_physItem != null)
                {
                    return this.m_physItem.Volume;
                }
                if (this.m_blockDef == null)
                {
                    return 0f;
                }
                float cubeSize = MyDefinitionManager.Static.GetCubeSize(this.m_blockDef.CubeSize);
                return (((this.m_blockDef.Size.Size * cubeSize) * cubeSize) * cubeSize);
            }
        }

        public bool HasIntegralAmounts =>
            ((this.m_physItem == null) ? (this.m_blockDef != null) : this.m_physItem.HasIntegralAmounts);

        public MyFixedPoint MaxStackAmount =>
            ((this.m_physItem == null) ? ((this.m_blockDef == null) ? MyFixedPoint.MaxValue : ((MyGridPickupComponent.Static == null) ? 1 : MyGridPickupComponent.Static.GetMaxStackSize(this.m_blockDef.Id))) : this.m_physItem.MaxStackAmount);

        public string DisplayNameText =>
            ((this.m_physItem == null) ? ((this.m_blockDef == null) ? "" : this.m_blockDef.DisplayNameText) : this.m_physItem.DisplayNameText);

        public string[] Icons
        {
            get
            {
                if (this.m_physItem != null)
                {
                    return this.m_physItem.Icons;
                }
                if (this.m_blockDef != null)
                {
                    return this.m_blockDef.Icons;
                }
                return new string[] { "" };
            }
        }

        public MyStringId? IconSymbol
        {
            get
            {
                if (this.m_physItem != null)
                {
                    return this.m_physItem.IconSymbol;
                }
                MyCubeBlockDefinition blockDef = this.m_blockDef;
                return null;
            }
        }
    }
}

