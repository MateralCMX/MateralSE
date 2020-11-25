using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace MateralSE.ConsoleApp
{
    public class Program : MyGridProgram
    {
        public void Main()
        {
            var myAssembler = GridTerminalSystem.GetBlockWithName("主装配机") as IMyAssembler;
            if (myAssembler == null) throw new ArgumentException("装配机不存在");
            if (!myAssembler.IsQueueEmpty) return;

            var cargoContainer = GridTerminalSystem.GetBlockWithName("箱子") as IMyCargoContainer;
            if (cargoContainer == null) throw new ArgumentException("箱子不存在");
            IMyInventory inventory = cargoContainer.GetInventory(0);
            var items = new List<MyInventoryItem>();
            inventory.GetItems(items);
            MyInventoryItem item = items[0];
            var typeKey = $"{item.Type.TypeId}/{item.Type.SubtypeId}";
            Echo(typeKey);


            MyDefinitionId blueprint = MyDefinitionId.Parse(typeKey);
            Echo($"{blueprint.TypeId} {blueprint.SubtypeId}");
            if (!myAssembler.CanUseBlueprint(blueprint))
            {
                Echo("不可以生产");
                return;
            }
            Echo("可以生产");
        }
    }
}
