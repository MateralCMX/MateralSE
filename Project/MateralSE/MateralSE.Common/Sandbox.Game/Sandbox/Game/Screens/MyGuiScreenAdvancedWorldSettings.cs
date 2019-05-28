namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Engine.Multiplayer;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.EntityComponents;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using VRage;
    using VRage.Game;
    using VRage.Library.Utils;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenAdvancedWorldSettings : MyGuiScreenBase
    {
        private const int MIN_DAY_TIME_MINUTES = 1;
        private const int MAX_DAY_TIME_MINUTES = 0x5a0;
        private readonly float MIN_SAFE_TIME_FOR_SUN;
        private MyGuiScreenWorldSettings m_parent;
        private bool m_isNewGame;
        private bool m_isConfirmed;
        private bool m_showWarningForOxygen;
        private bool m_recreating_control;
        private bool m_isHostilityChanged;
        private MyGuiControlTextbox m_passwordTextbox;
        private MyGuiControlCombobox m_onlineMode;
        private MyGuiControlCombobox m_worldSizeCombo;
        private MyGuiControlCombobox m_spawnShipTimeCombo;
        private MyGuiControlCombobox m_viewDistanceCombo;
        private MyGuiControlCombobox m_physicsOptionsCombo;
        private MyGuiControlCombobox m_assembler;
        private MyGuiControlCombobox m_charactersInventory;
        private MyGuiControlCombobox m_refinery;
        private MyGuiControlCombobox m_welder;
        private MyGuiControlCombobox m_grinder;
        private MyGuiControlCombobox m_soundModeCombo;
        private MyGuiControlCombobox m_asteroidAmountCombo;
        private MyGuiControlCombobox m_environment;
        private MyGuiControlCombobox m_blocksInventory;
        private MyGuiControlCheckbox m_autoHealing;
        private MyGuiControlCheckbox m_enableCopyPaste;
        private MyGuiControlCheckbox m_weaponsEnabled;
        private MyGuiControlCheckbox m_showPlayerNamesOnHud;
        private MyGuiControlCheckbox m_thrusterDamage;
        private MyGuiControlCheckbox m_cargoShipsEnabled;
        private MyGuiControlCheckbox m_enableSpectator;
        private MyGuiControlCheckbox m_respawnShipDelete;
        private MyGuiControlCheckbox m_resetOwnership;
        private MyGuiControlCheckbox m_permanentDeath;
        private MyGuiControlCheckbox m_destructibleBlocks;
        private MyGuiControlCheckbox m_enableIngameScripts;
        private MyGuiControlCheckbox m_enableToolShake;
        private MyGuiControlCheckbox m_enableOxygen;
        private MyGuiControlCheckbox m_enableOxygenPressurization;
        private MyGuiControlCheckbox m_enable3rdPersonCamera;
        private MyGuiControlCheckbox m_enableEncounters;
        private MyGuiControlCheckbox m_enableRespawnShips;
        private MyGuiControlCheckbox m_scenarioEditMode;
        private MyGuiControlCheckbox m_enableConvertToStation;
        private MyGuiControlCheckbox m_enableStationVoxelSupport;
        private MyGuiControlCheckbox m_enableSunRotation;
        private MyGuiControlCheckbox m_enableJetpack;
        private MyGuiControlCheckbox m_spawnWithTools;
        private MyGuiControlCheckbox m_enableVoxelDestruction;
        private MyGuiControlCheckbox m_enableDrones;
        private MyGuiControlCheckbox m_enableWolfs;
        private MyGuiControlCheckbox m_enableSpiders;
        private MyGuiControlCheckbox m_enableRemoteBlockRemoval;
        private MyGuiControlCheckbox m_enableContainerDrops;
        private MyGuiControlCheckbox m_blockLimits;
        private MyGuiControlCheckbox m_enableTurretsFriendlyFire;
        private MyGuiControlCheckbox m_enableSubGridDamage;
        private MyGuiControlCheckbox m_enableRealisticDampeners;
        private MyGuiControlCheckbox m_enableAdaptiveSimulationQuality;
        private MyGuiControlCheckbox m_enableVoxelHand;
        private MyGuiControlCheckbox m_enableResearch;
        private MyGuiControlCheckbox m_enableAutoRespawn;
        private MyGuiControlCheckbox m_enableSupergridding;
        private MyGuiControlButton m_okButton;
        private MyGuiControlButton m_cancelButton;
        private MyGuiControlButton m_survivalModeButton;
        private MyGuiControlButton m_creativeModeButton;
        private MyGuiControlSlider m_maxPlayersSlider;
        private MyGuiControlSlider m_sunRotationIntervalSlider;
        private MyGuiControlLabel m_enableCopyPasteLabel;
        private MyGuiControlLabel m_maxPlayersLabel;
        private MyGuiControlLabel m_maxFloatingObjectsLabel;
        private MyGuiControlLabel m_maxBackupSavesLabel;
        private MyGuiControlLabel m_sunRotationPeriod;
        private MyGuiControlLabel m_sunRotationPeriodValue;
        private MyGuiControlLabel m_enableWolfsLabel;
        private MyGuiControlLabel m_enableSpidersLabel;
        private MyGuiControlLabel m_maxGridSizeValue;
        private MyGuiControlLabel m_maxBlocksPerPlayerValue;
        private MyGuiControlLabel m_totalPCUValue;
        private MyGuiControlLabel m_maxBackupSavesValue;
        private MyGuiControlLabel m_maxFloatingObjectsValue;
        private MyGuiControlLabel m_enableContainerDropsLabel;
        private MyGuiControlLabel m_optimalSpawnDistanceValue;
        private MyGuiControlSlider m_maxFloatingObjectsSlider;
        private MyGuiControlSlider m_maxGridSizeSlider;
        private MyGuiControlSlider m_maxBlocksPerPlayerSlider;
        private MyGuiControlSlider m_totalPCUSlider;
        private MyGuiControlSlider m_optimalSpawnDistanceSlider;
        private MyGuiControlSlider m_maxBackupSavesSlider;
        private StringBuilder m_tempBuilder;
        private int m_customWorldSize;
        private int m_customViewDistance;
        private int? m_asteroidAmount;
        [CompilerGenerated]
        private Action OnOkButtonClicked;

        public event Action OnOkButtonClicked
        {
            [CompilerGenerated] add
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action a = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Combine(a, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, a);
                    if (ReferenceEquals(onOkButtonClicked, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                Action onOkButtonClicked = this.OnOkButtonClicked;
                while (true)
                {
                    Action source = onOkButtonClicked;
                    Action action3 = (Action) Delegate.Remove(source, value);
                    onOkButtonClicked = Interlocked.CompareExchange<Action>(ref this.OnOkButtonClicked, action3, source);
                    if (ReferenceEquals(onOkButtonClicked, source))
                    {
                        return;
                    }
                }
            }
        }

        public MyGuiScreenAdvancedWorldSettings(MyGuiScreenWorldSettings parent) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), new Vector2?(CalcSize()), false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.MIN_SAFE_TIME_FOR_SUN = 0.4668752f;
            this.m_tempBuilder = new StringBuilder();
            this.m_customViewDistance = 0x4e20;
            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedWorldSettings.ctor START");
            this.m_parent = parent;
            base.EnabledBackgroundFade = true;
            this.m_isNewGame = ReferenceEquals(parent.Checkpoint, null);
            this.m_isConfirmed = false;
            this.RecreateControls(true);
            this.m_isHostilityChanged = !this.m_isNewGame;
            MySandboxGame.Log.WriteLine("MyGuiScreenAdvancedWorldSettings.ctor END");
        }

        private void blockLimits_CheckedChanged(MyGuiControlCheckbox checkbox)
        {
            if (checkbox.IsChecked)
            {
                this.m_maxBlocksPerPlayerSlider.Value = 100000f;
                this.m_maxBlocksPerPlayerSlider.Enabled = true;
                this.m_maxGridSizeSlider.Value = 50000f;
                this.m_maxGridSizeSlider.Enabled = true;
                this.m_totalPCUSlider.Value = 100000f;
                this.m_totalPCUSlider.Enabled = true;
            }
            else
            {
                if (!this.m_recreating_control)
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? size = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextBlockLimitDisableWarning), MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), okButtonText, okButtonText, okButtonText, okButtonText, null, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, size));
                }
                this.m_maxGridSizeSlider.Value = 0f;
                this.m_maxGridSizeSlider.Enabled = false;
                this.m_maxBlocksPerPlayerSlider.Value = 0f;
                this.m_maxBlocksPerPlayerSlider.Enabled = false;
                this.m_totalPCUSlider.Value = 0f;
                this.m_totalPCUSlider.Enabled = false;
            }
        }

        public unsafe void BuildControls()
        {
            MyStringId? nullable5;
            Vector2 vector = new Vector2(50f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2? position = null;
            VRageMath.Vector4? backgroundColor = null;
            MyGuiControlParent scrolledControl = new MyGuiControlParent(position, new Vector2(base.Size.Value.X - (vector.X * 2f), 2.038f), backgroundColor, null);
            if (!this.m_isNewGame)
            {
                scrolledControl.Size = new Vector2(scrolledControl.Size.X, 1.9855f);
            }
            MyGuiControlScrollablePanel control = new MyGuiControlScrollablePanel(scrolledControl) {
                OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                ScrollbarVEnabled = true
            };
            position = base.Size;
            control.Size = new Vector2((position.Value.X - (vector.X * 2f)) - 0.035f, 0.74f);
            control.Position = new Vector2(-0.27f, -0.394f);
            backgroundColor = null;
            base.AddCaption(MyCommonTexts.ScreenCaptionAdvancedSettings, backgroundColor, new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList list = new MyGuiControlSeparatorList();
            backgroundColor = null;
            list.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.835f, 0f, backgroundColor);
            this.Controls.Add(list);
            MyGuiControlSeparatorList list2 = new MyGuiControlSeparatorList();
            backgroundColor = null;
            list2.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.835f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.835f, 0f, backgroundColor);
            this.Controls.Add(list2);
            int num = 0;
            this.MakeLabel(MySpaceTexts.WorldSettings_Password);
            this.MakeLabel(MyCommonTexts.WorldSettings_OnlineMode);
            this.m_maxPlayersLabel = this.MakeLabel(MyCommonTexts.MaxPlayers);
            this.m_maxFloatingObjectsLabel = this.MakeLabel(MySpaceTexts.MaxFloatingObjects);
            this.m_maxBackupSavesLabel = this.MakeLabel(MySpaceTexts.MaxBackupSaves);
            MyGuiControlLabel label = this.MakeLabel(MySpaceTexts.WorldSettings_EnvironmentHostility);
            MyGuiControlLabel label2 = this.MakeLabel(MySpaceTexts.WorldSettings_MaxGridSize);
            this.m_maxGridSizeValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_maxGridSizeValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlLabel label3 = this.MakeLabel(MySpaceTexts.WorldSettings_MaxBlocksPerPlayer);
            this.m_maxBlocksPerPlayerValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_maxBlocksPerPlayerValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlLabel label4 = this.MakeLabel(MySpaceTexts.WorldSettings_TotalPCU);
            this.m_totalPCUValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_totalPCUValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            MyGuiControlLabel label5 = this.MakeLabel(MySpaceTexts.WorldSettings_OptimalSpawnDistance);
            this.m_optimalSpawnDistanceValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_optimalSpawnDistanceValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_maxFloatingObjectsValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_maxFloatingObjectsValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_maxBackupSavesValue = this.MakeLabel(MyCommonTexts.Disabled);
            this.m_maxBackupSavesValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_sunRotationPeriod = this.MakeLabel(MySpaceTexts.SunRotationPeriod);
            this.m_sunRotationPeriodValue = this.MakeLabel(MySpaceTexts.SunRotationPeriod);
            this.m_sunRotationPeriodValue.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_sunRotationPeriod = this.MakeLabel(MySpaceTexts.SunRotationPeriod);
            this.MakeLabel(MyCommonTexts.WorldSettings_GameMode);
            this.MakeLabel(MySpaceTexts.WorldSettings_GameStyle);
            this.MakeLabel(MySpaceTexts.WorldSettings_Scenario);
            MyGuiControlLabel label6 = this.MakeLabel(MySpaceTexts.WorldSettings_AutoHealing);
            MyGuiControlLabel label7 = this.MakeLabel(MySpaceTexts.WorldSettings_ThrusterDamage);
            MyGuiControlLabel label8 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableSpectator);
            MyGuiControlLabel label9 = this.MakeLabel(MySpaceTexts.WorldSettings_ResetOwnership);
            MyGuiControlLabel label10 = this.MakeLabel(MySpaceTexts.WorldSettings_PermanentDeath);
            MyGuiControlLabel label11 = this.MakeLabel(MySpaceTexts.WorldSettings_DestructibleBlocks);
            MyGuiControlLabel label12 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableIngameScripts);
            MyGuiControlLabel label13 = this.MakeLabel(MySpaceTexts.WorldSettings_Enable3rdPersonCamera);
            MyGuiControlLabel label14 = this.MakeLabel(MySpaceTexts.WorldSettings_Encounters);
            MyGuiControlLabel label15 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableToolShake);
            MyGuiControlLabel label16 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableAdaptiveSimulationQuality);
            MyGuiControlLabel label17 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableVoxelHand);
            MyGuiControlLabel label18 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableCargoShips);
            this.m_enableCopyPasteLabel = this.MakeLabel(MySpaceTexts.WorldSettings_EnableCopyPaste);
            MyGuiControlLabel label19 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableWeapons);
            MyGuiControlLabel label20 = this.MakeLabel(MySpaceTexts.WorldSettings_ShowPlayerNamesOnHud);
            MyGuiControlLabel label21 = this.MakeLabel(MySpaceTexts.WorldSettings_CharactersInventorySize);
            MyGuiControlLabel label22 = this.MakeLabel(MySpaceTexts.WorldSettings_BlocksInventorySize);
            MyGuiControlLabel label23 = this.MakeLabel(MySpaceTexts.WorldSettings_RefinerySpeed);
            MyGuiControlLabel label24 = this.MakeLabel(MySpaceTexts.WorldSettings_AssemblerEfficiency);
            MyGuiControlLabel label25 = this.MakeLabel(MySpaceTexts.World_Settings_EnableOxygen);
            MyGuiControlLabel oxygenPressurizationLabel = this.MakeLabel(MySpaceTexts.World_Settings_EnableOxygenPressurization);
            MyGuiControlLabel label26 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableRespawnShips);
            MyGuiControlLabel label27 = this.MakeLabel(MySpaceTexts.WorldSettings_RespawnShipDelete);
            MyGuiControlLabel label28 = this.MakeLabel(MySpaceTexts.WorldSettings_LimitWorldSize);
            MyGuiControlLabel label29 = this.MakeLabel(MySpaceTexts.WorldSettings_WelderSpeed);
            MyGuiControlLabel label30 = this.MakeLabel(MySpaceTexts.WorldSettings_GrinderSpeed);
            MyGuiControlLabel label31 = this.MakeLabel(MySpaceTexts.WorldSettings_RespawnShipCooldown);
            MyGuiControlLabel label32 = this.MakeLabel(MySpaceTexts.WorldSettings_ViewDistance);
            MyGuiControlLabel label33 = this.MakeLabel(MyCommonTexts.WorldSettings_Physics);
            MyGuiControlLabel label34 = this.MakeLabel(MyCommonTexts.WorldSettings_BlockLimits);
            MyGuiControlLabel label35 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableConvertToStation);
            MyGuiControlLabel label36 = this.MakeLabel(MySpaceTexts.WorldSettings_StationVoxelSupport);
            MyGuiControlLabel label37 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableSunRotation);
            MyGuiControlLabel label38 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableTurrerFriendlyDamage);
            MyGuiControlLabel label39 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableSubGridDamage);
            this.MakeLabel(MySpaceTexts.WorldSettings_EnableRealisticDampeners);
            MyGuiControlLabel label40 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableJetpack);
            MyGuiControlLabel label41 = this.MakeLabel(MySpaceTexts.WorldSettings_SpawnWithTools);
            MyGuiControlLabel label42 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableDrones);
            this.m_enableWolfsLabel = this.MakeLabel(MySpaceTexts.WorldSettings_EnableWolfs);
            this.m_enableSpidersLabel = this.MakeLabel(MySpaceTexts.WorldSettings_EnableSpiders);
            MyGuiControlLabel label43 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableRemoteBlockRemoval);
            MyGuiControlLabel label44 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableResearch);
            MyGuiControlLabel label45 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableAutorespawn);
            MyGuiControlLabel label46 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableSupergridding);
            this.m_enableContainerDropsLabel = this.MakeLabel(MySpaceTexts.WorldSettings_EnableContainerDrops);
            MyGuiControlLabel label47 = this.MakeLabel(MySpaceTexts.WorldSettings_EnableVoxelDestruction);
            MyGuiControlLabel label48 = this.MakeLabel(MySpaceTexts.WorldSettings_SoundMode);
            MyGuiControlLabel label49 = this.MakeLabel(MySpaceTexts.Asteroid_Amount);
            float x = 0.309375f;
            position = null;
            backgroundColor = null;
            this.m_passwordTextbox = new MyGuiControlTextbox(position, null, 0x100, backgroundColor, 0.8f, MyGuiControlTextboxType.Normal, MyGuiControlTextboxStyleEnum.Default);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_onlineMode = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_environment = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            this.m_environment.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnvironment));
            this.m_environment.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.HostilityChanged);
            position = null;
            backgroundColor = null;
            this.m_autoHealing = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_thrusterDamage = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_cargoShipsEnabled = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableSpectator = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_resetOwnership = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_permanentDeath = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_destructibleBlocks = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableIngameScripts = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enable3rdPersonCamera = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableEncounters = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableRespawnShips = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableToolShake = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableAdaptiveSimulationQuality = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableVoxelHand = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableOxygen = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_enableOxygen.IsCheckedChanged = delegate (MyGuiControlCheckbox x) {
                if (this.m_showWarningForOxygen && x.IsChecked)
                {
                    MyStringId? okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    okButtonText = null;
                    Vector2? nullable2 = null;
                    MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MySpaceTexts.MessageBoxTextAreYouSureEnableOxygen), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), okButtonText, okButtonText, okButtonText, okButtonText, delegate (MyGuiScreenMessageBox.ResultEnum v) {
                        if (v == MyGuiScreenMessageBox.ResultEnum.NO)
                        {
                            x.IsChecked = false;
                        }
                    }, 0, MyGuiScreenMessageBox.ResultEnum.YES, true, nullable2));
                }
                if (x.IsChecked)
                {
                    this.m_enableOxygenPressurization.Enabled = true;
                    oxygenPressurizationLabel.Enabled = true;
                }
                else
                {
                    this.m_enableOxygenPressurization.IsChecked = false;
                    this.m_enableOxygenPressurization.Enabled = false;
                    oxygenPressurizationLabel.Enabled = false;
                }
            };
            position = null;
            backgroundColor = null;
            this.m_enableOxygenPressurization = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            if (!this.m_enableOxygen.IsChecked)
            {
                this.m_enableOxygenPressurization.Enabled = false;
                oxygenPressurizationLabel.Enabled = false;
            }
            position = null;
            backgroundColor = null;
            this.m_enableCopyPaste = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_weaponsEnabled = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_showPlayerNamesOnHud = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableSunRotation = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_enableSunRotation.IsCheckedChanged = delegate (MyGuiControlCheckbox control) {
                this.m_sunRotationIntervalSlider.Enabled = control.IsChecked;
                this.m_sunRotationPeriodValue.Visible = control.IsChecked;
            };
            position = null;
            backgroundColor = null;
            this.m_enableJetpack = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_spawnWithTools = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableAutoRespawn = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableSupergridding = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableConvertToStation = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableStationVoxelSupport = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            float width = this.m_onlineMode.Size.X * 0.95f;
            float? defaultValue = null;
            backgroundColor = null;
            this.m_maxPlayersSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 2f, (float) MyMultiplayerLobby.MAX_PLAYERS, width, defaultValue, backgroundColor, new StringBuilder("{0}").ToString(), 0, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            defaultValue = null;
            backgroundColor = null;
            this.m_maxFloatingObjectsSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 16f, 56f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 0, 0.7f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            this.m_maxFloatingObjectsSlider.Value = 0f;
            if (MySandboxGame.Config.ExperimentalMode)
            {
                this.m_maxFloatingObjectsSlider.MaxValue = 1024f;
            }
            defaultValue = null;
            backgroundColor = null;
            this.m_maxGridSizeSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 50000f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 1, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            defaultValue = null;
            backgroundColor = null;
            this.m_maxBlocksPerPlayerSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 100000f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 1, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            defaultValue = null;
            backgroundColor = null;
            this.m_totalPCUSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 100f, 100000f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 1, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            this.m_totalPCUSlider.Value = 0f;
            if (MySandboxGame.Config.ExperimentalMode)
            {
                this.m_totalPCUSlider.MinValue = 0f;
                this.m_totalPCUSlider.MaxValue = 1000000f;
            }
            defaultValue = null;
            backgroundColor = null;
            this.m_maxBackupSavesSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 1000f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 0, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            defaultValue = null;
            backgroundColor = null;
            this.m_optimalSpawnDistanceSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 900f, 25000f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 1, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, true, false);
            position = null;
            backgroundColor = null;
            this.m_enableVoxelDestruction = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableDrones = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableWolfs = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableSpiders = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableRemoteBlockRemoval = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableContainerDrops = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableTurretsFriendlyFire = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableSubGridDamage = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableRealisticDampeners = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_enableResearch = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            this.m_respawnShipDelete = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_worldSizeCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_spawnShipTimeCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_soundModeCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            this.m_soundModeCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_soundModeCombo_ItemSelected);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_asteroidAmountCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_assembler = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_charactersInventory = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_blocksInventory = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_refinery = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_welder = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_grinder = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_viewDistanceCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            backgroundColor = null;
            position = null;
            position = null;
            backgroundColor = null;
            this.m_physicsOptionsCombo = new MyGuiControlCombobox(position, new Vector2(x, 0.04f), backgroundColor, position, 10, position, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, backgroundColor);
            position = null;
            position = null;
            backgroundColor = null;
            int? buttonIndex = null;
            this.m_creativeModeButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Small, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.WorldSettings_GameModeCreative), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.CreativeClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_creativeModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeCreative);
            position = null;
            position = null;
            backgroundColor = null;
            buttonIndex = null;
            this.m_survivalModeButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Small, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.WorldSettings_GameModeSurvival), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.SurvivalClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_survivalModeButton.SetToolTip(MySpaceTexts.ToolTipWorldSettingsModeSurvival);
            if (MyFakes.ENABLE_ASTEROID_FIELDS)
            {
                this.m_asteroidAmountCombo.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_asteroidAmountCombo_ItemSelected);
                buttonIndex = null;
                nullable5 = null;
                this.m_asteroidAmountCombo.AddItem((long) (-4), MySpaceTexts.WorldSettings_AsteroidAmountProceduralNone, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_asteroidAmountCombo.AddItem((long) (-5), MySpaceTexts.WorldSettings_AsteroidAmountProceduralLowest, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_asteroidAmountCombo.AddItem(-1L, MySpaceTexts.WorldSettings_AsteroidAmountProceduralLow, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_asteroidAmountCombo.AddItem((long) (-2), MySpaceTexts.WorldSettings_AsteroidAmountProceduralNormal, buttonIndex, nullable5);
                if (MySandboxGame.Config.ExperimentalMode)
                {
                    buttonIndex = null;
                    nullable5 = null;
                    this.m_asteroidAmountCombo.AddItem((long) (-3), MySpaceTexts.WorldSettings_AsteroidAmountProceduralHigh, buttonIndex, nullable5);
                }
                this.m_asteroidAmountCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsAsteroidAmount));
            }
            buttonIndex = null;
            nullable5 = null;
            this.m_soundModeCombo.AddItem(0L, MySpaceTexts.WorldSettings_ArcadeSound, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_soundModeCombo.AddItem(1L, MySpaceTexts.WorldSettings_RealisticSound, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_onlineMode.AddItem(0L, MyCommonTexts.WorldSettings_OnlineModeOffline, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_onlineMode.AddItem(3L, MyCommonTexts.WorldSettings_OnlineModePrivate, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_onlineMode.AddItem(2L, MyCommonTexts.WorldSettings_OnlineModeFriends, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_onlineMode.AddItem(1L, MyCommonTexts.WorldSettings_OnlineModePublic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_environment.AddItem(0L, MySpaceTexts.WorldSettings_EnvironmentHostilitySafe, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_environment.AddItem(1L, MySpaceTexts.WorldSettings_EnvironmentHostilityNormal, buttonIndex, nullable5);
            if (MySandboxGame.Config.ExperimentalMode)
            {
                buttonIndex = null;
                nullable5 = null;
                this.m_environment.AddItem(2L, MySpaceTexts.WorldSettings_EnvironmentHostilityCataclysm, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_environment.AddItem(3L, MySpaceTexts.WorldSettings_EnvironmentHostilityCataclysmUnreal, buttonIndex, nullable5);
            }
            buttonIndex = null;
            nullable5 = null;
            this.m_worldSizeCombo.AddItem(0L, MySpaceTexts.WorldSettings_WorldSize10Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_worldSizeCombo.AddItem(1L, MySpaceTexts.WorldSettings_WorldSize20Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_worldSizeCombo.AddItem(2L, MySpaceTexts.WorldSettings_WorldSize50Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_worldSizeCombo.AddItem(3L, MySpaceTexts.WorldSettings_WorldSize100Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_worldSizeCombo.AddItem(4L, MySpaceTexts.WorldSettings_WorldSizeUnlimited, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(0L, MySpaceTexts.WorldSettings_RespawnShip_CooldownsDisabled, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(1L, MySpaceTexts.WorldSettings_RespawnShip_x01, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(2L, MySpaceTexts.WorldSettings_RespawnShip_x02, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(5L, MySpaceTexts.WorldSettings_RespawnShip_x05, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem((long) 10, MySpaceTexts.WorldSettings_RespawnShip_Default, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem((long) 20, MySpaceTexts.WorldSettings_RespawnShip_x2, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem((long) 50, MySpaceTexts.WorldSettings_RespawnShip_x5, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem((long) 100, MySpaceTexts.WorldSettings_RespawnShip_x10, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(200L, MySpaceTexts.WorldSettings_RespawnShip_x20, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(500L, MySpaceTexts.WorldSettings_RespawnShip_x50, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_spawnShipTimeCombo.AddItem(0x3e8L, MySpaceTexts.WorldSettings_RespawnShip_x100, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_assembler.AddItem(1L, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_assembler.AddItem(3L, MySpaceTexts.WorldSettings_Realistic_x3, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_assembler.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic_x10, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_charactersInventory.AddItem(1L, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_charactersInventory.AddItem(3L, MySpaceTexts.WorldSettings_Realistic_x3, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_charactersInventory.AddItem(5L, MySpaceTexts.WorldSettings_Realistic_x5, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_charactersInventory.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic_x10, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_blocksInventory.AddItem(1L, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_blocksInventory.AddItem(3L, MySpaceTexts.WorldSettings_Realistic_x3, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_blocksInventory.AddItem(5L, MySpaceTexts.WorldSettings_Realistic_x5, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_blocksInventory.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic_x10, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_refinery.AddItem(1L, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_refinery.AddItem(3L, MySpaceTexts.WorldSettings_Realistic_x3, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_refinery.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic_x10, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_welder.AddItem(5L, MySpaceTexts.WorldSettings_Realistic_half, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_welder.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_welder.AddItem((long) 20, MySpaceTexts.WorldSettings_Realistic_x2, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_welder.AddItem((long) 50, MySpaceTexts.WorldSettings_Realistic_x5, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_grinder.AddItem(5L, MySpaceTexts.WorldSettings_Realistic_half, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_grinder.AddItem((long) 10, MySpaceTexts.WorldSettings_Realistic, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_grinder.AddItem((long) 20, MySpaceTexts.WorldSettings_Realistic_x2, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_grinder.AddItem((long) 50, MySpaceTexts.WorldSettings_Realistic_x5, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_viewDistanceCombo.AddItem(0x1388L, MySpaceTexts.WorldSettings_ViewDistance_5_Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_viewDistanceCombo.AddItem(0x1b58L, MySpaceTexts.WorldSettings_ViewDistance_7_Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_viewDistanceCombo.AddItem(0x2710L, MySpaceTexts.WorldSettings_ViewDistance_10_Km, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_viewDistanceCombo.AddItem(0x3a98L, MySpaceTexts.WorldSettings_ViewDistance_15_Km, buttonIndex, nullable5);
            if (MySandboxGame.Config.ExperimentalMode)
            {
                buttonIndex = null;
                nullable5 = null;
                this.m_viewDistanceCombo.AddItem(0x4e20L, MySpaceTexts.WorldSettings_ViewDistance_20_Km, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_viewDistanceCombo.AddItem(0x7530L, MySpaceTexts.WorldSettings_ViewDistance_30_Km, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_viewDistanceCombo.AddItem(0x9c40L, MySpaceTexts.WorldSettings_ViewDistance_40_Km, buttonIndex, nullable5);
                buttonIndex = null;
                nullable5 = null;
                this.m_viewDistanceCombo.AddItem(0xc350L, MySpaceTexts.WorldSettings_ViewDistance_50_Km, buttonIndex, nullable5);
            }
            this.m_physicsOptionsCombo.SetToolTip(MyCommonTexts.WorldSettings_Physics_Tooltip);
            buttonIndex = null;
            nullable5 = null;
            this.m_physicsOptionsCombo.AddItem(4L, MyCommonTexts.WorldSettings_Physics_Fast, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_physicsOptionsCombo.AddItem(8L, MyCommonTexts.WorldSettings_Physics_Normal, buttonIndex, nullable5);
            buttonIndex = null;
            nullable5 = null;
            this.m_physicsOptionsCombo.AddItem((long) 0x20, MyCommonTexts.WorldSettings_Physics_Precise, buttonIndex, nullable5);
            position = null;
            backgroundColor = null;
            this.m_blockLimits = new MyGuiControlCheckbox(position, backgroundColor, null, false, MyGuiControlCheckboxStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.CheckExperimental(this.m_blockLimits, label34, MyCommonTexts.ToolTipWorldSettingsBlockLimits, false);
            this.m_soundModeCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsSoundMode));
            this.m_autoHealing.SetToolTip(string.Format(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsAutoHealing), (int) (MySpaceStatEffect.MAX_REGEN_HEALTH_RATIO * 100f)));
            this.m_thrusterDamage.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsThrusterDamage));
            this.m_cargoShipsEnabled.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnableCargoShips));
            this.CheckExperimental(this.m_enableSpectator, label8, MySpaceTexts.ToolTipWorldSettingsEnableSpectator, true);
            this.CheckExperimental(this.m_resetOwnership, label9, MySpaceTexts.ToolTipWorldSettingsResetOwnership, true);
            this.CheckExperimental(this.m_permanentDeath, label10, MySpaceTexts.ToolTipWorldSettingsPermanentDeath, true);
            this.m_destructibleBlocks.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsDestructibleBlocks));
            this.m_environment.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnvironment));
            this.m_onlineMode.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsOnlineMode));
            this.m_enableCopyPaste.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsEnableCopyPaste));
            this.m_showPlayerNamesOnHud.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsShowPlayerNamesOnHud));
            this.m_maxFloatingObjectsSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxFloatingObjects));
            this.m_maxBackupSavesSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxBackupSaves));
            this.m_maxGridSizeSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxGridSize));
            this.m_maxBlocksPerPlayerSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxBlocksPerPlayer));
            this.m_totalPCUSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsTotalPCU));
            this.m_optimalSpawnDistanceSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsOptimalSpawnDistance));
            this.m_maxPlayersSlider.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsMaxPlayer));
            this.m_weaponsEnabled.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsWeapons));
            this.m_worldSizeCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsLimitWorldSize));
            this.m_viewDistanceCombo.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsViewDistance));
            this.m_respawnShipDelete.SetTooltip(MyTexts.GetString(MySpaceTexts.TooltipWorldSettingsRespawnShipDelete));
            this.m_enableToolShake.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_ToolShake));
            this.CheckExperimental(this.m_enableAdaptiveSimulationQuality, label16, MySpaceTexts.ToolTipWorldSettings_AdaptiveSimulationQuality, false);
            this.m_enableOxygen.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableOxygen));
            this.m_enableOxygenPressurization.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableOxygenPressurization));
            this.m_enableJetpack.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableJetpack));
            this.m_enableAutoRespawn.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsAutorespawn));
            this.m_enableSupergridding.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettingsSupergridding));
            this.m_spawnWithTools.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_SpawnWithTools));
            this.m_enableEncounters.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableEncounters));
            this.m_enableSunRotation.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableSunRotation));
            this.m_enable3rdPersonCamera.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_Enable3rdPersonCamera));
            this.CheckExperimental(this.m_enableIngameScripts, label12, MySpaceTexts.ToolTipWorldSettings_EnableIngameScripts, true);
            this.m_cargoShipsEnabled.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_CargoShipsEnabled));
            this.CheckExperimental(this.m_enableWolfs, this.m_enableWolfsLabel, MySpaceTexts.ToolTipWorldSettings_EnableWolfs, true);
            this.CheckExperimental(this.m_enableSpiders, this.m_enableSpidersLabel, MySpaceTexts.ToolTipWorldSettings_EnableSpiders, true);
            this.m_enableRemoteBlockRemoval.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableRemoteBlockRemoval));
            this.m_enableContainerDrops.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableContainerDrops));
            this.m_enableConvertToStation.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableConvertToStation));
            this.CheckExperimental(this.m_enableStationVoxelSupport, label36, MySpaceTexts.ToolTipWorldSettings_StationVoxelSupport, true);
            this.m_enableRespawnShips.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableRespawnShips));
            this.m_enableVoxelDestruction.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableVoxelDestruction));
            this.m_enableTurretsFriendlyFire.SetToolTip(MyTexts.GetString(MySpaceTexts.TooltipWorldSettings_EnableTurrerFriendlyDamage));
            this.CheckExperimental(this.m_enableSubGridDamage, label39, MySpaceTexts.TooltipWorldSettings_EnableSubGridDamage, true);
            this.m_enableRealisticDampeners.SetToolTip(MyTexts.GetString(MySpaceTexts.TooltipWorldSettings_EnableRealisticDampeners));
            this.m_enableResearch.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_EnableResearch));
            this.m_charactersInventory.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_InventorySize));
            this.m_blocksInventory.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_BlocksInventorySize));
            this.m_assembler.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_AssemblerEfficiency));
            this.m_refinery.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_RefinerySpeed));
            this.m_welder.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_WeldingSpeed));
            this.m_grinder.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_GrindingSpeed));
            this.m_spawnShipTimeCombo.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_RespawnShipCooldown));
            scrolledControl.Controls.Add(label21);
            scrolledControl.Controls.Add(this.m_charactersInventory);
            scrolledControl.Controls.Add(label22);
            scrolledControl.Controls.Add(this.m_blocksInventory);
            scrolledControl.Controls.Add(label24);
            scrolledControl.Controls.Add(this.m_assembler);
            scrolledControl.Controls.Add(label23);
            scrolledControl.Controls.Add(this.m_refinery);
            scrolledControl.Controls.Add(label29);
            scrolledControl.Controls.Add(this.m_welder);
            scrolledControl.Controls.Add(label30);
            scrolledControl.Controls.Add(this.m_grinder);
            scrolledControl.Controls.Add(label);
            scrolledControl.Controls.Add(this.m_environment);
            if (this.m_isNewGame)
            {
                scrolledControl.Controls.Add(label49);
                scrolledControl.Controls.Add(this.m_asteroidAmountCombo);
            }
            if (MyFakes.ENABLE_NEW_SOUNDS)
            {
                scrolledControl.Controls.Add(label48);
                scrolledControl.Controls.Add(this.m_soundModeCombo);
            }
            scrolledControl.Controls.Add(label28);
            scrolledControl.Controls.Add(this.m_worldSizeCombo);
            scrolledControl.Controls.Add(label32);
            scrolledControl.Controls.Add(this.m_viewDistanceCombo);
            scrolledControl.Controls.Add(label31);
            scrolledControl.Controls.Add(this.m_spawnShipTimeCombo);
            defaultValue = null;
            backgroundColor = null;
            this.m_sunRotationIntervalSlider = new MyGuiControlSlider(new Vector2?(Vector2.Zero), 0f, 1f, this.m_onlineMode.Size.X * 0.95f, defaultValue, backgroundColor, null, 1, 0.8f, 0.05f, "White", null, MyGuiControlSliderStyleEnum.Default, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, false, false);
            this.m_sunRotationIntervalSlider.MinValue = !MySandboxGame.Config.ExperimentalMode ? this.MIN_SAFE_TIME_FOR_SUN : 0f;
            this.m_sunRotationIntervalSlider.MaxValue = 1f;
            this.m_sunRotationIntervalSlider.DefaultValue = 0f;
            this.m_sunRotationIntervalSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_sunRotationIntervalSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                this.m_tempBuilder.Clear();
                MyValueFormatter.AppendTimeInBestUnit(MathHelper.Clamp(MathHelper.InterpLog(s.Value, 1f, 1440f), 1f, 1440f) * 60f, this.m_tempBuilder);
                this.m_sunRotationPeriodValue.Text = this.m_tempBuilder.ToString();
            });
            this.m_sunRotationIntervalSlider.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipWorldSettings_DayDuration));
            scrolledControl.Controls.Add(this.m_sunRotationPeriod);
            scrolledControl.Controls.Add(this.m_sunRotationIntervalSlider);
            scrolledControl.Controls.Add(this.m_maxFloatingObjectsLabel);
            scrolledControl.Controls.Add(this.m_maxFloatingObjectsSlider);
            scrolledControl.Controls.Add(label2);
            scrolledControl.Controls.Add(this.m_maxGridSizeSlider);
            scrolledControl.Controls.Add(label3);
            scrolledControl.Controls.Add(this.m_maxBlocksPerPlayerSlider);
            scrolledControl.Controls.Add(label4);
            scrolledControl.Controls.Add(this.m_totalPCUSlider);
            scrolledControl.Controls.Add(label34);
            scrolledControl.Controls.Add(this.m_blockLimits);
            scrolledControl.Controls.Add(this.m_maxBackupSavesLabel);
            scrolledControl.Controls.Add(this.m_maxBackupSavesSlider);
            scrolledControl.Controls.Add(label5);
            scrolledControl.Controls.Add(this.m_optimalSpawnDistanceSlider);
            if (MyFakes.ENABLE_PHYSICS_SETTINGS)
            {
                scrolledControl.Controls.Add(label33);
                scrolledControl.Controls.Add(this.m_physicsOptionsCombo);
            }
            float num3 = 0.21f;
            Vector2 vector4 = new Vector2(0f, 0.052f);
            Vector2 vector2 = (-scrolledControl.Size / 2f) + new Vector2(0f, (this.m_creativeModeButton.Size.Y / 2f) + (vector4.Y / 3f));
            Vector2 vector3 = vector2 + new Vector2(num3, 0f);
            Vector2 size = this.m_onlineMode.Size;
            foreach (MyGuiControlBase base2 in scrolledControl.Controls)
            {
                base2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                if (base2 is MyGuiControlLabel)
                {
                    base2.Position = vector2 + (vector4 * num);
                    continue;
                }
                num++;
                base2.Position = vector3 + (vector4 * num);
                if (((num == 5) || ((num == 9) || (num == 0x11))) || (num == 0x13))
                {
                    float* singlePtr1 = (float*) ref vector2.Y;
                    singlePtr1[0] += vector4.Y / 5f;
                    float* singlePtr2 = (float*) ref vector3.Y;
                    singlePtr2[0] += vector4.Y / 5f;
                }
            }
            this.m_survivalModeButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER;
            this.m_survivalModeButton.Position = this.m_creativeModeButton.Position + new Vector2(this.m_onlineMode.Size.X, 0f);
            this.m_maxBackupSavesSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_maxBackupSavesSlider.ValueChanged, s => this.m_maxBackupSavesValue.Text = s.Value.ToString());
            this.m_maxFloatingObjectsSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_maxFloatingObjectsSlider.ValueChanged, s => this.m_maxFloatingObjectsValue.Text = s.Value.ToString());
            this.m_maxGridSizeSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_maxGridSizeSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                if (s.Value >= 100f)
                {
                    this.m_maxGridSizeValue.Text = (s.Value - (s.Value % 100f)).ToString();
                }
                else
                {
                    this.m_maxGridSizeValue.Text = MyTexts.GetString(MyCommonTexts.Disabled);
                }
            });
            this.m_maxBlocksPerPlayerSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_maxBlocksPerPlayerSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                if (s.Value >= 100f)
                {
                    this.m_maxBlocksPerPlayerValue.Text = (s.Value - (s.Value % 100f)).ToString();
                }
                else
                {
                    this.m_maxBlocksPerPlayerValue.Text = MyTexts.GetString(MyCommonTexts.Disabled);
                }
            });
            this.m_totalPCUSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_totalPCUSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                if (s.Value >= 100f)
                {
                    this.m_totalPCUValue.Text = (s.Value - (s.Value % 100f)).ToString();
                }
                else
                {
                    this.m_totalPCUValue.Text = MyTexts.GetString(MyCommonTexts.Disabled);
                }
            });
            this.m_optimalSpawnDistanceSlider.ValueChanged = (Action<MyGuiControlSlider>) Delegate.Combine(this.m_optimalSpawnDistanceSlider.ValueChanged, delegate (MyGuiControlSlider s) {
                if (s.Value >= 1000f)
                {
                    this.m_optimalSpawnDistanceValue.Text = (s.Value - (s.Value % 100f)).ToString();
                }
                else
                {
                    this.m_optimalSpawnDistanceValue.Text = MyTexts.GetString(MyCommonTexts.Disabled);
                }
            });
            this.m_maxGridSizeValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_maxGridSizeSlider.Position.Y);
            this.m_maxBlocksPerPlayerValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_maxBlocksPerPlayerSlider.Position.Y);
            this.m_totalPCUValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_totalPCUSlider.Position.Y);
            this.m_optimalSpawnDistanceValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_optimalSpawnDistanceSlider.Position.Y);
            this.m_maxFloatingObjectsValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_maxFloatingObjectsSlider.Position.Y);
            this.m_maxBackupSavesValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_maxBackupSavesSlider.Position.Y);
            scrolledControl.Controls.Add(this.m_maxGridSizeValue);
            scrolledControl.Controls.Add(this.m_maxBlocksPerPlayerValue);
            scrolledControl.Controls.Add(this.m_totalPCUValue);
            scrolledControl.Controls.Add(this.m_optimalSpawnDistanceValue);
            scrolledControl.Controls.Add(this.m_maxFloatingObjectsValue);
            scrolledControl.Controls.Add(this.m_maxBackupSavesValue);
            float num4 = 0.055f;
            label6.Position = new Vector2(label6.Position.X - (num3 / 2f), label6.Position.Y + num4);
            this.m_autoHealing.Position = new Vector2(this.m_autoHealing.Position.X - (num3 / 2f), this.m_autoHealing.Position.Y + num4);
            this.m_sunRotationPeriodValue.Position = new Vector2(this.m_sunRotationIntervalSlider.Position.X + 0.31f, this.m_sunRotationIntervalSlider.Position.Y);
            scrolledControl.Controls.Add(this.m_sunRotationPeriodValue);
            int count = scrolledControl.Controls.Count;
            scrolledControl.Controls.Add(label6);
            scrolledControl.Controls.Add(this.m_autoHealing);
            scrolledControl.Controls.Add(label27);
            scrolledControl.Controls.Add(this.m_respawnShipDelete);
            scrolledControl.Controls.Add(label8);
            scrolledControl.Controls.Add(this.m_enableSpectator);
            scrolledControl.Controls.Add(this.m_enableCopyPasteLabel);
            scrolledControl.Controls.Add(this.m_enableCopyPaste);
            scrolledControl.Controls.Add(label20);
            scrolledControl.Controls.Add(this.m_showPlayerNamesOnHud);
            scrolledControl.Controls.Add(label9);
            scrolledControl.Controls.Add(this.m_resetOwnership);
            scrolledControl.Controls.Add(label7);
            scrolledControl.Controls.Add(this.m_thrusterDamage);
            scrolledControl.Controls.Add(label10);
            scrolledControl.Controls.Add(this.m_permanentDeath);
            scrolledControl.Controls.Add(label19);
            scrolledControl.Controls.Add(this.m_weaponsEnabled);
            if (MyFakes.ENABLE_CARGO_SHIPS)
            {
                scrolledControl.Controls.Add(label18);
                scrolledControl.Controls.Add(this.m_cargoShipsEnabled);
            }
            scrolledControl.Controls.Add(label11);
            scrolledControl.Controls.Add(this.m_destructibleBlocks);
            if (MyFakes.ENABLE_PROGRAMMABLE_BLOCK)
            {
                scrolledControl.Controls.Add(label12);
                scrolledControl.Controls.Add(this.m_enableIngameScripts);
            }
            if (MyFakes.ENABLE_TOOL_SHAKE)
            {
                scrolledControl.Controls.Add(label15);
                scrolledControl.Controls.Add(this.m_enableToolShake);
            }
            scrolledControl.Controls.Add(label16);
            scrolledControl.Controls.Add(this.m_enableAdaptiveSimulationQuality);
            scrolledControl.Controls.Add(label17);
            scrolledControl.Controls.Add(this.m_enableVoxelHand);
            scrolledControl.Controls.Add(label14);
            scrolledControl.Controls.Add(this.m_enableEncounters);
            scrolledControl.Controls.Add(label13);
            scrolledControl.Controls.Add(this.m_enable3rdPersonCamera);
            scrolledControl.Controls.Add(label25);
            scrolledControl.Controls.Add(this.m_enableOxygen);
            scrolledControl.Controls.Add(oxygenPressurizationLabel);
            scrolledControl.Controls.Add(this.m_enableOxygenPressurization);
            scrolledControl.Controls.Add(label37);
            scrolledControl.Controls.Add(this.m_enableSunRotation);
            scrolledControl.Controls.Add(label35);
            scrolledControl.Controls.Add(this.m_enableConvertToStation);
            scrolledControl.Controls.Add(label36);
            scrolledControl.Controls.Add(this.m_enableStationVoxelSupport);
            scrolledControl.Controls.Add(label40);
            scrolledControl.Controls.Add(this.m_enableJetpack);
            scrolledControl.Controls.Add(label41);
            scrolledControl.Controls.Add(this.m_spawnWithTools);
            scrolledControl.Controls.Add(label47);
            scrolledControl.Controls.Add(this.m_enableVoxelDestruction);
            scrolledControl.Controls.Add(label42);
            scrolledControl.Controls.Add(this.m_enableDrones);
            scrolledControl.Controls.Add(this.m_enableWolfsLabel);
            scrolledControl.Controls.Add(this.m_enableWolfs);
            scrolledControl.Controls.Add(this.m_enableSpidersLabel);
            scrolledControl.Controls.Add(this.m_enableSpiders);
            scrolledControl.Controls.Add(label43);
            scrolledControl.Controls.Add(this.m_enableRemoteBlockRemoval);
            scrolledControl.Controls.Add(label39);
            scrolledControl.Controls.Add(this.m_enableSubGridDamage);
            scrolledControl.Controls.Add(label38);
            scrolledControl.Controls.Add(this.m_enableTurretsFriendlyFire);
            scrolledControl.Controls.Add(this.m_enableContainerDropsLabel);
            scrolledControl.Controls.Add(this.m_enableContainerDrops);
            scrolledControl.Controls.Add(label26);
            scrolledControl.Controls.Add(this.m_enableRespawnShips);
            scrolledControl.Controls.Add(label44);
            scrolledControl.Controls.Add(this.m_enableResearch);
            scrolledControl.Controls.Add(label45);
            scrolledControl.Controls.Add(this.m_enableAutoRespawn);
            scrolledControl.Controls.Add(label46);
            scrolledControl.Controls.Add(this.m_enableSupergridding);
            float num6 = 0.018f;
            Vector2 vector5 = new Vector2((num3 + num6) + 0.05f, 0f);
            int num7 = 2;
            float single1 = ((vector5.X * num7) - 0.05f) / 2f;
            float* singlePtr3 = (float*) ref vector3.X;
            singlePtr3[0] += num6;
            for (int i = count; i < scrolledControl.Controls.Count; i++)
            {
                MyGuiControlBase base3 = scrolledControl.Controls[i];
                int num11 = ((i - count) / 2) % num7;
                if (((i - count) % 2) == 0)
                {
                    base3.Position = (vector2 + (num11 * vector5)) + (vector4 * num);
                }
                else
                {
                    base3.Position = (vector3 + (num11 * vector5)) + (vector4 * num);
                    if (num11 == (num7 - 1))
                    {
                        num++;
                    }
                }
            }
            Vector2 vector6 = ((base.m_size.Value / 2f) - vector) * new Vector2(0f, 1f);
            float num8 = 25f;
            position = null;
            position = null;
            backgroundColor = null;
            buttonIndex = null;
            this.m_okButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Ok), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OkButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Ok));
            position = null;
            position = null;
            backgroundColor = null;
            buttonIndex = null;
            this.m_cancelButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.Default, position, backgroundColor, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Cancel), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.CancelButtonClicked), GuiSounds.MouseClick, 1f, buttonIndex, false);
            this.m_okButton.Position = vector6 + (new Vector2(-num8, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            this.m_okButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM;
            this.m_cancelButton.Position = vector6 + (new Vector2(num8, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE);
            this.m_cancelButton.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM;
            this.m_cancelButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipOptionsSpace_Cancel));
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(control);
            base.CloseButtonEnabled = true;
        }

        public static Vector2 CalcSize() => 
            new Vector2(0.6535714f, 0.9398855f);

        private void CancelButtonClicked(object sender)
        {
            this.CloseScreen();
        }

        private void CheckButton(MyGuiControlButton active, params MyGuiControlButton[] allButtons)
        {
            foreach (MyGuiControlButton button in allButtons)
            {
                if (ReferenceEquals(button, active) && !button.Checked)
                {
                    button.Checked = true;
                }
                else if (!ReferenceEquals(button, active) && button.Checked)
                {
                    button.Checked = false;
                }
            }
        }

        private void CheckButton(float value, params MyGuiControlButton[] allButtons)
        {
            bool flag = false;
            foreach (MyGuiControlButton button in allButtons)
            {
                if (button.UserData is float)
                {
                    if ((((float) button.UserData) == value) && !button.Checked)
                    {
                        flag = true;
                        button.Checked = true;
                    }
                    else if ((((float) button.UserData) != value) && button.Checked)
                    {
                        button.Checked = false;
                    }
                }
            }
            if (!flag)
            {
                allButtons[0].Checked = true;
            }
        }

        private void CheckExperimental(MyGuiControlBase control, MyGuiControlLabel label, MyStringId toolTip, bool enabled = true)
        {
            if (MySandboxGame.Config.ExperimentalMode)
            {
                control.SetToolTip(MyTexts.GetString(toolTip));
            }
            else if (enabled)
            {
                control.SetEnabledByExperimental();
                label.SetEnabledByExperimental();
            }
            else
            {
                control.SetDisabledByExperimental();
                label.SetDisabledByExperimental();
            }
        }

        private void CreativeClicked(object sender)
        {
            this.UpdateSurvivalState(false);
            this.m_enableContainerDrops.IsChecked = false;
        }

        private float GetAssemblerMultiplier() => 
            ((float) this.m_assembler.GetSelectedKey());

        private float GetBlocksInventoryMultiplier() => 
            ((float) this.m_blocksInventory.GetSelectedKey());

        public override string GetFriendlyName() => 
            "MyGuiScreenAdvancedWorldSettings";

        private MyGameModeEnum GetGameMode() => 
            (this.m_survivalModeButton.Checked ? MyGameModeEnum.Survival : MyGameModeEnum.Creative);

        private float GetGrinderMultiplier() => 
            (((float) this.m_grinder.GetSelectedKey()) / 10f);

        private float GetInventoryMultiplier() => 
            ((float) this.m_charactersInventory.GetSelectedKey());

        private float GetMultiplier(params MyGuiControlButton[] buttons)
        {
            foreach (MyGuiControlButton button in buttons)
            {
                if (button.Checked && (button.UserData is float))
                {
                    return (float) button.UserData;
                }
            }
            return 1f;
        }

        private float GetRefineryMultiplier() => 
            ((float) this.m_refinery.GetSelectedKey());

        public void GetSettings(MyObjectBuilder_SessionSettings output)
        {
            output.OnlineMode = (MyOnlineModeEnum) ((int) this.m_onlineMode.GetSelectedKey());
            output.EnvironmentHostility = (MyEnvironmentHostilityEnum) ((int) this.m_environment.GetSelectedKey());
            switch (this.AsteroidAmount)
            {
                case -5:
                    output.ProceduralDensity = 0.1f;
                    break;

                case -4:
                    output.ProceduralDensity = 0f;
                    break;

                case -3:
                    output.ProceduralDensity = 0.5f;
                    break;

                case -2:
                    output.ProceduralDensity = 0.35f;
                    break;

                case -1:
                    output.ProceduralDensity = 0.25f;
                    break;

                default:
                    throw new InvalidBranchException();
            }
            output.AutoHealing = this.m_autoHealing.IsChecked;
            output.CargoShipsEnabled = this.m_cargoShipsEnabled.IsChecked;
            output.EnableCopyPaste = this.m_enableCopyPaste.IsChecked;
            output.EnableSpectator = this.m_enableSpectator.IsChecked;
            output.ResetOwnership = this.m_resetOwnership.IsChecked;
            output.PermanentDeath = new bool?(this.m_permanentDeath.IsChecked);
            output.DestructibleBlocks = this.m_destructibleBlocks.IsChecked;
            output.EnableIngameScripts = this.m_enableIngameScripts.IsChecked;
            output.Enable3rdPersonView = this.m_enable3rdPersonCamera.IsChecked;
            output.EnableEncounters = this.m_enableEncounters.IsChecked;
            output.EnableToolShake = this.m_enableToolShake.IsChecked;
            output.AdaptiveSimulationQuality = this.m_enableAdaptiveSimulationQuality.IsChecked;
            output.EnableVoxelHand = this.m_enableVoxelHand.IsChecked;
            output.ShowPlayerNamesOnHud = this.m_showPlayerNamesOnHud.IsChecked;
            output.ThrusterDamage = this.m_thrusterDamage.IsChecked;
            output.WeaponsEnabled = this.m_weaponsEnabled.IsChecked;
            output.EnableOxygen = this.m_enableOxygen.IsChecked;
            if (output.EnableOxygen && (output.VoxelGeneratorVersion < 1))
            {
                output.VoxelGeneratorVersion = 1;
            }
            output.EnableOxygenPressurization = this.m_enableOxygenPressurization.IsChecked;
            output.RespawnShipDelete = this.m_respawnShipDelete.IsChecked;
            output.EnableConvertToStation = this.m_enableConvertToStation.IsChecked;
            output.StationVoxelSupport = this.m_enableStationVoxelSupport.IsChecked;
            output.EnableAutorespawn = this.m_enableAutoRespawn.IsChecked;
            output.EnableSupergridding = this.m_enableSupergridding.IsChecked;
            output.EnableRespawnShips = this.m_enableRespawnShips.IsChecked;
            output.EnableWolfs = this.m_enableWolfs.IsChecked;
            output.EnableSunRotation = this.m_enableSunRotation.IsChecked;
            output.EnableJetpack = this.m_enableJetpack.IsChecked;
            output.SpawnWithTools = this.m_spawnWithTools.IsChecked;
            output.EnableVoxelDestruction = this.m_enableVoxelDestruction.IsChecked;
            output.EnableDrones = this.m_enableDrones.IsChecked;
            output.EnableTurretsFriendlyFire = this.m_enableTurretsFriendlyFire.IsChecked;
            output.EnableSubgridDamage = this.m_enableSubGridDamage.IsChecked;
            output.EnableSpiders = this.m_enableSpiders.IsChecked;
            output.EnableRemoteBlockRemoval = this.m_enableRemoteBlockRemoval.IsChecked;
            output.EnableResearch = this.m_enableResearch.IsChecked;
            output.MaxPlayers = (short) this.m_maxPlayersSlider.Value;
            output.MaxFloatingObjects = (short) this.m_maxFloatingObjectsSlider.Value;
            output.MaxBackupSaves = (short) this.m_maxBackupSavesSlider.Value;
            output.MaxGridSize = (int) (this.m_maxGridSizeSlider.Value - (this.m_maxGridSizeSlider.Value % 100f));
            output.MaxBlocksPerPlayer = (int) (this.m_maxBlocksPerPlayerSlider.Value - (this.m_maxBlocksPerPlayerSlider.Value % 100f));
            output.TotalPCU = (int) (this.m_totalPCUSlider.Value - (this.m_totalPCUSlider.Value % 100f));
            output.OptimalSpawnDistance = (int) (this.m_optimalSpawnDistanceSlider.Value - (this.m_optimalSpawnDistanceSlider.Value % 100f));
            output.BlockLimitsEnabled = this.m_blockLimits.IsChecked ? MyBlockLimitsEnabledEnum.GLOBALLY : MyBlockLimitsEnabledEnum.NONE;
            output.SunRotationIntervalMinutes = MathHelper.Clamp(MathHelper.InterpLog(this.m_sunRotationIntervalSlider.Value, 1f, 1440f), 1f, 1440f);
            output.AssemblerEfficiencyMultiplier = this.GetAssemblerMultiplier();
            output.AssemblerSpeedMultiplier = this.GetAssemblerMultiplier();
            output.InventorySizeMultiplier = this.GetInventoryMultiplier();
            output.BlocksInventorySizeMultiplier = this.GetBlocksInventoryMultiplier();
            output.RefinerySpeedMultiplier = this.GetRefineryMultiplier();
            output.WelderSpeedMultiplier = this.GetWelderMultiplier();
            output.GrinderSpeedMultiplier = this.GetGrinderMultiplier();
            output.SpawnShipTimeMultiplier = this.GetSpawnShipTimeMultiplier();
            output.RealisticSound = ((int) this.m_soundModeCombo.GetSelectedKey()) == 1;
            output.EnvironmentHostility = (MyEnvironmentHostilityEnum) ((int) this.m_environment.GetSelectedKey());
            output.WorldSizeKm = this.GetWorldSize();
            output.ViewDistance = this.GetViewDistance();
            output.PhysicsIterations = (int) this.m_physicsOptionsCombo.GetSelectedKey();
            output.GameMode = this.GetGameMode();
            if (output.GameMode != MyGameModeEnum.Creative)
            {
                output.EnableContainerDrops = this.m_enableContainerDrops.IsChecked;
            }
        }

        private float GetSpawnShipTimeMultiplier() => 
            (((float) this.m_spawnShipTimeCombo.GetSelectedKey()) / 10f);

        public int GetViewDistance()
        {
            long selectedKey = this.m_viewDistanceCombo.GetSelectedKey();
            return ((selectedKey != 0) ? ((int) selectedKey) : this.m_customViewDistance);
        }

        private float GetWelderMultiplier() => 
            (((float) this.m_welder.GetSelectedKey()) / 10f);

        public int GetWorldSize()
        {
            long num;
            int worldSizeKm = this.m_parent.Settings.WorldSizeKm;
            if (num > 5L)
            {
                long selectedKey = this.m_worldSizeCombo.GetSelectedKey();
            }
            else
            {
                switch (((uint) this.m_worldSizeCombo.GetSelectedKey()))
                {
                    case 0:
                        return 10;

                    case 1:
                        return 20;

                    case 2:
                        return 50;

                    case 3:
                        return 100;

                    case 4:
                        return 0;

                    case 5:
                        return this.m_customWorldSize;

                    default:
                        break;
                }
            }
            return 0;
        }

        private void HostilityChanged()
        {
            this.m_isHostilityChanged = true;
        }

        private void LoadValues()
        {
            this.m_passwordTextbox.Text = !this.m_isNewGame ? this.m_parent.Checkpoint.Password : "";
            this.SetSettings(this.m_parent.Settings);
        }

        private void m_asteroidAmountCombo_ItemSelected()
        {
            this.m_asteroidAmount = new int?((int) this.m_asteroidAmountCombo.GetSelectedKey());
        }

        private void m_soundModeCombo_ItemSelected()
        {
            if (this.m_soundModeCombo.GetSelectedIndex() == 1)
            {
                this.m_parent.Settings.EnableOxygenPressurization = true;
            }
        }

        private MyGuiControlLabel MakeLabel(MyStringId textEnum)
        {
            Vector2? position = null;
            position = null;
            return new MyGuiControlLabel(position, position, MyTexts.GetString(textEnum), null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
        }

        private void OkButtonClicked(object sender)
        {
            this.m_isConfirmed = true;
            if (this.OnOkButtonClicked != null)
            {
                this.OnOkButtonClicked();
            }
            this.CloseScreen();
        }

        private void OnOnlineModeItemSelected()
        {
            if (((int) this.m_onlineMode.GetSelectedKey()) == 0)
            {
                this.m_optimalSpawnDistanceSlider.Value = this.m_optimalSpawnDistanceSlider.MinValue;
                this.m_optimalSpawnDistanceSlider.Enabled = false;
            }
            else
            {
                this.m_optimalSpawnDistanceSlider.Enabled = true;
                this.m_optimalSpawnDistanceSlider.Value = this.m_optimalSpawnDistanceSlider.DefaultValue.GetValueOrDefault(4000f);
            }
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            this.m_recreating_control = true;
            this.BuildControls();
            this.LoadValues();
            this.m_recreating_control = false;
        }

        public void SetSettings(MyObjectBuilder_SessionSettings settings)
        {
            float num2;
            this.m_onlineMode.SelectItemByKey((long) settings.OnlineMode, true);
            this.m_environment.SelectItemByKey((long) settings.EnvironmentHostility, true);
            this.m_worldSizeCombo.SelectItemByKey((long) this.WorldSizeEnumKey(settings.WorldSizeKm), true);
            this.m_spawnShipTimeCombo.SelectItemByKey((long) ((int) (settings.SpawnShipTimeMultiplier * 10f)), true);
            this.m_viewDistanceCombo.SelectItemByKey((long) this.ViewDistanceEnumKey(settings.ViewDistance), true);
            this.m_soundModeCombo.SelectItemByKey(settings.RealisticSound ? ((long) 1) : ((long) 0), true);
            this.m_asteroidAmountCombo.SelectItemByKey((long) ((int) settings.ProceduralDensity), true);
            int num = (int) (settings.ProceduralDensity * 100f);
            if (num > 10)
            {
                if (num == 0x19)
                {
                    this.m_asteroidAmountCombo.SelectItemByKey(-1L, true);
                    goto TR_000A;
                }
                else if (num == 0x23)
                {
                    this.m_asteroidAmountCombo.SelectItemByKey((long) (-2), true);
                    goto TR_000A;
                }
                else if (num == 50)
                {
                    this.m_asteroidAmountCombo.SelectItemByKey((long) (-3), true);
                    goto TR_000A;
                }
            }
            else if (num == 0)
            {
                this.m_asteroidAmountCombo.SelectItemByKey((long) (-4), true);
                goto TR_000A;
            }
            else if (num == 10)
            {
                this.m_asteroidAmountCombo.SelectItemByKey((long) (-5), true);
                goto TR_000A;
            }
            this.m_asteroidAmountCombo.SelectItemByKey(-1L, true);
        TR_000A:
            this.m_environment.SelectItemByKey((long) settings.EnvironmentHostility, true);
            if (this.m_physicsOptionsCombo.TryGetItemByKey((long) settings.PhysicsIterations) != null)
            {
                this.m_physicsOptionsCombo.SelectItemByKey((long) settings.PhysicsIterations, true);
            }
            else
            {
                this.m_physicsOptionsCombo.SelectItemByKey(4L, true);
            }
            this.m_autoHealing.IsChecked = settings.AutoHealing;
            this.m_cargoShipsEnabled.IsChecked = settings.CargoShipsEnabled;
            this.m_enableCopyPaste.IsChecked = settings.EnableCopyPaste;
            this.m_enableSpectator.IsChecked = settings.EnableSpectator;
            this.m_resetOwnership.IsChecked = settings.ResetOwnership;
            this.m_permanentDeath.IsChecked = settings.PermanentDeath.Value;
            this.m_destructibleBlocks.IsChecked = settings.DestructibleBlocks;
            this.m_enableEncounters.IsChecked = settings.EnableEncounters;
            this.m_enable3rdPersonCamera.IsChecked = settings.Enable3rdPersonView;
            this.m_enableIngameScripts.IsChecked = settings.EnableIngameScripts;
            this.m_enableToolShake.IsChecked = settings.EnableToolShake;
            this.m_enableAdaptiveSimulationQuality.IsChecked = settings.AdaptiveSimulationQuality;
            this.m_enableVoxelHand.IsChecked = settings.EnableVoxelHand;
            this.m_showPlayerNamesOnHud.IsChecked = settings.ShowPlayerNamesOnHud;
            this.m_thrusterDamage.IsChecked = settings.ThrusterDamage;
            this.m_weaponsEnabled.IsChecked = settings.WeaponsEnabled;
            this.m_enableOxygen.IsChecked = settings.EnableOxygen;
            if (settings.VoxelGeneratorVersion < 1)
            {
                this.m_showWarningForOxygen = true;
            }
            this.m_enableOxygenPressurization.IsChecked = settings.EnableOxygenPressurization;
            this.m_enableRespawnShips.IsChecked = settings.EnableRespawnShips;
            this.m_respawnShipDelete.IsChecked = settings.RespawnShipDelete;
            this.m_enableConvertToStation.IsChecked = settings.EnableConvertToStation;
            this.m_enableStationVoxelSupport.IsChecked = settings.StationVoxelSupport;
            this.m_enableSunRotation.IsChecked = settings.EnableSunRotation;
            this.m_enableJetpack.IsChecked = settings.EnableJetpack;
            this.m_spawnWithTools.IsChecked = settings.SpawnWithTools;
            this.m_enableAutoRespawn.IsChecked = settings.EnableAutorespawn;
            this.m_enableSupergridding.IsChecked = settings.EnableSupergridding;
            this.m_sunRotationIntervalSlider.Enabled = this.m_enableSunRotation.IsChecked;
            this.m_sunRotationPeriodValue.Visible = this.m_enableSunRotation.IsChecked;
            this.m_sunRotationIntervalSlider.Value = 0.03f;
            this.m_sunRotationIntervalSlider.Value = MathHelper.Clamp(MathHelper.InterpLogInv(settings.SunRotationIntervalMinutes, 1f, 1440f), 0f, 1f);
            this.m_maxPlayersSlider.Value = settings.MaxPlayers;
            this.m_maxFloatingObjectsSlider.Value = settings.MaxFloatingObjects;
            this.m_maxGridSizeSlider.Value = settings.MaxGridSize;
            this.m_maxBlocksPerPlayerSlider.Value = settings.MaxBlocksPerPlayer;
            this.m_blockLimits.IsChecked = settings.BlockLimitsEnabled != MyBlockLimitsEnabledEnum.NONE;
            this.m_blockLimits.IsCheckedChanged = new Action<MyGuiControlCheckbox>(this.blockLimits_CheckedChanged);
            this.blockLimits_CheckedChanged(this.m_blockLimits);
            if (MySandboxGame.Config.ExperimentalMode)
            {
                this.m_totalPCUSlider.MinValue = 0f;
                this.m_totalPCUSlider.MaxValue = 1000000f;
            }
            else
            {
                this.m_totalPCUSlider.MinValue = 100f;
                this.m_totalPCUSlider.MaxValue = (settings.OnlineMode == MyOnlineModeEnum.OFFLINE) ? 100000f : 50000f;
            }
            this.m_totalPCUSlider.Value = settings.TotalPCU;
            this.m_optimalSpawnDistanceSlider.Value = num2 = settings.OptimalSpawnDistance;
            this.m_optimalSpawnDistanceSlider.DefaultValue = new float?(num2);
            this.m_onlineMode.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.OnOnlineModeItemSelected);
            this.OnOnlineModeItemSelected();
            this.m_maxBackupSavesSlider.Value = settings.MaxBackupSaves;
            this.m_enableSubGridDamage.IsChecked = settings.EnableSubgridDamage;
            this.m_enableTurretsFriendlyFire.IsChecked = settings.EnableTurretsFriendlyFire;
            this.m_enableVoxelDestruction.IsChecked = settings.EnableVoxelDestruction;
            this.m_enableDrones.IsChecked = settings.EnableDrones;
            this.m_enableWolfs.IsChecked = settings.EnableWolfs;
            this.m_enableSpiders.IsChecked = settings.EnableSpiders;
            this.m_enableRemoteBlockRemoval.IsChecked = settings.EnableRemoteBlockRemoval;
            this.m_enableResearch.IsChecked = settings.EnableResearch;
            this.m_enableContainerDrops.IsChecked = (settings.GameMode != MyGameModeEnum.Creative) ? settings.EnableContainerDrops : false;
            this.m_assembler.SelectItemByKey((long) ((int) settings.AssemblerEfficiencyMultiplier), true);
            this.m_charactersInventory.SelectItemByKey((long) ((int) settings.InventorySizeMultiplier), true);
            this.m_blocksInventory.SelectItemByKey((long) ((int) settings.BlocksInventorySizeMultiplier), true);
            this.m_refinery.SelectItemByKey((long) ((int) settings.RefinerySpeedMultiplier), true);
            this.m_welder.SelectItemByKey((long) ((int) (settings.WelderSpeedMultiplier * 10f)), true);
            this.m_grinder.SelectItemByKey((long) ((int) (settings.GrinderSpeedMultiplier * 10f)), true);
            this.UpdateSurvivalState(settings.GameMode == MyGameModeEnum.Survival);
        }

        private void SurvivalClicked(object sender)
        {
            this.UpdateSurvivalState(true);
            this.m_enableContainerDrops.IsChecked = this.m_survivalModeButton.Checked;
        }

        public void UpdateSurvivalState(bool survivalEnabled)
        {
            this.m_creativeModeButton.Checked = !survivalEnabled;
            this.m_survivalModeButton.Checked = survivalEnabled;
            this.m_enableCopyPaste.Enabled = !survivalEnabled;
            this.m_enableCopyPasteLabel.Enabled = !survivalEnabled;
            this.m_enableContainerDrops.Enabled = survivalEnabled;
            this.m_enableContainerDropsLabel.Enabled = survivalEnabled;
        }

        private MyViewDistanceEnum ViewDistanceEnumKey(int viewDistance)
        {
            MyViewDistanceEnum enum2 = (MyViewDistanceEnum) viewDistance;
            if ((enum2 != MyViewDistanceEnum.CUSTOM) && System.Enum.IsDefined(typeof(MyViewDistanceEnum), enum2))
            {
                return (MyViewDistanceEnum) viewDistance;
            }
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_viewDistanceCombo.AddItem(5L, MySpaceTexts.WorldSettings_ViewDistance_Custom, sortOrder, toolTip);
            this.m_viewDistanceCombo.SelectItemByKey(5L, true);
            this.m_customViewDistance = viewDistance;
            return MyViewDistanceEnum.CUSTOM;
        }

        private MyWorldSizeEnum WorldSizeEnumKey(int worldSize)
        {
            if (worldSize <= 10)
            {
                if (worldSize == 0)
                {
                    return MyWorldSizeEnum.UNLIMITED;
                }
                if (worldSize == 10)
                {
                    return MyWorldSizeEnum.TEN_KM;
                }
            }
            else
            {
                if (worldSize == 20)
                {
                    return MyWorldSizeEnum.TWENTY_KM;
                }
                if (worldSize == 50)
                {
                    return MyWorldSizeEnum.FIFTY_KM;
                }
                if (worldSize == 100)
                {
                    return MyWorldSizeEnum.HUNDRED_KM;
                }
            }
            int? sortOrder = null;
            MyStringId? toolTip = null;
            this.m_worldSizeCombo.AddItem(5L, MySpaceTexts.WorldSettings_WorldSizeCustom, sortOrder, toolTip);
            this.m_customWorldSize = worldSize;
            return MyWorldSizeEnum.CUSTOM;
        }

        public int AsteroidAmount
        {
            get => 
                ((this.m_asteroidAmount != null) ? this.m_asteroidAmount.Value : -1);
            set
            {
                this.m_asteroidAmount = new int?(value);
                switch (value)
                {
                    case -4:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-4), true);
                        return;

                    case -3:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-3), true);
                        return;

                    case -2:
                        this.m_asteroidAmountCombo.SelectItemByKey((long) (-2), true);
                        return;

                    case -1:
                        this.m_asteroidAmountCombo.SelectItemByKey(-1L, true);
                        return;

                    case 0:
                        this.m_asteroidAmountCombo.SelectItemByKey(0L, true);
                        return;

                    case 1:
                    case 2:
                    case 3:
                    case 5:
                    case 6:
                        return;

                    case 4:
                        this.m_asteroidAmountCombo.SelectItemByKey(4L, true);
                        return;

                    case 7:
                        this.m_asteroidAmountCombo.SelectItemByKey(7L, true);
                        return;
                }
                if (value == 0x10)
                {
                    this.m_asteroidAmountCombo.SelectItemByKey((long) 0x10, true);
                }
            }
        }

        public string Password =>
            this.m_passwordTextbox.Text;

        public bool IsConfirmed =>
            this.m_isConfirmed;

        private enum AsteroidAmountEnum
        {
            None = 0,
            Normal = 4,
            More = 7,
            Many = 0x10,
            ProceduralLowest = -5,
            ProceduralLow = -1,
            ProceduralNormal = -2,
            ProceduralHigh = -3,
            ProceduralNone = -4
        }

        private enum MyFloraDensityEnum
        {
            NONE = 0,
            LOW = 10,
            MEDIUM = 20,
            HIGH = 30,
            EXTREME = 40
        }

        private enum MySoundModeEnum
        {
            Arcade,
            Realistic
        }

        private enum MyViewDistanceEnum
        {
            CUSTOM = 0,
            FIVE_KM = 0x1388,
            SEVEN_KM = 0x1b58,
            TEN_KM = 0x2710,
            FIFTEEN_KM = 0x3a98,
            TWENTY_KM = 0x4e20,
            THIRTY_KM = 0x7530,
            FORTY_KM = 0x9c40,
            FIFTY_KM = 0xc350
        }

        private enum MyWorldSizeEnum
        {
            TEN_KM,
            TWENTY_KM,
            FIFTY_KM,
            HUNDRED_KM,
            UNLIMITED,
            CUSTOM
        }
    }
}

