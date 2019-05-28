namespace Sandbox.Game.Screens
{
    using Sandbox.Game;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Localization;
    using Sandbox.Graphics.GUI;
    using System;
    using VRage;
    using VRage.Game;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenServerSearchSpace : MyGuiScreenServerSearchBase
    {
        public MyGuiScreenServerSearchSpace(MyGuiScreenJoinGame joinScreen) : base(joinScreen)
        {
        }

        private MyGuiControlIndeterminateCheckbox[] AddCheckboxRow(MyStringId?[] text, MySpaceBoolOptionEnum[] keys, MyStringId[] tooltip)
        {
            Action<MyGuiControlIndeterminateCheckbox>[] onClick = new Action<MyGuiControlIndeterminateCheckbox>[2];
            CheckStateEnum[] values = new CheckStateEnum[2];
            if (keys.Length != 0)
            {
                MyFilterBool filter = this.SpaceFilters.GetFilter(keys[0]);
                if (filter == null)
                {
                    throw new ArgumentOutOfRangeException("keys", keys[0], "Filter not found in dictionary!");
                }
                onClick[0] = delegate (MyGuiControlIndeterminateCheckbox c) {
                    filter.CheckValue = c.State;
                };
                values[0] = filter.CheckValue;
            }
            if (keys.Length > 1)
            {
                MyFilterBool bool1 = this.SpaceFilters.GetFilter(keys[1]);
                if (bool1 == null)
                {
                    throw new ArgumentOutOfRangeException("keys", keys[1], "Filter not found in dictionary!");
                }
                onClick[1] = delegate (MyGuiControlIndeterminateCheckbox c) {
                    bool1.CheckValue = c.State;
                };
                values[1] = bool1.CheckValue;
            }
            if (keys.Length > 2)
            {
                throw new ArgumentOutOfRangeException();
            }
            MyStringId?[] nullableArray1 = new MyStringId?[] { new MyStringId?(tooltip[0]), new MyStringId?(tooltip[1]) };
            return base.AddIndeterminateDuo(text, onClick, nullableArray1, values, base.EnableAdvanced);
        }

        private void AddNumericRangeOption(MyStringId text, MySpaceNumericOptionEnum key)
        {
            MyFilterRange filter = this.SpaceFilters.GetFilter(key);
            if (filter == null)
            {
                throw new ArgumentOutOfRangeException("key", key, "Filter not found in dictionary!");
            }
            base.AddNumericRangeOption(text, r => filter.Value = r, filter.Value, filter.Active, c => filter.Active = c.IsChecked, base.EnableAdvanced);
        }

        protected override void DrawBottomControls()
        {
            base.DrawBottomControls();
            MyStringId?[] text = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_EnableCopyPaste), new MyStringId?(MySpaceTexts.WorldSettings_EnableIngameScripts) };
            MySpaceBoolOptionEnum[] keys = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.CopyPaste, MySpaceBoolOptionEnum.Scripts };
            MyStringId[] tooltip = new MyStringId[] { MySpaceTexts.ToolTipWorldSettingsEnableCopyPaste, MySpaceTexts.ToolTipWorldSettings_EnableIngameScripts };
            this.AddCheckboxRow(text, keys, tooltip);
            MyStringId?[] nullableArray2 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_PermanentDeath), new MyStringId?(MySpaceTexts.WorldSettings_EnableWeapons) };
            MySpaceBoolOptionEnum[] enumArray2 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.PermanentDeath, MySpaceBoolOptionEnum.Weapons };
            MyStringId[] idArray2 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettingsPermanentDeath, MySpaceTexts.ToolTipWorldSettingsWeapons };
            this.AddCheckboxRow(nullableArray2, enumArray2, idArray2);
            MyStringId?[] nullableArray3 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_Enable3rdPersonCamera), new MyStringId?(MySpaceTexts.WorldSettings_EnableSpectator) };
            MySpaceBoolOptionEnum[] enumArray3 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.ThirdPerson, MySpaceBoolOptionEnum.Spectator };
            MyStringId[] idArray3 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettings_Enable3rdPersonCamera, MySpaceTexts.ToolTipWorldSettingsEnableSpectator };
            this.AddCheckboxRow(nullableArray3, enumArray3, idArray3);
            MyStringId?[] nullableArray4 = new MyStringId?[] { new MyStringId?(MySpaceTexts.World_Settings_EnableOxygenPressurization), new MyStringId?(MySpaceTexts.World_Settings_EnableOxygen) };
            MySpaceBoolOptionEnum[] enumArray4 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.Airtightness, MySpaceBoolOptionEnum.Oxygen };
            MyStringId[] idArray4 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettings_EnableOxygenPressurization, MySpaceTexts.ToolTipWorldSettings_EnableOxygen };
            this.AddCheckboxRow(nullableArray4, enumArray4, idArray4);
            MyStringId?[] nullableArray5 = new MyStringId?[] { new MyStringId?(MySpaceTexts.ServerDetails_ServerManagement), new MyStringId?(MySpaceTexts.WorldSettings_StationVoxelSupport) };
            MySpaceBoolOptionEnum[] enumArray5 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.ExternalServerManagement, MySpaceBoolOptionEnum.UnsupportedStations };
            MyStringId[] idArray5 = new MyStringId[] { MySpaceTexts.ServerDetails_ServerManagement, MySpaceTexts.ToolTipWorldSettings_StationVoxelSupport };
            this.AddCheckboxRow(nullableArray5, enumArray5, idArray5);
            MyStringId?[] nullableArray6 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_DestructibleBlocks), new MyStringId?(MySpaceTexts.WorldSettings_ThrusterDamage) };
            MySpaceBoolOptionEnum[] enumArray6 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.BlockDestruction, MySpaceBoolOptionEnum.ThrusterDamage };
            MyStringId[] idArray6 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettingsDestructibleBlocks, MySpaceTexts.ToolTipWorldSettingsThrusterDamage };
            this.AddCheckboxRow(nullableArray6, enumArray6, idArray6);
            MyStringId?[] nullableArray7 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_EnableCargoShips), new MyStringId?(MySpaceTexts.WorldSettings_Encounters) };
            MySpaceBoolOptionEnum[] enumArray7 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.CargoShips, MySpaceBoolOptionEnum.Encounters };
            MyStringId[] idArray7 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettingsEnableCargoShips, MySpaceTexts.ToolTipWorldSettings_EnableEncounters };
            this.AddCheckboxRow(nullableArray7, enumArray7, idArray7);
            MyStringId?[] nullableArray8 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_EnableSpiders), new MyStringId?(MySpaceTexts.WorldSettings_EnableRespawnShips) };
            MySpaceBoolOptionEnum[] enumArray8 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.Spiders, MySpaceBoolOptionEnum.RespawnShips };
            MyStringId[] idArray8 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettings_EnableSpiders, MySpaceTexts.ToolTipWorldSettings_EnableRespawnShips };
            this.AddCheckboxRow(nullableArray8, enumArray8, idArray8);
            MyStringId?[] nullableArray9 = new MyStringId?[] { new MyStringId?(MySpaceTexts.WorldSettings_EnableDrones), new MyStringId?(MySpaceTexts.WorldSettings_EnableWolfs) };
            MySpaceBoolOptionEnum[] enumArray9 = new MySpaceBoolOptionEnum[] { MySpaceBoolOptionEnum.Drones, MySpaceBoolOptionEnum.Wolves };
            MyStringId[] idArray9 = new MyStringId[] { MySpaceTexts.ToolTipWorldSettings_EnableDrones, MySpaceTexts.ToolTipWorldSettings_EnableWolfs };
            this.AddCheckboxRow(nullableArray9, enumArray9, idArray9);
        }

        protected override unsafe void DrawMidControls()
        {
            base.DrawMidControls();
            Vector2 currentPosition = base.CurrentPosition;
            base.CurrentPosition.Y = -0.102f;
            float* singlePtr1 = (float*) ref base.CurrentPosition.X;
            singlePtr1[0] += base.Padding / 2.4f;
            Vector2? size = null;
            VRageMath.Vector4? colorMask = null;
            MyGuiControlLabel control = new MyGuiControlLabel(new Vector2?(base.CurrentPosition), size, MyTexts.GetString(MySpaceTexts.WorldSettings_EnvironmentHostility), colorMask, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
            this.Controls.Add(control);
            size = null;
            colorMask = null;
            size = null;
            size = null;
            colorMask = null;
            MyGuiControlCombobox combo = new MyGuiControlCombobox(new Vector2?(base.CurrentPosition), size, colorMask, size, 10, size, false, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, colorMask);
            int? sortOrder = null;
            MyStringId? toolTip = null;
            combo.AddItem(-1L, MyCommonTexts.Any, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            combo.AddItem(0L, MySpaceTexts.WorldSettings_EnvironmentHostilitySafe, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            combo.AddItem(1L, MySpaceTexts.WorldSettings_EnvironmentHostilityNormal, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            combo.AddItem(2L, MySpaceTexts.WorldSettings_EnvironmentHostilityCataclysm, sortOrder, toolTip);
            sortOrder = null;
            toolTip = null;
            combo.AddItem(3L, MySpaceTexts.WorldSettings_EnvironmentHostilityCataclysmUnreal, sortOrder, toolTip);
            combo.Size = new Vector2(0.295f, 1f);
            combo.PositionX += (combo.Size.X / 2f) + (base.Padding * 12.3f);
            combo.SetTooltip(MyTexts.GetString(MySpaceTexts.ToolTipJoinGameServerSearch_EvnironmentHostility));
            MyFilterRange filter = this.SpaceFilters.GetFilter(MySpaceNumericOptionEnum.EnvionmentHostility);
            if (filter.Active)
            {
                combo.SelectItemByKey((long) filter.Value.Max, false);
            }
            else
            {
                combo.SelectItemByKey(-1L, false);
            }
            combo.ItemSelected += delegate {
                MyFilterRange range = this.SpaceFilters.GetFilter(MySpaceNumericOptionEnum.EnvionmentHostility);
                long selectedKey = combo.GetSelectedKey();
                if (selectedKey == -1L)
                {
                    range.Active = false;
                }
                else
                {
                    range.Active = true;
                    SerializableRange range2 = new SerializableRange {
                        Min = selectedKey,
                        Max = selectedKey
                    };
                    range.Value = range2;
                }
            };
            this.Controls.Add(combo);
            base.CurrentPosition.X = currentPosition.X;
            float* singlePtr2 = (float*) ref base.CurrentPosition.Y;
            singlePtr2[0] += 0.04f + base.Padding;
        }

        protected override unsafe void DrawTopControls()
        {
            base.DrawTopControls();
            this.AddNumericRangeOption(MySpaceTexts.MultiplayerJoinProductionMultipliers, MySpaceNumericOptionEnum.ProductionMultipliers);
            this.AddNumericRangeOption(MySpaceTexts.WorldSettings_InventorySize, MySpaceNumericOptionEnum.InventoryMultipier);
            float* singlePtr1 = (float*) ref base.CurrentPosition.Y;
            singlePtr1[0] += base.Padding;
        }

        private MySpaceServerFilterOptions SpaceFilters =>
            (base.FilterOptions as MySpaceServerFilterOptions);
    }
}

