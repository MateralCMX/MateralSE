namespace Sandbox.Game.Multiplayer
{
    using Sandbox.Game.World;
    using System;

    public class MyEntityCameraSettings
    {
        public double Distance;
        public Vector2? HeadAngle;
        private bool m_isFirstPerson;

        public bool IsFirstPerson
        {
            get => 
                (this.m_isFirstPerson || !MySession.Static.Settings.Enable3rdPersonView);
            set => 
                (this.m_isFirstPerson = value);
        }
    }
}

