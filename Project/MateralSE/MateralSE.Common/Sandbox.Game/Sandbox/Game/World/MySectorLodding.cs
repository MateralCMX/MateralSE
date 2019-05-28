namespace Sandbox.Game.World
{
    using System;
    using VRageRender;

    public class MySectorLodding
    {
        public MyNewLoddingSettings CurrentSettings = new MyNewLoddingSettings();
        private MyNewLoddingSettings m_lowSettings = new MyNewLoddingSettings();
        private MyNewLoddingSettings m_mediumSettings = new MyNewLoddingSettings();
        private MyNewLoddingSettings m_highSettings = new MyNewLoddingSettings();
        private MyNewLoddingSettings m_extremeSettings = new MyNewLoddingSettings();
        private MyRenderQualityEnum m_selectedQuality = MyRenderQualityEnum.HIGH;

        public void SelectQuality(MyRenderQualityEnum quality)
        {
            MyNewLoddingSettings lowSettings;
            this.m_selectedQuality = quality;
            switch (quality)
            {
                case MyRenderQualityEnum.LOW:
                    lowSettings = this.LowSettings;
                    break;

                case MyRenderQualityEnum.NORMAL:
                    lowSettings = this.MediumSettings;
                    break;

                case MyRenderQualityEnum.HIGH:
                    lowSettings = this.HighSettings;
                    break;

                case MyRenderQualityEnum.EXTREME:
                    lowSettings = this.ExtremeSettings;
                    break;

                default:
                    return;
            }
            this.CurrentSettings.CopyFrom(lowSettings);
            MyRenderProxy.UpdateNewLoddingSettings(lowSettings);
        }

        public void UpdatePreset(MyNewLoddingSettings lowLoddingSettings, MyNewLoddingSettings mediumLoddingSettings, MyNewLoddingSettings highLoddingSettings, MyNewLoddingSettings extremeLoddingSettings)
        {
            this.m_lowSettings.CopyFrom(lowLoddingSettings);
            this.m_mediumSettings.CopyFrom(mediumLoddingSettings);
            this.m_highSettings.CopyFrom(highLoddingSettings);
            this.m_extremeSettings.CopyFrom(extremeLoddingSettings);
            this.SelectQuality(this.m_selectedQuality);
        }

        public MyNewLoddingSettings LowSettings =>
            this.m_lowSettings;

        public MyNewLoddingSettings MediumSettings =>
            this.m_mediumSettings;

        public MyNewLoddingSettings HighSettings =>
            this.m_highSettings;

        public MyNewLoddingSettings ExtremeSettings =>
            this.m_extremeSettings;
    }
}

