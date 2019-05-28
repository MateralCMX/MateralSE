namespace Sandbox.Game.Screens
{
    using Sandbox;
    using Sandbox.Game;
    using Sandbox.Game.GUI;
    using Sandbox.Graphics.GUI;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using VRage.Utils;

    public class MyGuiScreenProgressAsync : MyGuiScreenProgressBase
    {
        private Func<IMyAsyncResult> m_beginAction;
        private Action<IMyAsyncResult, MyGuiScreenProgressAsync> m_endAction;
        private IMyAsyncResult m_asyncResult;

        public MyGuiScreenProgressAsync(MyStringId text, MyStringId? cancelText, Func<IMyAsyncResult> beginAction, Action<IMyAsyncResult, MyGuiScreenProgressAsync> endAction, object userData = null) : base(text, cancelText, true, true)
        {
            this.FriendlyName = "MyGuiScreenProgressAsync";
            this.m_beginAction = beginAction;
            this.m_endAction = endAction;
            this.UserData = userData;
        }

        public override string GetFriendlyName() => 
            this.FriendlyName;

        protected override void ProgressStart()
        {
            this.m_asyncResult = this.m_beginAction();
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            base.m_rotatingWheel.MultipleSpinningWheels = MyPerGameSettings.GUI.MultipleSpinningWheels;
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            if (base.State != MyGuiScreenState.OPENED)
            {
                return false;
            }
            if (this.m_asyncResult.IsCompleted)
            {
                this.m_endAction(this.m_asyncResult, this);
            }
            if ((this.m_asyncResult != null) && (this.m_asyncResult.Task.Exceptions != null))
            {
                foreach (Exception exception in this.m_asyncResult.Task.Exceptions)
                {
                    MySandboxGame.Log.WriteLine(exception);
                }
            }
            return true;
        }

        public string FriendlyName { get; set; }

        public object UserData { get; private set; }

        public StringBuilder Text
        {
            get => 
                base.m_progressTextLabel.TextToDraw;
            set => 
                (base.m_progressTextLabel.TextToDraw = value);
        }

        public MyStringId ProgressText
        {
            get => 
                base.ProgressText;
            set
            {
                if (base.ProgressText != value)
                {
                    base.m_progressTextLabel.PrepareForAsyncTextUpdate();
                    base.ProgressText = value;
                }
            }
        }

        public string ProgressTextString
        {
            get => 
                base.ProgressTextString;
            set
            {
                if (base.ProgressTextString != value)
                {
                    base.m_progressTextLabel.PrepareForAsyncTextUpdate();
                    base.ProgressTextString = value;
                }
            }
        }
    }
}

