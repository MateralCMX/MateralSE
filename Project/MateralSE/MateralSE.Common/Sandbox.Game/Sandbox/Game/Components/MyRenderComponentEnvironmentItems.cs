namespace Sandbox.Game.Components
{
    using Sandbox.Game.Entities.EnvironmentItems;
    using System;
    using System.Collections.Generic;

    internal class MyRenderComponentEnvironmentItems : MyRenderComponent
    {
        internal readonly MyEnvironmentItems EnvironmentItems;

        internal MyRenderComponentEnvironmentItems(MyEnvironmentItems environmentItems)
        {
            this.EnvironmentItems = environmentItems;
        }

        public override void AddRenderObjects()
        {
        }

        public override void RemoveRenderObjects()
        {
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair in this.EnvironmentItems.Sectors)
            {
                pair.Value.UnloadRenderObjects();
            }
            foreach (KeyValuePair<Vector3I, MyEnvironmentSector> pair2 in this.EnvironmentItems.Sectors)
            {
                pair2.Value.ClearInstanceData();
            }
        }
    }
}

