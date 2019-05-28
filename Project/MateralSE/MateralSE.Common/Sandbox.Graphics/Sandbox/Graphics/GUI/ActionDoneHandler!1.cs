namespace Sandbox.Graphics.GUI
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ActionDoneHandler<T>(IAsyncResult asyncResult, T asyncState);
}

