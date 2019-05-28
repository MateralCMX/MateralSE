namespace Sandbox.Game.GUI
{
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityRemoteAccess : MyStatBase
    {
        public MyStatControlledEntityRemoteAccess()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_remote_access");
        }

        public override void Update()
        {
            base.CurrentValue = 0f;
        }
    }
}

