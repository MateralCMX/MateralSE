namespace Sandbox.Game.Replication
{
    using System;

    internal class MyMeteorReplicable : MyEntityReplicableBaseEvent<MyMeteor>
    {
        public override void OnDestroyClient()
        {
            if ((base.Instance != null) && base.Instance.Save)
            {
                base.Instance.Close();
            }
        }
    }
}

