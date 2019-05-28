namespace VRage.Game.ModAPI
{
    using System;

    public interface IMyHudNotification
    {
        void Hide();
        void ResetAliveTime();
        void Show();

        string Text { get; set; }

        string Font { get; set; }

        int AliveTime { get; set; }
    }
}

