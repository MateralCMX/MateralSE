namespace Sandbox.Game.Screens.Helpers
{
    using Sandbox.Game.Entities;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using VRageMath;

    internal class MyGridColorHelper
    {
        private Dictionary<MyCubeGrid, Color> m_colors = new Dictionary<MyCubeGrid, Color>();
        private int m_lastColorIndex;

        public Color GetGridColor(MyCubeGrid grid)
        {
            Color color;
            if (!this.m_colors.TryGetValue(grid, out color))
            {
                while (true)
                {
                    int lastColorIndex = this.m_lastColorIndex;
                    this.m_lastColorIndex = lastColorIndex + 1;
                    color = new Vector3(((float) (lastColorIndex % 20)) / 20f, 0.75f, 1f).HSVtoColor();
                    if ((color.HueDistance(Color.Red) >= 0.04f) && (color.HueDistance(((float) 0.65f)) >= 0.07f))
                    {
                        this.m_colors[grid] = color;
                        break;
                    }
                }
            }
            return color;
        }

        public void Init(MyCubeGrid mainGrid = null)
        {
            this.m_lastColorIndex = 0;
            this.m_colors.Clear();
            if (mainGrid != null)
            {
                this.m_colors.Add(mainGrid, Color.White);
            }
        }
    }
}

