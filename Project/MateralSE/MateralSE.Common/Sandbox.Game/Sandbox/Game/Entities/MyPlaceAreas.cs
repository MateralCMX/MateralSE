namespace Sandbox.Game.Entities
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRageMath;
    using VRageRender;

    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate), StaticEventOwner]
    public class MyPlaceAreas : MySessionComponentBase
    {
        private MyDynamicAABBTreeD m_aabbTree = new MyDynamicAABBTreeD(MyConstants.GAME_PRUNING_STRUCTURE_AABB_EXTENSION, 1.0);
        public static MyPlaceAreas Static;
        public MyAreaMarkerDefinition AreaMarkerDefinition;

        public MyPlaceAreas()
        {
            Static = this;
        }

        public void AddPlaceArea(MyPlaceArea area)
        {
            if (area.PlaceAreaProxyId == -1)
            {
                BoundingBoxD worldAABB = area.WorldAABB;
                area.PlaceAreaProxyId = this.m_aabbTree.AddProxy(ref worldAABB, area, 0, true);
            }
        }

        public void Clear()
        {
            this.m_aabbTree.Clear();
        }

        [Event(null, 0xc6), Reliable, Server]
        private static void CreateNewPlaceArea(SerializableDefinitionId id, MyPositionAndOrientation positionAndOrientation)
        {
            MyObjectBuilder_AreaMarker objectBuilder = (MyObjectBuilder_AreaMarker) MyObjectBuilderSerializer.CreateNewObject(id);
            objectBuilder.PersistentFlags = MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            objectBuilder.PositionAndOrientation = new MyPositionAndOrientation?(positionAndOrientation);
            MyEntities.CreateFromObjectBuilderAndAdd(objectBuilder, false);
        }

        private void CurrentToolbar_SelectedSlotChanged(MyToolbar toolbar, MyToolbar.SlotArgs args)
        {
            if (!(toolbar.SelectedItem is MyToolbarItemAreaMarker))
            {
                this.AreaMarkerDefinition = null;
            }
        }

        private void CurrentToolbar_SlotActivated(MyToolbar toolbar, MyToolbar.SlotArgs args, bool userActivated)
        {
            if (!(toolbar.GetItemAtIndex(toolbar.SlotToIndex(args.SlotNumber.Value)) is MyToolbarItemAreaMarker))
            {
                this.AreaMarkerDefinition = null;
            }
        }

        private void CurrentToolbar_Unselected(MyToolbar toolbar)
        {
            this.AreaMarkerDefinition = null;
        }

        public void DebugDraw()
        {
            List<MyPlaceArea> elementsList = new List<MyPlaceArea>();
            List<BoundingBoxD> boxsList = new List<BoundingBoxD>();
            this.m_aabbTree.GetAll<MyPlaceArea>(elementsList, true, boxsList);
            for (int i = 0; i < elementsList.Count; i++)
            {
                MyRenderProxy.DebugDrawAABB(boxsList[i], Vector3.One, 1f, 1f, false, false, false);
            }
        }

        public void GetAllAreas(List<MyPlaceArea> result)
        {
            this.m_aabbTree.GetAll<MyPlaceArea>(result, false, null);
        }

        public void GetAllAreasInSphere(BoundingSphereD sphere, List<MyPlaceArea> result)
        {
            this.m_aabbTree.OverlapAllBoundingSphere<MyPlaceArea>(ref sphere, result, false);
        }

        public override void HandleInput()
        {
            base.HandleInput();
            if ((MyScreenManager.GetScreenWithFocus() is MyGuiScreenGamePlay) && ((MyControllerHelper.IsControl(MySpaceBindingCreator.CX_CHARACTER, MyControlsSpace.PRIMARY_TOOL_ACTION, MyControlStateType.NEW_PRESSED, false) && (MySession.Static.ControlledEntity != null)) && (this.AreaMarkerDefinition != null)))
            {
                this.PlaceAreaMarker();
            }
        }

        public override void LoadData()
        {
            base.LoadData();
            MyToolbarComponent.CurrentToolbar.SelectedSlotChanged += new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
            MyToolbarComponent.CurrentToolbar.SlotActivated += new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.Unselected += new Action<MyToolbar>(this.CurrentToolbar_Unselected);
        }

        public void MovePlaceArea(MyPlaceArea area)
        {
            if (area.PlaceAreaProxyId != -1)
            {
                BoundingBoxD worldAABB = area.WorldAABB;
                this.m_aabbTree.MoveProxy(area.PlaceAreaProxyId, ref worldAABB, Vector3.Zero);
            }
        }

        private void PlaceAreaMarker()
        {
            Vector3D position;
            Vector3D forward;
            if ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity))
            {
                position = MySector.MainCamera.Position;
                forward = MySector.MainCamera.WorldMatrix.Forward;
            }
            else
            {
                MatrixD xd = MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
                position = xd.Translation;
                forward = xd.Forward;
            }
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(position, position + (forward * 100.0), toList, 0x18);
            if (toList.Count != 0)
            {
                MyPhysics.HitInfo? nullable = null;
                foreach (MyPhysics.HitInfo info in toList)
                {
                    IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
                    if (hitEntity is MyCubeGrid)
                    {
                        nullable = new MyPhysics.HitInfo?(info);
                    }
                    else
                    {
                        if (!(hitEntity is MyVoxelMap))
                        {
                            continue;
                        }
                        nullable = new MyPhysics.HitInfo?(info);
                    }
                    break;
                }
                if (nullable != null)
                {
                    MyAreaMarkerDefinition areaMarkerDefinition = this.AreaMarkerDefinition;
                    if (areaMarkerDefinition != null)
                    {
                        Vector3D position = nullable.Value.Position;
                        Vector3D forward = Vector3D.Reject(forward, Vector3D.Up);
                        if (Vector3D.IsZero(forward))
                        {
                            forward = Vector3D.Forward;
                        }
                        MyPositionAndOrientation orientation = new MyPositionAndOrientation(position, (Vector3) Vector3D.Normalize(forward), (Vector3) Vector3D.Up);
                        MyObjectBuilder_AreaMarker builder = (MyObjectBuilder_AreaMarker) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) areaMarkerDefinition.Id);
                        builder.PersistentFlags = MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
                        builder.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                        if (builder.IsSynced)
                        {
                            SerializableDefinitionId id = (SerializableDefinitionId) areaMarkerDefinition.Id;
                            EndpointId targetEndpoint = new EndpointId();
                            Vector3D? nullable2 = null;
                            MyMultiplayer.RaiseStaticEvent<SerializableDefinitionId, MyPositionAndOrientation>(x => new Action<SerializableDefinitionId, MyPositionAndOrientation>(MyPlaceAreas.CreateNewPlaceArea), id, orientation, targetEndpoint, nullable2);
                        }
                        else
                        {
                            MyAreaMarker entity = MyEntityFactory.CreateEntity<MyAreaMarker>(builder);
                            entity.Init(builder);
                            MyEntities.Add(entity, true);
                        }
                    }
                }
            }
        }

        public void RemovePlaceArea(MyPlaceArea area)
        {
            if (area.PlaceAreaProxyId != -1)
            {
                this.m_aabbTree.RemoveProxy(area.PlaceAreaProxyId);
                area.PlaceAreaProxyId = -1;
            }
        }

        protected override void UnloadData()
        {
            MyToolbarComponent.CurrentToolbar.SelectedSlotChanged -= new Action<MyToolbar, MyToolbar.SlotArgs>(this.CurrentToolbar_SelectedSlotChanged);
            MyToolbarComponent.CurrentToolbar.SlotActivated -= new Action<MyToolbar, MyToolbar.SlotArgs, bool>(this.CurrentToolbar_SlotActivated);
            MyToolbarComponent.CurrentToolbar.Unselected -= new Action<MyToolbar>(this.CurrentToolbar_Unselected);
            base.UnloadData();
            List<MyPlaceArea> elementsList = new List<MyPlaceArea>();
            this.m_aabbTree.GetAll<MyPlaceArea>(elementsList, false, null);
            using (List<MyPlaceArea>.Enumerator enumerator = elementsList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.PlaceAreaProxyId = -1;
                }
            }
            this.Clear();
        }

        public override Type[] Dependencies =>
            new Type[] { typeof(MyToolbarComponent) };

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyPlaceAreas.<>c <>9 = new MyPlaceAreas.<>c();
            public static Func<IMyEventOwner, Action<SerializableDefinitionId, MyPositionAndOrientation>> <>9__14_0;

            internal Action<SerializableDefinitionId, MyPositionAndOrientation> <PlaceAreaMarker>b__14_0(IMyEventOwner x) => 
                new Action<SerializableDefinitionId, MyPositionAndOrientation>(MyPlaceAreas.CreateNewPlaceArea);
        }
    }
}

