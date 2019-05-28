namespace Sandbox.Game.Entities.Blocks
{
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Multiplayer;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage.Game;
    using VRage.Network;

    public class MyLaserBroadcaster : MyDataBroadcaster
    {
        public StringBuilder StateText = new StringBuilder();

        [Event(null, 0x47), Reliable, Broadcast]
        private void ChangeStateText(string newStateText)
        {
            if (base.Entity is MyProxyAntenna)
            {
                this.StateText.Clear();
                this.StateText.Append(newStateText);
            }
        }

        [Event(null, 0x54), Reliable, Broadcast]
        private void ChangeSuccessfullyContacting(long? newContact)
        {
            MyProxyAntenna entity = base.Entity as MyProxyAntenna;
            if (entity != null)
            {
                entity.SuccessfullyContacting = newContact;
            }
        }

        public override void InitProxyObjectBuilder(MyObjectBuilder_ProxyAntenna ob)
        {
            base.InitProxyObjectBuilder(ob);
            ob.IsLaser = true;
            ob.SuccessfullyContacting = this.SuccessfullyContacting;
            ob.StateText = this.StateText.ToString();
        }

        public void RaiseChangeStateText()
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyLaserBroadcaster, string>(this, x => new Action<string>(x.ChangeStateText), this.StateText.ToString(), targetEndpoint);
            }
        }

        public void RaiseChangeSuccessfullyContacting()
        {
            if (Sync.IsServer)
            {
                EndpointId targetEndpoint = new EndpointId();
                MyMultiplayer.RaiseEvent<MyLaserBroadcaster, long?>(this, x => new Action<long?>(x.ChangeSuccessfullyContacting), this.SuccessfullyContacting, targetEndpoint);
            }
        }

        public MyLaserAntenna RealAntenna =>
            (base.Entity as MyLaserAntenna);

        public long? SuccessfullyContacting
        {
            get
            {
                MyLaserAntenna realAntenna = this.RealAntenna;
                if (realAntenna != null)
                {
                    if (realAntenna.TargetId != null)
                    {
                        if (realAntenna.CanLaseTargetCoords)
                        {
                            return realAntenna.TargetId;
                        }
                        goto TR_0001;
                    }
                }
                MyProxyAntenna entity = base.Entity as MyProxyAntenna;
                if (entity != null)
                {
                    return entity.SuccessfullyContacting;
                }
            TR_0001:
                return null;
            }
        }

        public override bool ShowOnHud =>
            false;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyLaserBroadcaster.<>c <>9 = new MyLaserBroadcaster.<>c();
            public static Func<MyLaserBroadcaster, Action<long?>> <>9__8_0;
            public static Func<MyLaserBroadcaster, Action<string>> <>9__9_0;

            internal Action<string> <RaiseChangeStateText>b__9_0(MyLaserBroadcaster x) => 
                new Action<string>(x.ChangeStateText);

            internal Action<long?> <RaiseChangeSuccessfullyContacting>b__8_0(MyLaserBroadcaster x) => 
                new Action<long?>(x.ChangeSuccessfullyContacting);
        }
    }
}

