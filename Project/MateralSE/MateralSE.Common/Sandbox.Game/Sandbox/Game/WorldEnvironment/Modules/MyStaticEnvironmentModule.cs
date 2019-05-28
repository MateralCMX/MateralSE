namespace Sandbox.Game.WorldEnvironment.Modules
{
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities.Planet;
    using Sandbox.Game.WorldEnvironment;
    using Sandbox.Game.WorldEnvironment.Definitions;
    using Sandbox.Game.WorldEnvironment.ObjectBuilders;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.ObjectBuilders;
    using VRageMath;
    using VRageRender;

    public class MyStaticEnvironmentModule : MyEnvironmentModuleBase
    {
        private readonly HashSet<int> m_disabledItems = new HashSet<int>();
        private List<MyOrientedBoundingBoxD> m_boxes;
        private int m_minScannedLod = 15;

        public override void Close()
        {
        }

        public override unsafe void DebugDraw()
        {
            if (this.m_boxes != null)
            {
                for (int i = 0; i < this.m_boxes.Count; i++)
                {
                    MyOrientedBoundingBoxD obb = this.m_boxes[i];
                    Vector3D* vectordPtr1 = (Vector3D*) ref obb.Center;
                    vectordPtr1[0] += base.Sector.WorldPos;
                    MyRenderProxy.DebugDrawOBB(obb, Color.Aquamarine, 0.3f, true, true, false);
                }
            }
        }

        public override MyObjectBuilder_EnvironmentModuleBase GetObjectBuilder()
        {
            if (this.m_disabledItems.Count <= 0)
            {
                return null;
            }
            MyObjectBuilder_StaticEnvironmentModule module1 = new MyObjectBuilder_StaticEnvironmentModule();
            module1.DisabledItems = this.m_disabledItems;
            module1.MinScanned = this.m_minScannedLod;
            MyObjectBuilder_StaticEnvironmentModule module = module1;
            if (this.m_boxes != null)
            {
                foreach (MyOrientedBoundingBoxD xd in this.m_boxes)
                {
                    module.Boxes.Add(xd);
                }
            }
            return module;
        }

        public override void HandleSyncEvent(int logicalItem, object data, bool fromClient)
        {
        }

        public override unsafe void Init(MyLogicalEnvironmentSectorBase sector, MyObjectBuilder_Base ob)
        {
            base.Init(sector, ob);
            MyPlanetEnvironmentComponent owner = (MyPlanetEnvironmentComponent) sector.Owner;
            if (owner.CollisionCheckEnabled)
            {
                this.m_boxes = owner.GetCollidedBoxes(sector.Id);
                if (this.m_boxes != null)
                {
                    this.m_boxes = new List<MyOrientedBoundingBoxD>(this.m_boxes);
                }
            }
            MyObjectBuilder_StaticEnvironmentModule module = (MyObjectBuilder_StaticEnvironmentModule) ob;
            if (module != null)
            {
                HashSet<int> disabledItems = module.DisabledItems;
                foreach (int num in disabledItems)
                {
                    if (!this.m_disabledItems.Contains(num))
                    {
                        this.OnItemEnable(num, false);
                    }
                }
                this.m_disabledItems.UnionWith(disabledItems);
                if ((module.Boxes != null) && (module.MinScanned > 0))
                {
                    this.m_boxes = new List<MyOrientedBoundingBoxD>();
                    foreach (SerializableOrientedBoundingBoxD xd in module.Boxes)
                    {
                        this.m_boxes.Add((MyOrientedBoundingBoxD) xd);
                    }
                    this.m_minScannedLod = module.MinScanned;
                }
            }
            if (this.m_boxes != null)
            {
                Vector3D worldPos = sector.WorldPos;
                for (int i = 0; i < this.m_boxes.Count; i++)
                {
                    MyOrientedBoundingBoxD xd2 = this.m_boxes[i];
                    Vector3D* vectordPtr1 = (Vector3D*) ref xd2.Center;
                    vectordPtr1[0] -= worldPos;
                    this.m_boxes[i] = xd2;
                }
            }
        }

        private bool IsObstructed(int position)
        {
            if (this.m_boxes != null)
            {
                Sandbox.Game.WorldEnvironment.ItemInfo info;
                base.Sector.GetItem(position, out info);
                for (int i = 0; i < this.m_boxes.Count; i++)
                {
                    MyOrientedBoundingBoxD xd = this.m_boxes[i];
                    if (xd.Contains(ref info.Position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnItemEnable(int itemId, bool enabled)
        {
            Sandbox.Game.WorldEnvironment.ItemInfo info;
            if (enabled)
            {
                this.m_disabledItems.Remove(itemId);
            }
            else
            {
                this.m_disabledItems.Add(itemId);
            }
            base.Sector.GetItem(itemId, out info);
            if ((info.ModelIndex >= 0) != enabled)
            {
                short modelId = ~info.ModelIndex;
                base.Sector.UpdateItemModel(itemId, modelId);
            }
        }

        public override unsafe void ProcessItems(Dictionary<short, MyLodEnvironmentItemSet> items, int changedLodMin, int changedLodMax)
        {
            this.m_minScannedLod = changedLodMin;
            using (MyEnvironmentModelUpdateBatch batch = new MyEnvironmentModelUpdateBatch(base.Sector))
            {
                foreach (KeyValuePair<short, MyLodEnvironmentItemSet> pair in items)
                {
                    MyRuntimeEnvironmentItemInfo info;
                    base.Sector.GetItemDefinition((ushort) pair.Key, out info);
                    MyDefinitionId subtypeId = new MyDefinitionId(typeof(MyObjectBuilder_PhysicalModelCollectionDefinition), info.Subtype);
                    MyPhysicalModelCollectionDefinition definition = MyDefinitionManager.Static.GetDefinition<MyPhysicalModelCollectionDefinition>(subtypeId);
                    if (definition != null)
                    {
                        MyLodEnvironmentItemSet set = pair.Value;
                        for (int i = &set.LodOffsets.FixedElementField[changedLodMin]; i < set.Items.Count; i++)
                        {
                            int item = set.Items[i];
                            if (!this.m_disabledItems.Contains(item) && !this.IsObstructed(item))
                            {
                                batch.Add(definition.Items.Sample(MyHashRandomUtils.UniformFloatFromSeed(item)), item);
                            }
                        }
                    }
                }
            }
        }
    }
}

