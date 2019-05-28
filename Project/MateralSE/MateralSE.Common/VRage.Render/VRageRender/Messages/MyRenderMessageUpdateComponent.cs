namespace VRageRender.Messages
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using VRage.Collections;

    public class MyRenderMessageUpdateComponent : MyRenderMessageBase
    {
        private static ConcurrentDictionary<System.Type, DataFrame> m_dataPool = new ConcurrentDictionary<System.Type, DataFrame>();
        public uint ID;
        public UpdateType Type;
        public UpdateData Data;

        public override void Close()
        {
            base.Close();
            if (this.Data is VolatileComponentData)
            {
                this.Data.ComponentType = null;
            }
            m_dataPool[this.Data.DataType].Free(this.Data);
            this.Data = null;
        }

        public T Initialize<T>() where T: UpdateData
        {
            DataFrame orAdd;
            System.Type key = typeof(T);
            if (!m_dataPool.TryGetValue(key, out orAdd))
            {
                orAdd = m_dataPool.GetOrAdd(key, new DataFrame(key));
            }
            this.Data = orAdd.Allocate();
            return this.Data.As<T>();
        }

        public override MyRenderMessageType MessageClass =>
            MyRenderMessageType.StateChangeOnce;

        public override MyRenderMessageEnum MessageType =>
            MyRenderMessageEnum.UpdateRenderComponent;

        private class DataFrame
        {
            private MyConcurrentPool<UpdateData> m_messagePool;

            public DataFrame(Type type)
            {
                this.m_messagePool = new MyConcurrentPool<UpdateData>(5, null, 0x2710, ExpressionExtension.CreateActivator<UpdateData>(type));
            }

            public UpdateData Allocate() => 
                this.m_messagePool.Get();

            public void Free(UpdateData instance)
            {
                this.m_messagePool.Return(instance);
            }
        }

        public enum UpdateType
        {
            Update,
            Delete
        }
    }
}

