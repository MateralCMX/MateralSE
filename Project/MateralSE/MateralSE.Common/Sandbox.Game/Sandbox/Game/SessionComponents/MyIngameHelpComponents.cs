namespace Sandbox.Game.SessionComponents
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.Entities.Character;
    using Sandbox.Game.Localization;
    using Sandbox.Game.Weapons;
    using Sandbox.Game.World;
    using System;
    using System.Collections.Generic;
    using VRage.Game;

    [IngameObjective("IngameHelp_Components", 230)]
    internal class MyIngameHelpComponents : MyIngameHelpObjective
    {
        public MyIngameHelpComponents()
        {
            base.TitleEnum = MySpaceTexts.IngameHelp_Components_Title;
            base.RequiredIds = new string[] { "IngameHelp_Ingots" };
            base.RequiredCondition = (Func<bool>) Delegate.Combine(base.RequiredCondition, new Func<bool>(this.ComponentsInInventory));
            MyIngameHelpDetail detail1 = new MyIngameHelpDetail();
            detail1.TextEnum = MySpaceTexts.IngameHelp_Components_Detail1;
            MyIngameHelpDetail[] detailArray1 = new MyIngameHelpDetail[2];
            detailArray1[0] = detail1;
            MyIngameHelpDetail detail2 = new MyIngameHelpDetail();
            detail2.TextEnum = MySpaceTexts.IngameHelp_Components_Detail2;
            detail2.FinishCondition = new Func<bool>(this.BlockRepaired);
            detailArray1[1] = detail2;
            base.Details = detailArray1;
            base.DelayToHide = MySessionComponentIngameHelp.DEFAULT_OBJECTIVE_DELAY;
            base.FollowingId = "IngameHelp_ComponentsTip";
        }

        private bool BlockRepaired()
        {
            MyCharacter localCharacter = MySession.Static?.LocalCharacter;
            return ((localCharacter != null) && ((localCharacter.EquippedTool is MyWelder) && (!string.IsNullOrEmpty((localCharacter.EquippedTool as MyWelder).EffectId) && ((localCharacter.EquippedTool as MyWelder).IsShooting && ((localCharacter.EquippedTool as MyWelder).HasHitBlock && !(localCharacter.EquippedTool as MyWelder).GetTargetBlock().IsFullIntegrity)))));
        }

        private bool ComponentsInInventory()
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
                        if (enumerator.Current.Content is MyObjectBuilder_Component)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

