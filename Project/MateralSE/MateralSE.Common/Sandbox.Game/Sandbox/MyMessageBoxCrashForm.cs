namespace Sandbox
{
    using Sandbox.Game.Localization;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;
    using VRage;
    using VRage.FileSystem;

    public class MyMessageBoxCrashForm : Form
    {
        private IContainer components;
        private Button btnYes;
        private FlowLayoutPanel flMainPanel;
        private Panel pLogo;
        private Label lblMainText;
        private LinkLabel linklblLog;
        private Label lblEmailText;
        private FlowLayoutPanel flowLayoutPanel2;
        private Label lblEmail;
        private TextBox tbEmail;
        private Label lblDetails;
        private RichTextBox rtbDetails;

        public MyMessageBoxCrashForm(string gameName, string logPath)
        {
            Application.EnableVisualStyles();
            this.InitializeComponent();
            this.Text = $"{gameName} Crash";
            this.linklblLog.Text = logPath;
            this.linklblLog.Links.Add(0, logPath.Length, logPath);
            if (Directory.Exists(Path.Combine(new FileInfo(MyFileSystem.ExePath).Directory.FullName, "Content")))
            {
                this.lblMainText.Text = MyTexts.Get(MyCoreTexts.CrashScreen_MainText).ToString();
                this.linklblLog.Text = MyTexts.Get(MyCoreTexts.CrashScreen_Log).ToString();
                this.lblEmailText.Text = MyTexts.Get(MyCoreTexts.CrashScreen_EmailText).ToString();
                this.lblEmail.Text = MyTexts.Get(MyCoreTexts.CrashScreen_Email).ToString();
                this.lblDetails.Text = MyTexts.Get(MyCoreTexts.CrashScreen_Detail).ToString();
                this.btnYes.Text = MyTexts.Get(MyCoreTexts.CrashScreen_Yes).ToString();
            }
            else
            {
                MessageBox.Show("The content folder \"Content\" containing game assets is completely missing. Please verify integrity of game files using Steam. \n\n That is most likely the reason of the crash. As game cannot run without it.", "Content is missing", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                this.lblMainText.Text = "Space Engineers had a problem and crashed! We apologize for the inconvenience. Please click Send Log if you would like to help us analyze and fix the problem. For more information, check the log below";
                this.linklblLog.Text = "log";
                this.lblEmailText.Text = "Additionally, you can send us your email in case a member of our support staff needs more information about this error. \r\n \r\n If you would not mind being contacted about this issue please provide your e-mail address below. By sending the log, I grant my consent to the processing of my personal data (E-mail, Steam ID and IP address) to Keen SWH LTD. United Kingdom and it subsidiaries, in order for these data to be processed for the purpose of tracking the crash and requesting feedback with the intent to improve the game performance. I grant this consent for an indefinite term until my express revocation thereof. I confirm that I have been informed that the provision of these data is voluntary, and that I have the right to request their deletion. Registration is non-transferable. More information about the processing of my personal data in the scope required by legal regulations, in particular Regulation (EU) 2016/679 of the European Parliament and of the Council, can be found as of 25 May 2018 here. \r\n";
                this.lblEmail.Text = "Email (optional)";
                this.lblDetails.Text = "To help us resolve the problem, please provide a description of what you were doing when it occurred (optional)";
                this.btnYes.Text = "Send Log";
            }
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Yes;
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
            ComponentResourceManager manager = new ComponentResourceManager(typeof(MyMessageBoxCrashForm));
            this.btnYes = new Button();
            this.flMainPanel = new FlowLayoutPanel();
            this.pLogo = new Panel();
            this.lblMainText = new Label();
            this.linklblLog = new LinkLabel();
            this.lblEmailText = new Label();
            this.flowLayoutPanel2 = new FlowLayoutPanel();
            this.lblEmail = new Label();
            this.tbEmail = new TextBox();
            this.lblDetails = new Label();
            this.rtbDetails = new RichTextBox();
            this.flMainPanel.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            base.SuspendLayout();
            this.btnYes.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this.btnYes.Location = new Point(0x23c, 0x1ee);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new Size(0x57, 0x1b);
            this.btnYes.TabIndex = 0;
            this.btnYes.Text = "Send Log";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new EventHandler(this.btnYes_Click);
            this.flMainPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.flMainPanel.AutoScroll = true;
            this.flMainPanel.BackColor = SystemColors.ControlLightLight;
            this.flMainPanel.BorderStyle = BorderStyle.FixedSingle;
            this.flMainPanel.Controls.Add(this.lblMainText);
            this.flMainPanel.Controls.Add(this.linklblLog);
            this.flMainPanel.Controls.Add(this.lblEmailText);
            this.flMainPanel.Controls.Add(this.flowLayoutPanel2);
            this.flMainPanel.Controls.Add(this.lblDetails);
            this.flMainPanel.Controls.Add(this.rtbDetails);
            this.flMainPanel.FlowDirection = FlowDirection.TopDown;
            this.flMainPanel.Location = new Point(9, 0x5d);
            this.flMainPanel.Name = "flMainPanel";
            this.flMainPanel.Padding = new Padding(12, 0, 12, 0);
            this.flMainPanel.Size = new Size(650, 0x18b);
            this.flMainPanel.TabIndex = 1;
            this.flMainPanel.WrapContents = false;
            this.pLogo.AutoSize = true;
            this.pLogo.BackgroundImage = (Image) manager.GetObject("pLogo.BackgroundImage");
            this.pLogo.BackgroundImageLayout = ImageLayout.Center;
            this.pLogo.Location = new Point(9, 1);
            this.pLogo.Margin = new Padding(0);
            this.pLogo.MinimumSize = new Size(340, 0x59);
            this.pLogo.Name = "pLogo";
            this.pLogo.Size = new Size(650, 0x59);
            this.pLogo.TabIndex = 0;
            this.lblMainText.AutoSize = true;
            this.lblMainText.Location = new Point(12, 0);
            this.lblMainText.Margin = new Padding(0);
            this.lblMainText.Name = "lblMainText";
            this.lblMainText.Size = new Size(0x19f, 0x4b);
            this.lblMainText.TabIndex = 1;
            this.lblMainText.Text = manager.GetString("lblMainText.Text");
            this.linklblLog.AutoSize = true;
            this.linklblLog.Location = new Point(12, 0x4b);
            this.linklblLog.Margin = new Padding(0);
            this.linklblLog.Name = "linklblLog";
            this.linklblLog.Size = new Size(0x18, 15);
            this.linklblLog.TabIndex = 2;
            this.linklblLog.TabStop = true;
            this.linklblLog.Text = "log";
            this.linklblLog.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linklblLog_LinkClicked);
            this.lblEmailText.AutoSize = true;
            this.lblEmailText.Location = new Point(12, 90);
            this.lblEmailText.Margin = new Padding(0);
            this.lblEmailText.Name = "lblEmailText";
            this.lblEmailText.Size = new Size(0x270, 180);
            this.lblEmailText.TabIndex = 3;
            this.lblEmailText.Text = manager.GetString("lblEmailText.Text");
            this.flowLayoutPanel2.Controls.Add(this.lblEmail);
            this.flowLayoutPanel2.Controls.Add(this.tbEmail);
            this.flowLayoutPanel2.Location = new Point(12, 270);
            this.flowLayoutPanel2.Margin = new Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new Size(0x1a9, 0x19);
            this.flowLayoutPanel2.TabIndex = 4;
            this.lblEmail.AutoSize = true;
            this.lblEmail.Location = new Point(0, 3);
            this.lblEmail.Margin = new Padding(0, 3, 0, 0);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new Size(0x5b, 15);
            this.lblEmail.TabIndex = 0;
            this.lblEmail.Text = "Email (optional)";
            this.tbEmail.AcceptsReturn = true;
            this.tbEmail.Location = new Point(0x5f, 0);
            this.tbEmail.Margin = new Padding(4, 0, 0, 0);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new Size(0xd7, 0x17);
            this.tbEmail.TabIndex = 1;
            this.tbEmail.KeyDown += new KeyEventHandler(this.tbEmail_KeyDown);
            this.lblDetails.AutoSize = true;
            this.lblDetails.Location = new Point(12, 0x127);
            this.lblDetails.Margin = new Padding(0);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new Size(600, 30);
            this.lblDetails.TabIndex = 5;
            this.lblDetails.Text = "\r\nTo help us resolve the problem, please provide a description of what you were doing when it occurred (optional):";
            this.rtbDetails.AcceptsTab = true;
            this.rtbDetails.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.rtbDetails.Location = new Point(15, 0x148);
            this.rtbDetails.Name = "rtbDetails";
            this.rtbDetails.Size = new Size(0x26a, 0x2a);
            this.rtbDetails.TabIndex = 0;
            this.rtbDetails.Text = "";
            base.AutoScaleDimensions = new SizeF(7f, 15f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x29c, 0x20d);
            base.Controls.Add(this.pLogo);
            base.Controls.Add(this.flMainPanel);
            base.Controls.Add(this.btnYes);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.Icon = (Icon) manager.GetObject("$this.Icon");
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "MyMessageBoxCrashForm";
            base.StartPosition = FormStartPosition.CenterScreen;
            base.Tag = "";
            this.Text = "CLANG!";
            base.TopMost = true;
            this.flMainPanel.ResumeLayout(false);
            this.flMainPanel.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void linklblLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void tbEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.rtbDetails.Focus();
            }
        }

        public string Email =>
            this.tbEmail.Text;

        public string Message =>
            this.rtbDetails.Text;
    }
}

