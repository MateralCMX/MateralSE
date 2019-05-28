namespace VRage.Game.ModAPI.Ingame
{
    using Sandbox.Definitions;
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game;

    public static class MyPhysicalInventoryItemExtensions_ModAPI
    {
        public static MyItemInfo GetItemInfo(this MyItemType itemType)
        {
            MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition((MyDefinitionId) itemType);
            if (physicalItemDefinition == null)
            {
                return new MyItemInfo();
            }
            return new MyItemInfo { 
                Size = physicalItemDefinition.Size,
                Mass = physicalItemDefinition.Mass,
                Volume = physicalItemDefinition.Volume,
                MaxStackAmount = physicalItemDefinition.MaxStackAmount,
                UsesFractions = !physicalItemDefinition.HasIntegralAmounts,
                IsOre = physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ore),
                IsIngot = physicalItemDefinition.Id.TypeId == typeof(MyObjectBuilder_Ingot)
            };
        }
    }
}

