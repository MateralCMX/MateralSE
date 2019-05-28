namespace Sandbox.Game.Gui
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Physics;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Input;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    internal class MyTestersInputComponent : MyDebugComponent
    {
        public MyTestersInputComponent()
        {
            this.AddShortcut(MyKeys.Back, true, true, false, false, () => "Freeze cube builder gizmo", delegate {
                MyCubeBuilder.Static.FreezeGizmo = !MyCubeBuilder.Static.FreezeGizmo;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad0, false, false, false, false, () => "Add items to inventory (continuous)", delegate {
                this.AddItemsToInventory(0);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Add items to inventory", delegate {
                this.AddItemsToInventory(1);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Add components to inventory", delegate {
                this.AddItemsToInventory(2);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad3, true, false, false, false, () => "Fill inventory with iron", new Func<bool>(MyTestersInputComponent.FillInventoryWithIron));
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Add to inventory dialog...", delegate {
                MyGuiSandbox.AddScreen(new MyGuiScreenDialogInventoryCheat());
                return true;
            });
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Set container type", new Func<bool>(MyTestersInputComponent.SetContainerType));
            this.AddShortcut(MyKeys.NumPad6, true, false, false, false, () => "Toggle debug draw", new Func<bool>(MyTestersInputComponent.ToggleDebugDraw));
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Save the game", delegate {
                MyAsyncSaving.Start(null, null, false);
                return true;
            });
        }

        public bool AddItems(MyInventory inventory, MyObjectBuilder_PhysicalObject obj, bool overrideCheck) => 
            this.AddItems(inventory, obj, overrideCheck, 1);

        public bool AddItems(MyInventory inventory, MyObjectBuilder_PhysicalObject obj, bool overrideCheck, MyFixedPoint amount)
        {
            if (!overrideCheck && inventory.ContainItems(amount, obj))
            {
                return false;
            }
            if (!inventory.CanItemsBeAdded(amount, obj.GetId()))
            {
                return false;
            }
            inventory.AddItems(amount, obj);
            return true;
        }

        private void AddItemsToInventory(int variant)
        {
            bool overrideCheck = variant != 0;
            bool flag2 = variant != 0;
            bool flag3 = variant == 2;
            MyEntity controlledEntity = MySession.Static.ControlledEntity as MyEntity;
            if ((controlledEntity != null) && controlledEntity.HasInventory)
            {
                MyInventory inventory = controlledEntity.GetInventory(0);
                if (!flag3)
                {
                    MyObjectBuilder_AmmoMagazine magazine = new MyObjectBuilder_AmmoMagazine {
                        SubtypeName = "NATO_5p56x45mm",
                        ProjectilesCount = 50
                    };
                    this.AddItems(inventory, magazine, false, 5);
                    MyObjectBuilder_AmmoMagazine magazine2 = new MyObjectBuilder_AmmoMagazine {
                        SubtypeName = "NATO_25x184mm",
                        ProjectilesCount = 50
                    };
                    this.AddItems(inventory, magazine2, false);
                    MyObjectBuilder_AmmoMagazine magazine3 = new MyObjectBuilder_AmmoMagazine {
                        SubtypeName = "Missile200mm",
                        ProjectilesCount = 50
                    };
                    this.AddItems(inventory, magazine3, false);
                    this.AddItems(inventory, this.CreateGunContent("AutomaticRifleItem"), false);
                    this.AddItems(inventory, this.CreateGunContent("WelderItem"), false);
                    this.AddItems(inventory, this.CreateGunContent("AngleGrinderItem"), false);
                    this.AddItems(inventory, this.CreateGunContent("HandDrillItem"), false);
                }
                foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetAllDefinitions())
                {
                    if (((base2.Id.TypeId == typeof(MyObjectBuilder_Component)) || (base2.Id.TypeId == typeof(MyObjectBuilder_Ingot))) && ((!flag3 || (base2.Id.TypeId == typeof(MyObjectBuilder_Component))) && (!flag3 || (((MyComponentDefinition) base2).Volume <= 0.05f))))
                    {
                        MyObjectBuilder_PhysicalObject obj2 = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(base2.Id.TypeId);
                        obj2.SubtypeName = base2.Id.SubtypeName;
                        if (!this.AddItems(inventory, obj2, overrideCheck, 1) & flag2)
                        {
                            Matrix matrix = (Matrix) MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
                            MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(1, obj2, 1f), matrix.Translation + (matrix.Forward * 0.2f), matrix.Forward, matrix.Up, MySession.Static.ControlledEntity.Entity.Physics, null);
                        }
                    }
                }
                if (!flag3)
                {
                    string[] strArray;
                    MyDefinitionManager.Static.GetOreTypeNames(out strArray);
                    string[] strArray2 = strArray;
                    for (int i = 0; i < strArray2.Length; i++)
                    {
                        MyObjectBuilder_Ore ore = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(strArray2[i]);
                        if (!this.AddItems(inventory, ore, overrideCheck, 1) & flag2)
                        {
                            Matrix matrix2 = (Matrix) MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
                            MyFloatingObjects.Spawn(new MyPhysicalInventoryItem(1, ore, 1f), matrix2.Translation + (matrix2.Forward * 0.2f), matrix2.Forward, matrix2.Up, MySession.Static.ControlledEntity.Entity.Physics, null);
                        }
                    }
                }
            }
        }

        private MyObjectBuilder_PhysicalGunObject CreateGunContent(string subtypeName) => 
            ((MyObjectBuilder_PhysicalGunObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) new MyDefinitionId(typeof(MyObjectBuilder_PhysicalGunObject), subtypeName)));

        private static bool FillInventoryWithIron()
        {
            MyEntity controlledEntity = MySession.Static.ControlledEntity as MyEntity;
            if ((controlledEntity != null) && controlledEntity.HasInventory)
            {
                MyObjectBuilder_Ore objectBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Iron");
                MyInventory inventory = controlledEntity.GetInventory(0);
                inventory.AddItems(inventory.ComputeAmountThatFits(objectBuilder.GetId(), 0f, 0f), objectBuilder);
            }
            return true;
        }

        public override string GetName() => 
            "Testers";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? (!(MyScreenManager.GetScreenWithFocus() is MyGuiScreenDialogInventoryCheat) ? base.HandleInput() : false) : false);

        private static bool SetContainerType()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            if (localCharacter == null)
            {
                return false;
            }
            Matrix matrix = (Matrix) localCharacter.GetHeadMatrix(true, true, false, false, false);
            List<MyPhysics.HitInfo> toList = new List<MyPhysics.HitInfo>();
            MyPhysics.CastRay(matrix.Translation, matrix.Translation + (matrix.Forward * 100f), toList, 0);
            if (toList.Count == 0)
            {
                return false;
            }
            MyPhysics.HitInfo info = toList.FirstOrDefault<MyPhysics.HitInfo>();
            if (info.HkHitInfo.Body == null)
            {
                return false;
            }
            IMyEntity hitEntity = info.HkHitInfo.GetHitEntity();
            if (!(hitEntity is MyCargoContainer))
            {
                return false;
            }
            MyGuiSandbox.AddScreen(new MyGuiScreenDialogContainerType(hitEntity as MyCargoContainer));
            return true;
        }

        private static bool ToggleDebugDraw()
        {
            if (MyDebugDrawSettings.ENABLE_DEBUG_DRAW)
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = false;
                MyDebugDrawSettings.DEBUG_DRAW_EVENTS = false;
            }
            else
            {
                MyDebugDrawSettings.ENABLE_DEBUG_DRAW = true;
                MyDebugDrawSettings.DEBUG_DRAW_EVENTS = true;
            }
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTestersInputComponent.<>c <>9 = new MyTestersInputComponent.<>c();
            public static Func<string> <>9__0_0;
            public static Func<bool> <>9__0_1;
            public static Func<string> <>9__0_2;
            public static Func<string> <>9__0_4;
            public static Func<string> <>9__0_6;
            public static Func<string> <>9__0_8;
            public static Func<string> <>9__0_9;
            public static Func<bool> <>9__0_10;
            public static Func<string> <>9__0_11;
            public static Func<string> <>9__0_12;
            public static Func<string> <>9__0_13;
            public static Func<bool> <>9__0_14;

            internal string <.ctor>b__0_0() => 
                "Freeze cube builder gizmo";

            internal bool <.ctor>b__0_1()
            {
                MyCubeBuilder.Static.FreezeGizmo = !MyCubeBuilder.Static.FreezeGizmo;
                return true;
            }

            internal bool <.ctor>b__0_10()
            {
                MyGuiSandbox.AddScreen(new MyGuiScreenDialogInventoryCheat());
                return true;
            }

            internal string <.ctor>b__0_11() => 
                "Set container type";

            internal string <.ctor>b__0_12() => 
                "Toggle debug draw";

            internal string <.ctor>b__0_13() => 
                "Save the game";

            internal bool <.ctor>b__0_14()
            {
                MyAsyncSaving.Start(null, null, false);
                return true;
            }

            internal string <.ctor>b__0_2() => 
                "Add items to inventory (continuous)";

            internal string <.ctor>b__0_4() => 
                "Add items to inventory";

            internal string <.ctor>b__0_6() => 
                "Add components to inventory";

            internal string <.ctor>b__0_8() => 
                "Fill inventory with iron";

            internal string <.ctor>b__0_9() => 
                "Add to inventory dialog...";
        }
    }
}

