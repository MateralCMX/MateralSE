namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game;
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Entities.Cube;
    using Sandbox.Game.Localization;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Entity;
    using VRage.Game.ModAPI.Ingame;

    [IngameObjective("IngameHelp_RefiningOre", 210)]
    internal class MyIngameHelpRefiningOre : MyIngameHelpObjective
    {
        private HashSet<MyRefinery> m_observedRefineries = new HashSet<MyRefinery>();
        private bool m_ingotProduced;

        public MyIngameHelpRefiningOre()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_RefiningOre_Title;
            base.RequiredIds = new string[] { "IngameHelp_HandDrill", "IngameHelp_Building" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.OreInInventory));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_RefiningOre_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_RefiningOre_Detail2;
            detail2.FinishCondition = new Func<bool>(this.IngotFromRefinery);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool IngotFromRefinery() => 
            this.m_ingotProduced;

        private void MyInventory_OnTransferByUser(IMyInventory inventory1, IMyInventory inventory2, IMyInventoryItem item, MyFixedPoint amount)
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (((localCharacter != null) && (item.Content is MyObjectBuilder_Ore)) && ReferenceEquals(inventory1.Owner, localCharacter))
            {
                MyRefinery owner = inventory2.Owner as MyRefinery;
                if ((owner != null) && !this.m_observedRefineries.Contains(owner))
                {
                    owner.OutputInventory.ContentsAdded += new Action<MyPhysicalInventoryItem, MyFixedPoint>(this.OutputInventory_ContentsAdded);
                    this.m_observedRefineries.Add(owner);
                }
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MyInventory.OnTransferByUser += new Action<IMyInventory, IMyInventory, IMyInventoryItem, MyFixedPoint>(this.MyInventory_OnTransferByUser);
        }

        private bool OreInInventory()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (localCharacter != null)
            {
                using (List<MyPhysicalInventoryItem>.Enumerator enumerator = localCharacter.GetInventory(0).GetItems().GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        if (enumerator.Current.Content is MyObjectBuilder_Ore)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void OutputInventory_ContentsAdded(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            if (item.Content is MyObjectBuilder_Ingot)
            {
                this.m_ingotProduced = true;
            }
        }
    }
}

