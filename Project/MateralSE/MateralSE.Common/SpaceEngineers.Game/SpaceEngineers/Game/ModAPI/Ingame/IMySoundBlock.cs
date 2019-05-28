namespace SpaceEngineers.Game.ModAPI.Ingame
{
    using Sandbox.ModAPI.Ingame;
    using System;
    using System.Collections.Generic;
    using VRage.Game.ModAPI.Ingame;

    public interface IMySoundBlock : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
    {
        void GetSounds(List<string> sounds);
        void Play();
        void Stop();

        float Volume { get; set; }

        float Range { get; set; }

        bool IsSoundSelected { get; }

        float LoopPeriod { get; set; }

        string SelectedSound { get; set; }
    }
}

