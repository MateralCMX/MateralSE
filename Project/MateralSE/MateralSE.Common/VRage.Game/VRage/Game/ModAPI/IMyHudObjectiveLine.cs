namespace VRage.Game.ModAPI
{
    using System;
    using System.Collections.Generic;

    public interface IMyHudObjectiveLine
    {
        void AdvanceObjective();
        void Hide();
        void Show();

        bool Visible { get; }

        string Title { get; set; }

        string CurrentObjective { get; }

        List<string> Objectives { get; set; }
    }
}

