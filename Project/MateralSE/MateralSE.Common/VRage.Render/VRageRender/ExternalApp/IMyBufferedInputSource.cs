namespace VRageRender.ExternalApp
{
    using System;
    using System.Collections.Generic;
    using VRageMath;

    public interface IMyBufferedInputSource
    {
        void AddChar(char ch);
        void SwapBufferedTextInput(ref List<char> swappedBuffer);

        Vector2 MousePosition { get; }

        Vector2 MouseAreaSize { get; }
    }
}

