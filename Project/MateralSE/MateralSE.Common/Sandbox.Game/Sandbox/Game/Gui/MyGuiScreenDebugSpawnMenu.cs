namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders.Definitions;
    using Sandbox.Definitions;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Engine.Voxels;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Localization;
    using Sandbox.Game.SessionComponents.Clipboard;
    using Sandbox.Game.World;
    using Sandbox.Game.World.Generator;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.Voxels;
    using VRage.Input;
    using VRage.Library.Utils;
    using VRage.Network;
    using VRage.ObjectBuilders;
    using VRage.Serialization;
    using VRage.Utils;
    using VRage.Voxels;
    using VRageMath;
    using VRageRender;
    using VRageRender.Messages;

    [StaticEventOwner]
    internal class MyGuiScreenDebugSpawnMenu : MyGuiScreenDebugBase
    {
        private static readonly Vector2 SCREEN_SIZE = new Vector2(0.4f, 1.2f);
        private static readonly float HIDDEN_PART_RIGHT = 0.04f;
        private readonly Vector2 m_controlPadding;
        private MyGuiControlListbox m_asteroidListBox;
        private MyGuiControlListbox m_physicalObjectListbox;
        private static MyPhysicalItemDefinition m_lastSelectedPhysicalItemDefinition;
        private MyGuiControlListbox m_asteroidTypeListbox;
        private MyGuiControlListbox m_planetListbox;
        private string m_selectedCoreVoxelFile;
        private static string m_lastSelectedAsteroidName = null;
        private MyGuiControlTextbox m_amountTextbox;
        private MyGuiControlLabel m_errorLabel;
        private static long m_amount = 1L;
        private static int m_asteroidCounter = 0;
        private static float m_procAsteroidSizeValue = 64f;
        private static string m_procAsteroidSeedValue = "12345";
        private MyGuiControlSlider m_procAsteroidSize;
        private MyGuiControlTextbox m_procAsteroidSeed;
        private static string m_selectedPlanetName;
        private MyVoxelBase m_currentVoxel;
        private static int m_selectedScreen;
        private Screen[] Screens;
        public static SpawnAsteroidInfo m_lastAsteroidInfo;
        private MyGuiControlSlider m_planetSizeSlider;
        private MyGuiControlTextbox m_procPlanetSeedValue;

        public MyGuiScreenDebugSpawnMenu() : base(new Vector2((MyGuiManager.GetMaxMouseCoord().X - (SCREEN_SIZE.X * 0.5f)) + HIDDEN_PART_RIGHT, 0.5f), new Vector2?(SCREEN_SIZE), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), false)
        {
            this.m_controlPadding = new Vector2(0.02f, 0.02f);
            base.m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
            base.m_guiTransition = MySandboxGame.Config.UIOpacity;
            base.CanBeHidden = true;
            base.CanHideOthers = false;
            base.m_canCloseInCloseAllScreenCalls = true;
            base.m_canShareInput = true;
            base.m_isTopScreen = false;
            base.m_isTopMostScreen = false;
            Screen screen = new Screen {
                Name = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_Items),
                Creator = new CreateScreen(this.CreateObjectsSpawnMenu)
            };
            Screen[] screenArray1 = new Screen[5];
            screenArray1[0] = screen;
            screen = new Screen {
                Name = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_Asteroids),
                Creator = new CreateScreen(this.CreateAsteroidsSpawnMenu)
            };
            screenArray1[1] = screen;
            screen = new Screen {
                Name = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ProceduralAsteroids),
                Creator = new CreateScreen(this.CreateProceduralAsteroidsSpawnMenu)
            };
            screenArray1[2] = screen;
            screen = new Screen {
                Name = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_Planets),
                Creator = new CreateScreen(this.CreatePlanetsSpawnMenu)
            };
            screenArray1[3] = screen;
            screen = new Screen {
                Name = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_EmptyVoxelMap),
                Creator = new CreateScreen(this.CreateEmptyVoxelMapSpawnMenu)
            };
            screenArray1[4] = screen;
            this.Screens = screenArray1;
            this.RecreateControls(true);
        }

        private static void AddSeparator(MyGuiControlList list)
        {
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList {
                Size = new Vector2(1f, 0.01f),
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
            };
            VRageMath.Vector4? color = null;
            control.AddHorizontal(Vector2.Zero, 1f, 0f, color);
            list.Controls.Add(control);
        }

        public static MyObjectBuilder_VoxelMap CreateAsteroidObjectBuilder(string storageName)
        {
            MyObjectBuilder_VoxelMap local1 = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_VoxelMap>();
            local1.StorageName = storageName;
            local1.PersistentFlags = MyPersistentEntityFlags2.InScene | MyPersistentEntityFlags2.Enabled;
            local1.PositionAndOrientation = new MyPositionAndOrientation?(MyPositionAndOrientation.Default);
            local1.MutableStorage = false;
            return local1;
        }

        private unsafe void CreateAsteroidsSpawnMenu(float separatorSize, float usableWidth)
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_Asteroid);
            MyGuiControlLabel control = label1;
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            this.m_asteroidTypeListbox = base.AddListBox(0.2f, null);
            this.m_asteroidTypeListbox.MultiSelect = false;
            this.m_asteroidTypeListbox.VisibleRowsCount = 5;
            using (IEnumerator<MyVoxelMapStorageDefinition> enumerator = (from e in MyDefinitionManager.Static.GetVoxelMapStorageDefinitions()
                orderby e.Id.SubtypeId.ToString()
                select e).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string toolTip = enumerator.Current.Id.SubtypeId.ToString();
                    int? position = null;
                    this.m_asteroidTypeListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(toolTip), toolTip, null, toolTip, null), position);
                }
            }
            this.m_asteroidTypeListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnAsteroidTypeListbox_ItemSelected);
            this.m_asteroidTypeListbox.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnLoadAsteroid);
            if (this.m_asteroidTypeListbox.Items.Count > 0)
            {
                this.m_asteroidTypeListbox.SelectedItems.Add(this.m_asteroidTypeListbox.Items[0]);
            }
            this.OnAsteroidTypeListbox_ItemSelected(this.m_asteroidTypeListbox);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] -= 0.01f;
            MyStringId? tooltip = null;
            MyGuiControlButton button = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnAsteroid, new Action<MyGuiControlButton>(this.OnLoadAsteroid), true, tooltip);
            button.PositionX += 0.002f;
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += 0.01f;
            Vector2? size = null;
            MyGuiControlLabel label2 = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(0.002f, 0f)), size, MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_AsteroidGenerationCanTakeLong), new VRageMath.Vector4?(Color.Red.ToVector4()), 0.8f * base.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(label2);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.03f;
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            bool flag = (localHumanPlayer != null) && MySession.Static.IsUserSpaceMaster(localHumanPlayer.Id.SteamId);
            if (!flag)
            {
                size = null;
                MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(0.002f, 0f)), size, MyTexts.GetString(MyCommonTexts.Warning_SpacemasterOrHigherRequired), new VRageMath.Vector4?(Color.Red.ToVector4()), 0.8f * base.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(label3);
            }
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.03f;
            this.m_asteroidTypeListbox.Enabled = flag;
            button.Enabled = flag;
        }

        public static MyStorageBase CreateAsteroidStorage(string asteroid)
        {
            MyVoxelMapStorageDefinition definition;
            if ((asteroid != null) && MyDefinitionManager.Static.TryGetVoxelMapStorageDefinition(asteroid, out definition))
            {
                return (!definition.Context.IsBaseGame ? MyStorageBase.LoadFromFile(Path.Combine(definition.Context.ModPath, definition.StorageFile), null, true) : MyStorageBase.LoadFromFile(Path.Combine(MyFileSystem.ContentPath, definition.StorageFile), null, true));
            }
            if (string.IsNullOrEmpty(asteroid))
            {
                string msg = "Error: asteroid should not be null!";
                MyLog.Default.WriteLine(msg);
            }
            return null;
        }

        private unsafe MyGuiControlButton CreateDebugButton(float usableWidth, MyStringId text, Action<MyGuiControlButton> onClick, bool enabled = true, MyStringId? tooltip = new MyStringId?())
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.01f;
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlButton button = base.AddButton(MyTexts.Get(text), onClick, null, textColor, size, true, true);
            button.VisualStyle = MyGuiControlButtonStyleEnum.Rectangular;
            button.TextScale = base.m_scale;
            button.Size = new Vector2(usableWidth, button.Size.Y);
            button.Position += new Vector2(-HIDDEN_PART_RIGHT / 2f, 0f);
            button.Enabled = enabled;
            if (tooltip != null)
            {
                button.SetToolTip(tooltip.Value);
            }
            return button;
        }

        private unsafe void CreateEmptyVoxelMapSpawnMenu(float separatorSize, float usableWidth)
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = "Voxel Size: ";
            MyGuiControlLabel label = label1;
            this.Controls.Add(label);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            MyGuiControlSlider control = null;
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            control = new MyGuiControlSlider(new Vector2?(base.m_currentPosition), 2f, 10f, 0.285f, defaultValue, color, string.Empty, 1, 0.8f, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true, false) {
                DebugScale = base.m_sliderDebugScale,
                ColorMask = Color.White.ToVector4()
            };
            this.Controls.Add(control);
            Vector2? size = null;
            label = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(control.Size.X - 0.003f, control.Size.Y - 0.065f)), size, string.Empty, new VRageMath.Vector4?(Color.White.ToVector4()), 0.8f, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            this.Controls.Add(label);
            control.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(control.ValueChanged, delegate (MyGuiControlSlider s) {
                int num = 1 << (((int) s.Value) & 0x1f);
                label.Text = num + "m";
                m_procAsteroidSizeValue = num;
            });
            control.Value = 5f;
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += control.Size.Y;
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += separatorSize;
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] -= 0.01f;
            MyStringId? tooltip = null;
            this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnAsteroid, delegate (MyGuiControlButton x) {
                int procAsteroidSizeValue = (int) m_procAsteroidSizeValue;
                MyStorageBase storage = new MyOctreeStorage(null, new Vector3I(procAsteroidSizeValue));
                MyObjectBuilder_VoxelMap voxelMap = CreateAsteroidObjectBuilder(MakeStorageName("MyEmptyVoxelMap"));
                m_lastAsteroidInfo.Asteroid = null;
                m_lastAsteroidInfo.ProceduralRadius = procAsteroidSizeValue;
                MyClipboardComponent.Static.ActivateVoxelClipboard(voxelMap, storage, MySector.MainCamera.ForwardVector, (storage.Size * 0.5f).Length());
                this.CloseScreenNow();
            }, true, tooltip);
        }

        private void CreateMenu(float separatorSize, float usableWidth)
        {
            this.CreateScreenSelector();
            this.Screens[m_selectedScreen].Creator(separatorSize, usableWidth);
        }

        private unsafe void CreateObjectsSpawnMenu(float separatorSize, float usableWidth)
        {
            MyCharacterDetectorComponent component;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = "Select Item :";
            MyGuiControlLabel control = label1;
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            this.m_physicalObjectListbox = base.AddListBox(0.585f, null);
            this.m_physicalObjectListbox.MultiSelect = false;
            this.m_physicalObjectListbox.VisibleRowsCount = 0x11;
            foreach (MyPhysicalItemDefinition definition in from e in (from e in MyDefinitionManager.Static.GetAllDefinitions()
                where (e is MyPhysicalItemDefinition) && e.Public
                select e).Cast<MyPhysicalItemDefinition>()
                orderby e.DisplayNameText
                select e)
            {
                if (definition.CanSpawnFromScreen)
                {
                    string texture;
                    if ((definition.Icons == null) || (definition.Icons.Length == 0))
                    {
                        texture = MyGuiConstants.TEXTURE_ICON_FAKE.Texture;
                    }
                    else
                    {
                        texture = definition.Icons[0];
                    }
                    string icon = texture;
                    MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(definition.DisplayNameText), definition.DisplayNameText, icon, definition, null);
                    this.m_physicalObjectListbox.Items.Add(item);
                    if (ReferenceEquals(definition, m_lastSelectedPhysicalItemDefinition))
                    {
                        this.m_physicalObjectListbox.SelectedItems.Add(item);
                    }
                }
            }
            this.m_physicalObjectListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnPhysicalObjectListbox_ItemSelected);
            this.m_physicalObjectListbox.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnSpawnPhysicalObject);
            this.OnPhysicalObjectListbox_ItemSelected(this.m_physicalObjectListbox);
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ItemAmount);
            MyGuiControlLabel label2 = label3;
            this.Controls.Add(label2);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.03f;
            VRageMath.Vector4? textColor = null;
            this.m_amountTextbox = new MyGuiControlTextbox(new Vector2?(base.m_currentPosition), m_amount.ToString(), 6, textColor, base.m_scale, MyGuiControlTextboxType.DigitsOnly, MyGuiControlTextboxStyleEnum.Default);
            this.m_amountTextbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_amountTextbox.TextChanged += new Action<MyGuiControlTextbox>(this.OnAmountTextChanged);
            this.Controls.Add(this.m_amountTextbox);
            this.m_amountTextbox.Size = new Vector2(0.285f, 1f);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] -= 0.02f;
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += separatorSize + this.m_amountTextbox.Size.Y;
            this.m_errorLabel = base.AddLabel(MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_InvalidAmount), Color.Red.ToVector4(), 0.8f, null, "Debug");
            this.m_errorLabel.PositionX += 0.282f;
            this.m_errorLabel.PositionY -= 0.04f;
            this.m_errorLabel.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_errorLabel.Visible = false;
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.01f;
            MyStringId? tooltip = null;
            MyGuiControlButton button1 = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnObject, new Action<MyGuiControlButton>(this.OnSpawnPhysicalObject), true, tooltip);
            button1.PositionX += 0.002f;
            MyTerminalBlock detectedEntity = null;
            bool enabled = false;
            if (((MySession.Static.LocalCharacter != null) && MySession.Static.LocalCharacter.Components.TryGet<MyCharacterDetectorComponent>(out component)) && (component.UseObject != null))
            {
                detectedEntity = component.DetectedEntity as MyTerminalBlock;
            }
            string str = "-";
            if (((detectedEntity != null) && detectedEntity.HasInventory) && detectedEntity.HasLocalPlayerAccess())
            {
                str = detectedEntity.CustomName.ToString();
                enabled = true;
            }
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] -= 0.015f;
            tooltip = null;
            MyGuiControlButton button2 = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnTargeted, new Action<MyGuiControlButton>(this.OnSpawnIntoContainer), enabled, tooltip);
            button2.PositionX += 0.002f;
            base.AddLabel(MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_CurrentTarget) + str, Color.White.ToVector4(), base.m_scale, null, "Debug");
        }

        private unsafe void CreatePlanet(int seed, float size)
        {
            MyPlanetInitArguments arguments;
            MyPlanetInitArguments* argumentsPtr1;
            float single2;
            Vector3D vectord = (MySector.MainCamera.Position + ((MySector.MainCamera.ForwardVector * size) * 3f)) - new Vector3D((double) size);
            MyPlanetGeneratorDefinition generator = MyDefinitionManager.Static.GetDefinition<MyPlanetGeneratorDefinition>(MyStringHash.GetOrCompute(m_selectedPlanetName));
            MyPlanetStorageProvider dataProvider = new MyPlanetStorageProvider();
            dataProvider.Init((long) seed, generator, (double) (size / 2f));
            IMyStorage storage = new MyOctreeStorage(dataProvider, dataProvider.StorageSize);
            float num2 = dataProvider.Radius * generator.HillParams.Max;
            float radius = dataProvider.Radius;
            float num3 = radius + num2;
            float num4 = radius + (dataProvider.Radius * generator.HillParams.Min);
            if ((generator.AtmosphereSettings == null) || (generator.AtmosphereSettings.Value.Scale <= 1f))
            {
                single2 = 1.75f;
            }
            else
            {
                single2 = 1f + generator.AtmosphereSettings.Value.Scale;
            }
            MyPlanet planet = new MyPlanet {
                EntityId = MyRandom.Instance.NextLong()
            };
            arguments.StorageName = "test";
            arguments.Seed = seed;
            arguments.Storage = storage;
            arguments.PositionMinCorner = vectord;
            arguments.Radius = dataProvider.Radius;
            arguments.AtmosphereRadius = single2 * dataProvider.Radius;
            arguments.MaxRadius = num3;
            arguments.MinRadius = num4;
            arguments.HasAtmosphere = generator.HasAtmosphere;
            arguments.AtmosphereWavelengths = Vector3.Zero;
            arguments.GravityFalloff = generator.GravityFalloffPower;
            arguments.MarkAreaEmpty = true;
            MyAtmosphereSettings? atmosphereSettings = generator.AtmosphereSettings;
            argumentsPtr1->AtmosphereSettings = (atmosphereSettings != null) ? atmosphereSettings.GetValueOrDefault() : MyAtmosphereSettings.Defaults();
            argumentsPtr1 = (MyPlanetInitArguments*) ref arguments;
            arguments.SurfaceGravity = generator.SurfaceGravity;
            arguments.AddGps = false;
            arguments.SpherizeWithDistance = true;
            arguments.Generator = generator;
            arguments.UserCreated = true;
            arguments.InitializeComponents = true;
            arguments.FadeIn = false;
            planet.Init(arguments);
            SpawnAsteroidInfo info = new SpawnAsteroidInfo {
                Asteroid = null,
                RandomSeed = seed,
                Position = Vector3D.Zero,
                IsProcedural = true,
                ProceduralRadius = size
            };
            m_lastAsteroidInfo = info;
            MyClipboardComponent.Static.ActivateVoxelClipboard(planet.GetObjectBuilder(false), storage, MySector.MainCamera.ForwardVector, (storage.Size * 0.5f).Length());
        }

        private unsafe void CreatePlanetsSpawnMenu(float separatorSize, float usableWidth)
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_Asteroid);
            MyGuiControlLabel control = label1;
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            this.m_planetListbox = base.AddListBox(0.5f, null);
            this.m_planetListbox.MultiSelect = false;
            this.m_planetListbox.VisibleRowsCount = 14;
            using (IEnumerator<MyPlanetGeneratorDefinition> enumerator = (from e in MyDefinitionManager.Static.GetPlanetsGeneratorsDefinitions()
                orderby e.Id.SubtypeId.ToString()
                select e).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string toolTip = enumerator.Current.Id.SubtypeId.ToString();
                    int? position = null;
                    this.m_planetListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(toolTip), toolTip, null, toolTip, null), position);
                }
            }
            this.m_planetListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnPlanetListbox_ItemSelected);
            this.m_planetListbox.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnCreatePlanetClicked);
            if (this.m_planetListbox.Items.Count > 0)
            {
                this.m_planetListbox.SelectedItems.Add(this.m_planetListbox.Items[0]);
            }
            this.OnPlanetListbox_ItemSelected(this.m_planetListbox);
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ProceduralSize);
            MyGuiControlLabel label2 = label5;
            this.Controls.Add(label2);
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += 0.03f;
            float minValue = MyFakes.ENABLE_EXTENDED_PLANET_OPTIONS ? ((float) 100) : ((float) 0x4a38);
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            this.m_planetSizeSlider = new MyGuiControlSlider(new Vector2?(base.m_currentPosition), minValue, 120000f, 0.285f, defaultValue, color, string.Empty, 1, 0.8f, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, true, false);
            this.m_planetSizeSlider.DebugScale = base.m_sliderDebugScale;
            this.m_planetSizeSlider.ColorMask = Color.White.ToVector4();
            this.Controls.Add(this.m_planetSizeSlider);
            Vector2? size = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(this.m_planetSizeSlider.Size.X - 0.003f, this.m_planetSizeSlider.Size.Y - 0.065f)), size, string.Empty, new VRageMath.Vector4?(Color.White.ToVector4()), 0.8f, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            this.Controls.Add(label);
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += this.m_planetSizeSlider.Size.Y;
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += separatorSize;
            this.m_planetSizeSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_planetSizeSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                StringBuilder output = new StringBuilder();
                MyValueFormatter.AppendDistanceInBestUnit(s.Value, output);
                label.Text = output.ToString();
                m_procAsteroidSizeValue = s.Value;
            });
            this.m_planetSizeSlider.Value = 8000f;
            MyGuiControlLabel label6 = new MyGuiControlLabel();
            label6.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label6.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label6.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ProceduralSeed);
            MyGuiControlLabel label3 = label6;
            this.Controls.Add(label3);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += 0.03f;
            this.m_procPlanetSeedValue = new MyGuiControlTextbox(new Vector2?(base.m_currentPosition), m_procAsteroidSeedValue, 20, new VRageMath.Vector4?(Color.White.ToVector4()), base.m_scale, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_procPlanetSeedValue.TextChanged += t => (m_procAsteroidSeedValue = t.Text);
            this.m_procPlanetSeedValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_procPlanetSeedValue.Size = new Vector2(0.285f, 1f);
            this.Controls.Add(this.m_procPlanetSeedValue);
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] += 0.043f;
            MyStringId? tooltip = null;
            MyGuiControlButton button = this.CreateDebugButton(0.285f, MySpaceTexts.ScreenDebugSpawnMenu_GenerateSeed, buttonClicked => this.m_procPlanetSeedValue.Text = MyRandom.Instance.Next().ToString(), true, tooltip);
            button.PositionX += 0.002f;
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            bool flag = (localHumanPlayer != null) && MySession.Static.IsUserSpaceMaster(localHumanPlayer.Id.SteamId);
            if (!flag)
            {
                size = null;
                MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(0.002f, 0.05f)), size, MyTexts.GetString(MyCommonTexts.Warning_SpacemasterOrHigherRequired), new VRageMath.Vector4?(Color.Red.ToVector4()), 0.8f * base.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(label4);
            }
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] -= 0.01f;
            tooltip = null;
            MyGuiControlButton button1 = this.CreateDebugButton(0.285f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnAsteroid, new Action<MyGuiControlButton>(this.OnCreatePlanetClicked), true, tooltip);
            button1.PositionX += 0.002f;
            this.m_planetSizeSlider.Enabled = flag;
            this.m_procPlanetSeedValue.Enabled = flag;
            button.Enabled = flag;
            this.m_planetListbox.Enabled = flag;
            button1.Enabled = flag;
        }

        private unsafe void CreateProceduralAsteroidsSpawnMenu(float separatorSize, float usableWidth)
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += 0.025f;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ProceduralSize);
            MyGuiControlLabel control = label1;
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += 0.03f;
            float? defaultValue = null;
            VRageMath.Vector4? color = null;
            this.m_procAsteroidSize = new MyGuiControlSlider(new Vector2?(base.m_currentPosition), 5f, 2000f, 0.285f, defaultValue, color, string.Empty, 2, 0.75f * base.m_scale, 0f, "Debug", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, false, false);
            this.m_procAsteroidSize.DebugScale = base.m_sliderDebugScale;
            this.m_procAsteroidSize.ColorMask = Color.White.ToVector4();
            this.Controls.Add(this.m_procAsteroidSize);
            Vector2? size = null;
            MyGuiControlLabel label = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(this.m_procAsteroidSize.Size.X - 0.003f, this.m_procAsteroidSize.Size.Y - 0.065f)), size, string.Empty, new VRageMath.Vector4?(Color.White.ToVector4()), 0.8f, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER
            };
            this.Controls.Add(label);
            this.m_procAsteroidSize.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_procAsteroidSize.ValueChanged, delegate (MyGuiControlSlider s) {
                label.Text = MyValueFormatter.GetFormatedFloat(s.Value, 2) + "m";
                m_procAsteroidSizeValue = s.Value;
            });
            this.m_procAsteroidSize.Value = m_procAsteroidSizeValue;
            float* singlePtr3 = (float*) ref base.m_currentPosition.Y;
            singlePtr3[0] += this.m_procAsteroidSize.Size.Y;
            float* singlePtr4 = (float*) ref base.m_currentPosition.Y;
            singlePtr4[0] += separatorSize;
            MyGuiControlLabel label5 = new MyGuiControlLabel();
            label5.Position = new Vector2(base.m_currentPosition.X + 0.001f, base.m_currentPosition.Y);
            label5.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label5.Text = MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_ProceduralSeed);
            MyGuiControlLabel label2 = label5;
            this.Controls.Add(label2);
            float* singlePtr5 = (float*) ref base.m_currentPosition.Y;
            singlePtr5[0] += 0.03f;
            this.m_procAsteroidSeed = new MyGuiControlTextbox(new Vector2?(base.m_currentPosition), m_procAsteroidSeedValue, 20, new VRageMath.Vector4?(Color.White.ToVector4()), base.m_scale, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            this.m_procAsteroidSeed.TextChanged += t => (m_procAsteroidSeedValue = t.Text);
            this.m_procAsteroidSeed.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_procAsteroidSeed.Size = new Vector2(0.285f, 1f);
            this.Controls.Add(this.m_procAsteroidSeed);
            float* singlePtr6 = (float*) ref base.m_currentPosition.Y;
            singlePtr6[0] += this.m_procAsteroidSize.Size.Y + separatorSize;
            float* singlePtr7 = (float*) ref base.m_currentPosition.Y;
            singlePtr7[0] -= 0.015f;
            MyStringId? tooltip = null;
            MyGuiControlButton button = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_GenerateSeed, new Action<MyGuiControlButton>(this.generateSeedButton_OnButtonClick), true, tooltip);
            button.PositionX += 0.002f;
            float* singlePtr8 = (float*) ref base.m_currentPosition.Y;
            singlePtr8[0] -= 0.01f;
            tooltip = null;
            MyGuiControlButton button1 = this.CreateDebugButton(0.284f, MySpaceTexts.ScreenDebugSpawnMenu_SpawnAsteroid, new Action<MyGuiControlButton>(this.OnSpawnProceduralAsteroid), true, tooltip);
            button1.PositionX += 0.002f;
            float* singlePtr9 = (float*) ref base.m_currentPosition.Y;
            singlePtr9[0] += 0.01f;
            size = null;
            MyGuiControlLabel label3 = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(0.002f, 0f)), size, MyTexts.GetString(MySpaceTexts.ScreenDebugSpawnMenu_AsteroidGenerationCanTakeLong), new VRageMath.Vector4?(Color.Red.ToVector4()), 0.8f * base.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(label3);
            float* singlePtr10 = (float*) ref base.m_currentPosition.Y;
            singlePtr10[0] += 0.03f;
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            bool flag = (localHumanPlayer != null) && MySession.Static.IsUserSpaceMaster(localHumanPlayer.Id.SteamId);
            if (!flag)
            {
                size = null;
                MyGuiControlLabel label4 = new MyGuiControlLabel(new Vector2?(base.m_currentPosition + new Vector2(0.002f, 0f)), size, MyTexts.GetString(MyCommonTexts.Warning_SpacemasterOrHigherRequired), new VRageMath.Vector4?(Color.Red.ToVector4()), 0.8f * base.m_scale, "Debug", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
                this.Controls.Add(label4);
            }
            this.m_procAsteroidSize.Enabled = flag;
            this.m_procAsteroidSeed.Enabled = flag;
            button.Enabled = flag;
            button1.Enabled = flag;
        }

        public static MyStorageBase CreateProceduralAsteroidStorage(int seed, float radius)
        {
            int? generator = null;
            return new MyOctreeStorage(MyCompositeShapeProvider.CreateAsteroidShape(seed, radius, 0, generator), MyVoxelCoordSystems.FindBestOctreeSize(radius));
        }

        private unsafe void CreateScreenSelector()
        {
            float* singlePtr1 = (float*) ref base.m_currentPosition.X;
            singlePtr1[0] += 0.018f;
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += (MyGuiConstants.SCREEN_CAPTION_DELTA_Y + this.m_controlPadding.Y) - 0.071f;
            MyPlayer localHumanPlayer = MySession.Static.LocalHumanPlayer;
            VRageMath.Vector4? textColor = null;
            Vector2? size = null;
            MyGuiControlCombobox combo = base.AddCombo(null, textColor, size, 10);
            int? sortOrder = null;
            MyStringId? toolTip = null;
            combo.AddItem(0L, MySpaceTexts.ScreenDebugSpawnMenu_Items, sortOrder, toolTip);
            if ((localHumanPlayer != null) && MySession.Static.IsUserSpaceMaster(localHumanPlayer.Id.SteamId))
            {
                sortOrder = null;
                toolTip = null;
                combo.AddItem(1L, MySpaceTexts.ScreenDebugSpawnMenu_PredefinedAsteroids, sortOrder, toolTip);
                sortOrder = null;
                toolTip = null;
                combo.AddItem(2L, MySpaceTexts.ScreenDebugSpawnMenu_ProceduralAsteroids, sortOrder, toolTip);
                if (MyFakes.ENABLE_PLANETS)
                {
                    sortOrder = null;
                    toolTip = null;
                    combo.AddItem(3L, MySpaceTexts.ScreenDebugSpawnMenu_Planets, sortOrder, toolTip);
                }
                sortOrder = null;
                toolTip = null;
                combo.AddItem(4L, MySpaceTexts.ScreenDebugSpawnMenu_EmptyVoxelMap, sortOrder, toolTip);
            }
            combo.SelectItemByKey((long) m_selectedScreen, true);
            combo.ItemSelected += delegate {
                m_selectedScreen = (int) combo.GetSelectedKey();
                this.RecreateControls(false);
            };
        }

        private void CreateSlider(float usableWidth, float min, float max, ref MyGuiControlSlider slider)
        {
            VRageMath.Vector4? color = null;
            slider = base.AddSlider(string.Empty, 5f, min, max, color);
            slider.Size = new Vector2(400f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, slider.Size.Y);
            slider.LabelDecimalPlaces = 4;
            slider.DebugScale = base.m_sliderDebugScale;
            slider.ColorMask = Color.White.ToVector4();
        }

        private MyGuiControlLabel CreateSliderWithDescription(float usableWidth, float min, float max, string description, ref MyGuiControlSlider slider)
        {
            base.AddLabel(description, VRageMath.Vector4.One, base.m_scale, null, "Debug");
            this.CreateSlider(usableWidth, min, max, ref slider);
            return base.AddLabel("", VRageMath.Vector4.One, base.m_scale, null, "Debug");
        }

        private static float DenormalizeLog(float f, float min, float max) => 
            MathHelper.Clamp(MathHelper.InterpLog(f, min, max), min, max);

        private void generateSeedButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.m_procAsteroidSeed.Text = MyRandom.Instance.Next().ToString();
        }

        public static string GetAsteroidName() => 
            m_lastAsteroidInfo.Asteroid;

        public override string GetFriendlyName() => 
            "MyGuiScreenDebugSpawnMenu";

        private static Matrix GetPasteMatrix()
        {
            if ((MySession.Static.ControlledEntity == null) || ((MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.Entity) && (MySession.Static.GetCameraControllerEnum() != MyCameraControllerEnum.ThirdPersonSpectator)))
            {
                return (Matrix) MySector.MainCamera.WorldMatrix;
            }
            return (Matrix) MySession.Static.ControlledEntity.GetHeadMatrix(true, true, false, false);
        }

        private int GetProceduralAsteroidSeed(MyGuiControlTextbox textbox)
        {
            int result = 0x3039;
            if (!int.TryParse(textbox.Text, out result))
            {
                string text = textbox.Text;
                byte[] buffer = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(text));
                int num2 = 0;
                for (int i = 0; (i < 4) && (i < buffer.Length); i++)
                {
                    result |= buffer[i] << (num2 & 0x1f);
                    num2 += 8;
                }
            }
            return result;
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((MyInput.Static.IsNewKeyPressed(MyKeys.F12) || MyInput.Static.IsNewKeyPressed(MyKeys.F11)) || MyInput.Static.IsNewKeyPressed(MyKeys.F10))
            {
                this.CloseScreen();
            }
        }

        private bool IsValidAmount() => 
            (long.TryParse(this.m_amountTextbox.Text, out m_amount) && (m_amount >= 1L));

        public static string MakeStorageName(string storageNameBase)
        {
            string str = storageNameBase;
            int num = 0;
            while (true)
            {
                bool flag = false;
                DictionaryValuesReader<long, MyVoxelBase> instances = MySession.Static.VoxelMaps.Instances;
                using (Dictionary<long, MyVoxelBase>.ValueCollection.Enumerator enumerator = instances.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.StorageName == str)
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    num++;
                    str = storageNameBase + "-" + num;
                }
                if (!flag)
                {
                    return str;
                }
            }
        }

        private static float NormalizeLog(float f, float min, float max) => 
            MathHelper.Clamp(MathHelper.InterpLogInv(f, min, max), 0f, 1f);

        private void OnAmountTextChanged(MyGuiControlTextbox textbox)
        {
            this.m_errorLabel.Visible = false;
        }

        private void OnAsteroidTypeListbox_ItemSelected(MyGuiControlListbox listbox)
        {
            if (listbox.SelectedItems.Count != 0)
            {
                m_lastSelectedAsteroidName = listbox.SelectedItems[0].UserData as string;
                this.m_selectedCoreVoxelFile = m_lastSelectedAsteroidName;
            }
        }

        private void OnCreatePlanetClicked(object _)
        {
            int proceduralAsteroidSeed = this.GetProceduralAsteroidSeed(this.m_procPlanetSeedValue);
            this.CreatePlanet(proceduralAsteroidSeed, this.m_planetSizeSlider.Value);
            this.CloseScreenNow();
        }

        private void OnLoadAsteroid(object obj)
        {
            this.SpawnVoxelPreview();
            this.CloseScreenNow();
        }

        private void OnPhysicalObjectListbox_ItemSelected(MyGuiControlListbox listbox)
        {
            if (listbox.SelectedItems.Count != 0)
            {
                m_lastSelectedPhysicalItemDefinition = listbox.SelectedItems[0].UserData as MyPhysicalItemDefinition;
            }
        }

        private void OnPlanetListbox_ItemSelected(MyGuiControlListbox listbox)
        {
            if (listbox.SelectedItems.Count != 0)
            {
                m_selectedPlanetName = listbox.SelectedItems[0].UserData as string;
            }
        }

        private void OnSpawnIntoContainer(MyGuiControlButton myGuiControlButton)
        {
            if (!this.IsValidAmount() || (m_lastSelectedPhysicalItemDefinition == null))
            {
                this.m_errorLabel.Visible = true;
            }
            else
            {
                MyCharacterDetectorComponent component;
                if ((MySession.Static.LocalCharacter != null) && MySession.Static.LocalCharacter.Components.TryGet<MyCharacterDetectorComponent>(out component))
                {
                    MyTerminalBlock detectedEntity = component.DetectedEntity as MyTerminalBlock;
                    if ((detectedEntity != null) && detectedEntity.HasInventory)
                    {
                        SerializableDefinitionId id = (SerializableDefinitionId) m_lastSelectedPhysicalItemDefinition.Id;
                        EndpointId targetEndpoint = new EndpointId();
                        Vector3D? position = null;
                        MyMultiplayer.RaiseStaticEvent<long, SerializableDefinitionId, long, long>(x => new Action<long, SerializableDefinitionId, long, long>(MyGuiScreenDebugSpawnMenu.SpawnIntoContainer_Implementation), m_amount, id, detectedEntity.EntityId, MySession.Static.LocalPlayerId, targetEndpoint, position);
                    }
                }
            }
        }

        private void OnSpawnPhysicalObject(object obj)
        {
            if (!this.IsValidAmount())
            {
                this.m_errorLabel.Visible = true;
            }
            else
            {
                this.SpawnFloatingObjectPreview();
                this.CloseScreenNow();
            }
        }

        private void OnSpawnProceduralAsteroid(MyGuiControlButton obj)
        {
            int proceduralAsteroidSeed = this.GetProceduralAsteroidSeed(this.m_procAsteroidSeed);
            this.SpawnProceduralAsteroid(proceduralAsteroidSeed, this.m_procAsteroidSize.Value);
            this.CloseScreenNow();
        }

        public static void RecreateAsteroidBeforePaste(float dragVectorLength)
        {
            int randomSeed = m_lastAsteroidInfo.RandomSeed;
            float proceduralRadius = m_lastAsteroidInfo.ProceduralRadius;
            object[] objArray1 = new object[] { "ProcAsteroid-", randomSeed, "r", proceduralRadius };
            MyStorageBase storage = null;
            if (m_lastAsteroidInfo.IsProcedural)
            {
                storage = CreateProceduralAsteroidStorage(randomSeed, proceduralRadius);
            }
            else
            {
                MyStorageBase.UseStorageCache = false;
                if (m_lastAsteroidInfo.Asteroid != null)
                {
                    storage = CreateAsteroidStorage(m_lastAsteroidInfo.Asteroid);
                }
                else
                {
                    int procAsteroidSizeValue = (int) m_procAsteroidSizeValue;
                    storage = new MyOctreeStorage(null, new Vector3I(procAsteroidSizeValue));
                }
                MyStorageBase.UseStorageCache = MyStorageBase.UseStorageCache;
            }
            MyObjectBuilder_VoxelMap voxelMap = CreateAsteroidObjectBuilder(MakeStorageName(string.Concat(objArray1)));
            MyClipboardComponent.Static.ActivateVoxelClipboard(voxelMap, storage, MySector.MainCamera.ForwardVector, dragVectorLength);
        }

        public override unsafe void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            Vector2 vector1 = new Vector2(-0.05f, 0f);
            Vector2 vector = new Vector2(0.02f, 0.02f);
            float num = 0.8f;
            float separatorSize = 0.01f;
            float num4 = (SCREEN_SIZE.Y - 1f) / 2f;
            base.m_currentPosition = -base.m_size.Value / 2f;
            base.m_currentPosition += vector;
            float* singlePtr1 = (float*) ref base.m_currentPosition.Y;
            singlePtr1[0] += num4;
            base.m_scale = num;
            base.AddCaption(MyTexts.Get(MySpaceTexts.ScreenDebugSpawnMenu_Caption).ToString(), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2?(vector + new Vector2(-HIDDEN_PART_RIGHT, num4 - 0.03f)), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.44f), base.m_size.Value.X * 0.73f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.83f) / 2f, 0.365f), base.m_size.Value.X * 0.73f, 0f, color);
            this.Controls.Add(control);
            float* singlePtr2 = (float*) ref base.m_currentPosition.Y;
            singlePtr2[0] += MyGuiConstants.SCREEN_CAPTION_DELTA_Y + separatorSize;
            this.CreateMenu(separatorSize, (SCREEN_SIZE.X - HIDDEN_PART_RIGHT) - (vector.X * 2f));
        }

        private void ScreenAsteroids(object _)
        {
            MyGuiControlListbox.Item[] selectedItems = this.m_asteroidListBox.SelectedItems.ToArray();
            if (selectedItems.Length == 0)
            {
                StringBuilder messageCaption = new StringBuilder("Error");
                MyStringId? okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                okButtonText = null;
                Vector2? size = null;
                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("No asteroids selected"), messageCaption, okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, size));
            }
            else
            {
                string folder = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), MyPerGameSettings.GameNameSafe + "_AsteroidScreens");
                int state = 0;
                int pauseTimeout = 0;
                int asteroidIndex = 0;
                Action stateMachine = null;
                stateMachine = delegate {
                    int num;
                    if (pauseTimeout > 0)
                    {
                        num = pauseTimeout;
                        pauseTimeout = num - 1;
                    }
                    else
                    {
                        MyVoxelMapStorageDefinition userData = (MyVoxelMapStorageDefinition) selectedItems[asteroidIndex].UserData;
                        num = state;
                        if (num == 0)
                        {
                            this.SpawnVoxelPreview(userData.Id.SubtypeName);
                            pauseTimeout = 100;
                        }
                        else if (num == 1)
                        {
                            StringBuilder builder;
                            MyStringId? nullable;
                            Vector2? nullable2;
                            if (!MyClipboardComponent.Static.IsActive)
                            {
                                builder = new StringBuilder("Done");
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("Screening interrupted"), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, nullable2));
                                return;
                            }
                            string pathToSave = Path.Combine(folder, userData.Id.SubtypeName + ".png");
                            MyRenderProxy.TakeScreenshot(Vector2.One, pathToSave, false, true, false);
                            pauseTimeout = 10;
                            num = asteroidIndex;
                            asteroidIndex = num + 1;
                            if (asteroidIndex == selectedItems.Length)
                            {
                                builder = new StringBuilder("Done");
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable = null;
                                nullable2 = null;
                                MyScreenManager.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, new StringBuilder("All screens saved to\n" + folder), builder, nullable, nullable, nullable, nullable, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, false, nullable2));
                                return;
                            }
                        }
                        num = state;
                        state = num + 1;
                        if (state > 1)
                        {
                            state = 0;
                        }
                    }
                    MySandboxGame.Static.Invoke(stateMachine, "Asteroid screening");
                };
                MySandboxGame.Static.Invoke(stateMachine, "Asteroid screening");
                this.CloseScreenNow();
            }
        }

        [Event(null, 0x410), Reliable, Server]
        private static void SpawnAsteroid(SpawnAsteroidInfo asteroidInfo)
        {
            if (!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                MyStorageBase base2;
                string str;
                if (!asteroidInfo.IsProcedural)
                {
                    if (asteroidInfo.Asteroid != null)
                    {
                        base2 = CreateAsteroidStorage(asteroidInfo.Asteroid);
                    }
                    else
                    {
                        int proceduralRadius = (int) asteroidInfo.ProceduralRadius;
                        base2 = new MyOctreeStorage(null, new Vector3I(proceduralRadius));
                    }
                    str = MakeStorageName(asteroidInfo.Asteroid + "-" + asteroidInfo.RandomSeed);
                }
                else
                {
                    using (MyRandom.Instance.PushSeed(asteroidInfo.RandomSeed))
                    {
                        object[] objArray1 = new object[] { "ProcAsteroid-", asteroidInfo.RandomSeed, "r", asteroidInfo.ProceduralRadius };
                        str = MakeStorageName(string.Concat(objArray1));
                        base2 = CreateProceduralAsteroidStorage(asteroidInfo.RandomSeed, asteroidInfo.ProceduralRadius);
                    }
                }
                MyVoxelMap entity = new MyVoxelMap();
                entity.CreatedByUser = true;
                entity.Save = true;
                entity.AsteroidName = asteroidInfo.Asteroid;
                entity.Init(str, base2, asteroidInfo.Position - (base2.Size * 0.5f));
                Sandbox.Game.Entities.MyEntities.Add(entity, true);
                Sandbox.Game.Entities.MyEntities.RaiseEntityCreated(entity);
            }
        }

        public static void SpawnAsteroid(Vector3D pos)
        {
            m_lastAsteroidInfo.Position = pos;
            if (MySession.Static.HasCreativeRights || MySession.Static.CreativeMode)
            {
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<SpawnAsteroidInfo>(s => new Action<SpawnAsteroidInfo>(MyGuiScreenDebugSpawnMenu.SpawnAsteroid), m_lastAsteroidInfo, targetEndpoint, position);
            }
        }

        private void SpawnFloatingObjectPreview()
        {
            if (m_lastSelectedPhysicalItemDefinition != null)
            {
                MyFixedPoint amount = (MyFixedPoint) m_amount;
                MyObjectBuilder_PhysicalObject obj2 = (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject((SerializableDefinitionId) m_lastSelectedPhysicalItemDefinition.Id);
                if (((obj2 is MyObjectBuilder_PhysicalGunObject) || (obj2 is MyObjectBuilder_OxygenContainerObject)) || (obj2 is MyObjectBuilder_GasContainerObject))
                {
                    amount = 1;
                }
                MyObjectBuilder_FloatingObject floatingObject = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_FloatingObject>();
                floatingObject.PositionAndOrientation = new MyPositionAndOrientation?(MyPositionAndOrientation.Default);
                floatingObject.Item = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_InventoryItem>();
                floatingObject.Item.Amount = amount;
                floatingObject.Item.PhysicalContent = obj2;
                MyClipboardComponent.Static.ActivateFloatingObjectClipboard(floatingObject, Vector3D.Zero, 1f);
            }
        }

        [Event(null, 0x313), Reliable, Server]
        private static void SpawnIntoContainer_Implementation(long amount, SerializableDefinitionId item, long entityId, long playerId)
        {
            if ((!MyEventContext.Current.IsLocallyInvoked && !MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value)) && !MySession.Static.CreativeToolsEnabled(MyEventContext.Current.Sender.Value))
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
            else
            {
                VRage.Game.Entity.MyEntity entity;
                if (Sandbox.Game.Entities.MyEntities.TryGetEntityById(entityId, out entity, false) && (entity.HasInventory && ((MyTerminalBlock) entity).HasPlayerAccess(playerId)))
                {
                    MyInventory inventory = entity.GetInventory(0);
                    if (inventory.CheckConstraint(item))
                    {
                        inventory.AddItems((MyFixedPoint) Math.Min(amount, (decimal) inventory.ComputeAmountThatFits(item, 0f, 0f)), (MyObjectBuilder_PhysicalObject) MyObjectBuilderSerializer.CreateNewObject(item));
                    }
                }
            }
        }

        public static void SpawnPlanet(Vector3D pos)
        {
            EndpointId targetEndpoint = new EndpointId();
            Vector3D? position = null;
            MyMultiplayer.RaiseStaticEvent<string, float, int, Vector3D>(s => new Action<string, float, int, Vector3D>(MyGuiScreenDebugSpawnMenu.SpawnPlanet_Server), m_selectedPlanetName, m_lastAsteroidInfo.ProceduralRadius, m_lastAsteroidInfo.RandomSeed, pos, targetEndpoint, position);
        }

        [Event(null, 0x52f), Reliable, Broadcast]
        private static void SpawnPlanet_Client(string planetName, string storageNameBase, float size, int seed, Vector3D pos, long entityId)
        {
            MyWorldGenerator.AddPlanet(storageNameBase, planetName, planetName, pos, seed, size, true, entityId, false, true);
            if (MySession.Static.RequiresDX < 11)
            {
                MySession.Static.RequiresDX = 11;
            }
        }

        [Event(null, 0x518), Reliable, Server]
        private static void SpawnPlanet_Server(string planetName, float size, int seed, Vector3D pos)
        {
            if (MyEventContext.Current.IsLocallyInvoked || (MySession.Static.IsUserSpaceMaster(MyEventContext.Current.Sender.Value) && (MySession.Static.HasPlayerCreativeRights(MyEventContext.Current.Sender.Value) || MySession.Static.CreativeMode)))
            {
                object[] objArray1 = new object[] { planetName, "-", seed, "d", size };
                string storageNameBase = string.Concat(objArray1);
                MakeStorageName(storageNameBase);
                long entityId = MyRandom.Instance.NextLong();
                MyWorldGenerator.AddPlanet(storageNameBase, planetName, planetName, pos, seed, size, true, entityId, false, true);
                if (MySession.Static.RequiresDX < 11)
                {
                    MySession.Static.RequiresDX = 11;
                }
                EndpointId targetEndpoint = new EndpointId();
                Vector3D? position = null;
                MyMultiplayer.RaiseStaticEvent<string, string, float, int, Vector3D, long>(s => new Action<string, string, float, int, Vector3D, long>(MyGuiScreenDebugSpawnMenu.SpawnPlanet_Client), planetName, storageNameBase, size, seed, pos, entityId, targetEndpoint, position);
            }
            else
            {
                (MyMultiplayer.Static as MyMultiplayerServerBase).ValidationFailed(MyEventContext.Current.Sender.Value, true, null, true);
            }
        }

        private void SpawnProceduralAsteroid(int seed, float radius)
        {
            object[] objArray1 = new object[] { "ProcAsteroid-", seed, "r", radius };
            MyStorageBase storage = CreateProceduralAsteroidStorage(seed, radius);
            MyObjectBuilder_VoxelMap voxelMap = CreateAsteroidObjectBuilder(MakeStorageName(string.Concat(objArray1)));
            SpawnAsteroidInfo info = new SpawnAsteroidInfo {
                Asteroid = null,
                RandomSeed = seed,
                Position = Vector3D.Zero,
                IsProcedural = true,
                ProceduralRadius = radius
            };
            m_lastAsteroidInfo = info;
            MyClipboardComponent.Static.ActivateVoxelClipboard(voxelMap, storage, MySector.MainCamera.ForwardVector, (storage.Size * 0.5f).Length());
        }

        private void SpawnVoxelPreview()
        {
            this.SpawnVoxelPreview(this.m_selectedCoreVoxelFile);
        }

        private void SpawnVoxelPreview(string storageNameBase)
        {
            MyStorageBase storage = CreateAsteroidStorage(storageNameBase);
            MyObjectBuilder_VoxelMap voxelMap = CreateAsteroidObjectBuilder(MakeStorageName(storageNameBase));
            SpawnAsteroidInfo info = new SpawnAsteroidInfo {
                Asteroid = storageNameBase,
                Position = Vector3D.Zero,
                IsProcedural = false
            };
            m_lastAsteroidInfo = info;
            if (storage != null)
            {
                MyClipboardComponent.Static.ActivateVoxelClipboard(voxelMap, storage, MySector.MainCamera.ForwardVector, (storage.Size * 0.5f).Length());
            }
        }

        private void UpdateLayerSlider(MyGuiControlSlider slider, float minValue, float maxValue)
        {
            slider.Value = MathHelper.Max(minValue, MathHelper.Min(slider.Value, maxValue));
            slider.MaxValue = maxValue;
            slider.MinValue = minValue;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MyGuiScreenDebugSpawnMenu.<>c <>9 = new MyGuiScreenDebugSpawnMenu.<>c();
            public static Func<MyVoxelMapStorageDefinition, string> <>9__34_0;
            public static Action<MyGuiControlTextbox> <>9__35_0;
            public static Func<MyDefinitionBase, bool> <>9__36_0;
            public static Func<MyPhysicalItemDefinition, string> <>9__36_1;
            public static Func<IMyEventOwner, Action<long, SerializableDefinitionId, long, long>> <>9__51_0;
            public static Func<IMyEventOwner, Action<MyGuiScreenDebugSpawnMenu.SpawnAsteroidInfo>> <>9__69_0;
            public static Func<MyPlanetGeneratorDefinition, string> <>9__71_2;
            public static Action<MyGuiControlTextbox> <>9__71_0;
            public static Func<IMyEventOwner, Action<string, float, int, Vector3D>> <>9__74_0;
            public static Func<IMyEventOwner, Action<string, string, float, int, Vector3D, long>> <>9__75_0;

            internal string <CreateAsteroidsSpawnMenu>b__34_0(MyVoxelMapStorageDefinition e) => 
                e.Id.SubtypeId.ToString();

            internal bool <CreateObjectsSpawnMenu>b__36_0(MyDefinitionBase e) => 
                ((e is MyPhysicalItemDefinition) && e.Public);

            internal string <CreateObjectsSpawnMenu>b__36_1(MyPhysicalItemDefinition e) => 
                e.DisplayNameText;

            internal void <CreatePlanetsSpawnMenu>b__71_0(MyGuiControlTextbox t)
            {
                MyGuiScreenDebugSpawnMenu.m_procAsteroidSeedValue = t.Text;
            }

            internal string <CreatePlanetsSpawnMenu>b__71_2(MyPlanetGeneratorDefinition e) => 
                e.Id.SubtypeId.ToString();

            internal void <CreateProceduralAsteroidsSpawnMenu>b__35_0(MyGuiControlTextbox t)
            {
                MyGuiScreenDebugSpawnMenu.m_procAsteroidSeedValue = t.Text;
            }

            internal Action<long, SerializableDefinitionId, long, long> <OnSpawnIntoContainer>b__51_0(IMyEventOwner x) => 
                new Action<long, SerializableDefinitionId, long, long>(MyGuiScreenDebugSpawnMenu.SpawnIntoContainer_Implementation);

            internal Action<MyGuiScreenDebugSpawnMenu.SpawnAsteroidInfo> <SpawnAsteroid>b__69_0(IMyEventOwner s) => 
                new Action<MyGuiScreenDebugSpawnMenu.SpawnAsteroidInfo>(MyGuiScreenDebugSpawnMenu.SpawnAsteroid);

            internal Action<string, string, float, int, Vector3D, long> <SpawnPlanet_Server>b__75_0(IMyEventOwner s) => 
                new Action<string, string, float, int, Vector3D, long>(MyGuiScreenDebugSpawnMenu.SpawnPlanet_Client);

            internal Action<string, float, int, Vector3D> <SpawnPlanet>b__74_0(IMyEventOwner s) => 
                new Action<string, float, int, Vector3D>(MyGuiScreenDebugSpawnMenu.SpawnPlanet_Server);
        }

        private delegate void CreateScreen(float space, float width);

        [StructLayout(LayoutKind.Sequential)]
        private struct Screen
        {
            public string Name;
            public MyGuiScreenDebugSpawnMenu.CreateScreen Creator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpawnAsteroidInfo
        {
            [Serialize(MyObjectFlags.DefaultZero)]
            public string Asteroid;
            public int RandomSeed;
            public Vector3D Position;
            public bool IsProcedural;
            public float ProceduralRadius;
        }
    }
}

