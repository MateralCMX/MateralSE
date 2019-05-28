namespace VRage
{
    using System;

    public interface ITextEvaluator
    {
        string TokenEvaluate(string token, string context);
    }
}

