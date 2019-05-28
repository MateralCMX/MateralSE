namespace Sandbox.Game.Screens.DebugScreens.Game
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.Gui;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.ModAPI;
    using VRage.Game.Utils;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    [MyDebugScreen("Game", "Prefabs")]
    public class MyGuiScreenDebugPrefabs : MyGuiScreenDebugBase
    {
        private MyGuiControlCombobox m_groupCombo;
        private MyGuiControlCombobox m_prefabsCombo;
        private MyGuiControlCombobox m_behaviourCombo;
        private MyGuiControlSlider m_frequency;
        private MyGuiControlSlider m_AIactivationDistance;
        private MyGuiControlCheckbox m_isPirate;
        private MyGuiControlCheckbox m_reactorsOn;
        private MyGuiControlCheckbox m_isEncounter;
        private MyGuiControlCheckbox m_resetOwnership;
        private MyGuiControlCheckbox m_isCargoShip;
        private MyGuiControlSlider m_distanceSlider;

        public MyGuiScreenDebugPrefabs() : base(nullable, false)
        {
            this.RecreateControls(true);
        }

        private void ExportPrefab(MyGuiControlButton _)
        {
            string name = MyUtils.StripInvalidChars(MyClipboardComponent.Static.Clipboard.CopiedGridsName);
            MyClipboardComponent.Static.Clipboard.SaveClipboardAsPrefab(name, this.GetExportFineName("Prefabs", name, ".sbc"));
        }

        private unsafe void ExportSpawnGroup(MyGuiControlButton obj)
        {
            List<MyCubeGrid> second = MyEntities.GetEntities().OfType<MyCubeGrid>().ToList<MyCubeGrid>();
            List<MyVoxelBase> first = MyEntities.GetEntities().OfType<MyVoxelBase>().ToList<MyVoxelBase>();
            if (second.Count != 0)
            {
                MySpawnGroupDefinition.SpawnGroupPrefab* prefabPtr1;
                string text4;
                string displayName = second[0].DisplayName;
                string text3 = displayName;
                if (displayName == null)
                {
                    string local1 = displayName;
                    text3 = second[0].Name;
                    if (second[0].Name == null)
                    {
                        string local2 = second[0].Name;
                        string debugName = second[0].DebugName;
                        text3 = debugName ?? "No name";
                    }
                }
                string name = MyUtils.StripInvalidChars(text4);
                string folder = Path.Combine("SpawnGroups", Path.GetFileName(this.GetExportFineName("SpawnGroups", name, string.Empty)));
                Vector3D basePosition = first.Concat<MyEntity>(second).First<MyEntity>().PositionComp.GetPosition();
                MyObjectBuilder_SpawnGroupDefinition builder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_SpawnGroupDefinition>(name);
                builder.Voxels = Array.Empty<MyObjectBuilder_SpawnGroupDefinition.SpawnGroupVoxel>();
                builder.Prefabs = Array.Empty<MyObjectBuilder_SpawnGroupDefinition.SpawnGroupPrefab>();
                MySpawnGroupDefinition definition1 = new MySpawnGroupDefinition();
                definition1.Init(builder, MyModContext.BaseGame);
                definition1.Id = new MyDefinitionId(typeof(MyObjectBuilder_SpawnGroupDefinition), name);
                definition1.Frequency = this.m_frequency.Value;
                definition1.IsPirate = this.m_isPirate.IsChecked;
                definition1.ReactorsOn = this.m_reactorsOn.IsChecked;
                definition1.IsEncounter = this.m_isEncounter.IsChecked;
                definition1.IsCargoShip = this.m_isCargoShip.IsChecked;
                definition1.Voxels.AddRange(first.Select<MyVoxelBase, MySpawnGroupDefinition.SpawnGroupVoxel>(delegate (MyVoxelBase x) {
                    byte[] buffer;
                    x.Storage.Save(out buffer);
                    string str = MyUtils.StripInvalidChars(x.StorageName);
                    string text1 = this.GetExportFineName(folder, str, ".vx2");
                    Directory.CreateDirectory(Path.GetDirectoryName(text1));
                    File.WriteAllBytes(text1, buffer);
                    return new MySpawnGroupDefinition.SpawnGroupVoxel { 
                        CenterOffset = true,
                        StorageName = Path.GetFileNameWithoutExtension(str),
                        Offset = (Vector3) (x.PositionComp.GetPosition() - basePosition)
                    };
                }));
                Vector3D firstGridPosition = second[0].PositionComp.GetPosition();
                string path = this.GetExportFineName(folder, name, ".sbc");
                MySpawnGroupDefinition.SpawnGroupPrefab item = new MySpawnGroupDefinition.SpawnGroupPrefab {
                    Speed = 0f,
                    ResetOwnership = this.m_resetOwnership.IsChecked,
                    Position = (Vector3) (firstGridPosition - basePosition),
                    BeaconText = string.Empty,
                    PlaceToGridOrigin = false,
                    SubtypeId = MyPrefabManager.SavePrefabToPath(Path.GetFileNameWithoutExtension(path), path, second.Select<MyCubeGrid, MyObjectBuilder_CubeGrid>(delegate (MyCubeGrid x) {
                        MyObjectBuilder_CubeGrid objectBuilder = (MyObjectBuilder_CubeGrid) x.GetObjectBuilder(false);
                        MyPositionAndOrientation orientation = objectBuilder.PositionAndOrientation.Value;
                        orientation.Position -= firstGridPosition;
                        objectBuilder.PositionAndOrientation = new MyPositionAndOrientation?(orientation);
                        foreach (MyObjectBuilder_CubeBlock local1 in objectBuilder.CubeBlocks)
                        {
                            local1.Owner = 0L;
                            local1.BuiltBy = 0L;
                        }
                        return objectBuilder;
                    }).ToList<MyObjectBuilder_CubeGrid>()).Id.SubtypeId,
                    BehaviourActivationDistance = this.m_AIactivationDistance.Value
                };
                prefabPtr1->Behaviour = (this.m_behaviourCombo.GetSelectedKey() == 0) ? null : this.m_behaviourCombo.GetSelectedValue().ToString();
                prefabPtr1 = (MySpawnGroupDefinition.SpawnGroupPrefab*) ref item;
                definition1.Prefabs.Add(item);
                string filepath = this.GetExportFineName(folder, "Group__" + name, ".sbc");
                definition1.GetObjectBuilder().Save(filepath);
                MyHud.Notifications.Add(new MyHudNotificationDebug("Group saved: " + filepath, 0x2710, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
            }
        }

        private string GetExportFineName(string folder, string name, string extension)
        {
            int num = 0;
            while (true)
            {
                num++;
                string str2 = name + ((num == 0) ? string.Empty : ("_" + num.ToString())) + extension;
                string path = Path.Combine(MyFileSystem.UserDataPath, "Export", folder, str2);
                if (!MyFileSystem.FileExists(path) && !MyFileSystem.DirectoryExists(path))
                {
                    return path;
                }
            }
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugPrefabs";

        private void OnSpawnCargoShip(MyGuiControlButton obj)
        {
            if (!(MyFakes.ENABLE_CARGO_SHIPS && MySession.Static.CargoShipsEnabled))
            {
                MyHud.Notifications.Add(new MyHudNotificationDebug("Cargo ships are disabled in this world", 0x1388, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
            }
            else
            {
                MyGlobalEventBase eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip"));
                MyGlobalEvents.RemoveGlobalEvent(eventById);
                eventById.SetActivationTime(TimeSpan.Zero);
                MyGlobalEvents.AddGlobalEvent(eventById);
                MyHud.Notifications.Add(new MyHudNotificationDebug("Cargo ship will spawn soon(tm)", 0x1388, "White", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, MyNotificationLevel.Debug));
            }
        }

        private void OnSpawnPrefab(MyGuiControlButton _)
        {
            MyCamera mainCamera = MySector.MainCamera;
            MyPrefabDefinition definition = MyDefinitionManager.Static.GetPrefabDefinitions()[this.m_prefabsCombo.GetSelectedValue().ToString()];
            float radius = definition.BoundingSphere.Radius;
            Vector3D position = mainCamera.Position + (mainCamera.ForwardVector * (this.m_distanceSlider.Value + Math.Min((float) 100f, (float) (radius * 1.5f))));
            List<MyCubeGrid> grids = new List<MyCubeGrid>();
            Stack<Action> callbacks = new Stack<Action>();
            if (this.m_behaviourCombo.GetSelectedKey() != 0)
            {
                string AI = this.m_behaviourCombo.GetSelectedValue().ToString();
                callbacks.Push(delegate {
                    if (grids.Count > 0)
                    {
                        MyCubeGrid local1 = grids[0];
                        MyVisualScriptLogicProvider.SetName(local1.EntityId, "Drone");
                        MyVisualScriptLogicProvider.SetDroneBehaviourAdvanced("Drone", AI, true, true, null, false, MyEntities.GetEntities().OfType<MyCharacter>().Cast<MyEntity>().ToList<MyEntity>());
                        MyVisualScriptLogicProvider.SetName(local1.EntityId, null);
                    }
                });
            }
            MatrixD xd = MatrixD.CreateTranslation(position);
            Vector3 initialLinearVelocity = new Vector3();
            initialLinearVelocity = new Vector3();
            MyPrefabManager.Static.SpawnPrefab(grids, definition.Id.SubtypeName, position, (Vector3) xd.Forward, (Vector3) xd.Up, initialLinearVelocity, initialLinearVelocity, null, null, SpawningOptions.UseGridOrigin, 0L, false, callbacks);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector = new Vector2(0f, 0.03f);
            base.BackgroundColor = new VRageMath.Vector4(1f, 1f, 1f, 0.5f);
            base.m_currentPosition = (-base.m_size.Value / 2f) + new Vector2(0.02f, 0.13f);
            Vector2? captionOffset = null;
            base.AddCaption("Prefabs", new VRageMath.Vector4?(Color.Yellow.ToVector4()), captionOffset, 0.8f);
            base.AddShareFocusHint();
            base.m_currentPosition += vector;
            VRageMath.Vector4? textColor = null;
            captionOffset = null;
            MyGuiControlButton button1 = base.AddButton("Export clipboard as prefab", new Action<MyGuiControlButton>(this.ExportPrefab), null, textColor, captionOffset);
            button1.VisualStyle = MyGuiControlButtonStyleEnum.Default;
            button1.Size *= new Vector2(4f, 1f);
            base.m_currentPosition += vector;
            this.m_isPirate = base.AddCheckBox("IsPirate", true);
            this.m_reactorsOn = base.AddCheckBox("ReactorsOn", true);
            this.m_isEncounter = base.AddCheckBox("IsEncounter", true);
            this.m_resetOwnership = base.AddCheckBox("ResetOwnership", true);
            this.m_isCargoShip = base.AddCheckBox("IsCargoShip", false);
            textColor = null;
            this.m_frequency = base.AddSlider("Frequency", (float) 1f, (float) 0f, (float) 10f, textColor);
            textColor = null;
            this.m_AIactivationDistance = base.AddSlider("AI activation distance", (float) 1000f, (float) 1f, (float) 10000f, textColor);
            textColor = null;
            this.m_distanceSlider = base.AddSlider("Spawn distance", (float) 100f, (float) 1f, (float) 10000f, textColor);
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.02f;
            textColor = null;
            captionOffset = null;
            this.m_behaviourCombo = base.AddCombo(null, textColor, captionOffset, 10);
            int? sortOrder = null;
            this.m_behaviourCombo.AddItem(0L, "No AI", sortOrder, null);
            foreach (string str in MyDroneAIDataStatic.Presets.Keys)
            {
                sortOrder = null;
                this.m_behaviourCombo.AddItem((long) this.m_behaviourCombo.GetItemsCount(), str, sortOrder, null);
            }
            this.m_behaviourCombo.SelectItemByIndex(0);
            textColor = null;
            captionOffset = null;
            MyGuiControlButton button = base.AddButton("Export world as spawn group", new Action<MyGuiControlButton>(this.ExportSpawnGroup), null, textColor, captionOffset);
            button.VisualStyle = MyGuiControlButtonStyleEnum.Default;
            button.Size *= new Vector2(4f, 1f);
            base.m_currentPosition += vector;
            textColor = null;
            captionOffset = null;
            this.m_groupCombo = base.AddCombo(null, textColor, captionOffset, 10);
            foreach (MySpawnGroupDefinition definition in MyDefinitionManager.Static.GetSpawnGroupDefinitions())
            {
                sortOrder = null;
                this.m_groupCombo.AddItem((long) ((int) definition.Id.SubtypeId), definition.Id.SubtypeName, sortOrder, null);
            }
            this.m_groupCombo.SelectItemByIndex(0);
            textColor = null;
            captionOffset = null;
            base.AddButton("Spawn group", new Action<MyGuiControlButton>(this.SpawnGroup), null, textColor, captionOffset);
            base.m_currentPosition += vector;
            textColor = null;
            captionOffset = null;
            this.m_prefabsCombo = base.AddCombo(null, textColor, captionOffset, 10);
            foreach (MyPrefabDefinition definition2 in MyDefinitionManager.Static.GetPrefabDefinitions().Values)
            {
                sortOrder = null;
                this.m_prefabsCombo.AddItem((long) ((int) definition2.Id.SubtypeId), definition2.Id.SubtypeName, sortOrder, null);
            }
            this.m_prefabsCombo.SelectItemByIndex(0);
            textColor = null;
            captionOffset = null;
            base.AddButton("Spawn prefab", new Action<MyGuiControlButton>(this.OnSpawnPrefab), null, textColor, captionOffset);
            textColor = null;
            captionOffset = null;
            base.AddButton("Summon cargo ship spawn", new Action<MyGuiControlButton>(this.OnSpawnCargoShip), null, textColor, captionOffset).VisualStyle = MyGuiControlButtonStyleEnum.Default;
            button.Size *= new Vector2(4f, 1f);
        }

        private void SpawnGroup(MyGuiControlButton _)
        {
            MySpawnGroupDefinition local1 = MyDefinitionManager.Static.GetSpawnGroupDefinitions()[this.m_groupCombo.GetSelectedIndex()];
            var list = (from x in local1.Prefabs select new { 
                Position = x.Position,
                Prefab = MyDefinitionManager.Static.GetPrefabDefinition(x.SubtypeId)
            }).ToList();
            var list2 = (from x in local1.Voxels.Select(delegate (MySpawnGroupDefinition.SpawnGroupVoxel x) {
                MyOctreeStorage storage = (MyOctreeStorage) MyStorageBase.LoadFromFile(MyWorldGenerator.GetVoxelPrefabPath(x.StorageName), null, true);
                return (storage != null) ? new { 
                    Voxel = storage,
                    Position = x.Offset,
                    Name = x.StorageName
                } : null;
            })
                where x != null
                select x).ToList();
            BoundingSphere sphere = BoundingSphere.CreateInvalid();
            foreach (var type in list)
            {
                Vector3 position = type.Position;
                BoundingSphere boundingSphere = type.Prefab.BoundingSphere;
                sphere.Include(boundingSphere.Translate(ref position));
            }
            foreach (var type2 in list2)
            {
                Vector3I size = type2.Voxel.Size;
                sphere.Include(new BoundingSphere(type2.Position, (float) size.AbsMax()));
            }
            MyCamera mainCamera = MySector.MainCamera;
            float radius = sphere.Radius;
            Vector3D vectord = mainCamera.Position + (mainCamera.ForwardVector * (this.m_distanceSlider.Value + Math.Min((float) 100f, (float) (radius * 1.5f))));
            foreach (var type3 in list2)
            {
                MatrixD worldMatrix = MatrixD.CreateWorld(vectord + type3.Position);
                MyWorldGenerator.AddVoxelMap(type3.Name, type3.Voxel, worldMatrix, 0L, false, false);
            }
            foreach (var type4 in list)
            {
                Vector3D position = vectord + type4.Position;
                MatrixD xd2 = MatrixD.CreateTranslation(position);
                Vector3 initialLinearVelocity = new Vector3();
                initialLinearVelocity = new Vector3();
                MyPrefabManager.Static.SpawnPrefab(type4.Prefab.Id.SubtypeName, position, (Vector3) xd2.Forward, (Vector3) xd2.Up, initialLinearVelocity, initialLinearVelocity, null, null, SpawningOptions.UseGridOrigin, 0L, false, null);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugPrefabs.<>c <>9 = new MyGuiScreenDebugPrefabs.<>c();
            public static Func<MySpawnGroupDefinition.SpawnGroupPrefab, <>f__AnonymousType0<Vector3, MyPrefabDefinition>> <>9__16_0;
            public static Func<MySpawnGroupDefinition.SpawnGroupVoxel, <>f__AnonymousType1<MyOctreeStorage, Vector3, string>> <>9__16_1;
            public static Func<<>f__AnonymousType1<MyOctreeStorage, Vector3, string>, bool> <>9__16_2;

            internal <>f__AnonymousType0<Vector3, MyPrefabDefinition> <SpawnGroup>b__16_0(MySpawnGroupDefinition.SpawnGroupPrefab x) => 
                new { 
                    Position = x.Position,
                    Prefab = MyDefinitionManager.Static.GetPrefabDefinition(x.SubtypeId)
                };

            internal <>f__AnonymousType1<MyOctreeStorage, Vector3, string> <SpawnGroup>b__16_1(MySpawnGroupDefinition.SpawnGroupVoxel x)
            {
                MyOctreeStorage storage = (MyOctreeStorage) MyStorageBase.LoadFromFile(MyWorldGenerator.GetVoxelPrefabPath(x.StorageName), null, true);
                return ((storage != null) ? new { 
                    Voxel = storage,
                    Position = x.Offset,
                    Name = x.StorageName
                } : null);
            }

            internal bool <SpawnGroup>b__16_2(<>f__AnonymousType1<MyOctreeStorage, Vector3, string> x) => 
                (x != null);
        }
    }
}

