namespace SpaceEngineers.Game.Achievements
{
    using Sandbox.Game.Entities;
    using Sandbox.Game.SessionComponents;
    using Sandbox.Game.World;
    using SpaceEngineers.Game.Entities.Blocks;
    using System;
    using VRage.ModAPI;

    public class MyAchievement_NumberFiveIsAlive : MySteamAchievementBase
    {
        public override void SessionUpdate()
        {
            if ((!base.IsAchieved && (MySession.Static.LocalCharacter != null)) && (MySession.Static.LocalCharacter != null))
            {
                IMyEntity temporaryConnectedEntity = MySession.Static.LocalCharacter.SuitBattery.ResourceSink.TemporaryConnectedEntity;
                if (((MySession.Static.LocalCharacter.SuitEnergyLevel < 0.01) && (temporaryConnectedEntity != null)) && !ReferenceEquals(temporaryConnectedEntity, MySession.Static.LocalCharacter))
                {
                    MyMedicalRoom room = temporaryConnectedEntity as MyMedicalRoom;
                    if (((room != null) && room.IsWorking) && room.RefuelAllowed)
                    {
                        base.NotifyAchieved();
                    }
                    else
                    {
                        MyCockpit cockpit = temporaryConnectedEntity as MyCockpit;
                        if ((cockpit != null) && cockpit.hasPower)
                        {
                            base.NotifyAchieved();
                        }
                    }
                }
            }
        }

        public override string AchievementTag =>
            "MyAchievement_NumberFiveIsAlive";

        public override bool NeedsUpdate =>
            !MySession.Static.CreativeMode;
    }
}

