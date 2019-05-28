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

    [IngameObjective("IngameHelp_Ingots", 220)]
    internal class MyIngameHelpIngots : MyIngameHelpObjective
    {
        private HashSet<MyAssembler> m_observedAssemblers = new HashSet<MyAssembler>();
        private bool m_ingotAdded;
        private bool m_steelProduced;

        public MyIngameHelpIngots()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Ingots_Title;
            base.RequiredIds = new string[] { "IngameHelp_RefiningOre" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.IngotsInInventory));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Ingots_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[3];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Ingots_Detail2;
            detail2.FinishCondition = new Func<bool>(this.PutToAssembler);
            detailArray1[1] = detail2;
            MyIngameHelpDetail detail3 = new MyIngameHelpDetail();
            detail3.TextEnum = MySpaceTexts.IngameHelp_Ingots_Detail3;
            detail3.FinishCondition = new Func<bool>(this.SteelFromAssembler);
            detailArray1[2] = detail3;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
        }

        private bool IngotsInInventory()
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
                        MyPhysicalInventoryItem current = enumerator.Current;
                        if ((current.Content is MyObjectBuilder_Ingot) && (current.Content.SubtypeName == "Iron"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void MyInventory_OnTransferByUser(IMyInventory inventory1, IMyInventory inventory2, IMyInventoryItem item, MyFixedPoint amount)
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            if (((localCharacter != null) && ((item.Content is MyObjectBuilder_Ingot) && (item.Content.SubtypeName == "Iron"))) && ReferenceEquals(inventory1.Owner, localCharacter))
            {
                MyAssembler owner = inventory2.Owner as MyAssembler;
                if ((owner != null) && !this.m_observedAssemblers.Contains(owner))
                {
                    owner.OutputInventory.ContentsAdded += new Action<MyPhysicalInventoryItem, MyFixedPoint>(this.OutputInventory_ContentsAdded);
                    this.m_observedAssemblers.Add(owner);
                    this.m_ingotAdded = true;
                }
            }
        }

        public override void OnActivated()
        {
            base.OnActivated();
            MyInventory.OnTransferByUser += new Action<IMyInventory, IMyInventory, IMyInventoryItem, MyFixedPoint>(this.MyInventory_OnTransferByUser);
        }

        private void OutputInventory_ContentsAdded(MyPhysicalInventoryItem item, MyFixedPoint amount)
        {
            if ((item.Content is MyObjectBuilder_Component) && (item.Content.SubtypeName == "SteelPlate"))
            {
                this.m_steelProduced = true;
            }
        }

        private bool PutToAssembler() => 
            this.m_ingotAdded;

        private bool SteelFromAssembler() => 
            this.m_steelProduced;
    }
}

