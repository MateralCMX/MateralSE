namespace Sandbox.Game.GUI.DebugInputComponents.HonzaDebugInputComponent
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class LiveWatch : Form
    {
        private IContainer components;
        public DataGridView Watch;
        private DataGridViewTextBoxColumn Prop;
        private DataGridViewTextBoxColumn Value;
        private SplitContainer splitContainer1;
        public PropertyGrid propertyGrid1;

        public LiveWatch()
        {
            this.InitializeComponent();
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
            this.Watch = new DataGridView();
            this.Prop = new DataGridViewTextBoxColumn();
            this.Value = new DataGridViewTextBoxColumn();
            this.propertyGrid1 = new PropertyGrid();
            this.splitContainer1 = new SplitContainer();
            ((ISupportInitialize) this.Watch).BeginInit();
            this.splitContainer1.BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            base.SuspendLayout();
            this.Watch.AllowUserToAddRows = false;
            this.Watch.AllowUserToDeleteRows = false;
            this.Watch.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridViewColumn[] dataGridViewColumns = new DataGridViewColumn[] { this.Prop, this.Value };
            this.Watch.Columns.AddRange(dataGridViewColumns);
            this.Watch.Dock = DockStyle.Fill;
            this.Watch.Location = new Point(0, 0);
            this.Watch.Name = "Watch";
            this.Watch.ReadOnly = true;
            this.Watch.RowHeadersVisible = false;
            this.Watch.Size = new Size(0x18f, 0x144);
            this.Watch.TabIndex = 0;
            this.Prop.HeaderText = "Prop";
            this.Prop.Name = "Prop";
            this.Prop.ReadOnly = true;
            this.Value.HeaderText = "Value";
            this.Value.Name = "Value";
            this.Value.ReadOnly = true;
            this.propertyGrid1.Dock = DockStyle.Fill;
            this.propertyGrid1.Location = new Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new Size(0x18f, 320);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.Click += new EventHandler(this.propertyGrid1_Click);
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = Orientation.Horizontal;
            this.splitContainer1.Panel1.Controls.Add(this.Watch);
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Size = new Size(0x18f, 0x288);
            this.splitContainer1.SplitterDistance = 0x144;
            this.splitContainer1.TabIndex = 2;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScroll = true;
            base.ClientSize = new Size(0x18f, 0x288);
            base.Controls.Add(this.splitContainer1);
            base.Name = "LiveWatch";
            this.Text = "LiveWatch";
            ((ISupportInitialize) this.Watch).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.EndInit();
            this.splitContainer1.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {
        }

        private void Watch_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}

