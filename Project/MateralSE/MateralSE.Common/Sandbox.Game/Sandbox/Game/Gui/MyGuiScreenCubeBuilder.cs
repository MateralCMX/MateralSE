namespace Sandbox.Game.Gui
{
    using Sandbox;
    using Sandbox.Definitions;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Screens.Helpers;
    using Sandbox.Graphics.GUI;
    using Sandbox.Gui;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRage.Game;
    using VRage.Game.Definitions.Animation;
    using VRageMath;

    public class MyGuiScreenCubeBuilder : MyGuiScreenToolbarConfigBase
    {
        private MyGuiControlBlockGroupInfo m_blockGroupInfo;
        private MyGuiGridItem m_lastGridBlocksMouseOverItem;

        public MyGuiScreenCubeBuilder(int scrollOffset = 0, MyCubeBlock owner = null) : base(MyHud.HudDefinition.Toolbar, scrollOffset, owner)
        {
            MySandboxGame.Log.WriteLine("MyGuiScreenCubeBuilder.ctor START");
            MyGuiScreenToolbarConfigBase.Static = this;
            base.m_scrollOffset = ((float) scrollOffset) / 6.5f;
            base.m_size = new Vector2(1f, 1f);
            base.m_canShareInput = true;
            base.m_drawEvenWithoutFocus = true;
            base.EnabledBackgroundFade = true;
            base.m_screenOwner = owner;
            this.RecreateControls(true);
            MySandboxGame.Log.WriteLine("MyGuiScreenCubeBuilder.ctor END");
        }

        public override string GetFriendlyName() => 
            "MyGuiScreenCubeBuilder";

        private void m_researchGraph_MouseOverItemChanged(object sender, System.EventArgs e)
        {
            if (base.m_researchGraph.Visible)
            {
                MyGuiGridItem mouseOverItem = base.m_researchGraph.MouseOverItem;
                MyGuiGridItem gridItem = mouseOverItem ?? base.m_researchGraph.SelectedItem;
                this.ShowItem(gridItem);
            }
        }

        private void m_researchGraph_SelectedItemChanged(object sender, System.EventArgs e)
        {
            if (base.m_researchGraph.Visible)
            {
                MyGuiGridItem selectedItem = base.m_researchGraph.SelectedItem;
                this.ShowItem(selectedItem);
            }
        }

        private void OnGridMouseOverIndexChanged(MyGuiControlGrid myGuiControlGrid, MyGuiControlGrid.EventArgs eventArgs)
        {
            if (base.m_gridBlocks.Visible)
            {
                MyGuiGridItem mouseOverItem = base.m_gridBlocks.MouseOverItem;
                MyGuiGridItem gridItem = mouseOverItem ?? base.m_gridBlocks.SelectedItem;
                this.ShowItem(gridItem);
            }
        }

        private void OnSelectedItemChanged(MyGuiControlGrid arg1, MyGuiControlGrid.EventArgs arg2)
        {
            this.OnGridMouseOverIndexChanged(arg1, arg2);
        }

        public override void RecreateControls(bool contructor)
        {
            base.RecreateControls(contructor);
            base.m_gridBlocks.MouseOverIndexChanged += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.OnGridMouseOverIndexChanged);
            base.m_gridBlocks.ItemSelected += new Action<MyGuiControlGrid, MyGuiControlGrid.EventArgs>(this.OnSelectedItemChanged);
            base.m_researchGraph.MouseOverItemChanged += new EventHandler(this.m_researchGraph_MouseOverItemChanged);
            base.m_researchGraph.SelectedItemChanged += new EventHandler(this.m_researchGraph_SelectedItemChanged);
            this.m_blockGroupInfo = (MyGuiControlBlockGroupInfo) this.Controls.GetControlByName("BlockInfoPanel");
            this.m_blockGroupInfo.RegisterAllControls(this.Controls);
            this.m_blockGroupInfo.ColorMask = base.m_gridBlocks.ColorMask;
            this.m_blockGroupInfo.UpdateArrange();
            base.CloseButtonStyle = MyGuiControlButtonStyleEnum.CloseBackground;
            this.m_blockGroupInfo.SetBlockModeEnabled(!base.m_shipMode);
            foreach (MyGuiControlBase local1 in this.m_blockGroupInfo.GetControls(true))
            {
                local1.Visible = !base.m_shipMode;
                MyGuiControlStackPanel panel = local1 as MyGuiControlStackPanel;
                if (panel != null)
                {
                    using (List<MyGuiControlBase>.Enumerator enumerator2 = panel.GetControls(true).GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            enumerator2.Current.Visible = !base.m_shipMode;
                        }
                    }
                }
            }
        }

        private void ShowItem(MyGuiGridItem gridItem)
        {
            if ((gridItem != null) && !ReferenceEquals(this.m_lastGridBlocksMouseOverItem, gridItem))
            {
                MyGuiScreenToolbarConfigBase.GridItemUserData userData = gridItem.UserData as MyGuiScreenToolbarConfigBase.GridItemUserData;
                if (userData != null)
                {
                    MyDefinitionBase base2;
                    MyObjectBuilder_ToolbarItemDefinition itemData = userData.ItemData as MyObjectBuilder_ToolbarItemDefinition;
                    if ((itemData != null) && MyDefinitionManager.Static.TryGetDefinition<MyDefinitionBase>(itemData.DefinitionId, out base2))
                    {
                        this.m_blockGroupInfo.Visible = true;
                        this.m_lastGridBlocksMouseOverItem = gridItem;
                        MyCubeBlockDefinition definition2 = base2 as MyCubeBlockDefinition;
                        if (definition2 != null)
                        {
                            MyCubeBlockDefinitionGroup definitionGroup = MyDefinitionManager.Static.GetDefinitionGroup(definition2.BlockPairName);
                            this.m_blockGroupInfo.SetBlockGroup(definitionGroup);
                        }
                        else
                        {
                            MyPhysicalItemDefinition definition = base2 as MyPhysicalItemDefinition;
                            if (definition != null)
                            {
                                this.m_blockGroupInfo.SetGeneralDefinition(definition);
                            }
                            else
                            {
                                MyAnimationDefinition definition4 = base2 as MyAnimationDefinition;
                                if (definition4 != null)
                                {
                                    this.m_blockGroupInfo.SetGeneralDefinition(definition4);
                                }
                                else
                                {
                                    MyVoxelHandDefinition definition5 = base2 as MyVoxelHandDefinition;
                                    if (definition5 != null)
                                    {
                                        this.m_blockGroupInfo.SetGeneralDefinition(definition5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

