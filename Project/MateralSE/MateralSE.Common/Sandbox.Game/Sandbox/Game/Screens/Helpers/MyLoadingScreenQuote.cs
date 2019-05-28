namespace Sandbox.Game.Screens.Helpers
{
    using System;
    using VRage.Utils;

    internal class MyLoadingScreenQuote : MyLoadingScreenText
    {
        public readonly MyStringId Author;

        public MyLoadingScreenQuote(MyStringId text, MyStringId author) : base(text)
        {
            this.Author = author;
        }

        public static void Init()
        {
            MyStringId nullOrEmpty = MyStringId.NullOrEmpty;
            for (int i = 0; (nullOrEmpty = MyStringId.TryGet($"Quote{i:00}Text")) != MyStringId.NullOrEmpty; i++)
            {
                MyStringId author = MyStringId.TryGet($"Quote{i:00}Author");
                MyLoadingScreenText.m_texts.Add(new MyLoadingScreenQuote(nullOrEmpty, author));
            }
        }
    }
}

