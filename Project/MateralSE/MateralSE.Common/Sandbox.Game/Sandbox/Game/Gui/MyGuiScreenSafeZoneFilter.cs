namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Utils;
    using VRageMath;

    internal class MyGuiScreenSafeZoneFilter : MyGuiScreenDebugBase
    {
        public MyGuiControlListbox m_entityListbox;
        public MyGuiControlListbox m_restrictedListbox;
        private MyGuiControlCombobox m_accessCombobox;
        private MyGuiControlCombobox m_restrictionTypeCombobox;
        private MyGuiControlButton m_playersFilter;
        private MyGuiControlButton m_gridsFilter;
        private MyGuiControlButton m_floatingObjectsFilter;
        private MyGuiControlButton m_factionsFilter;
        private MyGuiControlButton m_moveLeftButton;
        private MyGuiControlButton m_moveLeftAllButton;
        private MyGuiControlButton m_moveRightButton;
        private MyGuiControlButton m_moveRightAllButton;
        private MyGuiControlButton m_closeButton;
        private MyGuiControlButton m_addContainedToSafeButton;
        private MyGuiControlLabel m_modeLabel;
        private MyGuiControlLabel m_controlLabelList;
        private MyGuiControlLabel m_controlLabelEntity;
        public MySafeZone m_selectedSafeZone;
        private MyGuiScreenAdminMenu.MyRestrictedTypeEnum m_selectedFilter;

        public MyGuiScreenSafeZoneFilter(Vector2 position, MySafeZone safeZone) : base(position, new Vector2(0.7f, 0.644084f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR * MySandboxGame.Config.UIBkOpacity), true)
        {
            base.CloseButtonEnabled = true;
            this.m_selectedSafeZone = safeZone;
            this.RecreateControls(true);
        }

        public override string GetFriendlyName() => 
            "MyGuiRenameDialog";

        private void m_accessCombobox_ItemSelected()
        {
        }

        private MyGuiControlButton MakeButtonCategory(Vector2 position, string textureName, string tooltip, Action<MyGuiControlButton> myAction, MyGuiScreenAdminMenu.MyRestrictedTypeEnum myEnum)
        {
            MyGuiHighlightTexture icon = new MyGuiHighlightTexture {
                Normal = $"Textures\GUI\Icons\buttons\small_variant\{textureName}.dds",
                Highlight = $"Textures\GUI\Icons\buttons\small_variant\{textureName}Highlight.dds",
                SizePx = new Vector2(48f, 48f)
            };
            Vector2? size = null;
            MyGuiControlButton control = this.MakeButtonCategoryTiny(position, 0f, tooltip, icon, myAction, size);
            control.UserData = myEnum;
            this.Controls.Add(control);
            control.Size = new Vector2(0.005f, 0.005f);
            return control;
        }

        private MyGuiControlButton MakeButtonCategoryTiny(Vector2 position, float rotation, string toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = new Vector2?())
        {
            Action<MyGuiControlButton> onButtonClick = onClick;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            icon.SizePx = new Vector2(48f, 48f);
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Square48, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, toolTip, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Icon = new MyGuiHighlightTexture?(icon);
            button1.IconRotation = rotation;
            button1.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            return button1;
        }

        private MyGuiControlButton MakeButtonTiny(Vector2 position, float rotation, string toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = new Vector2?())
        {
            Action<MyGuiControlButton> onButtonClick = onClick;
            VRageMath.Vector4? colorMask = null;
            int? buttonIndex = null;
            icon.SizePx = new Vector2(64f, 64f);
            MyGuiControlButton button1 = new MyGuiControlButton(new Vector2?(position), MyGuiControlButtonStyleEnum.Square, size, colorMask, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, toolTip, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, onButtonClick, GuiSounds.MouseClick, 1f, buttonIndex, false);
            button1.Icon = new MyGuiHighlightTexture?(icon);
            button1.IconRotation = rotation;
            button1.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
            return button1;
        }

        private void OnAccessChanged(MySafeZoneAccess access)
        {
            if (this.m_selectedSafeZone != null)
            {
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    this.m_selectedSafeZone.AccessTypePlayers = access;
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    this.m_selectedSafeZone.AccessTypeFactions = access;
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    this.m_selectedSafeZone.AccessTypeGrids = access;
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    this.m_selectedSafeZone.AccessTypeFloatingObjects = access;
                }
                this.RequestUpdateSafeZone();
            }
        }

        private void OnAddAllRestricted(MyGuiControlButton button)
        {
            if (this.m_selectedSafeZone != null)
            {
                ObservableCollection<MyGuiControlListbox.Item>.Enumerator enumerator;
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    using (enumerator = this.m_entityListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Players.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    using (enumerator = this.m_entityListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    using (enumerator = this.m_entityListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyFaction userData = (MyFaction) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Factions.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    using (enumerator = this.m_entityListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                this.RequestUpdateSafeZone();
                this.OnRestrictionChanged(this.m_selectedFilter);
            }
        }

        private void OnAddContainedToSafe(MyGuiControlButton button)
        {
            if (this.m_selectedSafeZone != null)
            {
                this.m_selectedSafeZone.AddContainedToList();
                this.UpdateRestrictedData();
                this.RequestUpdateSafeZone();
                this.OnRestrictionChanged(this.m_selectedFilter);
            }
        }

        private void OnAddRestricted(MyGuiControlButton button)
        {
            if (this.m_selectedSafeZone != null)
            {
                List<MyGuiControlListbox.Item>.Enumerator enumerator;
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    using (enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Players.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    using (enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    using (enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyFaction userData = (MyFaction) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Factions.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    using (enumerator = this.m_entityListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Add(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                this.RequestUpdateSafeZone();
                this.OnRestrictionChanged(this.m_selectedFilter);
            }
        }

        private void OnCancel(MyGuiControlButton button)
        {
            this.CloseScreen();
        }

        private void OnDoubleClickEntityItem(MyGuiControlListbox list)
        {
            this.OnAddRestricted(null);
        }

        private void OnDoubleClickRestrictedItem(MyGuiControlListbox list)
        {
            this.OnRemoveRestricted(null);
        }

        private void OnFilterChange(MyGuiControlButton button)
        {
            this.m_selectedFilter = (MyGuiScreenAdminMenu.MyRestrictedTypeEnum) button.UserData;
            if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
            {
                this.m_accessCombobox.SelectItemByIndex((int) this.m_selectedSafeZone.AccessTypePlayers);
            }
            else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
            {
                this.m_accessCombobox.SelectItemByIndex((int) this.m_selectedSafeZone.AccessTypeFactions);
            }
            else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
            {
                this.m_accessCombobox.SelectItemByIndex((int) this.m_selectedSafeZone.AccessTypeGrids);
            }
            else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
            {
                this.m_accessCombobox.SelectItemByIndex((int) this.m_selectedSafeZone.AccessTypeFloatingObjects);
            }
            this.m_playersFilter.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            this.m_factionsFilter.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            this.m_gridsFilter.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            this.m_floatingObjectsFilter.HighlightType = MyGuiControlHighlightType.WHEN_CURSOR_OVER;
            button.Selected = true;
            button.HighlightType = MyGuiControlHighlightType.FORCED;
            this.m_controlLabelList.Text = MyTexts.GetString(MySpaceTexts.SafeZone_SafeZoneFilter) + " " + button.Tooltips.ToolTips[0].Text;
            this.m_controlLabelEntity.Text = MyTexts.GetString(MySpaceTexts.SafeZone_ListOfEntities) + " " + button.Tooltips.ToolTips[0].Text;
            this.OnRestrictionChanged(this.m_selectedFilter);
        }

        private void OnRemoveAllRestricted(MyGuiControlButton button)
        {
            if (this.m_selectedSafeZone != null)
            {
                ObservableCollection<MyGuiControlListbox.Item>.Enumerator enumerator;
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    using (enumerator = this.m_restrictedListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Players.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    using (enumerator = this.m_restrictedListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    using (enumerator = this.m_restrictedListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyFaction userData = (MyFaction) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Factions.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    using (enumerator = this.m_restrictedListbox.Items.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                this.RequestUpdateSafeZone();
                this.OnRestrictionChanged(this.m_selectedFilter);
            }
        }

        private void OnRemoveRestricted(MyGuiControlButton button)
        {
            if (this.m_selectedSafeZone != null)
            {
                List<MyGuiControlListbox.Item>.Enumerator enumerator;
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    using (enumerator = this.m_restrictedListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Players.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    using (enumerator = this.m_restrictedListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    using (enumerator = this.m_restrictedListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            MyFaction userData = (MyFaction) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Factions.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    using (enumerator = this.m_restrictedListbox.SelectedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            long userData = (long) enumerator.Current.UserData;
                            this.m_selectedSafeZone.Entities.Remove(userData);
                        }
                    }
                    this.UpdateRestrictedData();
                }
                this.RequestUpdateSafeZone();
                this.OnRestrictionChanged(this.m_selectedFilter);
            }
        }

        private void OnRestrictionChanged(MyGuiScreenAdminMenu.MyRestrictedTypeEnum restrictionType)
        {
            this.UpdateRestrictedData();
            if (restrictionType == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
            {
                this.ShowFilteredEntities(MyEntityList.MyEntityTypeEnum.Grids);
            }
            else if (restrictionType == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
            {
                this.ShowFilteredEntities(MyEntityList.MyEntityTypeEnum.FloatingObjects);
            }
            else
            {
                int? nullable;
                if (restrictionType == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    this.m_entityListbox.Items.Clear();
                    foreach (MyPlayer.PlayerId id in MySession.Static.Players.GetAllPlayers())
                    {
                        MyPlayer player = null;
                        if (Sync.Players.TryGetPlayerById(id, out player) && !this.m_selectedSafeZone.Players.Contains(player.Identity.IdentityId))
                        {
                            nullable = null;
                            this.m_entityListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(player.DisplayName), null, null, player.Identity.IdentityId, null), nullable);
                        }
                    }
                }
                else if (restrictionType == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    this.m_entityListbox.Items.Clear();
                    foreach (KeyValuePair<long, IMyFaction> pair in MySession.Static.Factions.Factions)
                    {
                        if (!this.m_selectedSafeZone.Factions.Contains<IMyFaction>(pair.Value))
                        {
                            nullable = null;
                            this.m_entityListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(pair.Value.Name), null, null, pair.Value, null), nullable);
                        }
                    }
                }
            }
        }

        private void OnSelectEntityItem(MyGuiControlListbox list)
        {
            this.m_moveRightButton.Enabled = list.SelectedItems.Count > 0;
        }

        private void OnSelectRestrictedItem(MyGuiControlListbox list)
        {
            this.m_moveLeftButton.Enabled = list.SelectedItems.Count > 0;
        }

        public override void RecreateControls(bool constructor)
        {
            MyGuiControlButton button;
            base.RecreateControls(constructor);
            base.AddCaption(MyTexts.GetString(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_ConfigureFilter), new VRageMath.Vector4?(Color.White.ToVector4()), new Vector2(0f, 0.003f), 0.8f);
            MyGuiControlSeparatorList control = new MyGuiControlSeparatorList();
            VRageMath.Vector4? color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.88f) / 2f, (base.m_size.Value.Y / 2f) - 0.075f), base.m_size.Value.X * 0.88f, 0f, color);
            color = null;
            control.AddHorizontal(new Vector2(0f, 0f) - new Vector2((base.m_size.Value.X * 0.88f) / 2f, (-base.m_size.Value.Y / 2f) + 0.123f), base.m_size.Value.X * 0.88f, 0f, color);
            this.Controls.Add(control);
            this.m_playersFilter = this.MakeButtonCategory(new Vector2(-0.293f, -0.205f), "Character", MyTexts.GetString(MyCommonTexts.JoinGame_ColumnTitle_Players), new Action<MyGuiControlButton>(this.OnFilterChange), MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player);
            this.m_factionsFilter = this.MakeButtonCategory(new Vector2(-0.257f, -0.205f), "Animation", MyTexts.GetString(MyCommonTexts.ScreenPlayers_Factions), new Action<MyGuiControlButton>(this.OnFilterChange), MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction);
            this.m_gridsFilter = this.MakeButtonCategory(new Vector2(-0.221f, -0.205f), "Block", MyTexts.GetString(MySpaceTexts.Grids), new Action<MyGuiControlButton>(this.OnFilterChange), MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid);
            this.m_floatingObjectsFilter = this.MakeButtonCategory(new Vector2(-0.185f, -0.205f), "Modpack", MyTexts.GetString(MySpaceTexts.FloatingObjects), new Action<MyGuiControlButton>(this.OnFilterChange), MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects);
            Vector2 vector = new Vector2(0f, -0.223f);
            Vector2? size = null;
            this.m_moveLeftButton = button = this.MakeButtonTiny(vector + (5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 3.141593f, MyTexts.GetString(MySpaceTexts.Remove), MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnRemoveRestricted), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveLeftAllButton = button = this.MakeButtonTiny(vector + (6f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 3.141593f, MyTexts.GetString(MySpaceTexts.RemoveAll), MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnRemoveAllRestricted), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveRightAllButton = button = this.MakeButtonTiny(vector + (2f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 0f, MyTexts.GetString(MySpaceTexts.AddAll), MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, new Action<MyGuiControlButton>(this.OnAddAllRestricted), size);
            this.Controls.Add(button);
            size = null;
            this.m_moveRightButton = button = this.MakeButtonTiny(vector + (3f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA), 0f, MyTexts.GetString(MySpaceTexts.Add), MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, new Action<MyGuiControlButton>(this.OnAddRestricted), size);
            this.Controls.Add(button);
            this.m_moveLeftButton.Enabled = false;
            this.m_moveRightButton.Enabled = false;
            MyGuiControlLabel label1 = new MyGuiControlLabel();
            label1.Position = base.m_currentPosition + new Vector2(0.022f, -0.214f);
            label1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label1.Text = MyTexts.GetString(MySpaceTexts.SafeZone_Mode);
            this.m_modeLabel = label1;
            this.Controls.Add(this.m_modeLabel);
            color = null;
            this.m_accessCombobox = base.AddCombo<MySafeZoneAccess>(this.m_selectedSafeZone.AccessTypePlayers, new Action<MySafeZoneAccess>(this.OnAccessChanged), true, 4, null, color);
            this.m_accessCombobox.Position = new Vector2(0.308f, -0.224f);
            this.m_accessCombobox.Size = new Vector2((0.287f - this.m_modeLabel.Size.X) - 0.01f, 0.1f);
            this.m_accessCombobox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP;
            this.m_accessCombobox.ItemSelected += new MyGuiControlCombobox.ItemSelectedDelegate(this.m_accessCombobox_ItemSelected);
            MyGuiControlLabel label2 = new MyGuiControlLabel();
            label2.Position = new Vector2(0.03f, -0.173f);
            label2.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label2.Text = MyTexts.GetString(MySpaceTexts.SafeZone_SafeZoneFilter);
            this.m_controlLabelList = label2;
            color = null;
            MyGuiControlPanel panel1 = new MyGuiControlPanel(new Vector2(this.m_controlLabelList.PositionX - 0.0085f, this.m_controlLabelList.Position.Y - 0.005f), new Vector2(0.2865f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel1.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel = panel1;
            this.Controls.Add(panel);
            this.Controls.Add(this.m_controlLabelList);
            MyGuiControlLabel label3 = new MyGuiControlLabel();
            label3.Position = new Vector2(-0.3f, -0.173f);
            label3.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            label3.Text = MyTexts.GetString(MySpaceTexts.SafeZone_ListOfEntities);
            this.m_controlLabelEntity = label3;
            color = null;
            MyGuiControlPanel panel3 = new MyGuiControlPanel(new Vector2(this.m_controlLabelEntity.PositionX - 0.0085f, this.m_controlLabelEntity.Position.Y - 0.005f), new Vector2(0.2865f, 0.035f), color, null, null, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP);
            panel3.BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK_BORDER;
            MyGuiControlPanel panel2 = panel3;
            this.Controls.Add(panel2);
            this.Controls.Add(this.m_controlLabelEntity);
            this.m_restrictedListbox = new MyGuiControlListbox(new Vector2?(Vector2.Zero), MyGuiControlListboxStyleEnum.Blueprints);
            this.m_restrictedListbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_restrictedListbox.Enabled = true;
            this.m_restrictedListbox.VisibleRowsCount = 9;
            this.m_restrictedListbox.Position = (this.m_restrictedListbox.Size / 2f) + base.m_currentPosition;
            this.m_restrictedListbox.MultiSelect = true;
            this.Controls.Add(this.m_restrictedListbox);
            this.m_restrictedListbox.Position = new Vector2(0.022f, -0.145f);
            this.m_restrictedListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnSelectRestrictedItem);
            this.m_restrictedListbox.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnDoubleClickRestrictedItem);
            this.m_entityListbox = new MyGuiControlListbox(new Vector2?(Vector2.Zero), MyGuiControlListboxStyleEnum.Blueprints);
            this.m_entityListbox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_entityListbox.Enabled = true;
            this.m_entityListbox.VisibleRowsCount = 9;
            this.m_entityListbox.Position = (this.m_restrictedListbox.Size / 2f) + base.m_currentPosition;
            this.m_entityListbox.MultiSelect = true;
            this.Controls.Add(this.m_entityListbox);
            this.m_entityListbox.Position = new Vector2(-0.308f, -0.145f);
            this.m_entityListbox.ItemsSelected += new Action<MyGuiControlListbox>(this.OnSelectEntityItem);
            this.m_entityListbox.ItemDoubleClicked += new Action<MyGuiControlListbox>(this.OnDoubleClickEntityItem);
            size = null;
            size = null;
            color = null;
            int? buttonIndex = null;
            this.m_closeButton = new MyGuiControlButton(size, MyGuiControlButtonStyleEnum.Default, size, color, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.Close), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnCancel), GuiSounds.MouseClick, 1f, buttonIndex, false);
            size = null;
            size = null;
            color = null;
            buttonIndex = null;
            this.m_addContainedToSafeButton = new MyGuiControlButton(size, MyGuiControlButtonStyleEnum.Default, size, color, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER, null, MyTexts.Get(MySpaceTexts.ScreenDebugAdminMenu_SafeZones_FilterContained), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_ACTIVE, new Action<MyGuiControlButton>(this.OnAddContainedToSafe), GuiSounds.MouseClick, 1f, buttonIndex, false);
            Vector2 vector2 = new Vector2(0.002f, (base.m_size.Value.Y / 2f) - 0.071f);
            Vector2 vector3 = new Vector2(0.018f, 0f);
            this.m_closeButton.Position = vector2 - vector3;
            this.m_addContainedToSafeButton.Position = vector2 + vector3;
            this.m_addContainedToSafeButton.SetToolTip(MySpaceTexts.ToolTipSafeZone_AddContained);
            this.m_closeButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipNewsletter_Close));
            this.Controls.Add(this.m_closeButton);
            this.Controls.Add(this.m_addContainedToSafeButton);
            this.m_playersFilter.Selected = true;
            this.m_playersFilter.HighlightType = MyGuiControlHighlightType.FORCED;
            this.m_controlLabelList.Text = MyTexts.GetString(MySpaceTexts.SafeZone_SafeZoneFilter) + " " + this.m_playersFilter.Tooltips.ToolTips[0].Text;
            this.m_controlLabelEntity.Text = MyTexts.GetString(MySpaceTexts.SafeZone_ListOfEntities) + " " + this.m_playersFilter.Tooltips.ToolTips[0].Text;
            this.UpdateRestrictedData();
            this.OnRestrictionChanged(this.m_selectedFilter);
        }

        private void RequestUpdateSafeZone()
        {
            if (this.m_selectedSafeZone != null)
            {
                MySessionComponentSafeZones.RequestUpdateSafeZone((MyObjectBuilder_SafeZone) this.m_selectedSafeZone.GetObjectBuilder(false));
            }
        }

        private void ShowFilteredEntities(MyEntityList.MyEntityTypeEnum restrictionType)
        {
            MyGuiScreenAdminMenu firstScreenOfType = MyScreenManager.GetFirstScreenOfType<MyGuiScreenAdminMenu>();
            if (firstScreenOfType != null)
            {
                firstScreenOfType.ValueChanged(restrictionType);
            }
        }

        private void UpdateRestrictedData()
        {
            this.m_restrictedListbox.ClearItems();
            if (this.m_selectedSafeZone != null)
            {
                int? nullable;
                if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Player)
                {
                    foreach (long num in this.m_selectedSafeZone.Players)
                    {
                        MyPlayer.PlayerId id;
                        if (!Sync.Players.TryGetPlayerId(num, out id))
                        {
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(num.ToString()), null, null, num, null), nullable);
                            continue;
                        }
                        MyIdentity identity = Sync.Players.TryGetPlayerIdentity(id);
                        if (identity != null)
                        {
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(identity.DisplayName), null, null, num, null), nullable);
                            continue;
                        }
                        nullable = null;
                        this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(num.ToString()), null, null, num, null), nullable);
                    }
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Grid)
                {
                    foreach (long num2 in this.m_selectedSafeZone.Entities)
                    {
                        MyEntity entity;
                        if (!MyEntities.TryGetEntityById(num2, out entity, false))
                        {
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(num2.ToString()), null, null, num2, null), nullable);
                            continue;
                        }
                        MyCubeGrid grid = entity as MyCubeGrid;
                        if ((grid != null) && (!grid.Closed && (grid.Physics != null)))
                        {
                            string displayName = entity.DisplayName;
                            string str = displayName ?? (entity.Name ?? entity.ToString());
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(str), null, null, num2, null), nullable);
                        }
                    }
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.FloatingObjects)
                {
                    foreach (long num3 in this.m_selectedSafeZone.Entities)
                    {
                        MyEntity entity2;
                        if (!MyEntities.TryGetEntityById(num3, out entity2, false))
                        {
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(num3.ToString()), null, null, num3, null), nullable);
                            continue;
                        }
                        MyFloatingObject obj2 = entity2 as MyFloatingObject;
                        if (obj2 != null)
                        {
                            if (obj2.Closed)
                            {
                                continue;
                            }
                            if (obj2.Physics == null)
                            {
                                continue;
                            }
                            string displayName = entity2.DisplayName;
                            string str2 = displayName ?? (entity2.Name ?? entity2.ToString());
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(str2), null, null, num3, null), nullable);
                        }
                        MyInventoryBagEntity entity3 = entity2 as MyInventoryBagEntity;
                        if (((entity3 != null) && !entity3.Closed) && (entity3.Physics != null))
                        {
                            string displayName = entity2.DisplayName;
                            string str3 = displayName ?? (entity2.Name ?? entity2.ToString());
                            nullable = null;
                            this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(str3), null, null, num3, null), nullable);
                        }
                    }
                }
                else if (this.m_selectedFilter == MyGuiScreenAdminMenu.MyRestrictedTypeEnum.Faction)
                {
                    foreach (MyFaction faction in this.m_selectedSafeZone.Factions)
                    {
                        nullable = null;
                        this.m_restrictedListbox.Add(new MyGuiControlListbox.Item(new StringBuilder(faction.Name), null, null, faction, null), nullable);
                    }
                }
            }
        }
    }
}

