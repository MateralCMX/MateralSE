namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using VRage.Game.Entity.UseObject;

    [IngameObjective("IngameHelp_HandDrill", 210)]
    internal class MyIngameHelpHandDrill : MyIngameHelpObjective
    {
        private IMyUseObject m_interactiveObject;
        private bool m_rockPicked;
        private bool m_isDrilling;
        private bool m_diggedTunnel;

        public MyIngameHelpHandDrill()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_HandDrill_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.PlayerHasHandDrill));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_HandDrill_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[4];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HandDrill_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.PRIMARY_TOOL_ACTION) };
            detail.FinishCondition = new Func<bool>(this.PlayerIsDrillingStone);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HandDrill_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.USE) };
            detail.FinishCondition = new Func<bool>(this.PickedRocks);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_HandDrill_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.SECONDARY_TOOL_ACTION) };
            detail.FinishCondition = new Func<bool>(this.DiggedTunnel);
            detailArray1[3] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            MyCharacterDetectorComponent.OnInteractiveObjectChanged += new Action<IMyUseObject>(this.MyCharacterDetectorComponent_OnInteractiveObjectChanged);
        }

        private bool DiggedTunnel()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (localCharacter != null)
            {
                MyHandDrill equippedTool = localCharacter.EquippedTool as MyHandDrill;
                if (((equippedTool != null) && (equippedTool.IsShooting && (equippedTool.DrilledEntity is MyVoxelBase))) && !equippedTool.CollectingOre)
                {
                    this.m_diggedTunnel = true;
                }
            }
            return this.m_diggedTunnel;
        }

        private void MyCharacterDetectorComponent_OnInteractiveObjectChanged(IMyUseObject obj)
        {
            if (!(obj is MyFloatingObject) || !((MyFloatingObject) obj).ItemDefinition.Id.SubtypeName.Contains("Stone"))
            {
                this.m_interactiveObject = null;
            }
            else
            {
                this.m_interactiveObject = obj;
            }
        }

        private void MyCharacterDetectorComponent_OnInteractiveObjectUsed(IMyUseObject obj)
        {
            if (ReferenceEquals(this.m_interactiveObject, obj))
            {
                this.m_rockPicked = true;
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MyCharacterDetectorComponent.OnInteractiveObjectUsed += new Action<IMyUseObject>(this.MyCharacterDetectorComponent_OnInteractiveObjectUsed);
        }

        private bool PickedRocks() => 
            this.m_rockPicked;

        private bool PlayerHasHandDrill()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && (localCharacter.EquippedTool is MyHandDrill));
        }

        private bool PlayerIsDrillingStone()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (localCharacter != null)
            {
                MyHandDrill equippedTool = localCharacter.EquippedTool as MyHandDrill;
                if (((equippedTool != null) && (equippedTool.IsShooting && (equippedTool.DrilledEntity is MyVoxelBase))) && equippedTool.CollectingOre)
                {
                    this.m_isDrilling = true;
                }
            }
            return this.m_isDrilling;
        }
    }
}

