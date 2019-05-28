namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Common.ObjectBuilders;
    using Sandbox.Definitions;
    using Sandbox.Engine.Analytics;
    using Sandbox.Engine.Platform.VideoMode;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.Screens.Terminal.Controls;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics;
    using Sandbox.Graphics.GUI;
    using Sandbox.Graphics.GUI.IME;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.FileSystem;
    using VRage.Game;
    using VRage.Game.Definitions.Animation;
    using VRage.Game.Entity;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    public class MyGuiScreenToolbarConfigBase : MyGuiScreenBase
    {
        public static MyGuiScreenToolbarConfigBase Static;
        protected MyGuiControlSearchBox m_searchBox;
        protected MyGuiControlListbox m_categoriesListbox;
        protected MyGuiControlGrid m_gridBlocks;
        protected MyGuiControlScrollablePanel m_gridBlocksPanel;
        protected MyGuiControlScrollablePanel m_researchPanel;
        protected MyGuiControlResearchGraph m_researchGraph;
        protected MyGuiControlLabel m_blocksLabel;
        protected MyGuiControlGridDragAndDrop m_dragAndDrop;
        protected MyGuiControlToolbar m_toolbarControl;
        protected MyGuiControlContextMenu m_contextMenu;
        protected MyGuiControlContextMenu m_onDropContextMenu;
        protected MyObjectBuilder_ToolbarControlVisualStyle m_toolbarStyle;
        protected MyGuiControlTabControl m_tabControl;
        private MyShipController m_shipController;
        protected MyCharacter m_character;
        protected MyCubeGrid m_screenCubeGrid;
        protected const string SHIP_GROUPS_NAME = "Groups";
        protected const string CHARACTER_ANIMATIONS_GROUP_NAME = "CharacterAnimations";
        protected MyStringHash manipulationToolId;
        protected string[] m_forcedCategoryOrder;
        protected MySearchByStringCondition m_nameSearchCondition;
        protected MySearchByCategoryCondition m_categorySearchCondition;
        protected SortedDictionary<string, MyGuiBlockCategoryDefinition> m_sortedCategories;
        protected static List<MyGuiBlockCategoryDefinition> m_allSelectedCategories = new List<MyGuiBlockCategoryDefinition>();
        protected List<MyGuiBlockCategoryDefinition> m_searchInBlockCategories;
        private HashSet<string> m_tmpUniqueStrings;
        protected MyGuiBlockCategoryDefinition m_shipGroupsCategory;
        protected float m_scrollOffset;
        protected static float m_savedVPosition = 0f;
        protected int m_contextBlockX;
        protected int m_contextBlockY;
        protected int m_onDropContextMenuToolbarIndex;
        protected MyToolbarItem m_onDropContextMenuItem;
        protected bool m_shipMode;
        public static GroupModes GroupMode = GroupModes.Default;
        protected MyCubeBlock m_screenOwner;
        protected static bool m_ownerChanged = false;
        protected static VRage.Game.Entity.MyEntity m_previousOwner = null;
        private int m_framesBeforeSearchEnabled;
        private MyDragAndDropInfo m_dragAndDropInfo;
        private ConditionBase visibleCondition;
        protected MyGuiControlPcuBar m_PCUControl;
        private int m_frameCounterPCU;
        private readonly int PCU_UPDATE_EACH_N_FRAMES;
        private readonly List<int> m_blockOffsets;
        private float m_minVerticalPosition;
        private bool m_researchItemFound;

        public MyGuiScreenToolbarConfigBase(MyObjectBuilder_ToolbarControlVisualStyle toolbarStyle, int scrollOffset = 0, MyCubeBlock owner = null) : base(new Vector2(0.5f, 0.5f), new VRageMath.Vector4?(MyGuiConstants.SCREEN_BACKGROUND_COLOR), nullable, false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
        {
            this.manipulationToolId = MyStringHash.GetOrCompute("ManipulationTool");
            this.m_forcedCategoryOrder = new string[] { "ShipWeapons", "ShipTools", "Weapons", "Tools", "CharacterWeapons", "CharacterTools", "CharacterAnimations", "Groups" };
            this.m_nameSearchCondition = new MySearchByStringCondition();
            this.m_categorySearchCondition = new MySearchByCategoryCondition();
            this.m_sortedCategories = new SortedDictionary<string, MyGuiBlockCategoryDefinition>();
            this.m_searchInBlockCategories = new List<MyGuiBlockCategoryDefinition>();
            this.m_tmpUniqueStrings = new HashSet<string>();
            this.m_shipGroupsCategory = new MyGuiBlockCategoryDefinition();
            this.m_contextBlockX = -1;
            this.m_contextBlockY = -1;
            this.m_onDropContextMenuToolbarIndex = -1;
            this.m_framesBeforeSearchEnabled = 5;
            this.PCU_UPDATE_EACH_N_FRAMES = 1;
            this.m_blockOffsets = new List<int>();
            MySandboxGame.Log.WriteLine("MyGuiScreenCubeBuilder.ctor START");
            Static = this;
            this.m_toolbarStyle = toolbarStyle;
            this.visibleCondition = this.m_toolbarStyle.VisibleCondition;
            this.m_toolbarStyle.VisibleCondition = null;
            this.m_scrollOffset = ((float) scrollOffset) / 6.5f;
            base.m_size = new Vector2(1f, 1f);
            base.m_canShareInput = true;
            base.m_drawEvenWithoutFocus = true;
            base.EnabledBackgroundFade = true;
            this.m_screenOwner = owner;
            base.m_defaultJoystickDpadUse = false;
            base.GetType();
            if (typeof(MyGuiScreenToolbarConfigBase) == base.GetType())
            {
                this.RecreateControls(true);
            }
            this.m_framesBeforeSearchEnabled = 10;
            MySandboxGame.Log.WriteLine("MyGuiScreenCubeBuilder.ctor END");
        }

        private void AddAiCommandDefinitions(IMySearchCondition searchCondition)
        {
            foreach (MyAiCommandDefinition definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyAiCommandDefinition>())
            {
                if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && ((definition.AvailableInSurvival || MySession.Static.CreativeMode) && ((searchCondition == null) || searchCondition.MatchesCondition(definition))))
                {
                    this.AddToolbarItemDefinition<MyObjectBuilder_ToolbarItemAiCommand>(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddAnimationDefinition(MyGuiControlGrid grid, MyDefinitionBase definition)
        {
            MyObjectBuilder_ToolbarItemAnimation data = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemAnimation>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddAnimations(bool shipController, IMySearchCondition searchCondition)
        {
            foreach (MyAnimationDefinition definition in MyDefinitionManager.Static.GetAnimationDefinitions())
            {
                if (definition.Public)
                {
                    if (shipController)
                    {
                        if (!shipController)
                        {
                            continue;
                        }
                        if (!definition.AllowInCockpit)
                        {
                            continue;
                        }
                    }
                    if ((searchCondition == null) || searchCondition.MatchesCondition(definition))
                    {
                        this.AddAnimationDefinition(this.m_gridBlocks, definition);
                    }
                }
            }
        }

        private void AddAreaMarkerDefinitions(IMySearchCondition searchCondition)
        {
            foreach (MyAreaMarkerDefinition definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyAreaMarkerDefinition>())
            {
                if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && ((definition.AvailableInSurvival || MySession.Static.CreativeMode) && ((searchCondition == null) || searchCondition.MatchesCondition(definition))))
                {
                    this.AddToolbarItemDefinition<MyObjectBuilder_ToolbarItemAreaMarker>(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddBotDefinition(MyGuiControlGrid grid, MyBotDefinition definition)
        {
            MyObjectBuilder_ToolbarItemBot data = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemBot>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddBotDefinitions(IMySearchCondition searchCondition)
        {
            foreach (MyBotDefinition definition in MyDefinitionManager.Static.GetDefinitionsOfType<MyBotDefinition>())
            {
                if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && ((definition.AvailableInSurvival || MySession.Static.CreativeMode) && ((searchCondition == null) || searchCondition.MatchesCondition(definition))))
                {
                    this.AddBotDefinition(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddCategoryToDisplayList(string displayName, MyGuiBlockCategoryDefinition categoryID)
        {
            MyGuiControlListbox.Item item = new MyGuiControlListbox.Item(new StringBuilder(displayName), displayName, null, categoryID, null);
            int? position = null;
            this.m_categoriesListbox.Add(item, position);
        }

        private void AddCubeDefinition(MyGuiControlGrid grid, MyCubeBlockDefinitionGroup group, Vector2I position)
        {
            MyCubeBlockDefinition definition = MyFakes.ENABLE_NON_PUBLIC_BLOCKS ? group.Any : group.AnyPublic;
            if ((MyFakes.ENABLE_MULTIBLOCK_CONSTRUCTION || !MySession.Static.SurvivalMode) || (definition.MultiBlock == null))
            {
                int num1;
                string subicon = null;
                if ((definition.BlockStages != null) && (definition.BlockStages.Length != 0))
                {
                    subicon = MyGuiTextures.Static.GetTexture(MyHud.HudDefinition.Toolbar.ItemStyle.VariantTexture).Path;
                }
                string icon = null;
                if ((definition.DLCs != null) && (definition.DLCs.Length != 0))
                {
                    MyDLCs.MyDLC firstMissingDefinitionDLC = MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId);
                    if (firstMissingDefinitionDLC != null)
                    {
                        icon = firstMissingDefinitionDLC.Icon;
                    }
                    else
                    {
                        MyDLCs.MyDLC ydlc2;
                        if (MyDLCs.TryGetDLC(definition.DLCs[0], out ydlc2))
                        {
                            icon = ydlc2.Icon;
                        }
                    }
                }
                if (MyToolbarComponent.GlobalBuilding || (MySession.Static.ControlledEntity is MyCharacter))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = !(MySession.Static.ControlledEntity is MyCockpit) ? 0 : ((int) (MySession.Static.ControlledEntity as MyCockpit).BuildingMode);
                }
                bool enabled = (true & num1) & MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(definition, Sync.MyId);
                this.AddDefinitionAtPosition(grid, definition, position, enabled, subicon, icon);
            }
        }

        private unsafe void AddCubeDefinitionsToBlocks(IMySearchCondition searchCondition)
        {
            using (Dictionary<string, MyCubeBlockDefinitionGroup>.KeyCollection.Enumerator enumerator = MyDefinitionManager.Static.GetDefinitionPairNames().GetEnumerator())
            {
                MyCubeBlockDefinitionGroup definitionGroup;
                bool flag;
                int num;
                MyCubeBlockDefinition definition;
                goto TR_005F;
            TR_0034:
                if (flag)
                {
                    searchCondition.AddDefinitionGroup(definitionGroup);
                }
                goto TR_005F;
            TR_0036:
                num++;
                goto TR_004E;
            TR_0041:
                if ((definition.BlockStages != null) && (definition.BlockStages.Length != 0))
                {
                    for (int i = 0; (i < definition.BlockStages.Count<MyDefinitionId>()) && !flag; i++)
                    {
                        MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(definition.BlockStages[i]);
                        if ((cubeBlockDefinition != null) && searchCondition.MatchesCondition(cubeBlockDefinition))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                goto TR_0036;
            TR_004E:
                while (true)
                {
                    if ((num < definitionGroup.SizeCount) && !flag)
                    {
                        definition = definitionGroup[(MyCubeSize) ((byte) num)];
                        if ((MyFakes.ENABLE_NON_PUBLIC_BLOCKS || (((definition != null) && definition.Public) && definition.Enabled)) && (definition != null))
                        {
                            if (!searchCondition.MatchesCondition(definition) || ((MyFakes.ENABLE_GUI_HIDDEN_CUBEBLOCKS && !definition.GuiVisible) && !(searchCondition is MySearchByStringCondition)))
                            {
                                break;
                            }
                            flag = true;
                        }
                        else
                        {
                            goto TR_0036;
                        }
                    }
                    goto TR_0034;
                }
                goto TR_0041;
            TR_005F:
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        if ((!MyFakes.ENABLE_MULTIBLOCKS_IN_SURVIVAL && MySession.Static.SurvivalMode) && current.EndsWith("MultiBlock"))
                        {
                            continue;
                        }
                        definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(current);
                        Vector2I cubeBlockScreenPosition = MyDefinitionManager.Static.GetCubeBlockScreenPosition(current);
                        if (this.IsValidItem(definitionGroup))
                        {
                            if (searchCondition != null)
                            {
                                flag = false;
                                num = 0;
                                break;
                            }
                            if ((!MyFakes.ENABLE_GUI_HIDDEN_CUBEBLOCKS || definitionGroup.Any.GuiVisible) && (definitionGroup.AnyPublic != null))
                            {
                                if ((cubeBlockScreenPosition.Y > 0) && (cubeBlockScreenPosition.Y < this.m_blockOffsets.Count))
                                {
                                    int* numPtr1 = (int*) ref cubeBlockScreenPosition.Y;
                                    numPtr1[0] += this.m_blockOffsets[cubeBlockScreenPosition.Y - 1];
                                    int* numPtr2 = (int*) ref cubeBlockScreenPosition.Y;
                                    numPtr2[0] += cubeBlockScreenPosition.X / this.m_gridBlocks.ColumnsCount;
                                    int* numPtr3 = (int*) ref cubeBlockScreenPosition.X;
                                    numPtr3[0] = numPtr3[0] % this.m_gridBlocks.ColumnsCount;
                                }
                                this.AddCubeDefinition(this.m_gridBlocks, definitionGroup, cubeBlockScreenPosition);
                            }
                        }
                        continue;
                    }
                    goto TR_0031;
                }
                goto TR_004E;
            }
        TR_0031:
            if (searchCondition != null)
            {
                int num3 = 0;
                Vector2I vectori1 = new Vector2I(-1, -1);
                foreach (MyCubeBlockDefinitionGroup group2 in searchCondition.GetSortedBlocks())
                {
                    Vector2I vectori2;
                    vectori2.X = num3 % this.m_gridBlocks.ColumnsCount;
                    vectori2.Y = (int) (((float) num3) / ((float) this.m_gridBlocks.ColumnsCount));
                    num3++;
                    this.AddCubeDefinition(this.m_gridBlocks, group2, vectori2);
                    MyCubeBlockDefinition definition = MyFakes.ENABLE_NON_PUBLIC_BLOCKS ? group2.Any : group2.AnyPublic;
                    if ((definition.BlockStages != null) && ((definition.BlockStages.Length != 0) && (searchCondition is MySearchByCategoryCondition)))
                    {
                        string icon = null;
                        if ((definition.DLCs != null) && (definition.DLCs.Length != 0))
                        {
                            MyDLCs.MyDLC firstMissingDefinitionDLC = MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId);
                            if (firstMissingDefinitionDLC != null)
                            {
                                icon = firstMissingDefinitionDLC.Icon;
                            }
                            else
                            {
                                MyDLCs.MyDLC ydlc2;
                                if (MyDLCs.TryGetDLC(definition.DLCs[0], out ydlc2))
                                {
                                    icon = ydlc2.Icon;
                                }
                            }
                        }
                        for (int i = 0; i < definition.BlockStages.Length; i++)
                        {
                            vectori2.X = num3 % this.m_gridBlocks.ColumnsCount;
                            vectori2.Y = (int) (((float) num3) / ((float) this.m_gridBlocks.ColumnsCount));
                            num3++;
                            MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(definition.BlockStages[i]);
                            if (cubeBlockDefinition != null)
                            {
                                this.AddDefinitionAtPosition(this.m_gridBlocks, cubeBlockDefinition, vectori2, MySession.Static.GetComponent<MySessionComponentDLC>().HasDefinitionDLC(cubeBlockDefinition, Sync.MyId), null, icon);
                            }
                        }
                    }
                }
            }
            else
            {
                int num5 = 0;
                int colIdx = 0x7fffffff;
                int rowIdx = 0;
                while (true)
                {
                    if (rowIdx >= this.m_gridBlocks.RowsCount)
                    {
                        if (num5 > 0)
                        {
                            int count = num5 * this.m_gridBlocks.ColumnsCount;
                            this.m_gridBlocks.Items.RemoveRange(this.m_gridBlocks.Items.Count - count, count);
                            this.m_gridBlocks.RowsCount -= num5;
                        }
                        break;
                    }
                    int num8 = 0;
                    while (true)
                    {
                        if (num8 >= this.m_gridBlocks.ColumnsCount)
                        {
                            if (colIdx == 0)
                            {
                                num5++;
                            }
                            else if (num5 > 0)
                            {
                                for (int i = 0; i < this.m_gridBlocks.ColumnsCount; i++)
                                {
                                    MyGuiGridItem item2 = this.m_gridBlocks.TryGetItemAt(rowIdx, i);
                                    this.m_gridBlocks.SetItemAt(rowIdx, i, null);
                                    this.m_gridBlocks.SetItemAt(rowIdx - num5, i, item2);
                                }
                            }
                            colIdx = 0x7fffffff;
                            rowIdx++;
                            break;
                        }
                        MyGuiGridItem item = this.m_gridBlocks.TryGetItemAt(rowIdx, num8);
                        if ((item != null) && (colIdx != 0x7fffffff))
                        {
                            this.m_gridBlocks.SetItemAt(rowIdx, num8, null);
                            this.m_gridBlocks.SetItemAt(rowIdx, colIdx, item);
                            colIdx++;
                        }
                        else if ((item == null) && (colIdx > num8))
                        {
                            colIdx = num8;
                        }
                        num8++;
                    }
                }
            }
        }

        protected static void AddDefinition(MyGuiControlGrid grid, MyObjectBuilder_ToolbarItem data, MyDefinitionBase definition)
        {
            if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && (definition.AvailableInSurvival || !MySession.Static.SurvivalMode))
            {
                GridItemUserData userData = new GridItemUserData();
                userData.ItemData = data;
                MyGuiGridItem item = new MyGuiGridItem(definition.Icons, null, definition.DisplayNameText, userData, true);
                grid.Add(item, 0);
            }
        }

        protected virtual void AddDefinitionAtPosition(MyGuiControlGrid grid, MyDefinitionBase definition, Vector2I position, bool enabled = true, string subicon = null, string subIcon2 = null)
        {
            if (((definition != null) && (definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS)) && (definition.AvailableInSurvival || !MySession.Static.SurvivalMode))
            {
                int num1;
                if (!MySession.Static.ResearchEnabled || MySession.Static.CreativeToolsEnabled(Sync.MyId))
                {
                    num1 = 1;
                }
                else
                {
                    num1 = (int) MySessionComponentResearch.Static.CanUse(this.m_character ?? this.m_shipController?.Pilot, definition.Id);
                }
                bool researched = (bool) num1;
                enabled &= researched;
                string definitionTooltip = this.GetDefinitionTooltip(definition, researched);
                GridItemUserData userData = new GridItemUserData();
                userData.ItemData = MyToolbarItemFactory.ObjectBuilderFromDefinition(definition);
                MyGuiGridItem item = new MyGuiGridItem(definition.Icons, subicon, definitionTooltip, userData, enabled) {
                    SubIcon2 = subIcon2
                };
                int num = -position.Y - 1;
                if (position.Y < 0)
                {
                    grid.Add(item, num * 6);
                }
                else if (grid.IsValidIndex(position.Y, position.X))
                {
                    this.SetOrReplaceItemOnPosition(grid, item, position);
                }
                else if (grid.IsValidIndex(0, position.X))
                {
                    grid.RecalculateRowsCount();
                    grid.AddRows((position.Y - grid.RowsCount) + 1);
                    this.SetOrReplaceItemOnPosition(grid, item, position);
                }
            }
        }

        private void AddGridCreatorDefinition(MyGuiControlGrid grid, MyDefinitionBase definition)
        {
            MyObjectBuilder_ToolbarItemCreateGrid data = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemCreateGrid>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddGridCreators(IMySearchCondition searchCondition)
        {
            foreach (MyGridCreateToolDefinition definition in MyDefinitionManager.Static.GetGridCreatorDefinitions())
            {
                if (!definition.Public)
                {
                    continue;
                }
                if ((searchCondition == null) || searchCondition.MatchesCondition(definition))
                {
                    this.AddGridCreatorDefinition(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddGridGun(MyShipController shipController, MyDefinitionId gunId, IMySearchCondition searchCondition)
        {
            MyDefinitionBase definition = MyDefinitionManager.Static.GetDefinition(gunId);
            if ((searchCondition == null) || searchCondition.MatchesCondition(definition))
            {
                this.AddWeaponDefinition(this.m_gridBlocks, definition, true);
            }
        }

        private void AddGridItemToToolbar(MyObjectBuilder_ToolbarItem data)
        {
            MyToolbar currentToolbar = MyToolbarComponent.CurrentToolbar;
            int slotCount = currentToolbar.SlotCount;
            MyToolbarItem newItem = MyToolbarItemFactory.CreateToolbarItem(data);
            if (newItem != null)
            {
                RequestItemParameters(newItem, delegate (bool success) {
                    bool flag = false;
                    int num = 0;
                    int slot = 0;
                    while (true)
                    {
                        if (slot < slotCount)
                        {
                            MyToolbarItem slotItem = currentToolbar.GetSlotItem(slot);
                            if ((slotItem == null) || !slotItem.Equals(newItem))
                            {
                                slot++;
                                continue;
                            }
                            if (slotItem.WantsToBeActivated)
                            {
                                currentToolbar.ActivateItemAtSlot(slot, false, false, false);
                            }
                            num = slot;
                            flag = true;
                        }
                        for (int i = 0; i < slotCount; i++)
                        {
                            if (!(!flag && ReferenceEquals(currentToolbar.GetSlotItem(i), null)))
                            {
                                if (((i != num) && (currentToolbar.GetSlotItem(i) != null)) && currentToolbar.GetSlotItem(i).Equals(newItem))
                                {
                                    currentToolbar.SetItemAtSlot(i, null);
                                }
                            }
                            else
                            {
                                currentToolbar.SetItemAtSlot(i, newItem);
                                if (newItem.WantsToBeActivated)
                                {
                                    currentToolbar.ActivateItemAtSlot(i, false, false, false);
                                }
                                num = i;
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            int local1;
                            if (currentToolbar.SelectedSlot == null)
                            {
                                local1 = 0;
                            }
                            else
                            {
                                local1 = currentToolbar.SelectedSlot.Value;
                            }
                            int num4 = local1;
                            currentToolbar.SetItemAtSlot(num4, newItem);
                            if (newItem.WantsToBeActivated)
                            {
                                currentToolbar.ActivateItemAtSlot(num4, false, false, false);
                            }
                            flag = true;
                        }
                        return;
                    }
                });
            }
        }

        private void AddPrefabThrowerDefinition(MyGuiControlGrid grid, MyPrefabThrowerDefinition definition)
        {
            MyObjectBuilder_ToolbarItemPrefabThrower data = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemPrefabThrower>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddPrefabThrowers(IMySearchCondition searchCondition)
        {
            foreach (MyPrefabThrowerDefinition definition in MyDefinitionManager.Static.GetPrefabThrowerDefinitions())
            {
                if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && ((searchCondition == null) || searchCondition.MatchesCondition(definition)))
                {
                    this.AddPrefabThrowerDefinition(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddShipBlocksDefinitions(MyCubeGrid grid, bool isShip, IMySearchCondition searchCondition)
        {
            int num1;
            if (!isShip || (this.m_shipController == null))
            {
                num1 = 0;
            }
            else
            {
                num1 = (int) !this.m_shipController.EnableShipControl;
            }
            if ((num1 == 0) && MyFakes.ENABLE_SHIP_BLOCKS_TOOLBAR)
            {
                this.AddTerminalSingleBlocksToGridBlocks(grid, searchCondition);
            }
        }

        private void AddShipGroupsIntoCategoryList(MyCubeGrid grid)
        {
            if (((grid != null) && (grid.GridSystems.TerminalSystem != null)) && (grid.GridSystems.TerminalSystem.BlockGroups != null))
            {
                MyBlockGroup[] array = grid.GridSystems.TerminalSystem.BlockGroups.ToArray();
                Array.Sort<MyBlockGroup>(array, MyTerminalComparer.Static);
                List<string> collection = new List<string>();
                foreach (MyBlockGroup group in array)
                {
                    if (group != null)
                    {
                        collection.Add(group.Name.ToString());
                    }
                }
                if (collection.Count > 0)
                {
                    this.m_shipGroupsCategory.DisplayNameString = MyTexts.GetString(MySpaceTexts.DisplayName_Category_ShipGroups);
                    this.m_shipGroupsCategory.ItemIds = new HashSet<string>(collection);
                    this.m_shipGroupsCategory.SearchBlocks = false;
                    this.m_shipGroupsCategory.Name = "Groups";
                    this.m_sortedCategories.Add(this.m_shipGroupsCategory.Name, this.m_shipGroupsCategory);
                }
            }
        }

        private void AddShipGunsToCategories(Dictionary<string, MyGuiBlockCategoryDefinition> loadedCategories, SortedDictionary<string, MyGuiBlockCategoryDefinition> categories)
        {
            if (this.m_shipController != null)
            {
                foreach (KeyValuePair<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>> pair in this.m_shipController.CubeGrid.GridSystems.WeaponSystem.GetGunSets())
                {
                    MyDefinitionBase base2 = MyDefinitionManager.Static.GetDefinition(pair.Key);
                    foreach (KeyValuePair<string, MyGuiBlockCategoryDefinition> pair2 in loadedCategories)
                    {
                        if (!pair2.Value.IsShipCategory)
                        {
                            continue;
                        }
                        if (pair2.Value.HasItem(base2.Id.ToString()))
                        {
                            MyGuiBlockCategoryDefinition definition = null;
                            if (!categories.TryGetValue(pair2.Value.Name, out definition))
                            {
                                categories.Add(pair2.Value.Name, pair2.Value);
                            }
                        }
                    }
                }
            }
        }

        private void AddTerminalGroupsToGridBlocks(MyCubeGrid grid, long Owner, IMySearchCondition searchCondition)
        {
            if (((grid != null) && (grid.GridSystems.TerminalSystem != null)) && (grid.GridSystems.TerminalSystem.BlockGroups != null))
            {
                int index = 0;
                int columnsCount = this.m_gridBlocks.ColumnsCount;
                MyBlockGroup[] array = grid.GridSystems.TerminalSystem.BlockGroups.ToArray();
                Array.Sort<MyBlockGroup>(array, MyTerminalComparer.Static);
                foreach (MyBlockGroup group in array)
                {
                    if ((searchCondition == null) || searchCondition.MatchesCondition(group.Name.ToString()))
                    {
                        MyObjectBuilder_ToolbarItemTerminalGroup group2 = MyToolbarItemFactory.TerminalGroupObjectBuilderFromGroup(group);
                        bool enabled = false;
                        using (HashSet<MyTerminalBlock>.Enumerator enumerator = group.Blocks.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.IsFunctional)
                                {
                                    enabled = true;
                                    break;
                                }
                            }
                        }
                        group2.BlockEntityId = Owner;
                        GridItemUserData userData = new GridItemUserData();
                        userData.ItemData = group2;
                        this.m_gridBlocks.Add(new MyGuiGridItem(MyToolbarItemFactory.GetIconForTerminalGroup(group), null, group.Name.ToString(), userData, enabled), 0);
                        index++;
                    }
                }
                if (index > 0)
                {
                    int num4 = index % columnsCount;
                    if (num4 == 0)
                    {
                        num4 = columnsCount;
                    }
                    for (int i = 0; i < ((2 * columnsCount) - num4); i++)
                    {
                        if (index >= this.m_gridBlocks.GetItemsCount())
                        {
                            GridItemUserData userData = new GridItemUserData();
                            userData.ItemData = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemEmpty>();
                            this.m_gridBlocks.Add(new MyGuiGridItem("", null, string.Empty, userData, false), 0);
                        }
                        else
                        {
                            index++;
                            GridItemUserData userData = new GridItemUserData();
                            userData.ItemData = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemEmpty>();
                            this.m_gridBlocks.SetItemAt(index, new MyGuiGridItem("", null, string.Empty, userData, false));
                        }
                    }
                }
            }
        }

        private void AddTerminalSingleBlocksToGridBlocks(MyCubeGrid grid, IMySearchCondition searchCondition)
        {
            if ((grid != null) && (grid.GridSystems.TerminalSystem != null))
            {
                MyTerminalBlock[] array = grid.GridSystems.TerminalSystem.Blocks.ToArray();
                Array.Sort<MyTerminalBlock>(array, MyTerminalComparer.Static);
                foreach (MyTerminalBlock block in array)
                {
                    if (((block != null) && ((MyTerminalControlFactory.GetActions(block.GetType()).Count > 0) && ((((searchCondition == null) || searchCondition.MatchesCondition(block.BlockDefinition)) || searchCondition.MatchesCondition(block.CustomName.ToString())) && block.ShowInToolbarConfig))) && (block.BlockDefinition.AvailableInSurvival || !MySession.Static.SurvivalMode))
                    {
                        MyObjectBuilder_ToolbarItemTerminalBlock block2 = MyToolbarItemFactory.TerminalBlockObjectBuilderFromBlock(block);
                        GridItemUserData userData = new GridItemUserData();
                        userData.ItemData = block2;
                        this.m_gridBlocks.Add(new MyGuiGridItem(block.BlockDefinition.Icons, MyTerminalActionIcons.NONE, block.CustomName.ToString(), userData, block.IsFunctional), 0);
                    }
                }
            }
        }

        private void AddToolbarItemDefinition<T>(MyGuiControlGrid grid, MyDefinitionBase definition) where T: MyObjectBuilder_ToolbarItemDefinition, new()
        {
            T data = MyObjectBuilderSerializer.CreateNewObject<T>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddTools(MyShipController shipController, IMySearchCondition searchCondition)
        {
            foreach (KeyValuePair<MyDefinitionId, HashSet<IMyGunObject<MyDeviceBase>>> pair in shipController.CubeGrid.GridSystems.WeaponSystem.GetGunSets())
            {
                this.AddGridGun(shipController, pair.Key, searchCondition);
            }
        }

        protected virtual void AddToolsAndAnimations(IMySearchCondition searchCondition)
        {
            if (this.m_character == null)
            {
                if (this.m_screenOwner != null)
                {
                    long entityId = this.m_screenOwner.EntityId;
                    this.AddTerminalGroupsToGridBlocks(this.m_screenCubeGrid, entityId, searchCondition);
                    if (this.m_shipController != null)
                    {
                        if (this.m_shipController.EnableShipControl)
                        {
                            this.AddTools(this.m_shipController, searchCondition);
                        }
                        this.AddAnimations(true, searchCondition);
                    }
                }
            }
            else
            {
                MyCharacter thisEntity = this.m_character;
                foreach (MyDefinitionBase base2 in MyDefinitionManager.Static.GetWeaponDefinitions())
                {
                    if (((searchCondition == null) || searchCondition.MatchesCondition(base2)) && base2.Public)
                    {
                        MyInventory inventory = thisEntity.GetInventory(0);
                        bool enabled = ((base2.Id.SubtypeId == this.manipulationToolId) || ((inventory != null) && inventory.ContainItems(1, base2.Id, MyItemFlags.None))) | MySession.Static.CreativeMode;
                        if (enabled || (MyPerGameSettings.Game == GameEnum.SE_GAME))
                        {
                            this.AddWeaponDefinition(this.m_gridBlocks, base2, enabled);
                        }
                    }
                }
                if (MyPerGameSettings.EnableAi && MyFakes.ENABLE_BARBARIANS)
                {
                    this.AddAiCommandDefinitions(searchCondition);
                    this.AddBotDefinitions(searchCondition);
                    this.AddAreaMarkerDefinitions(searchCondition);
                }
                if (MySession.Static.GetVoxelHandAvailable(thisEntity))
                {
                    this.AddVoxelHands(searchCondition);
                }
                if (MyFakes.ENABLE_PREFAB_THROWER)
                {
                    this.AddPrefabThrowers(searchCondition);
                }
                this.AddAnimations(false, searchCondition);
                this.AddGridCreators(searchCondition);
            }
        }

        private void AddVoxelHandDefinition(MyGuiControlGrid grid, MyDefinitionBase definition)
        {
            MyObjectBuilder_ToolbarItemVoxelHand data = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemVoxelHand>();
            data.DefinitionId = (SerializableDefinitionId) definition.Id;
            AddDefinition(grid, data, definition);
        }

        private void AddVoxelHands(IMySearchCondition searchCondition)
        {
            foreach (MyVoxelHandDefinition definition in MyDefinitionManager.Static.GetVoxelHandDefinitions())
            {
                if (!definition.Public)
                {
                    continue;
                }
                if ((searchCondition == null) || searchCondition.MatchesCondition(definition))
                {
                    this.AddVoxelHandDefinition(this.m_gridBlocks, definition);
                }
            }
        }

        private void AddWeaponDefinition(MyGuiControlGrid grid, MyDefinitionBase definition, bool enabled = true)
        {
            if ((definition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && (definition.AvailableInSurvival || !MySession.Static.SurvivalMode))
            {
                MyObjectBuilder_ToolbarItemWeapon weapon = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_ToolbarItemWeapon>();
                weapon.DefinitionId = (SerializableDefinitionId) definition.Id;
                GridItemUserData userData = new GridItemUserData();
                userData.ItemData = weapon;
                MyGuiGridItem item = new MyGuiGridItem(definition.Icons, null, definition.DisplayNameText, userData, enabled);
                grid.Add(item, 0);
            }
        }

        public virtual bool AllowToolbarKeys() => 
            !this.m_searchBox.TextBox.HasFocus;

        private void CalculateBlockOffsets()
        {
            this.m_blockOffsets.Clear();
            foreach (string str in MyDefinitionManager.Static.GetDefinitionPairNames())
            {
                if ((MyFakes.ENABLE_MULTIBLOCKS_IN_SURVIVAL || !MySession.Static.SurvivalMode) || !str.EndsWith("MultiBlock"))
                {
                    MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(str);
                    Vector2I cubeBlockScreenPosition = MyDefinitionManager.Static.GetCubeBlockScreenPosition(str);
                    if ((this.IsValidItem(definitionGroup) && (!MyFakes.ENABLE_GUI_HIDDEN_CUBEBLOCKS || definitionGroup.Any.GuiVisible)) && (definitionGroup.AnyPublic != null))
                    {
                        if (this.m_blockOffsets.Count <= cubeBlockScreenPosition.Y)
                        {
                            for (int j = this.m_blockOffsets.Count - 1; j < cubeBlockScreenPosition.Y; j++)
                            {
                                this.m_blockOffsets.Add(0);
                            }
                        }
                        if (cubeBlockScreenPosition.Y >= 0)
                        {
                            int y = cubeBlockScreenPosition.Y;
                            this.m_blockOffsets[y] += 1;
                        }
                    }
                }
            }
            int num = 0;
            for (int i = 0; i < this.m_blockOffsets.Count; i++)
            {
                int num6 = (this.m_blockOffsets[i] - 1) / this.m_gridBlocks.ColumnsCount;
                num += num6;
                this.m_blockOffsets[i] = num;
            }
        }

        private bool CanDropItem(MyPhysicalInventoryItem item, MyGuiControlGrid dropFrom, MyGuiControlGrid dropTo) => 
            !ReferenceEquals(dropTo, dropFrom);

        private void categories_ItemClicked(MyGuiControlListbox sender)
        {
            this.m_gridBlocks.SetItemsToDefault();
            if (sender.SelectedItems.Count != 0)
            {
                object categorySearchCondition;
                MySearchByStringCondition condition1;
                m_allSelectedCategories.Clear();
                this.m_searchInBlockCategories.Clear();
                bool flag = false;
                using (List<MyGuiControlListbox.Item>.Enumerator enumerator = sender.SelectedItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyGuiBlockCategoryDefinition userData = (MyGuiBlockCategoryDefinition) enumerator.Current.UserData;
                        if (userData == null)
                        {
                            flag = true;
                            continue;
                        }
                        if (userData.SearchBlocks)
                        {
                            this.m_searchInBlockCategories.Add(userData);
                        }
                        m_allSelectedCategories.Add(userData);
                    }
                }
                this.m_categorySearchCondition.SelectedCategories = m_allSelectedCategories;
                this.AddToolsAndAnimations(this.m_categorySearchCondition);
                this.m_categorySearchCondition.SelectedCategories = this.m_searchInBlockCategories;
                if (!flag)
                {
                    categorySearchCondition = this.m_categorySearchCondition;
                }
                else if (!this.m_nameSearchCondition.IsValid)
                {
                    categorySearchCondition = null;
                }
                else
                {
                    categorySearchCondition = this.m_nameSearchCondition;
                }
                IMySearchCondition searchCondition = condition1;
                this.UpdateGridBlocksBySearchCondition(searchCondition);
                this.SearchResearch(searchCondition);
            }
        }

        public override bool CloseScreen()
        {
            m_savedVPosition = this.m_gridBlocksPanel.ScrollbarVPosition;
            Static = null;
            MyAnalyticsHelper.ReportActivityEnd(null, "show_toolbar_config");
            return base.CloseScreen();
        }

        private void contextMenu_ItemClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs args)
        {
            if (((this.m_contextBlockX >= 0) && ((this.m_contextBlockX < this.m_gridBlocks.RowsCount) && (this.m_contextBlockY >= 0))) && (this.m_contextBlockY < this.m_gridBlocks.ColumnsCount))
            {
                MyGuiGridItem item = this.m_gridBlocks.TryGetItemAt(this.m_contextBlockX, this.m_contextBlockY);
                if (item != null)
                {
                    MyObjectBuilder_ToolbarItemTerminal itemData = (MyObjectBuilder_ToolbarItemTerminal) (item.UserData as GridItemUserData).ItemData;
                    itemData._Action = (string) args.UserData;
                    this.AddGridItemToToolbar(itemData);
                    itemData._Action = null;
                }
            }
        }

        private static void CreateGraph(Dictionary<string, MyGuiControlResearchGraph.GraphNode> nodesByName, List<MyGuiControlResearchGraph.GraphNode> children)
        {
            int num = 0;
            while (true)
            {
                if (children.Count != num)
                {
                    bool flag = false;
                    foreach (MyGuiControlResearchGraph.GraphNode node in children)
                    {
                        if (string.IsNullOrEmpty(node.UnlockedBy))
                        {
                            continue;
                        }
                        MyGuiControlResearchGraph.GraphNode node2 = null;
                        if (nodesByName.TryGetValue(node.UnlockedBy, out node2))
                        {
                            num++;
                            node2.Children.Add(node);
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                }
                return;
            }
        }

        private MyGuiControlResearchGraph.GraphNode CreateNode(MyResearchGroupDefinition group, List<MyCubeBlockDefinition> items, string unlockedBy = null)
        {
            MyGuiControlResearchGraph.GraphNode node = new MyGuiControlResearchGraph.GraphNode {
                Name = group.Id.SubtypeName,
                UnlockedBy = unlockedBy
            };
            foreach (MyCubeBlockDefinition definition in items)
            {
                this.CreateNodeItem(node, definition);
            }
            return node;
        }

        private void CreateNodeItem(MyGuiControlResearchGraph.GraphNode node, MyCubeBlockDefinition definition)
        {
            int num1;
            int num2;
            if (!MySession.Static.ResearchEnabled || MySession.Static.CreativeToolsEnabled(Sync.MyId))
            {
                num1 = 1;
            }
            else
            {
                num1 = (int) MySessionComponentResearch.Static.CanUse(this.m_character ?? this.m_shipController?.Pilot, definition.Id);
            }
            bool researched = (bool) num1;
            string subicon = null;
            if ((definition.BlockStages != null) && (definition.BlockStages.Length != 0))
            {
                subicon = MyGuiTextures.Static.GetTexture(MyHud.HudDefinition.Toolbar.ItemStyle.VariantTexture).Path;
            }
            string icon = null;
            bool flag2 = true;
            if ((definition.DLCs != null) && (definition.DLCs.Length != 0))
            {
                MyDLCs.MyDLC firstMissingDefinitionDLC = MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId);
                if (firstMissingDefinitionDLC != null)
                {
                    icon = firstMissingDefinitionDLC.Icon;
                    flag2 = false;
                }
                else
                {
                    MyDLCs.MyDLC ydlc2;
                    if (MyDLCs.TryGetDLC(definition.DLCs[0], out ydlc2))
                    {
                        icon = ydlc2.Icon;
                    }
                }
            }
            if (MyToolbarComponent.GlobalBuilding || (MySession.Static.ControlledEntity is MyCharacter))
            {
                num2 = 1;
            }
            else
            {
                num2 = !(MySession.Static.ControlledEntity is MyCockpit) ? 0 : ((int) (MySession.Static.ControlledEntity as MyCockpit).BuildingMode);
            }
            bool enabled = (((bool) num2) & researched) & flag2;
            GridItemUserData userData = new GridItemUserData();
            userData.ItemData = MyToolbarItemFactory.ObjectBuilderFromDefinition(definition);
            MyGuiGridItem item = new MyGuiGridItem(definition.Icons, subicon, this.GetDefinitionTooltip(definition, researched), userData, enabled) {
                SubIcon2 = icon,
                ItemDefinition = definition,
                OverlayColorMask = new VRageMath.Vector4(0f, 1f, 0f, 0.25f)
            };
            node.Items.Add(item);
        }

        private List<MyGuiControlResearchGraph.GraphNode> CreateResearchGraph()
        {
            List<MyGuiControlResearchGraph.GraphNode> list = new List<MyGuiControlResearchGraph.GraphNode>();
            Dictionary<string, MyGuiControlResearchGraph.GraphNode> nodesByName = new Dictionary<string, MyGuiControlResearchGraph.GraphNode>();
            List<MyGuiControlResearchGraph.GraphNode> children = new List<MyGuiControlResearchGraph.GraphNode>();
            HashSet<SerializableDefinitionId> set = new HashSet<SerializableDefinitionId>();
            foreach (MyResearchGroupDefinition definition in MyDefinitionManager.Static.GetResearchGroupDefinitions())
            {
                HashSet<string> set2 = new HashSet<string>();
                List<MyCubeBlockDefinition> items = new List<MyCubeBlockDefinition>();
                if (definition.Members != null)
                {
                    SerializableDefinitionId[] members = definition.Members;
                    int index = 0;
                    while (true)
                    {
                        if (index >= members.Length)
                        {
                            if (set2.Count == 0)
                            {
                                MyGuiControlResearchGraph.GraphNode item = this.CreateNode(definition, items, null);
                                list.Add(item);
                                nodesByName.Add(item.Name, item);
                            }
                            else
                            {
                                foreach (string str2 in set2)
                                {
                                    MyGuiControlResearchGraph.GraphNode item = this.CreateNode(definition, items, str2);
                                    children.Add(item);
                                    if (!nodesByName.ContainsKey(item.Name))
                                    {
                                        nodesByName.Add(item.Name, item);
                                    }
                                }
                            }
                            break;
                        }
                        SerializableDefinitionId id = members[index];
                        MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(id);
                        if (((cubeBlockDefinition != null) && (cubeBlockDefinition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS)) && (cubeBlockDefinition.AvailableInSurvival || !MySession.Static.SurvivalMode))
                        {
                            MyResearchBlockDefinition researchBlock = MyDefinitionManager.Static.GetResearchBlock(id);
                            if (((researchBlock != null) && (researchBlock.UnlockedByGroups != null)) && (researchBlock.UnlockedByGroups.Length != 0))
                            {
                                foreach (string str in researchBlock.UnlockedByGroups)
                                {
                                    set2.Add(str);
                                }
                            }
                            set.Add(id);
                            MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName);
                            if ((definitionGroup != null) && (((definitionGroup.Large == null) || (definitionGroup.Small == null)) || (definitionGroup.Small.Id != cubeBlockDefinition.Id)))
                            {
                                items.Add(cubeBlockDefinition);
                            }
                        }
                        index++;
                    }
                }
            }
            Dictionary<string, MyGuiControlResearchGraph.GraphNode> dictionary2 = new Dictionary<string, MyGuiControlResearchGraph.GraphNode>();
            foreach (MyResearchBlockDefinition definition4 in MyDefinitionManager.Static.GetResearchBlockDefinitions())
            {
                if (set.Contains((SerializableDefinitionId) definition4.Id))
                {
                    continue;
                }
                MyCubeBlockDefinition cubeBlockDefinition = MyDefinitionManager.Static.GetCubeBlockDefinition(definition4.Id);
                if ((cubeBlockDefinition != null) && ((cubeBlockDefinition.Public || MyFakes.ENABLE_NON_PUBLIC_BLOCKS) && ((cubeBlockDefinition.AvailableInSurvival || !MySession.Static.SurvivalMode) && (definition4.UnlockedByGroups != null))))
                {
                    foreach (string str3 in definition4.UnlockedByGroups)
                    {
                        MyGuiControlResearchGraph.GraphNode node3 = null;
                        if (!nodesByName.TryGetValue(str3, out node3))
                        {
                            MyLog.Default.WriteLine($"Research group {str3} was not found for block {definition4.Id}.");
                        }
                        else
                        {
                            MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(cubeBlockDefinition.BlockPairName);
                            if ((definitionGroup != null) && (((definitionGroup.Large == null) || (definitionGroup.Small == null)) || (definitionGroup.Small.Id != cubeBlockDefinition.Id)))
                            {
                                MyGuiControlResearchGraph.GraphNode node4 = null;
                                if (!dictionary2.TryGetValue(str3, out node4))
                                {
                                    node4 = new MyGuiControlResearchGraph.GraphNode();
                                    dictionary2.Add(str3, node4);
                                    node4.Name = "Common_" + str3;
                                    node4.UnlockedBy = str3;
                                    node3.Children.Add(node4);
                                }
                                this.CreateNodeItem(node4, cubeBlockDefinition);
                            }
                        }
                    }
                }
            }
            CreateGraph(nodesByName, children);
            return list;
        }

        private void dragAndDrop_OnDrop(object sender, MyDragAndDropEventArgs eventArgs)
        {
            if (((eventArgs.DropTo != null) && !this.m_toolbarControl.IsToolbarGrid(eventArgs.DragFrom.Grid)) && this.m_toolbarControl.IsToolbarGrid(eventArgs.DropTo.Grid))
            {
                GridItemUserData userData = (GridItemUserData) eventArgs.Item.UserData;
                if (userData.ItemData is MyObjectBuilder_ToolbarItemEmpty)
                {
                    return;
                }
                if ((eventArgs.DropTo.ItemIndex >= 0) && (eventArgs.DropTo.ItemIndex < 9))
                {
                    MyToolbarItem item = MyToolbarItemFactory.CreateToolbarItem(userData.ItemData);
                    if (!(item is MyToolbarItemActions))
                    {
                        DropGridItemToToolbar(item, eventArgs.DropTo.ItemIndex);
                        if (item.WantsToBeActivated)
                        {
                            MyToolbarComponent.CurrentToolbar.ActivateItemAtSlot(eventArgs.DropTo.ItemIndex, false, false, false);
                        }
                    }
                    else if (!this.UpdateContextMenu(ref this.m_onDropContextMenu, item as MyToolbarItemActions, userData))
                    {
                        DropGridItemToToolbar(item, eventArgs.DropTo.ItemIndex);
                    }
                    else
                    {
                        this.m_onDropContextMenuToolbarIndex = eventArgs.DropTo.ItemIndex;
                        this.m_onDropContextMenu.Enabled = true;
                        this.m_onDropContextMenuItem = item;
                    }
                }
            }
            this.m_toolbarControl.HandleDragAndDrop(sender, eventArgs);
        }

        public static void DropGridItemToToolbar(MyToolbarItem item, int slot)
        {
            RequestItemParameters(item, delegate (bool success) {
                if (success)
                {
                    MyToolbar currentToolbar = MyToolbarComponent.CurrentToolbar;
                    for (int i = 0; i < currentToolbar.SlotCount; i++)
                    {
                        if ((currentToolbar.GetSlotItem(i) != null) && currentToolbar.GetSlotItem(i).Equals(item))
                        {
                            currentToolbar.SetItemAtSlot(i, null);
                        }
                    }
                    MyGuiAudio.PlaySound(MyGuiSounds.HudItem);
                    MyToolbarComponent.CurrentToolbar.SetItemAtSlot(slot, item);
                }
            });
        }

        private string GetDefinitionTooltip(MyDefinitionBase definition, bool researched)
        {
            StringBuilder builder = new StringBuilder(definition.DisplayNameText);
            if (!researched)
            {
                builder.Append("\n").Append(MyTexts.GetString(MyCommonTexts.ScreenCubeBuilderRequiresResearch)).Append(" ");
                MyCubeBlockDefinition definition2 = definition as MyCubeBlockDefinition;
                if (definition2 != null)
                {
                    MyResearchBlockDefinition researchBlock = MyDefinitionManager.Static.GetResearchBlock(definition2.Id);
                    if (researchBlock != null)
                    {
                        foreach (string str in researchBlock.UnlockedByGroups)
                        {
                            MyResearchGroupDefinition researchGroup = MyDefinitionManager.Static.GetResearchGroup(str);
                            if (researchGroup != null)
                            {
                                foreach (SerializableDefinitionId id in researchGroup.Members)
                                {
                                    MyDefinitionBase base2;
                                    if (MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(id, out base2) && !this.m_tmpUniqueStrings.Contains(base2.DisplayNameText))
                                    {
                                        builder.Append("\n");
                                        builder.Append(base2.DisplayNameText);
                                        this.m_tmpUniqueStrings.Add(base2.DisplayNameText);
                                    }
                                }
                            }
                        }
                    }
                }
                this.m_tmpUniqueStrings.Clear();
            }
            if (!definition.DLCs.IsNullOrEmpty<string>() && (MySession.Static.GetComponent<MySessionComponentDLC>().GetFirstMissingDefinitionDLC(definition, Sync.MyId) != null))
            {
                builder.Append("\n");
                for (int i = 0; i < definition.DLCs.Length; i++)
                {
                    builder.Append("\n");
                    builder.Append(MyDLCs.GetRequiredDLCTooltip(definition.DLCs[i]));
                }
            }
            return builder.ToString();
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenToolbarConfigBase";

        private MyIdentity GetIdentity()
        {
            MyPlayer playerFromCharacter = null;
            if (this.m_character != null)
            {
                playerFromCharacter = MyPlayer.GetPlayerFromCharacter(this.m_character);
            }
            else if (this.m_shipController != null)
            {
                if ((this.m_shipController.Pilot != null) && (this.m_shipController.ControllerInfo.Controller != null))
                {
                    playerFromCharacter = this.m_shipController.ControllerInfo.Controller.Player;
                }
            }
            else if (MySession.Static.LocalCharacter != null)
            {
                playerFromCharacter = MyPlayer.GetPlayerFromCharacter(MySession.Static.LocalCharacter);
            }
            return playerFromCharacter?.Identity;
        }

        private void grid_ItemClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (eventArgs.Button == MySharedButtonsEnum.Primary)
            {
                MyGuiGridItem item = sender.TryGetItemAt(eventArgs.RowIndex, eventArgs.ColumnIndex);
                if ((item != null) && item.Enabled)
                {
                    MyToolbarItemFactory.CreateToolbarItem(((GridItemUserData) item.UserData).ItemData);
                }
            }
            else if (eventArgs.Button != MySharedButtonsEnum.Secondary)
            {
                if (MyInput.Static.IsAnyShiftKeyPressed())
                {
                    this.grid_ItemShiftClicked(sender, eventArgs);
                }
            }
            else
            {
                MyGuiGridItem item2 = sender.TryGetItemAt(eventArgs.RowIndex, eventArgs.ColumnIndex);
                if ((item2 != null) && item2.Enabled)
                {
                    GridItemUserData userData = (GridItemUserData) item2.UserData;
                    MyToolbarItem item3 = MyToolbarItemFactory.CreateToolbarItem(userData.ItemData);
                    if (!(item3 is MyToolbarItemActions))
                    {
                        this.grid_ItemDoubleClicked(sender, eventArgs);
                    }
                    else
                    {
                        this.m_contextBlockX = eventArgs.RowIndex;
                        this.m_contextBlockY = eventArgs.ColumnIndex;
                        if (!this.UpdateContextMenu(ref this.m_contextMenu, item3 as MyToolbarItemActions, userData))
                        {
                            this.grid_ItemDoubleClicked(sender, eventArgs);
                        }
                    }
                }
            }
        }

        private void grid_ItemDoubleClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            try
            {
                MyGuiGridItem item = sender.TryGetItemAt(eventArgs.RowIndex, eventArgs.ColumnIndex);
                if ((item != null) && item.Enabled)
                {
                    GridItemUserData userData = (GridItemUserData) item.UserData;
                    if (!(userData.ItemData is MyObjectBuilder_ToolbarItemEmpty))
                    {
                        this.AddGridItemToToolbar(userData.ItemData);
                    }
                }
            }
            finally
            {
            }
        }

        private void grid_ItemShiftClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (eventArgs.Button == MySharedButtonsEnum.Primary)
            {
                MyGuiGridItem item = sender.TryGetItemAt(eventArgs.RowIndex, eventArgs.ColumnIndex);
                if ((item != null) && item.Enabled)
                {
                    MyToolbarItem item2 = MyToolbarItemFactory.CreateToolbarItem(((GridItemUserData) item.UserData).ItemData);
                    if (!item2.WantsToBeActivated)
                    {
                        item2.Activate();
                    }
                }
            }
        }

        private void grid_OnDrag(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            this.StartDragging(MyDropHandleType.MouseRelease, sender, ref eventArgs);
        }

        private void grid_PanelScrolled(MyGuiControlScrollablePanel panel)
        {
            if (this.m_contextMenu != null)
            {
                this.m_contextMenu.Deactivate();
            }
        }

        public override void HandleInput(bool receivedFocusInThisUpdate)
        {
            base.HandleInput(receivedFocusInThisUpdate);
            if ((base.FocusedControl == null) && MyInput.Static.IsKeyPress(MyKeys.Tab))
            {
                if (MyImeProcessor.Instance != null)
                {
                    MyImeProcessor.Instance.RegisterActiveScreen(this);
                }
                base.FocusedControl = this.m_searchBox.TextBox;
            }
            if (MyInput.Static.IsMouseReleased(MyMouseButtonsEnum.Right))
            {
                if (this.m_onDropContextMenu.Enabled)
                {
                    this.m_onDropContextMenu.Enabled = false;
                    this.m_contextMenu.Enabled = false;
                    this.m_onDropContextMenu.Activate(true);
                }
                else if (this.m_contextMenu.Enabled && !this.m_onDropContextMenu.Visible)
                {
                    this.m_contextMenu.Enabled = false;
                    this.m_contextMenu.Activate(true);
                }
            }
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.BUILD_SCREEN))
            {
                if (!this.m_searchBox.TextBox.HasFocus)
                {
                    if (base.m_closingCueEnum != null)
                    {
                        MyGuiSoundManager.PlaySound(base.m_closingCueEnum.Value);
                    }
                    else
                    {
                        MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    }
                    this.CloseScreen();
                }
                else if (MyInput.Static.IsNewGameControlJoystickOnlyPressed(MyControlsSpace.BUILD_SCREEN))
                {
                    if (base.m_closingCueEnum != null)
                    {
                        MyGuiSoundManager.PlaySound(base.m_closingCueEnum.Value);
                    }
                    else
                    {
                        MyGuiSoundManager.PlaySound(GuiSounds.MouseClick);
                    }
                    this.CloseScreen();
                }
            }
        }

        public override void HandleUnhandledInput(bool receivedFocusInThisUpdate)
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.PAUSE_GAME))
            {
                MySandboxGame.PauseToggle();
            }
        }

        private bool IsValidItem(MyCubeBlockDefinitionGroup group)
        {
            bool flag = false;
            int num = 0;
            while (true)
            {
                if (num < group.SizeCount)
                {
                    int num1;
                    MyCubeBlockDefinition definition = group[(MyCubeSize) ((byte) num)];
                    if (!MyFakes.ENABLE_NON_PUBLIC_BLOCKS && (((definition == null) || !definition.Public) || !definition.Enabled))
                    {
                        num++;
                        continue;
                    }
                    flag = true;
                    if (!MySession.Static.ResearchEnabled || MySession.Static.CreativeToolsEnabled(Sync.MyId))
                    {
                        num1 = 1;
                    }
                    else
                    {
                        num1 = (int) MySessionComponentResearch.Static.CanUse(this.m_character ?? this.m_shipController?.Pilot, definition.Id);
                    }
                    flag &= num1;
                }
                return flag;
            }
        }

        private void m_researchGraph_ItemClicked(object sender, MySharedButtonsEnum button)
        {
            MyGuiGridItem selectedItem = (sender as MyGuiControlResearchGraph).SelectedItem;
            if ((selectedItem != null) && selectedItem.Enabled)
            {
                if (button == MySharedButtonsEnum.Primary)
                {
                    MyToolbarItemFactory.CreateToolbarItem(((GridItemUserData) selectedItem.UserData).ItemData);
                }
                else if (button != MySharedButtonsEnum.Secondary)
                {
                    if (MyInput.Static.IsAnyShiftKeyPressed())
                    {
                        this.m_researchGraph_ItemDoubleClicked(sender, System.EventArgs.Empty);
                    }
                }
                else
                {
                    GridItemUserData userData = (GridItemUserData) selectedItem.UserData;
                    MyToolbarItemActions item = MyToolbarItemFactory.CreateToolbarItem(userData.ItemData) as MyToolbarItemActions;
                    if (item == null)
                    {
                        this.m_researchGraph_ItemDoubleClicked(sender, System.EventArgs.Empty);
                    }
                    else if (!this.UpdateContextMenu(ref this.m_contextMenu, item, userData))
                    {
                        this.m_researchGraph_ItemDoubleClicked(sender, System.EventArgs.Empty);
                    }
                }
            }
        }

        private void m_researchGraph_ItemDoubleClicked(object sender, System.EventArgs e)
        {
            MyGuiGridItem selectedItem = (sender as MyGuiControlResearchGraph).SelectedItem;
            if ((selectedItem != null) && selectedItem.Enabled)
            {
                GridItemUserData userData = (GridItemUserData) selectedItem.UserData;
                if (!(userData.ItemData is MyObjectBuilder_ToolbarItemEmpty))
                {
                    this.AddGridItemToToolbar(userData.ItemData);
                }
            }
        }

        private void m_researchGraph_ItemDragged(object sender, MyGuiGridItem item)
        {
            this.StartDragging(MyDropHandleType.MouseRelease, sender as MyGuiControlResearchGraph, item);
        }

        protected override void OnClosed()
        {
            this.m_toolbarStyle.VisibleCondition = this.visibleCondition;
            Static = null;
            base.OnClosed();
            MyGuiScreenGamePlay.ActiveGameplayScreen = null;
        }

        private void onDropContextMenu_ItemClicked(MyGuiControlContextMenu sender, MyGuiControlContextMenu.EventArgs args)
        {
            int onDropContextMenuToolbarIndex = this.m_onDropContextMenuToolbarIndex;
            if ((onDropContextMenuToolbarIndex >= 0) && (onDropContextMenuToolbarIndex < MyToolbarComponent.CurrentToolbar.SlotCount))
            {
                MyToolbarItem onDropContextMenuItem = this.m_onDropContextMenuItem;
                if (onDropContextMenuItem is MyToolbarItemActions)
                {
                    (onDropContextMenuItem as MyToolbarItemActions).ActionId = (string) args.UserData;
                    DropGridItemToToolbar(onDropContextMenuItem, onDropContextMenuToolbarIndex);
                }
            }
        }

        private void OnItemDragged(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            this.StartDragging(MyDropHandleType.MouseRelease, sender, ref eventArgs);
        }

        public void RecreateBlockCategories(Dictionary<string, MyGuiBlockCategoryDefinition> loadedCategories, SortedDictionary<string, MyGuiBlockCategoryDefinition> categories)
        {
            categories.Clear();
            foreach (KeyValuePair<string, MyGuiBlockCategoryDefinition> pair in loadedCategories)
            {
                pair.Value.ValidItems = 0;
            }
            if ((MySession.Static.ResearchEnabled && !MySession.Static.CreativeToolsEnabled(Sync.MyId)) && (MySessionComponentResearch.Static.m_requiredResearch.Count > 0))
            {
                foreach (string str in MyDefinitionManager.Static.GetDefinitionPairNames())
                {
                    MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(str);
                    if (this.IsValidItem(definitionGroup) && ((definitionGroup.AnyPublic != null) && MySessionComponentResearch.Static.CanUse(this.m_character, definitionGroup.AnyPublic.Id)))
                    {
                        foreach (MyGuiBlockCategoryDefinition definition in loadedCategories.Values)
                        {
                            if (definition.HasItem(definitionGroup.AnyPublic.Id.ToString()))
                            {
                                definition.ValidItems++;
                            }
                        }
                    }
                }
            }
            MyPlayer playerFromCharacter = null;
            if ((this.m_shipController == null) || !this.m_shipController.BuildingMode)
            {
                playerFromCharacter = MyPlayer.GetPlayerFromCharacter(this.m_character);
            }
            else if (this.m_shipController.Pilot != null)
            {
                playerFromCharacter = this.m_shipController.ControllerInfo.Controller.Player;
            }
            if (playerFromCharacter != null)
            {
                foreach (KeyValuePair<string, MyGuiBlockCategoryDefinition> pair2 in loadedCategories)
                {
                    if (((!MySession.Static.SurvivalMode || pair2.Value.AvailableInSurvival) || MySession.Static.IsUserAdmin(playerFromCharacter.Client.SteamUserId)) && (((!MySession.Static.CreativeMode || pair2.Value.ShowInCreative) && ((((this.m_character != null) && MySession.Static.GetVoxelHandAvailable(this.m_character)) || (pair2.Key.CompareTo("VoxelHands") != 0)) && ((((GroupMode != GroupModes.HideBlockGroups) || pair2.Value.IsAnimationCategory) || pair2.Value.IsToolCategory) && (((GroupMode != GroupModes.HideEmpty) || (pair2.Value.IsAnimationCategory || pair2.Value.IsToolCategory)) || ((pair2.Value.ItemIds.Count != 0) && ((!MySession.Static.ResearchEnabled || (MySession.Static.CreativeToolsEnabled(Sync.MyId) || (MySessionComponentResearch.Static.m_requiredResearch.Count <= 0))) || (pair2.Value.ValidItems != 0))))))) && pair2.Value.IsBlockCategory))
                    {
                        categories.Add(pair2.Value.Name, pair2.Value);
                    }
                }
            }
        }

        public override void RecreateControls(bool contructor)
        {
            MyObjectBuilder_GuiScreen screen;
            base.RecreateControls(contructor);
            MyAnalyticsHelper.ReportActivityStart(null, "show_toolbar_config", string.Empty, "gui", string.Empty, true);
            this.m_character = null;
            this.m_shipController = null;
            m_ownerChanged = !ReferenceEquals(m_previousOwner, MyToolbarComponent.CurrentToolbar.Owner);
            m_previousOwner = MyToolbarComponent.CurrentToolbar.Owner;
            if (MyToolbarComponent.CurrentToolbar.Owner == null)
            {
                this.m_character = MySession.Static.LocalCharacter;
            }
            else
            {
                this.m_shipController = MyToolbarComponent.CurrentToolbar.Owner as MyShipController;
            }
            this.m_screenCubeGrid = this.m_screenOwner?.CubeGrid;
            bool isShip = this.m_screenCubeGrid != null;
            string str = Path.Combine("Data", "Screens", "CubeBuilder.gsc");
            MyObjectBuilderSerializer.DeserializeXML<MyObjectBuilder_GuiScreen>(Path.Combine(MyFileSystem.ContentPath, str), out screen);
            base.Init(screen);
            this.m_tabControl = this.Controls.GetControlByName("Tab") as MyGuiControlTabControl;
            this.m_tabControl.TabButtonScale = 0.5f;
            MyGuiControlTabPage controlByName = this.m_tabControl.Controls.GetControlByName("BlocksPage") as MyGuiControlTabPage;
            this.m_gridBlocks = (MyGuiControlGrid) controlByName.Controls.GetControlByName("Grid");
            VRageMath.Vector4 colorMask = this.m_gridBlocks.ColorMask;
            this.m_gridBlocks.ColorMask = VRageMath.Vector4.One;
            this.m_gridBlocks.ItemBackgroundColorMask = colorMask;
            this.m_categoriesListbox = (MyGuiControlListbox) this.Controls.GetControlByName("CategorySelector");
            this.m_categoriesListbox.VisualStyle = MyGuiControlListboxStyleEnum.ToolsBlocks;
            this.m_categoriesListbox.ItemClicked += new Action<MyGuiControlListbox>(this.categories_ItemClicked);
            MyGuiControlTextbox control = (MyGuiControlTextbox) this.Controls.GetControlByName("SearchItemTextBox");
            MyGuiControlLabel label = (MyGuiControlLabel) this.Controls.GetControlByName("BlockSearchLabel");
            this.m_searchBox = new MyGuiControlSearchBox(new Vector2?(control.Position + new Vector2(-0.1f, 0f)), new Vector2?(control.Size + new Vector2(0.2f, 0f)), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
            this.m_searchBox.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.searchItemTexbox_TextChanged);
            this.m_searchBox.Enabled = false;
            MyGuiControlTextbox.MySkipCombination combination = new MyGuiControlTextbox.MySkipCombination {
                Shift = true,
                Keys = null
            };
            MyGuiControlTextbox.MySkipCombination[] combinationArray1 = new MyGuiControlTextbox.MySkipCombination[3];
            combinationArray1[0] = combination;
            combination = new MyGuiControlTextbox.MySkipCombination {
                Ctrl = true,
                Keys = null
            };
            combinationArray1[1] = combination;
            combination = new MyGuiControlTextbox.MySkipCombination();
            combination.Keys = new MyKeys[] { MyKeys.Snapshot, MyKeys.Delete };
            combinationArray1[2] = combination;
            this.m_searchBox.TextBox.SkipCombinations = combinationArray1;
            this.Controls.Add(this.m_searchBox);
            this.Controls.Remove(control);
            this.Controls.Remove(label);
            controlByName.Controls.Remove(this.m_gridBlocks);
            this.m_gridBlocks.VisualStyle = MyGuiControlGridStyleEnum.Toolbar;
            this.m_gridBlocksPanel = new MyGuiControlScrollablePanel(null);
            MyGuiStyleDefinition visualStyle = MyGuiControlGrid.GetVisualStyle(MyGuiControlGridStyleEnum.ToolsBlocks);
            this.m_gridBlocksPanel.BackgroundTexture = visualStyle.BackgroundTexture;
            this.m_gridBlocksPanel.ColorMask = colorMask;
            this.m_gridBlocksPanel.ScrolledControl = this.m_gridBlocks;
            this.m_gridBlocksPanel.ScrollbarVEnabled = true;
            this.m_gridBlocksPanel.ScrolledAreaPadding = new MyGuiBorderThickness(10f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            this.m_gridBlocksPanel.FitSizeToScrolledControl();
            this.m_gridBlocksPanel.Size += new Vector2(0f, 0.032f);
            this.m_gridBlocksPanel.PanelScrolled += new Action<MyGuiControlScrollablePanel>(this.grid_PanelScrolled);
            this.m_gridBlocksPanel.Position = new Vector2(-0.216f, -0.044f);
            controlByName.Controls.Add(this.m_gridBlocksPanel);
            if (this.m_scrollOffset != 0f)
            {
                this.m_gridBlocksPanel.SetPageVertical(this.m_scrollOffset);
            }
            else
            {
                this.m_gridBlocksPanel.ScrollbarVPosition = m_savedVPosition;
            }
            this.m_researchGraph = new MyGuiControlResearchGraph();
            this.m_researchGraph.ItemSize = new Vector2(0.05125f, 0.06833334f) * 0.75f;
            this.m_researchGraph.NodePadding = this.m_researchGraph.ItemSize / 7f;
            this.m_researchGraph.NodeMargin = this.m_researchGraph.ItemSize / 7f;
            this.m_researchGraph.Size = new Vector2(0.52f, 0f);
            this.m_researchGraph.ItemClicked += new EventHandler<MySharedButtonsEnum>(this.m_researchGraph_ItemClicked);
            this.m_researchGraph.ItemDoubleClicked += new EventHandler(this.m_researchGraph_ItemDoubleClicked);
            this.m_researchGraph.ItemDragged += new EventHandler<MyGuiGridItem>(this.m_researchGraph_ItemDragged);
            this.m_researchGraph.Nodes = this.CreateResearchGraph();
            MyGuiControlTabPage page2 = this.m_tabControl.Controls.GetControlByName("ResearchPage") as MyGuiControlTabPage;
            if (((MySession.Static == null) || (MySession.Static.Settings == null)) || !MySession.Static.Settings.EnableResearch)
            {
                page2.SetToolTip(MySpaceTexts.ToolbarConfig_ResearchTabDisabledTooltip);
                page2.Enabled = false;
            }
            else
            {
                page2.SetToolTip((string) null);
                page2.Enabled = true;
            }
            this.m_researchPanel = new MyGuiControlScrollablePanel(null);
            this.m_researchPanel.BackgroundTexture = visualStyle.BackgroundTexture;
            this.m_researchPanel.ColorMask = colorMask;
            this.m_researchPanel.ScrolledControl = this.m_researchGraph;
            this.m_researchPanel.ScrollbarVEnabled = true;
            this.m_researchPanel.ScrollbarHEnabled = true;
            this.m_researchPanel.ScrolledAreaPadding = new MyGuiBorderThickness(10f / MyGuiConstants.GUI_OPTIMAL_SIZE.X, 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y);
            this.m_researchPanel.FitSizeToScrolledControl();
            this.m_researchPanel.Size = this.m_gridBlocksPanel.Size;
            this.m_researchPanel.Position = this.m_gridBlocksPanel.Position;
            page2.Controls.Add(this.m_researchPanel);
            MyGuiControlLabel label2 = (MyGuiControlLabel) this.Controls.GetControlByName("LabelToolbar");
            label2.Position = new Vector2(label2.Position.X - 0.12f, label2.Position.Y);
            object[] args = new object[] { this.m_toolbarStyle };
            this.m_toolbarControl = (MyGuiControlToolbar) Activator.CreateInstance(MyPerGameSettings.GUI.ToolbarControl, args);
            this.m_toolbarControl.Position = this.m_toolbarStyle.CenterPosition - new Vector2(0.62f, 0.5f);
            this.m_toolbarControl.OriginAlign = this.m_toolbarStyle.OriginAlign;
            if (MyPerGameSettings.Game == GameEnum.ME_GAME)
            {
                this.m_toolbarControl.ToolbarGrid.ItemDragged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.OnItemDragged);
            }
            this.Controls.Add(this.m_toolbarControl);
            this.m_onDropContextMenu = new MyGuiControlContextMenu();
            this.m_onDropContextMenu.Deactivate();
            this.m_onDropContextMenu.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.onDropContextMenu_ItemClicked);
            this.Controls.Add(this.m_onDropContextMenu);
            this.m_gridBlocks.SetItemsToDefault();
            this.m_gridBlocks.ItemDoubleClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemDoubleClicked);
            this.m_gridBlocks.ItemClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemClicked);
            this.m_gridBlocks.ItemDragged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_OnDrag);
            this.m_dragAndDrop = new MyGuiControlGridDragAndDrop(MyGuiConstants.DRAG_AND_DROP_BACKGROUND_COLOR, MyGuiConstants.DRAG_AND_DROP_TEXT_COLOR, 0.7f, MyGuiConstants.DRAG_AND_DROP_TEXT_OFFSET, true);
            this.m_dragAndDrop.ItemDropped += new OnItemDropped(this.dragAndDrop_OnDrop);
            this.m_dragAndDrop.DrawBackgroundTexture = false;
            this.Controls.Add(this.m_dragAndDrop);
            this.m_contextMenu = new MyGuiControlContextMenu();
            this.m_contextMenu.ItemClicked += new Action<MyGuiControlContextMenu, MyGuiControlContextMenu.EventArgs>(this.contextMenu_ItemClicked);
            this.Controls.Add(this.m_contextMenu);
            this.m_contextMenu.Deactivate();
            MyGuiControlPcuBar bar1 = new MyGuiControlPcuBar(new Vector2(0.153f, 0.4f));
            bar1.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            this.m_PCUControl = bar1;
            this.Controls.Add(this.m_PCUControl);
            this.m_PCUControl.InitPCU(this.GetIdentity());
            this.SolveAspectRatio();
            this.AddCategoryToDisplayList(MyTexts.GetString(MySpaceTexts.DisplayName_Category_AllBlocks), null);
            Dictionary<string, MyGuiBlockCategoryDefinition> categories = MyDefinitionManager.Static.GetCategories();
            if ((this.m_screenCubeGrid == null) || ((this.m_shipController != null) && ((this.m_shipController == null) || this.m_shipController.BuildingMode)))
            {
                if ((this.m_character != null) || ((this.m_shipController != null) && this.m_shipController.BuildingMode))
                {
                    if (GroupMode != GroupModes.HideAll)
                    {
                        this.RecreateBlockCategories(categories, this.m_sortedCategories);
                    }
                    this.AddCubeDefinitionsToBlocks(this.m_categorySearchCondition);
                    this.m_tabControl.GetTab(1).IsTabVisible = true;
                    this.m_shipMode = false;
                    this.m_PCUControl.Visible = true;
                }
            }
            else
            {
                int num1;
                if (!isShip || (this.m_shipController == null))
                {
                    num1 = 0;
                }
                else
                {
                    num1 = (int) !this.m_shipController.EnableShipControl;
                }
                if (num1 != 0)
                {
                    this.m_categoriesListbox.Items.Clear();
                }
                else
                {
                    this.RecreateShipCategories(categories, this.m_sortedCategories, this.m_screenCubeGrid);
                    this.AddShipGroupsIntoCategoryList(this.m_screenCubeGrid);
                    this.AddShipBlocksDefinitions(this.m_screenCubeGrid, isShip, null);
                    this.AddShipGunsToCategories(categories, this.m_sortedCategories);
                }
                if ((this.m_shipController != null) && (this.m_shipController.ToolbarType != MyToolbarType.None))
                {
                    MyGuiBlockCategoryDefinition definition2 = null;
                    if (!this.m_sortedCategories.TryGetValue("CharacterAnimations", out definition2) && categories.TryGetValue("CharacterAnimations", out definition2))
                    {
                        this.m_sortedCategories.Add("CharacterAnimations", definition2);
                    }
                }
                this.m_researchGraph.Nodes.Clear();
                this.m_tabControl.GetTab(1).IsTabVisible = false;
                this.m_shipMode = true;
                this.m_PCUControl.Visible = false;
                this.m_PCUControl.Controls.Clear();
            }
            if (MyFakes.ENABLE_SHIP_BLOCKS_TOOLBAR)
            {
                this.m_gridBlocks.Visible = true;
                this.m_gridBlocksPanel.ScrollbarVEnabled = true;
            }
            else
            {
                this.m_gridBlocksPanel.ScrollbarVEnabled = !isShip;
                this.m_gridBlocks.Visible = !isShip;
            }
            this.SortCategoriesToDisplayList();
            if (this.m_categoriesListbox.Items.Count > 0)
            {
                this.SelectCategories();
            }
        }

        private void RecreateShipCategories(Dictionary<string, MyGuiBlockCategoryDefinition> loadedCategories, SortedDictionary<string, MyGuiBlockCategoryDefinition> categories, MyCubeGrid grid)
        {
            if (((grid != null) && (grid.GridSystems.TerminalSystem != null)) && (grid.GridSystems.TerminalSystem.BlockGroups != null))
            {
                categories.Clear();
                MyTerminalBlock[] array = grid.GridSystems.TerminalSystem.Blocks.ToArray();
                Array.Sort<MyTerminalBlock>(array, MyTerminalComparer.Static);
                List<string> list = new List<string>();
                foreach (MyTerminalBlock block in array)
                {
                    if (block != null)
                    {
                        string item = block.BlockDefinition.Id.ToString();
                        if (!list.Contains(item))
                        {
                            list.Add(item);
                        }
                    }
                }
                foreach (string str2 in list)
                {
                    foreach (KeyValuePair<string, MyGuiBlockCategoryDefinition> pair in loadedCategories)
                    {
                        if (!pair.Value.IsShipCategory)
                        {
                            continue;
                        }
                        if (pair.Value.HasItem(str2) && pair.Value.SearchBlocks)
                        {
                            MyGuiBlockCategoryDefinition definition = null;
                            if (!categories.TryGetValue(pair.Value.Name, out definition))
                            {
                                categories.Add(pair.Value.Name, pair.Value);
                            }
                        }
                    }
                }
            }
        }

        public override bool RegisterClicks() => 
            true;

        public static void ReinitializeBlockScrollbarPosition()
        {
            m_savedVPosition = 0f;
        }

        public static void RequestItemParameters(MyToolbarItem item, Action<bool> callback)
        {
            MyToolbarItemTerminalBlock block = item as MyToolbarItemTerminalBlock;
            if (block != null)
            {
                ITerminalAction actionOrNull = block.GetActionOrNull(block.ActionId);
                if ((actionOrNull != null) && (actionOrNull.GetParameterDefinitions().Count > 0))
                {
                    actionOrNull.RequestParameterCollection(block.Parameters, callback);
                    return;
                }
            }
            callback(true);
        }

        public static void Reset()
        {
            m_allSelectedCategories.Clear();
        }

        private void searchItemTexbox_TextChanged(string text)
        {
            if (this.m_framesBeforeSearchEnabled <= 0)
            {
                this.m_gridBlocks.SetItemsToDefault();
                string str = text;
                if (!string.IsNullOrWhiteSpace(str) && !string.IsNullOrEmpty(str))
                {
                    this.m_nameSearchCondition.SearchName = str;
                    if ((this.m_shipController != null) && !this.m_shipController.EnableShipControl)
                    {
                        this.AddAnimations(true, this.m_nameSearchCondition);
                    }
                    else
                    {
                        this.AddToolsAndAnimations(this.m_nameSearchCondition);
                        this.UpdateGridBlocksBySearchCondition(this.m_nameSearchCondition);
                        this.SearchResearch(this.m_nameSearchCondition);
                    }
                }
                else
                {
                    if ((this.m_character == null) && ((this.m_shipController == null) || !this.m_shipController.BuildingMode))
                    {
                        this.AddShipBlocksDefinitions(this.m_screenCubeGrid, true, null);
                    }
                    else
                    {
                        this.AddCubeDefinitionsToBlocks(null);
                    }
                    this.m_nameSearchCondition.Clean();
                    this.SearchResearch(null);
                }
            }
        }

        private void SearchNode(MyGuiControlResearchGraph.GraphNode node, IMySearchCondition searchCondition)
        {
            foreach (MyGuiGridItem item in node.Items)
            {
                bool flag = (searchCondition != null) && searchCondition.MatchesCondition(item.ItemDefinition);
                item.OverlayPercent = flag ? 1f : 0f;
                if (flag && (this.m_minVerticalPosition > node.Position.Y))
                {
                    this.m_minVerticalPosition = node.Position.Y;
                    this.m_researchItemFound = true;
                }
            }
            foreach (MyGuiControlResearchGraph.GraphNode node2 in node.Children)
            {
                this.SearchNode(node2, searchCondition);
            }
        }

        private void SearchResearch(IMySearchCondition searchCondition)
        {
            this.m_minVerticalPosition = float.MaxValue;
            this.m_researchItemFound = false;
            if ((this.m_researchGraph != null) && (this.m_researchGraph.Nodes != null))
            {
                foreach (MyGuiControlResearchGraph.GraphNode node in this.m_researchGraph.Nodes)
                {
                    this.SearchNode(node, searchCondition);
                }
            }
            if (this.m_researchItemFound)
            {
                this.m_researchPanel.SetVerticalScrollbarValue(this.m_minVerticalPosition);
            }
        }

        protected void SelectCategories()
        {
            List<MyGuiControlListbox.Item> list = new List<MyGuiControlListbox.Item>();
            if ((m_allSelectedCategories.Count == 0) || m_ownerChanged)
            {
                list.Add(this.m_categoriesListbox.Items[0]);
            }
            else
            {
                using (ObservableCollection<MyGuiControlListbox.Item>.Enumerator enumerator = this.m_categoriesListbox.Items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        MyGuiControlListbox.Item item;
                        if (m_allSelectedCategories.Exists(x => ReferenceEquals(x, item.UserData)))
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            m_allSelectedCategories.Clear();
            this.m_categoriesListbox.SelectedItems = list;
            this.categories_ItemClicked(this.m_categoriesListbox);
        }

        private void SetOrReplaceItemOnPosition(MyGuiControlGrid grid, MyGuiGridItem gridItem, Vector2I position)
        {
            MyGuiGridItem item = grid.TryGetItemAt(position.Y, position.X);
            grid.SetItemAt(position.Y, position.X, gridItem);
            if (item != null)
            {
                grid.Add(item, 0);
            }
        }

        private void SolveAspectRatio()
        {
            Rectangle fullscreenRectangle = MyGuiManager.GetFullscreenRectangle();
            MyAspectRatioEnum closestAspectRatio = MyVideoSettingsManager.GetClosestAspectRatio(((float) fullscreenRectangle.Width) / ((float) fullscreenRectangle.Height));
            Console.WriteLine(closestAspectRatio);
            switch (closestAspectRatio)
            {
                case MyAspectRatioEnum.Normal_4_3:
                case MyAspectRatioEnum.Unsupported_5_4:
                {
                    this.m_gridBlocks.ColumnsCount = 8;
                    this.m_gridBlocksPanel.Size *= new Vector2(0.82f, 1f);
                    this.m_researchPanel.Size = this.m_gridBlocksPanel.Size;
                    this.m_researchGraph.Size = new Vector2(0.4f, 0f);
                    this.m_researchGraph.InvalidateItemsLayout();
                    this.m_categoriesListbox.PositionX *= 0.9f;
                    MyGuiControlBase controlByName = this.Controls.GetControlByName("BlockInfoPanel");
                    controlByName.PositionX *= 0.78f;
                    MyGuiControlLabel label1 = (MyGuiControlLabel) this.Controls.GetControlByName("CaptionLabel2");
                    label1.PositionX *= 0.9f;
                    MyGuiControlLabel label2 = (MyGuiControlLabel) this.Controls.GetControlByName("LabelSubtitle");
                    label2.PositionX *= 0.9f;
                    this.m_searchBox.PositionX *= 0.68f;
                    break;
                }
                default:
                    break;
            }
            this.CalculateBlockOffsets();
        }

        protected void SortCategoriesToDisplayList()
        {
            foreach (string str in this.m_forcedCategoryOrder)
            {
                MyGuiBlockCategoryDefinition definition = null;
                if (this.m_sortedCategories.TryGetValue(str, out definition))
                {
                    this.AddCategoryToDisplayList(definition.DisplayNameText, definition);
                }
            }
            foreach (KeyValuePair<string, MyGuiBlockCategoryDefinition> pair in this.m_sortedCategories)
            {
                if (!this.m_forcedCategoryOrder.Contains<string>(pair.Key))
                {
                    this.AddCategoryToDisplayList(pair.Value.DisplayNameText, pair.Value);
                }
            }
        }

        private void StartDragging(MyDropHandleType dropHandlingType, MyGuiControlGrid grid, ref MyGuiControlGrid.EventArgs args)
        {
            MyDragAndDropInfo draggingFrom = new MyDragAndDropInfo {
                Grid = grid,
                ItemIndex = args.ItemIndex
            };
            MyGuiGridItem itemAt = grid.GetItemAt(args.ItemIndex);
            if (itemAt.Enabled)
            {
                this.m_dragAndDrop.StartDragging(dropHandlingType, args.Button, itemAt, draggingFrom, false);
                grid.HideToolTip();
            }
        }

        private void StartDragging(MyDropHandleType dropHandlingType, MyGuiControlResearchGraph graph, MyGuiGridItem draggingItem)
        {
            if (draggingItem.Enabled)
            {
                MyDragAndDropInfo draggingFrom = new MyDragAndDropInfo();
                this.m_dragAndDrop.StartDragging(dropHandlingType, MySharedButtonsEnum.Primary, draggingItem, draggingFrom, false);
                graph.HideToolTip();
            }
        }

        private void StopDragging()
        {
            this.m_dragAndDrop.Stop();
        }

        public override bool Update(bool hasFocus)
        {
            if (this.m_framesBeforeSearchEnabled > 0)
            {
                this.m_framesBeforeSearchEnabled--;
            }
            if (this.m_framesBeforeSearchEnabled == 0)
            {
                this.m_searchBox.Enabled = true;
                this.m_searchBox.TextBox.CanHaveFocus = true;
                if (MyImeProcessor.Instance != null)
                {
                    MyImeProcessor.Instance.RegisterActiveScreen(this);
                }
                base.FocusedControl = this.m_searchBox.TextBox;
                this.m_framesBeforeSearchEnabled--;
            }
            if ((this.m_frameCounterPCU < this.PCU_UPDATE_EACH_N_FRAMES) || !this.m_PCUControl.Visible)
            {
                this.m_frameCounterPCU++;
            }
            else
            {
                this.m_PCUControl.UpdatePCU(this.GetIdentity());
                this.m_frameCounterPCU = 0;
            }
            return base.Update(hasFocus);
        }

        private bool UpdateContextMenu(ref MyGuiControlContextMenu currentContextMenu, MyToolbarItemActions item, GridItemUserData data)
        {
            ListReader<ITerminalAction> reader = item.PossibleActions(this.m_toolbarControl.ShownToolbar.ToolbarType);
            if (reader.Count <= 0)
            {
                return false;
            }
            currentContextMenu.Enabled = true;
            currentContextMenu.CreateNewContextMenu();
            foreach (ITerminalAction action in reader)
            {
                currentContextMenu.AddItem(action.Name, "", action.Icon, action.Id);
            }
            return true;
        }

        protected virtual void UpdateGridBlocksBySearchCondition(IMySearchCondition searchCondition)
        {
            if (searchCondition != null)
            {
                searchCondition.CleanDefinitionGroups();
            }
            if ((this.m_shipController != null) && !this.m_shipController.EnableShipControl)
            {
                goto TR_0000;
            }
            else if ((this.m_character == null) && ((this.m_shipController == null) || !this.m_shipController.BuildingMode))
            {
                if (this.m_screenCubeGrid != null)
                {
                    this.AddShipBlocksDefinitions(this.m_screenCubeGrid, true, searchCondition);
                }
                goto TR_0000;
            }
            this.AddCubeDefinitionsToBlocks(searchCondition);
        TR_0000:
            this.m_gridBlocks.SelectedIndex = 0;
            this.m_gridBlocksPanel.ScrollbarVPosition = 0f;
        }

        protected void UpdateGridControl()
        {
            this.categories_ItemClicked(this.m_categoriesListbox);
        }

        public class GridItemUserData
        {
            public MyObjectBuilder_ToolbarItem ItemData;
        }

        public enum GroupModes
        {
            Default,
            HideEmpty,
            HideBlockGroups,
            HideAll
        }
    }
}

