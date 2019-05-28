namespace VRageRender
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRageRender.Messages;

    public class MyMessagePool : Dictionary<int, MyConcurrentQueue<MyRenderMessageBase>>
    {
        public MyMessagePool()
        {
            foreach (MyRenderMessageEnum enum2 in Enum.GetValues(typeof(MyRenderMessageEnum)))
            {
                base.Add((int) enum2, new MyConcurrentQueue<MyRenderMessageBase>());
            }
        }

        public void Clear(MyRenderMessageEnum message)
        {
            base[(int) message].Clear();
        }

        public T Get<T>(MyRenderMessageEnum renderMessageEnum) where T: MyRenderMessageBase, new()
        {
            MyRenderMessageBase base2;
            if (!base[(int) renderMessageEnum].TryDequeue(out base2))
            {
                base2 = Activator.CreateInstance<T>();
            }
            base2.Init();
            return (T) base2;
        }

        public void Return(MyRenderMessageBase message)
        {
            if (!message.IsPersistent)
            {
                message.Close();
                base[(int) message.MessageType].Enqueue(message);
            }
        }
    }
}

