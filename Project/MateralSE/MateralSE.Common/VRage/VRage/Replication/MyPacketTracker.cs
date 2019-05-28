namespace VRage.Replication
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class MyPacketTracker
    {
        private const int BUFFER_LENGTH = 5;
        private readonly List<byte> m_ids = new List<byte>();

        public OrderType Add(byte id)
        {
            if ((this.m_ids.Count == 1) && (id == (this.m_ids[0] + 1)))
            {
                this.m_ids[0] = id;
                return OrderType.InOrder;
            }
            if (this.m_ids.FindIndex(x => x == id) != -1)
            {
                return OrderType.Duplicate;
            }
            this.m_ids.Add(id);
            for (int i = 2; i < this.m_ids.Count; i++)
            {
                if ((this.m_ids[0] + 1) == this.m_ids[i])
                {
                    this.m_ids.RemoveAt(i);
                    this.m_ids.RemoveAt(0);
                    this.CleanUp();
                    return OrderType.OutOfOrder;
                }
            }
            if (this.m_ids.Count < 5)
            {
                return OrderType.InOrder;
            }
            int num2 = this.m_ids[0];
            this.m_ids.RemoveAt(0);
            this.CleanUp();
            return (OrderType) (3 + ((this.m_ids[0] - num2) - 2));
        }

        private void CleanUp()
        {
            byte num = 0;
            bool flag = true;
            bool flag2 = true;
            foreach (byte num2 in this.m_ids)
            {
                flag2 &= flag || (((byte) (num + 1)) == num2);
                num = num2;
                flag = false;
            }
            if (flag2)
            {
                this.m_ids.RemoveRange(0, this.m_ids.Count - 1);
            }
        }

        public MyPacketStatistics Statistics { get; set; }

        public enum OrderType
        {
            InOrder,
            OutOfOrder,
            Duplicate,
            Drop1,
            Drop2,
            Drop3,
            Drop4,
            DropX
        }
    }
}

