namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.AI;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI;
    using VRage.Game.ObjectBuilders.AI.Bot;
    using VRage.Game.Utils;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;

    public class MyMartinInputComponent : MyDebugComponent
    {
        private List<MyMarker> m_markers = new List<MyMarker>();

        public MyMartinInputComponent()
        {
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Add bots", new Func<bool>(this.AddBots));
            this.AddShortcut(MyKeys.Z, true, false, false, false, () => "One AI step", new Func<bool>(this.OneAIStep));
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "One Voxel step", new Func<bool>(this.OneVoxelStep));
            this.AddShortcut(MyKeys.Insert, true, false, false, false, () => "Add one bot", new Func<bool>(this.AddOneBot));
            this.AddShortcut(MyKeys.Home, true, false, false, false, () => "Add one barb", new Func<bool>(this.AddOneBarb));
            this.AddShortcut(MyKeys.T, true, false, false, false, () => "Do some action", new Func<bool>(this.DoSomeAction));
            this.AddShortcut(MyKeys.Y, true, false, false, false, () => "Clear some action", new Func<bool>(this.ClearSomeAction));
            this.AddShortcut(MyKeys.B, true, false, false, false, () => "Add berries", new Func<bool>(this.AddBerries));
            this.AddShortcut(MyKeys.L, true, false, false, false, () => "return to Last bot memory", new Func<bool>(this.ReturnToLastMemory));
            this.AddShortcut(MyKeys.N, true, false, false, false, () => "select Next bot", new Func<bool>(this.SelectNextBot));
            this.AddShortcut(MyKeys.K, true, false, false, false, () => "Kill not selected bots", new Func<bool>(this.KillNotSelectedBots));
            this.AddShortcut(MyKeys.M, true, false, false, false, () => "Toggle marker", new Func<bool>(this.ToggleMarker));
            this.AddSwitch(MyKeys.NumPad0, new Func<MyKeys, bool>(this.SwitchSwitch), new MyDebugComponent.MyRef<bool>(() => MyFakes.DEBUG_BEHAVIOR_TREE, val => MyFakes.DEBUG_BEHAVIOR_TREE = val), "allowed debug beh tree");
            this.AddSwitch(MyKeys.NumPad1, new Func<MyKeys, bool>(this.SwitchSwitch), new MyDebugComponent.MyRef<bool>(() => MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP, val => MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP = val), "one beh tree step");
            this.AddSwitch(MyKeys.H, new Func<MyKeys, bool>(this.SwitchSwitch), new MyDebugComponent.MyRef<bool>(() => MyFakes.ENABLE_AUTO_HEAL, val => MyFakes.ENABLE_AUTO_HEAL = val), "enable auto Heal");
        }

        private bool AddBerries()
        {
            this.AddSomething("Berries", 10);
            return true;
        }

        private bool AddBots()
        {
            for (int i = 0; i < 10; i++)
            {
                MyAgentDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_HumanoidBot), "TestingBarbarian")) as MyAgentDefinition;
                MyAIComponent.Static.SpawnNewBot(botDefinition);
            }
            return true;
        }

        private void AddMarker(MyMarker marker)
        {
            this.m_markers.Add(marker);
        }

        private bool AddOneBarb()
        {
            MyAgentDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_HumanoidBot), "SwordBarbarian")) as MyAgentDefinition;
            MyAIComponent.Static.SpawnNewBot(botDefinition);
            return true;
        }

        private bool AddOneBot()
        {
            MyAgentDefinition botDefinition = MyDefinitionManager.Static.GetBotDefinition(new MyDefinitionId(typeof(MyObjectBuilder_HumanoidBot), "NormalPeasant")) as MyAgentDefinition;
            MyAIComponent.Static.SpawnNewBot(botDefinition);
            return true;
        }

        private void AddSomething(string something, int amount)
        {
            foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyPhysicalItemDefinition definition = base2 as MyPhysicalItemDefinition;
                if ((definition != null) && (definition.CanSpawnFromScreen && (base2.DisplayNameText == something)))
                {
                    MyInventory inventory = (MySession.Static.ControlledEntity as MyEntity).GetInventory(0);
                    if (inventory != null)
                    {
                        MyObjectBuilder_PhysicalObject objectBuilder = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) base2.Id);
                        inventory.DebugAddItems(amount, objectBuilder);
                    }
                    break;
                }
            }
        }

        private void CheckAutoHeal()
        {
            if (MyFakes.ENABLE_AUTO_HEAL)
            {
                MyCharacter controlledEntity = MySession.Static.ControlledEntity as MyCharacter;
                if (((controlledEntity != null) && (controlledEntity.StatComp != null)) && (controlledEntity.StatComp.HealthRatio < 1f))
                {
                    this.AddSomething("Berries", 1);
                    this.ConsumeSomething("Berries", 1);
                }
            }
        }

        private bool ClearSomeAction() => 
            true;

        private void ConsumeSomething(string something, int amount)
        {
            foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetAllDefinitions())
            {
                MyPhysicalItemDefinition definition = base2 as MyPhysicalItemDefinition;
                if ((definition != null) && (definition.CanSpawnFromScreen && (base2.DisplayNameText == something)))
                {
                    MyInventory inventory = (MySession.Static.ControlledEntity as MyEntity).GetInventory(0);
                    if (inventory != null)
                    {
                        MyObjectBuilder_PhysicalObject obj1 = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) base2.Id);
                        inventory.ConsumeItem(definition.Id, amount, MySession.Static.LocalCharacterEntityId);
                    }
                    break;
                }
            }
        }

        private bool DoSomeAction() => 
            true;

        public override unsafe void Draw()
        {
            base.Draw();
            foreach (MyMarker marker in this.m_markers)
            {
                MyRenderProxy.DebugDrawSphere(marker.position, 0.5f, marker.color, 0.8f, true, false, true, false);
                MyRenderProxy.DebugDrawSphere(marker.position, 0.1f, marker.color, 1f, false, false, true, false);
                Vector3D position = marker.position;
                double* numPtr1 = (double*) ref position.Y;
                numPtr1[0] += 0.60000002384185791;
                string text = $"{marker.position.X:0.0},{marker.position.Y:0.0},{marker.position.Z:0.0}";
                MyRenderProxy.DebugDrawText3D(position, text, marker.color, 1f, false, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, -1, false);
            }
        }

        private MyMarker FindClosestMarkerInArea(Vector3D pos, double maxDistance)
        {
            double maxValue = double.MaxValue;
            MyMarker marker = null;
            foreach (MyMarker marker2 in this.m_markers)
            {
                double num2 = (marker2.position - pos).Length();
                if (num2 < maxValue)
                {
                    marker = marker2;
                    maxValue = num2;
                }
            }
            return ((maxValue >= maxDistance) ? null : marker);
        }

        public bool GetDirectedPositionOnGround(Vector3D initPosition, Vector3D direction, float amount, out Vector3D outPosition, float raycastHeight = 100f)
        {
            outPosition = new Vector3D();
            MyVoxelBase base2 = MySession.Static.VoxelMaps.TryGetVoxelMapByNameStart("Ground");
            if (base2 == null)
            {
                return false;
            }
            Vector3D to = initPosition + (direction * amount);
            LineD line = new LineD(initPosition, to);
            Vector3D? v = null;
            base2.GetIntersectionWithLine(ref line, out v, true, IntersectionFlags.ALL_TRIANGLES);
            if (v == null)
            {
                return false;
            }
            outPosition = v.Value;
            return true;
        }

        public override string GetName() => 
            "Martin";

        public override bool HandleInput()
        {
            if (MySession.Static == null)
            {
                return false;
            }
            this.CheckAutoHeal();
            return base.HandleInput();
        }

        public bool KillNotSelectedBots()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                MyBotCollection bots = MyAIComponent.Static.Bots;
                foreach (KeyValuePair<int, IMyBot> pair in MyAIComponent.Static.Bots.GetAllBots())
                {
                    MyAgentBot bot = pair.Value as MyAgentBot;
                    if ((bot != null) && (!bots.IsBotSelectedForDegugging(bot) && (bot.Player.Controller.ControlledEntity is MyCharacter)))
                    {
                        MyDamageInformation damageInfo = new MyDamageInformation(false, 1000f, MyDamageType.Weapon, MySession.Static.LocalPlayerId);
                        (bot.Player.Controller.ControlledEntity as MyCharacter).Kill(true, damageInfo);
                    }
                }
            }
            return true;
        }

        private static void MakeCharacterFakeTarget()
        {
            if (MyFakes.FakeTarget != null)
            {
                MyFakes.FakeTarget = null;
            }
            else
            {
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if (localCharacter != null)
                {
                    MyFakes.FakeTarget = localCharacter;
                }
            }
        }

        private static void MakeScreenWithIconGrid()
        {
            TmpScreen screen = new TmpScreen();
            MyGuiControlGrid control = new MyGuiControlGrid();
            screen.Controls.Add(control);
            control.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            control.VisualStyle = MyGuiControlGridStyleEnum.Inventory;
            control.RowsCount = 12;
            control.ColumnsCount = 0x12;
            foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetAllDefinitions())
            {
                control.Add(new MyGuiGridItem(base2.Icons, null, base2.DisplayNameText, null, true), 0);
            }
            MyGuiSandbox.AddScreen(screen);
        }

        private bool OneAIStep()
        {
            MyFakes.DEBUG_ONE_AI_STEP = true;
            return true;
        }

        private bool OneVoxelStep()
        {
            MyFakes.DEBUG_ONE_VOXEL_PATHFINDING_STEP = true;
            return true;
        }

        private bool ReturnToLastMemory()
        {
            if (MyDebugDrawSettings.DEBUG_DRAW_BOTS)
            {
                MyBotCollection bots = MyAIComponent.Static.Bots;
                foreach (KeyValuePair<int, IMyBot> pair in MyAIComponent.Static.Bots.GetAllBots())
                {
                    MyAgentBot bot = pair.Value as MyAgentBot;
                    if ((bot != null) && bots.IsBotSelectedForDegugging(bot))
                    {
                        bot.ReturnToLastMemory();
                    }
                }
            }
            return true;
        }

        public bool SelectNextBot()
        {
            MyAIComponent.Static.Bots.DebugSelectNextBot();
            return true;
        }

        public bool SwitchSwitch(MyKeys key)
        {
            bool flag = !base.GetSwitchValue(key);
            base.SetSwitch(key, flag);
            return true;
        }

        public bool SwitchSwitchDebugBeh(MyKeys key)
        {
            MyFakes.DEBUG_BEHAVIOR_TREE = !MyFakes.DEBUG_BEHAVIOR_TREE;
            base.SetSwitch(key, MyFakes.DEBUG_BEHAVIOR_TREE);
            return true;
        }

        public bool SwitchSwitchOneStep(MyKeys key)
        {
            MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP = true;
            base.SetSwitch(key, MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP);
            return true;
        }

        private bool ToggleMarker()
        {
            Vector3D outPosition = new Vector3D();
            if (!this.GetDirectedPositionOnGround(MySector.MainCamera.Position, MySector.MainCamera.ForwardVector, 1000f, out outPosition, 100f))
            {
                return false;
            }
            MyMarker item = this.FindClosestMarkerInArea(outPosition, 1.0);
            if (item != null)
            {
                this.m_markers.Remove(item);
            }
            else
            {
                this.m_markers.Add(new MyMarker(outPosition, Color.Blue));
            }
            return true;
        }

        private static void VoxelCellDrawing()
        {
            IMyControllableEntity controlledEntity = MySession.Static.ControlledEntity;
            if ((controlledEntity != null) && (MySector.MainCamera != null))
            {
                Vector3D translation = controlledEntity.Entity.WorldMatrix.Translation;
                MyVoxelBase base2 = null;
                foreach (MyVoxelBase base3 in MySession.Static.VoxelMaps.Instances)
                {
                    BoundingBoxD worldAABB = base3.PositionComp.WorldAABB;
                    if (worldAABB.Contains(translation) == ContainmentType.Contains)
                    {
                        base2 = base3;
                        break;
                    }
                }
                if (base2 != null)
                {
                    BoundingBoxD xd;
                    BoundingBoxD xd2;
                    MyCellCoord coord = new MyCellCoord();
                    MyVoxelCoordSystems.WorldPositionToGeometryCellCoord(base2.PositionLeftBottomCorner, ref translation, out coord.CoordInLod);
                    MyVoxelCoordSystems.GeometryCellCoordToWorldAABB(base2.PositionLeftBottomCorner, ref coord.CoordInLod, out xd);
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(base2.PositionLeftBottomCorner, ref translation, out coord.CoordInLod);
                    MyVoxelCoordSystems.VoxelCoordToWorldAABB(base2.PositionLeftBottomCorner, ref coord.CoordInLod, out xd2);
                    MyRenderProxy.DebugDrawAABB(xd2, Vector3.UnitX, 1f, 1f, true, false, false);
                    MyRenderProxy.DebugDrawAABB(xd, Vector3.UnitY, 1f, 1f, true, false, false);
                }
            }
        }

        private static void VoxelPlacement()
        {
            MyCamera mainCamera = MySector.MainCamera;
            if (mainCamera != null)
            {
                int num = 0;
                Vector3D point = (mainCamera.Position + (mainCamera.ForwardVector * 4.5)) - num;
                MyVoxelBase voxelMap = null;
                foreach (MyVoxelBase base3 in MySession.Static.VoxelMaps.Instances)
                {
                    BoundingBoxD worldAABB = base3.PositionComp.WorldAABB;
                    if (worldAABB.Contains(point) == ContainmentType.Contains)
                    {
                        voxelMap = base3;
                        break;
                    }
                }
                if (voxelMap != null)
                {
                    Vector3I vectori;
                    BoundingBoxD xd3;
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(voxelMap.PositionLeftBottomCorner, ref point, out vectori);
                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(voxelMap.PositionLeftBottomCorner, ref vectori, out point);
                    point += num;
                    float num2 = 3f;
                    MyVoxelCoordSystems.VoxelCoordToWorldAABB(voxelMap.PositionLeftBottomCorner, ref vectori, out xd3);
                    MyRenderProxy.DebugDrawAABB(xd3, Color.Blue, 1f, 1f, true, false, false);
                    BoundingSphereD ed = new BoundingSphereD(point, (double) (num2 * 0.5f));
                    MyRenderProxy.DebugDrawSphere(point, num2 * 0.5f, Color.White, 1f, true, false, true, false);
                    if (MyInput.Static.IsLeftMousePressed())
                    {
                        MyShapeSphere sphere1 = new MyShapeSphere();
                        sphere1.Center = ed.Center;
                        sphere1.Radius = (float) ed.Radius;
                        MyShape shape = sphere1;
                        if (shape != null)
                        {
                            float num3;
                            MyVoxelMaterialDefinition definition;
                            MyVoxelGenerator.CutOutShapeWithProperties(voxelMap, shape, out num3, out definition, null, false, false, false, false, false);
                        }
                    }
                }
            }
        }

        private static void VoxelReading()
        {
            MyCamera mainCamera = MySector.MainCamera;
            if (mainCamera != null)
            {
                int num = 0;
                Vector3D point = (mainCamera.Position + (mainCamera.ForwardVector * 4.5)) - num;
                MyVoxelBase base2 = null;
                foreach (MyVoxelBase base3 in MySession.Static.VoxelMaps.Instances)
                {
                    BoundingBoxD worldAABB = base3.PositionComp.WorldAABB;
                    if (worldAABB.Contains(point) == ContainmentType.Contains)
                    {
                        base2 = base3;
                        break;
                    }
                }
                if (base2 != null)
                {
                    Vector3I vectori;
                    Vector3I vectori2;
                    Vector3D worldPosition = point - (Vector3.One * 1f);
                    Vector3D vectord3 = point + (Vector3.One * 1f);
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(base2.PositionLeftBottomCorner, ref worldPosition, out vectori);
                    MyVoxelCoordSystems.WorldPositionToVoxelCoord(base2.PositionLeftBottomCorner, ref vectord3, out vectori2);
                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(base2.PositionLeftBottomCorner, ref vectori, out worldPosition);
                    MyVoxelCoordSystems.VoxelCoordToWorldPosition(base2.PositionLeftBottomCorner, ref vectori2, out vectord3);
                    BoundingBoxD aabb = BoundingBoxD.CreateInvalid();
                    aabb.Include(worldPosition);
                    aabb.Include(vectord3);
                    MyRenderProxy.DebugDrawAABB(aabb, Vector3.One, 1f, 1f, true, false, false);
                    if (MyInput.Static.IsNewLeftMousePressed())
                    {
                        MyStorageData target = new MyStorageData(MyStorageDataTypeFlags.All);
                        target.Resize(vectori, vectori2);
                        base2.Storage.ReadRange(target, MyStorageDataTypeFlags.Content, 0, vectori, vectori2);
                        base2.Storage.WriteRange(target, MyStorageDataTypeFlags.Content, vectori, vectori2, true, false);
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyMartinInputComponent.<>c <>9 = new MyMartinInputComponent.<>c();
            public static Func<string> <>9__2_0;
            public static Func<string> <>9__2_1;
            public static Func<string> <>9__2_2;
            public static Func<string> <>9__2_3;
            public static Func<string> <>9__2_4;
            public static Func<string> <>9__2_5;
            public static Func<string> <>9__2_6;
            public static Func<string> <>9__2_7;
            public static Func<string> <>9__2_8;
            public static Func<string> <>9__2_9;
            public static Func<string> <>9__2_10;
            public static Func<string> <>9__2_11;
            public static Func<bool> <>9__2_12;
            public static Action<bool> <>9__2_13;
            public static Func<bool> <>9__2_14;
            public static Action<bool> <>9__2_15;
            public static Func<bool> <>9__2_16;
            public static Action<bool> <>9__2_17;

            internal string <.ctor>b__2_0() => 
                "Add bots";

            internal string <.ctor>b__2_1() => 
                "One AI step";

            internal string <.ctor>b__2_10() => 
                "Kill not selected bots";

            internal string <.ctor>b__2_11() => 
                "Toggle marker";

            internal bool <.ctor>b__2_12() => 
                MyFakes.DEBUG_BEHAVIOR_TREE;

            internal void <.ctor>b__2_13(bool val)
            {
                MyFakes.DEBUG_BEHAVIOR_TREE = val;
            }

            internal bool <.ctor>b__2_14() => 
                MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP;

            internal void <.ctor>b__2_15(bool val)
            {
                MyFakes.DEBUG_BEHAVIOR_TREE_ONE_STEP = val;
            }

            internal bool <.ctor>b__2_16() => 
                MyFakes.ENABLE_AUTO_HEAL;

            internal void <.ctor>b__2_17(bool val)
            {
                MyFakes.ENABLE_AUTO_HEAL = val;
            }

            internal string <.ctor>b__2_2() => 
                "One Voxel step";

            internal string <.ctor>b__2_3() => 
                "Add one bot";

            internal string <.ctor>b__2_4() => 
                "Add one barb";

            internal string <.ctor>b__2_5() => 
                "Do some action";

            internal string <.ctor>b__2_6() => 
                "Clear some action";

            internal string <.ctor>b__2_7() => 
                "Add berries";

            internal string <.ctor>b__2_8() => 
                "return to Last bot memory";

            internal string <.ctor>b__2_9() => 
                "select Next bot";
        }

        public class MyMarker
        {
            public Vector3D position;
            public Color color;

            public MyMarker(Vector3D position, Color color)
            {
                this.position = position;
                this.color = color;
            }
        }

        private class TmpScreen : MyGuiScreenBase
        {
            public TmpScreen() : base(new Vector2(0.5f, 0.5f), new Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, 0f, 0f)
            {
                base.EnabledBackgroundFade = true;
                base.m_size = new Vector2(0.99f, 0.88544f);
                base.AddCaption("<new screen>", new Vector4?(Vector4.One), new Vector2(0f, 0.03f), 0.8f);
                base.CloseButtonEnabled = true;
                this.RecreateControls(true);
            }

            public override string GetFriendlyName() => 
                "TmpScreen";

            public override void RecreateControls(bool contructor)
            {
                base.RecreateControls(contructor);
            }
        }
    }
}

