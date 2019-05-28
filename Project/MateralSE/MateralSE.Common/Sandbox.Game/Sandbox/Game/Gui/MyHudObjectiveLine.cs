namespace Sandbox.Game.Gui
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using VRage.Game.ModAPI;

    public class MyHudObjectiveLine : IMyHudObjectiveLine
    {
        private string m_missionTitle = "";
        private int m_currentObjective;
        private List<string> m_objectives = new List<string>();

        public MyHudObjectiveLine()
        {
            this.Visible = false;
        }

        public void AdvanceObjective()
        {
            if (this.m_currentObjective < (this.m_objectives.Count - 1))
            {
                this.m_currentObjective++;
            }
        }

        public void Clear()
        {
            this.m_missionTitle = "";
            this.m_currentObjective = 0;
            this.m_objectives.Clear();
            this.Visible = false;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void ResetObjectives()
        {
            this.m_currentObjective = 0;
        }

        public void Show()
        {
            this.Visible = true;
        }

        public bool Visible { get; private set; }

        public string Title
        {
            get => 
                this.m_missionTitle;
            set => 
                (this.m_missionTitle = value);
        }

        public string CurrentObjective =>
            this.m_objectives[this.m_currentObjective];

        public List<string> Objectives
        {
            get => 
                this.m_objectives;
            set => 
                (this.m_objectives = value);
        }
    }
}

