namespace Sandbox.Game
{
    using System;
    using System.Collections.Generic;

    public class MyCreditsNotice
    {
        public string LogoTexture;
        public Vector2? LogoNormalizedSize;
        public float? LogoScale;
        public float LogoOffset = 0.07f;
        public readonly List<StringBuilder> CreditNoticeLines = new List<StringBuilder>();
    }
}

