namespace Sandbox.ModAPI
{
    using Sandbox.ModAPI.Ingame;
    using System;

    public interface IMyGridProgram
    {
        [Obsolete("Use overload Main(String, UpdateType)")]
        void Main(string argument);
        void Main(string argument, UpdateType updateSource);
        void Save();

        Func<IMyIntergridCommunicationSystem> IGC_ContextGetter { set; }

        Sandbox.ModAPI.Ingame.IMyGridTerminalSystem GridTerminalSystem { get; set; }

        Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me { get; set; }

        [Obsolete("Use Runtime.TimeSinceLastRun instead")]
        TimeSpan ElapsedTime { get; set; }

        string Storage { get; set; }

        IMyGridProgramRuntimeInfo Runtime { get; set; }

        Action<string> Echo { get; set; }

        bool HasMainMethod { get; }

        bool HasSaveMethod { get; }
    }
}

