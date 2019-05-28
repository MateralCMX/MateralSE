namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox.Definitions;
    using Sandbox.Game.AI;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRageMath;

    public class MyBotSpawningEnvironmentProxy : IMyEnvironmentModuleProxy
    {
        private MyEnvironmentSector m_sector;
        protected readonly MyRandom m_random = new MyRandom();
        protected List<int> m_items;
        protected Queue<int> m_spawnQueue;

        public void Close()
        {
            this.m_spawnQueue.Clear();
        }

        public void CommitLodChange(int lodBefore, int lodAfter)
        {
            if (lodAfter == 0)
            {
                MyEnvironmentBotSpawningSystem.Static.RegisterBotSpawningProxy(this);
            }
            else
            {
                MyEnvironmentBotSpawningSystem.Static.UnregisterBotSpawningProxy(this);
            }
        }

        public void CommitPhysicsChange(bool enabled)
        {
        }

        public void DebugDraw()
        {
        }

        public void HandleSyncEvent(int item, object data, bool fromClient)
        {
        }

        public void Init(MyEnvironmentSector sector, List<int> items)
        {
            this.m_sector = sector;
            this.m_items = items;
            this.m_spawnQueue = new Queue<int>();
            foreach (int num in this.m_items)
            {
                this.m_spawnQueue.Enqueue(num);
            }
        }

        public void OnItemChange(int index, short newModel)
        {
        }

        public void OnItemChangeBatch(List<int> items, int offset, short newModel)
        {
        }

        public bool OnSpawnTick()
        {
            if ((this.m_spawnQueue.Count != 0) && (MyAIComponent.Static.GetAvailableUncontrolledBotsCount() >= 1))
            {
                int count = this.m_spawnQueue.Count;
                int num2 = 0;
                while (num2 < count)
                {
                    num2++;
                    int item = this.m_spawnQueue.Dequeue();
                    this.m_spawnQueue.Enqueue(item);
                    if (this.m_sector.DataView.Items.Count >= item)
                    {
                        Sandbox.Game.WorldEnvironment.ItemInfo info = this.m_sector.DataView.Items[item];
                        Vector3D vectord = this.m_sector.SectorCenter + info.Position;
                        if (MyEnvironmentBotSpawningSystem.Static.IsHumanPlayerWithinRange((Vector3) vectord))
                        {
                            MyRuntimeEnvironmentItemInfo info2;
                            this.m_sector.Owner.GetDefinition((ushort) info.DefinitionIndex, out info2);
                            MyDefinitionId subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_BotCollectionDefinition), info2.Subtype);
                            MyBotCollectionDefinition definition = MyDefinitionManager.Static.GetDefinition<MyBotCollectionDefinition>(subtypeId);
                            using (this.m_random.PushSeed(item.GetHashCode()))
                            {
                                MyDefinitionId id = definition.Bots.Sample(this.m_random);
                                MyAgentDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(id) as MyAgentDefinition;
                                MyAIComponent.Static.SpawnNewBot(botDefinition, info.Position + this.m_sector.SectorCenter, false);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public long SectorId =>
            this.m_sector.SectorId;
    }
}

