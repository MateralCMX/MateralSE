namespace Sandbox.Game.GUI
{
    using Sandbox;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Utils;

    public class MyStatControlledEntityReloading : MyStatBase
    {
        private MyUserControllableGun m_lastConnected;
        private int m_reloadCompletionTime;
        private int m_reloadInterval;

        public MyStatControlledEntityReloading()
        {
            base.Id = MyStringHash.GetOrCompute("controlled_reloading");
        }

        private void OnReloading(int reloadTime)
        {
            int totalGamePlayTimeInMilliseconds = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (this.m_reloadCompletionTime <= totalGamePlayTimeInMilliseconds)
            {
                this.m_reloadCompletionTime = totalGamePlayTimeInMilliseconds + reloadTime;
                this.m_reloadInterval = reloadTime;
            }
        }

        public override void Update()
        {
            MyUserControllableGun controlledEntity = MySession.Static.ControlledEntity as MyUserControllableGun;
            if (!ReferenceEquals(controlledEntity, this.m_lastConnected))
            {
                if (this.m_lastConnected != null)
                {
                    this.m_lastConnected.ReloadStarted -= new Action<int>(this.OnReloading);
                }
                this.m_lastConnected = controlledEntity;
                if (controlledEntity != null)
                {
                    controlledEntity.ReloadStarted += new Action<int>(this.OnReloading);
                }
                this.m_reloadCompletionTime = MySandboxGame.TotalGamePlayTimeInMilliseconds;
            }
            int num = this.m_reloadCompletionTime - MySandboxGame.TotalGamePlayTimeInMilliseconds;
            if (num > 0)
            {
                base.CurrentValue = 1f - (((float) num) / ((float) this.m_reloadInterval));
            }
            else
            {
                base.CurrentValue = 0f;
            }
        }
    }
}

