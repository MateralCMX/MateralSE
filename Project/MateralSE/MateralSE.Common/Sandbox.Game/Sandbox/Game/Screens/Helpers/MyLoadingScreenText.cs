namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using System.Collections.Generic;
    using VRage.Collections;
    using VRage.Utils;
    using VRageMath;

    internal class MyLoadingScreenText
    {
        public readonly MyStringId Text;
        protected static List<MyLoadingScreenText> m_texts;

        static MyLoadingScreenText()
        {
            if (m_texts == null)
            {
                m_texts = new List<MyLoadingScreenText>();
            }
            else
            {
                m_texts.Clear();
            }
            MyLoadingScreenQuote.Init();
            MyLoadingScreenHint.Init();
        }

        public MyLoadingScreenText(MyStringId text)
        {
            this.Text = text;
        }

        public static MyLoadingScreenText GetRandomText() => 
            GetScreenText(MyUtils.GetRandomInt(m_texts.Count));

        public static MyLoadingScreenText GetScreenText(int i)
        {
            int num1 = MyMath.Mod(i, m_texts.Count);
            i = num1;
            return m_texts[i];
        }

        public override string ToString() => 
            this.Text.ToString();

        public static ListReader<MyLoadingScreenText> Texts =>
            m_texts;
    }
}

