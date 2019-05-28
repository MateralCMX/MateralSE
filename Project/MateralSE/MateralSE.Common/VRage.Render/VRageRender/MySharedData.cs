namespace VRageRender
{
    using System;
    using System.Runtime.InteropServices;
    using VRage.Collections;
    using VRage.Library.Threading;

    public class MySharedData
    {
        private readonly SpinLockRef m_lock = new SpinLockRef();
        private readonly MySwapQueue<HashSet<uint>> m_outputVisibleObjects = MySwapQueue.Create<HashSet<uint>>();
        private readonly MyMessageQueue m_outputRenderMessages = new MyMessageQueue();
        private readonly MyUpdateData m_inputRenderMessages = new MyUpdateData();
        private readonly MySwapQueue<MyBillboardBatch<MyBillboard>> m_inputBillboards = MySwapQueue.Create<MyBillboardBatch<MyBillboard>>();
        private readonly MySwapQueue<MyBillboardBatch<MyTriangleBillboard>> m_inputTriangleBillboards = MySwapQueue.Create<MyBillboardBatch<MyTriangleBillboard>>();
        private readonly ConcurrentCachingList<MyBillboard> m_persistentBillboards = new ConcurrentCachingList<MyBillboard>();
        public MyUpdateFrame MessagesForNextFrame = new MyUpdateFrame();

        public MyBillboard AddPersistentBillboard()
        {
            MyBillboard entity = new MyBillboard();
            this.m_persistentBillboards.Add(entity);
            return entity;
        }

        public void AfterRender()
        {
            using (this.m_lock.Acquire())
            {
                this.m_outputVisibleObjects.CommitWrite();
                this.m_outputVisibleObjects.Write.Clear();
            }
        }

        public void AfterUpdate(MyTimeSpan? updateTimestamp)
        {
            using (this.m_lock.Acquire())
            {
                if (updateTimestamp != null)
                {
                    this.m_inputRenderMessages.CurrentUpdateFrame.UpdateTimestamp = updateTimestamp.Value;
                }
                this.m_inputRenderMessages.CommitUpdateFrame(this.m_lock);
                this.m_inputBillboards.CommitWrite();
                this.m_inputBillboards.Write.Clear();
                this.m_inputTriangleBillboards.CommitWrite();
                this.m_inputTriangleBillboards.Write.Clear();
            }
        }

        public void ApplyActionOnPersistentBillboards(Action<MyBillboard> action)
        {
            foreach (MyBillboard billboard in this.m_persistentBillboards)
            {
                action(billboard);
            }
        }

        public void BeforeRender(MyTimeSpan? currentDrawTime)
        {
            using (this.m_lock.Acquire())
            {
                this.m_persistentBillboards.ApplyChanges();
                if (currentDrawTime != null)
                {
                    MyRenderProxy.CurrentDrawTime = currentDrawTime.Value;
                }
            }
        }

        public void BeforeUpdate()
        {
            using (this.m_lock.Acquire())
            {
                this.m_outputVisibleObjects.RefreshRead();
                this.m_outputRenderMessages.Commit();
            }
        }

        public MyUpdateFrame GetRenderFrame(out bool isPreFrame)
        {
            using (this.m_lock.Acquire())
            {
                if (!isPreFrame)
                {
                    this.m_inputBillboards.RefreshRead();
                    this.m_inputTriangleBillboards.RefreshRead();
                }
                return this.m_inputRenderMessages.GetRenderFrame(out isPreFrame);
            }
        }

        public void RemovePersistentBillboard(MyBillboard billboard)
        {
            this.m_persistentBillboards.Remove(billboard, false);
        }

        public void ReturnPreFrame(MyUpdateFrame frame)
        {
            this.m_inputRenderMessages.ReturnPreFrame(frame);
        }

        public MySwapQueue<MyBillboardBatch<MyBillboard>> Billboards =>
            this.m_inputBillboards;

        public MySwapQueue<MyBillboardBatch<MyTriangleBillboard>> TriangleBillboards =>
            this.m_inputTriangleBillboards;

        public MySwapQueue<HashSet<uint>> VisibleObjects =>
            this.m_outputVisibleObjects;

        public MyUpdateFrame CurrentUpdateFrame =>
            this.m_inputRenderMessages.CurrentUpdateFrame;

        public MyMessageQueue RenderOutputMessageQueue =>
            this.m_outputRenderMessages;

        public int PersistentBillboardsCount =>
            this.m_persistentBillboards.Count;
    }
}

