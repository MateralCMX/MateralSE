namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.GameSystems;
    using Sandbox.Game.GameSystems.Conveyors;
    using Sandbox.Game.GUI;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Audio;
    using VRage.Collections;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Groups;
    using VRage.Input;
    using VRage.ObjectBuilders;
    using VRage.Utils;
    using VRageMath;

    internal class MyTerminalInventoryController
    {
        private MyGuiControlList m_leftOwnersControl;
        private MyGuiControlRadioButton m_leftSuitButton;
        private MyGuiControlRadioButton m_leftGridButton;
        private MyGuiControlRadioButton m_leftFilterStorageButton;
        private MyGuiControlRadioButton m_leftFilterSystemButton;
        private MyGuiControlRadioButton m_leftFilterEnergyButton;
        private MyGuiControlRadioButton m_leftFilterAllButton;
        private MyGuiControlRadioButtonGroup m_leftTypeGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlRadioButtonGroup m_leftFilterGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlList m_rightOwnersControl;
        private MyGuiControlRadioButton m_rightSuitButton;
        private MyGuiControlRadioButton m_rightGridButton;
        private MyGuiControlRadioButton m_rightFilterShipButton;
        private MyGuiControlRadioButton m_rightFilterStorageButton;
        private MyGuiControlRadioButton m_rightFilterSystemButton;
        private MyGuiControlRadioButton m_rightFilterEnergyButton;
        private MyGuiControlRadioButton m_rightFilterAllButton;
        private MyGuiControlRadioButtonGroup m_rightTypeGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlRadioButtonGroup m_rightFilterGroup = new MyGuiControlRadioButtonGroup();
        private MyGuiControlButton m_throwOutButtonLeft;
        private MyDragAndDropInfo m_dragAndDropInfo;
        private MyGuiControlGridDragAndDrop m_dragAndDrop;
        private List<MyGuiControlGrid> m_controlsDisabledWhileDragged = new List<MyGuiControlGrid>();
        private VRage.Game.Entity.MyEntity m_userAsEntity;
        private VRage.Game.Entity.MyEntity m_interactedAsEntity;
        private VRage.Game.Entity.MyEntity m_openInventoryInteractedAsEntity;
        private VRage.Game.Entity.MyEntity m_userAsOwner;
        private VRage.Game.Entity.MyEntity m_interactedAsOwner;
        private List<VRage.Game.Entity.MyEntity> m_interactedGridOwners = new List<VRage.Game.Entity.MyEntity>();
        private List<VRage.Game.Entity.MyEntity> m_interactedGridOwnersMechanical = new List<VRage.Game.Entity.MyEntity>();
        private List<IMyConveyorEndpoint> m_reachableInventoryOwners = new List<IMyConveyorEndpoint>();
        private List<MyGridConveyorSystem> m_registeredConveyorSystems = new List<MyGridConveyorSystem>();
        private List<MyGridConveyorSystem> m_registeredConveyorMechanicalSystems = new List<MyGridConveyorSystem>();
        private MyGuiControlInventoryOwner m_focusedOwnerControl;
        private MyGuiControlGrid m_focusedGridControl;
        private MyPhysicalInventoryItem? m_selectedInventoryItem;
        private MyInventory m_selectedInventory;
        private bool m_leftShowsGrid;
        private bool m_rightShowsGrid;
        private bool m_filterCurrentShipOnly;
        private MyInventoryOwnerTypeEnum? m_leftFilterType;
        private MyInventoryOwnerTypeEnum? m_rightFilterType;
        private MyGridColorHelper m_colorHelper;
        private MyGuiControlSearchBox m_searchBoxLeft;
        private MyGuiControlSearchBox m_searchBoxRight;
        private static int m_persistentRadioSelectionLeft = 0;
        private static int m_persistentRadioSelectionRight = 0;
        private static readonly Vector2 m_controlListFullSize = new Vector2(0.437f, 0.618f);
        private static readonly Vector2 m_controlListSizeWithSearch = new Vector2(0.437f, 0.569f);
        private static readonly Vector2 m_leftControlListPosition = new Vector2(-0.452f, -0.276f);
        private static readonly Vector2 m_rightControlListPosition = new Vector2(0.4555f, -0.276f);
        private static readonly Vector2 m_leftControlListPosWithSearch = new Vector2(-0.452f, -0.227f);
        private static readonly Vector2 m_rightControlListPosWithSearch = new Vector2(0.4555f, -0.227f);
        private MyGuiControlCheckbox m_hideEmptyLeft;
        private MyGuiControlLabel m_hideEmptyLeftLabel;
        private MyGuiControlCheckbox m_hideEmptyRight;
        private MyGuiControlLabel m_hideEmptyRightLabel;
        private Predicate<IMyConveyorEndpoint> m_endpointPredicate;
        private IMyConveyorEndpointBlock m_interactedEndpointBlock;
        private bool m_selectionDirty;

        public MyTerminalInventoryController()
        {
            this.m_endpointPredicate = new Predicate<IMyConveyorEndpoint>(this.EndpointPredicate);
        }

        private void ApplyTypeGroupSelectionChange(MyGuiControlRadioButtonGroup obj, ref bool showsGrid, MyGuiControlList targetControlList, MyInventoryOwnerTypeEnum? filterType, MyGuiControlRadioButtonGroup filterButtonGroup, MyGuiControlCheckbox showEmpty, MyGuiControlLabel showEmptyLabel, MyGuiControlSearchBox searchBox, bool isLeftControllist)
        {
            MyGuiControlRadioButtonStyleEnum visualStyle = obj.SelectedButton.VisualStyle;
            if (visualStyle != MyGuiControlRadioButtonStyleEnum.FilterCharacter)
            {
                if (visualStyle == MyGuiControlRadioButtonStyleEnum.FilterGrid)
                {
                    showsGrid = true;
                    this.CreateInventoryControlsInList(this.m_filterCurrentShipOnly ? this.m_interactedGridOwnersMechanical : this.m_interactedGridOwners, targetControlList, filterType);
                    showEmpty.Visible = true;
                    showEmptyLabel.Visible = true;
                    searchBox.Visible = true;
                    searchBox.SearchText = searchBox.SearchText;
                    targetControlList.Position = isLeftControllist ? m_leftControlListPosWithSearch : m_rightControlListPosWithSearch;
                    targetControlList.Size = m_controlListSizeWithSearch;
                }
            }
            else
            {
                showsGrid = false;
                showEmpty.Visible = false;
                showEmptyLabel.Visible = false;
                searchBox.Visible = false;
                targetControlList.Position = isLeftControllist ? m_leftControlListPosition : m_rightControlListPosition;
                targetControlList.Size = m_controlListFullSize;
                if (ReferenceEquals(targetControlList, this.m_leftOwnersControl))
                {
                    this.CreateInventoryControlInList(this.m_userAsOwner, targetControlList);
                }
                else
                {
                    this.CreateInventoryControlInList(this.m_interactedAsOwner, targetControlList);
                }
            }
            foreach (MyGuiControlRadioButton local1 in filterButtonGroup)
            {
                local1.Visible = local1.Enabled = showsGrid;
            }
            this.RefreshSelectedInventoryItem();
        }

        private void BlockSearchLeft_TextChanged(string obj)
        {
            MyInventoryOwnerTypeEnum? leftFilterType = this.m_leftFilterType;
            MyInventoryOwnerTypeEnum character = MyInventoryOwnerTypeEnum.Character;
            if (!((((MyInventoryOwnerTypeEnum) leftFilterType.GetValueOrDefault()) == character) & (leftFilterType != null)))
            {
                this.SearchInList(this.m_searchBoxLeft.TextBox, this.m_leftOwnersControl, this.m_hideEmptyLeft.IsChecked);
            }
        }

        private void BlockSearchRight_TextChanged(string obj)
        {
            MyInventoryOwnerTypeEnum? rightFilterType = this.m_rightFilterType;
            MyInventoryOwnerTypeEnum character = MyInventoryOwnerTypeEnum.Character;
            if (!((((MyInventoryOwnerTypeEnum) rightFilterType.GetValueOrDefault()) == character) & (rightFilterType != null)))
            {
                this.SearchInList(this.m_searchBoxRight.TextBox, this.m_rightOwnersControl, this.m_hideEmptyRight.IsChecked);
            }
        }

        private void ClearDisabledControls()
        {
            using (List<MyGuiControlGrid>.Enumerator enumerator = this.m_controlsDisabledWhileDragged.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Enabled = true;
                }
            }
            this.m_controlsDisabledWhileDragged.Clear();
        }

        public void Close()
        {
            foreach (MyGridConveyorSystem local1 in this.m_registeredConveyorSystems)
            {
                local1.BlockAdded -= new Action<MyCubeBlock>(this.ConveyorSystem_BlockAdded);
                local1.BlockRemoved -= new Action<MyCubeBlock>(this.ConveyorSystem_BlockRemoved);
            }
            this.m_registeredConveyorSystems.Clear();
            foreach (MyGridConveyorSystem local2 in this.m_registeredConveyorMechanicalSystems)
            {
                local2.BlockAdded -= new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockAdded);
                local2.BlockRemoved -= new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockRemoved);
            }
            this.m_registeredConveyorMechanicalSystems.Clear();
            this.m_leftTypeGroup.Clear();
            this.m_leftFilterGroup.Clear();
            this.m_rightTypeGroup.Clear();
            this.m_rightFilterGroup.Clear();
            this.m_controlsDisabledWhileDragged.Clear();
            this.m_leftOwnersControl = null;
            this.m_leftSuitButton = null;
            this.m_leftGridButton = null;
            this.m_leftFilterStorageButton = null;
            this.m_leftFilterSystemButton = null;
            this.m_leftFilterEnergyButton = null;
            this.m_leftFilterAllButton = null;
            this.m_rightOwnersControl = null;
            this.m_rightSuitButton = null;
            this.m_rightGridButton = null;
            this.m_rightFilterShipButton = null;
            this.m_rightFilterStorageButton = null;
            this.m_rightFilterSystemButton = null;
            this.m_rightFilterEnergyButton = null;
            this.m_rightFilterAllButton = null;
            this.m_throwOutButtonLeft = null;
            this.m_dragAndDrop = null;
            this.m_dragAndDropInfo = null;
            this.m_focusedOwnerControl = null;
            this.m_focusedGridControl = null;
            this.m_selectedInventory = null;
            this.m_hideEmptyLeft.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_hideEmptyLeft.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.HideEmptyLeft_Checked));
            this.m_hideEmptyRight.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Remove(this.m_hideEmptyRight.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.HideEmptyRight_Checked));
            this.m_searchBoxLeft.OnTextChanged -= new MyGuiControlSearchBox.TextChangedDelegate(this.BlockSearchLeft_TextChanged);
            this.m_searchBoxRight.OnTextChanged -= new MyGuiControlSearchBox.TextChangedDelegate(this.BlockSearchRight_TextChanged);
            this.m_hideEmptyLeft = null;
            this.m_hideEmptyLeftLabel = null;
            this.m_hideEmptyRight = null;
            this.m_hideEmptyRightLabel = null;
            this.m_searchBoxLeft = null;
            this.m_searchBoxRight = null;
        }

        private void ConveyorSystem_BlockAdded(MyCubeBlock obj)
        {
            this.m_interactedGridOwners.Add(obj);
            if (this.m_leftShowsGrid)
            {
                this.LeftTypeGroup_SelectedChanged(this.m_leftTypeGroup);
            }
            if (this.m_rightShowsGrid)
            {
                this.RightTypeGroup_SelectedChanged(this.m_rightTypeGroup);
            }
            if (this.m_dragAndDropInfo != null)
            {
                this.ClearDisabledControls();
                this.DisableInvalidWhileDragging();
            }
        }

        private void ConveyorSystem_BlockRemoved(MyCubeBlock obj)
        {
            this.m_interactedGridOwners.Remove(obj);
            this.UpdateSelection();
            if (this.m_dragAndDropInfo != null)
            {
                this.ClearDisabledControls();
                this.DisableInvalidWhileDragging();
            }
        }

        private void ConveyorSystemMechanical_BlockAdded(MyCubeBlock obj)
        {
            this.m_interactedGridOwnersMechanical.Add(obj);
            if (this.m_leftShowsGrid)
            {
                this.LeftTypeGroup_SelectedChanged(this.m_leftTypeGroup);
            }
            if (this.m_rightShowsGrid)
            {
                this.RightTypeGroup_SelectedChanged(this.m_rightTypeGroup);
            }
            if (this.m_dragAndDropInfo != null)
            {
                this.ClearDisabledControls();
                this.DisableInvalidWhileDragging();
            }
        }

        private void ConveyorSystemMechanical_BlockRemoved(MyCubeBlock obj)
        {
            this.m_interactedGridOwnersMechanical.Remove(obj);
            this.UpdateSelection();
            if (this.m_dragAndDropInfo != null)
            {
                this.ClearDisabledControls();
                this.DisableInvalidWhileDragging();
            }
        }

        private static void CorrectItemAmount(ref MyPhysicalInventoryItem dragItem)
        {
            MyObjectBuilderType typeId = dragItem.Content.TypeId;
        }

        private void CreateInventoryControlInList(VRage.Game.Entity.MyEntity owner, MyGuiControlList listControl)
        {
            List<VRage.Game.Entity.MyEntity> owners = new List<VRage.Game.Entity.MyEntity>();
            if (owner != null)
            {
                owners.Add(owner);
            }
            MyInventoryOwnerTypeEnum? filterType = null;
            this.CreateInventoryControlsInList(owners, listControl, filterType);
        }

        private void CreateInventoryControlsInList(List<VRage.Game.Entity.MyEntity> owners, MyGuiControlList listControl, MyInventoryOwnerTypeEnum? filterType = new MyInventoryOwnerTypeEnum?())
        {
            if (listControl.Controls.Contains(this.m_focusedOwnerControl))
            {
                this.m_focusedOwnerControl = null;
            }
            List<MyGuiControlBase> controls = new List<MyGuiControlBase>();
            MyCubeGrid node = (this.m_interactedAsEntity != null) ? (this.m_interactedAsEntity.Parent as MyCubeGrid) : null;
            if (node != null)
            {
                MyCubeGridGroups.Static.Mechanical.GetGroup(node);
            }
            foreach (VRage.Game.Entity.MyEntity entity in owners)
            {
                if (entity == null)
                {
                    continue;
                }
                if (entity.HasInventory)
                {
                    if (filterType != null)
                    {
                        MyInventoryOwnerTypeEnum? nullable = filterType;
                        if (!((entity.InventoryOwnerType() == ((MyInventoryOwnerTypeEnum) nullable.GetValueOrDefault())) & (nullable != null)))
                        {
                            continue;
                        }
                    }
                    VRageMath.Vector4 labelColorMask = Color.White.ToVector4();
                    if (entity is MyCubeBlock)
                    {
                        labelColorMask = this.m_colorHelper.GetGridColor((entity as MyCubeBlock).CubeGrid).ToVector4();
                    }
                    MyGuiControlInventoryOwner item = new MyGuiControlInventoryOwner(entity, labelColorMask);
                    item.Size = new Vector2(listControl.Size.X - 0.045f, item.Size.Y);
                    item.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
                    foreach (MyGuiControlGrid local1 in item.ContentGrids)
                    {
                        local1.ItemSelected += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemSelected);
                        local1.ItemDragged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemDragged);
                        local1.ItemDoubleClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemDoubleClicked);
                        local1.ItemClicked += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.grid_ItemClicked);
                    }
                    item.SizeChanged += new Action<MyGuiControlBase>(this.inventoryControl_SizeChanged);
                    item.InventoryContentsChanged += new Action<MyGuiControlInventoryOwner>(this.ownerControl_InventoryContentsChanged);
                    if (entity is MyCubeBlock)
                    {
                        item.Enabled = (entity as MyCubeBlock).IsFunctional;
                    }
                    if (ReferenceEquals(entity, this.m_interactedAsOwner) || ReferenceEquals(entity, this.m_userAsOwner))
                    {
                        controls.Insert(0, item);
                    }
                    else
                    {
                        controls.Add(item);
                    }
                }
            }
            listControl.InitControls(controls);
        }

        private void DisableInvalidWhileDragging()
        {
            MyPhysicalInventoryItem userData = (MyPhysicalInventoryItem) this.m_dragAndDropInfo.Grid.GetItemAt(this.m_dragAndDropInfo.ItemIndex).UserData;
            MyInventory srcInventory = (MyInventory) this.m_dragAndDropInfo.Grid.UserData;
            this.DisableUnacceptingInventoryControls(userData, this.m_leftOwnersControl);
            this.DisableUnacceptingInventoryControls(userData, this.m_rightOwnersControl);
            this.DisableUnreachableInventoryControls(srcInventory, userData, this.m_leftOwnersControl);
            this.DisableUnreachableInventoryControls(srcInventory, userData, this.m_rightOwnersControl);
        }

        private void DisableUnacceptingInventoryControls(MyPhysicalInventoryItem item, MyGuiControlList list)
        {
            foreach (MyGuiControlBase base2 in list.Controls.GetVisibleControls())
            {
                if (base2.Enabled)
                {
                    MyGuiControlInventoryOwner owner = (MyGuiControlInventoryOwner) base2;
                    VRage.Game.Entity.MyEntity inventoryOwner = owner.InventoryOwner;
                    for (int i = 0; i < inventoryOwner.InventoryCount; i++)
                    {
                        if (!inventoryOwner.GetInventory(i).CanItemsBeAdded(0, item.Content.GetId()))
                        {
                            owner.ContentGrids[i].Enabled = false;
                            this.m_controlsDisabledWhileDragged.Add(owner.ContentGrids[i]);
                        }
                    }
                }
            }
        }

        private void DisableUnreachableInventoryControls(MyInventory srcInventory, MyPhysicalInventoryItem item, MyGuiControlList list)
        {
            bool flag = ReferenceEquals(srcInventory.Owner, this.m_userAsOwner);
            bool flag2 = ReferenceEquals(srcInventory.Owner, this.m_interactedAsOwner);
            VRage.Game.Entity.MyEntity objB = srcInventory.Owner;
            IMyConveyorEndpointBlock interactedAsEntity = null;
            if (flag)
            {
                interactedAsEntity = this.m_interactedAsEntity as IMyConveyorEndpointBlock;
            }
            else if (objB != null)
            {
                interactedAsEntity = objB as IMyConveyorEndpointBlock;
            }
            IMyConveyorEndpointBlock interactedAsEntity = null;
            if (this.m_interactedAsEntity != null)
            {
                interactedAsEntity = this.m_interactedAsEntity as IMyConveyorEndpointBlock;
            }
            if (interactedAsEntity != null)
            {
                long localPlayerId = MySession.Static.LocalPlayerId;
                this.m_interactedEndpointBlock = interactedAsEntity;
                MyGridConveyorSystem.AppendReachableEndpoints(interactedAsEntity.ConveyorEndpoint, localPlayerId, this.m_reachableInventoryOwners, item, this.m_endpointPredicate);
            }
            foreach (MyGuiControlBase base2 in list.Controls.GetVisibleControls())
            {
                if (base2.Enabled)
                {
                    bool flag1;
                    MyGuiControlInventoryOwner owner = (MyGuiControlInventoryOwner) base2;
                    VRage.Game.Entity.MyEntity inventoryOwner = owner.InventoryOwner;
                    IMyConveyorEndpoint conveyorEndpoint = null;
                    IMyConveyorEndpointBlock block3 = inventoryOwner as IMyConveyorEndpointBlock;
                    if (block3 != null)
                    {
                        conveyorEndpoint = block3.ConveyorEndpoint;
                    }
                    if (!flag || !ReferenceEquals(inventoryOwner, this.m_interactedAsOwner))
                    {
                        flag1 = flag2 && ReferenceEquals(inventoryOwner, this.m_userAsOwner);
                    }
                    else
                    {
                        flag1 = true;
                    }
                    bool flag3 = flag1;
                    bool flag4 = !this.m_reachableInventoryOwners.Contains(conveyorEndpoint);
                    bool flag5 = (interactedAsEntity != null) && this.m_reachableInventoryOwners.Contains(interactedAsEntity.ConveyorEndpoint);
                    bool flag6 = ReferenceEquals(inventoryOwner, this.m_userAsOwner) & flag5;
                    if (((!ReferenceEquals(inventoryOwner, objB) && !flag3) & flag4) && !flag6)
                    {
                        for (int i = 0; i < inventoryOwner.InventoryCount; i++)
                        {
                            if (owner.ContentGrids[i].Enabled)
                            {
                                owner.ContentGrids[i].Enabled = false;
                                this.m_controlsDisabledWhileDragged.Add(owner.ContentGrids[i]);
                            }
                        }
                    }
                }
            }
            this.m_reachableInventoryOwners.Clear();
        }

        private void dragDrop_OnItemDropped(object sender, MyDragAndDropEventArgs eventArgs)
        {
            if (eventArgs.DropTo == null)
            {
                if (((MyGuiControlGridDragAndDrop) sender).IsEmptySpace() && this.m_throwOutButtonLeft.Enabled)
                {
                    this.throwOutButton_OnButtonClick(this.m_throwOutButtonLeft);
                }
            }
            else
            {
                MyFixedPoint? nullable;
                MyGuiAudio.PlaySound(MyGuiSounds.HudItem);
                MyPhysicalInventoryItem inventoryItem = (MyPhysicalInventoryItem) eventArgs.Item.UserData;
                MyGuiControlGrid objA = eventArgs.DragFrom.Grid;
                MyGuiControlGrid dstGrid = eventArgs.DropTo.Grid;
                MyGuiControlInventoryOwner owner = (MyGuiControlInventoryOwner) objA.Owner;
                if (!(dstGrid.Owner is MyGuiControlInventoryOwner))
                {
                    return;
                }
                MyInventory srcInventory = (MyInventory) objA.UserData;
                MyInventory dstInventory = (MyInventory) dstGrid.UserData;
                if (!ReferenceEquals(objA, dstGrid))
                {
                    if (eventArgs.DragButton == MySharedButtonsEnum.Secondary)
                    {
                        this.ShowAmountTransferDialog(inventoryItem, delegate (float amount) {
                            if ((amount != 0f) && srcInventory.IsItemAt(eventArgs.DragFrom.ItemIndex))
                            {
                                inventoryItem.Amount = (MyFixedPoint) amount;
                                CorrectItemAmount(ref inventoryItem);
                                MyInventory.TransferByUser(srcInventory, dstInventory, inventoryItem.ItemId, eventArgs.DropTo.ItemIndex, new MyFixedPoint?(inventoryItem.Amount));
                                this.RefreshSelectedInventoryItem();
                            }
                        });
                    }
                    else
                    {
                        nullable = null;
                        MyInventory.TransferByUser(srcInventory, dstInventory, inventoryItem.ItemId, eventArgs.DropTo.ItemIndex, nullable);
                        this.RefreshSelectedInventoryItem();
                    }
                }
                else if (eventArgs.DragButton == MySharedButtonsEnum.Secondary)
                {
                    this.ShowAmountTransferDialog(inventoryItem, delegate (float amount) {
                        if ((amount != 0f) && srcInventory.IsItemAt(eventArgs.DragFrom.ItemIndex))
                        {
                            inventoryItem.Amount = (MyFixedPoint) amount;
                            CorrectItemAmount(ref inventoryItem);
                            MyInventory.TransferByUser(srcInventory, srcInventory, inventoryItem.ItemId, eventArgs.DropTo.ItemIndex, new MyFixedPoint?(inventoryItem.Amount));
                            if (dstGrid.IsValidIndex(eventArgs.DropTo.ItemIndex))
                            {
                                dstGrid.SelectedIndex = new int?(eventArgs.DropTo.ItemIndex);
                            }
                            else
                            {
                                dstGrid.SelectLastItem();
                            }
                            this.RefreshSelectedInventoryItem();
                        }
                    });
                }
                else
                {
                    nullable = null;
                    MyInventory.TransferByUser(srcInventory, srcInventory, inventoryItem.ItemId, eventArgs.DropTo.ItemIndex, nullable);
                    if (dstGrid.IsValidIndex(eventArgs.DropTo.ItemIndex))
                    {
                        dstGrid.SelectedIndex = new int?(eventArgs.DropTo.ItemIndex);
                    }
                    else
                    {
                        dstGrid.SelectLastItem();
                    }
                    this.RefreshSelectedInventoryItem();
                }
            }
            this.ClearDisabledControls();
            this.m_dragAndDropInfo = null;
        }

        private bool EndpointPredicate(IMyConveyorEndpoint endpoint)
        {
            if ((endpoint.CubeBlock == null) || !endpoint.CubeBlock.HasInventory)
            {
                return ReferenceEquals(endpoint.CubeBlock, this.m_interactedEndpointBlock);
            }
            return true;
        }

        private void GetGridInventories(MyCubeGrid grid, List<VRage.Game.Entity.MyEntity> outputInventories)
        {
            if (grid != null)
            {
                foreach (MyCubeBlock block in grid.GridSystems.ConveyorSystem.InventoryBlocks)
                {
                    if ((!(block is MyTerminalBlock) || (block as MyTerminalBlock).HasLocalPlayerAccess()) && ((ReferenceEquals(this.m_interactedAsEntity, block) || !(block is MyTerminalBlock)) || (block as MyTerminalBlock).ShowInInventory))
                    {
                        outputInventories.Add(block);
                    }
                }
            }
        }

        private MyCubeGrid GetInteractedGrid() => 
            ((this.m_interactedAsEntity != null) ? (this.m_interactedAsEntity.Parent as MyCubeGrid) : null);

        private unsafe void grid_ItemClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            bool flag = MyInput.Static.IsAnyCtrlKeyPressed();
            bool flag2 = MyInput.Static.IsAnyShiftKeyPressed();
            if (flag | flag2)
            {
                MyPhysicalInventoryItem userData = (MyPhysicalInventoryItem) sender.GetItemAt(eventArgs.ItemIndex).UserData;
                MyPhysicalInventoryItem* itemPtr1 = (MyPhysicalInventoryItem*) ref userData;
                itemPtr1->Amount = MyFixedPoint.Min((flag2 ? 100 : 1) * (flag ? 10 : 1), userData.Amount);
                this.TransferToOppositeFirst(userData);
                this.RefreshSelectedInventoryItem();
            }
        }

        private void grid_ItemDoubleClicked(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (!MyInput.Static.IsAnyShiftKeyPressed() && !MyInput.Static.IsAnyCtrlKeyPressed())
            {
                MyPhysicalInventoryItem userData = (MyPhysicalInventoryItem) sender.GetItemAt(eventArgs.ItemIndex).UserData;
                this.TransferToOppositeFirst(userData);
                this.RefreshSelectedInventoryItem();
            }
        }

        private void grid_ItemDragged(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (!MyInput.Static.IsAnyShiftKeyPressed() && !MyInput.Static.IsAnyCtrlKeyPressed())
            {
                this.StartDragging(MyDropHandleType.MouseRelease, sender, ref eventArgs);
            }
        }

        private void grid_ItemSelected(MyGuiControlGrid sender, MyGuiControlGrid.EventArgs eventArgs)
        {
            MyGuiControlGrid objB = sender;
            if ((this.m_focusedGridControl != null) && !ReferenceEquals(this.m_focusedGridControl, objB))
            {
                this.m_focusedGridControl.SelectedIndex = null;
            }
            this.m_focusedGridControl = objB;
            this.m_focusedOwnerControl = (MyGuiControlInventoryOwner) objB.Owner;
            this.RefreshSelectedInventoryItem();
        }

        private void HideEmptyLeft_Checked(MyGuiControlCheckbox obj)
        {
            MyInventoryOwnerTypeEnum? leftFilterType = this.m_leftFilterType;
            MyInventoryOwnerTypeEnum character = MyInventoryOwnerTypeEnum.Character;
            if (!((((MyInventoryOwnerTypeEnum) leftFilterType.GetValueOrDefault()) == character) & (leftFilterType != null)))
            {
                this.SearchInList(this.m_searchBoxLeft.TextBox, this.m_leftOwnersControl, obj.IsChecked);
            }
        }

        private void HideEmptyRight_Checked(MyGuiControlCheckbox obj)
        {
            MyInventoryOwnerTypeEnum? rightFilterType = this.m_rightFilterType;
            MyInventoryOwnerTypeEnum character = MyInventoryOwnerTypeEnum.Character;
            if (!((((MyInventoryOwnerTypeEnum) rightFilterType.GetValueOrDefault()) == character) & (rightFilterType != null)))
            {
                this.SearchInList(this.m_searchBoxRight.TextBox, this.m_rightOwnersControl, obj.IsChecked);
            }
        }

        public void Init(IMyGuiControlsParent controlsParent, VRage.Game.Entity.MyEntity thisEntity, VRage.Game.Entity.MyEntity interactedEntity, MyGridColorHelper colorHelper)
        {
            VRage.Game.Entity.MyEntity userAsEntity;
            VRage.Game.Entity.MyEntity interactedAsEntity;
            this.m_userAsEntity = thisEntity;
            this.m_interactedAsEntity = interactedEntity;
            this.m_colorHelper = colorHelper;
            this.m_leftOwnersControl = (MyGuiControlList) controlsParent.Controls.GetControlByName("LeftInventory");
            this.m_rightOwnersControl = (MyGuiControlList) controlsParent.Controls.GetControlByName("RightInventory");
            this.m_leftSuitButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftSuitButton");
            this.m_leftGridButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftGridButton");
            this.m_leftFilterStorageButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftFilterStorageButton");
            this.m_leftFilterSystemButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftFilterSystemButton");
            this.m_leftFilterEnergyButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftFilterEnergyButton");
            this.m_leftFilterAllButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("LeftFilterAllButton");
            this.m_rightSuitButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightSuitButton");
            this.m_rightGridButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightGridButton");
            this.m_rightFilterShipButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightFilterShipButton");
            this.m_rightFilterStorageButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightFilterStorageButton");
            this.m_rightFilterSystemButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightFilterSystemButton");
            this.m_rightFilterEnergyButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightFilterEnergyButton");
            this.m_rightFilterAllButton = (MyGuiControlRadioButton) controlsParent.Controls.GetControlByName("RightFilterAllButton");
            this.m_throwOutButtonLeft = (MyGuiControlButton) controlsParent.Controls.GetControlByName("ThrowOutButtonLeft");
            this.m_hideEmptyLeft = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("CheckboxHideEmptyLeft");
            this.m_hideEmptyLeftLabel = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("LabelHideEmptyLeft");
            this.m_hideEmptyRight = (MyGuiControlCheckbox) controlsParent.Controls.GetControlByName("CheckboxHideEmptyRight");
            this.m_hideEmptyRightLabel = (MyGuiControlLabel) controlsParent.Controls.GetControlByName("LabelHideEmptyRight");
            this.m_searchBoxLeft = (MyGuiControlSearchBox) controlsParent.Controls.GetControlByName("BlockSearchLeft");
            this.m_searchBoxRight = (MyGuiControlSearchBox) controlsParent.Controls.GetControlByName("BlockSearchRight");
            this.m_hideEmptyLeft.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_HideEmpty);
            this.m_hideEmptyRight.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_HideEmpty);
            this.m_hideEmptyLeft.Visible = false;
            this.m_hideEmptyLeftLabel.Visible = false;
            this.m_hideEmptyRight.Visible = true;
            this.m_hideEmptyRightLabel.Visible = true;
            this.m_searchBoxLeft.Visible = false;
            this.m_searchBoxRight.Visible = false;
            this.m_hideEmptyLeft.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_hideEmptyLeft.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.HideEmptyLeft_Checked));
            this.m_hideEmptyRight.IsCheckedChanged = (Action<MyGuiControlCheckbox>) Delegate.Combine(this.m_hideEmptyRight.IsCheckedChanged, new Action<MyGuiControlCheckbox>(this.HideEmptyRight_Checked));
            this.m_searchBoxLeft.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.BlockSearchLeft_TextChanged);
            this.m_searchBoxRight.OnTextChanged += new MyGuiControlSearchBox.TextChangedDelegate(this.BlockSearchRight_TextChanged);
            this.m_leftSuitButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowCharacter);
            this.m_leftGridButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowConnected);
            this.m_leftGridButton.ShowTooltipWhenDisabled = true;
            this.m_rightSuitButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowInteracted);
            this.m_rightGridButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowConnected);
            this.m_rightGridButton.ShowTooltipWhenDisabled = true;
            this.m_leftFilterAllButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterAll);
            this.m_leftFilterEnergyButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterEnergy);
            this.m_leftFilterStorageButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterStorage);
            this.m_leftFilterSystemButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterSystem);
            this.m_rightFilterAllButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterAll);
            this.m_rightFilterEnergyButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterEnergy);
            this.m_rightFilterShipButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterShip);
            this.m_rightFilterStorageButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterStorage);
            this.m_rightFilterSystemButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_FilterSystem);
            this.m_throwOutButtonLeft.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ThrowOut);
            this.m_throwOutButtonLeft.ShowTooltipWhenDisabled = true;
            this.m_throwOutButtonLeft.CueEnum = GuiSounds.None;
            this.m_leftTypeGroup.Add(this.m_leftSuitButton);
            this.m_leftTypeGroup.Add(this.m_leftGridButton);
            this.m_rightTypeGroup.Add(this.m_rightSuitButton);
            this.m_rightTypeGroup.Add(this.m_rightGridButton);
            this.m_leftFilterGroup.Add(this.m_leftFilterAllButton);
            this.m_leftFilterGroup.Add(this.m_leftFilterEnergyButton);
            this.m_leftFilterGroup.Add(this.m_leftFilterStorageButton);
            this.m_leftFilterGroup.Add(this.m_leftFilterSystemButton);
            this.m_rightFilterGroup.Add(this.m_rightFilterAllButton);
            this.m_rightFilterGroup.Add(this.m_rightFilterEnergyButton);
            this.m_rightFilterGroup.Add(this.m_rightFilterShipButton);
            this.m_rightFilterGroup.Add(this.m_rightFilterStorageButton);
            this.m_rightFilterGroup.Add(this.m_rightFilterSystemButton);
            this.m_throwOutButtonLeft.DrawCrossTextureWhenDisabled = false;
            this.m_dragAndDrop = new MyGuiControlGridDragAndDrop(MyGuiConstants.DRAG_AND_DROP_BACKGROUND_COLOR, MyGuiConstants.DRAG_AND_DROP_TEXT_COLOR, 0.7f, MyGuiConstants.DRAG_AND_DROP_TEXT_OFFSET, true);
            controlsParent.Controls.Add(this.m_dragAndDrop);
            this.m_dragAndDrop.DrawBackgroundTexture = false;
            this.m_throwOutButtonLeft.ButtonClicked += new Action<MyGuiControlButton>(this.throwOutButton_OnButtonClick);
            this.m_dragAndDrop.ItemDropped += new OnItemDropped(this.dragDrop_OnItemDropped);
            if ((this.m_userAsEntity == null) || !this.m_userAsEntity.HasInventory)
            {
                userAsEntity = null;
            }
            else
            {
                userAsEntity = this.m_userAsEntity;
            }
            VRage.Game.Entity.MyEntity entity = userAsEntity;
            if (entity != null)
            {
                this.m_userAsOwner = entity;
            }
            if ((this.m_interactedAsEntity == null) || !this.m_interactedAsEntity.HasInventory)
            {
                interactedAsEntity = null;
            }
            else
            {
                interactedAsEntity = this.m_interactedAsEntity;
            }
            VRage.Game.Entity.MyEntity entity2 = interactedAsEntity;
            if (entity2 != null)
            {
                this.m_interactedAsOwner = entity2;
            }
            MyCubeGrid grid = (this.m_interactedAsEntity != null) ? (this.m_interactedAsEntity.Parent as MyCubeGrid) : null;
            this.m_interactedGridOwners.Clear();
            if (grid != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in MyCubeGridGroups.Static.Logical.GetGroup(grid).Nodes)
                {
                    this.GetGridInventories(node.NodeData, this.m_interactedGridOwners);
                    node.NodeData.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystem_BlockAdded);
                    node.NodeData.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystem_BlockRemoved);
                    this.m_registeredConveyorSystems.Add(node.NodeData.GridSystems.ConveyorSystem);
                }
            }
            this.m_interactedGridOwnersMechanical.Clear();
            if (grid != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node node2 in MyCubeGridGroups.Static.Mechanical.GetGroup(grid).Nodes)
                {
                    this.GetGridInventories(node2.NodeData, this.m_interactedGridOwnersMechanical);
                    node2.NodeData.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockAdded);
                    node2.NodeData.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockRemoved);
                    this.m_registeredConveyorMechanicalSystems.Add(node2.NodeData.GridSystems.ConveyorSystem);
                }
            }
            if ((this.m_interactedAsEntity is MyCharacter) || (this.m_interactedAsEntity is MyInventoryBagEntity))
            {
                m_persistentRadioSelectionRight = 0;
            }
            this.m_leftTypeGroup.SelectedIndex = new int?(m_persistentRadioSelectionLeft);
            this.m_rightTypeGroup.SelectedIndex = new int?(m_persistentRadioSelectionRight);
            this.m_leftFilterGroup.SelectedIndex = 0;
            this.m_rightFilterGroup.SelectedIndex = 0;
            this.LeftTypeGroup_SelectedChanged(this.m_leftTypeGroup);
            this.RightTypeGroup_SelectedChanged(this.m_rightTypeGroup);
            MyInventoryOwnerTypeEnum? filterType = null;
            this.SetLeftFilter(filterType);
            filterType = null;
            this.SetRightFilter(filterType);
            this.m_leftTypeGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.LeftTypeGroup_SelectedChanged);
            this.m_rightTypeGroup.SelectedChanged += new Action<MyGuiControlRadioButtonGroup>(this.RightTypeGroup_SelectedChanged);
            this.m_leftFilterAllButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    MyInventoryOwnerTypeEnum? nullable = null;
                    this.SetLeftFilter(nullable);
                }
            };
            this.m_leftFilterEnergyButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetLeftFilter(2);
                }
            };
            this.m_leftFilterStorageButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetLeftFilter(1);
                }
            };
            this.m_leftFilterSystemButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetLeftFilter(3);
                }
            };
            this.m_rightFilterAllButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    MyInventoryOwnerTypeEnum? nullable = null;
                    this.SetRightFilter(nullable);
                }
            };
            this.m_rightFilterEnergyButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetRightFilter(2);
                }
            };
            this.m_rightFilterStorageButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetRightFilter(1);
                }
            };
            this.m_rightFilterSystemButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.SetRightFilter(3);
                }
            };
            this.m_rightFilterShipButton.SelectedChanged += delegate (MyGuiControlRadioButton button) {
                if (button.Selected)
                {
                    this.m_filterCurrentShipOnly = true;
                    MyInventoryOwnerTypeEnum? nullable = null;
                    this.SetRightFilter(nullable);
                }
            };
            if (this.m_interactedAsEntity == null)
            {
                this.m_leftGridButton.Enabled = false;
                this.m_leftGridButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowConnectedDisabled);
                this.m_rightGridButton.Enabled = false;
                this.m_rightGridButton.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ShowConnectedDisabled);
                this.m_rightTypeGroup.SelectedIndex = 0;
            }
            this.RefreshSelectedInventoryItem();
        }

        private void interactedObjectButton_OnButtonClick(MyGuiControlButton sender)
        {
            this.CreateInventoryControlInList(this.m_interactedAsOwner, this.m_rightOwnersControl);
        }

        private void inventoryControl_SizeChanged(MyGuiControlBase obj)
        {
            ((MyGuiControlList) obj.Owner).Recalculate();
        }

        private void LeftTypeGroup_SelectedChanged(MyGuiControlRadioButtonGroup obj)
        {
            this.ApplyTypeGroupSelectionChange(obj, ref this.m_leftShowsGrid, this.m_leftOwnersControl, this.m_leftFilterType, this.m_leftFilterGroup, this.m_hideEmptyLeft, this.m_hideEmptyLeftLabel, this.m_searchBoxLeft, true);
            this.m_leftOwnersControl.SetScrollBarPage(0f);
            if (obj.SelectedIndex != null)
            {
                m_persistentRadioSelectionLeft = obj.SelectedIndex.Value;
            }
        }

        private void ownerControl_InventoryContentsChanged(MyGuiControlInventoryOwner control)
        {
            if (ReferenceEquals(control, this.m_focusedOwnerControl))
            {
                this.RefreshSelectedInventoryItem();
            }
            this.UpdateDisabledControlsWhileDragging(control);
        }

        public void Refresh()
        {
            MyCubeGrid grid = (this.m_interactedAsEntity != null) ? (this.m_interactedAsEntity.Parent as MyCubeGrid) : null;
            this.m_interactedGridOwners.Clear();
            if (grid != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridLogicalGroupData>.Node node in MyCubeGridGroups.Static.Logical.GetGroup(grid).Nodes)
                {
                    this.GetGridInventories(node.NodeData, this.m_interactedGridOwners);
                    node.NodeData.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystem_BlockAdded);
                    node.NodeData.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystem_BlockRemoved);
                    this.m_registeredConveyorSystems.Add(node.NodeData.GridSystems.ConveyorSystem);
                }
            }
            this.m_interactedGridOwnersMechanical.Clear();
            if (grid != null)
            {
                foreach (MyGroups<MyCubeGrid, MyGridMechanicalGroupData>.Node node2 in MyCubeGridGroups.Static.Mechanical.GetGroup(grid).Nodes)
                {
                    this.GetGridInventories(node2.NodeData, this.m_interactedGridOwnersMechanical);
                    node2.NodeData.GridSystems.ConveyorSystem.BlockAdded += new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockAdded);
                    node2.NodeData.GridSystems.ConveyorSystem.BlockRemoved += new Action<MyCubeBlock>(this.ConveyorSystemMechanical_BlockRemoved);
                    this.m_registeredConveyorMechanicalSystems.Add(node2.NodeData.GridSystems.ConveyorSystem);
                }
            }
            this.m_leftTypeGroup.SelectedIndex = new int?(m_persistentRadioSelectionLeft);
            this.m_rightTypeGroup.SelectedIndex = new int?(m_persistentRadioSelectionRight);
            this.m_leftFilterGroup.SelectedIndex = 0;
            this.m_rightFilterGroup.SelectedIndex = 0;
            this.LeftTypeGroup_SelectedChanged(this.m_leftTypeGroup);
            this.RightTypeGroup_SelectedChanged(this.m_rightTypeGroup);
            this.SetLeftFilter(this.m_leftFilterType);
            this.SetRightFilter(this.m_rightFilterType);
        }

        private void RefreshSelectedInventoryItem()
        {
            if (this.m_focusedGridControl == null)
            {
                this.m_selectedInventory = null;
                this.m_selectedInventoryItem = null;
            }
            else
            {
                MyPhysicalInventoryItem? userData;
                this.m_selectedInventory = (MyInventory) this.m_focusedGridControl.UserData;
                MyGuiGridItem selectedItem = this.m_focusedGridControl.SelectedItem;
                if (selectedItem != null)
                {
                    userData = (MyPhysicalInventoryItem?) selectedItem.UserData;
                }
                else
                {
                    userData = null;
                }
                this.m_selectedInventoryItem = userData;
            }
            if (this.m_throwOutButtonLeft != null)
            {
                this.m_throwOutButtonLeft.Enabled = (this.m_selectedInventoryItem != null) && ((this.m_focusedOwnerControl != null) && ReferenceEquals(this.m_focusedOwnerControl.InventoryOwner, this.m_userAsOwner));
                if (this.m_throwOutButtonLeft.Enabled)
                {
                    this.m_throwOutButtonLeft.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ThrowOut);
                }
                else
                {
                    this.m_throwOutButtonLeft.SetToolTip(MySpaceTexts.ToolTipTerminalInventory_ThrowOutDisabled);
                }
            }
        }

        private void RightTypeGroup_SelectedChanged(MyGuiControlRadioButtonGroup obj)
        {
            this.ApplyTypeGroupSelectionChange(obj, ref this.m_rightShowsGrid, this.m_rightOwnersControl, this.m_rightFilterType, this.m_rightFilterGroup, this.m_hideEmptyRight, this.m_hideEmptyRightLabel, this.m_searchBoxRight, false);
            this.m_rightOwnersControl.SetScrollBarPage(0f);
            if (obj.SelectedIndex != null)
            {
                m_persistentRadioSelectionRight = obj.SelectedIndex.Value;
            }
        }

        private void SearchInList(MyGuiControlTextbox searchText, MyGuiControlList list, bool hideEmpty)
        {
            if (searchText.Text != "")
            {
                char[] separator = new char[] { ' ' };
                string[] strArray = searchText.Text.ToLower().Split(separator);
                using (ObservableCollection<MyGuiControlBase>.Enumerator enumerator = list.Controls.GetEnumerator())
                {
                    MyGuiControlBase current;
                    VRage.Game.Entity.MyEntity inventoryOwner;
                    bool flag;
                    bool flag2;
                    string[] strArray2;
                    int num;
                    int num2;
                    goto TR_0033;
                TR_0016:
                    if (!flag)
                    {
                        current.Visible = false;
                    }
                    else
                    {
                        int index = 0;
                        while (true)
                        {
                            if (index < inventoryOwner.InventoryCount)
                            {
                                if (inventoryOwner.GetInventory(index).CurrentMass == 0)
                                {
                                    index++;
                                    continue;
                                }
                                flag2 = false;
                            }
                            current.Visible = !(hideEmpty & flag2);
                            break;
                        }
                    }
                    goto TR_0033;
                TR_0018:
                    if (!flag)
                    {
                        num2++;
                    }
                    else
                    {
                        goto TR_0016;
                    }
                TR_0028:
                    while (true)
                    {
                        if (num2 < inventoryOwner.InventoryCount)
                        {
                            using (List<MyPhysicalInventoryItem>.Enumerator enumerator2 = inventoryOwner.GetInventory(num2).GetItems().GetEnumerator())
                            {
                                while (true)
                                {
                                    while (true)
                                    {
                                        if (enumerator2.MoveNext())
                                        {
                                            MyPhysicalInventoryItem current = enumerator2.Current;
                                            bool flag3 = true;
                                            string str3 = MyDefinitionManager.Static.GetPhysicalItemDefinition(current.Content).DisplayNameText.ToString().ToLower();
                                            strArray2 = strArray;
                                            num = 0;
                                            while (true)
                                            {
                                                if (num < strArray2.Length)
                                                {
                                                    string str4 = strArray2[num];
                                                    if (str3.Contains(str4))
                                                    {
                                                        num++;
                                                        continue;
                                                    }
                                                    flag3 = false;
                                                }
                                                if (!flag3)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    flag = true;
                                                }
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            goto TR_0016;
                        }
                        break;
                    }
                    goto TR_0018;
                TR_0033:
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            current = enumerator.Current;
                            inventoryOwner = (current as MyGuiControlInventoryOwner).InventoryOwner;
                            string str = inventoryOwner.DisplayNameText.ToString().ToLower();
                            flag = true;
                            flag2 = true;
                            strArray2 = strArray;
                            num = 0;
                            while (true)
                            {
                                if (num < strArray2.Length)
                                {
                                    string str2 = strArray2[num];
                                    if (str.Contains(str2))
                                    {
                                        num++;
                                        continue;
                                    }
                                    flag = false;
                                }
                                if (flag)
                                {
                                    goto TR_0016;
                                }
                                else
                                {
                                    num2 = 0;
                                }
                                break;
                            }
                        }
                        else
                        {
                            goto TR_0000;
                        }
                        break;
                    }
                    goto TR_0028;
                }
            }
            foreach (MyGuiControlBase base3 in list.Controls)
            {
                bool flag4 = true;
                VRage.Game.Entity.MyEntity inventoryOwner = (base3 as MyGuiControlInventoryOwner).InventoryOwner;
                int index = 0;
                while (true)
                {
                    if (index < inventoryOwner.InventoryCount)
                    {
                        if (inventoryOwner.GetInventory(index).CurrentMass == 0)
                        {
                            index++;
                            continue;
                        }
                        flag4 = false;
                    }
                    base3.Visible = !(hideEmpty & flag4);
                    break;
                }
            }
        TR_0000:
            list.SetScrollBarPage(0f);
        }

        private void SetLeftFilter(MyInventoryOwnerTypeEnum? filterType)
        {
            this.m_leftFilterType = filterType;
            if (this.m_leftShowsGrid)
            {
                this.CreateInventoryControlsInList(this.m_interactedGridOwners, this.m_leftOwnersControl, this.m_leftFilterType);
                this.m_searchBoxLeft.SearchText = this.m_searchBoxLeft.SearchText;
            }
            this.RefreshSelectedInventoryItem();
        }

        private void SetRightFilter(MyInventoryOwnerTypeEnum? filterType)
        {
            this.m_rightFilterType = filterType;
            if (this.m_rightFilterType != null)
            {
                this.m_filterCurrentShipOnly = false;
            }
            if (this.m_rightShowsGrid)
            {
                this.CreateInventoryControlsInList(this.m_filterCurrentShipOnly ? this.m_interactedGridOwnersMechanical : this.m_interactedGridOwners, this.m_rightOwnersControl, this.m_rightFilterType);
                this.m_searchBoxRight.SearchText = this.m_searchBoxRight.SearchText;
            }
            this.RefreshSelectedInventoryItem();
        }

        public void SetSearch(string text, bool interactedSide = true)
        {
            MyInventoryOwnerTypeEnum? nullable;
            MyGuiControlSearchBox box = interactedSide ? this.m_searchBoxRight : this.m_searchBoxLeft;
            if (box != null)
            {
                box.SearchText = text;
            }
            if (interactedSide)
            {
                nullable = null;
                this.SetRightFilter(nullable);
            }
            else
            {
                nullable = null;
                this.SetLeftFilter(nullable);
            }
        }

        private void ShowAmountTransferDialog(MyPhysicalInventoryItem inventoryItem, Action<float> onConfirmed)
        {
            MyFixedPoint amount = inventoryItem.Amount;
            MyObjectBuilderType typeId = inventoryItem.Content.TypeId;
            int minMaxDecimalDigits = 0;
            bool parseAsInteger = true;
            if ((typeId == typeof(MyObjectBuilder_Ore)) || (typeId == typeof(MyObjectBuilder_Ingot)))
            {
                minMaxDecimalDigits = 2;
                parseAsInteger = false;
            }
            float? defaultAmount = null;
            MyGuiScreenDialogAmount dialog = new MyGuiScreenDialogAmount(0f, (float) amount, MyCommonTexts.DialogAmount_AddAmountCaption, minMaxDecimalDigits, parseAsInteger, defaultAmount, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity);
            dialog.OnConfirmed += onConfirmed;
            if (this.m_interactedAsEntity != null)
            {
                Action<VRage.Game.Entity.MyEntity> entityCloseAction = null;
                entityCloseAction = entity => dialog.CloseScreen();
                this.m_interactedAsEntity.OnClose += entityCloseAction;
                dialog.Closed += delegate (MyGuiScreenBase source) {
                    this.m_interactedAsEntity.OnClose -= entityCloseAction;
                };
            }
            MyGuiSandbox.AddScreen(dialog);
        }

        private void StartDragging(MyDropHandleType dropHandlingType, MyGuiControlGrid gridControl, ref MyGuiControlGrid.EventArgs args)
        {
            this.m_dragAndDropInfo = new MyDragAndDropInfo();
            this.m_dragAndDropInfo.Grid = gridControl;
            this.m_dragAndDropInfo.ItemIndex = args.ItemIndex;
            this.DisableInvalidWhileDragging();
            MyGuiGridItem itemAt = this.m_dragAndDropInfo.Grid.GetItemAt(this.m_dragAndDropInfo.ItemIndex);
            this.m_dragAndDrop.StartDragging(dropHandlingType, args.Button, itemAt, this.m_dragAndDropInfo, false);
        }

        private void throwOutButton_OnButtonClick(MyGuiControlButton sender)
        {
            VRage.Game.Entity.MyEntity inventoryOwner = this.m_focusedOwnerControl.InventoryOwner;
            if ((this.m_selectedInventoryItem != null) && (inventoryOwner != null))
            {
                MyPhysicalInventoryItem item = this.m_selectedInventoryItem.Value;
                if (this.m_focusedGridControl.SelectedIndex != null)
                {
                    this.m_selectedInventory.DropItem(this.m_focusedGridControl.SelectedIndex.Value, item.Amount);
                }
            }
            this.RefreshSelectedInventoryItem();
        }

        private bool TransferToOppositeFirst(MyPhysicalInventoryItem item)
        {
            MyGuiControlInventoryOwner focusedOwnerControl = this.m_focusedOwnerControl;
            ObservableCollection<MyGuiControlBase>.Enumerator enumerator = (ReferenceEquals(focusedOwnerControl.Owner, this.m_leftOwnersControl) ? this.m_rightOwnersControl : this.m_leftOwnersControl).Controls.GetEnumerator();
            MyGuiControlInventoryOwner current = null;
            while (true)
            {
                if (enumerator.MoveNext())
                {
                    if (!enumerator.Current.Visible)
                    {
                        continue;
                    }
                    current = enumerator.Current as MyGuiControlInventoryOwner;
                }
                if (current == null)
                {
                    goto TR_0000;
                }
                else if (current.Enabled)
                {
                    int num1;
                    if (ReferenceEquals(focusedOwnerControl.InventoryOwner, this.m_userAsOwner) || ReferenceEquals(focusedOwnerControl.InventoryOwner, this.m_interactedAsOwner))
                    {
                        num1 = ReferenceEquals(current.InventoryOwner, this.m_userAsOwner) ? 1 : ((int) ReferenceEquals(current.InventoryOwner, this.m_interactedAsOwner));
                    }
                    else
                    {
                        num1 = 0;
                    }
                    if (num1 == 0)
                    {
                        bool flag = focusedOwnerControl.InventoryOwner is MyCharacter;
                        bool flag2 = current.InventoryOwner is MyCharacter;
                        IMyConveyorEndpointBlock block = (focusedOwnerControl.InventoryOwner == null) ? null : ((flag ? ((IMyConveyorEndpointBlock) this.m_interactedAsOwner) : ((IMyConveyorEndpointBlock) focusedOwnerControl.InventoryOwner)) as IMyConveyorEndpointBlock);
                        IMyConveyorEndpointBlock block2 = (current.InventoryOwner == null) ? null : ((flag2 ? ((IMyConveyorEndpointBlock) this.m_interactedAsOwner) : ((IMyConveyorEndpointBlock) current.InventoryOwner)) as IMyConveyorEndpointBlock);
                        if (block == null)
                        {
                            break;
                        }
                        if (block2 == null)
                        {
                            break;
                        }
                        try
                        {
                            MyGridConveyorSystem.AppendReachableEndpoints(block.ConveyorEndpoint, MySession.Static.LocalPlayerId, this.m_reachableInventoryOwners, item, this.m_endpointPredicate);
                            if (!this.m_reachableInventoryOwners.Contains(block2.ConveyorEndpoint))
                            {
                                return false;
                            }
                        }
                        finally
                        {
                            this.m_reachableInventoryOwners.Clear();
                        }
                        if (!MyGridConveyorSystem.Reachable(block.ConveyorEndpoint, block2.ConveyorEndpoint))
                        {
                            return false;
                        }
                    }
                    VRage.Game.Entity.MyEntity inventoryOwner = current.InventoryOwner;
                    VRage.Game.Entity.MyEntity entity1 = this.m_focusedOwnerControl.InventoryOwner;
                    MyInventory userData = (MyInventory) this.m_focusedGridControl.UserData;
                    MyInventory dst = null;
                    int index = 0;
                    while (true)
                    {
                        if (index < inventoryOwner.InventoryCount)
                        {
                            MyInventory inventory = inventoryOwner.GetInventory(index);
                            if (!inventory.CheckConstraint(item.Content.GetId()))
                            {
                                index++;
                                continue;
                            }
                            dst = inventory;
                        }
                        if (dst == null)
                        {
                            return false;
                        }
                        MyInventory.TransferByUser(userData, dst, userData.GetItems()[this.m_focusedGridControl.SelectedIndex.Value].ItemId, -1, new MyFixedPoint?(item.Amount));
                        return true;
                    }
                }
                else
                {
                    goto TR_0000;
                }
                break;
            }
            return false;
        TR_0000:
            return false;
        }

        public void UpdateBeforeDraw()
        {
            if (this.m_selectionDirty)
            {
                this.m_selectionDirty = false;
                if (this.m_leftShowsGrid)
                {
                    this.LeftTypeGroup_SelectedChanged(this.m_leftTypeGroup);
                }
                if (this.m_rightShowsGrid)
                {
                    this.RightTypeGroup_SelectedChanged(this.m_rightTypeGroup);
                }
            }
        }

        private void UpdateDisabledControlsWhileDragging(MyGuiControlInventoryOwner control)
        {
            if (this.m_controlsDisabledWhileDragged.Count != 0)
            {
                VRage.Game.Entity.MyEntity inventoryOwner = control.InventoryOwner;
                for (int i = 0; i < inventoryOwner.InventoryCount; i++)
                {
                    MyGuiControlGrid item = control.ContentGrids[i];
                    if (this.m_controlsDisabledWhileDragged.Contains(item) && item.Enabled)
                    {
                        item.Enabled = false;
                    }
                }
            }
        }

        private void UpdateSelection()
        {
            this.m_selectionDirty = this.m_leftShowsGrid || this.m_rightShowsGrid;
        }
    }
}

