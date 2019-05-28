namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using VRage.Utils;

    internal abstract class MyGuiScreenProgressBaseAsync<T> : MyGuiScreenProgressBase
    {
        private LinkedList<ProgressAction<T>> m_actions;
        private string m_constructorStackTrace;

        protected MyGuiScreenProgressBaseAsync(MyStringId progressText, MyStringId? cancelText = new MyStringId?()) : base(progressText, cancelText, true, true)
        {
            this.m_actions = new LinkedList<ProgressAction<T>>();
            if (Debugger.IsAttached)
            {
                this.m_constructorStackTrace = Environment.StackTrace;
            }
        }

        protected void AddAction(IAsyncResult asyncResult, ErrorHandler<T> errorHandler = null)
        {
            this.AddAction(asyncResult, new ActionDoneHandler<T>(this.OnActionCompleted), errorHandler);
        }

        protected unsafe void AddAction(IAsyncResult asyncResult, ActionDoneHandler<T> doneHandler, ErrorHandler<T> errorHandler = null)
        {
            ProgressAction<T>* actionPtr1;
            ProgressAction<T> action = new ProgressAction<T> {
                AsyncResult = asyncResult,
                ActionDoneHandler = doneHandler
            };
            actionPtr1->ErrorHandler = errorHandler ?? new ErrorHandler<T>(this.OnError);
            actionPtr1 = (ProgressAction<T>*) ref action;
            this.m_actions.AddFirst(action);
        }

        protected void CancelAll()
        {
            this.m_actions.Clear();
        }

        protected virtual void OnActionCompleted(IAsyncResult asyncResult, T asyncState)
        {
        }

        protected override void OnCancelClick(MyGuiControlButton sender)
        {
            this.CancelAll();
            base.OnCancelClick(sender);
        }

        protected virtual void OnError(Exception exception, T asyncState)
        {
            MyLog.Default.WriteLine(exception);
            throw exception;
        }

        protected void Retry()
        {
            this.m_actions.Clear();
            this.ProgressStart();
        }

        public override bool Update(bool hasFocus)
        {
            if (!base.Update(hasFocus))
            {
                return false;
            }
            LinkedListNode<ProgressAction<T>> first = this.m_actions.First;
            while (first != null)
            {
                if (!first.Value.AsyncResult.IsCompleted)
                {
                    first = first.Next;
                    continue;
                }
                try
                {
                    first.Value.ActionDoneHandler(first.Value.AsyncResult, (T) first.Value.AsyncResult.AsyncState);
                }
                catch (Exception exception)
                {
                    first.Value.ErrorHandler(exception, (T) first.Value.AsyncResult.AsyncState);
                }
                LinkedListNode<ProgressAction<T>> node = first;
                first = first.Next;
                this.m_actions.Remove(node);
            }
            return (base.State == MyGuiScreenState.OPENED);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProgressAction
        {
            public IAsyncResult AsyncResult;
            public ActionDoneHandler<T> ActionDoneHandler;
            public ErrorHandler<T> ErrorHandler;
        }
    }
}

