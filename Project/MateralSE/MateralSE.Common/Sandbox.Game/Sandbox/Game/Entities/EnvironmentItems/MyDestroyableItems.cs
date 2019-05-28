namespace Sandbox.Game.Entities.EnvironmentItems
{
    using Sandbox.Game.Multiplayer;
    using System;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    [MyEntityType(typeof(MyObjectBuilder_Bushes), false), MyEntityType(typeof(MyObjectBuilder_DestroyableItems), true)]
    public class MyDestroyableItems : MyEnvironmentItems
    {
        protected override MyEntity DestroyItem(int itemInstanceId)
        {
            base.RemoveItem(itemInstanceId, true, true);
            return null;
        }

        public override void DoDamage(float damage, int instanceId, Vector3D position, Vector3 normal, MyStringHash type)
        {
            if (Sync.IsServer)
            {
                base.RemoveItem(instanceId, true, true);
            }
        }
    }
}

