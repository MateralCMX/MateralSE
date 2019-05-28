namespace Sandbox.Game.Entities.EnvironmentItems
{
    using Sandbox.Game;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Utils;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 500)]
    public class MyEnvironmentItemsCoordinator : MySessionComponentBase
    {
        private static MyEnvironmentItemsCoordinator Static;
        private HashSet<MyEnvironmentItems> m_tmpItems;
        private List<TransferData> m_transferList;
        private float? m_transferTime;

        private void AddTransferData(MyEnvironmentItems from, MyEnvironmentItems to, int localId, MyStringHash subtypeId)
        {
            TransferData item = new TransferData {
                From = from,
                To = to,
                LocalId = localId,
                SubtypeId = subtypeId
            };
            this.m_transferList.Add(item);
        }

        private void FinalizeTransfers()
        {
            foreach (TransferData data in this.m_transferList)
            {
                if (this.MakeTransfer(data))
                {
                    this.m_tmpItems.Add(data.To);
                }
            }
            this.m_transferList.Clear();
            this.m_transferTime = null;
            using (HashSet<MyEnvironmentItems>.Enumerator enumerator2 = this.m_tmpItems.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    enumerator2.Current.EndBatch(true);
                }
            }
            this.m_tmpItems.Clear();
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            if (this.m_transferTime != null)
            {
                this.FinalizeTransfers();
            }
            return base.GetObjectBuilder();
        }

        public override void LoadData()
        {
            base.LoadData();
            this.m_transferList = new List<TransferData>();
            this.m_tmpItems = new HashSet<MyEnvironmentItems>();
            Static = this;
        }

        private bool MakeTransfer(TransferData data)
        {
            MyEnvironmentItems.ItemInfo info;
            if (!data.From.TryGetItemInfoById(data.LocalId, out info))
            {
                return false;
            }
            data.From.RemoveItem(data.LocalId, true, true);
            if (!data.To.IsBatching)
            {
                data.To.BeginBatch(true);
            }
            data.To.BatchAddItem(info.Transform.Position, data.SubtypeId, true);
            return true;
        }

        private void StartTimer(int updateTimeS)
        {
            if (this.m_transferTime == null)
            {
                this.m_transferTime = new float?((float) updateTimeS);
            }
        }

        public static void TransferItems(MyEnvironmentItems from, MyEnvironmentItems to, int localId, MyStringHash subtypeId, int timeS = 10)
        {
            Static.AddTransferData(from, to, localId, subtypeId);
            Static.StartTimer(timeS);
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            Static = null;
        }

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (this.m_transferTime != null)
            {
                this.m_transferTime = new float?(this.m_transferTime.Value - 0.01666667f);
                float? transferTime = this.m_transferTime;
                float num = 0f;
                if ((transferTime.GetValueOrDefault() < num) & (transferTime != null))
                {
                    this.FinalizeTransfers();
                }
            }
        }

        public override bool IsRequiredByGame =>
            (MyPerGameSettings.Game == GameEnum.ME_GAME);

        [StructLayout(LayoutKind.Sequential)]
        private struct TransferData
        {
            public MyEnvironmentItems From;
            public MyEnvironmentItems To;
            public int LocalId;
            public MyStringHash SubtypeId;
        }
    }
}

