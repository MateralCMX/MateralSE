namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    public class BoolData : VolatileComponentData
    {
        public bool Bool;

        public static implicit operator bool(BoolData data) => 
            data.Bool;

        public static void Update<TComponent>(uint actorId, bool data) where TComponent: MyRenderDirectComponent
        {
            MyRenderProxy.UpdateRenderComponent<BoolData, bool>(actorId, data, delegate (BoolData message, bool dataB) {
                message.Bool = dataB;
                message.SetComponent<TComponent>();
            });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__1<TComponent> where TComponent: MyRenderDirectComponent
        {
            public static readonly BoolData.<>c__1<TComponent> <>9;
            public static Action<BoolData, bool> <>9__1_0;

            static <>c__1()
            {
                BoolData.<>c__1<TComponent>.<>9 = new BoolData.<>c__1<TComponent>();
            }

            internal void <Update>b__1_0(BoolData message, bool dataB)
            {
                message.Bool = dataB;
                message.SetComponent<TComponent>();
            }
        }
    }
}

