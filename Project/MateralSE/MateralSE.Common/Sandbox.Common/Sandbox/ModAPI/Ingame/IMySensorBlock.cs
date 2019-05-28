namespace Sandbox.ModAPI.Ingame
{
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI.Ingame;

    public interface IMySensorBlock : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void DetectedEntities(List<MyDetectedEntityInfo> entities);

        float MaxRange { get; }

        float LeftExtend { get; set; }

        float RightExtend { get; set; }

        float TopExtend { get; set; }

        float BottomExtend { get; set; }

        float FrontExtend { get; set; }

        float BackExtend { get; set; }

        bool PlayProximitySound { get; set; }

        bool DetectPlayers { get; set; }

        bool DetectFloatingObjects { get; set; }

        bool DetectSmallShips { get; set; }

        bool DetectLargeShips { get; set; }

        bool DetectStations { get; set; }

        bool DetectSubgrids { get; set; }

        bool DetectAsteroids { get; set; }

        bool DetectOwner { get; set; }

        bool DetectFriendly { get; set; }

        bool DetectNeutral { get; set; }

        bool DetectEnemy { get; set; }

        bool IsActive { get; }

        MyDetectedEntityInfo LastDetectedEntity { get; }
    }
}

