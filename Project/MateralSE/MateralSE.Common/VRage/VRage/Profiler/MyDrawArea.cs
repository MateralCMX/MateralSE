namespace VRage.Profiler
{
    using System;
    using System.Runtime.CompilerServices;

    public class MyDrawArea
    {
        public readonly float XStart;
        public readonly float YStart;
        public readonly float XScale;
        public readonly float YScale;

        public MyDrawArea(float xStart, float yStart, float xScale, float yScale, float yRange)
        {
            this.Index = (int) Math.Round((double) (Math.Log((double) yRange, 2.0) * 2.0));
            this.XStart = xStart;
            this.YStart = yStart;
            this.XScale = xScale;
            this.YScale = yScale;
            this.UpdateRange();
        }

        public void DecreaseYRange()
        {
            int index = this.Index;
            this.Index = index - 1;
            this.UpdateRange();
        }

        public float GetYRange(int index) => 
            (((float) Math.Pow(2.0, (double) (index / 2))) * (1f + ((index % 2) * ((index < 0) ? 0.25f : 0.5f))));

        public void IncreaseYRange()
        {
            int index = this.Index;
            this.Index = index + 1;
            this.UpdateRange();
        }

        private void UpdateRange()
        {
            this.YRange = this.GetYRange(this.Index);
            this.YLegendMsCount = ((this.Index % 2) == 0) ? 8 : 12;
            this.YLegendMsIncrement = this.YRange / ((float) this.YLegendMsCount);
            this.YLegendIncrement = (this.YScale / this.YRange) * this.YLegendMsIncrement;
        }

        public float YRange { get; private set; }

        public float YLegendMsIncrement { get; private set; }

        public int YLegendMsCount { get; private set; }

        public float YLegendIncrement { get; private set; }

        public int Index { get; private set; }
    }
}

