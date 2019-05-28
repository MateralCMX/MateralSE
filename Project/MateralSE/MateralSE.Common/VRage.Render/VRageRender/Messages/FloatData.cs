namespace VRageRender.Messages
{
    using System;
    using System.Runtime.CompilerServices;
    using VRageRender;

    public class FloatData : VolatileComponentData
    {
        public float Float;

        public static implicit operator float(FloatData data) => 
            data.Float;

        public static void Update<TComponent>(uint actorId, float data) where TComponent: MyRenderDirectComponent
        {
            MyRenderProxy.UpdateRenderComponent<FloatData, float>(actorId, data, delegate (FloatData message, float dataF) {
                message.Float = dataF;
                message.SetComponent<TComponent>();
            });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__1<TComponent> where TComponent: MyRenderDirectComponent
        {
            public static readonly FloatData.<>c__1<TComponent> <>9;
            public static Action<FloatData, float> <>9__1_0;

            static <>c__1()
            {
                FloatData.<>c__1<TComponent>.<>9 = new FloatData.<>c__1<TComponent>();
            }

            internal void <Update>b__1_0(FloatData message, float dataF)
            {
                message.Float = dataF;
                message.SetComponent<TComponent>();
            }
        }
    }
}

