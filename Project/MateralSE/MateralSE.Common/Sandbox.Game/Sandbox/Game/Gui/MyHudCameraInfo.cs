namespace Sandbox.Game.Gui
{
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;

    public class MyHudCameraInfo
    {
        public MyHudCameraInfo()
        {
            this.Visible = false;
            this.IsDirty = true;
        }

        public void Disable()
        {
            this.Visible = false;
            this.IsDirty = true;
        }

        public void Draw(MyGuiControlMultilineText control)
        {
            if (!this.Visible)
            {
                if (this.IsDirty)
                {
                    control.Clear();
                    this.IsDirty = false;
                }
            }
            else if (this.IsDirty)
            {
                control.Clear();
                control.AppendText(this.CameraName);
                control.AppendLine();
                control.AppendText(this.ShipName);
                this.IsDirty = false;
            }
        }

        public void Enable(string shipName, string cameraName)
        {
            this.Visible = true;
            this.ShipName = shipName;
            this.CameraName = cameraName;
            this.IsDirty = true;
        }

        private bool Visible { get; set; }

        private string CameraName { get; set; }

        private string ShipName { get; set; }

        private bool IsDirty { get; set; }
    }
}

