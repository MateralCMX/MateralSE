namespace Sandbox.Game.GameSystems.Electricity
{
    using Sandbox.Game.EntityComponents;
    using System;

    public class MyRechargeSocket
    {
        private MyResourceDistributorComponent m_resourceDistributor;
        private MyResourceSinkComponent m_pluggedInConsumer;

        public void PlugIn(MyResourceSinkComponent consumer)
        {
            if (!ReferenceEquals(this.m_pluggedInConsumer, consumer))
            {
                this.m_pluggedInConsumer = consumer;
                if (this.m_resourceDistributor != null)
                {
                    this.m_resourceDistributor.AddSink(consumer);
                    consumer.Update();
                }
            }
        }

        public void Unplug()
        {
            if (this.m_pluggedInConsumer != null)
            {
                if (this.m_resourceDistributor != null)
                {
                    this.m_resourceDistributor.RemoveSink(this.m_pluggedInConsumer, true, false);
                }
                this.m_pluggedInConsumer = null;
            }
        }

        public MyResourceDistributorComponent ResourceDistributor
        {
            get => 
                this.m_resourceDistributor;
            set
            {
                if (!ReferenceEquals(this.m_resourceDistributor, value))
                {
                    if ((this.m_pluggedInConsumer != null) && (this.m_resourceDistributor != null))
                    {
                        this.m_resourceDistributor.RemoveSink(this.m_pluggedInConsumer, true, false);
                    }
                    this.m_resourceDistributor = value;
                    if ((this.m_pluggedInConsumer != null) && (this.m_resourceDistributor != null))
                    {
                        this.m_resourceDistributor.AddSink(this.m_pluggedInConsumer);
                    }
                }
            }
        }
    }
}

