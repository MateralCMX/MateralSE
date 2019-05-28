namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Definitions;
    using Sandbox.Engine.Platform;
    using Sandbox.Engine.Utils;
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Gui;
    using Sandbox.Game.Multiplayer;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.InteropServices;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.GUI;
    using VRage.Game.ObjectBuilders.Components;
    using VRage.Game.ObjectBuilders.Definitions;
    using VRage.Utils;

    [MyToolbarItemDescriptor(typeof(MyObjectBuilder_ToolbarItemCubeBlock))]
    public class MyToolbarItemCubeBlock : MyToolbarItemDefinition
    {
        private MyFixedPoint m_lastAmount = 0;

        public override bool Activate()
        {
            MyCharacter localCharacter = MySession.Static.LocalCharacter;
            MyDefinitionId weaponDefinition = new MyDefinitionId(typeof(MyObjectBuilder_CubePlacer));
            if (localCharacter == null)
            {
                if (MyBlockBuilderBase.SpectatorIsBuilding)
                {
                    MyCubeBuilder.Static.Activate(new MyDefinitionId?(((MyCubeBlockDefinition) base.Definition).Id));
                }
            }
            else
            {
                if (!MySessionComponentSafeZones.IsActionAllowed(localCharacter, MySafeZoneAction.Building, 0L))
                {
                    return false;
                }
                if ((localCharacter.CurrentWeapon == null) || (localCharacter.CurrentWeapon.DefinitionId != weaponDefinition))
                {
                    localCharacter.SwitchToWeapon(weaponDefinition);
                }
                MyCubeBuilder.Static.Activate(new MyDefinitionId?(((MyCubeBlockDefinition) base.Definition).Id));
            }
            return true;
        }

        public override bool AllowedInToolbarType(MyToolbarType type) => 
            ((type == MyToolbarType.Character) || ((type == MyToolbarType.Spectator) || (type == MyToolbarType.BuildCockpit)));

        public override void FillGridItem(MyGuiGridItem gridItem)
        {
            if (MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID)
            {
                if (this.m_lastAmount > 0)
                {
                    gridItem.AddText($"{this.m_lastAmount}x", MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                }
                else
                {
                    gridItem.ClearText(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM);
                }
            }
        }

        public override bool Init(MyObjectBuilder_ToolbarItem data)
        {
            base.ActivateOnClick = false;
            MyCubeBlockDefinition definition = base.Definition as MyCubeBlockDefinition;
            bool flag1 = base.Init(data);
            if ((flag1 && ((definition != null) && ((definition.BlockStages != null) && ((definition.BlockStages.Length != 0) && (MyHud.HudDefinition != null))))) && !Sandbox.Engine.Platform.Game.IsDedicated)
            {
                MyObjectBuilder_GuiTexture texture = MyGuiTextures.Static.GetTexture(MyHud.HudDefinition.Toolbar.ItemStyle.VariantTexture);
                base.SetSubIcon(texture.Path);
            }
            return flag1;
        }

        public override MyToolbarItem.ChangeInfo Update(MyEntity owner, long playerID = 0L)
        {
            MyToolbarItem.ChangeInfo none = MyToolbarItem.ChangeInfo.None;
            bool newEnabled = true;
            if (MyCubeBuilder.Static != null)
            {
                MyCubeBlockDefinition definition = MyCubeBuilder.Static.IsActivated ? MyCubeBuilder.Static.ToolbarBlockDefinition : null;
                MyCubeBlockDefinition definition2 = base.Definition as MyCubeBlockDefinition;
                if (!MyCubeBuilder.Static.IsActivated || (definition == null))
                {
                    base.WantsToBeSelected = false;
                }
                else
                {
                    base.WantsToBeSelected = definition.BlockPairName == definition2.BlockPairName;
                }
                MyCharacter localCharacter = MySession.Static.LocalCharacter;
                if ((MyFakes.ENABLE_GATHERING_SMALL_BLOCK_FROM_GRID && (definition2.CubeSize == MyCubeSize.Small)) && (localCharacter != null))
                {
                    MyInventory inventory = localCharacter.GetInventory(0);
                    MyFixedPoint point = (inventory != null) ? inventory.GetItemAmount(base.Definition.Id, MyItemFlags.None, false) : 0;
                    if (this.m_lastAmount != point)
                    {
                        this.m_lastAmount = point;
                        none |= MyToolbarItem.ChangeInfo.IconText;
                    }
                    if (MySession.Static.SurvivalMode)
                    {
                        newEnabled &= this.m_lastAmount > 0;
                    }
                    else
                    {
                        none |= MyToolbarItem.ChangeInfo.IconText;
                    }
                }
                if ((MySession.Static.ResearchEnabled && !MySession.Static.CreativeToolsEnabled(Sync.MyId)) && (MySessionComponentResearch.Static != null))
                {
                    newEnabled &= MySessionComponentResearch.Static.CanUse(localCharacter, base.Definition.Id);
                }
                if (base.Enabled != newEnabled)
                {
                    none |= base.SetEnabled(newEnabled);
                }
            }
            return none;
        }

        public MyFixedPoint Amount =>
            this.m_lastAmount;
    }
}

