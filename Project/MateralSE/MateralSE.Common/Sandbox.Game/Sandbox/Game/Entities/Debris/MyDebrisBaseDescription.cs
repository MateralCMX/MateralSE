namespace Sandbox.Game.Entities.Debris
{
    using System;

    public class MyDebrisBaseDescription
    {
        public string Model;
        public int LifespanMinInMiliseconds;
        public int LifespanMaxInMiliseconds;
        public Action<MyDebrisBase> OnCloseAction;
    }
}

