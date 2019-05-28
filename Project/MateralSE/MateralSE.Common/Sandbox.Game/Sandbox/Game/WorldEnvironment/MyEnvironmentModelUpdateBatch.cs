namespace Sandbox.Game.WorldEnvironment
{
    using Sandbox.Definitions;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;

    public class MyEnvironmentModelUpdateBatch : IDisposable
    {
        private Dictionary<MyDefinitionId, ModelList> m_modelPerItemDefinition = new Dictionary<MyDefinitionId, ModelList>();
        private IMyEnvironmentOwner m_owner;
        private MyLogicalEnvironmentSectorBase m_sector;

        public MyEnvironmentModelUpdateBatch(MyLogicalEnvironmentSectorBase sector)
        {
            this.m_sector = sector;
            this.m_owner = this.m_sector.Owner;
        }

        public unsafe void Add(MyDefinitionId modelDef, int item)
        {
            ModelList list;
            if (!this.m_modelPerItemDefinition.TryGetValue(modelDef, out list))
            {
                ModelList* listPtr1;
                list.Items = new List<int>();
                if (modelDef.TypeId.IsNull)
                {
                    list.Model = -1;
                }
                else
                {
                    MyPhysicalModelDefinition def = MyDefinitionManager.Static.GetDefinition<MyPhysicalModelDefinition>(modelDef);
                    listPtr1->Model = (def != null) ? this.m_owner.GetModelId(def) : ((short) (-1));
                }
                listPtr1 = (ModelList*) ref list;
                this.m_modelPerItemDefinition[modelDef] = list;
            }
            list.Items.Add(item);
        }

        public void Dispatch()
        {
            foreach (ModelList list in this.m_modelPerItemDefinition.Values)
            {
                this.m_sector.UpdateItemModelBatch(list.Items, list.Model);
            }
        }

        public void Dispose()
        {
            this.Dispatch();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ModelList
        {
            public List<int> Items;
            public short Model;
        }
    }
}

