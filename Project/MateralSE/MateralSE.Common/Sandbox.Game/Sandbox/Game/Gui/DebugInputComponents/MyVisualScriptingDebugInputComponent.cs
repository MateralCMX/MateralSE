namespace Sandbox.Game.GUI.DebugInputComponents
{
    using Sandbox.Definitions.GUI;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Entities.Inventory;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Gui;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using Sandbox.ModAPI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Definitions;
    using VRage.Game.Entity;
    using VRage.Game.SessionComponents;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;
    using VRageRender;

    public class MyVisualScriptingDebugInputComponent : MyDebugComponent
    {
        private List<MyAreaTriggerComponent> m_queriedTriggers = new List<MyAreaTriggerComponent>();
        private MyAreaTriggerComponent m_selectedTrigger;
        private MatrixD m_lastCapturedCameraMatrix;

        public MyVisualScriptingDebugInputComponent()
        {
            this.AddSwitch(MyKeys.NumPad0, keys => this.ToggleDebugDraw(), new MyDebugComponent.MyRef<bool>(() => MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER, null), "Debug Draw");
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Trigger: Attach new to entity", new Func<bool>(this.TryPutTriggerOnEntity));
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Trigger: Snap to position", new Func<bool>(this.SnapTriggerToPosition));
            this.AddShortcut(MyKeys.NumPad2, true, true, false, false, () => "Trigger: Snap to triggers position", new Func<bool>(this.SnapToTriggersPosition));
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Spawn Trigger", new Func<bool>(this.SpawnTrigger));
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Naming: FatBlock/Floating Object", new Func<bool>(this.TryNamingAnBlockOrFloatingObject));
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Trigger: Select", new Func<bool>(this.SelectTrigger));
            this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Naming: Grid", new Func<bool>(this.TryNamingAGrid));
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Delete trigger", new Func<bool>(this.DeleteTrigger));
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Trigger: Set Size", new Func<bool>(this.SetTriggerSize));
            this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Reset missions + run GameStarted", new Func<bool>(this.ResetMissionsAndRunGameStarted));
            this.AddShortcut(MyKeys.Add, true, false, false, false, () => "Trigger: Enlarge", () => this.ResizeATrigger(true));
            this.AddShortcut(MyKeys.Subtract, true, false, false, false, () => "Trigger: Shrink", () => this.ResizeATrigger(false));
            this.AddShortcut(MyKeys.Multiply, true, false, false, false, () => "Trigger: Rename", new Func<bool>(this.RenameTrigger));
            this.AddShortcut(MyKeys.T, true, true, false, false, () => "Copy camera data", new Func<bool>(this.CopyCameraDataToClipboard));
            this.AddShortcut(MyKeys.N, true, true, false, false, () => "Spawn empty entity", new Func<bool>(this.SpawnEntityDebug));
            this.AddShortcut(MyKeys.B, true, true, false, false, () => "Reload Screen", new Func<bool>(this.ReloadScreen));
            this.m_lastCapturedCameraMatrix = MatrixD.Identity;
        }

        private bool CopyCameraDataToClipboard()
        {
            MatrixD worldMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            object[] objArray1 = new object[] { "Position:  ", worldMatrix.Translation, "\nDirection: ", worldMatrix.Forward, "\nUp:        ", worldMatrix.Up };
            MyClipboardHelper.SetClipboard(string.Concat(objArray1));
            this.m_lastCapturedCameraMatrix = new MatrixD((Matrix) worldMatrix);
            return true;
        }

        public bool DeleteTrigger()
        {
            if (this.m_selectedTrigger == null)
            {
                return false;
            }
            if (this.m_selectedTrigger.Entity.DisplayName == "TriggerHolder")
            {
                this.m_selectedTrigger.Entity.Close();
            }
            else
            {
                this.m_selectedTrigger.Entity.Components.Remove(typeof(MyAreaTriggerComponent), this.m_selectedTrigger);
            }
            this.m_selectedTrigger = null;
            return true;
        }

        public override unsafe void Draw()
        {
            base.Draw();
            if (MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER)
            {
                Vector2 screenCoord = new Vector2(350f, 10f);
                StringBuilder builder = new StringBuilder();
                MyRenderProxy.DebugDrawText2D(screenCoord, "Queried Triggers", Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                foreach (MyAreaTriggerComponent component in this.m_queriedTriggers)
                {
                    float* singlePtr1 = (float*) ref screenCoord.Y;
                    singlePtr1[0] += 20f;
                    builder.Clear();
                    if ((component.Entity != null) && (component.Entity.Name != null))
                    {
                        builder.Append("EntityName: " + component.Entity.Name + " ");
                    }
                    object[] objArray1 = new object[] { "Trigger: ", component.Name, " radius: ", component.Radius };
                    builder.Append(string.Concat(objArray1));
                    MyRenderProxy.DebugDrawText2D(screenCoord, builder.ToString(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                float* singlePtr2 = (float*) ref screenCoord.X;
                singlePtr2[0] += 250f;
                screenCoord.Y = 10f;
                MyRenderProxy.DebugDrawText2D(screenCoord, "Selected Trigger", Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                float* singlePtr3 = (float*) ref screenCoord.Y;
                singlePtr3[0] += 20f;
                builder.Clear();
                if (this.m_selectedTrigger != null)
                {
                    if ((this.m_selectedTrigger.Entity != null) && (this.m_selectedTrigger.Entity.Name != null))
                    {
                        builder.Append("EntityName: " + this.m_selectedTrigger.Entity.Name + " ");
                    }
                    object[] objArray2 = new object[] { "Trigger: ", this.m_selectedTrigger.Name, " radius: ", this.m_selectedTrigger.Radius };
                    builder.Append(string.Concat(objArray2));
                    MyRenderProxy.DebugDrawText2D(screenCoord, builder.ToString(), Color.White, 0.7f, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false);
                }
                if (this.m_lastCapturedCameraMatrix != MatrixD.Identity)
                {
                    MyRenderProxy.DebugDrawAxis(this.m_lastCapturedCameraMatrix, 5f, true, false, false);
                }
            }
        }

        public override string GetName() => 
            "Visual Scripting";

        private void NameDialog(VRage.Game.Entity.MyEntity entity)
        {
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Name a Grid dialog: " + entity.DisplayName, entity.Name ?? (entity.DisplayName + " has no name."), delegate (string text) {
                VRage.Game.Entity.MyEntity entity;
                if (!Sandbox.Game.Entities.MyEntities.TryGetEntityByName(text, out entity))
                {
                    entity.Name = text;
                    Sandbox.Game.Entities.MyEntities.SetEntityName(entity, true);
                    return true;
                }
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Entity with same name already exits, please enter different name."), new StringBuilder("Naming error"), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                return false;
            }));
        }

        private bool ReloadScreen()
        {
            MyObjectBuilder_Definitions definitions;
            string[] strArray = new string[] { Path.Combine(MyFileSystem.ContentPath, "Data", "Hud.sbc"), Path.Combine(MyFileSystem.ContentPath, "Data", "GuiTextures.sbc") };
            MyHudDefinition hudDefinition = MyHud.HudDefinition;
            MyGuiTextureAtlasDefinition definition = MyDefinitionManagerBase.Static.GetDefinition<MyGuiTextureAtlasDefinition>(MyStringHash.GetOrCompute("Base"));
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(strArray[0], out definitions))
            {
                MyAPIGateway.Utilities.ShowNotification("Failed to load Hud.sbc!", 0xbb8, "Red");
                return false;
            }
            hudDefinition.Init(definitions.Definitions[0], hudDefinition.Context);
            if (!MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_Definitions>(strArray[1], out definitions))
            {
                MyAPIGateway.Utilities.ShowNotification("Failed to load GuiTextures.sbc!", 0xbb8, "Red");
                return false;
            }
            definition.Init(definitions.Definitions[0], definition.Context);
            MyScreenManager.CloseScreen(MyPerGameSettings.GUI.HUDScreen);
            MyScreenManager.AddScreen(Activator.CreateInstance(MyPerGameSettings.GUI.HUDScreen) as MyGuiScreenBase);
            return true;
        }

        private bool RenameTrigger()
        {
            if (this.m_selectedTrigger == null)
            {
                return false;
            }
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Rename Dialog", this.m_selectedTrigger.Name, delegate (string text) {
                this.m_selectedTrigger.Name = text;
                return true;
            }));
            return true;
        }

        private bool ResetMissionsAndRunGameStarted()
        {
            MyVisualScriptManagerSessionComponent component = MySession.Static.GetComponent<MyVisualScriptManagerSessionComponent>();
            if (component != null)
            {
                component.Reset();
            }
            return true;
        }

        public bool ResizeATrigger(bool enlarge)
        {
            if (this.m_selectedTrigger == null)
            {
                return false;
            }
            this.m_selectedTrigger.Radius = enlarge ? (this.m_selectedTrigger.Radius + 0.2) : (this.m_selectedTrigger.Radius - 0.2);
            return true;
        }

        private bool SelectTrigger()
        {
            Vector3D position = MyAPIGateway.Session.Camera.Position;
            double maxValue = double.MaxValue;
            if (this.m_selectedTrigger != null)
            {
                this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Red);
            }
            foreach (MyAreaTriggerComponent component in this.m_queriedTriggers)
            {
                double num2 = (component.Center - position).LengthSquared();
                if (num2 < maxValue)
                {
                    maxValue = num2;
                    this.m_selectedTrigger = component;
                }
            }
            if (Math.Abs((double) (maxValue - double.MaxValue)) < double.Epsilon)
            {
                this.m_selectedTrigger = null;
            }
            if (this.m_selectedTrigger != null)
            {
                this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Yellow);
            }
            return true;
        }

        public bool SetTriggerSize()
        {
            if (this.m_selectedTrigger == null)
            {
                return false;
            }
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Set trigger size dialog", this.m_selectedTrigger.Radius.ToString(CultureInfo.InvariantCulture), delegate (string text) {
                float num;
                if (!float.TryParse(text, out num))
                {
                    return false;
                }
                this.m_selectedTrigger.Radius = num;
                return true;
            }));
            return true;
        }

        private bool SnapToTriggersPosition()
        {
            if ((this.m_selectedTrigger != null) && (MySession.Static.ControlledEntity is MyCharacter))
            {
                MySession.Static.LocalCharacter.PositionComp.SetPosition(this.m_selectedTrigger.Center, null, false, true);
            }
            return true;
        }

        private bool SnapTriggerToPosition()
        {
            if (this.m_selectedTrigger == null)
            {
                return false;
            }
            this.m_selectedTrigger.Center = !(MyAPIGateway.Session.CameraController is MySpectatorCameraController) ? MyAPIGateway.Session.LocalHumanPlayer.GetPosition() : MyAPIGateway.Session.Camera.Position;
            return true;
        }

        public static VRage.Game.Entity.MyEntity SpawnEntity(Action<VRage.Game.Entity.MyEntity> onEntity)
        {
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Spawn new Entity", "", delegate (string text) {
                VRage.Game.Entity.MyEntity entity = new VRage.Game.Entity.MyEntity {
                    WorldMatrix = MyAPIGateway.Session.Camera.WorldMatrix
                };
                entity.PositionComp.SetPosition(MyAPIGateway.Session.Camera.Position, null, false, true);
                entity.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                entity.Components.Remove<MyPhysicsComponentBase>();
                entity.Components.Remove<MyRenderComponentBase>();
                entity.DisplayName = "EmptyEntity";
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
                entity.Name = text;
                Sandbox.Game.Entities.MyEntities.SetEntityName(entity, true);
                if (onEntity != null)
                {
                    onEntity(entity);
                }
                return true;
            }));
            return null;
        }

        public bool SpawnEntityDebug()
        {
            SpawnEntity(null);
            return true;
        }

        public bool SpawnTrigger()
        {
            MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Spawn new Trigger", "", delegate (string text) {
                MyAreaTriggerComponent component = new MyAreaTriggerComponent(text);
                VRage.Game.Entity.MyEntity entity = new VRage.Game.Entity.MyEntity();
                component.Radius = 2.0;
                component.Center = MyAPIGateway.Session.Camera.Position;
                entity.PositionComp.SetPosition(MyAPIGateway.Session.Camera.Position, null, false, true);
                entity.PositionComp.LocalVolume = new BoundingSphere(Vector3.Zero, 0.5f);
                entity.EntityId = MyEntityIdentifier.AllocateId(MyEntityIdentifier.ID_OBJECT_TYPE.ENTITY, MyEntityIdentifier.ID_ALLOCATION_METHOD.RANDOM);
                entity.Components.Remove<MyPhysicsComponentBase>();
                entity.Components.Remove<MyRenderComponentBase>();
                entity.DisplayName = "TriggerHolder";
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
                if (!entity.Components.Contains(typeof(MyTriggerAggregate)))
                {
                    entity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
                }
                entity.Components.Get<MyTriggerAggregate>().AddComponent(component);
                if (this.m_selectedTrigger != null)
                {
                    this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Red);
                }
                this.m_selectedTrigger = component;
                this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Yellow);
                return true;
            }));
            return true;
        }

        public bool ToggleDebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = false;
                MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER = false;
            }
            else
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER = true;
            }
            return true;
        }

        public bool TryNamingAGrid()
        {
            MatrixD worldMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(worldMatrix.Translation, worldMatrix.Translation + (worldMatrix.Forward * 5.0), toList, 15);
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = toList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    VRage.Game.Entity.MyEntity hitEntity = (VRage.Game.Entity.MyEntity) enumerator.Current.HkHitInfo.GetHitEntity();
                    if (hitEntity is MyCubeGrid)
                    {
                        this.NameDialog(hitEntity);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryNamingAnBlockOrFloatingObject()
        {
            MatrixD worldMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            RayD yd = new RayD(worldMatrix.Translation + (worldMatrix.Forward * 0.5), worldMatrix.Forward * 1000.0);
            BoundingSphereD boundingSphere = new BoundingSphereD(worldMatrix.Translation, 30.0);
            List<VRage.Game.Entity.MyEntity> entitiesInSphere = Sandbox.Game.Entities.MyEntities.GetEntitiesInSphere(ref boundingSphere);
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(worldMatrix.Translation, worldMatrix.Translation + (worldMatrix.Forward * 5.0), toList, 15);
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = toList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPhysicsBody userObject = (MyPhysicsBody) enumerator.Current.HkHitInfo.Body.UserObject;
                    if (userObject.Entity is MyFloatingObject)
                    {
                        VRage.Game.Entity.MyEntity entity = (VRage.Game.Entity.MyEntity) userObject.Entity;
                        this.NameDialog(entity);
                        return true;
                    }
                }
            }
            using (List<VRage.Game.Entity.MyEntity>.Enumerator enumerator2 = entitiesInSphere.GetEnumerator())
            {
                bool flag;
                while (true)
                {
                    if (enumerator2.MoveNext())
                    {
                        VRage.Game.Entity.MyEntity current = enumerator2.Current;
                        MyCubeGrid grid = current as MyCubeGrid;
                        if (grid == null)
                        {
                            continue;
                        }
                        if (yd.Intersects(current.PositionComp.WorldAABB) == null)
                        {
                            continue;
                        }
                        Vector3I? nullable2 = grid.RayCastBlocks(worldMatrix.Translation, worldMatrix.Translation + (worldMatrix.Forward * 100.0));
                        if (nullable2 == null)
                        {
                            continue;
                        }
                        MySlimBlock block = grid.GetCubeBlock(nullable2.Value);
                        if (block.FatBlock == null)
                        {
                            continue;
                        }
                        MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Name block dialog: " + block.FatBlock.DefinitionDisplayNameText, block.FatBlock.Name ?? (block.FatBlock.DefinitionDisplayNameText + " has no name."), delegate (string text) {
                            VRage.Game.Entity.MyEntity entity;
                            if (!Sandbox.Game.Entities.MyEntities.TryGetEntityByName(text, out entity))
                            {
                                block.FatBlock.Name = text;
                                Sandbox.Game.Entities.MyEntities.SetEntityName(block.FatBlock, true);
                                return true;
                            }
                            MyStringId? okButtonText = null;
                            okButtonText = null;
                            okButtonText = null;
                            okButtonText = null;
                            Vector2? size = null;
                            MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Entity with same name already exits, please enter different name."), new StringBuilder("Naming error"), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                            return false;
                        }));
                        entitiesInSphere.Clear();
                        flag = true;
                    }
                    else
                    {
                        goto TR_0000;
                    }
                    break;
                }
                return flag;
            }
        TR_0000:
            entitiesInSphere.Clear();
            return false;
        }

        private bool TryPutTriggerOnEntity()
        {
            MatrixD worldMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(worldMatrix.Translation, worldMatrix.Translation + (worldMatrix.Forward * 30.0), toList, 15);
            using (List<MyPhysics.HitInfo>.Enumerator enumerator = toList.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    MyPhysicsBody userObject = (MyPhysicsBody) enumerator.Current.HkHitInfo.Body.UserObject;
                    if (userObject.Entity is MyCubeGrid)
                    {
                        VRage.Game.Entity.MyEntity rayEntity = (VRage.Game.Entity.MyEntity) userObject.Entity;
                        MyGuiSandbox.AddScreen(new ValueGetScreenWithCaption("Entity Spawn on: " + rayEntity.DisplayName, "", delegate (string text) {
                            if (this.m_selectedTrigger != null)
                            {
                                this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Red);
                            }
                            this.m_selectedTrigger = new MyAreaTriggerComponent(text);
                            if (!rayEntity.Components.Contains(typeof(MyTriggerAggregate)))
                            {
                                rayEntity.Components.Add(typeof(MyTriggerAggregate), new MyTriggerAggregate());
                            }
                            rayEntity.Components.Get<MyTriggerAggregate>().AddComponent(this.m_selectedTrigger);
                            this.m_selectedTrigger.Center = MyAPIGateway.Session.Camera.Position;
                            this.m_selectedTrigger.Radius = 2.0;
                            this.m_selectedTrigger.CustomDebugColor = new Color?(Color.Yellow);
                            return true;
                        }));
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update10()
        {
            base.Update10();
            if (MyAPIGateway.Session != null)
            {
                this.m_queriedTriggers.Clear();
                foreach (MyAreaTriggerComponent component in MySessionComponentTriggerSystem.Static.GetIntersectingTriggers(MyAPIGateway.Session.Camera.Position))
                {
                    if (component != null)
                    {
                        this.m_queriedTriggers.Add(component);
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyVisualScriptingDebugInputComponent.<>c <>9 = new MyVisualScriptingDebugInputComponent.<>c();
            public static Func<bool> <>9__3_1;
            public static Func<string> <>9__3_2;
            public static Func<string> <>9__3_3;
            public static Func<string> <>9__3_4;
            public static Func<string> <>9__3_5;
            public static Func<string> <>9__3_6;
            public static Func<string> <>9__3_7;
            public static Func<string> <>9__3_8;
            public static Func<string> <>9__3_9;
            public static Func<string> <>9__3_10;
            public static Func<string> <>9__3_11;
            public static Func<string> <>9__3_12;
            public static Func<string> <>9__3_14;
            public static Func<string> <>9__3_16;
            public static Func<string> <>9__3_17;
            public static Func<string> <>9__3_18;
            public static Func<string> <>9__3_19;

            internal bool <.ctor>b__3_1() => 
                (MyDebugDrawSettings.ENABLE_DEBUG_DRAW && MyDebugDrawSettings.DEBUG_DRAW_UPDATE_TRIGGER);

            internal string <.ctor>b__3_10() => 
                "Trigger: Set Size";

            internal string <.ctor>b__3_11() => 
                "Reset missions + run GameStarted";

            internal string <.ctor>b__3_12() => 
                "Trigger: Enlarge";

            internal string <.ctor>b__3_14() => 
                "Trigger: Shrink";

            internal string <.ctor>b__3_16() => 
                "Trigger: Rename";

            internal string <.ctor>b__3_17() => 
                "Copy camera data";

            internal string <.ctor>b__3_18() => 
                "Spawn empty entity";

            internal string <.ctor>b__3_19() => 
                "Reload Screen";

            internal string <.ctor>b__3_2() => 
                "Trigger: Attach new to entity";

            internal string <.ctor>b__3_3() => 
                "Trigger: Snap to position";

            internal string <.ctor>b__3_4() => 
                "Trigger: Snap to triggers position";

            internal string <.ctor>b__3_5() => 
                "Spawn Trigger";

            internal string <.ctor>b__3_6() => 
                "Naming: FatBlock/Floating Object";

            internal string <.ctor>b__3_7() => 
                "Trigger: Select";

            internal string <.ctor>b__3_8() => 
                "Naming: Grid";

            internal string <.ctor>b__3_9() => 
                "Delete trigger";
        }
    }
}

