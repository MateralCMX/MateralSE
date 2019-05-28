namespace Sandbox
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;
    using VRage;
    using VRage.Game.ModAPI;

    public class ModCrashedTheGameMesageBox : Form
    {
        private IContainer components;
        private Panel pLogo;
        private Button CloseBtn;
        private LinkLabel LogLink;

        public ModCrashedTheGameMesageBox(ModCrashedException e, string logPath)
        {
            Application.EnableVisualStyles();
            this.InitializeComponent();
            this.CloseBtn.Text = MyTexts.GetString(MyCommonTexts.Close);
            this.Text = MyTexts.GetString(MyCommonTexts.ModCrashedTheGame);
            IMyModContext modContext = e.ModContext;
            string str = string.Format(MyTexts.GetString(MyCommonTexts.ModCrashedTheGameInfo), modContext.ModName, modContext.ModId, "log");
            this.LogLink.Text = str;
            this.LogLink.Links.Add(str.Length - 3, logPath.Length, logPath);
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(ModCrashedTheGameMesageBox));
            this.pLogo = new Panel();
            this.CloseBtn = new Button();
            this.LogLink = new LinkLabel();
            base.SuspendLayout();
            this.pLogo.AutoSize = true;
            this.pLogo.BackgroundImage = (Image) manager.GetObject("pLogo.BackgroundImage");
            this.pLogo.BackgroundImageLayout = ImageLayout.Center;
            this.pLogo.Location = new Point(0x26, 9);
            this.pLogo.Margin = new Padding(0);
            this.pLogo.MinimumSize = new Size(340, 0x59);
            this.pLogo.Name = "pLogo";
            this.pLogo.Size = new Size(0x1a9, 0x59);
            this.pLogo.TabIndex = 1;
            this.CloseBtn.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.CloseBtn.Location = new Point(0x19c, 0x106);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.Size = new Size(0x57, 0x1b);
            this.CloseBtn.TabIndex = 2;
            this.CloseBtn.Text = "Close";
            this.CloseBtn.UseVisualStyleBackColor = true;
            this.CloseBtn.Click += new EventHandler(this.btnYes_Click);
            this.LogLink.AutoSize = true;
            this.LogLink.Location = new Point(0x23, 0x71);
            this.LogLink.Name = "LogLink";
            this.LogLink.Size = new Size(0x8d, 0x4b);
            this.LogLink.TabIndex = 3;
            this.LogLink.TabStop = true;
            this.LogLink.Text = "Mod crashed the game\r\nPlease contact the author\r\n\r\nMore info in log:\r\n\r\n";
            this.LogLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linklblLog_LinkClicked);
            base.AutoScaleDimensions = new SizeF(7f, 15f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x1ff, 0x12d);
            base.Controls.Add(this.CloseBtn);
            base.Controls.Add(this.pLogo);
            base.Controls.Add(this.LogLink);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "ModCrashedTheGameMesageBox";
            base.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Mod crashed the Game!";
            base.TopMost = true;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void linklblLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }
    }
}

