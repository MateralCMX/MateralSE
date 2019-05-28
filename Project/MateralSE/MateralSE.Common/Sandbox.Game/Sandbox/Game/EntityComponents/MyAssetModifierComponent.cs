namespace Sandbox.Game.EntityComponents
{
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Networking;
    using Sandbox.Engine.Platform;
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ObjectBuilders.ComponentSystem;
    using VRage.GameServices;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRageRender;

    [StaticEventOwner, ProtoContract, MyComponentBuilder(typeof(MyObjectBuilder_AssetModifierComponent), true)]
    public class MyAssetModifierComponent : MyEntityComponentBase
    {
        private List<MyDefinitionId> m_assetModifiers;
        private MySessionComponentAssetModifiers m_sessionComponent;

        public MyAssetModifierComponent()
        {
            this.InitSessionComponent();
        }

        private void AddAssetModifier(MyDefinitionId id, MyGameInventoryItemSlot itemSlot)
        {
            if (this.m_assetModifiers == null)
            {
                this.m_assetModifiers = new List<MyDefinitionId>();
            }
            if (!Sandbox.Engine.Platform.Game.IsDedicated)
            {
                for (int i = 0; i < this.m_assetModifiers.Count; i++)
                {
                    MyDefinitionId item = this.m_assetModifiers[i];
                    MyGameInventoryItemDefinition inventoryItemDefinition = MyGameService.GetInventoryItemDefinition(item.SubtypeName);
                    if (inventoryItemDefinition == null)
                    {
                        this.m_assetModifiers.Remove(item);
                        i--;
                    }
                    else if (inventoryItemDefinition.ItemSlot == itemSlot)
                    {
                        this.m_assetModifiers.Remove(item);
                        i--;
                    }
                }
            }
            this.m_assetModifiers.Add(id);
        }

        [Event(null, 0xef), Reliable, Server, Broadcast]
        public static void ApplyAssetModifierSync(long entityId, byte[] checkData, bool addToList)
        {
            if ((!Sandbox.Engine.Platform.Game.IsDedicated && MyGameService.IsActive) && (checkData != null))
            {
                bool checkResult = false;
                List<MyGameInventoryItem> list = MyGameService.CheckItemData(checkData, out checkResult);
                if (checkResult)
                {
                    foreach (MyGameInventoryItem item in list)
                    {
                        if (MyGameService.GetInventoryItemDefinition(item.ItemDefinition.AssetModifierId) == null)
                        {
                            break;
                        }
                        MyEntity entityById = MyEntities.GetEntityById(entityId, false);
                        Dictionary<string, MyTextureChange> assetModifierDefinitionForRender = MyDefinitionManager.Static.GetAssetModifierDefinitionForRender(item.ItemDefinition.AssetModifierId);
                        if ((entityById != null) && (assetModifierDefinitionForRender != null))
                        {
                            MyAssetModifierComponent component;
                            if (addToList && entityById.Components.TryGet<MyAssetModifierComponent>(out component))
                            {
                                MyDefinitionId id = new MyDefinitionId(typeof(MyObjectBuilder_AssetModifierDefinition), item.ItemDefinition.AssetModifierId);
                                component.AddAssetModifier(id, item.ItemDefinition.ItemSlot);
                            }
                            if ((entityById.Render != null) && (entityById.Render.RenderObjectIDs[0] != uint.MaxValue))
                            {
                                MyRenderProxy.ChangeMaterialTexture(entityById.Render.RenderObjectIDs[0], assetModifierDefinitionForRender);
                            }
                        }
                    }
                }
            }
        }

        public override void Deserialize(MyObjectBuilder_ComponentBase builder)
        {
            base.Deserialize(builder);
            MyObjectBuilder_AssetModifierComponent component = builder as MyObjectBuilder_AssetModifierComponent;
            if (((component.AssetModifiers != null) && (component.AssetModifiers.Count > 0)) && ((this.m_assetModifiers == null) || (this.m_assetModifiers.Count == 0)))
            {
                this.m_assetModifiers = new List<MyDefinitionId>();
                foreach (SerializableDefinitionId id in component.AssetModifiers)
                {
                    this.m_assetModifiers.Add(id);
                }
                this.InitSessionComponent();
                this.m_sessionComponent.RegisterComponentForLazyUpdate(this);
            }
        }

        private void InitSessionComponent()
        {
            if ((this.m_sessionComponent == null) && (MySession.Static != null))
            {
                this.m_sessionComponent = MySession.Static.GetComponent<MySessionComponentAssetModifiers>();
            }
        }

        public override bool IsSerialized() => 
            true;

        public bool LazyUpdate()
        {
            if ((this.m_assetModifiers != null) && (base.Entity != null))
            {
                MyEntity entityById = MyEntities.GetEntityById(base.Entity.EntityId, false);
                if (entityById == null)
                {
                    goto TR_0000;
                }
                else if (entityById.InScene)
                {
                    using (List<MyDefinitionId>.Enumerator enumerator = this.m_assetModifiers.GetEnumerator())
                    {
                        while (true)
                        {
                            if (!enumerator.MoveNext())
                            {
                                break;
                            }
                            MyDefinitionId current = enumerator.Current;
                            if (MyGameService.IsActive)
                            {
                                MyGameInventoryItemDefinition inventoryItemDefinition = MyGameService.GetInventoryItemDefinition(current.SubtypeName);
                                if (inventoryItemDefinition != null)
                                {
                                    MyGameInventoryItemSlot itemSlot = inventoryItemDefinition.ItemSlot;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            ApplyAssetModifierSync(base.Entity.EntityId, null, false);
                        }
                    }
                }
                else
                {
                    goto TR_0000;
                }
            }
            return true;
        TR_0000:
            return false;
        }

        public override void OnRemovedFromScene()
        {
            base.OnRemovedFromScene();
            this.m_sessionComponent = null;
        }

        private void RemoveModifiers(MyEntity entity, MyGameInventoryItemSlot slot)
        {
            if (this.m_assetModifiers != null)
            {
                for (int i = 0; i < this.m_assetModifiers.Count; i++)
                {
                    MyDefinitionId item = this.m_assetModifiers[i];
                    MyGameInventoryItemDefinition inventoryItemDefinition = MyGameService.GetInventoryItemDefinition(item.SubtypeName);
                    if ((inventoryItemDefinition == null) || (inventoryItemDefinition.ItemSlot == slot))
                    {
                        this.m_assetModifiers.Remove(item);
                        i--;
                        if (entity.Render != null)
                        {
                            switch (slot)
                            {
                                case MyGameInventoryItemSlot.Face:
                                    SetDefaultTextures(entity, "Astronaut_head");
                                    return;

                                case MyGameInventoryItemSlot.Helmet:
                                    SetDefaultTextures(entity, "Head");
                                    SetDefaultTextures(entity, "Astronaut_head");
                                    SetDefaultTextures(entity, "Spacesuit_hood");
                                    return;

                                case MyGameInventoryItemSlot.Gloves:
                                    SetDefaultTextures(entity, "LeftGlove");
                                    SetDefaultTextures(entity, "RightGlove");
                                    return;

                                case MyGameInventoryItemSlot.Boots:
                                    SetDefaultTextures(entity, "Boots");
                                    return;

                                case MyGameInventoryItemSlot.Suit:
                                    SetDefaultTextures(entity, "Arms");
                                    SetDefaultTextures(entity, "RightArm");
                                    SetDefaultTextures(entity, "Gear");
                                    SetDefaultTextures(entity, "Cloth");
                                    SetDefaultTextures(entity, "Emissive");
                                    SetDefaultTextures(entity, "Backpack");
                                    return;

                                case MyGameInventoryItemSlot.Rifle:
                                    ResetRifle(entity);
                                    return;

                                case MyGameInventoryItemSlot.Welder:
                                    ResetWelder(entity);
                                    return;

                                case MyGameInventoryItemSlot.Grinder:
                                    ResetGrinder(entity);
                                    return;

                                case MyGameInventoryItemSlot.Drill:
                                    ResetDrill(entity);
                                    break;

                                default:
                                    return;
                            }
                        }
                        return;
                    }
                }
            }
        }

        [Event(null, 0x7b), Reliable, Server, Broadcast]
        private static void ResetAssetModifierSync(long entityId, MyGameInventoryItemSlot slot)
        {
            MyAssetModifierComponent component;
            MyEntity entityById = MyEntities.GetEntityById(entityId, false);
            if ((entityById != null) && entityById.Components.TryGet<MyAssetModifierComponent>(out component))
            {
                component.RemoveModifiers(entityById, slot);
            }
        }

        public static void ResetDrill(MyEntity entity)
        {
            MyRenderProxy.ChangeMaterialTexture(entity.Render.RenderObjectIDs[0], "HandDrill", null, null, null, null);
        }

        public static void ResetGrinder(MyEntity entity)
        {
            MyRenderProxy.ChangeMaterialTexture(entity.Render.RenderObjectIDs[0], "AngleGrinder", null, null, null, null);
        }

        public static void ResetRifle(MyEntity entity)
        {
            MyRenderProxy.ChangeMaterialTexture(entity.Render.RenderObjectIDs[0], "AutomaticRifle", null, null, null, null);
        }

        public void ResetSlot(MyGameInventoryItemSlot slot)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, MyGameInventoryItemSlot>(x => new Action<long, MyGameInventoryItemSlot>(MyAssetModifierComponent.ResetAssetModifierSync), base.Entity.EntityId, slot, targetEndpoint, position);
        }

        public static void ResetWelder(MyEntity entity)
        {
            MyRenderProxy.ChangeMaterialTexture(entity.Render.RenderObjectIDs[0], "Welder", null, null, null, null);
        }

        public override MyObjectBuilder_ComponentBase Serialize(bool copy = false)
        {
            MyObjectBuilder_AssetModifierComponent component = base.Serialize(copy) as MyObjectBuilder_AssetModifierComponent;
            if ((this.m_assetModifiers != null) && (this.m_assetModifiers.Count > 0))
            {
                component.AssetModifiers = new List<SerializableDefinitionId>();
                foreach (MyDefinitionId id in this.m_assetModifiers)
                {
                    component.AssetModifiers.Add((SerializableDefinitionId) id);
                }
            }
            return component;
        }

        public static void SetDefaultTextures(MyEntity entity, string materialName)
        {
            MyRenderProxy.ChangeMaterialTexture(entity.Render.RenderObjectIDs[0], materialName, null, null, null, null);
        }

        public bool TryAddAssetModifier(byte[] checkData)
        {
            if (((base.Entity == null) || base.Entity.Closed) || !base.Entity.InScene)
            {
                return false;
            }
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<long, byte[], bool>(x => new Action<long, byte[], bool>(MyAssetModifierComponent.ApplyAssetModifierSync), base.Entity.EntityId, checkData, true, targetEndpoint, position);
            return true;
        }

        [ProtoMember(0x20, IsRequired=false)]
        public List<MyDefinitionId> AssetModifiers =>
            this.m_assetModifiers;

        public override string ComponentTypeDebugString =>
            "Asset Modifier Component";

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyAssetModifierComponent.<>c <>9 = new MyAssetModifierComponent.<>c();
            public static Func<IMyEventOwner, Action<long, MyGameInventoryItemSlot>> <>9__9_0;
            public static Func<IMyEventOwner, Action<long, byte[], bool>> <>9__17_0;

            internal Action<long, MyGameInventoryItemSlot> <ResetSlot>b__9_0(IMyEventOwner x) => 
                new Action<long, MyGameInventoryItemSlot>(MyAssetModifierComponent.ResetAssetModifierSync);

            internal Action<long, byte[], bool> <TryAddAssetModifier>b__17_0(IMyEventOwner x) => 
                new Action<long, byte[], bool>(MyAssetModifierComponent.ApplyAssetModifierSync);
        }
    }
}

