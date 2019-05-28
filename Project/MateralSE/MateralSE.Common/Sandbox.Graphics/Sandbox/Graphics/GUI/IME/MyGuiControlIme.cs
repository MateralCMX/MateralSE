namespace Sandbox.Graphics.GUI.IME
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    public class MyGuiControlIme : TextBox
    {
        private SwitchIme activateInner;
        private SwitchIme deactivateInner;
        private bool m_isActive;

        public MyGuiControlIme()
        {
            this.activateInner = new SwitchIme(this.ActivateDelegate);
            this.deactivateInner = new SwitchIme(this.DeactivateDelegate);
            base.KeyDown += new KeyEventHandler(this.MyGuiControlIme_KeyDown);
            this.AutoFocusing = false;
        }

        private void ActivateDelegate()
        {
            base.ImeMode = ImeMode.On;
            this.m_isActive = true;
        }

        public void ActivateIme()
        {
            base.Invoke(this.activateInner);
        }

        public void ActivateInputListening()
        {
            if (MyImeProcessor.Instance != null)
            {
                MyImeProcessor.Instance.GuiImeControl = this;
            }
        }

        private void DeactivateDelegate()
        {
            this.m_isActive = false;
            base.ImeMode = ImeMode.Disable;
        }

        public void DeactivateIme()
        {
            base.Invoke(this.deactivateInner);
        }

        private void MyGuiControlIme_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                e.SuppressKeyPress = true;
            }
            else
            {
                Keys keyCode = e.KeyCode;
                if (((keyCode == Keys.Tab) || (keyCode == Keys.Enter)) || (keyCode == Keys.Escape))
                {
                    e.SuppressKeyPress = true;
                }
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            this.Text = string.Empty;
        }

        protected override void WndProc(ref Message m)
        {
            if ((!this.Focused && (base.CanFocus && this.AutoFocusing)) && base.Parent.ContainsFocus)
            {
                base.Focus();
            }
            if ((m.Msg != 260) && ((MyImeProcessor.Instance != null) && MyImeProcessor.Instance.WndProc(ref m)))
            {
                base.WndProc(ref m);
            }
        }

        public bool AutoFocusing { get; set; }

        private delegate void SwitchIme();
    }
}

