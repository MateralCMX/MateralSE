namespace Sandbox.Game.Gui
{
    using Sandbox.Engine.Utils;
    using Sandbox.Game.Components;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using VRage;
    using VRage.Game;
    using VRage.Input;
    using VRage.Utils;
    using VRageMath;

    public class MyTomasInputComponent : MyDebugComponent
    {
        public static float USE_WHEEL_ANIMATION_SPEED = 1f;
        private long m_previousSpectatorGridId;
        public static string ClipboardText = string.Empty;

        public MyTomasInputComponent()
        {
            this.AddShortcut(MyKeys.Delete, true, true, false, false, () => "Delete all characters", delegate {
                foreach (MyCharacter local1 in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCharacter>())
                {
                    if (local1 == MySession.Static.ControlledEntity)
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
                    }
                    local1.Close();
                }
                using (IEnumerator<MyCubeGrid> enumerator2 = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        foreach (MySlimBlock block in enumerator2.Current.GetBlocks())
                        {
                            if (!(block.FatBlock is MyCockpit))
                            {
                                continue;
                            }
                            MyCockpit fatBlock = block.FatBlock as MyCockpit;
                            if (fatBlock.Pilot != null)
                            {
                                fatBlock.Pilot.Close();
                            }
                        }
                    }
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad4, true, false, false, false, () => "Spawn cargo ship or barbarians", delegate {
                MyGlobalEventBase eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip"));
                if (eventById == null)
                {
                    eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnBarbarians"));
                }
                if (eventById != null)
                {
                    MyGlobalEvents.RemoveGlobalEvent(eventById);
                    eventById.SetActivationTime(TimeSpan.FromSeconds(1.0));
                    MyGlobalEvents.AddGlobalEvent(eventById);
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad5, true, false, false, false, () => "Spawn random meteor", delegate {
                MyMeteor.SpawnRandom((MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 20f)) + (MySector.DirectionToSunNormalized * 1000f), -MySector.DirectionToSunNormalized);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad8, true, false, false, false, () => "Switch control to next entity", delegate {
                if (MySession.Static.ControlledEntity != null)
                {
                    MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                    if ((cameraControllerEnum != MyCameraControllerEnum.Entity) && (cameraControllerEnum != MyCameraControllerEnum.ThirdPersonSpectator))
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.ControlledEntity.Entity, position);
                    }
                    else
                    {
                        List<MyEntity> list = Sandbox.Game.Entities.MyEntities.GetEntities().ToList<MyEntity>();
                        int index = list.IndexOf(MySession.Static.ControlledEntity.Entity);
                        List<MyEntity> list2 = new List<MyEntity>();
                        if ((index + 1) < list.Count)
                        {
                            list2.AddRange(list.GetRange(index + 1, (list.Count - index) - 1));
                        }
                        if (index != -1)
                        {
                            list2.AddRange(list.GetRange(0, index + 1));
                        }
                        MyCharacter entity = null;
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 < list2.Count)
                            {
                                MyCharacter character2 = list2[num2] as MyCharacter;
                                if (character2 == null)
                                {
                                    num2++;
                                    continue;
                                }
                                entity = character2;
                            }
                            if (entity != null)
                            {
                                MySession.Static.LocalHumanPlayer.Controller.TakeControl(entity);
                            }
                            break;
                        }
                    }
                }
                return true;
            });
            this.AddShortcut(MyKeys.NumPad7, true, false, false, false, () => "Use next ship", delegate {
                MyCharacterInputComponent.UseNextShip();
                return true;
            });
            this.AddShortcut(MyKeys.NumPad9, true, false, false, false, () => "Debug new grid screen", delegate {
                MyGuiSandbox.AddScreen(new DebugNewGridScreen());
                return true;
            });
            this.AddShortcut(MyKeys.N, true, false, false, false, () => "Refill all batteries", delegate {
                foreach (MyCubeGrid grid in Sandbox.Game.Entities.MyEntities.GetEntities())
                {
                    if (grid != null)
                    {
                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid.GetBlocks().GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyBatteryBlock fatBlock = enumerator2.Current.FatBlock as MyBatteryBlock;
                                if (fatBlock != null)
                                {
                                    fatBlock.CurrentStoredPower = fatBlock.MaxStoredPower;
                                }
                            }
                        }
                    }
                }
                return true;
            });
            this.AddShortcut(MyKeys.U, true, false, false, false, () => "Spawn new character", delegate {
                MyCharacterInputComponent.SpawnCharacter(null);
                return true;
            });
            this.AddShortcut(MyKeys.NumPad2, true, false, false, false, () => "Merge static grids", delegate {
                HashSet<MyCubeGrid> set = new HashSet<MyCubeGrid>();
                while (true)
                {
                    bool flag = false;
                    foreach (MyCubeGrid grid in Sandbox.Game.Entities.MyEntities.GetEntities())
                    {
                        if (((grid != null) && grid.IsStatic) && (grid.GridSizeEnum == MyCubeSize.Large))
                        {
                            if (set.Contains(grid))
                            {
                                continue;
                            }
                            foreach (MySlimBlock block in grid.GetBlocks().ToList<MySlimBlock>())
                            {
                                MyCubeGrid objA = grid.DetectMerge(block, null, null, false);
                                if (objA != null)
                                {
                                    flag = true;
                                    if (!ReferenceEquals(objA, grid))
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                set.Add(grid);
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return true;
                    }
                }
            });
            this.AddShortcut(MyKeys.Add, true, false, false, false, () => "Increase wheel animation speed", delegate {
                USE_WHEEL_ANIMATION_SPEED += 0.05f;
                return true;
            });
            this.AddShortcut(MyKeys.Subtract, true, false, false, false, () => "Decrease wheel animation speed", delegate {
                USE_WHEEL_ANIMATION_SPEED -= 0.05f;
                return true;
            });
            this.AddShortcut(MyKeys.Divide, true, false, false, false, () => "Show model texture names", delegate {
                MyFakes.ENABLE_DEBUG_DRAW_TEXTURE_NAMES = !MyFakes.ENABLE_DEBUG_DRAW_TEXTURE_NAMES;
                return true;
            });
            this.AddShortcut(MyKeys.NumPad1, true, false, false, false, () => "Throw from spectator: " + MySessionComponentThrower.USE_SPECTATOR_FOR_THROW.ToString(), delegate {
                MySessionComponentThrower.USE_SPECTATOR_FOR_THROW = !MySessionComponentThrower.USE_SPECTATOR_FOR_THROW;
                return true;
            });
            this.AddShortcut(MyKeys.F2, true, false, false, false, () => "Spectator to next small grid", () => this.SpectatorToNextGrid(MyCubeSize.Small));
            this.AddShortcut(MyKeys.F3, true, false, false, false, () => "Spectator to next large grid", () => this.SpectatorToNextGrid(MyCubeSize.Large));
            this.AddShortcut(MyKeys.Multiply, true, false, false, false, () => "Show model texture names", new Func<bool>(this.CopyAssetToClipboard));
        }

        private bool CopyAssetToClipboard()
        {
            if (!string.IsNullOrEmpty(ClipboardText))
            {
                MyClipboardHelper.SetClipboard(ClipboardText);
            }
            return true;
        }

        public override string GetName() => 
            "Tomas";

        public override bool HandleInput() => 
            ((MySession.Static != null) ? base.HandleInput() : false);

        public bool SpectatorToNextGrid(MyCubeSize size)
        {
            MyCubeGrid grid = null;
            MyCubeGrid grid2 = null;
            foreach (MyCubeGrid grid3 in Sandbox.Game.Entities.MyEntities.GetEntities())
            {
                if (grid3 == null)
                {
                    continue;
                }
                if (grid3.GridSizeEnum == size)
                {
                    if (this.m_previousSpectatorGridId == 0)
                    {
                        grid = grid3;
                    }
                    else
                    {
                        if (grid2 == null)
                        {
                            if (grid3.EntityId == this.m_previousSpectatorGridId)
                            {
                                grid2 = grid3;
                            }
                            if (grid == null)
                            {
                                grid = grid3;
                            }
                            continue;
                        }
                        grid = grid3;
                    }
                    break;
                }
            }
            if (grid == null)
            {
                return false;
            }
            Vector3D vectord = Vector3D.Transform(Vector3D.Forward, MySpectator.Static.Orientation);
            MySpectator.Static.Position = grid.PositionComp.GetPosition() - ((vectord * grid.PositionComp.WorldVolume.Radius) * 2.0);
            this.m_previousSpectatorGridId = grid.EntityId;
            return true;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyTomasInputComponent.<>c <>9 = new MyTomasInputComponent.<>c();
            public static Func<string> <>9__4_0;
            public static Func<bool> <>9__4_1;
            public static Func<string> <>9__4_2;
            public static Func<bool> <>9__4_3;
            public static Func<string> <>9__4_4;
            public static Func<bool> <>9__4_5;
            public static Func<string> <>9__4_6;
            public static Func<bool> <>9__4_7;
            public static Func<string> <>9__4_8;
            public static Func<bool> <>9__4_9;
            public static Func<string> <>9__4_10;
            public static Func<bool> <>9__4_11;
            public static Func<string> <>9__4_12;
            public static Func<bool> <>9__4_13;
            public static Func<string> <>9__4_14;
            public static Func<bool> <>9__4_15;
            public static Func<string> <>9__4_16;
            public static Func<bool> <>9__4_17;
            public static Func<string> <>9__4_18;
            public static Func<bool> <>9__4_19;
            public static Func<string> <>9__4_20;
            public static Func<bool> <>9__4_21;
            public static Func<string> <>9__4_22;
            public static Func<bool> <>9__4_23;
            public static Func<string> <>9__4_24;
            public static Func<bool> <>9__4_25;
            public static Func<string> <>9__4_26;
            public static Func<string> <>9__4_28;
            public static Func<string> <>9__4_30;

            internal string <.ctor>b__4_0() => 
                "Delete all characters";

            internal bool <.ctor>b__4_1()
            {
                foreach (MyCharacter local1 in Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCharacter>())
                {
                    if (local1 == MySession.Static.ControlledEntity)
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Spectator, null, position);
                    }
                    local1.Close();
                }
                using (IEnumerator<MyCubeGrid> enumerator2 = Sandbox.Game.Entities.MyEntities.GetEntities().OfType<MyCubeGrid>().GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        foreach (MySlimBlock block in enumerator2.Current.GetBlocks())
                        {
                            if (!(block.FatBlock is MyCockpit))
                            {
                                continue;
                            }
                            MyCockpit fatBlock = block.FatBlock as MyCockpit;
                            if (fatBlock.Pilot != null)
                            {
                                fatBlock.Pilot.Close();
                            }
                        }
                    }
                }
                return true;
            }

            internal string <.ctor>b__4_10() => 
                "Debug new grid screen";

            internal bool <.ctor>b__4_11()
            {
                MyGuiSandbox.AddScreen(new MyTomasInputComponent.DebugNewGridScreen());
                return true;
            }

            internal string <.ctor>b__4_12() => 
                "Refill all batteries";

            internal bool <.ctor>b__4_13()
            {
                foreach (MyCubeGrid grid in Sandbox.Game.Entities.MyEntities.GetEntities())
                {
                    if (grid != null)
                    {
                        using (HashSet<MySlimBlock>.Enumerator enumerator2 = grid.GetBlocks().GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                MyBatteryBlock fatBlock = enumerator2.Current.FatBlock as MyBatteryBlock;
                                if (fatBlock != null)
                                {
                                    fatBlock.CurrentStoredPower = fatBlock.MaxStoredPower;
                                }
                            }
                        }
                    }
                }
                return true;
            }

            internal string <.ctor>b__4_14() => 
                "Spawn new character";

            internal bool <.ctor>b__4_15()
            {
                MyCharacterInputComponent.SpawnCharacter(null);
                return true;
            }

            internal string <.ctor>b__4_16() => 
                "Merge static grids";

            internal bool <.ctor>b__4_17()
            {
                HashSet<MyCubeGrid> set = new HashSet<MyCubeGrid>();
                while (true)
                {
                    bool flag = false;
                    foreach (MyCubeGrid grid in Sandbox.Game.Entities.MyEntities.GetEntities())
                    {
                        if (((grid != null) && grid.IsStatic) && (grid.GridSizeEnum == MyCubeSize.Large))
                        {
                            if (set.Contains(grid))
                            {
                                continue;
                            }
                            foreach (MySlimBlock block in grid.GetBlocks().ToList<MySlimBlock>())
                            {
                                MyCubeGrid objA = grid.DetectMerge(block, null, null, false);
                                if (objA != null)
                                {
                                    flag = true;
                                    if (!ReferenceEquals(objA, grid))
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                set.Add(grid);
                            }
                        }
                        if (flag)
                        {
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return true;
                    }
                }
            }

            internal string <.ctor>b__4_18() => 
                "Increase wheel animation speed";

            internal bool <.ctor>b__4_19()
            {
                MyTomasInputComponent.USE_WHEEL_ANIMATION_SPEED += 0.05f;
                return true;
            }

            internal string <.ctor>b__4_2() => 
                "Spawn cargo ship or barbarians";

            internal string <.ctor>b__4_20() => 
                "Decrease wheel animation speed";

            internal bool <.ctor>b__4_21()
            {
                MyTomasInputComponent.USE_WHEEL_ANIMATION_SPEED -= 0.05f;
                return true;
            }

            internal string <.ctor>b__4_22() => 
                "Show model texture names";

            internal bool <.ctor>b__4_23()
            {
                MyFakes.ENABLE_DEBUG_DRAW_TEXTURE_NAMES = !MyFakes.ENABLE_DEBUG_DRAW_TEXTURE_NAMES;
                return true;
            }

            internal string <.ctor>b__4_24() => 
                ("Throw from spectator: " + MySessionComponentThrower.USE_SPECTATOR_FOR_THROW.ToString());

            internal bool <.ctor>b__4_25()
            {
                MySessionComponentThrower.USE_SPECTATOR_FOR_THROW = !MySessionComponentThrower.USE_SPECTATOR_FOR_THROW;
                return true;
            }

            internal string <.ctor>b__4_26() => 
                "Spectator to next small grid";

            internal string <.ctor>b__4_28() => 
                "Spectator to next large grid";

            internal bool <.ctor>b__4_3()
            {
                MyGlobalEventBase eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnCargoShip"));
                if (eventById == null)
                {
                    eventById = MyGlobalEvents.GetEventById(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "SpawnBarbarians"));
                }
                if (eventById != null)
                {
                    MyGlobalEvents.RemoveGlobalEvent(eventById);
                    eventById.SetActivationTime(TimeSpan.FromSeconds(1.0));
                    MyGlobalEvents.AddGlobalEvent(eventById);
                }
                return true;
            }

            internal string <.ctor>b__4_30() => 
                "Show model texture names";

            internal string <.ctor>b__4_4() => 
                "Spawn random meteor";

            internal bool <.ctor>b__4_5()
            {
                MyMeteor.SpawnRandom((MySector.MainCamera.Position + (MySector.MainCamera.ForwardVector * 20f)) + (MySector.DirectionToSunNormalized * 1000f), -MySector.DirectionToSunNormalized);
                return true;
            }

            internal string <.ctor>b__4_6() => 
                "Switch control to next entity";

            internal bool <.ctor>b__4_7()
            {
                if (MySession.Static.ControlledEntity != null)
                {
                    MyCameraControllerEnum cameraControllerEnum = MySession.Static.GetCameraControllerEnum();
                    if ((cameraControllerEnum != MyCameraControllerEnum.Entity) && (cameraControllerEnum != MyCameraControllerEnum.ThirdPersonSpectator))
                    {
                        Vector3D? position = null;
                        MySession.Static.SetCameraController(MyCameraControllerEnum.Entity, MySession.Static.ControlledEntity.Entity, position);
                    }
                    else
                    {
                        List<MyEntity> list = Sandbox.Game.Entities.MyEntities.GetEntities().ToList<MyEntity>();
                        int index = list.IndexOf(MySession.Static.ControlledEntity.Entity);
                        List<MyEntity> list2 = new List<MyEntity>();
                        if ((index + 1) < list.Count)
                        {
                            list2.AddRange(list.GetRange(index + 1, (list.Count - index) - 1));
                        }
                        if (index != -1)
                        {
                            list2.AddRange(list.GetRange(0, index + 1));
                        }
                        MyCharacter entity = null;
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 < list2.Count)
                            {
                                MyCharacter character2 = list2[num2] as MyCharacter;
                                if (character2 == null)
                                {
                                    num2++;
                                    continue;
                                }
                                entity = character2;
                            }
                            if (entity != null)
                            {
                                MySession.Static.LocalHumanPlayer.Controller.TakeControl(entity);
                            }
                            break;
                        }
                    }
                }
                return true;
            }

            internal string <.ctor>b__4_8() => 
                "Use next ship";

            internal bool <.ctor>b__4_9()
            {
                MyCharacterInputComponent.UseNextShip();
                return true;
            }
        }

        private class DebugNewGridScreen : MyGuiScreenBase
        {
            private MyGuiControlCombobox m_sizeCombobox;
            private MyGuiControlCheckbox m_staticCheckbox;

            public DebugNewGridScreen() : base(nullable, nullable2, nullable, false, null, 0f, 0f)
            {
                Vector2? nullable = null;
                nullable = null;
                base.EnabledBackgroundFade = true;
                this.RecreateControls(true);
            }

            public override string GetFriendlyName() => 
                "DebugNewGridScreen";

            private void okButton_ButtonClicked(MyGuiControlButton obj)
            {
                MyCubeBuilder.Static.StartStaticGridPlacement((MyCubeSize) ((byte) this.m_sizeCombobox.GetSelectedKey()), this.m_staticCheckbox.IsChecked);
                this.CloseScreen();
            }

            public override void RecreateControls(bool constructor)
            {
                base.RecreateControls(constructor);
                MyGuiControlCombobox combobox1 = new MyGuiControlCombobox();
                combobox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
                combobox1.Position = Vector2.Zero;
                this.m_sizeCombobox = combobox1;
                foreach (object obj2 in typeof(MyCubeSize).GetEnumValues())
                {
                    int? sortOrder = null;
                    this.m_sizeCombobox.AddItem((long) ((MyCubeSize) obj2), new StringBuilder(obj2.ToString()), sortOrder, null);
                }
                this.m_sizeCombobox.SelectItemByKey(0L, true);
                Vector2? position = null;
                Vector4? color = null;
                MyGuiControlCheckbox checkbox1 = new MyGuiControlCheckbox(position, color, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
                checkbox1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                checkbox1.IsChecked = true;
                this.m_staticCheckbox = checkbox1;
                MyGuiControlLabel label1 = new MyGuiControlLabel();
                label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                label1.Position = new Vector2(this.m_staticCheckbox.Size.X, 0f);
                label1.Text = "Static grid";
                MyGuiControlLabel control = label1;
                MyGuiControlButton button1 = new MyGuiControlButton();
                button1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP;
                button1.Text = "Ok";
                button1.Position = new Vector2(0f, 0.05f);
                MyGuiControlButton button = button1;
                button.ButtonClicked += new Action<MyGuiControlButton>(this.okButton_ButtonClicked);
                base.Elements.Add(this.m_sizeCombobox);
                base.Elements.Add(this.m_staticCheckbox);
                base.Elements.Add(control);
                base.Elements.Add(button);
            }
        }
    }
}

