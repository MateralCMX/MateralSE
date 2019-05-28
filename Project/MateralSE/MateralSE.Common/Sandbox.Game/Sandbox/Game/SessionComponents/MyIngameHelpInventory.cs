namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character.Components;
    using Sandbox.Game.Localization;
    using System;
    using VRage.Game.Entity.UseObject;
    using VRage.Input;

    [IngameObjective("IngameHelp_Inventory", 110)]
    internal class MyIngameHelpInventory : MyIngameHelpObjective
    {
        private IMyUseObject m_interactiveObject;
        private bool m_fPressed;
        private bool m_iPressed;

        public MyIngameHelpInventory()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Inventory_Title;
            base.RequiredIds = new string[] { "IngameHelp_Movement", "IngameHelp_Jetpack2" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.LookingOnInteractiveObject));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Inventory_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Inventory_Detail2
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.USE) };
            detail.FinishCondition = new Func<bool>(this.UsePressed);
            detailArray1[1] = detail;
            detail = new MyIngameHelpDetail {
                TextEnum = MySpaceTexts.IngameHelp_Inventory_Detail3
            };
            detail.Args = new object[] { GetHighlightedControl(MyControlsSpace.INVENTORY) };
            detail.FinishCondition = new Func<bool>(this.IPressed);
            detailArray1[2] = detail;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_InventoryTip";
            MyCharacterDetectorComponent.OnInteractiveObjectChanged += new Action<IMyUseObject>(this.MyCharacterDetectorComponent_OnInteractiveObjectChanged);
        }

        private bool IPressed()
        {
            if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.INVENTORY))
            {
                this.m_iPressed = true;
            }
            return this.m_iPressed;
        }

        private bool LookingOnInteractiveObject() => 
            (this.m_interactiveObject != null);

        private void MyCharacterDetectorComponent_OnInteractiveObjectChanged(IMyUseObject obj)
        {
            if (obj is MyFloatingObject)
            {
                this.m_interactiveObject = obj;
            }
            else
            {
                this.m_interactiveObject = null;
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

