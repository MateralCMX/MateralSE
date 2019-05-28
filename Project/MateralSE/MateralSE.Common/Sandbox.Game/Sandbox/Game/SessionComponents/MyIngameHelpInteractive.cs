namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Game;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;

    [IngameObjective("IngameHelp_Interactive", 0x17)]
    internal class MyIngameHelpInteractive : MyIngameHelpObjective
    {
        private float LOOKING_TIME = 1f;
        private IMyUseObject m_interactiveObject;
        private bool m_fPressed;
        private bool m_kPressed;
        private bool m_iPressed;
        private float m_lookingCounter;

        public MyIngameHelpInteractive()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Interactive_Title;
            base.RequiredIds = new string[] { "IngameHelp_Intro" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LookingOnInteractiveObjectDelayed));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Interactive_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[4];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Interactive_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.USE) };
            detail.FinishCondition = new Func<bool>(this.UsePressed);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Interactive_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.TERMINAL) };
            detail.FinishCondition = new Func<bool>(this.KPressed);
            detailArray1[2] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Interactive_Detail4
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.INVENTORY) };
            detail.FinishCondition = new Func<bool>(this.IPressed);
            detailArray1[3] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_InteractiveTip";
            MyCharacterDetectorComponent.OnInteractiveObjectChanged += new Action<IMyUseObject>(this.MyCharacterDetectorComponent_OnInteractiveObjectChanged);
        }

        private bool IPressed()
        {
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.INVENTORY) && this.LookingOnInteractiveObject()) && this.m_interactiveObject.SupportedActions.HasFlag(UseActionEnum.OpenInventory))
            {
                this.m_iPressed = true;
            }
            return this.m_iPressed;
        }

        private bool IsFriendly()
        {
            if (this.m_interactiveObject == null)
            {
                return false;
            }
            MyCubeBlock owner = this.m_interactiveObject.Owner as MyCubeBlock;
            return ((owner != null) && (owner.GetPlayerRelationToOwner() != MyRelationsBetweenPlayerAndBlock.Enemies));
        }

        private bool KPressed()
        {
            if ((MyInput.Static.IsNewGameControlPressed(MyControlsSpace.TERMINAL) && this.LookingOnInteractiveObject()) && this.m_interactiveObject.SupportedActions.HasFlag(UseActionEnum.OpenTerminal))
            {
                this.m_kPressed = true;
            }
            return this.m_kPressed;
        }

        private bool LookingOnInteractiveObject() => 
            ((this.m_interactiveObject != null) && this.IsFriendly());

        private bool LookingOnInteractiveObjectDelayed()
        {
            this.m_lookingCounter = !this.LookingOnInteractiveObject() ? 0f : (this.m_lookingCounter + 0.01666667f);
            return (this.m_lookingCounter > this.LOOKING_TIME);
        }

        private void MyCharacterDetectorComponent_OnInteractiveObjectChanged(IMyUseObject obj)
        {
            if ((obj == null) || !(obj is MyUseObjectBase))
            {
                this.m_interactiveObject = null;
            }
            else
            {
                MyCubeBlock owner = obj.Owner as MyCubeBlock;
                if ((owner != null) && (owner.GetPlayerRelationToOwner() != MyRelationsBetweenPlayerAndBlock.Enemies))
                {
                    this.m_interactiveObject = obj;
                }
            }
        }

        private void MyCharacterDetectorComponent_OnInteractiveObjectUsed(IMyUseObject obj)
        {
            if (ReferenceEquals(this.m_interactiveObject, obj))
            {
                this.m_fPressed = true;
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MyCharacterDetectorComponent.OnInteractiveObjectUsed += new Action<IMyUseObject>(this.MyCharacterDetectorComponent_OnInteractiveObjectUsed);
        }

        private bool UsePressed() => 
            this.m_fPressed;
    }
}

