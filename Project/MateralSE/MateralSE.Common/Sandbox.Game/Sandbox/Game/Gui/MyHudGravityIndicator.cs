namespace Sandbox.Game.Gui
{
    using System;
    using System.Runtime.CompilerServices;
    using VRage.Game.Entity;

    public class MyHudGravityIndicator
    {
        internal MyEntity Entity;

        public void Clean()
        {
            this.Entity = null;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Show(Action<MyHudGravityIndicator> propertiesInit)
        {
            this.Visible = true;
            if (propertiesInit != null)
            {
                propertiesInit(this);
            }
        }

        public bool Visible { get; private set; }
    }
}

