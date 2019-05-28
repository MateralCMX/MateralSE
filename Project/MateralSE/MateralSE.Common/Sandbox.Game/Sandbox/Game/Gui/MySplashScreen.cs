namespace Sandbox.Game.Gui
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class MySplashScreen : Form
    {
        private Graphics m_graphics;
        private string m_imageFile;
        private Image m_image;
        private PointF m_scale;

        public MySplashScreen(string image, PointF scale)
        {
            try
            {
                this.m_image = Image.FromFile(image);
                this.m_scale = scale;
            }
            catch (Exception)
            {
                this.m_image = null;
                return;
            }
            this.InitializeComponent();
            this.m_graphics = base.CreateGraphics();
            this.m_imageFile = image;
        }

        public void Draw()
        {
            if (this.m_image != null)
            {
                base.Show();
                RectangleF rect = new RectangleF(0f, 0f, this.m_image.Width * this.m_scale.X, this.m_image.Height * this.m_scale.Y);
                this.m_graphics.DrawImage(this.m_image, rect);
            }
        }

        private void InitializeComponent()
        {
            base.SuspendLayout();
            float num = this.m_image.Width * this.m_scale.X;
            float num2 = this.m_image.Height * this.m_scale.Y;
            base.ClientSize = new Size((int) num, (int) num2);
            base.Name = "SplashScreen";
            base.ResumeLayout(false);
            base.TopMost = true;
            base.FormBorderStyle = FormBorderStyle.None;
            base.CenterToScreen();
        }
    }
}

